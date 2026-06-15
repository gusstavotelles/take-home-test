namespace Fundo.Applications.WebApi.Exceptions;

public class LoanNotFoundException : Exception
{
    public Guid LoanId { get; }

    public LoanNotFoundException(Guid loanId)
        : base($"Loan with id '{loanId}' was not found.")
    {
        LoanId = loanId;
    }
}
