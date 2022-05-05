using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using GoodsKB.DAL.Extensions.Linq;
using GoodsKB.DAL.Extensions.Reflection;

namespace GoodsKB.DAL.Repositories.Filters;

internal class FilterConditionBuilder<TEntity> : IFilterConditionBuilder
	where TEntity : class
{
	public ReadOnlyDictionary<string, FilterDesc> GetFilters<TDto>() => FilterDescHelper<TDto>.Filters;

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
						fd.Parts!
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
						fd.Parts!
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
						fd.Parts!
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
		public static ReadOnlyDictionary<string, FilterDesc> Filters { get; }

		static FilterDescHelper()
		{
			var result = new Dictionary<string, FilterDesc>(StringComparer.OrdinalIgnoreCase);
			var props = typeof(TDto)
				.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty)
				.Where(x => Attribute.IsDefined(x, typeof(FilterAttribute), false) || Attribute.IsDefined(x, typeof(GroupFilterAttribute), false))
				.Select(x => (
					dtoProp: x,
					entityProp: typeof(TEntity).GetProperty(x.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty) ??
						throw new InvalidOperationException($"{typeof(TEntity).Name} does not have the {x.Name} property."),
					filterAttr: x.GetCustomAttribute<FilterAttribute>(false) ??
						throw new InvalidOperationException($"{typeof(GroupFilterAttribute).Name} cannot be used without {typeof(FilterAttribute).Name} for {typeof(TEntity).Name}.{x.Name}."),
					groupAttrs: x.GetCustomAttributes<GroupFilterAttribute>(false)))
				.ToArray();

			#region var groupFilterPartNames = new Dictionary<string, string[]>(StringComparer.InvariantCultureIgnoreCase);
			var groupFilterPartNames = new Dictionary<string, string[]>(StringComparer.InvariantCultureIgnoreCase);
			foreach (var p in props)
			{
				if (!string.IsNullOrWhiteSpace(p.filterAttr?.GroupFilter))
				{
					var groupFilterName = p.filterAttr.GroupFilter;
					if (!groupFilterPartNames.ContainsKey(groupFilterName))
					{
						var partNames = props
							.Select(x => (
								propName: x.entityProp.Name,
								filterIsGroupFilter: groupFilterName.Equals(x.filterAttr.GroupFilter, StringComparison.InvariantCultureIgnoreCase),
								filterAttr: x.filterAttr,
								groupAttr: x.groupAttrs.SingleOrDefault(g => groupFilterName.Equals(g.GroupFilter, StringComparison.InvariantCultureIgnoreCase))
							))
							.Where(x => x.filterIsGroupFilter || x.groupAttr != null)
							.Select(x => (
								propName: x.propName,
								conditionOrder: x.filterIsGroupFilter ? x.filterAttr.ConditionOrder : x.groupAttr!.ConditionOrder
							))
							.OrderBy(x => x.conditionOrder)
							.Select(x => x.propName)
							.ToArray();
						
						var redecl = partNames.GroupBy(x => x, StringComparer.InvariantCultureIgnoreCase).FirstOrDefault(x => x.Count() > 1);
						if (redecl != null)
						{
							throw new InvalidOperationException($"The same group filter is declared more than once for {typeof(TEntity).Name}.{redecl}.");
						}

						groupFilterPartNames.Add(groupFilterName, partNames);
					}
				}
				foreach (var group in p.groupAttrs)
				{
					var groupFilterName = group.GroupFilter;
					if (!groupFilterPartNames.ContainsKey(groupFilterName))
					{
						var partNames = props
							.Select(x => (
								propName: x.entityProp.Name,
								filterIsGroupFilter: groupFilterName.Equals(x.filterAttr.GroupFilter, StringComparison.InvariantCultureIgnoreCase),
								filterAttr: x.filterAttr,
								groupAttr: x.groupAttrs.SingleOrDefault(g => groupFilterName.Equals(g.GroupFilter, StringComparison.InvariantCultureIgnoreCase))
							))
							.Where(x => x.filterIsGroupFilter || x.groupAttr != null)
							.Select(x => (
								propName: x.propName,
								conditionOrder: x.filterIsGroupFilter ? x.filterAttr.ConditionOrder : x.groupAttr!.ConditionOrder
							))
							.OrderBy(x => x.conditionOrder)
							.Select(x => x.propName)
							.ToArray();
						
						var redecl = partNames.GroupBy(x => x, StringComparer.InvariantCultureIgnoreCase).FirstOrDefault(x => x.Count() > 1);
						if (redecl != null)
						{
							throw new InvalidOperationException($"The same group filter is declared more than once for {typeof(TEntity).Name}.{redecl}.");
						}

						groupFilterPartNames.Add(groupFilterName, partNames);
					}
				}
			}
			#endregion

			#region foreach is making FilterDesc and adding it to the result
			foreach (var p in props)
			{
				var propType = p.entityProp.PropertyType;

				if (!propType.IsAssignableFrom(p.dtoProp.PropertyType))
				{
					throw new InvalidOperationException($"{typeof(TEntity).Name}.{p.entityProp.Name} is not compatible with {typeof(TDto).Name}.{p.entityProp.Name}.");
				}

				var initials = p.filterAttr.GetInitialValues();
				var isNullAllowed = p.entityProp.IsNullable() && p.dtoProp.IsNullable();
				var underType = p.entityProp.GetUnderlyingSystemType();

				if (initials.isNullAllowed != null)
				{
					// The user can require a non-null filter value for nullable fields, but not vice versa.
					isNullAllowed = isNullAllowed && (bool)initials.isNullAllowed;
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
					allowed = (FO)initials.allowed;
					if (!isNullAllowed)
					{
						// The user is not allowed to perform nullable operations on non-nullable fields.
						allowed &= ~FO.TrueWhenNull;
					}
				}

				if ((allowed & FOs.All) == 0)
				{
					throw new InvalidOperationException($"No filter operation found for the {typeof(TDto).Name}.{p.dtoProp.Name} property.");
				}

				FO defop;
				if (initials.defaultOperation == null)
				{
					defop = FOs.GetDefault(underType) & allowed;
				}
				else
				{
					defop = (FO)initials.defaultOperation & allowed;
				}
				if (defop == FO.None)
				{
					defop = FOs.GetDefault(allowed);
				}

				bool isEmptyToNull;
				if (initials.isEmptyToNull == null)
				{
					isEmptyToNull = isNullAllowed && underType == typeof(string);
				}
				else
				{
					isEmptyToNull = isNullAllowed && (bool)initials.isEmptyToNull;
				}

				var filterDesc = new FilterDesc(p.entityProp.Name, propType, underType)
				{
					Allowed = allowed,
					IsNullAllowed = isNullAllowed,
					IsEmptyToNull = isEmptyToNull,
					Default = defop,
					Position = p.filterAttr.Position,
					Enabled = string.IsNullOrEmpty(p.filterAttr.GroupFilter),
					Visible = string.IsNullOrEmpty(p.filterAttr.GroupFilter) &&
						(initials.visible ?? groupFilterPartNames.SelectMany(x => x.Value).Any(x => x == p.entityProp.Name))
				};
				result.Add(p.entityProp.Name, filterDesc);
			}
			#endregion

			#region foreach is making FilterDesc for group filters and adding it to the result
			foreach (var p in groupFilterPartNames)
			{
				var isNullAllowed = true;
				var isEmptyToNull = true;
				var allowed = FOs.All | FOs.Flags;
				var defop = FOs.All | FOs.Flags;
				Type? underType = null;
				foreach (var filter in p.Value.Select(x => result[x]))
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
						throw new InvalidOperationException($"Could not infer a data type of the {p.Key} group filter.");
					}
				}

				if ((allowed & FOs.All) == 0)
				{
					throw new InvalidOperationException($"No allowed operations for the {p.Key} group filter.");
				}
				if ((defop & FOs.All) == 0)
				{
					defop = FOs.GetDefault(allowed);
				}

				Type propType = isNullAllowed ?
					result[p.Value[0]].OperandType :
					p.Value.Select(x => result[x]).First(x => !x.IsNullAllowed).OperandType;

				bool? visible;
				int position;
				var prop = props.First(x => x.entityProp.Name == p.Value[0]);
				if (p.Key.Equals(prop.filterAttr.GroupFilter, StringComparison.InvariantCultureIgnoreCase))
				{
					visible = prop.filterAttr.GetInitialValues().visible;
					position = prop.filterAttr.Position;
				}
				else
				{
					var groupAttr = prop.groupAttrs.First(x => p.Key.Equals(x.GroupFilter, StringComparison.InvariantCultureIgnoreCase));
					visible = groupAttr.Visible;
					position = groupAttr.Position;
				}

				var parts =
					p.Value
					.Select(x => props.First(a => a.entityProp.Name == x))
					.Select(x => new FilterDesc.GroupFilterPartDesc(
						x.entityProp.Name,
						p.Key.Equals(x.filterAttr.GroupFilter, StringComparison.InvariantCultureIgnoreCase) ?
							x.filterAttr.JoinByAnd :
							x.groupAttrs.First(a => p.Key.Equals(a.GroupFilter, StringComparison.InvariantCultureIgnoreCase)).JoinByAnd
					))
					.ToArray();

				var filterDesc = new FilterDesc(p.Key, propType, underType!)
				{
					IsNullAllowed = isNullAllowed,
					IsEmptyToNull = isEmptyToNull,
					Default = defop,
					Allowed = allowed,
					Position = position,
					Enabled = true,
					Visible = visible ?? true,
					Parts = parts
				};
				result.Add(p.Key, filterDesc);
			}
			#endregion

			Filters = new ReadOnlyDictionary<string, FilterDesc>(
				new SortedDictionary<string, FilterDesc>(
					result.OrderBy(x => x.Value.Position).ToDictionary(x => x.Key, x => x.Value),
					StringComparer.InvariantCultureIgnoreCase));
		}
	}
}