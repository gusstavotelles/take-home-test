namespace Fundo.Applications.WebApi.DTOs;

public class CreateLoanRequest
{
    public decimal Amount { get; set; }
    public decimal? CurrentBalance { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string? Status { get; set; }
}
