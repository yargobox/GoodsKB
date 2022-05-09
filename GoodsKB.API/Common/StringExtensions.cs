using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace GoodsKB.API.Extensions.Text;

internal static class TextExtensions
{
	/// <summary>
	/// Unescapes and splits a string into substrings based on a specified delimiting and escape characters and options.
	/// </summary>
	/// <remarks>
	/// Opposite method is <c>IEnumerable<string>.EscapeAndJoin</c>
	/// </remarks>
	[return: NotNullIfNotNull("input")]
	public static string[]? UnescapeAndSplit(this string? input, char delimiter, StringSplitOptions options = StringSplitOptions.None, char escape = '\\')
	{
		if (input == null) return null;
		if (input.IndexOf(escape) < 0) return input.Split(delimiter, options);

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

		if (options.HasFlag(StringSplitOptions.TrimEntries))
			return list.Select(x => x.Trim()).Where(x => options.HasFlag(StringSplitOptions.RemoveEmptyEntries) ? x.Length > 0 : true).ToArray();
		else
			return list.Where(x => options.HasFlag(StringSplitOptions.RemoveEmptyEntries) ? x.Length > 0 : true).ToArray();
	}
	static int ConsecutiveBackwordCharCount(string input, int startIndex, char charToCount)
	{
		int count = 0;
		while (startIndex >= 0 && input[startIndex--] == charToCount) count++;
		return count;
	}

	/// <summary>
	/// Escapes the members of a collection and concatenates them, using the specified separator between each member.
	/// </summary>
	/// <remarks>
	/// Opposite method is <c>string.UnescapeAndSplit</c>
	/// </remarks>
	[return: NotNullIfNotNull("input")]
	public static string? EscapeAndJoin(this IEnumerable<string>? input, char delimiter, char escape = '\\')
	{
		if (input == null) return null;

		var delimiterString = delimiter.ToString();
		var escapeString = escape.ToString();
		var escapedDelimiter = escapeString + delimiterString;
		var escapedEscape = new string(escape, 2);

		var next = false;
		var sb = new StringBuilder();
		foreach (var s in input)
		{
			if (next) sb.Append(delimiter); else next = true;

			sb.Append(s.Replace(escapeString, escapedEscape).Replace(delimiterString, escapedDelimiter));
		}

		return sb.ToString();
	}
}