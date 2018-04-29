using System;
using Bolt;
using UnityEngine;


public class coopDynamicSmooth : EntityBehaviour<IRigidbodyState>
{
	
	public override void Attached()
	{
		this.smoothPos = base.transform.localPosition;
		this.smoothRot = base.transform.rotation;
	}

	
	private void Start()
	{
		this.smoothSpeed = 10f;
		this.smoothPos = base.transform.localPosition;
		this.smoothRot = base.transform.rotation;
	}

	
	private void LateUpdate()
	{
		if ((this.updateOnClientOnly && BoltNetwork.isClient) || (BoltNetwork.isRunning && this.entity && this.entity.isAttached && !this.entity.isOwner))
		{
			if ((this.smoothPos - base.transform.localPosition).sqrMagnitude < 225f)
			{
				this.smoothPos = Vector3.Lerp(this.smoothPos, base.transform.localPosition, Time.deltaTime * this.smoothSpeed);
				base.transform.localPosition = this.smoothPos;
			}
			else
			{
				this.smoothPos = base.transform.localPosition;
			}
			if (this.smoothRotation)
			{
				this.smoothRot = Quaternion.Lerp(this.smoothRot, base.transform.rotation, Time.deltaTime * this.smoothSpeed);
				base.transform.rotation = this.smoothRot;
			}
		}
	}

	
	private Vector3 smoothPos;

	
	private Quaternion smoothRot;

	
	private float smoothSpeed = 10f;

	
	public bool smoothRotation;

	
	public bool updateOnClientOnly;
}
