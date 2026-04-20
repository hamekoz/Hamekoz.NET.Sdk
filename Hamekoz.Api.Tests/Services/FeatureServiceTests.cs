using Hamekoz.Api.Exceptions;
using Hamekoz.Api.Extensions;
using Hamekoz.Api.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hamekoz.Api.Tests.Services;

public class FeatureServiceTests
{
    private readonly FeatureService _service = new();

    [Fact]
    public async Task IsEnabledAsync_Should_ReturnDefaultValue_WhenFeatureDoesNotExist()
    {
        var enabled = await _service.IsEnabledAsync("new-feature", defaultValue: true);

        Assert.True(enabled);
    }

    [Fact]
    public async Task IsEnabledAsync_Should_UseLevel1Value_WhenNoMoreSpecificLevelExists()
    {
        await _service.SetLevel1Async("new-feature", isEnabled: false);

        var enabled = await _service.IsEnabledAsync("new-feature", "tenant-a");

        Assert.False(enabled);
    }

    [Fact]
    public async Task IsEnabledAsync_Should_PrioritizeLevel2OverLevel1()
    {
        await _service.SetLevel1Async("new-feature", isEnabled: false);
        await _service.SetLevel2Async("new-feature", "tenant-a", isEnabled: true);

        var enabled = await _service.IsEnabledAsync("new-feature", "tenant-a");

        Assert.True(enabled);
    }

    [Fact]
    public async Task IsEnabledAsync_Should_PrioritizeLevel3OverLevel2()
    {
        await _service.SetLevel2Async("new-feature", "tenant-a", isEnabled: false);
        await _service.SetLevel3Async("new-feature", "tenant-a", "user-a", isEnabled: true);

        var enabled = await _service.IsEnabledAsync("new-feature", "tenant-a", "user-a");

        Assert.True(enabled);
    }

    [Fact]
    public async Task IsEnabledAsync_Should_ThrowValidationException_WhenLevel3HasNoLevel2()
    {
        await Assert.ThrowsAsync<ValidationException>(() => _service.IsEnabledAsync("new-feature", level3Key: "user-a"));
    }

    [Fact]
    public async Task SetLevel2Async_Should_ThrowValidationException_WhenLevel2KeyIsEmpty()
    {
        await Assert.ThrowsAsync<ValidationException>(() => _service.SetLevel2Async("new-feature", string.Empty, true));
    }
}

public class FeatureServiceDiTests
{
    [Fact]
    public void AddFeatureManagementService_Should_RegisterFeatureServiceAsSingleton()
    {
        var services = new ServiceCollection();

        services.AddFeatureManagementService();

        using var provider = services.BuildServiceProvider();
        var first = provider.GetRequiredService<IFeatureService>();
        var second = provider.GetRequiredService<IFeatureService>();

        Assert.IsType<FeatureService>(first);
        Assert.Same(first, second);
    }

    [Fact]
    public void AddHamekozApi_Should_RegisterFeatureService()
    {
        var services = new ServiceCollection();
        services.AddDbContext<FeatureServiceTestDbContext>(options => options.UseInMemoryDatabase("feature-service-di"));

        services.AddHamekozApi<FeatureServiceTestDbContext>(typeof(FeatureServiceDiTests).Assembly);

        using var provider = services.BuildServiceProvider();
        var featureService = provider.GetRequiredService<IFeatureService>();

        Assert.IsType<FeatureService>(featureService);
    }
}

public class FeatureServiceTestDbContext(DbContextOptions<FeatureServiceTestDbContext> options) : DbContext(options);
