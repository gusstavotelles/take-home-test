using FluentAssertions;
using Fundo.Applications.WebApi.DTOs;
using Fundo.Applications.WebApi.Validators;
using Xunit;

namespace Fundo.Services.Tests.Unit;

public class CreateLoanRequestValidatorTests
{
    private readonly CreateLoanRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidPayload_Succeeds()
    {
        var result = _validator.Validate(new CreateLoanRequest
        {
            Amount = 100m,
            ApplicantName = "John",
            Status = "active"
        });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithZeroAmount_Fails()
    {
        var result = _validator.Validate(new CreateLoanRequest { Amount = 0m, ApplicantName = "x" });
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithEmptyApplicant_Fails()
    {
        var result = _validator.Validate(new CreateLoanRequest { Amount = 10m, ApplicantName = "" });
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithBalanceGreaterThanAmount_Fails()
    {
        var result = _validator.Validate(new CreateLoanRequest
        {
            Amount = 100m,
            CurrentBalance = 200m,
            ApplicantName = "x"
        });
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithUnknownStatus_Fails()
    {
        var result = _validator.Validate(new CreateLoanRequest
        {
            Amount = 100m,
            ApplicantName = "x",
            Status = "delinquent"
        });
        result.IsValid.Should().BeFalse();
    }
}
