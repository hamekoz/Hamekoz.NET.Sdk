using Hamekoz.Api.Exceptions;
using Hamekoz.Api.Models;
using Hamekoz.Api.Services;

using Microsoft.EntityFrameworkCore;

namespace Hamekoz.Api.Tests.Services;

// ---------- Test entities & context ----------

public class SampleEntity : Entity
{
    public required string Name { get; set; }
}

public class AuditableSampleEntity : AuditableEntity
{
    public required string Name { get; set; }
}

public class SoftDeletableSampleEntity : AuditableEntity, ISoftDeletable
{
    public required string Name { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    public DbSet<SampleEntity> Samples { get; set; }
    public DbSet<AuditableSampleEntity> AuditableSamples { get; set; }
    public DbSet<SoftDeletableSampleEntity> SoftDeletableSamples { get; set; }
}

// ---------- Helper ----------

public abstract class CrudServiceTestBase : IDisposable
{
    protected readonly TestDbContext DbContext;

    protected CrudServiceTestBase()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        DbContext = new TestDbContext(options);
    }

    public void Dispose() => DbContext.Dispose();
}

// ---------- Basic CRUD Tests ----------

public class CrudServiceTests : CrudServiceTestBase
{
    private readonly CrudService<SampleEntity, TestDbContext> _service;

    public CrudServiceTests()
    {
        _service = new CrudService<SampleEntity, TestDbContext>(DbContext);
    }

    [Fact]
    public async Task CreateAsync_Should_AddEntityAndReturnIt()
    {
        var entity = new SampleEntity { Name = "Test" };

        var result = await _service.CreateAsync(entity);

        Assert.NotNull(result);
        Assert.Equal("Test", result.Name);
        Assert.Equal(1, await DbContext.Samples.CountAsync());
    }

    [Fact]
    public async Task GetAllAsync_Should_ReturnAllEntities()
    {
        DbContext.Samples.AddRange(
            new SampleEntity { Name = "A" },
            new SampleEntity { Name = "B" });
        await DbContext.SaveChangesAsync();

        var results = await _service.GetAllAsync();

        Assert.Equal(2, results.Count());
    }

    [Fact]
    public async Task ReadByIdAsync_Should_ReturnCorrectEntity()
    {
        var entity = new SampleEntity { Name = "Find me" };
        DbContext.Samples.Add(entity);
        await DbContext.SaveChangesAsync();

        var result = await _service.ReadByIdAsync(entity.Id);

        Assert.NotNull(result);
        Assert.Equal("Find me", result.Name);
    }

    [Fact]
    public async Task ReadByIdAsync_Should_ReturnNull_WhenNotFound()
    {
        var result = await _service.ReadByIdAsync(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_Should_UpdateAndReturnEntity()
    {
        var entity = new SampleEntity { Name = "Original" };
        DbContext.Samples.Add(entity);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        entity.Name = "Updated";
        var result = await _service.UpdateAsync(entity);

        Assert.Equal("Updated", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_Should_Throw_WhenEntityNotFound()
    {
        var entity = new SampleEntity { Name = "Ghost" };

        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(entity));
    }

    [Fact]
    public async Task DeleteAsync_Should_RemoveEntity()
    {
        var entity = new SampleEntity { Name = "Delete me" };
        DbContext.Samples.Add(entity);
        await DbContext.SaveChangesAsync();

        await _service.DeleteAsync(entity.Id);

        Assert.Equal(0, await DbContext.Samples.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_Should_Throw_WhenEntityNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(999));
    }
}

// ---------- Pagination Tests ----------

public class CrudServicePaginationTests : CrudServiceTestBase
{
    private readonly CrudService<SampleEntity, TestDbContext> _service;

    public CrudServicePaginationTests()
    {
        _service = new CrudService<SampleEntity, TestDbContext>(DbContext);
    }

    [Fact]
    public async Task GetPagedAsync_Should_ReturnCorrectPage()
    {
        for (var i = 1; i <= 15; i++)
        {
            DbContext.Samples.Add(new SampleEntity { Name = $"Item {i}" });
        }
        await DbContext.SaveChangesAsync();

        var result = await _service.GetPagedAsync(page: 2, pageSize: 5);

        Assert.Equal(5, result.Items.Count());
        Assert.Equal(15, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public async Task GetPagedAsync_LastPage_HasNoNextPage()
    {
        for (var i = 1; i <= 10; i++)
        {
            DbContext.Samples.Add(new SampleEntity { Name = $"Item {i}" });
        }
        await DbContext.SaveChangesAsync();

        var result = await _service.GetPagedAsync(page: 2, pageSize: 5);

        Assert.False(result.HasNextPage);
        Assert.True(result.HasPreviousPage);
    }

    [Fact]
    public async Task GetPagedAsync_Should_ClampPageToOne_WhenPageIsZeroOrNegative()
    {
        DbContext.Samples.Add(new SampleEntity { Name = "Only" });
        await DbContext.SaveChangesAsync();

        var result = await _service.GetPagedAsync(page: 0, pageSize: 10);

        Assert.Equal(1, result.Page);
        Assert.Single(result.Items);
    }
}

// ---------- Audit Fields Tests ----------

public class CrudServiceAuditTests : CrudServiceTestBase
{
    private readonly CrudService<AuditableSampleEntity, TestDbContext> _service;

    public CrudServiceAuditTests()
    {
        _service = new CrudService<AuditableSampleEntity, TestDbContext>(DbContext);
    }

    [Fact]
    public async Task CreateAsync_Should_SetCreatedAt()
    {
        var before = DateTime.UtcNow;
        var entity = new AuditableSampleEntity { Name = "Audit" };

        var result = await _service.CreateAsync(entity);

        Assert.True(result.CreatedAt >= before);
        Assert.Null(result.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_Should_SetUpdatedAt()
    {
        var entity = new AuditableSampleEntity { Name = "Before" };
        DbContext.AuditableSamples.Add(entity);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var before = DateTime.UtcNow;
        entity.Name = "After";
        var result = await _service.UpdateAsync(entity);

        Assert.NotNull(result.UpdatedAt);
        Assert.True(result.UpdatedAt >= before);
    }
}

// ---------- Soft Delete Tests ----------

public class CrudServiceSoftDeleteTests : CrudServiceTestBase
{
    private readonly CrudService<SoftDeletableSampleEntity, TestDbContext> _service;

    public CrudServiceSoftDeleteTests()
    {
        _service = new CrudService<SoftDeletableSampleEntity, TestDbContext>(DbContext);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletable_Should_NotRemoveFromDatabase()
    {
        var entity = new SoftDeletableSampleEntity { Name = "Soft delete me" };
        DbContext.SoftDeletableSamples.Add(entity);
        await DbContext.SaveChangesAsync();

        await _service.DeleteAsync(entity.Id);

        var raw = await DbContext.SoftDeletableSamples.FindAsync(entity.Id);
        Assert.NotNull(raw);
        Assert.True(raw.IsDeleted);
        Assert.NotNull(raw.DeletedAt);
    }

    [Fact]
    public async Task GetAllAsync_Should_ExcludeSoftDeletedEntities()
    {
        DbContext.SoftDeletableSamples.AddRange(
            new SoftDeletableSampleEntity { Name = "Active" },
            new SoftDeletableSampleEntity { Name = "Deleted", IsDeleted = true, DeletedAt = DateTime.UtcNow });
        await DbContext.SaveChangesAsync();

        var results = await _service.GetAllAsync();

        Assert.Single(results);
        Assert.Equal("Active", results.First().Name);
    }

    [Fact]
    public async Task ReadByIdAsync_Should_ReturnNull_ForSoftDeletedEntity()
    {
        var entity = new SoftDeletableSampleEntity { Name = "Gone", IsDeleted = true, DeletedAt = DateTime.UtcNow };
        DbContext.SoftDeletableSamples.Add(entity);
        await DbContext.SaveChangesAsync();

        var result = await _service.ReadByIdAsync(entity.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeleted_Should_Throw_WhenAlreadyDeleted()
    {
        var entity = new SoftDeletableSampleEntity { Name = "Already gone", IsDeleted = true, DeletedAt = DateTime.UtcNow };
        DbContext.SoftDeletableSamples.Add(entity);
        await DbContext.SaveChangesAsync();

        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(entity.Id));
    }

    [Fact]
    public async Task GetPagedAsync_Should_ExcludeSoftDeletedEntities()
    {
        DbContext.SoftDeletableSamples.AddRange(
            new SoftDeletableSampleEntity { Name = "Active 1" },
            new SoftDeletableSampleEntity { Name = "Active 2" },
            new SoftDeletableSampleEntity { Name = "Deleted", IsDeleted = true, DeletedAt = DateTime.UtcNow });
        await DbContext.SaveChangesAsync();

        var result = await _service.GetPagedAsync(1, 10);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count());
    }
}
