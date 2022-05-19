namespace GoodsKB.DAL.Repositories;

using System.Linq.Expressions;
using GoodsKB.DAL.Configuration;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

internal class MongoSoftDelRepo<K, T, TDateTime> : MongoRepo<K, T, TDateTime>, ISoftDelRepo<K, T, TDateTime>
	where T : IEntity<K, TDateTime>, ISoftDelEntity<TDateTime>
	where TDateTime : struct
{
	protected MongoSoftDelRepo(IMongoDbContext context, string collectionName, IIdentityGenerator<K>? identityGenerator = null)
		: base(context, collectionName, identityGenerator)
	{
	}

	#region IRepo

	public override IQueryable<T> AsQueryable() => AsMongoQueryableInternal(SoftDel.Actual);

	public override async Task<long> GetCountAsync(Expression<Func<T, bool>>? where = null) => await GetCountAsync(SoftDel.Actual, where);

	public override async Task<T?> GetAsync(K id) => await GetAsync(SoftDel.Actual, id);

	public override async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>>? where = null, OrderBy<T>? orderBy = null, long? skip = null, int? take = null) =>
		await GetAsync(SoftDel.Actual, where, orderBy, skip, take);

	public override async Task<bool> DeleteAsync(K id)
	{
		var filter = _Filter.Eq(item => item.Id, id) & _Filter.Eq(x => x.Deleted, null);
		var update = Builders<T>.Update.CurrentDate(x => x.Deleted, UpdateDefinitionCurrentDateType.Date);
		var options = new UpdateOptions { IsUpsert = false };

		var result = await _col.UpdateOneAsync(filter, update, options);
		return await Task.FromResult(result.ModifiedCount > 0);
	}

	public override async Task<long> DeleteAsync(Expression<Func<T, bool>> where)
	{
		var filter = _Filter.Where(where) & _Filter.Where(x => x.Deleted == null);
		var update = Builders<T>.Update.CurrentDate(x => x.Deleted, UpdateDefinitionCurrentDateType.Date);
		var options = new UpdateOptions { IsUpsert = false };

		var result = await _col.UpdateManyAsync(filter, update, options);
		return await Task.FromResult(result.ModifiedCount);
	}

	#endregion


	#region ISoftDelRepo

	public virtual IQueryable<T> AsQueryable(SoftDel mode) => AsMongoQueryableInternal(mode);

	public virtual async Task<long> GetCountAsync(SoftDel mode, Expression<Func<T, bool>>? where = null)
	{
		var filter = MakeSoftDelFilter(mode);
		if (where != null)
		{
			filter = _Filter.Where(where) & filter;
		}

		Console.WriteLine(filter.Render(_col.DocumentSerializer, _col.Settings.SerializerRegistry).ToString());//!!!

		return await _col.CountDocumentsAsync(filter);
	}

	public virtual async Task<T?> GetAsync(SoftDel mode, K id)
	{
		var filter = _Filter.Eq(x => x.Id, id) & MakeSoftDelFilter(mode);

		return await (await _col.FindAsync(filter)).SingleOrDefaultAsync();
	}

	public virtual async Task<IEnumerable<T>> GetAsync(SoftDel mode, Expression<Func<T, bool>>? where = null, OrderBy<T>? orderBy = null, long? skip = null, int? take = null)
	{
		var filter = MakeSoftDelFilter(mode);
		if (where != null)
		{
			filter = _Filter.Where(where) & filter;
		}

		var options = new FindOptions<T>
		{
			Sort = OrderByToSortDefinition(orderBy),
			Skip = (int?)skip,
			Limit = take
		};

		//Console.WriteLine(filter.Render(_col.DocumentSerializer, _col.Settings.SerializerRegistry).ToString());//!!!

		return await (await _col.FindAsync(filter, options)).ToListAsync();
	}

	public virtual async Task<bool> RestoreAsync(K id)
	{
		var filter = _Filter.Eq(item => item.Id, id) & _Filter.Ne(x => x.Deleted, null);
		var update = Builders<T>.Update.Unset(x => x.Deleted);
		var options = new UpdateOptions { IsUpsert = false };

		var result = await _col.UpdateOneAsync(filter, update, options);
		return await Task.FromResult(result.ModifiedCount > 0);
	}

	public virtual async Task<long> RestoreAsync(Expression<Func<T, bool>> where)
	{
		var filter = _Filter.Where(where) & _Filter.Ne(x => x.Deleted, null);
		var update = Builders<T>.Update.Unset(x => x.Deleted);
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
			var indexModel = new CreateIndexModel<T>(Builders<T>.IndexKeys.Descending(x => x.Deleted), options);
			
			await _col.Indexes.CreateOneAsync(indexModel, new CreateOneIndexOptions { });
		}
	}

	protected IMongoQueryable<T> AsMongoQueryableInternal(SoftDel mode = SoftDel.All) => mode switch
		{
			SoftDel.Actual => _col.AsQueryable<T>().Where(x => x.Deleted == null),
			SoftDel.Deleted => _col.AsQueryable<T>().Where(x => x.Deleted != null),
			SoftDel.All => _col.AsQueryable<T>(),
			_ => throw new ArgumentException(nameof(mode))
		};

	protected static FilterDefinition<T> MakeSoftDelFilter(SoftDel mode) => mode switch
		{
			SoftDel.Actual => _Filter.Eq(x => x.Deleted, null),
			SoftDel.Deleted => _Filter.Ne(x => x.Deleted, null),
			SoftDel.All => _Filter.Empty,
			_ => throw new ArgumentException(nameof(mode))
		};
}