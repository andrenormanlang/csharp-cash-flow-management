using CashFlowManagement.Commands;
using CashFlowManagement.Enums;
using CashFlowManagement.Interfaces;
using CashFlowManagement.Models;
using CashFlowManagement.Services.Interfaces;
using CashFlowManagement.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CashFlowManagement.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ITransactionManager _transactionManager;
        private readonly IDataService _dataService;
        private DateTime _selectedDate = DateTime.Today;
        private decimal _amount;
        private string _description = string.Empty;
        private TransactionCategory _selectedTransactionCategory;
        private CategoryType _selectedType = CategoryType.Expense;
        private string _searchText = string.Empty;
        private DateTime? _filterMonth;
        private string _filterCategory = string.Empty;
        private CategoryType? _filterType;
        private decimal _monthlyRevenue;
        private decimal _monthlyExpenses;
        private decimal _netCashFlow;
        private string _statusMessage = string.Empty;
        private DateTime? _filterStartDate;
        private DateTime? _filterEndDate;
        private bool _useCustomDateRange;

        public event PropertyChangedEventHandler? PropertyChanged;

        // Collections
        public ObservableCollection<ITransaction> Transactions { get; } = new();
        public ObservableCollection<string> Categories { get; } = new();
        public ObservableCollection<TransactionCategory> ExpenseCategories { get; }
        public ObservableCollection<TransactionCategory> RevenueCategories { get; }
        public ObservableCollection<CategorySummary> TopExpenses { get; } = new();
        public ObservableCollection<CategorySummary> TopRevenues { get; } = new();

        // Commands
        public ICommand AddTransactionCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand GenerateReportCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand ClearFiltersCommand { get; }

        // Properties for Binding
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
            }
        }

        public decimal Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged();
                NotifyCanExecuteChanged();
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        public TransactionCategory SelectedTransactionCategory
        {
            get => _selectedTransactionCategory;
            set
            {
                _selectedTransactionCategory = value;
                OnPropertyChanged();
                NotifyCanExecuteChanged();
            }
        }

        public CategoryType SelectedType
        {
            get => _selectedType;
            set
            {
                if (_selectedType != value)
                {
                    _selectedType = value;
                    OnPropertyChanged();
                    // Reset the selected category when type changes
                    SelectedTransactionCategory = default;
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        public DateTime? FilterMonth
        {
            get => _filterMonth;
            set
            {
                _filterMonth = value;
                OnPropertyChanged();
                if (value.HasValue)
                {
                    // When month is selected, set the date range to cover the entire month
                    FilterStartDate = new DateTime(value.Value.Year, value.Value.Month, 1);
                    FilterEndDate = FilterStartDate.Value.AddMonths(1).AddDays(-1);
                }
                ApplyFilters();
            }
        }

        public string FilterCategory
        {
            get => _filterCategory;
            set
            {
                _filterCategory = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        public CategoryType? FilterType
        {
            get => _filterType;
            set
            {
                _filterType = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        public DateTime? FilterStartDate
        {
            get => _filterStartDate;
            set
            {
                _filterStartDate = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        public DateTime? FilterEndDate
        {
            get => _filterEndDate;
            set
            {
                _filterEndDate = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        public bool UseCustomDateRange
        {
            get => _useCustomDateRange;
            set
            {
                _useCustomDateRange = value;
                OnPropertyChanged();

                if (!value)
                {
                    // When switching back to month view, clear custom date range
                    FilterStartDate = null;
                    FilterEndDate = null;

                    // Ensure we have a month selected
                    if (!FilterMonth.HasValue)
                    {
                        FilterMonth = DateTime.Today;
                    }
                }
                else
                {
                    // When switching to custom range, initialize with current month if no dates set
                    if (!FilterStartDate.HasValue && FilterMonth.HasValue)
                    {
                        FilterStartDate = new DateTime(FilterMonth.Value.Year, FilterMonth.Value.Month, 1);
                        FilterEndDate = FilterStartDate.Value.AddMonths(1).AddDays(-1);
                        FilterMonth = null;
                    }
                }
            }
        }

        public decimal MonthlyRevenue
        {
            get => _monthlyRevenue;
            set
            {
                _monthlyRevenue = value;
                OnPropertyChanged();
            }
        }

        public decimal MonthlyExpenses
        {
            get => _monthlyExpenses;
            set
            {
                _monthlyExpenses = value;
                OnPropertyChanged();
            }
        }

        public decimal NetCashFlow
        {
            get => _netCashFlow;
            set
            {
                _netCashFlow = value;
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        // Constructor
        public MainViewModel(ITransactionManager transactionManager, IDataService dataService)
        {
            _transactionManager = transactionManager;
            _dataService = dataService;

            // Initialize collections
            ExpenseCategories = new ObservableCollection<TransactionCategory>();
            RevenueCategories = new ObservableCollection<TransactionCategory>();

            // Initialize commands
            AddTransactionCommand = new RelayCommand(AddTransaction, CanAddTransaction);
            SaveCommand = new RelayCommand(async () => await SaveDataAsync());
            LoadCommand = new RelayCommand(async () => await LoadDataAsync());
            GenerateReportCommand = new RelayCommand(GenerateReport);
            ClearFiltersCommand = new RelayCommand(ClearFilters);
            ExitCommand = new RelayCommand(() =>
            {
                var result = MessageBox.Show("Would you like to save your changes?",
                                            "Save Changes",
                                            MessageBoxButton.YesNoCancel,
                                            MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveDataAsync().Wait();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }

                Application.Current.Shutdown();
            });

            InitializeCategories();
        }

        // Methods
        private void InitializeCategories()
        {
            foreach (TransactionCategory category in Enum.GetValues(typeof(TransactionCategory)))
            {
                if (IsExpenseCategory(category))
                {
                    ExpenseCategories.Add(category);
                }
                else
                {
                    RevenueCategories.Add(category);
                }
            }
        }

        private static readonly HashSet<TransactionCategory> IncomeCategories = new(
        Enum.GetValues<TransactionCategory>()
            .TakeWhile(c => (int)c < (int)TransactionCategory.Rent)  // Take all until first expense category
    );

        private bool IsExpenseCategory(TransactionCategory category)
        {
            return !IncomeCategories.Contains(category);
        }


        public async Task LoadDataAsync()
        {
            try
            {
                var transactions = await _dataService.LoadTransactionsAsync();

                Transactions.Clear();
                TopExpenses.Clear();
                TopRevenues.Clear();
                Categories.Clear();

                foreach (var transaction in transactions)
                {
                    _transactionManager.AddTransaction(transaction);
                }

                UpdateCategories();

                // Don't set FilterMonth initially to show all transactions
                FilterMonth = null;
                FilterType = null;
                FilterCategory = string.Empty;
                SearchText = string.Empty;

                ApplyFilters(); // This will now load all transactions without filtering
                StatusMessage = "Data loaded successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading data: {ex.Message}";
                throw;
            }
        }

        public async Task SaveDataAsync()
        {
            try
            {
                await _dataService.SaveTransactionsAsync(_transactionManager.GetTransactions());
                StatusMessage = "Data saved successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error saving data: {ex.Message}";
            }
        }

        private void AddTransaction()
        {
            try
            {
                var transaction = new Transaction
                {
                    Date = SelectedDate,
                    Amount = Amount,
                    Description = Description,
                    Category = new Category
                    {
                        Name = SelectedTransactionCategory.ToString(),
                        Type = SelectedType,
                        TransactionCategory = SelectedTransactionCategory
                    }
                };

                _transactionManager.AddTransaction(transaction);
                Transactions.Add(transaction);
                ClearInputs();
                GenerateReport();
                StatusMessage = "Transaction added successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error adding transaction: {ex.Message}";
            }
        }

        private bool CanAddTransaction()
        {
            return Amount > 0;
        }

        private void GenerateReport()
        {
            if (!Transactions.Any()) return;

            var selectedMonth = FilterMonth ?? DateTime.Today;
            var monthStart = new DateTime(selectedMonth.Year, selectedMonth.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var monthlyTransactions = Transactions.Where(t =>
                t.Date >= monthStart && t.Date <= monthEnd).ToList();

            // Calculate totals
            var monthlyRevenue = monthlyTransactions
                .Where(t => t.Category.Type == CategoryType.Revenue)
                .Sum(t => t.Amount);

            var monthlyExpenses = monthlyTransactions
                .Where(t => t.Category.Type == CategoryType.Expense)
                .Sum(t => t.Amount);

            var netFlow = monthlyRevenue - monthlyExpenses;

            // Create report view model
            var reportViewModel = new ReportViewModel
            {
                ReportTitle = $"Financial Report for {selectedMonth:MMMM yyyy}",
                TotalRevenue = monthlyRevenue,
                TotalExpenses = monthlyExpenses,
                NetCashFlow = netFlow
            };

            // Top expenses
            var topExpenses = monthlyTransactions
                .Where(t => t.Category.Type == CategoryType.Expense)
                .GroupBy(t => t.Category.Name)
                .Select(g => new CategorySummary
                {
                    Category = g.Key,
                    Amount = g.Sum(t => t.Amount),
                    Percentage = monthlyExpenses > 0 ? Math.Round(g.Sum(t => t.Amount) / monthlyExpenses, 4) : 0
                })
                .OrderByDescending(c => c.Amount)
                .Take(3);

            foreach (var expense in topExpenses)
            {
                reportViewModel.TopExpenses.Add(expense);
            }

            // Top revenues
            var revenueItems = monthlyTransactions
                .Where(t => t.Category.Type == CategoryType.Revenue)
                .GroupBy(t => t.Category.Name)
                .Select(g => new CategorySummary
                {
                    Category = g.Key,
                    Amount = g.Sum(t => t.Amount),
                    Percentage = monthlyRevenue > 0 ? Math.Round(g.Sum(t => t.Amount) / monthlyRevenue, 4) : 0
                })
                .OrderByDescending(c => c.Amount)
                .Take(3);

            foreach (var revenueItem in revenueItems)
            {
                reportViewModel.TopRevenues.Add(revenueItem);
            }

            // Show report window
            var reportWindow = new ReportWindow
            {
                DataContext = reportViewModel,
                Owner = Application.Current.MainWindow
            };
            reportWindow.ShowDialog();

            // Update the monthly summary panel
            UpdateMonthlyTotals(monthlyRevenue, monthlyExpenses, netFlow);
        }

        private void UpdateMonthlyTotals(decimal revenue, decimal expenses, decimal netFlow)
        {
            MonthlyRevenue = revenue;
            MonthlyExpenses = expenses;
            NetCashFlow = netFlow;
        }

        private void ApplyFilters()
        {
            Transactions.Clear();
            var allTransactions = _transactionManager.GetTransactions();
            var filteredTransactions = allTransactions;

            // Apply text search
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLower();
                filteredTransactions = filteredTransactions.Where(t =>
                    t.Description.ToLower().Contains(search) ||
                    t.Category.Name.ToLower().Contains(search));
            }

            // Apply date range filter
            if (FilterStartDate.HasValue)
            {
                filteredTransactions = filteredTransactions.Where(t =>
                    t.Date.Date >= FilterStartDate.Value.Date);
            }

            if (FilterEndDate.HasValue)
            {
                filteredTransactions = filteredTransactions.Where(t =>
                    t.Date.Date <= FilterEndDate.Value.Date);
            }

            // Apply category filter
            if (!string.IsNullOrWhiteSpace(FilterCategory))
            {
                filteredTransactions = filteredTransactions.Where(t =>
                    t.Category.Name.Equals(FilterCategory, StringComparison.OrdinalIgnoreCase));
            }

            // Apply type filter only if it's not the "All" option
            if (FilterType.HasValue)
            {
                filteredTransactions = filteredTransactions.Where(t => t.Category.Type == FilterType.Value);
            }

            foreach (var transaction in filteredTransactions)
            {
                Transactions.Add(transaction);
            }

            // Update the report based on the current filter date range
            UpdateFilterRange();
            // Only update monthly totals without showing the report window
            UpdateMonthlyTotals();
        }

        private void UpdateMonthlyTotals()
        {
            if (!Transactions.Any()) return;

            var selectedMonth = FilterMonth ?? DateTime.Today;
            var monthStart = new DateTime(selectedMonth.Year, selectedMonth.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var monthlyTransactions = Transactions.Where(t =>
                t.Date >= monthStart && t.Date <= monthEnd).ToList();

            var monthlyRevenue = monthlyTransactions
                .Where(t => t.Category.Type == CategoryType.Revenue)
                .Sum(t => t.Amount);

            var monthlyExpenses = monthlyTransactions
                .Where(t => t.Category.Type == CategoryType.Expense)
                .Sum(t => t.Amount);

            var netFlow = monthlyRevenue - monthlyExpenses;

            MonthlyRevenue = monthlyRevenue;
            MonthlyExpenses = monthlyExpenses;
            NetCashFlow = netFlow;
        }

        private void UpdateFilterRange()
        {
            if (FilterStartDate.HasValue && FilterEndDate.HasValue)
            {
                // If date range spans a single month, update FilterMonth
                var start = FilterStartDate.Value;
                var end = FilterEndDate.Value;

                if (start.Year == end.Year && start.Month == end.Month)
                {
                    if (_filterMonth?.Month != start.Month || _filterMonth?.Year != start.Year)
                    {
                        _filterMonth = start;
                        OnPropertyChanged(nameof(FilterMonth));
                    }
                }
                else
                {
                    // If date range spans multiple months, clear FilterMonth display
                    if (_filterMonth != null)
                    {
                        _filterMonth = null;
                        OnPropertyChanged(nameof(FilterMonth));
                    }
                }
            }
            else if (!FilterMonth.HasValue)
            {
                // If no date range and no month selected, default to current month
                FilterMonth = DateTime.Today;
            }
        }

        private void ClearFilters()
        {
            FilterStartDate = null;
            FilterEndDate = null;
            FilterMonth = null;
            FilterCategory = string.Empty;
            FilterType = null;
            SearchText = string.Empty;
            StatusMessage = "Filters cleared";

            // Reset to current month for summary
            FilterMonth = DateTime.Today;
        }

        private void ClearInputs()
        {
            Amount = 0;
            Description = string.Empty;
            SelectedTransactionCategory = default;
            SelectedDate = DateTime.Today;
        }

        private void UpdateCategories()
        {
            var categories = _transactionManager.GetTransactions()
                .Select(t => t.Category.Name)
                .Distinct()
                .OrderBy(n => n);

            Categories.Clear();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }
        }

        private void NotifyCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class CategorySummary
        {
            public string Category { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public decimal Percentage { get; set; }
        }
    }
}
