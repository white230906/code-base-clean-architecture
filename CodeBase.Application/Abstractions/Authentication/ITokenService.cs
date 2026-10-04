using System.Security.Claims;

namespace CodeBase.Application.Abstractions.Authentication;

public interface ITokenService
{
    string GenerateAccessToken(IEnumerable<Claim> claims);
    string GenerateRefreshToken();
    string GenerateEmailVerificationToken(IEnumerable<Claim> claims, TimeSpan lifetime);
    ClaimsPrincipal ValidateToken(string token);
}
