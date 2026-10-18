using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using Competition.Application.Exeptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Hosting;

namespace Competition.Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
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

            case LoginFailedException:
            case UnauthorizedAccessException:
                status = HttpStatusCode.Unauthorized;
                response = new { message = exception.Message };
                break;

            case ForbiddenAccessException:
                status = HttpStatusCode.Forbidden;
                response = new { message = exception.Message };
                break;

            case Microsoft.EntityFrameworkCore.DbUpdateException { InnerException: Npgsql.PostgresException
                { SqlState: "23505", ConstraintName: "IX_MatchReviews_MatchId_AuthorId" or "IX_ReviewReactions_ReviewId_UserId" } }:
                status = HttpStatusCode.Conflict;
                response = new { message = "Another change was saved first. Reload and try again." };
                break;

            default:
                status = HttpStatusCode.InternalServerError;
                response = _env.IsDevelopment()
                    ? new { message = exception.Message, detail = exception.StackTrace }
                    : (object)new { message = "An unexpected error occurred." };
                break;
        }

        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

        context.Response.StatusCode = (int)status;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
