using GoodsKB.DAL.Configuration;
using MongoDB.Driver;

namespace GoodsKB.DAL.Repositories;

internal sealed class SequenceIdentityProvider<TKey> : IIdentityProvider<TKey>
{
	private record IdentityCounter
	{
		public string? Id;
		public TKey? Value;
	}

	public TKey BeforeStart { get; }
	public TKey Step { get; }

	private readonly IMongoCollection<IdentityCounter> _counters;
	private readonly string _collectionName;
	private readonly Func<TKey?, bool> _shouldProvideNextIdentity;

	public SequenceIdentityProvider(IMongoDbContext context, string collectionName, TKey beforeStart, TKey step, Func<TKey?, bool> shouldProvideNextIdentity)
	{
		_collectionName = collectionName;
		BeforeStart = beforeStart;
		Step = step;
		_shouldProvideNextIdentity = shouldProvideNextIdentity;

		_counters = context.DB.GetCollection<IdentityCounter>("identity_counters");
	}

	public async Task LoadAsync()
	{
		var filter = Builders<IdentityCounter>.Filter
			.Eq(x => x.Id, _collectionName);
		var update = Builders<IdentityCounter>.Update
			.SetOnInsert(x => x.Id, _collectionName)
			.SetOnInsert(x => x.Value, BeforeStart);
		var options = new UpdateOptions
		{
			IsUpsert = true
		};

		await _counters.UpdateOneAsync(filter, update, options);
	}

	public bool ShouldProvideNextIdentity(TKey? id) => _shouldProvideNextIdentity(id);

	public async Task<TKey> NextIdentityAsync()
	{
		var filter = Builders<IdentityCounter>.Filter
			.Eq(x => x.Id, _collectionName);
		var update = Builders<IdentityCounter>.Update
			.Inc(x => x.Value, Step);
		var options = new FindOneAndUpdateOptions<IdentityCounter, IdentityCounter>
		{
			IsUpsert = true,
			ReturnDocument = ReturnDocument.After
		};

		var id = (await _counters.FindOneAndUpdateAsync(filter, update, options)).Value;
		if (id is not null)
			return await Task.FromResult(id);
		throw new InvalidOperationException();
	}

	public async Task<TKey> LastIdentityAsync()
	{
		var filter = Builders<IdentityCounter>.Filter
			.Eq(x => x.Id, _collectionName);
		var initialCounter = new IdentityCounter
		{
			Id = _collectionName,
			Value = BeforeStart
		};
		var options = new FindOneAndReplaceOptions<IdentityCounter, IdentityCounter>
		{
			IsUpsert = true,
			ReturnDocument = ReturnDocument.After
		};

		var id = (await _counters.FindOneAndReplaceAsync(filter, initialCounter, options)).Value;
		if (id is not null)
			return await Task.FromResult(id);
		throw new InvalidOperationException();
	}
}