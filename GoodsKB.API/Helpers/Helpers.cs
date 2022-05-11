using System.Globalization;
using GoodsKB.DAL.Repositories;

namespace GoodsKB.API.Helpers;

internal static class SoftDelHelper
{
	public static bool TryParse(string? s, out SoftDel softDelMode)
	{
		if (string.IsNullOrWhiteSpace(s))
		{
			softDelMode = SoftDel.Actual;
			return false;
		}

		int n;
		if (int.TryParse(s, NumberStyles.Integer, null, out n) && Enum.GetValues<SoftDel>().Cast<int>().Contains(n))
		{
			softDelMode = (SoftDel)n;
			return true;
		}

		if (Enum.TryParse<SoftDel>(s, true, out softDelMode))
		{
			return true;
		}

		return false;
	}

	public static SoftDel TryParse(string? s, SoftDel @default)
	{
		if (string.IsNullOrWhiteSpace(s))
		{
			return @default;
		}

		int n;
		if (int.TryParse(s, NumberStyles.Integer, null, out n) && Enum.GetValues<SoftDel>().Cast<int>().Contains(n))
		{
			return (SoftDel)n;
		}

		SoftDel softDelMode;
		if (Enum.TryParse<SoftDel>(s, true, out softDelMode))
		{
			return softDelMode;
		}

		return @default;
	}

	public static SoftDel Parse(string? s)
	{
		SoftDel softDelMode;
		if (TryParse(s, out softDelMode))
			return softDelMode;

		throw new FormatException($"{s} is not valid SoftDelModes");
	}
}