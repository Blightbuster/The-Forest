using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using HutongGames.PlayMaker;
using PathologicalGames;
using TheForest.Audio;
using TheForest.Buildings.World;
using TheForest.Items.Inventory;
using TheForest.Items.World;
using TheForest.Player;
using TheForest.Utils;
using TheForest.World;
using UnityEngine;

[DoNotSerializePublic]
public class weaponInfo : EntityEventListener
{
	public Fish MyFish { get; private set; }

	public float baseWeaponDamage { get; private set; }

	public float baseWeaponSpeed { get; private set; }

	public float baseSmashDamage { get; private set; }

	public float baseTiredSpeed { get; private set; }

	public float baseStaminaDrain { get; private set; }

	public float baseSoundDetectRange { get; private set; }

	public float baseWeaponRange { get; private set; }

	public float WeaponDamage
	{
		get
		{
			float num = 1f + LocalPlayer.Stats.PhysicalStrength.CurrentStrengthScaled / 140f;
			if (LocalPlayer.Stats.IsHealthInGreyZone)
			{
				return this.weaponDamage * 1.3f * num;
			}
			return this.weaponDamage * num;
		}
	}

	public int DamageAmount
	{
		get
		{
			if (LocalPlayer.Stats.IsHealthInGreyZone)
			{
				return Mathf.RoundToInt((float)this.damageAmount * 1.3f);
			}
			return this.damageAmount;
		}
	}

	private void PlayEvent(string path, Transform t = null)
	{
		if (!string.IsNullOrEmpty(path))
		{
			if (t == null)
			{
				t = base.transform;
			}
			if (FMOD_StudioSystem.instance)
			{
				FMOD_StudioSystem.instance.PlayOneShot(path, t.position, null);
			}
		}
	}

	public override void OnEvent(SfxPlantHit evnt)
	{
		this.PlayEvent(this.plantHitEvent, null);
	}

	private void Awake()
	{
		this.baseWeaponDamage = this.weaponDamage;
		this.baseWeaponSpeed = this.weaponSpeed;
		this.baseSmashDamage = this.smashDamage;
		this.baseTiredSpeed = this.tiredSpeed;
		this.baseStaminaDrain = this.staminaDrain;
		this.baseSoundDetectRange = this.soundDetectRange;
		this.baseWeaponRange = this.weaponRange;
		if (BoltNetwork.isRunning && !this.setup)
		{
			return;
		}
		base.enabled = false;
		base.StartCoroutine(this.DelayedAwake());
	}

	private IEnumerator DelayedAwake()
	{
		yield return YieldPresets.WaitForFixedUpdate;
		base.enabled = true;
		this.Player = LocalPlayer.GameObject;
		this.damageAmount = 2;
		this.activeBool = true;
		if (!this.mainTrigger && !this.remotePlayer)
		{
			this.mainTriggerScript = LocalPlayer.MainCam.GetComponentInChildren<weaponInfo>();
			this.setupMainTrigger();
			this.setupHeldWeapon();
		}
		if (!base.GetComponent<QuickSelectViewClearOut>())
		{
			base.gameObject.AddComponent<QuickSelectViewClearOut>();
		}
		yield break;
	}

	private void Start()
	{
		if (!this.setup && base.GetComponent<EmptyObjectIdentifier>())
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		if (this.setup && this.setup.pmControl)
		{
			this.fsmJumpAttackBool = this.setup.pmControl.FsmVariables.GetFsmBool("doingJumpAttack");
			this.fsmHeavyAttackBool = this.setup.pmControl.FsmVariables.GetFsmBool("heavyAttackBool");
		}
		if (this.soundDetectGo)
		{
			this.soundCollider = this.soundDetectGo.GetComponent<SphereCollider>();
			this.soundDetectRangeInit = this.soundCollider.radius;
		}
		if (this.spear)
		{
			if (base.transform && base.transform.parent && base.transform.parent.Find("SpearTip"))
			{
				this.SpearTip = base.transform.parent.Find("SpearTip").transform;
			}
			if (base.transform && base.transform.parent && base.transform.parent.Find("SpearTip2"))
			{
				this.SpearTip2 = base.transform.parent.Find("SpearTip2").transform;
			}
		}
		this.weaponAudio = base.transform;
		this.currentWeaponScript = base.transform.GetComponent<weaponInfo>();
	}

	private void setupMainTrigger()
	{
		if (this.mainTriggerScript)
		{
			if (this.stick)
			{
				this.mainTriggerScript.stick = true;
			}
			else
			{
				this.mainTriggerScript.stick = false;
			}
			if (this.axe)
			{
				this.mainTriggerScript.axe = true;
			}
			else
			{
				this.mainTriggerScript.axe = false;
			}
			if (this.rock)
			{
				this.mainTriggerScript.rock = true;
			}
			else
			{
				this.mainTriggerScript.rock = false;
			}
			if (this.fireStick)
			{
				this.mainTriggerScript.fireStick = true;
			}
			else
			{
				this.mainTriggerScript.fireStick = false;
			}
			if (this.spear)
			{
				this.mainTriggerScript.spear = true;
			}
			else
			{
				this.mainTriggerScript.spear = false;
			}
			if (this.shell)
			{
				this.mainTriggerScript.spear = true;
			}
			else
			{
				this.mainTriggerScript.shell = false;
			}
			if (this.chainSaw)
			{
				this.mainTriggerScript.chainSaw = true;
			}
			else
			{
				this.mainTriggerScript.chainSaw = false;
			}
			if (this.machete)
			{
				this.mainTriggerScript.machete = true;
			}
			else
			{
				this.mainTriggerScript.machete = false;
			}
			this.mainTriggerScript.repairTool = this.repairTool;
			this.mainTriggerScript.weaponDamage = this.WeaponDamage;
			this.mainTriggerScript.smashDamage = this.smashDamage;
			this.mainTriggerScript.smallAxe = this.smallAxe;
			if (this.weaponRange == 0f)
			{
				this.mainTriggerScript.hitTriggerRange.transform.localScale = new Vector3(1f, 1f, 1f);
			}
			else if (BoltNetwork.isClient)
			{
				this.mainTriggerScript.hitTriggerRange.transform.localScale = new Vector3(1f, 1f, Mathf.Clamp(this.weaponRange, 1f, this.weaponRange));
			}
			else
			{
				this.mainTriggerScript.hitTriggerRange.transform.localScale = new Vector3(1f, 1f, this.weaponRange);
			}
		}
	}

	public void enableSpecialWeaponVars()
	{
		if (this.setup && this.setup.pmControl)
		{
			this.setup.pmControl.FsmVariables.GetFsmBool("groundAxeChop").Value = true;
		}
		if (this.animator)
		{
			this.animator.SetBoolReflected("groundAxeChopBool", true);
		}
	}

	public void disableSpecialWeaponVars()
	{
		if (this.setup && this.setup.pmControl)
		{
			this.setup.pmControl.FsmVariables.GetFsmBool("groundAxeChop").Value = false;
		}
		if (this.animator)
		{
			this.animator.SetBoolReflected("groundAxeChopBool", false);
		}
	}

	private void resetConnectFloat()
	{
		this.animator.SetFloatReflected("connectFloat", 0f);
	}

	private void resetEnemyDelay()
	{
		this.enemyDelay = false;
	}

	private void resetMyFish()
	{
		this.MyFish = null;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player") || this.animator.GetCurrentAnimatorStateInfo(2).tagHash == this.animControl.deathHash)
		{
			return;
		}
		if (this.currentWeaponScript == null)
		{
			return;
		}
		if (other.CompareTag("hanging") || other.CompareTag("corpseProp"))
		{
			if (this.animControl.smashBool)
			{
				if (LocalPlayer.Animator.GetFloat("tiredFloat") < 0.35f)
				{
					base.Invoke("spawnSmashWeaponBlood", 0.1f);
				}
				else
				{
					base.Invoke("spawnSmashWeaponBlood", 0.03f);
				}
			}
			else
			{
				this.spawnWeaponBlood(other, false);
			}
			Mood.HitRumble();
			other.gameObject.SendMessageUpwards("Hit", 0, SendMessageOptions.DontRequireReceiver);
			this.FauxMpHit(0);
			FMODCommon.PlayOneshotNetworked(this.currentWeaponScript.fleshHitEvent, base.transform, FMODCommon.NetworkRole.Any);
		}
		if (!ForestVR.Enabled && weaponInfo.GetInvalidAttackAngle(other))
		{
			return;
		}
		PlayerHitEnemy playerHitEnemy = null;
		if ((this.mainTrigger || (ForestVR.Enabled && !this.mainTrigger)) && this.repairTool)
		{
			RepairTool component = this.currentWeaponScript.gameObject.GetComponent<RepairTool>();
			if (component && component.IsRepairFocused)
			{
				this.currentWeaponScript.gameObject.SendMessage("OnRepairStructure", other.gameObject);
				if (component.FocusedRepairCollider)
				{
					this.currentWeaponScript.PlaySurfaceHit(component.FocusedRepairCollider, SfxInfo.SfxTypes.HitWood);
				}
			}
			return;
		}
		mutantTargetSwitching component2 = other.transform.GetComponent<mutantTargetSwitching>();
		if ((other.CompareTag("enemyCollide") || other.CompareTag("animalCollide") || other.CompareTag("Fish") || other.CompareTag("EnemyBodyPart")) && (this.mainTrigger || this.animControl.smashBool || this.chainSaw))
		{
			bool flag = false;
			if (component2 && component2.regular)
			{
				flag = true;
			}
			if (this.animControl.smashBool)
			{
				if (LocalPlayer.Animator.GetFloat("tiredFloat") < 0.35f)
				{
					base.Invoke("spawnSmashWeaponBlood", 0.1f);
				}
				else
				{
					base.Invoke("spawnSmashWeaponBlood", 0.03f);
				}
			}
			else if (!flag)
			{
				this.spawnWeaponBlood(other, false);
			}
		}
		if (!other.gameObject.CompareTag("PlayerNet") || (!this.mainTrigger && (this.mainTrigger || (!this.animControl.smashBool && !this.chainSaw))))
		{
			if (BoltNetwork.isClient)
			{
				playerHitEnemy = PlayerHitEnemy.Create(GlobalTargets.OnlyServer);
				playerHitEnemy.Target = other.GetComponentInParent<BoltEntity>();
			}
			if (other.gameObject.CompareTag("enemyHead") && !this.mainTrigger)
			{
				other.transform.SendMessageUpwards("HitHead", SendMessageOptions.DontRequireReceiver);
				if (playerHitEnemy != null)
				{
					playerHitEnemy.HitHead = true;
				}
			}
			if (other.gameObject.CompareTag("enemyCollide") && !this.mainTrigger && !this.animControl.smashBool && !this.repairTool)
			{
				other.transform.SendMessage("getSkinHitPosition", base.transform, SendMessageOptions.DontRequireReceiver);
			}
			if (other.gameObject.CompareTag("structure") && !this.repairTool && (!BoltNetwork.isRunning || BoltNetwork.isServer || !BoltNetwork.isClient || !PlayerPreferences.NoDestructionRemote))
			{
				this.setup.pmNoise.SendEvent("toWeaponNoise");
				Mood.HitRumble();
				other.SendMessage("Hit", SendMessageOptions.DontRequireReceiver);
				float damage = this.WeaponDamage * 4f;
				if (this.tht.atEnemy)
				{
					damage = this.WeaponDamage / 2f;
				}
				other.SendMessage("LocalizedHit", new LocalizedHitData(base.transform.position, damage), SendMessageOptions.DontRequireReceiver);
			}
			if (BoltNetwork.isClient && (other.CompareTag("jumpObject") || other.CompareTag("UnderfootWood")) && !this.repairTool)
			{
				this.FauxMpHit(Mathf.CeilToInt(this.WeaponDamage * 4f));
			}
			string tag = other.gameObject.tag;
			if (tag != null)
			{
				if (weaponInfo.<>f__switch$map3 == null)
				{
					weaponInfo.<>f__switch$map3 = new Dictionary<string, int>(9)
					{
						{
							"jumpObject",
							0
						},
						{
							"UnderfootWood",
							0
						},
						{
							"SLTier1",
							0
						},
						{
							"SLTier2",
							0
						},
						{
							"SLTier3",
							0
						},
						{
							"UnderfootRock",
							0
						},
						{
							"Target",
							0
						},
						{
							"Untagged",
							0
						},
						{
							"Block",
							0
						}
					};
				}
				int num;
				if (weaponInfo.<>f__switch$map3.TryGetValue(tag, out num))
				{
					if (num == 0)
					{
						if (!this.repairTool && (!BoltNetwork.isRunning || BoltNetwork.isServer || !BoltNetwork.isClient || !PlayerPreferences.NoDestructionRemote))
						{
							other.SendMessage("LocalizedHit", new LocalizedHitData(base.transform.position, this.WeaponDamage * 4f), SendMessageOptions.DontRequireReceiver);
							this.setup.pmNoise.SendEvent("toWeaponNoise");
						}
					}
				}
			}
			this.PlaySurfaceHit(other, SfxInfo.SfxTypes.None);
			if (this.spear && other.gameObject.CompareTag("Fish") && (this.MyFish == null || !this.MyFish.gameObject.activeSelf) && (!this.mainTrigger || ForestVR.Enabled))
			{
				base.transform.parent.SendMessage("GotBloody", SendMessageOptions.DontRequireReceiver);
				FMODCommon.PlayOneshotNetworked(this.fleshHitEvent, base.transform, FMODCommon.NetworkRole.Any);
				this.spearedFish.Add(other.gameObject);
				other.transform.parent = base.transform;
				other.transform.position = this.SpearTip.position;
				other.transform.rotation = this.SpearTip.rotation;
				this.MyFish = other.transform.GetComponent<Fish>();
				if (this.MyFish && this.MyFish.typeCaveFish)
				{
					other.transform.position = this.SpearTip2.position;
					other.transform.rotation = this.SpearTip2.rotation;
				}
				other.SendMessage("DieSpear", SendMessageOptions.DontRequireReceiver);
			}
			if (other.gameObject.CompareTag("hanging") || other.gameObject.CompareTag("corpseProp") || (other.gameObject.CompareTag("BreakableWood") && !this.mainTrigger))
			{
				Rigidbody component3 = other.GetComponent<Rigidbody>();
				float d = this.pushForce;
				if (other.gameObject.CompareTag("BreakableWood"))
				{
					d = 4500f;
				}
				if (component3)
				{
					component3.AddForceAtPosition(this.playerTr.forward * d * 0.75f * (0.016666f / Time.fixedDeltaTime), base.transform.position, ForceMode.Force);
				}
				if (!other.gameObject.GetComponent<WeaponHitSfxInfo>() && (other.gameObject.CompareTag("hanging") || other.gameObject.CompareTag("corpseProp")))
				{
					FMODCommon.PlayOneshotNetworked(this.currentWeaponScript.fleshHitEvent, this.weaponAudio.transform, FMODCommon.NetworkRole.Any);
				}
			}
			if (this.spear && !this.mainTrigger && (other.gameObject.CompareTag("Water") || other.gameObject.CompareTag("Ocean")))
			{
				if (!LocalPlayer.ScriptSetup.targetInfo.inYacht)
				{
					this.PlayGroundHit(this.waterHitEvent);
					base.StartCoroutine(this.spawnSpearSplash(other));
				}
				this.setup.pmNoise.SendEvent("toWeaponNoise");
			}
			if (!this.spear && !this.mainTrigger && (other.gameObject.CompareTag("Water") || other.gameObject.CompareTag("Ocean")) && !LocalPlayer.ScriptSetup.targetInfo.inYacht)
			{
				this.PlayGroundHit(this.waterHitEvent);
			}
			if (other.gameObject.CompareTag("Shell") && !this.mainTrigger)
			{
				other.gameObject.SendMessage("getAttackerType", 4, SendMessageOptions.DontRequireReceiver);
				other.gameObject.SendMessage("getAttacker", this.Player, SendMessageOptions.DontRequireReceiver);
				Mood.HitRumble();
				other.transform.SendMessageUpwards("Hit", 1, SendMessageOptions.DontRequireReceiver);
				this.PlayEvent(this.currentWeaponScript.shellHitEvent, this.weaponAudio);
			}
			if (other.gameObject.CompareTag("PlaneHull") && !this.mainTrigger)
			{
				this.PlayEvent(this.currentWeaponScript.planeHitEvent, this.weaponAudio);
			}
			if (other.gameObject.CompareTag("Tent") && !this.mainTrigger)
			{
				this.PlayEvent(this.currentWeaponScript.tentHitEvent, this.weaponAudio);
			}
			mutantHitReceiver component4 = other.GetComponent<mutantHitReceiver>();
			if ((other.gameObject.CompareTag("enemyCollide") || other.gameObject.CompareTag("animalCollide")) && this.mainTrigger && !this.enemyDelay && !this.animControl.smashBool)
			{
				if (BoltNetwork.isClient && other.gameObject.CompareTag("enemyCollide"))
				{
					CoopMutantClientHitPrediction componentInChildren = other.transform.root.gameObject.GetComponentInChildren<CoopMutantClientHitPrediction>();
					if (componentInChildren)
					{
						componentInChildren.getClientHitDirection(this.animator.GetInteger("hitDirection"));
						componentInChildren.StartPrediction();
					}
				}
				if (this.currentWeaponScript)
				{
					this.currentWeaponScript.transform.parent.SendMessage("GotBloody", SendMessageOptions.DontRequireReceiver);
				}
				Vector3 vector = other.transform.root.GetChild(0).InverseTransformPoint(this.playerTr.position);
				float num2 = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
				other.gameObject.SendMessage("getAttackerType", 4, SendMessageOptions.DontRequireReceiver);
				other.gameObject.SendMessage("getAttacker", this.Player, SendMessageOptions.DontRequireReceiver);
				if (playerHitEnemy != null)
				{
					playerHitEnemy.getAttackerType = 4;
				}
				this.animator.SetFloatReflected("connectFloat", 1f);
				base.Invoke("resetConnectFloat", 0.3f);
				if (num2 < -140f || num2 > 140f)
				{
					if (component4)
					{
						component4.takeDamage(1);
					}
					else
					{
						other.transform.SendMessageUpwards("takeDamage", 1, SendMessageOptions.DontRequireReceiver);
					}
					if (playerHitEnemy != null)
					{
						playerHitEnemy.takeDamage = 1;
					}
				}
				else
				{
					if (component4)
					{
						component4.takeDamage(0);
					}
					else
					{
						other.transform.SendMessageUpwards("takeDamage", 0, SendMessageOptions.DontRequireReceiver);
					}
					if (playerHitEnemy != null)
					{
						playerHitEnemy.takeDamage = 0;
					}
				}
				if (this.spear || this.shell || this.chainSaw)
				{
					other.transform.SendMessageUpwards("getAttackDirection", 3, SendMessageOptions.DontRequireReceiver);
					if (playerHitEnemy != null)
					{
						playerHitEnemy.getAttackDirection = 3;
					}
				}
				else if (this.axe || this.rock || this.stick)
				{
					int integer = this.animator.GetInteger("hitDirection");
					if (this.axe)
					{
						if (component4)
						{
							component4.getAttackDirection(integer);
							component4.getStealthAttack();
						}
						else
						{
							other.transform.SendMessageUpwards("getAttackDirection", integer, SendMessageOptions.DontRequireReceiver);
							other.transform.SendMessageUpwards("getStealthAttack", SendMessageOptions.DontRequireReceiver);
						}
					}
					else if (this.stick)
					{
						if (component4)
						{
							component4.getAttackDirection(integer);
						}
						else
						{
							other.transform.SendMessageUpwards("getAttackDirection", integer, SendMessageOptions.DontRequireReceiver);
						}
					}
					else if (component4)
					{
						component4.getAttackDirection(0);
						component4.getStealthAttack();
					}
					else
					{
						other.transform.SendMessageUpwards("getAttackDirection", 0, SendMessageOptions.DontRequireReceiver);
						other.transform.SendMessageUpwards("getStealthAttack", SendMessageOptions.DontRequireReceiver);
					}
					if (playerHitEnemy != null)
					{
						if (this.axe)
						{
							playerHitEnemy.getAttackDirection = integer;
						}
						else if (this.stick)
						{
							playerHitEnemy.getAttackDirection = integer;
						}
						else
						{
							playerHitEnemy.getAttackDirection = 0;
						}
						playerHitEnemy.getStealthAttack = true;
					}
				}
				else
				{
					int integer2 = this.animator.GetInteger("hitDirection");
					if (component4)
					{
						component4.getAttackDirection(integer2);
					}
					else
					{
						other.transform.SendMessageUpwards("getAttackDirection", integer2, SendMessageOptions.DontRequireReceiver);
					}
					if (playerHitEnemy != null)
					{
						playerHitEnemy.getAttackDirection = integer2;
					}
				}
				if (this.fireStick && UnityEngine.Random.value > 0.8f)
				{
					if (component4)
					{
						component4.Burn();
					}
					else
					{
						other.transform.SendMessageUpwards("Burn", SendMessageOptions.DontRequireReceiver);
					}
					if (playerHitEnemy != null)
					{
						playerHitEnemy.Burn = true;
					}
				}
				float num3 = this.weaponDamage;
				if (component2 && this.chainSaw && (component2.typeMaleCreepy || component2.typeFemaleCreepy || component2.typeFatCreepy))
				{
					num3 = this.weaponDamage / 2f;
				}
				if (this.hitReactions.kingHitBool || this.fsmHeavyAttackBool.Value)
				{
					if (component4)
					{
						if (this.fsmHeavyAttackBool.Value && this.axe && !this.smallAxe)
						{
							component4.sendHitFallDown(num3 * 3f);
							if (playerHitEnemy != null)
							{
								playerHitEnemy.Hit = (int)num3 * 3;
								playerHitEnemy.hitFallDown = true;
							}
						}
						else
						{
							component4.getCombo(3);
							component4.hitRelay((int)num3 * 3);
						}
					}
					else
					{
						int animalHitDirection = animalHealth.GetAnimalHitDirection(num2);
						other.transform.SendMessageUpwards("getCombo", 3, SendMessageOptions.DontRequireReceiver);
						other.transform.SendMessageUpwards("ApplyAnimalSkinDamage", animalHitDirection, SendMessageOptions.DontRequireReceiver);
						other.transform.SendMessageUpwards("Hit", (int)num3 * 3, SendMessageOptions.DontRequireReceiver);
						if (playerHitEnemy != null)
						{
							playerHitEnemy.getAttackDirection = animalHitDirection;
						}
					}
					if (playerHitEnemy != null)
					{
						playerHitEnemy.Hit = (int)num3 * 3;
						playerHitEnemy.getCombo = 3;
					}
					Mood.HitRumble();
					FMODCommon.PlayOneshotNetworked(this.currentWeaponScript.fleshHitEvent, this.weaponAudio.transform, FMODCommon.NetworkRole.Any);
				}
				else
				{
					if (component4)
					{
						component4.hitRelay((int)num3);
					}
					else
					{
						int animalHitDirection2 = animalHealth.GetAnimalHitDirection(num2);
						other.transform.SendMessageUpwards("ApplyAnimalSkinDamage", animalHitDirection2, SendMessageOptions.DontRequireReceiver);
						other.transform.SendMessageUpwards("Hit", (int)num3, SendMessageOptions.DontRequireReceiver);
						if (playerHitEnemy != null)
						{
							playerHitEnemy.getAttackDirection = animalHitDirection2;
						}
					}
					Mood.HitRumble();
					if (playerHitEnemy != null)
					{
						playerHitEnemy.Hit = (int)num3;
					}
					FMODCommon.PlayOneshotNetworked(this.currentWeaponScript.fleshHitEvent, this.weaponAudio.transform, FMODCommon.NetworkRole.Any);
				}
				this.setup.pmNoise.SendEvent("toWeaponNoise");
				this.hitReactions.enableWeaponHitState();
				this.animControl.hitCombo();
				if (((this.axe || this.rock) && !this.animator.GetBool("smallAxe")) || this.fsmHeavyAttackBool.Value)
				{
					if (component4)
					{
						component4.getCombo(3);
					}
					else
					{
						other.transform.SendMessageUpwards("getCombo", 3, SendMessageOptions.DontRequireReceiver);
					}
					if (playerHitEnemy != null)
					{
						playerHitEnemy.getCombo = 3;
					}
				}
				else if (!this.hitReactions.kingHitBool)
				{
					if (component4)
					{
						component4.getCombo(this.animControl.combo);
					}
					else
					{
						other.transform.SendMessageUpwards("getCombo", this.animControl.combo, SendMessageOptions.DontRequireReceiver);
					}
					if (playerHitEnemy != null)
					{
						playerHitEnemy.getCombo = this.animControl.combo;
					}
				}
			}
			if ((other.CompareTag("suitCase") || other.CompareTag("metalProp")) && this.animControl.smashBool)
			{
				other.transform.SendMessage("Hit", this.smashDamage, SendMessageOptions.DontRequireReceiver);
				Mood.HitRumble();
				if (playerHitEnemy != null)
				{
					playerHitEnemy.Hit = (int)this.smashDamage;
				}
				if (BoltNetwork.isRunning && other.CompareTag("suitCase"))
				{
					OpenSuitcase openSuitcase = OpenSuitcase.Create(GlobalTargets.Others);
					openSuitcase.Position = base.GetComponent<Collider>().transform.position;
					openSuitcase.Damage = (int)this.smashDamage;
					openSuitcase.Send();
				}
				if (this.smashSoundEnabled)
				{
					this.smashSoundEnabled = false;
					base.Invoke("EnableSmashSound", 0.3f);
					this.PlayEvent(this.smashHitEvent, null);
					if (BoltNetwork.isRunning)
					{
						FmodOneShot fmodOneShot = FmodOneShot.Create(GlobalTargets.Others, ReliabilityModes.Unreliable);
						fmodOneShot.EventPath = CoopAudioEventDb.FindId(this.smashHitEvent);
						fmodOneShot.Position = base.transform.position;
						fmodOneShot.Send();
					}
				}
				this.setup.pmNoise.SendEvent("toWeaponNoise");
				this.hitReactions.enableWeaponHitState();
				if (other.CompareTag("metalProp"))
				{
					Rigidbody component5 = other.GetComponent<Rigidbody>();
					if (component5)
					{
						component5.AddForceAtPosition((Vector3.down + LocalPlayer.Transform.forward * 0.2f) * this.pushForce * 2f * (0.016666f / Time.fixedDeltaTime), base.transform.position, ForceMode.Force);
					}
				}
			}
			if ((other.CompareTag("enemyCollide") || other.CompareTag("lb_bird") || other.CompareTag("animalCollide") || other.CompareTag("Fish") || other.CompareTag("EnemyBodyPart")) && !this.mainTrigger && !this.enemyDelay && (this.animControl.smashBool || this.chainSaw))
			{
				float num4 = this.smashDamage;
				if (this.chainSaw && !this.mainTrigger)
				{
					base.StartCoroutine(this.chainSawClampRotation(0.25f));
					num4 = this.smashDamage / 2f;
				}
				base.transform.parent.SendMessage("GotBloody", SendMessageOptions.DontRequireReceiver);
				this.enemyDelay = true;
				base.Invoke("resetEnemyDelay", 0.25f);
				if ((this.rock || this.stick || this.spear || this.noBodyCut) && !this.allowBodyCut)
				{
					other.transform.SendMessageUpwards("ignoreCutting", SendMessageOptions.DontRequireReceiver);
				}
				other.transform.SendMessage("getSkinHitPosition", base.transform, SendMessageOptions.DontRequireReceiver);
				other.transform.SendMessage("hitSuitCase", num4, SendMessageOptions.DontRequireReceiver);
				other.gameObject.SendMessage("getAttacker", this.Player, SendMessageOptions.DontRequireReceiver);
				other.gameObject.SendMessage("getAttackerType", 4, SendMessageOptions.DontRequireReceiver);
				if (this.fsmJumpAttackBool.Value && LocalPlayer.FpCharacter.jumpingTimer > 1.2f && !this.chainSaw)
				{
					other.transform.SendMessageUpwards("Explosion", -1, SendMessageOptions.DontRequireReceiver);
					if (BoltNetwork.isRunning)
					{
						playerHitEnemy.explosion = true;
					}
				}
				else if (!other.gameObject.CompareTag("Fish"))
				{
					if (other.gameObject.CompareTag("animalCollide"))
					{
						Vector3 vector2 = other.transform.root.GetChild(0).InverseTransformPoint(this.playerTr.position);
						float targetAngle = Mathf.Atan2(vector2.x, vector2.z) * 57.29578f;
						int animalHitDirection3 = animalHealth.GetAnimalHitDirection(targetAngle);
						other.transform.SendMessageUpwards("ApplyAnimalSkinDamage", animalHitDirection3, SendMessageOptions.DontRequireReceiver);
						other.transform.SendMessageUpwards("Hit", (int)num4, SendMessageOptions.DontRequireReceiver);
						Mood.HitRumble();
						if (playerHitEnemy != null)
						{
							playerHitEnemy.getAttackDirection = animalHitDirection3;
						}
					}
					else
					{
						other.transform.SendMessageUpwards("getAttackDirection", 3, SendMessageOptions.DontRequireReceiver);
						other.transform.SendMessageUpwards("Hit", num4, SendMessageOptions.DontRequireReceiver);
						Mood.HitRumble();
					}
				}
				else if (other.gameObject.CompareTag("Fish") && !this.spear)
				{
					other.transform.SendMessage("Hit", num4, SendMessageOptions.DontRequireReceiver);
					Mood.HitRumble();
				}
				if (playerHitEnemy != null)
				{
					playerHitEnemy.getAttackerType = 4;
					playerHitEnemy.Hit = (int)num4;
				}
				if (this.axe)
				{
					other.transform.SendMessageUpwards("HitAxe", SendMessageOptions.DontRequireReceiver);
					if (playerHitEnemy != null)
					{
						playerHitEnemy.HitAxe = true;
					}
				}
				if (other.CompareTag("lb_bird") || other.CompareTag("animalCollide"))
				{
					FMODCommon.PlayOneshotNetworked(this.animalHitEvent, base.transform, FMODCommon.NetworkRole.Any);
				}
				else if (other.CompareTag("enemyCollide"))
				{
					FMODCommon.PlayOneshotNetworked(this.fleshHitEvent, base.transform, FMODCommon.NetworkRole.Any);
				}
				else if (other.CompareTag("EnemyBodyPart"))
				{
					FMODCommon.PlayOneshotNetworked(this.hackBodyEvent, base.transform, FMODCommon.NetworkRole.Any);
					this.FauxMpHit((int)this.smashDamage);
				}
				this.setup.pmNoise.SendEvent("toWeaponNoise");
				this.hitReactions.enableWeaponHitState();
			}
			if (!this.mainTrigger && (other.CompareTag("BreakableWood") || other.CompareTag("BreakableRock")))
			{
				other.transform.SendMessage("Hit", this.WeaponDamage, SendMessageOptions.DontRequireReceiver);
				Mood.HitRumble();
				other.SendMessage("LocalizedHit", new LocalizedHitData(base.transform.position, this.WeaponDamage), SendMessageOptions.DontRequireReceiver);
				this.FauxMpHit((int)this.WeaponDamage);
			}
			if (other.CompareTag("lb_bird") && !this.mainTrigger)
			{
				base.transform.parent.SendMessage("GotBloody", SendMessageOptions.DontRequireReceiver);
				other.transform.SendMessage("Hit", this.WeaponDamage, SendMessageOptions.DontRequireReceiver);
				Mood.HitRumble();
				FMODCommon.PlayOneshotNetworked(this.animalHitEvent, base.transform, FMODCommon.NetworkRole.Any);
				this.setup.pmNoise.SendEvent("toWeaponNoise");
				this.hitReactions.enableWeaponHitState();
				if (playerHitEnemy != null)
				{
					playerHitEnemy.Hit = (int)this.WeaponDamage;
				}
			}
			if ((other.CompareTag("Tree") && !this.mainTrigger) || (other.CompareTag("MidTree") && !this.mainTrigger))
			{
				if (this.chainSaw)
				{
					base.StartCoroutine(this.chainSawClampRotation(0.5f));
				}
				this.animEvents.cuttingTree = true;
				this.animEvents.Invoke("resetCuttingTree", 0.5f);
				if (this.stick || this.fireStick)
				{
					other.SendMessage("HitStick", SendMessageOptions.DontRequireReceiver);
					this.setup.pmNoise.SendEvent("toWeaponNoise");
					this.animator.SetFloatReflected("weaponHit", 1f);
					this.PlayEvent(this.treeHitEvent, null);
					if (BoltNetwork.isRunning && base.entity.isOwner)
					{
						FmodOneShot fmodOneShot2 = FmodOneShot.Create(GlobalTargets.Others, ReliabilityModes.Unreliable);
						fmodOneShot2.Position = base.transform.position;
						fmodOneShot2.EventPath = CoopAudioEventDb.FindId(this.treeHitEvent);
						fmodOneShot2.Send();
					}
				}
				else if (!this.Delay)
				{
					this.Delay = true;
					base.Invoke("ResetDelay", 0.2f);
					this.SapDice = UnityEngine.Random.Range(0, 5);
					this.setup.pmNoise.SendEvent("toWeaponNoise");
					if (!this.noTreeCut)
					{
						if (this.SapDice == 1)
						{
							this.PlayerInv.GotSap(null);
						}
						if (other.GetType() == typeof(CapsuleCollider))
						{
							base.StartCoroutine(this.spawnWoodChips());
						}
						else
						{
							base.StartCoroutine(this.spawnWoodChips());
						}
						other.SendMessage("Hit", this.treeDamage, SendMessageOptions.DontRequireReceiver);
						Mood.HitRumble();
					}
					this.PlayEvent(this.treeHitEvent, null);
					if (BoltNetwork.isRunning && base.entity.isOwner)
					{
						FmodOneShot fmodOneShot3 = FmodOneShot.Create(GlobalTargets.Others, ReliabilityModes.Unreliable);
						fmodOneShot3.Position = base.transform.position;
						fmodOneShot3.EventPath = CoopAudioEventDb.FindId(this.treeHitEvent);
						fmodOneShot3.Send();
					}
				}
			}
			if (other.gameObject.CompareTag("Rope") && ForestVR.Enabled && this.mainTrigger)
			{
				this.setup.pmNoise.SendEvent("toWeaponNoise");
				int num5 = this.DamageAmount;
				other.SendMessage("Hit", 5, SendMessageOptions.DontRequireReceiver);
				Mood.HitRumble();
				this.PlayEvent(this.ropeHitEvent, null);
			}
			if ((other.CompareTag("SmallTree") || other.CompareTag("Rope")) && !this.mainTrigger)
			{
				this.setup.pmNoise.SendEvent("toWeaponNoise");
				int integer3 = this.animator.GetInteger("hitDirection");
				other.transform.SendMessage("getAttackDirection", integer3, SendMessageOptions.DontRequireReceiver);
				int num6 = this.DamageAmount;
				if (this.chainSaw || this.machete)
				{
					num6 *= 5;
				}
				other.SendMessage("Hit", num6, SendMessageOptions.DontRequireReceiver);
				Mood.HitRumble();
				if (this.chainSaw || this.machete)
				{
					other.SendMessage("Hit", num6, SendMessageOptions.DontRequireReceiver);
				}
				this.FauxMpHit(num6);
				if (this.chainSaw || this.machete)
				{
					this.FauxMpHit(num6);
				}
				if (!this.plantSoundBreak)
				{
					if (other.CompareTag("SmallTree"))
					{
						if (!string.IsNullOrEmpty(this.plantHitEvent))
						{
							FMODCommon.PlayOneshotNetworked(this.plantHitEvent, base.transform, FMODCommon.NetworkRole.Any);
						}
					}
					else if (other.CompareTag("Rope"))
					{
						this.PlayEvent(this.ropeHitEvent, null);
					}
					this.plantSoundBreak = true;
					base.Invoke("disablePlantBreak", 0.3f);
				}
				if (other.CompareTag("SmallTree"))
				{
					this.PlayerInv.GotLeaf();
				}
			}
			if (other.CompareTag("fire") && !this.mainTrigger && this.fireStick)
			{
				other.SendMessage("startFire");
			}
			if (playerHitEnemy != null && playerHitEnemy.Target && playerHitEnemy.Hit > 0)
			{
				if (ForestVR.Enabled && BoltNetwork.isClient)
				{
					playerHitEnemy.getCombo = UnityEngine.Random.Range(2, 4);
				}
				playerHitEnemy.Send();
			}
			return;
		}
		BoltEntity component6 = other.GetComponent<BoltEntity>();
		BoltEntity component7 = base.GetComponent<BoltEntity>();
		if (object.ReferenceEquals(component6, component7))
		{
			return;
		}
		if (this.lastPlayerHit + 0.4f < Time.time)
		{
			other.transform.root.SendMessage("getClientHitDirection", this.animator.GetInteger("hitDirection"), SendMessageOptions.DontRequireReceiver);
			other.transform.root.SendMessage("StartPrediction", SendMessageOptions.DontRequireReceiver);
			this.lastPlayerHit = Time.time;
			if (BoltNetwork.isRunning)
			{
				HitPlayer.Create(component6, EntityTargets.Everyone).Send();
			}
		}
	}

	private static bool GetInvalidAttackAngle(Collider other)
	{
		if (other.CompareTag("Tree") || other.CompareTag("MidTree") || other.CompareTag("enemyCollide") || other.CompareTag("lb_bird") || other.CompareTag("animalCollide") || other.CompareTag("Fish") || other.CompareTag("EnemyBodyPart") || other.CompareTag("jumpObject"))
		{
			Vector3 vector = LocalPlayer.Transform.InverseTransformPoint(other.bounds.center);
			float num = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
			if (num > 50f || num < -50f)
			{
				return true;
			}
		}
		return false;
	}

	private IEnumerator chainSawClampRotation(float delay)
	{
		if (Time.time < this.clampTime)
		{
			this.clampTime = Time.time + delay;
			yield break;
		}
		LocalPlayer.HitReactions.enableControllerFreeze();
		LocalPlayer.MainRotator.rotationSpeed = 0.55f;
		this.clampTime = Time.time + delay;
		while (Time.time < this.clampTime)
		{
			yield return null;
		}
		LocalPlayer.HitReactions.disableControllerFreeze();
		LocalPlayer.MainRotator.rotationSpeed = 5f;
		yield break;
	}

	private IEnumerator delayedTreeHit(Collider target, float d)
	{
		yield return YieldPresets.WaitPointZeroFiveSeconds;
		target.SendMessage("Hit", d, SendMessageOptions.DontRequireReceiver);
		yield break;
	}

	private void FauxMpHit(int damage)
	{
		if (BoltNetwork.isRunning)
		{
			FauxWeaponHit fauxWeaponHit = FauxWeaponHit.Create(GlobalTargets.Others);
			fauxWeaponHit.Damage = damage;
			fauxWeaponHit.Position = base.transform.position;
			fauxWeaponHit.Send();
		}
	}

	private void spawnSmashWeaponBlood()
	{
		this.spawnWeaponBlood(this.thisCollider, true);
	}

	private void spawnWeaponBlood(Collider other, bool smashBlood = false)
	{
		if (this.remotePlayer)
		{
			return;
		}
		if (Time.time < this.bloodCoolDown)
		{
			return;
		}
		this.bloodCoolDown = Time.time + 0.2f;
		if (this.animControl.smashBool)
		{
			this.bloodCoolDown = Time.time + 0.65f;
		}
		if (other == null)
		{
			return;
		}
		int num = UnityEngine.Random.Range(1, Prefabs.Instance.BloodHitPSPrefabs.Length);
		Vector3 vector = other.bounds.center + LocalPlayer.MainCamTr.forward * -1.3f;
		netId component = other.GetComponent<netId>();
		if (component)
		{
			vector = other.bounds.center;
		}
		if (other.gameObject.CompareTag("EnemyBodyPart") || this.animControl.smashBool)
		{
			vector = this.currentWeaponScript.transform.position;
		}
		float angle = 90f;
		if (this.animator.GetInteger("hitDirection") == 1)
		{
			angle = -90f;
		}
		if (this.spear || this.chainSaw)
		{
			vector = this.currentWeaponScript.transform.position;
			if (UnityEngine.Random.value > 0.5f)
			{
				angle = 90f;
			}
			else
			{
				angle = -90f;
			}
		}
		Quaternion quaternion = Quaternion.LookRotation(LocalPlayer.Transform.forward, Vector3.up);
		if (!this.animControl.smashBool)
		{
			quaternion *= Quaternion.AngleAxis(angle, Vector3.up);
		}
		if (this.animControl.smashBool)
		{
			PoolManager.Pools["Particles"].Spawn(Prefabs.Instance.SmashBloodPrefab.transform, vector, quaternion);
		}
		else
		{
			Transform tr = PoolManager.Pools["Particles"].Spawn(Prefabs.Instance.BloodHitPSPrefabs[0].transform, vector, quaternion);
			Transform tr2 = PoolManager.Pools["Particles"].Spawn(Prefabs.Instance.BloodHitPSPrefabs[1].transform, vector, quaternion);
			base.StartCoroutine(this.fixBloodPosition(tr, other, vector));
			base.StartCoroutine(this.fixBloodPosition(tr2, other, vector));
		}
	}

	private IEnumerator fixBloodPosition(Transform tr, Collider otherCol, Vector3 startPos)
	{
		if (otherCol == null)
		{
			yield break;
		}
		Vector3 relPos = otherCol.transform.InverseTransformPoint(startPos);
		float t = 0f;
		while (t < 0.5f && otherCol != null)
		{
			tr.position = otherCol.bounds.center + relPos;
			t += Time.deltaTime;
			yield return null;
		}
		yield break;
	}

	private IEnumerator spawnWoodChips()
	{
		yield return YieldPresets.WaitForEndOfFrame;
		yield return YieldPresets.WaitForEndOfFrame;
		Quaternion spawnAngle = Quaternion.LookRotation(this.currentWeaponScript.transform.up, Vector3.up);
		Prefabs.Instance.SpawnWoodChopPS(this.currentWeaponScript.transform.position, spawnAngle);
		if (BoltNetwork.isRunning && base.entity.isOwner)
		{
			spawnTreeDust spawnTreeDust = spawnTreeDust.Create(GlobalTargets.Others, ReliabilityModes.Unreliable);
			spawnTreeDust.position = this.currentWeaponScript.transform.position;
			spawnTreeDust.rotation = spawnAngle;
			spawnTreeDust.Send();
		}
		yield break;
	}

	public bool PlaySurfaceHit(Collider collider, SfxInfo.SfxTypes fallback = SfxInfo.SfxTypes.None)
	{
		if (this.Delay)
		{
			return false;
		}
		if (this.mainTrigger && !this.repairTool)
		{
			return false;
		}
		if (collider.gameObject.CompareTag("TerrainMain"))
		{
			this.PlayGroundHit(this.dirtHitEvent);
			return true;
		}
		UnderfootSurfaceDetector.SurfaceType surfaceType = UnderfootSurfaceDetector.GetSurfaceType(collider);
		string empty = string.Empty;
		if (surfaceType != UnderfootSurfaceDetector.SurfaceType.Wood)
		{
			if (surfaceType != UnderfootSurfaceDetector.SurfaceType.Rock)
			{
				if (Sfx.TryPlay<WeaponHitSfxInfo>(collider, base.transform, true))
				{
					return true;
				}
				if (fallback != SfxInfo.SfxTypes.None)
				{
					Sfx.Play(fallback, base.transform, true);
					return true;
				}
				return false;
			}
			else
			{
				empty = this.rockHitEvent;
			}
		}
		else
		{
			empty = this.treeHitEvent;
		}
		FMODCommon.PlayOneshotNetworked(empty, base.transform, FMODCommon.NetworkRole.Any);
		return surfaceType != UnderfootSurfaceDetector.SurfaceType.None;
	}

	private void PlayGroundHit(string path)
	{
		if (!this.GroundHitDelay)
		{
			FMODCommon.PlayOneshotNetworked(path, base.transform, FMODCommon.NetworkRole.Any);
			this.GroundHitDelay = true;
			base.Invoke("ResetGroundHitDelay", UnityEngine.Random.Range(0.15f, 0.25f));
		}
	}

	private IEnumerator spawnSpearSplash(Collider other)
	{
		yield return YieldPresets.WaitForEndOfFrame;
		yield return YieldPresets.WaitForEndOfFrame;
		if (other == null)
		{
			yield break;
		}
		float height = other.bounds.center.y + other.bounds.extents.y;
		height -= base.transform.position.y;
		Vector3 splashPos = base.transform.position + base.transform.up * height;
		splashPos.y = other.bounds.center.y + other.bounds.extents.y;
		Transform Splash = PoolManager.Pools["Particles"].Spawn(this.MyParticle, splashPos, base.transform.rotation);
		Splash.transform.parent = null;
		yield break;
	}

	private void disablePlantBreak()
	{
		this.plantSoundBreak = false;
	}

	private void EnableSmashSound()
	{
		this.smashSoundEnabled = true;
	}

	private void resetGroundHeight()
	{
		this.animator.SetFloatReflected("groundHeight", 0f);
	}

	private void ResetDelay()
	{
		this.Delay = false;
	}

	private void ResetGroundHitDelay()
	{
		this.GroundHitDelay = false;
	}

	private void enableSound()
	{
		if (this.soundDetectGo)
		{
			this.soundDetectGo.GetComponent<Collider>().enabled = true;
			this.soundCollider.radius = this.soundDetectRange;
		}
	}

	private void disableSound()
	{
		if (this.soundDetectGo)
		{
			this.soundCollider.radius = this.soundDetectRangeInit;
		}
	}

	private void Update()
	{
		if (SteamDSConfig.isDedicatedServer)
		{
			return;
		}
		if (TheForest.Utils.Input.GetButtonDown("AltFire"))
		{
			this.checkBurnableCloth();
		}
		if (this.stick && TheForest.Utils.Input.GetButtonDown("Fire1"))
		{
			this.checkBurnableCloth();
		}
		if (this.MyFish && !this.MyFish.gameObject.activeSelf)
		{
			this.MyFish = null;
		}
	}

	private void OnEnable()
	{
		if (this.activeBool)
		{
			this.setupHeldWeapon();
		}
	}

	private void setupHeldWeapon()
	{
		if (!this.mainTrigger && !this.remotePlayer)
		{
			if (this.mainTriggerScript)
			{
				this.mainTriggerScript.currentWeaponScript = this;
				this.mainTriggerScript.weaponAudio = base.transform;
				this.setupMainTrigger();
				if (this.canDoGroundAxeChop)
				{
					this.mainTriggerScript.enableSpecialWeaponVars();
				}
			}
			if (!this.thisCollider && base.transform.parent)
			{
				this.thisCollider = base.transform.parent.GetComponentsInChildren<Collider>(true)[0];
			}
			if (this.sendColliderToEvents && this.thisCollider)
			{
				this.setup.events.heldWeaponCollider = this.thisCollider;
			}
			base.Invoke("cleanUpSpearedFish", 0.2f);
			if (this.animator)
			{
				this.checkBurnableCloth();
			}
			this.animSpeed = Mathf.Clamp(this.weaponSpeed, 0.01f, 20f) / 20f;
			this.animSpeed += 0.5f;
			this.animTiredSpeed = this.tiredSpeed / 10f * (this.animSpeed - 0.5f);
			this.animTiredSpeed += 0.5f;
			if (!this.setup)
			{
				return;
			}
			if (this.setup.pmStamina)
			{
				if (this.animControl.tiredCheck)
				{
					this.animControl.tempTired = this.animSpeed;
					this.setup.pmStamina.FsmVariables.GetFsmFloat("notTiredSpeed").Value = this.animSpeed / 1f;
				}
				else
				{
					this.setup.pmStamina.FsmVariables.GetFsmFloat("notTiredSpeed").Value = this.animSpeed;
				}
			}
			if (this.setup && this.setup.pmStamina)
			{
				this.setup.pmStamina.FsmVariables.GetFsmFloat("tiredSpeed").Value = this.animTiredSpeed;
			}
			if (this.setup && this.setup.pmControl)
			{
				this.setup.pmControl.FsmVariables.GetFsmFloat("staminaDrain").Value = this.staminaDrain * -1f;
			}
			if (this.setup && this.setup.pmControl)
			{
				this.setup.pmControl.FsmVariables.GetFsmFloat("blockStaminaDrain").Value = this.blockStaminaDrain * -1f;
			}
			if (LocalPlayer.Stats)
			{
				LocalPlayer.Stats.blockDamagePercent = this.blockDamagePercent;
			}
			this.damageAmount = (int)this.WeaponDamage;
			if (this.setup && this.setup.pmStamina)
			{
				this.setup.pmStamina.SendEvent("toSetStats");
			}
		}
	}

	private void OnDisable()
	{
		if (!this.remotePlayer && this.clampTime > Time.time)
		{
			LocalPlayer.MainRotator.rotationSpeed = 5f;
			LocalPlayer.HitReactions.disableControllerFreeze();
		}
		if (this.activeBool && !this.mainTrigger && !this.remotePlayer)
		{
			foreach (GameObject gameObject in this.spearedFish)
			{
				if (gameObject)
				{
					PoolManager.Pools["creatures"].Despawn(gameObject.transform);
				}
			}
			if (this.canDoGroundAxeChop && this.mainTriggerScript)
			{
				this.mainTriggerScript.disableSpecialWeaponVars();
			}
		}
	}

	private void checkBurnableCloth()
	{
		if (this.activeBool && !this.mainTrigger && !this.remotePlayer)
		{
			if (this.doSingleArmBlock)
			{
				this.animator.SetFloatReflected("singleArmBlock", 1f);
			}
			else
			{
				this.animator.SetFloatReflected("singleArmBlock", 0f);
			}
			BurnableCloth componentInChildren = base.transform.parent.GetComponentInChildren<BurnableCloth>();
			if (componentInChildren && !this.doSingleArmBlock)
			{
				if (componentInChildren.enabled)
				{
					this.animator.SetFloatReflected("singleArmBlock", 1f);
				}
				else
				{
					this.animator.SetFloatReflected("singleArmBlock", 0f);
				}
			}
			if (this.stick && componentInChildren && !this.canDoGroundAxeChop)
			{
				if (componentInChildren._state == BurnableCloth.States.Burning)
				{
					this.enableSpecialWeaponVars();
				}
				else
				{
					this.disableSpecialWeaponVars();
				}
			}
			if (this.axe)
			{
				this.animator.SetFloatReflected("singleArmBlock", 1f);
			}
		}
	}

	private void cleanUpSpearedFish()
	{
		foreach (GameObject gameObject in this.spearedFish)
		{
			if (gameObject)
			{
				gameObject.transform.parent = null;
			}
		}
		this.spearedFish.Clear();
	}

	public void ResetStats()
	{
		this.weaponDamage = this.baseWeaponDamage;
		this.weaponSpeed = this.baseWeaponSpeed;
		this.smashDamage = this.baseSmashDamage;
		this.tiredSpeed = this.baseTiredSpeed;
		this.staminaDrain = this.baseStaminaDrain;
		this.soundDetectRange = this.baseSoundDetectRange;
		this.weaponRange = this.baseWeaponRange;
	}

	private PlayMakerFSM pmControl;

	private PlayMakerFSM pmNoise;

	private PlayMakerFSM pmRotate;

	private int SapDice;

	public PlayerInventory PlayerInv;

	public treeHitTrigger tht;

	public Animator animator;

	public playerHitReactions hitReactions;

	public Transform playerTr;

	public GameObject playerBase;

	public playerAnimatorControl animControl;

	public playerScriptSetup setup;

	private weaponInfo mainTriggerScript;

	public weaponInfo currentWeaponScript;

	public Transform weaponAudio;

	public animEventsManager animEvents;

	public WeaponBonus bonus;

	private FsmBool fsmJumpAttackBool;

	private FsmBool fsmHeavyAttackBool;

	private Transform SpearTip;

	private Transform SpearTip2;

	public List<GameObject> spearedFish = new List<GameObject>();

	private SphereCollider soundCollider;

	public GameObject soundDetectGo;

	public GameObject brokenGO;

	public GameObject hitTriggerRange;

	public bool doSingleArmBlock;

	public bool noTreeCut;

	public bool canDoGroundAxeChop;

	public bool noBodyCut;

	public bool allowBodyCut;

	public bool mainTrigger;

	public bool stick;

	public bool axe;

	public bool rock;

	public bool fireStick;

	public bool spear;

	public bool shell;

	public bool smallAxe;

	public bool repairTool;

	public bool chainSaw;

	public bool machete;

	public bool sendColliderToEvents;

	public Collider thisCollider;

	public string treeHitEvent;

	public string fleshHitEvent;

	public string hackBodyEvent;

	public string animalHitEvent;

	public string shellHitEvent;

	public string planeHitEvent;

	public string plantHitEvent;

	public string ropeHitEvent;

	public string dirtHitEvent;

	public string rockHitEvent;

	public string waterHitEvent;

	public string smashHitEvent;

	public string tentHitEvent;

	private bool Delay;

	private bool GroundHitDelay;

	private GameObject Player;

	private int damageAmount;

	private bool activeBool;

	public bool remotePlayer;

	public float pushForce = 10000f;

	public float weaponDamage;

	public float smashDamage;

	public float weaponSpeed;

	public float tiredSpeed;

	public float staminaDrain;

	public float soundDetectRange;

	public float weaponRange;

	public float treeDamage;

	public float blockStaminaDrain = 20f;

	public float blockDamagePercent;

	private float soundDetectRangeInit;

	public float animSpeed;

	public float animTiredSpeed;

	private bool Tired;

	private bool enemyDelay;

	private bool plantSoundBreak;

	private bool smashSoundEnabled = true;

	public Transform MyParticle;

	private float lastPlayerHit;

	private float clampTime;

	private float bloodCoolDown;
}
