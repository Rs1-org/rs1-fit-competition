using Competition.Application.Auth.DTOs;
using MediatR;

namespace Competition.Application.Auth.Commands;

public record RegisterUserCommand(RegisterUserDto UserDto) : IRequest<RegisterResponse>;