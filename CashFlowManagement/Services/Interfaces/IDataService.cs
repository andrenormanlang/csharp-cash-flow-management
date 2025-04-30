using CashFlowManagement.Interfaces;

namespace CashFlowManagement.Services.Interfaces
{
    /// <summary>
    /// Provides functionality for persisting and retrieving financial transaction data.
    /// </summary>
    public interface IDataService
    {
        Task<IEnumerable<ITransaction>> LoadTransactionsAsync();
        Task SaveTransactionsAsync(IEnumerable<ITransaction> transactions);
    }
}
