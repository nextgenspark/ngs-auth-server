using System.Net;
using System.Security;
using System.Text.Json;

namespace NextGenSpark.AuthServer.Middlewares
{
    public sealed class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
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
            catch (SecurityException ex)
            {
                _logger.LogWarning(ex, "Security exception occurred");

                await WriteErrorAsync(
                    context,
                    HttpStatusCode.Unauthorized,
                    "SECURITY_ERROR");
            }
            catch (OperationCanceledException)
            {
                // Client disconnected / request aborted – do not log as error
                context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");

                await WriteErrorAsync(
                    context,
                    HttpStatusCode.InternalServerError,
                    "INTERNAL_SERVER_ERROR");
            }
        }

        private static async Task WriteErrorAsync(
            HttpContext context,
            HttpStatusCode statusCode,
            string errorCode)
        {
            if (context.Response.HasStarted)
                return;

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new
                {
                    error = errorCode
                }));
        }
    }
}
