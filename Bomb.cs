using System;
using Bolt;
using TheForest.Tools;
using UnityEngine;


public class Bomb : EntityBehaviour<IRigidbodyState>
{
	
	public override void Attached()
	{
		Rigidbody component = base.GetComponent<Rigidbody>();
		if (component && !this.entity.isOwner)
		{
			component.isKinematic = true;
			component.useGravity = false;
		}
		if (this.entity.StateIs<IRigidbodyState>() && component)
		{
			base.state.Transform.SetTransforms(base.transform);
		}
	}

	
	private void Start()
	{
		if (!this.SkipAttach && BoltNetwork.isRunning && this.entity && !this.entity.isAttached)
		{
			BoltNetwork.Attach(base.gameObject);
		}
		base.Invoke("Explode", (float)this.WaitTime);
	}

	
	private void Explode()
	{
		this.Explosion.transform.parent = null;
		this.Explosion.SetActive(true);
		if (!BoltNetwork.isClient)
		{
			this.reactTrigger.SetActive(true);
			this.reactTrigger.transform.parent = null;
		}
		bool flag;
		if (this.SkipDestroyIfNotOwner)
		{
			if (!BoltNetwork.isRunning)
			{
				flag = true;
			}
			else
			{
				BoltEntity componentInParent = base.GetComponentInParent<BoltEntity>();
				flag = (!componentInParent || !componentInParent.isAttached || componentInParent.isOwner);
			}
		}
		else
		{
			flag = (!BoltNetwork.isRunning || this.SkipAttach || this.entity.IsOwner());
		}
		if (flag)
		{
			if (this.DestroyTarget)
			{
				UnityEngine.Object.Destroy(this.DestroyTarget, 0.25f);
			}
			else
			{
				BoltEntity componentInParent2 = base.GetComponentInParent<BoltEntity>();
				if (componentInParent2)
				{
					UnityEngine.Object.Destroy(componentInParent2.gameObject, 0.05f);
				}
				else
				{
					UnityEngine.Object.Destroy(base.gameObject, 0.05f);
				}
			}
			EventRegistry.Player.Publish(TfEvent.UsedBomb, null);
		}
	}

	
	private void setWaitTime(int w)
	{
		this.WaitTime = w;
	}

	
	public GameObject Explosion;

	
	public GameObject reactTrigger;

	
	public int WaitTime;

	
	public bool SkipAttach;

	
	public bool SkipDestroyIfNotOwner;

	
	public GameObject DestroyTarget;
}
