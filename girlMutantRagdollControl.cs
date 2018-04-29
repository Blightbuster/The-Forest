using System;
using System.Collections.Generic;
using UnityEngine;


public class girlMutantRagdollControl : MonoBehaviour
{
	
	private void Start()
	{
	}

	
	private void enableGirlBodyRagdoll()
	{
	}

	
	private void OnAnimatorMove()
	{
		if (this.doRagdoll)
		{
			this.updateRagdollJoints();
		}
	}

	
	private void Update()
	{
		if (this.doRagdoll)
		{
			this.updateRagdollJoints();
		}
	}

	
	private void LateUpdate()
	{
		if (this.doRagdoll)
		{
			this.updateRagdollJoints();
		}
	}

	
	private void updateRagdollJoints()
	{
		for (int i = 0; i < this.targetJoints.Count; i++)
		{
			this.targetJoints[i].rotation = this.ragdollJoints[i].rotation;
		}
	}

	
	public List<Transform> ragdollJoints = new List<Transform>();

	
	public List<Transform> targetJoints = new List<Transform>();

	
	public bool doRagdoll;
}
