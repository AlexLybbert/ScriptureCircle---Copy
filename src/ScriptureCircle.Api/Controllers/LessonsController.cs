using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScriptureCircle.Application.Services;
using ScriptureCircle.Shared.Lessons;

namespace ScriptureCircle.Api.Controllers;

[ApiController]
[Route("api/lessons")]
public sealed class LessonsController(LessonService lessons) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LessonDto>>> Get(CancellationToken cancellationToken)
    {
        var currentUserId = User.Identity?.IsAuthenticated == true ? this.GetUserId() : (Guid?)null;
        return Ok(await lessons.GetVisibleAsync(currentUserId, cancellationToken));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<LessonDto>> Create(CreateLessonRequest request, CancellationToken cancellationToken)
    {
        var created = await lessons.CreateAsync(this.GetUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }
}
