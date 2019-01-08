using System;
using System.Collections.Generic;
using Bolt;
using BoltInternal;
using UnityEngine;

public class wormHiveController : MonoBehaviour
{
	private void Start()
	{
		base.Invoke("SpawnHiveWorms", 0.25f);
	}

	private void SpawnHiveWorms()
	{
		for (int i = 0; i < this.minWorms; i++)
		{
			this.SpawnWorm(base.transform.position);
		}
		this.init = true;
	}

	public void SpawnWorm(Vector3 getPos)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.wormPrefab, getPos, base.transform.rotation);
		gameObject.SendMessage("setHiveController", base.gameObject);
		if (BoltNetwork.isRunning && BoltNetwork.isServer)
		{
			this.AttachToNetwork(BoltPrefabs.wormGo_net, StateSerializerTypeIds.IMutantWormState, gameObject);
		}
	}

	private void Update()
	{
		this.activeWormWalkers.RemoveAll((GameObject o) => o == null);
		this.activeWormTrees.RemoveAll((GameObject o) => o == null);
		this.activeWormSingle.RemoveAll((GameObject o) => o == null);
		this.activeWormAngels.RemoveAll((GameObject o) => o == null);
		if (this.activeWormWalkers.Count > 0 || this.activeWormAngels.Count > 0 || this.activeWormTrees.Count > 0)
		{
			this.anyFormSpawned = true;
		}
		else
		{
			this.anyFormSpawned = false;
		}
		if (this.activeWormSingle.Count == 0 && this.init)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void AttachToNetwork(PrefabId prefabId, UniqueId state, GameObject sourceGo)
	{
		if (BoltNetwork.isServer)
		{
			BoltEntity boltEntity = sourceGo.AddComponent<BoltEntity>();
			using (BoltEntitySettingsModifier boltEntitySettingsModifier = boltEntity.ModifySettings())
			{
				boltEntitySettingsModifier.persistThroughSceneLoads = true;
				boltEntitySettingsModifier.allowInstantiateOnClient = false;
				boltEntitySettingsModifier.clientPredicted = false;
				boltEntitySettingsModifier.prefabId = prefabId;
				boltEntitySettingsModifier.updateRate = 1;
				boltEntitySettingsModifier.sceneId = UniqueId.None;
				boltEntitySettingsModifier.serializerId = state;
			}
			BoltNetwork.Attach(boltEntity.gameObject);
		}
	}

	public List<GameObject> activeWormAngels = new List<GameObject>();

	public List<GameObject> activeWormWalkers = new List<GameObject>();

	public List<GameObject> activeWormTrees = new List<GameObject>();

	public List<GameObject> activeWormSingle = new List<GameObject>();

	public GameObject wormPrefab;

	public GameObject WormTreePrefab;

	public GameObject WormWalkerPrefab;

	public GameObject WormAngelPrefab;

	public float spawnFormCoolDown;

	public bool anyFormSpawned;

	public int respawnCount;

	public int currentWorms;

	private bool init;

	public int minWorms = 12;

	public int maxWorms = 35;

	public int maxRespawnAmount = 80;
}
