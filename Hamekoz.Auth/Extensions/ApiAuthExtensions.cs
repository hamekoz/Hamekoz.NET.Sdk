using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Hamekoz.Auth.Extensions;

public static class ApiAuthExtensions
{
    public static AuthenticationBuilder AddHamekozApiAuthentication(
        this IServiceCollection services, 
        IConfiguration config)
    {
        var jwtSettings = config.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("JwtSettings:SecretKey is missing from configuration.");
        }

        return services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });
    }
}
