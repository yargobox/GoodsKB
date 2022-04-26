using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using GoodsKB.DAL.Repositories;

namespace GoodsKB.BLL.Common;

public static class Extensions
{
	public static string ToPhoneNumber(this string phoneNumber)
	{
		if (phoneNumber == null) throw new ArgumentNullException(nameof(phoneNumber));
		bool plus = phoneNumber.TrimStart().StartsWith('+');
		phoneNumber = new string(phoneNumber.Where(c => Char.IsDigit(c)).ToArray());
		return plus ? '+' + phoneNumber : phoneNumber;
	}

	public static bool TryParseSoftDelMode(string? s, out SoftDelModes softDelMode)
	{
		if (string.IsNullOrEmpty(s))
		{
			softDelMode = SoftDelModes.Actual;
			return true;
		}

		int n;
		if (int.TryParse(s, NumberStyles.Integer, null, out n) && Enum.GetValues<SoftDelModes>().Cast<int>().Contains(n))
		{
			softDelMode = (SoftDelModes)n;
			return true;
		}

		if (Enum.TryParse<SoftDelModes>(s, true, out softDelMode))
		{
			return true;
		}

		return false;
	}

	public static SoftDelModes ParseSoftDelMode(string? s)
	{
		SoftDelModes softDelMode;
		if (TryParseSoftDelMode(s, out softDelMode))
			return softDelMode;

		throw new FormatException($"{s} is not valid SoftDelModes");
	}


	private const byte NonNullableContextValue = 1;
	private const byte NullableContextValue = 2;

	public static Type GetUnderlyingSystemType(this PropertyInfo propertyInfo)
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

	public static bool IsNullable(this PropertyInfo propertyInfo)
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
		switch (propertyNullableContext)
		{
			case NonNullableContextValue:
				return false;
			case NullableContextValue:
				return true;
			default:
				throw new NotSupportedException();
		}
	}
}