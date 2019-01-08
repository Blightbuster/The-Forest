using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Player
{
	public class LightFlicker : MonoBehaviour
	{
		private void Awake()
		{
			this._vis = base.transform.root.GetComponent<netPlayerVis>();
			this._light = base.GetComponent<Light>();
		}

		private void OnEnable()
		{
			this.random = UnityEngine.Random.Range(0f, 65535f);
		}

		private void OnDisable()
		{
			if (this.ManageDynamicShadows)
			{
				if (CoopPeerStarter.DedicatedHost || LocalPlayer.Transform == null || Scene.SceneTracker)
				{
					return;
				}
				if (Scene.SceneTracker.activePlayerLights.Contains(base.gameObject))
				{
					Scene.SceneTracker.activePlayerLights.Remove(base.gameObject);
				}
			}
		}

		private void Update()
		{
			float t = Mathf.PerlinNoise(this.random, Time.time * this._flickerSpeed);
			this._light.intensity = Mathf.Lerp(this._minIntensity, this._maxIntensity, t);
			if (this.ManageDynamicShadows)
			{
				this.manageShadows();
			}
			else
			{
				TheForestQualitySettings.UserSettings.ApplyQualitySetting(this._light, LightShadows.Soft);
			}
		}

		private void manageShadows()
		{
			if (CoopPeerStarter.DedicatedHost || LocalPlayer.Transform == null || Scene.SceneTracker == null)
			{
				return;
			}
			if ((Scene.SceneTracker.activePlayerLights.Count < 3 || Scene.SceneTracker.activePlayerLights.Contains(base.gameObject)) && this._vis.localplayerDist < 50f)
			{
				if (!Scene.SceneTracker.activePlayerLights.Contains(base.gameObject))
				{
					Scene.SceneTracker.activePlayerLights.Add(base.gameObject);
				}
				TheForestQualitySettings.UserSettings.ApplyQualitySetting(this._light, LightShadows.Soft);
			}
			else
			{
				if (Scene.SceneTracker.activePlayerLights.Contains(base.gameObject))
				{
					Scene.SceneTracker.activePlayerLights.Remove(base.gameObject);
				}
				TheForestQualitySettings.UserSettings.ApplyQualitySetting(this._light, LightShadows.None);
			}
		}

		public float _minIntensity = 0.2f;

		public float _maxIntensity = 0.3f;

		public float _flickerSpeed = 8f;

		private Light _light;

		public bool ManageDynamicShadows;

		private bool updateMaxLight;

		private netPlayerVis _vis;

		private float random;
	}
}
