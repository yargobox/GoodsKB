using GoodsKB.DAL.Configuration;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace GoodsKB.DAL.Repositories.Mongo;

internal class MongoRepo<TKey, TEntity> : MongoRepoBase<TKey, TEntity>, IMongoRepo<TKey, TEntity>
	where TEntity : class, IIdentifiableEntity<TKey>
{
	public virtual IMongoCollection<TEntity> MongoCollection => _col;
	public virtual IMongoQueryable<TEntity> MongoEntities => _col.AsQueryable<TEntity>();
	public virtual FilterDefinitionBuilder<TEntity> Filter => _Filter;
	public virtual UpdateDefinitionBuilder<TEntity> Update => _Update;
	public virtual SortDefinitionBuilder<TEntity> Sort => _Sort;
	public virtual ProjectionDefinitionBuilder<TEntity> Projection => _Projection;
	public virtual IndexKeysDefinitionBuilder<TEntity> IndexKeys => _IndexKeys;

	protected MongoRepo(IMongoDbContext context, string collectionName, IIdentityProvider<TKey>? identityProvider = null)
		: base(context, collectionName, identityProvider)
	{
	}

	public virtual async Task<IEnumerable<TEntity>> GetAsync(FilterDefinition<TEntity>? filter, SortDefinition<TEntity>? sort = null, int? limit = null, int? skip = null)
	{
		var options = new FindOptions<TEntity, TEntity>
		{
			Sort = sort,
			Limit = limit,
			Skip = skip
		};
		return await (await _col.FindAsync(filter ?? Filter.Empty, options)).ToListAsync();
	}

	public virtual async Task<IEnumerable<TEntityProjection>> GetAsync<TEntityProjection>(FilterDefinition<TEntity>? filter, ProjectionDefinition<TEntity, TEntityProjection> projection, SortDefinition<TEntity>? sort = null, int? limit = null, int? skip = null)
	{
		var options = new FindOptions<TEntity, TEntityProjection>
		{
			Projection = projection,
			Sort = sort,
			Limit = limit,
			Skip = skip
		};
		return await (await _col.FindAsync(filter ?? Filter.Empty, options)).ToListAsync();
	}

	public virtual async Task<long> UpdateAsync(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update)
	{
		var options = new UpdateOptions { IsUpsert = false };
		var result = await _col.UpdateManyAsync(filter, update, options);
		return await Task.FromResult(result.ModifiedCount);
	}
}
