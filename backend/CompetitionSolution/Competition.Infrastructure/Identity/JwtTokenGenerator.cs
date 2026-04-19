using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Competition.Application.Auth.Interfaces;
using Competition.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Competition.Infrastructure.Identity;

public class JwtTokenGenerator(IConfiguration configuration) : IJwtTokenGenerator
{
    private readonly string jwtKey = configuration["Jwt:Key"] 
    ?? throw new ArgumentNullException("Jwt:Key missing");
    private readonly string jwtIssuer = configuration["Jwt:Issuer"] 
    ?? throw new ArgumentNullException("Jwt:Issuer missing");
    private readonly string jwtAudience = configuration["Jwt:Audience"] 
    ?? throw new ArgumentNullException("Jwt:Audience missing");

    public string GenerateJwtToken(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
        };
        claims.AddRange(roles.Select(it => new Claim(ClaimTypes.Role, it)));
        
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