namespace GoodsKB.DAL.Repositories;

using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;

[DebuggerDisplay("{ToString()}")]
public sealed class OrderBy<T>
{
	public record struct Entry(string Name, bool Descending, Expression MemberSelector);

	private int _count;
	private Entry[] _sortOrders;

	public int Count => _count;

	private OrderBy() => _sortOrders = new Entry[5];

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public OrderBy<T> Push<V>(Expression<Func<T, V>> memberSelector, bool descending)
	{
		var name = ((memberSelector as LambdaExpression)?.Body as MemberExpression)?.Member.Name;
		if (name == null || _sortOrders.Take(_count).Any(x => x.Name == name))
		{
			throw new ArgumentException(nameof(memberSelector));
		}

		if (_count >= _sortOrders.Length)
		{
			Array.Resize<Entry>(ref _sortOrders, _count + 5);
		}

		_sortOrders[_count++] = new(name, descending, memberSelector);

		return this;
	}

	public Entry[] SortOrders => _sortOrders.Take(_count).ToArray();

	public override string ToString()
	{
		if (_count == 0) return string.Empty;

		var sb = new StringBuilder();
		foreach (var entry in _sortOrders.Take(_count))
		{
			if (sb.Length > 0) sb.Append(", ");

			sb
				.Append(entry.Name)
				.Append(entry.Descending ? " DESC" : " ASC");

		}
		return sb.ToString();
	}

	public static OrderBy<T> Asc<V>(Expression<Func<T, V>> memberSelector) => new OrderBy<T>().Push<V>(memberSelector, false);
	public static OrderBy<T> Desc<V>(Expression<Func<T, V>> memberSelector) => new OrderBy<T>().Push<V>(memberSelector, true);
}

public static class ExtensionsOfOrderBy
{
	public static OrderBy<T> Asc<T, V>(this OrderBy<T> @this, Expression<Func<T, V>> memberSelector) => @this.Push<V>(memberSelector, false);
	public static OrderBy<T> Desc<T, V>(this OrderBy<T> @this, Expression<Func<T, V>> memberSelector) => @this.Push<V>(memberSelector, true);
}