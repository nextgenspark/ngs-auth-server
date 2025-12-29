using System.Text.Json;

namespace NextGenSpark.AuthServer.Middlewares
{
    public sealed class TenantMiddleware
    {
        private const string TenantHeader = "X-Tenant-Id";
        private const string TenantItemKey = "TenantId";

        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // ✅ Skip tenant validation for health checks
            if (context.Request.Path.StartsWithSegments("/health"))
            {
                await _next(context);
                return;
            }

            // ✅ Read tenant header
            if (!context.Request.Headers.TryGetValue(TenantHeader, out var tenantValue) ||
                !Guid.TryParse(tenantValue, out var tenantId))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(new
                    {
                        error = "TENANT_HEADER_INVALID",
                        message = "Tenant header missing or invalid"
                    }));

                return;
            }

            // ✅ Store tenant id for this request only
            context.Items[TenantItemKey] = tenantId;

            // Continue pipeline
            await _next(context);
        }
    }
}
