using AutoMapper;
using Competition.Application.Auth.Commands;
using Competition.Application.Auth.DTOs;
using Competition.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Competition.Application.Auth.Handlers;

public class RegisterUserHandler(UserManager<ApplicationUser>userManager,IMapper mapper, RoleManager<ApplicationRole> roleManager):IRequestHandler<RegisterUserCommand, RegisterResponse>
{
    public async Task<RegisterResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = mapper.Map<ApplicationUser>(request.UserDto);
        user.UserName = request.UserDto.Email;
        user.Email = request.UserDto.Email;

        var result = await userManager.CreateAsync(user, request.UserDto.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.GroupBy(it=>it.Code)
                .ToDictionary(g=>g.Key,g=>g.Select(e=>e.Description).ToArray());
            return new RegisterResponse
            {
                Success = false,
                Errors = errors,
            };
        }

        if (!await roleManager.RoleExistsAsync("User"))
        {
            result = await roleManager.CreateAsync(new ApplicationRole
            {
                Name = "User"
            });
            if (!result.Succeeded)
            {
                var errors = result.Errors.GroupBy(e => e.Code.Contains("Email") ? "email" : "password")
                    .ToDictionary(g=>g.Key,g=>g.Select(e=>e.Description).ToArray());
                return new RegisterResponse
                {
                    Success = false,
                    Errors = errors,
                };
            }
        }
        
        await userManager.AddToRoleAsync(user, "User");

        return new RegisterResponse
        {
            Success = true,
            UserId = user.Id,
        };
    }
}