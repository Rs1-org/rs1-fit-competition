using Competition.Application.Auth.Interfaces;
using Competition.Application.Interfaces;
using Competition.Application.Services;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Identity;
using Competition.Infrastructure.Repositories;
using Competition.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Competition.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IRefreshTokenService,RefreshTokenService>();
            services.AddScoped<IJwtTokenGenerator,JwtTokenGenerator>();
            services.AddScoped<ICity,CityRepository>();
            services.AddScoped<IGeoData,GeoDataRepository>();
            services.AddScoped<ICountry,CountryRepository>();
            services.AddScoped<IStadium,StadiumRepository>();
            services.AddScoped<IMatch, MatchRepository>();
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