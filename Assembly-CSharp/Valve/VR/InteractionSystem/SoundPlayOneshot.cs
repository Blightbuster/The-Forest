using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	public class SoundPlayOneshot : MonoBehaviour
	{
		private void Awake()
		{
			this.thisAudioSource = base.GetComponent<AudioSource>();
			if (this.playOnAwake)
			{
				this.Play();
			}
		}

		public void Play()
		{
			if (this.thisAudioSource != null && this.thisAudioSource.isActiveAndEnabled && !Util.IsNullOrEmpty<AudioClip>(this.waveFiles))
			{
				this.thisAudioSource.volume = UnityEngine.Random.Range(this.volMin, this.volMax);
				this.thisAudioSource.pitch = UnityEngine.Random.Range(this.pitchMin, this.pitchMax);
				this.thisAudioSource.PlayOneShot(this.waveFiles[UnityEngine.Random.Range(0, this.waveFiles.Length)]);
			}
		}

		public void Pause()
		{
			if (this.thisAudioSource != null)
			{
				this.thisAudioSource.Pause();
			}
		}

		public void UnPause()
		{
			if (this.thisAudioSource != null)
			{
				this.thisAudioSource.UnPause();
			}
		}

		public AudioClip[] waveFiles;

		private AudioSource thisAudioSource;

		public float volMin;

		public float volMax;

		public float pitchMin;

		public float pitchMax;

		public bool playOnAwake;
	}
}
