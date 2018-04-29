using System;
using System.Collections.Generic;
using Bolt;
using FMOD;
using FMOD.Studio;
using UnityEngine;


public class FMODCommon
{
	
	public static EventInstance PlayOneshotNetworked(string path, Transform transform, FMODCommon.NetworkRole role)
	{
		return FMODCommon.PlayOneshotInternal(path, transform.position, role, new object[0]);
	}

	
	public static EventInstance PlayOneshot(string path, Transform transform)
	{
		return FMODCommon.PlayOneshotInternal(path, transform.position, FMODCommon.NetworkRole.None, new object[0]);
	}

	
	public static EventInstance PlayOneshot(string path, Vector3 position, FMODCommon.NetworkRole role, params object[] parameterValues)
	{
		return FMODCommon.PlayOneshotInternal(path, position, role, parameterValues);
	}

	
	public static EventInstance PlayOneshot(string path, Vector3 position, params object[] parameterValues)
	{
		return FMODCommon.PlayOneshotInternal(path, position, FMODCommon.NetworkRole.None, parameterValues);
	}

	
	private static int GetParameterIndex(string eventPath, string parameterName)
	{
		EventDescription eventDescription = FMOD_StudioSystem.instance.GetEventDescription(eventPath, true);
		if (eventDescription != null)
		{
			int num;
			UnityUtil.ERRCHECK(eventDescription.getParameterCount(out num));
			for (int i = 0; i < num; i++)
			{
				PARAMETER_DESCRIPTION parameter_DESCRIPTION;
				UnityUtil.ERRCHECK(eventDescription.getParameterByIndex(i, out parameter_DESCRIPTION));
				if (parameter_DESCRIPTION.name == parameterName)
				{
					return i;
				}
			}
		}
		return -1;
	}

	
	private static EventInstance PlayOneshotInternal(string path, Vector3 position, FMODCommon.NetworkRole role, params object[] parameterValues)
	{
		if (path.Length <= 0 || !FMOD_StudioSystem.instance)
		{
			return null;
		}
		if (BoltNetwork.isRunning)
		{
			bool flag = false;
			switch (role)
			{
			case FMODCommon.NetworkRole.None:
				flag = false;
				break;
			case FMODCommon.NetworkRole.Any:
				flag = true;
				break;
			case FMODCommon.NetworkRole.Server:
				flag = BoltNetwork.isServer;
				break;
			}
			if (flag)
			{
				int num = CoopAudioEventDb.FindId(path);
				if (num >= 0)
				{
					int num2 = -1;
					float value = 0f;
					if (parameterValues.Length == 2 && parameterValues[0] is string && parameterValues[1] is float)
					{
						num2 = FMODCommon.GetParameterIndex(path, parameterValues[0] as string);
						value = (float)parameterValues[1];
					}
					if (num2 >= 0)
					{
						FmodOneShotParameter fmodOneShotParameter = FmodOneShotParameter.Create(GlobalTargets.Others, ReliabilityModes.Unreliable);
						fmodOneShotParameter.EventPath = num;
						fmodOneShotParameter.Position = position;
						fmodOneShotParameter.Index = num2;
						fmodOneShotParameter.Value = value;
						fmodOneShotParameter.Send();
					}
					else
					{
						FmodOneShot fmodOneShot = FmodOneShot.Create(GlobalTargets.Others, ReliabilityModes.Unreliable);
						fmodOneShot.EventPath = num;
						fmodOneShot.Position = position;
						fmodOneShot.Send();
					}
				}
				else
				{
					UnityEngine.Debug.LogFormat("Couldn't find event path in CoopAudioEventDb: {0}", new object[]
					{
						path
					});
				}
			}
		}
		if (FMOD_StudioSystem.instance)
		{
			return FMOD_StudioSystem.instance.PlayOneShot(path, position, delegate(EventInstance instance)
			{
				instance.setParameterValue("time", FMOD_StudioEventEmitter.HoursSinceMidnight);
				FMODCommon.SetParameterValues(instance, parameterValues);
				return true;
			});
		}
		return null;
	}

	
	private static void SetParameterValues(EventInstance instance, object[] parameterValues)
	{
		string text = null;
		for (int i = 0; i < parameterValues.Length; i++)
		{
			if (i % 2 == 0)
			{
				text = (parameterValues[i] as string);
			}
			else if (text != null && text.Length > 0 && parameterValues[i] is float)
			{
				instance.setParameterValue(text, (float)parameterValues[i]);
			}
		}
	}

	
	public static void StopEvent(EventInstance instance)
	{
		if (instance != null)
		{
			RESULT result = instance.stop(STOP_MODE.ALLOWFADEOUT);
			if (result != RESULT.ERR_INVALID_HANDLE)
			{
				UnityUtil.ERRCHECK(result);
			}
		}
	}

	
	public static void PreloadEvents(params string[] paths)
	{
		foreach (string text in paths)
		{
			if (text != null)
			{
				FMOD_Listener.Preload(text);
			}
		}
	}

	
	public static void UnloadEvents(params string[] paths)
	{
		foreach (string text in paths)
		{
			if (text != null)
			{
				FMOD_Listener.UnPreload(text);
			}
		}
	}

	
	public static int CountChannels(ChannelGroup group)
	{
		int num = 0;
		UnityUtil.ERRCHECK(group.getNumChannels(out num));
		int num2 = 0;
		UnityUtil.ERRCHECK(group.getNumGroups(out num2));
		for (int i = 0; i < num2; i++)
		{
			ChannelGroup group2;
			UnityUtil.ERRCHECK(group.getGroup(i, out group2));
			num += FMODCommon.CountChannels(group2);
		}
		return num;
	}

	
	public static void CleanupOneshotEvents(List<FMODCommon.OneshotEventInfo> oneshotEvents, bool useMaximumAge)
	{
		int i = 0;
		while (i < oneshotEvents.Count)
		{
			FMODCommon.OneshotEventInfo oneshotEventInfo = oneshotEvents[i];
			PLAYBACK_STATE playback_STATE;
			UnityUtil.ERRCHECK(oneshotEventInfo.instance.getPlaybackState(out playback_STATE));
			bool flag = false;
			if (playback_STATE == PLAYBACK_STATE.STOPPED)
			{
				flag = true;
			}
			if (!flag)
			{
				if (oneshotEventInfo.channelGroup == null)
				{
					oneshotEventInfo.instance.getChannelGroup(out oneshotEventInfo.channelGroup);
				}
				if (oneshotEventInfo.channelGroup != null)
				{
					int num = FMODCommon.CountChannels(oneshotEventInfo.channelGroup);
					if (num > 0)
					{
						oneshotEventInfo.hasStarted = true;
					}
					else if (oneshotEventInfo.hasStarted)
					{
						UnityUtil.ERRCHECK(oneshotEventInfo.instance.stop(STOP_MODE.IMMEDIATE));
						flag = true;
					}
				}
			}
			if (!flag && useMaximumAge && oneshotEventInfo.useMaximumAge)
			{
				float num2 = Time.time - oneshotEventInfo.startTime;
				if (num2 > 10f)
				{
					UnityUtil.ERRCHECK(oneshotEventInfo.instance.stop(STOP_MODE.IMMEDIATE));
					flag = true;
				}
			}
			if (flag)
			{
				UnityUtil.ERRCHECK(oneshotEventInfo.instance.release());
				oneshotEvents.RemoveAt(i);
			}
			else
			{
				i++;
			}
		}
	}

	
	public static void AdoptOneshotEvents(List<FMODCommon.OneshotEventInfo> oneshotEvents)
	{
		if (oneshotEvents != null && oneshotEvents.Count > 0)
		{
			foreach (FMODCommon.OneshotEventInfo item in oneshotEvents)
			{
				FMODCommon.adoptedEvents.Add(item);
			}
			oneshotEvents.Clear();
			FMOD_Listener.FMODCommonUpdate = new Action(FMODCommon.CleanupAdoptedEvents);
		}
	}

	
	private static void CleanupAdoptedEvents()
	{
		if (Time.time - FMODCommon.lastCleanupTime >= 1f)
		{
			FMODCommon.lastCleanupTime = Time.time;
			FMODCommon.CleanupOneshotEvents(FMODCommon.adoptedEvents, true);
			if (FMODCommon.adoptedEvents.Count == 0)
			{
				FMOD_Listener.FMODCommonUpdate = null;
			}
		}
	}

	
	public static void UpdateLoopingEvents(List<FMODCommon.LoopingEventInfo> loopingEvents, Animator animator, Transform transform)
	{
		bool flag = animator.IsInTransition(0);
		AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
		FMOD.Studio.ATTRIBUTES_3D attributes = default(FMOD.Studio.ATTRIBUTES_3D);
		if (transform.hasChanged)
		{
			attributes = transform.to3DAttributes();
		}
		int i = 0;
		while (i < loopingEvents.Count)
		{
			FMODCommon.LoopingEventInfo loopingEventInfo = loopingEvents[i];
			if (flag || currentAnimatorStateInfo.fullPathHash != loopingEventInfo.startState)
			{
				UnityUtil.ERRCHECK(loopingEventInfo.instance.stop(STOP_MODE.ALLOWFADEOUT));
				UnityUtil.ERRCHECK(loopingEventInfo.instance.release());
				loopingEvents.RemoveAt(i);
			}
			else
			{
				if (transform.hasChanged)
				{
					UnityUtil.ERRCHECK(loopingEventInfo.instance.set3DAttributes(attributes));
				}
				i++;
			}
		}
	}

	
	public static void ReleaseIfValid(EventInstance evt, STOP_MODE stopMode = STOP_MODE.IMMEDIATE)
	{
		if (evt != null && evt.isValid())
		{
			UnityUtil.ERRCHECK(evt.stop(stopMode));
			UnityUtil.ERRCHECK(evt.release());
		}
	}

	
	public static int FindParameterIndex(EventDescription eventDescription, string parameterName)
	{
		int num = 0;
		UnityUtil.ERRCHECK(eventDescription.getParameterCount(out num));
		for (int i = 0; i < num; i++)
		{
			PARAMETER_DESCRIPTION parameter_DESCRIPTION;
			UnityUtil.ERRCHECK(eventDescription.getParameterByIndex(i, out parameter_DESCRIPTION));
			if (parameter_DESCRIPTION.name == parameterName)
			{
				return i;
			}
		}
		return -1;
	}

	
	private const float ONESHOT_MAXIMUM_AGE = 10f;

	
	private const float CLEANUP_PERIOD = 1f;

	
	private static List<FMODCommon.OneshotEventInfo> adoptedEvents = new List<FMODCommon.OneshotEventInfo>();

	
	private static float lastCleanupTime = 0f;

	
	public enum NetworkRole
	{
		
		None,
		
		Any,
		
		Server
	}

	
	public struct OneshotEventInfo
	{
		
		public OneshotEventInfo(EventInstance _instance, bool _useMaximumAge = true)
		{
			this.instance = _instance;
			this.channelGroup = null;
			this.hasStarted = false;
			this.startTime = Time.time;
			this.useMaximumAge = _useMaximumAge;
		}

		
		public EventInstance instance;

		
		public ChannelGroup channelGroup;

		
		public bool hasStarted;

		
		public float startTime;

		
		public bool useMaximumAge;
	}

	
	public struct LoopingEventInfo
	{
		
		public LoopingEventInfo(string _path, EventInstance _instance, int _startState)
		{
			this.path = _path;
			this.instance = _instance;
			this.startState = _startState;
		}

		
		public string path;

		
		public EventInstance instance;

		
		public int startState;
	}
}
