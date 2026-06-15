using Fundo.Applications.WebApi.Domain;
using Fundo.Applications.WebApi.DTOs;

namespace Fundo.Applications.WebApi.Services;

public interface ILoanService
{
    Task<IReadOnlyList<Loan>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Loan?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<Loan> CreateAsync(CreateLoanRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<Loan> ApplyPaymentAsync(Guid id, PaymentRequest request, Guid userId, CancellationToken cancellationToken = default);
}
