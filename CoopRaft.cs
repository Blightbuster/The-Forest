using System;
using System.Collections;
using Bolt;
using UnityEngine;


public class CoopRaft : EntityBehaviour<IRaftState>
{
	
	public override void Attached()
	{
		if (this.entity.IsOwner())
		{
			base.state.Transform.SetTransforms(base.transform);
		}
		else
		{
			this.raft_rigidbody.isKinematic = true;
			this.client_transform = new GameObject("client_raft_transform");
			base.state.Transform.SetTransforms(this.client_transform.transform);
			base.StartCoroutine(this.ClientPushEnabler(1f));
		}
	}

	
	public override void Detached()
	{
		if (this.client_transform)
		{
			UnityEngine.Object.Destroy(this.client_transform);
		}
	}

	
	private bool IsBeingDriven()
	{
		if (this.entity.isAttached)
		{
			foreach (BoltEntity exists in base.state.GrabbedBy)
			{
				if (exists)
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	
	private void Start()
	{
		base.enabled = BoltNetwork.isRunning;
	}

	
	private void LateUpdate()
	{
		if (this.entity.IsAttached() && !this.entity.IsOwner())
		{
			if (this.interpolate)
			{
				this.raft_rigidbody.isKinematic = this.IsBeingDriven();
				base.transform.position = Vector3.Slerp(base.transform.position, this.client_transform.transform.position, Time.deltaTime);
				base.transform.rotation = Quaternion.Slerp(base.transform.rotation, this.client_transform.transform.rotation, Time.deltaTime);
			}
			else
			{
				base.transform.position = this.client_transform.transform.position;
				base.transform.rotation = this.client_transform.transform.rotation;
			}
		}
	}

	
	private IEnumerator ClientPushEnabler(float waitTime)
	{
		this.interpolate = false;
		yield return new WaitForSeconds(waitTime);
		this.interpolate = true;
		yield break;
	}

	
	private bool interpolate;

	
	private GameObject client_transform;

	
	[SerializeField]
	private Rigidbody raft_rigidbody;
}
