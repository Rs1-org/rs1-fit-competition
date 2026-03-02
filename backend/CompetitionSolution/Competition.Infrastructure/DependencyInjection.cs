using Competition.Application.Interfaces;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Competition.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            // Registriraj generički repository
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            return services;
        }
    }
}