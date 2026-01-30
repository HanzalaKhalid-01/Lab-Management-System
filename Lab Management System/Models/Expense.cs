using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Lab_Management_System.Models;

public partial class Expense
{
    [Key]
    public int ExpenseId { get; set; }

    [StringLength(150)]
    public string Title { get; set; } = null!;

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Amount { get; set; }

    public DateOnly ExpenseDate { get; set; }

    [StringLength(250)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
}
