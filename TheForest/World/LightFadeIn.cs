using System;
using System.Collections;
using UnityEngine;

namespace TheForest.World
{
	
	public class LightFadeIn : MonoBehaviour
	{
		
		private void Awake()
		{
			this._targetIntensity = this._targetLight.intensity;
			this._targetLight.intensity = 0f;
		}

		
		private void OnEnable()
		{
			this._startIntensity = this._targetLight.intensity;
			base.StopCoroutine("FadeOutRoutine");
			base.StartCoroutine("FadeInRoutine");
		}

		
		private void OnDisable()
		{
			if (!base.enabled)
			{
				this._startIntensity = this._targetLight.intensity;
				base.StopCoroutine("FadeInRoutine");
				base.StartCoroutine("FadeOutRoutine");
			}
		}

		
		private IEnumerator FadeInRoutine()
		{
			this._targetLight.enabled = true;
			float startTime = Time.time;
			while (startTime + this._fadeInDuration > Time.time)
			{
				this._targetLight.intensity = Mathf.Lerp(this._startIntensity, this._targetIntensity, (Time.time - startTime) / this._fadeInDuration);
				yield return YieldPresets.WaitForEndOfFrame;
			}
			this._targetLight.intensity = this._targetIntensity;
			yield break;
		}

		
		private IEnumerator FadeOutRoutine()
		{
			float startTime = Time.time;
			while (startTime + this._fadeInDuration > Time.time)
			{
				this._targetLight.intensity = Mathf.Lerp(this._startIntensity, 0f, (Time.time - startTime) / this._fadeInDuration);
				yield return YieldPresets.WaitForEndOfFrame;
			}
			this._targetLight.intensity = 0f;
			this._targetLight.enabled = false;
			yield break;
		}

		
		public Light _targetLight;

		
		public float _fadeInDuration = 1f;

		
		private float _startIntensity;

		
		private float _targetIntensity;
	}
}
