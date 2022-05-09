namespace GoodsKB.DAL.Repositories.SortOrders;
using System.Reflection;
using GoodsKB.DAL.Extensions.Reflection;
using GoodsKB.DAL.Generic;

internal sealed partial class SortOrderConditionBuilder<TEntity>
{
	private static class DescHelper<TDto> where TDto : notnull
	{
		public static IReadOnlyDictionary<string, SortOrderDesc> SortOrders { get; }

		static DescHelper()
		{
			var entries = typeof(TDto)
				.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty)
				.Where(x => x.IsDefined(typeof(SortOrderAttribute), false))
				.Select(x => (
					dtoProp: x,
					entityProp: typeof(TEntity).GetProperty(x.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty) ??
						throw new InvalidOperationException($"{typeof(TEntity).Name} does not have a property {x.Name}."),
					attr: x.GetCustomAttribute<SortOrderAttribute>(false) ??
						throw new InvalidOperationException($"{typeof(TEntity).Name}.{x.Name} does not have an attribute {nameof(SortOrderAttribute)}.")
				))
				.ToArray();

			var orders = new OrderedDictionary<string, SortOrderDesc>(entries.Length, StringComparer.InvariantCultureIgnoreCase);

			foreach (var entry in entries)
			{
				var propType = entry.entityProp.PropertyType;

				if (!propType.IsAssignableFrom(entry.dtoProp.PropertyType))
				{
					throw new InvalidOperationException($"{typeof(TEntity).Name}.{entry.entityProp.Name} is not compatible with {typeof(TDto).Name}.{entry.entityProp.Name}.");
				}

				var initials = entry.attr.GetInitialValues();
				var underType = entry.entityProp.GetUnderlyingSystemType();

				SO allowed;
				if (initials.allowed == null)
				{
					allowed = underType.IsEnum ? SOs.All : SOs.GetAllowed(underType);
				}
				else
				{
					allowed = initials.allowed.Value;
				}

				if ((allowed & SOs.All) == 0)
				{
					throw new InvalidOperationException($"No sort operation found for the {typeof(TDto).Name}.{entry.dtoProp.Name} property.");
				}

				SO defop = initials.defaultOperation ?? SO.None;

				orders.Add(
					entry.entityProp.Name,
					new SortOrderDesc(entry.entityProp.Name, propType, underType)
					{
						Allowed = allowed,
						Default = defop
					}
				);
			}

			orders.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.InvariantCultureIgnoreCase));

			SortOrders = orders;
		}
	}
}