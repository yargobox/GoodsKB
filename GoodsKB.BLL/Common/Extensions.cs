using GoodsKB.DAL.Repositories;
using System.Globalization;

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
			softDelMode = (SoftDelModes) n;
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
}