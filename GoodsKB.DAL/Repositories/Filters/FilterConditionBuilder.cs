using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using GoodsKB.DAL.Extensions.Linq;
using GoodsKB.DAL.Extensions.Reflection;
using GoodsKB.DAL.Generic;

namespace GoodsKB.DAL.Repositories.Filters;

internal class FilterConditionBuilder<TEntity> : IFilterConditionBuilder
	where TEntity : class
{
	public IReadOnlyDictionary<string, FilterDesc> GetFilters<TDto>() => FilterDescHelper<TDto>.Filters;

	[return: NotNullIfNotNull("values")]
	public Expression? BuildCondition(FilterValues? values)
	{
		return values != null ? Expression.Lambda<Func<TEntity, bool>>(BuildConditionPredicate(values), PropertyPredicate<TEntity>.EntityParameter) : null;
	}

	private static Expression BuildConditionPredicate(FilterValues values)
	{
		int total = 0;
		var predicates = new Expression[values.Values.Count()];

		foreach (var item in values.Values.Where(x => x.PropertyName.Equals("id", StringComparison.OrdinalIgnoreCase))
						.Then(values.Values.Where(x => !x.PropertyName.Equals("id", StringComparison.OrdinalIgnoreCase))))
		{
			var fd = values.Filters[item.PropertyName];
			var operandsInfo = PropertyPredicate<TEntity>.GetOperandsInfo(item.Operation);

			if ((fd.Allowed & item.Operation) != item.Operation)
			{
				throw new InvalidOperationException(@$"Filter operation ""{item.Operation.ToString()}"" or its options is not allowed on field ""{fd.PropertyName}"".");
			}

			if (operandsInfo.operandCount == 1)
			{
				if (item.Value != null || item.Value2 != null)
					throw new ArgumentException("item.Value");

				var buildMethod = typeof(PropertyPredicate<>).MakeGenericType(typeof(TEntity))
					.GetMethod("Build", BindingFlags.Static | BindingFlags.Public, new Type[] { typeof(FO), typeof(Type), typeof(string) }) ??
					throw new InvalidCastException("Invalid filter arguments specified.");

				if (fd.IsGroupFilter)
				{
					var groupFilterPredicates =
						fd.GroupParts!
						.Select(x => (
							joinByAnd: x.JoinByAnd,
							predicate: (Expression)buildMethod.Invoke(null, new object[] { item.Operation, fd.OperandType, x.Name })!
						))
						.ToArray();
					predicates[total++] = JoinPredicates(groupFilterPredicates);
				}
				else
				{
					predicates[total++] = (Expression)buildMethod.Invoke(null, new object[] { item.Operation, fd.OperandType, fd.PropertyName })!;
				}
			}
			else if (operandsInfo.operandCount == 2)
			{
				if (item.Value2 != null)
				{
					throw new ArgumentException("item.Value2");
				}

				if (item.Value == null)
				{
					if (operandsInfo.isManyRightOperands || !fd.IsNullAllowed)
						throw new ArgumentNullException("item.Value");
				}
				else if (operandsInfo.isManyRightOperands)
				{
					if (item.Value is not IEnumerable)
						throw new ArgumentException("item.Value");
				}
				else if (!fd.OperandType.IsAssignableFrom(item.Value!.GetType()))
				{
					throw new ArgumentException("item.Value");
				}

				var buildMethod = typeof(PropertyPredicate<>).MakeGenericType(typeof(TEntity))
					.GetMethod("Build", BindingFlags.Static | BindingFlags.Public, new Type[] { typeof(FO), typeof(Type), typeof(string), fd.OperandType }) ??
					throw new MissingMethodException("An appropriate method to build the filter is missing.");

				if (fd.IsGroupFilter)
				{
					var groupFilterPredicates =
						fd.GroupParts!
						.Select(x => (
							joinByAnd: x.JoinByAnd,
							predicate: (Expression)buildMethod.Invoke(null, new object?[] { item.Operation, fd.OperandType, x.Name, item.Value })!
						))
						.ToArray();
					predicates[total++] = JoinPredicates(groupFilterPredicates);
				}
				else
				{
					predicates[total++] = (Expression)buildMethod.Invoke(null, new object?[] { item.Operation, fd.OperandType, fd.PropertyName, item.Value })!;
				}
			}
			else if (operandsInfo.operandCount == 3)
			{
				if (item.Value == null)
				{
					if (!fd.IsNullAllowed)
						throw new ArgumentNullException("item.Value");
				}
				else if (!fd.OperandType.IsAssignableFrom(item.Value!.GetType()))
				{
					throw new ArgumentException("item.Value");
				}

				if (item.Value2 == null)
				{
					if (!fd.IsNullAllowed)
						throw new ArgumentNullException("item.Value2");
				}
				else if (!fd.OperandType.IsAssignableFrom(item.Value2!.GetType()))
				{
					throw new ArgumentException("item.Value2");
				}

				var buildMethod = typeof(PropertyPredicate<>).MakeGenericType(typeof(TEntity))
					.GetMethod("Build", BindingFlags.Static | BindingFlags.Public, new Type[] { typeof(FO), typeof(Type), typeof(string), fd.OperandType, fd.OperandType }) ??
					throw new MissingMethodException("An appropriate method to build the filter is missing.");

				if (fd.IsGroupFilter)
				{
					var groupFilterPredicates =
						fd.GroupParts!
						.Select(x => (
							joinByAnd: x.JoinByAnd,
							predicate: (Expression)buildMethod.Invoke(null, new object?[] { item.Operation, fd.OperandType, x.Name, item.Value, item.Value2 })!
						))
						.ToArray();
					predicates[total++] = JoinPredicates(groupFilterPredicates);
				}
				else
				{
					predicates[total++] = (Expression)buildMethod.Invoke(null, new object?[] { item.Operation, fd.OperandType, fd.PropertyName, item.Value, item.Value2 })!;
				}
			}
		}

		if (total == 0) return Expression.Constant(true, typeof(bool));
		var p = predicates[0];
		for (int i = 1; i < total; i++) p = Expression.AndAlso(p, predicates[i]);
		return p;
	}

	private static Expression JoinPredicates((bool joinByAnd, Expression predicate)[] predicates)
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

	private static class FilterDescHelper<TDto>
	{
		public static IReadOnlyDictionary<string, FilterDesc> Filters { get; }
		private static readonly FilterAttribute _defaultFilterAttribute = new FilterAttribute();

		static FilterDescHelper()
		{
			var filters = new Dictionary<string, FilterDesc>(StringComparer.OrdinalIgnoreCase);
			var entries = typeof(TDto)
				.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty)
				.Select(x => (
					dtoProp: x,
					entityProp: typeof(TEntity).GetProperty(x.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty) ??
						throw new InvalidOperationException($"{typeof(TEntity).Name} does not have the {x.Name} property."),
					filterAttr: x.GetCustomAttributeOfExactType<FilterAttribute>(false),
					groupAttrs: x.GetCustomAttributesOfExactType<GroupFilterAttribute>(false),
					partAttrs: x.GetCustomAttributesOfExactType<FilterPartAttribute>(false)))
				.Where(x =>
					x.filterAttr != null ||
					x.groupAttrs.Any() ||
					x.partAttrs.Any()
				)
				.ToArray();

			// Make dictionary of group filter names and their part names defined by GroupFilterAttribute and FilterPartAttribute
			//
			var groupPartsByGroupFilters = new Dictionary<string, FilterDesc.GroupFilterPartDesc[]>(StringComparer.InvariantCultureIgnoreCase);
			foreach (var p in entries.Where(x => x.groupAttrs.Length > 0))
			{
				if (p.groupAttrs.Any(x => groupPartsByGroupFilters.ContainsKey(x.GroupFilter)))
				{
					throw new InvalidOperationException($"Duplicate group filter declaration for {typeof(TDto).Name}.{p.dtoProp.Name}.");
				}

				foreach (var g in p.groupAttrs)
				{
					var itself = new (FilterDesc.GroupFilterPartDesc partDesc, int conditionOrder)[]
					{
						( new FilterDesc.GroupFilterPartDesc(p.entityProp.Name, g.JoinByAnd), g.ConditionOrder )
					};

					var groupParts =
						itself.Then(
							entries.Select(x => (
								propName: x.entityProp.Name,
								partAttr: x.partAttrs.FirstOrDefault(t => g.GroupFilter.Equals(t.GroupFilter, StringComparison.InvariantCultureIgnoreCase))
							))
							.Where(x => x.partAttr != null)
							.Select(x => (
								partDesc: new FilterDesc.GroupFilterPartDesc(x.propName, x.partAttr!.JoinByAnd),
								conditionOrder: x.partAttr!.ConditionOrder
							))
						)
						.OrderBy(x => x.conditionOrder)
						.Select(x => x.partDesc)
						.ToArray();

					groupPartsByGroupFilters.Add(g.GroupFilter, groupParts);
				}
			}

			// Make filters defined by FilterAttribute
			//
			foreach (var p in entries.Where(x => x.filterAttr != null))
			{
				var filterDesc = MakeFilterDesc(p.entityProp, p.dtoProp, p.filterAttr!);
				filters.Add(p.entityProp.Name, filterDesc);
			}

			// Make filters for each part of each group filter
			//
			foreach (var p in groupPartsByGroupFilters
				.SelectMany(x => entries
					.Where(t => x.Value.Any(m => m.Name == t.entityProp.Name))
					.Select(t => (
						groupFilter: x.Key,
						dtoProp: t.dtoProp,
						entityProp: t.entityProp,
						groupAttr: entries
							.First(t => t.groupAttrs.Any(m => m.GroupFilter == x.Key))
							.groupAttrs
							.First(t => t.GroupFilter == x.Key)
				)))
			)
			{
				var filterDesc = MakeFilterDesc(p.entityProp, p.dtoProp, p.groupAttr);
				var filterPartName = MakeFilterPartName(p.groupFilter, p.entityProp.Name);
				filters.Add(filterPartName, filterDesc);
			}

			// Combine group filter parts to make a group filters from them that is defined by GroupFilterAttribute
			// Determine each filter property value as the strictest of all.
			//
			foreach (var p in groupPartsByGroupFilters)
			{
 				var isNullAllowed = true;
				var isEmptyToNull = true;
				var allowed = FOs.All | FOs.Flags;
				var defop = FOs.All | FOs.Flags;
				Type? underType = null;
				foreach (var filter in p.Value.Select(x => filters[MakeFilterPartName(p.Key, x.Name)]))
				{
					allowed &= filter.Allowed;
					defop &= filter.Default;
					isNullAllowed = isNullAllowed && filter.IsNullAllowed;
					isEmptyToNull = isEmptyToNull && filter.IsEmptyToNull;

					if (underType == null)
					{
						underType = filter.UnderlyingType;
					}
					else if (underType != filter.UnderlyingType)
					{
						throw new InvalidOperationException($"Could not infer group filter data type {p.Key}.");
					}
				}
				if ((allowed & FOs.All) == 0)
				{
					throw new InvalidOperationException($"No possible operations for a group filter {p.Key}.");
				}
				if ((defop & FOs.All) == 0)
				{
					defop = FOs.GetDefault(allowed);
				}

				var propType = p.Value
					.Select(x => filters[MakeFilterPartName(p.Key, x.Name)])
					.First(x => x.IsNullAllowed == isNullAllowed)
					.OperandType;
				var groupAttr = entries.First(x => x.groupAttrs.Any(t => t.GroupFilter == p.Key)).groupAttrs.First(x => x.GroupFilter == p.Key);
				var initials = groupAttr.GetInitialValues();

				var filterDesc = new FilterDesc(p.Key, propType, underType!)
				{
					IsNullAllowed = isNullAllowed,
					IsEmptyToNull = isEmptyToNull,
					Default = defop,
					Allowed = allowed,
					Enabled = true,
					Visible = initials.visible ?? true,
					GroupParts = p.Value
				};
				filters.Add(p.Key, filterDesc);
			}

			// Finally, get the position of each filter to provide an ordered collection of filters
			//
			var filterAttrByFilterNames = filters
				.Select(x => (
					filterName: x.Key,
					entry: entries.FirstOrDefault(t => t.entityProp.Name == x.Key)))
				.Where(x => x.entry != default)
				.Select(x => (
					filterName: x.filterName,
					filterAttr: (FilterAttribute)x.entry.filterAttr!
				))
				.ToArray();
			var groupAttrByFilterNames = filters
				.Select(x => (
					filterName: x.Key,
					entry: entries.FirstOrDefault(t => t.groupAttrs.Any(m => m.GroupFilter == x.Key))))
				.Where(x => x.entry != default)
				.Select(x => (
					filterName: x.filterName,
					filterAttr: (FilterAttribute)x.entry.groupAttrs.First(t => t.GroupFilter == x.filterName)
				))
				.ToArray();

			var filtersAndPositions = filters
				.Select(x => (
					filter: x,
					filterAttr: filterAttrByFilterNames.Then(groupAttrByFilterNames).FirstOrDefault(t => t.filterName == x.Key)
				))
				.Select(x => (
					filter: x.filter,
					position: x.filterAttr == default ? int.MaxValue : x.filterAttr.filterAttr.Position
				))
				.OrderBy(x => x.position)
				.Select(x => new KeyValuePair<string, FilterDesc>(x.filter.Key, x.filter.Value));

			Filters = new OrderedDictionary<string, FilterDesc>(filtersAndPositions, StringComparer.InvariantCultureIgnoreCase);
		}

		private static string MakeFilterPartName(string groupFilter, string entityProp) => string.Concat(groupFilter, ":", entityProp);

		private static FilterDesc MakeFilterDesc(PropertyInfo entityProp, PropertyInfo dtoProp, FilterAttribute filterAttr)
		{
			var propType = entityProp.PropertyType;

			if (!propType.IsAssignableFrom(dtoProp.PropertyType))
			{
				throw new InvalidOperationException($"{typeof(TEntity).Name}.{entityProp.Name} is not compatible with {typeof(TDto).Name}.{entityProp.Name}.");
			}

			var initials = filterAttr.GetInitialValues();
			var isNullAllowed = entityProp.IsNullable() && dtoProp.IsNullable();
			var underType = entityProp.GetUnderlyingSystemType();

			if (initials.isNullAllowed != null)
			{
				// The user can require a non-null filter value for nullable fields, but not vice versa.
				isNullAllowed = isNullAllowed && initials.isNullAllowed.Value;
			}

			FO allowed;
			if (initials.allowed == null)
			{
				if (underType.IsEnum)
				{
					allowed = FOs.Equality | FOs.Inclusion;
					if (underType.IsDefined(typeof(FlagsAttribute)))
					{
						allowed |= FOs.Bitwise;
					}
				}
				else
				{
					allowed = FOs.GetAllowed(underType);
				}

				if (isNullAllowed)
				{
					allowed |= FOs.Nullability | FO.TrueWhenNull;
				}
			}
			else
			{
				allowed = initials.allowed.Value;
				if (!isNullAllowed)
				{
					// The user is not allowed to perform nullable operations on non-nullable fields.
					allowed &= ~FO.TrueWhenNull;
				}
			}

			if ((allowed & FOs.All) == 0)
			{
				throw new InvalidOperationException($"No filter operation found for the {typeof(TDto).Name}.{dtoProp.Name} property.");
			}

			FO defop;
			if (initials.defaultOperation == null)
			{
				defop = FOs.GetDefault(underType) & allowed;
			}
			else
			{
				defop = initials.defaultOperation.Value & allowed;
			}
			if (defop == FO.None)
			{
				defop = FOs.GetDefault(allowed);
			}

			bool isEmptyToNull;
			if (initials.isEmptyToNull == null)
			{
				// By default, converts empty values to null only for string filters
				isEmptyToNull = isNullAllowed && underType == typeof(string);
			}
			else
			{
				isEmptyToNull = isNullAllowed && initials.isEmptyToNull.Value;
			}

			return new FilterDesc(entityProp.Name, propType, underType)
			{
				Allowed = allowed,
				IsNullAllowed = isNullAllowed,
				IsEmptyToNull = isEmptyToNull,
				Default = defop,
				Visible = initials.visible ?? true,
				Enabled = filterAttr is not GroupFilterAttribute
			};
		}
	}
}