namespace GoodsKB.DAL.Repositories;

using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

[DebuggerDisplay("{ToString()}")]
public sealed class OrderBy<T>
{
	public record struct Entry(string Name, bool Descending);

	private int _count;
	private Entry[] _sortOrders;

	public int Count => _count;

	private OrderBy() => _sortOrders = new Entry[5];

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public OrderBy<T> Push<V>(Expression<Func<T, V>> memberSelector, bool descending)
	{
		var propName = ((memberSelector as LambdaExpression)?.Body as MemberExpression)?.Member.Name;
		if (propName == null || _sortOrders.Take(_count).Any(x => x.Name == propName))
		{
			throw new ArgumentException(nameof(memberSelector));
		}

		if (_count >= _sortOrders.Length)
		{
			Array.Resize<Entry>(ref _sortOrders, _count + 5);
		}

		_sortOrders[_count++] = new(propName, descending);

		return this;
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public OrderBy<T> Push(Expression memberSelector, bool descending)
	{
		var lambda = (memberSelector as LambdaExpression) ?? throw new ArgumentException(nameof(memberSelector));
		var propName = (lambda.Body as MemberExpression)?.Member.Name ?? throw new ArgumentException(nameof(memberSelector));
		if (	lambda.Parameters.Count != 1 ||
				lambda.Parameters[0].Type != typeof(T) ||
				_sortOrders.Take(_count).Any(x => x.Name == propName))
		{
			throw new ArgumentException(nameof(memberSelector));
		}

		if (_count >= _sortOrders.Length)
		{
			Array.Resize<Entry>(ref _sortOrders, _count + 5);
		}

		_sortOrders[_count++] = new(propName, descending);

		return this;
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public OrderBy<T> Push(PropertyInfo propertyInfo, bool descending)
	{
		if (propertyInfo.DeclaringType != typeof(T))
		{
			throw new InvalidOperationException($"Specified property {propertyInfo.Name} is of a type other than {typeof(T).Name}.");
		}

		if (_count >= _sortOrders.Length)
		{
			Array.Resize<Entry>(ref _sortOrders, _count + 5);
		}

		_sortOrders[_count++] = new(propertyInfo.Name, descending);

		return this;
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public OrderBy<T> Push(string propName, bool descending)
	{
		var propertyInfo = typeof(T).GetProperty(propName) ??
			throw new InvalidOperationException($"{typeof(T).Name} does not have a property {propName}.");

		return Push(propertyInfo, descending);
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
	public static OrderBy<T> Asc(Expression memberSelector) => new OrderBy<T>().Push(memberSelector, false);
	public static OrderBy<T> Desc(Expression memberSelector) => new OrderBy<T>().Push(memberSelector, true);
	public static OrderBy<T> Asc(PropertyInfo propertyInfo) => new OrderBy<T>().Push(propertyInfo, false);
	public static OrderBy<T> Desc(PropertyInfo propertyInfo) => new OrderBy<T>().Push(propertyInfo, true);
	public static OrderBy<T> Asc(string propertyName) => new OrderBy<T>().Push(propertyName, false);
	public static OrderBy<T> Desc(string propertyName) => new OrderBy<T>().Push(propertyName, true);
}

public static class ExtensionsOfOrderBy
{
	public static OrderBy<T> Asc<T, V>(this OrderBy<T> @this, Expression<Func<T, V>> memberSelector) => @this.Push<V>(memberSelector, false);
	public static OrderBy<T> Desc<T, V>(this OrderBy<T> @this, Expression<Func<T, V>> memberSelector) => @this.Push<V>(memberSelector, true);
	public static OrderBy<T> Asc<T>(this OrderBy<T> @this, Expression memberSelector) => @this.Push(memberSelector, false);
	public static OrderBy<T> Desc<T>(this OrderBy<T> @this, Expression memberSelector) => @this.Push(memberSelector, true);
	public static OrderBy<T> Asc<T>(this OrderBy<T> @this, PropertyInfo propertyInfo) => @this.Push(propertyInfo, false);
	public static OrderBy<T> Desc<T>(this OrderBy<T> @this, PropertyInfo propertyInfo) => @this.Push(propertyInfo, true);
	public static OrderBy<T> Asc<T>(this OrderBy<T> @this, string propertyName) => @this.Push(propertyName, false);
	public static OrderBy<T> Desc<T>(this OrderBy<T> @this, string propertyName) => @this.Push(propertyName, true);
}