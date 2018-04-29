using System;
using UnityEngine;

namespace TheForest.Player
{
	
	public class LightFlicker : MonoBehaviour
	{
		
		private void Awake()
		{
			this._light = base.GetComponent<Light>();
		}

		
		private void Update()
		{
			float intensity = UnityEngine.Random.Range(this._minIntensity, this._maxIntensity);
			this._light.intensity = intensity;
			TheForestQualitySettings.UserSettings.ApplyQualitySetting(this._light, LightShadows.Soft);
		}

		
		public float _minIntensity = 0.2f;

		
		public float _maxIntensity = 0.3f;

		
		private Light _light;
	}
}
