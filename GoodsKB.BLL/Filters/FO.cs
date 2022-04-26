using GoodsKB.DAL.Repositories;

namespace GoodsKB.BLL.Services;

public static class FO
{
	public const FilterOperations All =
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
	public const FilterOperations Flags =
		FilterOperations.TrueWhenNull |
		FilterOperations.CaseInsensitive |
		FilterOperations.CaseInsensitiveInvariant;
	public const FilterOperations Equality =
		FilterOperations.Equal |
		FilterOperations.NotEqual;
	public const FilterOperations Arithmetic =
		FilterOperations.Greater |
		FilterOperations.GreaterOrEqual |
		FilterOperations.Less |
		FilterOperations.LessOrEqual |
		FilterOperations.Between |
		FilterOperations.NotBetween;
	public const FilterOperations Nullability =
		FilterOperations.IsNull |
		FilterOperations.IsNotNull;
	public const FilterOperations Likewise =
		FilterOperations.Like |
		FilterOperations.NotLike;
	public const FilterOperations Bitwise =
		FilterOperations.BitsAnd |
		FilterOperations.BitsOr;
	public const FilterOperations Inclusion =
		FilterOperations.In |
		FilterOperations.NotIn;

	public static FilterOperations GetAllowed(Type type)
	{
		FilterOperations fo;
		_allowedFilterOperationsBySystemTypes.TryGetValue(type, out fo);
		return fo;
	}
	public static FilterOperations GetDefault(Type type)
	{
		FilterOperations fo;
		_defaultFilterOperationsBySystemTypes.TryGetValue(type, out fo);
		return fo;
	}
	public static FilterOperations GetDefault(FilterOperations allowed)
	{
		if (allowed.HasFlag(FilterOperations.Equal)) return FilterOperations.Equal;
		if (allowed.HasFlag(FilterOperations.Between)) return FilterOperations.Between;
		if (allowed.HasFlag(FilterOperations.In)) return FilterOperations.In;
		if (allowed.HasFlag(FilterOperations.Like | FilterOperations.CaseInsensitive)) return FilterOperations.Like | FilterOperations.CaseInsensitive;
		if (allowed.HasFlag(FilterOperations.Like | FilterOperations.CaseInsensitiveInvariant)) return FilterOperations.Like | FilterOperations.CaseInsensitiveInvariant;
		if (allowed.HasFlag(FilterOperations.Like)) return FilterOperations.Like;

		if (allowed.HasFlag(FilterOperations.NotEqual)) return FilterOperations.NotEqual;
		if (allowed.HasFlag(FilterOperations.Greater)) return FilterOperations.Greater;
		if (allowed.HasFlag(FilterOperations.GreaterOrEqual)) return FilterOperations.GreaterOrEqual;
		if (allowed.HasFlag(FilterOperations.Less)) return FilterOperations.Less;
		if (allowed.HasFlag(FilterOperations.LessOrEqual)) return FilterOperations.LessOrEqual;
		if (allowed.HasFlag(FilterOperations.IsNull)) return FilterOperations.IsNull;
		if (allowed.HasFlag(FilterOperations.IsNotNull)) return FilterOperations.IsNotNull;
		if (allowed.HasFlag(FilterOperations.NotIn)) return FilterOperations.NotIn;
		if (allowed.HasFlag(FilterOperations.NotBetween)) return FilterOperations.NotBetween;
		if (allowed.HasFlag(FilterOperations.NotLike | FilterOperations.CaseInsensitive)) return FilterOperations.NotLike | FilterOperations.CaseInsensitive;
		if (allowed.HasFlag(FilterOperations.NotLike | FilterOperations.CaseInsensitiveInvariant)) return FilterOperations.NotLike | FilterOperations.CaseInsensitiveInvariant;
		if (allowed.HasFlag(FilterOperations.NotLike)) return FilterOperations.NotLike;
		if (allowed.HasFlag(FilterOperations.BitsAnd)) return FilterOperations.BitsAnd;
		if (allowed.HasFlag(FilterOperations.BitsOr)) return FilterOperations.BitsOr;

		return FilterOperations.None;
	}

	private static readonly Dictionary<Type, FilterOperations> _allowedFilterOperationsBySystemTypes = new ()
	{
		{ typeof(string), Equality | Likewise | Inclusion | FilterOperations.CaseInsensitive | FilterOperations.CaseInsensitiveInvariant },
		{ typeof(char), Equality | Inclusion },

		{ typeof(Guid), Equality | Inclusion },

		{ typeof(byte), Equality | Arithmetic | Inclusion },
		{ typeof(sbyte), Equality | Arithmetic | Inclusion },
		{ typeof(short), Equality | Arithmetic | Inclusion },
		{ typeof(ushort), Equality | Arithmetic | Inclusion },
		{ typeof(int), Equality | Arithmetic | Inclusion },
		{ typeof(uint), Equality | Arithmetic | Inclusion },
		{ typeof(long), Equality | Arithmetic | Inclusion },
		{ typeof(ulong), Equality | Arithmetic | Inclusion },

		{ typeof(DateTime), Equality | Arithmetic | Inclusion },
		{ typeof(DateTimeOffset), Equality | Arithmetic | Inclusion },
		{ typeof(DateOnly), Equality | Arithmetic | Inclusion },
		{ typeof(TimeOnly), Equality | Arithmetic | Inclusion },
		{ typeof(TimeSpan), Equality | Arithmetic | Inclusion },

		{ typeof(float), Equality | Arithmetic },
		{ typeof(double), Equality | Arithmetic },
		{ typeof(decimal), Equality | Arithmetic | Inclusion }
	};
	private static readonly Dictionary<Type, FilterOperations> _defaultFilterOperationsBySystemTypes = new ()
	{
		{ typeof(string), FilterOperations.Like | FilterOperations.CaseInsensitive },
		{ typeof(char), FilterOperations.In },

		{ typeof(Guid), FilterOperations.In },

		{ typeof(byte), FilterOperations.In },
		{ typeof(sbyte), FilterOperations.In },
		{ typeof(short), FilterOperations.In },
		{ typeof(ushort), FilterOperations.In },
		{ typeof(int), FilterOperations.In },
		{ typeof(uint), FilterOperations.In },
		{ typeof(long), FilterOperations.In },
		{ typeof(ulong), FilterOperations.In },

		{ typeof(DateTime), FilterOperations.Between },
		{ typeof(DateTimeOffset), FilterOperations.Between },
		{ typeof(DateOnly), FilterOperations.Between },
		{ typeof(TimeOnly), FilterOperations.Between },
		{ typeof(TimeSpan), FilterOperations.Between },

		{ typeof(float), FilterOperations.Equal },
		{ typeof(double), FilterOperations.Equal },
		{ typeof(decimal), FilterOperations.In }
	};
}