using System;
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
		if (LocalPlayer.vrPlayerControl.RightHand.GetStandardInteractionButton())
		{
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
		this.lastThrowForce = Player.instance.trackingOriginTransform.TransformVector(LocalPlayer.vrPlayerControl.RightHand.controller.velocity + LocalPlayer.MainCam.transform.forward / 2f) * 100f;
		this.lastThrowForceAngular = Player.instance.trackingOriginTransform.TransformVector(LocalPlayer.vrPlayerControl.RightHand.controller.angularVelocity) * 100f;
		this.lastThrowPosition = LocalPlayer.vrPlayerControl.RightHand.transform.position;
		if (this.lastThrowForce.magnitude < this.MinThrowVelocity)
		{
			return;
		}
		LocalPlayer.Inventory.SendMessage("ThrowProjectile");
	}

	
	public float MinThrowVelocity = 10f;

	
	public Vector3 lastThrowForce;

	
	public Vector3 lastThrowForceAngular;

	
	public Vector3 lastThrowPosition;

	
	private bool AllowThrowing;

	
	private bool checkThrowRelease;
}
