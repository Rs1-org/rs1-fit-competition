using AutoMapper;
using Competition.Application.Auth.Commands;
using Competition.Application.Commands.CityCommands.Create;
using Competition.Application.Commands.CountryCommands.Create;
using Competition.Application.DTOs.CityDto;
using Competition.Application.DTOs.CountryDto;
using Competition.Application.Helpers;
using Competition.Domain.Entities;

using Competition.Application.Commands.StadiumCommands.Create;
using Competition.Application.DTOs.StadiumDto;
using Competition.Domain.Entities.MatchEntities;

namespace Competition.Application.Mappings;

public class MappingProfile:Profile
{
    public MappingProfile()
    {
        CreateMap<RegisterUserCommand, ApplicationUser>();
        //City
        CreateMap<CreateCityCommand, GeoData>();
        CreateMap<CreateCityCommand, City>()
            .ForMember(m => m.Slug, opt => opt.MapFrom(src => SlugHelper.GenerateSlug(src.Name)));
        CreateMap<City, CityNameDto>();
        CreateMap<City, CityListDto>();
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

    }
}