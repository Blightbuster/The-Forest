using System;
using TheForest.Utils;
using UnityEngine;


public class alignMolotovFire : MonoBehaviour
{
	
	private void Awake()
	{
		if (CoopPeerStarter.DedicatedHost)
		{
			base.enabled = false;
		}
		else
		{
			this.origParent = base.transform.parent;
		}
	}

	
	private void Start()
	{
		this.pos = base.transform.position;
	}

	
	private void LateUpdate()
	{
		if (LocalPlayer.MainCamTr && !this.target)
		{
			this.target = LocalPlayer.MainCam.transform.Find("followMe").transform;
		}
		if (this.net)
		{
			if (this.target)
			{
				this.origParent.rotation = this.target.rotation;
			}
			return;
		}
		this.origParent.rotation = this.target.rotation;
		this.pos = Vector3.Slerp(this.pos, this.dummyTarget.position, Time.deltaTime * 18f);
		this.followTarget.position = this.pos;
		if (Vector3.Distance(this.followTarget.position, this.dummyTarget.position) > this.followDistance)
		{
			Vector3 vector = this.followTarget.position - this.dummyTarget.position;
			vector = Vector3.ClampMagnitude(vector, this.followDistance);
			this.followTarget.position = this.dummyTarget.position + vector;
		}
		base.transform.localPosition = Vector3.Slerp(base.transform.localPosition, this.followTarget.localPosition, Time.deltaTime * this.smoothTime);
	}

	
	public Transform target;

	
	public Transform followTarget;

	
	public Transform dummyTarget;

	
	public float xOffset;

	
	private Transform origParent;

	
	private Vector3 pos;

	
	public float smoothTime = 15f;

	
	public float followDistance = 0.25f;

	
	public bool net;
}
