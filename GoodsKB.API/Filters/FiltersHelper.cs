using System.Globalization;
using System.Text.RegularExpressions;
using GoodsKB.DAL.Repositories.Filters;
using System.Collections.ObjectModel;

namespace GoodsKB.API.Filters;

internal static class FiltersHelper
{
	static FiltersHelper()
	{
		_dotNumberFormatInvariantCulture = (CultureInfo) CultureInfo.InvariantCulture.Clone();
		_dotNumberFormatInvariantCulture.NumberFormat.CurrencyDecimalSeparator = ".";
	}

	public static string? SerializeToString(ReadOnlyDictionary<string, FilterDesc> filters, FilterValues? items)
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
	/// <param name="filters">Filter descriptions of DTO or database entity class itself</param>
	/// <param name="filter"></param>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException"></exception>
	/// <exception cref="FormatException"></exception>
	public static FilterValues? SerializeFromString(ReadOnlyDictionary<string, FilterDesc> filters, string? filter)
	{
		if (string.IsNullOrWhiteSpace(filter)) return null;

		var filterStringValues = SplitStringAndUnescapeDelimiter('\\', ';', filter, true);
		var filterValues = new FilterValue[filterStringValues.Length];
		int i = 0;
		FO trueOperation;
		foreach (var p in filterStringValues.Select(x => PrepareFilterValueFromString(x)))
		{
			FilterDesc? fd;
			if (!filters.TryGetValue(p.name, out fd))
			{
				throw new InvalidOperationException($"Filter {p.name} is not found.");
			}

			trueOperation = p.operation;
			if (trueOperation.HasFlag(FO.Like) && fd.Allowed.HasFlag(FO.BitsOr))
			{
				trueOperation = (p.operation & ~FO.Like) | FO.BitsOr;
			}

			if ((fd.Allowed & trueOperation) != trueOperation)
			{
				throw new InvalidOperationException($"Filter {fd.PropertyName} does not support this operation or option.");
			}

			if ((trueOperation & (FO.IsNull | FO.IsNotNull)) != 0)
			{
				if (!string.IsNullOrEmpty(p.arguments))
				{
					throw new FormatException($"Operation IsNull of the {fd.PropertyName} filter cannot have arguments.");
				}

				filterValues[i++] = new FilterValue(fd.PropertyName)
				{
					Operation = trueOperation
				};
			}
			else if ((trueOperation & (
					FO.Equal |
					FO.NotEqual |
					FO.Greater |
					FO.GreaterOrEqual |
					FO.Less |
					FO.LessOrEqual |
					FO.Like |
					FO.NotLike |
					FO.BitsAnd |
					FO.BitsOr
				)) != 0)
			{
				var value = ParseFilterValue(fd.UnderlyingType, p.arguments);
				if (fd.IsEmptyToNull && (trueOperation & (FO.Like | FO.NotLike)) == 0 && string.IsNullOrEmpty((string?)value))
				{
					value = (string?)null;
				}
				if (!fd.IsNullAllowed && value == null)
				{
					throw new InvalidOperationException($"Filter {fd.PropertyName} does not support nullable arguments.");
				}

				filterValues[i++] = new FilterValue(fd.PropertyName)
				{
					Operation = trueOperation,
					Value = value
				};
			}
			else if ((trueOperation & (FO.Between | FO.NotBetween)) != 0)
			{
				var arr = SplitStringAndUnescapeDelimiter('\\', ',', p.arguments!)
					.Select(x => ParseFilterValue(fd.UnderlyingType, x))
					.ToArray();
				if (!fd.IsNullAllowed && arr.Any(x => x == null))
				{
					throw new InvalidOperationException($"Filter {fd.PropertyName} does not support nullable arguments.");
				}
				if (arr.Length > 2)
				{
					throw new FormatException($"Operation Between of the {fd.PropertyName} filter must have 2 arguments.");
				}

				filterValues[i++] = new FilterValue(fd.PropertyName)
				{
					Operation = trueOperation,
					Value = arr.Length > 0 ? arr[0] : (string?)null,
					Value2 = arr.Length > 1 ? arr[1] : (string?)null
				};
			}
			else if ((trueOperation & (FO.In | FO.NotIn)) != 0)
			{
				var arr1 = SplitStringAndUnescapeDelimiter('\\', ',', p.arguments!)
					.Select(x => ParseFilterValue(fd.UnderlyingType, x));
				if (!fd.IsNullAllowed && arr1.Any(x => x == null))
				{
					throw new InvalidOperationException($"Filter {fd.PropertyName} does not support nullable arguments.");
				}

				var castMethod = typeof(Enumerable).GetMethod("Cast")?.MakeGenericMethod(new[] { fd.OperandType });
				var arr2 = castMethod!.Invoke(null, new object[] { arr1 });

				filterValues[i++] = new FilterValue(fd.PropertyName)
				{
					Operation = trueOperation,
					Value = arr2
				};
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		return new FilterValues(filters, filterValues);
	}

	/// <summary>
	/// Takes the first step of parsing the filter value string.
	/// It returns the argument(s) together as a single non-null string and does not check supported flags by operation or arguments.
	/// </summary>
	/// <param name="filterStringValue"></param>
	/// <param name="matchTimeout"></param>
	/// <exception cref="FormatException"></exception>
	private static (string name, FO operation, string? arguments) PrepareFilterValueFromString(string filterStringValue)
	{
		try
		{
			var m = _matchFilterValueFromString.Match(filterStringValue);
			if (m.Success)
			{
				var direct = !(m.Groups[1].Value == "!");
				var fo = m.Groups[4].Value switch
				{
					"=" => direct ? FO.Equal : FO.NotEqual,
					":" => direct ? FO.In : FO.NotIn,
					"-" => direct ? FO.Between : FO.NotBetween,
					">" => direct ? FO.Greater : throw new FormatException(),
					">=" => direct ? FO.GreaterOrEqual : throw new FormatException(),
					"<" => direct ? FO.Less : throw new FormatException(),
					"<=" => direct ? FO.LessOrEqual : throw new FormatException(),
					"~" => direct ? FO.Like : FO.NotLike,
					".=" => direct ? FO.BitsAnd : throw new FormatException(),
					".~" => direct ? FO.BitsOr : throw new FormatException(),
					"" => direct ? FO.IsNotNull : FO.IsNull,
					_ => throw new FormatException()
				};
				var flags = m.Groups[3].Value.ToLower();
				if (flags.Contains('i')) fo |= FO.CaseInsensitive;
				else if (flags.Contains('v')) fo |= FO.CaseInsensitiveInvariant;
				if (flags.Contains('n')) fo |= FO.TrueWhenNull;
				return (m.Groups[2].Value, fo, m.Groups[5].Value);
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
	private static string[] SplitStringAndUnescapeDelimiter(char escape, char delimiter, string input, bool removeEmptyEntries = false)
	{
		if (!input.Contains(escape))
		{
			return input.Split(delimiter, removeEmptyEntries ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
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

		return list.Where(x => removeEmptyEntries ? x.Length > 0 : true).ToArray();
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
		new Regex(@"^(\!?)([a-zA-z0-9_]+)((?:\|n|\|ni|\|nv|\|i|\|in|\|v|\|vn)?)(?:(=|:|-|>=|>|<=|<|~|\.=|\.~)(.*))?$", RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(300));
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