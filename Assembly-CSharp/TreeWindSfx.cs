﻿using System;
using FMOD.Studio;
using UnityEngine;

public class TreeWindSfx : MonoBehaviour
{
	private void Awake()
	{
		if (CoopPeerStarter.DedicatedHost)
		{
			UnityEngine.Object.Destroy(this);
		}
		CapsuleCollider component = base.transform.GetComponent<CapsuleCollider>();
		if (component)
		{
			component.height *= 2f;
			component.center = new Vector3(component.center.x, component.center.y * 2f, component.center.z);
		}
	}

	private void OnEnable()
	{
		TreeWindSfxManager.Add(this);
		if (FMOD_StudioSystem.instance)
		{
			EventDescription eventDescription = FMOD_StudioSystem.instance.GetEventDescription(this.EventPath, true);
			if (eventDescription != null)
			{
				if (this.WindParameterIndex == -1)
				{
					this.WindParameterIndex = FMODCommon.FindParameterIndex(eventDescription, "wind");
				}
				if (this.SizeParameterIndex == -1)
				{
					this.SizeParameterIndex = FMODCommon.FindParameterIndex(eventDescription, "size");
				}
				if (this.TimeParameterIndex == -1)
				{
					this.TimeParameterIndex = FMODCommon.FindParameterIndex(eventDescription, "time");
				}
			}
		}
	}

	private void OnDisable()
	{
		TreeWindSfx.StopEvent(this.WindEvent);
		TreeWindSfxManager.Remove(this);
	}

	public static void StopEvent(EventInstance evt)
	{
		if (evt != null && evt.isValid())
		{
			UnityUtil.ERRCHECK(evt.stop(STOP_MODE.ALLOWFADEOUT));
			UnityUtil.ERRCHECK(evt.release());
		}
	}

	public void Activate()
	{
		if (FMOD_StudioSystem.instance)
		{
			if (this.WindEvent == null || !this.WindEvent.isValid())
			{
				this.WindEvent = FMOD_StudioSystem.instance.GetEvent(this.EventPath);
				UnityUtil.ERRCHECK(this.WindEvent.set3DAttributes(UnityUtil.to3DAttributes(base.gameObject, null)));
				if (this.WindParameterIndex >= 0)
				{
					UnityUtil.ERRCHECK(this.WindEvent.setParameterValueByIndex(this.WindParameterIndex, TheForestAtmosphere.Instance.WindIntensity));
				}
				if (this.SizeParameterIndex >= 0)
				{
					UnityUtil.ERRCHECK(this.WindEvent.setParameterValueByIndex(this.SizeParameterIndex, this.size));
				}
				if (this.TimeParameterIndex >= 0)
				{
					UnityUtil.ERRCHECK(this.WindEvent.setParameterValueByIndex(this.TimeParameterIndex, FMOD_StudioEventEmitter.HoursSinceMidnight));
				}
				UnityUtil.ERRCHECK(this.WindEvent.start());
			}
			else
			{
				if (this.WindParameterIndex >= 0)
				{
					UnityUtil.ERRCHECK(this.WindEvent.setParameterValueByIndex(this.WindParameterIndex, TheForestAtmosphere.Instance.WindIntensity));
				}
				if (this.TimeParameterIndex >= 0)
				{
					UnityUtil.ERRCHECK(this.WindEvent.setParameterValueByIndex(this.TimeParameterIndex, FMOD_StudioEventEmitter.HoursSinceMidnight));
				}
			}
			this.WaitingForTimeout = false;
		}
	}

	public void Deactivate(float persistTime)
	{
		if (this.WaitingForTimeout)
		{
			if (Time.time >= this.Timeout)
			{
				TreeWindSfx.StopEvent(this.WindEvent);
				this.WindEvent = null;
			}
		}
		else
		{
			this.Timeout = Time.time + persistTime;
			this.WaitingForTimeout = true;
		}
	}

	public bool IsActive
	{
		get
		{
			return this.WindEvent != null;
		}
	}

	public static EventInstance BeginTransfer(Transform source)
	{
		EventInstance result = null;
		if (source != null)
		{
			TreeWindSfx componentInChildren = source.GetComponentInChildren<TreeWindSfx>();
			if (componentInChildren != null)
			{
				result = componentInChildren.WindEvent;
				componentInChildren.WindEvent = null;
			}
		}
		return result;
	}

	public static void CompleteTransfer(Transform destination, EventInstance windEvent)
	{
		if (windEvent != null)
		{
			TreeWindSfx treeWindSfx = null;
			if (destination != null)
			{
				treeWindSfx = destination.GetComponentInChildren<TreeWindSfx>();
			}
			if (treeWindSfx != null)
			{
				treeWindSfx.WindEvent = windEvent;
			}
			else
			{
				TreeWindSfx.StopEvent(windEvent);
			}
			windEvent = null;
		}
	}

	[Tooltip("Path of FMOD event to play")]
	public string EventPath;

	public float size;

	private EventInstance WindEvent;

	private int WindParameterIndex = -1;

	private int SizeParameterIndex = -1;

	private int TimeParameterIndex = -1;

	private float Timeout;

	private bool WaitingForTimeout;
}
