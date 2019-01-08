using System;
using System.Collections.Generic;
using System.Linq;

namespace TheForest.Utils
{
	public static class StringExtensions
	{
		public static bool IsNull(this string stringValue)
		{
			return stringValue == null;
		}

		public static bool IsEmpty(this string stringValue)
		{
			return stringValue == string.Empty;
		}

		public static bool NullOrEmpty(this string stringValue)
		{
			return string.IsNullOrEmpty(stringValue);
		}

		public static string FirstOrDefault(this IEnumerable<string> strings, string defaultResult = null)
		{
			if (strings == null)
			{
				return defaultResult;
			}
			return strings.FirstOrDefault(null) ?? defaultResult;
		}

		public static string FirstNotNull(this IEnumerable<string> strings, string defaultResult = null)
		{
			if (strings == null)
			{
				return defaultResult;
			}
			return strings.First((string stringValue) => stringValue != null) ?? defaultResult;
		}

		public static string FirstNotNullOrEmpty(this IEnumerable<string> strings, string defaultResult = null)
		{
			if (strings == null)
			{
				return defaultResult;
			}
			return strings.First((string stringValue) => !string.IsNullOrEmpty(stringValue)) ?? defaultResult;
		}

		public static string IfNull(this string stringValue, string defaultResult)
		{
			return stringValue ?? defaultResult;
		}

		public static string IfNullOrEmpty(this string stringValue, string defaultResult)
		{
			return (!string.IsNullOrEmpty(stringValue)) ? stringValue : defaultResult;
		}

		private const string EmptyString = "";
	}
}
