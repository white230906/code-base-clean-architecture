using System.Security.Claims;
using CodeBase.Application.Abstractions.Authentication;

namespace CodeBase.API.Services;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId => Guid.TryParse(
        Principal?.FindFirstValue(ClaimTypes.NameIdentifier),
        out var userId)
            ? userId
            : null;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public IReadOnlyCollection<string> Roles => Principal?
        .FindAll(ClaimTypes.Role)
        .Select(claim => claim.Value)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray() ?? [];
}
