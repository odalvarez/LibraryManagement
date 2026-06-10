using LibraryManagement.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace LibraryManagement.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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

    // MAP EXCEPTION TYPE TO HTTP RESPONSE
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            MaxBooksReachedException => (StatusCodes.Status400BadRequest, exception.Message),
            AuthorNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            KeyNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "Ocurrió un error interno en el servidor.")
        };

        _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = message,
            Instance = context.Request.Path
        };

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
