using System;
using System.Collections.Generic;
using Pathfinding;
using PathologicalGames;
using TheForest.Utils;
using TheForest.Utils.Settings;
using UnityEngine;


public class AnimalSpawnController : MonoBehaviour
{
	
	private void initAnimalSpawnController()
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
		this.sceneTracker = Scene.SceneTracker;
		AnimalSpawnController.lastUpdate = Time.time;
		this.Zones = base.GetComponentsInChildren<AnimalSpawnZone>();
		foreach (AnimalSpawnZone animalSpawnZone in this.Zones)
		{
			animalSpawnZone.SpawnedAnimals = new List<GameObject>();
			for (int j = 0; j < animalSpawnZone.Spawns.Length; j++)
			{
				animalSpawnZone.TotalWeight += animalSpawnZone.Spawns[j].Weight;
				animalSpawnZone.Spawns[j].WeightRunningTotal = animalSpawnZone.TotalWeight;
			}
		}
	}

	
	private void Start()
	{
		this.initAnimalSpawnController();
	}

	
	private void OnDestroy()
	{
	}

	
	private void Update()
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
		if (Scene.SceneTracker == null)
		{
			return;
		}
		if (Scene.SceneTracker.doingGlobalNavUpdate)
		{
			return;
		}
		if (!Scene.SceneTracker.waitForLoadSequence)
		{
			return;
		}
		if (Scene.SceneTracker.allPlayers.Count == 0)
		{
			return;
		}
		if (AnimalSpawnController.lastUpdate + 2f < Time.time)
		{
			this.allPositions.Clear();
			foreach (GameObject gameObject in this.sceneTracker.allPlayers)
			{
				if (gameObject)
				{
					this.allPositions.Add(gameObject.transform);
				}
			}
			this.filteredPositions.Clear();
			foreach (Transform transform in this.allPositions)
			{
				bool flag = false;
				foreach (Transform transform2 in this.filteredPositions)
				{
					if ((transform.position - transform2.position).magnitude < Mathf.Lerp(30f, 60f, 0.6f))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					this.filteredPositions.Add(transform);
				}
			}
			foreach (AnimalSpawnZone animalSpawnZone in this.Zones)
			{
				if (animalSpawnZone.DelayUntil <= Time.realtimeSinceStartup)
				{
					if (this.filteredPositions.Count == 0)
					{
						break;
					}
					this.UpdateZone(animalSpawnZone, this.filteredPositions);
				}
			}
			AnimalSpawnController.lastUpdate = Time.time;
		}
	}

	
	private void UpdateZone(AnimalSpawnZone zone, List<Transform> positions)
	{
		try
		{
			for (int i = 0; i < positions.Count; i++)
			{
				if ((positions[i].position - zone.transform.position).sqrMagnitude < zone.ZoneRadius * zone.ZoneRadius)
				{
					this.SpawnOneAnimalForZone(zone, positions[i]);
					positions.RemoveAt(i);
					i--;
				}
			}
		}
		catch (Exception exception)
		{
			BoltLog.Exception(exception);
		}
	}

	
	private void SpawnOneAnimalForZone(AnimalSpawnZone zone, Transform pos)
	{
		int num = Mathf.CeilToInt((float)zone.MaxAnimalsTotal * GameSettings.Animals.MaxAmountRatio);
		if (zone.SpawnedAnimals.Count >= num)
		{
			return;
		}
		float num2 = Mathf.Clamp01((float)zone.SpawnedAnimals.Count / (float)num) - 0.25f;
		if (num2 < UnityEngine.Random.value)
		{
			float num3 = UnityEngine.Random.Range(0f, zone.TotalWeight);
			for (int i = 0; i < zone.Spawns.Length; i++)
			{
				if (num3 <= zone.Spawns[i].WeightRunningTotal)
				{
					this.SpawnOneAnimalForZoneOfType(zone, zone.Spawns[i], pos);
					return;
				}
			}
		}
	}

	
	private void SpawnOneAnimalForZoneOfType(AnimalSpawnZone zone, AnimalSpawnConfig spawn, Transform basePos)
	{
		if (spawn.nocturnal && !Clock.Dark)
		{
			return;
		}
		float sendDir;
		if (spawn.largeAnimalType)
		{
			sendDir = -1f;
		}
		else
		{
			sendDir = 1f;
		}
		Vector3 pos;
		if (AnimalSpawnController.TryFindSpawnPosition(basePos, sendDir, out pos))
		{
			Transform transform = PoolManager.Pools["creatures"].Spawn(spawn.Prefab.transform, pos, zone.transform.rotation);
			if (transform)
			{
				this.SetupAnimal(transform, zone);
			}
		}
	}

	
	private void SpawnFromAssetBundle(AnimalSpawnZone zone, BundleKey bundleKey, Vector3 pos, Quaternion rot)
	{
		BundledPrefabSpawnPoolManager.instance.SpawnAnimalFromZone(bundleKey, pos, rot, zone);
	}

	
	private void OnSpawnedFromAssetBundle(GameObject animal, AnimalSpawnZone zone)
	{
		this.SetupAnimal(animal.transform, zone);
	}

	
	private void SetupAnimal(Transform animal, AnimalSpawnZone zone)
	{
		if (animal)
		{
			animal.GetChild(0).eulerAngles = new Vector3(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
		}
		if (animal)
		{
			animal.SendMessage("startUpdateSpawn");
		}
		if (animal)
		{
			animal.SendMessage("setSnowType", zone.snowType, SendMessageOptions.DontRequireReceiver);
		}
		zone.SpawnedAnimals.Add(animal.gameObject);
		AnimalSpawnController.AttachAnimalToNetwork(zone, animal.gameObject);
	}

	
	public static void AttachAnimalToNetwork(AnimalSpawnZone zone, GameObject gameObject)
	{
		AnimalDespawner animalDespawner = gameObject.GetComponent<AnimalDespawner>();
		if (!animalDespawner)
		{
			animalDespawner = gameObject.AddComponent<AnimalDespawner>();
		}
		animalDespawner.SpawnedFromZone = zone;
		animalDespawner.DespawnRadius = 70f;
		animalDespawner.UpdateRate = 1f;
		if (BoltNetwork.isServer)
		{
			BoltEntity boltEntity = gameObject.AddComponent<BoltEntity>();
			BoltEntity component = gameObject.GetComponent<CoopAnimalServer>().NetworkContainerPrefab.GetComponent<BoltEntity>();
			using (BoltEntitySettingsModifier boltEntitySettingsModifier = component.ModifySettings())
			{
				using (BoltEntitySettingsModifier boltEntitySettingsModifier2 = boltEntity.ModifySettings())
				{
					boltEntitySettingsModifier2.clientPredicted = boltEntitySettingsModifier.clientPredicted;
					boltEntitySettingsModifier2.persistThroughSceneLoads = boltEntitySettingsModifier.persistThroughSceneLoads;
					boltEntitySettingsModifier2.allowInstantiateOnClient = boltEntitySettingsModifier.allowInstantiateOnClient;
					boltEntitySettingsModifier2.prefabId = boltEntitySettingsModifier.prefabId;
					boltEntitySettingsModifier2.updateRate = boltEntitySettingsModifier.updateRate;
					boltEntitySettingsModifier2.serializerId = boltEntitySettingsModifier.serializerId;
				}
			}
			BoltNetwork.Attach(gameObject);
		}
	}

	
	public static Vector3 SpawnPos(Transform pos, float getDir)
	{
		Vector3 vector = Vector3.zero;
		vector = pos.forward * getDir;
		vector = Quaternion.Euler(0f, UnityEngine.Random.Range(-1f, 1f) * 100f, 0f) * vector;
		vector *= Mathf.Lerp(30f, 60f, UnityEngine.Random.value);
		return pos.position + vector;
	}

	
	public static bool TryFindSpawnPosition(Transform basePos, float sendDir, out Vector3 pos)
	{
		pos = Vector3.zero;
		for (int i = 0; i < 2; i++)
		{
			pos = AnimalSpawnController.SpawnPos(basePos, sendDir);
			pos.y += 100f;
			RaycastHit raycastHit;
			if (Physics.Raycast(pos, Vector3.down, out raycastHit, 300f, 67108864) && raycastHit.collider.CompareTag("TerrainMain") && AstarPath.active != null)
			{
				GraphNode node = AstarPath.active.GetNearest(raycastHit.point, NNConstraint.Default).node;
				if (node == null)
				{
					return false;
				}
				bool flag = false;
				using (List<uint>.Enumerator enumerator = Scene.MutantControler.mostCommonArea.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						int num = (int)enumerator.Current;
						if ((long)num == (long)((ulong)node.Area))
						{
							flag = true;
						}
					}
				}
				if (node != null && node.Walkable && flag)
				{
					pos = new Vector3((float)(node.position[0] / 1000), (float)(node.position[1] / 1000), (float)(node.position[2] / 1000));
					return true;
				}
			}
		}
		return false;
	}

	
	public const float OVERALL_SPAWN_CHANCE_MODIFIER = 0.25f;

	
	public const float SPAWN_UPDATE_RATE = 2f;

	
	public const float DESPAWN_UPDATE_RATE = 2f;

	
	public const float PLAYER_SPAWN_DISTANCE_MIN = 30f;

	
	public const float PLAYER_SPAWN_DISTANCE_MAX = 60f;

	
	public const float PLAYER_PROXIMITY_DISTANCE_RATIO = 0.6f;

	
	public const float PLAYER_DESPAWN_DISTANCE = 70f;

	
	private sceneTracker sceneTracker;

	
	public static float lastUpdate;

	
	[HideInInspector]
	public AnimalSpawnZone[] Zones;

	
	private List<Transform> allPositions = new List<Transform>();

	
	private List<Transform> filteredPositions = new List<Transform>();
}
