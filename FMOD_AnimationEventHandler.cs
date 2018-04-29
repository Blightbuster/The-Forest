using System;
using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine;


public class FMOD_AnimationEventHandler : MonoBehaviour
{
	
	private void Awake()
	{
		if (CoopPeerStarter.DedicatedHost)
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	
	private void Start()
	{
		if (!this.eventTransform)
		{
			this.eventTransform = base.transform;
		}
		base.InvokeRepeating("CleanupOneshotEvents", 1f, 1f);
	}

	
	private void OnEnable()
	{
		this.oneshotEvents = new List<FMODCommon.OneshotEventInfo>();
		this.loopingEvents = new List<FMODCommon.LoopingEventInfo>();
		this.animator = base.GetComponent<Animator>();
	}

	
	private void OnDisable()
	{
		if (this.loopingEvents != null)
		{
			int count = this.loopingEvents.Count;
			for (int i = 0; i < count; i++)
			{
				FMODCommon.LoopingEventInfo loopingEventInfo = this.loopingEvents[i];
				if (loopingEventInfo.instance.isValid())
				{
					UnityUtil.ERRCHECK(loopingEventInfo.instance.stop(STOP_MODE.ALLOWFADEOUT));
					UnityUtil.ERRCHECK(loopingEventInfo.instance.release());
				}
			}
			this.loopingEvents.Clear();
		}
		if (this.oneshotEvents != null)
		{
			int count2 = this.oneshotEvents.Count;
			for (int j = 0; j < count2; j++)
			{
				FMODCommon.OneshotEventInfo oneshotEventInfo = this.oneshotEvents[j];
				if (oneshotEventInfo.instance.isValid())
				{
					UnityUtil.ERRCHECK(oneshotEventInfo.instance.stop(STOP_MODE.ALLOWFADEOUT));
					UnityUtil.ERRCHECK(oneshotEventInfo.instance.release());
				}
			}
			this.oneshotEvents.Clear();
		}
	}

	
	private void Update()
	{
		if (this.animator != null)
		{
			FMODCommon.UpdateLoopingEvents(this.loopingEvents, this.animator, this.eventTransform);
		}
		if (this.eventTransform.hasChanged && this.oneshotsFollowTransform)
		{
			int count = this.oneshotEvents.Count;
			for (int i = 0; i < count; i++)
			{
				UnityUtil.ERRCHECK(this.oneshotEvents[i].instance.set3DAttributes(this.eventTransform.to3DAttributes()));
			}
		}
		this.eventTransform.hasChanged = false;
	}

	
	private void CleanupOneshotEvents()
	{
		FMODCommon.CleanupOneshotEvents(this.oneshotEvents, this.stopOneshotsAfterTimeout);
	}

	
	private void playFMODEvent(string path)
	{
		this.playFMODEvent(path, true);
	}

	
	private void playFMODEventNoTimeout(string path)
	{
		this.playFMODEvent(path, false);
	}

	
	private void playFMODEvent(string path, bool enableTimeout)
	{
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		if (!FMOD_StudioSystem.instance || FMOD_StudioSystem.instance.ShouldBeCulled(path, this.eventTransform.position))
		{
			return;
		}
		EventDescription eventDescription = FMOD_StudioSystem.instance.GetEventDescription(path, true);
		if (eventDescription == null)
		{
			Debug.LogWarning("Couldn't get event '" + path + "'");
			return;
		}
		bool flag;
		UnityUtil.ERRCHECK(eventDescription.isOneshot(out flag));
		int num = 0;
		if (this.animator != null)
		{
			num = this.animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
		}
		if (!flag)
		{
			for (int i = 0; i < this.loopingEvents.Count; i++)
			{
				FMODCommon.LoopingEventInfo loopingEventInfo = this.loopingEvents[i];
				if (loopingEventInfo.startState == num && loopingEventInfo.path == path)
				{
					return;
				}
			}
		}
		EventInstance eventInstance;
		UnityUtil.ERRCHECK(eventDescription.createInstance(out eventInstance));
		UnityUtil.ERRCHECK(eventInstance.set3DAttributes(this.eventTransform.to3DAttributes()));
		UnityUtil.ERRCHECK(eventInstance.start());
		if (flag)
		{
			this.oneshotEvents.Add(new FMODCommon.OneshotEventInfo(eventInstance, enableTimeout));
		}
		else
		{
			this.loopingEvents.Add(new FMODCommon.LoopingEventInfo(path, eventInstance, num));
		}
	}

	
	public Transform eventTransform;

	
	public bool oneshotsFollowTransform;

	
	[Tooltip("Stop oneshots after 10 seconds in case they don't stop by themselves")]
	public bool stopOneshotsAfterTimeout = true;

	
	private List<FMODCommon.OneshotEventInfo> oneshotEvents;

	
	private List<FMODCommon.LoopingEventInfo> loopingEvents;

	
	private Animator animator;
}
