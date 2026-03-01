using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;

namespace Competition.Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        HttpStatusCode status;
        object? response;

        switch (exception)
        {
            case FluentValidation.ValidationException validationException:
                status = HttpStatusCode.BadRequest;
                var failures = validationException.Errors.ToList();
                response = new
                {
                    message = "Validation failed",
                    errors = failures.Select(e => new { e.PropertyName, e.ErrorMessage })
                };
                break;

            case KeyNotFoundException:
                status = HttpStatusCode.NotFound;
                response = new { message = exception.Message };
                break;

            default:
                status = HttpStatusCode.InternalServerError;
                response = new { message = "An unexpected error occurred." };
                break;
        }

        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

        context.Response.StatusCode = (int)status;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}