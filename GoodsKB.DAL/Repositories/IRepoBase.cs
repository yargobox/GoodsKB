using System.Linq.Expressions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace GoodsKB.DAL.Repositories;

public enum SoftDelModes
{
	Actual = 0,
	Deleted = 1,
	All = 2
}

public interface IIdentifiableEntity<TKey>
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
	where TEntity : IIdentifiableEntity<TKey>
{
	IQueryable<TEntity> Entities { get; }
	IdentityPolicies IdentityPolicy { get; }
	Task<TEntity> CreateAsync(TEntity entity);
	Task<TEntity?> GetAsync(TKey id);
	Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>>? filter = null, int? limit = null);
	Task<bool> UpdateAsync(TEntity entity);
	Task<TEntity> UpdateCreateAsync(TEntity entity);
	Task<bool> DeleteAsync(TKey id);
	Task<long> DeleteAsync(Expression<Func<TEntity, bool>> filter);
}

public interface ISoftDelRepo<TKey, TEntity, TDateTime> : IRepoBase<TKey, TEntity>
	where TKey : struct
	where TEntity : IIdentifiableEntity<TKey>, ISoftDelEntity<TDateTime>
	where TDateTime : struct
{
	IQueryable<TEntity> EntitiesAll { get; }
	Task<TEntity?> GetAsync(SoftDelModes mode, TKey id);
	Task<IEnumerable<TEntity>> GetAsync(SoftDelModes mode, Expression<Func<TEntity, bool>>? filter = null, int? limit = null);
	Task<bool> RestoreAsync(TKey id);
	Task<long> RestoreAsync(Expression<Func<TEntity, bool>> filter);
}

public interface IMongoRepo<TKey, TEntity> : IRepoBase<TKey, TEntity>
	where TKey : struct
	where TEntity : IIdentifiableEntity<TKey>
{
	IMongoCollection<TEntity> MongoCollection { get; }
	IMongoQueryable<TEntity> MongoEntities { get; }
	FilterDefinitionBuilder<TEntity> Filter { get; }
	UpdateDefinitionBuilder<TEntity> Update { get; }
	SortDefinitionBuilder<TEntity> Sort { get; }
	ProjectionDefinitionBuilder<TEntity> Projection { get; }
	IndexKeysDefinitionBuilder<TEntity> IndexKeys { get; }

	Task<IEnumerable<TEntity>> GetAsync(FilterDefinition<TEntity>? filter, SortDefinition<TEntity>? sort = null, int? limit = null, int? skip = null);
	Task<IEnumerable<TEntityProjection>> GetAsync<TEntityProjection>(FilterDefinition<TEntity>? filter, ProjectionDefinition<TEntity, TEntityProjection> projection, SortDefinition<TEntity>? sort = null, int? limit = null, int? skip = null);
	Task<long> UpdateAsync(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update);
}

public interface IMongoSoftDelRepo<TKey, TEntity, TDateTime> : IMongoRepo<TKey, TEntity>, ISoftDelRepo<TKey, TEntity, TDateTime>
	where TKey : struct
	where TEntity : IIdentifiableEntity<TKey>, ISoftDelEntity<TDateTime>
	where TDateTime : struct
{
	IMongoQueryable<TEntity> MongoEntitiesAll { get; }
	Task<IEnumerable<TEntity>> GetAsync(SoftDelModes mode, FilterDefinition<TEntity>? filter, SortDefinition<TEntity>? sort = null, int? limit = null, int? skip = null);
	Task<IEnumerable<TEntityProjection>> GetAsync<TEntityProjection>(SoftDelModes mode, FilterDefinition<TEntity>? filter, ProjectionDefinition<TEntity, TEntityProjection> projection, SortDefinition<TEntity>? sort = null, int? limit = null, int? skip = null);
	Task<long> RestoreAsync(FilterDefinition<TEntity> filter);
}