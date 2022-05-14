namespace GoodsKB.DAL.Repositories.Mongo;

using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Repositories;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

internal class RepoMongo<K, T> : MongoRepo<K, T>, IRepoMongo<K, T>
	where T : IEntity<K>
{
	public virtual IMongoCollection<T> Collection => _col;
	public virtual IMongoQueryable<T> MongoQuery => _col.AsQueryable<T>();

	public FilterDefinitionBuilder<T> Filter => _Filter;
	public UpdateDefinitionBuilder<T> Update => _Update;
	public SortDefinitionBuilder<T> Sort => _Sort;
	public ProjectionDefinitionBuilder<T> Projection => _Projection;

	protected RepoMongo(IMongoDbContext context, string collectionName, IIdentityProvider<K>? identityProvider = null)
		: base(context, collectionName, identityProvider)
	{
	}

	public virtual async Task<IEnumerable<T>> MongoGetAsync(FilterDefinition<T>? where, SortDefinition<T>? orderBy = null, long? skip = null, int? take = null)
	{
		var options = new FindOptions<T, T>
		{
			Sort = orderBy,
			Skip = (int?)skip,
			Limit = take
		};

		return await (await _col.FindAsync(where ?? Filter.Empty, options)).ToListAsync();
	}

	public virtual async Task<IEnumerable<P>> MongoGetAsync<P>(FilterDefinition<T>? where, ProjectionDefinition<T, P> projection, SortDefinition<T>? orderBy = null, long? skip = null, int? take = null)
	{
		var options = new FindOptions<T, P>
		{
			Projection = projection,
			Sort = orderBy,
			Skip = (int?)skip,
			Limit = take
		};

		return await (await _col.FindAsync(where ?? Filter.Empty, options)).ToListAsync();
	}

	public virtual async Task<long> MongoUpdateAsync(FilterDefinition<T> where, UpdateDefinition<T> update)
	{
		var options = new UpdateOptions { IsUpsert = false };

		var result = await _col.UpdateManyAsync(where, update, options);
		return await Task.FromResult(result.ModifiedCount);
	}
}
