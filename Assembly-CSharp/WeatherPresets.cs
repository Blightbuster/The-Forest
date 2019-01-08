using System;
using UnityEngine;

public class WeatherPresets : MonoBehaviour
{
	private void Start()
	{
	}

	[SerializeField]
	public WeatherPresets.WeatherConfig[] WeatherConfigPresets;

	[Serializable]
	public class WeatherConfig
	{
		public float AlphaSaturation;

		public float CloudOpacityScale;

		public float SkyColorMultiplyer;

		public float OvercastAmount;
	}
}
