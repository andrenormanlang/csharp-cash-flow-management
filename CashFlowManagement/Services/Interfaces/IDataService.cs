using CashFlowManagement.Interfaces;

namespace CashFlowManagement.Services.Interfaces
{
    /// <summary>
    /// Provides functionality for persisting and retrieving financial transaction data.
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// Saves a collection of transactions asynchronously.
        /// </summary>
        /// <param name="transactions">The transactions to save.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SaveTransactionsAsync(IEnumerable<ITransaction> transactions);

        /// <summary>
        /// Loads a collection of transactions asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing the loaded transactions.</returns>
        Task<IEnumerable<ITransaction>> LoadTransactionsAsync();
    }
}
