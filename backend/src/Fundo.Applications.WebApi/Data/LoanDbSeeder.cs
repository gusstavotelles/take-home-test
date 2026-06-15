using Fundo.Applications.WebApi.Domain;

namespace Fundo.Applications.WebApi.Data;

public static class LoanDbSeeder
{
    public static async Task SeedAsync(LoanDbContext context, CancellationToken cancellationToken = default)
    {
        if (context.Loans.Any())
        {
            return;
        }

        var seedDate = DateTime.UtcNow;
        var loans = new[]
        {
            new Loan
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Amount = 25_000m,
                CurrentBalance = 18_750m,
                ApplicantName = "John Doe",
                Status = LoanStatus.Active,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Loan
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Amount = 15_000m,
                CurrentBalance = 0m,
                ApplicantName = "Jane Smith",
                Status = LoanStatus.Paid,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Loan
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Amount = 50_000m,
                CurrentBalance = 32_500m,
                ApplicantName = "Robert Johnson",
                Status = LoanStatus.Active,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Loan
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Amount = 1_500m,
                CurrentBalance = 500m,
                ApplicantName = "Maria Silva",
                Status = LoanStatus.Active,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            }
        };

        await context.Loans.AddRangeAsync(loans, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
