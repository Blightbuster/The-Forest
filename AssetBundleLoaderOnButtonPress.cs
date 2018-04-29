using System;
using AssetBundles;
using UnityEngine;


public class AssetBundleLoaderOnButtonPress : MonoBehaviour
{
	
	private void Awake()
	{
		AssetBundleManager.Initialize();
	}

	
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			for (int i = 0; i < this.numToSpawn; i++)
			{
				this.spawnPool.SpawnPrefab(this.assetName, base.transform.position + UnityEngine.Random.insideUnitSphere * 2f, UnityEngine.Random.rotation, null, null);
			}
		}
		if (Input.GetKeyDown(KeyCode.Quote))
		{
			this.spawnPool.ForceUnload();
		}
		if (Input.GetKeyDown(KeyCode.T))
		{
			GameObject gameObject = AssetBundleManager.LoadAssetImmediate(this.spawnPool.bundle, this.assetName);
			if (gameObject != null)
			{
				UnityEngine.Object.Instantiate<GameObject>(gameObject);
			}
		}
	}

	
	public int numToSpawn = 1;

	
	public string assetName;

	
	public BundledPrefabSpawnPool spawnPool;
}
