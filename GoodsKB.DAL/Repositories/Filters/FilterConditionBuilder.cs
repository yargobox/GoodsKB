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
		return values == null ? null :
			Expression.Lambda<Func<TEntity, bool>>(BuildConditionPredicate(values), PropertyPredicate<TEntity>.EntityParameter);
	}

	private static Expression BuildConditionPredicate(FilterValues values)
	{
		int total = 0;
		var predicates = new Expression[values.Values.Count()];

		foreach (var item in values.Values.Where(x => x.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
						.Then(values.Values.Where(x => !x.Name.Equals("id", StringComparison.OrdinalIgnoreCase))))
		{
			var def = values.Filters[item.Name];
			var info = PropertyPredicate<TEntity>.GetOperandsInfo(item.Operation);

			if ((def.Allowed & item.Operation) != item.Operation)
			{
				throw new InvalidOperationException(@$"Filter operation ""{item.Operation.ToString()}"" or its options is not allowed on field ""{def.Name}"".");
			}

			if (info.operandCount == 1)
			{
				if (item.Value != null || item.Value2 != null)
					throw new ArgumentException("item.Value");

				var buildMethod = typeof(PropertyPredicate<>).MakeGenericType(typeof(TEntity))
					.GetMethod("Build", BindingFlags.Static | BindingFlags.Public, new Type[] { typeof(FO), typeof(Type), typeof(string) }) ??
					throw new InvalidCastException("Invalid filter arguments specified.");

				predicates[total++] = (Expression)buildMethod.Invoke(null, new object[] { item.Operation, def.OperandType, def.Name })!;
			}
			else if (info.operandCount == 2)
			{
				if (item.Value2 != null)
				{
					throw new ArgumentException("item.Value2");
				}

				if (item.Value == null)
				{
					if (info.isManyRightOperands || !def.IsNullAllowed)
						throw new ArgumentNullException("item.Value");
				}
				else if (info.isManyRightOperands)
				{
					if (item.Value is not IEnumerable)
						throw new ArgumentException("item.Value");
				}
				else if (!def.OperandType.IsAssignableFrom(item.Value!.GetType()))
				{
					throw new ArgumentException("item.Value");
				}

				var buildMethod = typeof(PropertyPredicate<>).MakeGenericType(typeof(TEntity))
					.GetMethod("Build", BindingFlags.Static | BindingFlags.Public, new Type[] { typeof(FO), typeof(Type), typeof(string), def.OperandType }) ??
					throw new MissingMethodException("An appropriate method to build the filter is missing.");

				predicates[total++] = (Expression)buildMethod.Invoke(null, new object?[] { item.Operation, def.OperandType, def.Name, item.Value })!;
			}
			else if (info.operandCount == 3)
			{
				if (item.Value == null)
				{
					if (!def.IsNullAllowed)
						throw new ArgumentNullException("item.Value");
				}
				else if (!def.OperandType.IsAssignableFrom(item.Value!.GetType()))
				{
					throw new ArgumentException("item.Value");
				}

				if (item.Value2 == null)
				{
					if (!def.IsNullAllowed)
						throw new ArgumentNullException("item.Value2");
				}
				else if (!def.OperandType.IsAssignableFrom(item.Value2!.GetType()))
				{
					throw new ArgumentException("item.Value2");
				}

				var buildMethod = typeof(PropertyPredicate<>).MakeGenericType(typeof(TEntity))
					.GetMethod("Build", BindingFlags.Static | BindingFlags.Public, new Type[] { typeof(FO), typeof(Type), typeof(string), def.OperandType, def.OperandType }) ??
					throw new MissingMethodException("An appropriate method to build the filter is missing.");

				predicates[total++] = (Expression)buildMethod.Invoke(null, new object?[] { item.Operation, def.OperandType, def.Name, item.Value, item.Value2 })!;
			}
		}

		if (total == 0)
			return Expression.Constant(true, typeof(bool));
		else if (total == 1)
			return predicates[0];

		var predicate = Expression.AndAlso(predicates[0], predicates[1]);
		for (int j = 2; j < total; j++)
		{
			predicate = Expression.AndAlso(predicate, predicates[j]);
		}

		return predicate;
	}

	private static class FilterDescHelper<TDto>
	{
		public static ReadOnlyDictionary<string, FilterDesc> Filters { get; }

		static FilterDescHelper()
		{
			var filters = new Dictionary<string, FilterDesc>(StringComparer.OrdinalIgnoreCase);

			foreach (var p in typeof(TDto)
					// public non-static SET properties
					.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty)
					// with given attribute only
					.Where(x => Attribute.IsDefined(x, typeof(FilterAttribute), true))
					// return propSet and attr
					.Select(x => (
						entityProp: typeof(TEntity).GetProperty(x.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty) ??
							throw new InvalidOperationException(@$"""{typeof(TEntity).Name}"" does not have property ""{x.Name}""."),
						dtoProp: x,
						attr: x.GetCustomAttribute<FilterAttribute>(true)!)
					)
			)
			{
				var propType = p.entityProp.PropertyType;

				if (!propType.IsAssignableFrom(p.dtoProp.PropertyType))
				{
					throw new InvalidOperationException($"{typeof(TEntity).Name}.{p.entityProp.Name} is not compatible with {typeof(TDto).Name}.{p.entityProp.Name}.");
				}

				var initials = p.attr.GetValues();
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

				if ((allowed & FOs.All) == FO.None)
				{
					throw new InvalidOperationException($"No filter operation found for field {typeof(TDto).Name}.{p.dtoProp.Name}.");
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

				var itemProperty = Expression.Parameter(p.entityProp.DeclaringType!, "item");
				var propMember = Expression.PropertyOrField(itemProperty, p.entityProp.Name);
				var memberSelector = Expression.Lambda(propMember, itemProperty);

				filters.Add(p.entityProp.Name,
					new FilterDesc(p.entityProp.Name, propType, underType, memberSelector)
					{
						Allowed = allowed,
						IsNullAllowed = isNullAllowed,
						IsEmptyToNull = isEmptyToNull,
						Default = defop
					});
			}

			Filters = new ReadOnlyDictionary<string, FilterDesc>(filters);
		}
	}
}