namespace GoodsKB.DAL.Repositories;

using System.Linq.Expressions;
using GoodsKB.DAL.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

internal class MongoRepo<K, T, TDateTime> : IRepo<K, T, TDateTime>
	where T : IEntity<K, TDateTime>
	where TDateTime : struct
{
	protected readonly IMongoDbContext _context;
	protected readonly string _collectionName;
	protected readonly IMongoCollection<T> _col;
	protected readonly bool _hasCreated;
	protected readonly bool _hasUpdated;
	protected readonly bool _hasModified;

	protected static readonly FilterDefinitionBuilder<T> _Filter = Builders<T>.Filter;
	protected static readonly UpdateDefinitionBuilder<T> _Update = Builders<T>.Update;
	protected static readonly SortDefinitionBuilder<T> _Sort = Builders<T>.Sort;
	protected static readonly ProjectionDefinitionBuilder<T> _Projection = Builders<T>.Projection;
	protected static readonly IndexKeysDefinitionBuilder<T> _IndexKeys = Builders<T>.IndexKeys;

	protected MongoRepo(IMongoDbContext context, string collectionName, IIdentityGenerator<K>? identityGenerator = null)
	{
		_context = context;
		_collectionName = collectionName;
		IdentityGenerator = identityGenerator;

		_hasCreated = typeof(T).IsAssignableTo(typeof(ICreatedEntity<K, TDateTime>));
		_hasUpdated = typeof(T).IsAssignableTo(typeof(IUpdatedEntity<K, TDateTime>));
		_hasModified = typeof(T).IsAssignableTo(typeof(IModifiedEntity<K, TDateTime>));

		var filter = Builders<BsonDocument>.Filter.Eq("name", _collectionName);
		var options = new ListCollectionNamesOptions { Filter = filter };
		var exists = _context.DB.ListCollectionNames(options).Any();

		if (!exists)
		{
			var createOptions = new CreateCollectionOptions<T> { Collation = Collations.Ukrainian_CI_AS };

			OnCreate(createOptions);
		}

		_col = _context.GetCollection<T>(_collectionName);

		OnLoad();
	}

	#region IRepo

	public virtual IQueryable<T> AsQueryable() => _col.AsQueryable<T>();
	public IIdentityGenerator<K>? IdentityGenerator { get; }

	public virtual async Task<long> GetCountAsync(Expression<Func<T, bool>>? where = null) => await _col.CountDocumentsAsync(where ?? _Filter.Empty);

	public virtual async Task<T?> GetAsync(K id)
	{
		var filter = _Filter.Eq(x => x.Id, id);

		return await (await _col.FindAsync(filter)).SingleOrDefaultAsync();
	}

	public virtual async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>>? where = null, OrderBy<T>? orderBy = null, long? skip = null, int? take = null)
	{
		var options = new FindOptions<T, T>
		{
			Sort = OrderByToSortDefinition(orderBy),
			Skip = (int?)skip,
			Limit = take
		};

		return await (await _col.FindAsync(where ?? _Filter.Empty, options)).ToListAsync();
	}

	public virtual async Task<T> CreateAsync(T entity)
	{
		var generateId = IdentityGenerator != null && IdentityGenerator.IsEmpty(entity.Id);
		var fillCreated = false;
		var fillModified = false;

		for (int i = 0; ; )
		{
			if (generateId)
			{
				entity.Id = await IdentityGenerator!.GenerateIdAsync(_col, entity);
			}

			if (_hasCreated)
			{
				var createdEntity = (ICreatedEntity<K, TDateTime>)entity;
				if (createdEntity.Created == null || fillCreated)
				{
					fillCreated = true;
					createdEntity.Created = (TDateTime)(object)DateTimeOffset.Now;
				}
			}
			else if (_hasModified)
			{
				var modifiedEntity = (IModifiedEntity<K, TDateTime>)entity;
				if (modifiedEntity.Modified == null || fillModified)
				{
					fillModified = true;
					modifiedEntity.Modified = (TDateTime)(object)DateTimeOffset.Now;
				}
			}

			try
			{
				await _col.InsertOneAsync(entity);

				return await Task.FromResult(entity);
			}
			catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
			{
				if (generateId && ++i < IdentityGenerator!.MaxAttempts)
				{
					continue;
				}

				throw;
			}
		}
	}

	public virtual async Task<bool> UpdateAsync(T entity)
	{
		var filter = _Filter.Eq(x => x.Id, entity.Id);
		var options = new ReplaceOptions { IsUpsert = false };

		if (_hasUpdated)
		{
			var updatedEntity = (IUpdatedEntity<K, TDateTime>)entity;
			if (updatedEntity.Updated == null)
			{
				updatedEntity.Updated = (TDateTime)(object)DateTimeOffset.Now;
			}
		}
		else if (_hasModified)
		{
			var modifiedEntity = (IModifiedEntity<K, TDateTime>)entity;
			if (modifiedEntity.Modified == null)
			{
				modifiedEntity.Modified = (TDateTime)(object)DateTimeOffset.Now;
			}
		}

		var result = await _col.ReplaceOneAsync(filter, entity);
		return result.IsModifiedCountAvailable && result.ModifiedCount > 0;
	}

	public virtual async Task<T> UpdateCreateAsync(T entity)
	{
		if (IdentityGenerator != null)
		{
			throw new NotSupportedException($"{nameof(MongoRepo<K, T, TDateTime>)}.{nameof(UpdateCreateAsync)}() cannot be used with an external Id generator.");
		}

		var filter = _Filter.Eq(x => x.Id, entity.Id);
		var options = new ReplaceOptions { IsUpsert = true };

		if (_hasUpdated)
		{
			var updatedEntity = (IUpdatedEntity<K, TDateTime>)entity;
			if (updatedEntity.Updated == null)
			{
				updatedEntity.Updated = (TDateTime)(object)DateTimeOffset.Now;
			}
		}
		else if (_hasModified)
		{
			var modifiedEntity = (IModifiedEntity<K, TDateTime>)entity;
			if (modifiedEntity.Modified == null)
			{
				modifiedEntity.Modified = (TDateTime)(object)DateTimeOffset.Now;
			}
		}
		
		var result = await _col.ReplaceOneAsync(filter, entity, options);
		if (result.MatchedCount == 0)
		{
			entity.Id = (K)BsonTypeMapper.MapToDotNetValue(result.UpsertedId);
		}
		return await Task.FromResult(entity);
	}

	public virtual async Task<bool> DeleteAsync(K id)
	{
		var filter = _Filter.Eq(x => x.Id, id);

		var result = await _col.DeleteOneAsync(filter);
		return await Task.FromResult(result.DeletedCount > 0);
	}

	public virtual async Task<long> DeleteAsync(Expression<Func<T, bool>> where)
	{
		var result = await _col.DeleteManyAsync(where);
		
		return await Task.FromResult(result.DeletedCount);
	}

	#endregion

	/// <summary>
	/// Method is called before the collection creation. It is not blocking, may attempt twice.
	/// </summary>
	protected virtual bool OnCreate(CreateCollectionOptions<T> options)
	{
		try
		{
			_context.DB.CreateCollection(_collectionName, options);
			return true;
		}
		catch (MongoCommandException ex) when (ex.Code == 48/*  && ex.CodeName == "NamespaceExists" */)
		{
			return false;
		}
		catch (Exception ex) when ((ex.InnerException as MongoCommandException)?.Code == 48)
		{
			return false;
		}
	}

	/// <summary>
	/// Method is called always from the repository constructor.
	/// </summary>
	protected virtual void OnLoad()
	{
	}

	protected async Task<IEnumerable<string>> GetIndexNames()
	{
		var indexNames = (await (await _col.Indexes.ListAsync()).ToListAsync()).Select(x => (string)x["name"]);

		return await Task.FromResult(indexNames);
	}

	protected static readonly Expression<Func<T, bool>> _valueExistsIndexCondition = x => x.Id != null;

	protected async Task CreateIndexAsync(string indexName, bool unique, bool sparse, Expression<Func<T, object?>> memberSelector, bool descending = false, Collation? collation = null, Expression<Func<T, bool>>? where = null)
	{
		var options = new CreateIndexOptions<T>
		{
			Name = indexName,
			Unique = unique,
			Sparse = sparse,
			Collation = collation
		};

		if (where != null)
		{
			if (object.ReferenceEquals(where, _valueExistsIndexCondition))
			{
				options.PartialFilterExpression = _Filter.Exists(memberSelector);
			}
			else
			{
				options.PartialFilterExpression = _Filter.Where(where);
			}
		}

		var indexModel = new CreateIndexModel<T>(
			descending ?
				_IndexKeys.Descending(memberSelector) :
				_IndexKeys.Ascending(memberSelector),
			options);

		await _col.Indexes.CreateOneAsync(indexModel, new CreateOneIndexOptions { });
	}

	protected static SortDefinition<T>? OrderByToSortDefinition(OrderBy<T>? orderBy)
	{
		var sortOrders = orderBy?.SortOrders;

		if (sortOrders != null && sortOrders.Length > 0)
		{
			var sort = sortOrders[0].Descending ? _Sort.Descending(sortOrders[0].Name) : _Sort.Ascending(sortOrders[0].Name);
			for (int i = 1; i < sortOrders.Length; i++)
			{
				sort = sortOrders[i].Descending ? sort.Descending(sortOrders[i].Name) : sort.Ascending(sortOrders[i].Name);
			}
			return sort;
		}

		return null;
	}
}