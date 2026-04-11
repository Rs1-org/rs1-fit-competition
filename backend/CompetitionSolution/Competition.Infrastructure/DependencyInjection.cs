using Competition.Application.Auth.Interfaces;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Identity;
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
            return services;
        }
    }
}