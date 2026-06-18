using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace ScriptureCircle.Api.Controllers;

internal static class ControllerExtensions
{
    public static Guid GetUserId(this ControllerBase controller)
    {
        var value = controller.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? controller.User.FindFirstValue("sub");
        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("Missing user id claim.");
    }
}
