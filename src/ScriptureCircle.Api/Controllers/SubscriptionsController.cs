using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScriptureCircle.Application.Abstractions;
using ScriptureCircle.Application.Services;
using ScriptureCircle.Shared.Annotations;
using ScriptureCircle.Shared.Subscriptions;

namespace ScriptureCircle.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/subscriptions")]
public sealed class SubscriptionsController(SubscriptionService subscriptions, IFollowService follows, IAnnotationService annotations) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SubscriptionDto>>> Get(CancellationToken cancellationToken) =>
        Ok(await subscriptions.GetMineAsync(this.GetUserId(), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<SubscriptionDto>> Upsert(UpsertSubscriptionRequest request, CancellationToken cancellationToken) =>
        Ok(await subscriptions.UpsertAsync(this.GetUserId(), request, cancellationToken));

    [HttpPost("{creatorUserId:guid}")]
    public async Task<IActionResult> Subscribe(Guid creatorUserId, CancellationToken cancellationToken)
    {
        var result = await follows.SubscribeAsync(this.GetUserId(), creatorUserId, cancellationToken);
        return result switch
        {
            FollowResult.Success => NoContent(),
            FollowResult.CannotFollowSelf => BadRequest(new { message = "You cannot subscribe to yourself." }),
            _ => NotFound()
        };
    }

    [HttpDelete("{creatorUserId:guid}")]
    public async Task<IActionResult> Unsubscribe(Guid creatorUserId, CancellationToken cancellationToken) =>
        await follows.UnsubscribeAsync(this.GetUserId(), creatorUserId, cancellationToken) ? NoContent() : NotFound();

    [HttpGet("feed")]
    public async Task<ActionResult<IReadOnlyList<AnnotationDto>>> Feed(CancellationToken cancellationToken) =>
        Ok(await annotations.GetSubscriptionFeedAsync(this.GetUserId(), cancellationToken));
}
