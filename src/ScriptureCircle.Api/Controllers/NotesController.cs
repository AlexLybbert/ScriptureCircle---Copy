using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScriptureCircle.Application.Services;
using ScriptureCircle.Shared.Notes;
using ScriptureCircle.Shared.Scriptures;

namespace ScriptureCircle.Api.Controllers;

[ApiController]
[Route("api/notes")]
public sealed class NotesController(NoteService notes) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NoteDto>>> GetForReference([FromQuery] string volumeId, [FromQuery] string bookId, [FromQuery] string bookTitle, [FromQuery] int chapter, CancellationToken cancellationToken)
    {
        var currentUserId = User.Identity?.IsAuthenticated == true ? this.GetUserId() : (Guid?)null;
        var reference = new ScriptureReferenceDto(volumeId, bookId, bookTitle, chapter, null, null);
        return Ok(await notes.GetForReferenceAsync(reference, currentUserId, cancellationToken));
    }

    [Authorize]
    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<NoteDto>>> GetMine(CancellationToken cancellationToken) =>
        Ok(await notes.GetMineAsync(this.GetUserId(), cancellationToken));

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<NoteDto>> Create(CreateNoteRequest request, CancellationToken cancellationToken)
    {
        var created = await notes.CreateAsync(this.GetUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetMine), new { id = created.Id }, created);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<NoteDto>> Update(Guid id, UpdateNoteRequest request, CancellationToken cancellationToken)
    {
        var updated = await notes.UpdateAsync(this.GetUserId(), id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        await notes.DeleteAsync(this.GetUserId(), id, cancellationToken) ? NoContent() : NotFound();
}
