using Competition.Api.Middlewares;
using Competition.Application;
using Competition.Domain.Entities;
using Competition.Infrastructure;
using Competition.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NodaTime;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IClock>(SystemClock.Instance);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        o=>o.UseNodaTime()));

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", b =>
        b
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
    );
});

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();

using var scope = app.Services.CreateScope();
using var dbScope = app.Services.CreateScope();
var db = dbScope.ServiceProvider.GetRequiredService<AppDbContext>();
db.Database.Migrate();

app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using var roleScope = app.Services.CreateScope();
var roleManager = roleScope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

string[] roles = new[] { "Admin", "User", "Manager" };

foreach (var role in roles)
{
    if (!await roleManager.RoleExistsAsync(role))
    {
        await roleManager.CreateAsync(new ApplicationRole { Name = role });
    }
}

app.Run();