using System;
using UnityEngine;

namespace ScionEngine
{
	
	public static class ColorGrading
	{
		
		
		private static Material ColorGradingMat
		{
			get
			{
				if (ColorGrading.s_ColorGradingMat == null)
				{
					ColorGrading.s_ColorGradingMat = new Material(Shader.Find("Hidden/ScionColorGrading"));
					ColorGrading.s_ColorGradingMat.hideFlags = HideFlags.HideAndDontSave;
				}
				return ColorGrading.s_ColorGradingMat;
			}
		}

		
		public static void UploadColorGradingParams(Material mat, float numSlices)
		{
			float num = 1f / numSlices;
			Vector2 vector = new Vector2(numSlices * numSlices, numSlices);
			Vector4 value = default(Vector4);
			value.x = 1f * num - 1f / vector.x;
			value.y = 1f - 1f * num;
			value.z = numSlices - 1f;
			value.w = numSlices;
			Vector4 value2 = default(Vector4);
			value2.x = 0.5f / vector.x;
			value2.y = 0.5f / vector.y;
			value2.z = 0f;
			value2.w = num;
			mat.SetVector("_ColorGradingParams1", value);
			mat.SetVector("_ColorGradingParams2", value2);
		}

		
		private static Material s_ColorGradingMat;
	}
}
