using System.Linq.Expressions;

namespace GoodsKB.DAL.Repositories;

public enum SoftDelModes
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

public interface ISoftDelRepo<TKey, TEntity, TDateTime> : IRepoBase<TKey, TEntity>
	where TEntity : IIdentifiableEntity<TKey>, ISoftDelEntity<TDateTime>
	where TDateTime : struct
{
	IQueryable<TEntity> GetEntities(SoftDelModes mode);
	Task<long> GetCountAsync(SoftDelModes mode);
	Task<TEntity?> GetAsync(SoftDelModes mode, TKey id);
	Task<IEnumerable<TEntity>> GetAsync(SoftDelModes mode, Expression<Func<TEntity, bool>>? filter = null, int? limit = null);
	Task<bool> RestoreAsync(TKey id);
	Task<long> RestoreAsync(Expression<Func<TEntity, bool>> filter);
}