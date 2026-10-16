using Competition.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Competition.Tests.Infrastructure;

public abstract class BaseIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly HttpClient Client;
    protected readonly IServiceProvider ServiceProvider;

    protected BaseIntegrationTest(WebApplicationFactory<Program> factory)
    {
        // One name per test class: generated inside the options lambda, every scope would get a new empty database.
        var databaseName = "TestDb_" + Guid.NewGuid();

        var testFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext registration (EF Core 9+ also keeps the provider setup in IDbContextOptionsConfiguration)
                services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
                services.RemoveAll(typeof(IDbContextOptionsConfiguration<AppDbContext>));

                // Add In-Memory for testing
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(databaseName);
                });
            });
        });

        Client = testFactory.CreateClient();
        ServiceProvider = testFactory.Services;
    }
}
