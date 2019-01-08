using System;
using FMOD.Studio;
using UnityEngine;

public class ArtifactAudioState : MonoBehaviour
{
	private void PreprocessFMODEvent(EventInstance eventInstance)
	{
		this.lastEvent = eventInstance;
		this.ApplyToLastEvent();
	}

	private void ApplyToLastEvent()
	{
		if (this.lastEvent != null && this.lastEvent.isValid())
		{
			this.lastEvent.setParameterValue("body", (!this.bodyPresent) ? 0f : 1f);
			this.lastEvent.setParameterValue("working", (!this.working) ? 0f : 1f);
		}
	}

	public void SetBodyPresent(bool present)
	{
		this.bodyPresent = present;
		this.ApplyToLastEvent();
	}

	public void SetWorking(bool working)
	{
		this.working = working;
		this.ApplyToLastEvent();
	}

	private bool bodyPresent = true;

	private bool working;

	private EventInstance lastEvent;
}
