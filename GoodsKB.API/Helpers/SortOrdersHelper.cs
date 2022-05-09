namespace GoodsKB.API.SortOrders;
using System.Diagnostics.CodeAnalysis;
using GoodsKB.API.Extensions.Text;
using GoodsKB.DAL.Repositories.SortOrders;

internal static class SortOrdersHelper
{
	[return: NotNullIfNotNull("values")]
	public static string? SerializeToString(SortOrderValues? values)
	{
		if (values == null) return null;

		var orders = new List<KeyValuePair<string, SO>>();
		foreach (var p in values.Values.Where(x => x.Operation == SO.Ascending || x.Operation == SO.Descending))
		{
			SortOrderDesc? sod;
			if (!values.SortOrders.TryGetValue(p.PropertyName, out sod))
			{
				throw new InvalidOperationException($"SortOrder {p.PropertyName} is not found.");
			}
			if ((sod.Allowed & p.Operation) != p.Operation)
			{
				throw new InvalidOperationException($"SortOrder {sod.PropertyName} does not support this operation or option.");
			}

			orders.Add(new KeyValuePair<string, SO>(sod.PropertyName, p.Operation));
		}

		return orders
			.Select(x => x.Value == SO.Ascending ? x.Key : x.Key + ",2")
			.EscapeAndJoin(';', '\\');
	}

	public static SortOrderValues? SerializeFromString(IReadOnlyDictionary<string, SortOrderDesc> sortOrders, string? sort)
	{
		if (string.IsNullOrWhiteSpace(sort)) return null;

		var orderStringValues = sort.UnescapeAndSplit(';', StringSplitOptions.RemoveEmptyEntries, '\\');
		var ordersValues = new SortOrderValue[orderStringValues.Length];
		int i = 0;
		foreach (var s in orderStringValues)
		{
			string orderName;
			SO operation;
			if (s.EndsWith(",2"))
			{
				orderName = s.Substring(0, s.Length - 2);
				operation = SO.Descending;
			}
			else if (s.EndsWith(",1"))
			{
				orderName = s.Substring(0, s.Length - 2);
				operation = SO.Ascending;
			}
			else
			{
				orderName = s;
				operation = SO.Ascending;
			}

			SortOrderDesc? fd;
			if (!sortOrders.TryGetValue(orderName, out fd))
			{
				throw new InvalidOperationException($"SortOrder {orderName} is not found.");
			}

			if ((fd.Allowed & operation) != operation)
			{
				throw new InvalidOperationException($"SortOrder {fd.PropertyName} does not support this operation or option.");
			}

			ordersValues[i++] = new SortOrderValue(fd.PropertyName)
			{
				Operation = operation
			};
		}

		return new SortOrderValues(sortOrders, ordersValues);
	}
}