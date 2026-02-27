using Hamekoz.Auth.Service.API.Data;
using Hamekoz.Auth.Service.Api.Endpoints;
using Hamekoz.Auth.Extensions;

using Microsoft.AspNetCore.Identity;

using OpenIddict.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.AddSqlServerDbContext<HamekozAuthDbContext>("hamekoz-auth-service-api-db");

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<HamekozAuthDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<HamekozAuthDbContext>();
    })
    .AddServer(options =>
    {
        options.SetTokenEndpointUris("/connect/token")
               .SetAuthorizationEndpointUris("/connect/authorize")
               .SetEndSessionEndpointUris("/connect/logout");
               //.SetUserinfoEndpointUris("/connect/userinfo");

        options.AllowClientCredentialsFlow()
               .AllowAuthorizationCodeFlow()
               .AllowRefreshTokenFlow();

        options.RegisterScopes(OpenIddictConstants.Scopes.Email,
                               OpenIddictConstants.Scopes.Profile,
                               OpenIddictConstants.Scopes.Roles);

        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        options.UseAspNetCore()
               .EnableTokenEndpointPassthrough()
               .EnableAuthorizationEndpointPassthrough()
               .EnableEndSessionEndpointPassthrough();
               //.EnableUserinfoEndpointPassthrough();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

builder.Services.AddHostedService<Hamekoz.Auth.Service.Api.Worker>();

builder.Services.AddHamekozWebAuthentication(builder.Configuration)
    .AddOpenIddict(options =>
    {
        options.UseAspNetCore();
    });

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = OpenIddictConstants.Schemes.Bearer;
    options.DefaultChallengeScheme = OpenIddictConstants.Schemes.Bearer;
});

// Configuración de Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHamekozAuthEndpoints();

app.Run();
