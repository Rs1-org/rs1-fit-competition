using Competition.Application.Commands.PaymentCommands;
using Competition.Application.Queries.Payments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[ApiController]
[Route("api/payment")]
public class PaymentController(IMediator mediator) : ControllerBase
{
    [HttpGet("pricing")]
    public async Task<IActionResult> GetPricing(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetPremiumPricingQuery(), cancellationToken));

    [Authorize]
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetPremiumStatusQuery(), cancellationToken));

    [Authorize]
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(CreateCheckoutSessionCommand request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(request, cancellationToken));

    // Called by Stripe, not a user: the signature over the raw body is the authentication.
    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        await mediator.Send(new HandleStripeWebhookCommand(payload, Request.Headers["Stripe-Signature"].ToString()), cancellationToken);
        return Ok();
    }
}
