using System;
using System.Collections;
using System.Collections.Generic;
using PathologicalGames;
using TheForest.Modding.Bridge.Interfaces;
using TheForest.Utils;
using UnityEngine;


public class spawnMutants : MonoBehaviour, IMutantSpawner
{
	
	private void Awake()
	{
		if (BoltNetwork.isClient)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		this.tr = base.transform;
		this.sceneInfo = Scene.SceneTracker;
		this.mutantControl = Scene.MutantControler;
	}

	
	private void Start()
	{
		if (GameSetup.IsPeacefulMode || Cheats.NoEnemies)
		{
			return;
		}
		if (this.bossBabySpawn)
		{
			base.StartCoroutine(this.doSpawn());
			return;
		}
		if (Scene.LoadSave)
		{
			LoadSave.OnGameStart += delegate
			{
				if (this.instantSpawn || this.sinkholeSpawn)
				{
					base.InvokeRepeating("checkSpawn", 4f, 4f);
				}
			};
		}
		else if (this.bossBabySpawn)
		{
			this.checkSpawn();
		}
		else if (this.instantSpawn || this.sinkholeSpawn)
		{
			base.InvokeRepeating("checkSpawn", 4f, 4f);
		}
	}

	
	private void OnDisable()
	{
		this.alreadySpawned = false;
		base.CancelInvoke("checkSpawn");
		base.CancelInvoke("updateSpawnConditions");
	}

	
	private void OnDestroy()
	{
		base.StopAllCoroutines();
		base.CancelInvoke("checkSpawn");
		base.CancelInvoke("updateSpawnConditions");
	}

	
	public void invokeSpawn()
	{
		this.updateSpawnConditions();
		base.InvokeRepeating("checkSpawn", 0f, 3f);
	}

	
	public void updateSpawnConditions()
	{
		int num = this.amount_male_skinny + this.amount_female_skinny + this.amount_skinny_pale + this.amount_male + this.amount_female + this.amount_fireman + this.amount_pale + this.amount_armsy + this.amount_vags + this.amount_baby + this.amount_fat;
		if (num < 1)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		if (!LocalPlayer.IsInCaves)
		{
			this.mutantControl.allSleepingSpawns.RemoveAll((GameObject o) => o == null);
			if (this.mutantControl.allSleepingSpawns.Count < this.mutantControl.allWorldSpawns.Count / 2 && !Clock.Dark && this.mutantControl.allSleepingSpawns.Count < Scene.MutantSpawnManager.maxSleepingSpawns && Clock.Day < 7)
			{
				this.sleepingSpawn = true;
				if (!this.mutantControl.allSleepingSpawns.Contains(base.gameObject))
				{
					this.mutantControl.allSleepingSpawns.Add(base.gameObject);
				}
			}
			else
			{
				this.sleepingSpawn = false;
				if (this.mutantControl.allSleepingSpawns.Contains(base.gameObject))
				{
					this.mutantControl.allSleepingSpawns.Remove(base.gameObject);
				}
			}
		}
		foreach (GameObject gameObject in this.allMembers)
		{
			if (gameObject)
			{
				gameObject.SendMessage("initWakeUp", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	
	public void addToWorldSpawns()
	{
		if (!this.instantSpawn)
		{
			this.mutantControl.allWorldSpawns.Add(base.gameObject);
		}
	}

	
	private void disableDuringDay()
	{
		foreach (GameObject gameObject in this.allMembers)
		{
			if (!this.spawnInCave && !Clock.Dark)
			{
				if (gameObject)
				{
					gameObject.SetActive(false);
				}
				else
				{
					if (gameObject)
					{
						gameObject.SetActive(true);
					}
					base.CancelInvoke("disableDuringDay");
				}
			}
		}
	}

	
	private void checkSpawn()
	{
		if (!base.enabled)
		{
			return;
		}
		if (Scene.SceneTracker.allPlayers.Count == 0)
		{
			return;
		}
		this.allPlayers = new List<GameObject>(Scene.SceneTracker.allPlayers);
		this.allPlayers.RemoveAll((GameObject o) => o == null);
		if (this.allPlayers[0] == null)
		{
			return;
		}
		if (this.allPlayers.Count > 1)
		{
			this.allPlayers.Sort((GameObject c1, GameObject c2) => Vector3.Distance(base.transform.position, c1.transform.position).CompareTo(Vector3.Distance(base.transform.position, c2.transform.position)));
		}
		if (Clock.Day < this.daysTillSpawn)
		{
			return;
		}
		if (!this)
		{
			return;
		}
		this.checkPlayerDist = Vector3.Distance(this.allPlayers[0].transform.position, base.transform.position);
		if (this.spawnInCave && Scene.SceneTracker.allPlayersInCave.Count > 0 && this.checkPlayerDist < 130f && !this.alreadySpawned)
		{
			base.StartCoroutine("doSpawn");
			this.alreadySpawned = true;
		}
		else if (this.sinkholeSpawn && this.checkPlayerDist < 200f && !this.alreadySpawned)
		{
			base.StartCoroutine("doSpawn");
			this.alreadySpawned = true;
		}
		else if (!LocalPlayer.IsInCaves && !this.spawnInCave && !this.sinkholeSpawn)
		{
			base.StartCoroutine("doSpawn");
			base.CancelInvoke("checkSpawn");
		}
		float num = 160f;
		if (this.sinkholeSpawn)
		{
			num = 225f;
		}
		if ((this.spawnInCave || this.sinkholeSpawn) && this.checkPlayerDist > num && this.alreadySpawned)
		{
			bool flag = false;
			if (Scene.SceneTracker.allPlayersInCave.Count > 0 && this.spawnInCave)
			{
				flag = true;
			}
			if (this.sinkholeSpawn)
			{
				flag = true;
			}
			if (flag)
			{
				bool flag2 = false;
				foreach (GameObject gameObject in this.allMembers)
				{
					if (gameObject && gameObject.activeSelf && Vector3.Distance(gameObject.transform.position, this.allPlayers[0].transform.position) < 160f)
					{
						flag2 = true;
					}
				}
				if (!flag2)
				{
					base.StartCoroutine("despawnAll");
					this.alreadySpawned = false;
				}
			}
		}
	}

	
	public void clearSpawn()
	{
		this.allMembers.Clear();
	}

	
	private IEnumerator despawnAll()
	{
		foreach (GameObject gameObject in this.allMembers)
		{
			if (gameObject)
			{
				PoolManager.Pools["enemies"].Despawn(gameObject.transform);
			}
		}
		this.allMembers.Clear();
		yield return null;
		yield break;
	}

	
	private IEnumerator doSpawn()
	{
		this.allMembers.Clear();
		this.doLeader = true;
		base.StartCoroutine(this.spawnRegularMale());
		base.StartCoroutine(this.spawnRegularFemale());
		base.StartCoroutine(this.spawnRegularFireman());
		base.StartCoroutine(this.spawnArmsy());
		base.StartCoroutine(this.spawnVags());
		base.StartCoroutine(this.spawnFat());
		base.StartCoroutine(this.spawnBaby());
		base.StartCoroutine(this.spawnLargePale());
		base.StartCoroutine(this.spawnMaleSkinny());
		base.StartCoroutine(this.spawnFemaleSkinny());
		base.StartCoroutine(this.spawnPaleSkinny());
		yield return null;
		yield break;
	}

	
	private IEnumerator spawnRegularMale()
	{
		int count = 0;
		bool doName = false;
		while (count < this.amount_male)
		{
			Vector2 vector = this.Circle2(UnityEngine.Random.Range(2f, this.range));
			Vector3 vector2 = new Vector3(vector.x + base.transform.position.x, base.transform.position.y, vector.y + base.transform.position.z);
			this.clone = PoolManager.Pools["enemies"].Spawn(this.mutant.transform, vector2, Quaternion.identity);
			if (!doName)
			{
				this.clone.gameObject.name = this.clone.gameObject.name + this.nameCount;
				doName = true;
				this.nameCount++;
			}
			base.StartCoroutine(this.fixMutantPosition(this.clone, vector2));
			this.clone.GetChild(0).eulerAngles = new Vector3(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
			this.clone.SendMessage("setInstantSpawn", this.instantSpawn);
			if (this.leader && this.doLeader)
			{
				this.leaderGo = this.clone.gameObject;
				this.leaderGo.SendMessage("setLeader", this.paintedTribe);
				this.leaderGo.SendMessage("setInCave", this.spawnInCave);
				if (this.sinkholeSpawn)
				{
					this.leaderGo.SendMessage("setInCave", true);
				}
				this.leaderGo.SendMessage("addToSpawn", base.gameObject);
				this.leaderGo.SendMessage("eatingOnSpawn", this.eatingOnSpawn);
				this.doLeader = false;
			}
			else
			{
				if (this.leaderGo)
				{
					this.clone.SendMessage("setFollower", this.leaderGo);
				}
				this.clone.SendMessage("setInCave", this.spawnInCave);
				if (this.sinkholeSpawn)
				{
					this.clone.SendMessage("setInCave", true);
				}
				this.clone.SendMessage("setDefault", this.paintedTribe);
				this.clone.SendMessage("addToSpawn", base.gameObject);
				this.clone.SendMessage("eatingOnSpawn", this.eatingOnSpawn);
			}
			count++;
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator spawnRegularFemale()
	{
		int count = 0;
		bool doName = false;
		while (count < this.amount_female)
		{
			Vector2 vector = this.Circle2(UnityEngine.Random.Range(2f, this.range));
			Vector3 vector2 = new Vector3(vector.x + base.transform.position.x, base.transform.position.y, vector.y + base.transform.position.z);
			Transform transform = PoolManager.Pools["enemies"].Spawn(this.mutant_female.transform, vector2, Quaternion.identity);
			if (!doName)
			{
				transform.gameObject.name = transform.gameObject.name + this.nameCount;
				doName = true;
				this.nameCount++;
			}
			base.StartCoroutine(this.fixMutantPosition(transform, vector2));
			if (this.leader && this.doLeader)
			{
				this.leaderGo = transform.gameObject;
				transform.SendMessage("setFemale");
				this.leaderGo.SendMessage("setLeader", this.paintedTribe);
				this.leaderGo.SendMessage("setInCave", this.spawnInCave);
				if (this.sinkholeSpawn)
				{
					this.leaderGo.SendMessage("setInCave", true);
				}
				this.leaderGo.SendMessage("addToSpawn", base.gameObject);
				this.leaderGo.SendMessage("eatingOnSpawn", this.eatingOnSpawn);
				this.doLeader = false;
			}
			else
			{
				transform.transform.GetChild(0).eulerAngles = new Vector3(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
				transform.SendMessage("setInstantSpawn", this.instantSpawn);
				if (this.leaderGo)
				{
					transform.SendMessage("setFollower", this.leaderGo);
				}
				transform.SendMessage("setInCave", this.spawnInCave);
				if (this.sinkholeSpawn)
				{
					transform.SendMessage("setInCave", true);
				}
				transform.SendMessage("setFemale", this.paintedTribe);
				transform.SendMessage("addToSpawn", base.gameObject);
				transform.SendMessage("eatingOnSpawn", this.eatingOnSpawn);
			}
			count++;
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator spawnRegularFireman()
	{
		int count = 0;
		bool doName = false;
		while (count < this.amount_fireman)
		{
			Vector2 vector = this.Circle2(UnityEngine.Random.Range(2f, this.range));
			Vector3 vector2 = new Vector3(vector.x + base.transform.position.x, base.transform.position.y, vector.y + base.transform.position.z);
			Transform transform = PoolManager.Pools["enemies"].Spawn(this.mutant.transform, vector2, Quaternion.identity);
			if (!doName)
			{
				transform.gameObject.name = transform.gameObject.name + this.nameCount;
				this.nameCount++;
			}
			base.StartCoroutine(this.fixMutantPosition(transform, vector2));
			transform.transform.GetChild(0).eulerAngles = new Vector3(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
			transform.SendMessage("setInstantSpawn", this.instantSpawn);
			if (this.leaderGo)
			{
				transform.SendMessage("setFollower", this.leaderGo);
			}
			transform.SendMessage("forceDynamiteMan", this.useDynamiteMan);
			transform.SendMessage("setFireman", this.paintedTribe);
			transform.SendMessage("setInCave", this.spawnInCave);
			if (this.sinkholeSpawn)
			{
				transform.SendMessage("setInCave", true);
			}
			transform.SendMessage("addToSpawn", base.gameObject);
			transform.SendMessage("eatingOnSpawn", this.eatingOnSpawn);
			count++;
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator spawnLargePale()
	{
		int count = 0;
		bool doName = false;
		while (count < this.amount_pale)
		{
			Vector3 vector = new Vector3(0f, 0f, 0f);
			Vector2 vector2 = this.Circle2(UnityEngine.Random.Range(2f, this.range));
			vector = new Vector3(vector2.x + base.transform.position.x, base.transform.position.y, vector2.y + base.transform.position.z);
			Transform transform = PoolManager.Pools["enemies"].Spawn(this.mutant.transform, vector, Quaternion.identity);
			if (!this.paleOnCeiling)
			{
				transform.transform.GetChild(0).eulerAngles = new Vector3(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
			}
			if (!this.PaleOnCeiling)
			{
				base.StartCoroutine(this.fixMutantPosition(transform, vector));
			}
			if (!doName)
			{
				transform.gameObject.name = transform.gameObject.name + this.nameCount;
				this.nameCount++;
				doName = true;
			}
			transform.SendMessage("setInstantSpawn", this.instantSpawn);
			if (this.leader && this.doLeader)
			{
				this.leaderGo = transform.gameObject;
				this.leaderGo.SendMessage("setSkinnedTribe", this.skinnedTribe);
				this.leaderGo.SendMessage("setPaleLeader");
				this.leaderGo.SendMessage("setInCave", this.spawnInCave);
				if (this.sinkholeSpawn)
				{
					this.leaderGo.SendMessage("setInCave", true);
				}
				this.leaderGo.SendMessage("setPaleOnCeiling", this.paleOnCeiling);
				this.leaderGo.SendMessage("addToSpawn", base.gameObject);
				this.leaderGo.SendMessage("setPatrolling", this.patrolling);
				this.leaderGo.SendMessage("setWakeUpInCave", this.wakeUpOnSpawn);
				this.leaderGo.SendMessage("eatingOnSpawn", this.eatingOnSpawn);
				this.doLeader = false;
			}
			else
			{
				if (this.leaderGo)
				{
					transform.SendMessage("setFollower", this.leaderGo);
				}
				transform.SendMessage("setSkinnedTribe", this.skinnedTribe);
				transform.SendMessage("setPale");
				transform.SendMessage("setInCave", this.spawnInCave);
				if (this.sinkholeSpawn)
				{
					transform.SendMessage("setInCave", true);
				}
				transform.SendMessage("setPaleOnCeiling", this.paleOnCeiling);
				transform.SendMessage("setPatrolling", this.patrolling);
				transform.SendMessage("setWakeUpInCave", this.wakeUpOnSpawn);
				transform.SendMessage("addToSpawn", base.gameObject);
				transform.SendMessage("eatingOnSpawn", this.eatingOnSpawn);
			}
			count++;
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator spawnArmsy()
	{
		int count = 0;
		bool doName = false;
		while (count < this.amount_armsy)
		{
			Vector2 vector = this.Circle2(UnityEngine.Random.Range(2f, this.range));
			Vector3 vector2 = new Vector3(vector.x + base.transform.position.x, base.transform.position.y, vector.y + base.transform.position.z);
			Quaternion rot = Quaternion.identity;
			if (this.alignRotationToSpawn)
			{
				rot = base.transform.rotation;
			}
			Transform transform = PoolManager.Pools["enemies"].Spawn(this.armsy.transform, vector2, rot);
			if (!doName)
			{
				transform.gameObject.name = transform.gameObject.name + this.nameCount;
				doName = true;
				this.nameCount++;
			}
			base.StartCoroutine(this.fixMutantPosition(transform, vector2));
			if (this.leader && this.doLeader && this.creepySpawner)
			{
				this.leaderGo = transform.gameObject;
				this.doLeader = false;
			}
			else if (this.leaderGo)
			{
				transform.SendMessage("setFollower", this.leaderGo);
			}
			transform.SendMessage("setOldMutant", this.oldMutant);
			transform.SendMessage("setCreepyPale", this.pale);
			transform.SendMessage("setInstantSpawn", this.instantSpawn);
			transform.SendMessage("setArmsy", base.gameObject);
			transform.SendMessage("setInCave", this.spawnInCave);
			if (this.sinkholeSpawn)
			{
				transform.SendMessage("setInCave", true);
			}
			transform.SendMessage("setSleeping", this.sleepingSpawn);
			transform.SendMessage("addToSpawn", base.gameObject);
			count++;
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator spawnVags()
	{
		int count = 0;
		bool doName = false;
		while (count < this.amount_vags)
		{
			Vector2 vector = this.Circle2(UnityEngine.Random.Range(2f, this.range));
			Vector3 vector2 = new Vector3(vector.x + base.transform.position.x, base.transform.position.y, vector.y + base.transform.position.z);
			Quaternion rot = Quaternion.identity;
			if (this.alignRotationToSpawn)
			{
				rot = base.transform.rotation;
			}
			Transform transform = PoolManager.Pools["enemies"].Spawn(this.vags.transform, vector2, rot);
			if (!doName)
			{
				transform.gameObject.name = transform.gameObject.name + this.nameCount;
				doName = true;
				this.nameCount++;
			}
			base.StartCoroutine(this.fixMutantPosition(transform, vector2));
			if (this.leader && this.doLeader && this.creepySpawner)
			{
				this.leaderGo = transform.gameObject;
				this.doLeader = false;
			}
			else if (this.leaderGo)
			{
				transform.SendMessage("setFollower", this.leaderGo);
			}
			transform.SendMessage("setCreepyPale", this.pale);
			transform.SendMessage("setInstantSpawn", this.instantSpawn);
			transform.SendMessage("setVags", base.gameObject);
			transform.SendMessage("setInCave", this.spawnInCave);
			if (this.sinkholeSpawn)
			{
				transform.SendMessage("setInCave", true);
			}
			transform.SendMessage("setSleeping", this.sleepingSpawn);
			transform.SendMessage("addToSpawn", base.gameObject);
			count++;
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator spawnFat()
	{
		int count = 0;
		bool doName = false;
		while (count < this.amount_fat)
		{
			Vector2 vector;
			if (this.range < 2f)
			{
				vector = this.Circle2(this.range);
			}
			else
			{
				vector = this.Circle2(UnityEngine.Random.Range(2f, this.range));
			}
			Vector3 vector2 = new Vector3(vector.x + base.transform.position.x, base.transform.position.y, vector.y + base.transform.position.z);
			Quaternion rot = Quaternion.identity;
			if (this.alignRotationToSpawn)
			{
				rot = base.transform.rotation;
			}
			Transform transform = PoolManager.Pools["enemies"].Spawn(this.fat.transform, vector2, rot);
			if (!doName)
			{
				transform.gameObject.name = transform.gameObject.name + this.nameCount;
				doName = true;
				this.nameCount++;
			}
			base.StartCoroutine(this.fixMutantPosition(transform, vector2));
			if (this.leader && this.doLeader && this.creepySpawner)
			{
				this.leaderGo = transform.gameObject;
				this.doLeader = false;
			}
			else if (this.leaderGo)
			{
				transform.SendMessage("setFollower", this.leaderGo);
			}
			transform.SendMessage("setInstantSpawn", this.instantSpawn);
			transform.SendMessage("setFatCreepy", base.gameObject);
			transform.SendMessage("setInCave", this.spawnInCave);
			if (this.sinkholeSpawn)
			{
				transform.SendMessage("setInCave", true);
			}
			transform.SendMessage("setSleeping", this.sleepingSpawn);
			transform.SendMessage("addToSpawn", base.gameObject);
			count++;
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator spawnBaby()
	{
		int count = 0;
		bool doName = false;
		while (count < this.amount_baby)
		{
			Vector2 vector = this.Circle2(UnityEngine.Random.Range(2f, this.range));
			Vector3 vector2 = new Vector3(vector.x + base.transform.position.x, base.transform.position.y, vector.y + base.transform.position.z);
			Quaternion rot = Quaternion.identity;
			if (this.alignRotationToSpawn)
			{
				rot = base.transform.rotation;
			}
			Transform transform = PoolManager.Pools["enemies"].Spawn(this.baby.transform, vector2, rot);
			if (!doName)
			{
				transform.gameObject.name = transform.gameObject.name + this.nameCount;
				doName = true;
				this.nameCount++;
			}
			base.StartCoroutine(this.fixMutantPosition(transform, vector2));
			if (this.leaderGo)
			{
				transform.SendMessage("setFollower", this.leaderGo);
			}
			transform.SendMessage("setInstantSpawn", this.instantSpawn);
			transform.SendMessage("setBossBaby", this.bossBabySpawn);
			transform.SendMessage("setOldMutant", this.oldMutant);
			transform.SendMessage("setBaby", base.gameObject);
			transform.SendMessage("setInCave", this.spawnInCave);
			if (this.sinkholeSpawn)
			{
				transform.SendMessage("setInCave", true);
			}
			transform.SendMessage("setSleeping", this.sleepingSpawn);
			transform.SendMessage("addToSpawn", base.gameObject);
			count++;
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator spawnMaleSkinny()
	{
		int count = 0;
		bool doName = false;
		while (count < this.amount_male_skinny)
		{
			Vector2 vector = this.Circle2(UnityEngine.Random.Range(2f, this.range));
			Vector3 vector2 = new Vector3(vector.x + base.transform.position.x, base.transform.position.y, vector.y + base.transform.position.z);
			Transform transform = PoolManager.Pools["enemies"].Spawn(this.mutant.transform, vector2, Quaternion.identity);
			if (!doName)
			{
				transform.gameObject.name = transform.gameObject.name + this.nameCount;
				this.nameCount++;
				doName = true;
			}
			base.StartCoroutine(this.fixMutantPosition(transform, vector2));
			transform.SendMessage("setInstantSpawn", this.instantSpawn);
			if (this.leader && this.doLeader)
			{
				this.leaderGo = transform.gameObject;
				this.leaderGo.SendMessage("setSkinnyLeader");
				this.leaderGo.SendMessage("setMaleSkinny");
				this.leaderGo.SendMessage("setInCave", this.spawnInCave);
				if (this.sinkholeSpawn)
				{
					this.leaderGo.SendMessage("setInCave", true);
				}
				this.leaderGo.SendMessage("addToSpawn", base.gameObject);
				this.leaderGo.SendMessage("eatingOnSpawn", this.eatingOnSpawn);
				this.doLeader = false;
			}
			else
			{
				if (this.leaderGo)
				{
					transform.SendMessage("setFollower", this.leaderGo);
				}
				transform.SendMessage("setMaleSkinny");
				transform.SendMessage("setInCave", this.spawnInCave);
				if (this.sinkholeSpawn)
				{
					transform.SendMessage("setInCave", true);
				}
				transform.SendMessage("addToSpawn", base.gameObject);
				transform.SendMessage("eatingOnSpawn", this.eatingOnSpawn);
			}
			count++;
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator spawnFemaleSkinny()
	{
		int count = 0;
		bool doName = false;
		while (count < this.amount_female_skinny)
		{
			Vector2 vector = this.Circle2(UnityEngine.Random.Range(2f, this.range));
			Vector3 vector2 = new Vector3(vector.x + base.transform.position.x, base.transform.position.y, vector.y + base.transform.position.z);
			Transform transform = PoolManager.Pools["enemies"].Spawn(this.mutant_female.transform, vector2, Quaternion.identity);
			if (!doName)
			{
				transform.gameObject.name = transform.gameObject.name + this.nameCount;
				this.nameCount++;
				doName = true;
			}
			base.StartCoroutine(this.fixMutantPosition(transform, vector2));
			transform.SendMessage("setInstantSpawn", this.instantSpawn);
			transform.SendMessage("setFemaleSkinny");
			if (this.leaderGo)
			{
				transform.SendMessage("setFollower", this.leaderGo);
			}
			transform.SendMessage("setInCave", this.spawnInCave);
			if (this.sinkholeSpawn)
			{
				transform.SendMessage("setInCave", true);
			}
			transform.SendMessage("addToSpawn", base.gameObject);
			transform.SendMessage("eatingOnSpawn", this.eatingOnSpawn);
			count++;
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator spawnPaleSkinny()
	{
		int count = 0;
		bool doName = false;
		if (Cheats.NoEnemies)
		{
			yield break;
		}
		while (count < this.amount_skinny_pale)
		{
			Vector3 vector = new Vector3(0f, 0f, 0f);
			Vector2 vector2 = this.Circle2(UnityEngine.Random.Range(2f, this.range));
			vector = new Vector3(vector2.x + base.transform.position.x, base.transform.position.y, vector2.y + base.transform.position.z);
			Transform transform = PoolManager.Pools["enemies"].Spawn(this.mutant.transform, vector, Quaternion.identity);
			if (!this.PaleOnCeiling)
			{
				base.StartCoroutine(this.fixMutantPosition(transform, vector));
			}
			if (!this.paleOnCeiling)
			{
				transform.transform.GetChild(0).eulerAngles = new Vector3(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
			}
			if (!doName)
			{
				transform.gameObject.name = transform.gameObject.name + this.nameCount;
				this.nameCount++;
				doName = true;
			}
			transform.SendMessage("setInstantSpawn", this.instantSpawn);
			if (this.leader && this.doLeader && !this.creepySpawner)
			{
				this.leaderGo = transform.gameObject;
				this.leaderGo.SendMessage("setSkinnedTribe", this.skinnedTribe);
				this.leaderGo.SendMessage("setPaleLeader");
				this.leaderGo.SendMessage("setMaleSkinny");
				this.leaderGo.SendMessage("setInCave", this.spawnInCave);
				if (this.sinkholeSpawn)
				{
					this.leaderGo.SendMessage("setInCave", true);
				}
				this.leaderGo.SendMessage("setPaleOnCeiling", this.paleOnCeiling);
				this.leaderGo.SendMessage("addToSpawn", base.gameObject);
				this.leaderGo.SendMessage("setPatrolling", this.patrolling);
				this.leaderGo.SendMessage("setWakeUpInCave", this.wakeUpOnSpawn);
				this.leaderGo.SendMessage("eatingOnSpawn", this.eatingOnSpawn);
				this.doLeader = false;
			}
			else
			{
				if (this.leaderGo)
				{
					transform.SendMessage("setFollower", this.leaderGo);
				}
				transform.SendMessage("setSkinnedTribe", this.skinnedTribe);
				transform.SendMessage("setPale");
				transform.SendMessage("setMaleSkinny");
				transform.SendMessage("setInCave", this.spawnInCave);
				if (this.sinkholeSpawn)
				{
					transform.SendMessage("setInCave", true);
				}
				transform.SendMessage("setPaleOnCeiling", this.paleOnCeiling);
				transform.SendMessage("setPatrolling", this.patrolling);
				transform.SendMessage("setWakeUpInCave", this.wakeUpOnSpawn);
				transform.SendMessage("addToSpawn", base.gameObject);
				transform.SendMessage("eatingOnSpawn", this.eatingOnSpawn);
			}
			count++;
		}
		yield return null;
		yield break;
	}

	
	public Vector2 Circle2(float radius)
	{
		Vector2 insideUnitCircle = UnityEngine.Random.insideUnitCircle;
		insideUnitCircle.Normalize();
		return insideUnitCircle * radius;
	}

	
	private IEnumerator fixMutantPosition(Transform m, Vector3 newPos)
	{
		float t = 0f;
		while (t < 1f)
		{
			if (m)
			{
				m.position = newPos;
				m.SendMessage("updateWorldTransformPosition", SendMessageOptions.DontRequireReceiver);
			}
			t += Time.deltaTime;
			yield return null;
		}
		yield break;
	}

	
	public GameObject findClosestCeilingPos()
	{
		GameObject gameObject = null;
		float num = float.PositiveInfinity;
		foreach (GameObject gameObject2 in this.sceneInfo.ceilingMarkers)
		{
			float magnitude = (this.tr.position - gameObject2.transform.position).magnitude;
			if (magnitude < num && !gameObject2.CompareTag("ceilingOccupied"))
			{
				num = magnitude;
				gameObject = gameObject2;
			}
		}
		if (gameObject)
		{
			gameObject.tag = "ceilingOccupied";
		}
		return gameObject;
	}

	
	
	
	public int AmountMale_skinny
	{
		get
		{
			return this.amount_male_skinny;
		}
		set
		{
			this.amount_male_skinny = value;
		}
	}

	
	
	
	public int AmountFemale_skinny
	{
		get
		{
			return this.amount_female_skinny;
		}
		set
		{
			this.amount_female_skinny = value;
		}
	}

	
	
	
	public int AmountSkinny_pale
	{
		get
		{
			return this.amount_skinny_pale;
		}
		set
		{
			this.amount_skinny_pale = value;
		}
	}

	
	
	
	public int AmountMale
	{
		get
		{
			return this.amount_male;
		}
		set
		{
			this.amount_male = value;
		}
	}

	
	
	
	public int AmountFemale
	{
		get
		{
			return this.amount_female;
		}
		set
		{
			this.amount_female = value;
		}
	}

	
	
	
	public int AmountFireman
	{
		get
		{
			return this.amount_fireman;
		}
		set
		{
			this.amount_fireman = value;
		}
	}

	
	
	
	public int AmountPale
	{
		get
		{
			return this.amount_pale;
		}
		set
		{
			this.amount_pale = value;
		}
	}

	
	
	
	public int AmountArmsy
	{
		get
		{
			return this.amount_armsy;
		}
		set
		{
			this.amount_armsy = value;
		}
	}

	
	
	
	public int AmountVags
	{
		get
		{
			return this.amount_vags;
		}
		set
		{
			this.amount_vags = value;
		}
	}

	
	
	
	public int AmountBaby
	{
		get
		{
			return this.amount_baby;
		}
		set
		{
			this.amount_baby = value;
		}
	}

	
	
	
	public int AmountFat
	{
		get
		{
			return this.amount_fat;
		}
		set
		{
			this.amount_fat = value;
		}
	}

	
	
	
	public bool Leader
	{
		get
		{
			return this.leader;
		}
		set
		{
			this.leader = value;
		}
	}

	
	
	
	public bool Pale
	{
		get
		{
			return this.pale;
		}
		set
		{
			this.pale = value;
		}
	}

	
	
	
	public bool PaleOnCeiling
	{
		get
		{
			return this.paleOnCeiling;
		}
		set
		{
			this.paleOnCeiling = value;
		}
	}

	
	
	
	public bool Patrolling
	{
		get
		{
			return this.patrolling;
		}
		set
		{
			this.patrolling = value;
		}
	}

	
	
	
	public bool WakeUpOnSpawn
	{
		get
		{
			return this.wakeUpOnSpawn;
		}
		set
		{
			this.wakeUpOnSpawn = value;
		}
	}

	
	
	
	public bool EatingOnSpawn
	{
		get
		{
			return this.eatingOnSpawn;
		}
		set
		{
			this.eatingOnSpawn = value;
		}
	}

	
	
	
	public bool OldMutant
	{
		get
		{
			return this.oldMutant;
		}
		set
		{
			this.oldMutant = value;
		}
	}

	
	
	
	public bool PaintedTribe
	{
		get
		{
			return this.paintedTribe;
		}
		set
		{
			this.paintedTribe = value;
		}
	}

	
	
	
	public bool SkinnedTribe
	{
		get
		{
			return this.skinnedTribe;
		}
		set
		{
			this.skinnedTribe = value;
		}
	}

	
	private List<GameObject> allPlayers = new List<GameObject>();

	
	public float checkPlayerDist;

	
	public int amount_male_skinny;

	
	public int amount_female_skinny;

	
	public int amount_skinny_pale;

	
	public int amount_male;

	
	public int amount_female;

	
	public int amount_fireman;

	
	public bool useDynamiteMan;

	
	public int amount_pale;

	
	public int amount_armsy;

	
	public int amount_vags;

	
	public int amount_baby;

	
	public int amount_fat;

	
	public float range;

	
	public int daysTillSpawn;

	
	public int daysTillActiveDuringDay;

	
	public bool leader;

	
	public bool pale;

	
	public bool paleOnCeiling;

	
	public bool patrolling;

	
	public bool wakeUpOnSpawn;

	
	public bool eatingOnSpawn;

	
	public bool oldMutant;

	
	public bool paintedTribe;

	
	public bool skinnedTribe;

	
	public GameObject mutant;

	
	public GameObject mutant_female;

	
	public GameObject mutant_pale;

	
	public GameObject armsy;

	
	public GameObject vags;

	
	public GameObject baby;

	
	public GameObject fat;

	
	public bool sleepingSpawn;

	
	public bool spawnInCave;

	
	public bool sinkholeSpawn;

	
	public bool alignRotationToSpawn;

	
	public bool creepySpawner;

	
	public bool instantSpawn;

	
	public bool bossBabySpawn;

	
	public bool debugAddToLists;

	
	private bool alreadySpawned;

	
	private bool doLeader;

	
	public List<GameObject> allMembers = new List<GameObject>();

	
	private Transform clone;

	
	private GameObject leaderGo;

	
	private sceneTracker sceneInfo;

	
	private mutantController mutantControl;

	
	private Transform tr;

	
	private int nameCount;
}
