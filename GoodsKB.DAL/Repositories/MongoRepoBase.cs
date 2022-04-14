using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Common;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;

namespace GoodsKB.DAL.Repositories;

internal class MongoRepoBase<TKey, TEntity> : IRepoBase<TKey, TEntity>
	where TKey : struct
	where TEntity : IIdentifiableEntity<TKey>
{
	protected readonly IMongoDbContext _context;
	protected readonly string _collectionName;
	protected readonly IMongoCollection<TEntity> _col;

	public virtual IQueryable<TEntity> Entities => _col.AsQueryable<TEntity>();
	public IdentityPolicies IdentityPolicy { get; private set; }

	protected FilterDefinitionBuilder<TEntity> _Filter => Builders<TEntity>.Filter;
	protected UpdateDefinitionBuilder<TEntity> _Update => Builders<TEntity>.Update;
	protected SortDefinitionBuilder<TEntity> _Sort => Builders<TEntity>.Sort;
	protected ProjectionDefinitionBuilder<TEntity> _Projection => Builders<TEntity>.Projection;
	protected IndexKeysDefinitionBuilder<TEntity> _IndexKeys => Builders<TEntity>.IndexKeys;

	public MongoRepoBase(IMongoDbContext context, string collectionName)
	{
		_context = context;
		_collectionName = collectionName;
		_col = context.GetCollection<TEntity>(collectionName);

		var propInfo = typeof(TEntity).GetProperty("Id");
		if (propInfo != null)
		{
			var attr = (IdentityPolicyAttribute?) Attribute.GetCustomAttribute(propInfo, typeof(IdentityPolicyAttribute));
			if (attr != null) IdentityPolicy = attr.Policy;
		}
	}

	public virtual async Task<TEntity> CreateAsync(TEntity entity)
	{
		if (IdentityPolicy == IdentityPolicies.Sequential)
		{
			entity.Id = (TKey) (object) (new Random()).Next(0, 0x7FFFFFF);
		}
		else
		{
			if (typeof(TKey) == typeof(Guid))
				entity.Id = (TKey) (object) Guid.NewGuid();
			else if (typeof(TKey) == typeof(ObjectId))
				entity.Id = (TKey) (object) ObjectId.GenerateNewId();
			else
				throw new NotSupportedException("Identity data type is not supported");
		}

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
		if (result.ModifiedCount == 0) entity.Id = (TKey) BsonTypeMapper.MapToDotNetValue(result.UpsertedId);
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
}

internal class MongoRepo<TKey, TEntity> : MongoRepoBase<TKey, TEntity>, IMongoRepo<TKey, TEntity>
	where TKey : struct
	where TEntity : IIdentifiableEntity<TKey>
{
	public virtual IMongoCollection<TEntity> MongoCollection => _col;
	public virtual IMongoQueryable<TEntity> MongoEntities => _col.AsQueryable<TEntity>();
	public virtual FilterDefinitionBuilder<TEntity> Filter => _Filter;
	public virtual UpdateDefinitionBuilder<TEntity> Update => _Update;
	public virtual SortDefinitionBuilder<TEntity> Sort => _Sort;
	public virtual ProjectionDefinitionBuilder<TEntity> Projection => _Projection;
	public virtual IndexKeysDefinitionBuilder<TEntity> IndexKeys => _IndexKeys;

	public MongoRepo(IMongoDbContext context, string collectionName)
		: base(context, collectionName)
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

internal class MongoSoftDelRepo<TKey, TEntity, TDateTime> :  MongoRepo<TKey, TEntity>, IMongoSoftDelRepo<TKey, TEntity, TDateTime>
	where TKey : struct
	where TEntity : IIdentifiableEntity<TKey>, ISoftDelEntity<TDateTime>
	where TDateTime : struct
{
	private readonly Func<TDateTime> _UtcNow;
	public TDateTime UtcNow => _UtcNow();

	public MongoSoftDelRepo(IMongoDbContext context, string collectionName)
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
}

/* 
internal class MongoGuidIdentityRepo<TEntity> : MongoRepoBase<Guid, TEntity> where TEntity : IIdentifiableEntity<Guid>
{
	public MongoGuidIdentityRepo(IMongoDbContext context, string collectionName)
		: base(context, collectionName)
	{
	}

	public override async Task<Guid> CreateAsync(TEntity item)
	{
		if (item.Id == default(Guid)) item.Id = Guid.NewGuid();

		return await base.CreateAsync(item);
	}
}

internal class MongoIntIdentityRepo<TEntity> : MongoRepoBase<int, TEntity> where TEntity : IIdentifiableEntity<int>
{
	protected record IdentityCounter
	{
		public string? id;
		public int counter;
	}

	private IMongoCollection<IdentityCounter>? __identityCounters;
	protected IMongoCollection<IdentityCounter> _identityCounters
	{
		get
		{
			return __identityCounters ?? (__identityCounters = _context.GetCollection<IdentityCounter>("identity_counters"));
		}
	}

	public MongoIntIdentityRepo(IMongoDbContext context, string collectionName)
		: base(context, collectionName)
	{
	}

	public class JobIdGenerator : IIdGenerator
	{
		public object GenerateId(object container, object doc)
		{
			IMongoCollection<SequenceDocument> idSeqColl = (IMongoCollection<JobDocument>)container).Database.GetCollection<SequenceDocument>("Sequence");
				var filter = Builders<SequenceDocument>.Filter.Empty;
				var update = Builders<SequenceDocument>.Update.Inc(a => a.Value, 1);
				return idSeqColl.FindOneAndUpdate(filter, update, new FindOneAndUpdateOptions<SequenceDocument, SequenceDocument>
					{
						ReturnDocument = ReturnDocument.After,
						IsUpsert = true
					}
				).Value;
		}
	} 

	public override async Task<int> CreateAsync(TEntity item)
	{
		if (item.Id == default(int))
		{
			// https://stackoverflow.com/questions/49500551/insert-a-document-while-auto-incrementing-a-sequence-field-in-mongodb
			//https://stackoverflow.com/questions/50068823/how-to-insert-document-to-mongodb-and-return-the-same-document-or-its-id-back-u
			item.Id = (new Random()).Next(0, 0x7FFFFFF);//!!!
		}

		return await base.CreateAsync(item);
	}
}

internal class MongoIntIdentitySoftDelRepo<TEntity> : MongoIntIdentityRepo<TEntity> where TEntity : IIdentifiableEntity<int>, ISoftDelEntity<DateTimeOffset>
{
	public MongoIntIdentitySoftDelRepo(IMongoDbContext context, string collectionName)
		: base(context, collectionName)
	{
	}

	public override async Task DeleteAsync(int id)
	{
		var filter = _Filter.Eq(item => item.Id, id) & _Filter.Ne(x => x.Deleted, (DateTimeOffset?) null);
		var update = Builders<TEntity>.Update.Set(x => x.Deleted, DateTimeOffset.UtcNow);
		var options = new FindOneAndUpdateOptions<TEntity, TEntity> { IsUpsert = false };

		await Entities.FindOneAndUpdateAsync(filter, update, options);
	}
}
 */
public enum IdentityPolicies
{
	Random = 0,
	Sequential = 1
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class IdentityPolicyAttribute : Attribute
{
	public IdentityPolicies Policy { get; }
	public IdentityPolicyAttribute() { }
	public IdentityPolicyAttribute(IdentityPolicies policy) => Policy = policy;
}