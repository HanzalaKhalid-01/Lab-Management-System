using System.ComponentModel.DataAnnotations;

namespace LabManagement.ViewModels
{
    public class BillingViewModel
    {
        [Required]
        public int PatientId { get; set; }

        [Required]
        public List<int> SelectedTestIds { get; set; } = new();

        public decimal TotalAmount { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal PaidAmount { get; set; }
    }
}
