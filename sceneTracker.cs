using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using Pathfinding;
using PathologicalGames;
using TheForest.Items.Inventory;
using TheForest.Tools;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Events;


public class sceneTracker : MonoBehaviour
{
	
	private void OnDestroy()
	{
		this.currentNavStructures.Clear();
		this.globalNavStructures.Clear();
		this.dummyNavStructures.Clear();
		this.dummyNavBounds.Clear();
	}

	
	private void OnDeserialized()
	{
		this.doAwake();
	}

	
	private void Awake()
	{
		this.doAwake();
	}

	
	private void doAwake()
	{
		this.initMaxLizards = this.maxLizardAmount;
		this.initMaxRabbits = this.maxRabbitAmount;
		this.crashMarkers = GameObject.FindGameObjectsWithTag("crashMarker");
		this.planeCrash = GameObject.FindGameObjectWithTag("planeCrash");
		if (!this.planeCrash)
		{
			this.planeCrash = GameObject.FindGameObjectWithTag("savePlanePos");
		}
		this.maxSpawnedLogs = 50;
	}

	
	private void Start()
	{
		this.astarGuo = GameObject.Find("AstarGUOGo");
		this.resetHasAttackedPlayer();
		this.resetSearchingFire();
		this.ceilingMarkers = GameObject.FindGameObjectsWithTag("ceiling");
		this.initLizardAmount = this.maxLizardAmount;
		this.initRabbitAmount = this.maxRabbitAmount;
		this.initTurtleAmount = this.maxTurtleAmount;
		this.initTortoiseAmount = this.maxTortoiseAmount;
		this.initRaccoonAmount = this.maxRaccoonAmount;
		this.initDeerAmount = this.maxDeerAmount;
		if (!CoopPeerStarter.DedicatedHost)
		{
			if (FMOD_StudioSystem.instance)
			{
				this.SurfaceMusic = FMOD_StudioSystem.instance.GetEvent(this.SurfaceMusicPath);
				this.CaveMusic = FMOD_StudioSystem.instance.GetEvent(this.CaveMusicPath);
			}
			else
			{
				Debug.LogError("FMOD_StudioSystem.instance is null, could not initialize sceneTracker audio");
			}
		}
		this.MusicEnabled = true;
		this.CombatMusicEnabled = true;
		base.InvokeRepeating("updateAnimalCount", 1f, 60f);
		base.Invoke("checkPlanePos", 0.5f);
		base.InvokeRepeating("cleanupPlayerLists", 1f, 1f);
		if (BoltNetwork.isRunning)
		{
			this.maxAttackers = 12;
		}
		PlayMakerArrayListProxy[] components = base.transform.GetComponents<PlayMakerArrayListProxy>();
		foreach (PlayMakerArrayListProxy playMakerArrayListProxy in components)
		{
			if (playMakerArrayListProxy.referenceName == "attackers")
			{
				this.proxyAttackers = playMakerArrayListProxy;
			}
		}
		this.proxyAttackers.cleanPrefilledLists();
		base.StartCoroutine(this.managePlacedDynamicObjects());
		if (!BoltNetwork.isClient)
		{
			base.StartCoroutine(this.manageSpawnedLogs());
		}
	}

	
	public void OnEnable()
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
		AstarPath.OnGraphsUpdated = (OnScanDelegate)Delegate.Combine(AstarPath.OnGraphsUpdated, new OnScanDelegate(this.RecalculatePath));
	}

	
	private void OnDisable()
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
		AstarPath.OnGraphsUpdated = (OnScanDelegate)Delegate.Remove(AstarPath.OnGraphsUpdated, new OnScanDelegate(this.RecalculatePath));
		this.StopMusic();
		FMODCommon.ReleaseIfValid(this.SurfaceMusic, STOP_MODE.IMMEDIATE);
		FMODCommon.ReleaseIfValid(this.CaveMusic, STOP_MODE.IMMEDIATE);
	}

	
	public GameObject GetClosestPlayerFromPos(Vector3 worldPos)
	{
		GameObject result = null;
		int count = this.allPlayers.Count;
		if (count > 0)
		{
			float num = float.MaxValue;
			for (int i = 0; i < this.allPlayers.Count; i++)
			{
				GameObject gameObject = this.allPlayers[i];
				if (gameObject)
				{
					float num2 = Vector3.Distance(worldPos, gameObject.transform.position);
					if (num2 < num)
					{
						num = num2;
						result = gameObject;
					}
				}
			}
		}
		return result;
	}

	
	public float GetClosestPlayerDistanceFromPos(Vector3 worldPos)
	{
		int count = this.allPlayers.Count;
		float num;
		if (count > 0)
		{
			num = float.MaxValue;
			for (int i = 0; i < this.allPlayerPositions.Count; i++)
			{
				float num2 = Vector3.Distance(worldPos, this.allPlayerPositions[i]);
				if (num2 < num)
				{
					num = num2;
				}
			}
		}
		else
		{
			num = float.MaxValue;
		}
		return num;
	}

	
	public bool AreAllPlayersInCave()
	{
		return this.allPlayers.Count == this.allPlayersInCave.Count;
	}

	
	private void cleanupPlayerLists()
	{
		this.allPlayers.RemoveAll((GameObject o) => o == null);
		this.allPlayersInCave.RemoveAll((GameObject o) => o == null);
		this.allPlayers.TrimExcess();
		this.allPlayersInCave.TrimExcess();
	}

	
	private void updateAnimalCount()
	{
		if (this.maxLizardAmount < this.initLizardAmount)
		{
			this.maxLizardAmount++;
		}
		if (this.maxRabbitAmount < this.initRabbitAmount)
		{
			this.maxRabbitAmount++;
		}
		if (this.maxTurtleAmount < this.initTurtleAmount)
		{
			this.maxTurtleAmount++;
		}
		if (this.maxTortoiseAmount < this.initTortoiseAmount)
		{
			this.maxTortoiseAmount++;
		}
		if (this.maxRaccoonAmount < this.initRaccoonAmount)
		{
			this.maxRaccoonAmount++;
		}
		if (this.maxDeerAmount < this.initDeerAmount)
		{
			this.maxDeerAmount++;
		}
	}

	
	private void StopMusic()
	{
		if (this.ActiveMusic != null && this.ActiveMusic.isValid())
		{
			UnityUtil.ERRCHECK(this.ActiveMusic.stop(STOP_MODE.ALLOWFADEOUT));
			this.ActiveMusic = null;
		}
	}

	
	private void SetActiveMusic(EventInstance newMusic)
	{
		if (this.MusicEnabled && this.ActiveMusic != newMusic)
		{
			float value = 0f;
			if (this.TransitionParameter != null)
			{
				UnityUtil.ERRCHECK(this.TransitionParameter.getValue(out value));
			}
			this.StopMusic();
			this.ActiveMusic = newMusic;
			UnityUtil.ERRCHECK(this.ActiveMusic.getParameter("Transition", out this.TransitionParameter));
			UnityUtil.ERRCHECK(this.TransitionParameter.setValue(value));
			UnityUtil.ERRCHECK(this.ActiveMusic.start());
		}
	}

	
	public void DisableMusic()
	{
		this.MusicEnabled = false;
		this.StopMusic();
	}

	
	public void EnableMusic()
	{
		this.MusicEnabled = true;
		this.CombatMusicEnabled = true;
	}

	
	private sceneTracker.EnemyPresence CalculateEnemyPresence()
	{
		if (this.visibleEnemies.Count > 0 && this.proxyAttackers.arrayList.Count > 0)
		{
			return sceneTracker.EnemyPresence.Attacking;
		}
		if (this.closeEnemies.Count > 0)
		{
			return sceneTracker.EnemyPresence.Nearby;
		}
		return sceneTracker.EnemyPresence.None;
	}

	
	private void EnableCombatMusic()
	{
		this.CombatMusicEnabled = true;
	}

	
	private void Update()
	{
		if (!CoopPeerStarter.DedicatedHost && LocalPlayer.Transform && LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World)
		{
			Vector2 vector = new Vector2(TheForest.Utils.Input.GetAxis("Mouse X"), TheForest.Utils.Input.GetAxis("Mouse Y"));
			float num = vector.magnitude;
			num *= 1f;
			this.taaMotionClamp = Mathf.Clamp(1f - num, 0.2f, 1f);
		}
		else
		{
			this.taaMotionClamp = 1f;
		}
		this.allPlayerPositions.Clear();
		for (int i = 0; i < Scene.SceneTracker.allPlayers.Count; i++)
		{
			this.allPlayerPositions.Add(Scene.SceneTracker.allPlayers[i].transform.position);
		}
		if (LocalPlayer.IsInCaves)
		{
			this.SetActiveMusic(this.CaveMusic);
		}
		else
		{
			this.SetActiveMusic(this.SurfaceMusic);
		}
		try
		{
			sceneTracker.EnemyPresence enemyPresence = this.CalculateEnemyPresence();
			bool flag = enemyPresence == sceneTracker.EnemyPresence.Attacking && !this.CombatMusicEnabled;
			if (enemyPresence != this.CurrentEnemyPresence && !flag)
			{
				if (this.CurrentEnemyPresence == sceneTracker.EnemyPresence.Attacking)
				{
					this.CombatMusicEnabled = false;
					base.Invoke("EnableCombatMusic", 180f);
				}
				this.CurrentEnemyPresence = enemyPresence;
				switch (this.CurrentEnemyPresence)
				{
				case sceneTracker.EnemyPresence.None:
					UnityUtil.ERRCHECK(this.TransitionParameter.setValue(0f));
					break;
				case sceneTracker.EnemyPresence.Nearby:
				{
					float num2 = (!LocalPlayer.IsInCaves) ? this.SurfaceStressMusicChance : this.CaveStressMusicChance;
					if (UnityEngine.Random.Range(0f, 100f) < num2)
					{
						UnityUtil.ERRCHECK(this.TransitionParameter.setValue(2.5f));
					}
					else
					{
						UnityUtil.ERRCHECK(this.TransitionParameter.setValue(0f));
					}
					break;
				}
				case sceneTracker.EnemyPresence.Attacking:
					UnityUtil.ERRCHECK(this.TransitionParameter.setValue(3.5f));
					break;
				}
			}
			if (this.closeEnemies.Count <= 0)
			{
				sceneTracker.NearEnemies = false;
			}
		}
		catch
		{
		}
		if (this.proxyAttackers != null)
		{
			this.allAttackers = this.proxyAttackers.arrayList.Count;
		}
		if (BoltNetwork.isRunning)
		{
			this.maxAttackers = 12;
		}
		if (this.recentlyKilledEnemyCount > 0 && Time.time > this.swarmedByEnemiesReset && Clock.Day > 7)
		{
			this.hasSwarmedByEnemies = false;
			if (Time.time > this.killedEnemyTimer)
			{
				this.recentlyKilledEnemyCount = 0;
				this.hasSwarmedByEnemies = false;
			}
			if (this.recentlyKilledEnemyCount > 7)
			{
				this.hasSwarmedByEnemies = true;
				this.swarmedByEnemiesReset = Time.time + 900f;
				this.recentlyKilledEnemyCount = 0;
			}
		}
		else if (Time.time > this.swarmedByEnemiesReset)
		{
			this.killedEnemyTimer = Time.time + 250f;
			this.hasSwarmedByEnemies = false;
			this.recentlyKilledEnemyCount = 0;
		}
		else
		{
			this.killedEnemyTimer = Time.time + 250f;
		}
	}

	
	public void addToRecentlyKilledEnemies()
	{
		if (Time.time > this.swarmedByEnemiesReset)
		{
			this.recentlyKilledEnemyCount++;
		}
	}

	
	public void addAttacker(int hash)
	{
		if (this.proxyAttackers.arrayList.Count < this.maxAttackers && !this.proxyAttackers.arrayList.Contains(hash))
		{
			this.proxyAttackers.arrayList.Add(hash);
		}
	}

	
	public void removeAttacker(int hash)
	{
		if (this.proxyAttackers.arrayList.Contains(hash))
		{
			this.proxyAttackers.arrayList.Remove(hash);
		}
	}

	
	private void checkPlanePos()
	{
		if (!this.planeCrash)
		{
			this.planeCrash = GameObject.FindGameObjectWithTag("savePlanePos");
		}
	}

	
	private void sendSetupFamilies()
	{
		if (Scene.MutantControler)
		{
			Scene.MutantControler.startSetupFamilies();
		}
	}

	
	public void WentDark()
	{
		this.setNightAnimalAmount();
		this.updateMutantSpawners();
	}

	
	public void WentLight()
	{
		this.setDayAnimalAmount();
		if (Cheats.NoEnemiesDuringDay)
		{
			Scene.MutantControler.StartCoroutine("removeAllEnemies");
		}
		else
		{
			if (Scene.MutantSpawnManager.gameObject.activeSelf && !Clock.planecrash)
			{
				Scene.MutantSpawnManager.setMutantSpawnAmounts();
			}
			if (Scene.MutantControler.gameObject.activeSelf && !Clock.planecrash && !LocalPlayer.IsInCaves)
			{
				foreach (GameObject gameObject in Scene.MutantControler.activeCannibals)
				{
					if (gameObject)
					{
						gameObject.SendMessage("switchToSleep", SendMessageOptions.DontRequireReceiver);
					}
				}
				Scene.MutantSpawnManager.setMutantSpawnAmounts();
				Scene.MutantControler.activateNextSpawn();
			}
		}
	}

	
	public void updateMutantSpawners()
	{
		if (Scene.MutantSpawnManager.gameObject.activeSelf && !Clock.planecrash)
		{
			Scene.MutantSpawnManager.setMutantSpawnAmounts();
		}
		if (Scene.MutantControler.gameObject.activeSelf && !Clock.planecrash)
		{
			Scene.MutantControler.activateNextSpawn();
		}
	}

	
	private void setNightAnimalAmount()
	{
		this.maxLizardAmount = this.initMaxLizards;
		this.maxRabbitAmount = this.initMaxRabbits;
	}

	
	private void setDayAnimalAmount()
	{
		this.maxLizardAmount = this.initMaxLizards;
		this.maxRabbitAmount = this.initMaxRabbits;
	}

	
	public IEnumerator updatePlayerVisDir(Vector3 dir)
	{
		this.playerVisDir = dir;
		if (this.Eye)
		{
			this.Eye.ShowSeenAngle();
		}
		yield return null;
		yield break;
	}

	
	public void addToBuilt(GameObject go)
	{
		if (this.recentlyBuilt.Count > 10)
		{
			this.recentlyBuilt.RemoveAll((GameObject o) => o == null);
		}
		if (this.recentlyBuilt.Count > 10)
		{
			this.recentlyBuilt.RemoveAt(0);
		}
		this.recentlyBuilt.Add(go);
	}

	
	public void addToStructures(GameObject go)
	{
		this.structuresBuilt.Add(go);
		this.structuresBuilt.RemoveAll((GameObject o) => o == null);
	}

	
	public void addToVisible(GameObject go)
	{
		if (!this.visibleEnemies.Contains(go))
		{
			EventRegistry.Enemy.Publish(TfEvent.EnemyInSight, go);
			this.visibleEnemies.Add(go);
		}
	}

	
	public void addToEncounters(GameObject go)
	{
		if (!this.encounters.Contains(go))
		{
			this.encounters.Add(go);
		}
	}

	
	public void removeFromEncounters(GameObject go)
	{
		if (this.encounters.Contains(go))
		{
			this.encounters.Remove(go);
		}
	}

	
	public void removeFromVisible(GameObject go)
	{
		if (this.visibleEnemies.Contains(go))
		{
			this.visibleEnemies.Remove(go);
		}
	}

	
	public void addToJump(Transform tr)
	{
		if (!this.jumpObjects.Contains(tr))
		{
			this.jumpObjects.Add(tr);
		}
	}

	
	public void removeFromJump(Transform tr)
	{
		if (this.jumpObjects.Contains(tr))
		{
			this.jumpObjects.Remove(tr);
		}
	}

	
	public void resetSearchingFire()
	{
		this.hasSearchedFire = false;
	}

	
	public void resetHasAttackedPlayer()
	{
		this.hasAttackedPlayer = false;
	}

	
	public IEnumerator validateClimbingWalls(bool distanceCheck)
	{
		if (BoltNetwork.isClient)
		{
			yield break;
		}
		yield return YieldPresets.WaitTwoSeconds;
		if (!this.validatingClimbingWalls)
		{
			this.validatingClimbingWalls = true;
			List<GameObject> removeMe = new List<GameObject>();
			bool switchSide = false;
			for (int i = 0; i < this.climbableStructures.Count; i++)
			{
				if (this.climbableStructures[i])
				{
					bool validDistance = true;
					if (distanceCheck)
					{
						validDistance = ((this.climbableStructures[i].transform.position - LocalPlayer.Transform.position).sqrMagnitude < 2500f);
					}
					if (validDistance)
					{
						Vector3 castPos = this.climbableStructures[i].transform.position;
						castPos.y = Terrain.activeTerrain.SampleHeight(castPos) + Terrain.activeTerrain.transform.position.y;
						if (switchSide)
						{
							castPos -= this.climbableStructures[i].transform.right * 5.5f;
						}
						else
						{
							castPos += this.climbableStructures[i].transform.right * 5.5f;
						}
						Vector3 storePos = castPos;
						castPos.y += 100f;
						switchSide = !switchSide;
						RaycastHit hit;
						if (Physics.SphereCast(castPos, 4f, Vector3.down, out hit, 110f, this.climbLayerMask))
						{
							removeMe.Add(this.climbableStructures[i].gameObject);
						}
						else
						{
							climbableWallSetup cws = this.climbableStructures[i].GetComponent<climbableWallSetup>();
							if (!cws)
							{
								cws = this.climbableStructures[i].AddComponent<climbableWallSetup>();
							}
							cws.runToPosition = storePos;
							Debug.DrawRay(castPos, Vector3.down * 100f, Color.red, 15f);
						}
					}
				}
				yield return null;
				yield return null;
				yield return null;
			}
			if (removeMe.Count == 0)
			{
				this.validatingClimbingWalls = false;
				yield break;
			}
			for (int x = 0; x < removeMe.Count; x++)
			{
				if (this.climbableStructures.Contains(removeMe[x]))
				{
					this.climbableStructures.Remove(removeMe[x]);
				}
			}
			this.validatingClimbingWalls = false;
		}
		yield return null;
		yield break;
	}

	
	public IEnumerator doStructureBoundsNavRemove(Transform root, Bounds startBounds, float delay)
	{
		if (BoltNetwork.isClient)
		{
			yield break;
		}
		if (this.currentNavStructures.Contains(root.gameObject))
		{
			yield break;
		}
		this.currentNavStructures.Add(root.gameObject);
		if (!AstarPath.active)
		{
			yield break;
		}
		if (this.doingNavUpdate)
		{
			yield break;
		}
		this.doingNavUpdate = true;
		yield return new WaitForSeconds(delay);
		if (!root)
		{
			yield break;
		}
		Bounds combinedBounds = startBounds;
		List<Collider> allCol = new List<Collider>();
		for (int x = 0; x < this.currentNavStructures.Count; x++)
		{
			Collider[] getCol = this.currentNavStructures[x].GetComponentsInChildren<Collider>();
			for (int y = 0; y < getCol.Length; y++)
			{
				gridObjectBlocker st = getCol[y].transform.GetComponent<gridObjectBlocker>();
				if (st)
				{
					allCol.Add(getCol[y]);
				}
			}
		}
		Collider rootCol = root.GetComponent<Collider>();
		if (rootCol)
		{
			allCol.Add(rootCol);
		}
		for (int i = 0; i < allCol.Count; i++)
		{
			combinedBounds.Encapsulate(allCol[i].bounds);
		}
		GraphUpdateObject guo = new GraphUpdateObject(combinedBounds);
		while (this.graphsBeingUpdated)
		{
			yield return null;
		}
		this.graphsBeingUpdated = true;
		AstarPath.active.astarData.recastGraph.rasterizeColliders = false;
		int indexOfGraph = (int)AstarPath.active.astarData.recastGraph.graphIndex;
		guo.nnConstraint.graphMask = 1 << indexOfGraph;
		AstarPath.active.UpdateGraphs(guo, 0f);
		this.doingNavUpdate = false;
		this.currentNavStructures.Clear();
		base.StartCoroutine(this.validateClimbingWalls(true));
		while (this.graphsBeingUpdated)
		{
			AstarPath.active.astarData.recastGraph.rasterizeColliders = false;
			yield return null;
		}
		Scene.MutantControler.calculateMainNavArea();
		yield break;
	}

	
	public IEnumerator doGlobalStructureBoundsNavRemove(Transform root, Bounds startBounds)
	{
		if (BoltNetwork.isClient)
		{
			yield break;
		}
		if (this.globalNavStructures.Contains(root.gameObject))
		{
			yield break;
		}
		this.globalNavStructures.Add(root.gameObject);
		if (this.doingGlobalNavUpdate)
		{
			yield break;
		}
		this.doingGlobalNavUpdate = true;
		yield return new WaitForSeconds(0.5f);
		while (!this.waitForLoadSequence)
		{
			yield return null;
		}
		if (!AstarPath.active)
		{
			yield break;
		}
		if (!root)
		{
			yield break;
		}
		int globalNavStructureCount = this.globalNavStructures.Count;
		for (int y = 0; y < globalNavStructureCount; y++)
		{
			Transform startTr = this.globalNavStructures[y].transform;
			globalNavId id = startTr.GetComponent<globalNavId>();
			if (!id)
			{
				id = startTr.gameObject.AddComponent<globalNavId>();
				id.navId = y;
				for (int b = y + 1; b < globalNavStructureCount; b++)
				{
					globalNavId addId = this.globalNavStructures[b].GetComponent<globalNavId>();
					if (!addId && (startTr.position - this.globalNavStructures[b].transform.position).sqrMagnitude < 10000f)
					{
						addId = this.globalNavStructures[b].AddComponent<globalNavId>();
						addId.navId = y;
					}
				}
			}
		}
		int countUpdates = 0;
		List<GameObject> processedStructures = new List<GameObject>();
		for (int c = 0; c < globalNavStructureCount; c++)
		{
			globalNavId id2 = this.globalNavStructures[c].GetComponent<globalNavId>();
			int startId = id2.navId;
			if (!processedStructures.Contains(this.globalNavStructures[c]))
			{
				List<Collider> allCol = new List<Collider>();
				for (int x = 0; x < globalNavStructureCount; x++)
				{
					globalNavId getId = this.globalNavStructures[x].GetComponent<globalNavId>();
					if (getId.navId == startId)
					{
						processedStructures.Add(this.globalNavStructures[x]);
						Collider[] getCol = this.globalNavStructures[x].GetComponentsInChildren<Collider>();
						for (int y2 = 0; y2 < getCol.Length; y2++)
						{
							gridObjectBlocker st = getCol[y2].transform.GetComponent<gridObjectBlocker>();
							if (st)
							{
								allCol.Add(getCol[y2]);
							}
						}
						Collider rootCol = this.globalNavStructures[x].GetComponent<Collider>();
						if (rootCol && rootCol.GetComponent<gridObjectBlocker>())
						{
							allCol.Add(rootCol);
						}
					}
				}
				Bounds combinedBounds = allCol[0].bounds;
				for (int i = 1; i < allCol.Count; i++)
				{
					combinedBounds.Encapsulate(allCol[i].bounds);
				}
				countUpdates++;
				GraphUpdateObject guo = new GraphUpdateObject(combinedBounds);
				this.graphsBeingUpdated = true;
				AstarPath.active.astarData.recastGraph.rasterizeColliders = false;
				int indexOfGraph = (int)AstarPath.active.astarData.recastGraph.graphIndex;
				guo.nnConstraint.graphMask = 1 << indexOfGraph;
				AstarPath.active.UpdateGraphs(guo, 0f);
			}
		}
		float startNavTime = Time.realtimeSinceStartup;
		this.astarGuo.GetComponent<TileHandlerHelper>().ForceUpdate();
		AstarPath.active.FlushWorkItems();
		this.doingGlobalNavUpdate = false;
		foreach (GameObject go in this.globalNavStructures)
		{
			if (go)
			{
				globalNavId id3 = go.GetComponent<globalNavId>();
				if (id3)
				{
					UnityEngine.Object.Destroy(id3);
				}
			}
		}
		this.globalNavStructures.Clear();
		Scene.MutantControler.calculateMainNavArea();
		Debug.Log("startup nav cut structures time = " + (Time.realtimeSinceStartup - startNavTime));
		yield break;
	}

	
	public IEnumerator startDummyNavRemove(GameObject root, Vector3 pos, Bounds b)
	{
		if (this.dummyNavStructures.Contains(root.gameObject))
		{
			yield break;
		}
		this.dummyNavStructures.Add(root.gameObject);
		this.dummyNavBounds.Add(b);
		if (!AstarPath.active)
		{
			yield break;
		}
		if (this.doingDummyNavUpdate)
		{
			yield break;
		}
		this.doingDummyNavUpdate = true;
		yield return new WaitForSeconds(7f);
		Bounds combinedBounds = b;
		for (int i = 0; i < this.dummyNavBounds.Count; i++)
		{
			combinedBounds.Encapsulate(this.dummyNavBounds[i]);
		}
		GameObject nav = (GameObject)UnityEngine.Object.Instantiate((GameObject)Resources.Load("dummyRootNavRemove"), pos, Quaternion.identity);
		nav.SendMessage("doRootNavRemove", combinedBounds);
		this.doingDummyNavUpdate = false;
		this.dummyNavStructures.Clear();
		yield break;
	}

	
	public IEnumerator refreshCloseNavCutters(Vector3 pos)
	{
		if (BoltNetwork.isClient)
		{
			yield break;
		}
		this.worldNavCuts.RemoveAll((NavmeshCut o) => o == null);
		yield return YieldPresets.WaitTwoSeconds;
		for (int i = 0; i < this.worldNavCuts.Count; i++)
		{
			if (this.worldNavCuts[i] && (pos - this.worldNavCuts[i].transform.position).sqrMagnitude < 10000f)
			{
				this.worldNavCuts[i].enabled = false;
				this.worldNavCuts[i].enabled = true;
			}
			yield return null;
		}
		yield return null;
		yield break;
	}

	
	public void RecalculatePath(AstarPath astar)
	{
		this.graphsBeingUpdated = false;
	}

	
	public IEnumerator managePlacedDynamicObjects()
	{
		int i = 0;
		int y = 0;
		bool flip = false;
		for (;;)
		{
			try
			{
				if (this.placedDynamicObjects.Count > 0 && i <= this.placedDynamicObjects.Count)
				{
					if (CoopPeerStarter.DedicatedHost)
					{
						if (this.placedDynamicObjects[i] && this.placedDynamicObjects[i].gameObject.activeSelf)
						{
							this.placedDynamicObjects[i].gameObject.SetActive(false);
						}
					}
					else if (this.placedDynamicObjects[i] && LocalPlayer.Transform)
					{
						if ((LocalPlayer.Transform.position - this.placedDynamicObjects[i].position).sqrMagnitude < 5100f)
						{
							if (!this.placedDynamicObjects[i].gameObject.activeSelf)
							{
								this.placedDynamicObjects[i].gameObject.SetActive(true);
							}
						}
						else if (this.placedDynamicObjects[i].gameObject.activeSelf)
						{
							this.placedDynamicObjects[i].gameObject.SetActive(false);
						}
					}
				}
				if (this.placedDynamicObjectsAllPlayers.Count > 0 && y <= this.placedDynamicObjectsAllPlayers.Count)
				{
					if (CoopPeerStarter.DedicatedHost)
					{
						if (this.placedDynamicObjectsAllPlayers[y] && this.placedDynamicObjectsAllPlayers[y].gameObject.activeSelf)
						{
							this.placedDynamicObjectsAllPlayers[y].gameObject.SetActive(false);
						}
					}
					else if (this.placedDynamicObjectsAllPlayers[y] && LocalPlayer.Transform)
					{
						if (this.GetClosestPlayerDistanceFromPos(this.placedDynamicObjectsAllPlayers[y].position) < 75f)
						{
							if (!this.placedDynamicObjectsAllPlayers[y].gameObject.activeSelf)
							{
								this.placedDynamicObjectsAllPlayers[y].gameObject.SetActive(true);
							}
						}
						else if (this.placedDynamicObjectsAllPlayers[y].gameObject.activeSelf)
						{
							this.placedDynamicObjectsAllPlayers[y].gameObject.SetActive(false);
						}
					}
				}
			}
			catch
			{
			}
			y++;
			i++;
			if (i > this.placedDynamicObjects.Count)
			{
				i = 0;
			}
			if (y > this.placedDynamicObjectsAllPlayers.Count)
			{
				y = 0;
			}
			flip = !flip;
			yield return YieldPresets.WaitForFixedUpdate;
		}
		yield break;
	}

	
	public IEnumerator manageSpawnedLogs()
	{
		int y = 0;
		for (;;)
		{
			try
			{
				if (this.spawnedLogs.Count > 0 && y < this.spawnedLogs.Count && this.spawnedLogs[y] && this.allPlayers.Count > 0 && y > this.maxSpawnedLogs && y > this.maxSpawnedLogs)
				{
					this.removeFurthestLogFromPlayer();
				}
			}
			catch
			{
			}
			y++;
			if (y > this.spawnedLogs.Count)
			{
				y = 0;
			}
			yield return null;
		}
		yield break;
	}

	
	private void removeLog(Transform log)
	{
		if (PoolManager.Pools["PickUps"].IsSpawned(log))
		{
			PoolManager.Pools["PickUps"].Despawn(log);
		}
		else
		{
			UnityEngine.Object.Destroy(log.gameObject);
		}
	}

	
	private void removeFurthestLogFromPlayer()
	{
		float num = 0f;
		Transform transform = null;
		for (int i = 0; i < this.spawnedLogs.Count; i++)
		{
			float closestPlayerDistanceFromPos = this.GetClosestPlayerDistanceFromPos(this.spawnedLogs[i].position);
			if (closestPlayerDistanceFromPos > num)
			{
				num = closestPlayerDistanceFromPos;
				transform = this.spawnedLogs[i];
			}
		}
		if (transform)
		{
			this.removeLog(transform);
		}
	}

	
	private const float MUSIC_PREAMBIENT = 0f;

	
	private const float MUSIC_AMBIENT = 1.5f;

	
	private const float MUSIC_STRESS = 2.5f;

	
	private const float MUSIC_COMBAT = 3.5f;

	
	public lb_BirdController birdController;

	
	public PlayerVis Eye;

	
	public GameObject worldCollision;

	
	public string SurfaceMusicPath;

	
	public string CaveMusicPath;

	
	private GameObject astarGuo;

	
	public PlayMakerArrayListProxy proxyAttackers;

	
	[Range(0f, 100f)]
	[Tooltip("Percentage chance that stress music will play when above ground and enemies are nearby")]
	public float SurfaceStressMusicChance = 50f;

	
	[Range(0f, 100f)]
	[Tooltip("Percentage chance that stress music will play when in a cave and enemies are nearby")]
	public float CaveStressMusicChance = 50f;

	
	public bool validatingClimbingWalls;

	
	public LayerMask climbLayerMask;

	
	public TreeLodGrid treeLodGrid;

	
	public GameObject[] ceilingMarkers;

	
	public GameObject[] groupEncounters;

	
	public GameObject[] singleEncounters;

	
	public GameObject[] crashMarkers;

	
	public GameObject planeCrash;

	
	public GameObject[] playerHangingMarkers;

	
	public GameObject[] timmyCaveMarkers;

	
	public GameObject closeStructureTarget;

	
	public List<Transform> spawnedLogs = new List<Transform>();

	
	public List<Transform> placedDynamicObjects = new List<Transform>();

	
	public List<Transform> placedDynamicObjectsAllPlayers = new List<Transform>();

	
	public List<GameObject> storedRagDollPrefabs = new List<GameObject>();

	
	public List<GameObject> closeTrees = new List<GameObject>();

	
	public List<GameObject> beachMarkers = new List<GameObject>();

	
	public List<GameObject> swimMarkers = new List<GameObject>();

	
	public List<GameObject> caveMarkers = new List<GameObject>();

	
	public List<GameObject> fireFliesMarkers = new List<GameObject>();

	
	public List<GameObject> waypointMarkers = new List<GameObject>();

	
	public List<Transform> drinkMarkers = new List<Transform>();

	
	public List<Transform> dragMarkers = new List<Transform>();

	
	public List<GameObject> visibleEnemies = new List<GameObject>();

	
	public List<GameObject> closeEnemies = new List<GameObject>();

	
	public List<GameObject> allPlayers = new List<GameObject>();

	
	public List<Vector3> allPlayerPositions = new List<Vector3>();

	
	public List<GameObject> allClients = new List<GameObject>();

	
	public List<BoltEntity> allPlayerEntities = new List<BoltEntity>();

	
	public List<Transform> jumpObjects = new List<Transform>();

	
	public List<GameObject> recentlyBuilt = new List<GameObject>();

	
	public List<GameObject> structuresBuilt = new List<GameObject>();

	
	public List<GameObject> climbableStructures = new List<GameObject>();

	
	public List<GameObject> encounters = new List<GameObject>();

	
	public List<GameObject> feedingEncounters = new List<GameObject>();

	
	public List<Transform> caveWayPoints = new List<Transform>();

	
	public List<Transform> builtRockThrowers = new List<Transform>();

	
	public List<GameObject> allAnimals = new List<GameObject>();

	
	public List<GameObject> allRaccoons = new List<GameObject>();

	
	public List<GameObject> allRabbits = new List<GameObject>();

	
	public List<GameObject> allDeer = new List<GameObject>();

	
	public List<GameObject> allSquirrel = new List<GameObject>();

	
	public List<GameObject> allBoar = new List<GameObject>();

	
	public List<GameObject> allRabbitTraps = new List<GameObject>();

	
	public List<GameObject> allTrapTriggers = new List<GameObject>();

	
	public List<GameObject> allPlayerFires = new List<GameObject>();

	
	public List<GameObject> allPlayersInCave = new List<GameObject>();

	
	public List<GameObject> allVisTargets = new List<GameObject>();

	
	public List<mutantAiManager> aiManagers = new List<mutantAiManager>();

	
	public List<GameObject> clientBirds = new List<GameObject>();

	
	public GameObject EndgameBoss;

	
	public Vector3 playerVisDir;

	
	public static bool NearEnemies;

	
	public static bool AlertedEnemies;

	
	public int currLizardAmount;

	
	public int currTurtleAmount;

	
	public int currRabbitAmount;

	
	public int currTortoiseAmount;

	
	public int currRaccoonAmount;

	
	public int currDeerAmount;

	
	public int maxLizardAmount;

	
	public int maxTurtleAmount;

	
	public int maxRabbitAmount;

	
	public int maxSharkAmount;

	
	public int maxTortoiseAmount;

	
	public int maxRaccoonAmount;

	
	public int maxDeerAmount;

	
	private int initLizardAmount;

	
	private int initTurtleAmount;

	
	private int initRabbitAmount;

	
	private int initTortoiseAmount;

	
	private int initRaccoonAmount;

	
	private int initDeerAmount;

	
	private int initMaxLizards;

	
	private int initMaxRabbits;

	
	private int initMaxTortoise;

	
	private int initMaxRaccoon;

	
	private int initMaxDeer;

	
	public bool hasSearchedFire;

	
	public bool hasSearchedTree;

	
	public bool hasAttackedPlayer;

	
	public bool hasSwarmedByEnemies;

	
	public int recentlyKilledEnemyCount;

	
	private float killedEnemyTimer;

	
	private float swarmedByEnemiesReset;

	
	public int allAttackers;

	
	public int maxAttackers;

	
	public int allEnemies;

	
	public bool endBossSpawned;

	
	private int maxSpawnedLogs;

	
	public float taaMotionClamp;

	
	private EventInstance SurfaceMusic;

	
	private EventInstance CaveMusic;

	
	private EventInstance ActiveMusic;

	
	private ParameterInstance TransitionParameter;

	
	private bool MusicEnabled = true;

	
	private bool CombatMusicEnabled = true;

	
	public bool graphsBeingUpdated;

	
	public Color regularMutantColor = new Color(1f, 1f, 1f, 1f);

	
	public Color paleMutantColor = new Color(0.8039216f, 0.870588243f, 0.9137255f, 1f);

	
	public Color poisonedColor = new Color(0.670588255f, 0.796078444f, 0.5529412f, 1f);

	
	private sceneTracker.EnemyPresence CurrentEnemyPresence;

	
	public List<GameObject> currentNavStructures = new List<GameObject>();

	
	public bool doingNavUpdate;

	
	public List<GameObject> globalNavStructures = new List<GameObject>();

	
	public bool doingGlobalNavUpdate;

	
	public bool waitForLoadSequence;

	
	public List<GameObject> dummyNavStructures = new List<GameObject>();

	
	public List<Bounds> dummyNavBounds = new List<Bounds>();

	
	private bool doingDummyNavUpdate;

	
	public List<NavmeshCut> worldNavCuts = new List<NavmeshCut>();

	
	public class EnemyInSightEvent : UnityEvent<GameObject>
	{
	}

	
	private enum EnemyPresence
	{
		
		None,
		
		Nearby,
		
		Watching,
		
		Attacking
	}
}
