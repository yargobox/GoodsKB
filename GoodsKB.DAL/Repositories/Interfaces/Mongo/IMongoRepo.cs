using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace GoodsKB.DAL.Repositories.Mongo;

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