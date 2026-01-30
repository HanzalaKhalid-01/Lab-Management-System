using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab_Management_System.Models
{
    public class Bill
    {
        public int BillId { get; set; }
        public int BillNumber { get; set; }
        public int BillMonth { get; set; }
        public int BillYear { get; set; }
        public DateOnly BillDate { get; set; }
        public int PatientId { get; set; }
        public Patient Patient { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal NetAmount { get; set; }
        public string PaymentStatus { get; set; }
        public bool IsLocked { get; set; }
        public ICollection<BillItem> BillItems { get; set; }
        public ICollection<Payment> Payments { get; set; } // Added this property  
        public decimal BalanceAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
