using FluentAssertions;
using Fundo.Applications.WebApi.Data;
using Fundo.Applications.WebApi.Domain;
using Fundo.Applications.WebApi.DTOs;
using Fundo.Applications.WebApi.Exceptions;
using Fundo.Applications.WebApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Fundo.Services.Tests.Unit;

public class LoanServiceTests : IDisposable
{
    private readonly LoanDbContext _context;
    private readonly LoanService _service;
    private readonly Guid _userId = Guid.NewGuid();

    public LoanServiceTests()
    {
        var options = new DbContextOptionsBuilder<LoanDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new LoanDbContext(options);
        _service = new LoanService(_context, NullLogger<LoanService>.Instance);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task CreateAsync_PersistsLoanWithDefaultBalanceAndStatus()
    {
        var request = new CreateLoanRequest
        {
            Amount = 1_500m,
            ApplicantName = " Maria Silva ",
        };

        var loan = await _service.CreateAsync(request, _userId);

        loan.Id.Should().NotBeEmpty();
        loan.CurrentBalance.Should().Be(1_500m);
        loan.Status.Should().Be(LoanStatus.Active);
        loan.ApplicantName.Should().Be("Maria Silva");
        loan.OwnerId.Should().Be(_userId);

        var stored = await _context.Loans.FindAsync(loan.Id);
        stored.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_WhenBalanceIsZero_PersistsAsPaid()
    {
        var request = new CreateLoanRequest
        {
            Amount = 1_000m,
            CurrentBalance = 0m,
            ApplicantName = "Closed Loan",
        };

        var loan = await _service.CreateAsync(request, _userId);

        loan.Status.Should().Be(LoanStatus.Paid);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsExistingLoan()
    {
        var loan = await SeedLoanAsync();

        var result = await _service.GetByIdAsync(loan.Id, _userId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(loan.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid(), _userId);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllLoansOrderedByCreatedAtDesc()
    {
        await SeedLoanAsync(Guid.NewGuid(), createdAt: DateTime.UtcNow.AddMinutes(-10));
        await SeedLoanAsync(Guid.NewGuid(), createdAt: DateTime.UtcNow);

        var result = await _service.GetAllAsync(_userId);

        result.Should().HaveCount(2);
        result[0].CreatedAt.Should().BeAfter(result[1].CreatedAt);
    }

    [Fact]
    public async Task ApplyPaymentAsync_DecreasesBalance()
    {
        var loan = await SeedLoanAsync(balance: 500m);

        var updated = await _service.ApplyPaymentAsync(loan.Id, new PaymentRequest { Amount = 200m }, _userId);

        updated.CurrentBalance.Should().Be(300m);
    }

    [Fact]
    public async Task ApplyPaymentAsync_WhenLoanMissing_Throws()
    {
        var act = () => _service.ApplyPaymentAsync(Guid.NewGuid(), new PaymentRequest { Amount = 50m }, _userId);

        await act.Should().ThrowAsync<LoanNotFoundException>();
    }

    [Fact]
    public async Task ApplyPaymentAsync_WhenAmountExceedsBalance_Throws()
    {
        var loan = await SeedLoanAsync(balance: 100m);

        var act = () => _service.ApplyPaymentAsync(loan.Id, new PaymentRequest { Amount = 500m }, _userId);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private async Task<Loan> SeedLoanAsync(
        Guid? id = null,
        decimal balance = 1_000m,
        DateTime? createdAt = null)
    {
        var loan = new Loan
        {
            Id = id ?? Guid.NewGuid(),
            Amount = 1_000m,
            CurrentBalance = balance,
            ApplicantName = "Seed",
            Status = balance == 0 ? LoanStatus.Paid : LoanStatus.Active,
            OwnerId = _userId,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            UpdatedAt = createdAt ?? DateTime.UtcNow,
        };
        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();
        return loan;
    }
}
