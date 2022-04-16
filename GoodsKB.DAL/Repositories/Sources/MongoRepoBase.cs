using System.Linq.Expressions;
using GoodsKB.DAL.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace GoodsKB.DAL.Repositories;

internal class MongoRepoBase<TKey, TEntity> : IRepoBase<TKey, TEntity>
	where TEntity : IIdentifiableEntity<TKey>
{
	protected readonly IMongoDbContext _context;
	protected readonly string _collectionName;
	protected readonly IMongoCollection<TEntity> _col;

	protected MongoRepoBase(IMongoDbContext context, string collectionName, IIdentityProvider<TKey>? identityProvider = null)
	{
		_context = context;
		_collectionName = collectionName;
		IdentityProvider = identityProvider;

		var filter = Builders<BsonDocument>.Filter.Eq("name", _collectionName);
		var options = new ListCollectionNamesOptions { Filter = filter };
		var exists = _context.DB.ListCollectionNames(options).Any();

		_col = _context.GetCollection<TEntity>(_collectionName);

		if (!exists)
			OnCreated();
		OnLoad();
	}

	#region IRepoBase

	public virtual IQueryable<TEntity> Entities => _col.AsQueryable<TEntity>();
	public IIdentityProvider<TKey>? IdentityProvider { get; }
	protected FilterDefinitionBuilder<TEntity> _Filter => Builders<TEntity>.Filter;
	protected UpdateDefinitionBuilder<TEntity> _Update => Builders<TEntity>.Update;
	protected SortDefinitionBuilder<TEntity> _Sort => Builders<TEntity>.Sort;
	protected ProjectionDefinitionBuilder<TEntity> _Projection => Builders<TEntity>.Projection;
	protected IndexKeysDefinitionBuilder<TEntity> _IndexKeys => Builders<TEntity>.IndexKeys;

	public virtual async Task<TEntity> CreateAsync(TEntity entity)
	{
		if (IdentityProvider != null)
			entity.Id = await IdentityProvider.NextIdentityAsync();

		await _col.InsertOneAsync(entity);
		return await Task.FromResult(entity);
	}

	public virtual async Task<TEntity?> GetAsync(TKey id)
	{
		var filter = _Filter.Eq(item => item.Id, id);
		return await (await _col.FindAsync(filter)).SingleOrDefaultAsync();
	}

	public virtual async Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>>? filter = null, int? limit = null)
	{
		var options = new FindOptions<TEntity, TEntity> { Limit = limit };

		if (filter == null)
			return await (await _col.FindAsync(_Filter.Empty, options)).ToListAsync();
		else
			return await (await _col.FindAsync(filter, options)).ToListAsync();
	}

	public virtual async Task<bool> UpdateAsync(TEntity entity)
	{
		var filter = _Filter.Eq(existingItem => existingItem.Id, entity.Id);
		var options = new ReplaceOptions { IsUpsert = false };
		var result = await _col.ReplaceOneAsync(filter, entity);
		return result.IsModifiedCountAvailable ? result.ModifiedCount != 0 : result.MatchedCount != 0;
	}

	public virtual async Task<TEntity> UpdateCreateAsync(TEntity entity)
	{
		var filter = _Filter.Eq(existingItem => existingItem.Id, entity.Id);
		var options = new ReplaceOptions { IsUpsert = true };
		var result = await _col.ReplaceOneAsync(filter, entity, options);
		if (result.ModifiedCount == 0) entity.Id = (TKey)BsonTypeMapper.MapToDotNetValue(result.UpsertedId);
		return await Task.FromResult(entity);
	}

	public virtual async Task<bool> DeleteAsync(TKey id)
	{
		var filter = _Filter.Eq(item => item.Id, id);
		var result = await _col.DeleteOneAsync(filter);
		return await Task.FromResult(result.DeletedCount > 0);
	}

	public virtual async Task<long> DeleteAsync(Expression<Func<TEntity, bool>> filter)
	{
		var result = await _col.DeleteManyAsync(filter);
		return await Task.FromResult(result.DeletedCount);
	}

	#endregion

	/// <summary>Method is called after the collection creation. It is not blocking, may attempt twice.</summary>
	protected virtual void OnCreated()
	{
	}

	/// <summary>Method is called always from the repository constructor.</summary>
	protected virtual async void OnLoad()
	{
		if (IdentityProvider != null)
			await IdentityProvider.LoadAsync();
	}

	protected async Task<IEnumerable<string>> GetIndexNames()
	{
		var indexNames = (await (await _col.Indexes.ListAsync()).ToListAsync()).Select(x => (string)x["name"]);
		return await Task.FromResult(indexNames);
	}
	protected async Task CreateIndex(string indexName, bool unique, Expression<Func<TEntity, object?>> filter, bool descending = false)
	{
		var options = new CreateIndexOptions { Name = indexName, Unique = unique };
		var indexModel = new CreateIndexModel<TEntity>(descending ? _IndexKeys.Descending(filter) : _IndexKeys.Ascending(filter), options);
		await _col.Indexes.CreateOneAsync(indexModel, new CreateOneIndexOptions { });
	}
}