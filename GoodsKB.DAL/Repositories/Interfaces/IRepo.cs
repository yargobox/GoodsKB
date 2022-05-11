namespace GoodsKB.DAL.Repositories;

using System.Linq.Expressions;
using GoodsKB.DAL.Repositories.Filters;
using GoodsKB.DAL.Repositories.SortOrders;

public interface IIdentifiableEntity<K>
	where K : notnull
{
	K Id { get; set; }
}

public interface IRepo<K, T>
	where K : notnull
	where T : IIdentifiableEntity<K>
{
	IQueryable<T> Query { get; }
	IIdentityProvider<K>? IdentityProvider { get; }
	Task<long> GetCountAsync(Expression<Func<T, bool>>? where = null);
	Task<T?> GetAsync(K id);
	Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>>? where = null, OrderBy<T>? orderBy = null, long? skip = null, int? take = null);
	Task<T> CreateAsync(T entity);
	Task<bool> UpdateAsync(T entity);
	Task<T> UpdateCreateAsync(T entity);
	Task<bool> DeleteAsync(K id);
	Task<long> DeleteAsync(Expression<Func<T, bool>> where);
}