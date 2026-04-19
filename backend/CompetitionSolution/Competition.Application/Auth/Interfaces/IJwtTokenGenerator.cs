using Competition.Domain.Entities;

namespace Competition.Application.Auth.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateJwtToken(ApplicationUser user, IList<string> roles);
}