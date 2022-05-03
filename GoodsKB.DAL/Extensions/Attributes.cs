namespace GoodsKB.DAL.Extensions.Attributes;

internal static class AttributesExtensions
{
	internal static bool HasAttribute<TAttribute>(this Type type)
		where TAttribute : Attribute
	{
		var attr = type.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;
		return attr != null;
	}

	internal static TAttribute? GetAttribute<TAttribute>(this Type type)
		where TAttribute : Attribute
	{
		return type.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;
	}

	internal static TValue? GetAttributeValue<TAttribute, TValue>(this Type type, Func<TAttribute, TValue> valueSelector)
		where TAttribute : Attribute
	{
		var attr = type.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;
		return attr != null ? valueSelector(attr) : default(TValue);
	}
}