using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScriptureCircle.Application.Abstractions;
using ScriptureCircle.Shared.Notebooks;

namespace ScriptureCircle.Api.Controllers;

[ApiController]
[Route("api/notebooks")]
public sealed class NotebooksController(INotebookService notebooks) : ControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotebookDto>>> Get(CancellationToken cancellationToken)
    {
        return Ok(await notebooks.GetMineAsync(this.GetUserId(), cancellationToken));
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NotebookDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var notebook = await notebooks.GetAsync(this.GetUserId(), id, cancellationToken);

        return notebook is null ? NotFound() : Ok(notebook);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<NotebookDto>> Create(CreateNotebookRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await notebooks.CreateAsync(this.GetUserId(), request, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<NotebookDto>> Update(Guid id, UpdateNotebookRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var notebook = await notebooks.UpdateAsync(this.GetUserId(), id, request, cancellationToken);
            return notebook is null ? NotFound() : Ok(notebook);
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
        return await notebooks.DeleteAsync(this.GetUserId(), id, cancellationToken) ? NoContent() : NotFound();
    }

    [Authorize]
    [HttpPost("{id:guid}/annotations")]
    public async Task<IActionResult> AddAnnotation(Guid id, AddNotebookAnnotationRequest request, CancellationToken cancellationToken)
    {
        return await notebooks.AddAnnotationAsync(this.GetUserId(), id, request.AnnotationId, cancellationToken) ? NoContent() : NotFound();
    }

    [Authorize]
    [HttpDelete("{id:guid}/annotations/{annotationId:guid}")]
    public async Task<IActionResult> RemoveAnnotation(Guid id, Guid annotationId, CancellationToken cancellationToken)
    {
        return await notebooks.RemoveAnnotationAsync(this.GetUserId(), id, annotationId, cancellationToken) ? NoContent() : NotFound();
    }

    [Authorize]
    [HttpPut("{id:guid}/annotations/reorder")]
    public async Task<IActionResult> Reorder(Guid id, ReorderNotebookAnnotationsRequest request, CancellationToken cancellationToken)
    {
        return await notebooks.ReorderAnnotationsAsync(this.GetUserId(), id, request, cancellationToken) ? NoContent() : NotFound();
    }

    [HttpGet("shared/{shareSlug}")]
    public async Task<ActionResult<NotebookDto>> Shared(string shareSlug, CancellationToken cancellationToken)
    {
        var notebook = await notebooks.GetSharedAsync(shareSlug, cancellationToken);

        return notebook is null ? NotFound() : Ok(notebook);
    }
}
