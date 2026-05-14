using System.Security.Claims;

namespace FitFusion.Api.Auth;

public static class FirebaseUser
{
    /// <summary>Devuelve el Firebase uid del JWT, o null si no hay sesión.</summary>
    public static string? Uid(this ClaimsPrincipal? principal)
    {
        if (principal == null) return null;
        return principal.FindFirst("user_id")?.Value
            ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value;
    }

    public static string RequireUid(this ClaimsPrincipal? principal) =>
        principal.Uid() ?? throw new UnauthorizedAccessException("Sin uid en el token");
}
