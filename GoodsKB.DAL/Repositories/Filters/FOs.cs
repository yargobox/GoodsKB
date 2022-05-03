namespace GoodsKB.DAL.Repositories.Filters;

/// <summary>
/// Filter Operations helper class
/// </summary>
public static class FOs
{
	/// <summary>
	/// Mask of all operations
	/// </summary>
	public const FO All =
		FO.Equal |
		FO.NotEqual |
		FO.Greater |
		FO.GreaterOrEqual |
		FO.Less |
		FO.LessOrEqual |
		FO.IsNull |
		FO.IsNotNull |
		FO.In |
		FO.NotIn |
		FO.Between |
		FO.NotBetween |
		FO.Like |
		FO.NotLike |
		FO.BitsAnd |
		FO.BitsOr;
	
	/// <summary>
	/// Mask of all flags
	/// </summary>
	public const FO Flags =
		FO.TrueWhenNull |
		FO.CaseInsensitive |
		FO.CaseInsensitiveInvariant;

	/// <summary>
	/// Equality operations Equal and Not Equal
	/// </summary>
	public const FO Equality =
		FO.Equal |
		FO.NotEqual;

	/// <summary>
	/// Relational operations such as Greater than, Less than, or Between
	/// </summary>
	public const FO Relational =
		FO.Greater |
		FO.GreaterOrEqual |
		FO.Less |
		FO.LessOrEqual |
		FO.Between |
		FO.NotBetween;

	/// <summary>
	/// Nullability operations Is Null and Is Not Null
	/// </summary>
	public const FO Nullability =
		FO.IsNull |
		FO.IsNotNull;

	/// <summary>
	/// Likewise string operations Like and Not Like
	/// </summary>
	public const FO Likewise =
		FO.Like |
		FO.NotLike;
	
	/// <summary>
	/// Bitwise operations And and Or
	/// </summary>
	public const FO Bitwise =
		FO.BitsAnd |
		FO.BitsOr;
	
	/// <summary>
	/// Inclusion operations In and Not In
	/// </summary>
	public const FO Inclusion =
		FO.In |
		FO.NotIn;

	/// <summary>
	/// Get typical allowed operations for a particular data type
	/// </summary>
	/// <param name="type">System data type</param>
	/// <returns>Allowed operations mask or <c>FO.None</c></returns>
	public static FO GetAllowed(Type type)
	{
		FO fo;
		_allowedBySystemTypes.TryGetValue(type, out fo);
		return fo;
	}

	/// <summary>
	/// Get typical default operation for a particular data type
	/// </summary>
	/// <param name="type">System data type</param>
	/// <returns>Default operation or <c>FO.None</c></returns>
	public static FO GetDefault(Type type)
	{
		FO fo;
		_defaultBySystemTypes.TryGetValue(type, out fo);
		return fo;
	}

	/// <summary>
	/// Select a typical default operation from the allowed ones
	/// </summary>
	/// <param name="type">Allowed operations mask</param>
	/// <returns>Default operation or <c>FO.None</c></returns>
	public static FO GetDefault(FO allowed)
	{
		if (allowed.HasFlag(FO.Equal)) return FO.Equal;
		if (allowed.HasFlag(FO.Between)) return FO.Between;
		if (allowed.HasFlag(FO.In)) return FO.In;
		if (allowed.HasFlag(FO.Like | FO.CaseInsensitive)) return FO.Like | FO.CaseInsensitive;
		if (allowed.HasFlag(FO.Like | FO.CaseInsensitiveInvariant)) return FO.Like | FO.CaseInsensitiveInvariant;
		if (allowed.HasFlag(FO.Like)) return FO.Like;

		if (allowed.HasFlag(FO.NotEqual)) return FO.NotEqual;
		if (allowed.HasFlag(FO.Greater)) return FO.Greater;
		if (allowed.HasFlag(FO.GreaterOrEqual)) return FO.GreaterOrEqual;
		if (allowed.HasFlag(FO.Less)) return FO.Less;
		if (allowed.HasFlag(FO.LessOrEqual)) return FO.LessOrEqual;
		if (allowed.HasFlag(FO.IsNull)) return FO.IsNull;
		if (allowed.HasFlag(FO.IsNotNull)) return FO.IsNotNull;
		if (allowed.HasFlag(FO.NotIn)) return FO.NotIn;
		if (allowed.HasFlag(FO.NotBetween)) return FO.NotBetween;
		if (allowed.HasFlag(FO.NotLike | FO.CaseInsensitive)) return FO.NotLike | FO.CaseInsensitive;
		if (allowed.HasFlag(FO.NotLike | FO.CaseInsensitiveInvariant)) return FO.NotLike | FO.CaseInsensitiveInvariant;
		if (allowed.HasFlag(FO.NotLike)) return FO.NotLike;
		if (allowed.HasFlag(FO.BitsAnd)) return FO.BitsAnd;
		if (allowed.HasFlag(FO.BitsOr)) return FO.BitsOr;

		return FO.None;
	}

	private static readonly Dictionary<Type, FO> _allowedBySystemTypes = new()
	{
		{ typeof(string), Equality | Likewise | Inclusion | FO.CaseInsensitive | FO.CaseInsensitiveInvariant },
		{ typeof(char), Equality | Inclusion },

		{ typeof(Guid), Equality | Inclusion },

		{ typeof(byte), Equality | Relational | Inclusion },
		{ typeof(sbyte), Equality | Relational | Inclusion },
		{ typeof(short), Equality | Relational | Inclusion },
		{ typeof(ushort), Equality | Relational | Inclusion },
		{ typeof(int), Equality | Relational | Inclusion },
		{ typeof(uint), Equality | Relational | Inclusion },
		{ typeof(long), Equality | Relational | Inclusion },
		{ typeof(ulong), Equality | Relational | Inclusion },

		{ typeof(DateTime), Equality | Relational | Inclusion },
		{ typeof(DateTimeOffset), Equality | Relational | Inclusion },
		{ typeof(DateOnly), Equality | Relational | Inclusion },
		{ typeof(TimeOnly), Equality | Relational | Inclusion },
		{ typeof(TimeSpan), Equality | Relational | Inclusion },

		{ typeof(float), Equality | Relational },
		{ typeof(double), Equality | Relational },
		{ typeof(decimal), Equality | Relational | Inclusion }
	};
	private static readonly Dictionary<Type, FO> _defaultBySystemTypes = new()
	{
		{ typeof(string), FO.Like | FO.CaseInsensitive },
		{ typeof(char), FO.In },

		{ typeof(Guid), FO.In },

		{ typeof(byte), FO.In },
		{ typeof(sbyte), FO.In },
		{ typeof(short), FO.In },
		{ typeof(ushort), FO.In },
		{ typeof(int), FO.In },
		{ typeof(uint), FO.In },
		{ typeof(long), FO.In },
		{ typeof(ulong), FO.In },

		{ typeof(DateTime), FO.Between },
		{ typeof(DateTimeOffset), FO.Between },
		{ typeof(DateOnly), FO.Between },
		{ typeof(TimeOnly), FO.Between },
		{ typeof(TimeSpan), FO.Between },

		{ typeof(float), FO.Equal },
		{ typeof(double), FO.Equal },
		{ typeof(decimal), FO.In }
	};
}