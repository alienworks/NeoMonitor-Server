using System.Text.RegularExpressions;

namespace NeoState.Common
{
	public static class StringExtensions
	{
		private readonly static Regex _ipRegex = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b", RegexOptions.Compiled);

		public static bool TryMatchIpString(this string input, out string ip)
		{
			Match match = _ipRegex.Match(input);
			if (match.Success)
			{
				ip = match.Value;
				return true;
			}
			ip = input;
			return false;
		}
	}
}