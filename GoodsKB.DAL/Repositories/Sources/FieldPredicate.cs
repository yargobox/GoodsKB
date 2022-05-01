using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace GoodsKB.DAL.Repositories;

public static class FieldPredicate<TEntity>
{
	public const FilterOperations FilterOperationMask =
		FilterOperations.Equal |
		FilterOperations.NotEqual |
		FilterOperations.Greater |
		FilterOperations.GreaterOrEqual |
		FilterOperations.Less |
		FilterOperations.LessOrEqual |
		FilterOperations.IsNull |
		FilterOperations.IsNotNull |
		FilterOperations.In |
		FilterOperations.NotIn |
		FilterOperations.Between |
		FilterOperations.NotBetween |
		FilterOperations.Like |
		FilterOperations.NotLike |
		FilterOperations.BitsAnd |
		FilterOperations.BitsOr;
	public const FilterOperations FilterFlagMask =
		FilterOperations.TrueWhenNull |
		FilterOperations.CaseInsensitive |
		FilterOperations.CaseInsensitiveInvariant;

	public static readonly ParameterExpression EntityParameter = Expression.Parameter(typeof(TEntity), "item");
	public static readonly MethodInfo ToLowerMethodInfo = typeof(string).GetMethod("ToLower", new Type[] { })!;
	public static readonly MethodInfo ToLowerInvariantMethodInfo = typeof(string).GetMethod("ToLowerInvariant", new Type[] { })!;
	public static readonly MethodInfo ContainsMethodInfo = typeof(string).GetMethod("Contains", new Type[] { typeof(string) })!;

	public static Expression Build(FilterOperations operation, Type operandType, string propName)
	{
		switch (operation & FilterOperationMask)
		{
			case FilterOperations.IsNull:
				{
					var propMember = Expression.PropertyOrField(EntityParameter, propName);
					var predicate = Expression.Equal(propMember, Expression.Constant(null, operandType));
					return predicate;
				}
			case FilterOperations.IsNotNull:
				{
					var propMember = Expression.PropertyOrField(EntityParameter, propName);
					var predicate = Expression.NotEqual(propMember, Expression.Constant(null, operandType));
					return predicate;
				}
			default:
				throw new InvalidOperationException("Wrong number of operands or the filter operation itself.");
		}
	}
	public static Expression Build<TField>(FilterOperations operation, Expression<Func<TEntity, TField>> left) =>
		Build(operation, typeof(TField), GetPropName(left));

	public static Expression Build(FilterOperations operation, Type operandType, string propName, object? right)
	{
		switch (operation & FilterOperationMask)
		{
			case FilterOperations.Equal:
				{
					if ((operation & (FilterOperations.CaseInsensitive | FilterOperations.CaseInsensitiveInvariant)) != 0)
					{
						if (operandType != typeof(string))
							throw new InvalidOperationException(@$"The ""{propName}"" field filter. The ""Like"" and ""NotLike"" filter operations are only allowed on strings.");

						var propInfo = typeof(TEntity).GetProperty(propName)
							?? throw new InvalidOperationException($"{typeof(TEntity).Name} does not have a property named {propName}.");
						var propMember = Expression.MakeMemberAccess(EntityParameter, propInfo);

						Expression toLowerCall;
						string? lowerCaseRight;
						if (operation.HasFlag(FilterOperations.CaseInsensitive))
						{
							toLowerCall = Expression.Call(propMember, ToLowerMethodInfo);
							lowerCaseRight = right?.ToString()?.ToLower()!;
						}
						else
						{
							toLowerCall = Expression.Call(propMember, ToLowerInvariantMethodInfo);
							lowerCaseRight = right?.ToString()?.ToLowerInvariant()!;
						}

						var rightConst = Expression.Constant(lowerCaseRight, operandType);
						var predicate = Expression.Equal(toLowerCall, rightConst);
						if (operation.HasFlag(FilterOperations.TrueWhenNull) && right is not null)
						{
							var nullConst = Expression.Constant(null, operandType);
							predicate = Expression.OrElse(
								Expression.Equal(propMember, nullConst),
								predicate
							);
						}
						return predicate;
					}
					else
					{
						var propMember = Expression.PropertyOrField(EntityParameter, propName);
						var rightConst = Expression.Constant(right, operandType);
						var predicate = Expression.Equal(propMember, rightConst);
						if (operation.HasFlag(FilterOperations.TrueWhenNull) && right is not null)
						{
							var nullConst = Expression.Constant(null, operandType);
							predicate = Expression.OrElse(
								Expression.Equal(propMember, nullConst),
								predicate
							);
						}
						return predicate;
					}
				}
			case FilterOperations.NotEqual:
				{
					if ((operation & (FilterOperations.CaseInsensitive | FilterOperations.CaseInsensitiveInvariant)) != 0)
					{
						if (operandType != typeof(string))
							throw new InvalidOperationException(@$"The ""{propName}"" field filter. The ""Like"" and ""NotLike"" filter operations are only allowed on strings.");

						var propInfo = typeof(TEntity).GetProperty(propName)
							?? throw new InvalidOperationException(@$"""{typeof(TEntity).Name}"" does not have a property named ""{propName}"".");
						var propMember = Expression.MakeMemberAccess(EntityParameter, propInfo);

						Expression toLowerCall;
						string? lowerCaseRight;
						if (operation.HasFlag(FilterOperations.CaseInsensitive))
						{
							toLowerCall = Expression.Call(propMember, ToLowerMethodInfo);
							lowerCaseRight = right?.ToString()?.ToLower()!;
						}
						else
						{
							toLowerCall = Expression.Call(propMember, ToLowerInvariantMethodInfo);
							lowerCaseRight = right?.ToString()?.ToLowerInvariant()!;
						}

						var rightConst = Expression.Constant(lowerCaseRight, operandType);
						var predicate = Expression.NotEqual(toLowerCall, rightConst);
						if (operation.HasFlag(FilterOperations.TrueWhenNull) && right is not null)
						{
							var nullConst = Expression.Constant(null, operandType);
							predicate = Expression.OrElse(
								Expression.Equal(propMember, nullConst),
								predicate
							);
						}
						return predicate;
					}
					else
					{
						var propMember = Expression.PropertyOrField(EntityParameter, propName);
						var rightConst = Expression.Constant(right, operandType);
						var predicate = Expression.NotEqual(propMember, rightConst);
						if (operation.HasFlag(FilterOperations.TrueWhenNull) && right is not null)
						{
							var nullConst = Expression.Constant(null, operandType);
							predicate = Expression.OrElse(
								Expression.Equal(propMember, nullConst),
								predicate
							);
						}
						return predicate;
					}
				}
			case FilterOperations.Greater:
				{
					var propMember = Expression.PropertyOrField(EntityParameter, propName);
					var rightConst = Expression.Constant(right, operandType);
					var predicate = Expression.GreaterThan(propMember, rightConst);
					if (operation.HasFlag(FilterOperations.TrueWhenNull) && right is not null)
					{
						var nullConst = Expression.Constant(null, operandType);
						predicate = Expression.OrElse(
							Expression.Equal(propMember, nullConst),
							predicate
						);
					}
					return predicate;
				}
			case FilterOperations.GreaterOrEqual:
				{
					var propMember = Expression.PropertyOrField(EntityParameter, propName);
					var rightConst = Expression.Constant(right, operandType);
					var predicate = Expression.GreaterThanOrEqual(propMember, rightConst);
					if (operation.HasFlag(FilterOperations.TrueWhenNull) && right is not null)
					{
						var nullConst = Expression.Constant(null, operandType);
						predicate = Expression.OrElse(
							Expression.Equal(propMember, nullConst),
							predicate
						);
					}
					return predicate;
				}
			case FilterOperations.Less:
				{
					var propMember = Expression.PropertyOrField(EntityParameter, propName);
					var rightConst = Expression.Constant(right, operandType);
					var predicate = Expression.LessThan(propMember, rightConst);
					if (operation.HasFlag(FilterOperations.TrueWhenNull) && right is not null)
					{
						var nullConst = Expression.Constant(null, operandType);
						predicate = Expression.OrElse(
							Expression.Equal(propMember, nullConst),
							predicate
						);
					}
					return predicate;
				}
			case FilterOperations.LessOrEqual:
				{
					var propMember = Expression.PropertyOrField(EntityParameter, propName);
					var rightConst = Expression.Constant(right, operandType);
					var predicate = Expression.LessThanOrEqual(propMember, rightConst);
					if (operation.HasFlag(FilterOperations.TrueWhenNull) && right is not null)
					{
						var nullConst = Expression.Constant(null, operandType);
						predicate = Expression.OrElse(
							Expression.Equal(propMember, nullConst),
							predicate
						);
					}
					return predicate;
				}
			case FilterOperations.In:
				{
					if (right is null) throw new ArgumentNullException("right");
					var propMember = Expression.PropertyOrField(EntityParameter, propName);
					var rightConst = Expression.Constant(right, typeof(IEnumerable<>).MakeGenericType(operandType));
					var predicate = Expression.Call(typeof(Enumerable), "Contains", new Type[] { operandType }, rightConst, propMember);
					return predicate;
				}
			case FilterOperations.NotIn:
				{
					if (right is null) throw new ArgumentNullException("right");
					var propMember = Expression.PropertyOrField(EntityParameter, propName);
					var rightConst = Expression.Constant(right, typeof(IEnumerable<>).MakeGenericType(operandType));
					var predicate = Expression.Not(
						Expression.Call(typeof(Enumerable), "Contains", new Type[] { operandType }, rightConst, propMember)
					);
					return predicate;
				}
			case FilterOperations.Like:
			case FilterOperations.NotLike:
				{
					if (operandType != typeof(string))
						throw new InvalidOperationException(@$"The ""{propName}"" field filter. The ""Like"" and ""NotLike"" filter operations are only allowed on strings.");

					var propInfo = typeof(TEntity).GetProperty(propName)
							?? throw new InvalidOperationException(@$"""{typeof(TEntity).Name}"" does not have a property named ""{propName}"".");
					var propMember = Expression.MakeMemberAccess(EntityParameter, propInfo);

					Expression predicate;
					if ((operation & (FilterOperations.CaseInsensitive | FilterOperations.CaseInsensitiveInvariant)) != 0)
					{
						Expression toLowerCall;
						string? lowerCaseRight;
						if (operation.HasFlag(FilterOperations.CaseInsensitive))
						{
							toLowerCall = Expression.Call(propMember, ToLowerMethodInfo);
							lowerCaseRight = right?.ToString()?.ToLower()!;
						}
						else
						{
							toLowerCall = Expression.Call(propMember, ToLowerInvariantMethodInfo);
							lowerCaseRight = right?.ToString()?.ToLowerInvariant()!;
						}

						var rightConst = Expression.Constant(lowerCaseRight, operandType);
						var containsCall = Expression.Call(toLowerCall, ContainsMethodInfo, rightConst);
						predicate = operation.HasFlag(FilterOperations.Like) ?
							containsCall :
							Expression.Not(containsCall);
					}
					else
					{
						var rightConst = Expression.Constant(right, operandType);
						var containsCall = Expression.Call(propMember, ContainsMethodInfo, rightConst);
						predicate = operation.HasFlag(FilterOperations.Like) ?
							containsCall :
							Expression.Not(containsCall);
					}

					if (operation.HasFlag(FilterOperations.TrueWhenNull) && right is not null)
					{
						var nullConst = Expression.Constant(null, operandType);
						predicate = Expression.OrElse(
							Expression.Equal(propMember, nullConst),
							predicate
						);
					}
					return predicate;
				}
			case FilterOperations.BitsAnd:
				{
					var propMember = Expression.PropertyOrField(EntityParameter, propName);
					var rightConst = Expression.Constant(right, operandType);
					var predicate = Expression.Equal(
						Expression.And(propMember, rightConst),
						rightConst);
					if (operation.HasFlag(FilterOperations.TrueWhenNull) && right is not null)
					{
						var nullConst = Expression.Constant(null, operandType);
						predicate = Expression.OrElse(
							Expression.Equal(propMember, nullConst),
							predicate
						);
					}
					return predicate;
				}
			case FilterOperations.BitsOr:
				{
					var propMember = Expression.PropertyOrField(EntityParameter, propName);
					var rightConst = Expression.Constant(right, operandType);
					var zeroConst = Expression.Constant((object)0, operandType);
					var predicate = Expression.NotEqual(
						Expression.And(propMember, rightConst),
						zeroConst);
					if (operation.HasFlag(FilterOperations.TrueWhenNull) && right is not null)
					{
						var nullConst = Expression.Constant(null, operandType);
						predicate = Expression.OrElse(
							Expression.Equal(propMember, nullConst),
							predicate
						);
					}
					return predicate;
				}
			default:
				throw new InvalidOperationException("Wrong number of operands or the filter operation itself.");
		}
	}
	public static Expression Build<TField>(FilterOperations operation, Expression<Func<TEntity, TField>> left, TField right) =>
		Build(operation, typeof(TField), GetPropName(left), right);
	public static Expression Build<TField>(FilterOperations operation, Expression<Func<TEntity, TField>> left, IEnumerable<TField> right) =>
		Build(operation, typeof(TField), GetPropName(left), right);
	public static Expression Build(FilterOperations operation, Type operandType, string propName, object? right, object? secondRight)
	{
		switch (operation & FilterOperationMask)
		{
			case FilterOperations.Between:
				{
					var propMember = Expression.PropertyOrField(EntityParameter, propName);
					var rightConst = Expression.Constant(right, operandType);
					var secondRightConst = Expression.Constant(secondRight, operandType);
					var predicate = Expression.AndAlso(
						Expression.GreaterThanOrEqual(propMember, rightConst),
						Expression.LessThanOrEqual(propMember, secondRightConst));
					if (operation.HasFlag(FilterOperations.TrueWhenNull))
					{
						var nullConst = Expression.Constant(null, operandType);
						predicate = Expression.OrElse(
							Expression.Equal(propMember, nullConst),
							predicate
						);
					}
					return predicate;
				}
			case FilterOperations.NotBetween:
				{
					var propMember = Expression.PropertyOrField(EntityParameter, propName);
					var rightConst = Expression.Constant(right, operandType);
					var secondRightConst = Expression.Constant(secondRight, operandType);
					var predicate = Expression.OrElse(
						Expression.LessThan(propMember, rightConst),
						Expression.GreaterThan(propMember, secondRightConst));
					if (operation.HasFlag(FilterOperations.TrueWhenNull))
					{
						var nullConst = Expression.Constant(null, operandType);
						predicate = Expression.OrElse(
							Expression.Equal(propMember, nullConst),
							predicate
						);
					}
					return predicate;
				}
			default:
				throw new InvalidOperationException("Wrong number of operands or the filter operation itself.");
		}
	}
	public static Expression Build<TField>(FilterOperations operation, Expression<Func<TEntity, TField>> left, TField right, TField secondRight) =>
		Build(operation, typeof(TField), GetPropName(left), right, secondRight);

	public static (int operandCount, bool isManyRightOperands, bool isCaseInsensitiveCompilant) GetOperandsInfo(FilterOperations operation) =>
		(operation & FilterOperationMask) switch
		{
			FilterOperations.Equal => (2, false, true),
			FilterOperations.NotEqual => (2, false, true),
			FilterOperations.Greater => (2, false, false),
			FilterOperations.GreaterOrEqual => (2, false, false),
			FilterOperations.Less => (2, false, false),
			FilterOperations.LessOrEqual => (2, false, false),
			FilterOperations.IsNull => (1, false, false),
			FilterOperations.IsNotNull => (1, false, false),
			FilterOperations.In => (2, true, false),
			FilterOperations.NotIn => (2, true, false),
			FilterOperations.Between => (3, false, false),
			FilterOperations.NotBetween => (3, false, false),
			FilterOperations.Like => (2, false, true),
			FilterOperations.NotLike => (2, false, true),
			FilterOperations.BitsAnd => (2, false, false),
			FilterOperations.BitsOr => (2, false, false),
			_ => throw new NotSupportedException($"The {operation.ToString()} filter by field operation is not supported.")
		};

	private static string GetPropName<TField>(Expression<Func<TEntity, TField>> memberSelector) =>
		(
			memberSelector.Body as MemberExpression ??
			(memberSelector.Body as UnaryExpression)?.Operand as MemberExpression ??
			((memberSelector.Body as UnaryExpression)?.Operand as UnaryExpression)?.Operand as MemberExpression
		)?.Member.Name ?? throw new InvalidOperationException("Could not deduct a property name from the member selector.");
}