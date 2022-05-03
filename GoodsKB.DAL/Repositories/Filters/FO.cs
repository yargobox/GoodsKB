namespace GoodsKB.DAL.Repositories.Filters;

/// <summary>
/// Filter Operation
/// </summary>
[Flags]
public enum FO : int
{
	None = 0,
	Equal = 1,
	NotEqual = 2,
	Greater = 4,
	GreaterOrEqual = 8,
	Less = 0x10,
	LessOrEqual = 0x20,
	IsNull = 0x40,
	IsNotNull = 0x80,
	In = 0x100,
	NotIn = 0x200,
	Between = 0x400,
	NotBetween = 0x800,
	Like = 0x1000,
	NotLike = 0x2000,
	BitsAnd = 0x4000,
	BitsOr = 0x8000,

	TrueWhenNull = 0x100000,
	CaseInsensitive = 0x200000,
	CaseInsensitiveInvariant = 0x400000
}