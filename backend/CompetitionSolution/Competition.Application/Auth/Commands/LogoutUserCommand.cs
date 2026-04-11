using MediatR;

namespace Competition.Application.Auth.Commands;

public record LogoutUserCommand(string token) : IRequest<bool>;