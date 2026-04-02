using System.Collections.Generic;
using System.Security.Claims;
using System_Music.Models.SqlModels;

namespace System_Music.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user, IEnumerable<string> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
