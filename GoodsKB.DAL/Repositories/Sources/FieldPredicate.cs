using System.Linq.Expressions;
using System.Reflection;

namespace GoodsKB.DAL.Repositories;

public static class FieldPredicate<TEntity>
{
	static readonly ParameterExpression _itemParameter = Expression.Parameter(typeof(TEntity), "item");
	static readonly MethodInfo _toLowerMethodInfo = typeof(string).GetMethod("ToLower", new Type[] { })!;
	static readonly MethodInfo _toLowerInvariantMethodInfo = typeof(string).GetMethod("ToLowerInvariant", new Type[] { })!;
	static readonly MethodInfo _containsMethodInfo = typeof(string).GetMethod("Contains", new Type[] { typeof(string) })!;

	public static Expression Build<TField>(FilterOperations operation, Expression<Func<TEntity, TField>> left)
	{
		var propName = GetPropName(left);
		switch (operation & FilterOperations.OperationMask)
		{
			case FilterOperations.IsNull:
				{
					var propMember = Expression.PropertyOrField(_itemParameter, propName);
					var predicate = Expression.Equal(propMember, Expression.Constant(null, typeof(TField)));
					//return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
					return predicate;
				}
			case FilterOperations.IsNotNull:
				{
					var propMember = Expression.PropertyOrField(_itemParameter, propName);
					var predicate = Expression.NotEqual(propMember, Expression.Constant(null, typeof(TField)));
					//return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
					return predicate;
				}
			default:
				throw new InvalidOperationException("Wrong number of operands or the filter operation itself.");
		}
	}

	public static Expression Build<TField>(FilterOperations operation, Expression<Func<TEntity, TField>> left, TField right)
	{
		var propName = GetPropName(left);
		switch (operation & FilterOperations.OperationMask)
		{
			case FilterOperations.Equal:
				{
					if ((operation & (FilterOperations.CaseInsensitive | FilterOperations.CaseInsensitiveInvariant)) != 0)
					{
						if (typeof(TField) != typeof(string))
							throw new InvalidOperationException($"The \"{propName}\" field filter. The \"Like\" and \"NotLike\" filter operations are only allowed on strings.");

						var propInfo = typeof(TEntity).GetProperty(propName)
							?? throw new InvalidOperationException($"{typeof(TEntity).Name} does not have a property named {propName}.");
						var propMember = Expression.MakeMemberAccess(_itemParameter, propInfo);

						Expression toLowerCall;
						TField lowerCaseRight;
						if (operation.HasFlag(FilterOperations.CaseInsensitive))
						{
							toLowerCall = Expression.Call(propMember, _toLowerMethodInfo);
							lowerCaseRight = (TField)(object?)right?.ToString()?.ToLower()!;
						}
						else
						{
							toLowerCall = Expression.Call(propMember, _toLowerInvariantMethodInfo);
							lowerCaseRight = (TField)(object?)right?.ToString()?.ToLowerInvariant()!;
						}

						var rightConst = Expression.Constant(lowerCaseRight, typeof(TField));
						var predicate = Expression.Equal(toLowerCall, rightConst);
						if (operation.HasFlag(FilterOperations.TrueWhenNull) && right is not null)
						{
							var nullConst = Expression.Constant(null, typeof(TField));
							predicate = Expression.OrElse(
								Expression.Equal(propMember, nullConst),
								predicate
							);
						}
						//return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
						return predicate;
					}
					else
					{
						var propMember = Expression.PropertyOrField(_itemParameter, propName);
						var rightConst = Expression.Constant(right, typeof(TField));
						var predicate = Expression.Equal(propMember, rightConst);
						if (operation.HasFlag(FilterOperations.TrueWhenNull) && right is not null)
						{
							var nullConst = Expression.Constant(null, typeof(TField));
							predicate = Expression.OrElse(
								Expression.Equal(propMember, nullConst),
								predicate
							);
						}
						//return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
						return predicate;
					}
				}
			case FilterOperations.NotEqual:
				{
					if ((operation & (FilterOperations.CaseInsensitive | FilterOperations.CaseInsensitiveInvariant)) != 0)
					{
						if (typeof(TField) != typeof(string))
							throw new InvalidOperationException($"The \"{propName}\" field filter. The \"Like\" and \"NotLike\" filter operations are only allowed on strings.");

						var propInfo = typeof(TEntity).GetProperty(propName)
							?? throw new InvalidOperationException($"{typeof(TEntity).Name} does not have a property named {propName}.");
						var propMember = Expression.MakeMemberAccess(_itemParameter, propInfo);

						Expression toLowerCall;
						TField lowerCaseRight;
						if (operation.HasFlag(FilterOperations.CaseInsensitive))
						{
							toLowerCall = Expression.Call(propMember, _toLowerMethodInfo);
							lowerCaseRight = (TField)(object?)right?.ToString()?.ToLower()!;
						}
						else
						{
							toLowerCall = Expression.Call(propMember, _toLowerInvariantMethodInfo);
							lowerCaseRight = (TField)(object?)right?.ToString()?.ToLowerInvariant()!;
						}

						var rightConst = Expression.Constant(lowerCaseRight, typeof(TField));
						var predicate = Expression.NotEqual(toLowerCall, rightConst);
						if (operation.HasFlag(FilterOperations.TrueWhenNull) && right is not null)
						{
							var nullConst = Expression.Constant(null, typeof(TField));
							predicate = Expression.OrElse(
								Expression.Equal(propMember, nullConst),
								predicate
							);
						}
						//return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
						return predicate;
					}
					else
					{
						var propMember = Expression.PropertyOrField(_itemParameter, propName);
						var rightConst = Expression.Constant(right, typeof(TField));
						var predicate = Expression.NotEqual(propMember, rightConst);
						if (operation.HasFlag(FilterOperations.TrueWhenNull) && right is not null)
						{
							var nullConst = Expression.Constant(null, typeof(TField));
							predicate = Expression.OrElse(
								Expression.Equal(propMember, nullConst),
								predicate
							);
						}
						//return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
						return predicate;
					}
				}
			case FilterOperations.Greater:
				{
					var propMember = Expression.PropertyOrField(_itemParameter, propName);
					var rightConst = Expression.Constant(right, typeof(TField));
					var predicate = Expression.GreaterThan(propMember, rightConst);
					if (operation.HasFlag(FilterOperations.TrueWhenNull) && right is not null)
					{
						var nullConst = Expression.Constant(null, typeof(TField));
						predicate = Expression.OrElse(
							Expression.Equal(propMember, nullConst),
							predicate
						);
					}
					//return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
					return predicate;
				}
			case FilterOperations.GreaterOrEqual:
				{
					var propMember = Expression.PropertyOrField(_itemParameter, propName);
					var rightConst = Expression.Constant(right, typeof(TField));
					var predicate = Expression.GreaterThanOrEqual(propMember, rightConst);
					if (operation.HasFlag(FilterOperations.TrueWhenNull) && right is not null)
					{
						var nullConst = Expression.Constant(null, typeof(TField));
						predicate = Expression.OrElse(
							Expression.Equal(propMember, nullConst),
							predicate
						);
					}
					//return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
					return predicate;
				}
			case FilterOperations.Less:
				{
					var propMember = Expression.PropertyOrField(_itemParameter, propName);
					var rightConst = Expression.Constant(right, typeof(TField));
					var predicate = Expression.LessThan(propMember, rightConst);
					if (operation.HasFlag(FilterOperations.TrueWhenNull) && right is not null)
					{
						var nullConst = Expression.Constant(null, typeof(TField));
						predicate = Expression.OrElse(
							Expression.Equal(propMember, nullConst),
							predicate
						);
					}
					//return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
					return predicate;
				}
			case FilterOperations.LessOrEqual:
				{
					var propMember = Expression.PropertyOrField(_itemParameter, propName);
					var rightConst = Expression.Constant(right, typeof(TField));
					var predicate = Expression.LessThanOrEqual(propMember, rightConst);
					if (operation.HasFlag(FilterOperations.TrueWhenNull) && right is not null)
					{
						var nullConst = Expression.Constant(null, typeof(TField));
						predicate = Expression.OrElse(
							Expression.Equal(propMember, nullConst),
							predicate
						);
					}
					//return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
					return predicate;
				}
			case FilterOperations.Like:
			case FilterOperations.NotLike:
				{
					if (typeof(TField) != typeof(string))
						throw new InvalidOperationException($"The \"{propName}\" field filter. The \"Like\" and \"NotLike\" filter operations are only allowed on strings.");

					var propInfo = typeof(TEntity).GetProperty(propName)
							?? throw new InvalidOperationException($"{typeof(TEntity).Name} does not have a property named {propName}.");
					var propMember = Expression.MakeMemberAccess(_itemParameter, propInfo);

					Expression predicate;
					if ((operation & (FilterOperations.CaseInsensitive | FilterOperations.CaseInsensitiveInvariant)) != 0)
					{
						Expression toLowerCall;
						TField lowerCaseRight;
						if (operation.HasFlag(FilterOperations.CaseInsensitive))
						{
							toLowerCall = Expression.Call(propMember, _toLowerMethodInfo);
							lowerCaseRight = (TField)(object?)right?.ToString()?.ToLower()!;
						}
						else
						{
							toLowerCall = Expression.Call(propMember, _toLowerInvariantMethodInfo);
							lowerCaseRight = (TField)(object?)right?.ToString()?.ToLowerInvariant()!;
						}

						var rightConst = Expression.Constant(lowerCaseRight, typeof(TField));
						var containsCall = Expression.Call(toLowerCall, _containsMethodInfo, rightConst);
						predicate = (operation & ~FilterOperations.FlagMask) == FilterOperations.Like ?
							containsCall :
							Expression.Not(containsCall);
					}
					else
					{
						var rightConst = Expression.Constant(right, typeof(TField));
						var containsCall = Expression.Call(propMember, _containsMethodInfo, rightConst);
						predicate = (operation & ~FilterOperations.FlagMask) == FilterOperations.Like ?
							containsCall :
							Expression.Not(containsCall);
					}

					if (operation.HasFlag(FilterOperations.TrueWhenNull) && right is not null)
					{
						var nullConst = Expression.Constant(null, typeof(TField));
						predicate = Expression.OrElse(
							Expression.Equal(propMember, nullConst),
							predicate
						);
					}
					//return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
					return predicate;
				}
			case FilterOperations.BitsAnd:
				{
					var propMember = Expression.PropertyOrField(_itemParameter, propName);
					var rightConst = Expression.Constant(right, typeof(TField));
					var predicate = Expression.Equal(
						Expression.And(propMember, rightConst),
						rightConst);
					if (operation.HasFlag(FilterOperations.TrueWhenNull) && right is not null)
					{
						var nullConst = Expression.Constant(null, typeof(TField));
						predicate = Expression.OrElse(
							Expression.Equal(propMember, nullConst),
							predicate
						);
					}
					//return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
					return predicate;
				}
			case FilterOperations.BitsOr:
				{
					var propMember = Expression.PropertyOrField(_itemParameter, propName);
					var rightConst = Expression.Constant(right, typeof(TField));
					var zeroConst = Expression.Constant((TField)(object)0, typeof(TField));
					var predicate = Expression.NotEqual(
						Expression.And(propMember, rightConst),
						zeroConst);
					if (operation.HasFlag(FilterOperations.TrueWhenNull) && right is not null)
					{
						var nullConst = Expression.Constant(null, typeof(TField));
						predicate = Expression.OrElse(
							Expression.Equal(propMember, nullConst),
							predicate
						);
					}
					//return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
					return predicate;
				}
			default:
				throw new InvalidOperationException("Wrong number of operands or the filter operation itself.");
		}
	}

	public static Expression Build<TField>(FilterOperations operation, Expression<Func<TEntity, TField>> left, TField right, TField secondRight)
	{
		var propName = GetPropName(left);
		switch (operation & FilterOperations.OperationMask)
		{
			case FilterOperations.Between:
				{
					var propMember = Expression.PropertyOrField(_itemParameter, propName);
					var rightConst = Expression.Constant(right, typeof(TField));
					var secondRightConst = Expression.Constant(secondRight, typeof(TField));
					var predicate = Expression.AndAlso(
						Expression.GreaterThanOrEqual(propMember, rightConst),
						Expression.LessThanOrEqual(propMember, secondRightConst));
					if (operation.HasFlag(FilterOperations.TrueWhenNull))
					{
						var nullConst = Expression.Constant(null, typeof(TField));
						predicate = Expression.OrElse(
							Expression.Equal(propMember, nullConst),
							predicate
						);
					}
					//return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
					return predicate;
				}
			case FilterOperations.NotBetween:
				{
					var propMember = Expression.PropertyOrField(_itemParameter, propName);
					var rightConst = Expression.Constant(right, typeof(TField));
					var secondRightConst = Expression.Constant(secondRight, typeof(TField));
					var predicate = Expression.OrElse(
						Expression.LessThan(propMember, rightConst),
						Expression.GreaterThan(propMember, secondRightConst));
					if (operation.HasFlag(FilterOperations.TrueWhenNull))
					{
						var nullConst = Expression.Constant(null, typeof(TField));
						predicate = Expression.OrElse(
							Expression.Equal(propMember, nullConst),
							predicate
						);
					}
					//return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
					return predicate;
				}
			default:
				throw new InvalidOperationException("Wrong number of operands or the filter operation itself.");
		}
	}

	public static Expression Build<TField>(FilterOperations operation, Expression<Func<TEntity, TField>> left, IEnumerable<TField> right)
	{
		var propName = GetPropName(left);
		switch (operation & FilterOperations.OperationMask)
		{
			case FilterOperations.In:
				{
					var propMember = Expression.PropertyOrField(_itemParameter, propName);
					var rightConst = Expression.Constant(right, typeof(IEnumerable<TField>));
					var predicate = Expression.Call(typeof(Enumerable), "Contains", new Type[] { typeof(TField) }, rightConst, propMember);
					//return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
					return predicate;
				}
			case FilterOperations.NotIn:
				{
					var propMember = Expression.PropertyOrField(_itemParameter, propName);
					var rightConst = Expression.Constant(right, typeof(IEnumerable<TField>));
					var predicate = Expression.Not(
						Expression.Call(typeof(Enumerable), "Contains", new Type[] { typeof(TField) }, rightConst, propMember)
					);
					//return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
					return predicate;
				}
			default:
				throw new InvalidOperationException("Wrong number of operands or the filter operation itself.");
		}
	}

	public static (int operandCount, bool isManyRightOperands, bool isCaseInsensitiveCompilant) GetOperandsInfo(FilterOperations operation) =>
		(operation & FilterOperations.OperationMask) switch
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