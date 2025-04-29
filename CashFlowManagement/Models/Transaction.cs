using CashFlowManagement.Interfaces;

namespace CashFlowManagement.Models
{
    public record Transaction : ITransaction
    {
        public DateTime Date { get; init; }
        public decimal Amount { get; init; }
        public string Description { get; init; } = string.Empty;
        public ICategory Category { get; init; } = new Category();
    }

}
