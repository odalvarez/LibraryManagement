using LibraryManagement.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            MaxBooksReachedException => (StatusCodes.Status400BadRequest, exception.Message),
            AuthorNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            BookNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            DuplicateEmailException => (StatusCodes.Status409Conflict, exception.Message),
            DbUpdateException dbEx when IsUniqueConstraintViolation(dbEx)
                => (StatusCodes.Status409Conflict, "El correo electrónico ya está registrado."),
            _ => (StatusCodes.Status500InternalServerError, "Ocurrió un error interno en el servidor.")
        };

        // BUSINESS EXCEPTIONS (4xx) ARE WARNINGS; SERVER FAULTS (5xx) ARE ERRORS
        if (statusCode >= StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Server error: {Message}", exception.Message);
        else
            _logger.LogWarning("Business exception {StatusCode}: {Message}", statusCode, exception.Message);

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

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        var inner = ex.InnerException?.Message ?? string.Empty;
        return inner.Contains("unique", StringComparison.OrdinalIgnoreCase)
            || inner.Contains("duplicate key", StringComparison.OrdinalIgnoreCase);
    }
}
