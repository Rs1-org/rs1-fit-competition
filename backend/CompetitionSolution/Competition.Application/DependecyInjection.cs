using System.Reflection;
using Competition.Application.Auth.Handlers;
using Competition.Application.Auth.Interfaces;
using Competition.Application.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Competition.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PremiumBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        
        services.AddAutoMapper((serviceProvider, cfg) =>
        {
        }, Assembly.GetExecutingAssembly());
        
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RegisterUserHandler>();
        });
        
        return services;
    }
}