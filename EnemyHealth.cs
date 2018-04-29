using System;
using System.Collections;
using Bolt;
using FMOD.Studio;
using HutongGames.PlayMaker;
using ModAPI;
using PathologicalGames;
using TheForest.Items.Utils;
using TheForest.Tools;
using TheForest.Utils;
using TheForest.Utils.Settings;
using UltimateCheatmenu;
using UnityEngine;


public class EnemyHealth : EntityBehaviour
{
	
	
	private float HealthPercentage
	{
		get
		{
			return (float)this.Health / this.maxHealthFloat * 100f;
		}
	}

	
	private void Awake()
	{
		this.ragDollSetup = base.GetComponent<clsragdollify>();
		this.setup = base.GetComponent<mutantScriptSetup>();
		this.typeSetup = base.transform.parent.GetComponent<mutantTypeSetup>();
		this.ai = base.GetComponent<mutantAI>();
		this.familyFunctions = base.transform.parent.GetComponent<mutantFamilyFunctions>();
		this.targetSwitcher = base.GetComponentInChildren<mutantTargetSwitching>();
		if (!this.ai.creepy)
		{
			this.animator = base.GetComponent<Animator>();
		}
		else
		{
			this.animator = base.GetComponentInChildren<Animator>();
		}
		this.MP = base.gameObject.GetComponent<mutantPropManager>();
		this.bloodPropertyBlock = new MaterialPropertyBlock();
	}

	
	private void Start()
	{
		this.paleColor = Scene.SceneTracker.paleMutantColor;
		this.regularColor = Scene.SceneTracker.regularMutantColor;
		this.deadBlock = false;
		this.damageMult = 1;
		this.douseMult = 1;
		this.maxHealth = this.Health;
		this.maxHealthFloat = (float)this.Health;
		this.targetSwitcher = base.GetComponentInChildren<mutantTargetSwitching>();
		this.teethItemId = ItemUtils.ItemIdByName("Tooth");
		this.resetSkinDamage();
		if (this.setup.pmCombat)
		{
			this.fsmDeathRecoverBool = this.setup.pmCombat.FsmVariables.GetFsmBool("deathRecoverBool");
		}
		if (this.ai.pale && !this.ai.maleSkinny)
		{
			this.recoverValue = 27;
		}
		else
		{
			this.recoverValue = 17;
		}
	}

	
	private void OnDestroy()
	{
		if (this.ai.pale && !this.ai.skinned)
		{
			this.MySkin.material.color = this.paleColor;
		}
		else
		{
			this.MySkin.material.color = this.regularColor;
		}
	}

	
	private void OnEnable()
	{
		base.InvokeRepeating("rechargeHealth", 0f, 1.5f);
		this.deadBlock = false;
		this.alreadyBurnt = false;
		this.onFire = false;
		this.doStealthKill = false;
		this.douseMult = 1;
		this.deathFromTrap = false;
		this.poisoned = false;
		if (this.animator)
		{
			this.animator.SetBoolReflected("burning", false);
		}
		if (this.setup && this.setup.pmCombat)
		{
			this.setup.pmCombat.FsmVariables.GetFsmBool("onFireBool").Value = false;
		}
		this.resetSkinDamage();
	}

	
	private void resetSkinDamage()
	{
		if (!this.MySkin)
		{
			return;
		}
		this.MySkin.GetPropertyBlock(this.bloodPropertyBlock);
		this.bloodPropertyBlock.SetFloat("_Damage1", 0f);
		this.bloodPropertyBlock.SetFloat("_Damage2", 0f);
		this.bloodPropertyBlock.SetFloat("_Damage3", 0f);
		this.bloodPropertyBlock.SetFloat("_Damage4", 0f);
		this.MySkin.SetPropertyBlock(this.bloodPropertyBlock);
		this.animator.SetFloatReflected("skinDamage1", 0f);
		this.animator.SetFloatReflected("skinDamage2", 0f);
		this.animator.SetFloatReflected("skinDamage3", 0f);
		this.animator.SetFloatReflected("skinDamage4", 0f);
	}

	
	private void OnDisable()
	{
		this.doused = false;
		this.douseMult = 1;
		this.deathFromTrap = false;
		if (this.defaultMat)
		{
			this.MySkin.sharedMaterial = this.defaultMat;
		}
		this.StopOnFireEvent();
		this.hitEventBlock = false;
		base.CancelInvoke("disableBurn");
		base.CancelInvoke("HitFire");
		base.CancelInvoke("HitPoison");
		base.CancelInvoke("goRagdollSafety");
		base.CancelInvoke("rechargeHealth");
		foreach (GameObject gameObject in this.Fire)
		{
			if (gameObject)
			{
				gameObject.SetActive(false);
			}
		}
		if (this.setup && this.setup.pmCombat)
		{
			this.setup.pmCombat.FsmVariables.GetFsmBool("onFireBool").Value = false;
		}
	}

	
	private void Update()
	{
		if (this.Health > this.recoverValue)
		{
			this.fsmDeathRecoverBool.Value = true;
		}
		else
		{
			this.fsmDeathRecoverBool.Value = false;
		}
		if (this.ai.male || this.ai.female)
		{
			this.animator.SetIntegerReflected("ClientHealth", this.Health);
		}
		if (!this.animator.enabled)
		{
			return;
		}
		this.tired = (float)this.Health / this.maxHealthFloat;
		this.tired = 1f - this.tired;
		if (this.poisoned)
		{
			this.tired = 1f;
		}
		this.smoothTired = Mathf.Lerp(this.smoothTired, this.tired, Time.deltaTime);
		if (!this.ai.creepy && !this.ai.creepy_male && !this.ai.creepy_fat && !this.ai.creepy_baby)
		{
			if (!this.ai.female && !this.ai.pale && !this.ai.maleSkinny && !this.ai.femaleSkinny)
			{
				float value = this.smoothTired * -1f;
				this.animator.SetFloatReflected("mutantType", value);
			}
			else if (this.ai.female && !this.ai.femaleSkinny)
			{
				float value2 = this.smoothTired + 1f;
				this.animator.SetFloatReflected("mutantType", value2);
			}
		}
		if (this.ai.female || this.ai.maleSkinny || this.ai.femaleSkinny || this.ai.pale)
		{
			if (this.poisoned)
			{
				this.smoothPoisoned = Mathf.Lerp(this.smoothPoisoned, 3f, Time.deltaTime);
				this.animator.SetFloatReflected("aggression", this.smoothPoisoned);
				this.resetPoisonTimer = Time.time + 3f;
			}
			else if (!this.poisoned && Time.time < this.resetPoisonTimer)
			{
				this.smoothPoisoned = Mathf.Lerp(this.smoothPoisoned, 2f, Time.deltaTime);
				this.animator.SetFloatReflected("aggression", this.smoothPoisoned);
			}
		}
		this.UpdateOnFireEvent();
	}

	
	private void UpdateOnFireEvent()
	{
		if (this.onFireEventInstance != null)
		{
			UnityUtil.ERRCHECK(this.onFireEventInstance.set3DAttributes(base.transform.to3DAttributes()));
			if (this.onFireHealthParameter != null)
			{
				UnityUtil.ERRCHECK(this.onFireHealthParameter.setValue(this.HealthPercentage));
			}
		}
	}

	
	public void addHealth()
	{
		this.Health += 20;
	}

	
	private void rechargeHealth()
	{
		if (this.animator.GetCurrentAnimatorStateInfo(0).tagHash == this.deathTag && this.Health < this.maxHealth)
		{
			this.Health++;
		}
	}

	
	public void fatCreepyHit(int damage)
	{
		this.getAttackDirection(5);
		this.targetSwitcher.attackerType = 4;
		this.animator.SetIntegerReflected("hurtLevelInt", 4);
		this.animator.SetTriggerReflected("damageTrigger");
		this.setSkinDamage(UnityEngine.Random.Range(0, 3));
		this.Health -= damage;
		if (this.Health < 1)
		{
			this.HitReal(1);
			return;
		}
		this.pmGotHit();
	}

	
	public void Explosion(float explodeDist)
	{
		if (this.ai.creepy_boss && !this.ai.girlFullyTransformed)
		{
			return;
		}
		if (!this.explodeBlock)
		{
			if (this.ai.creepy_male || this.ai.creepy || this.ai.creepy_fat || this.ai.creepy_baby || this.ai.creepy_boss)
			{
				if (this.ai.creepy_boss)
				{
					this.Health -= 30;
				}
				else
				{
					this.Health -= 65;
				}
				if (this.Burnt && this.MySkin && !this.ai.creepy_boss && explodeDist > 0f)
				{
					if (this.setup.propManager && this.setup.propManager.lowSkinnyBody)
					{
						this.setup.propManager.lowSkinnyBody.GetComponent<SkinnedMeshRenderer>().sharedMaterial = this.Burnt;
					}
					if (this.setup.propManager && this.setup.propManager.lowBody)
					{
						this.setup.propManager.lowBody.GetComponent<SkinnedMeshRenderer>().sharedMaterial = this.Burnt;
					}
					this.MySkin.sharedMaterial = this.Burnt;
					this.alreadyBurnt = true;
				}
				this.setSkinDamage(UnityEngine.Random.Range(0, 3));
				if (this.Health <= 0)
				{
					if (this.ai.creepy_boss)
					{
						this.Die();
					}
					else
					{
						this.dieExplode();
					}
				}
				else
				{
					if (UnityEngine.Random.value > 0.75f && this.ai.creepy_boss && !this.animator.GetBool("hitStagger"))
					{
						this.setup.pmCombat.FsmVariables.GetFsmBool("staggered").Value = true;
						this.animator.SetBool("hitStagger", true);
						base.Invoke("resetStagger", 10f);
					}
					this.setup.pmCombat.SendEvent("toHitExplode");
				}
			}
			else if (this.ai.male || this.ai.female)
			{
				if (explodeDist < 0f)
				{
					this.getAttackDirection(5);
					this.targetSwitcher.attackerType = 4;
					this.animator.SetIntegerReflected("hurtLevelInt", 4);
					this.animator.SetTriggerReflected("damageTrigger");
					this.setSkinDamage(UnityEngine.Random.Range(0, 3));
					this.Health -= 40;
					if (this.Health < 1)
					{
						UnityEngine.Object.Instantiate(this.RagDollExploded, base.transform.position, base.transform.rotation);
						this.typeSetup.removeFromSpawnAndExplode();
						return;
					}
					this.pmGotHit();
				}
				else if (explodeDist < 10.5f)
				{
					UnityEngine.Object.Instantiate(this.RagDollExploded, base.transform.position, base.transform.rotation);
					this.typeSetup.removeFromSpawnAndExplode();
				}
				else if (explodeDist > 10.5f && explodeDist < 18f)
				{
					this.getAttackDirection(5);
					this.targetSwitcher.attackerType = 4;
					this.animator.SetIntegerReflected("hurtLevelInt", 4);
					this.animator.SetTriggerReflected("damageTrigger");
					if (this.Burnt && this.MySkin)
					{
						if (this.setup.propManager && this.setup.propManager.lowSkinnyBody)
						{
							this.setup.propManager.lowSkinnyBody.GetComponent<SkinnedMeshRenderer>().sharedMaterial = this.Burnt;
						}
						if (this.setup.propManager && this.setup.propManager.lowBody)
						{
							this.setup.propManager.lowBody.GetComponent<SkinnedMeshRenderer>().sharedMaterial = this.Burnt;
						}
						this.MySkin.sharedMaterial = this.Burnt;
						this.alreadyBurnt = true;
					}
					this.setSkinDamage(UnityEngine.Random.Range(0, 3));
					this.Health -= 40;
					if (this.Health < 1)
					{
						this.HitReal(1);
						return;
					}
					this.pmGotHit();
				}
				else if (explodeDist > 18f && explodeDist < 25f)
				{
					this.getAttackDirection(3);
					this.targetSwitcher.attackerType = 4;
					this.animator.SetIntegerReflected("hurtLevelInt", 0);
					this.Hit(15);
				}
			}
			else
			{
				this.setup.pmCombat.enabled = true;
				this.setup.pmCombat.FsmVariables.GetFsmBool("deathFinal").Value = true;
				this.setup.pmCombat.SendEvent("toDeath");
				if (this.familyFunctions)
				{
					this.familyFunctions.cancelEatMeEvent();
					this.familyFunctions.cancelRescueEvent();
				}
				UnityEngine.Object.Instantiate(this.RagDollExploded, base.transform.position, base.transform.rotation);
				this.typeSetup.removeFromSpawnAndExplode();
			}
			this.explodeBlock = true;
		}
		base.Invoke("resetExplodeBlock", 0.1f);
	}

	
	private void pmGotHit()
	{
		if (this.setup.pmCombat)
		{
			this.setup.pmCombat.enabled = true;
			this.setup.pmCombat.SendEvent("gotHit");
		}
		if (this.setup.pmSearchScript)
		{
			base.StartCoroutine(this.setup.pmSearchScript.gotHitRoutine());
		}
		if (this.setup.pmEncounter && this.setup.pmEncounter.enabled)
		{
			this.setup.pmEncounter.SendEvent("gotHit");
		}
	}

	
	private void resetExplodeBlock()
	{
		this.explodeBlock = false;
	}

	
	public void douseEnemy()
	{
		this.doused = true;
		this.douseMult++;
		base.Invoke("resetDouse", 30f);
	}

	
	public void setFireDouse()
	{
		this.doused = true;
		this.douseMult = 2;
		base.Invoke("resetDouse", 30f);
	}

	
	public void resetDouse()
	{
		this.douseMult--;
		if (this.douseMult < 1)
		{
			this.douseMult = 1;
			this.doused = false;
		}
	}

	
	private void resetStagger()
	{
		this.setup.pmCombat.FsmVariables.GetFsmBool("staggered").Value = false;
		this.animator.SetBool("hitStagger", false);
	}

	
	public void Poison()
	{
		this.poisoned = true;
		base.CancelInvoke("HitPoison");
		base.CancelInvoke("disablePoison");
		if (this.setup.propManager)
		{
			this.setup.propManager.poisoned = true;
		}
		if (this.setup.bodyVariation)
		{
			this.setup.bodyVariation.poisoned = true;
		}
		base.InvokeRepeating("HitPoison", 3f, 3f);
		Color poisonedColor = Scene.SceneTracker.poisonedColor;
		this.MySkin.GetPropertyBlock(this.bloodPropertyBlock);
		this.bloodPropertyBlock.SetColor("_Color", poisonedColor);
		this.MySkin.SetPropertyBlock(this.bloodPropertyBlock);
		if (BoltNetwork.isRunning && BoltNetwork.isServer)
		{
			mutantPoison mutantPoison = mutantPoison.Create(GlobalTargets.AllClients);
			mutantPoison.target = base.transform.parent.GetComponent<BoltEntity>();
			mutantPoison.skinColor = poisonedColor;
			mutantPoison.effectActive = true;
			mutantPoison.Send();
		}
		if (this.setup.ai.creepy_fat)
		{
			base.Invoke("disablePoison", 20f);
		}
		else
		{
			base.Invoke("disablePoison", 30f);
		}
	}

	
	public void disablePoison()
	{
		base.CancelInvoke("HitPoison");
		this.MySkin.GetPropertyBlock(this.bloodPropertyBlock);
		if (this.setup.propManager)
		{
			this.setup.propManager.poisoned = false;
		}
		if (this.setup.bodyVariation)
		{
			this.setup.bodyVariation.poisoned = false;
		}
		Color color = Scene.SceneTracker.regularMutantColor;
		if (this.ai.pale && !this.ai.skinned)
		{
			color = Scene.SceneTracker.paleMutantColor;
		}
		this.bloodPropertyBlock.SetColor("_Color", color);
		this.MySkin.SetPropertyBlock(this.bloodPropertyBlock);
		if (BoltNetwork.isRunning && BoltNetwork.isServer)
		{
			mutantPoison mutantPoison = mutantPoison.Create(GlobalTargets.AllClients);
			mutantPoison.target = base.transform.parent.GetComponent<BoltEntity>();
			mutantPoison.skinColor = color;
			mutantPoison.effectActive = false;
			mutantPoison.Send();
		}
		this.poisoned = false;
	}

	
	public void hitFallDown(float damage)
	{
		if (this.ai.creepy_boss && !this.ai.girlFullyTransformed)
		{
			return;
		}
		this.Health -= 20;
		if (this.Health < 1)
		{
			this.HitReal(1);
			return;
		}
		this.getAttackDirection(5);
		this.targetSwitcher.attackerType = 4;
		this.setup.animator.SetIntegerReflected("randInt1", 0);
		this.setup.animator.SetIntegerReflected("hurtLevelInt", 4);
		this.setup.animator.SetTriggerReflected("damageTrigger");
		if (this.setup.pmCombat)
		{
			this.setup.pmCombat.enabled = true;
			this.setup.pmCombat.SendEvent("gotHit");
		}
		if (this.setup.pmSearchScript)
		{
			base.StartCoroutine(this.setup.pmSearchScript.gotHitRoutine());
		}
		if (this.setup.pmEncounter && this.setup.pmEncounter.enabled)
		{
			this.setup.pmEncounter.SendEvent("gotHit");
		}
	}

	
	public void Burn()
	{
		if (this.ai.creepy_boss && !this.ai.girlFullyTransformed)
		{
			return;
		}
		if (this.ai.fireman_dynamite)
		{
			base.Invoke("doBeltExplosion", 4f);
		}
		if (this.douseMult > 1)
		{
			GameStats.BurntEnemy.Invoke();
			if (this.Burnt && this.MySkin)
			{
				if (this.setup.propManager && this.setup.propManager.lowSkinnyBody)
				{
					this.setup.propManager.lowSkinnyBody.GetComponent<SkinnedMeshRenderer>().sharedMaterial = this.Burnt;
				}
				if (this.setup.propManager && this.setup.propManager.lowBody)
				{
					this.setup.propManager.lowBody.GetComponent<SkinnedMeshRenderer>().sharedMaterial = this.Burnt;
				}
				this.MySkin.sharedMaterial = this.Burnt;
				this.alreadyBurnt = true;
			}
			if (this.Fire != null && this.Fire.Length > 0)
			{
				foreach (GameObject gameObject in this.Fire)
				{
					if (gameObject)
					{
						gameObject.SetActive(true);
					}
				}
			}
			this.onFire = true;
			if (!this.ai.creepy_boss && this.setup.pmCombat)
			{
				this.setup.pmCombat.FsmVariables.GetFsmBool("onFireBool").Value = true;
			}
			this.targetSwitcher.attackerType = 4;
			int num = this.douseMult - 1;
			if (num < 1)
			{
				num = 1;
			}
			this.Hit(5 * num);
			if (this.ai.creepy_male || this.ai.creepy || this.ai.creepy_fat || this.ai.creepy_baby)
			{
				if (!this.ai.creepy_boss)
				{
					this.animator.SetIntegerReflected("randInt1", UnityEngine.Random.Range(0, 3));
					this.animator.SetBoolReflected("onFireBool", true);
					this.setup.pmCombat.SendEvent("goToOnFire");
				}
				this.StartOnFireEvent();
			}
			base.InvokeRepeating("HitFire", UnityEngine.Random.Range(1f, 2f), 1f);
			if (this.setup.ai.creepy_fat)
			{
				base.Invoke("disableBurn", 10f);
			}
			else if (this.setup.ai.fireman || this.setup.ai.creepy_boss)
			{
				base.Invoke("disableBurn", 5f);
			}
			else
			{
				base.Invoke("disableBurn", UnityEngine.Random.Range(7f, 14f));
			}
		}
		else
		{
			this.singeBurn();
		}
	}

	
	public void disableBurn()
	{
		base.CancelInvoke("HitFire");
		if (this.Fire != null && this.Fire.Length > 0)
		{
			foreach (GameObject gameObject in this.Fire)
			{
				if (gameObject)
				{
					gameObject.SetActive(false);
				}
			}
		}
		this.onFire = false;
		this.StopOnFireEvent();
		this.hitEventBlock = false;
		if (this.ai.creepy || this.ai.creepy_male || this.ai.creepy_fat)
		{
			if (this.animator.enabled)
			{
				this.animator.SetBoolReflected("onFireBool", false);
			}
		}
		else
		{
			this.setup.pmCombat.SendEvent("toBurnRecover");
			if (this.setup.pmCombatScript)
			{
				this.setup.pmCombatScript.toBurnRecover = true;
			}
			if (this.animator.enabled)
			{
				this.animator.SetBoolReflected("burning", false);
			}
		}
		this.douseMult = 1;
	}

	
	private void doBeltExplosion()
	{
	}

	
	private void StartOnFireEvent()
	{
		if (SteamDSConfig.isDedicatedServer)
		{
			return;
		}
		if (!FMOD_StudioSystem.ForceFmodOff && FMOD_StudioSystem.instance && this.onFireEventInstance == null && this.OnFireEvent != null && this.OnFireEvent.Length > 0)
		{
			this.onFireEventInstance = FMOD_StudioSystem.instance.GetEvent(this.OnFireEvent);
			if (this.onFireEventInstance != null)
			{
				this.onFireEventInstance.getParameter("health", out this.onFireHealthParameter);
				this.UpdateOnFireEvent();
				UnityUtil.ERRCHECK(this.onFireEventInstance.start());
			}
		}
	}

	
	private void StopOnFireEvent()
	{
		if (this.onFireEventInstance != null)
		{
			UnityUtil.ERRCHECK(this.onFireEventInstance.stop(STOP_MODE.ALLOWFADEOUT));
			UnityUtil.ERRCHECK(this.onFireEventInstance.release());
			this.onFireEventInstance = null;
			this.onFireHealthParameter = null;
		}
	}

	
	private void TrapDamage()
	{
	}

	
	private void singeBurn()
	{
		if (this.ai.creepy_boss && !this.ai.girlFullyTransformed)
		{
			return;
		}
		if (this.Fire != null && this.Fire.Length > 0)
		{
			foreach (GameObject gameObject in this.Fire)
			{
				if (gameObject)
				{
					gameObject.SetActive(true);
				}
			}
		}
		this.onFire = true;
		if (this.Burnt && this.MySkin)
		{
			if (this.setup.propManager && this.setup.propManager.lowSkinnyBody)
			{
				this.setup.propManager.lowSkinnyBody.GetComponent<SkinnedMeshRenderer>().sharedMaterial = this.Burnt;
			}
			if (this.setup.propManager && this.setup.propManager.lowBody)
			{
				this.setup.propManager.lowBody.GetComponent<SkinnedMeshRenderer>().sharedMaterial = this.Burnt;
			}
			this.MySkin.sharedMaterial = this.Burnt;
			this.alreadyBurnt = true;
		}
		if (this.ai.creepy_male || this.ai.creepy || this.ai.creepy_fat || this.ai.creepy_baby)
		{
			if (this.animator.enabled)
			{
				this.animator.SetBoolReflected("onFireBool", true);
			}
			this.setup.pmCombat.SendEvent("toHitExplode");
		}
		else
		{
			this.getAttackDirection(3);
			this.animator.SetIntegerReflected("randInt1", 0);
			this.targetSwitcher.attackerType = 4;
			if (this.setup.pmCombat)
			{
				this.setup.pmCombat.FsmVariables.GetFsmBool("onFireBool").Value = true;
			}
		}
		int num = this.douseMult - 1;
		if (num < 1)
		{
			num = 1;
		}
		this.Hit(5 * num);
		base.InvokeRepeating("HitFire", UnityEngine.Random.Range(1f, 1.5f), 1f);
		base.Invoke("disableBurn", 4f);
	}

	
	private void HitFire()
	{
		if (this.ai.creepy_boss && !this.ai.girlFullyTransformed)
		{
			return;
		}
		if (!this.deadBlock)
		{
			this.setSkinDamage(UnityEngine.Random.Range(0, 3));
			this.targetSwitcher.attackerType = 4;
			int num = this.douseMult - 1;
			if (num < 1)
			{
				num = 1;
			}
			if (this.ai.creepy_boss)
			{
				this.HitFireDamageOnly(Mathf.CeilToInt((float)(2 * num) * GameSettings.Ai.fireDamageCreepyRatio));
			}
			else if (this.ai.creepy || this.ai.creepy_male || this.ai.creepy_fat || this.ai.creepy_baby || this.ai.creepy_boss)
			{
				this.Hit(Mathf.CeilToInt((float)(UnityEngine.Random.Range(3, 6) * num) * GameSettings.Ai.fireDamageCreepyRatio));
			}
			else
			{
				this.Hit(Mathf.CeilToInt((float)(UnityEngine.Random.Range(4, 6) * num) * GameSettings.Ai.fireDamageRatio));
			}
		}
	}

	
	private void HitPoison()
	{
		if (this.ai.creepy_boss && !this.ai.girlFullyTransformed)
		{
			return;
		}
		if (!this.deadBlock)
		{
			this.targetSwitcher.attackerType = 4;
			int max = 4;
			int min;
			if (this.ai.creepy || this.ai.creepy_male || this.ai.creepy_fat || this.ai.creepy_baby)
			{
				min = 1;
			}
			else
			{
				min = 2;
			}
			int num = UnityEngine.Random.Range(min, max);
			this.Health -= num;
			if (this.Health < 1)
			{
				this.Die();
			}
			else if (this.onFireEventInstance == null)
			{
				FMODCommon.PlayOneshot(this.HurtEvent, base.transform.position, new object[]
				{
					"mutant_health",
					this.HealthPercentage
				});
			}
		}
	}

	
	private void hitBlockReset()
	{
		this.hitBlock = false;
	}

	
	private void HitHead()
	{
	}

	
	private void HitFireDamageOnly(int damage)
	{
		this.Health -= damage;
		if (this.Health < 1)
		{
			this.Die();
		}
		else if (this.onFireEventInstance == null)
		{
			FMODCommon.PlayOneshot(this.HurtEvent, base.transform.position, new object[]
			{
				"mutant_health",
				this.HealthPercentage
			});
		}
	}

	
	public void setSkinDamage(int d)
	{
		float num = 0f;
		this.MySkin.GetPropertyBlock(this.bloodPropertyBlock);
		if (this.ai.creepy || this.ai.creepy_fat || this.ai.creepy_male || this.ai.creepy_baby)
		{
			if (this.ai.creepy_baby)
			{
				num = 1f;
			}
			else
			{
				num = 1f - (float)this.Health / (float)this.maxHealth;
			}
			this.bloodPropertyBlock.SetFloat("_Damage1", num);
			this.bloodPropertyBlock.SetFloat("_Damage2", num);
			this.bloodPropertyBlock.SetFloat("_Damage3", num);
			this.bloodPropertyBlock.SetFloat("_Damage4", num);
			this.animator.SetFloatReflected("skinDamage1", num);
			this.animator.SetFloatReflected("skinDamage2", num);
			this.animator.SetFloatReflected("skinDamage3", num);
			this.animator.SetFloatReflected("skinDamage4", num);
		}
		else
		{
			if (d == 0)
			{
				num = this.bloodPropertyBlock.GetFloat("_Damage1");
				if (num < 1f)
				{
					num += 0.5f;
					this.bloodPropertyBlock.SetFloat("_Damage1", num);
					this.animator.SetFloatReflected("skinDamage1", num);
				}
			}
			else if (d == 1)
			{
				num = this.bloodPropertyBlock.GetFloat("_Damage3");
				if (num < 1f)
				{
					num += 0.5f;
					this.bloodPropertyBlock.SetFloat("_Damage3", num);
					this.animator.SetFloatReflected("skinDamage3", num);
				}
			}
			else
			{
				this.bloodPropertyBlock.GetFloat("_Damage2");
				if (num < 1f)
				{
					num += 0.5f;
					this.bloodPropertyBlock.SetFloat("_Damage2", num);
					this.animator.SetFloatReflected("skinDamage2", num);
				}
			}
			num = this.bloodPropertyBlock.GetFloat("_Damage4");
			if (num < 1f)
			{
				num += 0.5f;
				this.bloodPropertyBlock.SetFloat("_Damage4", num);
				this.animator.SetFloatReflected("skinDamage4", num);
			}
		}
		this.MySkin.SetPropertyBlock(this.bloodPropertyBlock);
	}

	
	private void disableHeadDamage()
	{
		if (this.animator.enabled)
		{
			this.animator.SetBoolReflected("headDamageBool", false);
		}
		this.damageMult = 1;
	}

	
	public void getStealthAttack()
	{
		if (!this.setup.ai.creepy && !this.setup.ai.creepy_baby && !this.setup.ai.creepy_male && !this.setup.ai.creepy_fat)
		{
			if (this.hitDir == 1 && this.setup.search.lookingForTarget)
			{
				this.doStealthKill = true;
			}
			else
			{
				this.doStealthKill = false;
			}
		}
	}

	
	public void __Hit__Original(int damage)
	{
		if ((this.setup.ai.male || this.setup.ai.female) && !this.setup.ai.maleSkinny && !this.setup.ai.femaleSkinny && this.targetSwitcher.attackerType == 0)
		{
			return;
		}
		if (this.ai.creepy_boss && !this.ai.girlFullyTransformed)
		{
			return;
		}
		this.HitReal(damage);
	}

	
	public void HitReal(int damage)
	{
		if (this.hitBlock || this.deadBlock)
		{
			return;
		}
		if (this.ai.creepy_boss && !this.ai.girlFullyTransformed)
		{
			return;
		}
		if (!base.enabled)
		{
			return;
		}
		if (this.ai.creepy || this.ai.creepy_male || this.ai.creepy_fat || this.ai.creepy_baby || this.ai.creepy_boss)
		{
			this.Health -= damage;
			if (this.setup.pmCombat)
			{
				if (this.ai.creepy_boss)
				{
					if (this.animator.GetInteger("hitDirection") != 3 && this.animator.GetCurrentAnimatorStateInfo(0).tagHash != this.gettingHitTag)
					{
						if (this.hitLimb == 1)
						{
							this.animator.SetBool("hitLeg", true);
						}
						else if (this.hitLimb == 2)
						{
							this.animator.SetBool("hitWomb", true);
						}
						else
						{
							this.animator.SetBool("hitHead", true);
						}
					}
					base.Invoke("resetBossHits", 0.5f);
				}
				this.setup.pmCombat.FsmVariables.GetFsmBool("gettingHit").Value = true;
				if (this.ai.creepy && !this.animator.GetBool("rearUpBool"))
				{
					this.setup.pmCombat.SendEvent("gotHit");
					base.Invoke("resetGettingHit", 1.3f);
				}
				else
				{
					this.setup.pmCombat.SendEvent("gotHit");
					base.Invoke("resetGettingHit", 1.3f);
				}
			}
			if (this.Health < 1)
			{
				this.Die();
			}
			else if (this.onFireEventInstance == null)
			{
				FMODCommon.PlayOneshot(this.HurtEvent, base.transform.position, new object[]
				{
					"mutant_health",
					this.HealthPercentage
				});
			}
			return;
		}
		if (this.targetSwitcher)
		{
			this.targetSwitcher.attackerType = 4;
		}
		this.hitBlock = true;
		base.Invoke("hitBlockReset", 0.25f);
		if (this.animator.GetBool("deathBOOL"))
		{
			this.damageMult = 2;
		}
		else
		{
			this.damageMult = 1;
		}
		if (this.animator.GetBool("sleepBOOL"))
		{
			this.Health -= 100;
		}
		else
		{
			this.Health -= damage * this.damageMult;
		}
		if (this.Health >= 45)
		{
			this.animator.SetIntegerReflected("hurtLevelInt", 1);
		}
		if (this.Health < 45 && this.Health >= 30)
		{
			this.animator.SetIntegerReflected("hurtLevelInt", 2);
		}
		if (this.Health < 30 && this.Health >= 1 && this.animator)
		{
			this.animator.SetIntegerReflected("hurtLevelInt", 3);
		}
		base.Invoke("setMpRandInt", 1f);
		if (this.Health < 1)
		{
			if (this.animator)
			{
				this.animator.SetIntegerReflected("hurtLevelInt", 4);
			}
			this.setup.pmCombat.enabled = true;
			this.setup.pmCombat.FsmVariables.GetFsmBool("deathFinal").Value = true;
			if (this.onFire)
			{
				this.animator.SetBoolReflected("burning", true);
				this.animator.SetBoolReflected("deathBOOL", true);
			}
			if (this.animator)
			{
				this.animator.SetBoolReflected("deathfinalBOOL", true);
			}
			if (!this.doStealthKill && !this.onFire && this.animator)
			{
				this.animator.SetIntegerReflected("randInt1", UnityEngine.Random.Range(0, 8));
				if (!this.animator.GetBool("treeBOOL"))
				{
					this.animator.SetTriggerReflected("deathTrigger");
				}
			}
			this.Die();
		}
		else if (this.hitDir == 1)
		{
			if (!this.setup.ai.creepy && !this.setup.ai.creepy_male && !this.setup.ai.creepy_fat && !this.setup.ai.creepy_baby && this.setup.search.lookingForTarget && this.doStealthKill)
			{
				this.Die();
			}
			else
			{
				if (this.animator)
				{
					if (this.onFire && !this.hitEventBlock)
					{
						this.animator.SetBoolReflected("burning", true);
						if (!this.animator.GetBool("trapBool"))
						{
							this.animator.SetTriggerReflected("damageTrigger");
						}
					}
					else if (!this.hitEventBlock)
					{
						this.animator.SetTriggerReflected("damageBehindTrigger");
						this.animator.SetBoolReflected("burning", false);
					}
				}
				if (this.setup.pmCombat)
				{
					if (this.onFire && !this.hitEventBlock)
					{
						this.setup.pmCombat.enabled = true;
						this.setup.pmCombat.FsmVariables.GetFsmBool("onFireBool").Value = true;
						this.setup.pmCombat.SendEvent("gotHit");
						this.hitEventBlock = true;
					}
					else if (!this.onFire)
					{
						this.setup.pmCombat.SendEvent("gotHit");
						this.setup.pmCombat.FsmVariables.GetFsmBool("onFireBool").Value = false;
					}
				}
				if (this.setup.pmSearchScript)
				{
					base.StartCoroutine(this.setup.pmSearchScript.gotHitRoutine());
				}
				if (this.setup.pmEncounter && this.setup.pmEncounter.enabled)
				{
					this.setup.pmEncounter.SendEvent("gotHit");
				}
			}
		}
		else if (this.hitDir == 0)
		{
			this.doStealthKill = false;
			if (this.animator)
			{
				if (this.simplifyHitsForMp || BoltNetwork.isRunning)
				{
					if (!this.hitEventBlock)
					{
						if (this.onFire)
						{
							this.animator.SetBoolReflected("burning", true);
						}
						this.animator.SetTriggerReflected("simpleHitTrigger");
					}
				}
				else if (this.onFire && !this.hitEventBlock)
				{
					this.animator.SetBoolReflected("burning", true);
					if (!this.animator.GetBool("trapBool"))
					{
						this.animator.SetTriggerReflected("damageTrigger");
					}
				}
				else if (!this.hitEventBlock)
				{
					this.animator.SetBoolReflected("burning", false);
					this.animator.SetTriggerReflected("damageTrigger");
				}
			}
			if (this.setup.pmCombat)
			{
				this.setup.pmCombat.enabled = true;
				if (this.onFire && !this.hitEventBlock)
				{
					this.setup.pmCombat.enabled = true;
					this.setup.pmCombat.FsmVariables.GetFsmBool("onFireBool").Value = true;
					this.setup.pmCombat.SendEvent("gotHit");
					this.hitEventBlock = true;
				}
				else if (!this.onFire)
				{
					this.setup.pmCombat.SendEvent("gotHit");
					this.setup.pmCombat.FsmVariables.GetFsmBool("onFireBool").Value = false;
				}
			}
			if (this.setup.pmSearchScript)
			{
				base.StartCoroutine(this.setup.pmSearchScript.gotHitRoutine());
			}
			if (this.setup.pmEncounter && this.setup.pmEncounter.enabled)
			{
				this.setup.pmEncounter.SendEvent("gotHit");
			}
		}
		this.Blood();
		this.RandomSplurt = UnityEngine.Random.Range(0, 10);
		if (!this.ai.creepy && !this.ai.creepy_male && !this.ai.maleSkinny && !this.ai.femaleSkinny && !this.ai.pale && !this.ai.creepy_fat && !this.ai.creepy_baby && !this.alreadyBurnt && this.RandomSplurt == 2)
		{
			if (this.MP.MyRandom != 0 || this.MySkin)
			{
			}
			if (this.MP.MyRandom != 1 || this.MySkin)
			{
			}
			if (this.MP.MyRandom != 2 || this.MySkin)
			{
			}
			if (this.MP.MyRandom != 3 || this.MySkin)
			{
			}
			if (EnemyHealth.CurrentAttacker)
			{
				SendMessageEvent sendMessageEvent = SendMessageEvent.Raise(EnemyHealth.CurrentAttacker, EntityTargets.OnlyOwner);
				sendMessageEvent.Message = "GotBloody";
				sendMessageEvent.Send();
			}
			else
			{
				LocalPlayer.GameObject.SendMessage("GotBloody", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	
	public void resetPredictionBool()
	{
		this.animator.SetBoolReflected("ClientPredictionBool", false);
	}

	
	public void getAttackDirection(int hitDir)
	{
		if (!this.ai.creepy_male && !this.setup.ai.creepy_fat && !this.setup.ai.creepy_baby && !this.ai.creepy)
		{
			this.animator.SetIntegerReflected("hitDirection", hitDir);
		}
		if (this.ai.creepy_boss)
		{
			this.animator.SetInteger("hitDirection", hitDir);
		}
	}

	
	public void getCombo(int combo)
	{
		if (!this.ai.creepy_male && !this.ai.creepy_baby && !this.ai.creepy_fat && !this.ai.creepy)
		{
			this.animator.SetIntegerReflected("hitCombo", combo);
		}
	}

	
	public void takeDamage(int direction)
	{
		this.hitDir = direction;
	}

	
	private void setMpRandInt()
	{
	}

	
	public void Die()
	{
		EventRegistry.Enemy.Publish(TfEvent.KilledEnemy, this);
		Scene.SceneTracker.addToRecentlyKilledEnemies();
		if (this.setup.ai.creepy || this.setup.ai.creepy_male || this.setup.ai.creepy_baby || this.setup.ai.creepy_fat)
		{
			base.SendMessage("stopBossFightMusic", SendMessageOptions.DontRequireReceiver);
			if (this.ai.creepy_boss)
			{
			}
			base.CancelInvoke("HitFire");
			base.CancelInvoke("HitPoison");
			base.CancelInvoke("disablePoison");
			this.deadBlock = true;
			this.douseMult = 1;
			if (this.onFire)
			{
				this.ragDollSetup.burning = true;
			}
			if (this.alreadyBurnt)
			{
				this.ragDollSetup.alreadyBurnt = true;
			}
			if (this.onFire && !string.IsNullOrEmpty(this.DieByFireEvent))
			{
				FMODCommon.PlayOneshot(this.DieByFireEvent, base.transform);
			}
			else
			{
				FMODCommon.PlayOneshot(this.DieEvent, base.transform);
			}
			this.typeSetup.removeFromSpawn();
			if (this.ai.creepy_fat || this.ai.creepy_male)
			{
				this.animator.SetBoolReflected("deathBool", true);
				base.Invoke("goRagdollSafety", 6f);
				return;
			}
			if (this.ai.creepy_baby)
			{
				if (this.onFire)
				{
					return;
				}
				this.ragDollSetup.spinRagdoll = true;
				if (this.targetSwitcher.currentAttackerGo)
				{
					this.ragDollSetup.hitTr = this.targetSwitcher.currentAttackerGo.transform;
					this.ragDollSetup.metgoragdoll(this.targetSwitcher.currentAttackerGo.transform.forward * 10f);
				}
				else
				{
					this.ragDollSetup.metgoragdoll(default(Vector3));
				}
			}
			else
			{
				this.ragDollSetup.metgoragdoll(default(Vector3));
			}
			if (PoolManager.Pools["enemies"].IsSpawned(base.transform.parent))
			{
				PoolManager.Pools["enemies"].Despawn(base.transform.parent);
			}
			else
			{
				UnityEngine.Object.Destroy(base.transform.root.gameObject);
			}
		}
		else
		{
			base.CancelInvoke("HitFire");
			base.CancelInvoke("HitPoison");
			base.CancelInvoke("disablePoison");
			this.animator.SetBoolReflected("attackBOOL", false);
			this.deadBlock = true;
			this.douseMult = 1;
			if (this.doStealthKill && !this.animator.GetBool("trapBool"))
			{
				this.setup.pmCombat.FsmVariables.GetFsmBool("stealthKillBool").Value = true;
				this.animator.SetBoolReflected("stealthDeathBool", true);
			}
			this.setup.pmCombat.enabled = true;
			this.setup.pmCombat.FsmVariables.GetFsmBool("deathFinal").Value = true;
			this.setup.pmCombat.SendEvent("toDeath");
			this.setup.pmBrain.SendEvent("toDeath");
			if (this.setup.pmEncounter)
			{
				this.setup.pmEncounter.SendEvent("toDeath");
			}
			this.doStealthKill = false;
		}
		if (this.ai.girlFullyTransformed)
		{
			if (LocalPlayer.Stats)
			{
				LocalPlayer.Stats.IsFightingBoss = false;
			}
			if (BoltNetwork.isServer)
			{
				setupEndBoss setupEndBoss = setupEndBoss.Create(GlobalTargets.Everyone);
				setupEndBoss.bossActive = 2;
				setupEndBoss.Send();
			}
		}
	}

	
	private void goRagdollSafety()
	{
		this.ragDollSetup.metgoragdoll(default(Vector3));
		if (PoolManager.Pools["enemies"].IsSpawned(base.transform.parent))
		{
			PoolManager.Pools["enemies"].Despawn(base.transform.parent);
		}
		else
		{
			UnityEngine.Object.Destroy(base.transform.root.gameObject);
		}
	}

	
	private void dieExplode()
	{
		EventRegistry.Enemy.Publish(TfEvent.KilledEnemy, this);
		Scene.SceneTracker.addToRecentlyKilledEnemies();
		GameObject gameObject = UnityEngine.Object.Instantiate(this.RagDollExploded, base.transform.position, base.transform.rotation) as GameObject;
		gameObject.SendMessage("setSkin", this.MySkin.sharedMaterial, SendMessageOptions.DontRequireReceiver);
		this.typeSetup.removeFromSpawnAndExplode();
	}

	
	private void HitAxe()
	{
		this.Blood();
		if (this.Health >= -20 || this.animator.GetBool("deathfinalBOOL"))
		{
		}
		if (EnemyHealth.CurrentAttacker)
		{
			SendMessageEvent sendMessageEvent = SendMessageEvent.Raise(EnemyHealth.CurrentAttacker, EntityTargets.OnlyOwner);
			sendMessageEvent.Message = "GotBloody";
			sendMessageEvent.Send();
		}
		else
		{
			LocalPlayer.GameObject.SendMessage("GotBloody");
		}
	}

	
	private void BitShark()
	{
		this.Health--;
		this.Blood();
	}

	
	private void Blood()
	{
		if (this.BloodSplat)
		{
			base.Invoke("BloodActual", 0.1f);
		}
		if (BoltNetwork.isRunning && this.entity && this.entity.isAttached)
		{
			FxEnemeyBlood.Raise(this.entity).Send();
		}
	}

	
	public void BloodActual()
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		int num = 0;
		float angle = 90f;
		if ((this.ai.male || this.ai.female) && this.animator.GetInteger("hitDirection") == 1)
		{
			angle = -90f;
		}
		Quaternion quaternion = Quaternion.LookRotation(LocalPlayer.Transform.forward, Vector3.up);
		quaternion *= Quaternion.AngleAxis(angle, Vector3.up);
		Transform tr = PoolManager.Pools["Particles"].Spawn(Prefabs.Instance.BloodHitPSPrefabs[num].transform, this.BloodPos.position, quaternion);
		Transform tr2 = PoolManager.Pools["Particles"].Spawn(Prefabs.Instance.BloodHitPSPrefabs[1].transform, this.BloodPos.position, quaternion);
		base.StartCoroutine(this.fixBloodPosition(tr));
		base.StartCoroutine(this.fixBloodPosition(tr2));
		if (UnityEngine.Random.value > 0.5f && this.toothPickup != null && Time.time > this.teethCoolDown)
		{
			this.teethCoolDown = Time.time + 4f;
			GameObject gameObject = ItemUtils.SpawnItem(this.teethItemId, this.BloodPos.position, base.transform.rotation, false);
			if (gameObject)
			{
				gameObject.GetComponent<Rigidbody>().AddForce((float)UnityEngine.Random.Range(-100, 100), 100f, (float)UnityEngine.Random.Range(-100, 100));
				gameObject.GetComponent<Rigidbody>().AddTorque((float)UnityEngine.Random.Range(-100, 100), 100f, (float)UnityEngine.Random.Range(-100, 100));
			}
		}
	}

	
	private IEnumerator fixBloodPosition(Transform tr)
	{
		float t = 0f;
		while (t < 0.2f)
		{
			tr.position = this.BloodPos.position;
			t += Time.deltaTime;
			yield return null;
		}
		yield break;
	}

	
	private void removeBlood()
	{
	}

	
	public void getCurrentHealth()
	{
		this.setup.pmCombat.FsmVariables.GetFsmInt("statHealth").Value = this.Health;
	}

	
	public void checkDeathState()
	{
		if (!this.ai.creepy_male && !this.ai.creepy_baby && !this.ai.creepy_fat && !this.ai.creepy && this.animator.GetInteger("hitCombo") == 3 && this.animator.GetInteger("hurtLevelInt") > 2)
		{
			this.setup.pmCombat.SendEvent("toDeath");
		}
	}

	
	private void resetBossHits()
	{
		if (this.ai.creepy_boss)
		{
			this.animator.ResetTrigger("hitGroundTrigger");
			this.animator.SetBool("hitStagger", false);
			this.animator.SetBool("hitLeg", false);
			this.animator.SetBool("hitWomb", false);
			this.animator.SetBool("hitHead", false);
		}
	}

	
	private void setTrapped(GameObject go)
	{
		if (this.setup.pmCombat)
		{
			this.setup.pmCombat.SendEvent("toTrapped");
		}
		Vector3 position = go.transform.position;
		position.y = base.transform.position.y;
		base.transform.parent.transform.position = position;
		base.transform.rotation = go.transform.rotation;
	}

	
	public void releaseFromTrap()
	{
		if (this.setup.pmCombat)
		{
			this.setup.pmCombat.SendEvent("toTrapReset");
		}
	}

	
	private void DieTrap(int type)
	{
		if (this.deathFromTrap)
		{
			return;
		}
		if (!this.ai.creepy && !this.ai.creepy_male && !this.ai.creepy_baby && !this.ai.creepy_fat)
		{
			this.deathFromTrap = true;
			base.Invoke("resetDeathFromTrap", 2f);
			if (type == 0)
			{
				this.animator.SetIntegerReflected("trapTypeInt1", 0);
			}
			if (type == 1)
			{
				this.animator.SetIntegerReflected("trapTypeInt1", 1);
			}
			if (type == 2)
			{
				this.animator.SetIntegerReflected("trapTypeInt1", 2);
			}
			if (type == 3)
			{
				FMODCommon.PlayOneshotNetworked("event:/combat/damage/body_impact", base.transform, FMODCommon.NetworkRole.Server);
				this.Hit(100);
			}
			else
			{
				this.animator.SetBoolReflected("trapBool", true);
				if (type == 2)
				{
					this.animator.SetBoolReflected("enterTrapBool", true);
					this.animator.SetBoolReflected("deathBOOL", true);
				}
				else if (!this.animator.GetBool("treeBOOL"))
				{
					this.deadBlock = true;
					this.animator.SetTriggerReflected("deathTrigger");
				}
				Scene.SceneTracker.addToRecentlyKilledEnemies();
				EventRegistry.Enemy.Publish(TfEvent.KilledEnemy, this);
				EventRegistry.Achievements.Publish(TfEvent.Achievements.EnemyTrapKill, null);
				if (type == 2)
				{
					this.setup.pmBrain.SendEvent("toDeathTrapNoose");
				}
				else
				{
					this.setup.pmBrain.SendEvent("toDeathTrap");
				}
			}
		}
		else if (type == 3)
		{
			this.Hit(65);
		}
	}

	
	private void resetDeathFromTrap()
	{
		this.deathFromTrap = false;
	}

	
	private void setCurrentTrap(GameObject getTrap)
	{
		this.trapGo = getTrap;
	}

	
	private void setInNooseTrap(GameObject parent)
	{
		if (!this.ai.creepy && !this.ai.creepy_male && !this.ai.creepy_fat && !this.ai.creepy_baby)
		{
			this.trapParent = parent;
			this.setup.bodyCollisionCollider.enabled = false;
		}
	}

	
	private void setFootPivot(GameObject pivot)
	{
		if (!this.ai.creepy && !this.ai.creepy_male && !this.ai.creepy_fat && !this.ai.creepy_baby)
		{
			this.setup.pmEncounter.FsmVariables.GetFsmGameObject("footPivotGo").Value = pivot;
		}
	}

	
	private void setTrapLookat(GameObject go)
	{
		if (!this.ai.creepy && !this.ai.creepy_male && !this.ai.creepy_fat && !this.ai.creepy_baby)
		{
			this.setup.pmBrain.FsmVariables.GetFsmVector3("trapLookatDir").Value = go.transform.forward;
		}
	}

	
	private void enableFeedingEffect()
	{
	}

	
	private void disableFeedingEffect()
	{
		this.TorsoBlood.SetActive(false);
	}

	
	public void removeAllFeedingEffect()
	{
		this.EatenTorso.SetActive(false);
		this.TorsoBlood.SetActive(false);
	}

	
	public void enableBodyCollider()
	{
		this.setup.bodyCollisionCollider.enabled = true;
	}

	
	public void enableBodyColliderGo()
	{
		this.setup.bodyCollider.SetActive(true);
		if (this.setup.headColliderGo)
		{
			this.setup.headColliderGo.SetActive(true);
		}
	}

	
	public void disableBodyColliderGo()
	{
		this.setup.bodyCollider.SetActive(false);
		if (this.setup.headColliderGo)
		{
			this.setup.headColliderGo.SetActive(false);
		}
		base.Invoke("enableBodyColliderGo", 5f);
	}

	
	private void resetGettingHit()
	{
		this.setup.pmCombat.FsmVariables.GetFsmBool("gettingHit").Value = false;
	}

	
	public void Hit(int damage)
	{
		try
		{
			if (UCheatmenu.InstaKill)
			{
				this.Die();
				return;
			}
			this.HitReal(damage);
		}
		catch (Exception ex)
		{
			Log.Write("Exception thrown: " + ex.ToString(), "UltimateCheatmenu");
			this.__Hit__Original(damage);
		}
	}

	
	public Color paleColor;

	
	public Color regularColor;

	
	private PlayMakerFSM pmBase;

	
	private PlayMakerFSM pmSearch;

	
	private PlayMakerFSM pmSleep;

	
	public GameObject clubPickup;

	
	public GameObject EatenTorso;

	
	public GameObject TorsoBlood;

	
	public Transform toothPickup;

	
	private FsmBool fsmLookingForTarget;

	
	public Transform bloodAngleTr;

	
	public Renderer MySkin;

	
	private bool deadBlock;

	
	private bool explodeBlock;

	
	public GameObject ClubTrigger;

	
	public GameObject hips;

	
	public GameObject trapParent;

	
	private mutantScriptSetup setup;

	
	private mutantTypeSetup typeSetup;

	
	private mutantAI ai;

	
	private clsragdollify ragDollSetup;

	
	private mutantFamilyFunctions familyFunctions;

	
	public mutantTargetSwitching targetSwitcher;

	
	public bool simplifyHitsForMp;

	
	public int Health;

	
	public int maxHealth;

	
	public int recoverValue;

	
	public float maxHealthFloat;

	
	public GameObject[] Fire;

	
	public GameObject RagDollExploded;

	
	private int RandomSplurt;

	
	private float tired;

	
	private int hitDir;

	
	private Vector3 trapLookatDir;

	
	public GameObject trapGo;

	
	public bool hitBlock;

	
	private int damageMult;

	
	private bool doStealthKill;

	
	public bool onFire;

	
	private bool hitEventBlock;

	
	public bool alreadyBurnt;

	
	private bool doused;

	
	private int douseMult = 1;

	
	public int lastHitDir;

	
	private int deathTag = Animator.StringToHash("death");

	
	private int staggerTag = Animator.StringToHash("staggered");

	
	private int gettingHitTag = Animator.StringToHash("gettingHit");

	
	public bool poisoned;

	
	public int hitLimb;

	
	private bool deathFromTrap;

	
	private float teethCoolDown;

	
	private int teethItemId;

	
	private GameObject AiControl;

	
	private Animator animator;

	
	private float smoothTired;

	
	private float smoothPoisoned = 2f;

	
	private float resetPoisonTimer;

	
	public GameObject MyBody;

	
	public GameObject BloodSplat;

	
	public Transform BloodPos;

	
	public Material defaultMat;

	
	public Material Burnt;

	
	private FsmBool fsmDeathRecoverBool;

	
	[Header("FMOD Events (played for creepy mutants)")]
	public string OnFireEvent;

	
	public string HurtEvent;

	
	public string DieEvent;

	
	public string DieByFireEvent;

	
	private mutantPropManager MP;

	
	private EventInstance onFireEventInstance;

	
	private ParameterInstance onFireHealthParameter;

	
	private MaterialPropertyBlock bloodPropertyBlock;

	
	public static BoltEntity CurrentAttacker;
}
