using Kaleido.Modules.Services.Grpc.Products.Constants;
using Kaleido.Modules.Services.Grpc.Products.Exceptions;
using Kaleido.Modules.Services.Grpc.Products.Models;
using Kaleido.Modules.Services.Grpc.Products.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Modules.Services.Grpc.Products.Repositories;

public abstract class BaseRepository<T, U> : IBaseRepository<T>
    where T : BaseEntity
    where U : DbContext
{
    protected readonly ILogger<object> _logger;
    protected readonly U _dbContext;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(ILogger<object> logger, U dbContext, DbSet<T> dbSet)
    {
        _logger = logger;
        _dbContext = dbContext;
        _dbSet = dbSet;
    }

    public async Task<T> GetAsync(Guid key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Get called with Id: {Id}", key);
        // Get item where id is key and state is active
        var entity = await _dbSet.Where(p => p.Key == key && p.Status == EntityStatus.Active).FirstOrDefaultAsync(cancellationToken);
        if (entity == null)
        {
            throw new EntityNotFoundException($"{typeof(T).Name} with key: {key} not found");
        }
        return entity;
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetAll called");
        return await _dbSet
            .Where(p => p.Status == EntityStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating {EntityName} with key: {Key}", typeof(T).Name, entity.Key);
        entity.Id = Guid.NewGuid();
        var storedEntity = await _dbSet.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("{EntityName} with key: {Key} created", typeof(T).Name, entity.Key);
        return storedEntity.Entity;
    }

    public async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity.Key == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(entity.Key));
        }

        _logger.LogInformation("Updating {EntityName} with key: {Key}", typeof(T).Name, entity.Key);
        await UpdateStatusAsync(entity.Key, EntityStatus.Archived, cancellationToken);

        entity.Status = EntityStatus.Active;
        var storedEntity = await CreateAsync(entity, cancellationToken);
        _logger.LogInformation("{EntityName} with key: {Key} updated", typeof(T).Name, entity.Key);
        return storedEntity;
    }

    public async Task<T?> UpdateStatusAsync(Guid key, EntityStatus status, CancellationToken cancellationToken = default)
    {
        var entity = await GetAsync(key, cancellationToken);
        if (entity == null)
        {
            return null;
        }

        _logger.LogInformation("Updating {EntityName} with key: {Key} to status: {Status}", typeof(T).Name, entity.Key, status);
        entity.Status = status;
        var stored = _dbContext.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return stored.Entity;
    }

    public async Task DeleteAsync(Guid key, CancellationToken cancellationToken = default)
    {
        await UpdateStatusAsync(key, EntityStatus.Deleted, cancellationToken);
    }

    public async Task<IEnumerable<T>> GetRevisionsAsync(Guid key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetRevisions called with Id: {Id}", key);
        return await _dbSet
            .Where(p => p.Key == key)
            .OrderByDescending(p => p.Revision)
            .ToListAsync(cancellationToken);
    }

    public async Task<T> GetRevisionAsync(Guid key, int revision, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetRevision called with Id: {Id} and Revision: {Revision}", key, revision);
        var entity = await _dbSet
            .Where(p => p.Key == key && p.Revision == revision)
            .FirstOrDefaultAsync(cancellationToken);
        if (entity == null)
        {
            throw new EntityNotFoundException($"{typeof(T).Name} with key: {key} and revision: {revision} not found");
        }
        return entity;
    }
}