using Newtonsoft.Json;

namespace NeoState.Common.Tools
{
	public static class JsonTool
	{
		public static T DeserializeObject<T>(string json) where T : class
		{
			T result;
			try
			{
				result = JsonConvert.DeserializeObject<T>(json);
			}
			catch
			{
				result = null;
			}
			return result;
		}
	}
}