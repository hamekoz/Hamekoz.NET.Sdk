using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Hamekoz.Auth.Extensions;

public static class WebAuthExtensions
{
    public static AuthenticationBuilder AddHamekozWebAuthentication(
        this IServiceCollection services, 
        IConfiguration config)
    {
        var authBuilder = services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/login";
                options.LogoutPath = "/signout";
                options.ExpireTimeSpan = TimeSpan.FromDays(14);
                options.SlidingExpiration = true;
            })
            .AddCookie("ExternalCookie", options =>
            {
                options.Cookie.Name = "ExternalCookie";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            });

        if (!string.IsNullOrEmpty(config["Authentication:Google:ClientId"]))
        {
            authBuilder.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
            {
                options.SignInScheme = "ExternalCookie";
                options.ClientId = config["Authentication:Google:ClientId"]!;
                options.ClientSecret = config["Authentication:Google:ClientSecret"]!;
                options.SaveTokens = true;

                options.Scope.Add("https://www.googleapis.com/auth/user.birthday.read");

                options.ClaimActions.MapJsonKey(System.Security.Claims.ClaimTypes.NameIdentifier, "sub");
                options.ClaimActions.MapJsonKey(System.Security.Claims.ClaimTypes.Name, "name");
                options.ClaimActions.MapJsonKey(System.Security.Claims.ClaimTypes.Email, "email");
                options.ClaimActions.MapJsonKey(System.Security.Claims.ClaimTypes.DateOfBirth, "birthday");
            });
        }

        if (!string.IsNullOrEmpty(config["Authentication:Facebook:AppId"]))
        {
            authBuilder.AddFacebook(FacebookDefaults.AuthenticationScheme, options =>
            {
                options.SignInScheme = "ExternalCookie";
                options.AppId = config["Authentication:Facebook:AppId"]!;
                options.AppSecret = config["Authentication:Facebook:AppSecret"]!;
            });
        }

        if (!string.IsNullOrEmpty(config["Authentication:Microsoft:ClientId"]))
        {
            authBuilder.AddMicrosoftAccount(MicrosoftAccountDefaults.AuthenticationScheme, options =>
            {
                options.SignInScheme = "ExternalCookie";
                options.ClientId = config["Authentication:Microsoft:ClientId"]!;
                options.ClientSecret = config["Authentication:Microsoft:ClientSecret"]!;
            });
        }

        return authBuilder;
    }
}
