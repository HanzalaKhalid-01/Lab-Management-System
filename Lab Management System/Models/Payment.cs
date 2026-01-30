using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Lab_Management_System.Models;

public partial class Payment
{
    [Key]
    public int PaymentId { get; set; }

    public int BillId { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal AmountPaid { get; set; }

    public DateTime PaymentDate { get; set; }

    [StringLength(50)]
    public string? PaymentMethod { get; set; }

    [StringLength(250)]
    public string? Notes { get; set; }

    [ForeignKey("BillId")]
    [InverseProperty("Payments")]
    public virtual Bill Bill { get; set; } = null!;
}
