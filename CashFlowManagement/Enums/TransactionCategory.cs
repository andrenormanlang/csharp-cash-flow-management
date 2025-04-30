namespace CashFlowManagement.Enums
{
    public enum CategoryType
    {
        Expense,
        Revenue
    }

    public enum TransactionCategory
    {
        // Revenue Categories (0-99)
        Salary = 0,
        Investment = 1,
        Bonus = 2,
        Other = 3,

        // Expense Categories (100+)
        Rent = 100,
        Utilities = 101,
        Groceries = 102,
        Transportation = 103,
        Healthcare = 104,
        Entertainment = 105,
        Education = 106,
        Shopping = 107,
        Miscellaneous = 108
    }
}

