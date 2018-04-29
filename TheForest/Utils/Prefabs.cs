using System;
using System.Collections;
using System.Collections.Generic;
using PathologicalGames;
using TheForest.Buildings.Creation;
using TheForest.Buildings.World;
using TheForest.Commons.Enums;
using TheForest.Items;
using TheForest.Networking;
using UnityEngine;

namespace TheForest.Utils
{
	
	public class Prefabs : MonoBehaviour
	{
		
		private void Awake()
		{
			Prefabs.Instance = this;
		}

		
		private IEnumerator Start()
		{
			while (!PoolManager.Pools.ContainsKey("Particles"))
			{
				yield return null;
			}
			this._psSpawnPool = PoolManager.Pools["Particles"];
			yield break;
		}

		
		private void OnDestroy()
		{
			if (Prefabs.Instance == this)
			{
				Prefabs.Instance = null;
			}
		}

		
		public void SpawnHitPS(HitParticles type, Vector3 pos, Quaternion rot)
		{
			if (this._psSpawnPool)
			{
				this._psSpawnPool.Spawn(this.HitPSPrefabs[(int)type], pos, rot);
			}
		}

		
		public void SpawnFireHitPS(Vector3 pos, Quaternion rot)
		{
			if (this._psSpawnPool)
			{
				this._psSpawnPool.Spawn(this.FireHitPSPrefab, pos, rot);
			}
		}

		
		public void SpawnBloodHitPS(int index, Vector3 pos, Quaternion rot)
		{
			if (this._psSpawnPool)
			{
				this._psSpawnPool.Spawn(this.BloodHitPSPrefabs[(int)Mathf.Repeat((float)index, (float)this.BloodHitPSPrefabs.Length)], pos, rot);
			}
		}

		
		public void SpawnWoodChopPS(Vector3 pos, Quaternion rot)
		{
			if (this._psSpawnPool)
			{
				this._psSpawnPool.Spawn(this.woodChopPrefab, pos, rot);
			}
		}

		
		public void SpawnFootPS(int index, Vector3 pos, Quaternion rot)
		{
			if (this._psSpawnPool)
			{
				this._psSpawnPool.Spawn(this.FootStepPrefabs[(int)Mathf.Repeat((float)index, (float)this.FootStepPrefabs.Length)], pos, rot);
			}
		}

		
		public void SpawnBurntDustAndSmokePS(Vector3 pos, Quaternion rot)
		{
			if (this._psSpawnPool && this.BurntSmokeAndDustPrefab)
			{
				this._psSpawnPool.Spawn(this.BurntSmokeAndDustPrefab, pos, rot);
			}
		}

		
		public void SpawnNextFrame(Transform prefab, Vector3 pos, Quaternion rot, Transform parent = null)
		{
			base.StartCoroutine(this.DelayedSpawn(prefab, pos, rot, parent));
		}

		
		public void SpawnNextFrameMP(Transform prefab, Vector3 pos, Quaternion rot, Transform parent = null)
		{
			base.StartCoroutine(this.DelayedSpawnMP(prefab, pos, rot, parent));
		}

		
		public void SpawnNextFrameFromPool(string pool, Transform prefab, Vector3 pos, Quaternion rot)
		{
			base.StartCoroutine(this.DelayedPoolSpawn(pool, prefab, pos, rot));
		}

		
		public void SpawnNextFrameFromPoolMP(string pool, Transform prefab, Vector3 pos, Quaternion rot)
		{
			base.StartCoroutine(this.DelayedPoolSpawnMP(pool, prefab, pos, rot));
		}

		
		public void SpawnFromPool(string pool, Transform prefab, Vector3 pos, Quaternion rot, bool spawnedFromTree = false)
		{
			Transform transform = PoolManager.Pools[pool].Spawn(prefab, pos, rot);
			if (spawnedFromTree)
			{
				transform.SendMessage("enableSpawnedfromTree", SendMessageOptions.DontRequireReceiver);
			}
		}

		
		public void SpawnFromPoolMP(string pool, Transform prefab, Vector3 pos, Quaternion rot, bool spawnedFromTree = false)
		{
			Transform transform = PoolManager.Pools[pool].Spawn(prefab, pos, rot);
			transform.parent = null;
			if (spawnedFromTree)
			{
				transform.SendMessage("enableSpawnedfromTree", SendMessageOptions.DontRequireReceiver);
			}
			if (BoltNetwork.isRunning)
			{
				BoltNetwork.Attach(transform.gameObject);
			}
		}

		
		public Material GetGhostClearColorMenu()
		{
			return this.GhostClearColorMenu;
		}

		
		public Material GetGhostClearAlphaMenu()
		{
			return this.GhostClearAlphaMenu;
		}

		
		public Material GetGhostClear()
		{
			return this.GhostClear;
		}

		
		public Material GetGhostClearGround()
		{
			return this.GhostClearGround;
		}

		
		public Material GetWallChunkBillboardMat()
		{
			return this.WallChunkBillboardMat;
		}

		
		public Material GetMaterialInstance(Material sourceMaterial)
		{
			if (this._materialInstances == null)
			{
				this._materialInstances = new Dictionary<Material, Material>();
			}
			Material material = null;
			if (!this._materialInstances.TryGetValue(sourceMaterial, out material))
			{
				material = new Material(sourceMaterial);
				this._materialInstances.Add(sourceMaterial, material);
			}
			return material;
		}

		
		private IEnumerator DelayedSpawn(Transform prefab, Vector3 pos, Quaternion rot, Transform parent)
		{
			yield return null;
			Transform t = UnityEngine.Object.Instantiate<Transform>(prefab, pos, rot);
			if (parent)
			{
				t.parent = parent;
			}
			yield break;
		}

		
		private IEnumerator DelayedSpawnMP(Transform prefab, Vector3 pos, Quaternion rot, Transform parent)
		{
			yield return null;
			BoltEntity be = BoltNetwork.Instantiate(prefab.gameObject, pos, rot);
			if (parent)
			{
				be.transform.parent = parent;
			}
			yield break;
		}

		
		private IEnumerator DelayedPoolSpawn(string pool, Transform prefab, Vector3 pos, Quaternion rot)
		{
			yield return null;
			PoolManager.Pools[pool].Spawn(prefab, pos, rot);
			yield break;
		}

		
		private IEnumerator DelayedPoolSpawnMP(string pool, Transform prefab, Vector3 pos, Quaternion rot)
		{
			yield return null;
			Transform t = PoolManager.Pools[pool].Spawn(prefab, pos, rot);
			BoltNetwork.Attach(t.gameObject);
			yield break;
		}

		
		[Header("AI")]
		[NameFromEnumIndex(typeof(EnemyType))]
		public GameObject[] _deadMutantBodies;

		
		[Header("Buildings")]
		public Constructions Constructions;

		
		public Transform LogBuiltPrefab;

		
		public Transform LogBridgeBuiltPrefab;

		
		public Renderer LogBridgeBuiltPrefabLOD1;

		
		public Transform LogStairsBuiltPrefab;

		
		public Transform LogStiltStairsBuiltPrefab;

		
		public Transform LogStiltStairsGhostBuiltPrefab;

		
		public Transform LogFloorBuiltPrefab;

		
		public Transform LogRoofBuiltPrefab;

		
		public Transform LogWallExBuiltPrefab;

		
		public Mesh LogWallExBuiltPrefabLOD1;

		
		public Mesh LogWallExBuiltPrefabLOD2;

		
		public Transform LogWallDefensiveExBuiltPrefab;

		
		public Renderer[] LogWallDefensiveExBuiltPrefabLOD1;

		
		public Renderer[] DefensiveWallReinforcementBuiltPrefabLOD1;

		
		public Transform[] RockFenceChunksGhostPrefabs;

		
		public Transform[] RockFenceChunksGhostFillPrefabs;

		
		public Transform[] RockFenceChunksBuiltPrefabs;

		
		public Renderer[] RockFenceChunksBuiltPrefabsLOD1;

		
		public Transform[] RockFenceChunkDestroyPrefabs;

		
		public Transform PerLogWDReinforcementBuiltPrefab;

		
		public WallDefensiveGateArchitect WallDefensiveGateGhostPrefab;

		
		public WallDefensiveChunkArchitect WallDefensiveChunkGhostPrefab;

		
		public WallDefensiveGate WallDefensiveGateTriggerPrefab;

		
		public Transform StickFenceExBuiltPrefab;

		
		public Transform BoneFenceExBuiltPrefab;

		
		public Transform RockPathExBuiltPrefab;

		
		public Transform DoorPrefab;

		
		public Transform DoorGhostPrefab;

		
		public Transform CraneLogBuiltPrefab;

		
		public Transform ZiplineRopeBuiltPrefab;

		
		public Transform AnchorPrefab;

		
		public Transform TorsoPrefab;

		
		public Transform TorsoGhostPrefab;

		
		public GardenDirtPile[] GardenDirtPilePrefabs;

		
		public GardenDirtPile[] GardenDirtPileSmallPrefabs;

		
		public Prefabs.TrophyPrefab[] TrophyPrefabs;

		
		public BuildMission BuildMissionPrefab;

		
		public TriggerTagSensor WaterSensor;

		
		public OnDestroyProxyS DestroyProxy;

		
		public Transform ZiplineLog;

		
		[Header("Buildings Mats")]
		public Material GhostClearColorMenu;

		
		public Material GhostClearAlphaMenu;

		
		public Material GhostClear;

		
		public Material GhostBlocked;

		
		public Material GhostClearGround;

		
		public Material WallChunkBillboardMat;

		
		public Color[] GhostTints;

		
		[Header("Drawings")]
		public Material[] TimmyDrawingsMats;

		
		public Transform[] TimmyDrawingsPrefab;

		
		[Header("UI")]
		public GameObject LogTextPrefab;

		
		public GameObject LogIconPrefab;

		
		public GameObject StickTextPrefab;

		
		public GameObject StickIconPrefab;

		
		public GameObject RockTextPrefab;

		
		public GameObject RockIconPrefab;

		
		[Header("Destruction")]
		public Transform BuildingCollapsingDust;

		
		public Transform LogPickupPrefab;

		
		public BuildingRepair BuildingRepairTriggerPrefab;

		
		public GameObject DestroyedLeafShelter;

		
		[Header("Particles")]
		[NameFromEnumIndex(typeof(HitParticles))]
		public ParticleSystem[] HitPSPrefabs;

		
		public ParticleSystem FireHitPSPrefab;

		
		public ParticleSystem FliesPSPrefab;

		
		public ParticleSystem[] BloodHitPSPrefabs;

		
		public ParticleSystem[] FootStepPrefabs;

		
		public ParticleSystem woodChopPrefab;

		
		public ParticleSystem BurntSmokeAndDustPrefab;

		
		public Transform SmashBloodPrefab;

		
		[Header("Mp")]
		public GameObject HashPositionToNamePrefab;

		
		public GameObject PlayerPrefab;

		
		public StealItemTrigger DeadBackpackPrefab;

		
		public GameObject DeadPlayerLootIconSheen;

		
		public GameObject DeadPlayerLootIconPickup;

		
		public CoopWeaponUpgradesProxy WeaponUpgradesProxy;

		
		public AnimationSequenceProxy AnimationSequenceProxy;

		
		[Header("Environment")]
		public Material LowQualityWaterMaterial;

		
		public Material PlaneMossMaterial;

		
		[Header("GameModes")]
		[NameFromEnumIndex(typeof(GameTypes))]
		public GameObject[] GameModePrefabs;

		
		[Header("Debug/Diag")]
		public GameObject DeviceDebugInformation;

		
		private SpawnPool _psSpawnPool;

		
		private Dictionary<Material, Material> _materialInstances;

		
		public static Prefabs Instance;

		
		[Serializable]
		public class TrophyPrefab
		{
			
			[ItemIdPicker]
			public int _itemId;

			
			public Transform _prefab;
		}
	}
}
