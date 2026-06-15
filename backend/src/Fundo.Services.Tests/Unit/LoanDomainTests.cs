using FluentAssertions;
using Fundo.Applications.WebApi.Domain;
using Xunit;

namespace Fundo.Services.Tests.Unit;

public class LoanDomainTests
{
    [Fact]
    public void ApplyPayment_WithValidAmount_DecreasesBalance()
    {
        var loan = new Loan { Amount = 1_000m, CurrentBalance = 1_000m, Status = LoanStatus.Active };

        loan.ApplyPayment(250m);

        loan.CurrentBalance.Should().Be(750m);
        loan.Status.Should().Be(LoanStatus.Active);
    }

    [Fact]
    public void ApplyPayment_WhenBalanceReachesZero_MarksAsPaid()
    {
        var loan = new Loan { Amount = 500m, CurrentBalance = 500m, Status = LoanStatus.Active };

        loan.ApplyPayment(500m);

        loan.CurrentBalance.Should().Be(0m);
        loan.Status.Should().Be(LoanStatus.Paid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void ApplyPayment_WithNonPositiveAmount_Throws(decimal amount)
    {
        var loan = new Loan { Amount = 1_000m, CurrentBalance = 1_000m };

        var act = () => loan.ApplyPayment(amount);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ApplyPayment_WhenAmountExceedsBalance_Throws()
    {
        var loan = new Loan { Amount = 1_000m, CurrentBalance = 100m, Status = LoanStatus.Active };

        var act = () => loan.ApplyPayment(200m);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*exceeds*");
    }

    [Fact]
    public void ApplyPayment_WhenLoanIsPaid_Throws()
    {
        var loan = new Loan { Amount = 1_000m, CurrentBalance = 0m, Status = LoanStatus.Paid };

        var act = () => loan.ApplyPayment(50m);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already paid*");
    }
}
