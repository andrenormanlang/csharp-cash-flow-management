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
        /// <summary>
        /// Gets the name of the category.
        /// </summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Gets the type of the category (Expense or Revenue).
        /// </summary>
        public CategoryType Type { get; init; }

        /// <summary>
        /// Gets the specific transaction category enumeration value.
        /// </summary>
        public TransactionCategory TransactionCategory { get; init; }
    }
}
