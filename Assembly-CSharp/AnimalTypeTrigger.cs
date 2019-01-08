using System;
using TheForest.Tools;
using TheForest.Utils;
using UnityEngine;

public class AnimalTypeTrigger : MonoBehaviour
{
	private void Awake()
	{
		if (CoopPeerStarter.DedicatedHost)
		{
			UnityEngine.Object.Destroy(this);
		}
		if (!this._viewBasedEvent && !this._enableBasedEvent)
		{
			base.enabled = false;
		}
	}

	private void OnEnable()
	{
		if (CoopPeerStarter.DedicatedHost)
		{
			UnityEngine.Object.Destroy(this);
		}
		if (this._viewBasedEvent)
		{
			this._viewInspectTime = Time.time + 1f;
		}
		if (this._enableBasedEvent)
		{
			this.publishAnimalEvent();
		}
	}

	private void publishAnimalEvent()
	{
		EventRegistry.Player.Publish(TfEvent.InspectedAnimal, this._type);
		this._doneInspect = true;
		UnityEngine.Object.Destroy(this);
	}

	private void publishPlantEvent()
	{
		EventRegistry.Player.Publish(TfEvent.InspectedPlant, this._type);
		this._doneInspect = true;
		UnityEngine.Object.Destroy(this);
	}

	private void Update()
	{
		if (this._viewBasedEvent && !this._doneInspect && LocalPlayer.MainCam)
		{
			if (Time.time > this._viewInspectTime)
			{
				float sqrMagnitude = (LocalPlayer.MainCam.transform.position - base.transform.position).sqrMagnitude;
				if ((sqrMagnitude < 350f && this.targetInView()) || this._targetInViewCheck)
				{
					this._targetInViewCheck = true;
					if (Time.time > this._viewInspectTime2)
					{
						if (this.targetInView())
						{
							if (this._plantType)
							{
								this.publishPlantEvent();
							}
							else
							{
								this.publishAnimalEvent();
							}
						}
						else
						{
							this._viewInspectTime = Time.time + 1f;
							this._targetInViewCheck = false;
						}
					}
				}
				else
				{
					this._viewInspectTime = Time.time + 1f;
					this._viewInspectTime2 = Time.time + 2.5f;
				}
			}
			else
			{
				this._viewInspectTime2 = Time.time + 2.5f;
			}
		}
		if (this._doneInspect || this._enableBasedEvent || this._viewBasedEvent)
		{
			return;
		}
		if (this._inspectLostStartTime > 0f)
		{
			if (Time.realtimeSinceStartup - this._inspectLostStartTime > 1f)
			{
				base.enabled = false;
			}
		}
		else if (Time.realtimeSinceStartup - this._inspectStartTime > 4f)
		{
			EventRegistry.Player.Publish(TfEvent.InspectedAnimal, this._type);
			this._doneInspect = true;
			base.enabled = false;
		}
	}

	private bool targetInView()
	{
		if (!LocalPlayer.MainCam)
		{
			return false;
		}
		Vector3 vector = LocalPlayer.MainCam.WorldToViewportPoint(base.transform.position);
		return vector.z > 0f && vector.z < LocalPlayer.MainCam.farClipPlane && vector.x > 0.3f && vector.x < 0.7f && vector.y > 0.3f && vector.y < 0.7f;
	}

	private void GrabEnter()
	{
		if (this._doneInspect || this._enableBasedEvent || this._viewBasedEvent)
		{
			return;
		}
		this._inspectLostStartTime = -1f;
		if (!base.enabled && !this._doneInspect)
		{
			this._inspectStartTime = Time.realtimeSinceStartup;
			base.enabled = true;
		}
	}

	private void GrabExit()
	{
		if (this._doneInspect || this._enableBasedEvent || this._viewBasedEvent)
		{
			return;
		}
		this._inspectLostStartTime = Time.realtimeSinceStartup;
	}

	public void setupFishType(int fishType)
	{
		if (fishType == 0)
		{
			this._type = AnimalType.Carp;
		}
		if (fishType == 1)
		{
			this._type = AnimalType.Carp;
		}
		if (fishType == 2)
		{
			this._type = AnimalType.ButterFlyFish;
		}
		if (fishType == 3)
		{
			this._type = AnimalType.YellowTail;
		}
		if (fishType == 4)
		{
			this._type = AnimalType.RockBeauty;
		}
		if (fishType == 5)
		{
			this._type = AnimalType.Surgeon;
		}
		if (fishType == 6)
		{
			this._type = AnimalType.Cod;
		}
	}

	public AnimalType _type;

	public bool _enableBasedEvent;

	public bool _viewBasedEvent;

	public bool _plantType;

	private float _inspectStartTime;

	private float _inspectLostStartTime = -1f;

	private bool _doneInspect;

	private bool _startInspectAnimal;

	private float _viewInspectTime;

	private float _viewInspectTime2;

	private bool _targetInViewCheck;
}
