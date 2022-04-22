using System.Linq.Expressions;

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

	TrueWhenNull = 0x100000,
	CaseInsensitive = 0x200000,
	CaseInsensitiveInvariant = 0x400000,

	Flags = TrueWhenNull | CaseInsensitive | CaseInsensitiveInvariant
}

public interface IFieldFilter<TEntity>
	where TEntity : class
{
	public FilterOperations Operation { get; }
	public string Name { get; }
	public Type OperandType { get; }
	Expression<Func<TEntity, bool>> Condition { get; }
}