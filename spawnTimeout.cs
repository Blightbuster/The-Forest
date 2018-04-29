using System;
using PathologicalGames;
using UnityEngine;


public class spawnTimeout : MonoBehaviour
{
	
	public void invokeDespawn()
	{
		if (this.despawnTime > 0f)
		{
			base.Invoke("doDespawn", this.despawnTime);
		}
	}

	
	private void doDespawn()
	{
		if (PoolManager.Pools["misc"].IsSpawned(base.transform))
		{
			PoolManager.Pools["misc"].Despawn(base.transform);
		}
	}

	
	public float despawnTime;
}
