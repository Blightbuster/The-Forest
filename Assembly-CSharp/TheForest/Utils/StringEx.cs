using System;

namespace TheForest.Utils
{
	public class StringEx
	{
		public static string TryFormat(string format, params object[] args)
		{
			string result;
			try
			{
				result = string.Format(format, args);
			}
			catch
			{
				result = format;
			}
			return result;
		}
	}
}
