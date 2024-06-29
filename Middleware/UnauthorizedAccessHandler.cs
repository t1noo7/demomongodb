// Custom unauthorized access handling middleware
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;

public class UnauthorizedAccessHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UnauthorizedAccessHandlerMiddleware> _logger;

    public UnauthorizedAccessHandlerMiddleware(RequestDelegate next, ILogger<UnauthorizedAccessHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        // Check if the request is unauthorized
        if (!context.User.Identity.IsAuthenticated || (!context.User.IsInRole("Staff") && !context.Request.Path.Equals("/UnauthorizedAccess", StringComparison.OrdinalIgnoreCase)))
        {
            // Redirect to the unauthorized access page
            context.Response.Redirect("/UnauthorizedAccess");
            return;
        }

        // Continue with the pipeline
        await _next(context);
    }
}