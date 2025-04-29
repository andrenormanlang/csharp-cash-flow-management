using CashFlowManagement.Commands;
using CashFlowManagement.Enums;
using CashFlowManagement.Interfaces;
using CashFlowManagement.Models;
using CashFlowManagement.Services.Interfaces;
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
            SelectedType = IsExpenseCategory(value) ? CategoryType.Expense : CategoryType.Revenue;
            NotifyCanExecuteChanged();
        }
    }

    public CategoryType SelectedType
    {
        get => _selectedType;
        set
        {
            _selectedType = value;
            OnPropertyChanged();
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
            Transactions.Clear();
            var transactions = await _dataService.LoadTransactionsAsync();
            foreach (var transaction in transactions)
            {
                _transactionManager.AddTransaction(transaction);
                Transactions.Add(transaction);
            }
            UpdateCategories();
            GenerateReport();
            StatusMessage = "Data loaded successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading data: {ex.Message}";
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
        return Amount > 0 && SelectedTransactionCategory != null && SelectedDate != null;
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
        MonthlyRevenue = monthlyTransactions
            .Where(t => t.Category.Type == CategoryType.Revenue)
            .Sum(t => t.Amount);

        MonthlyExpenses = monthlyTransactions
            .Where(t => t.Category.Type == CategoryType.Expense)
            .Sum(t => t.Amount);

        NetCashFlow = MonthlyRevenue - MonthlyExpenses;

        UpdateTopCategories(monthlyTransactions);
    }

    private void UpdateTopCategories(List<ITransaction> transactions)
    {
        TopExpenses.Clear();
        TopRevenues.Clear();

        if (!transactions.Any()) return;

        // Top expenses
        var expenses = transactions
            .Where(t => t.Category.Type == CategoryType.Expense)
            .GroupBy(t => t.Category.Name)
            .Select(g => new CategorySummary
            {
                Category = g.Key,
                Amount = g.Sum(t => t.Amount),
                Percentage = MonthlyExpenses > 0 ? g.Sum(t => t.Amount) / MonthlyExpenses * 100 : 0
            })
            .OrderByDescending(c => c.Amount)
            .Take(3);

        foreach (var expense in expenses)
        {
            TopExpenses.Add(expense);
        }

        // Top revenues
        var revenues = transactions
            .Where(t => t.Category.Type == CategoryType.Revenue)
            .GroupBy(t => t.Category.Name)
            .Select(g => new CategorySummary
            {
                Category = g.Key,
                Amount = g.Sum(t => t.Amount),
                Percentage = MonthlyRevenue > 0 ? g.Sum(t => t.Amount) / MonthlyRevenue * 100 : 0
            })
            .OrderByDescending(c => c.Amount)
            .Take(3);

        foreach (var revenue in revenues)
        {
            TopRevenues.Add(revenue);
        }
    }

    private void ApplyFilters()
    {
        var query = _transactionManager.GetTransactions().AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            query = query.Where(t =>
                t.Description.ToLower().Contains(search) ||
                t.Category.Name.ToLower().Contains(search));
        }

        if (FilterMonth.HasValue)
        {
            query = query.Where(t =>
                t.Date.Year == FilterMonth.Value.Year &&
                t.Date.Month == FilterMonth.Value.Month);
        }

        if (!string.IsNullOrWhiteSpace(FilterCategory))
        {
            query = query.Where(t => t.Category.Name == FilterCategory);
        }

        if (FilterType.HasValue)
        {
            query = query.Where(t => t.Category.Type == FilterType.Value);
        }

        Transactions.Clear();
        foreach (var transaction in query)
        {
            Transactions.Add(transaction);
        }

        GenerateReport();
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
