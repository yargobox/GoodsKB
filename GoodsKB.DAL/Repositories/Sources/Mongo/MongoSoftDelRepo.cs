using GoodsKB.DAL.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;

namespace GoodsKB.DAL.Repositories.Mongo;

internal class MongoSoftDelRepo<TKey, TEntity, TDateTime> :  MongoRepo<TKey, TEntity>, IMongoSoftDelRepo<TKey, TEntity, TDateTime>
	where TKey : struct
	where TEntity : IIdentifiableEntity<TKey>, ISoftDelEntity<TDateTime>
	where TDateTime : struct
{
	private readonly Func<TDateTime> _UtcNow;
	public TDateTime UtcNow => _UtcNow();

	protected MongoSoftDelRepo(IMongoDbContext context, string collectionName)
		: base(context, collectionName)
	{
		if (typeof(TDateTime) == typeof(DateTime))
			_UtcNow = () => (TDateTime) (object) DateTime.UtcNow;
		else if (typeof(TDateTime) == typeof(DateTimeOffset))
			_UtcNow = () => (TDateTime) (object) DateTimeOffset.UtcNow;
		else
			throw new NotSupportedException($"{typeof(TDateTime).Name} data type is not supported");
	}


	#region IRepoBase

	public override IQueryable<TEntity> Entities => _col.AsQueryable<TEntity>().Where(x => x.Deleted == null);

	public override async Task<TEntity?> GetAsync(TKey id) => await GetAsync(SoftDelModes.Actual, id);

	public override async Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>>? filter = null, int? limit = null)
		=> await GetAsync(SoftDelModes.Actual, filter, limit);

	public override async Task<bool> DeleteAsync(TKey id)
	{
		var filter = _Filter.Eq(item => item.Id, id) & _Filter.Ne(x => x.Deleted, (TDateTime?) null);
		var update = Builders<TEntity>.Update.Set(x => x.Deleted, UtcNow);
		var options = new FindOneAndUpdateOptions<TEntity, TEntity> { IsUpsert = false, ReturnDocument = ReturnDocument.After };

		var result = await _col.FindOneAndUpdateAsync(filter, update, options);
		return await Task.FromResult(result != null);
	}

	public override async Task<long> DeleteAsync(Expression<Func<TEntity, bool>> filter)
	{
		var query = _col.AsQueryable<TEntity>().Where(x => x.Deleted == null).Where(filter).GetExecutionModel().ToBsonDocument();
		var update = Builders<TEntity>.Update.Set(x => x.Deleted, UtcNow);
		var options = new UpdateOptions { IsUpsert = false };

		var result = await _col.UpdateManyAsync(query, update, options);
		return await Task.FromResult(result.ModifiedCount);
	}

	#endregion


	#region IMongoRepo

	public override IMongoQueryable<TEntity> MongoEntities => _col.AsQueryable<TEntity>().Where(x => x.Deleted == null);

	public override async Task<IEnumerable<TEntity>> GetAsync(FilterDefinition<TEntity>? filter, SortDefinition<TEntity>? sort = null, int? limit = null, int? skip = null)
	{
		filter = filter == null ? _Filter.Ne(x => x.Deleted, (TDateTime?) null) : filter & _Filter.Ne(x => x.Deleted, (TDateTime?) null);
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
		filter = filter == null ? _Filter.Ne(x => x.Deleted, (TDateTime?) null) : filter & _Filter.Ne(x => x.Deleted, (TDateTime?) null);
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
		filter &= _Filter.Ne(x => x.Deleted, (TDateTime?) null);
		var options = new UpdateOptions { IsUpsert = false };

		var result = await _col.UpdateManyAsync(filter, update, options);
		return await Task.FromResult(result.ModifiedCount);
	}

	#endregion


	#region ISoftDelRepo

	public virtual IQueryable<TEntity> EntitiesAll => _col.AsQueryable<TEntity>();

	public virtual async Task<TEntity?> GetAsync(SoftDelModes mode, TKey id)
	{
		FilterDefinition<TEntity> filter;
		if (mode == SoftDelModes.Actual)
			filter = _Filter.Eq(x => x.Id, id) & _Filter.Eq(x => x.Deleted, (TDateTime?) null);
		else if (mode == SoftDelModes.Deleted)
			filter = _Filter.Eq(x => x.Id, id) & _Filter.Ne(x => x.Deleted, (TDateTime?) null);
		else
			filter = _Filter.Eq(x => x.Id, id);

		return await (await _col.FindAsync(filter)).SingleOrDefaultAsync();
	}

	public virtual async Task<IEnumerable<TEntity>> GetAsync(SoftDelModes mode, Expression<Func<TEntity, bool>>? filter = null, int? limit = null)
	{
		if (mode == SoftDelModes.Actual)
		{
			if (filter == null)
			{
				if (limit == null || limit < 0)
					return await MongoEntitiesAll.Where(x => x.Deleted == null).ToListAsync();
				else
					return await MongoEntities.Where(x => x.Deleted == null).Take((int)limit).ToListAsync();
			}
			else
			{
				if (limit == null || limit < 0)
					return await MongoEntities.Where(x => x.Deleted == null).Where(filter).ToListAsync();
				else
					return await MongoEntities.Where(x => x.Deleted == null).Where(filter).Take((int)limit).ToListAsync();
			}
		}
		else if (mode == SoftDelModes.Deleted)
		{
			if (filter == null)
			{
				if (limit == null || limit < 0)
					return await MongoEntitiesAll.Where(x => x.Deleted != null).ToListAsync();
				else
					return await MongoEntitiesAll.Where(x => x.Deleted != null).Take((int)limit).ToListAsync();
			}
			else
			{
				if (limit == null || limit < 0)
					return await MongoEntitiesAll.Where(x => x.Deleted != null).Where(filter).ToListAsync();
				else
					return await MongoEntitiesAll.Where(x => x.Deleted != null).Where(filter).Take((int)limit).ToListAsync();
			}
		}
		else
		{
			if (filter == null)
			{
				if (limit == null || limit < 0)
					return await MongoEntitiesAll.ToListAsync();
				else
					return await MongoEntitiesAll.Take((int)limit).ToListAsync();
			}
			else
			{
				if (limit == null || limit < 0)
					return await MongoEntitiesAll.Where(filter).ToListAsync();
				else
					return await MongoEntitiesAll.Where(filter).Take((int)limit).ToListAsync();
			}
		}
	}

	public virtual async Task<bool> RestoreAsync(TKey id)
	{
		var filter = _Filter.Eq(item => item.Id, id) & _Filter.Ne(x => x.Deleted, (TDateTime?) null);
		var update = Builders<TEntity>.Update.Set(x => x.Deleted, (TDateTime?) null);
		var options = new FindOneAndUpdateOptions<TEntity, TEntity> { IsUpsert = false, ReturnDocument = ReturnDocument.After };

		var result = await _col.FindOneAndUpdateAsync(filter, update, options);
		return await Task.FromResult(result != null);
	}

	public virtual async Task<long> RestoreAsync(Expression<Func<TEntity, bool>> filter)
	{
		var query = _col.AsQueryable<TEntity>().Where(filter).Where(x => x.Deleted != null).GetExecutionModel().ToBsonDocument();
		var update = Builders<TEntity>.Update.Set(x => x.Deleted, (TDateTime?) null);
		var options = new UpdateOptions { IsUpsert = false };

		var result = await _col.UpdateManyAsync(query, update, options);
		return await Task.FromResult(result.ModifiedCount);
	}

	#endregion


	#region IMongoSoftDelRepo

	public virtual IMongoQueryable<TEntity> MongoEntitiesAll => _col.AsQueryable<TEntity>();

	public virtual async Task<IEnumerable<TEntity>> GetAsync(SoftDelModes mode, FilterDefinition<TEntity>? filter, SortDefinition<TEntity>? sort = null, int? limit = null, int? skip = null)
	{
		if (filter == null) filter = _Filter.Empty;
		if (mode == SoftDelModes.Actual)
			filter &= _Filter.Eq(x => x.Deleted, (TDateTime?) null);
		else if (mode == SoftDelModes.Deleted)
			filter &= _Filter.Ne(x => x.Deleted, (TDateTime?) null);

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
		if (filter == null) filter = _Filter.Empty;
		if (mode == SoftDelModes.Actual)
			filter &= _Filter.Eq(x => x.Deleted, (TDateTime?) null);
		else if (mode == SoftDelModes.Deleted)
			filter &= _Filter.Ne(x => x.Deleted, (TDateTime?) null);

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
		filter &= _Filter.Ne(x => x.Deleted, (TDateTime?) null);
		var update = _Update.Set(x => x.Deleted, (TDateTime?) null);
		var options = new UpdateOptions { IsUpsert = false };
		var result = await _col.UpdateManyAsync(filter, update, options);
		return await Task.FromResult(result.ModifiedCount);
	}

	#endregion

	protected override async void OnRepositoryCheck()
	{
		var deletedIndexName = "deleted_ix";
		
		var indexeNames = await GetIndexNames();
		if ( !indexeNames.Contains(deletedIndexName) )
		{
			var options = new CreateIndexOptions { Name = deletedIndexName, Unique = false };
			var indexModel = new CreateIndexModel<TEntity>(IndexKeys.Descending(x => x.Deleted), options);
			await _col.Indexes.CreateOneAsync(indexModel, new CreateOneIndexOptions { });
		}
	}
}
