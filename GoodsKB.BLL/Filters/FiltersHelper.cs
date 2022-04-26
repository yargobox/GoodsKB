using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using GoodsKB.BLL.Common;
using GoodsKB.DAL.Repositories;

namespace GoodsKB.BLL.Services;

public static class FiltersHelper<TEntity>
	where TEntity : class
{
	private static class DefinitionHelper<TDto>
	{
		public static ReadOnlyDictionary<string, FilterDefinition> Definitions { get; }

		static DefinitionHelper()
		{
			var filters = new Dictionary<string, FilterDefinition>();

			foreach (var p in typeof(TDto)
					// public non-static SET properties
					.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty)
					// with given attribute only
					.Where(x => Attribute.IsDefined(x, typeof(UserFilterAttribute), true))
					// return propSet and attr
					.Select(x => (
						entityProp: typeof(TEntity).GetProperty(x.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty) ??
							throw new InvalidOperationException(@$"""{typeof(TEntity).Name}"" does not have property ""{x.Name}""."),
						dtoProp: x,
						attr: x.GetCustomAttribute<UserFilterAttribute>(true)!)
					)
			)
			{
				var propType = p.entityProp.PropertyType;

				if (!propType.IsAssignableFrom(p.dtoProp.PropertyType))
				{
					throw new InvalidOperationException($"{typeof(TEntity).Name}.{p.entityProp.Name} is not compatible with {typeof(TDto).Name}.{p.entityProp.Name}.");
				}

				var initials = p.attr.GetInitialValues();
				bool isNullAllowed = p.entityProp.IsNullable() && p.dtoProp.IsNullable();
				Type underType = p.entityProp.GetUnderlyingSystemType();

				if (initials.isNullAllowed != null)
				{
					// The user can require a non-null filter value for nullable fields, but not vice versa.
					isNullAllowed = isNullAllowed && (bool)initials.isNullAllowed;
				}

				FilterOperations allowed;
				if (initials.allowed == null)
				{
					if (underType.IsEnum)
					{
						allowed = FO.Equality | FO.Inclusion;
						if (underType.IsDefined(typeof(FlagsAttribute)))
						{
							allowed |= FO.Bitwise;
						}
					}
					else
					{
						allowed = FO.GetAllowed(underType);
					}

					if (isNullAllowed)
					{
						allowed |= FO.Nullability | FilterOperations.TrueWhenNull;
					}
				}
				else
				{
					allowed = (FilterOperations)initials.allowed;
					if (!isNullAllowed)
					{
						// The user is not allowed to perform nullable operations on non-nullable fields.
						allowed &= ~FilterOperations.TrueWhenNull;
					}
				}

				if ((allowed & FO.All) == FilterOperations.None)
				{
					throw new InvalidOperationException($"No filter operation found for field {typeof(TDto).Name}.{p.dtoProp.Name}.");
				}

				FilterOperations defop;
				if (initials.defaultOperation == null)
				{
					defop = FO.GetDefault(underType) & allowed;
				}
				else
				{
					defop = (FilterOperations)initials.defaultOperation & allowed;
				}
				if (defop == FilterOperations.None)
				{
					defop = FO.GetDefault(allowed);
				}

				bool emptyToNull;
				if (initials.emptyToNull == null)
				{
					emptyToNull = isNullAllowed && underType == typeof(string);
				}
				else
				{
					emptyToNull = isNullAllowed && (bool)initials.emptyToNull;
				}

				var itemProperty = Expression.Parameter(p.entityProp.DeclaringType!, "item");
				var propMember = Expression.PropertyOrField(itemProperty, p.entityProp.Name);
				var memberSelector = Expression.Lambda(propMember, itemProperty);

				filters.Add(p.entityProp.Name,
					new FilterDefinition(p.entityProp.Name, propType, memberSelector)
					{
						AllowedOperations = allowed,
						IsNullAllowed = isNullAllowed,
						EmptyToNull = emptyToNull,
						DefaultOperation = defop
					});
			}

			Definitions = new ReadOnlyDictionary<string, FilterDefinition>(filters);
		}
	}

	public static FilterValues? SerializeFromString<TDto>(string? s)
	{
		throw new NotSupportedException();
		var definitions = DefinitionHelper<TDto>.Definitions;
		return new FilterValues(definitions, new FilterValue[] { new FilterValue("Id") { Operation = FilterOperations.Equal, Value = 2 } });
	}
	public static string? SerializeToString(FilterValues? items)
	{
		throw new NotSupportedException();
	}

	public static Expression BuildConditionPredicate(FilterValues values)
	{
		int total = 0;
		var predicates = new Expression[values.Values.Count()];

		foreach (var item in values.Values)
		{
			var def = values.Definitions[item.Name];
			var info = FieldPredicate<TEntity>.GetOperandsInfo(item.Operation);

			if ((def.AllowedOperations & item.Operation) != item.Operation)
				throw new InvalidOperationException(@$"Filter operation ""{item.Operation.ToString()}"" or its options is not allowed on field ""{def.Name}"".");

			if (info.operandCount == 1)
			{
				if (item.Value != null || item.Value2 != null)
					throw new ArgumentException("item.Value");

				var buildMethod = typeof(FieldPredicate<>).MakeGenericType(typeof(TEntity))
					.GetMethod("Build", BindingFlags.Static | BindingFlags.Public, new Type[] { typeof(FilterOperations), typeof(Type), typeof(string) }) ??
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

				var buildMethod = typeof(FieldPredicate<>).MakeGenericType(typeof(TEntity))
					.GetMethod("Build", BindingFlags.Static | BindingFlags.Public, new Type[] { typeof(FilterOperations), typeof(Type), typeof(string), def.OperandType }) ??
					throw new InvalidCastException("Invalid filter arguments specified.");

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

				var buildMethod = typeof(FieldPredicate<>).MakeGenericType(typeof(TEntity))
					.GetMethod("Build", BindingFlags.Static | BindingFlags.Public, new Type[] { typeof(FilterOperations), typeof(Type), typeof(string), def.OperandType, def.OperandType }) ??
					throw new InvalidCastException("Invalid filter arguments specified.");

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
	
	[return: NotNullIfNotNull("values")]
	public static Expression<Func<TEntity, bool>>? BuildCondition(FilterValues? values)
	{
		return values != null ?
			Expression.Lambda<Func<TEntity, bool>>(BuildConditionPredicate(values), FieldPredicate<TEntity>.EntityParameter) :
			null;
	}
}