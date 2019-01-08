using System;
using System.Collections.Generic;
using PathologicalGames;
using TheForest.Utils;
using UnityEngine;

public class disableCoopAnimalsController : MonoBehaviour
{
	private void Start()
	{
		this.spawnGo = Scene.animalSpawner;
		this.updateRate = Time.time + 2f;
	}

	private void Update()
	{
		if (Time.time > this.updateRate)
		{
			if (this.allPlayersInCave())
			{
				if (!this.animalsDisabled)
				{
					this.disableAnimals();
				}
			}
			else if (this.animalsDisabled)
			{
				this.enableAnimals();
			}
			this.updateRate = Time.time + 2f;
		}
	}

	private void disableAnimals()
	{
		this.animalsDisabled = true;
		this.spawnGo.SetActive(false);
		List<Transform> list = new List<Transform>();
		foreach (Transform item in PoolManager.Pools["creatures"])
		{
			list.Add(item);
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].gameObject.activeSelf && !list[i].gameObject.name.Contains("Fish") && !list[i].gameObject.name.Contains("fish"))
			{
				PoolManager.Pools["creatures"].Despawn(list[i].transform);
			}
		}
	}

	private void enableAnimals()
	{
		this.animalsDisabled = false;
		this.spawnGo.SetActive(true);
	}

	private bool allPlayersInCave()
	{
		return Scene.SceneTracker.allPlayersInCave.Count == Scene.SceneTracker.allPlayers.Count;
	}

	private GameObject spawnGo;

	private float updateRate;

	private bool animalsDisabled;

	private AnimalSpawnController asc;
}
