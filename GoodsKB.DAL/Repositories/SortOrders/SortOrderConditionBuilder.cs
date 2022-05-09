namespace GoodsKB.DAL.Repositories.SortOrders;
using System.Linq.Expressions;

internal sealed partial class SortOrderConditionBuilder<TEntity> : ISortOrderConditionBuilder where TEntity : notnull
{
	public IReadOnlyDictionary<string, SortOrderDesc> GetSortOrders<TDto>()  where TDto : notnull => DescHelper<TDto>.SortOrders;

	public T Apply<T>(T queryable, SortOrderValues? values) where T : IQueryable
	{
		if (values == null || values.Values.Any() == false) return queryable;

		var q = queryable as IQueryable<TEntity>;
		if (q == null)
			throw new ArgumentException(nameof(queryable));

		var entityParameter = Expression.Parameter(typeof(TEntity), "item");
		bool next = false;
		foreach (var orderValue in values.Values)
		{
			var fd = values.SortOrders[orderValue.PropertyName];

			if ((fd.Allowed & orderValue.Operation) != orderValue.Operation)
			{
				throw new InvalidOperationException($"Sort operation {orderValue.Operation.ToString()} or its options is not allowed on {typeof(TEntity).Name}.{fd.PropertyName}.");
			}
			if ((orderValue.Operation & SOs.All) == 0)
			{
				throw new InvalidOperationException($"Invalid sort operation on {typeof(TEntity).Name}.{orderValue.PropertyName}.");
			}

			var propInfo = typeof(TEntity).GetProperty(orderValue.PropertyName)
				?? throw new InvalidOperationException($"{typeof(TEntity).Name} does not have a property {orderValue.PropertyName}.");
			var propMember = Expression.MakeMemberAccess(entityParameter, propInfo);
			var orderByExp = Expression.Lambda(propMember, entityParameter);
			var methodName = next ?
				(orderValue.Operation.HasFlag(SO.Ascending) ? "ThenBy" : "ThenByDescending") :
				(orderValue.Operation.HasFlag(SO.Ascending) ? "OrderBy" : "OrderByDescending");
			next = true;
			var predicate = Expression.Call(
				typeof(Queryable),
				methodName,
				new Type[] { typeof(TEntity), propInfo.PropertyType },
				q.Expression,
				Expression.Quote(orderByExp));

			q = q.Provider.CreateQuery<TEntity>(predicate);
		}

		return (T)q;
	}
}