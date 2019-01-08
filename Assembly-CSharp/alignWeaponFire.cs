using System;
using TheForest.Utils;
using UnityEngine;

public class alignWeaponFire : MonoBehaviour
{
	private void Awake()
	{
		if (CoopPeerStarter.DedicatedHost)
		{
			base.enabled = false;
		}
	}

	private void LateUpdate()
	{
		if (LocalPlayer.MainCamTr && !this.target)
		{
			this.target = LocalPlayer.MainCamTr.Find("followMe").transform;
		}
		if (this.net)
		{
			if (this.target)
			{
				this.rotateTransform.rotation = this.target.rotation;
			}
			return;
		}
		this.target = LocalPlayer.MainCamTr;
		this.camLookAtTarget.transform.position = this.target.position;
		Vector3 localPosition = this.camLookAtTarget.localPosition;
		localPosition.y = this.rotateTransform.localPosition.y;
		this.camLookAtTarget.localPosition = localPosition;
		this.rotateTransform.LookAt(this.camLookAtTarget, this.rotateTransform.parent.up);
	}

	public Transform target;

	public Transform camLookAtTarget;

	public Transform rotateTransform;

	public float rotOffset = 90f;

	public float driftYOffset = 0.6f;

	public bool net;
}
