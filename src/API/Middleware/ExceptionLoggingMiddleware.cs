using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ICMarkets.API.Middleware;

public class ExceptionLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionLoggingMiddleware> _logger;

    public ExceptionLoggingMiddleware(RequestDelegate next, ILogger<ExceptionLoggingMiddleware> logger)
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
            _logger.Log(
                ex is ValidationException ? LogLevel.Warning : LogLevel.Error,
                ex,
                "Request {Method} {Path} failed. CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                CorrelationIdMiddleware.GetCorrelationId(context));
            throw;
        }
    }
}
