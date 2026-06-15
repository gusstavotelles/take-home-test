using Fundo.Applications.WebApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace Fundo.Applications.WebApi.Data;

public class LoanDbContext : DbContext
{
    public LoanDbContext(DbContextOptions<LoanDbContext> options) : base(options) { }

    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<User> Users => Set<User>();

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
            entity.Property(l => l.OwnerId);
            entity.HasIndex(l => l.Status);
            entity.HasIndex(l => l.OwnerId);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(200);
            entity.Property(u => u.CreatedAt).IsRequired();
            entity.HasIndex(u => u.Username).IsUnique();
        });
    }
}
