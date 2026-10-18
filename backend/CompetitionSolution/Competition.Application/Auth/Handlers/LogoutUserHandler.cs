using Competition.Application.Auth.Commands;
using Competition.Application.Auth.Interfaces;
using MediatR;

namespace Competition.Application.Auth.Handlers;

public class LogoutUserHandler(IRefreshTokenService refreshTokenService) : IRequestHandler<LogoutUserCommand, bool>
{
    public async Task<bool> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        await refreshTokenService.RevokeTokenAsync(request.token, cancellationToken);
        return true;
    }
}
