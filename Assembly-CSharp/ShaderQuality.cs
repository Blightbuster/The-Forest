using System;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Examples/Shader Quality")]
public class ShaderQuality : MonoBehaviour
{
	private void Update()
	{
		int num = (QualitySettings.GetQualityLevel() + 1) * 100;
		if (this.mCurrent != num)
		{
			this.mCurrent = num;
			Shader.globalMaximumLOD = this.mCurrent;
		}
	}

	private int mCurrent = 600;
}
