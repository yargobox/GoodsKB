using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace GoodsKB.DAL.Linq;

public static class Extensions
{
	public static async Task<List<T>> ToListAsync<T>(this IQueryable<T> queryable)
	{
		if (queryable is IMongoQueryable<T> mongoQueryable)
			return await IAsyncCursorSourceExtensions.ToListAsync(mongoQueryable);
		else
			return await Task.FromResult(queryable.ToList());
	}
}