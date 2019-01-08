using System;
using PathologicalGames;
using UnityEngine;

public class CutBush2 : MonoBehaviour
{
	private void Awake()
	{
		this.startHealth = this.Health;
	}

	private void OnEnable()
	{
		this.Health = this.startHealth;
	}

	public void SetLodBase(LOD_Base lb)
	{
		this.LodBase = lb;
	}

	private void Hit(int damage)
	{
		if (this.Burst)
		{
			UnityEngine.Object.Instantiate<GameObject>(this.Burst, this.MyBurstPos.position, this.MyBurstPos.rotation);
		}
		this.Health -= damage;
		if (this.Health <= 0)
		{
			this.CutDown();
		}
	}

	public void CutDown()
	{
		if (this.sapling)
		{
			UnityEngine.Object.Instantiate<GameObject>(this.MyCut, this.MyBurstPos.position, this.MyBurstPos.rotation);
		}
		else
		{
			UnityEngine.Object.Instantiate<GameObject>(this.MyCut, base.transform.position, base.transform.rotation);
		}
		if (this.LodBase)
		{
			this.LodBase.CurrentLodTransform = null;
			this.LodBase = null;
		}
		if (PoolManager.Pools[(!this.sapling) ? "Bushes" : "Trees"].IsSpawned(base.transform))
		{
			PoolManager.Pools[(!this.sapling) ? "Bushes" : "Trees"].Despawn(base.transform);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void DespawnBush()
	{
		if (this.LodBase)
		{
			this.LodBase.CurrentLodTransform = null;
			this.LodBase = null;
		}
		if (PoolManager.Pools[(!this.sapling) ? "Bushes" : "Trees"].IsSpawned(base.transform))
		{
			PoolManager.Pools[(!this.sapling) ? "Bushes" : "Trees"].Despawn(base.transform);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void Explosion()
	{
		base.Invoke("CutDown", 0.35f);
	}

	public int Health = 4;

	public GameObject Burst;

	public GameObject MyCut;

	public Transform MyBurstPos;

	public LOD_Base LodBase;

	public bool sapling;

	private int startHealth;
}
