using System;
using Bolt;
using TheForest.Utils;
using UnityEngine;

public class activateHangGlider : EntityBehaviour<IDynamicPickup>
{
	private void GrabEnter(GameObject grabber)
	{
		if (!LocalPlayer.FpCharacter.Sitting)
		{
			if (Vector3.Distance(base.transform.position, LocalPlayer.Transform.position) < 5f)
			{
				this.Sheen.SetActive(false);
				this.MyPickUp.SetActive(true);
			}
			else
			{
				this.Sheen.SetActive(true);
				this.MyPickUp.SetActive(false);
			}
			base.enabled = true;
		}
	}

	private void GrabExit(GameObject grabber)
	{
		if (base.enabled)
		{
			base.enabled = false;
			this.Sheen.SetActive(true);
			this.MyPickUp.SetActive(false);
		}
	}

	private void Update()
	{
		if (CoopPeerStarter.DedicatedHost)
		{
			return;
		}
		if (LocalPlayer.AnimControl.holdingGlider)
		{
			return;
		}
		float num = Vector3.Distance(base.transform.position, LocalPlayer.Transform.position);
		if (LocalPlayer.AnimControl.onRope || num > 5f)
		{
			this.MyPickUp.SetActive(false);
			this.Sheen.SetActive(true);
			return;
		}
		this.Sheen.SetActive(false);
		this.MyPickUp.SetActive(true);
		if (TheForest.Utils.Input.GetButtonDown("Take"))
		{
			this.SendPickupGlider();
		}
	}

	private void SendPickupGlider()
	{
		LocalPlayer.SpecialActions.SendMessage("pickupGlider");
		if (BoltNetwork.isRunning && base.entity && base.entity.isAttached && !base.entity.isOwner)
		{
			PickupGlider pickupGlider = PickupGlider.Create(GlobalTargets.Others);
			pickupGlider.targetEntity = base.entity;
			pickupGlider.Send();
		}
		else
		{
			UnityEngine.Object.Destroy(this.destroyTarget);
		}
		base.enabled = false;
	}

	public GameObject Sheen;

	public GameObject MyPickUp;

	public GameObject destroyTarget;
}
