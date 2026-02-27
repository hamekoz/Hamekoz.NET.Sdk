using Hamekoz.Auth.Service.API.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Hamekoz.Auth.Service.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapHamekozAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/challenge/{provider}", (string provider, string? returnUrl = "/") =>
        {
            var scheme = provider.ToLowerInvariant() switch
            {
                "google" => GoogleDefaults.AuthenticationScheme,
                "facebook" => FacebookDefaults.AuthenticationScheme,
                "microsoft" => MicrosoftAccountDefaults.AuthenticationScheme,
                _ => null
            };

            if (scheme is null)
                return Results.BadRequest("Proveedor no válido.");

            return Results.Challenge(
                new AuthenticationProperties { RedirectUri = $"/callback?returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}" },
                [scheme]);
        }).AllowAnonymous().WithName("Challenge");

        endpoints.MapGet("/callback", async (HttpContext context, UserManager<IdentityUser> userManager, HamekozAuthDbContext db, string? returnUrl = "/") =>
        {
            var authenticateResult = await context.AuthenticateAsync("ExternalCookie");
            if (!authenticateResult.Succeeded || authenticateResult.Principal == null)
            {
                return Results.Redirect("/login?error=auth_failed");
            }

            var principal = authenticateResult.Principal;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = principal.FindFirst(ClaimTypes.Name)?.Value;
            var providerId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var provider = authenticateResult.Properties?.Items.FirstOrDefault(p => p.Key == ".AuthScheme").Value ?? "Unknown";

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(providerId))
            {
                return Results.Redirect("/login?error=missing_claims");
            }

            // Sync User with AspNetCore Identity
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new IdentityUser { UserName = email, Email = email };
                var createResult = await userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return Results.Redirect("/login?error=user_creation_failed");
                }
            }

            var info = new UserLoginInfo(provider, providerId, provider);
            var addLoginResult = await userManager.AddLoginAsync(user, info);
            
            // Si ya tiene el login, AddLoginAsync fallará con un InvalidOperationException o resultará en false, pero lo ignoramos si ya existe.

            // Crear los claims definitivos que utilizará la aplicación
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, name ?? email),
                new Claim(ClaimTypes.Email, email),
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var finalPrincipal = new ClaimsPrincipal(identity);

            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, finalPrincipal, new AuthenticationProperties());
            await context.SignOutAsync("ExternalCookie");

            var finalRedirectUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;
            return Results.Redirect(finalRedirectUrl);

        }).AllowAnonymous().WithName("Callback");

        endpoints.MapGet("/signout", (string? returnUrl = "/") =>
        {
            return Results.SignOut(new AuthenticationProperties { RedirectUri = returnUrl },
                [CookieAuthenticationDefaults.AuthenticationScheme]);
        }).RequireAuthorization();
    }
}
