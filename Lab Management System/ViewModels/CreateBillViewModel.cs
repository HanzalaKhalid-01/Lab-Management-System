using Lab_Management_System.Models;
using System.Collections.Generic;

namespace Lab_Management_System.Models.ViewModels
{
    public class CreateBillViewModel
    {
        // Patient info
        public int? SelectedPatientId { get; set; } // existing patient
        public string PatientName { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public int? Age { get; set; }
        public string ReferredBy { get; set; }

        // Test selection
        public List<int> SelectedTestIds { get; set; } = new();
        public List<Test> Tests { get; set; } = new();

        // Payment / discount
        public decimal DiscountAmount { get; set; }
        public decimal PaidAmount { get; set; }

        // **New**: existing patients & their bills
        public List<Patient> ExistingPatients { get; set; } = new();
        public List<Bill> ExistingBills { get; set; } = new();
    }
}
