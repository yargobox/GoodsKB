namespace GoodsKB.DAL.Common;

internal static class Extensions
{
	public static Type[] GetGenericInterfaceArgs(this Type derived, Type genericInterfaceToTestFor)
	{
		// this conditional is necessary if type can be an interface,
		// because an interface doesn't implement itself: for example,
		// typeof (IList<int>).GetInterfaces () does not contain IList<int>!

		if (derived.IsInterface && derived.IsGenericType &&  derived.GetGenericTypeDefinition() == genericInterfaceToTestFor)
			return derived.GetGenericArguments();

		foreach (var i in derived.GetInterfaces())
			if (i.IsGenericType && i.GetGenericTypeDefinition() == genericInterfaceToTestFor)
				return i.GetGenericArguments();

		return Array.Empty<Type>();
	}
}