using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using NextGenSpark.AuthServer.Application.Services;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
using System.Security.Claims;
using OpenIddict.Abstractions;
using Microsoft.AspNetCore.RateLimiting;
using NextGenSpark.AuthServer.Application.Interfaces;

namespace NextGenSpark.AuthServer.Controllers
{
    [ApiController]
    public sealed class TokenController : Controller
    {
        private readonly RefreshTokenService _refreshTokenService;
        private readonly IAuditService _auditService;

        public TokenController(
            RefreshTokenService refreshTokenService,
            IAuditService auditService)
        {
            _refreshTokenService = refreshTokenService;
            _auditService = auditService;
        }

        [EnableRateLimiting("token")]
        [HttpPost("/connect/token")]
        public async Task<IActionResult> Exchange()
        {
            var tenantId = HttpContext.Items.TryGetValue("TenantId", out var t)
                ? (Guid?)t
                : null;

            var request = HttpContext.GetOpenIddictServerRequest()
                ?? throw new InvalidOperationException("OIDC request missing");

            // -----------------------------
            // CLIENT CREDENTIALS FLOW
            // -----------------------------
            if (request.IsClientCredentialsGrantType())
            {
                var identity = new ClaimsIdentity(
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                identity.AddClaim(Claims.Subject, request.ClientId!);
                identity.AddClaim(Claims.ClientId, request.ClientId!);

                await _auditService.LogAsync(
                    tenantId,
                    null,
                    "TOKEN_ISSUED_CLIENT_CREDENTIALS",
                    new
                    {
                        clientId = request.ClientId,
                        ip = HttpContext.Connection.RemoteIpAddress?.ToString()
                    });

                return SignIn(
                    new ClaimsPrincipal(identity),
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            // -----------------------------
            // REFRESH TOKEN FLOW (ROTATION)
            // -----------------------------
            if (request.IsRefreshTokenGrantType())
            {
                var result = await HttpContext.AuthenticateAsync(
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                if (!result.Succeeded)
                {
                    await _auditService.LogAsync(
                        tenantId,
                        null,
                        "TOKEN_REFRESH_FAILED",
                        new { reason = "Invalid refresh token" });

                    return Unauthorized();
                }

                var principal = result.Principal!;
                var authorizationId = Guid.Parse(
                    principal.FindFirst("authorization_id")!.Value);

                var newRefreshToken = await _refreshTokenService.RotateAsync(
                    authorizationId,
                    request.RefreshToken!);

                var identity = new ClaimsIdentity(
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                foreach (var claim in principal.Claims)
                    identity.AddClaim(claim);

                // Ensure subject exists (OIDC compliance)
                identity.AddClaim(
                    Claims.Subject,
                    principal.GetClaim(Claims.Subject)!);

                await _auditService.LogAsync(
                    tenantId,
                    null,
                    "TOKEN_REFRESHED",
                    new
                    {
                        authorizationId,
                        ip = HttpContext.Connection.RemoteIpAddress?.ToString()
                    });

                var properties = new AuthenticationProperties(
                    new Dictionary<string, string?>
                    {
                        ["refresh_token"] = newRefreshToken
                    });

                return SignIn(
                    new ClaimsPrincipal(identity),
                    properties,
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            // -----------------------------
            // UNSUPPORTED GRANT
            // -----------------------------
            await _auditService.LogAsync(
                tenantId,
                null,
                "TOKEN_REQUEST_INVALID",
                new { grantType = request.GrantType });

            return BadRequest("Unsupported grant type");
        }
    }
}
