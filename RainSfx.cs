using System;
using FMOD.Studio;
using UnityEngine;


public class RainSfx : MonoBehaviour
{
	
	private void Awake()
	{
		if (CoopPeerStarter.DedicatedHost)
		{
			base.enabled = false;
		}
	}

	
	private void OnEnable()
	{
		this.eventInstance = FMODCommon.PlayOneshot(this.eventPath, base.transform.position, new object[]
		{
			"wind",
			TheForestAtmosphere.Instance.WindIntensity
		});
	}

	
	private void Update()
	{
		if (this.eventInstance != null)
		{
			UnityUtil.ERRCHECK(this.eventInstance.setParameterValue("wind", TheForestAtmosphere.Instance.WindIntensity));
			if (base.transform.hasChanged)
			{
				UnityUtil.ERRCHECK(this.eventInstance.set3DAttributes(base.transform.position.to3DAttributes()));
				base.transform.hasChanged = false;
			}
		}
	}

	
	private void OnDisable()
	{
		if (this.eventInstance != null)
		{
			UnityUtil.ERRCHECK(this.eventInstance.stop(STOP_MODE.ALLOWFADEOUT));
			UnityUtil.ERRCHECK(this.eventInstance.release());
			this.eventInstance = null;
		}
	}

	
	public string eventPath;

	
	private EventInstance eventInstance;
}
