using CashFlowManagement.Enums;

namespace CashFlowManagement.Interfaces
{
    /// <summary>
    /// Represents a category for financial transactions, defining the type and nature of the transaction.
    /// </summary>
    public interface ICategory
    {
        /// <summary>
        /// Gets the name of the category.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the type of the category (Expense or Revenue).
        /// </summary>
        CategoryType Type { get; }

        /// <summary>
        /// Gets the specific transaction category enumeration value.
        /// </summary>
        TransactionCategory TransactionCategory { get; }
    }
}
