using GoodsKB.DAL.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;

namespace GoodsKB.DAL.Repositories;

internal class MongoRepoBase<TKey, TEntity> : IRepoBase<TKey, TEntity>
	where TKey : struct
	where TEntity : IIdentifiableEntity<TKey>
{
	protected readonly IMongoDbContext _context;
	protected readonly string _collectionName;
	protected readonly IMongoCollection<TEntity> _col;

	protected MongoRepoBase(IMongoDbContext context, string collectionName)
	{
		_context = context;
		_collectionName = collectionName;

		(IdentityType, IdentityProvider) = this.GetIdentityInfo();

		var filter = Builders<BsonDocument>.Filter.Eq("name", _collectionName);
		var options = new ListCollectionNamesOptions { Filter = filter };
		var exists = _context.DB.ListCollectionNames(options).Any();

		// wil be created if not exists
		_col = _context.GetCollection<TEntity>(_collectionName);

		if (!exists) OnRepositoryCreated();
		OnRepositoryCheck();
	}

	#region IRepoBase

	public virtual IQueryable<TEntity> Entities => _col.AsQueryable<TEntity>();
	public RepositoryIdentityTypes IdentityType { get; private set; }
	public RepositoryIdentityProviders IdentityProvider { get; private set; }

	protected FilterDefinitionBuilder<TEntity> _Filter => Builders<TEntity>.Filter;
	protected UpdateDefinitionBuilder<TEntity> _Update => Builders<TEntity>.Update;
	protected SortDefinitionBuilder<TEntity> _Sort => Builders<TEntity>.Sort;
	protected ProjectionDefinitionBuilder<TEntity> _Projection => Builders<TEntity>.Projection;
	protected IndexKeysDefinitionBuilder<TEntity> _IndexKeys => Builders<TEntity>.IndexKeys;

	public virtual async Task<TEntity> CreateAsync(TEntity entity)
	{
		if (IdentityType == RepositoryIdentityTypes.Sequential)
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

	#endregion

	protected virtual void OnRepositoryCreated()
	{
	}
	protected virtual void OnRepositoryCheck()
	{
	}

	protected async Task<IEnumerable<string>> GetIndexNames()
	{
		var indexNames = (await (await _col.Indexes.ListAsync()).ToListAsync()).Select(x => (string) x["name"]);
		return await Task.FromResult(indexNames);
	}
	protected async Task CreateIndex(string indexName, bool unique, Expression<Func<TEntity, object?>> filter, bool descending = false)
	{
		var options = new CreateIndexOptions { Name = indexName, Unique = unique };
		var indexModel = new CreateIndexModel<TEntity>(descending ? _IndexKeys.Descending(filter) : _IndexKeys.Ascending(filter), options);
		await _col.Indexes.CreateOneAsync(indexModel, new CreateOneIndexOptions { });
	}
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
