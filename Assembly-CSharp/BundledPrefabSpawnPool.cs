using System;
using System.Collections;
using System.Collections.Generic;
using AssetBundles;
using UnityEngine;

public class BundledPrefabSpawnPool : MonoBehaviour
{
	private void Start()
	{
		base.StartCoroutine(this.CleanupPoolsOnInterval());
	}

	public void ForceUnload()
	{
		AssetBundleManager.UnloadAssetBundle(this.bundle);
	}

	private IEnumerator CleanupPoolsOnInterval()
	{
		WaitForSeconds wait = YieldPresets.WaitThreeSeconds;
		for (;;)
		{
			yield return wait;
			if (this.loading == 0 && this.assetPools.Count > 0)
			{
				this.CheckCleanupPools();
			}
			else if (this.cleanupCalls > 0)
			{
				for (int i = 0; i < this.cleanupCalls; i++)
				{
					AssetBundleManager.UnloadAssetBundle(this.bundle);
				}
				this.cleanupCalls = 0;
			}
		}
		yield break;
	}

	private void CheckCleanupPools()
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, BundledAssetPool> keyValuePair in this.assetPools)
		{
			if (keyValuePair.Value.AttemptClean(this.timeToLive))
			{
				list.Add(keyValuePair.Key);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			this.assetPools.Remove(list[i]);
		}
	}

	public void SpawnPrefab(string assetName, Vector3 pos, Quaternion rot, Transform parent, Action<GameObject> callback = null)
	{
		if (this.assetPools.ContainsKey(assetName))
		{
			this.HandleSpawn(this.assetPools[assetName], pos, rot, parent, callback);
		}
		else
		{
			base.StartCoroutine(this.SpawnAfterLoad(assetName, pos, rot, parent, callback));
		}
	}

	private void HandleSpawn(BundledAssetPool pool, Vector3 pos, Quaternion rot, Transform parent, Action<GameObject> callback)
	{
		GameObject next = pool.GetNext(pos, rot, parent);
		if (next.GetComponent<BundledPrefabSpawn>() == null)
		{
			next.AddComponent<BundledPrefabSpawn>().Init(this, pool.id);
		}
		next.BroadcastMessage("OnSpawned", this, SendMessageOptions.DontRequireReceiver);
		if (callback != null)
		{
			callback(next);
		}
	}

	private IEnumerator SpawnAfterLoad(string assetName, Vector3 pos, Quaternion rot, Transform parent, Action<GameObject> callback)
	{
		bool startedToLoad = AssetBundleManager.LoadAssetAsyncCallback<GameObject>(this.bundle, assetName, new Func<GameObject, bool>(this.OnPrefabLoaded));
		if (startedToLoad)
		{
			this.loading++;
		}
		while (startedToLoad && !this.assetPools.ContainsKey(assetName))
		{
			yield return null;
		}
		if (startedToLoad)
		{
			this.HandleSpawn(this.assetPools[assetName], pos, rot, parent, callback);
		}
		else
		{
			Debug.LogError("failed to load: " + assetName + " from bundle: " + this.bundle);
		}
		yield break;
	}

	private bool OnPrefabLoaded(GameObject prefab)
	{
		this.loading--;
		if (base.gameObject.activeInHierarchy)
		{
			if (prefab == null)
			{
				Debug.Log("loaded a null prefab");
			}
			else
			{
				Debug.Log("loaded asset: " + prefab.name);
			}
			this.cleanupCalls++;
			if (prefab != null && !this.assetPools.ContainsKey(prefab.name))
			{
				this.assetPools.Add(prefab.name, new BundledAssetPool(prefab));
			}
			return true;
		}
		return false;
	}

	public void AddToPool(string id, GameObject obj)
	{
		if (this.assetPools.ContainsKey(id))
		{
			this.assetPools[id].AddToPool(obj);
		}
	}

	public void RemoveFromPool(string id, GameObject obj)
	{
		if (this.assetPools.ContainsKey(id))
		{
			this.assetPools[id].DecrementSpawned(obj);
		}
	}

	public string bundle;

	public float timeToLive = 5f;

	private Dictionary<string, BundledAssetPool> assetPools = new Dictionary<string, BundledAssetPool>();

	private int loading;

	private int cleanupCalls;
}
