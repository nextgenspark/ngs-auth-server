using Microsoft.AspNetCore.Mvc;
using NextGenSpark.AuthServer.Application.Interfaces;

namespace NextGenSpark.AuthServer.Controllers
{

    public sealed class AuthorizationController : Controller
    {
        private readonly IAuditService _auditService;
        private readonly ILogger<AuthorizationController> _logger;

        public AuthorizationController(
            IAuditService auditService,
            ILogger<AuthorizationController> logger)
        {
            _auditService = auditService;
            _logger = logger;
        }

        [HttpGet("/connect/authorize")]
        public async Task<IActionResult> Authorize()
        {
            var tenantId = HttpContext.Items["TenantId"] as Guid?;

            // User not logged in → redirect to login
            if (!User.Identity!.IsAuthenticated)
            {
                var returnUrl = Request.Path + Request.QueryString;

                await _auditService.LogAsync(
                    tenantId,
                    null,
                    "AUTHORIZE_REDIRECT_TO_LOGIN",
                    new { returnUrl });

                return Redirect($"/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
            }

            // Logged-in user reached authorize page
            await _auditService.LogAsync(
                tenantId,
                null,
                "AUTHORIZE_PAGE_VIEWED",
                new
                {
                    user = User.Identity.Name,
                    query = Request.QueryString.ToString()
                });

            _logger.LogInformation(
                "Authorization page accessed for tenant {TenantId} by user {User}",
                tenantId,
                User.Identity.Name);

            return View();
        }
    }
}
