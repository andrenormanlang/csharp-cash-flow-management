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
        Freelance = 1,
        Investment = 2,
        Bonus = 3,
        CSN=4,
        Other = 5,

        // Expense Categories (100+)
        Rent = 100,
        Utilities = 101,
        Groceries = 102,
        Transportation = 103,
        Healthcare = 104,
        Entertainment = 105,
        Education = 106,
        Shopping = 107,
        Travel = 108,
        DiningOut = 109,
        Subscriptions = 110,
        PersonalCare = 111,
        Miscellaneous = 112,
    }
}

