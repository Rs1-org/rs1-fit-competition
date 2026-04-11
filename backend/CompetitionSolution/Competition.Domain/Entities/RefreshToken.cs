namespace Competition.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public string Token { get; set; }
    public DateTime Expires { get; set; }
    public bool IsRevoked { get; set; }

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; }
}