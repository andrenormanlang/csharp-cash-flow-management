using CashFlowManagement.Interfaces;

namespace CashFlowManagement.Services.Interfaces
{
    public interface IDataService
    {
        Task SaveTransactionsAsync(IEnumerable<ITransaction> transactions);
        Task<IEnumerable<ITransaction>> LoadTransactionsAsync();
    }
}
