namespace GoodsKB.DAL.Repositories;

using System.Linq.Expressions;

/// <summary>
/// Soft Delete Mode
/// </summary>
public enum SoftDel
{
	Actual = 0,
	Deleted = 1,
	All = 2
}

public interface ISoftDelRepo<K, T, TDateTime> : IRepo<K, T, TDateTime>
	where T : IEntity<K, TDateTime>, ISoftDelEntity<TDateTime>
	where TDateTime : struct
{
	IQueryable<T> AsQueryable(SoftDel mode);
	Task<long> GetCountAsync(SoftDel mode, Expression<Func<T, bool>>? where = null);
	Task<T?> GetAsync(SoftDel mode, K id);
	Task<IEnumerable<T>> GetAsync(SoftDel mode, Expression<Func<T, bool>>? where = null, OrderBy<T>? orderBy = null, long? skip = null, int? take = null);
	Task<bool> RestoreAsync(K id);
	Task<long> RestoreAsync(Expression<Func<T, bool>> where);
}