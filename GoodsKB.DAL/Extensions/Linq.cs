namespace GoodsKB.DAL.Extensions.Linq;

internal static class LinqExtensions
{
	internal static IEnumerable<T> Then<T, T1>(this IEnumerable<T>? _this, IEnumerable<T1>? next) where T1 : T
	{
		if (_this != null) foreach (var x in _this) yield return x;
		if (next != null) foreach (var x in next) yield return x;
	}
}