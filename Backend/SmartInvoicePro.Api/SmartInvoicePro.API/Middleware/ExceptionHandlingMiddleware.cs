using System.Net;
using System.Text.Json;
using FluentValidation;
using SmartInvoicePro.Application.Common;
using SmartInvoicePro.Application.Exceptions;

namespace SmartInvoicePro.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        var (statusCode, response) = exception switch
        {
            NotFoundException notFound => (HttpStatusCode.NotFound, ApiResponse.Fail(notFound.Message)),
            Application.Exceptions.ValidationException validation => (HttpStatusCode.BadRequest, ApiResponse.Fail(validation.Message, validation.Errors)),
            UnauthorizedException unauthorized => (HttpStatusCode.Unauthorized, ApiResponse.Fail(unauthorized.Message)),
            ForbiddenException forbidden => (HttpStatusCode.Forbidden, ApiResponse.Fail(forbidden.Message)),
            FluentValidation.ValidationException fv => (HttpStatusCode.BadRequest, ApiResponse.Fail("Validation failed", fv.Errors.Select(e => e.ErrorMessage))),
            _ => (HttpStatusCode.InternalServerError, ApiResponse.Fail("An unexpected error occurred."))
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
