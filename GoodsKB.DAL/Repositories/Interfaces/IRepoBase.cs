using System.Linq.Expressions;

namespace GoodsKB.DAL.Repositories;

public interface IIdentifiableEntity<TKey>
{
	TKey Id { get; set; }
}

public interface IRepoBase<TKey, TEntity>
	where TEntity : IIdentifiableEntity<TKey>
{
	IQueryable<TEntity> Entities { get; }
	IIdentityProvider<TKey>? IdentityProvider { get; }
	Task<long> GetCountAsync();
	Task<TEntity> CreateAsync(TEntity entity);
	Task<TEntity?> GetAsync(TKey id);
	Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>>? filter = null, int? limit = null);
	Task<bool> UpdateAsync(TEntity entity);
	Task<TEntity> UpdateCreateAsync(TEntity entity);
	Task<bool> DeleteAsync(TKey id);
	Task<long> DeleteAsync(Expression<Func<TEntity, bool>> filter);
}