using CashFlowManagement.Enums;

namespace CashFlowManagement.Interfaces
{
    public interface ICategory
    {
        string Name { get; }
        CategoryType Type { get; }
        TransactionCategory TransactionCategory { get; }
    }
}
