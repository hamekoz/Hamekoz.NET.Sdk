using Hamekoz.Api.Models;

namespace Hamekoz.Api.Services;

/// <summary>
/// Defines a generic service interface for CRUD operations on entities of type T.
/// </summary>
/// <typeparam name="T">Entity Type</typeparam>
public interface ICrudService<T> where T : Entity
{
    public Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a paginated subset of entities.
    /// </summary>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task<PagedResult<T>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    public Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);

    public Task<T?> ReadByIdAsync(int id, CancellationToken cancellationToken = default);

    public Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    public Task<T> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
