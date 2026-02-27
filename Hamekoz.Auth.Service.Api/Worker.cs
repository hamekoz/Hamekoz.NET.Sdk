using Hamekoz.Auth.Service.API.Data;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;

namespace Hamekoz.Auth.Service.Api;

public class Worker(
    IServiceProvider serviceProvider,
    IHostEnvironment env) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<HamekozAuthDbContext>();
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        if (await manager.FindByClientIdAsync("postman", cancellationToken) is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "postman",
                ClientSecret = "postman-secret",
                DisplayName = "Postman",
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.EndSession,
                    //OpenIddictConstants.Permissions.Endpoints.UserInfo,

                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,

                    OpenIddictConstants.Permissions.Prefixes.Scope + "email",
                    OpenIddictConstants.Permissions.Prefixes.Scope + "profile",
                    OpenIddictConstants.Permissions.Prefixes.Scope + "roles"
                },
                RedirectUris =
                {
                    new Uri("https://oauth.pstmn.io/v1/callback")
                }
            }, cancellationToken);
        }

        // Seed Admin User
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        if (await userManager.FindByNameAsync("admin@hamekoz.com") is null)
        {
            var user = new IdentityUser { UserName = "admin@hamekoz.com", Email = "admin@hamekoz.com", EmailConfirmed = true };
            await userManager.CreateAsync(user, "Admin123!");
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
