using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Lab_Management_System.Models;

public partial class LabManagementDbContext : DbContext
{
    public LabManagementDbContext()
    {
    }

    public LabManagementDbContext(DbContextOptions<LabManagementDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bill> Bills { get; set; }

    public virtual DbSet<BillItem> BillItems { get; set; }

    public virtual DbSet<Expense> Expenses { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Test> Tests { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bill>(entity =>
        {
            entity.HasKey(e => e.BillId).HasName("PK__Bills__11F2FC6A5D1E8BE7");

            entity.Property(e => e.BalanceAmount).HasComputedColumnSql("(([TotalAmount]-[DiscountAmount])-[PaidAmount])", true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Patient).WithMany(p => p.Bills)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bills_Patients");
        });

        modelBuilder.Entity<BillItem>(entity =>
        {
            entity.HasKey(e => e.BillItemId).HasName("PK__BillItem__47605AF5FFE68BA9");

            entity.HasOne(d => d.Bill).WithMany(p => p.BillItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BillItems_Bills");

            entity.HasOne(d => d.Test).WithMany(p => p.BillItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BillItems_Tests");
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.ExpenseId).HasName("PK__Expenses__1445CFD32E0A5D31");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.PatientId).HasName("PK__Patients__970EC36675B84647");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A38B6A0C092");

            entity.Property(e => e.PaymentDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Bill).WithMany(p => p.Payments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_Bills");
        });

        modelBuilder.Entity<Test>(entity =>
        {
            entity.HasKey(e => e.TestId).HasName("PK__Tests__8CC33160061E1A73");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
