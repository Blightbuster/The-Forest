using System;
using PathologicalGames;
using UnityEngine;

public class TurnOffPoolPickUp : MonoBehaviour
{
	private void OnEnable()
	{
		base.CancelInvoke("TurnOff");
		base.Invoke("TurnOff", (float)this.Wait);
	}

	private void TurnOff()
	{
		if (!BoltNetwork.isClient)
		{
			if (PoolManager.Pools["PickUps"].IsSpawned(base.transform))
			{
				if (BoltNetwork.isServer)
				{
					BoltEntity component = base.GetComponent<BoltEntity>();
					if (component && component.isAttached)
					{
						BoltNetwork.Detach(base.gameObject);
					}
				}
				PoolManager.Pools["PickUps"].Despawn(base.transform);
			}
			else
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	public int Wait;
}
