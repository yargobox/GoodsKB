using System.Linq.Expressions;

namespace GoodsKB.DAL.Repositories;

public interface IIdentifiableEntity<TKey>
	where TKey : struct
{
	TKey Id { get; set; }
}

public interface IRepoBase<TKey, TEntity>
	where TKey : struct
	where TEntity : IIdentifiableEntity<TKey>
{
	IQueryable<TEntity> Entities { get; }
	RepositoryIdentityTypes IdentityType { get; }
	RepositoryIdentityProviders IdentityProvider { get; }
	Task<TEntity> CreateAsync(TEntity entity);
	Task<TEntity?> GetAsync(TKey id);
	Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>>? filter = null, int? limit = null);
	Task<bool> UpdateAsync(TEntity entity);
	Task<TEntity> UpdateCreateAsync(TEntity entity);
	Task<bool> DeleteAsync(TKey id);
	Task<long> DeleteAsync(Expression<Func<TEntity, bool>> filter);
}