using System.Linq.Expressions;
using System.Reflection;

namespace GoodsKB.DAL.Repositories;

public class FieldFilter<TEntity, TField> : IFieldFilter<TEntity>
	where TEntity : class
{
	public FieldFilterOperations Operation { get; }
	public Type OperandType { get; }
	public string Name { get; }
	public Expression<Func<TEntity, bool>> Condition { get; }

	public FieldFilter(FieldFilterOperations operation, Expression<Func<TEntity, TField?>> leftOperand)
		: this(operation, ((MemberExpression)leftOperand.Body).Member.Name) { }
	public FieldFilter(FieldFilterOperations operation, string name)
	{
		(int operandCount, bool isManyRightOperands, bool isCaseInsensitiveCompilant) = GetOperandsInfo(operation);
		if (isManyRightOperands || operandCount != 1)
			throw new InvalidOperationException($"The number of operands in the {operation.ToString()} filter of the \"{Name}\" field must be {operandCount}.");
		if (!isCaseInsensitiveCompilant && (operation & (FieldFilterOperations.CaseInsensitive | FieldFilterOperations.CaseInsensitiveInvariant)) != 0)
			throw new InvalidOperationException($"The \"{Name}\" field is not a string data type and cannot support string comparison options in the filter.");

		Operation = operation;
		Name = name;
		OperandType = typeof(TField);
		Condition = MakeOperator(default(TField?), default(TField?), null);
	}
	public FieldFilter(FieldFilterOperations operation, Expression<Func<TEntity, TField?>> leftOperand, TField rightOperand)
		: this(operation, ((MemberExpression)leftOperand.Body).Member.Name, rightOperand) { }
	public FieldFilter(FieldFilterOperations operation, string name, TField rightOperand)
	{
		(int operandCount, bool isManyRightOperands, bool isCaseInsensitiveCompilant) = GetOperandsInfo(operation);
		if (isManyRightOperands || operandCount != 2)
			throw new InvalidOperationException($"The number of operands in the {operation.ToString()} filter of the \"{Name}\" field must be {operandCount}.");
		if (!isCaseInsensitiveCompilant && (operation & (FieldFilterOperations.CaseInsensitive | FieldFilterOperations.CaseInsensitiveInvariant)) != 0)
			throw new InvalidOperationException($"The \"{Name}\" field is not a string data type and cannot support string comparison options in the filter.");

		Operation = operation;
		Name = name;
		OperandType = typeof(TField);
		Condition = MakeOperator(operation, Name, rightOperand);
	}
	public FieldFilter(FieldFilterOperations operation, Expression<Func<TEntity, TField?>> leftOperand, TField? rightOperand, TField? rightSecondOperand)
		: this(operation, ((MemberExpression)leftOperand.Body).Member.Name, rightOperand, rightSecondOperand) { }
	public FieldFilter(FieldFilterOperations operation, string name, TField? rightOperand, TField? secondRightOperand)
	{
		(int operandCount, bool isManyRightOperands, bool isCaseInsensitiveCompilant) = GetOperandsInfo(operation);
		if (isManyRightOperands || operandCount != 3)
			throw new InvalidOperationException($"The number of operands in the {operation.ToString()} filter of the \"{Name}\" field must be {operandCount}.");
		if (!isCaseInsensitiveCompilant && (operation & (FieldFilterOperations.CaseInsensitive | FieldFilterOperations.CaseInsensitiveInvariant)) != 0)
			throw new InvalidOperationException($"The \"{Name}\" field is not a string data type and cannot support string comparison options in the filter.");

		Operation = operation;
		Name = name;
		OperandType = typeof(TField);
		Condition = MakeOperator(rightOperand, secondRightOperand, null);
	}
	public FieldFilter(FieldFilterOperations operation, Expression<Func<TEntity, TField?>> leftOperand, IEnumerable<TField?> rightOperands)
		: this(operation, ((MemberExpression)leftOperand.Body).Member.Name, rightOperands) { }
	public FieldFilter(FieldFilterOperations operation, string name, IEnumerable<TField?> rightOperands)
	{
		(int operandCount, bool isManyRightOperands, bool isCaseInsensitiveCompilant) = GetOperandsInfo(operation);
		if (!isManyRightOperands)
			throw new InvalidOperationException($"The number of operands in the {operation.ToString()} filter of the \"{Name}\" field must be {operandCount}.");
		if (!isCaseInsensitiveCompilant && (operation & (FieldFilterOperations.CaseInsensitive | FieldFilterOperations.CaseInsensitiveInvariant)) != 0)
			throw new InvalidOperationException($"The \"{Name}\" field is not a string data type and cannot support string comparison options in the filter.");

		Operation = operation;
		Name = name;
		OperandType = typeof(TField);
		Condition = MakeOperator(default(TField?), default(TField?), rightOperands);
	}

	public static (int operandCount, bool isManyRightOperands, bool isCaseInsensitiveCompilant) GetOperandsInfo(FieldFilterOperations operation)
		=> (operation & ~FieldFilterOperations.Flags) switch
		{
			FieldFilterOperations.Equal => (2, false, true),
			FieldFilterOperations.NotEqual => (2, false, true),
			FieldFilterOperations.Greater => (2, false, false),
			FieldFilterOperations.GreaterOrEqual => (2, false, false),
			FieldFilterOperations.Less => (2, false, false),
			FieldFilterOperations.LessOrEqual => (2, false, false),
			FieldFilterOperations.IsNull => (1, false, false),
			FieldFilterOperations.IsNotNull => (1, false, false),
			FieldFilterOperations.In => (1, true, false),
			FieldFilterOperations.NotIn => (1, true, false),
			FieldFilterOperations.Between => (3, false, false),
			FieldFilterOperations.NotBetween => (3, false, false),
			FieldFilterOperations.Like => (2, false, true),
			FieldFilterOperations.NotLike => (2, false, true),
			FieldFilterOperations.BitsAnd => (2, false, false),
			FieldFilterOperations.BitsOr => (2, false, false),
			_ => throw new NotSupportedException($"The {operation.ToString()} filter by field operation is not supported.")
		};


	static readonly ParameterExpression itemParam = Expression.Parameter(typeof(TEntity), "item");
	static readonly MethodInfo _toLowerMethodInfo = typeof(string).GetMethod("ToLower", new Type[] { })!;
	static readonly MethodInfo _toLowerInvariantMethodInfo = typeof(string).GetMethod("ToLowerInvariant", new Type[] { })!;
	static readonly MethodInfo _containsMethodInfo = typeof(string).GetMethod("Contains", new Type[] { typeof(string) })!;

	private static Expression<Func<TEntity, bool>> MakeOperator(FieldFilterOperations operation, string memberName, TField RightOperand)
	{
		switch (operation & ~FieldFilterOperations.Flags)
		{
			case FieldFilterOperations.Equal:
				{
					if ((operation & (FieldFilterOperations.CaseInsensitive | FieldFilterOperations.CaseInsensitiveInvariant)) != 0)
					{
						if (typeof(TField) != typeof(string))
							throw new InvalidOperationException($"The \"{memberName}\" field filter. The \"Like\" and \"NotLike\" filter operations are only allowed on strings.");

						var propInfo = typeof(TEntity).GetProperty(memberName)
							?? throw new InvalidOperationException($"{typeof(TEntity).Name} does not have a property named {memberName}.");
						var propMember = Expression.MakeMemberAccess(itemParam, propInfo);

						Expression toLowerCall;
						TField lowerCaseRightOperand;

						if (operation.HasFlag(FieldFilterOperations.CaseInsensitive))
						{
							toLowerCall = Expression.Call(propMember, _toLowerMethodInfo);
							lowerCaseRightOperand = (TField)(object?)RightOperand?.ToString()?.ToLower()!;
						}
						else
						{
							toLowerCall = Expression.Call(propMember, _toLowerInvariantMethodInfo);
							lowerCaseRightOperand = (TField)(object?)RightOperand?.ToString()?.ToLowerInvariant()!;
						}

						var rightOperandConst = Expression.Constant(lowerCaseRightOperand, typeof(TField));

						BinaryExpression expr;
						if (operation.HasFlag(FieldFilterOperations.TrueWhenNull) && RightOperand != null)
						{
							expr = Expression.OrElse(
								Expression.Equal(propMember, Expression.Constant(null, typeof(TField?))),
								Expression.Equal(toLowerCall, rightOperandConst)
							);
						}
						else
						{
							expr = Expression.Equal(toLowerCall, rightOperandConst);
						}

						return Expression.Lambda<Func<TEntity, bool>>(expr, itemParam);
					}
					else
					{
						var prop = Expression.PropertyOrField(itemParam, memberName);
						var rhs = Expression.Constant(RightOperand, typeof(TField?));

						BinaryExpression expr;
						if (operation.HasFlag(FieldFilterOperations.TrueWhenNull) && RightOperand != null)
						{
							expr = Expression.OrElse(
								Expression.Equal(prop, Expression.Constant(null, typeof(TField?))),
								Expression.Equal(prop, rhs)
							);
						}
						else
						{
							expr = Expression.Equal(prop, rhs);
						}

						return Expression.Lambda<Func<TEntity, bool>>(expr, itemParam);
					}
				}
			case FieldFilterOperations.NotEqual:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, memberName);
					ConstantExpression rhs = Expression.Constant(RightOperand, typeof(TField));
					BinaryExpression expr = Expression.NotEqual(prop, rhs);
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			case FieldFilterOperations.Greater:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, memberName);
					ConstantExpression rhs = Expression.Constant(RightOperand, typeof(TField));
					BinaryExpression expr = Expression.GreaterThan(prop, rhs);
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			case FieldFilterOperations.GreaterOrEqual:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, memberName);
					ConstantExpression rhs = Expression.Constant(RightOperand, typeof(TField));
					BinaryExpression expr = Expression.GreaterThanOrEqual(prop, rhs);
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			case FieldFilterOperations.Less:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, memberName);
					ConstantExpression rhs = Expression.Constant(RightOperand, typeof(TField));
					BinaryExpression expr = Expression.LessThan(prop, rhs);
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			case FieldFilterOperations.LessOrEqual:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, memberName);
					ConstantExpression rhs = Expression.Constant(RightOperand, typeof(TField));
					BinaryExpression expr = Expression.LessThanOrEqual(prop, rhs);
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			case FieldFilterOperations.Like:
				{
					if (typeof(TField?) == typeof(string))
					{
						return BuildStringContainsPredicate<TEntity>(memberName, ((string)(object)RightOperand)?.ToLower());
					}
					throw new InvalidOperationException("The Like and NotLike filter operations are only allowed on strings.");
				}
			case FieldFilterOperations.NotLike:
				{
					if (typeof(TField?) == typeof(string))
					{
						var lowerCaseRightOperand = (string?)(object?)RightOperand;
						if (lowerCaseRightOperand != null)
							lowerCaseRightOperand = lowerCaseRightOperand.ToLower();

						var propertyInfo = typeof(TEntity).GetProperty(memberName);
						ParameterExpression pe = Expression.Parameter(typeof(TEntity), "p");
						MemberExpression memberExpression = Expression.MakeMemberAccess(pe, propertyInfo!);

						var toLowerMethodInfo = typeof(string).GetMethod("ToLower", new Type[] { });
						Expression toLowerCall = Expression.Call(memberExpression, toLowerMethodInfo!);

						var containsMethodInfo = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
						ConstantExpression constantExpression = Expression.Constant(lowerCaseRightOperand, typeof(string));

						MethodCallExpression call = Expression.Call(toLowerCall, containsMethodInfo!, constantExpression);
						UnaryExpression expr = Expression.Not(call);

						return Expression.Lambda<Func<TEntity, bool>>(expr, pe);
					}
					throw new InvalidOperationException("The Like and NotLike filter operations are only allowed on strings.");
				}
			case FieldFilterOperations.BitsAnd:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, memberName);
					ConstantExpression rhs = Expression.Constant(RightOperand, typeof(TField));
					BinaryExpression expr = Expression.Equal((BinaryExpression)Expression.And(prop, rhs), rhs);
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			case FieldFilterOperations.BitsOr:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, memberName);
					ConstantExpression rhs = Expression.Constant(RightOperand, typeof(TField));
					ConstantExpression zero = Expression.Constant(0, typeof(TField));
					BinaryExpression expr = Expression.NotEqual((BinaryExpression)Expression.Or(prop, rhs), zero);
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			default: throw new FormatException($"The {(int)operation} filter operation is not supported.");
		}
	}

	public Expression<Func<TEntity, bool>> MakeOperator(TField? RightOperand, TField? SecondRightOperand, IEnumerable<TField?>? RightOperands)
	{
		switch (Operation & ~FieldFilterOperations.Flags)
		{
			case FieldFilterOperations.Equal:
				{
					if ((Operation & (FieldFilterOperations.CaseInsensitive | FieldFilterOperations.CaseInsensitiveInvariant)) != 0)
					{
						if (OperandType != typeof(string))
							throw new InvalidOperationException($"The \"{Name}\" field filter. The \"Like\" and \"NotLike\" filter operations are only allowed on strings.");

						PropertyInfo propertyInfo = typeof(TEntity).GetProperty(Name)
							?? throw new InvalidOperationException($"{typeof(TEntity).Name} does not have a property named {Name}.");
						MemberExpression memberExpression = Expression.MakeMemberAccess(itemParam, propertyInfo);

						Expression toLowerCall;

						string? lowerRightOperand;
						if (Operation.HasFlag(FieldFilterOperations.CaseInsensitive))
						{
							toLowerCall = Expression.Call(memberExpression, _toLowerMethodInfo);
							lowerRightOperand = ((string?)(object?)RightOperand)?.ToLower();
						}
						else
						{
							toLowerCall = Expression.Call(memberExpression, _toLowerInvariantMethodInfo);
							lowerRightOperand = ((string?)(object?)RightOperand)?.ToLowerInvariant();
						}

						ConstantExpression constantExpression = Expression.Constant(lowerRightOperand, typeof(string));
						BinaryExpression expr = Expression.Equal(toLowerCall, constantExpression);

						return Expression.Lambda<Func<TEntity, bool>>(expr, itemParam);
					}
					else
					{
						var prop = Expression.PropertyOrField(itemParam, Name);
						var rhs = Expression.Constant(RightOperand, typeof(TField?));

						BinaryExpression expr;
						if (Operation.HasFlag(FieldFilterOperations.TrueWhenNull) && RightOperand is not null)
						{
							expr = Expression.OrElse(
								Expression.Equal(prop, Expression.Constant(null, typeof(TField?))),
								Expression.Equal(prop, rhs)
							);
						}
						else
						{
							expr = Expression.Equal(prop, rhs);
						}

						return Expression.Lambda<Func<TEntity, bool>>(expr, itemParam);
					}
				}
			case FieldFilterOperations.NotEqual:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, Name);
					ConstantExpression rhs = Expression.Constant(RightOperand, typeof(TField));
					BinaryExpression expr = Expression.NotEqual(prop, rhs);
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			case FieldFilterOperations.Greater:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, Name);
					ConstantExpression rhs = Expression.Constant(RightOperand, typeof(TField));
					BinaryExpression expr = Expression.GreaterThan(prop, rhs);
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			case FieldFilterOperations.GreaterOrEqual:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, Name);
					ConstantExpression rhs = Expression.Constant(RightOperand, typeof(TField));
					BinaryExpression expr = Expression.GreaterThanOrEqual(prop, rhs);
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			case FieldFilterOperations.Less:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, Name);
					ConstantExpression rhs = Expression.Constant(RightOperand, typeof(TField));
					BinaryExpression expr = Expression.LessThan(prop, rhs);
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			case FieldFilterOperations.LessOrEqual:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, Name);
					ConstantExpression rhs = Expression.Constant(RightOperand, typeof(TField));
					BinaryExpression expr = Expression.LessThanOrEqual(prop, rhs);
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			case FieldFilterOperations.IsNull:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, Name);
					BinaryExpression expr = Expression.Equal(prop, Expression.Constant(null, typeof(TField?)));
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			case FieldFilterOperations.IsNotNull:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, Name);
					BinaryExpression expr = Expression.NotEqual(prop, Expression.Constant(null, typeof(TField?)));
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			case FieldFilterOperations.In:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, Name);
					//var rhs = Expression.Constant(RightOperands!.ToArray(), typeof(TField?[]));
					ConstantExpression rhs = Expression.Constant(RightOperands!, typeof(IEnumerable<TField?>));
					MethodCallExpression expr = Expression.Call(typeof(Enumerable), "Contains", new Type[] { typeof(TField?) }, rhs, prop);
					return Expression.Lambda<Func<TEntity, bool>>(expr, item);
				}
			case FieldFilterOperations.NotIn:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, Name);
					ConstantExpression rhs = Expression.Constant(RightOperands!, typeof(IEnumerable<TField?>));
					UnaryExpression expr = Expression.Not(
						Expression.Call(typeof(Enumerable), "Contains", new Type[] { typeof(TField?) }, rhs, prop)
					);
					return Expression.Lambda<Func<TEntity, bool>>(expr, item);
				}
			case FieldFilterOperations.Between:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, Name);
					ConstantExpression rhs1 = Expression.Constant(RightOperand, typeof(TField?));
					ConstantExpression rhs2 = Expression.Constant(SecondRightOperand, typeof(TField?));
					BinaryExpression expr = Expression.AndAlso(Expression.GreaterThanOrEqual(prop, rhs1), Expression.LessThanOrEqual(prop, rhs2));
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			case FieldFilterOperations.NotBetween:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, Name);
					ConstantExpression rhs1 = Expression.Constant(RightOperand, typeof(TField?));
					ConstantExpression rhs2 = Expression.Constant(SecondRightOperand, typeof(TField?));
					BinaryExpression expr = Expression.OrElse(Expression.LessThan(prop, rhs1), Expression.GreaterThan(prop, rhs2));
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			case FieldFilterOperations.Like:
				{
					if (typeof(TField?) == typeof(string))
					{
						return BuildStringContainsPredicate<TEntity>(Name!, ((string)(object)RightOperand)?.ToLower());
					}
					throw new InvalidOperationException("The Like and NotLike filter operations are only allowed on strings.");
				}
			case FieldFilterOperations.NotLike:
				{
					if (typeof(TField?) == typeof(string))
					{
						var lowerCaseRightOperand = (string?)(object?)RightOperand;
						if (lowerCaseRightOperand != null)
							lowerCaseRightOperand = lowerCaseRightOperand.ToLower();

						var propertyInfo = typeof(TEntity).GetProperty(Name);
						ParameterExpression pe = Expression.Parameter(typeof(TEntity), "p");
						MemberExpression memberExpression = Expression.MakeMemberAccess(pe, propertyInfo!);

						var toLowerMethodInfo = typeof(string).GetMethod("ToLower", new Type[] { });
						Expression toLowerCall = Expression.Call(memberExpression, toLowerMethodInfo!);

						var containsMethodInfo = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
						ConstantExpression constantExpression = Expression.Constant(lowerCaseRightOperand, typeof(string));

						MethodCallExpression call = Expression.Call(toLowerCall, containsMethodInfo!, constantExpression);
						UnaryExpression expr = Expression.Not(call);

						return Expression.Lambda<Func<TEntity, bool>>(expr, pe);
					}
					throw new InvalidOperationException("The Like and NotLike filter operations are only allowed on strings.");
				}
			case FieldFilterOperations.BitsAnd:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, Name);
					ConstantExpression rhs = Expression.Constant(RightOperand, typeof(TField));
					BinaryExpression expr = Expression.Equal((BinaryExpression)Expression.And(prop, rhs), rhs);
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			case FieldFilterOperations.BitsOr:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, Name);
					ConstantExpression rhs = Expression.Constant(RightOperand, typeof(TField));
					ConstantExpression zero = Expression.Constant(0, typeof(TField));
					BinaryExpression expr = Expression.NotEqual((BinaryExpression)Expression.Or(prop, rhs), zero);
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			default: throw new FormatException($"The {(int)Operation} filter operation is not supported.");
		}
	}

	private static Expression<Func<T, bool>> BuildStringContainsPredicate<T>(string propertyName, string propertyValue)
	{
		var propertyInfo = typeof(T).GetProperty(propertyName);

		ParameterExpression pe = Expression.Parameter(typeof(T), "p");

		MemberExpression memberExpression = Expression.MakeMemberAccess(pe, propertyInfo!);

		var toLowerMethodInfo = typeof(string).GetMethod("ToLower", new Type[] { });
		Expression toLowerCall = Expression.Call(memberExpression, toLowerMethodInfo!);

		var containsMethodInfo = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
		ConstantExpression constantExpression = Expression.Constant(propertyValue, typeof(string));

		Expression call = Expression.Call(toLowerCall, containsMethodInfo!, constantExpression);

		return Expression.Lambda<Func<T, bool>>(call, pe);
	}
}