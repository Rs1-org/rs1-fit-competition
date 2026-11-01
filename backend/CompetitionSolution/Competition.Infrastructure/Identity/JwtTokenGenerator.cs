using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Competition.Application.Auth.Interfaces;
using Competition.Application.Payments;
using Competition.Domain.Entities;
using Competition.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Competition.Infrastructure.Identity;

public class JwtTokenGenerator(IOptions<JwtSettings> options) : IJwtTokenGenerator
{
    private readonly string jwtKey = options.Value.Key;
    private readonly string jwtIssuer = options.Value.Issuer;
    private readonly string jwtAudience = options.Value.Audience;

    public string GenerateJwtToken(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
        };
        claims.AddRange(roles.Select(it => new Claim(ClaimTypes.Role, it)));
        if (!string.IsNullOrWhiteSpace(user.SecurityStamp))
            claims.Add(new Claim("security_stamp", user.SecurityStamp));
        if (user.PremiumUntil.HasValue)
            claims.Add(new Claim("premium_until", PremiumPlan.Format(user.PremiumUntil)!));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
