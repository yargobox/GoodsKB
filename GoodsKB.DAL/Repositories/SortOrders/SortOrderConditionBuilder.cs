namespace GoodsKB.DAL.Repositories.SortOrders;

using System.Linq.Expressions;

/// <summary>
/// Sort orders condition builder
/// </summary>
/// <remarks>
/// Provides information about defined sort orders (<c>SortOrderAttribute</c>)
/// for DTO properties and functionality for building and applying custom sort conditions
/// to an IQueryable instance based on them.
/// </remarks>
public static partial class SortOrderConditionBuilder
{
	public static IReadOnlyDictionary<string, SortOrderDesc> GetSortOrders<T, TDto>() where T : notnull where TDto : notnull
		=> DescHelper<T, TDto>.SortOrders;

	public static IQueryable<T> OrderBy<T>(this IQueryable<T> @this, SortOrderValues? values)
	{
		if (values == null || values.Values.Any() == false) return @this;

		var entityParameter = Expression.Parameter(typeof(T), "item");
		bool next = false;
		foreach (var orderValue in values.Values)
		{
			var fd = values.SortOrders[orderValue.PropertyName];

			if ((fd.Allowed & orderValue.Operation) != orderValue.Operation)
			{
				throw new InvalidOperationException($"Sort operation {orderValue.Operation.ToString()} or its options is not allowed on {typeof(T).Name}.{fd.PropertyName}.");
			}
			if ((orderValue.Operation & SOs.All) == 0)
			{
				throw new InvalidOperationException($"Invalid sort operation on {typeof(T).Name}.{orderValue.PropertyName}.");
			}

			var propMember = Expression.MakeMemberAccess(entityParameter, fd.PropertyInfo);
			var orderByExp = Expression.Lambda(propMember, entityParameter);
			var methodName = next ?
				(orderValue.Operation.HasFlag(SO.Ascending) ? "ThenBy" : "ThenByDescending") :
				(orderValue.Operation.HasFlag(SO.Ascending) ? "OrderBy" : "OrderByDescending");
			next = true;
			var predicate = Expression.Call(
				typeof(Queryable),
				methodName,
				new Type[] { typeof(T), fd.PropertyInfo.PropertyType },
				@this.Expression,
				Expression.Quote(orderByExp));

			@this = @this.Provider.CreateQuery<T>(predicate);
		}

		return @this;
	}

	public static OrderBy<T>? BuildCondition<T>(this SortOrderValues? values)
	{
		if (values == null) return null;

		OrderBy<T>? orderBy = null;
		foreach (var orderValue in values.Values)
		{
			var fd = values.SortOrders[orderValue.PropertyName];

			if ((fd.Allowed & orderValue.Operation) != orderValue.Operation)
			{
				throw new InvalidOperationException($"Sort operation {orderValue.Operation.ToString()} or its options is not allowed on {typeof(T).Name}.{fd.PropertyName}.");
			}
			if ((orderValue.Operation & SOs.All) == 0)
			{
				throw new InvalidOperationException($"Invalid sort operation on {typeof(T).Name}.{orderValue.PropertyName}.");
			}

			orderBy = orderBy != null ?
				(orderValue.Operation.HasFlag(SO.Ascending) ?
					orderBy.Asc(fd.PropertyInfo) :
					orderBy.Desc(fd.PropertyInfo)
				) :
				(orderValue.Operation.HasFlag(SO.Ascending) ?
					GoodsKB.DAL.Repositories.OrderBy<T>.Asc(fd.PropertyInfo) :
					GoodsKB.DAL.Repositories.OrderBy<T>.Desc(fd.PropertyInfo)
				);
		}

		return orderBy;
	}
}