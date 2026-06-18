using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScriptureCircle.Application.Abstractions;
using ScriptureCircle.Shared.Annotations;

namespace ScriptureCircle.Api.Controllers;

[ApiController]
[Route("api/annotations")]
public sealed class AnnotationsController(IAnnotationService annotations) : ControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AnnotationDto>>> Get(
        [FromQuery] string? volumeId,
        [FromQuery] string? bookId,
        [FromQuery] int? chapterNumber,
        CancellationToken cancellationToken)
    {
        var hasAnyFilter = volumeId is not null || bookId is not null || chapterNumber is not null;
        if (!hasAnyFilter)
        {
            return Ok(await annotations.GetForUserAsync(this.GetUserId(), cancellationToken));
        }

        if (string.IsNullOrWhiteSpace(volumeId) || string.IsNullOrWhiteSpace(bookId) || chapterNumber is null)
        {
            return BadRequest(new { message = "volumeId, bookId, and chapterNumber are required when filtering annotations." });
        }

        return Ok(await annotations.GetForReferenceAsync(this.GetUserId(), volumeId, bookId, chapterNumber.Value, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AnnotationDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.Identity?.IsAuthenticated == true ? this.GetUserId() : (Guid?)null;
        var annotation = await annotations.GetAsync(id, userId, cancellationToken);
        return annotation is null ? NotFound() : Ok(annotation);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<AnnotationDto>> Create(CreateAnnotationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var annotation = await annotations.CreateAsync(this.GetUserId(), request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = annotation.Id }, annotation);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AnnotationDto>> Update(Guid id, UpdateAnnotationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var annotation = await annotations.UpdateAsync(id, this.GetUserId(), request, cancellationToken);
            return annotation is null ? NotFound() : Ok(annotation);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return await annotations.DeleteAsync(id, this.GetUserId(), cancellationToken) ? NoContent() : NotFound();
    }

    [HttpGet("shared/{shareSlug}")]
    public async Task<ActionResult<AnnotationDto>> Shared(string shareSlug, CancellationToken cancellationToken)
    {
        var annotation = await annotations.GetSharedAsync(shareSlug, cancellationToken);
        return annotation is null ? NotFound() : Ok(annotation);
    }
}
