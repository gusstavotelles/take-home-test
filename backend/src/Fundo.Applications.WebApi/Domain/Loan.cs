namespace Fundo.Applications.WebApi.Domain;

public class Loan
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public decimal Amount { get; set; }
    public decimal CurrentBalance { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string Status { get; set; } = LoanStatus.Active;
    public Guid? OwnerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public void ApplyPayment(decimal payment)
    {
        if (payment <= 0)
        {
            throw new ArgumentException("Payment amount must be greater than zero.", nameof(payment));
        }

        if (Status == LoanStatus.Paid)
        {
            throw new InvalidOperationException("Loan is already paid in full.");
        }

        if (payment > CurrentBalance)
        {
            throw new InvalidOperationException("Payment amount exceeds the current balance.");
        }

        CurrentBalance -= payment;
        UpdatedAt = DateTime.UtcNow;

        if (CurrentBalance == 0)
        {
            Status = LoanStatus.Paid;
        }
    }
}
