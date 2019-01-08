﻿using System;
using TheForest.Utils;
using UnityEngine;

public class SpawnItemFromPool : MonoBehaviour
{
	private void Awake()
	{
		if (base.gameObject.activeSelf && Prefabs.Instance && (this._clientSide || !BoltNetwork.isClient))
		{
			Vector3 position = base.transform.position;
			if (this._aboveTerrain)
			{
				float num = Terrain.activeTerrain.SampleHeight(position);
				if (position.y < num)
				{
					position.y = num + 0.5f;
				}
			}
			if (BoltNetwork.isServer && this._prefab.GetComponent<BoltEntity>())
			{
				Prefabs.Instance.SpawnFromPoolMP(this._pool, this._prefab, position, base.transform.rotation, this._setSpawnedFromTree);
			}
			else
			{
				Prefabs.Instance.SpawnFromPool(this._pool, this._prefab, position, base.transform.rotation, this._setSpawnedFromTree);
			}
			if (this._destroyAfter)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	public Transform _prefab;

	public string _pool = "PickUps";

	public bool _destroyAfter = true;

	public bool _aboveTerrain = true;

	public bool _setSpawnedFromTree;

	public bool _clientSide;
}
