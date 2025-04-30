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
    /// <summary>
    /// View model for the main window of the application, providing data binding and business logic for the UI.
    /// </summary>
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
        private ITransaction? _selectedTransaction;
        private bool _isEditMode;

        /// <summary>
        /// Event raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        // Collections
        public ObservableCollection<ITransaction> Transactions { get; } = new();
        public ObservableCollection<TransactionCategory> ExpenseCategories { get; }
        public ObservableCollection<TransactionCategory> RevenueCategories { get; }
        public ObservableCollection<CategorySummary> TopExpenses { get; } = new();
        public ObservableCollection<CategorySummary> TopRevenues { get; } = new();

        // Commands
        public ICommand AddTransactionCommand { get; }
        public ICommand UpdateTransactionCommand { get; }
        public ICommand DeleteTransactionCommand { get; }
        public ICommand CancelEditCommand { get; }
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

        public ITransaction? SelectedTransaction
        {
            get => _selectedTransaction;
            set
            {
                _selectedTransaction = value;
                if (value != null)
                {
                    SelectedDate = value.Date;
                    Amount = value.Amount;
                    Description = value.Description;
                    SelectedType = value.Category.Type;
                    SelectedTransactionCategory = value.Category.TransactionCategory;
                }
                OnPropertyChanged();
            }
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                _isEditMode = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        /// <param name="transactionManager">The transaction manager service.</param>
        /// <param name="dataService">The data persistence service.</param>
        public MainViewModel(ITransactionManager transactionManager, IDataService dataService)
        {
            _transactionManager = transactionManager;
            _dataService = dataService;

            // Initialize collections
            ExpenseCategories = new ObservableCollection<TransactionCategory>();
            RevenueCategories = new ObservableCollection<TransactionCategory>();

            // Initialize commands
            AddTransactionCommand = new RelayCommand(AddTransaction, CanAddTransaction);
            UpdateTransactionCommand = new RelayCommand(UpdateTransaction, CanUpdateTransaction);
            DeleteTransactionCommand = new RelayCommand(DeleteTransaction, CanDeleteTransaction);
            CancelEditCommand = new RelayCommand(CancelEdit);
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

                foreach (var transaction in transactions)
                {
                    _transactionManager.AddTransaction(transaction);
                }

                ApplyFilters();
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

                if (IsEditMode)
                {
                    UpdateTransaction();
                }
                else
                {
                    _transactionManager.AddTransaction(transaction);
                    Transactions.Add(transaction);
                    ClearInputs();
                    UpdateMonthlyTotals();
                    StatusMessage = "Transaction added successfully";
                }
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

        private void UpdateTransaction()
        {
            if (SelectedTransaction == null) return;

            try
            {
                var newTransaction = new Transaction
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

                _transactionManager.UpdateTransaction(SelectedTransaction, newTransaction);

                // Update the observable collection
                var index = Transactions.IndexOf(SelectedTransaction);
                if (index != -1)
                {
                    Transactions[index] = newTransaction;
                }

                IsEditMode = false;
                ClearInputs();
                SelectedTransaction = null;
                UpdateMonthlyTotals();
                StatusMessage = "Transaction updated successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating transaction: {ex.Message}";
            }
        }

        private void DeleteTransaction()
        {
            if (SelectedTransaction == null) return;

            var result = MessageBox.Show(
                "Are you sure you want to delete this transaction?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _transactionManager.DeleteTransaction(SelectedTransaction);
                    Transactions.Remove(SelectedTransaction);
                    ClearInputs();
                    SelectedTransaction = null;
                    UpdateMonthlyTotals();
                    StatusMessage = "Transaction deleted successfully";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error deleting transaction: {ex.Message}";
                }
            }
        }

        private void CancelEdit()
        {
            IsEditMode = false;
            SelectedTransaction = null;
            ClearInputs();
        }

        private bool CanUpdateTransaction()
        {
            return IsEditMode && SelectedTransaction != null && Amount > 0;
        }

        private bool CanDeleteTransaction()
        {
            return SelectedTransaction != null;
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
                ReportTitle = selectedMonth.ToString("dd/MM/yyyy"),
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

        private void UpdateMonthlyTotals()
        {
            if (!FilterMonth.HasValue) return;

            var flow = _transactionManager.CalculateMonthlyFlow(FilterMonth.Value);
            MonthlyRevenue = flow.revenues;
            MonthlyExpenses = flow.expenses;
            NetCashFlow = flow.netCashFlow;
        }

        private void ApplyFilters()
        {
            Transactions.Clear();
            var filteredTransactions = _transactionManager.GetTransactions();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLower();
                filteredTransactions = filteredTransactions.Where(t =>
                    t.Description.ToLower().Contains(search) ||
                    t.Category.Name.ToLower().Contains(search));
            }

            if (FilterMonth.HasValue)
            {
                var monthStart = new DateTime(FilterMonth.Value.Year, FilterMonth.Value.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                filteredTransactions = filteredTransactions.Where(t =>
                    t.Date.Date >= monthStart && t.Date.Date <= monthEnd);
            }

            if (!string.IsNullOrWhiteSpace(FilterCategory))
            {
                filteredTransactions = filteredTransactions.Where(t =>
                    t.Category.Name.Equals(FilterCategory, StringComparison.OrdinalIgnoreCase));
            }

            if (FilterType.HasValue)
            {
                filteredTransactions = filteredTransactions.Where(t =>
                    t.Category.Type == FilterType.Value);
            }

            foreach (var transaction in filteredTransactions)
            {
                Transactions.Add(transaction);
            }

            UpdateMonthlyTotals();
        }

        private void ClearFilters()
        {
            FilterMonth = DateTime.Today;
            FilterCategory = string.Empty;
            FilterType = null;
            SearchText = string.Empty;
            StatusMessage = "Filters cleared";
            ApplyFilters();
        }

        private void ClearInputs()
        {
            Amount = 0;
            Description = string.Empty;
            SelectedTransactionCategory = default;
            SelectedDate = DateTime.Today;
            IsEditMode = false;
        }

        private void NotifyCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// Represents a summary of a transaction category with amount and percentage information.
        /// </summary>
        public class CategorySummary
        {
            /// <summary>
            /// Gets or sets the category name.
            /// </summary>
            public string Category { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the total amount for the category.
            /// </summary>
            public decimal Amount { get; set; }

            /// <summary>
            /// Gets or sets the percentage this category represents of the total.
            /// </summary>
            public decimal Percentage { get; set; }
        }

        /// <summary>
        /// Raises the PropertyChanged event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
