using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using GoodsKB.BLL.Common;
using GoodsKB.DAL.Repositories;

namespace GoodsKB.BLL.Services;

public static class FiltersHelper<TEntity>
	where TEntity : class
{
	private static class DefinitionHelper<TDto>
	{
		public static ReadOnlyDictionary<string, FilterDefinition> Definitions { get; }

		static DefinitionHelper()
		{
			var filters = new Dictionary<string, FilterDefinition>(StringComparer.OrdinalIgnoreCase);

			foreach (var p in typeof(TDto)
					// public non-static SET properties
					.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty)
					// with given attribute only
					.Where(x => Attribute.IsDefined(x, typeof(UserFilterAttribute), true))
					// return propSet and attr
					.Select(x => (
						entityProp: typeof(TEntity).GetProperty(x.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty) ??
							throw new InvalidOperationException(@$"""{typeof(TEntity).Name}"" does not have property ""{x.Name}""."),
						dtoProp: x,
						attr: x.GetCustomAttribute<UserFilterAttribute>(true)!)
					)
			)
			{
				var propType = p.entityProp.PropertyType;

				if (!propType.IsAssignableFrom(p.dtoProp.PropertyType))
				{
					throw new InvalidOperationException($"{typeof(TEntity).Name}.{p.entityProp.Name} is not compatible with {typeof(TDto).Name}.{p.entityProp.Name}.");
				}

				var initials = p.attr.GetInitialValues();
				var isNullAllowed = p.entityProp.IsNullable() && p.dtoProp.IsNullable();
				var underType = p.entityProp.GetUnderlyingSystemType();

				if (initials.isNullAllowed != null)
				{
					// The user can require a non-null filter value for nullable fields, but not vice versa.
					isNullAllowed = isNullAllowed && (bool)initials.isNullAllowed;
				}

				FilterOperations allowed;
				if (initials.allowed == null)
				{
					if (underType.IsEnum)
					{
						allowed = FO.Equality | FO.Inclusion;
						if (underType.IsDefined(typeof(FlagsAttribute)))
						{
							allowed |= FO.Bitwise;
						}
					}
					else
					{
						allowed = FO.GetAllowed(underType);
					}

					if (isNullAllowed)
					{
						allowed |= FO.Nullability | FilterOperations.TrueWhenNull;
					}
				}
				else
				{
					allowed = (FilterOperations)initials.allowed;
					if (!isNullAllowed)
					{
						// The user is not allowed to perform nullable operations on non-nullable fields.
						allowed &= ~FilterOperations.TrueWhenNull;
					}
				}

				if ((allowed & FO.All) == FilterOperations.None)
				{
					throw new InvalidOperationException($"No filter operation found for field {typeof(TDto).Name}.{p.dtoProp.Name}.");
				}

				FilterOperations defop;
				if (initials.defaultOperation == null)
				{
					defop = FO.GetDefault(underType) & allowed;
				}
				else
				{
					defop = (FilterOperations)initials.defaultOperation & allowed;
				}
				if (defop == FilterOperations.None)
				{
					defop = FO.GetDefault(allowed);
				}

				bool emptyToNull;
				if (initials.emptyToNull == null)
				{
					emptyToNull = isNullAllowed && underType == typeof(string);
				}
				else
				{
					emptyToNull = isNullAllowed && (bool)initials.emptyToNull;
				}

				var itemProperty = Expression.Parameter(p.entityProp.DeclaringType!, "item");
				var propMember = Expression.PropertyOrField(itemProperty, p.entityProp.Name);
				var memberSelector = Expression.Lambda(propMember, itemProperty);

				filters.Add(p.entityProp.Name,
					new FilterDefinition(p.entityProp.Name, propType, underType, memberSelector)
					{
						Allowed = allowed,
						IsNullAllowed = isNullAllowed,
						EmptyToNull = emptyToNull,
						Default = defop
					});
			}

			Definitions = new ReadOnlyDictionary<string, FilterDefinition>(filters);
		}
	}

	static FiltersHelper()
	{
		_dotNumberFormatInvariantCulture = (CultureInfo) CultureInfo.InvariantCulture.Clone();
		_dotNumberFormatInvariantCulture.NumberFormat.CurrencyDecimalSeparator = ".";
	}

	public static Expression BuildConditionPredicate(FilterValues values)
	{
		int total = 0;
		var predicates = new Expression[values.Values.Count()];

		foreach (var item in values.Values)
		{
			var def = values.Definitions[item.Name];
			var info = FieldPredicate<TEntity>.GetOperandsInfo(item.Operation);

			if ((def.Allowed & item.Operation) != item.Operation)
			{
				throw new InvalidOperationException(@$"Filter operation ""{item.Operation.ToString()}"" or its options is not allowed on field ""{def.Name}"".");
			}

			if (info.operandCount == 1)
			{
				if (item.Value != null || item.Value2 != null)
					throw new ArgumentException("item.Value");

				var buildMethod = typeof(FieldPredicate<>).MakeGenericType(typeof(TEntity))
					.GetMethod("Build", BindingFlags.Static | BindingFlags.Public, new Type[] { typeof(FilterOperations), typeof(Type), typeof(string) }) ??
					throw new InvalidCastException("Invalid filter arguments specified.");

				predicates[total++] = (Expression)buildMethod.Invoke(null, new object[] { item.Operation, def.OperandType, def.Name })!;
			}
			else if (info.operandCount == 2)
			{
				if (item.Value2 != null)
				{
					throw new ArgumentException("item.Value2");
				}

				if (item.Value == null)
				{
					if (info.isManyRightOperands || !def.IsNullAllowed)
						throw new ArgumentNullException("item.Value");
				}
				else if (info.isManyRightOperands)
				{
					if (item.Value is not IEnumerable)
						throw new ArgumentException("item.Value");
				}
				else if (!def.OperandType.IsAssignableFrom(item.Value!.GetType()))
				{
					throw new ArgumentException("item.Value");
				}

				var buildMethod = typeof(FieldPredicate<>).MakeGenericType(typeof(TEntity))
					.GetMethod("Build", BindingFlags.Static | BindingFlags.Public, new Type[] { typeof(FilterOperations), typeof(Type), typeof(string), def.OperandType }) ??
					throw new MissingMethodException("An appropriate method to build the filter is missing.");

				predicates[total++] = (Expression)buildMethod.Invoke(null, new object?[] { item.Operation, def.OperandType, def.Name, item.Value })!;
			}
			else if (info.operandCount == 3)
			{
				if (item.Value == null)
				{
					if (!def.IsNullAllowed)
						throw new ArgumentNullException("item.Value");
				}
				else if (!def.OperandType.IsAssignableFrom(item.Value!.GetType()))
				{
					throw new ArgumentException("item.Value");
				}

				if (item.Value2 == null)
				{
					if (!def.IsNullAllowed)
						throw new ArgumentNullException("item.Value2");
				}
				else if (!def.OperandType.IsAssignableFrom(item.Value2!.GetType()))
				{
					throw new ArgumentException("item.Value2");
				}

				var buildMethod = typeof(FieldPredicate<>).MakeGenericType(typeof(TEntity))
					.GetMethod("Build", BindingFlags.Static | BindingFlags.Public, new Type[] { typeof(FilterOperations), typeof(Type), typeof(string), def.OperandType, def.OperandType }) ??
					throw new MissingMethodException("An appropriate method to build the filter is missing.");

				predicates[total++] = (Expression)buildMethod.Invoke(null, new object?[] { item.Operation, def.OperandType, def.Name, item.Value, item.Value2 })!;
			}
		}

		if (total == 0)
			return Expression.Constant(true, typeof(bool));
		else if (total == 1)
			return predicates[0];

		var predicate = Expression.AndAlso(predicates[0], predicates[1]);
		for (int j = 2; j < total; j++)
		{
			predicate = Expression.AndAlso(predicate, predicates[j]);
		}

		return predicate;
	}

	[return: NotNullIfNotNull("values")]
	public static Expression<Func<TEntity, bool>>? BuildCondition(FilterValues? values)
	{
		return values != null ?
			Expression.Lambda<Func<TEntity, bool>>(BuildConditionPredicate(values), FieldPredicate<TEntity>.EntityParameter) :
			null;
	}

	public static string? SerializeToString(FilterValues? items)
	{
		throw new NotSupportedException();
	}

	/// <summary>
	/// 
	/// Flags:
	/// TrueWhenNull: [n]
	/// CaseInsensitive: [i]
	/// CaseInsensitiveInvariant: [v]
	///
	/// Operations:
	/// IsNull:			nl[-n]|{name}
	/// IsNotNull:		nnl[-n]|{name}
	/// Equal:			eq[-niv]|{name}[|[arg1]]
	/// NotEqual:		neq[-niv]|{name}[|[arg1]]
	/// In:				in[-niv]|{name}[|[arg1],[arg2]...]
	/// NotIn:			nin[-niv]|{name}[|[arg1],[arg2]...]
	/// Greater:		gt[-n]|{name}[|[arg1]]
	/// GreaterOrEqual:	gte[-n]|{name}[|[arg1]]
	/// Less:			lt[-n]|{name}[|[arg1]]
	/// LessOrEqual:	lte[-n]|{name}[|[arg1]]
	/// Between:		bw[-n]|{name}|[arg1],[arg2]
	/// NotBetween:		nbw[-n]|{name}|[arg1],[arg2]
	/// Like:			lk[-niv]|{name}[|[arg1]]
	/// NotLike:		nlk[-niv]|{name}[|[arg1]]
	/// BitsAnd:		all[-n]|{name}[|[arg1]]
	/// BitsOr:			any[-n]|{name}[|[arg1]]
	/// </summary>
	/// <typeparam name="TDto"></typeparam>
	/// <param name="filter"></param>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException"></exception>
	/// <exception cref="FormatException"></exception>
	public static FilterValues? SerializeFromString<TDto>(string? filter)
	{
		if (string.IsNullOrWhiteSpace(filter)) return null;

		var filterStringValues = SplitStringAndUnescapeDelimiter('\\', ';', filter);
		var filterValues = new FilterValue[filterStringValues.Length];
		int i = 0;
		var definitions = DefinitionHelper<TDto>.Definitions;
		FilterDefinition? fd;
		foreach (var p in filterStringValues.Select(x => PrepareFilterValueFromString(x)))
		{
			if (!definitions.TryGetValue(p.name, out fd))
			{
				throw new InvalidOperationException($"Filter {p.name} is not found.");
			}
			if (	(fd.Allowed & p.operation) != p.operation && (
						(p.operation & (FilterOperations.Like | FilterOperations.BitsOr)) == (FilterOperations.Like | FilterOperations.BitsOr) &&
						(fd.Allowed & (FilterOperations.Like | FilterOperations.BitsOr)) == 0
					)
				)
			{
				throw new InvalidOperationException($"Filter {fd.Name} does not support this operation or option.");
			}

			if ((p.operation & (FilterOperations.IsNull | FilterOperations.IsNotNull)) != 0)
			{
				if (!string.IsNullOrEmpty(p.arguments))
				{
					throw new FormatException($"Operation IsNull of the {fd.Name} filter cannot have arguments.");
				}

				filterValues[i++] = new FilterValue(fd.Name)
				{
					Operation = p.operation
				};
			}
			else if ((p.operation & (
					FilterOperations.Equal |
					FilterOperations.NotEqual |
					FilterOperations.Greater |
					FilterOperations.GreaterOrEqual |
					FilterOperations.Less |
					FilterOperations.LessOrEqual |
					FilterOperations.Like |
					FilterOperations.NotLike |
					FilterOperations.BitsAnd |
					FilterOperations.BitsOr
				)) != 0)
			{
				var value = ParseFilterValue(fd.UnderlyingOperandType, p.arguments);
				if (fd.EmptyToNull && string.IsNullOrEmpty((string?)value))
				{
					value = (string?)null;
				}
				if (!fd.IsNullAllowed && value == null)
				{
					throw new InvalidOperationException($"Filter {fd.Name} does not support nullable arguments.");
				}

				filterValues[i++] = new FilterValue(fd.Name)
				{
					Operation = p.operation,
					Value = value
				};
			}
			else if ((p.operation & (FilterOperations.Between | FilterOperations.NotBetween)) != 0)
			{
				var arr = SplitStringAndUnescapeDelimiter('\\', ',', p.arguments!).Select(x => ParseFilterValue(fd.UnderlyingOperandType, x)).ToArray();
				if (!fd.IsNullAllowed && arr.Any(x => x == null))
				{
					throw new InvalidOperationException($"Filter {fd.Name} does not support nullable arguments.");
				}
				if (arr.Length > 2)
				{
					throw new FormatException($"Operation Between of the {fd.Name} filter must have 2 arguments.");
				}

				filterValues[i++] = new FilterValue(fd.Name)
				{
					Operation = p.operation,
					Value = arr.Length > 0 ? arr[0] : (string?)null,
					Value2 = arr.Length > 1 ? arr[1] : (string?)null
				};
			}
			else if ((p.operation & (FilterOperations.In | FilterOperations.NotIn)) != 0)
			{
				var arr1 = SplitStringAndUnescapeDelimiter('\\', ',', p.arguments!).Select(x => ParseFilterValue(fd.UnderlyingOperandType, x));
				if (!fd.IsNullAllowed && arr1.Any(x => x == null))
				{
					throw new InvalidOperationException($"Filter {fd.Name} does not support nullable arguments.");
				}

				var castMethod = typeof(Enumerable).GetMethod("Cast")?.MakeGenericMethod(new[] { fd.OperandType });
				var arr2 = castMethod!.Invoke(null, new object[] { arr1 });

				filterValues[i++] = new FilterValue(fd.Name)
				{
					Operation = p.operation,
					Value = arr2
				};
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		return new FilterValues(definitions, filterValues);
	}

	/// <summary>
	/// Takes the first step of parsing the filter value string.
	/// It returns the argument(s) together as a single non-null string and does not check supported flags by operation or arguments.
	/// </summary>
	/// <param name="filterStringValue"></param>
	/// <param name="matchTimeout"></param>
	/// <exception cref="FormatException"></exception>
	private static (string name, FilterOperations operation, string? arguments) PrepareFilterValueFromString(string filterStringValue)
	{
		try
		{
			var m = _matchFilterValueFromString.Match(filterStringValue);
			if (m.Success)
			{
				var fo = _operationNames[m.Groups[2].Value.ToLower()];
				var flags = m.Groups[3].Value.ToLower();
				if (flags.Contains('i')) fo |= FilterOperations.CaseInsensitive;
				else if (flags.Contains('v')) fo |= FilterOperations.CaseInsensitiveInvariant;
				if (flags.Contains('n')) fo |= FilterOperations.TrueWhenNull;
				return (m.Groups[1].Value, fo, m.Groups.Count > 4 ? m.Groups[4].Value : string.Empty);
			}
		}
		catch (Exception ex)
		{
			if (ex is FormatException) throw;
			throw new FormatException(ex.Message, ex);
		}
		throw new FormatException();
	}
	
	/// <summary>
	/// Split string and unescape it by given delimiter character.
	/// </summary>
	/// <param name="delimiter"></param>
	/// <param name="input">An escaped input string in which the delimiter is escaped by two consecutive delimiter characters.</param>
	/// <exception cref="FormatException"></exception>
	private static string[] SplitStringAndUnescapeDelimiter(char escape, char delimiter, string input)
	{
		if (!input.Contains(escape))
		{
			return input.Split(delimiter, StringSplitOptions.None);
		}

		var escapeEscape = new string(escape, 2);
		var escapeDelimiter = string.Concat(escape, delimiter);
		var sescape = new string(escape, 1);
		var sdelimiter = new string(delimiter, 1);

		var list = new List<string>();
		int i = 0, j, k = 0;
		while ((j = input.IndexOf(delimiter, i)) >= 0)
		{
			if (ConsecutiveBackwordCharCount(input, j - 1, escape) % 2 == 0)
			{
				if (j + 1 < input.Length)
				{
					list.Add(input.Substring(k, j - k).Replace(escapeEscape, sescape).Replace(escapeDelimiter, sdelimiter));
					k = i = j + 1;
				}
				else
				{
					break;
				}
			}
			else if (j + 1 < input.Length)
			{
				i = j + 1;
			}
			else
			{
				break;
			}
		}
		list.Add(input.Substring(k, input.Length - k).Replace(escapeEscape, sescape).Replace(escapeDelimiter, sdelimiter));

		return list.ToArray();
	}
	private static int ConsecutiveBackwordCharCount(string input, int startIndex, char charToCount)
	{
		int count = 0;
		while (startIndex >= 0 && input[startIndex--] == charToCount) count++;
		return count;
	}

	private static object? ParseFilterValue(Type operandType, string? value)
	{
		Func<string?, object?>? converter;
		if (_parseFilterValueConverters.TryGetValue(operandType, out converter))
		{
			return converter(value);
		}
		throw new NotSupportedException($"Data type {operandType.Name} is not supported by filter.");
	}

	private static readonly Regex _matchFilterValueFromString =
		new Regex(@"^([^-]+)-(eq|neq|lk|nlk|bw|nbw|in|nin|gt|gte|lt|lte|all|any|nl|nnl)((?:n|ni|nv|i|in|v|vn)?)(?:-(.*))?$", RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(300));
	private static readonly Dictionary<string, FilterOperations> _operationNames = new()
	{
		{ "eq", FilterOperations.Equal },
		{ "neq", FilterOperations.NotEqual },
		{ "lk", FilterOperations.Like },
		{ "nlk", FilterOperations.NotLike },
		{ "bw", FilterOperations.Between },
		{ "nbw", FilterOperations.NotBetween },
		{ "in", FilterOperations.In },
		{ "nin", FilterOperations.NotIn },
		{ "gt", FilterOperations.Greater },
		{ "gte", FilterOperations.GreaterOrEqual },
		{ "lt", FilterOperations.Less },
		{ "lte", FilterOperations.LessOrEqual },
		{ "all", FilterOperations.BitsAnd },
		{ "any", FilterOperations.BitsOr },
		{ "nl", FilterOperations.IsNull },
		{ "nnl", FilterOperations.IsNotNull }
	};
	private static readonly CultureInfo _dotNumberFormatInvariantCulture;
	private static readonly Dictionary<Type, Func<string?, object?>> _parseFilterValueConverters = new ()
	{
		{ typeof(string), s => s },
 		{ typeof(char), s => string.IsNullOrEmpty(s) ? (char?)null : s[0] },

		{ typeof(Guid), s => string.IsNullOrEmpty(s) ? (Guid?)null : Guid.Parse(s) },

		{ typeof(byte), s => string.IsNullOrEmpty(s) ? (byte?)null : byte.Parse(s) },
		{ typeof(sbyte), s => string.IsNullOrEmpty(s) ? (sbyte?)null : sbyte.Parse(s) },
		{ typeof(short), s => string.IsNullOrEmpty(s) ? (short?)null : short.Parse(s) },
		{ typeof(ushort), s => string.IsNullOrEmpty(s) ? (ushort?)null : ushort.Parse(s) },
		{ typeof(int), s => string.IsNullOrEmpty(s) ? (int?)null : int.Parse(s) },
		{ typeof(uint), s => string.IsNullOrEmpty(s) ? (uint?)null : uint.Parse(s) },
		{ typeof(long), s => string.IsNullOrEmpty(s) ? (long?)null : long.Parse(s) },
		{ typeof(ulong), s => string.IsNullOrEmpty(s) ? (ulong?)null : ulong.Parse(s) },

		{ typeof(DateTime), s => string.IsNullOrEmpty(s) ? (DateTime?)null : DateTime.ParseExact(s, "yyyy-MM-ddTHH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture) },
		{ typeof(DateTimeOffset), s => string.IsNullOrEmpty(s) ? (DateTimeOffset?)null : DateTimeOffset.ParseExact(s, "yyyy-MM-ddTHH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture) },
		{ typeof(DateOnly), s => string.IsNullOrEmpty(s) ? (DateOnly?)null : DateOnly.ParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture) },
		{ typeof(TimeOnly), s => string.IsNullOrEmpty(s) ? (TimeOnly?)null : TimeOnly.ParseExact(s, "HH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture) },
		{ typeof(TimeSpan), s => string.IsNullOrEmpty(s) ? (TimeSpan?)null : TimeSpan.ParseExact(s, "d:hh:mm:ss.FFFFFFF", CultureInfo.InvariantCulture) },

		{ typeof(float), s => string.IsNullOrEmpty(s) ? (float?)null : float.Parse(s, NumberStyles.Float, _dotNumberFormatInvariantCulture) },
		{ typeof(double), s => string.IsNullOrEmpty(s) ? (double?)null : double.Parse(s, NumberStyles.Float, _dotNumberFormatInvariantCulture) },
		{ typeof(decimal), s => string.IsNullOrEmpty(s) ? (decimal?)null : decimal.Parse(s, NumberStyles.Integer | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, _dotNumberFormatInvariantCulture) }
	};
}