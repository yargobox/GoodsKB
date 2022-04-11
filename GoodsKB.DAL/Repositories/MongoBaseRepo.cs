using GoodsKB.DAL.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GoodsKB.DAL.Repositories;

internal class MongoBaseRepo<TKey, TEntity> : IBaseRepo<TKey, TEntity>, IDisposable where TEntity : IEntityId<TKey>
{
	protected IMongoCollection<TEntity> _entities;
	protected IMongoDbContext _context;

	protected MongoBaseRepo(IMongoDbContext context, string collectionName)
	{
		_context = context;
		_entities = context.GetCollection<TEntity>(collectionName);
	}

	public virtual async Task<TKey> CreateAsync(TEntity item)
	{
		await _entities.InsertOneAsync(item);
		return await Task.FromResult(item.Id);
	}

	public virtual async Task<TEntity?> GetAsync(TKey id)
	{
		var filter = FilterBuilder.Eq(item => item.Id, id);
		return await _entities.Find(filter).SingleOrDefaultAsync();
	}

	public virtual async Task<IEnumerable<TEntity>> GetAsync()
	{
		return await _entities.Find(new BsonDocument()).ToListAsync();
	}

	public virtual async Task<IEnumerable<TEntity>> GetAsync(FilterDefinition<TEntity> filter)
	{
		return await _entities.Find(filter).ToListAsync();
	}

	public virtual async Task UpdateAsync(TEntity item)
	{
		var filter = FilterBuilder.Eq(existingItem => existingItem.Id, item.Id);
		await _entities.ReplaceOneAsync(filter, item);
	}

	public virtual async Task DeleteAsync(TKey id)
	{
		var filter = FilterBuilder.Eq(item => item.Id, id);
		await _entities.DeleteOneAsync(filter);
	}

	public FilterDefinitionBuilder<TEntity> FilterBuilder { get; private set; } = Builders<TEntity>.Filter;

	public bool Disposed { get; protected set; }
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
	~MongoBaseRepo()
	{
		Dispose(false);
	}
	protected virtual void Dispose(bool disposing)
	{
		if (Disposed) return;
		Disposed = true;

		_context = null!;
		_entities = null!;
		FilterBuilder = null!;
	}
}

internal class MongoGuidIdentityRepo<TEntity> : MongoBaseRepo<Guid, TEntity> where TEntity : IEntityId<Guid>
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

internal class MongoIntIdentityRepo<TEntity> : MongoBaseRepo<int, TEntity> where TEntity : IEntityId<int>
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