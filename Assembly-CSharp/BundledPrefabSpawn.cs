using System;
using UnityEngine;

public class BundledPrefabSpawn : MonoBehaviour
{
	public string id { get; private set; }

	public void Init(BundledPrefabSpawnPool spawnPool, string id)
	{
		this.bundledPool = spawnPool;
		this.id = id;
	}

	private void OnDisable()
	{
		if (this.bundledPool != null)
		{
			this.bundledPool.AddToPool(this.id, base.gameObject);
		}
	}

	private void OnDestroy()
	{
		if (this.bundledPool != null)
		{
			this.bundledPool.RemoveFromPool(this.id, base.gameObject);
		}
	}

	private BundledPrefabSpawnPool bundledPool;
}
