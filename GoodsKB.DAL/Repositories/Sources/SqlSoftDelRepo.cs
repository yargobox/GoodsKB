namespace GoodsKB.DAL.Repositories;

using System.Linq.Expressions;

internal class SqlSoftDelRepo<K, T, TDateTime> : SqlRepo<K, T>, ISoftDelRepo<K, T, TDateTime>
	where K : notnull
	where T : IIdentifiableEntity<K>, ISoftDelEntity<TDateTime>
	where TDateTime : struct
{
	protected SqlSoftDelRepo()
	{
		throw new NotSupportedException();
	}

	#region IRepo

	public override IQueryable<T> Query => throw new NotSupportedException();

	public override Task<long> GetCountAsync(Expression<Func<T, bool>>? where = null) => throw new NotSupportedException();

	public override Task<T?> GetAsync(K id) => throw new NotSupportedException();

	public override Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>>? where = null, OrderBy<T>? orderBy = null, long? skip = null, int? take = null) =>
		throw new NotSupportedException();

	public override Task<bool> DeleteAsync(K id)
	{
		throw new NotSupportedException();
	}

	public override Task<long> DeleteAsync(Expression<Func<T, bool>> where)
	{
		throw new NotSupportedException();
	}

	#endregion


	#region ISoftDelRepo

	public virtual IQueryable<T> GetQuery(SoftDel mode = SoftDel.All) => throw new NotSupportedException();

	public virtual Task<long> GetCountAsync(SoftDel mode, Expression<Func<T, bool>>? where = null)
	{
		throw new NotSupportedException();
	}

	public virtual Task<T?> GetAsync(SoftDel mode, K id)
	{
		throw new NotSupportedException();
	}

	public virtual Task<IEnumerable<T>> GetAsync(SoftDel mode, Expression<Func<T, bool>>? where = null, OrderBy<T>? orderBy = null, long? skip = null, int? take = null)
	{
		throw new NotSupportedException();
	}

	public virtual Task<bool> RestoreAsync(K id)
	{
		throw new NotSupportedException();
	}

	public virtual Task<long> RestoreAsync(Expression<Func<T, bool>> where)
	{
		throw new NotSupportedException();
	}

	#endregion
}