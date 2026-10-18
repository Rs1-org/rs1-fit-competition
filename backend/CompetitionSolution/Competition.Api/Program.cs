using System.Globalization;
using System.Security.Claims;
using System.Text;
using Competition.Api.Middlewares;
using Competition.Application;
using Competition.Domain.Entities;
using Competition.Infrastructure;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using Competition.Api.Live;
using Competition.Application.Live;
using MediatR;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

// Render supplies PORT dynamically. Keep launch profiles and Compose unchanged elsewhere.
if (Environment.GetEnvironmentVariable("PORT") is { Length: > 0 } port)
{
    if (!int.TryParse(port, out var portNumber) || portNumber is < 1 or > 65535)
        throw new InvalidOperationException("PORT must be a valid TCP port.");
    builder.WebHost.UseUrls($"http://0.0.0.0:{portNumber}");
}

// Values come from appsettings.{Environment}.json, user-secrets or environment variables (e.g. Jwt__Key).
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
var corsSettings = builder.Configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>() ?? new CorsSettings();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection is not configured. Set it via user-secrets or the ConnectionStrings__DefaultConnection environment variable.");

builder.Services.AddSingleton<IClock>(SystemClock.Instance);

builder.Services.AddControllers();
builder.Services.AddSignalR(options => { options.EnableDetailedErrors = false; options.MaximumReceiveMessageSize = 4096; });
builder.Services.AddScoped<INotificationHandler<MatchChangedNotification>, MatchChangedHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        o=>o.UseNodaTime()));

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);


builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsSettings.PolicyName, b =>
        b
            .WithOrigins(corsSettings.AllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
    );
});

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,

            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Key)
            ),

            RoleClaimType = ClaimTypes.Role
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                // Previously issued bearer tokens keep their normal expiry; new sessions support immediate revocation.
                var stamp = context.Principal?.FindFirstValue("security_stamp");
                if (stamp is null) return;
                var subject = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? context.Principal?.FindFirstValue("sub");
                var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                if (!Guid.TryParse(subject, out var userId) || !await db.Users.AsNoTracking().AnyAsync(
                        user => user.Id == userId && user.IsActive && user.SecurityStamp == stamp,
                        context.HttpContext.RequestAborted))
                    context.Fail("This session is no longer valid. Sign in again.");
            },
        };
    });

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(document => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

builder.Services.AddAuthorization();

var app = builder.Build();
// Enabled explicitly on Render, whose edge terminates TLS. Do not enable this on a
// directly exposed server: the platform must strip/overwrite incoming forwarded headers.
app.UseForwardedHeaders();
app.UseMiddleware<ExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();

using var dbScope = app.Services.CreateScope();
var db = dbScope.ServiceProvider.GetRequiredService<AppDbContext>();
if (db.Database.IsRelational())
    db.Database.Migrate();
else
    db.Database.EnsureCreated();

if (!app.Environment.IsDevelopment())
    await SampleAccountProtection.DisableDefaultPasswordsAsync(db);

await dbScope.ServiceProvider.GetRequiredService<DataSeeder>().SeedAsync(app.Lifetime.ApplicationStopping);

// Browser WebSocket upgrades are not governed by CORS, so check their Origin too.
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/hubs") && context.Request.Headers.TryGetValue("Origin", out var origin)
        && (origin.Count != 1 || !corsSettings.AllowedOrigins.Contains(origin.ToString(), StringComparer.OrdinalIgnoreCase)))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return;
    }
    await next(context);
});
app.UseCors(CorsSettings.PolicyName);

app.UseAuthentication();
app.UseAuthorization();
// Liveness does not query Postgres, allowing Neon to suspend idle compute.
app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    revision = app.Configuration["Deployment:Revision"] ?? "local"
})).AllowAnonymous();
app.MapGet("/health/ready", async (AppDbContext context, CancellationToken cancellationToken) =>
    await context.Database.CanConnectAsync(cancellationToken)
        ? Results.Ok(new { status = "ready" })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable)).AllowAnonymous();
app.MapControllers();
app.MapHub<MatchHub>("/hubs/matches");

app.Run();public partial class Program { }
