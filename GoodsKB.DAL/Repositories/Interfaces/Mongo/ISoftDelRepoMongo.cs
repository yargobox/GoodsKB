namespace GoodsKB.DAL.Repositories.Mongo;

using MongoDB.Driver;
using MongoDB.Driver.Linq;

public interface ISoftDelRepoMongo<K, T, TDateTime> : IRepoMongo<K, T, TDateTime>, ISoftDelRepo<K, T, TDateTime>
	where T : IEntity<K, TDateTime>, ISoftDelEntity<TDateTime>
	where TDateTime : struct
{
	IMongoQueryable<T> AsMongoQueryable(SoftDel mode);
	Task<IEnumerable<T>> MongoGetAsync(SoftDel mode, FilterDefinition<T>? where, SortDefinition<T>? orderBy = null, long? skip = null, int? take = null);
	Task<IEnumerable<P>> MongoGetAsync<P>(SoftDel mode, FilterDefinition<T>? where, ProjectionDefinition<T, P> projection, SortDefinition<T>? orderBy = null, long? skip = null, int? take = null);
	Task<long> MongoRestoreAsync(FilterDefinition<T> where);
}