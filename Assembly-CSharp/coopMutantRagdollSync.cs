using System;
using Bolt;
using UnityEngine;

public class coopMutantRagdollSync : EntityBehaviour<IMutantState>
{
	private void Start()
	{
	}

	public override void Attached()
	{
		this.rb = this.rootGo.GetComponent<Rigidbody>();
		base.state.Transform.SetTransforms(this.rootGo.transform);
		base.state.RotationTransform.SetTransforms(this.rootGo.transform);
		if (!base.entity.isOwner)
		{
			this.rb.isKinematic = true;
			this.rb.useGravity = false;
		}
	}

	public GameObject rootGo;

	private Rigidbody rb;
}
