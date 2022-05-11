namespace GoodsKB.DAL.Repositories;

using System.Linq.Expressions;
using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Repositories.Filters;
using GoodsKB.DAL.Repositories.SortOrders;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

internal class MongoRepo<K, T> : IRepo<K, T>
	where K : notnull
	where T : IIdentifiableEntity<K>
{
	protected readonly IMongoDbContext _context;
	protected readonly string _collectionName;
	protected readonly IMongoCollection<T> _col;

	protected static readonly FilterDefinitionBuilder<T> _Filter = Builders<T>.Filter;
	protected static readonly UpdateDefinitionBuilder<T> _Update = Builders<T>.Update;
	protected static readonly SortDefinitionBuilder<T> _Sort = Builders<T>.Sort;
	protected static readonly ProjectionDefinitionBuilder<T> _Projection = Builders<T>.Projection;
	protected static readonly IndexKeysDefinitionBuilder<T> _IndexKeys = Builders<T>.IndexKeys;

	protected MongoRepo(IMongoDbContext context, string collectionName, IIdentityProvider<K>? identityProvider = null)
	{
		_context = context;
		_collectionName = collectionName;
		IdentityProvider = identityProvider;

		var filter = Builders<BsonDocument>.Filter.Eq("name", _collectionName);
		var options = new ListCollectionNamesOptions { Filter = filter };
		var exists = _context.DB.ListCollectionNames(options).Any();

		_col = _context.GetCollection<T>(_collectionName);

		if (!exists)
			OnCreated();
		OnLoad();
	}

	#region IRepo

	public virtual IQueryable<T> Query => _col.AsQueryable<T>();
	public IIdentityProvider<K>? IdentityProvider { get; }

	public virtual async Task<long> GetCountAsync(Expression<Func<T, bool>>? where = null) => await _col.CountDocumentsAsync(where ?? _Filter.Empty);

	public virtual async Task<T?> GetAsync(K id)
	{
		var filter = _Filter.Eq(x => x.Id, id);

		return await (await _col.FindAsync(filter)).SingleOrDefaultAsync();
	}

	public virtual async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>>? where = null, OrderBy<T>? orderBy = null, long? skip = null, int? take = null)
	{
		var options = new FindOptions<T, T>
		{
			Sort = OrderByToSortDefinition(orderBy),
			Skip = (int?)skip,
			Limit = take
		};

		return await (await _col.FindAsync(where ?? _Filter.Empty, options)).ToListAsync();
	}

	public virtual async Task<T> CreateAsync(T entity)
	{
		if (IdentityProvider != null)
		{
			entity.Id = await IdentityProvider.NextIdentityAsync();
		}

		await _col.InsertOneAsync(entity);
		return await Task.FromResult(entity);
	}

	public virtual async Task<bool> UpdateAsync(T entity)
	{
		var filter = _Filter.Eq(x => x.Id, entity.Id);
		var options = new ReplaceOptions { IsUpsert = false };
		
		var result = await _col.ReplaceOneAsync(filter, entity);
		return result.IsModifiedCountAvailable ? result.ModifiedCount != 0 : result.MatchedCount != 0;
	}

	public virtual async Task<T> UpdateCreateAsync(T entity)
	{
		var filter = _Filter.Eq(x => x.Id, entity.Id);
		var options = new ReplaceOptions { IsUpsert = true };
		
		var result = await _col.ReplaceOneAsync(filter, entity, options);
		if (result.ModifiedCount == 0)
		{
			entity.Id = (K)BsonTypeMapper.MapToDotNetValue(result.UpsertedId);
		}
		return await Task.FromResult(entity);
	}

	public virtual async Task<bool> DeleteAsync(K id)
	{
		var filter = _Filter.Eq(x => x.Id, id);

		var result = await _col.DeleteOneAsync(filter);
		return await Task.FromResult(result.DeletedCount > 0);
	}

	public virtual async Task<long> DeleteAsync(Expression<Func<T, bool>> where)
	{
		var result = await _col.DeleteManyAsync(where);
		
		return await Task.FromResult(result.DeletedCount);
	}

	#endregion

	/// <summary>
	/// Method is called after the collection creation. It is not blocking, may attempt twice.
	/// </summary>
	protected virtual void OnCreated()
	{
	}

	/// <summary>
	/// Method is called always from the repository constructor.
	/// </summary>
	protected virtual async void OnLoad()
	{
		if (IdentityProvider != null)
		{
			await IdentityProvider.LoadAsync();
		}
	}

	protected async Task<IEnumerable<string>> GetIndexNames()
	{
		var indexNames = (await (await _col.Indexes.ListAsync()).ToListAsync()).Select(x => (string)x["name"]);

		return await Task.FromResult(indexNames);
	}

	protected async Task CreateIndex(string indexName, bool unique, Expression<Func<T, object?>> memberSelector, bool descending = false, Collation? collation = null, Expression<Func<T, bool>>? where = null)
	{
		var options = new CreateIndexOptions<T>
		{
			Name = indexName,
			Unique = unique,
			Collation = collation ?? Collations.Ukrainian_CI_AI
		};

		if (where != null)
		{
			options.PartialFilterExpression = _Filter.Where(where);
		}

		var indexModel = new CreateIndexModel<T>(
			descending ?
				_IndexKeys.Descending(memberSelector) :
				_IndexKeys.Ascending(memberSelector),
			options);

		await _col.Indexes.CreateOneAsync(indexModel, new CreateOneIndexOptions { });
	}

	protected static SortDefinition<T>? OrderByToSortDefinition(OrderBy<T>? orderBy)
	{
		var sortOrders = orderBy?.SortOrders;

		if (sortOrders != null && sortOrders.Length > 0)
		{
			var sort = sortOrders[0].Descending ? _Sort.Descending(sortOrders[0].Name) : _Sort.Ascending(sortOrders[0].Name);
			for (int i = 1; i < sortOrders.Length; i++)
			{
				sort = sortOrders[i].Descending ? sort.Descending(sortOrders[i].Name) : sort.Ascending(sortOrders[i].Name);
			}
			return sort;
		}

		return null;
	}
}