using Microsoft.AspNetCore.Mvc;
using ScriptureCircle.Application.Abstractions;
using ScriptureCircle.Shared.Scriptures;

namespace ScriptureCircle.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class ScripturesController(IScriptureProvider scriptureProvider) : ControllerBase
{
    [HttpGet("scriptures/{volumeId}/{bookId}/{chapter:int}")]
    [HttpGet("content/scriptures/{volumeId}/{bookId}/{chapter:int}")]
    public async Task<ActionResult<ScriptureChapterDto>> GetChapter(
        string volumeId,
        string bookId,
        int chapter,
        [FromQuery] int? selectedStart,
        [FromQuery] int? selectedEnd,
        CancellationToken cancellationToken)
    {
        return Ok(await scriptureProvider.GetChapterAsync(volumeId, bookId, chapter, selectedStart, selectedEnd, cancellationToken));
    }
}
