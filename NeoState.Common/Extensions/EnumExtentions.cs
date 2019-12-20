using System;

namespace NeoState.Common
{
	public static class EnumExtensions
	{
		/// <summary>
		/// Is this function necessary?
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public static T ParseEnum<T>(string value)
		{
			return (T)Enum.Parse(typeof(T), value, true);
		}
	}
}