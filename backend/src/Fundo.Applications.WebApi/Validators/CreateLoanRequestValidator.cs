using FluentValidation;
using Fundo.Applications.WebApi.Domain;
using Fundo.Applications.WebApi.DTOs;

namespace Fundo.Applications.WebApi.Validators;

public class CreateLoanRequestValidator : AbstractValidator<CreateLoanRequest>
{
    public CreateLoanRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.CurrentBalance)
            .GreaterThanOrEqualTo(0).When(x => x.CurrentBalance.HasValue)
            .WithMessage("Current balance cannot be negative.")
            .LessThanOrEqualTo(x => x.Amount).When(x => x.CurrentBalance.HasValue)
            .WithMessage("Current balance cannot exceed the loan amount.");

        RuleFor(x => x.ApplicantName)
            .NotEmpty().WithMessage("Applicant name is required.")
            .MaximumLength(200).WithMessage("Applicant name must be 200 characters or fewer.");

        RuleFor(x => x.Status)
            .Must(s => string.IsNullOrEmpty(s) || LoanStatus.IsValid(s))
            .WithMessage($"Status must be '{LoanStatus.Active}' or '{LoanStatus.Paid}'.");
    }
}
