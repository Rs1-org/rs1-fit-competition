namespace Competition.Application.Auth.DTOs;

public class RegisterResponse
{
    public bool Success { get; set; }
    public Guid? UserId { get; set; }
    public Dictionary<string, string[]> Errors { get; set; } = new();
}