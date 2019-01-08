using System;
using System.Collections;
using HutongGames.PlayMaker;
using TheForest.Utils;
using UnityEngine;

public class mutantAnimatorControl : MonoBehaviour
{
	private void Start()
	{
		this.animator = base.GetComponent<Animator>();
		this.controller = base.transform.parent.GetComponent<CharacterController>();
		this.events = base.GetComponent<enemyAnimEvents>();
		this.thisTr = base.transform;
		this.rootTr = base.transform.parent;
		this.ai = base.gameObject.GetComponent<mutantAI>();
		this.setup = base.GetComponent<mutantScriptSetup>();
		this.hashId = base.GetComponent<mutantMaleHashId>();
		this.target = LocalPlayer.Transform;
		this.mainTriggerOffsetTr = this.setup.mainWeapon.transform.parent;
		this.setup.search.worldPositionTr.parent = null;
		this.jumpingHash = Animator.StringToHash("jumping");
		this.deathHash = Animator.StringToHash("death");
		this.jumpFallHash = Animator.StringToHash("jumpFall");
		this.inTrapHash = Animator.StringToHash("inTrap");
		this.runTrapHash = Animator.StringToHash("runTrap");
		if (!BoltNetwork.isRunning)
		{
			this.animator.SetBool(Scene.animHash.spCheckHash, true);
		}
		else
		{
			this.animator.SetBool(Scene.animHash.spCheckHash, false);
		}
		this.controllerRadius = this.controller.radius;
		this.setControllerRadius = this.controller.radius;
		if (this.setup.pmBrain)
		{
			this.fsmPlayerDist = this.setup.pmBrain.FsmVariables.GetFsmFloat("playerDist");
			this.fsmDoControllerBool = this.setup.pmBrain.FsmVariables.GetFsmBool("enableControllerBool");
			this.fsmEnableGravity = this.setup.pmBrain.FsmVariables.GetFsmBool("enableGravityBool");
			this.fsmTargetSeen = this.setup.pmBrain.FsmVariables.GetFsmBool("targetSeenBool");
		}
		if (this.setup.pmSleep)
		{
			this.fsmInCaveBool = this.setup.pmSleep.FsmVariables.GetFsmBool("inCaveBool");
			this.fsmNoMoveBool = this.setup.pmSleep.FsmVariables.GetFsmBool("noMoveBool");
		}
		if (this.setup.pmCombat)
		{
			this.fsmDeathBool = this.setup.pmCombat.FsmVariables.GetFsmBool("deathBool");
			this.fsmWallClimb = this.setup.pmCombat.FsmVariables.GetFsmBool("toClimbWall");
		}
		this.layerMask = 103948289;
		this.wallLayerMask = 103948289;
		base.Invoke("initAnimator", 0.5f);
		if (BoltNetwork.isRunning)
		{
			this.setup.pmCombat.FsmVariables.GetFsmBool("boltIsActive").Value = true;
		}
		else
		{
			this.setup.pmCombat.FsmVariables.GetFsmBool("boltIsActive").Value = false;
		}
	}

	private void OnEnable()
	{
		this.timeSinceEnabled = Time.time;
		base.Invoke("initAnimator", 0.5f);
		if (!this.animator)
		{
			this.animator = base.GetComponent<Animator>();
		}
		if (BoltNetwork.isRunning && this.initBool)
		{
			this.setup.pmCombat.FsmVariables.GetFsmBool("boltIsActive").Value = true;
		}
		else if (this.initBool)
		{
			this.setup.pmCombat.FsmVariables.GetFsmBool("boltIsActive").Value = false;
		}
		if (BoltNetwork.isRunning && !base.IsInvoking("forceDisableNetCollisions"))
		{
			base.InvokeRepeating("forceDisableNetCollisions", 2f, 1f);
		}
	}

	private void OnDisable()
	{
		base.CancelInvoke("initAnimator");
		this.initBool = false;
		this.doResetTrigger = false;
		this.dynaCoolDown = false;
		this.worldUpdateCheck = false;
		if (this.setup && this.setup.pmCombat)
		{
			this.setup.pmCombat.FsmVariables.GetFsmInt("dynamiteCounter").Value = 0;
		}
	}

	private void initAnimator()
	{
		this.initSpeed = this.animator.speed;
		this.initBool = true;
	}

	private void forceDisableNetCollisions()
	{
		if (!this.controller || !this.rootTr.gameObject.activeSelf)
		{
			return;
		}
		if (!this.controller.enabled)
		{
			return;
		}
		if (LocalPlayer.AnimControl && this.controller.enabled)
		{
			if (LocalPlayer.AnimControl.playerCollider.enabled)
			{
				Physics.IgnoreCollision(this.controller, LocalPlayer.AnimControl.playerCollider, true);
			}
			if (LocalPlayer.AnimControl.playerHeadCollider.enabled)
			{
				Physics.IgnoreCollision(this.controller, LocalPlayer.AnimControl.playerHeadCollider, true);
			}
		}
		for (int i = 0; i < Scene.SceneTracker.allPlayers.Count; i++)
		{
			if (Scene.SceneTracker.allPlayers[i] && Scene.SceneTracker.allPlayers[i].CompareTag("PlayerNet"))
			{
				CapsuleCollider component = Scene.SceneTracker.allPlayers[i].GetComponent<CapsuleCollider>();
				if (component && component.enabled && Scene.SceneTracker.allPlayers[i].activeSelf)
				{
					Physics.IgnoreCollision(this.controller, component, true);
				}
			}
		}
	}

	private void checkGroundTags()
	{
	}

	private void enableIK()
	{
	}

	private void disableIK()
	{
	}

	public void resetAllActionBools()
	{
		if (this.animator.enabled)
		{
			this.animator.SetBool(Scene.animHash.crouchBOOLHash, false);
			this.animator.SetBool(Scene.animHash.screamBOOLHash, false);
			this.animator.SetBool(Scene.animHash.runAwayBOOLHash, false);
			this.animator.SetBool(Scene.animHash.attackBOOLHash, false);
			this.animator.SetBool(Scene.animHash.damageBOOLHash, false);
			this.animator.SetBool(Scene.animHash.attackLeftBOOLHash, false);
			this.animator.SetBool(Scene.animHash.attackRightBOOLHash, false);
			this.animator.SetBool(Scene.animHash.attackJumpBOOLHash, false);
			this.animator.SetBool(Scene.animHash.attackMovingBOOLHash, false);
			this.animator.SetBool(Scene.animHash.turnAroundBOOLHash, false);
			this.animator.SetBool(Scene.animHash.sideStepBOOLHash, false);
			this.animator.SetBool(Scene.animHash.dodgeBOOLHash, false);
			this.animator.SetBool(Scene.animHash.freakoutBOOLHash, false);
			this.animator.SetBool(Scene.animHash.idleWaryBOOLHash, false);
			this.animator.SetBool(Scene.animHash.fearBOOLHash, false);
			this.animator.SetBool(Scene.animHash.backAwayBOOLHash, false);
			this.animator.SetBool(Scene.animHash.jumpBOOLHash, false);
			this.animator.SetBool(Scene.animHash.onRockBOOLHash, false);
			this.animator.SetBool(Scene.animHash.treeBOOLHash, false);
			this.animator.SetBool(Scene.animHash.sideWalkBOOLHash, false);
			this.animator.SetBool(Scene.animHash.attackMidBOOLHash, false);
			this.animator.SetBool(Scene.animHash.walkBOOLHash, false);
			this.animator.SetBool(Scene.animHash.damageBehindBOOLHash, false);
			this.animator.SetBool(Scene.animHash.ritualBOOLHash, false);
			this.animator.SetBool(Scene.animHash.feedingBOOLHash, false);
			this.animator.SetBool(Scene.animHash.encounterBOOLHash, false);
			this.animator.SetBool(Scene.animHash.rescueBool1Hash, false);
			this.animator.SetBool(Scene.animHash.jumpGapBoolHash, false);
		}
	}

	private void OnAnimatorIK(int layer)
	{
		if (!this.initBool || !this.animator.enabled)
		{
			return;
		}
		if (this.setup.search.currentTarget)
		{
			Collider component = this.setup.search.currentTarget.GetComponent<Collider>();
			if (component)
			{
				this.lookPos = component.bounds.center;
			}
			else
			{
				this.lookPos = this.setup.search.currentTarget.transform.position;
			}
			this.lookPos.y = this.lookPos.y + 1.3f;
		}
		this.animator.SetLookAtPosition(this.lookPos);
		if (this.setup.pmBrain && this.animator)
		{
			if (!this.fsmTargetSeen.Value && !this.forceIkBool)
			{
				this.animator.SetLookAtWeight(0f);
			}
			else if (this.fullBodyState.tagHash == this.hashId.deathTag)
			{
				this.animator.SetLookAtWeight(0f, 0.1f, 0.6f, 1f, 0.9f);
			}
			else if (this.fullBodyState.tagHash == this.hashId.idleTag && !this.animator.IsInTransition(0))
			{
				this.animator.SetLookAtWeight(1f, 0.2f, 0.8f, 1f, 0.9f);
			}
			else if (this.fullBodyState.tagHash == this.hashId.idleTag && this.animator.IsInTransition(0))
			{
				this.animator.SetLookAtWeight(1f, 0.2f, 0.8f, 1f, 0.9f);
			}
			else if (this.fullBodyState.tagHash == this.hashId.idleTag && this.nextFullBodyState.tagHash != this.hashId.idleTag && this.animator.IsInTransition(0))
			{
				this.transitionTime = this.animator.GetAnimatorTransitionInfo(0).normalizedTime;
				this.animator.SetLookAtWeight(1f - this.transitionTime, 0.2f, 0.8f, 1f, 0.9f);
			}
			else if (this.fullBodyState.tagHash != this.hashId.idleTag && !this.animator.IsInTransition(0))
			{
				this.animator.SetLookAtWeight(0f, 0.2f, 0.8f, 1f, 0.9f);
			}
			else if (this.nextFullBodyState.tagHash == this.hashId.idleTag && this.fullBodyState.tagHash != this.hashId.idleTag && this.animator.IsInTransition(0))
			{
				this.transitionTime = this.animator.GetAnimatorTransitionInfo(0).normalizedTime;
				this.animator.SetLookAtWeight(this.transitionTime, 0.2f, 0.8f, 1f, 0.9f);
			}
		}
	}

	private void Update()
	{
		if (this.setup.vis.animEnabled || (BoltNetwork.isRunning && BoltNetwork.isServer))
		{
			this.setup.search.worldPositionTr.position = this.rootTr.position;
		}
		if (this.fullBodyState.tagHash == this.deathHash && this.fullBodyState.normalizedTime > 0.95f && (this.animator.GetBool(Scene.animHash.deathfinalBOOLHash) || this.animator.GetBool(Scene.animHash.stealthDeathBoolHash)) && this.setup.pmBrain.ActiveStateName != "death" && this.setup.pmBrain.ActiveStateName != "deathFinish")
		{
			this.setup.pmBrain.SendEvent("toDeath");
			this.setup.pmCombat.SendEvent("toDeath");
		}
		if (this.ai.male || this.ai.female)
		{
			if (this.animator.GetBool("deathBOOL"))
			{
				this.deathBoolCheck = true;
			}
			else
			{
				this.deathBoolCheck = false;
			}
		}
		if (this.animator.enabled)
		{
			if (this.initBool)
			{
				Vector3 position = this.thisTr.position;
				if (this.worldUpdateCheck)
				{
					position = this.setup.search.worldPositionTr.position;
				}
				if (Terrain.activeTerrain)
				{
					this.terrainPosY = Terrain.activeTerrain.SampleHeight(position) + Terrain.activeTerrain.transform.position.y;
				}
				else
				{
					this.terrainPosY = this.rootTr.position.y;
				}
			}
			if (this.ai.targetAngle > -45f && this.ai.targetAngle < 45f)
			{
				if (!this.animator.GetBool(this.goForwardHash))
				{
					this.animator.SetBool(this.goForwardHash, true);
				}
				if (this.animator.GetBool(this.goLeftHash))
				{
					this.animator.SetBool(this.goLeftHash, false);
				}
				if (this.animator.GetBool(this.goRightHash))
				{
					this.animator.SetBool(this.goRightHash, false);
				}
				if (this.animator.GetBool(this.goBackHash))
				{
					this.animator.SetBool(this.goBackHash, false);
				}
			}
			else if (this.ai.targetAngle < -45f && this.ai.targetAngle > -110f)
			{
				if (this.animator.GetBool(this.goForwardHash))
				{
					this.animator.SetBool(this.goForwardHash, false);
				}
				if (!this.animator.GetBool(this.goLeftHash))
				{
					this.animator.SetBool(this.goLeftHash, true);
				}
				if (this.animator.GetBool(this.goRightHash))
				{
					this.animator.SetBool(this.goRightHash, false);
				}
				if (this.animator.GetBool(this.goBackHash))
				{
					this.animator.SetBool(this.goBackHash, false);
				}
			}
			else if (this.ai.targetAngle > 45f && this.ai.targetAngle < 110f)
			{
				if (this.animator.GetBool(this.goForwardHash))
				{
					this.animator.SetBool(this.goForwardHash, false);
				}
				if (this.animator.GetBool(this.goLeftHash))
				{
					this.animator.SetBool(this.goLeftHash, false);
				}
				if (!this.animator.GetBool(this.goRightHash))
				{
					this.animator.SetBool(this.goRightHash, true);
				}
				if (this.animator.GetBool(this.goBackHash))
				{
					this.animator.SetBool(this.goBackHash, false);
				}
			}
			else if (this.ai.targetAngle > 110f || this.ai.targetAngle < -110f)
			{
				if (this.animator.GetBool(this.goForwardHash))
				{
					this.animator.SetBool(this.goForwardHash, false);
				}
				if (this.animator.GetBool(this.goLeftHash))
				{
					this.animator.SetBool(this.goLeftHash, false);
				}
				if (this.animator.GetBool(this.goRightHash))
				{
					this.animator.SetBool(this.goRightHash, false);
				}
				if (!this.animator.GetBool(this.goBackHash))
				{
					this.animator.SetBool(this.goBackHash, true);
				}
			}
			if (this.controller.enabled && Time.time - 1f > this.timeSinceEnabled && !this.animator.GetBool(Scene.animHash.onCeilingBoolHash) && !this.animator.GetBool(Scene.animHash.sleepBOOLHash) && !this.animator.GetBool(Scene.animHash.treeBOOLHash) && !this.fsmWallClimb.Value && this.fullBodyState.tagHash != this.hashId.deathTag)
			{
				if (!this.controller.isGrounded && this.controller.velocity.y < -20f)
				{
					if (!this.startedFall)
					{
						this.longFallTimer = Time.time;
						this.animator.SetBool("jumpLandBool", false);
						this.startedFall = true;
					}
					if (this.fullBodyState.tagHash != this.jumpFallHash && Time.time - 0.25f > this.longFallTimer)
					{
						this.animator.SetBool("fallingBool", true);
					}
				}
				else if (this.controller.isGrounded && this.startedFall)
				{
					if (Time.time - 1.6f > this.longFallTimer)
					{
						this.animator.SetBool("hitGroundBool", true);
						this.animator.SetBool("fallingBool", false);
						this.startedFall = false;
						this.setup.health.Health = 0;
						this.setup.health.Die();
					}
					else
					{
						this.animator.SetBool("jumpLandBool", true);
						this.animator.SetBool("fallingBool", false);
						this.animator.SetBool("hitGroundBool", false);
						this.startedFall = false;
					}
				}
				else if (Time.time - 5f > this.longFallTimer && !this.controller.isGrounded && this.startedFall)
				{
					this.animator.SetBool("hitGroundBool", true);
					this.animator.SetBool("fallingBool", false);
					this.startedFall = false;
					this.setup.health.Health = 0;
					this.setup.health.Die();
				}
			}
		}
		if (!this.animator.enabled && this.ai.doMove)
		{
			if (this.controller.enabled)
			{
				this.controller.enabled = false;
			}
			Quaternion rotation = Quaternion.identity;
			this.wantedDir = this.ai.wantedDir;
			Vector3 vector = this.ai.wantedDir;
			if (vector != Vector3.zero && vector.sqrMagnitude > 0f)
			{
				rotation = Quaternion.LookRotation(vector, Vector3.up);
			}
			this.setup.search.worldPositionTr.rotation = rotation;
			if (this.initBool && !this.fsmInCaveBool.Value)
			{
				Vector3 position2 = this.setup.search.worldPositionTr.position;
				this.setup.search.worldPositionTr.position = position2 + this.wantedDir * this.offScreenSpeed * Time.deltaTime;
				if (Time.time > this.offScreenUpdateTimer)
				{
					this.rootTr.position = this.setup.search.worldPositionTr.position;
					this.thisTr.rotation = this.setup.search.worldPositionTr.rotation;
					this.offScreenUpdateTimer = Time.time + 1f;
				}
			}
		}
	}

	public void forceJumpLand()
	{
		this.events.playLandGround();
	}

	private void OnAnimatorMove()
	{
		if (!this.initBool || !this.animator.enabled)
		{
			return;
		}
		this.fullBodyState = this.animator.GetCurrentAnimatorStateInfo(0);
		this.nextFullBodyState = this.animator.GetNextAnimatorStateInfo(0);
		if (this.fullBodyState.tagHash == this.hashId.deathTag || this.nextFullBodyState.tagHash == this.hashId.deathTag)
		{
			this.setControllerRadius = Mathf.Lerp(this.setControllerRadius, 1.35f, Time.deltaTime * 2f);
			this.controller.radius = this.setControllerRadius;
			if (!this.playerCollideDisabled)
			{
				this.disablePlayerCollision();
			}
		}
		else if (this.fullBodyState.tagHash == this.climbTreeHash)
		{
			if (!this.playerCollideDisabled)
			{
				this.disablePlayerCollision();
			}
			this.controller.radius = (this.setControllerRadius = 0.2f);
		}
		if (this.fullBodyState.tagHash != this.hashId.deathTag && this.nextFullBodyState.tagHash != this.hashId.deathTag && this.fullBodyState.tagHash != this.climbTreeHash)
		{
			if (this.playerCollideDisabled)
			{
				this.enablePlayerCollision();
			}
			this.setControllerRadius = Mathf.Lerp(this.setControllerRadius, this.controllerRadius, Time.deltaTime * 2f);
			this.controller.radius = this.setControllerRadius;
		}
		if (this.fullBodyState.shortNameHash == this.attackBelowHash || this.fullBodyState.shortNameHash == this.attackBelowMirHash)
		{
			this.setup.mainWeapon.transform.localPosition = new Vector3(0f, 0.5f, 0.5f);
		}
		else
		{
			this.setup.mainWeapon.transform.localPosition = new Vector3(0f, 2.4f, 1.74f);
		}
		if (this.fullBodyState.tagHash == this.climbTreeHash && this.fsmWallClimb.Value)
		{
			Vector3 position = this.setup.headJoint.transform.position;
			position.y += 1f;
			if (Physics.SphereCast(position, 0.5f, this.thisTr.forward, out this.wallHit, 4f, this.wallLayerMask))
			{
				this.doClimbOutCheck = true;
				if (Physics.Raycast(this.setup.headJoint.transform.position, Vector3.up, out this.wallHit, 4f, this.wallLayerMask))
				{
					this.doClimbOutCheck = false;
					if (this.setup.pmCombat)
					{
						this.setup.pmCombat.SendEvent("cancelEvent");
					}
				}
			}
			else
			{
				Vector3 vector = this.thisTr.position;
				vector.y = this.setup.headJoint.transform.position.y + 5f;
				vector += this.thisTr.forward * 3f;
				if (Physics.Raycast(vector, Vector3.down, out this.wallHit, 25f, this.wallLayerMask))
				{
					if (this.wallHit.collider.gameObject.CompareTag("structure") || this.wallHit.collider.gameObject.CompareTag("UnderfootWood"))
					{
						if (this.setup.pmCombat)
						{
							this.setup.pmCombat.SendEvent("next");
						}
						this.setup.pmCombatScript.doAction = true;
						this.doClimbOutCheck = false;
					}
					else
					{
						if (this.setup.pmCombat)
						{
							this.setup.pmCombat.SendEvent("cancelEvent");
						}
						this.setup.pmCombatScript.cancelEvent = true;
						this.doClimbOutCheck = false;
					}
				}
				if (this.doClimbOutCheck)
				{
					if (this.setup.pmCombat)
					{
						this.setup.pmCombat.SendEvent("cancelEvent");
					}
					this.setup.pmCombatScript.cancelEvent = true;
					this.doClimbOutCheck = false;
				}
			}
		}
		if (this.fullBodyState.tagHash == this.inTrapHash || this.fullBodyState.tagHash == this.runTrapHash)
		{
			Vector3 position2 = this.mainTriggerOffsetTr.transform.position;
			position2.y = this.setup.rightFoot.transform.position.y;
			this.mainTriggerOffsetTr.position = position2;
			this.mainTriggerOffsetTr.rotation = this.setup.hipsJoint.rotation;
			this.animator.SetBoolReflected("enterTrapBool", false);
			this.fixTriggerOffset = true;
			this.setup.mutantStats.inNooseTrap = true;
		}
		else if (this.fixTriggerOffset)
		{
			this.mainTriggerOffsetTr.localPosition = Vector3.zero;
			this.mainTriggerOffsetTr.localRotation = Quaternion.identity;
			this.setup.mutantStats.inNooseTrap = false;
			this.fixTriggerOffset = false;
		}
		if (this.fullBodyState.shortNameHash == this.feedingHash)
		{
			if (!this.setup.heldMeat.activeSelf)
			{
				this.setup.heldMeat.SetActive(true);
			}
		}
		else if (this.setup.heldMeat.activeSelf)
		{
			this.setup.heldMeat.SetActive(false);
		}
		if (this.fullBodyState.tagHash == this.setup.hashs.idleTag)
		{
			this.animator.SetBool(Scene.animHash.dropFromTrapHash, false);
		}
		if (this.setup.waterDetect.underWater)
		{
			this.animator.speed = 0.55f;
		}
		else if (this.fullBodyState.tagHash == this.jumpFallHash && this.animator.enabled)
		{
			this.animator.speed = 1f;
			this.pos = this.thisTr.position;
			this.pos.y = this.pos.y + 1.4f;
			if (Physics.Raycast(this.pos, Vector3.down, out this.hit, 100f, this.layerMask))
			{
				this.hitDistance = this.hit.distance;
				if (this.hit.distance < 3f)
				{
					this.animator.SetTriggerReflected("jumpLandTrigger");
					if (this.animator.GetBool("deathfinalBOOL"))
					{
						this.animator.SetBool("hitGroundBool", true);
					}
					this.events.playLandGround();
					base.Invoke("resetJumpTrigger", 1.3f);
				}
			}
		}
		else if (this.fullBodyState.tagHash == this.jumpingHash)
		{
			this.animator.speed = 1f;
		}
		else if (this.patrolling)
		{
			this.animator.speed = 0.8f;
		}
		else if (this.setup.health.poisoned)
		{
			if (this.ai.maleSkinny || this.ai.femaleSkinny)
			{
				this.animator.speed = this.ai.animSpeed / 1.3f;
			}
			else if (this.ai.pale)
			{
				this.animator.speed = this.ai.animSpeed / 1.2f;
			}
			else
			{
				this.animator.speed = this.ai.animSpeed / 1.2f;
			}
		}
		else
		{
			this.animator.speed = this.ai.animSpeed;
		}
		if (this.initBool)
		{
			if (this.fsmDoControllerBool.Value)
			{
				if (this.fsmInCaveBool.Value)
				{
					this.controllerOn();
				}
				else if (this.animator && this.fsmPlayerDist.Value > 220f && !this.fsmInCaveBool.Value)
				{
					this.controllerOff();
				}
				else if (this.animator && this.fsmPlayerDist.Value < 220f)
				{
					this.controllerOn();
				}
				else
				{
					this.controllerOff();
				}
			}
			else
			{
				this.controllerOff();
			}
		}
	}

	private void controllerOn()
	{
		if (!this.controller.enabled)
		{
			this.controller.enabled = true;
		}
		if (this.fsmEnableGravity.Value)
		{
			this.animGravity = this.animator.GetFloat("Gravity");
		}
		else
		{
			this.animGravity = 0f;
		}
		this.moveDir = this.animator.deltaPosition;
		this.moveDir.y = this.moveDir.y - this.gravity * Time.deltaTime * this.animGravity * 1.8f;
		if (this.stopMovement)
		{
			this.moveDir.x = 0f;
			this.moveDir.z = 0f;
		}
		this.currLayerState1 = this.animator.GetCurrentAnimatorStateInfo(0);
		if (!this.fsmNoMoveBool.Value)
		{
			this.controller.Move(this.moveDir);
			if (!this.animator.GetBool(Scene.animHash.idleWaryBOOLHash))
			{
				this.thisTr.rotation = this.animator.rootRotation;
			}
		}
		Vector3 vector = Vector3.zero;
		if (this.fullBodyState.tagHash == this.hashId.deathTag && this.fullBodyState.normalizedTime < 1f && this.fullBodyState.shortNameHash != Scene.animHash.nooseTrapDeathHash && this.fullBodyState.shortNameHash != Scene.animHash.spikeWallDeathHash)
		{
			Vector3 origin = new Vector3(this.thisTr.position.x, this.thisTr.position.y + 2f, this.thisTr.position.z);
			RaycastHit raycastHit;
			if (Physics.Raycast(origin, Vector3.down, out raycastHit, 10f, this.layerMask))
			{
				vector = raycastHit.normal;
			}
			else
			{
				vector = Vector3.up;
			}
		}
		else
		{
			vector = Vector3.up;
		}
		this.thisTr.rotation = Quaternion.Lerp(this.thisTr.rotation, Quaternion.LookRotation(Vector3.Cross(this.thisTr.right, vector), vector), Time.deltaTime * 8f);
	}

	private void controllerOff()
	{
		if (this.initBool)
		{
			if (this.controller.enabled)
			{
				this.controller.enabled = false;
			}
			Vector3 position = this.thisTr.position;
			if ((this.setup.vis.animReduced || this.setup.vis.animDisabled) && !BoltNetwork.isRunning)
			{
				position = this.setup.search.worldPositionTr.position;
			}
			if (Terrain.activeTerrain)
			{
				this.terrainPosY = Terrain.activeTerrain.SampleHeight(position) + Terrain.activeTerrain.transform.position.y;
			}
			else
			{
				this.terrainPosY = this.rootTr.position.y;
			}
			if (this.fsmEnableGravity.Value)
			{
				this.animGravity = this.animator.GetFloat(Scene.animHash.GravityHash);
			}
			else
			{
				this.animGravity = 0f;
			}
			if ((this.setup.vis.animReduced || this.setup.vis.animDisabled) && !this.deathBoolCheck && !BoltNetwork.isRunning)
			{
				this.moveDir = this.ai.wantedDir;
			}
			else
			{
				this.moveDir = this.animator.deltaPosition;
			}
			if (!this.fsmNoMoveBool.Value)
			{
				this.setup.search.worldPositionTr.Translate(this.moveDir * 0.5f, Space.World);
				if (this.animGravity > 0.5f)
				{
					this.setup.search.worldPositionTr.position = new Vector3(this.setup.search.worldPositionTr.position.x, this.terrainPosY, this.setup.search.worldPositionTr.position.z);
				}
				if (!this.animator.GetBool(Scene.animHash.idleWaryBOOLHash))
				{
					if ((this.setup.vis.animReduced || this.setup.vis.animDisabled) && !this.deathBoolCheck && !BoltNetwork.isRunning)
					{
						if (this.ai.wantedDir != Vector3.zero)
						{
							this.setup.search.worldPositionTr.rotation = Quaternion.LookRotation(this.ai.wantedDir, Vector3.up);
						}
					}
					else
					{
						this.setup.search.worldPositionTr.rotation = this.animator.rootRotation;
					}
				}
				if ((this.setup.vis.animReduced || this.setup.vis.animDisabled) && !this.deathBoolCheck && !BoltNetwork.isRunning)
				{
					if (Time.time > this.offScreenUpdateTimer)
					{
						this.rootTr.position = this.setup.search.worldPositionTr.position;
						this.thisTr.rotation = this.setup.search.worldPositionTr.rotation;
						this.offScreenUpdateTimer = Time.time + 1f;
					}
				}
				else
				{
					this.rootTr.position = this.setup.search.worldPositionTr.position;
					this.thisTr.rotation = this.setup.search.worldPositionTr.rotation;
				}
			}
		}
	}

	private void LateUpdate()
	{
	}

	private void setDeathTrigger()
	{
		if (this.animator.enabled)
		{
			this.animator.SetTriggerReflected("deathTrigger");
		}
	}

	private void enableController()
	{
		this.fsmDoControllerBool.Value = true;
	}

	private void disableDeathBool()
	{
	}

	private void enableDeathBool()
	{
	}

	private void resetJumpTrigger()
	{
		if (this.animator.enabled)
		{
			this.animator.ResetTrigger("jumpLandTrigger");
			this.animator.SetBool("hitGroundBool", false);
		}
		this.doResetTrigger = false;
	}

	public void enablePatrolAnimSpeed()
	{
		this.patrolling = true;
	}

	public void disablePatrolAnimSpeed()
	{
		this.patrolling = false;
	}

	public IEnumerator smoothChangeIdle(float i)
	{
		if (!this.animator.enabled)
		{
			yield break;
		}
		if (this.setup.health.poisoned)
		{
			yield break;
		}
		if (this.changeBlock)
		{
			yield break;
		}
		this.changeBlock = true;
		float t = 0f;
		float startVal = this.animator.GetFloat(Scene.animHash.aggressionHash);
		float val = startVal;
		while (t < 1f)
		{
			if (this.setup.health.poisoned)
			{
				this.changeBlock = false;
				yield break;
			}
			t += Time.deltaTime * 1f;
			val = Mathf.Lerp(startVal, i, t);
			if (this.animator.enabled)
			{
				this.animator.SetFloatReflected(Scene.animHash.aggressionHash, val);
			}
			yield return null;
		}
		this.changeBlock = false;
		yield break;
	}

	private IEnumerator fixFootPosition()
	{
		while (this.animator.GetBool("trapBool"))
		{
			this.footPivot = this.setup.pmEncounter.FsmVariables.GetFsmGameObject("footPivotGo").Value;
			if (!this.footPivot)
			{
				yield break;
			}
			Vector3 position = this.setup.pmEncounter.FsmVariables.GetFsmGameObject("footPivotGo").Value.transform.position;
			position.y = this.rootTr.position.y;
			this.rootTr.position = Vector3.Lerp(this.rootTr.position, position, Time.deltaTime * 5f);
			yield return null;
		}
		yield break;
	}

	private void disableFixFootPosition()
	{
		base.StopCoroutine("fixFootPosition");
	}

	public void resetInTrap()
	{
		this.setup.hitReceiver.inNooseTrap = false;
	}

	private void disableBodyCollider()
	{
		this.setup.bodyCollider.SetActive(false);
		base.Invoke("enableBodyCollider", 5f);
	}

	private void enableBodyCollider()
	{
		this.setup.bodyCollider.SetActive(true);
	}

	private void resetTrapDrop()
	{
		this.animator.SetBoolReflected("dropFromTrap", false);
	}

	private void startDynamiteCoolDown()
	{
		if (!this.dynaCoolDown)
		{
			this.dynaCoolDown = true;
			base.Invoke("resetDynamiteCounter", 75f);
		}
	}

	private void resetDynamiteCounter()
	{
		this.setup.pmCombat.FsmVariables.GetFsmInt("dynamteCounter").Value = 0;
		this.dynaCoolDown = false;
	}

	private void disablePlayerCollision()
	{
		if (LocalPlayer.AnimControl && this.controller.enabled)
		{
			if (LocalPlayer.AnimControl.playerCollider.enabled)
			{
				Physics.IgnoreCollision(this.controller, LocalPlayer.AnimControl.playerCollider, true);
			}
			if (LocalPlayer.AnimControl.playerHeadCollider.enabled)
			{
				Physics.IgnoreCollision(this.controller, LocalPlayer.AnimControl.playerHeadCollider, true);
			}
		}
		if (BoltNetwork.isRunning)
		{
			for (int i = 0; i < Scene.SceneTracker.allPlayers.Count; i++)
			{
				if (Scene.SceneTracker.allPlayers[i] && Scene.SceneTracker.allPlayers[i].CompareTag("PlayerNet"))
				{
					CoopPlayerRemoteSetup component = Scene.SceneTracker.allPlayers[i].GetComponent<CoopPlayerRemoteSetup>();
					this.controller.enabled = true;
					if (this.controller.enabled && component)
					{
						CapsuleCollider component2 = component.transform.GetComponent<CapsuleCollider>();
						if (component2 && component2.enabled && component2.gameObject.activeSelf && this.controller.gameObject.activeSelf)
						{
							Physics.IgnoreCollision(this.controller, component.transform.GetComponent<CapsuleCollider>(), true);
						}
					}
				}
			}
		}
		this.playerCollideDisabled = true;
	}

	public void enablePlayerCollision()
	{
		if (this.playerCollideDisabled)
		{
			if (LocalPlayer.AnimControl && this.controller.enabled)
			{
				if (LocalPlayer.AnimControl.playerCollider.enabled)
				{
					Physics.IgnoreCollision(this.controller, LocalPlayer.AnimControl.playerCollider, false);
				}
				if (LocalPlayer.AnimControl.playerHeadCollider.enabled)
				{
					Physics.IgnoreCollision(this.controller, LocalPlayer.AnimControl.playerHeadCollider, false);
				}
			}
			if (BoltNetwork.isRunning)
			{
				for (int i = 0; i < Scene.SceneTracker.allPlayers.Count; i++)
				{
					if (Scene.SceneTracker.allPlayers[i] && Scene.SceneTracker.allPlayers[i].CompareTag("PlayerNet"))
					{
						CoopPlayerRemoteSetup component = Scene.SceneTracker.allPlayers[i].GetComponent<CoopPlayerRemoteSetup>();
						this.controller.enabled = true;
						if (this.controller.enabled)
						{
						}
					}
				}
			}
			this.playerCollideDisabled = false;
		}
	}

	public void runGotHitScripts()
	{
		this.setup.familyFunctions.sendCancelEvent();
		this.events.disableAllWeapons();
		this.events.bombDisable();
		this.events.enableCollision();
		this.ai.forceTreeDown();
		this.setup.familyFunctions.resetFamilyParams();
		this.setup.familyFunctions.sendAggressive();
		GameObject value = this.setup.pmCombat.FsmVariables.GetFsmGameObject("currentMemberGo").Value;
		this.setup.health.getCurrentHealth();
		this.setup.pmCombat.FsmVariables.GetFsmBool("fearBOOL").Value = false;
		this.setup.aiManager.flee = false;
		this.setup.pmBrain.FsmVariables.GetFsmBool("playerIsRed").Value = false;
	}

	private Animator animator;

	private CharacterController controller;

	private Transform thisTr;

	private Transform rootTr;

	private Transform mainTriggerOffsetTr;

	private mutantAI ai;

	private mutantScriptSetup setup;

	private enemyAnimEvents events;

	private mutantMaleHashId hashId;

	public AnimatorStateInfo fullBodyState;

	public AnimatorStateInfo currLayerState1;

	public AnimatorStateInfo nextLayerState1;

	public AnimatorStateInfo nextFullBodyState;

	public Transform target;

	public float hitDistance;

	public float offScreenSpeed;

	public bool inWater;

	public bool inBlockerTrigger;

	public bool inTreeTrigger;

	public Collider blockerCollider;

	public bool worldUpdateCheck;

	public float gravity;

	private float animGravity;

	private Vector3 moveDir = Vector3.zero;

	private float currYPos;

	public float velY;

	public float accelY;

	public Vector3 wantedDir;

	public bool initBool;

	private Vector3 lookPos;

	private bool patrolling;

	private bool doResetTrigger;

	public bool forceIkBool;

	public bool playerCollideDisabled;

	private float timeSinceEnabled;

	private bool fixTriggerOffset;

	public FsmFloat fsmPlayerDist;

	public FsmBool fsmDoControllerBool;

	public FsmBool fsmDeathBool;

	public FsmBool fsmInCaveBool;

	private FsmBool fsmEnableGravity;

	public FsmBool fsmTargetSeen;

	public FsmBool fsmNoMoveBool;

	private FsmBool fsmWallClimb;

	private float initSpeed;

	public float terrainPosY;

	private float transitionTime;

	private float controllerRadius;

	private bool startedFall;

	private float longFallTimer;

	private bool deathBoolCheck;

	public int jumpingHash;

	public int deathHash;

	public int jumpFallHash;

	public int inTrapHash;

	public int runTrapHash;

	private int goForwardHash = Animator.StringToHash("goForward");

	private int goLeftHash = Animator.StringToHash("goLeft");

	private int goRightHash = Animator.StringToHash("goRight");

	private int goBackHash = Animator.StringToHash("goBack");

	public int feedingHash = Animator.StringToHash("feeding2");

	public int climbHash = Animator.StringToHash("treeClimb");

	public int climbTreeHash = Animator.StringToHash("climbingTree");

	public int attackBelowHash = Animator.StringToHash("attackStructureBelow");

	public int attackBelowMirHash = Animator.StringToHash("attackStructureBelowMirror");

	public int landingHash = Animator.StringToHash("landing");

	public int inTreehash = Animator.StringToHash("inTree");

	public int inTreeMirhash = Animator.StringToHash("inTreeMir");

	public int idlehash = Animator.StringToHash("idle");

	public int runhash = Animator.StringToHash("running");

	public int onRockHash = Animator.StringToHash("onRock");

	public int burningLoopHash = Animator.StringToHash("burningLoop");

	public int burningLoopMirHash = Animator.StringToHash("burningLoopMir");

	public int idleToBurningHash = Animator.StringToHash("idleToBurning");

	public int burnToDeathHash = Animator.StringToHash("burnToDeath");

	public int burnToDeathHashMir = Animator.StringToHash("burnToDeathMir");

	private RaycastHit hit;

	private Vector3 pos;

	private int layerMask;

	private int wallLayerMask;

	public bool stopMovement;

	private float setControllerRadius;

	private RaycastHit wallHit;

	private bool doClimbOutCheck;

	public float distanceTune = 2.5f;

	private float offScreenUpdateTimer;

	private Vector3 smoothPos;

	private bool changeBlock;

	private GameObject footPivot;

	private bool dynaCoolDown;
}
