using FinalProject_SeventhSem.Application.Exceptions;
using System.Net;
using System.Text.Json;

namespace FinalProject_SeventhSem.Middleware;


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
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, title, errors) = exception switch
        {
            Application.Exceptions.ValidationException ve =>
                (HttpStatusCode.BadRequest, ve.Message, ve.Errors),

            NotFoundException nfe =>
                (HttpStatusCode.NotFound, nfe.Message, (IReadOnlyDictionary<string, string[]>?)null),

            BadRequestException bre =>
                (HttpStatusCode.BadRequest, bre.Message, null),

            TestExpiredException tee =>
                (HttpStatusCode.BadRequest, tee.Message, null),

            UnauthorizedException uae =>
                (HttpStatusCode.Forbidden, uae.Message, null),

            OrganizationNotVerifiedException onve =>
                (HttpStatusCode.Forbidden, onve.Message, null),

            ConflictException ce =>
                (HttpStatusCode.Conflict, ce.Message, null),

            InvalidRefreshTokenException irte =>
                (HttpStatusCode.Unauthorized, irte.Message, null),

            RateLimitException rle =>
                (HttpStatusCode.TooManyRequests, rle.Message, null),

            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", null)
        };

        context.Response.StatusCode = (int)statusCode;

        var body = new ErrorResponse(
            Status: (int)statusCode,
            Title: title,
            Errors: errors,
            TraceId: context.TraceIdentifier);

        var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(json);
    }
}

public record ErrorResponse(
    int Status,
    string Title,
    IReadOnlyDictionary<string, string[]>? Errors,
    string TraceId);

