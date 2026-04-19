using Competition.Application.Auth.DTOs;
using MediatR;

namespace Competition.Application.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<LoginResponse>;