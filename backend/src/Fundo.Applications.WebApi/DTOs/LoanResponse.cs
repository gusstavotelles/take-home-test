using Fundo.Applications.WebApi.Domain;

namespace Fundo.Applications.WebApi.DTOs;

public class LoanResponse
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public decimal CurrentBalance { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string Status { get; set; } = LoanStatus.Active;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static LoanResponse FromDomain(Loan loan) => new()
    {
        Id = loan.Id,
        Amount = loan.Amount,
        CurrentBalance = loan.CurrentBalance,
        ApplicantName = loan.ApplicantName,
        Status = loan.Status,
        CreatedAt = loan.CreatedAt,
        UpdatedAt = loan.UpdatedAt
    };
}
