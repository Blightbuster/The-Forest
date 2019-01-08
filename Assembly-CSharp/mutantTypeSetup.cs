using System;
using System.Collections;
using Bolt;
using BoltInternal;
using PathologicalGames;
using TheForest.Interfaces;
using TheForest.Utils;
using TheForest.Utils.Settings;
using UnityEngine;

public class mutantTypeSetup : MonoBehaviour, IBurnable
{
	private void OnSpawned()
	{
		base.StopCoroutine("doSpawnDummy");
		if (this.setup.pmCombat)
		{
			this.setup.pmCombat.FsmVariables.GetFsmBool("hordeModeActive").Value = Scene.MutantControler.hordeModeActive;
		}
		base.StartCoroutine(this.initDefaultParams());
	}

	private void Start()
	{
		if (this.setup.ai.creepy_boss && BoltNetwork.isRunning && BoltNetwork.isServer)
		{
			this.AttachToNetwork(BoltPrefabs.girlMutant_net, StateSerializerTypeIds.IMutantGirlBoss);
		}
	}

	private IEnumerator initDefaultParams()
	{
		this.canStoreDefaultParams = false;
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		this.canStoreDefaultParams = true;
		this.storeDefaultParams();
		yield break;
	}

	public void storeDefaultParams()
	{
		if (!this.ai.male && !this.ai.female)
		{
			return;
		}
		if (this.canStoreDefaultParams)
		{
			this.storeOnCeilingBool = this.animator.GetBool("onCeilingBool");
			this.storeSkinnyBool = this.animator.GetBool("skinnyBool");
			this.storePaleMutantBool = this.animator.GetBool("paleMutantBool");
			this.storeMutantType = this.animator.GetFloat("mutantType");
		}
	}

	public void setDefaultParams()
	{
		if (!this.ai.male && !this.ai.female)
		{
			return;
		}
		if (this.canStoreDefaultParams)
		{
			this.animator.SetBoolReflected("onCeilingBool", this.storeOnCeilingBool);
			this.animator.SetBoolReflected("skinnyBool", this.storeSkinnyBool);
			this.animator.SetBoolReflected("paleMutantBool", this.storePaleMutantBool);
			this.animator.SetFloatReflected("mutantType", this.storeMutantType);
		}
	}

	private void OnDespawned()
	{
		base.CancelInvoke("removeSpawnCoolDown");
		this.removeSpawnBlock = false;
		base.StopCoroutine("doSpawnDummy");
		this.ResetFromSpikes();
		if (BoltNetwork.isServer)
		{
			BoltEntity component = base.gameObject.GetComponent<BoltEntity>();
			if (component)
			{
				UnityEngine.Object.Destroy(component);
			}
			CoopMutantSetup component2 = base.gameObject.GetComponent<CoopMutantSetup>();
			if (component2)
			{
				UnityEngine.Object.Destroy(component2);
			}
		}
		this.removeFromLists();
		if (this.setup.ai.creepy || this.setup.ai.creepy_male || this.setup.ai.creepy_baby || this.setup.ai.creepy_fat)
		{
			this.ai.target = this.setup.currentWaypoint.transform;
			this.setup.ai.pale = false;
			this.health.Health = this.health.maxHealth;
			if (this.setup.pmMotor)
			{
				this.setup.pmMotor.SendEvent("toStop");
			}
			this.setup.pmCombat.FsmVariables.GetFsmBool("inCaveBool").Value = false;
			this.animator.SetBool("deathBool", false);
			this.animator.SetBool("onFireBool", false);
			this.animator.SetBool("burning", false);
			this.oldMutant = false;
		}
		else
		{
			if (this.setup.mutantStats)
			{
				this.setup.mutantStats.targetDown = false;
			}
			this.health.Health = this.health.maxHealth;
			this.setup.disableNonActiveFSM("temp");
			if (this.setup.pmSleep)
			{
				this.setup.pmSleep.enabled = true;
				this.setup.pmSleep.FsmVariables.GetFsmBool("getSleepPosBool").Value = true;
				this.setup.pmSleep.FsmVariables.GetFsmBool("inCaveBool").Value = false;
				this.setup.pmSleep.FsmVariables.GetFsmBool("spawnerInCave").Value = false;
			}
			this.ai.target = this.setup.currentWaypoint.transform;
			if (this.setup.pmMotor)
			{
				this.setup.pmMotor.SendEvent("toStop");
			}
			this.animator.enabled = true;
			this.animator.SetBool("skinnyHasStick", false);
			this.animator.SetBoolReflected("deathfinalBOOL", false);
			this.animator.SetBoolReflected("deathBOOL", false);
			this.animator.SetBoolReflected("trapBool", false);
			this.animator.SetBoolReflected("dropFromTrap", false);
			this.animator.SetBoolReflected("enterTrapBool", false);
			this.setup.pmCombat.FsmVariables.GetFsmBool("deathFinal").Value = false;
			this.setup.pmBrain.FsmVariables.GetFsmBool("deadBool").Value = false;
			this.setup.pmCombat.FsmVariables.GetFsmBool("deathBool").Value = false;
			this.setup.pmCombat.FsmVariables.GetFsmBool("dynamiteMan").Value = false;
			this.setup.pmBrain.FsmVariables.GetFsmBool("leaderBool").Value = false;
			this.setup.pmBrain.FsmVariables.GetFsmBool("firemanBool").Value = false;
			this.setup.pmBrain.FsmVariables.GetFsmBool("paleBool").Value = false;
			this.setup.pmBrain.FsmVariables.GetFsmBool("eatBodyBool").Value = false;
			this.setup.pmBrain.FsmVariables.GetFsmBool("enableGravityBool").Value = true;
			this.setup.pmBrain.FsmVariables.GetFsmBool("playerIsRed").Value = false;
			this.setup.pmCombat.FsmVariables.GetFsmBool("eatBodyBool").Value = false;
			this.setup.pmCombat.FsmVariables.GetFsmBool("getFiremanBool").Value = false;
			this.setup.pmBrain.FsmVariables.GetFsmBool("femaleSkinnyBool").Value = false;
			this.setup.pmSleep.FsmVariables.GetFsmBool("paleOnCeiling").Value = false;
			if (this.setup.pmSearchScript)
			{
				this.setup.pmSearchScript._currentMemberGo = null;
				this.setup.pmSearchScript._eatBody = false;
			}
			if (this.health)
			{
				this.health.removeAllFeedingEffect();
			}
			this.setup.ai.skinned = false;
			this.setup.ai.painted = false;
			this.setup.ai.fireman = false;
			this.setup.ai.leader = false;
			this.setup.ai.fireman_dynamite = false;
			this.setup.ai.pale = false;
			this.setup.ai.femaleSkinny = false;
			this.setup.ai.maleSkinny = false;
			this.animator.SetBoolReflected("paleMutantBool", false);
			this.animator.SetBoolReflected("skinnyBool", false);
			this.animator.SetFloatReflected("mutantType", 0f);
			this.props.resetProps();
			this.oldMutant = false;
			if (this.setup.pmBrain)
			{
				this.setup.pmBrain.FsmVariables.GetFsmGameObject("spawnGo").Value = null;
			}
			this.setup.spawnGo = null;
			base.transform.GetChild(0).localScale = new Vector3(1.1f, 1.1f, 1.1f);
			if (this.setup.pmBrain)
			{
				this.setup.pmBrain.FsmVariables.GetFsmGameObject("leaderGo").Value = null;
			}
			if (this.setup.pmCombat)
			{
				this.setup.pmCombat.FsmVariables.GetFsmGameObject("leaderGo").Value = null;
			}
			if (this.followSetup)
			{
				this.followSetup.followersList.Clear();
			}
		}
	}

	private void Awake()
	{
		this.animator = base.transform.GetComponentInChildren<Animator>();
		this.getParams = base.GetComponent<getAnimatorParams>();
		this.setup = base.transform.GetComponentInChildren<mutantScriptSetup>();
		this.ai = base.transform.GetComponentInChildren<mutantAI>();
		this.props = base.transform.GetComponentInChildren<mutantPropManager>();
		this.propsFemale = base.transform.GetComponentInChildren<setupBodyVariation>();
		this.health = base.transform.GetComponentInChildren<EnemyHealth>();
		this.followSetup = base.transform.GetComponent<mutantFollowerFunctions>();
		this.dayCycle = base.transform.GetComponentInChildren<mutantDayCycle>();
		this.controlGo = Scene.MutantControler.gameObject;
		if (this.controlGo)
		{
			this.mutantControl = Scene.MutantControler;
		}
		this.spawnManager = this.controlGo.GetComponent<mutantSpawnManager>();
		this.stats = base.transform.GetComponent<targetStats>();
		this.waterLayer = LayerMask.NameToLayer("Water");
		this.soundDetect = base.transform.GetComponentInChildren<mutantSoundDetect>();
		this.ResetFromSpikes();
	}

	public void AttachToNetwork(PrefabId prefabId, UniqueId state)
	{
		if (BoltNetwork.isServer)
		{
			base.gameObject.AddComponent<CoopMutantSetup>();
			BoltEntity boltEntity = base.gameObject.AddComponent<BoltEntity>();
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

	public void resetSkinColor()
	{
		if (this.health.MySkin)
		{
			this.health.disablePoison();
		}
	}

	private IEnumerator setLeader(bool painted)
	{
		yield return YieldPresets.WaitForFixedUpdate;
		this.AttachToNetwork(BoltPrefabs.mutant_male_net, StateSerializerTypeIds.IMutantMaleState);
		this.animator.enabled = true;
		this.setup.ai.painted = painted;
		this.setup.ai.leader = true;
		if (this.ai.male)
		{
			this.props.setRegularMale(0);
			this.props.enableLeaderProps(1);
			if (painted)
			{
				this.animator.speed = 1.22f;
			}
			else
			{
				this.animator.speed = 1.16f;
			}
			this.setup.pmBrain.FsmVariables.GetFsmBool("firemanBool").Value = false;
			this.ai.animSpeed = this.animator.speed;
			if (painted)
			{
				this.health.Health = Mathf.FloorToInt(52f * GameSettings.Ai.leaderHealthRatio + GameSettings.Ai.paintedHealthAmount);
			}
			else
			{
				this.health.Health = Mathf.FloorToInt(52f * GameSettings.Ai.leaderHealthRatio);
			}
			this.health.maxHealth = this.health.Health;
			this.health.maxHealthFloat = (float)this.health.Health;
			if (painted)
			{
				base.transform.GetChild(0).localScale = new Vector3(1.17f, 1.17f, 1.17f);
			}
			else
			{
				base.transform.GetChild(0).localScale = new Vector3(1.1f, 1.1f, 1.1f);
			}
		}
		this.setup.pmBrain.FsmVariables.GetFsmBool("leaderBool").Value = true;
		this.setup.pmBrain.FsmVariables.GetFsmBool("firemanBool").Value = false;
		this.ai.animSpeed = this.animator.speed;
		if (BoltNetwork.isServer)
		{
			base.GetComponentInParent<BoltEntity>().GetState<IMutantState>().CharacterScale = base.transform.GetChild(0).localScale;
		}
		yield return YieldPresets.WaitForFixedUpdate;
		this.resetSkinColor();
		yield break;
	}

	private IEnumerator setPaleLeader()
	{
		yield return YieldPresets.WaitForFixedUpdate;
		this.AttachToNetwork(BoltPrefabs.mutant_male_net, StateSerializerTypeIds.IMutantMaleState);
		this.animator.enabled = true;
		this.setup.pmBrain.FsmVariables.GetFsmBool("leaderBool").Value = true;
		this.setup.pmBrain.FsmVariables.GetFsmBool("firemanBool").Value = false;
		this.setup.ai.leader = true;
		this.props.enablePaleProps();
		this.setup.pmBrain.FsmVariables.GetFsmBool("paleBool").Value = true;
		this.animator.SetBoolReflected("paleMutantBool", true);
		this.animator.SetBoolReflected("skinnyBool", false);
		this.animator.SetFloatReflected("mutantType", 2f);
		this.animator.speed = 1.18f;
		this.ai.animSpeed = this.animator.speed;
		this.setup.ai.pale = true;
		base.transform.GetChild(0).localScale = new Vector3(1.27f, 1.27f, 1.27f);
		if (BoltNetwork.isServer)
		{
			base.GetComponentInParent<BoltEntity>().GetState<IMutantState>().CharacterScale = base.transform.GetChild(0).localScale;
		}
		yield return YieldPresets.WaitForFixedUpdate;
		this.resetSkinColor();
		yield break;
	}

	private IEnumerator setFireman(bool painted)
	{
		yield return YieldPresets.WaitForFixedUpdate;
		this.AttachToNetwork(BoltPrefabs.mutant_male_net, StateSerializerTypeIds.IMutantMaleState);
		this.animator.enabled = true;
		this.setup.ai.fireman = true;
		this.props.setRegularMale(0);
		this.props.enableFiremanProps();
		this.setup.pmBrain.FsmVariables.GetFsmBool("firemanBool").Value = true;
		if (painted)
		{
			this.animator.speed = 1.2f;
		}
		else
		{
			this.animator.speed = 1.15f;
		}
		this.ai.animSpeed = this.animator.speed;
		if (BoltNetwork.isServer)
		{
			base.GetComponentInParent<BoltEntity>().GetState<IMutantState>().CharacterScale = base.transform.GetChild(0).localScale;
		}
		yield return YieldPresets.WaitForFixedUpdate;
		this.resetSkinColor();
		yield break;
	}

	public void forceDynamiteMan(bool onoff)
	{
		if (onoff)
		{
			this.setup.pmCombat.FsmVariables.GetFsmBool("dynamiteMan").Value = true;
			this.ai.fireman_dynamite = true;
			this.props.dynamiteMan = true;
		}
		else
		{
			this.setup.pmCombat.FsmVariables.GetFsmBool("dynamiteMan").Value = false;
			this.ai.fireman_dynamite = false;
			this.props.dynamiteMan = false;
		}
	}

	private IEnumerator setPale()
	{
		yield return YieldPresets.WaitForFixedUpdate;
		if (!base.GetComponent<BoltEntity>())
		{
			this.AttachToNetwork(BoltPrefabs.mutant_male_net, StateSerializerTypeIds.IMutantMaleState);
		}
		this.animator.enabled = true;
		this.props.enablePaleProps();
		this.setup.pmBrain.FsmVariables.GetFsmBool("paleBool").Value = true;
		this.setup.pmBrain.FsmVariables.GetFsmBool("firemanBool").Value = false;
		this.animator.SetBoolReflected("paleMutantBool", true);
		this.animator.SetBoolReflected("skinnyBool", false);
		this.animator.SetFloatReflected("mutantType", 2f);
		if (this.ai.skinned)
		{
			this.animator.speed = 1.22f;
		}
		this.animator.speed = UnityEngine.Random.Range(1.1f, 1.16f);
		this.ai.animSpeed = this.animator.speed;
		this.setup.ai.pale = true;
		if (this.setup.ai.skinned)
		{
			this.health.Health = Mathf.FloorToInt(70f * GameSettings.Ai.largePaleHealthRatio + GameSettings.Ai.skinMaskHealthAmount);
		}
		else
		{
			this.health.Health = Mathf.FloorToInt(70f * GameSettings.Ai.largePaleHealthRatio);
		}
		this.health.maxHealth = this.health.Health;
		this.health.maxHealthFloat = (float)this.health.Health;
		base.transform.GetChild(0).localScale = new Vector3(1.24f, 1.24f, 1.24f);
		if (BoltNetwork.isServer)
		{
			base.GetComponentInParent<BoltEntity>().GetState<IMutantState>().CharacterScale = base.transform.GetChild(0).localScale;
		}
		yield return YieldPresets.WaitForFixedUpdate;
		this.resetSkinColor();
		yield break;
	}

	public void setOldMutant(bool isTrue)
	{
		this.oldMutant = isTrue;
	}

	public void setSkinnedTribe(bool isTrue)
	{
		this.props.setSkinnedMutant(isTrue);
		this.ai.skinned = isTrue;
	}

	private IEnumerator setCreepyPale(bool set)
	{
		this.animator.enabled = true;
		if (set)
		{
			this.animator.speed = 1.15f;
			this.ai.animSpeed = this.animator.speed;
			this.setup.ai.pale = set;
			if (this.ai.creepy_male)
			{
				this.health.Health = Mathf.FloorToInt(250f * GameSettings.Ai.creepyHealthRatio);
				this.health.maxHealth = Mathf.FloorToInt(250f * GameSettings.Ai.creepyHealthRatio);
				this.health.maxHealthFloat = (float)this.health.Health;
			}
			else
			{
				this.health.Health = Mathf.FloorToInt(225f * GameSettings.Ai.creepyHealthRatio);
				this.health.maxHealth = Mathf.FloorToInt(225f * GameSettings.Ai.creepyHealthRatio);
				this.health.maxHealthFloat = (float)this.health.Health;
			}
			if (this.creepyPaleMat)
			{
				base.transform.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial = this.creepyPaleMat;
			}
		}
		else
		{
			if (this.oldMutant && !this.ai.creepy_baby)
			{
				this.animator.speed = 0.77f;
			}
			else
			{
				this.animator.speed = 1.05f;
			}
			this.ai.animSpeed = this.animator.speed;
			this.setup.ai.pale = set;
			if (this.oldMutant)
			{
				base.transform.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial = this.oldCreepyMat;
			}
			else if (this.creepyPaleMat)
			{
				base.transform.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial = this.creepyMat;
			}
			if (this.ai.creepy_male)
			{
				if (this.oldMutant)
				{
					this.health.Health = Mathf.FloorToInt(65f * GameSettings.Ai.creepyHealthRatio);
				}
				else
				{
					this.health.Health = Mathf.FloorToInt(200f * GameSettings.Ai.creepyHealthRatio);
				}
				this.health.maxHealth = this.health.Health;
				this.health.maxHealthFloat = (float)this.health.Health;
			}
			else
			{
				this.health.Health = Mathf.FloorToInt(175f * GameSettings.Ai.creepyHealthRatio);
				this.health.maxHealth = this.health.Health;
				this.health.maxHealthFloat = (float)this.health.Health;
			}
		}
		yield return YieldPresets.WaitForFixedUpdate;
		this.resetSkinColor();
		yield break;
	}

	private IEnumerator setDefault(bool painted)
	{
		yield return YieldPresets.WaitForFixedUpdate;
		this.AttachToNetwork(BoltPrefabs.mutant_male_net, StateSerializerTypeIds.IMutantMaleState);
		this.animator.enabled = true;
		if (painted)
		{
			this.props.setPaintedMale(0);
		}
		else
		{
			this.props.setRegularMale(0);
		}
		if (painted)
		{
			this.ai.painted = true;
		}
		this.animator.SetFloat("mutantType", 0f);
		this.setup.pmBrain.FsmVariables.GetFsmBool("firemanBool").Value = false;
		this.animator.SetBoolReflected("skinnyBool", false);
		if (painted)
		{
			this.animator.speed = UnityEngine.Random.Range(1.18f, 1.24f);
		}
		else
		{
			this.animator.speed = UnityEngine.Random.Range(1.1f, 1.15f);
		}
		this.ai.animSpeed = this.animator.speed;
		if (painted)
		{
			this.health.Health = Mathf.FloorToInt(52f * GameSettings.Ai.leaderHealthRatio + GameSettings.Ai.paintedHealthAmount);
		}
		else
		{
			this.health.Health = Mathf.FloorToInt(52f * GameSettings.Ai.regularHealthRatio);
		}
		this.health.maxHealth = this.health.Health;
		this.health.maxHealthFloat = (float)this.health.Health;
		if (painted)
		{
			base.transform.GetChild(0).localScale = new Vector3(1.16f, 1.16f, 1.16f);
		}
		else
		{
			base.transform.GetChild(0).localScale = new Vector3(1.1f, 1.1f, 1.1f);
		}
		if (!painted)
		{
			this.props.enableDefaultProps();
		}
		if (BoltNetwork.isServer)
		{
			base.GetComponentInParent<BoltEntity>().GetState<IMutantState>().CharacterScale = base.transform.GetChild(0).localScale;
		}
		yield return YieldPresets.WaitForFixedUpdate;
		this.resetSkinColor();
		yield break;
	}

	private IEnumerator setFemale(bool painted)
	{
		yield return YieldPresets.WaitForFixedUpdate;
		this.AttachToNetwork(BoltPrefabs.mutant_female_net, StateSerializerTypeIds.IMutantFemaleState);
		this.animator.enabled = true;
		if (painted)
		{
			this.propsFemale.setFemalePainted(0);
		}
		else
		{
			this.propsFemale.setFemaleRegular(0);
		}
		if (painted)
		{
			this.ai.painted = true;
		}
		this.animator.SetFloat("mutantType", 1f);
		this.setup.pmBrain.FsmVariables.GetFsmBool("firemanBool").Value = false;
		this.animator.SetBoolReflected("skinnyBool", false);
		if (painted)
		{
			this.animator.speed = 1.25f;
		}
		else
		{
			this.animator.speed = 1.18f;
		}
		this.ai.animSpeed = this.animator.speed;
		base.transform.GetChild(0).localScale = new Vector3(1.08f, 1.08f, 1.08f);
		this.health.Health = Mathf.FloorToInt(46f * GameSettings.Ai.regularHealthRatio + GameSettings.Ai.paintedHealthAmount);
		this.health.maxHealth = this.health.Health;
		this.health.maxHealthFloat = (float)this.health.Health;
		this.health.recoverValue = Mathf.FloorToInt(24f * GameSettings.Ai.regularHealthRatio + GameSettings.Ai.paintedHealthAmount);
		this.setup.pmBrain.FsmVariables.GetFsmBool("femaleBool").Value = true;
		if (BoltNetwork.isServer)
		{
			base.GetComponentInParent<BoltEntity>().GetState<IMutantState>().CharacterScale = base.transform.GetChild(0).localScale;
		}
		yield return YieldPresets.WaitForFixedUpdate;
		this.resetSkinColor();
		yield break;
	}

	private IEnumerator setArmsy(GameObject go)
	{
		yield return YieldPresets.WaitForFixedUpdate;
		this.AttachToNetwork(BoltPrefabs.male_creepy_net, StateSerializerTypeIds.IMutantMaleCreepyState);
		if (this.setup.pmCombat)
		{
			this.setup.pmCombat.FsmVariables.GetFsmGameObject("spawnGo").Value = go;
		}
		this.setup.spawnGo = go;
		this.ai.animSpeed = 1.05f;
		this.animator.speed = 1.05f;
		if (BoltNetwork.isServer)
		{
			base.GetComponentInParent<BoltEntity>().GetState<IMutantState>().CharacterScale = base.transform.GetChild(0).localScale;
		}
		yield return YieldPresets.WaitForFixedUpdate;
		this.resetSkinColor();
		yield break;
	}

	private IEnumerator setVags(GameObject go)
	{
		yield return YieldPresets.WaitForFixedUpdate;
		this.AttachToNetwork(BoltPrefabs.female_creepy_net, StateSerializerTypeIds.IMutantFemaleCreepyState);
		if (this.setup.pmCombat)
		{
			this.setup.pmCombat.FsmVariables.GetFsmGameObject("spawnGo").Value = go;
		}
		this.setup.spawnGo = go;
		if (BoltNetwork.isServer)
		{
			base.GetComponentInParent<BoltEntity>().GetState<IMutantState>().CharacterScale = base.transform.GetChild(0).localScale;
		}
		yield return YieldPresets.WaitForFixedUpdate;
		this.resetSkinColor();
		yield break;
	}

	private IEnumerator setBaby(GameObject go)
	{
		yield return YieldPresets.WaitForFixedUpdate;
		if (this.oldMutant)
		{
			base.transform.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial = this.oldCreepyMat;
		}
		else
		{
			base.transform.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial = this.creepyMat;
		}
		if (this.setup.pmCombat)
		{
			this.setup.pmCombat.FsmVariables.GetFsmGameObject("spawnGo").Value = go;
			this.setup.pmCombat.FsmVariables.GetFsmBool("bossBaby").Value = this.bossBaby;
		}
		this.setup.spawnGo = go;
		yield return YieldPresets.WaitForFixedUpdate;
		yield return YieldPresets.WaitForFixedUpdate;
		yield return YieldPresets.WaitForFixedUpdate;
		this.AttachToNetwork(BoltPrefabs.mutant_baby_net, StateSerializerTypeIds.IMutantBabyState);
		if (this.bossBaby)
		{
			this.ai.animSpeed = 1.35f;
			this.animator.speed = 1.35f;
		}
		else
		{
			this.ai.animSpeed = 1.18f;
			this.animator.speed = 1.18f;
		}
		if (BoltNetwork.isServer)
		{
			base.GetComponentInParent<BoltEntity>().GetState<IMutantState>().CharacterScale = base.transform.GetChild(0).localScale;
		}
		yield return YieldPresets.WaitForFixedUpdate;
		this.resetSkinColor();
		yield break;
	}

	private IEnumerator setFatCreepy(GameObject go)
	{
		yield return YieldPresets.WaitForFixedUpdate;
		this.AttachToNetwork(BoltPrefabs.mutant_creepy_fat_net, StateSerializerTypeIds.IMutantFatCreepy);
		if (this.setup.pmCombat)
		{
			this.setup.pmCombat.FsmVariables.GetFsmGameObject("spawnGo").Value = go;
		}
		this.setup.spawnGo = go;
		this.ai.animSpeed = 1.15f;
		this.animator.speed = 1.15f;
		yield return YieldPresets.WaitForFixedUpdate;
		this.resetSkinColor();
		yield break;
	}

	private IEnumerator setGirlMutant()
	{
		yield return YieldPresets.WaitForFixedUpdate;
		Debug.Log("SETTING GIRL MUTANT");
		creepyAnimatorControl cac = this.setup.ai.GetComponent<creepyAnimatorControl>();
		if (cac)
		{
			cac.enabled = true;
		}
		cac.activateGirlMutantInWorld();
		yield break;
	}

	private void setSkinnyLeader()
	{
		this.setup.ai.leader = true;
	}

	private void setBossBaby(bool onoff)
	{
		this.bossBaby = onoff;
	}

	private IEnumerator setMaleSkinny()
	{
		yield return YieldPresets.WaitForFixedUpdate;
		if (!base.GetComponent<BoltEntity>())
		{
			this.AttachToNetwork(BoltPrefabs.mutant_male_net, StateSerializerTypeIds.IMutantMaleState);
		}
		this.animator.enabled = true;
		if (this.ai.pale)
		{
			this.props.enableMaleSkinnyPaleProps();
		}
		else
		{
			this.props.enableMaleSkinnyProps();
		}
		this.setup.pmBrain.FsmVariables.GetFsmBool("femaleSkinnyBool").Value = true;
		this.animator.SetBool("skinnyBool", true);
		if (!this.mutantControl.activeSkinnyCannibals.Contains(base.gameObject))
		{
			this.mutantControl.activeSkinnyCannibals.Add(base.gameObject);
		}
		this.animator.SetFloatReflected("mutantType", 3f);
		if (this.ai.pale)
		{
			if (this.ai.skinned)
			{
				this.health.Health = Mathf.FloorToInt(45f * GameSettings.Ai.paleSkinnyHealthRatio + GameSettings.Ai.skinMaskHealthAmount);
			}
			else
			{
				this.health.Health = Mathf.FloorToInt(35f * GameSettings.Ai.paleSkinnyHealthRatio);
			}
			this.animator.speed = 1.38f;
			base.transform.GetChild(0).localScale = new Vector3(1.05f, 1.05f, 1.05f);
		}
		else
		{
			this.animator.speed = 1.2f;
			this.health.Health = Mathf.FloorToInt(43f * GameSettings.Ai.skinnyHealthRatio);
			base.transform.GetChild(0).localScale = new Vector3(1.03f, 1.03f, 1.03f);
		}
		this.health.maxHealth = this.health.Health;
		this.health.maxHealthFloat = (float)this.health.Health;
		this.health.recoverValue = Mathf.FloorToInt(20f * GameSettings.Ai.skinnyHealthRatio);
		this.ai.animSpeed = this.animator.speed;
		this.setup.ai.maleSkinny = true;
		if (BoltNetwork.isServer)
		{
			base.GetComponentInParent<BoltEntity>().GetState<IMutantState>().CharacterScale = base.transform.GetChild(0).localScale;
		}
		yield return YieldPresets.WaitForFixedUpdate;
		this.resetSkinColor();
		yield break;
	}

	private IEnumerator setFemaleSkinny()
	{
		yield return YieldPresets.WaitForFixedUpdate;
		this.AttachToNetwork(BoltPrefabs.mutant_female_net, StateSerializerTypeIds.IMutantFemaleState);
		this.animator.enabled = true;
		this.propsFemale.enableFemaleSkinny();
		this.setup.pmBrain.FsmVariables.GetFsmBool("femaleBool").Value = true;
		this.setup.pmBrain.FsmVariables.GetFsmBool("femaleSkinnyBool").Value = true;
		this.animator.SetBoolReflected("skinnyBool", true);
		if (!this.mutantControl.activeSkinnyCannibals.Contains(base.gameObject))
		{
			this.mutantControl.activeSkinnyCannibals.Add(base.gameObject);
		}
		this.animator.SetFloat("mutantType", 3f);
		this.health.Health = Mathf.FloorToInt(38f * GameSettings.Ai.skinnyHealthRatio);
		this.health.maxHealth = Mathf.FloorToInt(38f * GameSettings.Ai.skinnyHealthRatio);
		this.health.maxHealthFloat = (float)this.health.Health;
		this.health.recoverValue = Mathf.FloorToInt(18f * GameSettings.Ai.skinnyHealthRatio);
		this.animator.speed = 1.2f;
		this.ai.animSpeed = this.animator.speed;
		this.setup.ai.femaleSkinny = true;
		if (BoltNetwork.isServer)
		{
			base.GetComponentInParent<BoltEntity>().GetState<IMutantState>().CharacterScale = base.transform.GetChild(0).localScale;
		}
		yield return YieldPresets.WaitForFixedUpdate;
		this.resetSkinColor();
		yield break;
	}

	private IEnumerator setFollower(GameObject leader)
	{
		yield return YieldPresets.WaitForFixedUpdate;
		if (leader)
		{
			if (this.ai.creepy_baby || this.ai.creepy || this.ai.creepy_fat || this.ai.creepy_male)
			{
				this.setup.pmCombat.FsmVariables.GetFsmGameObject("leaderGo").Value = leader;
			}
			else
			{
				this.setup.pmBrain.FsmVariables.GetFsmGameObject("leaderGo").Value = leader;
			}
			leader.SendMessage("addFollower", base.gameObject, SendMessageOptions.DontRequireReceiver);
		}
		yield break;
	}

	private void setInCave(bool set)
	{
		if (this.ai.creepy || this.ai.creepy_baby || this.ai.creepy_male || this.ai.creepy_fat)
		{
			this.setup.pmCombat.FsmVariables.GetFsmBool("inCaveBool").Value = set;
		}
		else
		{
			if (this.setup.pmSleep)
			{
				this.setup.pmSleep.FsmVariables.GetFsmBool("inCaveBool").Value = set;
			}
			if (this.setup.pmSleep)
			{
				this.setup.pmSleep.FsmVariables.GetFsmBool("spawnerInCave").Value = set;
			}
		}
		this.inCave = set;
	}

	private void setWakeUpInCave(bool set)
	{
		this.setup.dayCycle.sleepBlocker = set;
	}

	private void enableSleepBlocker()
	{
		this.setup.dayCycle.sleepBlocker = true;
	}

	private void setSleeping(bool set)
	{
		if (this.ai.creepy || this.ai.creepy_baby || this.ai.creepy_male)
		{
			this.setup.pmCombat.FsmVariables.GetFsmBool("sleepOnAwake").Value = set;
		}
	}

	private IEnumerator addFollower(GameObject follower)
	{
		yield return YieldPresets.WaitForFixedUpdate;
		if (this.followSetup)
		{
			this.followSetup.followersList.Add(follower);
		}
		yield break;
	}

	private void setPaleOnCeiling(bool set)
	{
		this.animator.enabled = true;
		this.setup.pmSleep.FsmVariables.GetFsmBool("paleOnCeiling").Value = set;
		this.animator.SetBoolReflected("onCeilingBool", set);
	}

	private void forceCeilingBool()
	{
		if (this.setup.pmSleep.FsmVariables.GetFsmBool("paleOnCeiling").Value)
		{
			this.animator.SetBoolReflected("paleMutantBool", true);
			this.animator.SetBoolReflected("onCeilingBool", true);
		}
	}

	private IEnumerator addToSpawn(GameObject go)
	{
		yield return YieldPresets.WaitForFixedUpdate;
		if (go)
		{
			this.spawner = go.GetComponent<spawnMutants>();
			this.spawner.allMembers.Add(base.gameObject);
			if (this.setup.pmBrain)
			{
				this.setup.pmBrain.FsmVariables.GetFsmGameObject("spawnGo").Value = go;
			}
			this.setup.spawnGo = go;
			if (this.instantSpawn)
			{
				this.setInstantSpawn(true);
			}
			if (this.controlGo)
			{
				if (this.ai.creepy_baby)
				{
					if (!this.mutantControl.activeBabies.Contains(base.gameObject))
					{
						this.mutantControl.activeBabies.Add(base.gameObject);
					}
				}
				else if (!this.mutantControl.activeCannibals.Contains(base.gameObject))
				{
					this.mutantControl.activeCannibals.Add(base.gameObject);
				}
				bool value;
				if (this.setup.pmSleep)
				{
					value = this.setup.pmSleep.FsmVariables.GetFsmBool("inCaveBool").Value;
				}
				else if (this.setup.pmCombat)
				{
					value = this.setup.pmCombat.FsmVariables.GetFsmBool("inCaveBool").Value;
				}
				else
				{
					value = this.setup.pmCombat.FsmVariables.GetFsmBool("inCaveBool").Value;
				}
				if (value)
				{
					if (!this.mutantControl.activeCaveCannibals.Contains(base.gameObject))
					{
						this.mutantControl.activeCaveCannibals.Add(base.gameObject);
					}
				}
				else if (!this.ai.creepy_baby && !this.mutantControl.activeWorldCannibals.Contains(base.gameObject))
				{
					this.mutantControl.activeWorldCannibals.Add(base.gameObject);
				}
			}
		}
		yield break;
	}

	public bool IsBurning
	{
		get
		{
			return this.setup.hitReceiver.IsBurning;
		}
	}

	public void removeFromSpawn()
	{
		if (!this.removeSpawnBlock)
		{
			if (this.ai.leader)
			{
				this.sendRemoveLeader();
			}
			if (this.spawner)
			{
				this.spawner.allMembers.Remove(base.gameObject);
			}
			if (this.controlGo && this.spawner && this.spawner.allMembers.Count == 0)
			{
				this.updateSpawnManagers();
			}
			this.removeFromLists();
			this.updateSpawnerAmounts();
			if ((this.setup.ai.male || this.setup.ai.female) && base.gameObject.activeSelf)
			{
				base.StartCoroutine("doSpawnDummy");
			}
			this.removeSpawnBlock = true;
			base.Invoke("removeSpawnCoolDown", 15f);
		}
	}

	private void removeSpawnCoolDown()
	{
		this.removeSpawnBlock = false;
	}

	public void removeFromSpawnAndExplode()
	{
		if (this.ai.leader)
		{
			this.sendRemoveLeader();
		}
		if (this.spawner)
		{
			this.spawner.allMembers.Remove(base.gameObject);
		}
		if (this.controlGo && this.spawner && this.spawner.allMembers.Count == 0)
		{
			this.updateSpawnManagers();
		}
		this.removeFromLists();
		this.updateSpawnerAmounts();
		if (PoolManager.Pools["enemies"].IsSpawned(base.transform))
		{
			PoolManager.Pools["enemies"].Despawn(base.transform);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void sendRemoveLeader()
	{
		foreach (GameObject gameObject in this.spawner.allMembers)
		{
			if (gameObject)
			{
				gameObject.SendMessage("removeLeaderGo", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	private void removeLeaderGo()
	{
		if (this.setup.pmBrain)
		{
			this.setup.pmBrain.FsmVariables.GetFsmGameObject("leaderGo").Value = null;
		}
	}

	private void updateSpawnerAmounts()
	{
		if (this.spawner)
		{
			if (this.ai.maleSkinny && this.ai.pale)
			{
				this.spawner.amount_skinny_pale--;
			}
			if (this.ai.male && this.ai.pale && !this.ai.maleSkinny)
			{
				this.spawner.amount_pale--;
			}
			if (this.ai.creepy)
			{
				this.spawner.amount_vags--;
			}
			if (this.ai.creepy_baby)
			{
				this.spawner.amount_baby--;
			}
			if (this.ai.creepy_fat)
			{
				this.spawner.amount_fat--;
			}
			if (this.ai.creepy_male)
			{
				this.spawner.amount_armsy--;
			}
		}
	}

	private void removeFromLists()
	{
		if (this.mutantControl.activeCannibals.Contains(base.gameObject))
		{
			this.mutantControl.activeCannibals.Remove(base.gameObject);
		}
		if (this.mutantControl.activeCaveCannibals.Contains(base.gameObject))
		{
			this.mutantControl.activeCaveCannibals.Remove(base.gameObject);
		}
		if (this.mutantControl.activeWorldCannibals.Contains(base.gameObject))
		{
			this.mutantControl.activeWorldCannibals.Remove(base.gameObject);
		}
		if (this.mutantControl.activeBabies.Contains(base.gameObject))
		{
			this.mutantControl.activeBabies.Remove(base.gameObject);
		}
	}

	private void updateSpawnManagers()
	{
		if (Clock.Day > 1)
		{
			if (this.ai.creepy_boss)
			{
				if (this.mutantControl.numActiveCreepySpawns > 0)
				{
					this.mutantControl.numActiveCreepySpawns--;
				}
				if (this.mutantControl.numActiveGirlSpawns > 0)
				{
					this.mutantControl.numActiveGirlSpawns--;
				}
			}
			else if ((this.ai.male || this.ai.female) && this.ai.painted)
			{
				if (this.spawnManager.offsetPainted > -5)
				{
					this.spawnManager.offsetPainted--;
				}
				this.mutantControl.numActivePaintedSpawns--;
				if (this.spawnManager.desiredPainted > 0)
				{
					this.spawnManager.desiredPainted--;
				}
			}
			else if (this.ai.pale && this.ai.skinned)
			{
				if (this.spawnManager.offsetSkinned > -5)
				{
					this.spawnManager.offsetSkinned--;
				}
				this.mutantControl.numActiveSkinnedSpawns--;
				if (this.spawnManager.desiredSkinned > 0)
				{
					this.spawnManager.desiredSkinned--;
				}
			}
			else if (this.ai.pale && this.ai.maleSkinny)
			{
				if (this.spawnManager.offsetSkinnyPale > -5)
				{
					this.spawnManager.offsetSkinnyPale--;
				}
				this.mutantControl.numActiveSkinnyPaleSpawns--;
				if (this.spawnManager.desiredSkinnyPale > 0)
				{
					this.spawnManager.desiredSkinnyPale--;
				}
			}
			else if (this.ai.maleSkinny || this.ai.femaleSkinny)
			{
				if (this.spawnManager.offsetSkinny > -5)
				{
					this.spawnManager.offsetSkinny--;
				}
				this.mutantControl.numActiveSkinnySpawns--;
				if (this.spawnManager.desiredSkinny > 1)
				{
					this.spawnManager.desiredSkinny -= 2;
				}
				if (this.mutantControl.activeSkinnyCannibals.Contains(base.gameObject))
				{
					this.mutantControl.activeSkinnyCannibals.Remove(base.gameObject);
				}
			}
			else if (this.ai.pale && this.ai.male)
			{
				if (this.spawnManager.offsetPale > -5)
				{
					this.spawnManager.offsetPale--;
				}
				this.mutantControl.numActivePaleSpawns--;
				if (this.spawnManager.desiredPale > 0)
				{
					this.spawnManager.desiredPale--;
				}
			}
			else if (this.ai.male || this.ai.female)
			{
				if (this.spawnManager.offsetRegular > -5)
				{
					this.spawnManager.offsetRegular--;
				}
				this.mutantControl.numActiveRegularSpawns--;
				if (this.spawnManager.desiredRegular > 0)
				{
					this.spawnManager.desiredRegular--;
				}
			}
			else if (this.ai.creepy || this.ai.creepy_male || this.ai.creepy_fat)
			{
				if (this.spawnManager.offsetCreepy > -5)
				{
					this.spawnManager.offsetCreepy--;
				}
				this.mutantControl.numActiveCreepySpawns--;
				if (this.spawnManager.desiredCreepy > 0)
				{
					this.spawnManager.desiredCreepy--;
				}
			}
		}
		UnityEngine.Object.Destroy(this.spawner.gameObject);
		this.mutantControl.numActiveSpawns--;
		if (Scene.MutantControler.hordeModeActive && Scene.MutantControler.activeWorldCannibals.Count == 0)
		{
			Debug.Log("all mutants dead, preparing next horde level..");
			Scene.MutantControler.doNextHordeWave();
		}
	}

	private void ResetFromSpikes()
	{
		if (this.getParams != null)
		{
			this.getParams.DeadPosition = base.transform.position;
			this.getParams.useSpikes = false;
		}
	}

	public void setPositionAtSpikes(Vector3 nearSpikes)
	{
		if (this.getParams)
		{
			this.getParams.DeadPosition = nearSpikes;
			this.getParams.useSpikes = true;
			base.transform.root.position = nearSpikes;
			this.setup.pmBrain.FsmVariables.GetFsmBool("disableControllerBool").Value = true;
			base.StartCoroutine(this.lockMutantToSpikes(nearSpikes));
		}
	}

	private IEnumerator lockMutantToSpikes(Vector3 spikePos)
	{
		float t = 0f;
		while (t < 1f)
		{
			base.transform.position = spikePos;
			t += Time.deltaTime;
			yield return null;
		}
		yield break;
	}

	private IEnumerator doSpawnDummy()
	{
		AnimatorStateInfo state = this.animator.GetCurrentAnimatorStateInfo(0);
		while (this.setup.health.onFire)
		{
			state = this.animator.GetCurrentAnimatorStateInfo(0);
			if (state.shortNameHash == this.setup.animControl.burnToDeathHash || state.shortNameHash == this.setup.animControl.burnToDeathHashMir || state.tagHash == this.setup.animControl.deathHash)
			{
				break;
			}
			yield return null;
		}
		float deadTimer = 0f;
		bool deadBreak = false;
		if (this.animator)
		{
			this.animator.enabled = true;
		}
		if (!this.animator.GetBool("trapBool"))
		{
			this.setup.animator.transform.SendMessage("setupRagDollParts", true, SendMessageOptions.DontRequireReceiver);
			if (BoltNetwork.isServer)
			{
				ragdollActivate ragdollActivate = ragdollActivate.Create(GlobalTargets.AllClients);
				ragdollActivate.Target = base.transform.GetComponent<BoltEntity>();
				ragdollActivate.Send();
			}
		}
		if (this.ai.male && !this.ai.pale && !this.ai.maleSkinny)
		{
			this.spawnHeldWeapon();
		}
		if (this.ai.male && this.setup.propManager.regularStick.activeSelf)
		{
			GameObject gameObject;
			if (BoltNetwork.isRunning)
			{
				gameObject = UnityEngine.Object.Instantiate<GameObject>(this.setup.stickPickupMP, this.setup.charLeftWeaponGo.transform.position, this.setup.charLeftWeaponGo.transform.rotation);
			}
			else
			{
				gameObject = UnityEngine.Object.Instantiate<GameObject>(this.setup.stickPickup, this.setup.charLeftWeaponGo.transform.position, this.setup.charLeftWeaponGo.transform.rotation);
			}
			gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
			this.setup.propManager.regularStick.SetActive(false);
		}
		this.ai.StartCoroutine("toStop");
		if (this.animator.enabled)
		{
			while (state.tagHash != this.setup.hashs.deathTag)
			{
				this.ai.StartCoroutine("toStop");
				state = this.animator.GetCurrentAnimatorStateInfo(0);
				yield return null;
			}
			while (state.normalizedTime < 1f && !deadBreak)
			{
				state = this.animator.GetCurrentAnimatorStateInfo(0);
				deadTimer += Time.deltaTime;
				if (deadTimer > 5f)
				{
					deadBreak = true;
				}
				yield return null;
			}
		}
		yield return YieldPresets.WaitTwoSeconds;
		yield return YieldPresets.WaitPointFiveSeconds;
		this.getParams.StartCoroutine("spawnDummy", new getAnimatorParams.DummyParams
		{
			Angle = this.setup.thisGo.transform.rotation,
			IsDummyLoad = false,
			DiedLayingDown = (state.IsName("Base Layer.dyingLoop") || state.IsName("Base Layer.dyringLoopMirror") || state.IsName("Base Layer.dyingToIdle") || state.IsName("Base Layer.dyingToIdleMirror") || state.IsName("Base Layer.dyingToDead") || state.IsName("Base Layer.deathMir1") || state.IsName("Base Layer.hitStomachMir1") || state.IsName("Base Layer.hitFaceRightMir1") || state.IsName("Base Layer.hitFaceLeftMir1") || state.IsName("Base Layer.hitStomach1") || state.IsName("Base Layer.hitFaceRight1") || state.IsName("Base Layer.hitFaceLeft1"))
		});
		yield return null;
		yield break;
	}

	private void spawnHeldWeapon()
	{
		bool flag = false;
		for (int i = 0; i < this.props.regularWeapons.Length; i++)
		{
			if (this.props.regularWeapons[i].activeSelf)
			{
				pickupSpawn component = this.props.regularWeapons[i].GetComponent<pickupSpawn>();
				if (component)
				{
					UnityEngine.Object.Instantiate<GameObject>(component.spawnMe, this.setup.charLeftWeaponGo.transform.position, this.setup.charLeftWeaponGo.transform.rotation);
					this.props.regularWeapons[i].SetActive(false);
					flag = true;
				}
			}
		}
		for (int j = 0; j < this.props.advancedWeapons.Length; j++)
		{
			if (this.props.advancedWeapons[j].activeSelf)
			{
				pickupSpawn component2 = this.props.advancedWeapons[j].GetComponent<pickupSpawn>();
				if (component2)
				{
					UnityEngine.Object.Instantiate<GameObject>(component2.spawnMe, this.setup.charLeftWeaponGo.transform.position, this.setup.charLeftWeaponGo.transform.rotation);
					this.props.advancedWeapons[j].SetActive(false);
					flag = true;
				}
			}
		}
		if (!flag)
		{
			UnityEngine.Object.Instantiate<GameObject>(this.setup.clubPickup, this.setup.charLeftWeaponGo.transform.position, this.setup.charLeftWeaponGo.transform.rotation);
			this.setup.charLeftWeaponGo.SetActive(false);
		}
	}

	public void armsyInCave()
	{
		this.dayCycle.armsyInCave();
	}

	private void enableWeapon()
	{
		if (!this.setup.ai.pale && !this.setup.ai.female && this.setup.weapon)
		{
			this.setup.weapon.SetActive(true);
		}
	}

	public void setVisionRange(float range)
	{
		this.setup.search.modifiedVisRange = range;
	}

	public void setLighterRange(float range)
	{
		this.setup.search.modLighterRange = range;
	}

	public void setMudRange(float range)
	{
		this.setup.search.modMudRange = range;
	}

	public void setCrouchRange(float range)
	{
		this.setup.search.modCrouchRange = range;
	}

	private void setVisionLayersOn()
	{
		if (this.setup.search)
		{
			this.setup.search.setupCrouchedVisLayerMask();
		}
	}

	private void setVisionLayersOff()
	{
		if (this.setup.search)
		{
			this.setup.search.setupStandingVisLayerMask();
		}
	}

	private void killThisEnemy()
	{
		this.health.HitReal(1000);
	}

	private void knockDownThisEnemy()
	{
		this.health.getAttackDirection(5);
		this.health.targetSwitcher.attackerType = 4;
		this.animator.SetIntegerReflected("hurtLevelInt", 4);
		this.animator.SetTriggerReflected("damageTrigger");
		this.health.Health = 5;
		if (this.setup.pmCombat)
		{
			this.setup.pmCombat.enabled = true;
			this.setup.pmCombat.SendEvent("gotHit");
		}
	}

	public void initWakeUp()
	{
		if (this.setup.dayCycle)
		{
			this.setup.dayCycle.initWakeUp();
		}
	}

	private void setPatrolling(bool on)
	{
		if (this.setup.pmSearchScript)
		{
			this.setup.pmSearchScript._doPatrol = on;
		}
	}

	private void eatingOnSpawn(bool onoff)
	{
		if (onoff)
		{
			this.setup.pmSleep.FsmVariables.GetFsmBool("doEating").Value = true;
		}
		else
		{
			this.setup.pmSleep.FsmVariables.GetFsmBool("doEating").Value = false;
		}
	}

	private void setInstantSpawn(bool set)
	{
		if (set && !Scene.MutantControler.activeInstantSpawnedCannibals.Contains(base.gameObject))
		{
			Scene.MutantControler.activeInstantSpawnedCannibals.Add(base.gameObject);
		}
		this.instantSpawn = set;
	}

	public void getRagDollName(int name)
	{
		this.storedRagDollName = name;
	}

	private void onSoundAlert(Collider col)
	{
		if (this.animator.enabled && this.soundDetect)
		{
			this.soundDetect.OnTriggerEnter(col);
		}
	}

	public void updateWorldTransformPosition()
	{
		this.setup.search.worldPositionTr.position = base.transform.position;
	}

	private void onArtifactAttract(Transform artifact)
	{
		if (this.inCave)
		{
			return;
		}
		if (this.setup.pmSearchScript)
		{
			this.setup.pmSearchScript._lastArtifactPos = artifact.position;
			this.setup.pmSearchScript._lastArtifactTr = artifact;
			this.setup.pmSearchScript._toAttractArtifact = true;
		}
		if (this.setup.pmCombat)
		{
			this.setup.pmCombat.SendEvent("toAttract");
			this.setup.pmCombat.FsmVariables.GetFsmBool("toAttractArtifact").Value = true;
		}
		if (this.setup.search)
		{
			this.setup.search._lastArtifactPos = artifact.position;
		}
	}

	private void onArtifactRepel(Transform artifact)
	{
		if (this.inCave)
		{
			return;
		}
		if (this.setup.pmSearchScript)
		{
			this.setup.pmSearchScript._lastArtifactPos = artifact.position;
			this.setup.pmSearchScript._lastArtifactTr = artifact;
			this.setup.pmSearchScript._toRepelArtifact = true;
		}
		if (this.setup.pmCombatScript)
		{
			this.setup.pmCombatScript.toRepelArtifact = true;
		}
		if (this.setup.pmCombat)
		{
			this.setup.pmCombat.SendEvent("toRepel");
			this.setup.pmCombat.FsmVariables.GetFsmBool("toRepelArtifact").Value = true;
			this.setup.pmCombat.FsmVariables.GetFsmBool("timeOutBool").Value = true;
		}
		if (this.setup.search)
		{
			this.setup.search._lastArtifactPos = artifact.position;
		}
	}

	private mutantAI ai;

	private getAnimatorParams getParams;

	private mutantPropManager props;

	private setupBodyVariation propsFemale;

	private EnemyHealth health;

	private mutantFollowerFunctions followSetup;

	private mutantDayCycle dayCycle;

	public mutantScriptSetup setup;

	private mutantController mutantControl;

	private mutantSpawnManager spawnManager;

	private Animator animator;

	public Material creepyPaleMat;

	public Material creepyMat;

	public Material oldCreepyMat;

	public targetStats stats;

	public mutantSoundDetect soundDetect;

	public bool targetBusy;

	private bool instantSpawn;

	public spawnMutants spawner;

	public GameObject dummyMutant;

	private GameObject controlGo;

	private int waterLayer;

	public bool inWater;

	public bool inCave;

	public bool oldMutant;

	public bool bossBaby;

	private bool storeSkinnyBool;

	private bool storePaleMutantBool;

	private bool storeOnCeilingBool;

	private float storeMutantType;

	public int storedRagDollName;

	private bool canStoreDefaultParams;

	private bool removeSpawnBlock;
}
