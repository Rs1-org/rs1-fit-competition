using Competition.Application.Auth.DTOs;
using MediatR;

namespace Competition.Application.Auth.Commands;

public record LoginUserCommand(LoginUserDto LoginUser):IRequest<LoginResponse>;