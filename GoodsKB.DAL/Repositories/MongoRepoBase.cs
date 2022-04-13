using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Common;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;

namespace GoodsKB.DAL.Repositories;

internal class MongoRepo<TKey, TEntity> : IMongoRepo<TKey, TEntity>
	where TKey : struct
	where TEntity : IIdentEntity<TKey>
{
	protected IMongoDbContext _context;

	public IdentityPolicies IdentityPolicy { get; protected set; }

	public IMongoCollection<TEntity> Entities { get; protected set; }
	public FilterDefinitionBuilder<TEntity> Filter { get; private set; } = Builders<TEntity>.Filter;
	public UpdateDefinitionBuilder<TEntity> Update { get; private set; } = Builders<TEntity>.Update;
	public SortDefinitionBuilder<TEntity> Sort { get; private set; } = Builders<TEntity>.Sort;
	public ProjectionDefinitionBuilder<TEntity> Projection { get; private set; } = Builders<TEntity>.Projection;
	public IndexKeysDefinitionBuilder<TEntity> IndexKeys { get; private set; } = Builders<TEntity>.IndexKeys;

	public MongoRepo(IMongoDbContext context, string collectionName)
	{
		_context = context;
		Entities = context.GetCollection<TEntity>(collectionName);

		var propInfo = typeof(TEntity).GetProperty("Id");
		if (propInfo != null)
		{
			var attr = (IdentityPolicyAttribute?) Attribute.GetCustomAttribute(propInfo, typeof(IdentityPolicyAttribute));
			if (attr != null) IdentityPolicy = attr.Policy;
		}
	}

	public virtual async Task<TKey> CreateAsync(TEntity item)
	{
		if (IdentityPolicy == IdentityPolicies.Sequential)
		{
			item.Id = (TKey) (object) (new Random()).Next(0, 0x7FFFFFF);
		}
		else
		{
			if (typeof(TKey) == typeof(Guid))
				item.Id = (TKey) (object) Guid.NewGuid();
			else if (typeof(TKey) == typeof(ObjectId))
				item.Id = (TKey) (object) ObjectId.GenerateNewId();
			else
				throw new NotSupportedException("Identity data type is not supported");
		}

		await Entities.InsertOneAsync(item);
		return await Task.FromResult(item.Id);
	}

	public virtual async Task<TEntity?> GetAsync(TKey id)
	{
		var filter = Filter.Eq(item => item.Id, id);
		return await (await Entities.FindAsync(filter)).SingleOrDefaultAsync();
	}

	public virtual async Task<IEnumerable<TEntity>> GetAsync()
	{
		return await (await Entities.FindAsync(Filter.Empty)).ToListAsync();
	}

	public virtual async Task UpdateAsync(TEntity item)
	{
		var filter = Filter.Eq(existingItem => existingItem.Id, item.Id);
		await Entities.ReplaceOneAsync(filter, item);
	}

	public virtual async Task DeleteAsync(TKey id)
	{
		var filter = Filter.Eq(item => item.Id, id);
		await Entities.DeleteOneAsync(filter);
	}

/* 	public virtual async Task<TEntity?> GetByCondition(Expression<Func<TEntity, bool>> filter)
	{
		var query = Entities.AsQueryable<TEntity>().Where(filter);
		
		Console.WriteLine("Query => " + query.ToString());

		return await query.FirstOrDefaultAsync();
	} */

	public virtual async Task<IEnumerable<TEntity>> GetAsync(FilterDefinition<TEntity>? filter, SortDefinition<TEntity>? sort = null, int? limit = null, int? skip = null)
	{
		var options = new FindOptions<TEntity, TEntity>
		{
			Sort = sort,
			Limit = limit,
			Skip = skip
		};
		
		return await (await Entities.FindAsync(filter ?? Filter.Empty, options)).ToListAsync();
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
		
		return await (await Entities.FindAsync(filter ?? Filter.Empty, options)).ToListAsync();
	}

	public virtual async Task UpdateAsync(TKey id, UpdateDefinition<TEntity> update)
	{
		var filter = Filter.Eq(existingItem => existingItem.Id, id);
		var options = new UpdateOptions { IsUpsert = false };

		await Entities.UpdateOneAsync(filter, update, options);
	}

	public virtual async Task UpdateAsync(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update)
	{
		var options = new UpdateOptions { IsUpsert = false };

		await Entities.UpdateManyAsync(filter, update, options);
	}

	public virtual async Task DeleteAsync(FilterDefinition<TEntity> filter)
	{
/* 		await Entities.DeleteManyAsync(filter);

		Expression<Func<TEntity, bool>> cond = x => true;

		var result = await Entities.DeleteManyAsync(cond);
		return result.DeletedCount; */
		await Entities.DeleteManyAsync(filter);
	}
}

internal class MongoSoftDelRepo<TKey, TEntity, TDateTime> :  MongoRepo<TKey, TEntity>, IMongoSoftDelRepo<TKey, TEntity, TDateTime>
	where TKey : struct
	where TEntity : IIdentEntity<TKey>, ISoftDelEntity<TDateTime>
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

	public override async Task<TEntity?> GetAsync(TKey id) => await GetAsync(id, SoftDelModes.Actual);

	public override async Task<IEnumerable<TEntity>> GetAsync() => await GetAsync(SoftDelModes.Actual);

	public override async Task DeleteAsync(TKey id)
	{
		var filter = Filter.Eq(item => item.Id, id) & Filter.Ne(x => x.Deleted, (TDateTime?) null);
		var update = Builders<TEntity>.Update.Set(x => x.Deleted, UtcNow);
		var options = new FindOneAndUpdateOptions<TEntity, TEntity> { IsUpsert = false };

		await Entities.FindOneAndUpdateAsync(filter, update, options);
	}

	#endregion


	#region IMongoRepo

	public override async Task<IEnumerable<TEntity>> GetAsync(FilterDefinition<TEntity>? filter, SortDefinition<TEntity>? sort = null, int? limit = null, int? skip = null)
	{
		filter = filter == null ? Filter.Ne(x => x.Deleted, (TDateTime?) null) : filter & Filter.Ne(x => x.Deleted, (TDateTime?) null);
		var options = new FindOptions<TEntity, TEntity>
		{
			Sort = sort,
			Limit = limit,
			Skip = skip
		};

		return await (await Entities.FindAsync(filter, options)).ToListAsync();
	}

	public override async Task<IEnumerable<TEntityProjection>> GetAsync<TEntityProjection>(FilterDefinition<TEntity>? filter, ProjectionDefinition<TEntity, TEntityProjection> projection, SortDefinition<TEntity>? sort = null, int? limit = null, int? skip = null)
	{
		filter = filter == null ? Filter.Ne(x => x.Deleted, (TDateTime?) null) : filter & Filter.Ne(x => x.Deleted, (TDateTime?) null);
		var options = new FindOptions<TEntity, TEntityProjection>
		{
			Projection = projection,
			Sort = sort,
			Limit = limit,
			Skip = skip
		};
		
		return await (await Entities.FindAsync(filter, options)).ToListAsync();
	}

	public override async Task UpdateAsync(TKey id, UpdateDefinition<TEntity> update)
	{
		var filter = Filter.Eq(existingItem => existingItem.Id, id) & Filter.Ne(x => x.Deleted, (TDateTime?) null);
		var options = new UpdateOptions { IsUpsert = false };

		await Entities.UpdateOneAsync(filter, update, options);
	}

	public override async Task UpdateAsync(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update)
	{
		filter &= Filter.Ne(x => x.Deleted, (TDateTime?) null);
		var options = new UpdateOptions { IsUpsert = false };

		await Entities.UpdateManyAsync(filter, update, options);
	}

	public override async Task DeleteAsync(FilterDefinition<TEntity> filter)
	{
		filter &= Filter.Ne(x => x.Deleted, (TDateTime?) null);
		var update = Builders<TEntity>.Update.Set(x => x.Deleted, UtcNow);
		var options = new UpdateOptions { IsUpsert = false };

		await Entities.UpdateManyAsync(filter, update, options);
	}

	#endregion


	#region ISoftDelRepo

	public virtual async Task<TEntity?> GetAsync(TKey id, SoftDelModes mode)
	{
		FilterDefinition<TEntity> filter;
		if (mode == SoftDelModes.Actual)
			filter = Filter.Eq(x => x.Id, id) & Filter.Eq(x => x.Deleted, (TDateTime?) null);
		else if (mode == SoftDelModes.Deleted)
			filter = Filter.Eq(x => x.Id, id) & Filter.Ne(x => x.Deleted, (TDateTime?) null);
		else
			filter = Filter.Eq(x => x.Id, id);

		return await (await Entities.FindAsync(filter)).SingleOrDefaultAsync();
	}

	public virtual async Task<IEnumerable<TEntity>> GetAsync(SoftDelModes mode)
	{
		FilterDefinition<TEntity> filter;
		if (mode == SoftDelModes.Actual)
			filter = Filter.Eq(x => x.Deleted, (TDateTime?) null);
		else if (mode == SoftDelModes.Deleted)
			filter = Filter.Ne(x => x.Deleted, (TDateTime?) null);
		else
			filter = Filter.Empty;

		return await (await Entities.FindAsync(filter)).ToListAsync();
	}

	public virtual async Task RestoreAsync(TKey id)
	{
		var filter = Filter.Eq(item => item.Id, id) & Filter.Ne(x => x.Deleted, (TDateTime?) null);
		var update = Builders<TEntity>.Update.Set(x => x.Deleted, (TDateTime?) null);
		var options = new FindOneAndUpdateOptions<TEntity, TEntity> { IsUpsert = false };

		await Entities.FindOneAndUpdateAsync(filter, update, options);
	}

	#endregion


	#region IMongoSoftDelRepo

	public virtual async Task<IEnumerable<TEntity>> GetAsync(SoftDelModes mode, FilterDefinition<TEntity>? filter, SortDefinition<TEntity>? sort = null, int? limit = null, int? skip = null)
	{
		if (filter == null) filter = Filter.Empty;
		if (mode == SoftDelModes.Actual)
			filter &= Filter.Eq(x => x.Deleted, (TDateTime?) null);
		else if (mode == SoftDelModes.Deleted)
			filter &= Filter.Ne(x => x.Deleted, (TDateTime?) null);

		var options = new FindOptions<TEntity, TEntity>
		{
			Sort = sort,
			Limit = limit,
			Skip = skip
		};

		return await (await Entities.FindAsync(filter, options)).ToListAsync();
	}

	public virtual async Task<IEnumerable<TEntityProjection>> GetAsync<TEntityProjection>(SoftDelModes mode, FilterDefinition<TEntity>? filter, ProjectionDefinition<TEntity, TEntityProjection> projection, SortDefinition<TEntity>? sort = null, int? limit = null, int? skip = null)
	{
		if (filter == null) filter = Filter.Empty;
		if (mode == SoftDelModes.Actual)
			filter &= Filter.Eq(x => x.Deleted, (TDateTime?) null);
		else if (mode == SoftDelModes.Deleted)
			filter &= Filter.Ne(x => x.Deleted, (TDateTime?) null);

		var options = new FindOptions<TEntity, TEntityProjection>
		{
			Projection = projection,
			Sort = sort,
			Limit = limit,
			Skip = skip
		};
		
		return await (await Entities.FindAsync(filter, options)).ToListAsync();
	}
	
	public virtual async Task RestoreAsync(FilterDefinition<TEntity> filter)
	{
		filter &= Filter.Ne(x => x.Deleted, (TDateTime?) null);
		var update = Builders<TEntity>.Update.Set(x => x.Deleted, (TDateTime?) null);
		var options = new FindOneAndUpdateOptions<TEntity, TEntity> { IsUpsert = false };

		await Entities.FindOneAndUpdateAsync(filter, update, options);
	}

	#endregion
}

internal class MongoGuidIdentityRepo<TEntity> : MongoRepo<Guid, TEntity> where TEntity : IIdentEntity<Guid>
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

internal class MongoIntIdentityRepo<TEntity> : MongoRepo<int, TEntity> where TEntity : IIdentEntity<int>
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

	/* 	public class JobIdGenerator : IIdGenerator
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
	} */

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

internal class MongoIntIdentitySoftDelRepo<TEntity> : MongoIntIdentityRepo<TEntity> where TEntity : IIdentEntity<int>, ISoftDelEntity<DateTimeOffset>
{
	public MongoIntIdentitySoftDelRepo(IMongoDbContext context, string collectionName)
		: base(context, collectionName)
	{
	}

	public override async Task DeleteAsync(int id)
	{
		var filter = Filter.Eq(item => item.Id, id) & Filter.Ne(x => x.Deleted, (DateTimeOffset?) null);
		var update = Builders<TEntity>.Update.Set(x => x.Deleted, DateTimeOffset.UtcNow);
		var options = new FindOneAndUpdateOptions<TEntity, TEntity> { IsUpsert = false };

		await Entities.FindOneAndUpdateAsync(filter, update, options);
	}
}

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