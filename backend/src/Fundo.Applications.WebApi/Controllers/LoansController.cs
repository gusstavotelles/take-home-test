using FluentValidation;
using Fundo.Applications.WebApi.DTOs;
using Fundo.Applications.WebApi.Exceptions;
using Fundo.Applications.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fundo.Applications.WebApi.Controllers;

[ApiController]
[Route("loans")]
[Produces("application/json")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;
    private readonly IValidator<CreateLoanRequest> _createValidator;
    private readonly IValidator<PaymentRequest> _paymentValidator;

    public LoansController(
        ILoanService loanService,
        IValidator<CreateLoanRequest> createValidator,
        IValidator<PaymentRequest> paymentValidator)
    {
        _loanService = loanService;
        _createValidator = createValidator;
        _paymentValidator = paymentValidator;
    }

    /// <summary>List all loans.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LoanResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LoanResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var loans = await _loanService.GetAllAsync(cancellationToken);
        return Ok(loans.Select(LoanResponse.FromDomain));
    }

    /// <summary>Get a loan by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoanResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var loan = await _loanService.GetByIdAsync(id, cancellationToken)
            ?? throw new LoanNotFoundException(id);

        return Ok(LoanResponse.FromDomain(loan));
    }

    /// <summary>Create a new loan.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoanResponse>> Create(
        [FromBody] CreateLoanRequest request,
        CancellationToken cancellationToken)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var loan = await _loanService.CreateAsync(request, cancellationToken);
        var response = LoanResponse.FromDomain(loan);
        return CreatedAtAction(nameof(GetById), new { id = loan.Id }, response);
    }

    /// <summary>Apply a payment to a loan.</summary>
    [HttpPost("{id:guid}/payment")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LoanResponse>> ApplyPayment(
        Guid id,
        [FromBody] PaymentRequest request,
        CancellationToken cancellationToken)
    {
        await _paymentValidator.ValidateAndThrowAsync(request, cancellationToken);
        var loan = await _loanService.ApplyPaymentAsync(id, request, cancellationToken);
        return Ok(LoanResponse.FromDomain(loan));
    }
}
