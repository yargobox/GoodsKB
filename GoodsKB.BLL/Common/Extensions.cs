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
}