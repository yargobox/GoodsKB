using System.Linq.Expressions;

namespace GoodsKB.DAL.Repositories;

public class FieldFilter<TEntity, TField> : IFieldFilter<TEntity>
	where TEntity : class
{
	public FieldFilterOperations Operation { get; }
	public Type OperandType { get; }
	public string Name { get; }
	public Expression<Func<TEntity, bool>> Condition { get; }

	public FieldFilter(FieldFilterOperations operation, Expression<Func<TEntity, TField?>> leftOperand)
		: this(operation, ((MemberExpression) leftOperand.Body).Member.Name) { }
	public FieldFilter(FieldFilterOperations operation, string name)
	{
		(int operandCount, bool isManyRightOperands) = GetOperandCount(operation);
		if (isManyRightOperands || operandCount != 1)
		{
			throw new InvalidOperationException($"The number of operands for the {operation.ToString()} filter by field must be {operandCount}.");
		}

		Operation = operation;
		Name = name;
		OperandType = typeof(TField);
		Condition = MakeOperator(default(TField?), default(TField?), null);
	}
	public FieldFilter(FieldFilterOperations operation, Expression<Func<TEntity, TField?>> leftOperand, TField? rightOperand)
		: this(operation, ((MemberExpression) leftOperand.Body).Member.Name, rightOperand) { }
	public FieldFilter(FieldFilterOperations operation, string name, TField? rightOperand)
	{
		(int operandCount, bool isManyRightOperands) = GetOperandCount(operation);
		if (isManyRightOperands || operandCount != 2)
		{
			throw new InvalidOperationException($"The number of operands for the {operation.ToString()} filter by field must be {operandCount}.");
		}

		Operation = operation;
		Name = name;
		OperandType = typeof(TField);
		Condition = MakeOperator(rightOperand, default(TField?), null);
	}
	public FieldFilter(FieldFilterOperations operation, Expression<Func<TEntity, TField?>> leftOperand, TField? rightOperand, TField? rightSecondOperand)
		: this(operation, ((MemberExpression) leftOperand.Body).Member.Name, rightOperand, rightSecondOperand) { }
	public FieldFilter(FieldFilterOperations operation, string name, TField? rightOperand, TField? secondRightOperand)
	{
		(int operandCount, bool isManyRightOperands) = GetOperandCount(operation);
		if (isManyRightOperands || operandCount != 3)
		{
			throw new InvalidOperationException($"The number of operands for the {operation.ToString()} filter by field must be {operandCount}.");
		}

		Operation = operation;
		Name = name;
		OperandType = typeof(TField);
		Condition = MakeOperator(rightOperand, secondRightOperand, null);
	}
	public FieldFilter(FieldFilterOperations operation, Expression<Func<TEntity, TField?>> leftOperand, IEnumerable<TField?> rightOperands)
		: this(operation, ((MemberExpression) leftOperand.Body).Member.Name, rightOperands) { }
	public FieldFilter(FieldFilterOperations operation, string name, IEnumerable<TField?> rightOperands)
	{
		(int operandCount, bool isManyRightOperands) = GetOperandCount(operation);
		if (!isManyRightOperands)
		{
			throw new InvalidOperationException($"The number of operands for the {operation.ToString()} filter by field must be {operandCount}.");
		}

		Operation = operation;
		Name = name;
		OperandType = typeof(TField);
		Condition = MakeOperator(default(TField?), default(TField?), rightOperands);
	}

	public static (int operandCount, bool isManyRightOperands) GetOperandCount(FieldFilterOperations operation)
		=> (operation & ~FieldFilterOperations.Flags) switch
		{
			FieldFilterOperations.Equal => (2, false),
			FieldFilterOperations.NotEqual => (2, false),
			FieldFilterOperations.Greater => (2, false),
			FieldFilterOperations.GreaterOrEqual => (2, false),
			FieldFilterOperations.Less => (2, false),
			FieldFilterOperations.LessOrEqual => (2, false),
			FieldFilterOperations.IsNull => (1, false),
			FieldFilterOperations.IsNotNull => (1, false),
			FieldFilterOperations.In => (1, true),
			FieldFilterOperations.NotIn => (1, true),
			FieldFilterOperations.Between => (3, false),
			FieldFilterOperations.NotBetween => (3, false),
			FieldFilterOperations.Like => (2, false),
			FieldFilterOperations.NotLike => (2, false),
			FieldFilterOperations.BitsAnd => (2, false),
			FieldFilterOperations.BitsOr => (2, false),
			_ => throw new NotSupportedException($"The {operation.ToString()} filter by field operation is not supported.")
		};

	public Expression<Func<TEntity, bool>> MakeOperator(TField? RightOperand, TField? SecondRightOperand, IEnumerable<TField?>? RightOperands)
	{
		switch (Operation & ~FieldFilterOperations.Flags)
		{
			case FieldFilterOperations.Equal:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, Name);
					ConstantExpression rhs = Expression.Constant(RightOperand, typeof(TField));
					BinaryExpression expr = Expression.Equal(prop, rhs);
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
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
			case FieldFilterOperations.NotLike:
				throw new NotImplementedException();
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
}