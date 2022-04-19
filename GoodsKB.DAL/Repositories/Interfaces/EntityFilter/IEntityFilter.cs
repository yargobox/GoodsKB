using System.Linq.Expressions;
using System.Reflection;

namespace GoodsKB.DAL.Repositories;

public enum FilterOperations
{
	Equal = 0,
	NotEqual = 1,
	Greater = 2,
	GreaterOrEqual = 3,
	Less = 4,
	LessOrEqual = 5,
	IsNull = 6,
	IsNotNull = 7,
	In = 8,
	NotIn = 9,
	Between = 10,
	NotBetween = 11,
	Like = 12,
	NotLike = 13,
	BitsAnd = 14,
	BitsOr = 15,

	TrueOnNull = 0x100000,

	Flags = TrueOnNull
}

public interface IEntityFilter<TEntity>
	where TEntity : class
{
	Expression<Func<TEntity, bool>> Filter { get; }
}

public sealed class FieldFilter<TEntity, TField>
{
	public FilterOperations FilterOperation { get; }
	public Type OperandType { get; }
	public int OperandCount { get; }
	public Expression<Func<TEntity, TField?>> LeftOperand { get; }
	public TField? RightOperand { get; }
	public TField? SecondRightOperand { get; }
	public IEnumerable<TField?>? RightOperands { get; }
	public Expression<Func<TEntity, bool>> Condition { get; }

	public FieldFilter(FilterOperations filterOperation, Expression<Func<TEntity, TField?>> leftOperand)
	{
		(int operandCount, bool isManyRightOperands) = GetOperandCount(filterOperation);
		if (isManyRightOperands || operandCount != 1)
		{
			throw new InvalidOperationException($"The number of operands for the {filterOperation.ToString()} filter by field must be {operandCount}.");
		}

		FilterOperation = filterOperation;
		OperandType = typeof(TField);
		OperandCount = operandCount;
		LeftOperand = leftOperand;
		Condition = MakeOperator();
	}
	public FieldFilter(FilterOperations filterOperation, Expression<Func<TEntity, TField?>> leftOperand, TField? rightOperand)
	{
		(int operandCount, bool isManyRightOperands) = GetOperandCount(filterOperation);
		if (isManyRightOperands || operandCount != 2)
		{
			throw new InvalidOperationException($"The number of operands for the {filterOperation.ToString()} filter by field must be {operandCount}.");
		}

		FilterOperation = filterOperation;
		OperandType = typeof(TField);
		OperandCount = operandCount;
		LeftOperand = leftOperand;
		RightOperand = rightOperand;
		Condition = MakeOperator();
	}
	public FieldFilter(FilterOperations filterOperation, Expression<Func<TEntity, TField?>> leftOperand, TField? rightOperand, TField? rightSecondOperand)
	{
		(int operandCount, bool isManyRightOperands) = GetOperandCount(filterOperation);
		if (isManyRightOperands || operandCount != 3)
		{
			throw new InvalidOperationException($"The number of operands for the {filterOperation.ToString()} filter by field must be {operandCount}.");
		}

		FilterOperation = filterOperation;
		OperandType = typeof(TField);
		OperandCount = operandCount;
		LeftOperand = leftOperand;
		RightOperand = rightOperand;
		SecondRightOperand = rightSecondOperand;
		Condition = MakeOperator();
	}
	public FieldFilter(FilterOperations filterOperation, Expression<Func<TEntity, TField?>> leftOperand, IEnumerable<TField?> rightOperands)
	{
		(int operandCount, bool isManyRightOperands) = GetOperandCount(filterOperation);
		if (!isManyRightOperands)
		{
			throw new InvalidOperationException($"The number of operands for the {filterOperation.ToString()} filter by field must be {operandCount}.");
		}

		FilterOperation = filterOperation;
		OperandType = typeof(TField);
		OperandCount = operandCount;
		LeftOperand = leftOperand;
		RightOperands = rightOperands;
		Condition = MakeOperator();
	}

	public static (int operandCount, bool isManyRightOperands) GetOperandCount(FilterOperations filterOperation)
		=> (filterOperation & ~FilterOperations.Flags) switch
		{
			FilterOperations.Equal => (2, false),
			FilterOperations.NotEqual => (2, false),
			FilterOperations.Greater => (2, false),
			FilterOperations.GreaterOrEqual => (2, false),
			FilterOperations.Less => (2, false),
			FilterOperations.LessOrEqual => (2, false),
			FilterOperations.IsNull => (1, false),
			FilterOperations.IsNotNull => (1, false),
			FilterOperations.In => (1, true),
			FilterOperations.NotIn => (1, true),
			FilterOperations.Between => (3, false),
			FilterOperations.NotBetween => (3, false),
			FilterOperations.Like => (2, false),
			FilterOperations.NotLike => (2, false),
			FilterOperations.BitsAnd => (2, false),
			FilterOperations.BitsOr => (2, false),
			_ => throw new NotSupportedException($"The {filterOperation.ToString()} filter by field operation is not supported.")
		};

	public Expression<Func<TEntity, bool>> MakeOperator()
	{
		var leftPropName = ((MemberExpression) LeftOperand.Body).Member.Name;

		switch (FilterOperation & ~FilterOperations.Flags)
		{
			case FilterOperations.Equal:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, leftPropName);
					ConstantExpression rhs = Expression.Constant(RightOperand, typeof(TField));
					BinaryExpression expr = Expression.Equal(prop, rhs);
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			case FilterOperations.NotEqual:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, leftPropName);
					ConstantExpression rhs = Expression.Constant(RightOperand, typeof(TField));
					BinaryExpression expr = Expression.NotEqual(prop, rhs);
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			case FilterOperations.Greater:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, leftPropName);
					ConstantExpression rhs = Expression.Constant(RightOperand, typeof(TField));
					BinaryExpression expr = Expression.GreaterThan(prop, rhs);
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			case FilterOperations.GreaterOrEqual:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, leftPropName);
					ConstantExpression rhs = Expression.Constant(RightOperand, typeof(TField));
					BinaryExpression expr = Expression.GreaterThanOrEqual(prop, rhs);
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			case FilterOperations.Less:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, leftPropName);
					ConstantExpression rhs = Expression.Constant(RightOperand, typeof(TField));
					BinaryExpression expr = Expression.LessThan(prop, rhs);
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			case FilterOperations.LessOrEqual:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, leftPropName);
					ConstantExpression rhs = Expression.Constant(RightOperand, typeof(TField));
					BinaryExpression expr = Expression.LessThanOrEqual(prop, rhs);
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			case FilterOperations.IsNull:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, leftPropName);
					BinaryExpression expr = Expression.Equal(prop, Expression.Constant(null, typeof(TField?)));
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			case FilterOperations.IsNotNull:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, leftPropName);
					BinaryExpression expr = Expression.NotEqual(prop, Expression.Constant(null, typeof(TField?)));
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			case FilterOperations.In:
				{
					/* 	var ps = Expression.Parameter(typeof(int), "s");
						var pt = Expression.Parameter(typeof(int), "t");
						var ex2 = Expression.Lambda(
							Expression.Quote(
								Expression.Lambda(
									Expression.Add(ps, pt),
								pt)),
							ps);

						var f2a = (Func<int, Expression<Func<int, int>>>)ex2.Compile();
						var f2b = f2a(200).Compile();
						Console.WriteLine(f2b(123)); */

					/* ParameterExpression paramItem = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression memberItemProp = Expression.PropertyOrField(paramItem, leftPropName);
					ParameterExpression paramGetProp = Expression.Parameter(typeof(Func<TEntity, TField?>), "getProp");
					ParameterExpression paramProp = Expression.Parameter(typeof(TField?), "prop");
					ParameterExpression paramCol = Expression.Parameter(typeof(IEnumerable<TField?>), "col");
					// Expression<Func<IEnumerable<TField?>, Func<TEntity, TField?>, TEntity, bool>> contains =
					// bool (IEnumerable<TField?> col, Func<TEntity, TField?> getProp, TEntity item) => col.Contains(getProp(item));
					var methodIn = typeof(FieldFilter<TEntity, TField>).GetMethod("In", BindingFlags.NonPublic | BindingFlags.Static, new Type[] { typeof(IEnumerable<TField?>), typeof(Func<TEntity, TField?>), typeof(TEntity) });
					MethodCallExpression callContains = Expression.Call(
						methodIn !,
						paramCol, paramGetProp, paramItem
					);
					var exprInner = Expression.Lambda(
						Expression.Quote(
							Expression.Lambda(
								callContains,
								paramCol,
								paramGetProp,
								paramItem
							)
						),
						paramCol,
						paramGetProp
					);
					//var exprOuter = (Func<TEntity, Expression<Func<IEnumerable<TField?>, Func<TEntity, TField?>, TEntity, bool>>>) exprInner.Compile();
					var expr2 = exprInner.Compile();
					var expr = (Func<IEnumerable<TField?>, Func<TEntity, TField?>, Expression<Func<IEnumerable<TField?>, Func<TEntity, TField?>, TEntity, bool>>>) expr2 !;
					var res = expr(RightOperands !, LeftOperand.Compile() !);
					return res; */

					/* var methodInfo = typeof(FieldFilter<TEntity, TField>).GetMethod("In", BindingFlags.NonPublic | BindingFlags.Static);
					var lambdaExpression = Expression.Lambda<Func<bool>>(Expression.Constant(true));
					var methodCallExpression = Expression.Call(null, methodInfo!, Expression.Constant(lambdaExpression));
					var wrapperLambda = Expression.Lambda(methodCallExpression);
					wrapperLambda.Compile(); */

					/* var pItem = Expression.Parameter(typeof(TEntity), "item");
					var mItemProp = Expression.PropertyOrField(pItem, leftPropName);
					var pGetProp = Expression.Parameter(typeof(Func<TEntity, TField?>), "getProp");
					var pProp = Expression.Parameter(typeof(TField?), "prop");
					var pCol = Expression.Parameter(typeof(IEnumerable<TField?>), "col");
					var mIn = typeof(FieldFilter<TEntity, TField>).GetMethod("In", BindingFlags.NonPublic | BindingFlags.Static);
					var callContains = Expression.Call(
						null,
						mIn !,
						pCol, mItemProp
					);
					
					return res;  */

					var parameterExp = Expression.Parameter(typeof(FieldFilter<TEntity, TField>), "type");
					var propertyExp = Expression.PropertyOrField(parameterExp, nameof(RightOperands));

					var parameterExp = Expression.Parameter(typeof(TEntity), "type");
					var propertyExp = Expression.PropertyOrField(parameterExp, leftPropName);
					//MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(TField?) });
					var method = typeof(Enumerable).GetMethods()
										.Where(m => m.Name == "Contains")
										.Single(m => m.GetParameters().Length == 2)
										.MakeGenericMethod(typeof(TField?));
					var someValue = Expression.Constant(propertyValue, typeof(TField?));
					var containsMethodExp = Expression.Call(propertyExp, method!, someValue);

					return Expression.Lambda<Func<TEntity, bool>>(containsMethodExp, parameterExp);
				}
			case FilterOperations.NotIn:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, leftPropName);
					Expression<Func<IEnumerable<TField?>>> rhs = () => (IEnumerable<TField?>) RightOperands !;
					BinaryExpression expr = Expression.Equal(
						Expression.Call(
							rhs,
							typeof(IEnumerable<TField?>).GetMethod("Contains", new[] { typeof(TField?) })!,
							prop
						),
						Expression.Constant(false)
					);
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			case FilterOperations.Between:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, leftPropName);
					ConstantExpression rhs1 = Expression.Constant(RightOperand, typeof(TField?));
					ConstantExpression rhs2 = Expression.Constant(SecondRightOperand, typeof(TField?));
					BinaryExpression expr = Expression.AndAlso(Expression.GreaterThanOrEqual(prop, rhs1), Expression.LessThanOrEqual(prop, rhs2));
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			case FilterOperations.NotBetween:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, leftPropName);
					ConstantExpression rhs1 = Expression.Constant(RightOperand, typeof(TField?));
					ConstantExpression rhs2 = Expression.Constant(SecondRightOperand, typeof(TField?));
					BinaryExpression expr = Expression.OrElse(Expression.LessThan(prop, rhs1), Expression.GreaterThan(prop, rhs2));
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			/* case FilterOperations.Like: return (2, false);
			case FilterOperations.NotLike: return (2, false); */
			case FilterOperations.BitsAnd:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, leftPropName);
					ConstantExpression rhs = Expression.Constant(RightOperand, typeof(TField));
					BinaryExpression expr = Expression.Equal((BinaryExpression)Expression.And(prop, rhs), rhs);
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			case FilterOperations.BitsOr:
				{
					ParameterExpression item = Expression.Parameter(typeof(TEntity), "item");
					MemberExpression prop = Expression.PropertyOrField(item, leftPropName);
					ConstantExpression rhs = Expression.Constant(RightOperand, typeof(TField));
					ConstantExpression zero = Expression.Constant(0, typeof(TField));
					BinaryExpression expr = Expression.NotEqual((BinaryExpression)Expression.Or(prop, rhs), zero);
					return Expression.Lambda<Func<TEntity, bool>>(expr, new ParameterExpression[] { item });
				}
			default: throw new FormatException($"The {(int)FilterOperation} filter operation is not supported.");
		}
	}

	private static bool In(IEnumerable<TField?> col, TField? prop)
	{
		return col.Contains(prop);
	}

	private static Expression<Func<TEntity, bool>> GetContainsExpression<T>(string propertyName, TField? propertyValue)
	{
		var parameterExp = Expression.Parameter(typeof(TEntity), "type");
		var propertyExp = Expression.PropertyOrField(parameterExp, propertyName);
		//MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(TField?) });
		var method = typeof(Enumerable).GetMethods()
							.Where(m => m.Name == "Contains")
							.Single(m => m.GetParameters().Length == 2)
							.MakeGenericMethod(typeof(TField?));
		var someValue = Expression.Constant(propertyValue, typeof(TField?));
		var containsMethodExp = Expression.Call(propertyExp, method!, someValue);

		return Expression.Lambda<Func<TEntity, bool>>(containsMethodExp, parameterExp);
	}
}