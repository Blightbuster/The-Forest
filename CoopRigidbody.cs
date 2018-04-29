using System;
using Bolt;
using UnityEngine;


public class CoopRigidbody : EntityBehaviour<IRigidbodyState>
{
	
	public override void Attached()
	{
		if (!base.entity.isOwner)
		{
			foreach (Collider collider in base.GetComponentsInChildren<Collider>())
			{
				collider.enabled = collider.isTrigger;
			}
			foreach (Rigidbody rigidbody in base.GetComponentsInChildren<Rigidbody>())
			{
				rigidbody.useGravity = false;
				rigidbody.isKinematic = true;
			}
		}
		base.state.Transform.SetTransforms(this.targetRigidbody.transform);
	}

	
	[SerializeField]
	private Rigidbody targetRigidbody;
}
