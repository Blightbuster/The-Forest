using System;
using System.Collections;
using Bolt;
using PathologicalGames;
using UnityEngine;

public class FireParticle : EntityBehaviour<IFireParticle>
{
	public override void Attached()
	{
		if (base.entity && base.entity.isAttached && base.state != null)
		{
			base.state.Transform.SetTransforms(base.transform);
		}
	}

	private void OnEnable()
	{
		base.StartCoroutine(this.DelayedEnable(BoltNetwork.isRunning && base.entity && !base.entity.isAttached));
	}

	private void OnDisable()
	{
		this.Parent = null;
	}

	private IEnumerator DelayedEnable(bool delay)
	{
		if (delay)
		{
			yield return null;
		}
		if (BoltNetwork.isRunning && base.entity && !base.entity.isAttached)
		{
			BoltNetwork.Attach(base.gameObject);
		}
		if ((!BoltNetwork.isRunning || (base.entity && base.entity.isAttached && base.entity.isOwner)) && this.MyFuel <= 0f)
		{
			this.MyFuel = 20f;
		}
		yield break;
	}

	private void Update()
	{
		if (!BoltNetwork.isRunning || (base.entity && base.entity.isAttached && base.entity.isOwner))
		{
			if (this.MyFuel > 0f)
			{
				this.Burning = true;
				this.MyFuel -= 1f * Time.deltaTime;
			}
			else if (this.Burning)
			{
				this.EndFire();
			}
		}
	}

	private void EndFire()
	{
		if (this.Parent)
		{
			this.Parent.ExtinguishPoint(this);
		}
		if (BoltNetwork.isRunning && base.entity && base.entity.isAttached && base.entity.isOwner)
		{
			BoltNetwork.Detach(base.gameObject);
		}
		SpawnPool spawnPool = PoolManager.Pools["Particles"];
		if (spawnPool.IsSpawned(base.transform))
		{
			spawnPool.Despawn(base.transform);
		}
		else if (base.transform.parent)
		{
			base.gameObject.SetActive(false);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public float MyFuel;

	[HideInInspector]
	public FireDamage Parent;

	private bool Burning;
}
