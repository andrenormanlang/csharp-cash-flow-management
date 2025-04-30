using CashFlowManagement.Interfaces;
using CashFlowManagement.Enums;

namespace CashFlowManagement.Services.Interfaces
{
    /// <summary>
    /// Provides functionality for managing financial transactions.
    /// </summary>
    public interface ITransactionManager
    {
        /// <summary>
        /// Retrieves all transactions in the system.
        /// </summary>
        /// <returns>An enumerable collection of all transactions.</returns>
        IEnumerable<ITransaction> GetTransactions();

        /// <summary>
        /// Adds a new transaction to the system.
        /// </summary>
        /// <param name="transaction">The transaction to add.</param>
        void AddTransaction(ITransaction transaction);

        /// <summary>
        /// Deletes an existing transaction from the system.
        /// </summary>
        /// <param name="transaction">The transaction to delete.</param>
        void DeleteTransaction(ITransaction transaction);

        /// <summary>
        /// Updates an existing transaction with new information.
        /// </summary>
        /// <param name="oldTransaction">The transaction to be updated.</param>
        /// <param name="newTransaction">The new transaction information.</param>
        void UpdateTransaction(ITransaction oldTransaction, ITransaction newTransaction);

        /// <summary>
        /// Calculates the monthly financial flow including revenues, expenses, and net cash flow.
        /// </summary>
        /// <param name="month">The month for which to calculate the flow.</param>
        /// <returns>A tuple containing (revenues, expenses, netCashFlow).</returns>
        (decimal revenues, decimal expenses, decimal netCashFlow) CalculateMonthlyFlow(DateTime month);

        /// <summary>
        /// Filters transactions based on various criteria.
        /// </summary>
        /// <param name="searchText">Optional text to search for in transaction descriptions or categories.</param>
        /// <param name="month">Optional month to filter by.</param>
        /// <param name="category">Optional category to filter by.</param>
        /// <param name="type">Optional transaction type to filter by.</param>
        /// <returns>A filtered collection of transactions.</returns>
        IEnumerable<ITransaction> FilterTransactions(string? searchText,
            DateTime? month, string? category, CategoryType? type);
    }
}
