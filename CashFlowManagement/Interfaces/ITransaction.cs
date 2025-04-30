namespace CashFlowManagement.Interfaces
{
    /// <summary>
    /// Represents a financial transaction with its associated properties.
    /// </summary>
    public interface ITransaction
    {
        /// <summary>
        /// Gets the date when the transaction occurred.
        /// </summary>
        DateTime Date { get; }

        /// <summary>
        /// Gets the monetary amount of the transaction.
        /// </summary>
        decimal Amount { get; }

        /// <summary>
        /// Gets the description or details of the transaction.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the category classification of the transaction.
        /// </summary>
        ICategory Category { get; }
    }
}


