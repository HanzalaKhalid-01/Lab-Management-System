namespace LabManagement.ViewModels
{
    public class DailyClosingViewModel
    {
        public DateTime Date { get; set; }

        public int TotalBills { get; set; }

        public decimal GrossAmount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal NetAmount { get; set; }

        public decimal CashReceived { get; set; }
        public decimal CreditGiven { get; set; }

        public int PaidBills { get; set; }
        public int PartialBills { get; set; }
        public int UnpaidBills { get; set; }
    }
}
