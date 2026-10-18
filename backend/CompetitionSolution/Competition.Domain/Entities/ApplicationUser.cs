using Microsoft.AspNetCore.Identity;

namespace Competition.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string PublicId { get; set; } = BaseEntity.NewPublicId();
    // Historical accounts have no recorded registration time.
    public NodaTime.Instant? RegisteredAt { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
