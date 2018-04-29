using System;
using FMOD.Studio;
using UnityEngine;


public class objectOnWater : MonoBehaviour
{
	
	private void OnEnable()
	{
		if (SteamDSConfig.isDedicatedServer)
		{
			base.enabled = false;
		}
		else if (FMOD_StudioSystem.instance)
		{
			this.onWater = FMOD_StudioSystem.instance.GetEvent(this.onWaterEvent);
		}
		this._bouyancy = base.gameObject.GetComponent<Buoyancy>();
	}

	
	private void Update()
	{
		PLAYBACK_STATE playback_STATE;
		UnityUtil.ERRCHECK(this.onWater.getPlaybackState(out playback_STATE));
		if (!this._bouyancy)
		{
			this._bouyancy = base.gameObject.GetComponent<Buoyancy>();
		}
		if (this._bouyancy.inWaterCounter > 0)
		{
			if (playback_STATE == PLAYBACK_STATE.STOPPED)
			{
				UnityUtil.ERRCHECK(this.onWater.set3DAttributes(UnityUtil.to3DAttributes(base.gameObject, null)));
				UnityUtil.ERRCHECK(this.onWater.start());
			}
			else
			{
				UnityUtil.ERRCHECK(this.onWater.set3DAttributes(UnityUtil.to3DAttributes(base.gameObject, null)));
			}
		}
		else if (playback_STATE == PLAYBACK_STATE.PLAYING || playback_STATE == PLAYBACK_STATE.STARTING)
		{
			UnityUtil.ERRCHECK(this.onWater.stop(STOP_MODE.ALLOWFADEOUT));
		}
	}

	
	private void OnDisable()
	{
		if (this.onWater != null)
		{
			UnityUtil.ERRCHECK(this.onWater.release());
		}
	}

	
	[Header("FMOD EVENT")]
	public string onWaterEvent;

	
	public Buoyancy _bouyancy;

	
	private EventInstance onWater;
}
