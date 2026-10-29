using AutoMapper;
using Competition.Application.Auth.Commands;
using Competition.Application.Commands.CityCommands.Create;
using Competition.Application.Commands.CountryCommands.Create;
using Competition.Application.Commands.MatchCommands.Create;
using Competition.Application.Commands.StadiumCommands.Create;
using Competition.Application.DTOs.CityDto;
using Competition.Application.DTOs.CountryDto;
using Competition.Application.DTOs.MatchDto;
using Competition.Application.DTOs.CompetitionDto;
using Competition.Application.DTOs.ClubDto;
using Competition.Application.DTOs.SeasonDto;
using Competition.Application.DTOs.RoundDto;
using Competition.Application.DTOs.PersonDto;
using Competition.Application.DTOs.StadiumDto;
using Competition.Application.Commands.ManagerCommands.Create;
using Competition.Application.Commands.ManagerCommands.Update;
using Competition.Application.Commands.PlayerCommands.Create;
using Competition.Application.Commands.PlayerCommands.Update;
using Competition.Application.Commands.RefereeCommands.Create;
using Competition.Application.Commands.RefereeCommands.Update;
using Competition.Application.Helpers;
using Competition.Domain.Entities;
using Competition.Domain.Entities.MatchEntities;
using Competition.Domain.Entities.CompetitionEntities;
using CompetitionEntity = Competition.Domain.Entities.CompetitionEntities.Competition;
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
            .ForMember(m => m.CountryPublicId, opt => opt.MapFrom(src => src.Country != null ? src.Country.PublicId : string.Empty))
            .ForMember(m => m.Latitude, opt => opt.MapFrom(src => src.GeoData!.Latitude))
            .ForMember(m => m.Longitude, opt => opt.MapFrom(src => src.GeoData!.Longitude))
            .ForMember(m => m.Description, opt => opt.MapFrom(src => src.GeoData!.Description));

        //Country
        CreateMap<CreateCountryCommands, Country>()
            .ForMember(m => m.Slug, opt => opt.MapFrom(src => SlugHelper.GenerateSlug(src.Name)));
        CreateMap<Country, CountryNameDto>();
        CreateMap<Country, CountryListDto>()
            .ForMember(d => d.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToDateTimeUtc()));
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
            .ForMember(m => m.CityPublicId, opt => opt.MapFrom(src => src.City != null ? src.City.PublicId : string.Empty))
            .ForMember(m => m.CityName, opt => opt.MapFrom(src => src.City != null ? src.City.Name : null))
            .ForMember(m => m.Latitude, opt => opt.MapFrom(src => src.GeoData != null ? src.GeoData.Latitude : (decimal)0))
            .ForMember(m => m.Longitude, opt => opt.MapFrom(src => src.GeoData != null ? src.GeoData.Longitude : (decimal)0))
            .ForMember(m => m.GeoDescription, opt => opt.MapFrom(src => src.GeoData != null ? src.GeoData.Description : null))
            .ForMember(m => m.CityLatitude, opt => opt.MapFrom(src => src.City != null && src.City.GeoData != null ? src.City.GeoData.Latitude : (decimal?)null))
            .ForMember(m => m.CityLongitude, opt => opt.MapFrom(src => src.City != null && src.City.GeoData != null ? src.City.GeoData.Longitude : (decimal?)null));

        CreateMap<Club, ClubListDto>()
            .ForMember(d => d.FoundingDate, opt => opt.MapFrom(s => NodaTimeMappingHelper.FormatNullableLocalDate(s.FoundingDate)))
            .ForMember(d => d.CityName, opt => opt.MapFrom(s => s.City != null ? s.City.Name : null))
            .ForMember(d => d.CountryName, opt => opt.MapFrom(s => s.City != null && s.City.Country != null ? s.City.Country.Name : null))
            .ForMember(d => d.StadiumName, opt => opt.MapFrom(s => s.Stadium != null ? s.Stadium.Name : null));
        CreateMap<Club, ClubDetailsDto>()
            .IncludeBase<Club, ClubListDto>()
            .ForMember(d => d.CityPublicId, opt => opt.MapFrom(s => s.City != null ? s.City.PublicId : null))
            .ForMember(d => d.CountryPublicId, opt => opt.MapFrom(s => s.City != null && s.City.Country != null ? s.City.Country.PublicId : null))
            .ForMember(d => d.StadiumPublicId, opt => opt.MapFrom(s => s.Stadium != null ? s.Stadium.PublicId : null))
            .ForMember(d => d.StadiumSlug, opt => opt.MapFrom(s => s.Stadium != null ? s.Stadium.Slug : null));

        ConfigureMatchMappings();
        ConfigureCompetitionMappings();
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
            .ForMember(d => d.HomeSelection, opt => opt.Ignore())
            .ForMember(d => d.AwaySelection, opt => opt.Ignore())
            .ForMember(d => d.Round, opt => opt.Ignore())
            .ForMember(d => d.Slug, opt => opt.Ignore());          // validated or generated in handler

        CreateMap<Match, MatchDetailsDto>()
            .ForMember(d => d.StadiumSlug, opt => opt.MapFrom(src => src.Stadium != null ? src.Stadium.Slug : null))
            .ForMember(d => d.HomeSelectionSlug, opt => opt.MapFrom(src => src.HomeSelection != null ? src.HomeSelection.Slug : null))
            .ForMember(d => d.AwaySelectionSlug, opt => opt.MapFrom(src => src.AwaySelection != null ? src.AwaySelection.Slug : null))
            .ForMember(d => d.RoundSlug, opt => opt.MapFrom(src => src.Round != null ? src.Round.Slug : null))
            .ForMember(d => d.SeasonPublicId, opt => opt.MapFrom(src => src.Round != null && src.Round.Season != null ? src.Round.Season.PublicId : null))
            .ForMember(d => d.CompetitionPublicId, opt => opt.MapFrom(src =>
                src.Round != null && src.Round.Season != null && src.Round.Season.Competition != null ? src.Round.Season.Competition.PublicId : null))
            .ForMember(d => d.StatusPublicId, opt => opt.MapFrom(src => src.Status != null ? src.Status.PublicId : string.Empty))
            .ForMember(d => d.StadiumPublicId, opt => opt.MapFrom(src => src.Stadium != null ? src.Stadium.PublicId : string.Empty))
            .ForMember(d => d.HomeSelectionPublicId, opt => opt.MapFrom(src => src.HomeSelection != null ? src.HomeSelection.PublicId : null))
            .ForMember(d => d.AwaySelectionPublicId, opt => opt.MapFrom(src => src.AwaySelection != null ? src.AwaySelection.PublicId : null))
            .ForMember(d => d.RoundPublicId, opt => opt.MapFrom(src => src.Round != null ? src.Round.PublicId : null))
            .ForMember(d => d.Date, opt => opt.MapFrom(src => NodaTimeMappingHelper.FormatInstant(src.Date)))
            .ForMember(d => d.StatusName, opt => opt.MapFrom(src => src.Status != null ? src.Status.Name : null))
            .ForMember(d => d.StadiumName, opt => opt.MapFrom(src => src.Stadium != null ? src.Stadium.Name : null))
            .ForMember(d => d.HomeSelectionName, opt => opt.MapFrom(src => src.HomeSelection != null ? src.HomeSelection.Name : null))
            .ForMember(d => d.AwaySelectionName, opt => opt.MapFrom(src => src.AwaySelection != null ? src.AwaySelection.Name : null))
            .ForMember(d => d.RoundName, opt => opt.MapFrom(src => src.Round != null ? src.Round.Name : null))
            .ForMember(d => d.SeasonName, opt => opt.MapFrom(src => src.Round != null && src.Round.Season != null ? src.Round.Season.Name : null))
            .ForMember(d => d.CompetitionName, opt => opt.MapFrom(src =>
                src.Round != null && src.Round.Season != null && src.Round.Season.Competition != null ? src.Round.Season.Competition.Name : null));

        CreateMap<Match, MatchListDto>()
            .ForMember(d => d.RoundPublicId, opt => opt.MapFrom(src => src.Round != null ? src.Round.PublicId : null))
            .ForMember(d => d.Date, opt => opt.MapFrom(src => NodaTimeMappingHelper.FormatInstant(src.Date)))
            .ForMember(d => d.StatusName, opt => opt.MapFrom(src => src.Status != null ? src.Status.Name : null))
            .ForMember(d => d.StadiumName, opt => opt.MapFrom(src => src.Stadium != null ? src.Stadium.Name : null))
            .ForMember(d => d.HomeSelectionName, opt => opt.MapFrom(src => src.HomeSelection != null ? src.HomeSelection.Name : null))
            .ForMember(d => d.AwaySelectionName, opt => opt.MapFrom(src => src.AwaySelection != null ? src.AwaySelection.Name : null))
            .ForMember(d => d.RoundName, opt => opt.MapFrom(src => src.Round != null ? src.Round.Name : null))
            .ForMember(d => d.SeasonName, opt => opt.MapFrom(src => src.Round != null && src.Round.Season != null ? src.Round.Season.Name : null))
            .ForMember(d => d.CompetitionName, opt => opt.MapFrom(src =>
                src.Round != null && src.Round.Season != null && src.Round.Season.Competition != null ? src.Round.Season.Competition.Name : null));

        CreateMap<MatchStatus, MatchStatusListDto>();
    }

    private void ConfigureCompetitionMappings()
    {
        CreateMap<CompetitionEntity, CompetitionListDto>()
            .ForMember(d => d.CountryName, opt => opt.MapFrom(src => src.Country != null ? src.Country.Name : null))
            .ForMember(d => d.SeasonCount, opt => opt.MapFrom(src => src.Seasons.Count));

        CreateMap<CompetitionEntity, CompetitionDetailsDto>()
            .ForMember(d => d.CountryPublicId, opt => opt.MapFrom(src => src.Country != null ? src.Country.PublicId : null))
            .ForMember(d => d.CountryName, opt => opt.MapFrom(src => src.Country != null ? src.Country.Name : null));

        CreateMap<Season, SeasonListDto>()
            .ForMember(d => d.StartDate, opt => opt.MapFrom(src => NodaTimeMappingHelper.FormatLocalDate(src.StartDate)))
            .ForMember(d => d.EndDate, opt => opt.MapFrom(src => NodaTimeMappingHelper.FormatLocalDate(src.EndDate)))
            .ForMember(d => d.CompetitionName, opt => opt.MapFrom(src => src.Competition != null ? src.Competition.Name : string.Empty))
            .ForMember(d => d.RoundCount, opt => opt.MapFrom(src => src.Rounds.Count));

        CreateMap<Season, SeasonDetailsDto>()
            .ForMember(d => d.CompetitionPublicId, opt => opt.MapFrom(src => src.Competition != null ? src.Competition.PublicId : string.Empty))
            .ForMember(d => d.StartDate, opt => opt.MapFrom(src => NodaTimeMappingHelper.FormatLocalDate(src.StartDate)))
            .ForMember(d => d.EndDate, opt => opt.MapFrom(src => NodaTimeMappingHelper.FormatLocalDate(src.EndDate)))
            .ForMember(d => d.CompetitionName, opt => opt.MapFrom(src => src.Competition != null ? src.Competition.Name : string.Empty))
            .ForMember(d => d.CompetitionSlug, opt => opt.MapFrom(src => src.Competition != null ? src.Competition.Slug : string.Empty));

        CreateMap<Round, RoundListDto>()
            .ForMember(d => d.MatchCount, opt => opt.MapFrom(src => src.Matches.Count));

        CreateMap<Round, RoundDetailsDto>()
            .ForMember(d => d.SeasonPublicId, opt => opt.MapFrom(src => src.Season != null ? src.Season.PublicId : string.Empty))
            .ForMember(d => d.SeasonName, opt => opt.MapFrom(src => src.Season != null ? src.Season.Name : string.Empty))
            .ForMember(d => d.SeasonSlug, opt => opt.MapFrom(src => src.Season != null ? src.Season.Slug : string.Empty))
            .ForMember(d => d.CompetitionName, opt => opt.MapFrom(src => src.Season != null && src.Season.Competition != null ? src.Season.Competition.Name : string.Empty))
            .ForMember(d => d.CompetitionSlug, opt => opt.MapFrom(src => src.Season != null && src.Season.Competition != null ? src.Season.Competition.Slug : string.Empty));
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
            .ForMember(d => d.Position, opt => opt.Ignore())
            .ForMember(d => d.SubPosition, opt => opt.Ignore())
            .ForMember(d => d.Picture, opt => opt.MapFrom(s => s.Picture ?? Array.Empty<byte>()));
        CreateMap<UpdatePlayerCommand, Player>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.ModifiedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore())
            .ForMember(d => d.Position, opt => opt.Ignore())
            .ForMember(d => d.SubPosition, opt => opt.Ignore())
            .ForMember(d => d.Picture, opt => opt.Condition(src => src.Picture != null));
        CreateMap<Player, PlayerDto>()
            .ForMember(d => d.PositionName, opt => opt.MapFrom(s => s.Position != null ? s.Position.Name : null))
            .ForMember(d => d.SubPositionName, opt => opt.MapFrom(s => s.SubPosition != null ? s.SubPosition.Name : null));

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
