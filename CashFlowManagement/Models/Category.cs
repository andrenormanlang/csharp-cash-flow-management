using CashFlowManagement.Interfaces;
using CashFlowManagement.Enums;

namespace CashFlowManagement.Models
{
    public record Category : ICategory
    {
        public string Name { get; init; } = string.Empty;
        public CategoryType Type { get; init; }
        public TransactionCategory TransactionCategory { get; init; }
    }
}
