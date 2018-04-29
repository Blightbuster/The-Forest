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
		foreach (GameObject go in this.allMembers)
		{
			if (go)
			{
				PoolManager.Pools["enemies"].Despawn(go.transform);
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
			Vector2 pos = this.Circle2(UnityEngine.Random.Range(2f, this.range));
			Vector3 spawnPos = new Vector3(pos.x + base.transform.position.x, base.transform.position.y, pos.y + base.transform.position.z);
			this.clone = PoolManager.Pools["enemies"].Spawn(this.mutant.transform, spawnPos, Quaternion.identity);
			if (!doName)
			{
				this.clone.gameObject.name = this.clone.gameObject.name + this.nameCount;
				doName = true;
				this.nameCount++;
			}
			base.StartCoroutine(this.fixMutantPosition(this.clone, spawnPos));
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
			Transform clonef = null;
			Vector2 pos = this.Circle2(UnityEngine.Random.Range(2f, this.range));
			Vector3 spawnPos = new Vector3(pos.x + base.transform.position.x, base.transform.position.y, pos.y + base.transform.position.z);
			clonef = PoolManager.Pools["enemies"].Spawn(this.mutant_female.transform, spawnPos, Quaternion.identity);
			if (!doName)
			{
				clonef.gameObject.name = clonef.gameObject.name + this.nameCount;
				doName = true;
				this.nameCount++;
			}
			base.StartCoroutine(this.fixMutantPosition(clonef, spawnPos));
			if (this.leader && this.doLeader)
			{
				this.leaderGo = clonef.gameObject;
				clonef.SendMessage("setFemale");
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
				clonef.transform.GetChild(0).eulerAngles = new Vector3(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
				clonef.SendMessage("setInstantSpawn", this.instantSpawn);
				if (this.leaderGo)
				{
					clonef.SendMessage("setFollower", this.leaderGo);
				}
				clonef.SendMessage("setInCave", this.spawnInCave);
				if (this.sinkholeSpawn)
				{
					clonef.SendMessage("setInCave", true);
				}
				clonef.SendMessage("setFemale", this.paintedTribe);
				clonef.SendMessage("addToSpawn", base.gameObject);
				clonef.SendMessage("eatingOnSpawn", this.eatingOnSpawn);
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
			Transform cloneFM = null;
			Vector2 pos = this.Circle2(UnityEngine.Random.Range(2f, this.range));
			Vector3 spawnPos = new Vector3(pos.x + base.transform.position.x, base.transform.position.y, pos.y + base.transform.position.z);
			cloneFM = PoolManager.Pools["enemies"].Spawn(this.mutant.transform, spawnPos, Quaternion.identity);
			if (!doName)
			{
				cloneFM.gameObject.name = cloneFM.gameObject.name + this.nameCount;
				this.nameCount++;
			}
			base.StartCoroutine(this.fixMutantPosition(cloneFM, spawnPos));
			cloneFM.transform.GetChild(0).eulerAngles = new Vector3(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
			cloneFM.SendMessage("setInstantSpawn", this.instantSpawn);
			if (this.leaderGo)
			{
				cloneFM.SendMessage("setFollower", this.leaderGo);
			}
			cloneFM.SendMessage("forceDynamiteMan", this.useDynamiteMan);
			cloneFM.SendMessage("setFireman", this.paintedTribe);
			cloneFM.SendMessage("setInCave", this.spawnInCave);
			if (this.sinkholeSpawn)
			{
				cloneFM.SendMessage("setInCave", true);
			}
			cloneFM.SendMessage("addToSpawn", base.gameObject);
			cloneFM.SendMessage("eatingOnSpawn", this.eatingOnSpawn);
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
			Transform cloneP = null;
			Vector3 spawnPos = new Vector3(0f, 0f, 0f);
			Vector2 pos = this.Circle2(UnityEngine.Random.Range(2f, this.range));
			spawnPos = new Vector3(pos.x + base.transform.position.x, base.transform.position.y, pos.y + base.transform.position.z);
			cloneP = PoolManager.Pools["enemies"].Spawn(this.mutant.transform, spawnPos, Quaternion.identity);
			if (!this.paleOnCeiling)
			{
				cloneP.transform.GetChild(0).eulerAngles = new Vector3(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
			}
			if (!this.PaleOnCeiling)
			{
				base.StartCoroutine(this.fixMutantPosition(cloneP, spawnPos));
			}
			if (!doName)
			{
				cloneP.gameObject.name = cloneP.gameObject.name + this.nameCount;
				this.nameCount++;
				doName = true;
			}
			cloneP.SendMessage("setInstantSpawn", this.instantSpawn);
			if (this.leader && this.doLeader)
			{
				this.leaderGo = cloneP.gameObject;
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
					cloneP.SendMessage("setFollower", this.leaderGo);
				}
				cloneP.SendMessage("setSkinnedTribe", this.skinnedTribe);
				cloneP.SendMessage("setPale");
				cloneP.SendMessage("setInCave", this.spawnInCave);
				if (this.sinkholeSpawn)
				{
					cloneP.SendMessage("setInCave", true);
				}
				cloneP.SendMessage("setPaleOnCeiling", this.paleOnCeiling);
				cloneP.SendMessage("setPatrolling", this.patrolling);
				cloneP.SendMessage("setWakeUpInCave", this.wakeUpOnSpawn);
				cloneP.SendMessage("addToSpawn", base.gameObject);
				cloneP.SendMessage("eatingOnSpawn", this.eatingOnSpawn);
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
			Transform cloneARM = null;
			Vector2 pos = this.Circle2(UnityEngine.Random.Range(2f, this.range));
			Vector3 spawnPos = new Vector3(pos.x + base.transform.position.x, base.transform.position.y, pos.y + base.transform.position.z);
			Quaternion spawnRot = Quaternion.identity;
			if (this.alignRotationToSpawn)
			{
				spawnRot = base.transform.rotation;
			}
			cloneARM = PoolManager.Pools["enemies"].Spawn(this.armsy.transform, spawnPos, spawnRot);
			if (!doName)
			{
				cloneARM.gameObject.name = cloneARM.gameObject.name + this.nameCount;
				doName = true;
				this.nameCount++;
			}
			base.StartCoroutine(this.fixMutantPosition(cloneARM, spawnPos));
			if (this.leader && this.doLeader && this.creepySpawner)
			{
				this.leaderGo = cloneARM.gameObject;
				this.doLeader = false;
			}
			else if (this.leaderGo)
			{
				cloneARM.SendMessage("setFollower", this.leaderGo);
			}
			cloneARM.SendMessage("setOldMutant", this.oldMutant);
			cloneARM.SendMessage("setCreepyPale", this.pale);
			cloneARM.SendMessage("setInstantSpawn", this.instantSpawn);
			cloneARM.SendMessage("setArmsy", base.gameObject);
			cloneARM.SendMessage("setInCave", this.spawnInCave);
			if (this.sinkholeSpawn)
			{
				cloneARM.SendMessage("setInCave", true);
			}
			cloneARM.SendMessage("setSleeping", this.sleepingSpawn);
			cloneARM.SendMessage("addToSpawn", base.gameObject);
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
			Transform cloneVAG = null;
			Vector2 pos = this.Circle2(UnityEngine.Random.Range(2f, this.range));
			Vector3 spawnPos = new Vector3(pos.x + base.transform.position.x, base.transform.position.y, pos.y + base.transform.position.z);
			Quaternion spawnRot = Quaternion.identity;
			if (this.alignRotationToSpawn)
			{
				spawnRot = base.transform.rotation;
			}
			cloneVAG = PoolManager.Pools["enemies"].Spawn(this.vags.transform, spawnPos, spawnRot);
			if (!doName)
			{
				cloneVAG.gameObject.name = cloneVAG.gameObject.name + this.nameCount;
				doName = true;
				this.nameCount++;
			}
			base.StartCoroutine(this.fixMutantPosition(cloneVAG, spawnPos));
			if (this.leader && this.doLeader && this.creepySpawner)
			{
				this.leaderGo = cloneVAG.gameObject;
				this.doLeader = false;
			}
			else if (this.leaderGo)
			{
				cloneVAG.SendMessage("setFollower", this.leaderGo);
			}
			cloneVAG.SendMessage("setCreepyPale", this.pale);
			cloneVAG.SendMessage("setInstantSpawn", this.instantSpawn);
			cloneVAG.SendMessage("setVags", base.gameObject);
			cloneVAG.SendMessage("setInCave", this.spawnInCave);
			if (this.sinkholeSpawn)
			{
				cloneVAG.SendMessage("setInCave", true);
			}
			cloneVAG.SendMessage("setSleeping", this.sleepingSpawn);
			cloneVAG.SendMessage("addToSpawn", base.gameObject);
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
			Transform clone = null;
			Vector2 pos;
			if (this.range < 2f)
			{
				pos = this.Circle2(this.range);
			}
			else
			{
				pos = this.Circle2(UnityEngine.Random.Range(2f, this.range));
			}
			Vector3 spawnPos = new Vector3(pos.x + base.transform.position.x, base.transform.position.y, pos.y + base.transform.position.z);
			Quaternion spawnRot = Quaternion.identity;
			if (this.alignRotationToSpawn)
			{
				spawnRot = base.transform.rotation;
			}
			clone = PoolManager.Pools["enemies"].Spawn(this.fat.transform, spawnPos, spawnRot);
			if (!doName)
			{
				clone.gameObject.name = clone.gameObject.name + this.nameCount;
				doName = true;
				this.nameCount++;
			}
			base.StartCoroutine(this.fixMutantPosition(clone, spawnPos));
			if (this.leader && this.doLeader && this.creepySpawner)
			{
				this.leaderGo = clone.gameObject;
				this.doLeader = false;
			}
			else if (this.leaderGo)
			{
				clone.SendMessage("setFollower", this.leaderGo);
			}
			clone.SendMessage("setInstantSpawn", this.instantSpawn);
			clone.SendMessage("setFatCreepy", base.gameObject);
			clone.SendMessage("setInCave", this.spawnInCave);
			if (this.sinkholeSpawn)
			{
				clone.SendMessage("setInCave", true);
			}
			clone.SendMessage("setSleeping", this.sleepingSpawn);
			clone.SendMessage("addToSpawn", base.gameObject);
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
			Transform cloneBaby = null;
			Vector2 pos = this.Circle2(UnityEngine.Random.Range(2f, this.range));
			Vector3 spawnPos = new Vector3(pos.x + base.transform.position.x, base.transform.position.y, pos.y + base.transform.position.z);
			Quaternion spawnRot = Quaternion.identity;
			if (this.alignRotationToSpawn)
			{
				spawnRot = base.transform.rotation;
			}
			cloneBaby = PoolManager.Pools["enemies"].Spawn(this.baby.transform, spawnPos, spawnRot);
			if (!doName)
			{
				cloneBaby.gameObject.name = cloneBaby.gameObject.name + this.nameCount;
				doName = true;
				this.nameCount++;
			}
			base.StartCoroutine(this.fixMutantPosition(cloneBaby, spawnPos));
			if (this.leaderGo)
			{
				cloneBaby.SendMessage("setFollower", this.leaderGo);
			}
			cloneBaby.SendMessage("setInstantSpawn", this.instantSpawn);
			cloneBaby.SendMessage("setBossBaby", this.bossBabySpawn);
			cloneBaby.SendMessage("setOldMutant", this.oldMutant);
			cloneBaby.SendMessage("setBaby", base.gameObject);
			cloneBaby.SendMessage("setInCave", this.spawnInCave);
			if (this.sinkholeSpawn)
			{
				cloneBaby.SendMessage("setInCave", true);
			}
			cloneBaby.SendMessage("setSleeping", this.sleepingSpawn);
			cloneBaby.SendMessage("addToSpawn", base.gameObject);
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
			Transform cloneMaleSkinny = null;
			Vector2 pos = this.Circle2(UnityEngine.Random.Range(2f, this.range));
			Vector3 spawnPos = new Vector3(pos.x + base.transform.position.x, base.transform.position.y, pos.y + base.transform.position.z);
			cloneMaleSkinny = PoolManager.Pools["enemies"].Spawn(this.mutant.transform, spawnPos, Quaternion.identity);
			if (!doName)
			{
				cloneMaleSkinny.gameObject.name = cloneMaleSkinny.gameObject.name + this.nameCount;
				this.nameCount++;
				doName = true;
			}
			base.StartCoroutine(this.fixMutantPosition(cloneMaleSkinny, spawnPos));
			cloneMaleSkinny.SendMessage("setInstantSpawn", this.instantSpawn);
			if (this.leader && this.doLeader)
			{
				this.leaderGo = cloneMaleSkinny.gameObject;
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
					cloneMaleSkinny.SendMessage("setFollower", this.leaderGo);
				}
				cloneMaleSkinny.SendMessage("setMaleSkinny");
				cloneMaleSkinny.SendMessage("setInCave", this.spawnInCave);
				if (this.sinkholeSpawn)
				{
					cloneMaleSkinny.SendMessage("setInCave", true);
				}
				cloneMaleSkinny.SendMessage("addToSpawn", base.gameObject);
				cloneMaleSkinny.SendMessage("eatingOnSpawn", this.eatingOnSpawn);
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
			Transform cloneFemaleSkinny = null;
			Vector2 pos = this.Circle2(UnityEngine.Random.Range(2f, this.range));
			Vector3 spawnPos = new Vector3(pos.x + base.transform.position.x, base.transform.position.y, pos.y + base.transform.position.z);
			cloneFemaleSkinny = PoolManager.Pools["enemies"].Spawn(this.mutant_female.transform, spawnPos, Quaternion.identity);
			if (!doName)
			{
				cloneFemaleSkinny.gameObject.name = cloneFemaleSkinny.gameObject.name + this.nameCount;
				this.nameCount++;
				doName = true;
			}
			base.StartCoroutine(this.fixMutantPosition(cloneFemaleSkinny, spawnPos));
			cloneFemaleSkinny.SendMessage("setInstantSpawn", this.instantSpawn);
			cloneFemaleSkinny.SendMessage("setFemaleSkinny");
			if (this.leaderGo)
			{
				cloneFemaleSkinny.SendMessage("setFollower", this.leaderGo);
			}
			cloneFemaleSkinny.SendMessage("setInCave", this.spawnInCave);
			if (this.sinkholeSpawn)
			{
				cloneFemaleSkinny.SendMessage("setInCave", true);
			}
			cloneFemaleSkinny.SendMessage("addToSpawn", base.gameObject);
			cloneFemaleSkinny.SendMessage("eatingOnSpawn", this.eatingOnSpawn);
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
			Transform cloneP = null;
			Vector3 spawnPos = new Vector3(0f, 0f, 0f);
			Vector2 pos = this.Circle2(UnityEngine.Random.Range(2f, this.range));
			spawnPos = new Vector3(pos.x + base.transform.position.x, base.transform.position.y, pos.y + base.transform.position.z);
			cloneP = PoolManager.Pools["enemies"].Spawn(this.mutant.transform, spawnPos, Quaternion.identity);
			if (!this.PaleOnCeiling)
			{
				base.StartCoroutine(this.fixMutantPosition(cloneP, spawnPos));
			}
			if (!this.paleOnCeiling)
			{
				cloneP.transform.GetChild(0).eulerAngles = new Vector3(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
			}
			if (!doName)
			{
				cloneP.gameObject.name = cloneP.gameObject.name + this.nameCount;
				this.nameCount++;
				doName = true;
			}
			cloneP.SendMessage("setInstantSpawn", this.instantSpawn);
			if (this.leader && this.doLeader && !this.creepySpawner)
			{
				this.leaderGo = cloneP.gameObject;
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
					cloneP.SendMessage("setFollower", this.leaderGo);
				}
				cloneP.SendMessage("setSkinnedTribe", this.skinnedTribe);
				cloneP.SendMessage("setPale");
				cloneP.SendMessage("setMaleSkinny");
				cloneP.SendMessage("setInCave", this.spawnInCave);
				if (this.sinkholeSpawn)
				{
					cloneP.SendMessage("setInCave", true);
				}
				cloneP.SendMessage("setPaleOnCeiling", this.paleOnCeiling);
				cloneP.SendMessage("setPatrolling", this.patrolling);
				cloneP.SendMessage("setWakeUpInCave", this.wakeUpOnSpawn);
				cloneP.SendMessage("addToSpawn", base.gameObject);
				cloneP.SendMessage("eatingOnSpawn", this.eatingOnSpawn);
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
