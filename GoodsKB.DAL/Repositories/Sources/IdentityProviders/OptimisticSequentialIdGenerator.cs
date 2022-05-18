namespace GoodsKB.DAL.Repositories;

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

internal sealed class OptimisticSequentialIdGenerator<TEntity, TDateTime> : IIdentityGenerator<int?>
	where TEntity : IEntity<int?, TDateTime>
{
	[BsonIgnoreExtraElements]
	private sealed class EntryId : IEntity<int?, TDateTime>
	{
		public int? Id { get; set; }
	}

	public int MaxAttempts { get; }

	public readonly int StartAt;
	public readonly int Step;

	private int _lastKnownId = int.MinValue;
	private IMongoCollection<TEntity>? _collection;

	public OptimisticSequentialIdGenerator(int startAt = 1, int step = 1, int maxAttempts = 10)
	{
		if (startAt == int.MinValue) throw new ArgumentOutOfRangeException(nameof(startAt) + " cannot be equal to the lowest 'int' value.");
		if (step == 0) throw new ArgumentException(nameof(step) + " cannot be zero.");
		if (maxAttempts < 1) throw new ArgumentOutOfRangeException(nameof(maxAttempts));

		StartAt = startAt;
		Step = step;
		MaxAttempts = maxAttempts;
	}

	public bool IsEmpty(int? id) => id == null || (Step > 0 ? id < StartAt : id > StartAt);

	public int? GenerateId(object container, object document)
	{
		var collection = (IMongoCollection<TEntity>)container;

		if (_collection == null)
		{
			_collection = collection;
		}
		else if (_collection != collection)
		{
			throw new InvalidOperationException($"The identity provider was initialized for {_collection.CollectionNamespace.CollectionName}, but another {collection.CollectionNamespace.CollectionName} is using it.");
		}

		return GenerateId();
	}

	public async Task<int?> GenerateIdAsync(object container, object document)
	{
		var collection = (IMongoCollection<TEntity>)container;

		if (_collection == null)
		{
			_collection = collection;
		}
		else if (_collection != collection)
		{
			throw new InvalidOperationException($"The identity provider was initialized for {_collection.CollectionNamespace.CollectionName}, but another {collection.CollectionNamespace.CollectionName} is using it.");
		}

		return await GenerateIdAsync();
	}

	private int GenerateId()
	{
		if (_lastKnownId == int.MinValue)
		{
			var filter = Builders<TEntity>.Filter.Empty;
			var sort = Step > 0 ? Builders<TEntity>.Sort.Descending(x => x.Id) : Builders<TEntity>.Sort.Ascending(x => x.Id);
			var options = new FindOptions<TEntity, EntryId> { Limit = 1, Sort = sort };

			var lastEntry = _collection!.FindSync(filter, options).FirstOrDefault();

			if (lastEntry == null)
			{
				if (System.Threading.Interlocked.CompareExchange(ref _lastKnownId, StartAt, int.MinValue) == int.MinValue)
				{
					return StartAt;
				}
			}
			else if (lastEntry.Id != null)
			{
				if (System.Threading.Interlocked.CompareExchange(ref _lastKnownId, lastEntry.Id.Value + Step, int.MinValue) == int.MinValue)
				{
					return lastEntry.Id.Value + Step;
				}
			}
			else
			{
				throw new InvalidDataException($"Could not obtain Id from a collection {_collection!.CollectionNamespace.CollectionName}.");
			}
		}

		int lastId, newId;
		do
		{
			lastId = _lastKnownId;
			newId = lastId + Step;
			if (newId == int.MinValue) throw new OverflowException();
		}
		while (System.Threading.Interlocked.CompareExchange(ref _lastKnownId, newId, lastId) != lastId);

		return newId;
	}

	private async Task<int> GenerateIdAsync()
	{
		if (_lastKnownId == int.MinValue)
		{
			var filter = Builders<TEntity>.Filter.Empty;
			var sort = Step > 0 ? Builders<TEntity>.Sort.Descending(x => x.Id) : Builders<TEntity>.Sort.Ascending(x => x.Id);
			var options = new FindOptions<TEntity, EntryId> { Limit = 1, Sort = sort };

			var lastEntry = await (await _collection!.FindAsync(filter, options)).FirstOrDefaultAsync();

			if (lastEntry == null)
			{
				if (System.Threading.Interlocked.CompareExchange(ref _lastKnownId, StartAt, int.MinValue) == int.MinValue)
				{
					return StartAt;
				}
			}
			else if (lastEntry.Id != null)
			{
				if (System.Threading.Interlocked.CompareExchange(ref _lastKnownId, lastEntry.Id.Value + Step, int.MinValue) == int.MinValue)
				{
					return lastEntry.Id.Value + Step;
				}
			}
			else
			{
				throw new InvalidDataException($"Could not obtain Id from a collection {_collection!.CollectionNamespace.CollectionName}.");
			}
		}

		int lastId, newId;
		do
		{
			lastId = _lastKnownId;
			newId = lastId + Step;
			if (newId == int.MinValue) throw new OverflowException();
		}
		while (System.Threading.Interlocked.CompareExchange(ref _lastKnownId, newId, lastId) != lastId);

		return newId;
	}
}