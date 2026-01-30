using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Lab_Management_System.Models;

public partial class Patient
{
    [Key]
    public int PatientId { get; set; }

    [StringLength(150)]
    public string FullName { get; set; } = null!;

    public int? Age { get; set; }

    [StringLength(10)]
    public string? Gender { get; set; }

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(150)]
    public string? ReferredBy { get; set; }

    [StringLength(250)]
    public string? Address { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    [InverseProperty("Patient")]
    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();
}
