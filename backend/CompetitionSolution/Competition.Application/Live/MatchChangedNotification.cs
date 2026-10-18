using MediatR;

namespace Competition.Application.Live;

/// <summary>Published only after the database unit of work succeeds. Public identifiers only.</summary>
public record MatchChangedNotification(string MatchPublicId, string Change) : INotification;
