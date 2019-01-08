using System;
using Bolt;
using PathologicalGames;
using UnityEngine;

public class BushDamage : EntityBehaviour
{
	private void Awake()
	{
		this.StartHealth = this.Health;
	}

	private void OnEnable()
	{
		this.Health = this.StartHealth;
	}

	public void SetLodBase(LOD_Base lb)
	{
		this.LodBase = lb;
	}

	private void Hit(int damage)
	{
		if (this.snowType && PoolManager.Pools.ContainsKey("Particles"))
		{
			PoolManager.Pools["Particles"].Spawn(this.snowBurst, this.MyBurstPos.position, Quaternion.identity);
		}
		UnityEngine.Object.Instantiate<Transform>(this.Burst, this.MyBurstPos.position, this.MyBurstPos.rotation);
		this.Health -= damage;
		if (this.Health <= 0)
		{
			this.CutDown();
		}
	}

	private void CutDown()
	{
		if (base.entity.IsAttached() && base.entity.StateIs<IGardenDirtPileState>())
		{
			DestroyPickUp destroyPickUp = DestroyPickUp.Create(GlobalTargets.OnlyServer);
			destroyPickUp.PickUpEntity = base.entity;
			destroyPickUp.Send();
		}
		else
		{
			this.CutDownReal();
		}
	}

	public void CutDownReal()
	{
		Transform transform = UnityEngine.Object.Instantiate<Transform>(this.MyCut, base.transform.position, base.transform.rotation);
		transform.localScale = base.transform.localScale;
		GameObject gameObject = (!this.DestroyTarget) ? base.gameObject : this.DestroyTarget;
		if (this.LodBase)
		{
			this.LodBase.CurrentLodTransform = null;
			this.LodBase = null;
		}
		if (PoolManager.Pools["Bushes"].IsSpawned(gameObject.transform))
		{
			PoolManager.Pools["Bushes"].Despawn(gameObject.transform);
		}
		else
		{
			UnityEngine.Object.Destroy(gameObject);
		}
	}

	public void DespawnBush()
	{
		GameObject gameObject = (!this.DestroyTarget) ? base.gameObject : this.DestroyTarget;
		if (this.LodBase)
		{
			this.LodBase.CurrentLodTransform = null;
			this.LodBase = null;
		}
		if (PoolManager.Pools["Bushes"].IsSpawned(gameObject.transform))
		{
			PoolManager.Pools["Bushes"].Despawn(gameObject.transform);
		}
		else
		{
			UnityEngine.Object.Destroy(gameObject);
		}
	}

	private void Explosion(float dist)
	{
		base.Invoke("CutDown", 1f);
	}

	public int Health;

	public Transform Burst;

	public Transform snowBurst;

	public Transform MyCut;

	public Transform MyBurstPos;

	public GameObject DestroyTarget;

	public LOD_Base LodBase;

	public bool snowType;

	private int StartHealth;
}
