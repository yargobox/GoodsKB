namespace GoodsKB.DAL.Repositories.Mongo;

using MongoDB.Driver;
using MongoDB.Driver.Linq;

public interface IRepoMongo<K, T> : IRepo<K, T>
	where T : IEntity<K>
{
	IMongoCollection<T> Collection { get; }
	IMongoQueryable<T> MongoQuery { get; }

	FilterDefinitionBuilder<T> Filter { get; }
	UpdateDefinitionBuilder<T> Update { get; }
	SortDefinitionBuilder<T> Sort { get; }
	ProjectionDefinitionBuilder<T> Projection { get; }

	Task<IEnumerable<T>> MongoGetAsync(FilterDefinition<T>? where, SortDefinition<T>? orderBy = null, long? skip = null, int? take = null);
	Task<IEnumerable<P>> MongoGetAsync<P>(FilterDefinition<T>? where, ProjectionDefinition<T, P> projection, SortDefinition<T>? orderBy = null, long? skip = null, int? take = null);
	Task<long> MongoUpdateAsync(FilterDefinition<T> where, UpdateDefinition<T> update);
}