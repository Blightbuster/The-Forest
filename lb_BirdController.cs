using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using TheForest.Utils;
using UnityEngine;


public class lb_BirdController : MonoBehaviour
{
	
	public void AllFlee()
	{
		for (int i = 0; i < this.myBirds.Length; i++)
		{
			if (this.myBirds[i].activeSelf)
			{
				this.myBirds[i].SendMessage("Flee");
			}
		}
	}

	
	public void Pause()
	{
		if (this.pause)
		{
			this.AllUnPause();
		}
		else
		{
			this.AllPause();
		}
	}

	
	public void AllPause()
	{
		this.pause = true;
		for (int i = 0; i < this.myBirds.Length; i++)
		{
			if (this.myBirds[i].activeSelf)
			{
				this.myBirds[i].SendMessage("PauseBird");
			}
		}
	}

	
	public void AllUnPause()
	{
		this.pause = false;
		for (int i = 0; i < this.myBirds.Length; i++)
		{
			if (this.myBirds[i].activeSelf)
			{
				this.myBirds[i].SendMessage("UnPauseBird");
			}
		}
	}

	
	public void SpawnAmount(int amt)
	{
		for (int i = 0; i <= amt; i++)
		{
			this.SpawnBird(this.getNextPlayerSpawn());
		}
	}

	
	private GameObject getNextPlayerSpawn()
	{
		GameObject gameObject = null;
		int num = 0;
		if (Scene.SceneTracker.allPlayers.Count > 0)
		{
			while (gameObject == null)
			{
				if (this.lastPlayerCounter >= Scene.SceneTracker.allPlayers.Count)
				{
					this.lastPlayerCounter = 0;
				}
				gameObject = Scene.SceneTracker.allPlayers[this.lastPlayerCounter];
				this.lastPlayerCounter++;
				num++;
				if (num > 10)
				{
					gameObject = LocalPlayer.GameObject;
					break;
				}
			}
		}
		return gameObject;
	}

	
	private bool isBirdVisible(GameObject bird, GameObject player)
	{
		Vector3 vector = player.transform.InverseTransformPoint(bird.transform.position);
		float num = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
		float sqrMagnitude = (bird.transform.position - player.transform.position).sqrMagnitude;
		return num < 90f && num > -90f && sqrMagnitude < 10000f;
	}

	
	private void checkHandConditions()
	{
		if (CoopPeerStarter.DedicatedHost)
		{
			return;
		}
		if (LocalPlayer.GameObject)
		{
			this.birdCount = 0;
			if (LocalPlayer.Rigidbody.velocity.magnitude < 0.2f && !this.breakBool && Scene.SceneTracker.allAttackers == 0 && !LocalPlayer.Animator.GetBool("bookHeld") && !LocalPlayer.FpCharacter.PushingSled)
			{
				if (BoltNetwork.isClient)
				{
					for (int i = 0; i < Scene.SceneTracker.clientBirds.Count; i++)
					{
						if (Scene.SceneTracker.clientBirds[i] && this.isBirdVisible(Scene.SceneTracker.clientBirds[i], LocalPlayer.GameObject))
						{
							this.birdCount++;
						}
					}
				}
				else
				{
					for (int j = 0; j < this.myBirds.Length; j++)
					{
						if (this.myBirds[j] && this.isBirdVisible(this.myBirds[j], LocalPlayer.GameObject))
						{
							this.birdCount++;
						}
					}
				}
				if (this.birdCount > 4)
				{
					this.repeatCount++;
				}
				if (this.repeatCount > 3 && UnityEngine.Random.value < 0.5f)
				{
					LocalPlayer.GameObject.SendMessage("doBirdOnHand");
					this.repeatCount = 0;
					this.breakBool = true;
					base.Invoke("resetBreakBool", 100f);
				}
			}
			else
			{
				this.repeatCount = 0;
				this.birdCount = 0;
			}
		}
	}

	
	private void resetBreakBool()
	{
		this.breakBool = false;
	}

	
	private void Start()
	{
		base.StartCoroutine(this.StartBirdsRoutine());
	}

	
	private IEnumerator StartBirdsRoutine()
	{
		base.InvokeRepeating("checkHandConditions", 1f, 8f);
		if (BoltNetwork.isClient)
		{
			yield break;
		}
		this.rg = AstarPath.active.astarData.recastGraph;
		this.layerMask = 513;
		if (GameSetup.IsHardSurvivalMode)
		{
			this.idealNumberOfBirds = 4;
		}
		base.InvokeRepeating("updateBirdAmounts", 1f, 120f);
		this.initIdealBirds = this.idealNumberOfBirds;
		this.initMaxBirds = this.maximumNumberOfBirds;
		this.modIdealBirds = this.idealNumberOfBirds;
		this.modMaxBirds = this.maximumNumberOfBirds;
		this.sceneInfo = Scene.SceneTracker;
		if (this.idealNumberOfBirds >= this.maximumNumberOfBirds)
		{
			this.idealNumberOfBirds = this.maximumNumberOfBirds - 1;
		}
		this.myBirds = new GameObject[this.maximumNumberOfBirds];
		int nextVal = 0;
		for (int i = 0; i < this.myBirds.Length; i++)
		{
			GameObject bird = UnityEngine.Object.Instantiate<GameObject>(this.birdPrefabs[nextVal], Vector3.zero, Quaternion.identity);
			this.myBirds[i] = bird;
			nextVal++;
			if (nextVal == this.birdPrefabs.Length)
			{
				nextVal = 0;
			}
		}
		for (int j = 0; j < this.myBirds.Length; j++)
		{
			lb_Bird component = this.myBirds[j].GetComponent<lb_Bird>();
			if (component)
			{
				component.SetController(this);
			}
			this.myBirds[j].SetActive(false);
		}
		if (!base.IsInvoking("UpdateBirds"))
		{
			if (GameSetup.IsHardSurvivalMode)
			{
				base.InvokeRepeating("UpdateBirds", 10f, 30f);
			}
			else
			{
				base.InvokeRepeating("UpdateBirds", 10f, 5f);
			}
		}
		base.StopCoroutine("UpdateTargets");
		this.birdPerchTargets.Clear();
		this.birdGroundTargets.Clear();
		base.StartCoroutine("UpdateTargets");
		yield return null;
		yield break;
	}

	
	private void Update()
	{
		if (Scene.SceneTracker == null)
		{
			return;
		}
		if (Time.time > this.removeBirdDelay && !Clock.planecrash)
		{
			this.updateRemoveBirdTargets();
		}
	}

	
	private void updateRemoveBirdTargets()
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
		if (Scene.SceneTracker == null)
		{
			return;
		}
		if (Scene.SceneTracker.allPlayers.Count == 0)
		{
			return;
		}
		if (LocalPlayer.IsInCaves && !BoltNetwork.isRunning)
		{
			for (int i = 0; i < this.myBirds.Length; i++)
			{
				if (this.myBirds[i].activeSelf)
				{
					this.Unspawn(this.myBirds[i]);
				}
			}
		}
		else if (this.myBirds[this.removeBirdIndex].activeSelf && Scene.SceneTracker.GetClosestPlayerDistanceFromPos(this.myBirds[this.removeBirdIndex].transform.position) > this.unspawnDistance)
		{
			this.Unspawn(this.myBirds[this.removeBirdIndex]);
		}
		this.removeBirdIndex = ((this.removeBirdIndex != this.myBirds.Length - 1) ? (this.removeBirdIndex + 1) : 0);
		this.removeBirdDelay = Time.time + 1f;
	}

	
	private void OnDeserialized()
	{
		base.StopCoroutine("UpdateTargets");
	}

	
	private void OnEnable()
	{
		Scene.SceneTracker.birdController = this;
		if (BoltNetwork.isClient)
		{
			return;
		}
		base.StopCoroutine("UpdateTargets");
		base.StartCoroutine("UpdateTargets");
		if (!base.IsInvoking("UpdateBirds"))
		{
			base.InvokeRepeating("UpdateBirds", 30f, 5f);
		}
	}

	
	private Vector3 FindPointInGroundTarget(GameObject target)
	{
		Vector3 vector;
		vector.x = UnityEngine.Random.Range(target.GetComponent<Collider>().bounds.max.x, target.GetComponent<Collider>().bounds.min.x);
		vector.y = target.GetComponent<Collider>().bounds.max.y;
		vector.z = UnityEngine.Random.Range(target.GetComponent<Collider>().bounds.max.z, target.GetComponent<Collider>().bounds.min.z);
		float y = target.GetComponent<Collider>().bounds.size.y;
		RaycastHit raycastHit;
		if (y > 0f && Physics.Raycast(vector, -Vector3.up, out raycastHit, y, this.groundLayer))
		{
			return raycastHit.point;
		}
		return vector;
	}

	
	private void updateBirdAmounts()
	{
		if (this.initIdealBirds < this.modIdealBirds)
		{
			this.initIdealBirds++;
		}
		if (this.initMaxBirds < this.modMaxBirds)
		{
			this.initMaxBirds++;
		}
	}

	
	private void UpdateBirds()
	{
		if (Scene.SceneTracker.doingGlobalNavUpdate)
		{
			return;
		}
		if (!Scene.SceneTracker.waitForLoadSequence)
		{
			return;
		}
		float num = 1f;
		bool flag = false;
		if (BoltNetwork.isRunning && Scene.SceneTracker.allPlayers.Count > 0)
		{
			for (int i = 1; i < Scene.SceneTracker.allPlayers.Count; i++)
			{
				if (Scene.SceneTracker.allPlayers[i] && Scene.SceneTracker.allPlayers[i].activeSelf && (Scene.SceneTracker.allPlayers[0].transform.position - Scene.SceneTracker.allPlayers[i].transform.position).sqrMagnitude > 80f)
				{
					flag = true;
				}
			}
			if (flag)
			{
				num = 1.5f;
			}
		}
		if (Clock.Dark)
		{
			this.idealNumberOfBirds = Mathf.FloorToInt((float)(this.initIdealBirds / 3) * num);
			this.maximumNumberOfBirds = Mathf.FloorToInt((float)(this.initMaxBirds / 3));
		}
		else
		{
			this.idealNumberOfBirds = Mathf.FloorToInt((float)this.initIdealBirds * num);
			this.maximumNumberOfBirds = Mathf.FloorToInt((float)this.initMaxBirds);
		}
		if (this.activeBirds < this.idealNumberOfBirds && this.AreThereActiveTargets() && !Clock.planecrash)
		{
			this.SpawnBird(this.getNextPlayerSpawn());
		}
		else if (this.activeBirds < this.maximumNumberOfBirds && (double)UnityEngine.Random.value < 0.05 && this.AreThereActiveTargets() && !Clock.planecrash)
		{
			this.SpawnBird(this.getNextPlayerSpawn());
		}
	}

	
	private IEnumerator checkRemoveBirds()
	{
		goto IL_00;
		for (;;)
		{
			IL_00:
			goto IL_00;
		}
	}

	
	private IEnumerator UpdateTargets()
	{
		for (;;)
		{
			yield return null;
			yield return null;
			yield return null;
			this.gtRemove.Clear();
			this.ptRemove.Clear();
			for (int i = 0; i < this.birdGroundTargets.Count; i++)
			{
				if (this.birdGroundTargets[i] && (!this.birdGroundTargets[i].transform || Scene.SceneTracker.GetClosestPlayerDistanceFromPos(this.birdGroundTargets[i].transform.position) > this.unspawnDistance))
				{
					this.gtRemove.Add(this.birdGroundTargets[i]);
				}
			}
			for (int j = 0; j < this.birdPerchTargets.Count; j++)
			{
				if (this.birdPerchTargets[j] && (this.birdPerchTargets[j].transform || Scene.SceneTracker.GetClosestPlayerDistanceFromPos(this.birdPerchTargets[j].transform.position) > this.unspawnDistance || !this.birdPerchTargets[j].gameObject.CompareTag("lb_perchTarget")))
				{
					this.ptRemove.Add(this.birdPerchTargets[j]);
				}
			}
			foreach (GameObject item in this.gtRemove)
			{
				this.birdGroundTargets.Remove(item);
			}
			foreach (GameObject item2 in this.ptRemove)
			{
				this.birdPerchTargets.Remove(item2);
			}
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			this.layerMask = 513;
			for (int x = 0; x < Scene.SceneTracker.allPlayers.Count; x++)
			{
				Collider[] hits = Physics.OverlapSphere(Scene.SceneTracker.allPlayers[x].transform.position, this.unspawnDistance, this.layerMask);
				for (int k = 0; k < hits.Length; k++)
				{
					if (hits[k].tag == "lb_groundTarget" && !this.birdGroundTargets.Contains(hits[k].gameObject))
					{
						this.birdGroundTargets.Add(hits[k].gameObject);
					}
					if (hits[k].tag == "lb_perchTarget" && !this.birdPerchTargets.Contains(hits[k].gameObject))
					{
						this.birdPerchTargets.Add(hits[k].gameObject);
					}
				}
				yield return null;
			}
			yield return YieldPresets.WaitFiveSeconds;
		}
		yield break;
	}

	
	public void Unspawn(GameObject bird)
	{
		bird.transform.position = Vector3.zero;
		bird.SetActive(false);
		this.activeBirds--;
	}

	
	public void despawnAll()
	{
		base.CancelInvoke("UpdateBirds");
		base.StopAllCoroutines();
		foreach (GameObject gameObject in this.myBirds)
		{
			if (gameObject)
			{
				this.Unspawn(gameObject);
			}
		}
	}

	
	private void SpawnBird(GameObject closestPlayer)
	{
		if (!this.pause)
		{
			GameObject gameObject = null;
			int num = Mathf.FloorToInt((float)UnityEngine.Random.Range(0, this.myBirds.Length));
			int num2 = 0;
			if (this.myBirds.Length == 0)
			{
				return;
			}
			if (closestPlayer == null)
			{
				return;
			}
			if (Scene.SceneTracker.allPlayersInCave.Contains(closestPlayer))
			{
				return;
			}
			while (gameObject == null)
			{
				if (!this.myBirds[num].activeSelf)
				{
					gameObject = this.myBirds[num];
				}
				num = ((num + 1 < this.myBirds.Length) ? (num + 1) : 0);
				num2++;
				if (num2 >= this.myBirds.Length)
				{
					return;
				}
			}
			lb_Bird component = gameObject.GetComponent<lb_Bird>();
			if (this.sceneInfo.beachMarkers.Count > 0)
			{
				int num3 = 0;
				for (int i = 0; i < this.sceneInfo.beachMarkers.Count; i++)
				{
					if ((this.sceneInfo.beachMarkers[i].transform.position - closestPlayer.transform.position).sqrMagnitude < 14400f)
					{
						num3++;
					}
				}
				if (num3 == 0 && component.typeSeagull)
				{
					return;
				}
				if (num3 > 0 && (component.typeSparrow || component.typeCrow || component.typeBlueBird || component.typeRedBird))
				{
					return;
				}
			}
			gameObject.transform.position = this.FindPositionOffCamera(closestPlayer);
			if (gameObject.transform.position == Vector3.zero)
			{
				return;
			}
			gameObject.SetActive(true);
			if (gameObject.GetComponent<BoltEntity>() && BoltNetwork.isRunning)
			{
				lb_BirdController.AttachBirdToNetwork(gameObject);
			}
			this.activeBirds++;
			this.BirdFindTarget(gameObject);
		}
	}

	
	public static void AttachBirdToNetwork(GameObject go)
	{
		if (BoltNetwork.isServer)
		{
			BoltEntity component = go.GetComponent<BoltEntity>();
			BoltEntity component2 = go.GetComponent<CoopAnimalServer>().NetworkContainerPrefab.GetComponent<BoltEntity>();
			using (BoltEntitySettingsModifier boltEntitySettingsModifier = component2.ModifySettings())
			{
				using (BoltEntitySettingsModifier boltEntitySettingsModifier2 = component.ModifySettings())
				{
					boltEntitySettingsModifier2.clientPredicted = boltEntitySettingsModifier.clientPredicted;
					boltEntitySettingsModifier2.persistThroughSceneLoads = boltEntitySettingsModifier.persistThroughSceneLoads;
					boltEntitySettingsModifier2.allowInstantiateOnClient = boltEntitySettingsModifier.allowInstantiateOnClient;
					boltEntitySettingsModifier2.prefabId = boltEntitySettingsModifier.prefabId;
					boltEntitySettingsModifier2.updateRate = boltEntitySettingsModifier.updateRate;
					boltEntitySettingsModifier2.serializerId = boltEntitySettingsModifier.serializerId;
				}
			}
			BoltNetwork.Attach(go);
		}
	}

	
	private bool AreThereActiveTargets()
	{
		return this.birdGroundTargets.Count > 0 || this.birdPerchTargets.Count > 0;
	}

	
	private Vector3 FindPositionOffCamera(GameObject player)
	{
		Vector3 vector = -player.transform.forward * (float)UnityEngine.Random.Range(60, 70);
		vector = Quaternion.Euler(0f, UnityEngine.Random.Range(-1f, 1f) * 100f, 0f) * vector;
		vector = player.transform.position + vector;
		vector.y = Terrain.activeTerrain.SampleHeight(vector) + Terrain.activeTerrain.transform.position.y;
		GraphNode node = this.rg.GetNearest(vector, NNConstraint.Default).node;
		if (node == null)
		{
			return Vector3.zero;
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
		if (flag)
		{
			vector.y += 30f;
			return vector;
		}
		return Vector3.zero;
	}

	
	private Vector2 Circle2(float radius)
	{
		Vector2 insideUnitCircle = UnityEngine.Random.insideUnitCircle;
		insideUnitCircle.Normalize();
		return insideUnitCircle * radius;
	}

	
	public void BirdFindTarget(GameObject bird)
	{
		this.birdPerchTargets.RemoveAll((GameObject item) => item == null);
		this.birdPerchTargets.RemoveAll((GameObject item) => item != item.GetComponent<SphereCollider>().enabled);
		this.birdGroundTargets.RemoveAll((GameObject item) => item == null);
		this.birdGroundTargets.TrimExcess();
		List<GameObject> list = new List<GameObject>();
		List<GameObject> list2 = new List<GameObject>();
		for (int i = 0; i < this.birdGroundTargets.Count; i++)
		{
			if ((this.birdGroundTargets[i].transform.position - bird.transform.position).sqrMagnitude < 10000f)
			{
				list.Add(this.birdGroundTargets[i]);
			}
		}
		for (int j = 0; j < this.birdPerchTargets.Count; j++)
		{
			if ((this.birdPerchTargets[j].transform.position - bird.transform.position).sqrMagnitude < 10000f)
			{
				list2.Add(this.birdPerchTargets[j]);
			}
		}
		if (list.Count > 0 || list2.Count > 0)
		{
			float num = 0f;
			float num2 = (float)list2.Count * 1f;
			for (int k = 0; k < list.Count; k++)
			{
				num += list[k].GetComponent<Collider>().bounds.size.x * list[k].GetComponent<Collider>().bounds.size.z;
			}
			if (num2 == 0f || UnityEngine.Random.value < num / (num + num2))
			{
				GameObject gameObject = list[Mathf.FloorToInt((float)UnityEngine.Random.Range(0, list.Count))];
				if (bird)
				{
					bird.SendMessage("setNewFlyTarget", this.FindPointInGroundTarget(gameObject), SendMessageOptions.DontRequireReceiver);
					bird.SendMessage("clearPerchTarget", SendMessageOptions.DontRequireReceiver);
				}
			}
			else
			{
				GameObject gameObject = list2[Mathf.FloorToInt((float)UnityEngine.Random.Range(0, list2.Count))];
				if (bird)
				{
					bird.SendMessage("setNewFlyTarget", gameObject.transform.position, SendMessageOptions.DontRequireReceiver);
					bird.SendMessage("setFlyingToPerch", SendMessageOptions.DontRequireReceiver);
					bird.SendMessage("setPerchTarget", gameObject, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		else
		{
			this.tempPos = this.Circle2(70f);
			GameObject closestPlayerFromPos = Scene.SceneTracker.GetClosestPlayerFromPos(bird.transform.position);
			if (closestPlayerFromPos == null)
			{
				return;
			}
			this.flyPos = new Vector3(closestPlayerFromPos.transform.position.x + this.tempPos.x, closestPlayerFromPos.transform.position.y + (float)UnityEngine.Random.Range(10, 25), closestPlayerFromPos.transform.position.z + this.tempPos.y);
			if (bird)
			{
				bird.SendMessage("setNewFlyTarget", this.flyPos, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	
	public int idealNumberOfBirds;

	
	public int maximumNumberOfBirds;

	
	public float unspawnDistance = 10f;

	
	private sceneTracker sceneInfo;

	
	public float drag;

	
	public float landDrag;

	
	public float flySpeed;

	
	public float landSpeed;

	
	public float upSpeed;

	
	public LayerMask groundLayer;

	
	public bool seagull = true;

	
	public bool blueBird = true;

	
	public bool redBird = true;

	
	public bool chickadee = true;

	
	public bool sparrow = true;

	
	public bool crow = true;

	
	private bool pause;

	
	public GameObject[] myBirds;

	
	public GameObject[] birdPrefabs;

	
	public List<GameObject> birdGroundTargets = new List<GameObject>();

	
	public List<GameObject> birdPerchTargets = new List<GameObject>();

	
	private int activeBirds;

	
	private int birdIndex;

	
	private float removeBirdDelay;

	
	private int removeBirdIndex;

	
	public int initIdealBirds;

	
	public int initMaxBirds;

	
	private int modIdealBirds;

	
	private int modMaxBirds;

	
	private int birdCount;

	
	private int repeatCount;

	
	private bool breakBool;

	
	private Vector2 tempPos;

	
	private Vector3 flyPos;

	
	private int layerMask;

	
	public int lastPlayerCounter;

	
	private RecastGraph rg;

	
	private List<GameObject> gtRemove = new List<GameObject>();

	
	private List<GameObject> ptRemove = new List<GameObject>();
}
