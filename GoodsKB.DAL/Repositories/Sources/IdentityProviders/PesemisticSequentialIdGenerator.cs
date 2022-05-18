namespace GoodsKB.DAL.Repositories;

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

internal sealed class PesemisticSequentialIdGenerator<TEntity, TDateTime> : IIdentityGenerator<int?>
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

	public PesemisticSequentialIdGenerator(int startAt = 1, int step = 1, int maxAttempts = 8)
	{
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
		var entity = (TEntity)document;

		return GenerateId(collection, entity);
	}

	public async Task<int?> GenerateIdAsync(object container, object document)
	{
		var collection = (IMongoCollection<TEntity>)container;
		var entity = (TEntity)document;

		return await GenerateIdAsync(collection, entity);
	}

	private int? GenerateId(IMongoCollection<TEntity> collection, TEntity entity)
	{
		var filter = Builders<TEntity>.Filter.Empty;
		var sort = Step > 0 ? Builders<TEntity>.Sort.Descending(x => x.Id) : Builders<TEntity>.Sort.Ascending(x => x.Id);
		var options = new FindOptions<TEntity, EntryId> { Limit = 1, Sort = sort };

		for (; ; )
		{
			var lastEntry = collection.FindSync(filter, options).FirstOrDefault();

			if (lastEntry == null)
			{
				return StartAt;
			}
			else if (lastEntry.Id != null)
			{
				var newId = lastEntry.Id.Value + Step;

				if (!IsEmpty(entity.Id) && entity.Id == newId)
				{
					Thread.Sleep(20);
				}

				return newId;
			}
			else
			{
				throw new InvalidDataException($"Could not obtain Id from a collection {collection.CollectionNamespace.CollectionName}.");
			}
		}
	}

	private async Task<int?> GenerateIdAsync(IMongoCollection<TEntity> collection, TEntity entity)
	{
		var filter = Builders<TEntity>.Filter.Empty;
		var sort = Step > 0 ? Builders<TEntity>.Sort.Descending(x => x.Id) : Builders<TEntity>.Sort.Ascending(x => x.Id);
		var options = new FindOptions<TEntity, EntryId> { Limit = 1, Sort = sort };

		for (; ; )
		{
			var lastEntry = await (await collection.FindAsync(filter, options)).FirstOrDefaultAsync();

			if (lastEntry == null)
			{
				return StartAt;
			}
			else if (lastEntry.Id != null)
			{
				var newId = lastEntry.Id.Value + Step;

				if (!IsEmpty(entity.Id) && entity.Id == newId)
				{
					await Task.Delay(20);
				}

				return newId;
			}
			else
			{
				throw new InvalidDataException($"Could not obtain Id from a collection {collection.CollectionNamespace.CollectionName}.");
			}
		}
	}
}