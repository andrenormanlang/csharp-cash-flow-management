namespace CashFlowManagement.Interfaces
{
    public interface ITransaction
    {
        DateTime Date { get; }
        decimal Amount { get; }
        string Description { get; }
        ICategory Category { get; }
    }
}


