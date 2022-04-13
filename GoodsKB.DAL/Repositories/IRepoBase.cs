using System.Linq.Expressions;
using MongoDB.Driver;

namespace GoodsKB.DAL.Repositories;

public enum SoftDelModes
{
	Actual = 0,
	Deleted = 1,
	All = 2
}

public interface IIdentEntity<TKey>
	where TKey : struct
{
	TKey Id { get; set; }
}

public interface ISoftDelEntity<TDateTime>
	where TDateTime : struct
{
	TDateTime? Deleted { get; set; }
}

public interface IRepoBase<TKey, TEntity>
	where TKey : struct
	where TEntity : IIdentEntity<TKey>
{
	IdentityPolicies IdentityPolicy { get; }
	Task<TKey> CreateAsync(TEntity entity);
	Task<TEntity?> GetAsync(TKey id);
	Task<IEnumerable<TEntity>> GetAsync();
	Task UpdateAsync(TEntity entity);
	Task DeleteAsync(TKey id);

	/* Task<TEntity?> GetByCondition(Expression<Func<TEntity, bool>> filter); */
}

public interface ISoftDelRepo<TKey, TEntity, TDateTime> : IRepoBase<TKey, TEntity>
	where TKey : struct
	where TEntity : IIdentEntity<TKey>, ISoftDelEntity<TDateTime>
	where TDateTime : struct
{
	Task<TEntity?> GetAsync(TKey id, SoftDelModes mode);
	Task<IEnumerable<TEntity>> GetAsync(SoftDelModes mode);
	Task RestoreAsync(TKey id);
}

public interface IMongoRepo<TKey, TEntity> : IRepoBase<TKey, TEntity>
	where TKey : struct
	where TEntity : IIdentEntity<TKey>
{
	IMongoCollection<TEntity> Entities { get; }
	FilterDefinitionBuilder<TEntity> Filter { get; }
	UpdateDefinitionBuilder<TEntity> Update { get; }
	SortDefinitionBuilder<TEntity> Sort { get; }
	ProjectionDefinitionBuilder<TEntity> Projection { get; }
	IndexKeysDefinitionBuilder<TEntity> IndexKeys { get; }

	Task<IEnumerable<TEntity>> GetAsync(FilterDefinition<TEntity>? filter, SortDefinition<TEntity>? sort = null, int? limit = null, int? skip = null);
	Task<IEnumerable<TEntityProjection>> GetAsync<TEntityProjection>(FilterDefinition<TEntity>? filter, ProjectionDefinition<TEntity, TEntityProjection> projection, SortDefinition<TEntity>? sort = null, int? limit = null, int? skip = null);
	Task UpdateAsync(TKey id, UpdateDefinition<TEntity> update);
	Task UpdateAsync(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update);
	Task DeleteAsync(FilterDefinition<TEntity> filter);
}

public interface IMongoSoftDelRepo<TKey, TEntity, TDateTime> : IMongoRepo<TKey, TEntity>, ISoftDelRepo<TKey, TEntity, TDateTime>
	where TKey : struct
	where TEntity : IIdentEntity<TKey>, ISoftDelEntity<TDateTime>
	where TDateTime : struct
{
	Task<IEnumerable<TEntity>> GetAsync(SoftDelModes mode, FilterDefinition<TEntity>? filter, SortDefinition<TEntity>? sort = null, int? limit = null, int? skip = null);
	Task<IEnumerable<TEntityProjection>> GetAsync<TEntityProjection>(SoftDelModes mode, FilterDefinition<TEntity>? filter, ProjectionDefinition<TEntity, TEntityProjection> projection, SortDefinition<TEntity>? sort = null, int? limit = null, int? skip = null);
	Task RestoreAsync(FilterDefinition<TEntity> filter);
}