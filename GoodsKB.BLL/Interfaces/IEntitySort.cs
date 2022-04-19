namespace GoodsKB.BLL.Services;

public interface IEntitySort<TEntity>
{
	IQueryable<TEntity> Apply(IQueryable<TEntity> query);
}