using System;
using System.Collections.Generic;
using UnityEngine;


public class BundledAssetPool
{
	
	public BundledAssetPool(GameObject prefab)
	{
		this.id = prefab.name;
		this.prefab = prefab;
	}

	
	public bool HasInactive()
	{
		return this.inactive.Count > 0;
	}

	
	public GameObject GetNext(Vector3 pos, Quaternion rot, Transform parent)
	{
		GameObject gameObject;
		if (this.inactive.Count > 0)
		{
			gameObject = this.inactive[this.inactive.Count - 1];
			this.inactive.RemoveAt(this.inactive.Count - 1);
			gameObject.transform.position = pos;
			gameObject.transform.rotation = rot;
			gameObject.SetActive(true);
		}
		else
		{
			gameObject = (GameObject)UnityEngine.Object.Instantiate(this.prefab, pos, rot);
		}
		gameObject.transform.parent = parent;
		this.spawned++;
		this.lastActiveTime = Time.time;
		return gameObject;
	}

	
	public void AddToPool(GameObject obj)
	{
		this.spawned--;
		obj.SetActive(false);
		this.inactive.Add(obj);
		this.lastActiveTime = Time.time;
	}

	
	public void DecrementSpawned(GameObject obj)
	{
		if (this.inactive.Contains(obj))
		{
			this.inactive.Remove(obj);
		}
		else
		{
			this.spawned--;
		}
		this.lastActiveTime = Time.time;
	}

	
	public bool AttemptClean(float timeToLive)
	{
		if (this.prefab != null && (this.spawned > 0 || Time.time - this.lastActiveTime < timeToLive))
		{
			return false;
		}
		for (int i = 0; i < this.inactive.Count; i++)
		{
			UnityEngine.Object.Destroy(this.inactive[i]);
		}
		this.inactive.Clear();
		return true;
	}

	
	public string id;

	
	public GameObject prefab;

	
	private List<GameObject> inactive = new List<GameObject>();

	
	private int spawned;

	
	private float lastActiveTime;
}
