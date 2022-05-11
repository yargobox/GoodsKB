namespace GoodsKB.DAL.Repositories.Mongo;

using MongoDB.Driver;
using MongoDB.Driver.Linq;

public interface ISoftDelRepoMongo<K, T, TDateTime> : IRepoMongo<K, T>, ISoftDelRepo<K, T, TDateTime>
	where K : notnull
	where T : IIdentifiableEntity<K>, ISoftDelEntity<TDateTime>
	where TDateTime : struct
{
	IMongoQueryable<T> GetMongoQuery(SoftDel mode = SoftDel.All);
	Task<IEnumerable<T>> GetAsync(SoftDel mode, FilterDefinition<T>? where, SortDefinition<T>? orderBy = null, long? skip = null, int? take = null);
	Task<IEnumerable<P>> GetAsync<P>(SoftDel mode, FilterDefinition<T>? where, ProjectionDefinition<T, P> projection, SortDefinition<T>? orderBy = null, long? skip = null, int? take = null);
	Task<long> RestoreAsync(FilterDefinition<T> where);
}