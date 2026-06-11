using Fundo.Applications.WebApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace Fundo.Applications.WebApi.Data;

public class LoanDbContext : DbContext
{
    public LoanDbContext(DbContextOptions<LoanDbContext> options) : base(options) { }

    public DbSet<Loan> Loans => Set<Loan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Loan>(entity =>
        {
            entity.ToTable("Loans");
            entity.HasKey(l => l.Id);
            entity.Property(l => l.ApplicantName).IsRequired().HasMaxLength(200);
            entity.Property(l => l.Status).IsRequired().HasMaxLength(20);
            entity.Property(l => l.Amount).HasColumnType("decimal(18,2)");
            entity.Property(l => l.CurrentBalance).HasColumnType("decimal(18,2)");
            entity.Property(l => l.CreatedAt).IsRequired();
            entity.Property(l => l.UpdatedAt).IsRequired();
            entity.HasIndex(l => l.Status);
        });
    }
}
