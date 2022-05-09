namespace GoodsKB.DAL.Repositories.SortOrders;

/// <summary>
/// Sort orders condition builder
/// </summary>
/// <remarks>
/// Provides information about defined sort orders (<c>SortOrderAttribute</c>)
/// for DTO properties and functionality for building and applying custom sort conditions
/// to an IQueryable instance based on them.
/// </remarks>
public interface ISortOrderConditionBuilder
{
	IReadOnlyDictionary<string, SortOrderDesc> GetSortOrders<TDto>() where TDto : notnull;

	T Apply<T>(T queryable, SortOrderValues? values) where T : IQueryable;
}