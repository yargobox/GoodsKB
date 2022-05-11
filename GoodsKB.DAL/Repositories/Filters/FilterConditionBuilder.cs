namespace GoodsKB.DAL.Repositories.Filters;

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using GoodsKB.DAL.Extensions.Linq;

/// <summary>
/// Filter conditions builder
/// </summary>
/// <remarks>
/// Provides information about defined filters (<c>FilterAttribute</c>, <c>GroupFilterAttribute</c>,
/// <c>FilterPartAttribute</c>) for DTO properties and functionality for building and applying
/// custom filtering conditions to an IQueryable instance based on them.
/// </remarks>
public static partial class FilterConditionBuilder
{
	public static IReadOnlyDictionary<string, FilterDesc> GetFilters<T, TDto>() where T : notnull where TDto : notnull
		=> DescHelper<T, TDto>.Filters;

	public static IQueryable<T> Where<T>(this IQueryable<T> @this, FilterValues? values)
	{
		if (values == null) return @this;

		var cond = values.BuildCondition<T>();
		@this = @this.Where(cond);

		return @this;
	}

	[return: NotNullIfNotNull("values")]
	public static Expression<Func<T, bool>>? BuildCondition<T>(this FilterValues? values)
	{
		return values != null ?
			Expression.Lambda<Func<T, bool>>(
				BuildConditionPredicate<T>(values),
				FilteringPredicate<T>.EntityParameter) :
			null;
	}

	public static Expression BuildConditionPredicate<T>(FilterValues values)
	{
		int total = 0;
		var predicates = new Expression[values.Values.Count()];

		foreach (var filterValue in values.Values.Where(x => x.PropertyName.Equals("id", StringComparison.OrdinalIgnoreCase))
						.Then(values.Values.Where(x => !x.PropertyName.Equals("id", StringComparison.OrdinalIgnoreCase))))
		{
			var fd = values.Filters[filterValue.PropertyName];
			var operandsInfo = FilteringPredicate<T>.GetOperandsInfo(filterValue.Operation);

			if ((fd.Allowed & filterValue.Operation) != filterValue.Operation)
			{
				throw new InvalidOperationException($"Filter operation {filterValue.Operation.ToString()} or its options is not allowed on {typeof(T).Name}.{fd.PropertyName}.");
			}

			if (operandsInfo.operandCount == 1)
			{
				if (filterValue.Value != null || filterValue.Value2 != null)
					throw new ArgumentException("filterValue.Value");

				var buildMethod = typeof(FilteringPredicate<>).MakeGenericType(typeof(T))
					.GetMethod("Build", BindingFlags.Static | BindingFlags.Public, new Type[] { typeof(FO), typeof(Type), typeof(string) }) ??
					throw new InvalidCastException("Invalid filter arguments specified.");

				if (fd.IsGroupFilter)
				{
					var groupFilterPredicates =
						fd.GroupParts!
						.Select(x => (
							joinByAnd: x.JoinByAnd,
							predicate: (Expression)buildMethod.Invoke(null, new object[] { filterValue.Operation, fd.OperandType, x.Name })!
						))
						.ToArray();
					predicates[total++] = JoinPredicates(groupFilterPredicates);
				}
				else
				{
					predicates[total++] = (Expression)buildMethod.Invoke(null, new object[] { filterValue.Operation, fd.OperandType, fd.PropertyName })!;
				}
			}
			else if (operandsInfo.operandCount == 2)
			{
				if (filterValue.Value2 != null)
				{
					throw new ArgumentException("filterValue.Value2");
				}

				if (filterValue.Value == null)
				{
					if (operandsInfo.isManyRightOperands || !fd.IsNullAllowed)
						throw new ArgumentNullException("filterValue.Value");
				}
				else if (operandsInfo.isManyRightOperands)
				{
					if (filterValue.Value is not IEnumerable)
						throw new ArgumentException("filterValue.Value");
				}
				else if (!fd.OperandType.IsAssignableFrom(filterValue.Value!.GetType()))
				{
					throw new ArgumentException("filterValue.Value");
				}

				var buildMethod = typeof(FilteringPredicate<>).MakeGenericType(typeof(T))
					.GetMethod("Build", BindingFlags.Static | BindingFlags.Public, new Type[] { typeof(FO), typeof(Type), typeof(string), fd.OperandType }) ??
					throw new MissingMethodException("An appropriate method to build the filter is missing.");

				if (fd.IsGroupFilter)
				{
					var groupFilterPredicates =
						fd.GroupParts!
						.Select(x => (
							joinByAnd: x.JoinByAnd,
							predicate: (Expression)buildMethod.Invoke(null, new object?[] { filterValue.Operation, fd.OperandType, x.Name, filterValue.Value })!
						))
						.ToArray();
					predicates[total++] = JoinPredicates(groupFilterPredicates);
				}
				else
				{
					predicates[total++] = (Expression)buildMethod.Invoke(null, new object?[] { filterValue.Operation, fd.OperandType, fd.PropertyName, filterValue.Value })!;
				}
			}
			else if (operandsInfo.operandCount == 3)
			{
				if (filterValue.Value == null)
				{
					if (!fd.IsNullAllowed)
						throw new ArgumentNullException("filterValue.Value");
				}
				else if (!fd.OperandType.IsAssignableFrom(filterValue.Value!.GetType()))
				{
					throw new ArgumentException("filterValue.Value");
				}

				if (filterValue.Value2 == null)
				{
					if (!fd.IsNullAllowed)
						throw new ArgumentNullException("filterValue.Value2");
				}
				else if (!fd.OperandType.IsAssignableFrom(filterValue.Value2!.GetType()))
				{
					throw new ArgumentException("filterValue.Value2");
				}

				var buildMethod = typeof(FilteringPredicate<>).MakeGenericType(typeof(T))
					.GetMethod("Build", BindingFlags.Static | BindingFlags.Public, new Type[] { typeof(FO), typeof(Type), typeof(string), fd.OperandType, fd.OperandType }) ??
					throw new MissingMethodException("An appropriate method to build the filter is missing.");

				if (fd.IsGroupFilter)
				{
					var groupFilterPredicates =
						fd.GroupParts!
						.Select(x => (
							joinByAnd: x.JoinByAnd,
							predicate: (Expression)buildMethod.Invoke(null, new object?[] { filterValue.Operation, fd.OperandType, x.Name, filterValue.Value, filterValue.Value2 })!
						))
						.ToArray();
					predicates[total++] = JoinPredicates(groupFilterPredicates);
				}
				else
				{
					predicates[total++] = (Expression)buildMethod.Invoke(null, new object?[] { filterValue.Operation, fd.OperandType, fd.PropertyName, filterValue.Value, filterValue.Value2 })!;
				}
			}
		}

		if (total == 0) return Expression.Constant(true, typeof(bool));
		var p = predicates[0];
		for (int i = 1; i < total; i++) p = Expression.AndAlso(p, predicates[i]);
		return p;
	}

	static Expression JoinPredicates((bool joinByAnd, Expression predicate)[] predicates)
	{
		if (predicates.Length == 0)
			return Expression.Constant(true, typeof(bool));
		else if (predicates.Length == 1)
			return predicates[0].predicate;

		var byand = predicates.Where(x => x.joinByAnd).Select(x => x.predicate).ToArray();
		var byor = predicates.Where(x => !x.joinByAnd).Select(x => x.predicate).ToArray();

		if (byand.Length == 0)
		{
			var p = byor[0];
			for (int i = 1; i < byor.Length; i++) p = Expression.OrElse(p, byor[i]);
			return p;
		}
		else if (byand.Length == 1)
		{
			var p = byor[0];
			for (int i = 1; i < byor.Length; i++) p = Expression.OrElse(p, byor[i]);
			return Expression.AndAlso(byand[0], p);
		}
		else if (byor.Length == 0)
		{
			var p = byand[0];
			for (int i = 1; i < byand.Length; i++) p = Expression.AndAlso(p, byand[i]);
			return p;
		}
		else if (byor.Length == 1)
		{
			var p = byand[0];
			for (int i = 1; i < byand.Length; i++) p = Expression.AndAlso(p, byand[i]);
			return Expression.AndAlso(p, byor[0]);
		}
		else
		{
			var pand = byand[0];
			for (int i = 1; i < byand.Length; i++) pand = Expression.AndAlso(pand, byand[i]);
			var por = byor[0];
			for (int i = 1; i < byor.Length; i++) por = Expression.OrElse(por, byor[i]);
			return Expression.AndAlso(pand, por);
		}
	}
}