using MongoDB.Driver;

namespace GoodsKB.DAL.Repositories;

public interface IIdentifiableEntity<TKey>
{
	TKey Id { get; set; }
}

public interface ISoftDelEntity<TDateTime> where TDateTime : struct
{
	TDateTime? Deleted { get; set; }
}

public interface IBaseRepo<TKey, TEntity> where TEntity : IIdentifiableEntity<TKey>
{
	Task<TKey> CreateAsync(TEntity entity);
	Task<TEntity?> GetAsync(TKey id);
	Task<IEnumerable<TEntity>> GetAsync();
	Task<IEnumerable<TEntity>> GetAsync(FilterDefinition<TEntity> filter);
	Task UpdateAsync(TEntity entity);
	Task DeleteAsync(TKey id);

	FilterDefinitionBuilder<TEntity> Filter { get; }
}