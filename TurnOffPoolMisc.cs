using System;
using PathologicalGames;
using UnityEngine;


public class TurnOffPoolMisc : MonoBehaviour
{
	
	private void OnEnable()
	{
		base.CancelInvoke("TurnOff");
		base.Invoke("TurnOff", this.Wait);
	}

	
	private void TurnOff()
	{
		BoltEntity boltEntity = null;
		if (BoltNetwork.isRunning)
		{
			boltEntity = base.GetComponent<BoltEntity>();
			if (boltEntity && boltEntity.isAttached && !boltEntity.isOwner)
			{
				return;
			}
		}
		if (PoolManager.Pools["misc"].IsSpawned(base.transform))
		{
			if (BoltNetwork.isServer && boltEntity && boltEntity.isAttached)
			{
				BoltNetwork.Detach(base.gameObject);
			}
			PoolManager.Pools["misc"].Despawn(base.transform);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	
	public float Wait;
}
