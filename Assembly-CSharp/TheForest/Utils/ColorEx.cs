using System;
using UnityEngine;

namespace TheForest.Utils
{
	public static class ColorEx
	{
		public static float SqrEuclideanDistance(Color from, Color to)
		{
			float num = Mathf.Pow(to.r - from.r, 2f);
			float num2 = Mathf.Pow(to.g - from.g, 2f);
			float num3 = Mathf.Pow(to.b - from.b, 2f);
			return num3 + num2 + num;
		}

		public static int ClosestColorIndex(Color c, Color[] cs)
		{
			int result = -1;
			float num = float.MaxValue;
			for (int i = 0; i < cs.Length; i++)
			{
				float num2 = ColorEx.SqrEuclideanDistance(c, cs[i]);
				if (Mathf.Approximately(num2, 0f))
				{
					result = i;
					break;
				}
				if (num2 < num)
				{
					num = num2;
					result = i;
				}
			}
			return result;
		}

		public static Color ClosestColor(Color c, Color[] cs)
		{
			int num = ColorEx.ClosestColorIndex(c, cs);
			if (num > -1)
			{
				return cs[num];
			}
			return default(Color);
		}
	}
}
