using MongoDB.Driver;

namespace GoodsKB.DAL.Repositories;

public interface IEntityId<TKey>
{
	TKey Id { get; set; }
}

public interface IBaseRepo<TKey, TEntity> where TEntity : IEntityId<TKey>
{
	Task<TKey> CreateAsync(TEntity entity);
	Task<TEntity?> GetAsync(TKey id);
	Task<IEnumerable<TEntity>> GetAsync();
	Task<IEnumerable<TEntity>> GetAsync(FilterDefinition<TEntity> filter);
	Task UpdateAsync(TEntity entity);
	Task DeleteAsync(TKey id);

	FilterDefinitionBuilder<TEntity> FilterBuilder { get; }
}