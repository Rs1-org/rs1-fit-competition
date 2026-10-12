using Competition.Application.Auth.Interfaces;
using Competition.Application.Interfaces;
using Competition.Application.Services;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Identity;
using Competition.Infrastructure.Repositories;
using Competition.Infrastructure.Services;
using Competition.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Competition.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<JwtSettings>()
                .Bind(configuration.GetSection(JwtSettings.SectionName))
                .Validate(s => !string.IsNullOrWhiteSpace(s.Key) && s.Key.Length >= 32,
                    "Jwt:Key is missing or shorter than 32 characters. Set it via user-secrets or the Jwt__Key environment variable.")
                .Validate(s => !string.IsNullOrWhiteSpace(s.Issuer) && !string.IsNullOrWhiteSpace(s.Audience),
                    "Jwt:Issuer and Jwt:Audience are required.")
                .ValidateOnStart();
            services.AddOptions<CorsSettings>()
                .Bind(configuration.GetSection(CorsSettings.SectionName))
                .Validate(s => s.AllowedOrigins.Length > 0,
                    "Cors:AllowedOrigins must contain at least one origin.")
                .ValidateOnStart();
            services.AddOptions<BlobStorageSettings>()
                .Bind(configuration.GetSection(BlobStorageSettings.SectionName));
            services.AddOptions<StripeSettings>()
                .Bind(configuration.GetSection(StripeSettings.SectionName));
            services.AddOptions<SeedSettings>()
                .Bind(configuration.GetSection(SeedSettings.SectionName));

            services.AddScoped<IRefreshTokenService,RefreshTokenService>();
            services.AddScoped<IJwtTokenGenerator,JwtTokenGenerator>();
            services.AddScoped<ICity,CityRepository>();
            services.AddScoped<IGeoData,GeoDataRepository>();
            services.AddScoped<ICountry,CountryRepository>();
            services.AddScoped<IStadium,StadiumRepository>();
            services.AddScoped<IMatch, MatchRepository>();
            services.AddScoped<ICompetition, CompetitionRepository>();
            services.AddScoped<IFormationMark, FormationMarkRepository>();
            services.AddScoped<IFormation, FormationRepository>();
            services.AddScoped<IPosition, PositionRepository>();
            services.AddScoped<ISubPosition, SubPositionRepository>();
            services.AddScoped<ISelection, SelectionRepository>();
            services.AddScoped<IGallery, GalleryRepository>();
            services.AddScoped<IPlayerAppearances, PlayerAppearanceReader>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IBlobStorageService, BlobStorageService>();
            services.AddScoped<IImageService,ImageService>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IPlayer, PlayerRepository>();
            services.AddScoped<IReferee, RefereeRepository>();
            services.AddScoped<IManager, ManagerRepository>();
            services.AddScoped<IPlayerSelectionHistoryService, PlayerSelectionHistoryService>();
            services.AddScoped<ISelectionManagerHistoryService, SelectionManagerHistoryService>();
            services.AddScoped<ITransferValueHistoryService, TransferValueHistoryService>();
            services.AddScoped<IMandatDateService, MandatDateService>();

            return services;
        }
    }
}