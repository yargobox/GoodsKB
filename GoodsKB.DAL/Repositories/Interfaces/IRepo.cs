namespace GoodsKB.DAL.Repositories;

using System.Linq.Expressions;

public interface IEntity<K>
{
	K? Id { get; set; }
}

public interface IRepo<K, T>
	where T : IEntity<K>
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