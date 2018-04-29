﻿using System;
using Bolt;
using PathologicalGames;
using TheForest.Utils;
using UnityEngine;


public class AnimalDespawner : EntityBehaviour
{
	
	private void OnEnable()
	{
		this.LastUpdate = Time.time;
		this.sceneTracker = Scene.SceneTracker;
	}

	
	private void OnDisable()
	{
	}

	
	private void Update()
	{
		if (this.LastUpdate + this.UpdateRate < Time.time)
		{
			this.UpdateSpawnState(true);
			this.LastUpdate = Time.time;
		}
		if (base.transform.position.x > 2000f || base.transform.position.x < -2000f || base.transform.position.z > 2000f || base.transform.position.z < -2000f)
		{
			this.UpdateSpawnState(false);
			Debug.Log(base.gameObject.name + " was destroyed due to being way outisde the terrain area");
		}
	}

	
	public void UpdateSpawnState(bool testPlayer = true)
	{
		if (testPlayer)
		{
			for (int i = 0; i < this.sceneTracker.allPlayers.Count; i++)
			{
				GameObject gameObject = this.sceneTracker.allPlayers[i];
				if (gameObject && (gameObject.transform.position - base.transform.position).sqrMagnitude < this.DespawnRadius * this.DespawnRadius)
				{
					return;
				}
			}
			Vector3 vector = LocalPlayer.Transform.InverseTransformPoint(base.transform.position);
			float num = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
			if (num < 90f && num > -90f)
			{
				return;
			}
		}
		if (this.SpawnedFromZone)
		{
			this.SpawnedFromZone.SpawnedAnimals.Remove(base.gameObject);
			this.SpawnedFromZone = null;
		}
		if (BoltNetwork.isRunning)
		{
			BoltNetwork.Detach(base.gameObject);
			UnityEngine.Object.Destroy(base.gameObject.GetComponent<BoltEntity>());
			UnityEngine.Object.Destroy(this);
		}
		if (base.GetComponent<CoopAnimalServer>().NonPooled || !PoolManager.Pools["creatures"].IsSpawned(base.transform))
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			PoolManager.Pools["creatures"].Despawn(base.transform);
			UnityEngine.Object.Destroy(this);
		}
	}

	
	private sceneTracker sceneTracker;

	
	[HideInInspector]
	public float UpdateRate = 30f;

	
	[HideInInspector]
	public float DespawnRadius = 128f;

	
	[HideInInspector]
	public float LastUpdate;

	
	[HideInInspector]
	public AnimalSpawnZone SpawnedFromZone;
}
