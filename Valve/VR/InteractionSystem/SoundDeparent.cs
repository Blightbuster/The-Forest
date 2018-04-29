using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	
	public class SoundDeparent : MonoBehaviour
	{
		
		private void Awake()
		{
			this.thisAudioSource = base.GetComponent<AudioSource>();
		}

		
		private void Start()
		{
			base.gameObject.transform.parent = null;
			if (this.destroyAfterPlayOnce)
			{
				UnityEngine.Object.Destroy(base.gameObject, this.thisAudioSource.clip.length);
			}
		}

		
		public bool destroyAfterPlayOnce = true;

		
		private AudioSource thisAudioSource;
	}
}
