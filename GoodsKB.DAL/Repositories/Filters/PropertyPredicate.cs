using System.Linq.Expressions;
using System.Reflection;

namespace GoodsKB.DAL.Repositories.Filters;

internal static class PropertyPredicate<TEntity>
{
	public static readonly ParameterExpression EntityParameter = Expression.Parameter(typeof(TEntity), "item");
	public static readonly MethodInfo ToLowerMethodInfo = typeof(string).GetMethod("ToLower", new Type[] { })!;
	public static readonly MethodInfo ToLowerInvariantMethodInfo = typeof(string).GetMethod("ToLowerInvariant", new Type[] { })!;
	public static readonly MethodInfo ContainsMethodInfo = typeof(string).GetMethod("Contains", new Type[] { typeof(string) })!;

	public static Expression Build(FO operation, Type operandType, string propName)
	{
		switch (operation & FOs.All)
		{
			case FO.IsNull:
				{
					var propMember = Expression.PropertyOrField(EntityParameter, propName);
					var nullConst = Expression.Constant(null, operandType);
					var predicate = Expression.Equal(propMember, nullConst);
					return predicate;
				}
			case FO.IsNotNull:
				{
					var propMember = Expression.PropertyOrField(EntityParameter, propName);
					var nullConst = Expression.Constant(null, operandType);
					var predicate = Expression.NotEqual(propMember, nullConst);
					if (operation.HasFlag(FO.TrueWhenNull))
					{
						predicate = Expression.OrElse(
							Expression.Equal(propMember, nullConst),
							predicate
						);
					}
					return predicate;
				}
			default:
				throw new InvalidOperationException($"Invalid number of arguments or the operation itself on {propName}.");
		}
	}
	public static Expression Build<TField>(FO operation, Expression<Func<TEntity, TField>> left) =>
		Build(operation, typeof(TField), GetMemberName(left));

	public static Expression Build(FO operation, Type operandType, string propName, object? right)
	{
		switch (operation & FOs.All)
		{
			case FO.Equal:
				{
					if ((operation & (FO.CaseInsensitive | FO.CaseInsensitiveInvariant)) != 0)
					{
						if (!typeof(string).IsAssignableFrom(operandType))
							throw new InvalidOperationException($"The Like or NotLike operation cannot be applied to {propName}. These operations are performed only on strings.");

						var propInfo = typeof(TEntity).GetProperty(propName)
							?? throw new InvalidOperationException($"{typeof(TEntity).Name} does not have a property named {propName}.");
						var propMember = Expression.MakeMemberAccess(EntityParameter, propInfo);

						Expression toLowerCall;
						string? lowerCaseRight;
						if (operation.HasFlag(FO.CaseInsensitive))
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
						if (operation.HasFlag(FO.TrueWhenNull) && right is not null)
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
						if (operation.HasFlag(FO.TrueWhenNull) && right is not null)
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
			case FO.NotEqual:
				{
					if ((operation & (FO.CaseInsensitive | FO.CaseInsensitiveInvariant)) != 0)
					{
						if (operandType != typeof(string))
							throw new InvalidOperationException($"The Like or NotLike operation cannot be applied to {propName}. These operations are performed only on strings.");

						var propInfo = typeof(TEntity).GetProperty(propName)
							?? throw new InvalidOperationException($"{typeof(TEntity).Name} does not have a property named {propName}.");
						var propMember = Expression.MakeMemberAccess(EntityParameter, propInfo);

						Expression toLowerCall;
						string? lowerCaseRight;
						if (operation.HasFlag(FO.CaseInsensitive))
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
						if (operation.HasFlag(FO.TrueWhenNull) && right is not null)
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
						if (operation.HasFlag(FO.TrueWhenNull) && right is not null)
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
			case FO.Greater:
				{
					var propMember = Expression.PropertyOrField(EntityParameter, propName);
					var rightConst = Expression.Constant(right, operandType);
					var predicate = Expression.GreaterThan(propMember, rightConst);
					if (operation.HasFlag(FO.TrueWhenNull) && right is not null)
					{
						var nullConst = Expression.Constant(null, operandType);
						predicate = Expression.OrElse(
							Expression.Equal(propMember, nullConst),
							predicate
						);
					}
					return predicate;
				}
			case FO.GreaterOrEqual:
				{
					var propMember = Expression.PropertyOrField(EntityParameter, propName);
					var rightConst = Expression.Constant(right, operandType);
					var predicate = Expression.GreaterThanOrEqual(propMember, rightConst);
					if (operation.HasFlag(FO.TrueWhenNull) && right is not null)
					{
						var nullConst = Expression.Constant(null, operandType);
						predicate = Expression.OrElse(
							Expression.Equal(propMember, nullConst),
							predicate
						);
					}
					return predicate;
				}
			case FO.Less:
				{
					var propMember = Expression.PropertyOrField(EntityParameter, propName);
					var rightConst = Expression.Constant(right, operandType);
					var predicate = Expression.LessThan(propMember, rightConst);
					if (operation.HasFlag(FO.TrueWhenNull) && right is not null)
					{
						var nullConst = Expression.Constant(null, operandType);
						predicate = Expression.OrElse(
							Expression.Equal(propMember, nullConst),
							predicate
						);
					}
					return predicate;
				}
			case FO.LessOrEqual:
				{
					var propMember = Expression.PropertyOrField(EntityParameter, propName);
					var rightConst = Expression.Constant(right, operandType);
					var predicate = Expression.LessThanOrEqual(propMember, rightConst);
					if (operation.HasFlag(FO.TrueWhenNull) && right is not null)
					{
						var nullConst = Expression.Constant(null, operandType);
						predicate = Expression.OrElse(
							Expression.Equal(propMember, nullConst),
							predicate
						);
					}
					return predicate;
				}
			case FO.In:
				{
					if (right is null) throw new ArgumentNullException("right");
					var propMember = Expression.PropertyOrField(EntityParameter, propName);
					var rightConst = Expression.Constant(right, typeof(IEnumerable<>).MakeGenericType(operandType));
					Expression predicate = Expression.Call(typeof(Enumerable), "Contains", new Type[] { operandType }, rightConst, propMember);
					if (operation.HasFlag(FO.TrueWhenNull))
					{
						var nullConst = Expression.Constant(null, operandType);
						predicate = Expression.OrElse(
							Expression.Equal(propMember, nullConst),
							predicate
						);
					}
					return predicate;
				}
			case FO.NotIn:
				{
					if (right is null) throw new ArgumentNullException("right");
					var propMember = Expression.PropertyOrField(EntityParameter, propName);
					var rightConst = Expression.Constant(right, typeof(IEnumerable<>).MakeGenericType(operandType));
					Expression predicate = Expression.Not(
						Expression.Call(typeof(Enumerable), "Contains", new Type[] { operandType }, rightConst, propMember)
					);
					if (operation.HasFlag(FO.TrueWhenNull))
					{
						var nullConst = Expression.Constant(null, operandType);
						predicate = Expression.OrElse(
							Expression.Equal(propMember, nullConst),
							predicate
						);
					}
					return predicate;
				}
			case FO.Like:
			case FO.NotLike:
				{
					if (operandType != typeof(string))
						throw new InvalidOperationException($"The Like or NotLike operation cannot be applied to {propName}. These operations are performed only on strings.");

					var propInfo = typeof(TEntity).GetProperty(propName)
							?? throw new InvalidOperationException($"{typeof(TEntity).Name} does not have a property named {propName}.");
					var propMember = Expression.MakeMemberAccess(EntityParameter, propInfo);

					Expression predicate;
					if ((operation & (FO.CaseInsensitive | FO.CaseInsensitiveInvariant)) != 0)
					{
						Expression toLowerCall;
						string? lowerCaseRight;
						if (operation.HasFlag(FO.CaseInsensitive))
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
						predicate = operation.HasFlag(FO.Like) ?
							containsCall :
							Expression.Not(containsCall);
					}
					else
					{
						var rightConst = Expression.Constant(right, operandType);
						var containsCall = Expression.Call(propMember, ContainsMethodInfo, rightConst);
						predicate = operation.HasFlag(FO.Like) ?
							containsCall :
							Expression.Not(containsCall);
					}

					if (operation.HasFlag(FO.TrueWhenNull) && right is not null)
					{
						var nullConst = Expression.Constant(null, operandType);
						predicate = Expression.OrElse(
							Expression.Equal(propMember, nullConst),
							predicate
						);
					}
					return predicate;
				}
			case FO.BitsAnd:
				{
					var propMember = Expression.PropertyOrField(EntityParameter, propName);
					var rightConst = Expression.Constant(right, operandType);
					var predicate = Expression.Equal(
						Expression.And(propMember, rightConst),
						rightConst);
					if (operation.HasFlag(FO.TrueWhenNull) && right is not null)
					{
						var nullConst = Expression.Constant(null, operandType);
						predicate = Expression.OrElse(
							Expression.Equal(propMember, nullConst),
							predicate
						);
					}
					return predicate;
				}
			case FO.BitsOr:
				{
					var propMember = Expression.PropertyOrField(EntityParameter, propName);
					var rightConst = Expression.Constant(right, operandType);
					var zeroConst = Expression.Constant((object)0, operandType);
					var predicate = Expression.NotEqual(
						Expression.And(propMember, rightConst),
						zeroConst);
					if (operation.HasFlag(FO.TrueWhenNull) && right is not null)
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
				throw new InvalidOperationException($"Invalid number of arguments or the operation itself on {propName}.");
		}
	}
	public static Expression Build<TField>(FO operation, Expression<Func<TEntity, TField>> left, TField right) =>
		Build(operation, typeof(TField), GetMemberName(left), right);
	public static Expression Build<TField>(FO operation, Expression<Func<TEntity, TField>> left, IEnumerable<TField> right) =>
		Build(operation, typeof(TField), GetMemberName(left), right);

	public static Expression Build(FO operation, Type operandType, string propName, object? right, object? secondRight)
	{
		switch (operation & FOs.All)
		{
			case FO.Between:
				{
					var propMember = Expression.PropertyOrField(EntityParameter, propName);
					var rightConst = Expression.Constant(right, operandType);
					var secondRightConst = Expression.Constant(secondRight, operandType);
					var predicate = Expression.AndAlso(
						Expression.GreaterThanOrEqual(propMember, rightConst),
						Expression.LessThanOrEqual(propMember, secondRightConst));
					if (operation.HasFlag(FO.TrueWhenNull))
					{
						var nullConst = Expression.Constant(null, operandType);
						predicate = Expression.OrElse(
							Expression.Equal(propMember, nullConst),
							predicate
						);
					}
					return predicate;
				}
			case FO.NotBetween:
				{
					var propMember = Expression.PropertyOrField(EntityParameter, propName);
					var rightConst = Expression.Constant(right, operandType);
					var secondRightConst = Expression.Constant(secondRight, operandType);
					var predicate = Expression.OrElse(
						Expression.LessThan(propMember, rightConst),
						Expression.GreaterThan(propMember, secondRightConst));
					if (operation.HasFlag(FO.TrueWhenNull))
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
				throw new InvalidOperationException($"Invalid number of arguments or the operation itself on {propName}.");
		}
	}
	public static Expression Build<TField>(FO operation, Expression<Func<TEntity, TField>> left, TField right, TField secondRight) =>
		Build(operation, typeof(TField), GetMemberName(left), right, secondRight);

	public static (int operandCount, bool isManyRightOperands, bool isCaseInsensitiveCompilant) GetOperandsInfo(FO operation) =>
		(operation & FOs.All) switch
		{
			FO.Equal => (2, false, true),
			FO.NotEqual => (2, false, true),
			FO.Greater => (2, false, false),
			FO.GreaterOrEqual => (2, false, false),
			FO.Less => (2, false, false),
			FO.LessOrEqual => (2, false, false),
			FO.IsNull => (1, false, false),
			FO.IsNotNull => (1, false, false),
			FO.In => (2, true, false),
			FO.NotIn => (2, true, false),
			FO.Between => (3, false, false),
			FO.NotBetween => (3, false, false),
			FO.Like => (2, false, true),
			FO.NotLike => (2, false, true),
			FO.BitsAnd => (2, false, false),
			FO.BitsOr => (2, false, false),
			_ => throw new InvalidOperationException($"Filter operation [{((long)operation).ToString()}] is invalid or not set.")
		};

	private static string GetMemberName<TField>(Expression<Func<TEntity, TField>> memberSelector) =>
		(
			memberSelector.Body as MemberExpression ??
			(memberSelector.Body as UnaryExpression)?.Operand as MemberExpression ??
			((memberSelector.Body as UnaryExpression)?.Operand as UnaryExpression)?.Operand as MemberExpression
		)?.Member.Name ?? throw new InvalidOperationException($"Could not obtain a property name from the member selector expression for {typeof(TEntity).Name}.");
}