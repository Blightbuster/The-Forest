using System;
using System.Collections.Generic;
using UnityEngine;


public class BundledPrefabSpawnPoolManager : MonoBehaviour
{
	
	private void Awake()
	{
		BundledPrefabSpawnPoolManager.instance = this;
		BundledPrefabSpawnPool[] componentsInChildren = base.GetComponentsInChildren<BundledPrefabSpawnPool>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			this._pools.Add(componentsInChildren[i].bundle, componentsInChildren[i]);
		}
	}

	
	private void OnDestroy()
	{
		if (BundledPrefabSpawnPoolManager.instance == this)
		{
			BundledPrefabSpawnPoolManager.instance = null;
		}
	}

	
	public void SpawnObject(string bundle, string asset, Vector3 pos, Quaternion rot, Transform parent, Action<GameObject> callback = null)
	{
		if (this._pools.ContainsKey(bundle))
		{
			this._pools[bundle].SpawnPrefab(asset, pos, rot, parent, callback);
		}
		else
		{
			Debug.LogError("trying to load an assetbundle not in the manager: " + bundle + ":" + asset);
		}
	}

	
	public void SpawnObject(BundleKey bundleKey, Vector3 pos, Quaternion rot, Transform parent, Action<GameObject> callback = null)
	{
		this.SpawnObject(bundleKey.bundle, bundleKey.asset, pos, rot, parent, callback);
	}

	
	public void SpawnAnimalFromZone(BundleKey bundleKey, Vector3 pos, Quaternion rot, AnimalSpawnZone spawnZone)
	{
		Debug.Log("Starting to spawn animal from spawn zone");
		this.queuedAnimalZones.Add(spawnZone);
		this.SpawnObject(bundleKey, pos, rot, null, new Action<GameObject>(this.OnSpawnAnimalFromZone));
	}

	
	private void OnSpawnAnimalFromZone(GameObject spawned)
	{
		AnimalSpawnZone animalSpawnZone = null;
		for (int i = 0; i < this.queuedAnimalZones.Count; i++)
		{
			for (int j = 0; j < this.queuedAnimalZones[i].Spawns.Length; j++)
			{
				if (this.queuedAnimalZones[i].Spawns[j].bundleKey.asset == spawned.GetComponent<BundledPrefabSpawn>().id)
				{
					animalSpawnZone = this.queuedAnimalZones[i];
					break;
				}
			}
		}
		if (animalSpawnZone != null)
		{
			Debug.Log("successfully spawned animal from spawn zone");
			this.queuedAnimalZones.Remove(animalSpawnZone);
			this.OnSpawnedAnimalInZone(spawned, animalSpawnZone);
		}
	}

	
	public static BundledPrefabSpawnPoolManager instance;

	
	public Action<GameObject, AnimalSpawnZone> OnSpawnedAnimalInZone = delegate
	{
	};

	
	private Dictionary<string, BundledPrefabSpawnPool> _pools = new Dictionary<string, BundledPrefabSpawnPool>();

	
	private List<AnimalSpawnZone> queuedAnimalZones = new List<AnimalSpawnZone>();
}
