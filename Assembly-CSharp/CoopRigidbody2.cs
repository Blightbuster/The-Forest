﻿using System;
using Bolt;
using UnityEngine;

public class CoopRigidbody2 : EntityBehaviour<IRigidbodyState>
{
	public override void Attached()
	{
		if (!this.targetRigidbody)
		{
			this.targetRigidbody = base.GetComponent<Rigidbody>();
		}
		if (this.targetRigidbody)
		{
			if (!base.entity.isOwner)
			{
				this.targetRigidbody.useGravity = false;
				this.targetRigidbody.isKinematic = true;
			}
			base.state.Transform.SetTransforms(this.targetRigidbody.transform);
		}
		else
		{
			Debug.LogErrorFormat("no rigidbody found on {0}, object will not replicate its position in multiplayer", new object[]
			{
				base.gameObject.name
			});
		}
	}

	[SerializeField]
	private Rigidbody targetRigidbody;
}
