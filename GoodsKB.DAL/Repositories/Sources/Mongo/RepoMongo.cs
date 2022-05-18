namespace GoodsKB.DAL.Repositories.Mongo;

using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Repositories;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

internal class RepoMongo<K, T, TDateTime> : MongoRepo<K, T, TDateTime>, IRepoMongo<K, T, TDateTime>
	where T : IEntity<K, TDateTime>
	where TDateTime : struct
{
	public IMongoCollection<T> Collection => _col;
	public virtual IMongoQueryable<T> AsMongoQueryable() => _col.AsQueryable<T>();

	public FilterDefinitionBuilder<T> Filter => _Filter;
	public UpdateDefinitionBuilder<T> Update => _Update;
	public SortDefinitionBuilder<T> Sort => _Sort;
	public ProjectionDefinitionBuilder<T> Projection => _Projection;

	protected RepoMongo(IMongoDbContext context, string collectionName, IIdentityGenerator<K>? identityGenerator = null)
		: base(context, collectionName, identityGenerator)
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

		if (_hasUpdated)
		{
			update.CurrentDate(nameof(IUpdatedEntity<K, TDateTime>.Updated), UpdateDefinitionCurrentDateType.Date);
		}
		else if (_hasModified)
		{
			update.CurrentDate(nameof(IModifiedEntity<K, TDateTime>.Modified), UpdateDefinitionCurrentDateType.Date);
		}

		var result = await _col.UpdateManyAsync(where, update, options);
		return await Task.FromResult(result.ModifiedCount);
	}
}