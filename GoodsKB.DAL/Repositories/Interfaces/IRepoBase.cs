using System.Linq.Expressions;
using GoodsKB.DAL.Repositories.Filters;
using GoodsKB.DAL.Repositories.SortOrders;

namespace GoodsKB.DAL.Repositories;

public interface IIdentifiableEntity<TKey>
{
	TKey Id { get; set; }
}

public interface IRepoBase<TKey, TEntity>
	where TEntity : class, IIdentifiableEntity<TKey>
{
	IQueryable<TEntity> Entities { get; }
	IIdentityProvider<TKey>? IdentityProvider { get; }
	IFilterConditionBuilder FilterCondition { get; }
	ISortOrderConditionBuilder SortOrderCondition { get; }
	Task<long> GetCountAsync();
	Task<TEntity> CreateAsync(TEntity entity);
	Task<TEntity?> GetAsync(TKey id);
	Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>>? filter = null, int? limit = null);
	Task<bool> UpdateAsync(TEntity entity);
	Task<TEntity> UpdateCreateAsync(TEntity entity);
	Task<bool> DeleteAsync(TKey id);
	Task<long> DeleteAsync(Expression<Func<TEntity, bool>> filter);
}