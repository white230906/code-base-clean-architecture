namespace CodeBase.API.Extensions;

public static class CookieExtensions
{
    public const string AccessTokenCookieName = "X-Access-Token";
    public const string RefreshTokenCookieName = "X-Refresh-Token";

    public static void WriteAuthCookies(
        this HttpResponse response,
        string accessToken,
        string refreshToken,
        TimeSpan accessTokenLifetime,
        TimeSpan refreshTokenLifetime)
    {
        var accessOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.Add(accessTokenLifetime)
        };

        var refreshOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.Add(refreshTokenLifetime)
        };

        response.Cookies.Append(AccessTokenCookieName, accessToken, accessOptions);
        response.Cookies.Append(RefreshTokenCookieName, refreshToken, refreshOptions);
    }
    public static void DeleteAuthCookies(this HttpResponse response)
    {
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/"
        };

        response.Cookies.Delete(AccessTokenCookieName, options);
        response.Cookies.Delete(RefreshTokenCookieName, options);
    }
}
