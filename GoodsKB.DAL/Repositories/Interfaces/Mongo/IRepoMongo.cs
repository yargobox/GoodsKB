namespace GoodsKB.DAL.Repositories.Mongo;

using MongoDB.Driver;
using MongoDB.Driver.Linq;

public interface IRepoMongo<K, T, TDateTime> : IRepo<K, T, TDateTime>
	where T : IEntity<K, TDateTime>
	where TDateTime : struct
{
	IMongoCollection<T> Collection { get; }
	IMongoQueryable<T> AsMongoQueryable();

	FilterDefinitionBuilder<T> Filter { get; }
	UpdateDefinitionBuilder<T> Update { get; }
	SortDefinitionBuilder<T> Sort { get; }
	ProjectionDefinitionBuilder<T> Projection { get; }

	Task<IEnumerable<T>> MongoGetAsync(FilterDefinition<T>? where, SortDefinition<T>? orderBy = null, long? skip = null, int? take = null);
	Task<IEnumerable<P>> MongoGetAsync<P>(FilterDefinition<T>? where, ProjectionDefinition<T, P> projection, SortDefinition<T>? orderBy = null, long? skip = null, int? take = null);
	Task<long> MongoUpdateAsync(FilterDefinition<T> where, UpdateDefinition<T> update);
}