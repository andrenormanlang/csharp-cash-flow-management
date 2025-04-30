using CashFlowManagement.Interfaces;
using CashFlowManagement.Enums;

namespace CashFlowManagement.Models
{
    /// <summary>
    /// Represents an immutable category for financial transactions.
    /// Implements the ICategory interface.
    /// </summary>
    public record Category : ICategory
    {
        public string Name { get; init; } = string.Empty;
        public CategoryType Type { get; init; }
        public TransactionCategory TransactionCategory { get; init; }
    }
}
