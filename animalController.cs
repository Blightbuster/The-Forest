using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using PathologicalGames;
using TheForest.Utils;
using TheForest.Utils.Settings;
using TheForest.Utils.WorkSchedulerInterfaces;
using UnityEngine;


public class animalController : MonoBehaviour, IThreadSafeTask
{
	
	
	private GameObject currentCamGo
	{
		get
		{
			return (!LocalPlayer.GameObject) ? Scene.ActiveMB.gameObject : LocalPlayer.GameObject;
		}
	}

	
	
	private Camera currentCamera
	{
		get
		{
			return (!LocalPlayer.MainCam) ? Camera.main : LocalPlayer.MainCam;
		}
	}

	
	private void Awake()
	{
		this.sceneInfo = Scene.SceneTracker;
		this.thisTr = base.transform;
		if (GameSetup.IsHardSurvivalMode && this.turtleAmount > 0)
		{
			this.turtleAmount = 1;
		}
		this.layer = 26;
		this.layerMask = 1 << this.layer;
		if (this.WsToken == -1)
		{
			this.WsToken = WorkScheduler.Register(this, base.transform.position, true);
		}
	}

	
	private void OnEnable()
	{
		base.InvokeRepeating("callSpawnCreatures", UnityEngine.Random.Range(0.1f, 4f), 4f);
		if (this.WsToken == -1)
		{
			this.WsToken = WorkScheduler.Register(this, base.transform.position, false);
		}
	}

	
	private void OnDisable()
	{
		base.CancelInvoke("callSpawnCreatures");
		base.StopAllCoroutines();
		if (this.WsToken != -1)
		{
			WorkScheduler.Unregister(this, this.WsToken);
			this.WsToken = -1;
		}
	}

	
	private void OnDestroy()
	{
		try
		{
			if (this.WsToken != -1)
			{
				WorkScheduler.Unregister(this, this.WsToken);
				this.WsToken = -1;
			}
		}
		catch
		{
		}
	}

	
	
	
	public bool ShouldDoMainThreadRefresh { get; set; }

	
	public void ThreadedRefresh()
	{
		this.ShouldDoMainThreadRefresh = (this.FishSpawnBackCounter.Count > 0 && this.FishSpawnBackCounter.Peek() < Scene.Clock.ElapsedGameTime);
	}

	
	public void MainThreadRefresh()
	{
		while (this.FishSpawnBackCounter.Count > 0 && this.FishSpawnBackCounter.Peek() < Scene.Clock.ElapsedGameTime)
		{
			this.FishSpawnBackCounter.Dequeue();
			this.fishCountOffset--;
		}
	}

	
	private void callSpawnCreatures()
	{
		if (!Clock.planecrash && this.currentCamera)
		{
			this.doSpawnCreatures();
		}
		else if (CoopPeerStarter.DedicatedHost && !Clock.planecrash)
		{
			this.doSpawnCreatures();
		}
	}

	
	private void doSpawnCreatures()
	{
		if (!BoltNetwork.isRunning)
		{
			if (this.lizardAmount > 0)
			{
				base.StartCoroutine("spawnLizard");
			}
			if (this.rabbitAmount > 0)
			{
				base.StartCoroutine("spawnRabbit");
			}
			if (this.tortoiseAmount > 0)
			{
				base.StartCoroutine("spawnTortoise");
			}
			if (this.raccoonAmount > 0)
			{
				base.StartCoroutine("spawnRaccoon");
			}
			if (this.deerAmount > 0)
			{
				base.StartCoroutine("spawnDeer");
			}
		}
		if (this.fishAmount > 0)
		{
			base.StartCoroutine("spawnFish");
		}
		if (this.turtleAmount > 0)
		{
			base.StartCoroutine("spawnTurtles");
		}
	}

	
	private IEnumerator spawnLizard()
	{
		this.foundSpawn = false;
		if (Vector3.Distance(this.currentCamGo.transform.position, this.thisTr.position) < this.lizardSpawnDist && this.sceneInfo.currLizardAmount < this.sceneInfo.maxLizardAmount)
		{
			while (!this.foundSpawn)
			{
				this.tempPos = this.RandomSpawnPos((float)UnityEngine.Random.Range(10, 40));
				this.spawnPos = new Vector3(this.tempPos.x + this.thisTr.position.x, this.thisTr.position.y + 60f, this.tempPos.y + this.thisTr.position.z);
				if (Physics.Raycast(this.spawnPos, Vector3.down, out this.hit, 200f, this.layerMask) && this.hit.collider.CompareTag("TerrainMain") && AstarPath.active != null)
				{
					GraphNode node = AstarPath.active.GetNearest(this.hit.point).node;
					if (node.Walkable)
					{
						this.foundSpawn = this.pointOffCamera(this.hit.point);
						yield return null;
					}
				}
				yield return null;
			}
			this.SpawnAnimal(this.lizardGo, this.hit.point, animalController.AnimalType.Lizard);
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator spawnRabbit()
	{
		this.foundSpawn = false;
		if (Vector3.Distance(this.currentCamGo.transform.position, this.thisTr.position) < this.rabbitSpawnDist && this.sceneInfo.currRabbitAmount < this.sceneInfo.maxRabbitAmount)
		{
			while (!this.foundSpawn)
			{
				this.tempPos = this.RandomSpawnPos((float)UnityEngine.Random.Range(10, 30));
				this.spawnPos = new Vector3(this.tempPos.x + this.thisTr.position.x, this.thisTr.position.y + 60f, this.tempPos.y + this.thisTr.position.z);
				if (Physics.Raycast(this.spawnPos, Vector3.down, out this.hit, 200f, this.layerMask) && this.hit.collider.CompareTag("TerrainMain") && AstarPath.active != null)
				{
					GraphNode node = AstarPath.active.GetNearest(this.hit.point).node;
					if (node.Walkable)
					{
						this.foundSpawn = this.pointOffCamera(this.hit.point);
						yield return null;
					}
				}
				yield return null;
			}
			this.SpawnAnimal(this.rabbitGo, this.hit.point, animalController.AnimalType.Rabbit);
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator spawnTortoise()
	{
		this.foundSpawn = false;
		if (Vector3.Distance(this.currentCamGo.transform.position, this.thisTr.position) < this.tortoiseSpawnDist && this.sceneInfo.currTortoiseAmount < this.sceneInfo.maxTortoiseAmount)
		{
			while (!this.foundSpawn)
			{
				this.tempPos = this.RandomSpawnPos((float)UnityEngine.Random.Range(10, 30));
				this.spawnPos = new Vector3(this.tempPos.x + this.thisTr.position.x, this.thisTr.position.y + 60f, this.tempPos.y + this.thisTr.position.z);
				if (Physics.Raycast(this.spawnPos, Vector3.down, out this.hit, 200f, this.layerMask) && this.hit.collider.CompareTag("TerrainMain") && AstarPath.active != null)
				{
					GraphNode node = AstarPath.active.GetNearest(this.hit.point).node;
					if (node.Walkable)
					{
						this.foundSpawn = this.pointOffCamera(this.hit.point);
						yield return null;
					}
				}
				yield return null;
			}
			this.SpawnAnimal(this.tortoiseGo, this.hit.point, animalController.AnimalType.Tortoise);
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator spawnRaccoon()
	{
		this.foundSpawn = false;
		if (Vector3.Distance(this.currentCamGo.transform.position, this.thisTr.position) < this.raccoonSpawnDist && this.sceneInfo.currRaccoonAmount < this.sceneInfo.maxRaccoonAmount)
		{
			while (!this.foundSpawn)
			{
				this.tempPos = this.RandomSpawnPos((float)UnityEngine.Random.Range(10, 30));
				this.spawnPos = new Vector3(this.tempPos.x + this.thisTr.position.x, this.thisTr.position.y + 60f, this.tempPos.y + this.thisTr.position.z);
				if (Physics.Raycast(this.spawnPos, Vector3.down, out this.hit, 200f, this.layerMask) && this.hit.collider.CompareTag("TerrainMain") && AstarPath.active != null)
				{
					GraphNode node = AstarPath.active.GetNearest(this.hit.point).node;
					if (node.Walkable)
					{
						this.foundSpawn = this.pointOffCamera(this.hit.point);
						yield return null;
					}
				}
				yield return null;
			}
			this.SpawnAnimal(this.raccoonGo, this.hit.point, animalController.AnimalType.Raccon);
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator spawnDeer()
	{
		this.foundSpawn = false;
		if (Vector3.Distance(this.currentCamGo.transform.position, this.thisTr.position) < this.deerSpawnDist && this.sceneInfo.currDeerAmount < this.sceneInfo.maxDeerAmount)
		{
			while (!this.foundSpawn)
			{
				this.tempPos = this.RandomSpawnPos((float)UnityEngine.Random.Range(10, 30));
				this.spawnPos = new Vector3(this.tempPos.x + this.thisTr.position.x, this.thisTr.position.y + 60f, this.tempPos.y + this.thisTr.position.z);
				if (Physics.Raycast(this.spawnPos, Vector3.down, out this.hit, 200f, this.layerMask) && this.hit.collider.CompareTag("TerrainMain") && AstarPath.active != null)
				{
					GraphNode node = AstarPath.active.GetNearest(this.hit.point).node;
					if (node.Walkable)
					{
						this.foundSpawn = this.pointOffCamera(this.hit.point);
						yield return null;
					}
				}
				yield return null;
			}
			this.SpawnAnimal(this.deerGo, this.hit.point, animalController.AnimalType.Deer);
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator spawnTurtles()
	{
		if (BoltNetwork.isClient)
		{
			yield break;
		}
		if (LocalPlayer.IsInCaves)
		{
			yield break;
		}
		if (Time.time < this.setSpawnDelay)
		{
			yield break;
		}
		if (this.getClosestPlayer(false) == null)
		{
			yield break;
		}
		float getDist = Vector3.Distance(this.getClosestPlayer(false).position, this.thisTr.position);
		if (getDist < this.turtleSpawnDist && this.currTurtleAmount < this.turtleAmount)
		{
			this.tempPos = this.RandomSpawnPos(UnityEngine.Random.Range(5f, 20f));
			this.spawnPos = new Vector3(this.tempPos.x + this.thisTr.position.x, this.thisTr.position.y, this.tempPos.y + this.thisTr.position.z);
			this.SpawnAnimal(this.turtleGo, this.spawnPos, animalController.AnimalType.Turtle);
			this.foundSpawn = false;
			yield return YieldPresets.WaitTenSeconds;
		}
		yield return null;
		yield break;
	}

	
	private void AttachAnimalToNetwork(GameObject gameObject)
	{
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

	
	private IEnumerator spawnFish()
	{
		if (this.sharkFish && BoltNetwork.isClient)
		{
			yield break;
		}
		if (this.fishGo.Length == 0)
		{
			yield break;
		}
		Transform closestPlayer = (!this.caveFish || SteamDSConfig.isDedicatedServer || !LocalPlayer.IsInCaves) ? this.getClosestPlayer(this.caveFish) : LocalPlayer.Transform;
		if (closestPlayer == null)
		{
			if (this.fishSpawned)
			{
				for (int i = 0; i < this.allFish.Count; i++)
				{
					if (this.allFish[i])
					{
						PoolManager.Pools["creatures"].Despawn(this.allFish[i].transform);
					}
				}
			}
			yield break;
		}
		this.fishPlayerDist = Vector3.Distance(closestPlayer.position, this.thisTr.position);
		if (this.fishPlayerDist < this.fishSpawnDist && !this.fishSpawned)
		{
			bool doneFish = false;
			while ((float)(this.totalFish + this.fishCountOffset) < (float)this.fishAmount * GameSettings.Animals.FishMaxAmountRatio && !doneFish)
			{
				this.tempPos = this.RandomSpawnPos(UnityEngine.Random.Range(5f, this.fishSpawnRange));
				this.spawnPos = new Vector3(this.tempPos.x + this.thisTr.position.x, this.thisTr.position.y, this.tempPos.y + this.thisTr.position.z);
				int randFish = UnityEngine.Random.Range(0, this.fishGo.Length);
				int startingFish = this.totalFish;
				this.SpawnAnimal(this.fishGo[randFish], this.spawnPos, animalController.AnimalType.Fish);
				if (startingFish == this.totalFish)
				{
					doneFish = true;
				}
				yield return null;
			}
			this.fishSpawned = true;
		}
		else if (this.fishPlayerDist > this.fishSpawnDist)
		{
			if (this.fishSpawned)
			{
				for (int j = 0; j < this.allFish.Count; j++)
				{
					if (this.allFish[j])
					{
						PoolManager.Pools["creatures"].Despawn(this.allFish[j].transform);
					}
				}
			}
			this.fishSpawned = false;
			this.allFish.Clear();
			this.totalFish = 0;
		}
		yield return null;
		yield break;
	}

	
	private bool pointOffCamera(Vector3 pos)
	{
		this.screenPos = this.currentCamera.WorldToViewportPoint(pos);
		return this.screenPos.x < 0f || this.screenPos.x > 1f || this.screenPos.y < 0f || this.screenPos.y > 1f;
	}

	
	private Vector2 RandomSpawnPos(float radius)
	{
		Vector2 insideUnitCircle = UnityEngine.Random.insideUnitCircle;
		insideUnitCircle.Normalize();
		return insideUnitCircle * radius;
	}

	
	public void seCurrTurtleAmount(int amount)
	{
		this.currTurtleAmount += amount;
	}

	
	private Transform getClosestPlayer(bool cave)
	{
		this.closestDist = float.PositiveInfinity;
		List<GameObject> list;
		if (cave)
		{
			list = this.sceneInfo.allPlayersInCave;
		}
		else
		{
			list = this.sceneInfo.allPlayers;
		}
		if (list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i])
				{
					float sqrMagnitude = (base.transform.position - list[i].transform.position).sqrMagnitude;
					if (sqrMagnitude < this.closestDist)
					{
						this.closestDist = sqrMagnitude;
						this.closestPlayer = list[i].transform;
					}
				}
			}
		}
		return this.closestPlayer;
	}

	
	public void addToSpawnDelay(float val)
	{
		this.addSpawnDelay += val;
		this.setSpawnDelay = this.addSpawnDelay + Time.time;
	}

	
	public void AddFishSpawnBackCounter()
	{
		this.totalFish--;
		this.fishCountOffset++;
		this.FishSpawnBackCounter.Enqueue(Scene.Clock.ElapsedGameTime + GameSettings.Animals.FishRespawnDelay);
	}

	
	private void SpawnAnimal(GameObject prefab, Vector3 spawnPos, animalController.AnimalType animalType)
	{
		Transform transform = PoolManager.Pools["creatures"].Spawn(prefab.transform, spawnPos, Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f));
		if (transform != null)
		{
			switch (animalType)
			{
			case animalController.AnimalType.Lizard:
				transform.SendMessage("startUpdateSpawn");
				this.sceneInfo.currLizardAmount++;
				break;
			case animalController.AnimalType.Turtle:
				transform.SendMessage("startUpdateSpawn");
				transform.SendMessage("setController", base.gameObject);
				this.currTurtleAmount++;
				if (BoltNetwork.isRunning)
				{
					this.AttachAnimalToNetwork(transform.gameObject);
				}
				break;
			case animalController.AnimalType.Rabbit:
				transform.SendMessage("startUpdateSpawn");
				this.sceneInfo.currRabbitAmount++;
				break;
			case animalController.AnimalType.Fish:
				transform.position = new Vector3(transform.position.x, transform.position.y + UnityEngine.Random.Range(-0.4f, 0f), transform.position.z);
				transform.SendMessage("startFish", base.gameObject);
				transform.SendMessage("setOcean", this.oceanFish);
				transform.SendMessage("setCave", this.caveFish);
				this.totalFish++;
				this.allFish.Add(transform.gameObject);
				transform.GetComponent<Fish>().controller = this;
				if (this.sharkFish && BoltNetwork.isRunning)
				{
					this.AttachAnimalToNetwork(transform.gameObject);
				}
				break;
			case animalController.AnimalType.Tortoise:
				transform.SendMessage("startUpdateSpawn");
				this.sceneInfo.currTortoiseAmount++;
				break;
			case animalController.AnimalType.Raccon:
				transform.SendMessage("startUpdateSpawn");
				this.sceneInfo.currRaccoonAmount++;
				break;
			case animalController.AnimalType.Deer:
				transform.SendMessage("startUpdateSpawn");
				this.sceneInfo.currDeerAmount++;
				break;
			}
		}
	}

	
	private void OnSpawnLizard(GameObject spawned)
	{
		spawned.SendMessage("startUpdateSpawn");
		this.sceneInfo.currLizardAmount++;
	}

	
	private sceneTracker sceneInfo;

	
	public int lizardAmount;

	
	public int turtleAmount;

	
	public int gooseAmount;

	
	public int rabbitAmount;

	
	public int fishAmount;

	
	public int tortoiseAmount;

	
	public int raccoonAmount;

	
	public int deerAmount;

	
	public GameObject lizardGo;

	
	public BundleKey lizardKey;

	
	public GameObject turtleGo;

	
	public GameObject gooseGo;

	
	public GameObject rabbitGo;

	
	public GameObject[] fishGo;

	
	public GameObject tortoiseGo;

	
	public GameObject raccoonGo;

	
	public GameObject deerGo;

	
	public bool oceanFish;

	
	public bool caveFish;

	
	public bool sharkFish;

	
	public float lizardSpawnDist;

	
	public float turtleSpawnDist;

	
	public float rabbitSpawnDist;

	
	public float fishSpawnDist;

	
	public float fishSpawnRange;

	
	public float tortoiseSpawnDist;

	
	public float raccoonSpawnDist;

	
	public float deerSpawnDist;

	
	public List<GameObject> allFish = new List<GameObject>();

	
	private int currTurtleAmount;

	
	public float addSpawnDelay = 1f;

	
	[SerializeThis]
	public float setSpawnDelay;

	
	private float delayUpdateInterval = 1f;

	
	private Transform thisTr;

	
	public int totalFish;

	
	public int fishCountOffset;

	
	private Vector3 screenPos;

	
	private Vector3 spawnPos;

	
	private Vector2 tempPos;

	
	private float terrainPosY;

	
	private bool foundSpawn;

	
	private int type;

	
	public bool fishSpawned;

	
	private RaycastHit hit;

	
	private int layer;

	
	private int layerMask;

	
	private int WsToken = -1;

	
	[HideInInspector]
	public Queue<float> FishSpawnBackCounter = new Queue<float>();

	
	public float fishPlayerDist;

	
	private float closestDist = float.PositiveInfinity;

	
	private Transform closestPlayer;

	
	private enum AnimalType
	{
		
		Lizard,
		
		Turtle,
		
		Goose,
		
		Rabbit,
		
		Fish,
		
		Tortoise,
		
		Raccon,
		
		Deer
	}
}
