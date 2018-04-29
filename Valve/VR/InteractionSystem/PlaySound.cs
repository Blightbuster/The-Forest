using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	
	[RequireComponent(typeof(AudioSource))]
	public class PlaySound : MonoBehaviour
	{
		
		private void Awake()
		{
			this.audioSource = base.GetComponent<AudioSource>();
			this.clip = this.audioSource.clip;
			if (this.audioSource.playOnAwake)
			{
				if (this.useRetriggerTime)
				{
					base.InvokeRepeating("Play", this.timeInitial, UnityEngine.Random.Range(this.timeMin, this.timeMax));
				}
				else
				{
					this.Play();
				}
			}
			else if (!this.audioSource.playOnAwake && this.playOnAwakeWithDelay)
			{
				this.PlayWithDelay(this.delayOffsetTime);
				if (this.useRetriggerTime)
				{
					base.InvokeRepeating("Play", this.timeInitial, UnityEngine.Random.Range(this.timeMin, this.timeMax));
				}
			}
			else if (this.audioSource.playOnAwake && this.playOnAwakeWithDelay)
			{
				this.PlayWithDelay(this.delayOffsetTime);
				if (this.useRetriggerTime)
				{
					base.InvokeRepeating("Play", this.timeInitial, UnityEngine.Random.Range(this.timeMin, this.timeMax));
				}
			}
		}

		
		public void Play()
		{
			if (this.looping)
			{
				this.PlayLooping();
			}
			else
			{
				this.PlayOneShotSound();
			}
		}

		
		public void PlayWithDelay(float delayTime)
		{
			if (this.looping)
			{
				base.Invoke("PlayLooping", delayTime);
			}
			else
			{
				base.Invoke("PlayOneShotSound", delayTime);
			}
		}

		
		public AudioClip PlayOneShotSound()
		{
			if (!this.audioSource.isActiveAndEnabled)
			{
				return null;
			}
			this.SetAudioSource();
			if (this.stopOnPlay)
			{
				this.audioSource.Stop();
			}
			if (this.disableOnEnd)
			{
				base.Invoke("Disable", this.clip.length);
			}
			this.audioSource.PlayOneShot(this.clip);
			return this.clip;
		}

		
		public AudioClip PlayLooping()
		{
			this.SetAudioSource();
			if (!this.audioSource.loop)
			{
				this.audioSource.loop = true;
			}
			this.audioSource.Play();
			if (this.stopOnEnd)
			{
				base.Invoke("Stop", this.audioSource.clip.length);
			}
			return this.clip;
		}

		
		public void Disable()
		{
			base.gameObject.SetActive(false);
		}

		
		public void Stop()
		{
			this.audioSource.Stop();
		}

		
		private void SetAudioSource()
		{
			if (this.useRandomVolume)
			{
				this.audioSource.volume = UnityEngine.Random.Range(this.volMin, this.volMax);
				if (this.useRandomSilence && (float)UnityEngine.Random.Range(0, 1) < this.percentToNotPlay)
				{
					this.audioSource.volume = 0f;
				}
			}
			if (this.useRandomPitch)
			{
				this.audioSource.pitch = UnityEngine.Random.Range(this.pitchMin, this.pitchMax);
			}
			if (this.waveFile.Length > 0)
			{
				this.audioSource.clip = this.waveFile[UnityEngine.Random.Range(0, this.waveFile.Length)];
				this.clip = this.audioSource.clip;
			}
		}

		
		[Tooltip("List of audio clips to play.")]
		public AudioClip[] waveFile;

		
		[Tooltip("Stops the currently playing clip in the audioSource. Otherwise clips will overlap/mix.")]
		public bool stopOnPlay;

		
		[Tooltip("After the audio clip finishes playing, disable the game object the sound is on.")]
		public bool disableOnEnd;

		
		[Tooltip("Loop the sound after the wave file variation has been chosen.")]
		public bool looping;

		
		[Tooltip("If the sound is looping and updating it's position every frame, stop the sound at the end of the wav/clip length. ")]
		public bool stopOnEnd;

		
		[Tooltip("Start a wave file playing on awake, but after a delay.")]
		public bool playOnAwakeWithDelay;

		
		[Header("Random Volume")]
		public bool useRandomVolume = true;

		
		[Tooltip("Minimum volume that will be used when randomly set.")]
		[Range(0f, 1f)]
		public float volMin = 1f;

		
		[Tooltip("Maximum volume that will be used when randomly set.")]
		[Range(0f, 1f)]
		public float volMax = 1f;

		
		[Header("Random Pitch")]
		[Tooltip("Use min and max random pitch levels when playing sounds.")]
		public bool useRandomPitch = true;

		
		[Tooltip("Minimum pitch that will be used when randomly set.")]
		[Range(-3f, 3f)]
		public float pitchMin = 1f;

		
		[Tooltip("Maximum pitch that will be used when randomly set.")]
		[Range(-3f, 3f)]
		public float pitchMax = 1f;

		
		[Header("Random Time")]
		[Tooltip("Use Retrigger Time to repeat the sound within a time range")]
		public bool useRetriggerTime;

		
		[Tooltip("Inital time before the first repetion starts")]
		[Range(0f, 360f)]
		public float timeInitial;

		
		[Tooltip("Minimum time that will pass before the sound is retriggered")]
		[Range(0f, 360f)]
		public float timeMin;

		
		[Tooltip("Maximum pitch that will be used when randomly set.")]
		[Range(0f, 360f)]
		public float timeMax;

		
		[Header("Random Silence")]
		[Tooltip("Use Retrigger Time to repeat the sound within a time range")]
		public bool useRandomSilence;

		
		[Tooltip("Percent chance that the wave file will not play")]
		[Range(0f, 1f)]
		public float percentToNotPlay;

		
		[Header("Delay Time")]
		[Tooltip("Time to offset playback of sound")]
		public float delayOffsetTime;

		
		private AudioSource audioSource;

		
		private AudioClip clip;
	}
}
