using System.Text.Json.Serialization;
using Competition.Application.Common.Models;
using Competition.Application.Common.Validation;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Domain.Entities.MatchEntities;
using Competition.Domain.Entities.Person;
using FluentValidation;
using MediatR;
using NodaTime;
using Competition.Application.Live;

namespace Competition.Application.Events;

public class GetEventsQuery : IRequest<PagedResult<EventDto>>
{
    public string? MatchPublicId { get; set; }
    public string? EventTypePublicId { get; set; }
    public string? SelectionPublicId { get; set; }
    public string? PlayerPublicId { get; set; }
    public int? MinuteFrom { get; set; }
    public int? MinuteTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
public record GetEventQuery(string PublicId) : IRequest<EventDto>;
public record GetEventLookupsQuery : IRequest<EventLookupsDto>;
public record GetEventContextQuery(string MatchPublicId) : IRequest<EventContextDto>;
public abstract class SaveEventCommand
{
    public string MatchPublicId { get; set; } = string.Empty;
    public string EventTypePublicId { get; set; } = string.Empty;
    public string MatchPartPublicId { get; set; } = string.Empty;
    public string SelectionPublicId { get; set; } = string.Empty;
    public string? PlayerPublicId { get; set; }
    public string? RelatedPlayerPublicId { get; set; }
    public int Minute { get; set; }
    public int AddedMinute { get; set; }
    public string? Note { get; set; }
}
public class CreateEventCommand : SaveEventCommand, IRequest<SavedEventDto>;
public class UpdateEventCommand : SaveEventCommand, IRequest<SavedEventDto>
{
    [JsonIgnore] public string PublicId { get; set; } = string.Empty;
}
public record DeleteEventCommand(string PublicId) : IRequest<SavedEventDto>;

public class SaveEventValidator<T> : AbstractValidator<T> where T : SaveEventCommand
{
    public SaveEventValidator()
    {
        RuleFor(x => x.MatchPublicId).MustBeValidPublicId();
        RuleFor(x => x.EventTypePublicId).MustBeValidPublicId();
        RuleFor(x => x.MatchPartPublicId).MustBeValidPublicId();
        RuleFor(x => x.SelectionPublicId).MustBeValidPublicId();
        RuleFor(x => x.PlayerPublicId!).MustBeValidPublicId().When(x => x.PlayerPublicId is not null);
        RuleFor(x => x.RelatedPlayerPublicId!).MustBeValidPublicId().When(x => x.RelatedPlayerPublicId is not null);
        RuleFor(x => x.RelatedPlayerPublicId).NotEqual(x => x.PlayerPublicId).When(x => x.RelatedPlayerPublicId is not null);
        RuleFor(x => x.Minute).InclusiveBetween(0, 130);
        RuleFor(x => x.AddedMinute).InclusiveBetween(0, 30);
        RuleFor(x => x.Note).MaximumLength(500);
    }
}
public class CreateEventValidator : SaveEventValidator<CreateEventCommand>;
public class UpdateEventValidator : SaveEventValidator<UpdateEventCommand>
{
    public UpdateEventValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}
public class DeleteEventValidator : AbstractValidator<DeleteEventCommand>
{
    public DeleteEventValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}
public class GetEventValidator : AbstractValidator<GetEventQuery>
{
    public GetEventValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}
public class GetEventContextValidator : AbstractValidator<GetEventContextQuery>
{
    public GetEventContextValidator() => RuleFor(x => x.MatchPublicId).MustBeValidPublicId();
}
public class GetEventsValidator : AbstractValidator<GetEventsQuery>
{
    public GetEventsValidator()
    {
        RuleFor(x => x.MatchPublicId!).MustBeValidPublicId().When(x => x.MatchPublicId is not null);
        RuleFor(x => x.EventTypePublicId!).MustBeValidPublicId().When(x => x.EventTypePublicId is not null);
        RuleFor(x => x.SelectionPublicId!).MustBeValidPublicId().When(x => x.SelectionPublicId is not null);
        RuleFor(x => x.PlayerPublicId!).MustBeValidPublicId().When(x => x.PlayerPublicId is not null);
        RuleFor(x => x.MinuteFrom).InclusiveBetween(0, 130).When(x => x.MinuteFrom.HasValue);
        RuleFor(x => x.MinuteTo).InclusiveBetween(0, 130).When(x => x.MinuteTo.HasValue);
        RuleFor(x => x.MinuteTo).GreaterThanOrEqualTo(x => x.MinuteFrom).When(x => x.MinuteFrom.HasValue && x.MinuteTo.HasValue);
        RuleFor(x => x.Page).InclusiveBetween(1, 1000000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

internal static class EventRules
{
    public static async Task ApplyAsync(Event item, SaveEventCommand input, IEventStore store, IPublicIdLookup lookup, CancellationToken ct)
    {
        var match = await lookup.GetRequiredIdAsync<Match>(input.MatchPublicId, ct);
        if (item.MatchId != Guid.Empty && item.MatchId != match)
            throw BusinessRule.Violation(nameof(input.MatchPublicId), "An event cannot be moved to another match.");
        var type = await lookup.GetRequiredIdAsync<EventType>(input.EventTypePublicId, ct);
        var part = await lookup.GetRequiredIdAsync<MatchPart>(input.MatchPartPublicId, ct);
        var selection = await lookup.GetRequiredIdAsync<Selection>(input.SelectionPublicId, ct);
        if (!await store.SelectionBelongsToMatchAsync(match, selection, ct))
            throw BusinessRule.Violation(nameof(input.SelectionPublicId), "Choose an active selection playing in this match.");
        Guid? player = input.PlayerPublicId is null ? null : await lookup.GetRequiredIdAsync<Player>(input.PlayerPublicId, ct);
        Guid? related = input.RelatedPlayerPublicId is null ? null : await lookup.GetRequiredIdAsync<Player>(input.RelatedPlayerPublicId, ct);
        if (player.HasValue && !await store.PlayerBelongsToSelectionAsync(player.Value, selection, ct))
            throw BusinessRule.Violation(nameof(input.PlayerPublicId), "Choose a player recorded for this selection.");
        if (related.HasValue && !await store.PlayerBelongsToSelectionAsync(related.Value, selection, ct))
            throw BusinessRule.Violation(nameof(input.RelatedPlayerPublicId), "Choose a player recorded for this selection.");
        var code = await store.TypeCodeAsync(type, ct);
        if (code == "substitution" && (!player.HasValue || !related.HasValue || player == related))
            throw BusinessRule.Violation(nameof(input.RelatedPlayerPublicId), "A substitution needs two different players: player off and player on.");
        if (code != "substitution" && related.HasValue)
            throw BusinessRule.Violation(nameof(input.RelatedPlayerPublicId), "A second player is only used for substitutions.");
        item.MatchId = match; item.EventTypeId = type; item.MatchPartId = part; item.SelectionId = selection;
        item.PlayerId = player; item.RelatedPlayerId = related; item.Minute = input.Minute; item.AddedMinute = input.AddedMinute;
        item.Note = string.IsNullOrWhiteSpace(input.Note) ? null : input.Note.Trim();
    }
}
public class CreateEventHandler(IEventStore store, IPublicIdLookup lookup, IUnitOfWork unit, IPublisher publisher) : IRequestHandler<CreateEventCommand, SavedEventDto>
{
    public async Task<SavedEventDto> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var item = new Event();
        await EventRules.ApplyAsync(item, request, store, lookup, cancellationToken);
        await store.AddAsync(item, cancellationToken);
        await unit.SaveChangesAsync(cancellationToken);
        await publisher.Publish(new MatchChangedNotification(request.MatchPublicId, "events"), cancellationToken);
        return new(item.PublicId);
    }
}
public class UpdateEventHandler(IEventStore store, IPublicIdLookup lookup, IUnitOfWork unit, IPublisher publisher) : IRequestHandler<UpdateEventCommand, SavedEventDto>
{
    public async Task<SavedEventDto> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var item = await store.FindAsync(request.PublicId, cancellationToken) ?? throw new KeyNotFoundException("Event not found.");
        await EventRules.ApplyAsync(item, request, store, lookup, cancellationToken);
        await unit.SaveChangesAsync(cancellationToken);
        await publisher.Publish(new MatchChangedNotification(request.MatchPublicId, "events"), cancellationToken);
        return new(item.PublicId);
    }
}
public class DeleteEventHandler(IEventStore store, IUnitOfWork unit, IClock clock, IPublisher publisher) : IRequestHandler<DeleteEventCommand, SavedEventDto>
{
    public async Task<SavedEventDto> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        var item = await store.FindAsync(request.PublicId, cancellationToken) ?? throw new KeyNotFoundException("Event not found.");
        item.DeletedAt = clock.GetCurrentInstant();
        await unit.SaveChangesAsync(cancellationToken);
        await publisher.Publish(new MatchChangedNotification(item.Match.PublicId, "events"), cancellationToken);
        return new(item.PublicId);
    }
}
public class GetEventsHandler(IEventStore store) : IRequestHandler<GetEventsQuery, PagedResult<EventDto>>
{
    public Task<PagedResult<EventDto>> Handle(GetEventsQuery request, CancellationToken cancellationToken) => store.GetPageAsync(request, cancellationToken);
}
public class GetEventHandler(IEventStore store) : IRequestHandler<GetEventQuery, EventDto>
{
    public async Task<EventDto> Handle(GetEventQuery request, CancellationToken cancellationToken) =>
        await store.GetAsync(request.PublicId, cancellationToken) ?? throw new KeyNotFoundException("Event not found.");
}
public class GetEventLookupsHandler(IEventStore store) : IRequestHandler<GetEventLookupsQuery, EventLookupsDto>
{
    public Task<EventLookupsDto> Handle(GetEventLookupsQuery request, CancellationToken cancellationToken) => store.LookupsAsync(cancellationToken);
}
public class GetEventContextHandler(IEventStore store) : IRequestHandler<GetEventContextQuery, EventContextDto>
{
    public async Task<EventContextDto> Handle(GetEventContextQuery request, CancellationToken cancellationToken) =>
        await store.ContextAsync(request.MatchPublicId, cancellationToken) ?? throw new KeyNotFoundException("Match not found.");
}
