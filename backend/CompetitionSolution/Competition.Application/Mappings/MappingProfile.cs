using AutoMapper;
using Competition.Application.Auth.Commands;
using Competition.Domain.Entities;

namespace Competition.Application.Mappings;

public class MappingProfile:Profile
{
    public MappingProfile()
    {
        CreateMap<RegisterUserCommand, ApplicationUser>();
    }
}