namespace GoodsKB.DAL.Repositories.Mongo;
using System.Linq.Expressions;
using System.Reflection;
using GoodsKB.DAL.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

internal class MongoSoftDelRepo<TKey, TEntity, TDateTime> : MongoRepo<TKey, TEntity>, IMongoSoftDelRepo<TKey, TEntity, TDateTime>
	where TEntity : class, IIdentifiableEntity<TKey>, ISoftDelEntity<TDateTime>
	where TDateTime : struct
{
	protected MongoSoftDelRepo(IMongoDbContext context, string collectionName, IIdentityProvider<TKey>? identityProvider = null)
		: base(context, collectionName, identityProvider)
	{
	}

	#region IRepoBase

	public override IQueryable<TEntity> Entities => _col.AsQueryable<TEntity>().Where(x => x.Deleted == null);

	public override async Task<long> GetCountAsync(Expression<Func<TEntity, bool>>? filter = null) => await GetCountAsync(SoftDelModes.Actual, filter);

	public override async Task<TEntity?> GetAsync(TKey id) => await GetAsync(SoftDelModes.Actual, id);

	public override async Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>>? filter = null, int? limit = null) =>
		await GetAsync(SoftDelModes.Actual, filter, limit);

	public override async Task<bool> DeleteAsync(TKey id)
	{
		var filter = _Filter.Eq(item => item.Id, id) & _Filter.Eq(x => x.Deleted, null);
		var update = Builders<TEntity>.Update.CurrentDate(x => x.Deleted);
		var options = new UpdateOptions { IsUpsert = false };

		var result = await _col.UpdateOneAsync(filter, update, options);
		return await Task.FromResult(result.ModifiedCount > 0);
	}

	public override async Task<long> DeleteAsync(Expression<Func<TEntity, bool>> filter)
	{
		var query = _Filter.Where(filter) & _Filter.Where(x => x.Deleted == null);
		var update = Builders<TEntity>.Update.CurrentDate(x => x.Deleted);
		var options = new UpdateOptions { IsUpsert = false };

		var result = await _col.UpdateManyAsync(query, update, options);
		return await Task.FromResult(result.ModifiedCount);
	}

	#endregion


	#region IMongoRepo

	public override IMongoQueryable<TEntity> MongoEntities => _col.AsQueryable<TEntity>().Where(x => x.Deleted == null);

	public override async Task<IEnumerable<TEntity>> GetAsync(FilterDefinition<TEntity>? filter, SortDefinition<TEntity>? sort = null, int? limit = null, int? skip = null)
	{
		filter = filter == null ? _Filter.Ne(x => x.Deleted, null) : filter & _Filter.Ne(x => x.Deleted, null);
		var options = new FindOptions<TEntity, TEntity>
		{
			Sort = sort,
			Limit = limit,
			Skip = skip
		};

		return await (await _col.FindAsync(filter, options)).ToListAsync();
	}

	public override async Task<IEnumerable<TEntityProjection>> GetAsync<TEntityProjection>(FilterDefinition<TEntity>? filter, ProjectionDefinition<TEntity, TEntityProjection> projection, SortDefinition<TEntity>? sort = null, int? limit = null, int? skip = null)
	{
		filter = filter == null ? _Filter.Ne(x => x.Deleted, null) : filter & _Filter.Ne(x => x.Deleted, null);
		var options = new FindOptions<TEntity, TEntityProjection>
		{
			Projection = projection,
			Sort = sort,
			Limit = limit,
			Skip = skip
		};

		return await (await _col.FindAsync(filter, options)).ToListAsync();
	}

	public override async Task<long> UpdateAsync(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update)
	{
		filter &= _Filter.Ne(x => x.Deleted, null);
		var options = new UpdateOptions { IsUpsert = false };

		var result = await _col.UpdateManyAsync(filter, update, options);
		return await Task.FromResult(result.ModifiedCount);
	}

	#endregion


	#region ISoftDelRepo

	public virtual IQueryable<TEntity> GetEntities(SoftDelModes mode) => GetMongoEntities(mode);

	public virtual async Task<long> GetCountAsync(SoftDelModes mode, Expression<Func<TEntity, bool>>? filter = null)
	{
		var fd = SoftDelFilter(mode);
		if (filter != null) fd = Filter.Where(filter) & fd;

		return await _col.CountDocumentsAsync(fd);
	}

	public virtual async Task<TEntity?> GetAsync(SoftDelModes mode, TKey id)
	{
		var fd = _Filter.Eq(x => x.Id, id) & SoftDelFilter(mode);

		return await (await _col.FindAsync(fd)).SingleOrDefaultAsync();
	}

	public virtual async Task<IEnumerable<TEntity>> GetAsync(SoftDelModes mode, Expression<Func<TEntity, bool>>? filter = null, int? limit = null)
	{
		var fd = SoftDelFilter(mode);
		if (filter != null) fd = Filter.Where(filter) & fd;
		var options = new FindOptions<TEntity>
		{
			Limit = limit
		};

		return await (await _col.FindAsync(filter, options)).ToListAsync();
	}

	public virtual async Task<bool> RestoreAsync(TKey id)
	{
		var filter = _Filter.Eq(item => item.Id, id) & _Filter.Ne(x => x.Deleted, null);
		var update = Builders<TEntity>.Update.Unset(x => x.Deleted);
		var options = new UpdateOptions { IsUpsert = false };

		var result = await _col.UpdateOneAsync(filter, update, options);
		return await Task.FromResult(result.ModifiedCount > 0);
	}

	public virtual async Task<long> RestoreAsync(Expression<Func<TEntity, bool>> filter)
	{
		var fd = _Filter.Where(filter) & _Filter.Ne(x => x.Deleted, null);
		var update = Builders<TEntity>.Update.Unset(x => x.Deleted);
		var options = new UpdateOptions { IsUpsert = false };

		var result = await _col.UpdateManyAsync(fd, update, options);
		return await Task.FromResult(result.ModifiedCount);
	}

	#endregion


	#region IMongoSoftDelRepo

	public virtual IMongoQueryable<TEntity> GetMongoEntities(SoftDelModes mode) => mode switch
		{
			SoftDelModes.Actual => _col.AsQueryable<TEntity>().Where(x => x.Deleted == null),
			SoftDelModes.Deleted => _col.AsQueryable<TEntity>().Where(x => x.Deleted != null),
			SoftDelModes.All => _col.AsQueryable<TEntity>(),
			_ => throw new ArgumentException(nameof(mode))
		};

	public virtual async Task<IEnumerable<TEntity>> GetAsync(SoftDelModes mode, FilterDefinition<TEntity>? filter, SortDefinition<TEntity>? sort = null, int? limit = null, int? skip = null)
	{
		var fd = SoftDelFilter(mode);
		if (filter != null) fd = filter & fd;
		var options = new FindOptions<TEntity, TEntity>
		{
			Sort = sort,
			Limit = limit,
			Skip = skip
		};

		return await (await _col.FindAsync(filter, options)).ToListAsync();
	}

	public virtual async Task<IEnumerable<TEntityProjection>> GetAsync<TEntityProjection>(SoftDelModes mode, FilterDefinition<TEntity>? filter, ProjectionDefinition<TEntity, TEntityProjection> projection, SortDefinition<TEntity>? sort = null, int? limit = null, int? skip = null)
	{
		var fd = SoftDelFilter(mode);
		if (filter != null) fd = filter & fd;
		var options = new FindOptions<TEntity, TEntityProjection>
		{
			Projection = projection,
			Sort = sort,
			Limit = limit,
			Skip = skip
		};

		return await (await _col.FindAsync(filter, options)).ToListAsync();
	}

	public virtual async Task<long> RestoreAsync(FilterDefinition<TEntity> filter)
	{
		filter &= _Filter.Ne(x => x.Deleted, null);
		var update = _Update.Unset(x => x.Deleted);
		var options = new UpdateOptions { IsUpsert = false };
		var result = await _col.UpdateManyAsync(filter, update, options);
		return await Task.FromResult(result.ModifiedCount);
	}

	#endregion

	protected override async void OnLoad()
	{
		base.OnLoad();

		var deletedIndexName = "deleted_ix";

		var indexeNames = await GetIndexNames();
		if (!indexeNames.Contains(deletedIndexName))
		{
			var options = new CreateIndexOptions { Name = deletedIndexName, Unique = false };
			var indexModel = new CreateIndexModel<TEntity>(IndexKeys.Descending(x => x.Deleted), options);
			await _col.Indexes.CreateOneAsync(indexModel, new CreateOneIndexOptions { });
		}
	}

	private FilterDefinition<TEntity> SoftDelFilter(SoftDelModes mode) => mode switch
		{
			SoftDelModes.Actual => _Filter.Eq(x => x.Deleted, null),
			SoftDelModes.Deleted => _Filter.Ne(x => x.Deleted, null),
			SoftDelModes.All => _Filter.Empty,
			_ => throw new ArgumentException(nameof(mode))
		};
}
