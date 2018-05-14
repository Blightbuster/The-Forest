using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using HutongGames.PlayMaker;
using TheForest.Items;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;


public class playerAnimatorControl : MonoBehaviour
{
	
	
	public bool HorizontalMovement
	{
		get
		{
			return Mathf.Abs(this.horizontalSpeed) > 0.01f;
		}
	}

	
	
	public bool VerticalMovement
	{
		get
		{
			return Mathf.Abs(this.verticalSpeed) > 0.01f;
		}
	}

	
	private void Awake()
	{
		this.wallSetup = base.transform.GetComponentInChildren<wallTriggerSetup>();
		this.setup = base.GetComponent<playerScriptSetup>();
		this.buoyancy = base.transform.GetComponentInParent<Buoyancy>();
		this.animEvents = base.transform.GetComponent<animEventsManager>();
		this.planeCrashGo = GameObject.FindWithTag("planeCrash");
		this.player = base.transform.GetComponentInParent<PlayerInventory>();
		this.sledHinge = base.transform.GetComponentInChildren<HingeJoint>();
		this.sledPivot = base.GetComponentInParent<Rigidbody>().transform.Find("sledPivot").transform;
		if (this.planeCrashGo)
		{
			this.setup.sceneInfo.planeCrash = this.planeCrashGo;
			this.planeCrash = this.planeCrashGo.transform;
		}
		else
		{
			GameObject gameObject = new GameObject();
			this.planeCrash = gameObject.transform;
			this.planeCrash.position = base.transform.position;
			if (this.setup.sceneInfo != null)
			{
				this.setup.sceneInfo.planeCrash = this.planeCrash.gameObject;
			}
		}
	}

	
	private void Start()
	{
		this.animator = base.gameObject.GetComponent<Animator>();
		this.reactions = base.transform.parent.GetComponent<playerHitReactions>();
		this.controller = base.transform.GetComponentInParent<Rigidbody>();
		this.playerCollider = this.controller.GetComponent<CapsuleCollider>();
		this.playerHeadCollider = this.controller.GetComponent<SphereCollider>();
		this.enemyCollider = this.setup.enemyBlockerGo.GetComponent<CapsuleCollider>();
		this.forcePos = base.transform.GetComponent<ForceLocalPosZero>();
		this.rb = base.transform.GetComponentInParent<Rigidbody>();
		this.tr = base.transform;
		this.rootTr = base.transform.root;
		this.smoothCamX = 0f;
		this.fsmPlayerAngle = this.setup.pmRotate.FsmVariables.GetFsmFloat("playerAngle");
		this.fsmTiredFloat = this.setup.pmStamina.FsmVariables.GetFsmFloat("notTiredSpeed");
		this.fsmButtonHeldBool = this.setup.pmControl.FsmVariables.GetFsmBool("checkHeldBool");
		this.fsmCanDoAxeSmash = this.setup.pmControl.FsmVariables.GetFsmBool("canDoAxeSmash");
		this.stickAttackHash = Animator.StringToHash("stickAttack");
		this.axeAttackHash = Animator.StringToHash("AxeAttack");
		this.axeHeavyAttackHash = Animator.StringToHash("doAttackHeavyBool");
		this.idleHash = Animator.StringToHash("idling");
		this.hangingHash = Animator.StringToHash("hanging");
		this.checkArmsHash = Animator.StringToHash("checkArms");
		this.heldHash = Animator.StringToHash("held");
		this.attackHash = Animator.StringToHash("attacking");
		this.smashHash = Animator.StringToHash("smash");
		this.blockHash = Animator.StringToHash("block");
		this.deathHash = Animator.StringToHash("death");
		this.swimHash = Animator.StringToHash("swimming");
		this.climbingHash = Animator.StringToHash("climbing");
		this.climbIdleHash = Animator.StringToHash("climbIdle");
		this.enterClimbHash = Animator.StringToHash("enterClimb");
		this.explodeHash = Animator.StringToHash("explode");
		this.knockBackHash = Animator.StringToHash("knockBack");
		this.setup.pmControl.FsmVariables.GetFsmInt("knockBackHash").Value = Animator.StringToHash("knockBack");
		this.setup.pmControl.FsmVariables.GetFsmInt("climbHash").Value = Animator.StringToHash("climbing");
		this.setup.pmControl.FsmVariables.GetFsmInt("climbIdleHash").Value = this.climbIdleHash;
		this.setup.pmControl.FsmVariables.GetFsmInt("sledHash").Value = Animator.StringToHash("onSled");
		this.setup.pmControl.FsmVariables.GetFsmInt("stick1Hash").Value = Animator.StringToHash("stickCombo1");
		this.setup.pmControl.FsmVariables.GetFsmInt("stick2Hash").Value = Animator.StringToHash("stickCombo2");
		this.setup.pmControl.FsmVariables.GetFsmInt("repairAttackHash").Value = Animator.StringToHash("upperBody.repairHammerAttack");
		this.setup.pmControl.FsmVariables.GetFsmInt("attackHash").Value = Animator.StringToHash("attacking");
		this.setup.pmControl.FsmVariables.GetFsmInt("shellRideHash").Value = Animator.StringToHash("shellRide");
		this.setup.pmControl.FsmVariables.GetFsmGameObject("inventoryGo").Value = this.player._inventoryGO;
		this.layerMask = 69345280;
		base.InvokeRepeating("checkPlaneDistance", 5f, 5f);
	}

	
	private void checkPlaneDistance()
	{
		if (Scene.SceneTracker == null)
		{
			return;
		}
		if (!this.planeCrash)
		{
			this.planeCrash = GameObject.FindWithTag("planeCrash").transform;
		}
		this.planeDist = Vector3.Distance(this.tr.position, this.planeCrash.position);
	}

	
	private bool IsLeftHandBusy()
	{
		return !this.player.IsLeftHandEmpty();
	}

	
	private void Update()
	{
		if (!ForestVR.Enabled)
		{
			if ((this.fullBodyState3.tagHash == this.idleHash || (this.nextFullBodyState3.tagHash == this.idleHash && this.animator.IsInTransition(3))) && !this.skinningAnimal && !this.endGameCutScene)
			{
				if (this.setup.leftHandHeld.gameObject.activeSelf)
				{
					this.setup.leftHandHeld.gameObject.SetActive(false);
				}
			}
			else if (!this.setup.leftHandHeld.gameObject.activeSelf)
			{
				this.setup.leftHandHeld.gameObject.SetActive(true);
			}
		}
		if (ForestVR.Enabled)
		{
			this.animator.SetFloat("VRBlend", 1f);
		}
		else
		{
			this.animator.SetFloat("VRBlend", 0f);
		}
		if (this.currLayerState1.shortNameHash == this.eatMeatHash)
		{
			if (this.setup.weaponRight.gameObject.activeSelf)
			{
				this.setup.weaponRight.gameObject.SetActive(false);
			}
		}
		else if (!this.setup.weaponRight.gameObject.activeSelf)
		{
			this.setup.weaponRight.gameObject.SetActive(true);
		}
		if (!LocalPlayer.FpCharacter.crouchHeightBlocked && this.animator.GetFloat("normCamX") > this.setup.treeHit.fsmAxeSmashAngle.Value)
		{
			this.fsmCanDoAxeSmash.Value = true;
		}
		else
		{
			this.fsmCanDoAxeSmash.Value = false;
		}
		if (LocalPlayer.Stats.Stamina < 10f)
		{
			this.animator.SetFloatReflected("climbSpeed", 10f);
		}
		else
		{
			this.animator.SetFloatReflected("climbSpeed", this.animator.GetFloat("vSpeed"));
		}
		if (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._bowId))
		{
			if (this.animator.GetFloat("bowSpeed") != 1.2f)
			{
				this.animator.SetFloatReflected("bowSpeed", 1.2f);
			}
		}
		else if (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._bowRecurveId) && this.animator.GetFloat("bowSpeed") != 0.65f)
		{
			this.animator.SetFloatReflected("bowSpeed", 0.65f);
		}
		if (this.setup.heldLog1.activeSelf || this.setup.heldLog2.activeSelf)
		{
			this.animator.SetBool("logHeld", true);
		}
		else
		{
			this.animator.SetBool("logHeld", false);
		}
		if (this.wallSetup.atWallCheck)
		{
			Scene.HudGui.DropButton.SetActive(false);
		}
		else if (this.carry && !this.wallSetup.atWallCheck)
		{
			Scene.HudGui.DropButton.SetActive(true);
		}
		if (this.blockCamX)
		{
			this.startPlaneBlend = 0f;
		}
		else
		{
			this.startPlaneBlend = Mathf.Lerp(this.startPlaneBlend, 1f, Time.deltaTime);
		}
		if (this.doShellRideMode)
		{
			this.HandleShellRideControlUpdate();
		}
		if (this.doSledPushMode)
		{
			this.hVel = Mathf.SmoothDamp(this.hVel, TheForest.Utils.Input.GetAxis("Horizontal") * 50f, ref this.curVel, 0.1f);
			if (Mathf.Abs(this.hVel) > 0.0001f)
			{
				this.animEvents.IsSledTurning = true;
				this.rootTr.RotateAround(this.sledPivot.position, Vector3.up, this.hVel * Time.deltaTime);
			}
			else
			{
				this.curVel = 0f;
				this.animEvents.IsSledTurning = false;
			}
		}
		float @float = this.animator.GetFloat("normCamX");
		float float2 = this.animator.GetFloat("pedIdleBlend");
		float float3 = this.animator.GetFloat("normCamY");
		float float4 = this.animator.GetFloat("vSpeed");
		float float5 = this.animator.GetFloat("hSpeed");
		float float6 = this.animator.GetFloat("overallSpeed");
		this.currLayerState0 = this.animator.GetCurrentAnimatorStateInfo(0);
		this.currLayerState1 = this.animator.GetCurrentAnimatorStateInfo(1);
		this.nextLayerState1 = this.animator.GetNextAnimatorStateInfo(1);
		this.nextLayerState0 = this.animator.GetNextAnimatorStateInfo(0);
		this.fullBodyState2 = this.animator.GetCurrentAnimatorStateInfo(2);
		this.fullBodyState3 = this.animator.GetCurrentAnimatorStateInfo(3);
		this.nextFullBodyState2 = this.animator.GetNextAnimatorStateInfo(2);
		this.nextFullBodyState3 = this.animator.GetNextAnimatorStateInfo(3);
		this.animator.SetBoolReflected("movingBool", false);
		float b = Mathf.Clamp(this.normCamX * 12f, 0f, 10f);
		this.lookDownBlendVal = Mathf.Lerp(this.lookDownBlendVal, b, Time.deltaTime * 5f);
		this.animator.SetFloat("lookDownBlend", this.lookDownBlendVal);
		if (this.IsLeftHandBusy())
		{
			if (!this.tiredCheck)
			{
				this.tempTired = this.fsmTiredFloat.Value;
				this.fsmTiredFloat.Value = this.fsmTiredFloat.Value / 1f;
				this.tiredCheck = true;
			}
		}
		else if (this.tiredCheck)
		{
			this.fsmTiredFloat.Value = this.tempTired;
			this.tiredCheck = false;
		}
		if (this.nextLayerState1.tagHash == this.idleHash && !this.coldOffsetBool)
		{
			this.coldOffsetBool = true;
			this.coldOffsetTimer = Time.time + 1f;
		}
		if (Time.time > this.coldOffsetTimer)
		{
			this.coldOffsetBool = false;
		}
		if (!LocalPlayer.Inventory.IsRightHandEmpty() || this.onRockThrower || LocalPlayer.Animator.GetBool("lookAtPhoto") || LocalPlayer.Animator.GetBool("zipLineAttach") || LocalPlayer.Animator.GetBool("logHeld") || this.coldOffsetBool)
		{
			this.animator.SetBoolReflected("blockColdBool", true);
		}
		else
		{
			this.animator.SetBoolReflected("blockColdBool", false);
		}
		if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Book && this.currLayerState1.shortNameHash != this.bookIdleToIdleHash)
		{
			LocalPlayer.Rigidbody.velocity = new Vector3(0f, LocalPlayer.Rigidbody.velocity.y, 0f);
			LocalPlayer.FpCharacter.playerPhysicMaterial.dynamicFriction = 10f;
			LocalPlayer.FpCharacter.playerPhysicMaterial.staticFriction = 10f;
			LocalPlayer.FpCharacter.playerPhysicMaterial.frictionCombine = PhysicMaterialCombine.Average;
		}
		if (this.fullBodyState2.shortNameHash != this.injuredLoopHash)
		{
			this.getPlayerPos = LocalPlayer.Transform.position;
		}
		else
		{
			LocalPlayer.Transform.position = this.getPlayerPos;
		}
		if (this.fullBodyState2.shortNameHash == this.timmyOnMachineHash && this.fullBodyState2.normalizedTime > 0.95f)
		{
			this.animator.CrossFade("fullBodyActions.idle", 0f, 2, 0f);
		}
		if (this.animator.GetBool("lightWeaponBool") || this.fullBodyState2.tagHash == this.explodeHash || this.currLayerState0.shortNameHash == this.landHeavyHash)
		{
			LocalPlayer.Inventory.blockRangedAttack = true;
		}
		else if (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._flintLockId) && this.currLayerState1.shortNameHash != this.flintLockIdleHash && this.currLayerState1.shortNameHash != this.flintLockToAimHash && this.currLayerState1.shortNameHash != this.flintLockAimIdleHash)
		{
			LocalPlayer.Inventory.blockRangedAttack = true;
		}
		else if (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._flaregunId) && this.currLayerState1.shortNameHash != this.flareGunIdleHash && this.currLayerState1.shortNameHash != this.flareGunToAimHash && this.currLayerState1.shortNameHash != this.flareGunAimIdleHash)
		{
			LocalPlayer.Inventory.blockRangedAttack = true;
		}
		else if (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._flareId) && this.currLayerState1.shortNameHash == this.flareLightHash)
		{
			LocalPlayer.Inventory.blockRangedAttack = true;
		}
		else if ((LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._bowId) || LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._bowRecurveId)) && this.currLayerState1.shortNameHash != this.drawBowIdleHash)
		{
			if (this.currLayerState1.shortNameHash == this.drawBowHash && this.currLayerState1.normalizedTime > 0.8f)
			{
				LocalPlayer.Inventory.blockRangedAttack = false;
			}
			else
			{
				LocalPlayer.Inventory.blockRangedAttack = !ForestVR.Enabled;
			}
		}
		else if (this.currLayerState1.tagHash == this.knockBackHash || this.fullBodyState2.tagHash == this.knockBackHash || this.fullBodyState2.tagHash == this.deathHash)
		{
			LocalPlayer.Inventory.blockRangedAttack = true;
		}
		else
		{
			LocalPlayer.Inventory.blockRangedAttack = false;
		}
		if (this.nextLayerState1.tagHash == this.heldHash && this.currLayerState1.tagHash == this.attackHash && this.animator.GetBool("bowHeld") && this.animator.IsInTransition(1))
		{
			this.animEvents.StopCoroutine("smoothEnableSpine");
			this.animEvents.StartCoroutine("smoothEnableSpine");
		}
		if (this.currLayerState1.shortNameHash == this.bowIdleHash && !this.animator.IsInTransition(1) && this.currLayerState0.tagHash != this.explodeHash)
		{
			this.animator.SetLayerWeightReflected(4, 1f);
		}
		if (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._repairToolId) && this.currLayerState1.shortNameHash == this.repairHammerAttackHash && this.fullBodyState3.shortNameHash == this.repairHammerAttackHash && this.nextLayerState1.tagHash != this.heldHash)
		{
			this.animator.Play(this.repairHammerAttackHash, 3, this.currLayerState1.normalizedTime);
		}
		if (!LocalPlayer.FpCharacter.jumpCoolDown)
		{
			if ((this.currLayerState1.tagHash == this.idleHash && !this.animator.IsInTransition(1)) || this.useRootMotion)
			{
				if (!this.upsideDown && !this.introCutScene)
				{
					this.animator.SetLayerWeightReflected(1, 0f);
				}
			}
			else if (this.currLayerState0.tagHash != this.shellRideHash)
			{
				if (!this.animator.GetBool("zipLineAttach") && !this.animator.GetBool("craneAttach"))
				{
					if (this.nextLayerState1.tagHash == this.heldHash && this.currLayerState1.tagHash != this.attackHash && this.currLayerState1.tagHash != this.heldHash && this.currLayerState1.tagHash != this.smashHash && this.currLayerState1.tagHash != this.blockHash && this.currLayerState1.tagHash != this.knockBackHash)
					{
						float normalizedTime = this.animator.GetAnimatorTransitionInfo(1).normalizedTime;
						this.animator.SetLayerWeightReflected(1, normalizedTime);
					}
					else if (this.nextLayerState1.tagHash == this.idleHash)
					{
						float normalizedTime2 = this.animator.GetAnimatorTransitionInfo(1).normalizedTime;
						this.animator.SetLayerWeightReflected(1, 1f - normalizedTime2);
					}
					else if (!this.animator.IsInTransition(1) && this.currLayerState1.tagHash == this.heldHash)
					{
						this.animator.SetLayerWeightReflected(1, 1f);
					}
				}
			}
			if (this.fullBodyState2.tagHash == this.deathHash)
			{
				this.animator.SetLayerWeightReflected(2, 1f);
				this.animator.SetLayerWeightReflected(1, 0f);
				this.animator.SetLayerWeightReflected(3, 0f);
				this.animator.SetLayerWeightReflected(4, 0f);
				LocalPlayer.Rigidbody.velocity = new Vector3(0f, LocalPlayer.Rigidbody.velocity.y, 0f);
				this.pos = new Vector3(this.tr.position.x, this.tr.position.y + 5f, this.tr.position.z);
				RaycastHit raycastHit;
				if (Physics.Raycast(this.pos, Vector3.down, out raycastHit, 30f, this.layerMask))
				{
					if (!this.deathPositionCheck)
					{
						this.deathPos = raycastHit.point;
						this.deathPositionCheck = true;
					}
					this.tr.rotation = Quaternion.Lerp(this.tr.rotation, Quaternion.LookRotation(Vector3.Cross(this.tr.right, raycastHit.normal), raycastHit.normal), Time.deltaTime * 6f);
				}
			}
			else if (this.currLayerState0.tagHash != this.shellRideHash && this.fullBodyState2.shortNameHash != this.fillPotHash)
			{
				if (this.fullBodyState2.tagHash != this.swimHash && !this.animator.GetBool("zipLineAttach") && !this.animator.GetBool("craneAttach"))
				{
					if (this.fullBodyState2.tagHash == this.explodeHash)
					{
						this.animator.SetLayerWeightReflected(2, 1f);
						LocalPlayer.Rigidbody.velocity = new Vector3(0f, LocalPlayer.Rigidbody.velocity.y, 0f);
					}
					else if (this.nextFullBodyState2.tagHash == this.idleHash && this.animator.IsInTransition(2) && !this.animator.GetBool(this.axeAttackHash) && this.fullBodyState2.tagHash != this.heldHash && !this.animator.GetBool(this.stickAttackHash) && this.fullBodyState2.tagHash != this.idleHash && this.animator.GetFloat("crouch") < 0.5f && !LocalPlayer.Animator.GetBool("potHeld"))
					{
						float normalizedTime3 = this.animator.GetAnimatorTransitionInfo(2).normalizedTime;
						this.animator.SetLayerWeightReflected(2, 0.95f - normalizedTime3);
					}
				}
			}
			if (this.fullBodyState2.tagHash != this.deathHash)
			{
				this.deathPositionCheck = false;
			}
			if (this.currLayerState1.shortNameHash != this.toChainSawIdleHash)
			{
				if (this.nextFullBodyState3.tagHash == this.checkArmsHash && this.fullBodyState3.tagHash == this.idleHash && this.animator.IsInTransition(3))
				{
					float normalizedTime4 = this.animator.GetAnimatorTransitionInfo(3).normalizedTime;
					this.animator.SetLayerWeightReflected(3, normalizedTime4);
					this.leftArmActive = true;
				}
				else if (!this.animator.IsInTransition(3) && this.fullBodyState3.tagHash == this.checkArmsHash)
				{
					this.leftArmActive = true;
					this.animator.SetLayerWeightReflected(3, 1f);
				}
				else if (this.nextFullBodyState3.tagHash == this.checkArmsHash && this.fullBodyState3.tagHash == this.checkArmsHash && this.animator.IsInTransition(3))
				{
					this.animator.SetLayerWeightReflected(3, 1f);
				}
				else if (this.nextFullBodyState3.tagHash == this.idleHash)
				{
					float normalizedTime5 = this.animator.GetAnimatorTransitionInfo(3).normalizedTime;
					this.animator.SetLayerWeightReflected(3, 1f - normalizedTime5);
					this.leftArmActive = false;
				}
				else if (this.fullBodyState3.tagHash == this.idleHash && !this.animator.IsInTransition(3))
				{
					this.animator.SetLayerWeightReflected(3, 0f);
				}
			}
		}
		Vector3 vector = this.tr.InverseTransformDirection(this.controller.velocity);
		if (this.doSledPushMode)
		{
			vector.x = TheForest.Utils.Input.GetAxis("Horizontal") * -8f;
		}
		if (this.onRope)
		{
			float num = 50f;
			if (LocalPlayer.FpCharacter.run)
			{
				num = 100f;
			}
			vector.x = TheForest.Utils.Input.GetAxis("Horizontal") * num;
			vector.z = TheForest.Utils.Input.GetAxis("Vertical") * num;
		}
		this.verticalSpeed = Mathf.SmoothStep(this.verticalSpeed, vector.z / this.maxSpeed, Time.deltaTime * this.walkBlendSpeed);
		this.horizontalSpeed = Mathf.SmoothStep(this.horizontalSpeed, vector.x / this.maxSpeed, Time.deltaTime * this.walkBlendSpeed);
		this.tempVelocity = this.controller.velocity;
		if (this.doSledPushMode)
		{
			this.tempVelocity = vector;
		}
		this.tempVelocity.y = 0f;
		this.overallSpeed = this.tempVelocity.magnitude / this.maxSpeed;
		if (this.onRope)
		{
			this.overallSpeed = 1f;
		}
		if (Mathf.Abs(float5 - this.horizontalSpeed) > 0.001f)
		{
			this.animator.SetFloat("hSpeed", this.horizontalSpeed);
		}
		if (Mathf.Abs(float4 - this.verticalSpeed) > 0.001f)
		{
			this.animator.SetFloat("vSpeed", this.verticalSpeed);
		}
		if (Mathf.Abs(float6 - this.overallSpeed) > 0.001f)
		{
			this.animator.SetFloat("overallSpeed", this.overallSpeed);
		}
		if (this.oculusDemo)
		{
			this.camX = LocalPlayer.MainCamTr.eulerAngles.x;
			this.camForward = LocalPlayer.MainCamTr.forward;
		}
		else
		{
			this.camX = LocalPlayer.MainCamTr.eulerAngles.x;
			this.camForward = LocalPlayer.MainCamTr.forward;
		}
		Vector3 forward = this.tr.forward;
		forward.y = 0f;
		this.camForward.y = 0f;
		Vector3 lhs = Vector3.Cross(forward, this.camForward);
		float num2 = Vector3.Dot(lhs, this.tr.up);
		if (this.camX > 180f)
		{
			this.normCamX = this.camX - 360f;
			this.absCamX = this.normCamX / 45f;
		}
		else
		{
			this.normCamX = this.camX;
			this.absCamX = this.normCamX / 45f;
		}
		if (!this.swimming && !LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._flintLockId) && !LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._flaregunId))
		{
			if (LocalPlayer.FpCharacter.crouch)
			{
				this.animator.SetFloatReflected("spineBlendX", 6f, 0.5f, Time.deltaTime * 5f);
			}
			else if (LocalPlayer.FpCharacter.crouching && !LocalPlayer.FpCharacter.crouch)
			{
				this.animator.SetFloatReflected("spineBlendX", 5f, 0.5f, Time.deltaTime * 5f);
			}
			else
			{
				this.animator.SetFloatReflected("spineBlendX", 10f, 0.5f, Time.deltaTime * 2f);
			}
		}
		else if (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._flintLockId) || LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._flaregunId))
		{
			this.animator.SetFloatReflected("spineBlendX", 10f, 0.5f, Time.deltaTime * 2f);
		}
		if (this.swimming && !this.onRope)
		{
			if (this.fullyAttachedToSled)
			{
				this.resetPushSled();
			}
			this.normCamX /= 82f;
			this.normCamX = Mathf.Clamp(this.normCamX, -1f, 0.75f) - 0.1f;
			float value = Mathf.Lerp(this.animator.GetLayerWeight(2), 1f, Time.deltaTime * 2f);
			if (this.animator.GetLayerWeight(2) < 1f)
			{
				this.animator.SetLayerWeightReflected(2, value);
			}
			else
			{
				this.animator.SetLayerWeightReflected(2, 1f);
			}
			if (LocalPlayer.FpCharacter.Diving)
			{
				this.animator.SetFloatReflected("spineBlendX", 10f, 0.5f, Time.deltaTime * 2f);
			}
			else
			{
				this.animator.SetFloatReflected("spineBlendX", 0f, 0.5f, Time.deltaTime * 2f);
			}
		}
		else if (this.animator.GetBool("bowHeld") || this.animator.GetBool("flintLockHeld") || this.animator.GetBool("chainSawHeld"))
		{
			this.normCamX /= 85f;
			this.normCamX = Mathf.Clamp(this.normCamX, -1f, 1f);
		}
		else if (this.animator.GetBool("spearHeld"))
		{
			this.normCamX /= 70f;
			this.normCamX = Mathf.Clamp(this.normCamX, -1f, 1f);
		}
		else
		{
			this.normCamX /= 82f;
			this.normCamX = Mathf.Clamp(this.normCamX, -1f, 0.75f) - 0.1f;
		}
		this.camY = LocalPlayer.MainRotator.followAngles.y;
		this.headCamY = LocalPlayer.CamRotator.followAngles.y;
		if (this.camY < -100f && this.prevCamY > 100f)
		{
			this.camYOffset -= 360f;
		}
		else if (this.camY > 100f && this.prevCamY < -100f)
		{
			this.camYOffset += 360f;
		}
		this.fsmPlayerAngle.Value = this.camY;
		this.camY -= this.camYOffset;
		this.normCamY = this.camY / 60f;
		this.normCamY = Mathf.Clamp(this.normCamY, -1f, 1f) * 10f;
		this.smoothCamX = Mathf.Lerp(this.smoothCamX, this.normCamX, Time.deltaTime * this.torsoFollowSpeed);
		if (this.normCamY == 10f)
		{
			this.animator.SetIntegerReflected("turnLegInt", 1);
			base.Invoke("resetLegInt", 0.35f);
		}
		else if (this.normCamY == -10f)
		{
			this.animator.SetIntegerReflected("turnLegInt", -1);
			base.Invoke("resetLegInt", 0.35f);
		}
		this.smoothCamY = this.normCamY;
		this.prevCamY = LocalPlayer.MainRotator.followAngles.y;
		if (LocalPlayer.FpCharacter.SailingRaft || this.onRockThrower || LocalPlayer.FpCharacter.Sitting)
		{
			this.animator.SetFloat("normCamY", this.headCamY);
		}
		else
		{
			this.animator.SetFloat("normCamY", this.smoothCamY);
		}
		if (Mathf.Abs(float2 - this.smoothCamX) > 0.001f)
		{
			this.animator.SetFloat("pedIdleBlend", this.smoothCamX);
		}
		if (Mathf.Abs(@float - this.normCamX) > 0.001f)
		{
			if (this.upsideDown)
			{
				this.animator.SetFloat("normCamX", -this.normCamX);
			}
			else if (LocalPlayer.FpCharacter.Sitting)
			{
				this.animator.SetFloat("normCamX", this.normCamX * 100f);
			}
			else
			{
				this.animator.SetFloat("normCamX", this.normCamX * this.startPlaneBlend);
			}
		}
		if (this.buoyancy.InWater && !this.onRopeWithGroundBelow)
		{
			if (this.rootTr.position.y + 0.5f < this.buoyancy.WaterLevel)
			{
				this.setup.targetInfo.inWater = true;
				if (!this.swimLayerChange && !this.waterBlock)
				{
					LocalPlayer.SpecialItems.SendMessage("stopLightHeldFire", SendMessageOptions.DontRequireReceiver);
					this.animator.SetBoolReflected("swimmingBool", true);
					this.swimming = true;
					LocalPlayer.Inventory.MemorizeItem(Item.EquipmentSlot.LeftHand);
					if (!LocalPlayer.Inventory.IsRightHandEmpty())
					{
						if (!LocalPlayer.Inventory.RightHand.IsHeldOnly)
						{
							LocalPlayer.Inventory.MemorizeItem(Item.EquipmentSlot.RightHand);
						}
						LocalPlayer.Inventory.StashEquipedWeapon(false);
					}
					else if (LocalPlayer.Inventory.Logs.HasLogs)
					{
						LocalPlayer.Inventory.Logs.PutDown(false, true, false, null);
						if (LocalPlayer.Inventory.Logs.HasLogs)
						{
							LocalPlayer.Inventory.Logs.PutDown(false, true, false, null);
						}
					}
					base.CancelInvoke("disableSwimming");
					this.waterBlock = true;
					LocalPlayer.Transform.localEulerAngles = new Vector3(0f, LocalPlayer.Transform.localEulerAngles.y, 0f);
				}
				if (this.rootTr.position.y + 1f < this.buoyancy.WaterLevel)
				{
					LocalPlayer.Inventory.StashLeftHand();
				}
				if ((this.absCamX > 0.65f && TheForest.Utils.Input.GetAxis("Vertical") > 0f) || LocalPlayer.WaterViz.WaterLevelSensor.position.y + 0.25f < this.buoyancy.WaterLevel)
				{
					if (!this.doShellRideMode)
					{
						this.rb.useGravity = false;
					}
					this.animator.SetBoolReflected("swimDiveBool", true);
					LocalPlayer.FpCharacter.Diving = true;
					LocalPlayer.FpCharacter.CanJump = false;
					if (LocalPlayer.Inventory.Owns(this._rebreatherId, true) && !LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.Chest, this._rebreatherId) && LocalPlayer.IsInCaves)
					{
						LocalPlayer.Inventory.Equip(this._rebreatherId, false);
					}
				}
				else if (LocalPlayer.FpCharacter.Diving && LocalPlayer.WaterViz.WaterLevelSensor.position.y > this.buoyancy.WaterLevel)
				{
					if (!this.doShellRideMode)
					{
						this.rb.useGravity = true;
					}
					this.animator.SetBoolReflected("swimDiveBool", false);
					LocalPlayer.FpCharacter.Diving = false;
					LocalPlayer.FpCharacter.CanJump = true;
				}
			}
			else if (this.rootTr.position.y + 0.25f > this.buoyancy.WaterLevel)
			{
				this.setup.targetInfo.inWater = false;
				if (this.waterBlock)
				{
					if (!LocalPlayer.FpCharacter.Grounded && !LocalPlayer.FpCharacter.jumping)
					{
						base.Invoke("disableSwimming", 1.2f);
					}
					else
					{
						this.disableSwimming();
					}
					this.waterBlock = false;
				}
			}
		}
		else
		{
			this.setup.targetInfo.inWater = false;
			if (this.waterBlock)
			{
				if (!LocalPlayer.FpCharacter.Grounded && !LocalPlayer.FpCharacter.jumping)
				{
					base.Invoke("disableSwimming", 1.2f);
				}
				else
				{
					this.disableSwimming();
				}
				this.waterBlock = false;
			}
		}
		if (this.carry && ((TheForest.Utils.Input.GetButtonDown("Drop") && !this.wallSetup.atWallCheck && !this.holdingGirl) || this.swimming))
		{
			this.DropBody();
		}
		if (this.waterBlock)
		{
			this.animator.SetFloatReflected("inWaterBlend", 10f, 1f, Time.deltaTime * 3f);
		}
		else if (this.doShellRideMode || LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._shellId) || LocalPlayer.FpCharacter.crouch || LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._mapId))
		{
			this.animator.SetFloatReflected("inWaterBlend", -10f, 1f, Time.deltaTime * 3f);
		}
		else if (this.animator.GetFloat("inWaterBlend") > 0.04f || this.animator.GetFloat("inWaterBlend") < -0.04f)
		{
			this.animator.SetFloatReflected("inWaterBlend", 0f, 1f, Time.deltaTime * 3f);
		}
		else
		{
			this.animator.SetFloatReflected("inWaterBlend", 0f);
		}
		this.armAngle = this.setup.leftArm.rotation;
	}

	
	private void disableSwimming()
	{
		base.StartCoroutine("smoothDisableLayerNew", 2);
		this.swimming = false;
		this.animator.SetBoolReflected("swimmingBool", false);
		this.animator.SetBoolReflected("swimDiveBool", false);
		LocalPlayer.GameObject.GetComponent<Rigidbody>().useGravity = true;
		LocalPlayer.FpCharacter.Diving = false;
		LocalPlayer.FpCharacter.CanJump = true;
		if (!this.onRope)
		{
			LocalPlayer.Inventory.EquipPreviousUtility(false);
			if (LocalPlayer.Inventory.IsRightHandEmpty() && !LocalPlayer.Inventory.Logs.HasLogs)
			{
				LocalPlayer.Inventory.EquipPreviousWeapon(false);
			}
		}
		this.waterBlock = false;
	}

	
	public void resetLegInt()
	{
		this.camYOffset = LocalPlayer.MainRotator.followAngles.y;
		this.animator.SetIntegerReflected("turnLegInt", 0);
	}

	
	private void LateUpdate()
	{
		Vector3 vector = this.tr.InverseTransformDirection(LocalPlayer.MainCamTr.forward);
		Quaternion quaternion = this.setup.leftArm.rotation;
		if ((LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._flintLockId) || LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._flaregunId)) && LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Inventory)
		{
			Quaternion quaternion2 = this.setup.rightArm.rotation;
			Quaternion quaternion3 = this.setup.leftArm.rotation;
			Quaternion quaternion4 = this.setup.rightHandWrist.rotation;
			Quaternion quaternion5 = this.setup.leftHandWrist.rotation;
			if ((this.nextLayerState1.shortNameHash == this.reloadFlintlockHash || this.nextLayerState1.shortNameHash == this.reloadFlareGunlockHash) && !LocalPlayer.Inventory.IsLeftHandEmpty())
			{
				LocalPlayer.Inventory.MemorizeItem(Item.EquipmentSlot.LeftHand);
				LocalPlayer.Inventory.StashLeftHand();
			}
			if (this.currLayerState1.shortNameHash == this.reloadFlintlockHash || this.currLayerState1.shortNameHash == this.reloadFlareGunlockHash)
			{
				if (this.currLayerState1.normalizedTime < 0.13f)
				{
					this.clampArmsVal = 1f;
					LocalPlayer.Inventory.LockEquipmentSlot(Item.EquipmentSlot.LeftHand);
					this.shouldUnlockLeftHandSlot = true;
				}
				else if (this.currLayerState1.normalizedTime < 0.65f)
				{
					this.clampArmsVal = Mathf.Lerp(this.clampArmsVal, 0f, Time.deltaTime * 2f);
					LocalPlayer.Inventory.LockEquipmentSlot(Item.EquipmentSlot.LeftHand);
					this.shouldUnlockLeftHandSlot = true;
				}
				else if (this.currLayerState1.normalizedTime > 0.65f && this.currLayerState1.normalizedTime < 0.93f)
				{
					this.clampArmsVal = Mathf.Lerp(this.clampArmsVal, 1f, Time.deltaTime * 3f);
					LocalPlayer.Inventory.LockEquipmentSlot(Item.EquipmentSlot.LeftHand);
					this.shouldUnlockLeftHandSlot = true;
				}
				else
				{
					this.clampArmsVal = 1f;
					LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.LeftHand);
					this.shouldUnlockLeftHandSlot = false;
					if (LocalPlayer.Inventory.IsLeftHandEmpty())
					{
						LocalPlayer.Inventory.EquipPreviousUtility(false);
					}
				}
			}
			else
			{
				if (this.shouldUnlockLeftHandSlot)
				{
					this.shouldUnlockLeftHandSlot = false;
					LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.LeftHand);
				}
				this.clampArmsVal = 1f;
			}
			if (!this.fovAimMode)
			{
				this.storeActualFovValue = LocalPlayer.MainCam.fieldOfView;
				this.fovOverride = LocalPlayer.MainCam.fieldOfView;
			}
			if (TheForest.Utils.Input.GetButton("AltFire") && !this.fovAimMode)
			{
				if (this.animator.GetBool("flintLockHeld"))
				{
					LocalPlayer.ScriptSetup.pmControl.SendEvent("toFlintLockAim");
				}
				else if (this.animator.GetBool("flaregunHeld"))
				{
					LocalPlayer.ScriptSetup.pmControl.SendEvent("toFlareGunAim");
				}
			}
			if (LocalPlayer.Animator.GetBool("spearRaiseBool"))
			{
				this.fovAimMode = true;
				PlayerPreferences.CanUpdateFov = false;
				this.fovOverride = Mathf.Lerp(this.fovOverride, 50f, Time.deltaTime * 5f);
				LocalPlayer.MainCam.fieldOfView = this.fovOverride;
			}
			else if (this.fovAimMode)
			{
				if (this.storeActualFovValue - LocalPlayer.MainCam.fieldOfView < 0.01f)
				{
					this.fovAimMode = false;
					PlayerPreferences.CanUpdateFov = true;
				}
				this.fovOverride = Mathf.Lerp(this.fovOverride, this.storeActualFovValue, Time.deltaTime * 5f);
				LocalPlayer.MainCam.fieldOfView = this.fovOverride;
			}
			quaternion3 = Quaternion.AngleAxis(this.normCamX * this.offsetFlintlockArmsMult * this.clampArmsVal * this.animator.GetLayerWeight(4), LocalPlayer.Transform.right) * quaternion3;
			quaternion2 = Quaternion.AngleAxis(this.normCamX * this.offsetFlintlockArmsMult * this.clampArmsVal * this.animator.GetLayerWeight(4), LocalPlayer.Transform.right) * quaternion2;
			quaternion5 = Quaternion.AngleAxis((this.normCamX + this.wristAimOffset) * this.offsetFlintlockHandsMult * this.clampArmsVal * this.animator.GetLayerWeight(4), LocalPlayer.Transform.right) * quaternion5;
			quaternion4 = Quaternion.AngleAxis((this.normCamX + this.wristAimOffset) * this.offsetFlintlockHandsMult * this.clampArmsVal * this.animator.GetLayerWeight(4), LocalPlayer.Transform.right) * quaternion4;
			if (!this.animator.GetBool("onHand"))
			{
				this.setup.leftArm.rotation = quaternion3;
				this.setup.leftHandWrist.rotation = quaternion5;
			}
			this.setup.rightArm.rotation = quaternion2;
			this.setup.rightHandWrist.rotation = quaternion4;
		}
		else if (this.shouldUnlockLeftHandSlot)
		{
			this.shouldUnlockLeftHandSlot = false;
			LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.LeftHand);
		}
		else if (this.fovAimMode)
		{
			this.fovAimMode = false;
			PlayerPreferences.CanUpdateFov = true;
		}
		if (this.leftArmActive)
		{
			if (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.LeftHand, this._torchId) && this.fullBodyState3.tagHash == this.checkArmsHash && LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Inventory)
			{
				if (this.onRope)
				{
					quaternion = Quaternion.AngleAxis(this.normCamX * this.armMultiplyer * 2f, LocalPlayer.Transform.right) * quaternion;
					quaternion *= Quaternion.AngleAxis(-vector.x * this.armMultiplyer * 2f, LocalPlayer.Transform.up);
				}
				else
				{
					float b = 0f;
					float b2 = 1f;
					if (this.fullBodyState3.shortNameHash != this.flashLightIdlehash)
					{
						b2 = 0f;
					}
					this.leftArmDamp = Mathf.Lerp(this.leftArmDamp, b2, Time.deltaTime * 5f);
					if (LocalPlayer.FpCharacter.crouch && LocalPlayer.Inventory.IsRightHandEmpty())
					{
						b = -1f;
					}
					else if (LocalPlayer.FpCharacter.crouch)
					{
						b = -0.4f;
					}
					this.torchCrouchOffset = Mathf.Lerp(this.torchCrouchOffset, b, Time.deltaTime * 10f);
					quaternion = Quaternion.AngleAxis((this.normCamX + this.torchCrouchOffset) * this.armMultiplyer * 0.8f * this.animator.GetLayerWeight(4) * this.leftArmDamp, LocalPlayer.Transform.right) * quaternion;
				}
				this.setup.leftArm.rotation = quaternion;
			}
		}
		else if (this.leftArmDamp > 0f)
		{
			this.leftArmDamp = Mathf.Lerp(this.leftArmDamp, 0f, Time.deltaTime * 5f);
		}
		if (this.animator.GetBool("shellHeld") && !this.doShellRideMode)
		{
			float to;
			if (this.animator.GetBool("lighterHeld") || this.animator.GetBool("flashLightHeld") || this.animator.GetBool("pedHeld") || this.animator.GetBool("walkmanHeld"))
			{
				to = 1f;
			}
			else
			{
				to = 0f;
			}
			this.smoothShellBlend = Mathf.SmoothStep(this.smoothShellBlend, to, Time.deltaTime * 10f);
			this.animator.SetFloatReflected("shellBlend", this.smoothShellBlend);
		}
		if (BoltNetwork.isRunning && LocalPlayer.Entity != null && LocalPlayer.Entity.isAttached && Time.time > this.frozenUpdateTimer && this.overallSpeed < 0.1f)
		{
			this.animator.SetFloatReflected("pullCraneSpeed", (float)((this.animator.GetFloat("pullCraneSpeed") <= 0f) ? 1 : 0));
			this.frozenUpdateTimer = Time.time + 5f;
		}
		Quaternion quaternion6 = this.setup.neckJnt.rotation;
		vector = this.tr.InverseTransformDirection(LocalPlayer.MainCamTr.forward);
		float num = 5f;
		if (this.cliffClimb)
		{
			num = 45f;
		}
		if (this.normCamX > 0f)
		{
			quaternion6 = Quaternion.AngleAxis(this.normCamX * num, LocalPlayer.Transform.right) * quaternion6;
		}
		this.setup.neckJnt.rotation = quaternion6;
		Vector3 center = this.playerCollider.center;
		Vector3 center2 = this.playerHeadCollider.center;
		center.z = Mathf.Clamp(this.normCamX, 0f, 0.4f);
		center2.z = Mathf.Clamp(this.normCamX, 0f, 0.4f);
		if (!this.doingGroundChop && !this.doingJumpCrouch)
		{
			this.playerHeadCollider.center = center2;
		}
		if (!this.cliffClimb)
		{
			this.playerCollider.center = center;
		}
	}

	
	private void FixedUpdate()
	{
		if (this.currLayerState0.tagHash == this.shellRideHash)
		{
			if (!this.doShellRideMode)
			{
				this.enableShellRideParams();
			}
			this.HandleShellRideControlFixedUpdate();
		}
		else
		{
			if (this.doShellRideMode)
			{
				this.disableShellRideParams();
			}
			this.doShellRideMode = false;
		}
		if (this.onRope && ++this.fixedCount > 5)
		{
			if (this.closePlayer)
			{
				this.closePlayerYVel = (this.closePlayer.position.y - this.closePlayerHeight) * 100f;
				this.closePlayerHeight = this.closePlayer.position.y;
			}
			this.yVel = (LocalPlayer.Transform.position.y - this.lastYPos) * 100f;
			this.lastYPos = LocalPlayer.Transform.position.y;
			this.fixedCount = 0;
		}
	}

	
	private void OnDestroy()
	{
		if (this.fovAimMode)
		{
			PlayerPreferences.CanUpdateFov = true;
		}
	}

	
	private void updateCliffClimb()
	{
		if (this.cliffClimb)
		{
			int num = 8192;
			this.playerHeadCollider.enabled = false;
			LocalPlayer.AnimControl.playerCollider.height = 3.5f;
			LocalPlayer.AnimControl.playerCollider.center = new Vector3(0f, 0f, 0f);
			LocalPlayer.FpCharacter.playerPhysicMaterial.dynamicFriction = 0f;
			LocalPlayer.FpCharacter.playerPhysicMaterial.staticFriction = 0f;
			LocalPlayer.FpCharacter.playerPhysicMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
			LocalPlayer.Rigidbody.drag = 25f;
			LocalPlayer.Rigidbody.angularDrag = 25f;
			this.rootTr.localEulerAngles = new Vector3(this.rootTr.localEulerAngles.x, this.rootTr.localEulerAngles.y, 0f);
			LocalPlayer.PlayerBase.transform.localEulerAngles = Vector3.zero;
			if (!this.allowCliffReset)
			{
				base.Invoke("enableCliffReset", 2f);
			}
			RaycastHit raycastHit;
			if (Physics.Raycast(this.rb.position - this.rootTr.forward, this.rootTr.forward, out raycastHit, 12f, num))
			{
				if (!raycastHit.collider.CompareTag("climbWall") && LocalPlayer.IsInCaves)
				{
					this.player.SpecialActions.SendMessage("exitClimbCliffGround");
					this.allowCliffReset = false;
					base.CancelInvoke("enableCliffReset");
				}
				if (!LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._climbingAxeId))
				{
					this.player.SpecialActions.SendMessage("exitClimbCliffGround");
					this.allowCliffReset = false;
					base.CancelInvoke("enableCliffReset");
				}
				Vector3 a = raycastHit.point - this.rootTr.forward * 1.6f;
				if (LocalPlayer.IsInCaves)
				{
					a = raycastHit.point - this.rootTr.forward * 1.36f;
				}
				Vector3 origin = this.rootTr.position + this.rootTr.right * 2f + this.rootTr.up * 2f;
				if (Physics.Raycast(origin, this.rootTr.forward, out this.nHit, 20f, num))
				{
					if (LocalPlayer.IsInCaves && this.nHit.collider.gameObject.CompareTag("climbWall"))
					{
						this.normal1 = this.nHit.normal;
					}
					else if (!LocalPlayer.IsInCaves)
					{
						this.normal1 = this.nHit.normal;
					}
				}
				origin = this.rootTr.position + this.rootTr.right * -2f + this.rootTr.up * 2f;
				if (Physics.Raycast(origin, this.rootTr.forward, out this.nHit, 20f, num))
				{
					if (LocalPlayer.IsInCaves && this.nHit.collider.gameObject.CompareTag("climbWall"))
					{
						this.normal2 = this.nHit.normal;
					}
					else if (!LocalPlayer.IsInCaves)
					{
						this.normal2 = this.nHit.normal;
					}
				}
				origin = this.rootTr.position + this.rootTr.right * -2f + this.rootTr.up * -2f;
				if (Physics.Raycast(origin, this.rootTr.forward, out this.nHit, 20f, num))
				{
					if (LocalPlayer.IsInCaves && this.nHit.collider.gameObject.CompareTag("climbWall"))
					{
						this.normal3 = this.nHit.normal;
					}
					else if (!LocalPlayer.IsInCaves)
					{
						this.normal3 = this.nHit.normal;
					}
				}
				origin = this.rootTr.position + this.rootTr.right * 2f + this.rootTr.up * -2f;
				if (Physics.Raycast(origin, this.rootTr.forward, out this.nHit, 20f, num))
				{
					if (LocalPlayer.IsInCaves && this.nHit.collider.gameObject.CompareTag("climbWall"))
					{
						this.normal4 = this.nHit.normal;
					}
					else if (!LocalPlayer.IsInCaves)
					{
						this.normal4 = this.nHit.normal;
					}
				}
				origin = this.rootTr.position;
				if (Physics.Raycast(origin, this.rootTr.forward, out this.nHit, 20f, num))
				{
					if (LocalPlayer.IsInCaves && this.nHit.collider.gameObject.CompareTag("climbWall"))
					{
						this.normal5 = this.nHit.normal;
					}
					else if (!LocalPlayer.IsInCaves)
					{
						this.normal5 = this.nHit.normal;
					}
				}
				Vector3 vector = (this.normal1 + this.normal2 + this.normal3 + this.normal4 + this.normal5) / 5f;
				float num2 = (vector - this.prevNormal).magnitude * 10f;
				if (!LocalPlayer.IsInCaves && this.rb.velocity.magnitude < 0.08f && num2 < 0.08f)
				{
					this.rootTr.rotation = this.rootTr.rotation;
				}
				else
				{
					this.rootTr.rotation = Quaternion.Lerp(this.rootTr.rotation, Quaternion.LookRotation(vector * -1f, this.rootTr.up), Time.deltaTime * 4f);
				}
				this.prevNormal = vector;
				if (Vector3.Angle(Vector3.up, vector) < 30f && this.allowCliffReset)
				{
					this.player.SpecialActions.SendMessage("exitClimbCliffGround");
					this.allowCliffReset = false;
					base.CancelInvoke("enableCliffReset");
				}
				Vector3 deltaPosition = this.animator.deltaPosition;
				this.rb.MovePosition(a + deltaPosition * 1.6f);
				if (this.animator.deltaPosition.magnitude < 0.05f)
				{
					this.rb.velocity = Vector3.zero;
				}
			}
			else if (this.allowCliffReset)
			{
				this.player.SpecialActions.SendMessage("exitClimbCliffGround");
				this.allowCliffReset = false;
				base.CancelInvoke("enableCliffReset");
			}
		}
	}

	
	private void enableCliffReset()
	{
		this.allowCliffReset = true;
	}

	
	private void enablePointAtMap()
	{
		this.animator.SetBoolReflected("pointAtMap", true);
	}

	
	private void lookAtCamera()
	{
		this.tr.LookAt(this.setup.lookAtTr);
	}

	
	private void OnAnimatorMove()
	{
		this.storeLeftArmAngle = this.setup.leftArm.rotation;
		if (LocalPlayer.FpCharacter.jumping && LocalPlayer.FpCharacter.crouch)
		{
			if (this.setup && this.setup.headJnt)
			{
				this.doingJumpCrouch = true;
				Vector3 center = this.rootTr.InverseTransformPoint(this.setup.headJnt.position);
				center.x = this.playerHeadCollider.center.x;
				center.z = this.playerHeadCollider.center.z;
				this.playerHeadCollider.center = center;
			}
		}
		else if (this.fullBodyState2.shortNameHash == this.axeGround1Hash || this.fullBodyState2.shortNameHash == this.axeGround2Hash || this.fullBodyState2.shortNameHash == this.axeAttackHash)
		{
			this.doingGroundChop = true;
			if (this.setup && this.setup.headJnt && this.rootTr)
			{
				Vector3 center2 = this.rootTr.InverseTransformPoint(this.setup.headJnt.position);
				this.playerHeadCollider.center = center2;
			}
		}
		else if (this.doingGroundChop || this.doingJumpCrouch)
		{
			Vector3 center3 = this.playerHeadCollider.center;
			center3.z = this.headColliderZpos;
			center3.x = 0f;
			if (LocalPlayer.FpCharacter.crouching)
			{
				center3.y = -0.1f;
			}
			else
			{
				center3.y = 1.76f;
			}
			this.playerHeadCollider.center = center3;
			this.doingGroundChop = false;
			this.doingJumpCrouch = false;
		}
		if (this.useRootMotion)
		{
			if (!this.cliffClimb)
			{
				this.animator.applyRootMotion = true;
			}
			if (this.player._inventoryGO.activeSelf)
			{
				LocalPlayer.Inventory.Close();
			}
			if (this.lockGravity)
			{
				this.rb.useGravity = false;
			}
			else
			{
				this.rb.useGravity = true;
				this.rb.isKinematic = false;
			}
			if (this.cliffClimb)
			{
				this.updateCliffClimb();
				this.rb.isKinematic = false;
			}
			else if (this.lockGravity)
			{
				this.rb.isKinematic = true;
			}
			Vector3 position = this.rootTr.position;
			if (this.onRopeHeightCheck && this.currLayerState0.shortNameHash != this.enterClimbTopHash && this.currLayerState0.tagHash != this.climbOutHash && this.nextLayerState0.shortNameHash != this.enterClimbTopHash && !this.blockHeightCheck)
			{
				if (Physics.SphereCast(position, 0.2f, Vector3.down, out this.ropeHit, 6f, this.layerMask))
				{
					this.onRopeWithGroundBelow = true;
				}
				else
				{
					this.onRopeWithGroundBelow = false;
				}
			}
			else
			{
				this.onRopeWithGroundBelow = false;
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			if (this.currLayerState0.shortNameHash == this.enterClimbTopHash || this.currLayerState0.tagHash == this.climbOutHash)
			{
				flag3 = true;
			}
			if (!this.cliffClimb)
			{
				Vector3 deltaPosition = this.animator.deltaPosition;
				this.closePlayer = null;
				if (this.onRopeHeightCheck && BoltNetwork.isRunning)
				{
					this.closePlayer = this.findIntersectingPlayers();
					if (this.closePlayer)
					{
						float num = this.closePlayer.position.y - LocalPlayer.Transform.position.y;
						float num2 = 4.6f;
						if (num < num2 && num > 0f && this.lockGravity && !flag3)
						{
							if (this.onRopeWithGroundBelow && this.ropeHit.distance < 5f)
							{
								if (this.closePlayerYVel < -8f)
								{
									flag = true;
								}
							}
							else
							{
								this.rootTr.position = new Vector3(this.rootTr.position.x, this.rootTr.position.y - (num2 - num), this.rootTr.position.z);
							}
							if (this.yVel < -10f)
							{
								flag2 = true;
							}
							deltaPosition.y = 0f;
						}
						if (this.animator.deltaPosition.y > 0f && TheForest.Utils.Input.GetAxis("Vertical") > 0.2f && num < num2 && num > 0f && !flag3)
						{
							this.rootTr.position = new Vector3(this.rootTr.position.x, this.rootTr.position.y - (num2 - num), this.rootTr.position.z);
							deltaPosition.y = 0f;
						}
					}
				}
				this.rootTr.position += deltaPosition;
				if (this.useRootRotation)
				{
					this.rootTr.rotation = this.animator.rootRotation;
				}
			}
			if (this.onRope || this.injured)
			{
				LocalPlayer.Animator.SetLayerWeightReflected(4, 0f);
			}
			if (this.cliffClimb)
			{
				LocalPlayer.Animator.SetLayerWeightReflected(2, 0f);
				LocalPlayer.Animator.SetLayerWeightReflected(1, 0f);
			}
			if (this.onRope)
			{
				if (this.cliffClimb)
				{
					if (LocalPlayer.MainCam.fov > 80f)
					{
						LocalPlayer.CamRotator.rotationRange = new Vector2(88f, 80f);
					}
					else
					{
						LocalPlayer.CamRotator.rotationRange = new Vector2(98f, 85f);
					}
				}
				else if (this.normCamX > 0f && this.currLayerState0.tagHash == this.climbIdleHash)
				{
					LocalPlayer.CamRotator.rotationRange = new Vector2(103f, 100f);
				}
				else if (this.normCamX < 0f && this.currLayerState0.tagHash == this.climbIdleHash)
				{
					LocalPlayer.CamRotator.rotationRange = new Vector2(150f, 105f);
				}
				else if (this.normCamX > 0f && this.currLayerState0.tagHash != this.climbIdleHash)
				{
					LocalPlayer.CamRotator.rotationRange = new Vector2(73f, 105f);
				}
				else
				{
					LocalPlayer.CamRotator.rotationRange = new Vector2(150f, 105f);
				}
			}
			if (this.injured)
			{
				this.tr.localEulerAngles = new Vector3(0f, 0f, 0f);
				LocalPlayer.Animator.SetLayerWeightReflected(4, 0f);
				LocalPlayer.Animator.SetLayerWeightReflected(0, 0f);
				LocalPlayer.Animator.SetLayerWeightReflected(1, 0f);
				LocalPlayer.Animator.SetLayerWeightReflected(2, 1f);
				LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
				return;
			}
			if (this.onRopeWithGroundBelow && !flag3 && this.onRopeHeightCheck && !this.cliffClimb)
			{
				float num3 = this.playerCollider.height / 2f;
				if (this.ropeHit.distance >= num3 || flag2)
				{
				}
				if (this.ropeHit.distance < 5.5f && (TheForest.Utils.Input.GetAxis("Vertical") < -0.1f || flag2 || flag) && this.animator.GetBool("setClimbBool") && (this.currLayerState0.tagHash == this.climbingHash || this.currLayerState0.tagHash == this.climbIdleHash))
				{
					int integer = this.animator.GetInteger("climbTypeInt");
					if (integer == 0)
					{
						this.player.SpecialActions.SendMessage("exitClimbRopeGround", false);
					}
					if (integer == 1)
					{
						this.player.SpecialActions.SendMessage("exitClimbWallGround");
					}
				}
			}
		}
		else
		{
			if (this.animator)
			{
				this.animator.applyRootMotion = false;
			}
			this.onRopeWithGroundBelow = false;
		}
	}

	
	private IEnumerator fixExitClimbPosition(Vector3 groundPos)
	{
		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime;
			if (LocalPlayer.Transform.position.y - this.playerCollider.height / 2f < groundPos.y)
			{
				Vector3 position = LocalPlayer.Transform.position;
				position.y = groundPos.y + this.playerCollider.height / 2f;
				LocalPlayer.Transform.position = position;
			}
			yield return null;
		}
		yield break;
	}

	
	public void hitCombo()
	{
		if (!this.comboBlock)
		{
			this.combo++;
			this.comboBlock = true;
			base.Invoke("resetComboBlock", 0.27f);
			if (this.combo >= 1)
			{
				if (this.combo > 3)
				{
					this.combo = 0;
				}
				this.animator.SetBoolReflected("comboBool", true);
			}
		}
	}

	
	private void resetCombo()
	{
		this.combo = 0;
	}

	
	private void setComboOne()
	{
		this.combo = 1;
	}

	
	private void resetComboBlock()
	{
		this.comboBlock = false;
	}

	
	private void setStickAttack()
	{
		this.animator.SetBoolReflected("stickAttack", true);
	}

	
	public void resetAnimator()
	{
		this.animator.SetTriggerReflected("resetTrigger");
	}

	
	public void enableUseRootMotion()
	{
		this.useRootMotion = true;
	}

	
	public void enterClimbMode()
	{
		LocalPlayer.SpecialItems.SendMessage("stopLightHeldFire", SendMessageOptions.DontRequireReceiver);
		this.useRootMotion = true;
		this.onRope = true;
		this.onRopeHeightCheck = true;
	}

	
	public void enterPushMode()
	{
		this.doSledPushMode = true;
	}

	
	public void exitPushMode()
	{
		this.doSledPushMode = false;
	}

	
	public void exitClimbMode()
	{
		if (this.useRootMotion)
		{
			this.playerHeadCollider.enabled = true;
			LocalPlayer.GameObject.GetComponent<Rigidbody>().freezeRotation = true;
			this.rootTr.localEulerAngles = new Vector3(0f, this.rootTr.localEulerAngles.y, 0f);
			this.controller.useGravity = true;
			this.controller.isKinematic = false;
			this.animator.SetLayerWeightReflected(4, 1f);
			this.player.PM.FsmVariables.GetFsmBool("climbBool").Value = false;
			this.player.PM.FsmVariables.GetFsmBool("climbTopBool").Value = false;
			LocalPlayer.CamRotator.rotationRange = new Vector2(LocalPlayer.FpCharacter.minCamRotationRange, 0f);
			LocalPlayer.MainRotator.enabled = true;
			if (this.onRope || this.cliffClimb)
			{
				LocalPlayer.MainRotator.enabled = true;
				LocalPlayer.MainRotator.resetOriginalRotation = true;
			}
			this.setup.pmControl.SendEvent("toExitClimb");
			int integer = this.animator.GetInteger("climbTypeInt");
			if (integer == 0)
			{
				if (this.onRope)
				{
					this.player.SpecialActions.SendMessage("resetClimbRope");
				}
			}
			else if (integer == 1)
			{
				this.player.SpecialActions.SendMessage("resetClimbWall");
				this.player.SpecialActions.SendMessage("resetClimbCliff");
			}
			this.animator.SetBoolReflected("exitClimbTopBool", false);
			this.onRope = false;
			this.onRopeHeightCheck = false;
			this.useRootMotion = false;
			base.CancelInvoke("enableCliffReset");
			this.allowCliffReset = false;
			this.cliffClimb = false;
		}
	}

	
	public void resetPushSled()
	{
		this.player.SpecialActions.SendMessage("forceExitSled");
	}

	
	public void sendEnableSledTrigger()
	{
		this.player.SpecialActions.SendMessage("enableSledTrigger");
	}

	
	public void resetCliffClimb()
	{
		this.player.SpecialActions.SendMessage("exitClimbCliffGround");
	}

	
	public void enableAnimLayer2()
	{
		if (this.animator.GetFloat("crouch") < 0.5f)
		{
			this.animator.SetLayerWeightReflected(2, 1f);
		}
		else
		{
			this.animator.SetLayerWeightReflected(2, 0f);
		}
	}

	
	public void setDeathState()
	{
	}

	
	public void setTimmyPickup(GameObject go)
	{
		if (!this.carry)
		{
			this.placedBodyGo = go;
			this.placedBodyGo.SetActive(false);
			this.heldBodyGo.SetActive(true);
			base.Invoke("setCarryBool", 0.5f);
		}
	}

	
	public void setMutantPickUp(GameObject go)
	{
		if (!this.heldBodyGo.activeSelf)
		{
			Scene.HudGui.DropButton.SetActive(true);
			this.player.MemorizeItem(Item.EquipmentSlot.RightHand);
			this.player.StashEquipedWeapon(false);
			dummyAnimatorControl component = go.GetComponent<dummyAnimatorControl>();
			this.placedBodyGo = go;
			if (BoltNetwork.isRunning && go.GetComponentInChildren<BoltEntity>())
			{
				SetCorpsePosition setCorpsePosition = SetCorpsePosition.Create(GlobalTargets.Everyone);
				setCorpsePosition.Corpse = go.GetComponentInChildren<BoltEntity>();
				setCorpsePosition.Corpse.Freeze(false);
				setCorpsePosition.Pickup = true;
				setCorpsePosition.Send();
				if (BoltNetwork.isClient)
				{
					this.placedBodyGo.SendMessage("sendResetRagDoll", SendMessageOptions.DontRequireReceiver);
				}
			}
			else
			{
				this.placedBodyGo.SetActive(false);
			}
			this.heldBodyGo.SetActive(true);
			if (component)
			{
				component.BodyCollider.enabled = true;
				component.burnTrigger.gameObject.SetActive(true);
			}
			base.Invoke("setCarryBool", 0.5f);
			this.animator.SetBoolReflected("bodyHeld", true);
		}
	}

	
	public GameObject DropBody()
	{
		return this.DropBody(false);
	}

	
	public GameObject DropBody(bool destroy)
	{
		this.animator.SetBoolReflected("bodyHeld", false);
		this.carry = false;
		if (!this.swimming)
		{
			this.player.ShowRightHand(false);
		}
		Scene.HudGui.DropButton.SetActive(false);
		this.heldBodyGo.SetActive(false);
		Vector3 vector = this.tr.position + this.tr.forward * 2f;
		if (BoltNetwork.isRunning)
		{
			vector = this.tr.position + this.tr.forward * 3f;
		}
		Quaternion rotation = this.rootTr.rotation;
		if (!BoltNetwork.isRunning)
		{
			this.placedBodyGo.transform.position = this.tr.position + this.tr.forward * 2f;
		}
		this.placedBodyGo.transform.rotation = this.rootTr.rotation;
		this.placedBodyGo.transform.localEulerAngles = new Vector3(this.placedBodyGo.transform.localEulerAngles.x, this.placedBodyGo.transform.localEulerAngles.y + 35f, this.placedBodyGo.transform.localEulerAngles.z);
		Vector3 vector2 = new Vector3(this.placedBodyGo.transform.position.x, this.placedBodyGo.transform.position.y + 8f, this.placedBodyGo.transform.position.z);
		int num = 103948289;
		Vector3 origin = vector;
		origin.y += 5f;
		RaycastHit raycastHit;
		if (Physics.Raycast(origin, Vector3.down, out raycastHit, 15f, num, QueryTriggerInteraction.Ignore))
		{
			this.placedBodyGo.transform.position = raycastHit.point;
			this.placedBodyGo.transform.rotation = Quaternion.LookRotation(Vector3.Cross(this.placedBodyGo.transform.right, raycastHit.normal), raycastHit.normal);
			vector = raycastHit.point;
		}
		if (BoltNetwork.isRunning && this.placedBodyGo.GetComponent<BoltEntity>() && this.placedBodyGo.GetComponent<BoltEntity>().IsAttached())
		{
			DroppedBody droppedBody = DroppedBody.Create(GlobalTargets.Everyone);
			droppedBody.Target = this.placedBodyGo.GetComponent<BoltEntity>();
			droppedBody.rot = this.placedBodyGo.transform.rotation;
			droppedBody.Send();
			SetCorpsePosition setCorpsePosition = SetCorpsePosition.Create(GlobalTargets.Everyone);
			setCorpsePosition.Corpse = this.placedBodyGo.GetComponent<BoltEntity>();
			setCorpsePosition.Corpse.Freeze(false);
			setCorpsePosition.Position = vector;
			setCorpsePosition.Rotation = this.placedBodyGo.transform.rotation;
			setCorpsePosition.Pickup = false;
			setCorpsePosition.Destroy = destroy;
			setCorpsePosition.Send();
			this.placedBodyGo.transform.position = new Vector3(4096f, 4096f, 4096f);
		}
		else
		{
			this.placedBodyGo.SetActive(true);
			this.placedBodyGo.SendMessage("dropFromCarry", false, SendMessageOptions.DontRequireReceiver);
			this.placedBodyGo.SendMessage("setRagDollDrop", SendMessageOptions.DontRequireReceiver);
		}
		return this.placedBodyGo;
	}

	
	private void setCarryBool()
	{
		this.carry = true;
	}

	
	private IEnumerator checkChargeConditions()
	{
		this.fsmButtonHeldBool.Value = false;
		this.animator.SetBoolReflected("chargingBool", true);
		this.animator.SetBoolReflected("doAttackHeavyBool", false);
		float comboTimer = Time.time + 0.4f;
		bool startedCharge = false;
		while (!this.fsmButtonHeldBool.Value)
		{
			if (Time.time > comboTimer && !startedCharge)
			{
				LocalPlayer.ScriptSetup.pmControl.SendEvent("initChargeAttack");
				startedCharge = true;
			}
			if (TheForest.Utils.Input.GetButtonUp("Fire1"))
			{
				if (Time.time > comboTimer)
				{
					this.animator.SetBoolReflected("doAttackHeavyBool", true);
					this.animator.SetBoolReflected("stickAttack", false);
					this.fsmButtonHeldBool.Value = true;
				}
				else
				{
					this.animator.SetBoolReflected("chargingBool", false);
					this.fsmButtonHeldBool.Value = true;
				}
				yield break;
			}
			yield return null;
			if (TheForest.Utils.Input.GetButtonDown("Fire1"))
			{
				this.animator.SetBoolReflected("chargingBool", false);
				this.fsmButtonHeldBool.Value = true;
				yield break;
			}
		}
		yield break;
	}

	
	private void heavySwingDebug()
	{
	}

	
	public int getTerrainSurface()
	{
		return TerrainHelper.GetProminantTextureIndex(base.transform.position);
	}

	
	private void disableLockGravity()
	{
		this.lockGravity = false;
	}

	
	private void setBlockParams()
	{
		if (this.fullBodyState2.tagHash != this.explodeHash)
		{
			this.animator.SetLayerWeightReflected(2, 0f);
		}
	}

	
	private void checkCrouchLayers()
	{
		if (LocalPlayer.Animator.GetFloat("crouch") > 5f)
		{
			base.StartCoroutine(this.smoothDisableLayerNew(2));
		}
	}

	
	public void sendCoopBlock()
	{
	}

	
	public void forceAnimSpineReset()
	{
		this.animator.SetLayerWeightReflected(4, 1f);
	}

	
	public void resetCamera()
	{
		LocalPlayer.CamFollowHead.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
	}

	
	public void doSmoothEnableLayer2()
	{
		this.smoothEnableLayer(2, 0.5f);
	}

	
	public void doSmoothDisableLayer2()
	{
		this.smoothDisableLayer(2, 0.5f);
	}

	
	public IEnumerator scaleCapsuleForAxe()
	{
		yield break;
	}

	
	public IEnumerator smoothEnableLayer(int l, float s)
	{
		base.StopCoroutine("smoothDisableLayer");
		float t = 0f;
		float startVal = this.animator.GetLayerWeight(l);
		float val = startVal;
		if (this.swimLayerChange)
		{
			yield break;
		}
		while (t < 1f * s)
		{
			this.swimLayerChange = true;
			t += Time.deltaTime * s;
			val = Mathf.Lerp(startVal, 1f, t);
			this.animator.SetLayerWeightReflected(l, val);
			yield return null;
		}
		this.swimLayerChange = false;
		yield break;
	}

	
	public IEnumerator smoothDisableLayer(int l, float s)
	{
		base.StopCoroutine("smoothEnableLayer");
		float t = 0f;
		float startVal = this.animator.GetLayerWeight(l);
		float val = startVal;
		if (this.swimLayerChange)
		{
			yield break;
		}
		while (t < 1f * s)
		{
			this.swimLayerChange = true;
			t += Time.deltaTime * s;
			val = Mathf.Lerp(startVal, 0f, t);
			this.animator.SetLayerWeightReflected(l, val);
			yield return null;
		}
		this.swimLayerChange = false;
		yield break;
	}

	
	public IEnumerator smoothEnableLayerNew(int l)
	{
		if (!this.animator)
		{
			yield break;
		}
		base.StopCoroutine("smoothDisableLayerNew");
		float t = 0f;
		float startVal = this.animator.GetLayerWeight(l);
		float val = startVal;
		while (t < 1f)
		{
			t += Time.deltaTime * 2f;
			val = Mathf.Lerp(startVal, 1f, t);
			this.animator.SetLayerWeightReflected(l, val);
			yield return null;
		}
		yield break;
	}

	
	public IEnumerator smoothDisableLayerNew(int l)
	{
		if (!this.animator)
		{
			yield break;
		}
		float t = 0f;
		float startVal = this.animator.GetLayerWeight(l);
		float val = startVal;
		while (t < 1f)
		{
			t += Time.deltaTime * 2f;
			val = Mathf.Lerp(startVal, 0f, t);
			this.animator.SetLayerWeightReflected(l, val);
			yield return null;
		}
		yield break;
	}

	
	public void stopSmoothEnableRoutines()
	{
		base.StopCoroutine("smoothEnableLayerNew");
		base.StopCoroutine("smoothDisableLayerNew");
	}

	
	public void disconnectFromObject()
	{
		if (this.currRaft)
		{
			this.currRaft.SendMessage("offRaft", SendMessageOptions.DontRequireReceiver);
		}
		if (this.onRockThrower)
		{
			LocalPlayer.SpecialActions.SendMessage("exitThrower", SendMessageOptions.DontRequireReceiver);
		}
		if (this.sitting)
		{
			LocalPlayer.SpecialActions.SendMessage("forceDisableBench", SendMessageOptions.DontRequireReceiver);
		}
		LocalPlayer.SpecialActions.SendMessage("forceDisableSled", SendMessageOptions.DontRequireReceiver);
		LocalPlayer.SpecialActions.SendMessage("ForceCraneReset");
		if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Book)
		{
			LocalPlayer.Create.CloseTheBook(false);
		}
	}

	
	
	public bool WaterBlock
	{
		get
		{
			return this.waterBlock;
		}
	}

	
	private void enableBirdOnHand()
	{
		this.setup.smallBirdGo.SetActive(true);
		this.bird = UnityEngine.Object.Instantiate<GameObject>((GameObject)Resources.Load("CutScene/smallBird_ANIM_landOnFinger_prefab"), this.setup.smallBirdGo.transform.position, this.setup.smallBirdGo.transform.rotation);
		this.bird.transform.parent = this.setup.smallBirdGo.transform;
		this.bird.transform.localPosition = Vector3.zero;
		this.bird.transform.localRotation = Quaternion.identity;
		Animator component = this.bird.GetComponent<Animator>();
		component.SetBool("toHand", true);
		base.Invoke("disableBirdOnHand", 10f);
	}

	
	public void disableBirdOnHand()
	{
		if (this.bird)
		{
			UnityEngine.Object.Destroy(this.bird);
		}
	}

	
	public void cancelAnimatorActions()
	{
		this.animator.SetBoolReflected("lookAtItemRight", false);
		this.animator.SetBoolReflected("lookAtPhoto", false);
		this.animator.SetBoolReflected("lookAtItem", false);
	}

	
	public void runGotHitScripts()
	{
		if (LocalPlayer.Animator.GetBool("bookHeld"))
		{
			this.animEvents.bookHeld.SendMessage("FinalCloseBook", SendMessageOptions.DontRequireReceiver);
		}
		this.disconnectFromObject();
		this.animEvents.disableWeapon();
		this.animEvents.disableSmashWeapon();
		this.animEvents.disableWeapon2();
		LocalPlayer.HitReactions.enableHitState();
		if (this.currLayerState1.shortNameHash != this.reloadFlintlockHash && this.currLayerState1.shortNameHash != this.reloadFlareGunlockHash)
		{
			LocalPlayer.Inventory.CancelReloadDelay();
		}
		this.disableBirdOnHand();
		this.animEvents.resetDrinkParams();
		if (this.skinningAnimal)
		{
			LocalPlayer.SpecialActions.SendMessage("forceSkinningReset");
		}
		LocalPlayer.SpecialActions.SendMessage("forceGirlReset");
		LocalPlayer.SpecialActions.SendMessage("ForceCraneReset");
		LocalPlayer.Animator.SetBool("resetBook", true);
		base.Invoke("disableResetBook", 1f);
	}

	
	private void disableResetBook()
	{
		LocalPlayer.Animator.SetBool("resetBook", false);
	}

	
	private void resetCarryingGirl()
	{
		LocalPlayer.SpecialActions.SendMessage("forceGirlResetFromExplosion");
	}

	
	public void runReset2Scripts()
	{
		if (!this.endGameCutScene)
		{
			this.exitClimbMode();
			this.resetCombo();
			LocalPlayer.HitReactions.disableControllerFreeze();
			LocalPlayer.HitReactions.disableBlockState();
		}
	}

	
	public void runWaitForInputScripts()
	{
		this.setBlockParams();
		this.animEvents.disableWeapon();
	}

	
	private Transform findIntersectingPlayers()
	{
		for (int i = 0; i < Scene.SceneTracker.allPlayers.Count; i++)
		{
			if (Scene.SceneTracker.allPlayers[i] && Scene.SceneTracker.allPlayers[i].CompareTag("PlayerNet") && LocalPlayer.GameObject.activeSelf)
			{
				Collider component = Scene.SceneTracker.allPlayers[i].GetComponent<Collider>();
				if (this.playerHeadCollider.enabled && component.gameObject.activeSelf && component.enabled)
				{
					Physics.IgnoreCollision(this.playerHeadCollider, component, true);
				}
				if (this.playerCollider.enabled && component.gameObject.activeSelf && component.enabled)
				{
					Physics.IgnoreCollision(this.playerCollider, component, true);
				}
			}
		}
		return null;
	}

	
	public void lockPlayerParams()
	{
		LocalPlayer.ScriptSetup.bodyCollisionGo.SetActive(false);
		LocalPlayer.AnimControl.playerHeadCollider.enabled = false;
		LocalPlayer.AnimControl.playerCollider.enabled = false;
		LocalPlayer.AnimControl.useRootMotion = true;
		LocalPlayer.FpCharacter.allowFallDamage = false;
		LocalPlayer.FpCharacter.Locked = true;
		LocalPlayer.ScriptSetup.forceLocalPos.enabled = false;
		LocalPlayer.Inventory.UnequipItemAtSlot(Item.EquipmentSlot.Chest, false, true, false);
		Scene.HudGui.ShowHud(false);
		LocalPlayer.FpCharacter.CanJump = false;
		LocalPlayer.AnimControl.endGameCutScene = true;
		LocalPlayer.Create.Grabber.gameObject.SetActive(false);
		LocalPlayer.AnimControl.playerHeadCollider.enabled = false;
		LocalPlayer.Animator.SetBool("onHand", false);
		LocalPlayer.Animator.SetBool("jumpBool", false);
		LocalPlayer.Inventory.HideAllEquiped(false, false);
		LocalPlayer.Inventory.StashLeftHand();
		LocalPlayer.Inventory.LockEquipmentSlot(Item.EquipmentSlot.LeftHand);
		LocalPlayer.Animator.SetLayerWeightReflected(0, 1f);
		LocalPlayer.Animator.SetLayerWeightReflected(1, 1f);
		LocalPlayer.Animator.SetLayerWeightReflected(2, 1f);
		LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
		LocalPlayer.Animator.SetLayerWeightReflected(4, 0f);
		LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("noControl").Value = true;
		LocalPlayer.MainRotator.rotationRange = new Vector2(0f, 0f);
		LocalPlayer.MainRotator.enabled = false;
		LocalPlayer.CamRotator.stopInput = true;
		LocalPlayer.CamRotator.rotationRange = new Vector2(0f, 0f);
		LocalPlayer.CamRotator.enabled = false;
		LocalPlayer.Stats.CancelInvoke("CheckBlood");
		LocalPlayer.FpCharacter.drinking = true;
		LocalPlayer.FpCharacter.enabled = false;
		LocalPlayer.CamFollowHead.smoothLock = true;
		LocalPlayer.CamFollowHead.lockYCam = true;
		LocalPlayer.AnimControl.playerHeadCollider.enabled = false;
		LocalPlayer.AnimControl.lockGravity = true;
		LocalPlayer.Rigidbody.isKinematic = true;
		LocalPlayer.HitReactions.disableControllerFreeze();
		LocalPlayer.Animator.SetBoolReflected("injuredBool", false);
	}

	
	public void unlockPlayerParams()
	{
		if (LocalPlayer.ScriptSetup.leftHandHeld)
		{
			LocalPlayer.ScriptSetup.leftHandHeld.gameObject.SetActive(true);
		}
		LocalPlayer.ScriptSetup.bodyCollisionGo.SetActive(true);
		LocalPlayer.AnimControl.playerHeadCollider.enabled = true;
		LocalPlayer.AnimControl.playerCollider.enabled = true;
		LocalPlayer.AnimControl.endGameCutScene = false;
		LocalPlayer.AnimControl.skinningAnimal = false;
		LocalPlayer.AnimControl.useRootMotion = false;
		LocalPlayer.AnimControl.useRootRotation = false;
		LocalPlayer.Create.Grabber.gameObject.SetActive(true);
		LocalPlayer.AnimControl.playerHeadCollider.enabled = true;
		LocalPlayer.AnimControl.playerCollider.enabled = true;
		LocalPlayer.CamFollowHead.lockYCam = false;
		LocalPlayer.CamFollowHead.smoothLock = false;
		LocalPlayer.CamFollowHead.followAnim = false;
		LocalPlayer.CamRotator.resetOriginalRotation = true;
		LocalPlayer.CamRotator.stopInput = false;
		LocalPlayer.CamRotator.rotationRange = new Vector2(LocalPlayer.FpCharacter.minCamRotationRange, 0f);
		LocalPlayer.CamRotator.enabled = true;
		LocalPlayer.FpCharacter.enabled = true;
		LocalPlayer.FpCharacter.Locked = false;
		LocalPlayer.FpCharacter.drinking = false;
		LocalPlayer.FpCharacter.CanJump = true;
		LocalPlayer.AnimControl.animEvents.StartCoroutine("smoothEnableSpine");
		LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.LeftHand);
		LocalPlayer.Inventory.ShowAllEquiped(true);
		LocalPlayer.FpCharacter.allowFallDamage = true;
		LocalPlayer.AnimControl.endGameCutScene = false;
		LocalPlayer.AnimControl.playerHeadCollider.enabled = true;
		LocalPlayer.AnimControl.lockGravity = false;
		LocalPlayer.Rigidbody.isKinematic = false;
		LocalPlayer.Rigidbody.useGravity = true;
		Scene.HudGui.ShowHud(true);
		LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("noControl").Value = false;
		LocalPlayer.ScriptSetup.forceLocalPos.enabled = true;
		LocalPlayer.MainRotator.rotationRange = new Vector2(0f, 999f);
		LocalPlayer.MainRotator.resetOriginalRotation = true;
		LocalPlayer.MainRotator.enabled = true;
		LocalPlayer.Inventory.enabled = true;
		LocalPlayer.ScriptSetup.pmControl.SendEvent("toResetPlayer");
		LocalPlayer.Create.Grabber.gameObject.SetActive(true);
	}

	
	private void enableShellRideParams()
	{
		base.StartCoroutine(this.smoothDisableLayerNew(1));
		base.StartCoroutine(this.smoothDisableLayerNew(2));
		base.StartCoroutine(this.smoothDisableLayerNew(4));
		this.playerCollider.radius = 0.82f;
		this.shellRideStopTimer = Time.time + 0.5f;
		LocalPlayer.MainRotator.enabled = false;
		LocalPlayer.CamRotator.rotationRange = new Vector2(70f, 90f);
		LocalPlayer.ScriptSetup.pmControl.SendEvent("cancelBird");
		LocalPlayer.AnimControl.disableBirdOnHand();
		LocalPlayer.Sfx.SetTurtleSledLoop(true);
		base.StopCoroutine("scaleCapsuleForShell");
		base.StartCoroutine(this.scaleCapsuleForShell(true));
	}

	
	private void disableShellRideParams()
	{
		base.StartCoroutine(this.smoothEnableLayerNew(1));
		base.StartCoroutine(this.smoothEnableLayerNew(4));
		LocalPlayer.MainRotator.resetOriginalRotation = true;
		LocalPlayer.MainRotator.enabled = true;
		LocalPlayer.CamRotator.rotationRange = new Vector2(LocalPlayer.FpCharacter.minCamRotationRange, 0f);
		LocalPlayer.Sfx.SetTurtleSledLoop(false);
		LocalPlayer.Rigidbody.useGravity = true;
		this.playerCollider.radius = this.playerColliderRadius;
		base.StopCoroutine("scaleCapsuleForShell");
		base.StartCoroutine(this.scaleCapsuleForShell(false));
	}

	
	private IEnumerator scaleCapsuleForShell(bool enable)
	{
		float val = 0f;
		if (!enable)
		{
			val = 1f;
		}
		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime / 0.18f;
			if (enable)
			{
				val = Mathf.SmoothStep(0f, 1f, t);
			}
			else
			{
				val = Mathf.SmoothStep(1f, 0f, t);
			}
			LocalPlayer.FpCharacter.ScaleCapsuleForCrouching(val / 1f);
			yield return null;
		}
		yield break;
	}

	
	private void HandleShellRideControlUpdate()
	{
		this.hVel = Mathf.SmoothDamp(this.hVel, TheForest.Utils.Input.GetAxis("Horizontal") * 75f, ref this.curVel, 0.1f);
		this.shellBlendVal = Mathf.SmoothStep(this.hVel, this.shellBlendVal, Time.deltaTime / 9f);
		this.animator.SetFloatReflected("shellBlend", this.shellBlendVal);
		if (Mathf.Abs(this.hVel) > 0.0001f)
		{
			this.rootTr.RotateAround(base.transform.position, Vector3.up, this.hVel * Time.deltaTime);
		}
		else
		{
			this.curVel = 0f;
		}
		Vector3 vector = this.tr.forward * -1f;
	}

	
	private void HandleShellRideControlFixedUpdate()
	{
		this.doShellRideMode = true;
		Vector3 origin = new Vector3(this.tr.position.x, this.tr.position.y + 0.5f, this.tr.position.z);
		float num = 0f;
		Vector3 vector = Vector3.up;
		RaycastHit raycastHit;
		if (Physics.Raycast(origin, Vector3.down, out raycastHit, 2f, this.layerMask, QueryTriggerInteraction.Ignore))
		{
			vector = raycastHit.normal;
			num = Vector3.Angle(raycastHit.normal, LocalPlayer.Transform.forward);
			num = (num - 90f) / 30f;
			num = Mathf.Clamp(num, -1f, 1f);
		}
		if (this.buoyancy.InWater)
		{
			vector = Vector3.up;
		}
		this.tr.rotation = Quaternion.Lerp(this.tr.rotation, Quaternion.LookRotation(Vector3.Cross(this.tr.right, vector), vector), Time.deltaTime * 5f);
		LocalPlayer.FpCharacter.capsule.height = LocalPlayer.FpCharacter.crouchHeight;
		LocalPlayer.FpCharacter.capsule.center = new Vector3(0f, LocalPlayer.FpCharacter.crouchCapsuleCenter, 0f);
		if (this.setup.treeHit.heightFromTerrain < 0.6f && this.setup.treeHit.heightFromTerrain > -1f)
		{
			Vector3 normalized = this.rb.velocity.normalized;
			normalized.y = this.tr.forward.y;
			float num2 = Vector3.AngleBetween(normalized, this.tr.forward);
			Vector3 a = Vector3.Cross(this.tr.right, vector);
			Vector3 force = a * (15f * (1f + num2 * 16f)) + (a + a * num * -20f) + -normalized * (num2 * 90f);
			if (this.buoyancy.InWater)
			{
				force = Vector3.zero;
				Vector3 velocity = LocalPlayer.Rigidbody.velocity;
				velocity.y = 0f;
				LocalPlayer.Rigidbody.velocity = velocity;
			}
			else
			{
				LocalPlayer.Rigidbody.AddForce(force, ForceMode.Force);
			}
		}
		if (this.rb.velocity.magnitude < 0.8f)
		{
			if (Time.time > this.shellRideStopTimer)
			{
				this.animator.SetBool("shellRideBool", false);
			}
		}
		else
		{
			this.shellRideStopTimer = Time.time + 0.6f;
		}
		if (this.buoyancy.InWater)
		{
			LocalPlayer.Rigidbody.useGravity = false;
		}
		else
		{
			LocalPlayer.Rigidbody.useGravity = true;
		}
	}

	
	private void aimModeValid()
	{
		if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Inventory)
		{
			this.setup.pmControl.SendEvent("toResetSpear");
		}
	}

	
	public bool PlayerIsAttacking()
	{
		return this.currLayerState1.tagHash == this.attackingHash || this.fullBodyState2.tagHash == this.axeCombo1Hash || this.fullBodyState2.shortNameHash == this.axeAttackGround10Hash || this.fullBodyState2.shortNameHash == this.axeToAxeAttackHash || this.fullBodyState2.shortNameHash == this.flyingAxeAttachHash;
	}

	
	public bool USE_NEW_BOOK;

	
	public float playerColliderRadius = 0.66f;

	
	public float armMultiplyer = 10f;

	
	public bool oculusDemo;

	
	private Animator animator;

	
	public Rigidbody controller;

	
	private playerHitReactions reactions;

	
	public playerScriptSetup setup;

	
	private weaponInfo mainWeaponInfo;

	
	private Buoyancy buoyancy;

	
	private PlayerInventory player;

	
	public animEventsManager animEvents;

	
	public HingeJoint sledHinge;

	
	public Transform sledPivot;

	
	public CapsuleCollider playerCollider;

	
	public SphereCollider playerHeadCollider;

	
	public CapsuleCollider enemyCollider;

	
	public ForceLocalPosZero forcePos;

	
	public wallTriggerSetup wallSetup;

	
	public GameObject cliffCollide;

	
	public GameObject torchVisGo;

	
	public RaftPush currRaft;

	
	private Rigidbody rb;

	
	public GameObject bookHeldGo;

	
	public GameObject oarHeld;

	
	public GameObject planeCrashGo;

	
	public Transform planeCrash;

	
	public List<Transform> starLocations = new List<Transform>();

	
	public AnimatorStateInfo currLayerState0;

	
	public AnimatorStateInfo currLayerState1;

	
	public AnimatorStateInfo nextLayerState1;

	
	public AnimatorStateInfo nextLayerState0;

	
	public AnimatorStateInfo fullBodyState2;

	
	public AnimatorStateInfo fullBodyState3;

	
	public AnimatorStateInfo nextFullBodyState2;

	
	public AnimatorStateInfo nextFullBodyState3;

	
	private AnimatorStateInfo currLayterState7;

	
	private AnimatorStateInfo locoState;

	
	private int timmyOnMachineHash = Animator.StringToHash("putTimmyOnMachine");

	
	public float maxSpeed;

	
	public float torsoFollowSpeed;

	
	public float walkBlendSpeed;

	
	public float planeDist;

	
	private float colliderMove;

	
	public float storePrevYRotSpeed;

	
	public float offsetFlintlockArmsMult;

	
	public float offsetFlintlockHandsMult;

	
	private float horizontalSpeed;

	
	private float verticalSpeed;

	
	public float overallSpeed;

	
	private float initialAccel;

	
	public float tempTired;

	
	private Vector3 tempVelocity;

	
	private Quaternion storeLeftArmAngle;

	
	private float hVel;

	
	public bool tiredCheck;

	
	public int combo;

	
	private bool comboBlock;

	
	private bool waterBlock;

	
	private bool pointBlock;

	
	public bool onFire;

	
	public bool lockGravity;

	
	public bool swimLayerChange;

	
	public bool swimming;

	
	public bool leftArmActive;

	
	public bool upsideDown;

	
	private float startPlaneBlend;

	
	public bool blockCamX;

	
	private bool deathPositionCheck;

	
	public bool endGameCutScene;

	
	private Vector3 deathPos;

	
	private float torchCrouchOffset = -1f;

	
	public bool fullyAttachedToSled;

	
	private float waterBlendFloat;

	
	public bool knockedDown;

	
	public bool smashBool;

	
	public bool useRootMotion;

	
	public bool useRootRotation;

	
	public bool doSledPushMode;

	
	public bool doShellRideMode;

	
	public bool onRope;

	
	public bool onRopeHeightCheck;

	
	public bool carry;

	
	public bool injured;

	
	public bool cliffClimb;

	
	public bool allowCliffReset;

	
	public bool skinningAnimal;

	
	public bool holdingGirl;

	
	public bool sitting;

	
	public bool loadingAnimation;

	
	public bool standingOnRaft;

	
	public bool blockInventoryOpen;

	
	public bool doneOutOfWorldRoutine;

	
	public bool exitingACave;

	
	public bool enteringACave;

	
	public GameObject placedTimmyGo;

	
	public GameObject heldTimmyGo;

	
	public GameObject placedBodyGo;

	
	public GameObject heldBodyGo;

	
	public GameObject heldTimmyPhotoGo;

	
	public GameObject inventoryNapkin;

	
	private Quaternion armAngle;

	
	private Vector3 prevNormal;

	
	private float prevMag;

	
	private float currMag;

	
	public Vector3 cliffEnterNormal;

	
	public Vector3 cliffEnterPos;

	
	private Vector3 normal1;

	
	private Vector3 normal2;

	
	private Vector3 normal3;

	
	private Vector3 normal4;

	
	private Vector3 normal5;

	
	private RaycastHit nHit;

	
	[ItemIdPicker]
	public int _rebreatherId;

	
	[ItemIdPicker]
	public int _torchId;

	
	[ItemIdPicker]
	public int _lighterId;

	
	[ItemIdPicker]
	public int _planeAxeId;

	
	[ItemIdPicker]
	public int _polaroid1Id;

	
	[ItemIdPicker]
	public int _polaroid2Id;

	
	[ItemIdPicker]
	public int _polaroid3Id;

	
	[ItemIdPicker]
	public int _flintLockId;

	
	[ItemIdPicker]
	public int _flaregunId;

	
	[ItemIdPicker]
	public int _flareId;

	
	[ItemIdPicker]
	public int _bowId;

	
	[ItemIdPicker]
	public int _bowRecurveId;

	
	[ItemIdPicker]
	public int _repairToolId;

	
	[ItemIdPicker]
	public int _walkmanId;

	
	[ItemIdPicker]
	public int _radioId;

	
	[ItemIdPicker]
	public int _climbingAxeId;

	
	[ItemIdPicker]
	public int _shellId;

	
	[ItemIdPicker]
	public int _slingShotId;

	
	[ItemIdPicker]
	public int _timmyPhotoId;

	
	[ItemIdPicker]
	public int _mapId;

	
	[ItemIdPicker]
	public int _pedometerId;

	
	[ItemIdPicker]
	public int _artifactHeldId;

	
	private int stickAttackHash;

	
	private int axeAttackHash;

	
	private int axeHeavyAttackHash;

	
	public int idleHash;

	
	public int hangingHash;

	
	private int checkArmsHash;

	
	private int heldHash;

	
	private int attackHash;

	
	private int smashHash;

	
	private int blockHash;

	
	public int deathHash;

	
	private int swimHash;

	
	private int climbingHash;

	
	private int climbIdleHash;

	
	private int enterClimbHash;

	
	private int enterClimbTopHash = Animator.StringToHash("idleToClimbRopeTop");

	
	private int climbOutHash = Animator.StringToHash("climbOut");

	
	private int explodeHash;

	
	public int knockBackHash;

	
	public int reloadFlintlockHash = Animator.StringToHash("reloadFlintLock");

	
	public int reloadFlareGunlockHash = Animator.StringToHash("flaregunReload");

	
	private int flintLockIdleHash = Animator.StringToHash("flintlockIdle");

	
	private int flintLockToAimHash = Animator.StringToHash("toAimIdle");

	
	private int flintLockAimIdleHash = Animator.StringToHash("aimIdle");

	
	private int flareGunIdleHash = Animator.StringToHash("flareGunIdle");

	
	private int flareGunToAimHash = Animator.StringToHash("toFlareAimIdle");

	
	private int flareGunAimIdleHash = Animator.StringToHash("aimFlareIdle");

	
	private int bowIdleHash = Animator.StringToHash("bowIdle");

	
	private int drawBowIdleHash = Animator.StringToHash("drawBowIdle");

	
	private int drawBowHash = Animator.StringToHash("drawBow");

	
	private int repairHammerAttackHash = Animator.StringToHash("repairHammerAttack");

	
	private int injuredLoopHash = Animator.StringToHash("injuredLoop");

	
	public int idleToBookHash = Animator.StringToHash("idleToBookIdle");

	
	public int bookIdleHash = Animator.StringToHash("bookIdle");

	
	public int bookIdleToIdleHash = Animator.StringToHash("bookIdleToIdle");

	
	private int toChainSawIdleHash = Animator.StringToHash("toChainsawIdle");

	
	public int chainSawAttackHash = Animator.StringToHash("chainSawAttack");

	
	private int flashLightIdlehash = Animator.StringToHash("flashlightIdle");

	
	public int flareLightHash = Animator.StringToHash("flareLight");

	
	private int axeGround1Hash = Animator.StringToHash("axeAttackGround1");

	
	private int axeToAxeGroundHash = Animator.StringToHash("axeToAxeAttack");

	
	private int axeGround2Hash = Animator.StringToHash("axeAttackGround1 0");

	
	private int shellRideHash = Animator.StringToHash("shellRide");

	
	private int fillPotHash = Animator.StringToHash("fillPot");

	
	public int spearThrowHash = Animator.StringToHash("throwSpear");

	
	public int landHeavyHash = Animator.StringToHash("landHeavy");

	
	private int eatMeatHash = Animator.StringToHash("eatMeat");

	
	public int attackingHash = Animator.StringToHash("attacking");

	
	public int axeCombo1Hash = Animator.StringToHash("axeCombo1");

	
	public int flyingAxeAttachHash = Animator.StringToHash("flyingAxeAttack");

	
	public int axeAttackGround10Hash = Animator.StringToHash("axeAttackGround1 0");

	
	public int axeToAxeAttackHash = Animator.StringToHash("axeToAxeAttack");

	
	public int drinkAtPondHash = Animator.StringToHash("drinkAtPond");

	
	public int fillWaterSkinHash = Animator.StringToHash("fillWaterSkin");

	
	private FsmFloat fsmPlayerAngle;

	
	private FsmFloat fsmTiredFloat;

	
	private FsmBool fsmButtonHeldBool;

	
	private FsmBool fsmCanDoAxeSmash;

	
	private Transform tr;

	
	private Transform rootTr;

	
	private float lastYPos;

	
	private float camX;

	
	public float camY;

	
	public float headCamY;

	
	private Vector3 camForward;

	
	public float normCamX;

	
	public float absCamX;

	
	private float normCamY;

	
	private float smoothCamX;

	
	private float smoothCamY;

	
	private float camYOffset;

	
	private float prevCamY;

	
	private bool doFootReset;

	
	private bool shouldUnlockLeftHandSlot;

	
	public bool onRaft;

	
	public bool introCutScene;

	
	public bool onRockThrower;

	
	public bool doingGroundChop;

	
	private bool doingJumpCrouch;

	
	private Vector3 getPlayerPos;

	
	private float coldOffsetTimer;

	
	public bool coldOffsetBool;

	
	private float mouseCurrentPosx;

	
	private float mouseDeltax;

	
	private int layerMask;

	
	private Vector3 pos;

	
	private float curVel;

	
	private float shellBlendVal;

	
	private float lookDownBlendVal;

	
	private float smoothShellBlend;

	
	public float clampArmsVal = 1f;

	
	public float wristAimOffset;

	
	public bool fovAimMode;

	
	private float fovOverride;

	
	private float storeActualFovValue;

	
	private float frozenUpdateTimer;

	
	public float leftArmDamp;

	
	private float headColliderZpos;

	
	private float maxHeadColliderOffset = 0.45f;

	
	private int fixedCount;

	
	private float yVel;

	
	private float closePlayerYVel;

	
	private Vector3 tempPrevPos;

	
	private int ropeFixDelay;

	
	private float closePlayerHeight;

	
	private int closePlayerCheck;

	
	private Transform closePlayer;

	
	public bool onRopeWithGroundBelow;

	
	public bool blockHeightCheck;

	
	private RaycastHit ropeHit;

	
	private GameObject bird;

	
	private float shellRideStopTimer;

	
	private float shellParticleTimer;
}
