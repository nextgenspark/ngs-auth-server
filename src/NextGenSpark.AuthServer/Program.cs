using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using NextGenSpark.AuthServer.Application.Interfaces;
using NextGenSpark.AuthServer.Application.Services;
using NextGenSpark.AuthServer.Infrastructure.Interfaces;
using NextGenSpark.AuthServer.Infrastructure.Persistence;
using NextGenSpark.AuthServer.Infrastructure.Security;
using NextGenSpark.AuthServer.Middlewares;
using NextGenSpark.AuthServer.OpenIddict;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// Anti-forgery
builder.Services.AddAntiforgery();

// Authentication (secure cookie)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.SlidingExpiration = false;
    });

builder.Services.AddAuthorization();

// Infrastructure DI
builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<RefreshTokenRepository>();
builder.Services.AddScoped<LoginAttemptRepository>();
builder.Services.AddScoped<AuditLogRepository>();

// Application DI
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<RefreshTokenService>();
builder.Services.AddScoped<IAuditService, AuditService>();

// Security
builder.Services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();

// OpenIddict
builder.Services.AddAuthServerOpenIddict(
    builder.Configuration,
    builder.Environment);

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
    });

    options.AddFixedWindowLimiter("token", opt =>
    {
        opt.PermitLimit = 20;
        opt.Window = TimeSpan.FromMinutes(1);
    });
});

// Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// 🔐 HTTPS first
app.UseHttpsRedirection();

// Global exception handling
app.UseMiddleware<GlobalExceptionMiddleware>();

// Static files (login UI)
app.UseStaticFiles();

app.UseRouting();

// Tenant resolution (MUST be before auth)
app.UseMiddleware<TenantMiddleware>();

// Rate limiting BEFORE auth
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Health endpoint
app.MapHealthChecks("/health");

// Controllers
app.MapControllers();

app.Run();
