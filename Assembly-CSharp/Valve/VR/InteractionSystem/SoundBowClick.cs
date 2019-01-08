using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	public class SoundBowClick : MonoBehaviour
	{
		private void Awake()
		{
			this.thisAudioSource = base.GetComponent<AudioSource>();
		}

		public void PlayBowTensionClicks(float normalizedTension)
		{
			float num = this.pitchTensionCurve.Evaluate(normalizedTension);
			this.thisAudioSource.pitch = (this.maxPitch - this.minPitch) * num + this.minPitch;
			this.thisAudioSource.PlayOneShot(this.bowClick);
		}

		public AudioClip bowClick;

		public AnimationCurve pitchTensionCurve;

		public float minPitch;

		public float maxPitch;

		private AudioSource thisAudioSource;
	}
}
