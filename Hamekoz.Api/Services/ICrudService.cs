using Hamekoz.Api.Models;

namespace Hamekoz.Api.Services;

/// <summary>
/// Defines a generic service interface for CRUD operations on entities of type T.
/// </summary>
/// <typeparam name="T">Entity Type</typeparam>s
public interface ICrudService<T> where T : IEntity
{
    public Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    public Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);

    public Task<T?> ReadByIdAsync(int id, CancellationToken cancellationToken = default);

    public Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    public Task<T> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
