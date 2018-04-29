using System;
using UnityEngine;


public class fixMolotovAudio : MonoBehaviour
{
	
	private void Start()
	{
		base.Invoke("initAudioEmitter", 1f);
	}

	
	private void initAudioEmitter()
	{
		FMOD_StudioEventEmitter component = base.transform.GetComponent<FMOD_StudioEventEmitter>();
		if (component)
		{
			component.enabled = true;
			component.startEventOnAwake = true;
		}
	}
}
