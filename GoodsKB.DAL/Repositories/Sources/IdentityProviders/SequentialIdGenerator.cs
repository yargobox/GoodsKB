namespace GoodsKB.DAL.Repositories;

using MongoDB.Driver;

internal sealed class SequentialIdGenerator<TEntity, TDateTime> : IIdentityGenerator<int?>
	where TEntity : IEntity<int?, TDateTime>
{
	private record IdentityCounter
	{
		public string? Id;
		public int? Counter;
	}

	public int MaxAttempts => 1;

	public int StartAt { get; }
	public int Step { get; }

	private IMongoCollection<TEntity>? _collection;
	private IMongoCollection<IdentityCounter>? _counters;

	public SequentialIdGenerator(int startAt = 1, int step = 1)
	{
		if (step == 0) throw new ArgumentException(nameof(step) + " cannot be zero.");

		StartAt = startAt;
		Step = step;
	}

	public bool IsEmpty(int? id) => id == null || (Step > 0 ? id < StartAt : id > StartAt);

	public int? GenerateId(object container, object document)
	{
		var collection = (IMongoCollection<TEntity>)container;

		if (_collection == null)
		{
			_counters = collection.Database.GetCollection<IdentityCounter>("identity_counters");
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
			_counters = collection.Database.GetCollection<IdentityCounter>("identity_counters");
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
		var filter = Builders<IdentityCounter>.Filter
			.Eq(x => x.Id, _collection!.CollectionNamespace.FullName);
		var update = Builders<IdentityCounter>.Update
			.Inc(x => x.Counter, Step);
		var options = new FindOneAndUpdateOptions<IdentityCounter, IdentityCounter?>
		{
			IsUpsert = false,
			ReturnDocument = ReturnDocument.After
		};

		for (int i = 0; i < 2; i++)
		{
			var identityCounter = _counters!.FindOneAndUpdate(filter, update, options);
			if (identityCounter == null)
			{
				identityCounter = new IdentityCounter() { Id = _collection!.CollectionNamespace.FullName, Counter = StartAt };
				try
				{
					_counters.InsertOne(identityCounter);
				}
				catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
				{
					continue;
				}

				return identityCounter.Counter.Value;
			}
			else if (identityCounter.Counter != null)
			{
				return identityCounter.Counter.Value;
			}
			else
			{
				throw new InvalidDataException($"Could not obtain Id for a collection {_collection!.CollectionNamespace.CollectionName}.");
			}
		}
		
		throw new InvalidDataException($"Could not generate a new id for a collection {_collection!.CollectionNamespace.CollectionName}.");
	}

	private async Task<int> GenerateIdAsync()
	{
		var filter = Builders<IdentityCounter>.Filter
			.Eq(x => x.Id, _collection!.CollectionNamespace.FullName);
		var update = Builders<IdentityCounter>.Update
			.Inc(x => x.Counter, Step);
		var options = new FindOneAndUpdateOptions<IdentityCounter, IdentityCounter?>
		{
			IsUpsert = false,
			ReturnDocument = ReturnDocument.After
		};

		for (int i = 0; i < 2; i++)
		{
			var identityCounter = await _counters!.FindOneAndUpdateAsync(filter, update, options);
			if (identityCounter == null)
			{
				identityCounter = new IdentityCounter() { Id = _collection!.CollectionNamespace.FullName, Counter = StartAt };
				try
				{
					await _counters.InsertOneAsync(identityCounter);
				}
				catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
				{
					continue;
				}

				return await Task.FromResult(identityCounter.Counter.Value);
			}
			else if (identityCounter.Counter != null)
			{
				return await Task.FromResult(identityCounter.Counter.Value);
			}
			else
			{
				throw new InvalidDataException($"Could not obtain Id for a collection {_collection!.CollectionNamespace.CollectionName}.");
			}
		}
		
		throw new InvalidDataException($"Could not generate a new id for a collection {_collection!.CollectionNamespace.CollectionName}.");
	}
}