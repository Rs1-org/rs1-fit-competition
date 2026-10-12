using System;
using NodaTime;

namespace Competition.Domain.Entities;

public abstract class BaseEntity
{
    public const int PublicIdLength = 32;

    public Guid Id { get; set; }

    /// <summary>
    /// Identifier exposed to the frontend instead of <see cref="Id"/> (RS1 item 135).
    /// Generated on insert by AppDbContext when left empty.
    /// </summary>
    public string PublicId { get; set; } = string.Empty;

    public Instant CreatedAt { get; set; }
    public Instant ModifiedAt { get; set; }
    public Instant? DeletedAt { get; set; }

    public static string NewPublicId() => Guid.NewGuid().ToString("N");
}
