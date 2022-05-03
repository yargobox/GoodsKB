using System.Reflection;

namespace GoodsKB.DAL.Extensions.Reflection;

internal static class ReflectionExtensions
{
	internal static Type GetUnderlyingSystemType(this PropertyInfo propertyInfo)
	{
		var propertyType = propertyInfo.PropertyType;

		if (propertyType.IsGenericType)
		{
			return propertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ?
				propertyType.GenericTypeArguments[0].UnderlyingSystemType :
				propertyType.UnderlyingSystemType;
		}
		else
		{
			return Nullable.GetUnderlyingType(propertyType)?.UnderlyingSystemType ?? propertyType.UnderlyingSystemType;
		}
	}

	internal static bool IsNullable(this PropertyInfo propertyInfo)
	{
		var propertyType = propertyInfo.PropertyType;

		if (propertyType.IsValueType)
		{
			return Nullable.GetUnderlyingType(propertyType) != null;
		}
		else if (propertyType.IsGenericType)
		{
			return propertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
		}

		var classNullableContextAttribute = propertyInfo?.DeclaringType?.CustomAttributes
			.FirstOrDefault(c => c.AttributeType.Name == "NullableContextAttribute");

		var classNullableContext = classNullableContextAttribute
			?.ConstructorArguments
			.First(ca => ca.ArgumentType.Name == "Byte")
			.Value;

		// EDIT: This logic is not correct for nullable generic types
		var propertyNullableContext = propertyInfo?.CustomAttributes
			.FirstOrDefault(c => c.AttributeType.Name == "NullableAttribute")
			?.ConstructorArguments
			.First(ca => ca.ArgumentType.Name == "Byte")
			.Value;

		// If the property does not have the nullable attribute then it's 
		// nullability is determined by the declaring class 
		propertyNullableContext ??= classNullableContext;

		// If NullableContextAttribute on class is not set and the property
		// does not have the NullableAttribute, then the proeprty is non nullable
		if (propertyNullableContext == null)
		{
			return true;
		}

		// nullableContext == 0 means context is null oblivious (Ex. Pre C#8)
		// nullableContext == 1 means not nullable
		// nullableContext == 2 means nullable
		switch ((byte)propertyNullableContext)
		{
			case 1:// NonNullableContextValue
				return false;
			case 2:// NullableContextValue
				return true;
			default:
				throw new NotSupportedException();
		}
	}

	internal static Type[] GetGenericInterfaceArgs(this Type derived, Type genericInterfaceToTestFor)
	{
		// this conditional is necessary if type can be an interface,
		// because an interface doesn't implement itself: for example,
		// typeof (IList<int>).GetInterfaces () does not contain IList<int>!

		if (derived.IsInterface && derived.IsGenericType && derived.GetGenericTypeDefinition() == genericInterfaceToTestFor)
			return derived.GetGenericArguments();

		foreach (var i in derived.GetInterfaces())
			if (i.IsGenericType && i.GetGenericTypeDefinition() == genericInterfaceToTestFor)
				return i.GetGenericArguments();

		return Array.Empty<Type>();
	}
}

