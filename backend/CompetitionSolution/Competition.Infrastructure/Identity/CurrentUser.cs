using System.Security.Claims;
using Competition.Application.Auth.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Competition.Infrastructure.Identity;

public class CurrentUser(IHttpContextAccessor http) : ICurrentUser
{
    public Guid UserId
    {
        get
        {
            var principal = http.HttpContext?.User;
            var value = principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal?.FindFirstValue("sub");
            if (value is null || !Guid.TryParse(value, out var id))
                throw new UnauthorizedAccessException("Sign in is required.");
            return id;
        }
    }
}
