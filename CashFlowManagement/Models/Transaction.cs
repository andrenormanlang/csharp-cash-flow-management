using CashFlowManagement.Interfaces;

namespace CashFlowManagement.Models
{
    /// <summary>
    /// Represents an immutable financial transaction.
    /// Implements the ITransaction interface.
    /// </summary>
    public record Transaction : ITransaction
    {
        /// <summary>
        /// Gets the date when the transaction occurred.
        /// </summary>
        public DateTime Date { get; init; }

        /// <summary>
        /// Gets the monetary amount of the transaction.
        /// </summary>
        public decimal Amount { get; init; }

        /// <summary>
        /// Gets the description or details of the transaction.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Gets the category classification of the transaction.
        /// </summary>
        public ICategory Category { get; init; } = new Category();
    }
}
