using System.Linq.Expressions;
using System.Reflection;

namespace GoodsKB.DAL.Repositories;

public sealed class FieldFilter<TEntity> : IFieldFilter<TEntity>
	where TEntity : class
{
	public FilterOperations Operation { get; }
	public Type OperandType { get; }
	public string Name { get; }
	public Expression<Func<TEntity, bool>> Condition { get; }

	static readonly ParameterExpression _itemParameter = Expression.Parameter(typeof(TEntity), "item");
	static readonly MethodInfo _toLowerMethodInfo = typeof(string).GetMethod("ToLower", new Type[] { })!;
	static readonly MethodInfo _toLowerInvariantMethodInfo = typeof(string).GetMethod("ToLowerInvariant", new Type[] { })!;
	static readonly MethodInfo _containsMethodInfo = typeof(string).GetMethod("Contains", new Type[] { typeof(string) })!;

	public FieldFilter(FilterOperations operation, Type operandType, string name, Expression<Func<TEntity, bool>> condition)
	{
		Operation = operation;
		OperandType = operandType;
		Name = name;
		Condition = condition;
	}

	public static IFieldFilter<TEntity> Create<TField>(FilterOperations operation, Expression<Func<TEntity, TField>> left)
	{
		var propName = (
				left.Body as MemberExpression ??
				(left.Body as UnaryExpression)?.Operand as MemberExpression ??
				((left.Body as UnaryExpression)?.Operand as UnaryExpression)?.Operand as MemberExpression
			)?.Member.Name ?? throw new InvalidCastException();
		var condition = Create<TField>(operation, propName);
		return new FieldFilter<TEntity>(operation, typeof(TField), propName, condition);
	}
	private static Expression<Func<TEntity, bool>> Create<TField>(FilterOperations operation, string propName)
	{
		(int operandCount, bool isManyRightOperands, bool isCaseInsensitiveCompilant) = GetOperandsInfo(operation);
		if (isManyRightOperands || operandCount != 1)
			throw new InvalidOperationException($"The number of operands in the {operation.ToString()} filter of the \"{propName}\" field must be {operandCount}.");
		if (!isCaseInsensitiveCompilant && (operation & (FilterOperations.CaseInsensitive | FilterOperations.CaseInsensitiveInvariant)) != 0)
			throw new InvalidOperationException($"The \"{propName}\" field is not a string data type and cannot support string comparison options in the filter.");

		switch (operation & ~FilterOperations.Flags)
		{
			case FilterOperations.IsNull:
				{
					var propMember = Expression.PropertyOrField(_itemParameter, propName);
					var predicate = Expression.Equal(propMember, Expression.Constant(null, typeof(TField)));
					return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
				}
			case FilterOperations.IsNotNull:
				{
					var propMember = Expression.PropertyOrField(_itemParameter, propName);
					var predicate = Expression.NotEqual(propMember, Expression.Constant(null, typeof(TField)));
					return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
				}
			default:
				throw new InvalidOperationException($"The {(int)operation} filter operation is not supported.");
		}
	}

	public static IFieldFilter<TEntity> Create<TField>(FilterOperations operation, Expression<Func<TEntity, TField>> left, TField right)
	{
		var propName = (
				left.Body as MemberExpression ??
				(left.Body as UnaryExpression)?.Operand as MemberExpression ??
				((left.Body as UnaryExpression)?.Operand as UnaryExpression)?.Operand as MemberExpression
			)?.Member.Name ?? throw new InvalidCastException();
		var condition = Create(operation, propName, right);
		return new FieldFilter<TEntity>(operation, typeof(TField), propName, condition);
	}
	private static Expression<Func<TEntity, bool>> Create<TField>(FilterOperations operation, string propName, TField right)
	{
		(int operandCount, bool isManyRightOperands, bool isCaseInsensitiveCompilant) = GetOperandsInfo(operation);
		if (isManyRightOperands || operandCount != 2)
			throw new InvalidOperationException($"The number of operands in the {operation.ToString()} filter of the \"{propName}\" field must be {operandCount}.");
		if (!isCaseInsensitiveCompilant && (operation & (FilterOperations.CaseInsensitive | FilterOperations.CaseInsensitiveInvariant)) != 0)
			throw new InvalidOperationException($"The \"{propName}\" field is not a string data type and cannot support string comparison options in the filter.");

		switch (operation & ~FilterOperations.Flags)
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
						return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
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
						return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
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
						return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
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
						return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
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
					return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
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
					return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
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
					return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
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
					return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
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
						predicate = (operation & ~FilterOperations.Flags) == FilterOperations.Like ?
							containsCall :
							Expression.Not(containsCall);
					}
					else
					{
						var rightConst = Expression.Constant(right, typeof(TField));
						var containsCall = Expression.Call(propMember, _containsMethodInfo, rightConst);
						predicate = (operation & ~FilterOperations.Flags) == FilterOperations.Like ?
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
					return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
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
					return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
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
					return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
				}
			default:
				throw new InvalidOperationException($"The {(int)operation} filter operation is not supported.");
		}
	}

	public static IFieldFilter<TEntity> Create<TField>(FilterOperations operation, Expression<Func<TEntity, TField>> left, TField right, TField secondRight)
	{
		var propName = (
				left.Body as MemberExpression ??
				(left.Body as UnaryExpression)?.Operand as MemberExpression ??
				((left.Body as UnaryExpression)?.Operand as UnaryExpression)?.Operand as MemberExpression
			)?.Member.Name ?? throw new InvalidCastException();
		var condition = Create(operation, propName, right, secondRight);
		return new FieldFilter<TEntity>(operation, typeof(TField), propName, condition);
	}
	private static Expression<Func<TEntity, bool>> Create<TField>(FilterOperations operation, string propName, TField right, TField secondRight)
	{
		(int operandCount, bool isManyRightOperands, bool isCaseInsensitiveCompilant) = GetOperandsInfo(operation);
		if (isManyRightOperands || operandCount != 3)
			throw new InvalidOperationException($"The number of operands in the {operation.ToString()} filter of the \"{propName}\" field must be {operandCount}.");
		if (!isCaseInsensitiveCompilant && (operation & (FilterOperations.CaseInsensitive | FilterOperations.CaseInsensitiveInvariant)) != 0)
			throw new InvalidOperationException($"The \"{propName}\" field is not a string data type and cannot support string comparison options in the filter.");

		switch (operation & ~FilterOperations.Flags)
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
					return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
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
					return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
				}
			default:
				throw new InvalidOperationException($"The {(int)operation} filter operation is not supported.");
		}
	}

	public static IFieldFilter<TEntity> Create<TField>(FilterOperations operation, Expression<Func<TEntity, TField>> left, IEnumerable<TField> right)
	{
		var propName = (
				left.Body as MemberExpression ??
				(left.Body as UnaryExpression)?.Operand as MemberExpression ??
				((left.Body as UnaryExpression)?.Operand as UnaryExpression)?.Operand as MemberExpression
			)?.Member.Name ?? throw new InvalidCastException();
		var condition = Create<TField>(operation, propName, right);
		return new FieldFilter<TEntity>(operation, typeof(TField), propName, condition);
	}
	private static Expression<Func<TEntity, bool>> Create<TField>(FilterOperations operation, string propName, IEnumerable<TField> right)
	{
		(int operandCount, bool isManyRightOperands, bool isCaseInsensitiveCompilant) = GetOperandsInfo(operation);
		if (!isManyRightOperands || operandCount != 1)
			throw new InvalidOperationException($"The number of operands in the {operation.ToString()} filter of the \"{propName}\" field must be {operandCount}.");
		if (!isCaseInsensitiveCompilant && (operation & (FilterOperations.CaseInsensitive | FilterOperations.CaseInsensitiveInvariant)) != 0)
			throw new InvalidOperationException($"The \"{propName}\" field is not a string data type and cannot support string comparison options in the filter.");

		switch (operation & ~FilterOperations.Flags)
		{
			case FilterOperations.In:
				{
					var propMember = Expression.PropertyOrField(_itemParameter, propName);
					var rightConst = Expression.Constant(right, typeof(IEnumerable<TField>));
					var predicate = Expression.Call(typeof(Enumerable), "Contains", new Type[] { typeof(TField) }, rightConst, propMember);
					return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
				}
			case FilterOperations.NotIn:
				{
					var propMember = Expression.PropertyOrField(_itemParameter, propName);
					var rightConst = Expression.Constant(right, typeof(IEnumerable<TField>));
					var predicate = Expression.Not(
						Expression.Call(typeof(Enumerable), "Contains", new Type[] { typeof(TField) }, rightConst, propMember)
					);
					return Expression.Lambda<Func<TEntity, bool>>(predicate, _itemParameter);
				}
			default:
				throw new InvalidOperationException($"The {(int)operation} filter operation is not supported.");
		}
	}

	public static (int operandCount, bool isManyRightOperands, bool isCaseInsensitiveCompilant) GetOperandsInfo(FilterOperations operation)
		=> (operation & ~FilterOperations.Flags) switch
		{
			FilterOperations.Equal => (2, false, true),
			FilterOperations.NotEqual => (2, false, true),
			FilterOperations.Greater => (2, false, false),
			FilterOperations.GreaterOrEqual => (2, false, false),
			FilterOperations.Less => (2, false, false),
			FilterOperations.LessOrEqual => (2, false, false),
			FilterOperations.IsNull => (1, false, false),
			FilterOperations.IsNotNull => (1, false, false),
			FilterOperations.In => (1, true, false),
			FilterOperations.NotIn => (1, true, false),
			FilterOperations.Between => (3, false, false),
			FilterOperations.NotBetween => (3, false, false),
			FilterOperations.Like => (2, false, true),
			FilterOperations.NotLike => (2, false, true),
			FilterOperations.BitsAnd => (2, false, false),
			FilterOperations.BitsOr => (2, false, false),
			_ => throw new NotSupportedException($"The {operation.ToString()} filter by field operation is not supported.")
		};
}