using Competition.Application.DTOs;
using Competition.Domain.Entities;

namespace Competition.Application.Auth.DTOs;

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
    public UserDto User { get; set; } = new();
    public List<string> Roles { get; set; } = [];
}