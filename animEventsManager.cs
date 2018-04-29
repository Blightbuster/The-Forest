using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using FMOD.Studio;
using PathologicalGames;
using TheForest.Items;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;


public class animEventsManager : EntityEventListener
{
	
	private void OnDeserialized()
	{
		this.animator = base.gameObject.GetComponent<Animator>();
		if (this.Remote)
		{
			return;
		}
		this.SetUpWeapons();
	}

	
	private void Awake()
	{
		this.buoyancy = base.transform.parent.GetComponent<Buoyancy>();
		this.underFoot = base.transform.parent.GetComponent<CoopUnderfootSurface>();
		this.headBob = base.transform.parent.GetComponent<FirstPersonHeadBob>();
		this.animator = base.gameObject.GetComponent<Animator>();
		this.hitTrigger = base.transform.GetComponentInChildren<treeHitTrigger>();
		this.aiInfo = base.transform.GetComponentInParent<playerAiInfo>();
		if (this.Remote)
		{
			this.vis = base.transform.parent.GetComponent<netPlayerVis>();
			return;
		}
		this.SetUpWeapons();
	}

	
	private ItemGroupEvent[] AllItemGroupEvents()
	{
		return new ItemGroupEvent[]
		{
			this.bowDrawEvent,
			this.stickSwooshEvent,
			this.axeSwooshEvent,
			this.rockSwooshEvent,
			this.spearSwooshEvent,
			this.swordSwooshEvent,
			this.fireStickSwooshEvent
		};
	}

	
	private void Start()
	{
		this._enableWeaponTimer = 0f;
		this._stats = base.transform.parent.GetComponent<targetStats>();
		this.water = Scene.Clock.GetComponent<WaterOnTerrain>();
		if (Terrain.activeTerrain && Terrain.activeTerrain.materialTemplate)
		{
			this.snowStartHeight = Terrain.activeTerrain.materialTemplate.GetFloat("_SnowStartHeight");
			this.snowFadeLength = Terrain.activeTerrain.materialTemplate.GetFloat("_SnowFadeLength");
			this.snowStartHeight += this.snowFadeLength / 4f;
			this.snowFadeLength /= 2f;
		}
		this.hashIdle = Animator.StringToHash("idling");
		this.previousPosition = base.transform.position;
		this.eventsByItemIdCache = new Dictionary<int, ItemGroupEvent>();
		if (FMOD_StudioSystem.instance)
		{
			this.sledEvent = FMOD_StudioSystem.instance.GetEvent(this.pushSledEvent);
			if (this.sledEvent != null)
			{
				this.sledEvent.getParameter("speed", out this.sledSpeedParameter);
			}
			if (this.Remote)
			{
				return;
			}
			foreach (ItemGroupEvent itemGroupEvent in this.AllItemGroupEvents())
			{
				foreach (int key in itemGroupEvent._itemIds)
				{
					this.eventsByItemIdCache[key] = itemGroupEvent;
					FMOD_StudioSystem.PreloadEvent(itemGroupEvent.eventPath);
				}
			}
			FMOD_StudioSystem.PreloadEvent(this.breatheInEvent);
			FMOD_StudioSystem.PreloadEvent(this.breatheOutEvent);
			FMOD_StudioSystem.PreloadEvent(this.fallEvent);
		}
		else if (!CoopPeerStarter.DedicatedHost)
		{
			Debug.LogError("FMOD_StudioSystem.instance is null, could not initialize animEventManager audio");
		}
	}

	
	private void FixedUpdate()
	{
		if (ForestVR.Enabled)
		{
			if (!LocalPlayer.Inventory.IsSlotEmpty(Item.EquipmentSlot.RightHand))
			{
				float num = (this._weaponVelocityTr.position - this._prevWepPos).magnitude * 100f;
				this._prevWepPos = this._weaponVelocityTr.position;
				if (num > this._vrEnableWeaponVelocity)
				{
					this._enableWeaponTimer = Time.time + this._vrWeaponAttackWindow;
				}
			}
			if (Time.time < this._enableWeaponTimer)
			{
				this.enableSmashWeapon();
			}
			else
			{
				this.disableSmashWeapon();
			}
		}
	}

	
	private void Update()
	{
		this.currState0 = this.animator.GetCurrentAnimatorStateInfo(0);
		this.currState2 = this.animator.GetCurrentAnimatorStateInfo(2);
		if (this.sledEvent != null)
		{
			PLAYBACK_STATE state;
			UnityUtil.ERRCHECK(this.sledEvent.getPlaybackState(out state));
			bool flag = false;
			int shortNameHash = this.animator.GetCurrentAnimatorStateInfo(1).shortNameHash;
			int num = (!this.animator.IsInTransition(1)) ? 0 : this.animator.GetNextAnimatorStateInfo(1).shortNameHash;
			if (shortNameHash == animEventsManager.idleToPushSledHash || num == animEventsManager.idleToPushSledHash || shortNameHash == animEventsManager.pushSledIdleHash || num == animEventsManager.pushSledIdleHash)
			{
				int shortNameHash2 = this.animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
				int num2 = (!this.animator.IsInTransition(0)) ? 0 : this.animator.GetNextAnimatorStateInfo(0).shortNameHash;
				if (this.IsSledTurning)
				{
					flag = true;
				}
				else if (shortNameHash2 == animEventsManager.locomotionHash || num2 == animEventsManager.locomotionHash)
				{
					flag = true;
				}
			}
			if (flag)
			{
				UnityUtil.ERRCHECK(this.sledEvent.set3DAttributes(UnityUtil.to3DAttributes(base.gameObject, null)));
				Vector3 vector = (base.transform.position - this.previousPosition) / Time.deltaTime;
				vector.y = 0f;
				UnityUtil.ERRCHECK(this.sledSpeedParameter.setValue(Mathf.Clamp01(vector.magnitude / LocalPlayer.FpCharacter.runSpeed)));
				if (!state.isPlaying())
				{
					UnityUtil.ERRCHECK(this.sledEvent.start());
				}
			}
			else if (state.isPlaying())
			{
				UnityUtil.ERRCHECK(this.sledEvent.stop(STOP_MODE.ALLOWFADEOUT));
			}
		}
		this.previousPosition = base.transform.position;
	}

	
	private void SetUpWeapons()
	{
		if (this.Remote || Application.isPlaying)
		{
			return;
		}
		Debug.Log("setting up weapons");
		this.dig = base.transform.parent.GetComponentInChildren<Digger>();
		Transform[] componentsInChildren = base.transform.root.GetComponentsInChildren<Transform>(true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform.name == "hitTrigger")
			{
				this.mainWeaponCollider = transform.GetComponent<Collider>();
				if (this.mainWeaponCollider)
				{
					this.mainWeaponCollider.enabled = false;
				}
				this.hitTriggerTr = transform.parent.transform;
				Debug.Log("hitTirggerTr setup");
			}
			if (transform.name == "stickHeld")
			{
				this.stickCollider1 = transform.GetComponentInChildren<BoxCollider>();
				this.stickCollider2 = transform.GetComponentInChildren<CapsuleCollider>();
				this.stickCollider2.enabled = false;
			}
			if (transform.name == "AxeHeld")
			{
				this.axeCollider = transform.GetComponentInChildren<Collider>();
			}
			if (transform.name == "AxeCraftedHeld")
			{
				this.axeCraftedCollider = transform.GetComponentInChildren<Collider>();
			}
			if (transform.name == "RockHeld")
			{
				this.rockCollider = transform.GetComponentInChildren<Collider>();
			}
			if (transform.name == "spearHeld")
			{
				this.spearCollider = transform.GetComponentInChildren<Collider>();
			}
			if (transform.name == "AxeHeldRusty")
			{
				this.axeRustyCollider = transform.GetComponentInChildren<Collider>();
			}
			if (transform.name == "AxePlaneHeld")
			{
				this.axePlaneCollider = transform.GetComponentInChildren<Collider>();
			}
			if (transform.name == "testCube")
			{
			}
			if (transform.name == "armHeld")
			{
				this.armHeldCollider = transform.GetComponentInChildren<Collider>();
			}
			if (transform.name == "legHeld")
			{
				this.legHeldCollider = transform.GetComponentInChildren<Collider>();
			}
			if (transform.name == "HeadHeld")
			{
				this.headHeldCollider = transform.GetComponentInChildren<CapsuleCollider>();
			}
			if (transform.name == "ClubCraftedHeld")
			{
				this.clubCraftedHeldCollider = transform.GetComponentInChildren<Collider>();
			}
			if (transform.name == "ClubHeld")
			{
				this.clubHeldCollider = transform.GetComponentInChildren<Collider>();
			}
			if (transform.name == "SkullHeld")
			{
				this.skullHeldCollider = transform.GetComponentInChildren<Collider>();
			}
			if (transform.name == "stickHeldUpgraded")
			{
				this.stickUpgradedHeldCollider = transform.GetComponentInChildren<BoxCollider>();
				this.stickUpgradedHeldCollider2 = transform.GetComponentInChildren<CapsuleCollider>();
			}
			if (transform.name == "RockHeldUpgraded")
			{
				this.rockUpgradedHeldCollider = transform.GetComponentInChildren<Collider>();
			}
			if (transform.name == "AxeClimbingHeld")
			{
				this.climbingAxeCollider = transform.GetComponentInChildren<Collider>();
			}
			if (transform.name == "boneHeld")
			{
				this.boneHeldCollider = transform.GetComponentInChildren<Collider>();
			}
			if (transform.name == "turtleShellHeld")
			{
				this.turtleShellHeldCollider = transform.GetComponentInChildren<Collider>();
			}
			if (transform.name == "KatanaHeld")
			{
				this.katanaHeldCollider = transform.GetComponentInChildren<Collider>();
			}
			if (transform.name == "spearHeldUpgraded")
			{
				this.spearUpgradedCollider = transform.GetComponentInChildren<Collider>();
			}
			if (transform.name == "TennisRacketHeld")
			{
				this.tennisRacketHeldCollider = transform.GetComponentInChildren<Collider>();
			}
			if (transform.name == "RepairToolHeld")
			{
				this.repairToolHeldCollider = transform.GetComponentInChildren<Collider>();
			}
			if (transform.name == "CodHeld")
			{
				this.codCollider = transform.GetComponentInChildren<Collider>();
			}
			if (transform.name == "GenericMeatHeld")
			{
				this.meatHeldCollider = transform.GetComponentInChildren<Collider>();
			}
			if (transform.name == "LizardHeld")
			{
				this.lizardHeldCollider = transform.GetComponentInChildren<Collider>();
			}
			if (transform.name == "rabbitHeldDead")
			{
				this.rabbitHeldCollider = transform.GetComponentInChildren<Collider>();
			}
			if (transform.name == "SmallGenericMeatHeld")
			{
				this.smallMeatHeldCollider = transform.GetComponentInChildren<Collider>();
			}
		}
	}

	
	private void setAttackFalse(bool setAttack)
	{
		if (this.Remote)
		{
			return;
		}
		int value = UnityEngine.Random.Range(0, 2);
		this.animator.SetIntegerReflected("randInt1", value);
		this.animator.SetBoolReflected("attack", setAttack);
		this.animator.SetBoolReflected("AxeAttack", setAttack);
	}

	
	private void enableWeapon()
	{
		if (this.Remote)
		{
			return;
		}
		if (this.mainWeaponCollider)
		{
			this.mainWeaponCollider.enabled = true;
		}
		this.axePlaneCollider.enabled = true;
		this.axeCraftedCollider.enabled = true;
		this.stickCollider1.enabled = true;
		this.axeCollider.enabled = true;
		this.rockCollider.enabled = true;
		this.spearCollider.enabled = true;
		this.axeRustyCollider.enabled = true;
		this.armHeldCollider.enabled = true;
		this.legHeldCollider.enabled = true;
		this.headHeldCollider.enabled = true;
		this.clubCraftedHeldCollider.enabled = true;
		this.clubHeldCollider.enabled = true;
		this.skullHeldCollider.enabled = true;
		this.stickUpgradedHeldCollider.enabled = true;
		this.rockUpgradedHeldCollider.enabled = true;
		this.climbingAxeCollider.enabled = true;
		this.boneHeldCollider.enabled = true;
		this.turtleShellHeldCollider.enabled = true;
		this.katanaHeldCollider.enabled = true;
		this.spearUpgradedCollider.enabled = true;
		this.tennisRacketHeldCollider.enabled = true;
		this.repairToolHeldCollider.enabled = true;
		if (this.heldWeaponCollider)
		{
			this.heldWeaponCollider.enabled = true;
		}
	}

	
	private void enableWeaponMP()
	{
		if (!BoltNetwork.isClient)
		{
			return;
		}
		Debug.Log("doing enable weapon MP");
		this.enableWeapon();
	}

	
	private void enableWeapon2()
	{
		if (this.Remote)
		{
			return;
		}
		this.stickCollider2.enabled = true;
		this.stickUpgradedHeldCollider2.enabled = true;
	}

	
	private void enableSmashWeapon()
	{
		if (this.Remote)
		{
			return;
		}
		this.axeCraftedCollider.enabled = true;
		this.stickCollider2.enabled = true;
		this.axeCollider.enabled = true;
		this.rockCollider.enabled = true;
		this.spearCollider.enabled = true;
		this.axePlaneCollider.enabled = true;
		this.axeRustyCollider.enabled = true;
		this.headHeldCollider.enabled = true;
		LocalPlayer.AnimControl.smashBool = true;
		this.armHeldCollider.enabled = true;
		this.legHeldCollider.enabled = true;
		this.skullHeldCollider.enabled = true;
		this.clubHeldCollider.enabled = true;
		this.clubCraftedHeldCollider.enabled = true;
		this.stickUpgradedHeldCollider2.enabled = true;
		this.rockUpgradedHeldCollider.enabled = true;
		this.climbingAxeCollider.enabled = true;
		this.boneHeldCollider.enabled = true;
		this.katanaHeldCollider.enabled = true;
		this.spearUpgradedCollider.enabled = true;
		this.tennisRacketHeldCollider.enabled = true;
		this.repairToolHeldCollider.enabled = true;
		if (this.heldWeaponCollider)
		{
			this.heldWeaponCollider.enabled = true;
		}
	}

	
	public void disableWeapon()
	{
		if (this.Remote)
		{
			return;
		}
		LocalPlayer.Inventory.AttackEnded.Invoke();
		if (this.mainWeaponCollider)
		{
			this.mainWeaponCollider.enabled = false;
		}
		this.axeCraftedCollider.enabled = false;
		this.stickCollider1.enabled = false;
		this.stickCollider2.enabled = false;
		this.axeCollider.enabled = false;
		this.rockCollider.enabled = false;
		this.spearCollider.enabled = false;
		this.axeRustyCollider.enabled = false;
		this.axePlaneCollider.enabled = false;
		this.armHeldCollider.enabled = false;
		this.legHeldCollider.enabled = false;
		this.headHeldCollider.enabled = false;
		this.clubCraftedHeldCollider.enabled = false;
		this.clubHeldCollider.enabled = false;
		this.skullHeldCollider.enabled = false;
		this.stickUpgradedHeldCollider.enabled = false;
		this.stickUpgradedHeldCollider2.enabled = false;
		this.rockUpgradedHeldCollider.enabled = false;
		this.climbingAxeCollider.enabled = false;
		this.boneHeldCollider.enabled = false;
		this.turtleShellHeldCollider.enabled = false;
		this.katanaHeldCollider.enabled = false;
		this.spearUpgradedCollider.enabled = false;
		this.tennisRacketHeldCollider.enabled = false;
		this.repairToolHeldCollider.enabled = false;
		if (this.heldWeaponCollider)
		{
			this.heldWeaponCollider.enabled = false;
		}
	}

	
	public void disableWeapon2()
	{
		if (this.Remote)
		{
			return;
		}
		LocalPlayer.Inventory.AttackEnded.Invoke();
		this.stickCollider1.enabled = false;
		this.stickCollider2.enabled = false;
		this.stickUpgradedHeldCollider2.enabled = false;
	}

	
	public void disableSmashWeapon()
	{
		if (this.Remote)
		{
			return;
		}
		LocalPlayer.Inventory.AttackEnded.Invoke();
		this.axeCraftedCollider.enabled = false;
		this.stickCollider1.enabled = false;
		this.stickCollider2.enabled = false;
		this.axeCollider.enabled = false;
		this.rockCollider.enabled = false;
		this.spearCollider.enabled = false;
		LocalPlayer.AnimControl.smashBool = false;
		this.axeRustyCollider.enabled = false;
		this.axePlaneCollider.enabled = false;
		this.armHeldCollider.enabled = false;
		this.legHeldCollider.enabled = false;
		this.headHeldCollider.enabled = false;
		this.skullHeldCollider.enabled = false;
		this.stickUpgradedHeldCollider.enabled = false;
		this.stickUpgradedHeldCollider2.enabled = false;
		this.rockUpgradedHeldCollider.enabled = false;
		this.clubHeldCollider.enabled = false;
		this.clubCraftedHeldCollider.enabled = false;
		this.climbingAxeCollider.enabled = false;
		this.boneHeldCollider.enabled = false;
		this.katanaHeldCollider.enabled = false;
		this.spearUpgradedCollider.enabled = false;
		this.tennisRacketHeldCollider.enabled = false;
		this.repairToolHeldCollider.enabled = false;
		if (this.heldWeaponCollider)
		{
			this.heldWeaponCollider.enabled = true;
		}
	}

	
	private void doStaminaDrain(float amount)
	{
		if (this.Remote)
		{
			return;
		}
		LocalPlayer.ScriptSetup.stats.setStamina(amount);
	}

	
	private void disableHitFloat()
	{
		if (this.Remote)
		{
			return;
		}
		this.animator.SetFloatReflected("weaponHit", 0f);
	}

	
	private void enableDig()
	{
		if (this.Remote)
		{
			return;
		}
		this.dig.doDig();
	}

	
	private void goToCombo()
	{
		if (this.Remote)
		{
			return;
		}
		LocalPlayer.ScriptSetup.pmControl.SendEvent("goToCombo");
	}

	
	private void goToStickCombo()
	{
		if (this.Remote)
		{
			return;
		}
		LocalPlayer.ScriptSetup.pmControl.SendEvent("goToStickCombo");
	}

	
	private void testEvent()
	{
		if (this.Remote)
		{
			return;
		}
	}

	
	private void setHitDirection(int dir)
	{
		if (this.Remote)
		{
			return;
		}
		this.animator.SetIntegerReflected("hitDirection", dir);
	}

	
	private void goToReset()
	{
		if (this.Remote)
		{
			return;
		}
		LocalPlayer.ScriptSetup.pmControl.SendEvent("goToReset");
	}

	
	private void PlayWeaponOneshot(string path)
	{
		FMODCommon.PlayOneshotNetworked(path, LocalPlayer.ScriptSetup.weaponRight, FMODCommon.NetworkRole.Server);
	}

	
	private void PlayRaftPaddle()
	{
		if (this.Remote)
		{
			return;
		}
		objectOnWater componentInParent = base.transform.GetComponentInParent<objectOnWater>();
		if (componentInParent && componentInParent._bouyancy.inWaterCounter == 0)
		{
			return;
		}
		FMODCommon.PlayOneshotNetworked("event:/player/actions/paddling", base.transform, FMODCommon.NetworkRole.Server);
	}

	
	private void soundStickSwoosh()
	{
		if (this.Remote)
		{
			return;
		}
		if (!LocalPlayer.Inventory.IsRightHandEmpty())
		{
			if (LocalPlayer.Inventory.IsWeaponBurning)
			{
				this.PlayWeaponOneshot(this.fireStickSwooshEvent.eventPath);
			}
			else
			{
				int itemId = LocalPlayer.Inventory.RightHand._itemId;
				if (this.eventsByItemIdCache.ContainsKey(itemId))
				{
					this.PlayWeaponOneshot(this.eventsByItemIdCache[itemId].eventPath);
				}
				else
				{
					this.PlayWeaponOneshot(this.axeSwooshEvent.eventPath);
				}
			}
		}
	}

	
	private void soundAxeSwoosh()
	{
		if (this.Remote)
		{
			return;
		}
		if (LocalPlayer.Inventory.IsWeaponBurning)
		{
			this.PlayWeaponOneshot(this.fireStickSwooshEvent.eventPath);
		}
		else
		{
			this.PlayWeaponOneshot(this.axeSwooshEvent.eventPath);
		}
	}

	
	private void soundRockSwoosh()
	{
		if (this.Remote)
		{
			return;
		}
		this.PlayWeaponOneshot(this.rockSwooshEvent.eventPath);
	}

	
	private void soundSpearSwoosh()
	{
		if (this.Remote)
		{
			return;
		}
		this.PlayWeaponOneshot(this.spearSwooshEvent.eventPath);
	}

	
	private void soundBreatheIn()
	{
		if (this.Remote)
		{
			return;
		}
		FMODCommon.PlayOneshot(this.breatheInEvent, base.transform);
	}

	
	private void soundBreatheOut()
	{
		if (this.Remote)
		{
			return;
		}
		FMODCommon.PlayOneshot(this.breatheOutEvent, base.transform);
	}

	
	private void PlayFallEvent(float fallParameterValue)
	{
		EventInstance @event = FMOD_StudioSystem.instance.GetEvent(this.fallEvent);
		UnityUtil.ERRCHECK(@event.setParameterValue("fall", fallParameterValue));
		UnityUtil.ERRCHECK(@event.set3DAttributes(UnityUtil.to3DAttributes(base.gameObject, null)));
		UnityUtil.ERRCHECK(@event.start());
		UnityUtil.ERRCHECK(@event.release());
	}

	
	public override void OnEvent(SfxFallLight evnt)
	{
		this.PlayFallEvent(0f);
	}

	
	private void soundFallLight()
	{
		if (this.Remote)
		{
			return;
		}
		if (BoltNetwork.isRunning)
		{
			SfxFallLight.Raise(base.entity, EntityTargets.EveryoneExceptOwner).Send();
		}
		this.PlayFallEvent(0f);
	}

	
	public override void OnEvent(SfxFallHeavy evnt)
	{
		this.PlayFallEvent(1f);
	}

	
	private void soundFallHeavy()
	{
		if (this.Remote)
		{
			return;
		}
		if (BoltNetwork.isRunning)
		{
			SfxFallHeavy.Raise(base.entity, EntityTargets.EveryoneExceptOwner).Send();
		}
		this.PlayFallEvent(1f);
	}

	
	private void PlayLighterSpark()
	{
		if (this.Remote)
		{
			return;
		}
		LocalPlayer.Sfx.PlayLighterSound();
	}

	
	private void playDrawBow()
	{
		if (this.Remote)
		{
			return;
		}
		FMODCommon.PlayOneshot(this.bowDrawEvent.eventPath, LocalPlayer.ScriptSetup.leftHand);
	}

	
	private void playChargedSound()
	{
		if (this.Remote)
		{
			return;
		}
	}

	
	private void resetAnimLayer(int l)
	{
		if (this.Remote)
		{
			return;
		}
		this.animator.SetLayerWeightReflected(l, 0f);
	}

	
	private void exitClimbRope()
	{
		if (this.Remote)
		{
			return;
		}
		LocalPlayer.AnimControl.exitClimbMode();
	}

	
	private void resetAnimSpine()
	{
		if (this.Remote)
		{
			return;
		}
		LocalPlayer.ScriptSetup.pmControl.SendEvent("toResetSpine");
	}

	
	private void enableArmIK()
	{
	}

	
	private void skinnedBloodBurst()
	{
		if (this.Remote)
		{
			return;
		}
		Vector3 vector = this.axePlaneCollider.transform.position;
		vector += LocalPlayer.MainCamTr.transform.right * 0.4f + LocalPlayer.MainCamTr.forward * 0.2f;
		Prefabs.Instance.SpawnBloodHitPS(0, vector, LocalPlayer.MainCamTr.transform.rotation);
		Prefabs.Instance.SpawnBloodHitPS(1, vector, LocalPlayer.MainCamTr.transform.rotation);
		Prefabs.Instance.SpawnBloodHitPS(2, vector, LocalPlayer.MainCamTr.transform.rotation);
	}

	
	public void disableArmIK()
	{
		if (this.Remote)
		{
			return;
		}
		if (this.armIK.IsActive)
		{
			this.armIK.IsActive = false;
		}
	}

	
	private void disableSpine()
	{
		if (this.Remote)
		{
			return;
		}
		base.StartCoroutine("smoothDisableSpine");
	}

	
	public void enableSpine()
	{
		if (this.Remote)
		{
			return;
		}
		base.StopCoroutine("smoothDisableSpine");
		this.animator.SetLayerWeightReflected(4, 1f);
	}

	
	private void enableDrawBowBlend()
	{
		if (this.Remote)
		{
			return;
		}
		base.StartCoroutine("drawBowBlend");
	}

	
	private void disableMainRotator()
	{
		if (this.Remote)
		{
			return;
		}
	}

	
	private void enableMainRotator()
	{
		if (this.Remote)
		{
			return;
		}
	}

	
	private void enableHeavyAttackRange()
	{
		if (this.Remote)
		{
			return;
		}
		this.hitTriggerTr.localScale = new Vector3(3f, this.hitTriggerTr.localScale.y, this.hitTriggerTr.localScale.z);
		base.Invoke("disableHeavyAttackRange", 1f);
	}

	
	private void disableHeavyAttackRange()
	{
		if (this.Remote)
		{
			return;
		}
		this.hitTriggerTr.localScale = new Vector3(1f, this.hitTriggerTr.localScale.y, this.hitTriggerTr.localScale.z);
	}

	
	private void staminaDrain(float amount)
	{
		if (this.Remote)
		{
			return;
		}
		LocalPlayer.Stats.Stamina -= amount;
	}

	
	private void lightFire()
	{
		if (this.Remote)
		{
			return;
		}
		int layerMask = 268435456;
		Collider[] array = Physics.OverlapSphere(LocalPlayer.ScriptSetup.leftHand.position, 5f, layerMask);
		foreach (Collider collider in array)
		{
			collider.gameObject.SendMessage("receiveLightFire", SendMessageOptions.DontRequireReceiver);
		}
	}

	
	public void resetDrinkParams()
	{
		if (this.Remote)
		{
			return;
		}
		if (LocalPlayer.FpCharacter.drinking)
		{
			LocalPlayer.Create.Grabber.gameObject.SetActive(true);
			LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.RightHand);
			LocalPlayer.FpCharacter.Locked = false;
			LocalPlayer.FpCharacter.CanJump = true;
			LocalPlayer.CamFollowHead.lockYCam = false;
			LocalPlayer.CamFollowHead.smoothLock = false;
			LocalPlayer.MainRotator.enabled = true;
			LocalPlayer.CamRotator.stopInput = false;
			LocalPlayer.CamRotator.rotationRange = new Vector2(LocalPlayer.FpCharacter.minCamRotationRange, 0f);
			LocalPlayer.FpCharacter.enabled = true;
			LocalPlayer.FpCharacter.Locked = false;
			base.StartCoroutine("smoothEnableSpine");
			base.StartCoroutine(LocalPlayer.AnimControl.smoothDisableLayerNew(2));
			LocalPlayer.FpCharacter.drinking = false;
			LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("noControl").Value = false;
		}
	}

	
	private void resetLegTurns()
	{
		if (this.Remote)
		{
			return;
		}
		if ((double)this.animator.GetFloat("overallSpeed") > 0.2)
		{
			LocalPlayer.AnimControl.resetLegInt();
		}
	}

	
	private void throwSpear()
	{
		if (this.Remote)
		{
			return;
		}
		if (LocalPlayer.AnimControl.currLayerState1.tagHash == LocalPlayer.AnimControl.knockBackHash || LocalPlayer.AnimControl.nextLayerState1.tagHash == LocalPlayer.AnimControl.knockBackHash || LocalPlayer.AnimControl.fullBodyState2.tagHash == LocalPlayer.AnimControl.knockBackHash || LocalPlayer.AnimControl.nextFullBodyState2.tagHash == LocalPlayer.AnimControl.knockBackHash)
		{
			return;
		}
		if (!LocalPlayer.Inventory.IsSlotLocked(Item.EquipmentSlot.RightHand))
		{
			if (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._spearId))
			{
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.spearSpawn, this.spearThrowPos.position, this.spearThrowPos.rotation);
				Rigidbody component = gameObject.GetComponent<Rigidbody>();
				component.AddForce(gameObject.transform.up * this.throwForce * (0.016666f / Time.fixedDeltaTime), ForceMode.Force);
				LocalPlayer.Inventory.RightHand._held.SendMessage("OnProjectileThrown", gameObject, SendMessageOptions.DontRequireReceiver);
				if (LocalPlayer.Inventory.AmountOf(this._spearId, true) > 1)
				{
					LocalPlayer.Inventory.MemorizeOverrideItem(Item.EquipmentSlot.RightHand);
				}
				LocalPlayer.Inventory.UnequipItemAtSlot(Item.EquipmentSlot.RightHand, false, false, true);
			}
			else if (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._spearUpgradedId))
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(this.spearSpawn_upgraded, this.spearThrowPos_upgraded.position, this.spearThrowPos_upgraded.rotation);
				Rigidbody component2 = gameObject2.GetComponent<Rigidbody>();
				component2.AddForce(gameObject2.transform.up * this.throwForce * 1.25f * (0.016666f / Time.fixedDeltaTime), ForceMode.Force);
				LocalPlayer.Inventory.RightHand._held.SendMessage("OnProjectileThrown", gameObject2, SendMessageOptions.DontRequireReceiver);
				if (LocalPlayer.Inventory.AmountOf(this._spearUpgradedId, true) > 1)
				{
					LocalPlayer.Inventory.MemorizeOverrideItem(Item.EquipmentSlot.RightHand);
				}
				LocalPlayer.Inventory.UnequipItemAtSlot(Item.EquipmentSlot.RightHand, false, false, true);
			}
		}
	}

	
	public void smoothLockCam()
	{
	}

	
	public IEnumerator smoothDisableSpine()
	{
		base.StopCoroutine("smoothEnableSpine");
		float t = 0f;
		float startVal = this.animator.GetLayerWeight(4);
		float val = startVal;
		while (t < 1f)
		{
			t += Time.deltaTime * 2f;
			val = Mathf.Lerp(startVal, 0f, t);
			this.animator.SetLayerWeightReflected(4, val);
			yield return null;
		}
		yield break;
	}

	
	public IEnumerator smoothEnableSpine()
	{
		base.StopCoroutine("smoothDisableSpine");
		float t = 0f;
		float startVal = this.animator.GetLayerWeight(4);
		float val = startVal;
		while (t < 1f)
		{
			t += Time.deltaTime * 2f;
			val = Mathf.Lerp(startVal, 1f, t);
			this.animator.SetLayerWeightReflected(4, val);
			yield return null;
		}
		yield break;
	}

	
	private IEnumerator drawBowBlend()
	{
		if (this.Remote)
		{
			yield break;
		}
		float t = 0f;
		float val = 1f;
		while (t < 1f)
		{
			t += Time.deltaTime * 2f;
			val = Mathf.Lerp(1f, 0f, t);
			this.animator.SetLayerWeightReflected(4, val);
			this.animator.SetFloatReflected("blendFloat", val);
			yield return null;
		}
		yield return new WaitForSeconds(1f);
		this.animator.SetFloatReflected("blendFloat", 1f);
		yield break;
	}

	
	private void syncBookHeld()
	{
		if (this.bookHeld && LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Book && LocalPlayer.Inventory.IsSlotAndNextSlotEmpty(Item.EquipmentSlot.RightHand) && !LocalPlayer.Animator.GetBool("resetBook"))
		{
			this.bookHeld.SetActive(true);
			this.bookHeld.SendMessage("setOpenBook", SendMessageOptions.DontRequireReceiver);
		}
	}

	
	private void lockRotation()
	{
	}

	
	private void unlockRotation()
	{
	}

	
	private IEnumerator axeStuckInGround()
	{
		if (this.Remote)
		{
			yield break;
		}
		LocalPlayer.MainRotator.rotationSpeed = 0.55f;
		yield return YieldPresets.WaitPointFiveSeconds;
		LocalPlayer.MainRotator.rotationSpeed = 5f;
		yield break;
	}

	
	public IEnumerator axeHitTree()
	{
		if (this.Remote)
		{
			yield break;
		}
		if (this.cuttingTree)
		{
			LocalPlayer.HitReactions.enableControllerFreeze();
			LocalPlayer.MainRotator.rotationSpeed = 0.6f;
			yield return YieldPresets.WaitPointThreeSeconds;
			LocalPlayer.HitReactions.disableControllerFreeze();
			LocalPlayer.MainRotator.rotationSpeed = 5f;
		}
		yield break;
	}

	
	public void resetCuttingTree()
	{
		this.cuttingTree = false;
	}

	
	private IEnumerator fixRotation()
	{
		if (this.Remote)
		{
			yield break;
		}
		if (!this.introCutScene)
		{
			yield break;
		}
		if (this.Remote)
		{
			yield break;
		}
		float t = 0f;
		Quaternion val = LocalPlayer.Transform.rotation;
		GameObject o = new GameObject("tempOrient");
		GameObject p = new GameObject("playerOrient");
		p.transform.position = LocalPlayer.PlayerBase.transform.position;
		p.transform.rotation = LocalPlayer.Transform.rotation;
		o.transform.rotation = LocalPlayer.Transform.rotation;
		o.transform.localEulerAngles = new Vector3(0f, o.transform.localEulerAngles.y, 0f);
		while (t < 1f)
		{
			t += Time.deltaTime;
			val = Quaternion.Slerp(val, o.transform.rotation, t);
			LocalPlayer.Transform.rotation = val;
			yield return null;
		}
		UnityEngine.Object.Destroy(o);
		UnityEngine.Object.Destroy(p);
		yield break;
	}

	
	private void startCheckArms()
	{
		if (this.Remote)
		{
			return;
		}
		if (!this.introCutScene)
		{
			return;
		}
		if (this.Remote)
		{
			return;
		}
		LocalPlayer.CamFollowHead.smoothUnLock = true;
		LocalPlayer.AnimControl.StartCoroutine("smoothEnableLayerNew", 1);
		LocalPlayer.AnimControl.StartCoroutine("smoothEnableLayerNew", 4);
		base.Invoke("setCheckArms", 0.5f);
		base.Invoke("resetCheckArms", 2.5f);
		LocalPlayer.Stats.PlayWakeMusic();
		base.Invoke("disableCutSceneBool", 1.2f);
		base.Invoke("disableBlockCam", 1.5f);
		this.introCutScene = false;
	}

	
	private void disableBlockCam()
	{
		if (this.Remote)
		{
			return;
		}
		LocalPlayer.AnimControl.blockCamX = false;
	}

	
	private void disableCutSceneBool()
	{
		if (this.Remote)
		{
			return;
		}
		LocalPlayer.AnimControl.introCutScene = false;
	}

	
	private void setCheckArms()
	{
		if (this.Remote)
		{
			return;
		}
		this.animator.SetBoolReflected("checkArms", true);
	}

	
	private void resetCheckArms()
	{
		if (this.Remote)
		{
			return;
		}
		LocalPlayer.Animator.SetBoolReflected("checkArms", false);
	}

	
	private void enableToy()
	{
		if (this.toyHeld)
		{
			this.toyHeld.SetActive(true);
			this.toyHeld.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
			this.toyHeld.transform.localPosition = new Vector3(0.0016f, 0.0125f, -0.1412f);
			this.toyHeld.transform.localEulerAngles = new Vector3(20.353f, 88.137f, -96.11f);
			Transform[] componentsInChildren = this.toyHeld.GetComponentsInChildren<Transform>(true);
			foreach (Transform transform in componentsInChildren)
			{
				transform.gameObject.SetActive(true);
			}
		}
	}

	
	private void switchToy()
	{
		LocalPlayer.SpecialActions.SendMessage("switchTimmyToy", this.toyHeld, SendMessageOptions.DontRequireReceiver);
	}

	
	private void activateNextBedScreen()
	{
		LocalPlayer.SpecialActions.SendMessage("activateNextScreen");
	}

	
	private void timmyRemovedFromArtifact()
	{
		LocalPlayer.SpecialActions.SendMessage("timmyRemovedFromArtifact");
	}

	
	private void allTubesConnected()
	{
		LocalPlayer.SpecialActions.SendMessage("allTubesConnected");
	}

	
	private void enableAmmoBallHeld()
	{
		this.ammoBallHeld.SetActive(true);
		base.Invoke("disableAmmoBallHeld", 1f);
	}

	
	private void disableAmmoBallHeld()
	{
		this.ammoBallHeld.SetActive(false);
	}

	
	private void toggleCassettePlayer()
	{
		LocalPlayer.SpecialItems.SendMessage("disableWaitForWalkman", SendMessageOptions.DontRequireReceiver);
	}

	
	private void leftArmSplash()
	{
		if (this.animator.IsInTransition(0) || this.animator.GetFloat("overallSpeed") < 0.05f)
		{
			return;
		}
		Vector3 position = this.leftArmSplashPos.transform.position;
		bool flag = this.buoyancy.InWater && this.buoyancy.WaterLevel - base.transform.position.y < 3.9f;
		if (flag && this.buoyancy.WaterCollider != null)
		{
			position.y = this.buoyancy.WaterCollider.bounds.center.y + this.buoyancy.WaterCollider.bounds.extents.y;
			PoolManager.Pools["Particles"].Spawn(this.waterArmParticle.transform, position, Quaternion.identity);
		}
	}

	
	private void rightArmSplash()
	{
		if (this.animator.IsInTransition(0) || this.animator.GetFloat("overallSpeed") < 0.05f)
		{
			return;
		}
		Vector3 position = this.rightArmSplashPos.transform.position;
		bool flag = this.buoyancy.InWater && this.buoyancy.WaterLevel - base.transform.position.y < 3.9f;
		if (flag && this.buoyancy.WaterCollider != null)
		{
			position.y = this.buoyancy.WaterCollider.bounds.center.y + this.buoyancy.WaterCollider.bounds.extents.y;
			PoolManager.Pools["Particles"].Spawn(this.waterArmParticle.transform, position, Quaternion.identity);
		}
	}

	
	private void stepLeft()
	{
		if (this.animator.IsInTransition(0) || this.animator.GetFloat("overallSpeed") < 0.05f)
		{
			return;
		}
		if (this.IsOnSnow() || CoopPeerStarter.DedicatedHost)
		{
			return;
		}
		Vector3 position = this.leftFootSpawnPos.transform.position;
		position.y = Terrain.activeTerrain.SampleHeight(this.leftFootSpawnPos.position) + Terrain.activeTerrain.transform.position.y;
		float f = position.y - this.leftFootSpawnPos.position.y;
		bool flag = Mathf.Abs(f) <= 1f;
		int prominantTextureIndex = TerrainHelper.GetProminantTextureIndex(base.transform.position);
		if (this.Remote && this.currState2.shortNameHash != this.raftIdleHash && this.currState2.shortNameHash != this.raftPaddleHash)
		{
			this.playFootstepIfRemote(prominantTextureIndex, flag, false);
		}
		if (!flag)
		{
			return;
		}
		if (this.Remote && this.vis.localplayerDist > 35f)
		{
			return;
		}
		if (this.water.RainIntensity > 0.6f)
		{
			PoolManager.Pools["Particles"].Spawn(this.waterFootParticle.transform, position, Quaternion.identity);
		}
		else if (prominantTextureIndex == 6 || prominantTextureIndex == 1)
		{
			PoolManager.Pools["Particles"].Spawn(this.leafFootParticle.transform, position, Quaternion.identity);
		}
		else if (prominantTextureIndex == 3 || prominantTextureIndex == 0)
		{
			PoolManager.Pools["Particles"].Spawn(this.dustFootParticle.transform, position, Quaternion.identity);
		}
		else if (prominantTextureIndex == 4)
		{
			PoolManager.Pools["Particles"].Spawn(this.sandFootParticle.transform, position, Quaternion.identity);
		}
	}

	
	private void stepRight()
	{
		if (this.animator.IsInTransition(0) || this.animator.GetFloat("overallSpeed") < 0.05f)
		{
			return;
		}
		if (this.IsOnSnow() || CoopPeerStarter.DedicatedHost)
		{
			return;
		}
		Vector3 position = this.rightFootSpawnPos.transform.position;
		position.y = Terrain.activeTerrain.SampleHeight(this.rightFootSpawnPos.position) + Terrain.activeTerrain.transform.position.y;
		float f = position.y - this.rightFootSpawnPos.position.y;
		bool flag = Mathf.Abs(f) <= 1f;
		int prominantTextureIndex = TerrainHelper.GetProminantTextureIndex(base.transform.position);
		if (this.Remote && this.currState2.shortNameHash != this.raftIdleHash && this.currState2.shortNameHash != this.raftPaddleHash)
		{
			this.playFootstepIfRemote(prominantTextureIndex, flag, false);
		}
		if (!flag)
		{
			return;
		}
		if (this.Remote && this.vis.localplayerDist > 35f)
		{
			return;
		}
		if (this.water.RainIntensity > 0.6f)
		{
			PoolManager.Pools["Particles"].Spawn(this.waterFootParticle.transform, position, Quaternion.identity);
		}
		else if (prominantTextureIndex == 6 || prominantTextureIndex == 1)
		{
			PoolManager.Pools["Particles"].Spawn(this.leafFootParticle.transform, position, Quaternion.identity);
		}
		else if (prominantTextureIndex == 3 || prominantTextureIndex == 0)
		{
			PoolManager.Pools["Particles"].Spawn(this.dustFootParticle.transform, position, Quaternion.identity);
		}
		else if (prominantTextureIndex == 4)
		{
			PoolManager.Pools["Particles"].Spawn(this.sandFootParticle.transform, position, Quaternion.identity);
		}
	}

	
	private void stepLeftSnow()
	{
		if (this.animator.IsInTransition(0))
		{
			return;
		}
		Vector3 position = this.leftFootSpawnPos.transform.position;
		bool flag = this.buoyancy.InWater && this.buoyancy.WaterLevel - base.transform.position.y < 2f;
		if (!flag)
		{
			if (this.IsOnSnow())
			{
				if (!this.Remote && LocalPlayer.IsInCaves)
				{
					return;
				}
				if (this.Remote && this.aiInfo && this.aiInfo.netInCave)
				{
					return;
				}
				position.y = Terrain.activeTerrain.SampleHeight(this.leftFootSpawnPos.position) + Terrain.activeTerrain.transform.position.y;
				float f = position.y - this.leftFootSpawnPos.position.y;
				bool flag2 = Mathf.Abs(f) <= 1f;
				if (this.Remote && this.currState2.shortNameHash != this.raftIdleHash && this.currState2.shortNameHash != this.raftPaddleHash)
				{
					this.playFootstepIfRemote(-1, flag2, false);
				}
				if (!flag2)
				{
					return;
				}
				if (this.Remote && this.vis.localplayerDist > 35f)
				{
					return;
				}
				PoolManager.Pools["Particles"].Spawn(this.snowFootParticle.transform, position, Quaternion.identity);
			}
			return;
		}
		if (this._stats.inYacht)
		{
			return;
		}
		if (this.buoyancy.WaterCollider != null)
		{
			position.y = this.buoyancy.WaterCollider.bounds.center.y + this.buoyancy.WaterCollider.bounds.extents.y;
			PoolManager.Pools["Particles"].Spawn(this.waterFootParticle.transform, position, Quaternion.identity);
			PoolManager.Pools["Particles"].Spawn(this.waterLegParticle.transform, position, Quaternion.identity);
		}
	}

	
	private void stepRightSnow()
	{
		if (this.animator.IsInTransition(0))
		{
			return;
		}
		Vector3 position = this.rightFootSpawnPos.transform.position;
		bool flag = this.buoyancy.InWater && this.buoyancy.WaterLevel - base.transform.position.y < 2f;
		if (!flag)
		{
			if (this.IsOnSnow())
			{
				if (!this.Remote && LocalPlayer.IsInCaves)
				{
					return;
				}
				if (this.Remote && this.aiInfo && this.aiInfo.netInCave)
				{
					return;
				}
				position.y = Terrain.activeTerrain.SampleHeight(this.rightFootSpawnPos.position) + Terrain.activeTerrain.transform.position.y;
				float f = position.y - this.rightFootSpawnPos.position.y;
				bool flag2 = Mathf.Abs(f) <= 1f;
				if (this.Remote && this.currState2.shortNameHash != this.raftIdleHash && this.currState2.shortNameHash != this.raftPaddleHash)
				{
					this.playFootstepIfRemote(-1, flag2, false);
				}
				if (!flag2)
				{
					return;
				}
				if (this.Remote && this.vis.localplayerDist > 35f)
				{
					return;
				}
				PoolManager.Pools["Particles"].Spawn(this.snowFootParticle.transform, position, Quaternion.identity);
			}
			return;
		}
		if (this._stats.inYacht)
		{
			return;
		}
		if (this.buoyancy.WaterCollider != null)
		{
			position.y = this.buoyancy.WaterCollider.bounds.center.y + this.buoyancy.WaterCollider.bounds.extents.y;
			PoolManager.Pools["Particles"].Spawn(this.waterFootParticle.transform, position, Quaternion.identity);
			PoolManager.Pools["Particles"].Spawn(this.waterLegParticle.transform, position, Quaternion.identity);
		}
	}

	
	private void landHeavy()
	{
		this.playLandSFXIfRemote();
	}

	
	private void landLight()
	{
		this.playLandSFXIfRemote();
	}

	
	public bool IsOnSnow()
	{
		if (base.transform.position.z < -300f && base.transform.position.y > this.snowStartHeight)
		{
			Terrain activeTerrain = Terrain.activeTerrain;
			if (!activeTerrain || this.snowFadeLength <= 0f)
			{
				return true;
			}
			Vector3 vector = activeTerrain.transform.InverseTransformPoint(base.transform.position);
			TerrainData terrainData = activeTerrain.terrainData;
			Vector2 vector2 = new Vector2(vector.x / terrainData.size.x, vector.z / terrainData.size.z);
			Vector3 interpolatedNormal = terrainData.GetInterpolatedNormal(vector2.x, vector2.y);
			float num = (base.transform.position.y - this.snowStartHeight) / this.snowFadeLength;
			num -= (1f - interpolatedNormal.y * interpolatedNormal.y) * 2f;
			num += 0.5f;
			if (num >= 1f || (num > 0f && UnityEngine.Random.value < num))
			{
				return true;
			}
		}
		return false;
	}

	
	private static string getFootstepForPosition(UnderfootSurfaceDetector.SurfaceType surface, bool isTouchingTerrain, int terrainNumber, Vector3 position, bool isOnSnow)
	{
		switch (surface)
		{
		case UnderfootSurfaceDetector.SurfaceType.None:
			if (!isTouchingTerrain)
			{
				return null;
			}
			if (isOnSnow)
			{
				return "event:/player/foots/foot_snow";
			}
			if (terrainNumber < 0)
			{
				terrainNumber = TerrainHelper.GetProminantTextureIndex(position);
			}
			switch (terrainNumber)
			{
			case 0:
				return "event:/player/foots/foot_default";
			case 1:
				return "event:/player/foots/foot_mud";
			case 2:
			case 3:
			case 5:
			case 7:
				return "event:/player/foots/foot_rock";
			case 4:
				return "event:/player/foots/foot_sand";
			case 6:
				return "event:/player/foots/foot_leaves";
			default:
				return "event:/player/foots/foot_default";
			}
			break;
		case UnderfootSurfaceDetector.SurfaceType.Wood:
			return "event:/player/foots/foot_wood";
		case UnderfootSurfaceDetector.SurfaceType.Rock:
			return "event:/player/foots/foot_rock";
		case UnderfootSurfaceDetector.SurfaceType.Carpet:
			return "event:/player/foots/foot_carpet";
		case UnderfootSurfaceDetector.SurfaceType.Dirt:
			return "event:/player/foots/foot_default";
		case UnderfootSurfaceDetector.SurfaceType.Metal:
			return "event:/player/foots/foot_metal";
		case UnderfootSurfaceDetector.SurfaceType.Plastic:
			return "event:/player/foots/foot_plastic";
		case UnderfootSurfaceDetector.SurfaceType.MetalGrate:
			return "event:/player/foots/foot_metal_grate";
		case UnderfootSurfaceDetector.SurfaceType.BrokenGlass:
			return "event:/player/foots/foot_plastic_glass";
		default:
			return "event:/player/foots/foot_default";
		}
	}

	
	private static string getLandSFXForPosition(UnderfootSurfaceDetector.SurfaceType surface, bool isTouchingTerrain, int terrainNumber, bool isOnSnow)
	{
		if (surface != UnderfootSurfaceDetector.SurfaceType.None)
		{
			return "event:/player/foots/foot_jump_default";
		}
		if (!isTouchingTerrain)
		{
			return null;
		}
		if (isOnSnow)
		{
			return "event:/player/foots/foot_jump_snow";
		}
		if (terrainNumber == 1)
		{
			return "event:/player/foots/foot_jump_mud";
		}
		if (terrainNumber != 4)
		{
			return "event:/player/foots/foot_jump_default";
		}
		return "event:/player/foots/foot_jump_sand";
	}

	
	private float calculateSpeedParameter()
	{
		if (!LocalPlayer.FpCharacter)
		{
			return 0f;
		}
		return LocalPlayer.FpCharacter.CalculateSpeedParameter(this.animator.GetFloat("overallSpeed") * LocalPlayer.AnimControl.maxSpeed);
	}

	
	private void playFootstepIfRemote(int terrainNumber, bool isTouchingTerrain, bool isOnSnow)
	{
		if (!this.Remote)
		{
			return;
		}
		string footstepForPosition = animEventsManager.getFootstepForPosition(this.underFoot.Surface, isTouchingTerrain, terrainNumber, base.transform.position, isOnSnow);
		if (!string.IsNullOrEmpty(footstepForPosition))
		{
			FMODCommon.PlayOneshot(footstepForPosition, base.transform.position, new object[]
			{
				"speed",
				this.calculateSpeedParameter(),
				"gore",
				(!this.underFoot.IsOnGore) ? 0f : 1f
			});
		}
	}

	
	private void playLandSFXIfRemote()
	{
		if (!this.Remote)
		{
			return;
		}
		int prominantTextureIndex = TerrainHelper.GetProminantTextureIndex(base.transform.position);
		float num = Terrain.activeTerrain.SampleHeight(this.leftFootSpawnPos.position) + Terrain.activeTerrain.transform.position.y;
		float f = num - this.leftFootSpawnPos.position.y;
		bool isTouchingTerrain = Mathf.Abs(f) <= 1f;
		string landSFXForPosition = animEventsManager.getLandSFXForPosition(this.underFoot.Surface, isTouchingTerrain, prominantTextureIndex, this.IsOnSnow());
		if (!string.IsNullOrEmpty(landSFXForPosition))
		{
			FMODCommon.PlayOneshot(landSFXForPosition, base.transform.position, new object[]
			{
				"gore",
				(!this.underFoot.IsOnGore) ? 0f : 1f
			});
		}
	}

	
	public bool Remote;

	
	private playerAiInfo aiInfo;

	
	private Buoyancy buoyancy;

	
	private CoopUnderfootSurface underFoot;

	
	private FirstPersonHeadBob headBob;

	
	private Animator animator;

	
	private treeHitTrigger hitTrigger;

	
	[SerializeField]
	private simpleIkSolver armIK;

	
	private netPlayerVis vis;

	
	private WaterOnTerrain water;

	
	public Digger dig;

	
	public GameObject bowArrow;

	
	public GameObject spearSpawn;

	
	public GameObject spearSpawn_upgraded;

	
	public Transform spearThrowPos;

	
	public Transform spearThrowPos_upgraded;

	
	public GameObject bookHeld;

	
	public GameObject toyHeld;

	
	public GameObject ammoBallHeld;

	
	private targetStats _stats;

	
	public float throwForce = 1000f;

	
	[ItemIdPicker]
	public int _spearId;

	
	[ItemIdPicker]
	public int _spearUpgradedId;

	
	public Transform hitTriggerTr;

	
	private int hashIdle;

	
	[SerializeField]
	private Collider mainWeaponCollider;

	
	[SerializeField]
	private BoxCollider stickCollider1;

	
	[SerializeField]
	private CapsuleCollider stickCollider2;

	
	[SerializeField]
	private Collider axeCollider;

	
	[SerializeField]
	private Collider axeCraftedCollider;

	
	[SerializeField]
	private Collider rockCollider;

	
	[SerializeField]
	private Collider spearCollider;

	
	[SerializeField]
	private Collider axeRustyCollider;

	
	[SerializeField]
	private Collider axePlaneCollider;

	
	[SerializeField]
	private Collider armHeldCollider;

	
	[SerializeField]
	private Collider legHeldCollider;

	
	[SerializeField]
	private Collider headHeldCollider;

	
	[SerializeField]
	private Collider clubHeldCollider;

	
	[SerializeField]
	private Collider clubCraftedHeldCollider;

	
	[SerializeField]
	private Collider skullHeldCollider;

	
	[SerializeField]
	private Collider stickUpgradedHeldCollider;

	
	[SerializeField]
	private Collider stickUpgradedHeldCollider2;

	
	[SerializeField]
	private Collider rockUpgradedHeldCollider;

	
	[SerializeField]
	private Collider climbingAxeCollider;

	
	[SerializeField]
	private Collider boneHeldCollider;

	
	[SerializeField]
	private Collider turtleShellHeldCollider;

	
	[SerializeField]
	private Collider katanaHeldCollider;

	
	[SerializeField]
	private Collider spearUpgradedCollider;

	
	[SerializeField]
	private Collider tennisRacketHeldCollider;

	
	[SerializeField]
	private Collider repairToolHeldCollider;

	
	[SerializeField]
	private Collider codCollider;

	
	[SerializeField]
	private Collider meatHeldCollider;

	
	[SerializeField]
	private Collider lizardHeldCollider;

	
	[SerializeField]
	private Collider rabbitHeldCollider;

	
	[SerializeField]
	private Collider smallMeatHeldCollider;

	
	public Collider heldWeaponCollider;

	
	[ItemIdPicker]
	public int _lighterId;

	
	public ParticleSystem leafFootParticle;

	
	public ParticleSystem snowFootParticle;

	
	public ParticleSystem dustFootParticle;

	
	public ParticleSystem sandFootParticle;

	
	public ParticleSystem waterFootParticle;

	
	public ParticleSystem waterLegParticle;

	
	public ParticleSystem waterArmParticle;

	
	public Transform leftFootSpawnPos;

	
	public Transform rightFootSpawnPos;

	
	public Transform leftArmSplashPos;

	
	public Transform rightArmSplashPos;

	
	public Collider underFootCollider;

	
	public ItemGroupEvent bowDrawEvent;

	
	public ItemGroupEvent stickSwooshEvent;

	
	public ItemGroupEvent axeSwooshEvent;

	
	public ItemGroupEvent rockSwooshEvent;

	
	public ItemGroupEvent spearSwooshEvent;

	
	public ItemGroupEvent swordSwooshEvent;

	
	public ItemGroupEvent fireStickSwooshEvent;

	
	private Dictionary<int, ItemGroupEvent> eventsByItemIdCache;

	
	public string breatheInEvent;

	
	public string breatheOutEvent;

	
	public string fallEvent;

	
	public string pushSledEvent;

	
	private EventInstance sledEvent;

	
	private ParameterInstance sledSpeedParameter;

	
	private float snowStartHeight;

	
	private float snowFadeLength;

	
	private AnimatorStateInfo currState0;

	
	private AnimatorStateInfo currState2;

	
	private int raftIdleHash = Animator.StringToHash("raftIdle");

	
	private int raftPaddleHash = Animator.StringToHash("raftPaddleLoop");

	
	private Vector3 previousPosition;

	
	public Transform _weaponVelocityTr;

	
	private Vector3 _prevWepPos;

	
	public float _vrWeaponAttackWindow = 0.5f;

	
	public float _vrEnableWeaponVelocity = 30f;

	
	private float _enableWeaponTimer;

	
	[NonSerialized]
	public bool IsSledTurning;

	
	private static int idleToPushSledHash = Animator.StringToHash("idleToPushSled");

	
	private static int pushSledIdleHash = Animator.StringToHash("pushSledIdle");

	
	private static int locomotionHash = Animator.StringToHash("locomotion");

	
	public bool smoothBlock;

	
	public bool cuttingTree;

	
	public bool introCutScene;

	
	private const string FOOTSTEP_DEFAULT = "event:/player/foots/foot_default";

	
	private const string FOOTSTEP_SAND = "event:/player/foots/foot_sand";

	
	private const string FOOTSTEP_MUD = "event:/player/foots/foot_mud";

	
	private const string FOOTSTEP_LEAVES = "event:/player/foots/foot_leaves";

	
	private const string FOOTSTEP_ROCK = "event:/player/foots/foot_rock";

	
	private const string FOOTSTEP_SNOW = "event:/player/foots/foot_snow";

	
	private const string FOOTSTEP_WOOD = "event:/player/foots/foot_wood";

	
	private const string FOOTSTEP_METAL = "event:/player/foots/foot_metal";

	
	private const string FOOTSTEP_CARPET = "event:/player/foots/foot_carpet";

	
	private const string FOOTSTEP_PLASTIC = "event:/player/foots/foot_plastic";

	
	private const string FOOTSTEP_METAL_GRATE = "event:/player/foots/foot_metal_grate";

	
	private const string FOOTSTEP_BROKEN_GLASS = "event:/player/foots/foot_plastic_glass";

	
	private const string LAND_DEFAULT = "event:/player/foots/foot_jump_default";

	
	private const string LAND_SAND = "event:/player/foots/foot_jump_sand";

	
	private const string LAND_MUD = "event:/player/foots/foot_jump_mud";

	
	private const string LAND_SNOW = "event:/player/foots/foot_jump_snow";
}
