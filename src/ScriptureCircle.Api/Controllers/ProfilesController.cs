using Microsoft.AspNetCore.Mvc;
using ScriptureCircle.Application.Abstractions;
using ScriptureCircle.Shared.Profiles;

namespace ScriptureCircle.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class ProfilesController(IProfileService profiles) : ControllerBase
{
    [HttpGet("{profileSlug}")]
    public async Task<ActionResult<PublicProfileDto>> Get(string profileSlug, CancellationToken cancellationToken)
    {
        var currentUserId = User.Identity?.IsAuthenticated == true ? this.GetUserId() : (Guid?)null;
        var profile = await profiles.GetAsync(profileSlug, currentUserId, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpGet("{profileSlug}/annotations")]
    public async Task<IActionResult> Annotations(string profileSlug, CancellationToken cancellationToken)
    {
        var annotations = await profiles.GetPublicAnnotationsAsync(profileSlug, cancellationToken);
        return annotations is null ? NotFound() : Ok(annotations);
    }

    [HttpGet("{profileSlug}/notebooks")]
    public async Task<IActionResult> Notebooks(string profileSlug, CancellationToken cancellationToken)
    {
        var notebooks = await profiles.GetPublicNotebooksAsync(profileSlug, cancellationToken);
        return notebooks is null ? NotFound() : Ok(notebooks);
    }
}
