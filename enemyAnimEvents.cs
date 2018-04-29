using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using FMOD.Studio;
using PathologicalGames;
using TheForest.Utils;
using TheForest.Utils.Settings;
using UnityEngine;


public class enemyAnimEvents : EntityEventListener
{
	
	public void SetActor(int newActor)
	{
		this.actor = newActor;
	}

	
	private void OnDeserialized()
	{
		this.doStart();
	}

	
	private void OnEnable()
	{
		this.doingLookAt = false;
		if (!this.setup)
		{
			return;
		}
		if (this.setup.bodyCollisionCollider)
		{
			this.setup.bodyCollisionCollider.enabled = true;
		}
	}

	
	private void Start()
	{
		this.doStart();
		if (!CoopPeerStarter.DedicatedHost)
		{
			FMOD_StudioSystem.PreloadEvent(this.walkFootstepEvent);
			FMOD_StudioSystem.PreloadEvent(this.runFootstepEvent);
		}
	}

	
	private void doStart()
	{
		this.feetAudio = base.transform.Find("feetAudioGo").gameObject;
		this.waterDetect = base.transform.parent.GetComponentInChildren<mutantWaterDetect>();
		this.animator = base.gameObject.GetComponent<Animator>();
		this.setup = base.GetComponent<mutantScriptSetup>();
		this.ai = base.GetComponent<mutantAI>();
		this.ai_net = base.GetComponent<mutantAI_net>();
		this.followers = base.transform.root.GetComponent<mutantFollowerFunctions>();
		this.oneshotEvents = new List<FMODCommon.OneshotEventInfo>();
		this.loopingEvents = new List<FMODCommon.LoopingEventInfo>();
		base.InvokeRepeating("cleanupOneshotEvents", 1f, 1f);
		this.idleTimeoutEnd = 0f;
		this.idleStateHash = Animator.StringToHash("Base Layer.idle01");
		this.inIdleState = false;
		if (this.Remote)
		{
			return;
		}
		if (!this.eventPosition)
		{
			this.eventPosition = base.gameObject;
		}
		if (this.parryChecker)
		{
			this.parryChecker.SetActive(false);
		}
		this.actor = UnityEngine.Random.Range(0, 2);
		if (this.setup)
		{
			this.mainWeaponCollider = this.setup.mainWeapon.GetComponent<BoxCollider>();
			this.leftWeaponCollider = this.setup.leftWeapon.GetComponent<Collider>();
		}
		this.defaultWeaponRange = this.mainWeaponCollider.size.z;
		this.defaultWeaponPos = this.mainWeaponCollider.center.z;
	}

	
	private void Update()
	{
		if (!CoopPeerStarter.DedicatedHost)
		{
			FMODCommon.UpdateLoopingEvents(this.loopingEvents, this.animator, base.transform);
		}
		base.transform.hasChanged = false;
		if (this.animator)
		{
			AnimatorStateInfo currentAnimatorStateInfo = this.animator.GetCurrentAnimatorStateInfo(0);
			if (currentAnimatorStateInfo.tagHash == this.idleHash)
			{
				if (this.mainWeaponCollider && this.mainWeaponCollider.enabled)
				{
					this.mainWeaponCollider.enabled = false;
				}
				if (this.leftWeaponCollider && this.leftWeaponCollider.enabled)
				{
					this.leftWeaponCollider.enabled = false;
				}
			}
			if (currentAnimatorStateInfo.fullPathHash == this.idleStateHash)
			{
				if (!this.inIdleState)
				{
					this.idleTimeoutEnd = Time.time + 2f;
				}
				this.inIdleState = true;
			}
			else
			{
				this.inIdleState = false;
			}
		}
	}

	
	private void OnDisable()
	{
		if (this.loopingEvents != null)
		{
			foreach (FMODCommon.LoopingEventInfo loopingEventInfo in this.loopingEvents)
			{
				UnityUtil.ERRCHECK(loopingEventInfo.instance.stop(STOP_MODE.ALLOWFADEOUT));
				UnityUtil.ERRCHECK(loopingEventInfo.instance.release());
			}
			this.loopingEvents.Clear();
		}
		FMODCommon.AdoptOneshotEvents(this.oneshotEvents);
	}

	
	private IEnumerator doParryCheck()
	{
		float t = 0f;
		while (!this.weaponEnabled)
		{
			t += Time.deltaTime;
			if (t > 0.75f)
			{
				yield break;
			}
			if (this.parryChecker)
			{
				this.parryChecker.SetActive(true);
			}
			if (TheForest.Utils.Input.GetButtonDown("AltFire"))
			{
				this.parryBool = true;
				yield break;
			}
			yield return null;
		}
		yield break;
	}

	
	private void setHitDirection(int dir)
	{
	}

	
	private void enableParryCheck()
	{
		if (!CoopPeerStarter.DedicatedHost)
		{
			base.StartCoroutine("doParryCheck");
		}
	}

	
	private void cancelParryCheck()
	{
		if (this.Remote)
		{
			return;
		}
		base.StopCoroutine("doParryCheck");
		if (this.parryChecker)
		{
			this.parryChecker.SetActive(false);
		}
		this.parryBool = false;
	}

	
	private void disableAnimBool(string fromAnim)
	{
		if (this.Remote)
		{
			return;
		}
		this.animator.SetBoolReflected(fromAnim, false);
	}

	
	private void disableDamageBool(bool setDamage)
	{
		if (this.Remote)
		{
			return;
		}
		this.animator.SetBoolReflected("damageBOOL", false);
		if (UnityEngine.Random.Range(0, 5) < 2)
		{
		}
	}

	
	private void enableWeapon()
	{
		if (this.weaponsBlocked)
		{
			return;
		}
		this.leftHandWeapon = true;
		this.weaponEnabled = true;
		this.parryDir = 0;
		if (this.Remote)
		{
			base.CancelInvoke("disableWeapon");
			if (this.mainWeapon_net)
			{
				this.mainWeapon_net.enabled = true;
				base.Invoke("disableWeapon", 0.7f);
			}
			return;
		}
		base.CancelInvoke("disableWeapon");
		this.playWeaponSwoosh();
		if (this.female)
		{
			this.mainWeaponCollider.size = new Vector3(1f, 1f, this.defaultWeaponRange * 0.65f);
			this.mainWeaponCollider.center = new Vector3(0f, 0f, 0f);
			this.mainWeaponCollider.enabled = true;
			if (this.leftWeaponCollider)
			{
				this.leftWeaponCollider.enabled = true;
			}
		}
		else
		{
			this.mainWeaponCollider.size = new Vector3(1f, 1f, this.defaultWeaponRange);
			this.mainWeaponCollider.center = new Vector3(0f, 0f, this.defaultWeaponPos);
			this.mainWeaponCollider.enabled = true;
			if (this.leftWeaponCollider)
			{
				this.leftWeaponCollider.enabled = true;
			}
		}
		base.Invoke("disableWeapon", 0.7f);
	}

	
	public void disableWeapon()
	{
		this.leftHandWeapon = false;
		this.weaponEnabled = false;
		this.parryBool = false;
		if (this.Remote)
		{
			if (this.mainWeapon_net)
			{
				this.mainWeapon_net.enabled = false;
			}
			return;
		}
		if (this.female)
		{
			this.mainWeaponCollider.enabled = false;
			this.mainWeaponCollider.size = new Vector3(1f, 1f, this.defaultWeaponRange);
			this.mainWeaponCollider.center = new Vector3(0f, 0f, this.defaultWeaponPos);
			if (this.leftWeaponCollider)
			{
				this.leftWeaponCollider.enabled = false;
			}
		}
		else
		{
			this.mainWeaponCollider.enabled = false;
			this.mainWeaponCollider.size = new Vector3(1f, 1f, this.defaultWeaponRange);
			this.mainWeaponCollider.center = new Vector3(0f, 0f, this.defaultWeaponPos);
			if (this.leftWeaponCollider)
			{
				this.leftWeaponCollider.enabled = false;
			}
		}
	}

	
	private void enableClawsWeapon()
	{
		if (this.weaponsBlocked)
		{
			return;
		}
		this.weaponEnabled = true;
		this.parryDir = 1;
		if (this.Remote)
		{
			if (this.mainWeapon_net)
			{
				this.mainWeapon_net.enabled = true;
			}
			this.noFireAttack = true;
			base.Invoke("resetNoFireAttack", 1f);
			return;
		}
		this.noFireAttack = true;
		base.Invoke("resetNoFireAttack", 1f);
		this.playWeaponSwoosh();
		this.mainWeaponCollider.size = new Vector3(1f, 1f, this.defaultWeaponRange * 0.65f);
		this.mainWeaponCollider.center = new Vector3(0f, 0f, 0f);
		if (this.female)
		{
			this.mainWeaponCollider.enabled = true;
		}
		else
		{
			this.mainWeaponCollider.enabled = true;
		}
	}

	
	private void resetNoFireAttack()
	{
		this.noFireAttack = false;
	}

	
	public void disableClawsWeapon()
	{
		this.weaponEnabled = false;
		this.parryBool = false;
		if (this.Remote)
		{
			if (this.mainWeapon_net)
			{
				this.mainWeapon_net.enabled = false;
			}
			this.noFireAttack = false;
			return;
		}
		this.noFireAttack = false;
		if (this.female)
		{
			this.mainWeaponCollider.enabled = false;
		}
		else
		{
			this.mainWeaponCollider.enabled = false;
		}
		this.mainWeaponCollider.size = new Vector3(1f, 1f, this.defaultWeaponRange);
		this.mainWeaponCollider.center = new Vector3(0f, 0f, this.defaultWeaponPos);
	}

	
	public void enableRagdoll()
	{
		if (this.Remote)
		{
			return;
		}
	}

	
	private void sendDragEvent()
	{
		if (this.Remote)
		{
			return;
		}
		if (this.setup.familyFunctions.busy && this.setup.familyFunctions.targetFamilyFunctions)
		{
			this.setup.familyFunctions.targetFamilyFunctions.setup.pmCombatScript.toRescue = true;
		}
	}

	
	public void sendDropEvent()
	{
		if (this.Remote)
		{
			return;
		}
		if (this.setup.familyFunctions.targetFamilyFunctions)
		{
			this.setup.familyFunctions.targetFamilyFunctions.setup.pmCombatScript.toDrop = true;
		}
	}

	
	private void doAnimNextEvent()
	{
		if (this.Remote)
		{
			return;
		}
		this.setup.pmCombat.SendEvent("toAnimNext");
	}

	
	private void sendAlertEvent()
	{
		if (this.Remote)
		{
			return;
		}
		this.followers.sendAlertEvent();
	}

	
	public void disableCollision()
	{
		if (this.Remote)
		{
			return;
		}
	}

	
	public void enableCollision()
	{
		if (this.Remote)
		{
			return;
		}
		if (!this.animator.GetBool("deathBOOL"))
		{
			base.transform.root.gameObject.layer = 14;
		}
	}

	
	private void disableJumpCollision()
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
		if (this.setup.collisionDetect.currentJumpCollider)
		{
			if (this.setup.collisionDetect.currentJumpCollider.GetComponent<getStructureStrength>())
			{
				return;
			}
			if (this.setup.controller.enabled && this.setup.collisionDetect.currentJumpCollider.enabled && this.setup.collisionDetect.currentJumpCollider.gameObject.activeInHierarchy && this.setup.controller.gameObject.activeSelf)
			{
				Physics.IgnoreCollision(this.setup.controller, this.setup.collisionDetect.currentJumpCollider, true);
				this.lastJumpCollider = this.setup.collisionDetect.currentJumpCollider;
			}
		}
	}

	
	private void enableJumpCollision()
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
		if (this.lastJumpCollider && this.setup.controller.enabled && this.lastJumpCollider.enabled && this.lastJumpCollider.gameObject.activeInHierarchy && this.setup.controller.gameObject.activeSelf)
		{
			Physics.IgnoreCollision(this.setup.controller, this.lastJumpCollider, false);
		}
	}

	
	private void bombEquip()
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
		if (!this.ai.fireman_dynamite)
		{
			if (this.setup.fireBombGo)
			{
				this.setup.fireBombGo.SetActive(true);
			}
		}
	}

	
	private void bombLight()
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
		if (!this.ai.fireman_dynamite)
		{
			if (this.setup.fireBombGo)
			{
				this.setup.fireBombGo.SendMessage("enableFire", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	
	private void bombThrow()
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
		GameObject gameObject;
		if (this.ai.fireman_dynamite)
		{
			gameObject = (UnityEngine.Object.Instantiate(this.setup.thrownFireBombGo, this.setup.clawsWeapon.transform.position, Quaternion.identity) as GameObject);
		}
		else
		{
			if (this.setup.fireBombGo)
			{
				this.setup.fireBombGo.SetActive(false);
			}
			gameObject = (UnityEngine.Object.Instantiate(this.setup.thrownFireBombGo, this.setup.clawsWeapon.transform.position, Quaternion.identity) as GameObject);
		}
		Transform transform;
		if (this.setup.ai.target.CompareTag("Player") || this.setup.ai.target.CompareTag("PlayerNet"))
		{
			transform = this.setup.ai.target.transform;
		}
		else if (this.setup.ai.lastPlayerTarget != null)
		{
			transform = this.setup.ai.lastPlayerTarget;
		}
		else
		{
			transform = this.setup.ai.target.transform;
		}
		Vector3 vector = base.transform.InverseTransformPoint(transform.position);
		float num = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
		float num2 = Vector3.Distance(base.transform.position, transform.position);
		float num3;
		if (num2 < 20f)
		{
			num3 = 0.65f;
		}
		else if (num2 < 30f)
		{
			num3 = 1.5f;
		}
		else if (num2 < 45f)
		{
			num3 = 2.5f;
		}
		else
		{
			num3 = 2.5f;
		}
		num3 *= GameSettings.Ai.firemanThrowTimeRatio;
		Vector3 target;
		if (num < 45f && num > -45f && num2 > 12f)
		{
			target = transform.position;
		}
		else
		{
			target = base.transform.position + base.transform.forward * 35f;
		}
		Rigidbody component = gameObject.GetComponent<Rigidbody>();
		Vector3 a = this.calculateBestThrowSpeed(this.setup.clawsWeapon.transform.position, target, num3);
		component.AddForce(a * (0.016666f / Time.fixedDeltaTime), ForceMode.VelocityChange);
		component.AddTorque(base.transform.right * 10f, ForceMode.VelocityChange);
	}

	
	public void bombDisable()
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
		if (this.setup.fireBombGo && this.setup.fireBombGo.activeSelf)
		{
			this.setup.fireBombGo.SetActive(false);
		}
	}

	
	private Vector3 calculateBestThrowSpeed(Vector3 origin, Vector3 target, float timeToTarget)
	{
		Vector3 vector = target - origin;
		Vector3 vector2 = vector;
		vector2.y = 0f;
		float y = vector.y;
		float magnitude = vector2.magnitude;
		float y2 = y / timeToTarget + 0.5f * Physics.gravity.magnitude * timeToTarget;
		float d = magnitude / timeToTarget;
		Vector3 vector3 = vector2.normalized;
		vector3 *= d;
		vector3.y = y2;
		return vector3;
	}

	
	public void playPlayerSighted()
	{
	}

	
	public override void OnEvent(SfxEnemyPlayerSighted evnt)
	{
		this.playPlayerSighted();
	}

	
	private float sqrDistanceToLocalPlayer()
	{
		return (base.transform.position - LocalPlayer.Transform.position).sqrMagnitude;
	}

	
	private void playRunStep()
	{
		if (FMOD_StudioSystem.instance && this.sqrDistanceToLocalPlayer() < 1600f)
		{
			FMOD_StudioSystem.instance.PlayOneShot(this.runFootstepEvent, this.feetAudio.transform.position, null);
		}
	}

	
	private void playWalkStep()
	{
		if (FMOD_StudioSystem.instance && this.sqrDistanceToLocalPlayer() < 1600f)
		{
			FMOD_StudioSystem.instance.PlayOneShot(this.walkFootstepEvent, this.feetAudio.transform.position, null);
		}
	}

	
	private void playWeaponSwoosh()
	{
		if (this.Remote)
		{
			this.playFMODEvent(this.weaponSwooshEvent);
		}
		else
		{
			if (!this.Remote && BoltNetwork.isRunning && this.entity && this.entity.isAttached)
			{
				SfxEnemyWeaponSwoosh.Raise(this.entity, EntityTargets.EveryoneExceptOwner).Send();
			}
			if (this.setup.ai.fireman)
			{
				this.playFMODEvent(this.fireWeaponSwooshEvent);
			}
			else
			{
				this.playFMODEvent(this.weaponSwooshEvent);
			}
		}
	}

	
	public override void OnEvent(SfxEnemyWeaponSwoosh evnt)
	{
		this.playWeaponSwoosh();
	}

	
	private void playFallDown()
	{
		if (!this.Remote && BoltNetwork.isRunning && this.entity && this.entity.isAttached)
		{
			SfxEnemyFallDown.Raise(this.entity, EntityTargets.EveryoneExceptOwner).Send();
		}
		this.playFMODEvent(this.fallDownEvent, this.eventPosition, delegate(EventInstance instance)
		{
			instance.setParameterValue("fall", 1f);
		});
	}

	
	public override void OnEvent(SfxEnemyFallDown evnt)
	{
		this.playFallDown();
	}

	
	private void playFallDownShort()
	{
		if (!this.Remote && BoltNetwork.isRunning && this.entity && this.entity.isAttached)
		{
			SfxEnemyFallDownShort.Raise(this.entity, EntityTargets.EveryoneExceptOwner).Send();
		}
		this.playFMODEvent(this.fallDownEvent, this.eventPosition, delegate(EventInstance instance)
		{
			instance.setParameterValue("fall", 0f);
		});
	}

	
	public override void OnEvent(SfxEnemyFallDownShort evnt)
	{
		this.playFallDownShort();
	}

	
	public void playLandGround()
	{
		if (!this.setup)
		{
			return;
		}
		if (!this.setup.typeSetup)
		{
			return;
		}
		if (this.setup.typeSetup.inWater)
		{
			this.playFMODEvent(this.landWaterEvent);
		}
		else
		{
			this.playFMODEvent(this.landGroundEvent);
		}
	}

	
	private void playTreeJumpStart()
	{
		if (!this.Remote && BoltNetwork.isRunning && this.entity && this.entity.isAttached)
		{
			SfxEnemyTreeJumpStart.Raise(this.entity, EntityTargets.EveryoneExceptOwner).Send();
		}
		this.playFMODEvent(this.treeJumpStartEvent, this.feetAudio, null);
	}

	
	public override void OnEvent(SfxEnemyTreeJumpStart evnt)
	{
		this.playTreeJumpStart();
	}

	
	private void playTreeJumpLand()
	{
		if (!this.Remote && BoltNetwork.isRunning && this.entity && this.entity.isAttached)
		{
			SfxEnemyTreeJumpLand.Raise(this.entity, EntityTargets.EveryoneExceptOwner).Send();
		}
		this.playFMODEvent(this.treeJumpLandEvent, this.feetAudio, null);
	}

	
	public void oldWaterSplash()
	{
		if (this.waterDetect.inWater)
		{
			Vector3 position = this.feetAudio.transform.position;
			PoolManager.Pools["Pool_Particles"].Spawn(this.waterSplash.transform, position, Quaternion.identity);
		}
	}

	
	public void doWaterSplash(Collider other)
	{
		if (this.Remote)
		{
			return;
		}
		Vector3 position = this.feetAudio.transform.position;
		position.y += 2f;
		if (other)
		{
			position.y = other.bounds.center.y * other.transform.localScale.y + other.bounds.extents.y + 1f;
		}
		PoolManager.Pools["Particles"].Spawn(this.waterSplash.transform, position, Quaternion.identity);
	}

	
	public void doWalkSplash()
	{
		if (this.Remote)
		{
			return;
		}
		if (this.debugWalkSplash)
		{
			Vector3 position;
			if (this.setup.animControl.fullBodyState.normalizedTime < 0.5f)
			{
				position = this.setup.rightFoot.position;
			}
			else
			{
				position = this.setup.leftFoot.position;
			}
			position.y += 0.5f;
			PoolManager.Pools["Particles"].Spawn(this.waterSplashWalk.transform, position, Quaternion.identity);
			return;
		}
		if (this.waterDetect.currentWaterCollider)
		{
			Vector3 position2;
			if (this.setup.animControl.fullBodyState.normalizedTime < 0.5f)
			{
				position2 = this.setup.rightFoot.position;
			}
			else
			{
				position2 = this.setup.leftFoot.position;
			}
			position2.y = this.waterDetect.currentWaterCollider.bounds.center.y * this.waterDetect.currentWaterCollider.transform.localScale.y + this.waterDetect.currentWaterCollider.bounds.extents.y + 0.6f;
			PoolManager.Pools["Particles"].Spawn(this.waterSplashWalk.transform, position2, Quaternion.identity);
		}
	}

	
	public void doRunSplash()
	{
		if (this.Remote)
		{
			return;
		}
		if (this.debugRunSplash)
		{
			Vector3 position;
			if (this.setup.animControl.fullBodyState.normalizedTime > 0.5f)
			{
				position = this.setup.rightFoot.position;
			}
			else
			{
				position = this.setup.leftFoot.position;
			}
			position.y += 0.5f;
			PoolManager.Pools["Particles"].Spawn(this.waterSplashWalk.transform, position, Quaternion.identity);
			return;
		}
		if (this.waterDetect.currentWaterCollider)
		{
			Vector3 position2;
			if (this.setup.animControl.fullBodyState.normalizedTime > 0.5f)
			{
				position2 = this.setup.rightFoot.position;
			}
			else
			{
				position2 = this.setup.leftFoot.position;
			}
			position2.y = this.waterDetect.currentWaterCollider.bounds.center.y * this.waterDetect.currentWaterCollider.transform.localScale.y + this.waterDetect.currentWaterCollider.bounds.extents.y + 0.6f;
			PoolManager.Pools["Particles"].Spawn(this.waterSplashWalk.transform, position2, Quaternion.identity);
		}
	}

	
	private void setupDodgeEvent()
	{
		this.setup.bodyCollisionCollider.enabled = false;
		base.StartCoroutine(this.smoothLookAtPlayer(0.75f));
		base.Invoke("enableBodyCollider", 0.4f);
	}

	
	private void enableBodyCollider()
	{
		this.setup.bodyCollisionCollider.enabled = true;
	}

	
	public IEnumerator smoothLookAtPlayer(float getTime)
	{
		if (this.doingLookAt)
		{
			yield break;
		}
		float t = 0f;
		while (t < getTime)
		{
			this.doingLookAt = true;
			if (this.setup.ai.target)
			{
				this.setup.pmCombatScript.doSmoothLookAt(this.setup.ai.target.position, 3f);
			}
			t += Time.deltaTime;
			yield return null;
		}
		this.doingLookAt = false;
		yield break;
	}

	
	public void playStalkPlayer()
	{
		this.playFMODEvent(this.stalkPlayerEvent);
	}

	
	public void playSightedScream()
	{
		this.playFMODEvent(this.sightedScreamEvent);
	}

	
	private void playDyingToRecover()
	{
		this.playFMODEvent(this.dyingToRecoverEvent);
	}

	
	private void playDeathStanding()
	{
		this.playFMODEvent(this.deathStandingEvent);
	}

	
	private void playToAttack()
	{
		this.playFMODEvent(this.toAttackEvent);
	}

	
	private void playAlerted()
	{
		this.playFMODEvent(this.alertedEvent);
	}

	
	public override void OnEvent(SfxEnemyTreeJumpLand evnt)
	{
		this.playTreeJumpLand();
	}

	
	private float getMutantType()
	{
		float result = 10f;
		if (this.female)
		{
			result = 0f;
			if (this.femaleSkinny)
			{
				result = 1f;
			}
		}
		else if (this.male)
		{
			result = 2f;
			if (this.maleSkinny)
			{
				result = 3f;
			}
			else if (this.fireman)
			{
				result = 4f;
			}
			else if (this.leader)
			{
				result = 5f;
			}
			else if (this.pale)
			{
				result = 6f;
			}
		}
		else if (this.creepy)
		{
			result = 7f;
		}
		else if (this.creepy_male)
		{
			result = 8f;
		}
		else if (this.creepy_baby)
		{
			result = 9f;
		}
		return result;
	}

	
	private void cleanupOneshotEvents()
	{
		FMODCommon.CleanupOneshotEvents(this.oneshotEvents, true);
	}

	
	private void playFMODEvent(string path)
	{
		this.playFMODEvent(path, this.eventPosition, null);
	}

	
	private void playFMODEvent(string path, GameObject location, Action<EventInstance> setup)
	{
		if (!FMOD_StudioSystem.instance)
		{
			return;
		}
		try
		{
			if (!FMOD_StudioSystem.instance.ShouldBeCulled(path, location.transform.position))
			{
				if (this.loopingEvents == null)
				{
					this.loopingEvents = new List<FMODCommon.LoopingEventInfo>();
				}
				EventDescription eventDescription;
				UnityUtil.ERRCHECK(FMOD_StudioSystem.instance.System.getEvent(path, out eventDescription));
				if (eventDescription == null)
				{
					Debug.LogWarning("Couldn't get event '" + path + "'");
				}
				else
				{
					bool flag;
					UnityUtil.ERRCHECK(eventDescription.isOneshot(out flag));
					if (this.animator)
					{
						AnimatorStateInfo currentAnimatorStateInfo = this.animator.GetCurrentAnimatorStateInfo(0);
						if (currentAnimatorStateInfo.fullPathHash != this.idleStateHash || Time.time >= this.idleTimeoutEnd)
						{
							if (!flag && this.loopingEvents != null)
							{
								foreach (FMODCommon.LoopingEventInfo loopingEventInfo in this.loopingEvents)
								{
									if (loopingEventInfo.startState == currentAnimatorStateInfo.nameHash && loopingEventInfo.path == path)
									{
										return;
									}
								}
							}
							EventInstance eventInstance;
							UnityUtil.ERRCHECK(eventDescription.createInstance(out eventInstance));
							eventInstance.setParameterValue("type", this.getMutantType());
							eventInstance.setParameterValue("actor", (float)this.actor);
							UnityUtil.ERRCHECK(eventInstance.set3DAttributes(UnityUtil.to3DAttributes(location, null)));
							if (setup != null)
							{
								setup(eventInstance);
							}
							UnityUtil.ERRCHECK(eventInstance.start());
							if (flag)
							{
								this.oneshotEvents.Add(new FMODCommon.OneshotEventInfo(eventInstance, true));
							}
							else
							{
								this.loopingEvents.Add(new FMODCommon.LoopingEventInfo(path, eventInstance, currentAnimatorStateInfo.fullPathHash));
							}
						}
					}
				}
			}
		}
		catch (NullReferenceException ex)
		{
			Debug.LogError("Null reference exception playing event '" + path + "':\n" + ex.ToString());
		}
	}

	
	public IEnumerator disableAllWeapons()
	{
		float timer = 0f;
		while (timer < 0.5f)
		{
			this.weaponsBlocked = true;
			this.disableWeapon();
			this.disableClawsWeapon();
			timer += Time.deltaTime;
			yield return null;
		}
		this.weaponsBlocked = false;
		this.leftHandWeapon = false;
		yield break;
	}

	
	
	private bool leader
	{
		get
		{
			return (!this.ai) ? this.ai_net.leader : this.ai.leader;
		}
	}

	
	
	private bool maleSkinny
	{
		get
		{
			return (!this.ai) ? this.ai_net.maleSkinny : this.ai.maleSkinny;
		}
	}

	
	
	private bool femaleSkinny
	{
		get
		{
			return (!this.ai) ? this.ai_net.femaleSkinny : this.ai.femaleSkinny;
		}
	}

	
	
	private bool male
	{
		get
		{
			return (!this.ai) ? this.ai_net.male : this.ai.male;
		}
	}

	
	
	private bool female
	{
		get
		{
			return (!this.ai) ? this.ai_net.female : this.ai.female;
		}
	}

	
	
	private bool creepy
	{
		get
		{
			return (!this.ai) ? this.ai_net.creepy : this.ai.creepy;
		}
	}

	
	
	private bool creepy_male
	{
		get
		{
			return (!this.ai) ? this.ai_net.creepy_male : this.ai.creepy_male;
		}
	}

	
	
	private bool creepy_baby
	{
		get
		{
			return (!this.ai) ? this.ai_net.creepy_baby : this.ai.creepy_baby;
		}
	}

	
	
	private bool fireman
	{
		get
		{
			return (!this.ai) ? this.ai_net.fireman : this.ai.fireman;
		}
	}

	
	
	private bool pale
	{
		get
		{
			return (!this.ai) ? this.ai_net.pale : this.ai.pale;
		}
	}

	
	private const string IDLE_STATE_PATH = "Base Layer.idle01";

	
	private const float IDLE_STATE_TIMEOUT = 2f;

	
	private const float SQR_FOOTSTEP_DISTANCE_THRESHOLD = 1600f;

	
	private Animator animator;

	
	private mutantWaterDetect waterDetect;

	
	private GameObject weapon;

	
	private GameObject clawsWeapon;

	
	private GameObject feetAudio;

	
	private mutantScriptSetup setup;

	
	private mutantAI ai;

	
	private mutantAI_net ai_net;

	
	private mutantFollowerFunctions followers;

	
	public Collider mainWeapon_net;

	
	public BoxCollider mainWeaponCollider;

	
	public Collider leftWeaponCollider;

	
	private float defaultWeaponRange;

	
	private float defaultWeaponPos;

	
	public GameObject waterSplash;

	
	public GameObject waterSplashWalk;

	
	public GameObject waterSplashRun;

	
	private List<FMODCommon.OneshotEventInfo> oneshotEvents;

	
	private List<FMODCommon.LoopingEventInfo> loopingEvents;

	
	public bool Remote;

	
	public GameObject parryChecker;

	
	public GameObject eventPosition;

	
	public bool weaponsBlocked;

	
	public bool leftHandWeapon;

	
	private bool weaponEnabled;

	
	[Header("FMOD")]
	public string walkFootstepEvent;

	
	public string runFootstepEvent;

	
	public string stalkPlayerEvent;

	
	public string sightedScreamEvent;

	
	public string dyingToRecoverEvent;

	
	public string deathStandingEvent;

	
	public string toAttackEvent;

	
	public string alertedEvent;

	
	public string fallDownEvent = "event:/mutants/foley/body_fall";

	
	public string weaponSwooshEvent = "event:/mutants/foley/whoosh/weapon_normal_whoosh";

	
	public string fireWeaponSwooshEvent = "event:/mutants/foley/whoosh/weapon_flame_shoosh";

	
	public string landGroundEvent = "event:/mutants/foots/mutant_dirt_jump_land";

	
	public string landWaterEvent = "event:/mutants/foots/mutant_water_jump_land";

	
	public string treeJumpStartEvent = "event:/mutants/foots/mutant_tree_land_med";

	
	public string treeJumpLandEvent = "event:/mutants/foots/mutant_tree_jump_land";

	
	public bool parryBool;

	
	public int parryDir;

	
	public bool noFireAttack;

	
	private int idleHash = Animator.StringToHash("idle");

	
	private float idleTimeoutEnd;

	
	private int idleStateHash;

	
	private bool inIdleState;

	
	private int actor;

	
	public bool debugRunSplash;

	
	public bool debugWalkSplash;

	
	private float bodyColliderheight;

	
	private Collider lastJumpCollider;

	
	private bool doingLookAt;
}
