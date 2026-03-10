using Hamekoz.Api.Exceptions;
using Hamekoz.Api.Models;

using Microsoft.EntityFrameworkCore;

namespace Hamekoz.Api.Services;
public class CrudService<T, C>(C context) : ICrudService<T>
    where T : Entity
    where C : DbContext
{
    private const int DefaultPageSize = 10;

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = context.Set<T>().AsQueryable();

        if (typeof(ISoftDeletable).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => !EF.Property<bool>(e, nameof(ISoftDeletable.IsDeleted)));
        }

        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<PagedResult<T>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = DefaultPageSize;

        var query = context.Set<T>().AsQueryable();

        if (typeof(ISoftDeletable).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => !EF.Property<bool>(e, nameof(ISoftDeletable.IsDeleted)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public virtual async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity is AuditableEntity auditable)
        {
            auditable.CreatedAt = DateTime.UtcNow;
        }

        context.Set<T>().Add(entity);
        await context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public virtual async Task<T?> ReadByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await context.Set<T>().FindAsync([id], cancellationToken: cancellationToken);

        if (entity is ISoftDeletable softDeletable && softDeletable.IsDeleted)
        {
            return null;
        }

        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        var existing = await context.FindAsync<T>([entity.Id], cancellationToken: cancellationToken)
            ?? throw new NotFoundException($"Not found {nameof(T)} with Id {entity.Id}");

        if (existing is ISoftDeletable softDeletable && softDeletable.IsDeleted)
        {
            throw new NotFoundException($"Not found {nameof(T)} with Id {entity.Id}");
        }

        context.Entry(existing).CurrentValues.SetValues(entity);

        if (existing is AuditableEntity auditable)
        {
            auditable.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public virtual async Task<T> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await context.Set<T>().FindAsync([id], cancellationToken: cancellationToken)
            ?? throw new NotFoundException($"Not found {nameof(T)} with Id {id}");

        if (entity is ISoftDeletable softDeletable)
        {
            if (softDeletable.IsDeleted)
            {
                throw new NotFoundException($"Not found {nameof(T)} with Id {id}");
            }

            softDeletable.IsDeleted = true;
            softDeletable.DeletedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            context.Set<T>().Remove(entity);
            await context.SaveChangesAsync(cancellationToken);
        }

        return entity;
    }
}
