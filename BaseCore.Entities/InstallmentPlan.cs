namespace BaseCore.Entities
{
    public class InstallmentPlan
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal DownPaymentAmount { get; set; }
        public decimal FinancedAmount { get; set; }
        public int Months { get; set; }
        public decimal MonthlyInterestRate { get; set; }
        public decimal MonthlyPaymentAmount { get; set; }
        public int PaidPeriods { get; set; }
        public string? BuyerFullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CitizenId { get; set; }
        public string? Address { get; set; }
        public string? FinanceCompany { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? Note { get; set; }

        public Order? Order { get; set; }
    }
}
