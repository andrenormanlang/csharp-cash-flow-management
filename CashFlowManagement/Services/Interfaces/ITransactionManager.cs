using CashFlowManagement.Interfaces;
using CashFlowManagement.Enums;

namespace CashFlowManagement.Services.Interfaces
{
    /// <summary>
    /// Provides functionality for managing financial transactions.
    /// </summary>
    public interface ITransactionManager
    {
        IEnumerable<ITransaction> GetTransactions();
        void AddTransaction(ITransaction transaction);
        void DeleteTransaction(ITransaction transaction);
        void UpdateTransaction(ITransaction oldTransaction, ITransaction newTransaction);
        (decimal revenues, decimal expenses, decimal netCashFlow) CalculateMonthlyFlow(DateTime month);
        IEnumerable<ITransaction> FilterTransactions(string? searchText = null, DateTime? month = null, string? category = null, CategoryType? type = null);
    }
}
