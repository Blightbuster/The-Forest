using System;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class VRThrowable : MonoBehaviour
{
	private void Start()
	{
		if (!ForestVR.Enabled)
		{
			base.enabled = false;
			return;
		}
	}

	private void Update()
	{
		if (LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.World)
		{
			return;
		}
		if (LocalPlayer.vrPlayerControl.RightHand.GetStandardInteractionButton())
		{
			if (!this.checkThrowRelease)
			{
				this.ThrowProjectile();
			}
			this.checkThrowRelease = true;
		}
		else if (this.checkThrowRelease)
		{
			this.ThrowProjectile();
			this.checkThrowRelease = false;
		}
	}

	private void ThrowProjectile()
	{
		this.lastThrowForce = Player.instance.trackingOriginTransform.TransformVector(LocalPlayer.vrPlayerControl.RightHand.controller.velocity + LocalPlayer.MainCam.transform.forward / 2f) * 100f * this.ThrowForceMultiply;
		if (!this.IgnoreAngularVelocity)
		{
			this.lastThrowForceAngular = Player.instance.trackingOriginTransform.TransformVector(LocalPlayer.vrPlayerControl.RightHand.controller.angularVelocity) * 100f;
		}
		this.lastThrowPosition = LocalPlayer.vrPlayerControl.RightHand.transform.position;
		if (this.lastThrowForce.magnitude < this.MinThrowVelocity)
		{
			return;
		}
		if (this.SpearType)
		{
			LocalPlayer.ScriptSetup.events.throwSpear();
		}
		else
		{
			LocalPlayer.Inventory.SendMessage("ThrowProjectile");
		}
	}

	private float MinThrowVelocity = 500f;

	public float ThrowForceMultiply = 1f;

	public bool IgnoreAngularVelocity;

	public bool SpearType;

	[HideInInspector]
	public Vector3 lastThrowForce;

	[HideInInspector]
	public Vector3 lastThrowForceAngular;

	[HideInInspector]
	public Vector3 lastThrowPosition;

	private bool AllowThrowing;

	private bool checkThrowRelease;
}
