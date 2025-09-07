using System.Text.Json.Serialization;

using Hamekoz.Api.Example;
using Hamekoz.Api.Example.Data;
using Hamekoz.Api.Extensions;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<HamekozExampleDbContext>(options => options.UseInMemoryDatabase("Hamekoz.Api.Example"));

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddUniqueImplementationOfServices();
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
builder.Services.AddHamekozApi<HamekozExampleDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.UseHamekozMiddlewares();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<HamekozExampleDbContext>();
    dbContext.Database.EnsureDeleted();
    dbContext.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Hamekoz API Example v1");
        options.RoutePrefix = "swagger";
    });
}

app.MapControllers();

app.MapDefaultEndpoints();

app.Run();
