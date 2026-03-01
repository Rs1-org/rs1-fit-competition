using System;
using NodaTime;

namespace Competition.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant ModifiedAt { get; set; }
    public Instant? DeletedAt { get; set; }
}