using System;
using System.Collections;
using Bolt;
using TheForest.Utils;
using UnityEngine;


public class activateSledPush : EntityBehaviour<IMultiHolderState>
{
	
	private bool CanGrabThisSled()
	{
		BoltEntity componentInParent = base.GetComponentInParent<BoltEntity>();
		if (componentInParent && componentInParent.isAttached && componentInParent.StateIs<IMultiHolderState>())
		{
			IMultiHolderState state = componentInParent.GetState<IMultiHolderState>();
			return state.IsReal && !state.GrabbedBy;
		}
		return false;
	}

	
	private bool playerCanGrabSled()
	{
		return !CoopPeerStarter.DedicatedHost && !LocalPlayer.AnimControl.onRope && !LocalPlayer.FpCharacter.Diving && LocalPlayer.FpCharacter.Grounded && !LocalPlayer.AnimControl.onFire && !LocalPlayer.FpCharacter.drinking && !LocalPlayer.AnimControl.swimming && !LocalPlayer.AnimControl.skinningAnimal && !this.coolDown;
	}

	
	private void SetEnable(bool value)
	{
		if (BoltNetwork.isRunning && base.entity && base.entity.isAttached && base.entity.isOwner && !base.state.IsReal)
		{
			Debug.LogWarning("Can't change enable state for sledpush when sled is not real");
			return;
		}
		base.enabled = value;
	}

	
	private void Start()
	{
		this.col = base.transform.GetComponent<Collider>();
		this.logScript = base.transform.parent.GetComponentInChildren<LogHolder>();
		this.allLookat = base.transform.root.GetComponentsInChildren<lookAtDir>();
		this.hideScript = base.transform.GetComponent<hideSledPushTrigger>();
		foreach (lookAtDir lookAtDir in this.allLookat)
		{
			lookAtDir.enabled = false;
		}
		if (!BoltNetwork.isRunning || (base.entity && base.entity.isAttached && base.state.IsReal))
		{
			this.SetEnable(false);
		}
	}

	
	private void GrabEnter()
	{
		if (!BoltNetwork.isRunning || this.CanGrabThisSled())
		{
			this.inTrigger = true;
			if (this.coolDown)
			{
				return;
			}
			this.logTrigger.SetActive(false);
			this.Sheen.SetActive(false);
			this.MyPickUp.SetActive(!this.onSled);
			this.SetEnable(true);
		}
	}

	
	private void GrabExit()
	{
		if (!BoltNetwork.isRunning || this.CanGrabThisSled())
		{
			this.inTrigger = false;
			if (base.enabled)
			{
				this.logTrigger.SetActive(true);
				this.Sheen.SetActive(true);
				this.MyPickUp.SetActive(false);
			}
			if (!this.onSled)
			{
				this.SetEnable(false);
			}
		}
	}

	
	public void Interraction(bool allow)
	{
		this.inTrigger = allow;
		if (allow)
		{
			this.logTrigger.SetActive(false);
			this.Sheen.SetActive(false);
			this.MyPickUp.SetActive(true);
		}
		else
		{
			this.logTrigger.SetActive(false);
			this.Sheen.SetActive(false);
			this.MyPickUp.SetActive(false);
		}
		base.gameObject.SetActive(allow);
	}

	
	
	private bool distanceCheck
	{
		get
		{
			if (BoltNetwork.isRunning)
			{
				float magnitude = (base.transform.position - LocalPlayer.Transform.position).magnitude;
				return magnitude < 3.7f;
			}
			return true;
		}
	}

	
	private void Update()
	{
		if (!this.playerCanGrabSled())
		{
			this.Sheen.SetActive(false);
			this.MyPickUp.SetActive(false);
			return;
		}
		this.Sheen.SetActive(false);
		this.MyPickUp.SetActive(true);
		if (!this.onSled && !this.coolDown && this.inTrigger && !LocalPlayer.IsInCaves && this.distanceCheck && TheForest.Utils.Input.GetButtonAfterDelay("Take", 0.5f, false))
		{
			if (BoltNetwork.isRunning)
			{
				if (this.CanGrabThisSled())
				{
					this.Sheen.SetActive(false);
					this.MyPickUp.SetActive(false);
					SledGrab sledGrab = SledGrab.Create(GlobalTargets.OnlyServer);
					sledGrab.Player = LocalPlayer.Entity;
					sledGrab.Sled = base.GetComponentInParent<BoltEntity>();
					sledGrab.Send();
				}
			}
			else
			{
				this.enableSled();
			}
		}
		else if (this.onSled)
		{
			if (!this.coolDown && this.inTrigger && TheForest.Utils.Input.GetButtonDown("Take"))
			{
				base.StartCoroutine(this.disableSled(false));
			}
			else if (LocalPlayer.PlayerPushSledAction.currentSled != base.transform)
			{
				base.StartCoroutine(this.disableSled(false));
			}
		}
		else if ((!this.inTrigger || Grabber.FocusedItemGO != base.gameObject) && !this.coolDown)
		{
			this.GrabExit();
		}
		if (this.onSled)
		{
			if (this.flagTrigger.activeSelf)
			{
				this.flagTrigger.SetActive(false);
			}
			if (!this.hideScript.enabled)
			{
				this.hideScript.enabled = true;
			}
		}
		else
		{
			if (!this.flagTrigger.activeSelf)
			{
				this.flagTrigger.SetActive(true);
			}
			if (this.hideScript.enabled)
			{
				this.hideScript.enabled = false;
			}
		}
	}

	
	public void enableSled()
	{
		if (base.transform.root.GetComponent<Rigidbody>())
		{
			LocalPlayer.SpecialActions.SendMessage("enterPushSled", base.transform);
			Grabber.SetFilter(base.gameObject);
			this.Sheen.SetActive(false);
			this.MyPickUp.SetActive(false);
			this.onSled = true;
			this.coolDown = true;
			base.Invoke("resetCoolDown", 1.2f);
		}
	}

	
	public IEnumerator disableSled(bool checkCoolDown = false)
	{
		if (checkCoolDown)
		{
			while (this.coolDown)
			{
				yield return null;
			}
		}
		if (BoltNetwork.isRunning)
		{
			BoltEntity componentInParent = base.GetComponentInParent<BoltEntity>();
			if (componentInParent)
			{
				BoltNetwork.Destroy(componentInParent);
			}
		}
		Grabber.SetFilter(null);
		LocalPlayer.SpecialActions.SendMessage("exitPushSled");
		this.logTrigger.SetActive(true);
		if (!this.flagTrigger.activeSelf)
		{
			this.flagTrigger.SetActive(true);
		}
		if (this.hideScript.enabled)
		{
			this.hideScript.enabled = false;
		}
		this.Sheen.SetActive(false);
		this.MyPickUp.SetActive(false);
		this.onSled = false;
		this.coolDown = true;
		base.Invoke("resetCoolDown", 1.2f);
		yield break;
	}

	
	public void enableSuspension()
	{
		foreach (lookAtDir lookAtDir in this.allLookat)
		{
			lookAtDir.enabled = true;
		}
	}

	
	public void disableSuspension()
	{
		foreach (lookAtDir lookAtDir in this.allLookat)
		{
			lookAtDir.enabled = false;
		}
	}

	
	public void resetTrigger()
	{
		this.onSled = false;
	}

	
	private void resetCoolDown()
	{
		this.coolDown = false;
	}

	
	public GameObject Sheen;

	
	public GameObject MyPickUp;

	
	public bool onSled;

	
	public lookAtDir[] allLookat;

	
	public LogHolder logScript;

	
	private hideSledPushTrigger hideScript;

	
	public GameObject logTrigger;

	
	public GameObject flagTrigger;

	
	private Collider col;

	
	private bool coolDown;

	
	private bool inTrigger;
}
