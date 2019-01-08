using System;
using FMOD.Studio;
using UnityEngine;

public class snapshotTrigger : MonoBehaviour
{
	private void OnEnable()
	{
		Debug.Log("Event Enabled");
		FMOD_StudioEventEmitter component = base.GetComponent<FMOD_StudioEventEmitter>();
		component.preStartAction = delegate(EventInstance evt)
		{
			evt.setParameterValue("Intensity", this.intensityLevel);
			Debug.Log("parameter set to: " + this.intensityLevel);
		};
	}

	private void Update()
	{
	}

	[Header("Parameter Controls")]
	[Range(0f, 100f)]
	public float intensityLevel;
}
