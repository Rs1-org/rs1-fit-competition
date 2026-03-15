using Competition.Application.Interfaces;
using Competition.Application.Services;
using Competition.Infrastructure.Data;
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
            services.AddScoped<ICity,CityRepository>();
            services.AddScoped<IGeoData,GeoDataRepository>();
            services.AddScoped<ICountry,CountryRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IBlobStorageService, BlobStorageService>();
            services.AddScoped<IImageService,ImageService>();
            // Registriraj generički repository
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IPlayerService,PlayerService>();
            services.AddScoped<IManagerService, ManagerService>();
            services.AddScoped<IRefereeService, RefereeService>();

            return services;
        }
    }
}