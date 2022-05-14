namespace GoodsKB.DAL.Repositories.Mongo;

using GoodsKB.DAL.Configuration;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

internal class SoftDelRepoMongo<K, T, TDateTime> : MongoSoftDelRepo<K, T, TDateTime>, ISoftDelRepoMongo<K, T, TDateTime>
	where T : IEntity<K>, ISoftDelEntity<TDateTime>
	where TDateTime : struct
{
	protected SoftDelRepoMongo(IMongoDbContext context, string collectionName, IIdentityProvider<K>? identityProvider = null)
		: base(context, collectionName, identityProvider)
	{
	}

	#region IRepoMongo

	public virtual IMongoCollection<T> Collection => _col;
	public virtual IMongoQueryable<T> MongoQuery => _col.AsQueryable<T>();

	public FilterDefinitionBuilder<T> Filter => _Filter;
	public UpdateDefinitionBuilder<T> Update => _Update;
	public SortDefinitionBuilder<T> Sort => _Sort;
	public ProjectionDefinitionBuilder<T> Projection => _Projection;

	public virtual async Task<IEnumerable<T>> MongoGetAsync(FilterDefinition<T>? where, SortDefinition<T>? orderBy = null, long? skip = null, int? take = null)
	{
		where = where == null ? _Filter.Ne(x => x.Deleted, null) : where & _Filter.Ne(x => x.Deleted, null);
		
		var options = new FindOptions<T, T>
		{
			Sort = orderBy,
			Skip = (int?)skip,
			Limit = take
		};

		return await (await _col.FindAsync(where, options)).ToListAsync();
	}

	public virtual async Task<IEnumerable<P>> MongoGetAsync<P>(FilterDefinition<T>? where, ProjectionDefinition<T, P> projection, SortDefinition<T>? orderBy = null, long? skip = null, int? take = null)
	{
		where = where == null ? _Filter.Ne(x => x.Deleted, null) : where & _Filter.Ne(x => x.Deleted, null);
		
		var options = new FindOptions<T, P>
		{
			Projection = projection,
			Sort = orderBy,
			Skip = (int?)skip,
			Limit = take
		};

		return await (await _col.FindAsync(where, options)).ToListAsync();
	}

	public virtual async Task<long> MongoUpdateAsync(FilterDefinition<T> where, UpdateDefinition<T> update)
	{
		where &= _Filter.Ne(x => x.Deleted, null);
		var options = new UpdateOptions { IsUpsert = false };

		var result = await _col.UpdateManyAsync(where, update, options);
		return await Task.FromResult(result.ModifiedCount);
	}

	#endregion


	#region ISoftDelRepoMongo

	public virtual IMongoQueryable<T> MongoGetQuery(SoftDel mode = SoftDel.All) => GetMongoQueryInternal(mode);

	public virtual async Task<IEnumerable<T>> MongoGetAsync(SoftDel mode, FilterDefinition<T>? where, SortDefinition<T>? orderBy = null, long? skip = null, int? take = null)
	{
		var filter = MakeSoftDelFilter(mode);
		if (where != null)
		{
			filter = where & filter;
		}

		var options = new FindOptions<T, T>
		{
			Sort = orderBy,
			Skip = (int?)skip,
			Limit = take
		};

		return await (await _col.FindAsync(filter, options)).ToListAsync();
	}

	public virtual async Task<IEnumerable<P>> MongoGetAsync<P>(SoftDel mode, FilterDefinition<T>? where, ProjectionDefinition<T, P> projection, SortDefinition<T>? orderBy = null, long? skip = null, int? take = null)
	{
		var filter = MakeSoftDelFilter(mode);
		if (where != null)
		{
			filter = where & filter;
		}

		var options = new FindOptions<T, P>
		{
			Projection = projection,
			Sort = orderBy,
			Skip = (int?)skip,
			Limit = take
		};

		return await (await _col.FindAsync(filter, options)).ToListAsync();
	}

	public virtual async Task<long> MongoRestoreAsync(FilterDefinition<T> where)
	{
		where &= _Filter.Ne(x => x.Deleted, null);
		var update = _Update.Unset(x => x.Deleted);
		var options = new UpdateOptions { IsUpsert = false };

		var result = await _col.UpdateManyAsync(where, update, options);
		return await Task.FromResult(result.ModifiedCount);
	}

	#endregion
}