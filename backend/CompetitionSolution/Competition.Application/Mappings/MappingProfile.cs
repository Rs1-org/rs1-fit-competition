using AutoMapper;
using Competition.Application.Auth.Commands;
using Competition.Application.Commands.CityCommands.Create;
using Competition.Application.Commands.CountryCommands.Create;
using Competition.Application.Commands.MatchCommands.Create;
using Competition.Application.Commands.StadiumCommands.Create;
using Competition.Application.DTOs.CityDto;
using Competition.Application.DTOs.CountryDto;
using Competition.Application.DTOs.MatchDto;
using Competition.Application.DTOs.PersonDto;
using Competition.Application.DTOs.StadiumDto;
using Competition.Application.Features.ManagerCommands.Create;
using Competition.Application.Features.ManagerCommands.Update;
using Competition.Application.Features.PlayerCommands.Create;
using Competition.Application.Features.PlayerCommands.Update;
using Competition.Application.Features.RefereeCommands.Create;
using Competition.Application.Features.RefereeCommands.Update;
using Competition.Application.Helpers;
using Competition.Domain.Entities;
using Competition.Domain.Entities.MatchEntities;
using Competition.Domain.Entities.Person;
using NodaTime;

namespace Competition.Application.Mappings;

public class MappingProfile:Profile
{
    public MappingProfile()
    {
        CreateMap<string, LocalDate>().ConvertUsing(s => NodaTimeMappingHelper.ParseLocalDate(s));
        CreateMap<LocalDate, string>().ConvertUsing(d => NodaTimeMappingHelper.FormatLocalDate(d));
        CreateMap<string?, LocalDate?>().ConvertUsing(s => NodaTimeMappingHelper.ParseNullableLocalDate(s));
        CreateMap<LocalDate?, string?>().ConvertUsing(d => NodaTimeMappingHelper.FormatNullableLocalDate(d));

        CreateMap<RegisterUserCommand, ApplicationUser>();
        //City
        CreateMap<CreateCityCommand, GeoData>();
        CreateMap<CreateCityCommand, City>()
            .ForMember(m => m.Slug, opt => opt.MapFrom(src => SlugHelper.GenerateSlug(src.Name)));
        CreateMap<City, CityNameDto>();
        CreateMap<City, CityListDto>()
            .ForMember(m => m.CountryName, opt => opt.MapFrom(src => src.Country != null ? src.Country.Name : null));
        CreateMap<City, CityDetailsDto>()
            .ForMember(m => m.CountryName, opt => opt.MapFrom(src => src.Country!.Name))
            .ForMember(m => m.Latitude, opt => opt.MapFrom(src => src.GeoData!.Latitude))
            .ForMember(m => m.Longitude, opt => opt.MapFrom(src => src.GeoData!.Longitude))
            .ForMember(m => m.Description, opt => opt.MapFrom(src => src.GeoData!.Description));

        //Country
        CreateMap<CreateCountryCommands, Country>()
            .ForMember(m => m.Slug, opt => opt.MapFrom(src => SlugHelper.GenerateSlug(src.Name)));
        CreateMap<Country, CountryNameDto>();
        CreateMap<Country, CountryListDto>();
        CreateMap<Country, CountryDetailsDto>()
            .ForMember(s => s.Cities, opt => opt.MapFrom(src => src.Cities));

        //Stadium
        CreateMap<CreateStadiumCommand, GeoData>()
            .ForMember(m => m.Description, opt => opt.MapFrom(src => src.GeoDescription));
        CreateMap<CreateStadiumCommand, Stadium>()
            .ForMember(m => m.Slug, opt => opt.MapFrom(src => SlugHelper.GenerateSlug(src.Name)));
        CreateMap<Stadium, StadiumListDto>()
            .ForMember(m => m.CityName, opt => opt.MapFrom(src => src.City != null ? src.City.Name : null))
            .ForMember(m => m.CountryName, opt => opt.MapFrom(src => src.City != null && src.City.Country != null ? src.City.Country.Name : null));
        CreateMap<Stadium, StadiumDetailsDto>()
            .ForMember(m => m.CityName, opt => opt.MapFrom(src => src.City != null ? src.City.Name : null))
            .ForMember(m => m.Latitude, opt => opt.MapFrom(src => src.GeoData != null ? src.GeoData.Latitude : (decimal)0))
            .ForMember(m => m.Longitude, opt => opt.MapFrom(src => src.GeoData != null ? src.GeoData.Longitude : (decimal)0))
            .ForMember(m => m.GeoDescription, opt => opt.MapFrom(src => src.GeoData != null ? src.GeoData.Description : null));

        ConfigureMatchMappings();
        ConfigurePersonMappings();
    }

    private void ConfigureMatchMappings()
    {
        CreateMap<CreateMatchCommand, Match>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.ModifiedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore())
            .ForMember(d => d.Date, opt => opt.Ignore())         // set manually in handler
            .ForMember(d => d.Status, opt => opt.Ignore())
            .ForMember(d => d.Stadium, opt => opt.Ignore())
            .ForMember(d => d.Slug, opt => opt.MapFrom(src => SlugHelper.GenerateSlug(src.Slug)));

        CreateMap<Match, MatchDetailsDto>()
            .ForMember(d => d.Date, opt => opt.MapFrom(src => NodaTimeMappingHelper.FormatInstant(src.Date)))
            .ForMember(d => d.StatusName, opt => opt.MapFrom(src => src.Status != null ? src.Status.Name : null))
            .ForMember(d => d.StadiumName, opt => opt.MapFrom(src => src.Stadium != null ? src.Stadium.Name : null));

        CreateMap<Match, MatchListDto>()
            .ForMember(d => d.Date, opt => opt.MapFrom(src => NodaTimeMappingHelper.FormatInstant(src.Date)))
            .ForMember(d => d.StatusName, opt => opt.MapFrom(src => src.Status != null ? src.Status.Name : null))
            .ForMember(d => d.StadiumName, opt => opt.MapFrom(src => src.Stadium != null ? src.Stadium.Name : null));

        CreateMap<MatchStatus, MatchStatusListDto>();
    }

    private void ConfigurePersonMappings()
    {
        CreateMap<PlayerDto, CreatePlayerCommand>().ConstructUsing(d => new CreatePlayerCommand(d));
        CreateMap<PlayerDto, UpdatePlayerCommand>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ConstructUsing((src, ctx) =>
            {
                var id = ctx.Items.TryGetValue("Id", out var value) && value is Guid guid ? guid : Guid.Empty;
                return new UpdatePlayerCommand(id, src);
            });
        CreateMap<CreatePlayerCommand, Player>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.ModifiedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore())
            .ForMember(d => d.Picture, opt => opt.MapFrom(s => s.Picture ?? Array.Empty<byte>()));
        CreateMap<UpdatePlayerCommand, Player>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.ModifiedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore())
            .ForMember(d => d.Picture, opt => opt.Condition(src => src.Picture != null));
        CreateMap<Player, PlayerDto>();

        CreateMap<RefereeDto, CreateRefereeCommand>().ConstructUsing(d => new CreateRefereeCommand(d));
        CreateMap<RefereeDto, UpdateRefereeCommand>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ConstructUsing((src, ctx) =>
            {
                var id = ctx.Items.TryGetValue("Id", out var value) && value is Guid guid ? guid : Guid.Empty;
                return new UpdateRefereeCommand(id, src);
            });
        CreateMap<CreateRefereeCommand, Referee>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.ModifiedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore())
            .ForMember(d => d.Picture, opt => opt.MapFrom(s => s.Picture ?? Array.Empty<byte>()));
        CreateMap<UpdateRefereeCommand, Referee>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.ModifiedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore())
            .ForMember(d => d.Picture, opt => opt.Condition(src => src.Picture != null));
        CreateMap<Referee, RefereeDto>();

        CreateMap<ManagerDto, CreateManagerCommand>().ConstructUsing(d => new CreateManagerCommand(d));
        CreateMap<ManagerDto, UpdateManagerCommand>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ConstructUsing((src, ctx) =>
            {
                var id = ctx.Items.TryGetValue("Id", out var value) && value is Guid guid ? guid : Guid.Empty;
                return new UpdateManagerCommand(id, src);
            });
        CreateMap<CreateManagerCommand, Manager>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.ModifiedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore())
            .ForMember(d => d.Picture, opt => opt.MapFrom(s => s.Picture ?? Array.Empty<byte>()));
        CreateMap<UpdateManagerCommand, Manager>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.ModifiedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore())
            .ForMember(d => d.Picture, opt => opt.Condition(src => src.Picture != null));
        CreateMap<Manager, ManagerDto>();
    }
}