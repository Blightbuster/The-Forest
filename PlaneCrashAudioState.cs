using System;
using FMOD.Studio;
using TheForest.Utils;
using UnityEngine;


public class PlaneCrashAudioState : MonoBehaviour
{
	
	public static void Spawn()
	{
		if (PlaneCrashAudioState.sInstance != null)
		{
			return;
		}
		EventInstance eventInstance = null;
		if (FMOD_StudioSystem.instance)
		{
			eventInstance = FMOD_StudioSystem.instance.GetEvent("snapshot:/amb_off");
		}
		if (eventInstance == null)
		{
			return;
		}
		GameObject gameObject = new GameObject("Plane Crash Audio State");
		PlaneCrashAudioState.sInstance = gameObject.AddComponent<PlaneCrashAudioState>();
		PlaneCrashAudioState.sInstance.snapshotInstance = eventInstance;
		UnityUtil.ERRCHECK(eventInstance.start());
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
	}

	
	private void OnDisable()
	{
		if (this.snapshotInstance != null)
		{
			UnityUtil.ERRCHECK(this.snapshotInstance.stop(STOP_MODE.ALLOWFADEOUT));
			UnityUtil.ERRCHECK(this.snapshotInstance.release());
		}
		if (PlaneCrashAudioState.sInstance == this)
		{
			PlaneCrashAudioState.sInstance = null;
		}
	}

	
	public static void Disable()
	{
		if (PlaneCrashAudioState.sInstance != null)
		{
			UnityEngine.Object.Destroy(PlaneCrashAudioState.sInstance.gameObject);
		}
	}

	
	private void OnLevelWasLoaded()
	{
		if (Application.loadedLevelName == "TitleScene")
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else if (Scene.PlaneCrashAnimGO && !Scene.PlaneCrashAnimGO.activeSelf)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	
	private EventInstance snapshotInstance;

	
	private static PlaneCrashAudioState sInstance;
}
