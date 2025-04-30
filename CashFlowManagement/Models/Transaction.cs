using CashFlowManagement.Interfaces;

namespace CashFlowManagement.Models
{
    /// <summary>
    /// Represents an immutable financial transaction.
    /// Implements the ITransaction interface.
    /// </summary>
    public record Transaction : ITransaction
    {
        public DateTime Date { get; init; }
        public decimal Amount { get; init; }
        public string Description { get; init; } = string.Empty;
        public ICategory Category { get; init; } = new Category();
    }
}
