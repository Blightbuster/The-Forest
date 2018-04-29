using System;
using System.Collections;
using System.Collections.Generic;

namespace TheForest.Utils
{
	
	public static class ListExtensions
	{
		
		public static bool NullOrEmpty<T>(this ICollection<T> collectionValue)
		{
			return collectionValue == null || collectionValue.Count == 0;
		}

		
		public static bool NullOrEmpty<T>(this T[] arrayValue)
		{
			return arrayValue == null || arrayValue.Length == 0;
		}

		
		public static int SafeCount<T>(this ICollection<T> arrayValue)
		{
			if (arrayValue == null)
			{
				return 0;
			}
			return arrayValue.Count;
		}

		
		public static int SafeCount<T>(this IList<T> arrayValue)
		{
			if (arrayValue == null)
			{
				return 0;
			}
			return arrayValue.Count;
		}

		
		public static int SafeCount(this ArrayList arrayValue)
		{
			if (arrayValue == null)
			{
				return 0;
			}
			return arrayValue.Count;
		}

		
		public static int SafeCount<T>(this T[] arrayValue)
		{
			if (arrayValue == null)
			{
				return 0;
			}
			return arrayValue.Length;
		}

		
		public static bool IndexValid<T>(this T[] arrayValue, int arrayIndex)
		{
			return arrayIndex >= 0 && arrayValue != null && arrayValue.Length != 0 && arrayIndex < arrayValue.Length;
		}

		
		public static T GetIndexSafe<T>(this T[] arrayValue, int arrayIndex, T defaultResult = default(T))
		{
			if (arrayIndex < 0)
			{
				return defaultResult;
			}
			if (arrayValue == null)
			{
				return defaultResult;
			}
			return (arrayValue.Length != 0 && arrayIndex < arrayValue.Length) ? arrayValue[arrayIndex] : defaultResult;
		}

		
		public static bool IndexValid<T>(this List<T> arrayValue, int arrayIndex)
		{
			return arrayIndex >= 0 && arrayValue != null && arrayValue.Count != 0 && arrayIndex < arrayValue.Count;
		}

		
		public static T GetIndexSafe<T>(this List<T> arrayValue, int arrayIndex, T defaultResult = default(T))
		{
			if (arrayIndex < 0)
			{
				return defaultResult;
			}
			if (arrayValue == null)
			{
				return defaultResult;
			}
			return (arrayValue.Count != 0 && arrayIndex < arrayValue.Count) ? arrayValue[arrayIndex] : defaultResult;
		}
	}
}
