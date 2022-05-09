namespace GoodsKB.DAL.Repositories.SortOrders;

/// <summary>
/// Sort Order
/// </summary>
[Flags]
public enum SO : int
{
	None = 0,
	Ascending = 1,
	Descending = 2
}

/// <summary>
/// Sort Operations helper
/// </summary>
public static class SOs
{
	/// <summary>
	/// Mask of all operations
	/// </summary>
	public const SO All = SO.Ascending | SO.Descending;

	/// <summary>
	/// Get typical allowed operations for a particular data type
	/// </summary>
	/// <param name="type">System data type</param>
	/// <returns>Allowed operations mask or <c>SO.None</c></returns>
	public static SO GetAllowed(Type type) => _allowedBySystemTypes.Contains(type) ? SOs.All : SO.None;

	/// <summary>
	/// Get typical default operation for a particular data type
	/// </summary>
	/// <param name="type">System data type</param>
	/// <returns>Default operation or <c>SO.None</c></returns>
	public static SO GetDefault(Type type) => _defaultBySystemTypes.Contains(type) ? SO.Ascending : SO.None;

	/// <summary>
	/// Select a typical default operation from the allowed ones
	/// </summary>
	/// <param name="type">Allowed operations mask</param>
	/// <returns>Default operation or <c>SO.None</c></returns>
	public static SO GetDefault(SO allowed)
	{
		if (allowed.HasFlag(SO.Ascending)) return SO.Ascending;
		if (allowed.HasFlag(SO.Descending)) return SO.Descending;
		return SO.None;
	}

	private static readonly HashSet<Type> _allowedBySystemTypes = new()
	{
		typeof(string),
		typeof(char),

		typeof(Guid),

		typeof(byte),
		typeof(sbyte),
		typeof(short),
		typeof(ushort),
		typeof(int),
		typeof(uint),
		typeof(long),
		typeof(ulong),

		typeof(DateTime),
		typeof(DateTimeOffset),
		typeof(DateOnly),
		typeof(TimeOnly),
		typeof(TimeSpan),

		typeof(float),
		typeof(double),
		typeof(decimal)
	};
	private static readonly HashSet<Type> _defaultBySystemTypes = new()
	{
		typeof(string),
		typeof(char),

		typeof(Guid),

		typeof(byte),
		typeof(sbyte),
		typeof(short),
		typeof(ushort),
		typeof(int),
		typeof(uint),
		typeof(long),
		typeof(ulong),

		typeof(DateTime),
		typeof(DateTimeOffset),
		typeof(DateOnly),
		typeof(TimeOnly),
		typeof(TimeSpan),

		typeof(float),
		typeof(double),
		typeof(decimal),
	};
}