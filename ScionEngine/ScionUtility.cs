using System;
using UnityEngine;

namespace ScionEngine
{
	
	public static class ScionUtility
	{
		
		public static float GetWhitePointMultiplier(float whitePoint)
		{
			return 11.2f / whitePoint;
		}

		
		public static float CoCToPixels(float widthInPixels)
		{
			return widthInPixels / 0.035f;
		}

		
		public static float ComputeApertureDiameter(float fNumber, float focalLength)
		{
			return focalLength / fNumber;
		}

		
		public static float Square(float val)
		{
			return val * val;
		}

		
		public static float GetFocalLength(float tanHalfFoV)
		{
			return 17.5f / tanHalfFoV * 0.001f;
		}

		
		public static float GetFieldOfView(float focalLength)
		{
			float f = 17.5f / (focalLength * 1000f);
			return Mathf.Atan(f) * 2f * 57.29578f;
		}

		
		
		public static Texture2D WhiteTexture
		{
			get
			{
				if (ScionUtility.s_WhiteTexture == null)
				{
					ScionUtility.s_WhiteTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false, true);
					ScionUtility.s_WhiteTexture.hideFlags = HideFlags.HideAndDontSave;
					ScionUtility.s_WhiteTexture.SetPixel(0, 0, Color.white);
					ScionUtility.s_WhiteTexture.Apply(false, true);
				}
				return ScionUtility.s_WhiteTexture;
			}
		}

		
		public const float DefaultWhitePoint = 11.2f;

		
		private static Texture2D s_WhiteTexture;
	}
}
