using System.Text.Json.Serialization;
using Competition.Application.Auth.Interfaces;
using Competition.Application.Common.Models;
using Competition.Application.Common.Validation;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using FluentValidation;
using MediatR;
using NodaTime;

namespace Competition.Application.Admin.Users;

public class GetAllUsersQuery : IRequest<PagedResult<UserListDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Search { get; init; }
    public string? Role { get; init; }
    public bool? IsActive { get; init; }
    public DateTimeOffset? RegisteredFrom { get; init; }
    public DateTimeOffset? RegisteredTo { get; init; }
}

public class ChangeUserRoleCommand : IRequest<bool>
{
    [JsonIgnore] public string PublicId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class SetUserActiveCommand : IRequest<bool>
{
    [JsonIgnore] public string PublicId { get; set; } = string.Empty;
    public bool? IsActive { get; set; }
}

public class GetAllUsersQueryValidator : AbstractValidator<GetAllUsersQuery>
{
    public GetAllUsersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Search).MaximumLength(256);
        RuleFor(x => x.Role).Must(x => x is "Admin" or "Moderator" or "User" or "Manager").When(x => x.Role is not null);
        RuleFor(x => x.RegisteredTo).GreaterThanOrEqualTo(x => x.RegisteredFrom)
            .When(x => x.RegisteredFrom.HasValue && x.RegisteredTo.HasValue);
    }
}

public class ChangeUserRoleCommandValidator : AbstractValidator<ChangeUserRoleCommand>
{
    public ChangeUserRoleCommandValidator()
    {
        RuleFor(x => x.PublicId).MustBeValidPublicId();
        RuleFor(x => x.Role).Must(x => x is "Admin" or "Moderator" or "User")
            .WithMessage("Choose Admin, Moderator or User.");
    }
}

public class SetUserActiveCommandValidator : AbstractValidator<SetUserActiveCommand>
{
    public SetUserActiveCommandValidator()
    {
        RuleFor(x => x.PublicId).MustBeValidPublicId();
        RuleFor(x => x.IsActive).NotNull();
    }
}

public class GetAllUsersQueryHandler(IAdminUserStore users, ICurrentUser current)
    : IRequestHandler<GetAllUsersQuery, PagedResult<UserListDto>>
{
    public Task<PagedResult<UserListDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken) =>
        users.GetFilteredAsync(new UserFilter(request.Search, request.Role, request.IsActive,
            request.RegisteredFrom.HasValue ? Instant.FromDateTimeOffset(request.RegisteredFrom.Value) : null,
            request.RegisteredTo.HasValue ? Instant.FromDateTimeOffset(request.RegisteredTo.Value) : null),
            current.UserId, request.Page, request.PageSize, cancellationToken);
}

internal static class UserManagementRules
{
    public static async Task<ApplicationUser> FindOtherUserAsync(string publicId, IAdminUserStore users, ICurrentUser current, CancellationToken ct)
    {
        var user = await users.FindAsync(publicId, ct) ?? throw new KeyNotFoundException("User not found.");
        if (user.Id == current.UserId)
            throw BusinessRule.Violation("PublicId", "You cannot change your own role or account status.");
        return user;
    }
}

public class ChangeUserRoleCommandHandler(IAdminUserStore users, ICurrentUser current, IUnitOfWork unit)
    : IRequestHandler<ChangeUserRoleCommand, bool>
{
    public async Task<bool> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await UserManagementRules.FindOtherUserAsync(request.PublicId, users, current, cancellationToken);
        await users.SetRoleAsync(user, request.Role, cancellationToken);
        await unit.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public class SetUserActiveCommandHandler(IAdminUserStore users, ICurrentUser current, IUnitOfWork unit)
    : IRequestHandler<SetUserActiveCommand, bool>
{
    public async Task<bool> Handle(SetUserActiveCommand request, CancellationToken cancellationToken)
    {
        var user = await UserManagementRules.FindOtherUserAsync(request.PublicId, users, current, cancellationToken);
        await users.SetActiveAsync(user, request.IsActive!.Value, cancellationToken);
        await unit.SaveChangesAsync(cancellationToken);
        return true;
    }
}
