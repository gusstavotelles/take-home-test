using Fundo.Applications.WebApi.Data;
using Fundo.Applications.WebApi.Domain;
using Fundo.Applications.WebApi.DTOs;
using Fundo.Applications.WebApi.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Fundo.Applications.WebApi.Services;

public class LoanService : ILoanService
{
    private readonly LoanDbContext _context;
    private readonly ILogger<LoanService> _logger;

    public LoanService(LoanDbContext context, ILogger<LoanService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Loan>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Loans
            .AsNoTracking()
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<Loan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Loans
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<Loan> CreateAsync(CreateLoanRequest request, CancellationToken cancellationToken = default)
    {
        var status = string.IsNullOrWhiteSpace(request.Status) ? LoanStatus.Active : request.Status;
        var balance = request.CurrentBalance ?? request.Amount;

        var loan = new Loan
        {
            Amount = request.Amount,
            CurrentBalance = balance,
            ApplicantName = request.ApplicantName.Trim(),
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (loan.CurrentBalance == 0 && loan.Status == LoanStatus.Active)
        {
            loan.Status = LoanStatus.Paid;
        }

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created loan {LoanId} for {Applicant}", loan.Id, loan.ApplicantName);
        return loan;
    }

    public async Task<Loan> ApplyPaymentAsync(Guid id, PaymentRequest request, CancellationToken cancellationToken = default)
    {
        var loan = await _context.Loans.FirstOrDefaultAsync(l => l.Id == id, cancellationToken)
            ?? throw new LoanNotFoundException(id);

        loan.ApplyPayment(request.Amount);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Applied payment of {Payment} to loan {LoanId}. New balance: {Balance}",
            request.Amount, loan.Id, loan.CurrentBalance);

        return loan;
    }
}
