using CashFlowManagement.Interfaces;
using CashFlowManagement.Enums;
using CashFlowManagement.Services.Interfaces;

namespace CashFlowManagement.Services
{
    public class TransactionManager : ITransactionManager
    {
        private readonly List<ITransaction> _transactions;
        private readonly Dictionary<DateTime, List<ITransaction>> _monthlyTransactions;

        public TransactionManager()
        {
            _transactions = new List<ITransaction>();
            _monthlyTransactions = new Dictionary<DateTime, List<ITransaction>>();
        }

        public IEnumerable<ITransaction> GetTransactions() => _transactions;

        public void AddTransaction(ITransaction transaction)
        {
            _transactions.Add(transaction);

            var monthKey = new DateTime(transaction.Date.Year, transaction.Date.Month, 1);
            if (!_monthlyTransactions.ContainsKey(monthKey))
            {
                _monthlyTransactions[monthKey] = new List<ITransaction>();
            }
            _monthlyTransactions[monthKey].Add(transaction);
        }

        public (decimal revenues, decimal expenses, decimal netCashFlow) CalculateMonthlyFlow(DateTime month)
        {
            var monthKey = new DateTime(month.Year, month.Month, 1);
            if (!_monthlyTransactions.ContainsKey(monthKey))
                return (0, 0, 0);

            decimal revenues = _monthlyTransactions[monthKey]
                .Where(t => t.Category.Type == CategoryType.Revenue)
                .Sum(t => t.Amount);

            decimal expenses = _monthlyTransactions[monthKey]
                .Where(t => t.Category.Type == CategoryType.Expense)
                .Sum(t => t.Amount);

            return (revenues, expenses, revenues - expenses);
        }

        public IEnumerable<ITransaction> FilterTransactions(string? searchText = null,
            DateTime? month = null, string? category = null, CategoryType? type = null)
        {
            var query = _transactions.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var search = searchText.ToLower();
                query = query.Where(t =>
                    t.Description.ToLower().Contains(search) ||
                    t.Category.Name.ToLower().Contains(search));
            }

            if (month.HasValue)
            {
                query = query.Where(t =>
                    t.Date.Year == month.Value.Year &&
                    t.Date.Month == month.Value.Month);
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(t => t.Category.Name == category);
            }

            if (type.HasValue)
            {
                query = query.Where(t => t.Category.Type == type.Value);
            }

            return query.ToList();
        }
    }
}
