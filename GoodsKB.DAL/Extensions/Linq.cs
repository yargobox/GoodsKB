namespace GoodsKB.DAL.Extensions.Linq;

internal static class LinqExtensions
{
	internal static IEnumerable<T> Then<T, T1>(this IEnumerable<T> _this, IEnumerable<T1> next) where T1 : T
	{
		foreach (var x in _this) yield return x;
		foreach (var x in next) yield return x;
	}
}