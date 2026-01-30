namespace LabManagement.ViewModels
{
    public class PatientCreditViewModel
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public string PhoneNumber { get; set; }

        public int TotalBills { get; set; }
        public decimal TotalCredit { get; set; }
        public int PendingBills { get; set; }
    }
}
