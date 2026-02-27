using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Hamekoz.Auth.Service.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;

        public AuthController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            // Este endpoint redirige al usuario al proveedor de Google
            var properties = new AuthenticationProperties { RedirectUri = Url.Action(nameof(GoogleCallback)) };
            return Challenge(properties, "Google");
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            // Este es el endpoint de retorno de llamada (callback) de Google
            var authenticateResult = await HttpContext.AuthenticateAsync("Google");

            if (!authenticateResult.Succeeded)
            {
                // Manejar el error de autenticación
                return BadRequest("Google authentication failed.");
            }

            // Aquí se extrae la información del usuario de Google
            var email = authenticateResult.Principal.FindFirstValue(ClaimTypes.Email);
            var name = authenticateResult.Principal.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email not found in Google claims.");
            }

            // Buscar si el usuario ya existe en nuestra base de datos
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Si no existe, crear un nuevo usuario
                user = new IdentityUser { UserName = email, Email = email };
                await _userManager.CreateAsync(user);
            }

            // Crear los claims que se incluirán en el token
            var claims = new List<Claim>
            {
                new(OpenIddictConstants.Claims.Subject, user.Id),
                new(OpenIddictConstants.Claims.Email, user.Email ?? email),
                new(OpenIddictConstants.Claims.Name, name ?? email), // Fallback to email if name is null
                new(OpenIddictConstants.Claims.EmailVerified, "true", OpenIddictConstants.Destinations.AccessToken)
            };
            
            // Crear el principal con los claims
            var identity = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            
            // Emitir el token de seguridad
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
    }
}