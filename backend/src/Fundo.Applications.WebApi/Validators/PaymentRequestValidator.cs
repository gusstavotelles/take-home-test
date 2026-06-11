using FluentValidation;
using Fundo.Applications.WebApi.DTOs;

namespace Fundo.Applications.WebApi.Validators;

public class PaymentRequestValidator : AbstractValidator<PaymentRequest>
{
    public PaymentRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than zero.");
    }
}
