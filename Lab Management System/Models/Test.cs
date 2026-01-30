using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Lab_Management_System.Models;

public partial class Test
{
    [Key]
    public int TestId { get; set; }

    [StringLength(150)]
    public string TestName { get; set; } = null!;

    [Column(TypeName = "decimal(10, 2)")]
    public decimal DefaultPrice { get; set; }

    [StringLength(250)]
    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    [InverseProperty("Test")]
    public virtual ICollection<BillItem> BillItems { get; set; } = new List<BillItem>();
}
