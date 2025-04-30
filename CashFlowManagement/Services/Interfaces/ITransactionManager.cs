using CashFlowManagement.Interfaces;
using CashFlowManagement.Enums;

namespace CashFlowManagement.Services.Interfaces
{
    public interface ITransactionManager
    {
        IEnumerable<ITransaction> GetTransactions();
        void AddTransaction(ITransaction transaction);
        void DeleteTransaction(ITransaction transaction);
        void UpdateTransaction(ITransaction oldTransaction, ITransaction newTransaction);
        (decimal revenues, decimal expenses, decimal netCashFlow) CalculateMonthlyFlow(DateTime month);
        IEnumerable<ITransaction> FilterTransactions(string? searchText,
            DateTime? month, string? category, CategoryType? type);
    }
}
