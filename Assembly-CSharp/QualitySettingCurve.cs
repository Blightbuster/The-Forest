using System;
using UnityEngine;

public class QualitySettingCurve : PropertyAttribute
{
	public QualitySettingCurve(int qualityLevels)
	{
		this._qualityLevels = qualityLevels;
	}

	public int _qualityLevels;
}
