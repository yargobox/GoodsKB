using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace GoodsKB.DAL.Repositories.Mongo;

public interface IMongoSoftDelRepo<TKey, TEntity, TDateTime> : IMongoRepo<TKey, TEntity>, ISoftDelRepo<TKey, TEntity, TDateTime>
	where TEntity : IIdentifiableEntity<TKey>, ISoftDelEntity<TDateTime>
	where TDateTime : struct
{
	IMongoQueryable<TEntity> MongoEntitiesAll { get; }
	Task<IEnumerable<TEntity>> GetAsync(SoftDelModes mode, FilterDefinition<TEntity>? filter, SortDefinition<TEntity>? sort = null, int? limit = null, int? skip = null);
	Task<IEnumerable<TEntityProjection>> GetAsync<TEntityProjection>(SoftDelModes mode, FilterDefinition<TEntity>? filter, ProjectionDefinition<TEntity, TEntityProjection> projection, SortDefinition<TEntity>? sort = null, int? limit = null, int? skip = null);
	Task<long> RestoreAsync(FilterDefinition<TEntity> filter);
}