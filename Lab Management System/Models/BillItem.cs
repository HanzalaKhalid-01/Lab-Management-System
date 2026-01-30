using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Lab_Management_System.Models;

public partial class BillItem
{
    [Key]
    public int BillItemId { get; set; }

    public int BillId { get; set; }

    public int TestId { get; set; }

    [StringLength(150)]
    public string TestNameSnapshot { get; set; } = null!;

    [Column(TypeName = "decimal(10, 2)")]
    public decimal TestPriceSnapshot { get; set; }

    [ForeignKey("BillId")]
    [InverseProperty("BillItems")]
    public virtual Bill Bill { get; set; } = null!;

    [ForeignKey("TestId")]
    [InverseProperty("BillItems")]
    public virtual Test Test { get; set; } = null!;
}
