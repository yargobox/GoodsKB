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

public interface ISoftDelEntity<TDateTime>
	where TDateTime : struct
{
	TDateTime? Deleted { get; set; }
}

public interface ISoftDelRepo<K, T, TDateTime> : IRepo<K, T>
	where K : notnull
	where T : IIdentifiableEntity<K>, ISoftDelEntity<TDateTime>
	where TDateTime : struct
{
	IQueryable<T> GetQuery(SoftDel mode = SoftDel.All);
	Task<long> GetCountAsync(SoftDel mode, Expression<Func<T, bool>>? where = null);
	Task<T?> GetAsync(SoftDel mode, K id);
	Task<IEnumerable<T>> GetAsync(SoftDel mode, Expression<Func<T, bool>>? where = null, OrderBy<T>? orderBy = null, long? skip = null, int? take = null);
	Task<bool> RestoreAsync(K id);
	Task<long> RestoreAsync(Expression<Func<T, bool>> where);
}