using CashFlowManagement.Interfaces;
using CashFlowManagement.Enums;
using CashFlowManagement.Services.Interfaces;

namespace CashFlowManagement.Services
{
    /// <summary>
    /// Manages the storage, retrieval, and manipulation of financial transactions.
    /// Implements ITransactionManager interface.
    /// </summary>
    public class TransactionManager : ITransactionManager
    {
        private readonly List<ITransaction> _transactions;
        private readonly Dictionary<DateTime, List<ITransaction>> _monthlyTransactions;

        /// <summary>
        /// Initializes a new instance of the TransactionManager class.
        /// </summary>
        public TransactionManager()
        {
            _transactions = new List<ITransaction>();
            _monthlyTransactions = new Dictionary<DateTime, List<ITransaction>>();
        }

        /// <summary>
        /// Retrieves all transactions stored in the system.
        /// </summary>
        /// <returns>An enumerable collection of all transactions.</returns>
        public IEnumerable<ITransaction> GetTransactions() => _transactions;

        /// <summary>
        /// Adds a new transaction to both the main list and monthly organization.
        /// </summary>
        /// <param name="transaction">The transaction to add.</param>
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

        /// <summary>
        /// Deletes a transaction from both the main list and monthly organization.
        /// </summary>
        /// <param name="transaction">The transaction to delete.</param>
        public void DeleteTransaction(ITransaction transaction)
        {
            _transactions.Remove(transaction);

            var monthKey = new DateTime(transaction.Date.Year, transaction.Date.Month, 1);
            if (_monthlyTransactions.ContainsKey(monthKey))
            {
                _monthlyTransactions[monthKey].Remove(transaction);
                if (!_monthlyTransactions[monthKey].Any())
                {
                    _monthlyTransactions.Remove(monthKey);
                }
            }
        }

        /// <summary>
        /// Updates an existing transaction with new information.
        /// </summary>
        /// <param name="oldTransaction">The transaction to be updated.</param>
        /// <param name="newTransaction">The new transaction information.</param>
        public void UpdateTransaction(ITransaction oldTransaction, ITransaction newTransaction)
        {
            // Remove old transaction
            DeleteTransaction(oldTransaction);

            // Add new transaction
            AddTransaction(newTransaction);
        }

        /// <summary>
        /// Calculates the monthly financial flow including revenues, expenses, and net cash flow.
        /// </summary>
        /// <param name="month">The month for which to calculate the flow.</param>
        /// <returns>A tuple containing (revenues, expenses, netCashFlow).</returns>
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

        /// <summary>
        /// Filters transactions based on various criteria.
        /// </summary>
        /// <param name="searchText">Optional text to search for in transaction descriptions or categories.</param>
        /// <param name="month">Optional month to filter by.</param>
        /// <param name="category">Optional category to filter by.</param>
        /// <param name="type">Optional transaction type to filter by.</param>
        /// <returns>A filtered collection of transactions.</returns>
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
