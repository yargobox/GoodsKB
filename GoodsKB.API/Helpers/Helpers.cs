using System.Globalization;
using GoodsKB.DAL.Repositories;

namespace GoodsKB.API.Helpers;

internal static class SoftDelModeHelper
{
	public static bool TryParse(string? s, out SoftDelModes softDelMode)
	{
		if (string.IsNullOrWhiteSpace(s))
		{
			softDelMode = SoftDelModes.Actual;
			return false;
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

	public static SoftDelModes TryParse(string? s, SoftDelModes @default)
	{
		if (string.IsNullOrWhiteSpace(s))
		{
			return @default;
		}

		int n;
		if (int.TryParse(s, NumberStyles.Integer, null, out n) && Enum.GetValues<SoftDelModes>().Cast<int>().Contains(n))
		{
			return (SoftDelModes)n;
		}

		SoftDelModes softDelMode;
		if (Enum.TryParse<SoftDelModes>(s, true, out softDelMode))
		{
			return softDelMode;
		}

		return @default;
	}

	public static SoftDelModes Parse(string? s)
	{
		SoftDelModes softDelMode;
		if (TryParse(s, out softDelMode))
			return softDelMode;

		throw new FormatException($"{s} is not valid SoftDelModes");
	}
}