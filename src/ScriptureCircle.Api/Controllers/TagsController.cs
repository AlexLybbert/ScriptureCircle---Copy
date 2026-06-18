using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScriptureCircle.Application.Abstractions;
using ScriptureCircle.Shared.Tags;

namespace ScriptureCircle.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tags")]
public sealed class TagsController(ITagService tags) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TagDto>>> Get(CancellationToken cancellationToken) =>
        Ok(await tags.GetMineAsync(this.GetUserId(), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<TagDto>> Create(CreateTagRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await tags.CreateAsync(this.GetUserId(), request, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        await tags.DeleteAsync(this.GetUserId(), id, cancellationToken) ? NoContent() : NotFound();
}
