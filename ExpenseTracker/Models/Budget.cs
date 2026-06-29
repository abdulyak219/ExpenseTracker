namespace ExpenseTracker.Models
{
    public class Budget
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public decimal? MonthlyBudget { get; set; }
        public decimal? TotalBudget { get; set; }
    }
}
