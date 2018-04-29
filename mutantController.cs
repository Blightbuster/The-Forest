using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using PathologicalGames;
using TheForest.Items.Inventory;
using TheForest.Modding.Bridge.Interfaces;
using TheForest.Tools;
using TheForest.Utils;
using UnityEngine;


public class mutantController : MonoBehaviour, IMutantController
{
	
	private void OnDeserialized()
	{
		Clock.planecrash = false;
		this.startDelay = false;
	}

	
	private void Awake()
	{
		this.RefreshMaxActiveMutants(null);
		EventRegistry.Game.Subscribe(TfEvent.AllowEnemiesSet, new EventRegistry.SubscriberCallback(this.RefreshMaxActiveMutants));
	}

	
	private void OnDestroy()
	{
		EventRegistry.Game.Unsubscribe(TfEvent.AllowEnemiesSet, new EventRegistry.SubscriberCallback(this.RefreshMaxActiveMutants));
		this.initMutantSpawnerCallback = null;
	}

	
	private void RefreshMaxActiveMutants(object o = null)
	{
		this.currentMaxActiveMutants = ((!Cheats.NoEnemies) ? this.maxActiveMutants : 0);
		if (GameSetup.IsCreativeGame)
		{
			base.StartCoroutine(this.restartEnemiesFromPauseMenu());
		}
	}

	
	private IEnumerator restartEnemiesFromPauseMenu()
	{
		if (Scene.HudGui.Loading._cam.activeSelf)
		{
			yield break;
		}
		if (this.doingRestartEnemies)
		{
			yield break;
		}
		while (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Pause)
		{
			this.doingRestartEnemies = true;
			yield return null;
		}
		yield return YieldPresets.WaitPointFiveSeconds;
		if (this.currentMaxActiveMutants > 0)
		{
			this.startSetupFamilies();
		}
		else
		{
			base.StartCoroutine(this.removeAllEnemies());
		}
		this.doingRestartEnemies = false;
		yield break;
	}

	
	private void Start()
	{
		base.Invoke("doStart", 2f);
		base.Invoke("doFirstTimeSetup", 120f);
	}

	
	private void doStart()
	{
		this.allCaveSpawnsGo = GameObject.FindGameObjectsWithTag("mutantSpawn");
		this.allPaleSpawnsGo = GameObject.FindGameObjectsWithTag("mutantPaleSpawn");
		this.allSpawnPointsGo = GameObject.FindGameObjectsWithTag("entrance");
		this.allRegularSpawns.Clear();
		this.allPaintedSpawns.Clear();
		this.allSkinnedSpawns.Clear();
		this.allPaleSpawns.Clear();
		for (int i = 0; i < this.allCaveSpawnsGo.Length; i++)
		{
			this.allCaveSpawns.Add(this.allCaveSpawnsGo[i]);
		}
		for (int j = 0; j < this.allPaleSpawnsGo.Length; j++)
		{
			this.allPaleSpawns.Add(this.allPaleSpawnsGo[j]);
		}
		for (int k = 0; k < this.allSpawnPointsGo.Length; k++)
		{
			this.allSpawnPoints.Add(this.allSpawnPointsGo[k]);
		}
		if (Clock.Day == 0)
		{
			if (!this.skipInitialDelay)
			{
				this.startDelay = false;
				if (Clock.Dark)
				{
					base.Invoke("disableStartDelay", 100f);
				}
				else
				{
					base.Invoke("disableStartDelay", 270f);
				}
			}
			else if (!Clock.planecrash)
			{
				this.startSetupFamilies();
				if (this.hordeModeActive && this.hordeModeAuto)
				{
					if (this.hordeConstantSpawning)
					{
						base.InvokeRepeating("doConstantHordeSpawning", 1f, 20f);
					}
					else
					{
						base.InvokeRepeating("doHordeSpawning", 12f, 3f);
					}
				}
			}
		}
		else if (!Clock.planecrash)
		{
			this.startSetupFamilies();
		}
	}

	
	private void Update()
	{
		if (this.overrideClockDay)
		{
			Clock.Day = this.setDay;
		}
	}

	
	private void doFirstTimeSetup()
	{
		if (!this.firstTimeSetup)
		{
			base.StartCoroutine("setupFamilies");
		}
	}

	
	private void enableDoDespawn()
	{
	}

	
	private void disableStartDelay()
	{
		this.startDelay = false;
		this.startSetupFamilies();
	}

	
	private IEnumerator despawnGo(GameObject go)
	{
		yield return YieldPresets.WaitOneSecond;
		if (BoltNetwork.isClient)
		{
			yield break;
		}
		bool despawn = true;
		while (despawn)
		{
			bool test = true;
			if (test)
			{
				if (PoolManager.Pools["enemies"].IsSpawned(go.transform))
				{
					PoolManager.Pools["enemies"].Despawn(go.transform);
				}
				this.activeCannibals.Remove(go);
				despawn = false;
				yield return null;
			}
			else if (Vector3.Distance(LocalPlayer.Transform.position, go.transform.position) > 70f)
			{
				if (PoolManager.Pools["enemies"].IsSpawned(go.transform))
				{
					PoolManager.Pools["enemies"].Despawn(go.transform);
				}
				this.activeCannibals.Remove(go);
				despawn = false;
				yield return null;
			}
			else if (this.pointOffCamera(go.transform.position))
			{
				if (PoolManager.Pools["enemies"].IsSpawned(go.transform))
				{
					PoolManager.Pools["enemies"].Despawn(go.transform);
				}
				this.activeCannibals.Remove(go);
				despawn = false;
				yield return null;
			}
			yield return YieldPresets.WaitOneSecond;
		}
		yield return null;
		yield break;
	}

	
	public void startSetupFamilies()
	{
		if (CoopPeerStarter.DedicatedHost)
		{
			LocalPlayer.Transform.position = Scene.PlaneCrash.transform.position;
		}
		if (this.hordeModeActive)
		{
			return;
		}
		Scene.ActiveMB.StartCoroutine(this.setupFamilies());
	}

	
	public IEnumerator setupFamilies()
	{
		if (this.hordeModeActive)
		{
			yield break;
		}
		if (LocalPlayer.IsInCaves)
		{
			yield return YieldPresets.WaitThreeSeconds;
		}
		Scene.ActiveMB.StopCoroutine(this.updateSpawns());
		if (BoltNetwork.isClient)
		{
			yield break;
		}
		this.calculateMainNavArea();
		this.firstTimeSetup = true;
		if (!Clock.planecrash && !this.startDelay && !Cheats.NoEnemies && !this.disableAllEnemies && !this.setupBreak)
		{
			this.setupBreak = true;
			base.Invoke("resetSetupBreak", 1f);
			if (this.activeCannibals.Count > 0)
			{
				foreach (GameObject go in this.activeCannibals)
				{
					if (go)
					{
						Scene.ActiveMB.StartCoroutine(this.despawnGo(go));
					}
				}
			}
			while (this.activeCannibals.Count > 0)
			{
				yield return null;
			}
			foreach (GameObject go2 in this.allWorldSpawns)
			{
				if (go2)
				{
					UnityEngine.Object.Destroy(go2);
				}
			}
			foreach (GameObject go3 in this.allCaveSpawns)
			{
				if (go3)
				{
					spawnMutants spawn = go3.GetComponent<spawnMutants>();
					spawn.enabled = false;
				}
			}
			this.count = 0;
			this.numActiveSpawns = 0;
			this.numActiveCreepySpawns = 0;
			this.numActivePaleSpawns = 0;
			this.numActiveRegularSpawns = 0;
			this.numActivePaintedSpawns = 0;
			this.numActiveSkinnySpawns = 0;
			this.numActiveSkinnedSpawns = 0;
			this.setDayConditions();
			Scene.ActiveMB.StartCoroutine(this.updateSpawns());
		}
		yield break;
	}

	
	public IEnumerator updateSpawns()
	{
		if (BoltNetwork.isClient)
		{
			yield break;
		}
		this.allWorldSpawns.RemoveAll((GameObject o) => o == null);
		this.allCaveSpawns.RemoveAll((GameObject o) => o == null);
		this.allSkinnySpawns.RemoveAll((GameObject o) => o == null);
		this.allSkinnyPaleSpawns.RemoveAll((GameObject o) => o == null);
		this.allRegularSpawns.RemoveAll((GameObject o) => o == null);
		this.allPaintedSpawns.RemoveAll((GameObject o) => o == null);
		this.allPaleSpawns.RemoveAll((GameObject o) => o == null);
		this.allCreepySpawns.RemoveAll((GameObject o) => o == null);
		this.allSkinnedSpawns.RemoveAll((GameObject o) => o == null);
		foreach (GameObject go in this.allWorldSpawns)
		{
			go.SendMessage("updateSpawnConditions", SendMessageOptions.DontRequireReceiver);
		}
		this.setDayConditions();
		if (Clock.Day == 0 && Scene.Atmosphere.TimeOfDay > 270f && Scene.Atmosphere.TimeOfDay < 359f && !LocalPlayer.IsInCaves)
		{
			yield return new WaitForSeconds(0f);
		}
		if (!this.disableAllEnemies && !this.startDelay && !Cheats.NoEnemies)
		{
			this.sortSpawnPointsByDistance();
			this.spawnCounter = 4;
			bool allSpawned = false;
			while (!Scene.SceneTracker.waitForLoadSequence)
			{
				yield return null;
			}
			while (Scene.SceneTracker.doingGlobalNavUpdate)
			{
				yield return null;
			}
			yield return YieldPresets.WaitOneSecond;
			yield return YieldPresets.WaitOneSecond;
			yield return YieldPresets.WaitOneSecond;
			while (this.mostCommonArea.Count == 0)
			{
				yield return null;
			}
			while ((this.activeWorldCannibals.Count < this.currentMaxActiveMutants && !LocalPlayer.IsInCaves && !allSpawned) || (this.count < this.allCaveSpawns.Count && LocalPlayer.IsInCaves))
			{
				if (!LocalPlayer.IsInCaves)
				{
					if (Cheats.NoEnemiesDuringDay && !Clock.Dark)
					{
						yield break;
					}
					Transform spawnPoint = this.allSpawnPoints[this.spawnCounter].transform;
					this.spawnCounter++;
					if (this.spawnCounter >= this.allSpawnPoints.Count)
					{
						this.spawnCounter = this.allSpawnPoints.Count;
					}
					GraphNode node = AstarPath.active.GetNearest(spawnPoint.position, NNConstraint.Default).node;
					int areaCount = 0;
					while (node.Area != this.mostCommonArea[areaCount])
					{
						node = AstarPath.active.GetNearest(this.allSpawnPoints[this.spawnCounter].transform.position, NNConstraint.Default).node;
						areaCount++;
						if (areaCount == this.mostCommonArea.Count)
						{
							areaCount = 0;
						}
						this.spawnCounter++;
						if (this.spawnCounter >= this.allSpawnPoints.Count)
						{
							node.Area = this.mostCommonArea[0];
							this.spawnCounter = this.allSpawnPoints.Count;
						}
						spawnPoint = this.allSpawnPoints[this.spawnCounter].transform;
						yield return null;
					}
					this.lastValidSpawn = spawnPoint;
					GameObject newSpawn = UnityEngine.Object.Instantiate(this.spawnGo, spawnPoint.position, spawnPoint.rotation) as GameObject;
					spawnMutants spawn = newSpawn.GetComponent<spawnMutants>();
					if (this.allCreepySpawns.Count < this.spawnManager.desiredCreepy)
					{
						this.setupCreepySpawn(spawn);
						this.allCreepySpawns.Add(spawn.gameObject);
					}
					else if (this.allSkinnedSpawns.Count < this.spawnManager.desiredSkinned)
					{
						this.setupSkinnedSpawn(spawn);
						this.allSkinnedSpawns.Add(spawn.gameObject);
					}
					else if (this.allPaintedSpawns.Count < this.spawnManager.desiredPainted)
					{
						this.setupRegularPaintedSpawn(spawn);
						this.allPaintedSpawns.Add(spawn.gameObject);
					}
					else if (this.allRegularSpawns.Count < this.spawnManager.desiredRegular)
					{
						this.setupRegularSpawn(spawn);
						this.allRegularSpawns.Add(spawn.gameObject);
					}
					else if (this.allPaleSpawns.Count < this.spawnManager.desiredPale)
					{
						this.setupPaleSpawn(spawn);
						this.allPaleSpawns.Add(spawn.gameObject);
					}
					else if (this.allSkinnyPaleSpawns.Count < this.spawnManager.desiredSkinnyPale)
					{
						this.setupSkinnyPaleSpawn(spawn);
						this.allSkinnyPaleSpawns.Add(spawn.gameObject);
					}
					else if (this.allSkinnySpawns.Count < this.spawnManager.desiredSkinny)
					{
						this.setupSkinnySpawn(spawn);
						this.allSkinnySpawns.Add(spawn.gameObject);
					}
					else
					{
						allSpawned = true;
					}
					spawn.enabled = true;
					spawn.invokeSpawn();
					spawn.addToWorldSpawns();
					this.numActiveCannibals = this.activeWorldCannibals.Count;
					yield return new WaitForFixedUpdate();
				}
				else if (LocalPlayer.IsInCaves)
				{
					this.sortCaveSpawnsByDistance(LocalPlayer.Transform);
					if (this.allCaveSpawns[this.count] != null)
					{
						spawnMutants spawn2 = this.allCaveSpawns[this.count].GetComponent<spawnMutants>();
						if (!spawn2.enabled)
						{
							spawn2.enabled = true;
							spawn2.invokeSpawn();
							if (!this.activeFamilies.Contains(this.allCaveSpawns[this.count]))
							{
								this.activeFamilies.Add(this.allCaveSpawns[this.count]);
							}
							this.numActiveCannibals = this.activeCannibals.Count;
							this.count++;
						}
					}
				}
				this.worldMutantsActive = false;
				yield return YieldPresets.WaitForFixedUpdate;
				yield return YieldPresets.WaitForFixedUpdate;
				yield return YieldPresets.WaitForFixedUpdate;
				foreach (GameObject go2 in this.allWorldSpawns)
				{
					if (go2)
					{
						go2.SendMessage("updateSpawnConditions", SendMessageOptions.DontRequireReceiver);
					}
				}
			}
		}
		this.count = 0;
		yield break;
	}

	
	public IEnumerator updateCaveSpawns()
	{
		if (this.updatingCaves)
		{
			yield break;
		}
		if (BoltNetwork.isClient)
		{
			yield break;
		}
		this.allCaveSpawns.RemoveAll((GameObject o) => o == null);
		if (!this.disableAllEnemies && !this.startDelay && !Cheats.NoEnemies)
		{
			this.updatingCaves = true;
			this.spawnCounter = 5;
			this.count = 0;
			while (this.count < this.allCaveSpawns.Count)
			{
				this.sortCaveSpawnsByDistance(LocalPlayer.Transform);
				if (this.allCaveSpawns[this.count] != null)
				{
					spawnMutants spawn = this.allCaveSpawns[this.count].GetComponent<spawnMutants>();
					if (!spawn.enabled)
					{
						spawn.enabled = true;
						spawn.invokeSpawn();
						if (!this.activeFamilies.Contains(this.allCaveSpawns[this.count]))
						{
							this.activeFamilies.Add(this.allCaveSpawns[this.count]);
						}
						this.count++;
					}
				}
				yield return YieldPresets.WaitForFixedUpdate;
			}
		}
		this.updatingCaves = false;
		this.count = 0;
		yield break;
	}

	
	private void disableWorldMutantsActive()
	{
		this.worldMutantsActive = false;
	}

	
	public void activateNextSpawn()
	{
		base.StartCoroutine("updateSpawns");
	}

	
	public void updateFamilies()
	{
	}

	
	private void setDayConditions()
	{
		Scene.MutantSpawnManager.setMutantSpawnAmounts();
		if (LocalPlayer.IsInCaves)
		{
			this.maxActiveMutants = 20;
		}
		else if (Clock.Day == 0 && !Clock.Dark)
		{
			this.maxActiveMutants = 15;
		}
		else if (Clock.Day == 0 && Clock.Dark)
		{
			this.maxActiveMutants = 16;
		}
		else if (Clock.Day == 1 && !Clock.Dark)
		{
			this.maxActiveMutants = 15;
		}
		else if (Clock.Day == 1 && Clock.Dark)
		{
			this.maxActiveMutants = 16;
		}
		else if (Clock.Day == 2 && !Clock.Dark)
		{
			this.maxActiveMutants = 15;
		}
		else if (Clock.Day == 2 && Clock.Dark)
		{
			this.maxActiveMutants = 16;
		}
		else if (Clock.Day == 3 && !Clock.Dark)
		{
			this.maxActiveMutants = 15;
		}
		else if (Clock.Day == 3 && Clock.Dark)
		{
			this.maxActiveMutants = 18;
		}
		else if (Clock.Day == 4 && !Clock.Dark)
		{
			this.maxActiveMutants = 15;
		}
		else if (Clock.Day == 4 && Clock.Dark)
		{
			this.maxActiveMutants = 18;
		}
		else if (Clock.Day == 5 && !Clock.Dark)
		{
			this.maxActiveMutants = 16;
		}
		else if (Clock.Day == 5 && Clock.Dark)
		{
			this.maxActiveMutants = 18;
		}
		else if (Clock.Day >= 10 && !Clock.Dark)
		{
			this.maxActiveMutants = 18;
		}
		else if (Clock.Day >= 10 && Clock.Dark)
		{
			this.maxActiveMutants = 18;
		}
		this.currentMaxActiveMutants = ((!Cheats.NoEnemies) ? this.maxActiveMutants : 0);
	}

	
	private void sortCaveSpawnsByDistance(Transform target)
	{
		this.allCaveSpawns.RemoveAll((GameObject o) => o == null);
		this.allCaveSpawns.TrimExcess();
		this.allCaveSpawns.Sort((GameObject c1, GameObject c2) => (target.position - c1.transform.position).sqrMagnitude.CompareTo((target.position - c2.transform.position).sqrMagnitude));
	}

	
	private void sortSpawnPointsByDistance()
	{
		this.allSpawnPoints.RemoveAll((GameObject o) => o == null);
		this.allCaveSpawns.RemoveAll((GameObject o) => o == null);
		this.allSpawnPoints.TrimExcess();
		this.allCaveSpawns.TrimExcess();
		this.allSpawnPoints.Sort((GameObject c1, GameObject c2) => Vector3.Distance(LocalPlayer.Transform.position, c1.transform.position).CompareTo(Vector3.Distance(LocalPlayer.Transform.position, c2.transform.position)));
		this.allCaveSpawns.Sort((GameObject c1, GameObject c2) => Vector3.Distance(LocalPlayer.Transform.position, c1.transform.position).CompareTo(Vector3.Distance(LocalPlayer.Transform.position, c2.transform.position)));
	}

	
	public Transform findClosestEnemy(Transform trn)
	{
		float num = float.PositiveInfinity;
		GameObject gameObject = null;
		foreach (GameObject gameObject2 in this.activeCannibals)
		{
			if (gameObject2 != null)
			{
				float magnitude = (gameObject2.transform.position - trn.position).magnitude;
				if (magnitude < num)
				{
					gameObject = gameObject2;
					num = magnitude;
				}
			}
		}
		if (gameObject != null)
		{
			return gameObject.transform;
		}
		return null;
	}

	
	private void resetSetupBreak()
	{
		this.setupBreak = false;
	}

	
	private bool pointOffCamera(Vector3 pos)
	{
		Vector3 vector = LocalPlayer.MainCam.WorldToViewportPoint(pos);
		return vector.x < 0f || vector.x > 1f || vector.y < 0f || vector.y > 1f;
	}

	
	private void setupSkinnySpawn(spawnMutants spawn)
	{
		if (Clock.Day < 6)
		{
			spawn.amount_female_skinny = UnityEngine.Random.Range(0, 2);
		}
		else
		{
			spawn.amount_female_skinny = UnityEngine.Random.Range(0, 3);
		}
		if (spawn.amount_female_skinny == 0)
		{
			spawn.amount_male_skinny = 2;
		}
		else if (Clock.Day < 6)
		{
			spawn.amount_male_skinny = UnityEngine.Random.Range(1, 3);
		}
		else
		{
			spawn.amount_male_skinny = UnityEngine.Random.Range(1, 4);
		}
		if (UnityEngine.Random.value > 0.1f && spawn.amount_male_skinny > 0)
		{
			spawn.leader = true;
		}
		else
		{
			spawn.leader = false;
		}
		this.numActiveSkinnySpawns++;
		this.numActiveSpawns++;
	}

	
	private void setupSkinnyPaleSpawn(spawnMutants spawn)
	{
		if (Clock.Day < 6)
		{
			spawn.amount_skinny_pale = UnityEngine.Random.Range(1, 4);
		}
		else
		{
			spawn.amount_skinny_pale = UnityEngine.Random.Range(2, 6);
		}
		spawn.leader = true;
		this.numActiveSkinnyPaleSpawns++;
		this.numActiveSpawns++;
	}

	
	private void setupRegularSpawn(spawnMutants spawn)
	{
		spawn.amount_male = UnityEngine.Random.Range(1, 4);
		if (Clock.Day > 8)
		{
			spawn.amount_female = UnityEngine.Random.Range(0, 3);
		}
		else
		{
			spawn.amount_female = UnityEngine.Random.Range(0, 2);
		}
		float num = 0.6f;
		if (GameSetup.IsHardMode)
		{
			num = 0.35f;
		}
		if (UnityEngine.Random.value > num && Clock.Day > 6)
		{
			if (GameSetup.IsHardMode)
			{
				spawn.amount_fireman = UnityEngine.Random.Range(0, 3);
			}
			else
			{
				spawn.amount_fireman = UnityEngine.Random.Range(0, 2);
			}
		}
		if (UnityEngine.Random.value < 0.05f)
		{
			spawn.leader = false;
		}
		else
		{
			spawn.leader = true;
		}
		this.numActiveRegularSpawns++;
		this.numActiveSpawns++;
	}

	
	private void setupPaleSpawn(spawnMutants spawn)
	{
		if (Clock.Day < 10)
		{
			spawn.amount_pale = UnityEngine.Random.Range(1, 4);
		}
		else
		{
			spawn.amount_pale = UnityEngine.Random.Range(2, 6);
		}
		if (UnityEngine.Random.value < 0.2f)
		{
			spawn.leader = false;
		}
		else
		{
			spawn.leader = true;
		}
		spawn.pale = true;
		this.numActivePaleSpawns++;
		this.numActiveSpawns++;
	}

	
	private void setupRegularPaintedSpawn(spawnMutants spawn)
	{
		spawn.amount_male = UnityEngine.Random.Range(2, 4);
		spawn.amount_female = UnityEngine.Random.Range(0, 3);
		float num = 0.6f;
		if (GameSetup.IsHardMode)
		{
			num = 0.35f;
		}
		if (UnityEngine.Random.value > num && Clock.Day > 6)
		{
			if (GameSetup.IsHardMode)
			{
				spawn.amount_fireman = UnityEngine.Random.Range(0, 3);
			}
			else
			{
				spawn.amount_fireman = UnityEngine.Random.Range(0, 2);
			}
		}
		spawn.paintedTribe = true;
		spawn.leader = true;
		this.numActivePaintedSpawns++;
		this.numActiveSpawns++;
	}

	
	private void setupSkinnedSpawn(spawnMutants spawn)
	{
		spawn.amount_pale = UnityEngine.Random.Range(1, 4);
		spawn.amount_skinny_pale = UnityEngine.Random.Range(1, 4);
		spawn.paintedTribe = false;
		spawn.skinnedTribe = true;
		spawn.leader = true;
		this.numActiveSkinnedSpawns++;
		this.numActiveSpawns++;
	}

	
	private void setupCreepySpawn(spawnMutants spawn)
	{
		spawn.amount_vags = UnityEngine.Random.Range(0, 2);
		if (spawn.amount_vags == 0)
		{
			spawn.amount_armsy = UnityEngine.Random.Range(0, 3);
		}
		else
		{
			spawn.amount_armsy = UnityEngine.Random.Range(0, 2);
		}
		int num = spawn.amount_vags + spawn.amount_armsy;
		if (Clock.Day > 12 && num < 3)
		{
			spawn.amount_fat = UnityEngine.Random.Range(0, 2);
		}
		if (spawn.amount_armsy == 0 && spawn.amount_fat == 0 && spawn.amount_vags == 0)
		{
			spawn.amount_armsy = 1;
			if (UnityEngine.Random.value > 0.5f)
			{
				spawn.amount_vags = 1;
			}
		}
		if (spawn.amount_vags > 0 && UnityEngine.Random.value > 0.5f)
		{
			spawn.amount_baby = UnityEngine.Random.Range(2, 6);
		}
		else if (UnityEngine.Random.value > 0.5f && (spawn.amount_armsy > 0 || spawn.amount_fat > 0) && num < 3)
		{
			spawn.amount_skinny_pale = UnityEngine.Random.Range(1, 4);
		}
		if (UnityEngine.Random.value > 0.7f && Clock.Day > 12)
		{
			spawn.pale = true;
		}
		spawn.leader = true;
		spawn.creepySpawner = true;
		this.numActiveCreepySpawns++;
		this.numActiveSpawns++;
	}

	
	public void enableMpCaveMutants()
	{
		if (GameSetup.IsPeacefulMode || Cheats.NoEnemies)
		{
			return;
		}
		if (!base.IsInvoking("activateClosestCaveSpawn"))
		{
			base.InvokeRepeating("activateClosestCaveSpawn", 3f, 1f);
		}
	}

	
	public void disableMpCaveMutants()
	{
		base.CancelInvoke("activeClosestCaveSpawn");
		if (Scene.SceneTracker.allPlayersInCave.Count == 0 && this.activeCaveCannibals.Count > 0)
		{
			foreach (GameObject gameObject in this.activeCaveCannibals)
			{
				if (gameObject)
				{
					base.StartCoroutine("despawnGo", gameObject);
				}
			}
		}
		if (this.allCaveSpawns.Count > 0)
		{
			foreach (GameObject gameObject2 in this.allCaveSpawns)
			{
				spawnMutants component = gameObject2.GetComponent<spawnMutants>();
				component.enabled = false;
			}
		}
	}

	
	public void disableWorldMutants()
	{
		if (this.activeWorldCannibals.Count > 0)
		{
			foreach (GameObject gameObject in this.activeWorldCannibals)
			{
				if (gameObject)
				{
					base.StartCoroutine("despawnGo", gameObject);
				}
			}
		}
	}

	
	private void activateClosestCaveSpawn()
	{
		if (this.allCaveSpawns.Count == 0)
		{
			return;
		}
		this.allCaveSpawns.RemoveAll((GameObject o) => o == null);
		foreach (GameObject gameObject in Scene.SceneTracker.allPlayersInCave)
		{
			if (gameObject)
			{
				if (this.allCaveSpawns.Count == 0)
				{
					break;
				}
				this.sortCaveSpawnsByDistance(gameObject.transform);
				for (int i = 0; i < 14; i++)
				{
					if ((this.allCaveSpawns[i].transform.position - gameObject.transform.position).sqrMagnitude < 32400f)
					{
						spawnMutants component = this.allCaveSpawns[i].GetComponent<spawnMutants>();
						if (!component.enabled)
						{
							component.enabled = true;
							component.invokeSpawn();
						}
					}
				}
			}
		}
	}

	
	public void dawnSetupFamilies()
	{
		base.StartCoroutine("doDawnSetupFamilies");
	}

	
	public IEnumerator doDawnSetupFamilies()
	{
		if (BoltNetwork.isClient)
		{
			yield break;
		}
		yield return YieldPresets.WaitForFixedUpdate;
		if (this.activeCannibals.Count > 0)
		{
			foreach (GameObject go in this.activeCannibals)
			{
				if (go && LocalPlayer.Transform && Vector3.Distance(go.transform.position, LocalPlayer.Transform.position) > 75f)
				{
					Scene.ActiveMB.StartCoroutine(this.despawnGo(go));
				}
			}
		}
		yield return YieldPresets.WaitOneSecond;
		this.activateNextSpawn();
		yield break;
	}

	
	public IEnumerator removeAllEnemies()
	{
		if (BoltNetwork.isClient)
		{
			yield break;
		}
		yield return YieldPresets.WaitForFixedUpdate;
		if (this.activeCannibals.Count > 0)
		{
			foreach (GameObject go in this.activeCannibals)
			{
				if (go)
				{
					Scene.ActiveMB.StartCoroutine(this.despawnGo(go));
				}
			}
		}
		if (this.activeBabies.Count > 0)
		{
			foreach (GameObject go2 in this.activeBabies)
			{
				if (go2)
				{
					Scene.ActiveMB.StartCoroutine(this.despawnGo(go2));
				}
			}
		}
		while (this.activeCannibals.Count > 0)
		{
			yield return null;
		}
		foreach (GameObject go3 in this.allWorldSpawns)
		{
			if (go3)
			{
				UnityEngine.Object.Destroy(go3);
			}
		}
		foreach (GameObject go4 in this.allCaveSpawns)
		{
			if (go4)
			{
				spawnMutants spawn = go4.GetComponent<spawnMutants>();
				spawn.enabled = false;
			}
		}
		this.count = 0;
		this.numActiveSpawns = 0;
		this.numActiveCreepySpawns = 0;
		this.numActivePaleSpawns = 0;
		this.numActiveRegularSpawns = 0;
		this.numActivePaintedSpawns = 0;
		this.numActiveSkinnySpawns = 0;
		yield break;
	}

	
	public IEnumerator removeWorldMutants()
	{
		if (BoltNetwork.isClient)
		{
			yield break;
		}
		yield return YieldPresets.WaitForFixedUpdate;
		if (this.activeWorldCannibals.Count > 0)
		{
			foreach (GameObject go in this.activeWorldCannibals)
			{
				if (go)
				{
					Scene.ActiveMB.StartCoroutine(this.despawnGo(go));
				}
			}
		}
		while (this.activeWorldCannibals.Count > 0)
		{
			yield return null;
		}
		foreach (GameObject go2 in this.allWorldSpawns)
		{
			if (go2)
			{
				UnityEngine.Object.Destroy(go2);
			}
		}
		this.count = 0;
		this.numActiveSpawns = 0;
		this.numActiveCreepySpawns = 0;
		this.numActivePaleSpawns = 0;
		this.numActiveRegularSpawns = 0;
		this.numActivePaintedSpawns = 0;
		this.numActiveSkinnySpawns = 0;
		yield break;
	}

	
	public IEnumerator removeCaveMutants()
	{
		if (BoltNetwork.isClient)
		{
			yield break;
		}
		yield return YieldPresets.WaitForFixedUpdate;
		if (this.activeCaveCannibals.Count > 0)
		{
			foreach (GameObject go in this.activeCaveCannibals)
			{
				if (go)
				{
					Scene.ActiveMB.StartCoroutine(this.despawnGo(go));
				}
			}
		}
		this.activeCaveCannibals.RemoveAll((GameObject o) => o == null);
		while (this.activeCaveCannibals.Count > 0)
		{
			yield return null;
		}
		foreach (GameObject go2 in this.allCaveSpawns)
		{
			if (go2)
			{
				spawnMutants spawn = go2.GetComponent<spawnMutants>();
				spawn.enabled = false;
			}
		}
		yield break;
	}

	
	public void doHordeSpawning()
	{
		if (this.activeWorldCannibals.Count == 0 && !this.doneNewHordeWave && !this.hordeModePaused)
		{
			this.doNextHordeWave();
			this.doneNewHordeWave = true;
		}
		if (this.activeWorldCannibals.Count > 0)
		{
			this.doneNewHordeWave = false;
		}
	}

	
	private void doConstantHordeSpawning()
	{
		base.StartCoroutine("addToHordeSpawn");
	}

	
	public void doNextHordeWave()
	{
		this.hordeLevel++;
		if (this.hordeLevel == 1)
		{
			base.StartCoroutine("updateHordeSpawns", this.startHordeSpawnDelay);
		}
		else
		{
			base.StartCoroutine("updateHordeSpawns", this.nextWaveSpawnDelay);
		}
	}

	
	public IEnumerator updateHordeSpawns(int delay)
	{
		yield return new WaitForSeconds((float)delay);
		if (BoltNetwork.isClient)
		{
			yield break;
		}
		this.allWorldSpawns.RemoveAll((GameObject o) => o == null);
		this.allCaveSpawns.RemoveAll((GameObject o) => o == null);
		this.currentMaxActiveMutants = 30;
		if (!this.startDelay)
		{
			this.sortSpawnPointsByDistance();
			this.spawnCounter = this.spawnCounterMin;
			bool allSpawned = false;
			while (!allSpawned)
			{
				Transform spawnPoint = this.allSpawnPoints[this.spawnCounter].transform;
				this.spawnCounter++;
				if (this.spawnCounter >= this.allSpawnPoints.Count)
				{
					this.spawnCounter = this.spawnCounterMin;
				}
				GameObject newSpawn = UnityEngine.Object.Instantiate(this.spawnGo, spawnPoint.position, spawnPoint.rotation) as GameObject;
				spawnMutants spawn = newSpawn.GetComponent<spawnMutants>();
				if (this.initMutantSpawnerCallback != null)
				{
					if (this.initMutantSpawnerCallback(spawn))
					{
						allSpawned = true;
					}
				}
				else if (this.numActiveCreepySpawns < this.spawnManager.desiredCreepy)
				{
					this.setupCreepySpawn(spawn);
				}
				else if (this.numActiveRegularSpawns < this.spawnManager.desiredRegular)
				{
					this.setupRegularSpawn(spawn);
				}
				else if (this.numActiveSkinnedSpawns < this.spawnManager.desiredSkinned)
				{
					this.setupSkinnedSpawn(spawn);
				}
				else if (this.numActivePaintedSpawns < this.spawnManager.desiredPainted)
				{
					this.setupRegularPaintedSpawn(spawn);
				}
				else if (this.numActivePaleSpawns < this.spawnManager.desiredPale)
				{
					this.setupPaleSpawn(spawn);
				}
				else if (this.numActiveSkinnyPaleSpawns < this.spawnManager.desiredSkinnyPale)
				{
					this.setupSkinnyPaleSpawn(spawn);
				}
				else if (this.numActiveSkinnySpawns < this.spawnManager.desiredSkinny)
				{
					this.setupSkinnySpawn(spawn);
				}
				else
				{
					allSpawned = true;
				}
				spawn.enabled = true;
				spawn.invokeSpawn();
				spawn.addToWorldSpawns();
				this.numActiveCannibals = this.activeWorldCannibals.Count;
				this.worldMutantsActive = false;
				yield return YieldPresets.WaitPointTwoFiveSeconds;
				if (this.activeWorldCannibals.Count >= this.currentMaxActiveMutants)
				{
					allSpawned = true;
				}
			}
			foreach (GameObject go in this.allWorldSpawns)
			{
				if (go)
				{
					go.SendMessage("updateSpawnConditions", SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		this.count = 0;
		yield break;
	}

	
	public IEnumerator addToHordeSpawn()
	{
		if (BoltNetwork.isClient)
		{
			yield break;
		}
		this.allWorldSpawns.RemoveAll((GameObject o) => o == null);
		this.allCaveSpawns.RemoveAll((GameObject o) => o == null);
		this.currentMaxActiveMutants = 25;
		this.sortSpawnPointsByDistance();
		this.spawnCounter = this.spawnCounterMin;
		if (this.allWorldSpawns.Count < 6 || this.activeWorldCannibals.Count < 8)
		{
			Transform spawnPoint = this.allSpawnPoints[this.spawnCounter].transform;
			this.spawnCounter++;
			if (this.spawnCounter >= 6)
			{
				this.spawnCounter = this.spawnCounterMin;
			}
			GameObject newSpawn = UnityEngine.Object.Instantiate(this.spawnGo, spawnPoint.position, spawnPoint.rotation) as GameObject;
			spawnMutants spawn = newSpawn.GetComponent<spawnMutants>();
			if (this.initMutantSpawnerCallback != null)
			{
				if (this.initMutantSpawnerCallback(spawn))
				{
				}
			}
			else
			{
				int rand = UnityEngine.Random.Range(0, 15);
				if (rand < 3)
				{
					this.setupSkinnySpawn(spawn);
				}
				else if (rand > 3 && rand < 5)
				{
					this.setupSkinnyPaleSpawn(spawn);
				}
				else if (rand > 5 && rand < 9)
				{
					this.setupRegularSpawn(spawn);
				}
				else if (rand > 9 && rand < 12)
				{
					this.setupPaleSpawn(spawn);
				}
				else if (rand > 12 && rand < 14)
				{
					this.setupCreepySpawn(spawn);
				}
			}
			spawn.enabled = true;
			spawn.invokeSpawn();
			spawn.addToWorldSpawns();
			this.numActiveCannibals = this.activeWorldCannibals.Count;
			this.worldMutantsActive = false;
			yield return YieldPresets.WaitPointTwoFiveSeconds;
			if (spawn)
			{
				spawn.SendMessage("updateSpawnConditions", SendMessageOptions.DontRequireReceiver);
			}
		}
		this.count = 0;
		yield break;
	}

	
	public void calculateMainNavArea()
	{
		navAreaSetup[] componentsInChildren = this.navRef.GetComponentsInChildren<navAreaSetup>();
		int num = 0;
		if (!this.navRef.gameObject.activeInHierarchy)
		{
			return;
		}
		foreach (navAreaSetup navAreaSetup in componentsInChildren)
		{
			if (navAreaSetup.areaNum > num)
			{
				num = navAreaSetup.areaNum;
			}
		}
		this.mostCommonArea.Clear();
		if (this.rg == null)
		{
			this.rg = AstarPath.active.astarData.recastGraph;
		}
		for (int j = 0; j <= num; j++)
		{
			List<uint> list = new List<uint>();
			foreach (navAreaSetup navAreaSetup2 in componentsInChildren)
			{
				if (navAreaSetup2.areaNum == j)
				{
					GraphNode node = this.rg.GetNearest(navAreaSetup2.gameObject.transform.position, NNConstraint.Default).node;
					if (node != null)
					{
						list.Add(node.Area);
					}
				}
			}
			if (list.Count != 0)
			{
				Dictionary<uint, int> dictionary = new Dictionary<uint, int>();
				this.mostCommonArea.Add(list[0]);
				dictionary.Add(list[0], 1);
				for (int l = 1; l < list.Count; l++)
				{
					if (dictionary.ContainsKey(list[l]))
					{
						Dictionary<uint, int> dictionary3;
						Dictionary<uint, int> dictionary2 = dictionary3 = dictionary;
						uint key2;
						uint key = key2 = list[l];
						int num2 = dictionary3[key2];
						dictionary2[key] = num2 + 1;
						if (dictionary[list[l]] > dictionary[this.mostCommonArea[j]])
						{
							this.mostCommonArea[j] = list[l];
						}
					}
					else
					{
						dictionary.Add(list[l], 1);
					}
				}
			}
		}
	}

	
	
	public List<GameObject> AllSkinnySpawns
	{
		get
		{
			return this.allSkinnySpawns;
		}
	}

	
	
	public List<GameObject> AllSkinnyPaleSpawns
	{
		get
		{
			return this.allSkinnyPaleSpawns;
		}
	}

	
	
	public List<GameObject> AllRegularSpawns
	{
		get
		{
			return this.allRegularSpawns;
		}
	}

	
	
	public List<GameObject> AllPaintedSpawns
	{
		get
		{
			return this.allPaintedSpawns;
		}
	}

	
	
	public List<GameObject> AllPaleSpawns
	{
		get
		{
			return this.allPaleSpawns;
		}
	}

	
	
	public List<GameObject> AllCreepySpawns
	{
		get
		{
			return this.allCreepySpawns;
		}
	}

	
	
	public List<GameObject> AllSkinnedSpawns
	{
		get
		{
			return this.allSkinnedSpawns;
		}
	}

	
	
	public List<GameObject> AllWorldSpawns
	{
		get
		{
			return this.allWorldSpawns;
		}
	}

	
	
	public List<GameObject> AllCaveSpawns
	{
		get
		{
			return this.allCaveSpawns;
		}
	}

	
	
	public List<GameObject> AllSleepingSpawns
	{
		get
		{
			return this.allSleepingSpawns;
		}
	}

	
	
	public List<GameObject> ActiveCannibals
	{
		get
		{
			return this.activeCannibals;
		}
	}

	
	
	public List<GameObject> ActiveCaveCannibals
	{
		get
		{
			return this.activeCaveCannibals;
		}
	}

	
	
	public List<GameObject> ActiveWorldCannibals
	{
		get
		{
			return this.activeWorldCannibals;
		}
	}

	
	
	public List<GameObject> ActiveSkinnyCannibals
	{
		get
		{
			return this.activeSkinnyCannibals;
		}
	}

	
	
	public List<GameObject> AllSpawnPoints
	{
		get
		{
			return this.allSpawnPoints;
		}
	}

	
	
	public List<GameObject> ActiveInstantSpawnedCannibals
	{
		get
		{
			return this.activeInstantSpawnedCannibals;
		}
	}

	
	
	public List<GameObject> ActiveBabies
	{
		get
		{
			return this.activeBabies;
		}
	}

	
	
	public List<GameObject> ActiveNetCannibals
	{
		get
		{
			return this.activeNetCannibals;
		}
	}

	
	public void WaveSpawn(bool onoff, bool auto, bool constantSpawn = false, int level = 0, int startDelay = 60, int waveDelay = 30)
	{
		this.hordeModeActive = onoff;
		this.hordeModeAuto = auto;
		this.hordeConstantSpawning = constantSpawn;
		this.hordeLevel = level;
		this.startHordeSpawnDelay = startDelay;
		this.nextWaveSpawnDelay = waveDelay;
	}

	
	public void WaveSpawnPause(bool onoff)
	{
		this.hordeModePaused = onoff;
	}

	
	public void SendWave()
	{
		this.doNextHordeWave();
		this.doneNewHordeWave = true;
	}

	
	public void SetSpawnPoints(List<GameObject> spawnPoints, int skippedClosestPoints)
	{
		if (spawnPoints != null && spawnPoints.Count > 0)
		{
			this.spawnCounterMin = skippedClosestPoints;
			this.allSpawnPoints = spawnPoints;
		}
	}

	
	public void SetInitSpawnerCallback(Func<IMutantSpawner, bool> callback)
	{
		this.initMutantSpawnerCallback = callback;
	}

	
	public mutantSpawnManager spawnManager;

	
	public GameObject spawnGo;

	
	public Transform divider;

	
	public Transform navRef;

	
	public bool overrideClockDay;

	
	public int setDay;

	
	public int maxActiveMutants;

	
	public int maxActiveSpawners;

	
	public int numActiveSkinnySpawns;

	
	public int numActiveSkinnyPaleSpawns;

	
	public int numActiveRegularSpawns;

	
	public int numActivePaintedSpawns;

	
	public int numActivePaleSpawns;

	
	public int numActiveCreepySpawns;

	
	public int numActiveSkinnedSpawns;

	
	public int numActiveSpawns;

	
	public int deadFamilies;

	
	public int numActiveCannibals;

	
	private GameObject[] allFamilySpawnsGo;

	
	private GameObject[] allPaleSpawnsGo;

	
	private GameObject[] allCreepySpawnsGo;

	
	private GameObject[] allSkinnedSpawnsGo;

	
	private GameObject[] allSpawnPointsGo;

	
	private GameObject[] allCaveSpawnsGo;

	
	public List<GameObject> activeFamilies = new List<GameObject>();

	
	public List<GameObject> allSkinnySpawns = new List<GameObject>();

	
	public List<GameObject> allSkinnyPaleSpawns = new List<GameObject>();

	
	public List<GameObject> allRegularSpawns = new List<GameObject>();

	
	public List<GameObject> allPaintedSpawns = new List<GameObject>();

	
	public List<GameObject> allPaleSpawns = new List<GameObject>();

	
	public List<GameObject> allCreepySpawns = new List<GameObject>();

	
	public List<GameObject> allSkinnedSpawns = new List<GameObject>();

	
	public List<GameObject> allWorldSpawns = new List<GameObject>();

	
	public List<GameObject> allCaveSpawns = new List<GameObject>();

	
	public List<GameObject> allSleepingSpawns = new List<GameObject>();

	
	public List<GameObject> activeCannibals = new List<GameObject>();

	
	public List<GameObject> activeCaveCannibals = new List<GameObject>();

	
	public List<GameObject> activeWorldCannibals = new List<GameObject>();

	
	public List<GameObject> activeSkinnyCannibals = new List<GameObject>();

	
	public List<GameObject> allSpawnPoints = new List<GameObject>();

	
	public List<GameObject> activeInstantSpawnedCannibals = new List<GameObject>();

	
	public List<GameObject> activeBabies = new List<GameObject>();

	
	public List<GameObject> activeNetCannibals = new List<GameObject>();

	
	private Transform lastValidSpawn;

	
	public int currentMaxActiveMutants;

	
	private int count;

	
	private int spawnCounterMin;

	
	private int spawnCounter;

	
	private bool setupBreak;

	
	private bool startDelay;

	
	private bool firstTimeSetup;

	
	private bool worldMutantsActive;

	
	public bool skipInitialDelay;

	
	public bool disableAllEnemies;

	
	private RecastGraph rg;

	
	public bool hordeModeActive;

	
	public bool hordeModeAuto;

	
	public bool hordeModePaused;

	
	public bool hordeConstantSpawning;

	
	public int hordeLevel;

	
	public int startHordeSpawnDelay = 60;

	
	public int nextWaveSpawnDelay = 30;

	
	private Func<IMutantSpawner, bool> initMutantSpawnerCallback;

	
	private bool doingRestartEnemies;

	
	private bool updatingCaves;

	
	private bool doneNewHordeWave;

	
	public List<uint> mostCommonArea = new List<uint>();
}
