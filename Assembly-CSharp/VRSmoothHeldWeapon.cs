using System;
using UnityEngine;

public class VRSmoothHeldWeapon : MonoBehaviour
{
	private void OnEnable()
	{
		if (!ForestVR.Enabled)
		{
			base.enabled = false;
			return;
		}
		if (this.followTransform == null)
		{
			this.followTransform = new GameObject("tempFollow").transform;
		}
		this.offsetTransform = new GameObject("tempOffset").transform;
		this.aimTransform = new GameObject("tempAim").transform;
		this.offsetTransform.parent = base.transform.parent;
		this.offsetTransform.localPosition = Vector3.zero;
		this.offsetTransform.localRotation = Quaternion.identity;
		this.followTransform.position = base.transform.position;
		this.followTransform.rotation = base.transform.rotation;
		this.followTransform.parent = this.offsetTransform;
		this.aimTransform.parent = base.transform.parent;
		this.aimTransform.position = this.offsetTransform.position;
		this.aimTransform.rotation = this.offsetTransform.rotation;
		this.smoothRot = this.offsetTransform.rotation;
	}

	private void OnDisable()
	{
		if (this.followTransform != null)
		{
			UnityEngine.Object.Destroy(this.followTransform.gameObject);
		}
		if (this.offsetTransform != null)
		{
			UnityEngine.Object.Destroy(this.offsetTransform.gameObject);
		}
		if (this.aimTransform != null)
		{
			UnityEngine.Object.Destroy(this.aimTransform.gameObject);
		}
	}

	private void LateUpdate()
	{
		if (this.followTransform != null)
		{
			this.smoothRot = Quaternion.Lerp(this.smoothRot, this.aimTransform.rotation, Time.deltaTime * this.smoothTime);
			this.offsetTransform.rotation = this.smoothRot;
			base.transform.position = this.followTransform.position;
			base.transform.rotation = this.followTransform.rotation;
		}
	}

	public Transform followTransform;

	public Transform offsetTransform;

	private Transform aimTransform;

	private Quaternion smoothRot;

	public float smoothTime;

	private int skipFrameCount;
}
