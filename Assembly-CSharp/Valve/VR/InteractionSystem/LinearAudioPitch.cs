using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	public class LinearAudioPitch : MonoBehaviour
	{
		private void Awake()
		{
			if (this.audioSource == null)
			{
				this.audioSource = base.GetComponent<AudioSource>();
			}
			if (this.linearMapping == null)
			{
				this.linearMapping = base.GetComponent<LinearMapping>();
			}
		}

		private void Update()
		{
			if (this.applyContinuously)
			{
				this.Apply();
			}
		}

		private void Apply()
		{
			float t = this.pitchCurve.Evaluate(this.linearMapping.value);
			this.audioSource.pitch = Mathf.Lerp(this.minPitch, this.maxPitch, t);
		}

		public LinearMapping linearMapping;

		public AnimationCurve pitchCurve;

		public float minPitch;

		public float maxPitch;

		public bool applyContinuously = true;

		private AudioSource audioSource;
	}
}
