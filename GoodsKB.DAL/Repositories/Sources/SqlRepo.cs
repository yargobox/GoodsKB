namespace GoodsKB.DAL.Repositories;

using System.Linq.Expressions;

internal class SqlRepo<K, T, TDateTime> : IRepo<K, T, TDateTime>
	where T : IEntity<K, TDateTime>
	where TDateTime : struct
{
	protected SqlRepo()
	{
		throw new NotSupportedException();
	}

	#region IRepo

	public virtual IQueryable<T> AsQueryable() => throw new NotSupportedException();
	public IIdentityGenerator<K>? IdentityGenerator { get; }

	public virtual Task<long> GetCountAsync(Expression<Func<T, bool>>? where = null) => throw new NotSupportedException();

	public virtual Task<T?> GetAsync(K id)
	{
		throw new NotSupportedException();
	}

	public virtual Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>>? where = null, OrderBy<T>? orderBy = null, long? skip = null, int? take = null)
	{
		throw new NotSupportedException();
	}

	public virtual Task<T> CreateAsync(T entity)
	{
		throw new NotSupportedException();
	}

	public virtual Task<bool> UpdateAsync(T entity)
	{
		throw new NotSupportedException();
	}

	public virtual Task<T> UpdateCreateAsync(T entity)
	{
		throw new NotSupportedException();
	}

	public virtual Task<bool> DeleteAsync(K id)
	{
		throw new NotSupportedException();
	}

	public virtual Task<long> DeleteAsync(Expression<Func<T, bool>> where)
	{
		throw new NotSupportedException();
	}

	#endregion
}