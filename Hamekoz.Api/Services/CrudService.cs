using Hamekoz.Api.Exceptions;
using Hamekoz.Api.Models;

using Microsoft.EntityFrameworkCore;

namespace Hamekoz.Api.Services;
public class CrudService<T, C>(C context) : ICrudService<T> 
    where T : IEntity 
    where C : DbContext
{
    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Set<T>().ToListAsync(cancellationToken);

    public virtual async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        context.Set<T>().Add(entity);
        await context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public virtual async Task<T?> ReadByIdAsync(int id, CancellationToken cancellationToken = default)
        => await context.Set<T>().FindAsync([id], cancellationToken: cancellationToken);

    public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        var existing = await context.FindAsync<T>([entity.Id], cancellationToken: cancellationToken)
            ?? throw new NotFoundException($"Not found {nameof(T)} with Id {entity.Id}");

        context.Entry(existing).CurrentValues.SetValues(entity);
        await context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public virtual async Task<T> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await context.Set<T>().FindAsync([id], cancellationToken: cancellationToken)
            ?? throw new NotFoundException($"Not found {nameof(T)} with Id {id}");

        context.Set<T>().Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
        return entity;

    }
}
