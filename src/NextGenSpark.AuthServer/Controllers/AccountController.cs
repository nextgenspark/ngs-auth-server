using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NextGenSpark.AuthServer.Application.Interfaces;
using NextGenSpark.AuthServer.Shared.Constants;
using System.Security.Claims;
using Microsoft.AspNetCore.RateLimiting;

namespace NextGenSpark.AuthServer.Controllers
{
    public sealed class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IAuditService _audit;

        public AccountController(IUserService userService, IAuditService audit)
        {
            _userService = userService;
            _audit = audit;
        }

        [HttpGet("/login")]
        public IActionResult Login(string? returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [ValidateAntiForgeryToken]
        [EnableRateLimiting("login")]
        [HttpPost("/login")]
        public async Task<IActionResult> Login(
            string username,
            string password,
            string? returnUrl)
        {
            var tenantId = (Guid)HttpContext.Items["TenantId"];
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            if (!await _userService.ValidateAsync(tenantId, username, password, ip))
            {
                await _audit.LogAsync(tenantId, null, "LOGIN_FAILED", new { username, ip });
                return Unauthorized();
            }

            var claims = new[]
            {
            new Claim(ClaimTypes.Name, username),
            new Claim(AuthClaimTypes.TenantId, tenantId.ToString())
        };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(
                    new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));

            await _audit.LogAsync(tenantId, null, "LOGIN_SUCCESS", new { username, ip });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return Redirect("/");
        }

        [ValidateAntiForgeryToken]
        [HttpPost("/logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/login");
        }
    }

}
