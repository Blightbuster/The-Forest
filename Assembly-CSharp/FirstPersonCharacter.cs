using System;
using System.Collections;
using HutongGames.PlayMaker;
using TheForest.Items.Inventory;
using TheForest.UI.Multiplayer;
using TheForest.Utils;
using TheForest.Utils.Physics;
using TheForest.Utils.Settings;
using TheForest.World;
using UnityEngine;

public class FirstPersonCharacter : MonoBehaviour, IOnCollisionEnterProxy, IOnCollisionStayProxy
{
	public bool Grounded { get; private set; }

	public float PrevVelocity
	{
		get
		{
			return this.prevVelocity;
		}
	}

	public bool swimming
	{
		get
		{
			return this.buoyancy && this.buoyancy.InWater && !LocalPlayer.AnimControl.onRopeWithGroundBelow;
		}
	}

	public bool Sitting { get; set; }

	public float recoveringFromRun { get; set; }

	private void Awake()
	{
		this.buoyancy = base.GetComponent<Buoyancy>();
		this.rb = base.GetComponent<Rigidbody>();
		this.playerPhysicMaterial = base.GetComponent<CapsuleCollider>().material;
		this.playerPhysicMaterial2 = base.GetComponent<SphereCollider>().material;
		this.rb.freezeRotation = true;
		this.rb.useGravity = false;
		this.collFlags = base.transform.GetComponent<RigidBodyCollisionFlags>();
		this.setup = base.GetComponentInChildren<playerScriptSetup>();
		this.targets = base.GetComponentInChildren<playerTargetFunctions>();
		this.UnLockView();
		this.capsule = (base.GetComponent<Collider>() as CapsuleCollider);
		this.defaultMass = this.rb.mass;
		this.originalHeight = this.capsule.height;
		this.originalYPos = this.capsule.center.y;
		this.crouchCapsuleCenter = (this.crouchHeight - this.originalHeight) / 2f;
		this.Grounded = true;
		this.fsmCrouchBool = FsmVariables.GlobalVariables.GetFsmBool("playerCrouchBool");
	}

	private void OnDeserialized()
	{
		if (!this.collFlags)
		{
			this.collFlags = base.gameObject.AddComponent<RigidBodyCollisionFlags>();
			this.collFlags.coll = this.capsule;
			this.collFlags.collType = 1;
			this.collFlags.cColl = base.transform.GetComponent<CapsuleCollider>();
		}
	}

	private void Start()
	{
		if (!base.GetComponent<OnCollisionEnterProxy>())
		{
			base.gameObject.AddComponent<OnCollisionEnterProxy>();
		}
		if (!base.GetComponent<OnCollisionExitProxy>())
		{
			base.gameObject.AddComponent<OnCollisionExitProxy>();
		}
		if (!base.GetComponent<OnCollisionStayProxy>())
		{
			base.gameObject.AddComponent<OnCollisionStayProxy>();
		}
		this.getSlopeLimit = this.extremeAngleGroundedLimit;
		this.storeMaxVelocity = this.maximumVelocity;
		this.headCollider = base.transform.GetComponent<SphereCollider>();
		this.fsmClimbBool = LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("climbBool");
		this.fsmGrounded = LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("grounded");
		this.Stats = base.gameObject.GetComponent<PlayerStats>();
		this.animator = this.setup.playerBase.GetComponent<Animator>();
		this.vrAdapter = base.transform.GetComponentInChildren<VRPlayerAdapter>();
		this.fsmCrouchBlockedBool = this.setup.pmControl.FsmVariables.GetFsmBool("crouchBlockedBool");
	}

	private void OnDisable()
	{
		this.playerPhysicMaterial.staticFriction = 1f;
		this.playerPhysicMaterial.dynamicFriction = 1f;
		this.playerPhysicMaterial.frictionCombine = PhysicMaterialCombine.Average;
		this.playerPhysicMaterial.bounceCombine = PhysicMaterialCombine.Minimum;
		this.playerPhysicMaterial.bounciness = 0f;
	}

	private void Update()
	{
		if (Scene.HudGui == null)
		{
			return;
		}
		if (this.crouchHeightBlocked)
		{
			this.fsmCrouchBlockedBool.Value = true;
		}
		else
		{
			this.fsmCrouchBlockedBool.Value = false;
		}
		if (this.collFlags.groundAngleVal > this.extremeAngleGroundedLimit && this.onSlipperySlope)
		{
			this.clampInputVal = 0.04f;
		}
		else if (this.collFlags.groundAngleVal > this.extremeAngleGroundedLimit && !this.doingClampInput && !this.jumping && !this.Grounded && !LocalPlayer.AnimControl.swimming)
		{
			this.clampInputVal = 0f;
		}
		else if (!this.doingClampInput && !Scene.HudGui.MpPlayerListCamGo.activeInHierarchy && !Scene.HudGui.PauseMenu.activeInHierarchy && !ChatBox.IsChatOpen && LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Inventory)
		{
			this.clampInputVal = 1f;
		}
		if ((Scene.HudGui.MpPlayerListCamGo.activeInHierarchy || Scene.HudGui.PauseMenu.activeInHierarchy || ChatBox.IsChatOpen || LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Inventory) && !LocalPlayer.AnimControl.flyingGlider)
		{
			this.clampInputVal = 0f;
			this.rb.velocity = new Vector3(0f, this.rb.velocity.y, 0f);
		}
		if (this.Grounded)
		{
			this.validateGrounded = true;
		}
		else
		{
			this.validateGrounded = false;
		}
		this.fsmGrounded.Value = this.validateGrounded;
		if (this.crouching)
		{
			float num = this.originalHeight / 2f;
			Vector3 position = base.transform.position;
			position.y -= 0.75f;
			if (Physics.SphereCast(position, 0.75f, Vector3.up * (num - this.capsule.height / 2f), out this.hitInfo, num - this.capsule.height / 2f + 1.25f, this.CollisionLayers, QueryTriggerInteraction.Ignore))
			{
				this.crouchHeightBlocked = true;
			}
			else
			{
				this.crouchHeightBlocked = false;
			}
		}
		else
		{
			this.crouchHeightBlocked = false;
		}
		this.contactWithEnemy = false;
		bool flag = true;
		if (this.onSlipperySlope && this.collFlags.groundAngleVal > this.extremeAngleGroundedLimit)
		{
			flag = false;
		}
		if (LocalPlayer.AnimControl.swimming && !this.Diving && TheForest.Utils.Input.GetButtonDown("Jump"))
		{
			if (this.collFlags.collisionFlags == CollisionFlags.Sides || this.allowWaterJump)
			{
				LocalPlayer.Sfx.PlayJumpSound();
				this.rb.velocity = new Vector3(this.velocity.x, this.CalculateJumpVerticalSpeed() * 1.5f, this.velocity.z);
			}
			else if (!this.blockWaterJump)
			{
				LocalPlayer.Sfx.PlayJumpSound();
				this.rb.velocity = new Vector3(this.velocity.x, this.CalculateWaterJumpVerticalSpeed(), this.velocity.z);
				this.blockWaterJump = true;
				base.Invoke("resetBlockWaterJump", 1f);
			}
		}
		this.HandleFrictionParams();
		if (this.allowJump && !TheForest.Utils.Input.GetButton("Jump"))
		{
			if (!this.jumping)
			{
				LocalPlayer.Animator.SetBoolReflected("jumpBool", false);
			}
		}
		else if (this.allowJump && this.CanJump && (this.Grounded || (Time.time < this.fauxGroundedTimer && !this.blockFauxJump)) && FirstPersonCharacter.GetJumpInput() && !LocalPlayer.AnimControl.useRootMotion && !this.crouchHeightBlocked && !LocalPlayer.AnimControl.doSledPushMode && !LocalPlayer.AnimControl.onRope && !this.fsmClimbBool.Value && !this.Diving && !this.Locked && !this.jumpInputBlock && LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Inventory)
		{
			if (Time.time < this.fauxGroundedTimer)
			{
				this.blockFauxJump = true;
				base.Invoke("resetFauxJump", 0.5f);
			}
			this.jumpInputBlock = true;
			base.StartCoroutine("startJumpTimer");
			base.StartCoroutine("clampInput");
			base.Invoke("resetJumpInputBlock", 0.2f);
			LocalPlayer.Sfx.PlayJumpSound();
			LocalPlayer.Animator.SetBoolReflected("jumpBool", true);
			if (flag)
			{
				this.rb.velocity = new Vector3(this.velocity.x, this.CalculateJumpVerticalSpeed(), this.velocity.z);
			}
			base.Invoke("allowJumpingAttack", 0.15f);
			LocalPlayer.ScriptSetup.pmControl.SendEvent("goToJump");
			this.jumping = true;
			this.allowJump = false;
		}
		bool value = this.setup.pmControl.FsmVariables.GetFsmBool("heavyAttackBool").Value;
		if (!this.crouch && !value)
		{
			if (ForestVR.Enabled && PlayerPreferences.VRUsePhysicalCrouching)
			{
				this.crouch = LocalPlayer.vrPlayerControl.toggleCrouchFromVrHeight;
			}
			else
			{
				this.crouch = FirstPersonCharacter.GetCrouchInput();
			}
		}
		else if (LocalPlayer.AnimControl.enteringACave)
		{
			this.standUp = true;
		}
		else if (ForestVR.Enabled && PlayerPreferences.VRUsePhysicalCrouching)
		{
			this.standUp = !LocalPlayer.vrPlayerControl.toggleCrouchFromVrHeight;
		}
		else
		{
			this.standUp = FirstPersonCharacter.GetStangUpInput();
		}
		if (PlayerPreferences.UseSprintToggle)
		{
			if (TheForest.Utils.Input.GetButtonDown("Run") && !this.forceStopRun)
			{
				this.run = !this.run;
				if (this.run)
				{
					this.CantRun = false;
				}
			}
			else if (!TheForest.Utils.Input.GetButton("Run") && LocalPlayer.AnimControl.overallSpeed < 0.4f)
			{
				this.run = false;
			}
		}
		else
		{
			this.run = TheForest.Utils.Input.GetButton("Run");
			if (TheForest.Utils.Input.GetButtonDown("Run"))
			{
				this.CantRun = false;
			}
		}
		if (this.run && LocalPlayer.AnimControl.overallSpeed > 0.4f)
		{
			this.recoveringFromRun = this.timeToRecoverFromRun;
			this.running = true;
		}
		else
		{
			this.running = false;
		}
		if (this.crouch)
		{
			if (!this.crouching && !LocalPlayer.AnimControl.swimming && this.validateGrounded && !LocalPlayer.AnimControl.doShellRideMode && !LocalPlayer.AnimControl.doSledPushMode && !LocalPlayer.AnimControl.enteringACave)
			{
				this.crouching = true;
				this.standingUp = false;
				base.StartCoroutine("EnableCrouch");
			}
			if (this.standUp || value || LocalPlayer.AnimControl.swimming || this.animator.GetBool("zipLineAttach") || this.animator.GetBool("craneAttach") || LocalPlayer.AnimControl.doShellRideMode || LocalPlayer.AnimControl.doSledPushMode || LocalPlayer.AnimControl.enteringACave || LocalPlayer.AnimControl.endGameCutScene)
			{
				base.StartCoroutine("DisableCrouch");
				this.standingUp = true;
				this.crouch = false;
				this.standUp = false;
			}
		}
	}

	private static bool GetStangUpInput()
	{
		if (ForestVR.Enabled)
		{
			bool flag = FirstPersonCharacter.GetDropInput() || FirstPersonCharacter.GetJumpInput();
			return (!PlayerPreferences.UseCrouchToggle) ? (flag || !TheForest.Utils.Input.GetButton("Crouch")) : (!flag && TheForest.Utils.Input.GetButtonDown("Crouch"));
		}
		return (!PlayerPreferences.UseCrouchToggle) ? (!TheForest.Utils.Input.GetButton("Crouch")) : TheForest.Utils.Input.GetButtonDown("Crouch");
	}

	private static bool GetCrouchInput()
	{
		if (ForestVR.Enabled)
		{
			bool flag = FirstPersonCharacter.GetDropInput() || FirstPersonCharacter.GetJumpInput();
			return (!PlayerPreferences.UseCrouchToggle) ? (!flag && TheForest.Utils.Input.GetButton("Crouch")) : (!flag && TheForest.Utils.Input.GetButtonDown("Crouch"));
		}
		return (!PlayerPreferences.UseCrouchToggle) ? TheForest.Utils.Input.GetButton("Crouch") : TheForest.Utils.Input.GetButtonDown("Crouch");
	}

	public static bool GetDropInput()
	{
		if (!ForestVR.Enabled)
		{
			return TheForest.Utils.Input.GetButtonDown("Drop");
		}
		if (VRControllerDisplayManager.GetActiveControllerType() == VRControllerDisplayManager.VRControllerType.Vive)
		{
			return TheForest.Utils.Input.GetAxis("Drop") > 0.85f && TheForest.Utils.Input.GetButtonDown("Crouch");
		}
		return TheForest.Utils.Input.GetAxis("Drop") > 0.85f;
	}

	public static bool GetJumpInput()
	{
		if (!ForestVR.Enabled)
		{
			return TheForest.Utils.Input.GetButtonDown("Jump");
		}
		if (VRControllerDisplayManager.GetActiveControllerType() == VRControllerDisplayManager.VRControllerType.Vive)
		{
			return TheForest.Utils.Input.GetAxis("Jump") > 0.85f && TheForest.Utils.Input.GetButtonDown("Crouch");
		}
		return TheForest.Utils.Input.GetAxis("Jump") > 0.85f;
	}

	public void disableToggledCrouch()
	{
		this.standingUp = true;
		this.crouch = false;
		this.standUp = false;
		base.StartCoroutine("DisableCrouch");
	}

	private void FixedUpdate()
	{
		if (!this.swimming)
		{
			this.allowWaterJump = false;
		}
		if (this.multisledContact || this.enforceHighDrag)
		{
			this.rb.drag = 10f;
			this.rb.angularDrag = 10f;
		}
		else
		{
			this.rb.drag = 0f;
			this.rb.angularDrag = 0.05f;
		}
		this.multisledContact = false;
		this.checkGrounded = this.Grounded;
		this.prevYPos = this.rb.position.y;
		this.checkSwimming = this.swimming;
		this.setup.targetInfo.onRaft = this.standingOnRaft;
		this.CheckStandingOnExtremeSlope();
		this.ExitOnVSyncNotReady();
		this.HandleHeightAdjustments();
		if (this.SailingRaft)
		{
			return;
		}
		if (!this.MovementLocked && (!this.Locked || (this.Grounded && !this.prevGrounded)))
		{
			if (this.Grounded)
			{
				if (!this.prevGrounded)
				{
					this.HandleLanded();
				}
				this.allowFallDamage = false;
				this.prevGrounded = true;
				this.jumping = false;
			}
			else
			{
				if (this.prevGrounded)
				{
					this.HandleStartJumping();
				}
				this.prevGrounded = false;
			}
			if (LocalPlayer.AnimControl.swimming)
			{
				this.animator.SetBoolReflected("jumpBool", false);
			}
			Vector3 vector = this.DetermineVelocityChange();
			if (this.Locked)
			{
				vector.x = 0f;
				vector.z = 0f;
			}
			if (this.Grounded || this.swimming)
			{
				if (this.swimming && !this.Grounded)
				{
					this.HandleSwimmingSpeed(vector);
				}
				else
				{
					if (this._doingExitVelocity)
					{
						this.rb.AddForce(vector, ForceMode.Acceleration);
					}
					else if (!LocalPlayer.AnimControl.doShellRideMode && !LocalPlayer.AnimControl.flyingGlider)
					{
						this.rb.AddForce(vector, ForceMode.VelocityChange);
					}
					if (this._doingExitVelocity)
					{
						Vector3 forceDir = LocalPlayer.SpecialActions.GetComponent<playerZipLineAction>()._forceDir;
						this.rb.AddForce(forceDir * 10f, ForceMode.Acceleration);
					}
				}
				this.ClampVelocity();
				this.ApplyGroundingForce();
			}
			if (this.hitByEnemy)
			{
				this.ClampVelocity();
			}
			if (!this.swimming && !this.Diving && this.jumping)
			{
				this.HandleJumpSpeed(vector);
			}
			else
			{
				this.clampAirTouch = 1f;
			}
			if (ForestVR.Enabled && PlayerPreferences.VRAutoRun)
			{
				Vector2 vector2 = new Vector2(TheForest.Utils.Input.GetAxis("Horizontal"), TheForest.Utils.Input.GetAxis("Vertical"));
				if (vector2.magnitude > 0.85f)
				{
					this.run = true;
				}
			}
			bool flag = this.run && !this.CantRun && this.Stats.Stamina > 0f && LocalPlayer.AnimControl.overallSpeed > 0.4f;
			if (flag)
			{
				this.HandleRunningStaminaAndSpeed();
			}
			else
			{
				this.HandleWalkingSpeedOptions();
			}
		}
		if (this.Diving)
		{
			this.ApplySwimmingForce();
		}
		else
		{
			this.rb.AddForce(new Vector3(0f, -this.gravity * this.rb.mass, 0f));
			this.ResetVariables();
		}
	}

	private void CheckStandingOnExtremeSlope()
	{
		if (this.onSlipperySlope)
		{
			if (this.modExtremeAngleLimit > 0f)
			{
				this.extremeAngleGroundedLimit = this.modExtremeAngleLimit;
			}
			else
			{
				this.extremeAngleGroundedLimit = this.getSlopeLimit / 1.5f;
			}
		}
		else
		{
			this.extremeAngleGroundedLimit = this.getSlopeLimit;
		}
	}

	private void ExitOnVSyncNotReady()
	{
		if (PlayerPreferences.VSync)
		{
			if (Time.realtimeSinceStartup - this.lastUpdateTime < Time.fixedDeltaTime)
			{
				return;
			}
			this.lastUpdateTime = Time.realtimeSinceStartup;
		}
	}

	private void HandleFrictionParams()
	{
		if (LocalPlayer.AnimControl.doShellRideMode)
		{
			if (LocalPlayer.Stats.IsInNorthColdArea())
			{
				this.playerPhysicMaterial.staticFriction = 0.15f;
				this.playerPhysicMaterial.dynamicFriction = 0.15f;
			}
			else
			{
				this.playerPhysicMaterial.staticFriction = 0.4f;
				this.playerPhysicMaterial.dynamicFriction = 0.4f;
			}
			if (this.jumpingTimer > 3f)
			{
				this.playerPhysicMaterial.bounceCombine = PhysicMaterialCombine.Minimum;
				this.playerPhysicMaterial.bounciness = 0f;
			}
			else
			{
				this.playerPhysicMaterial.bounceCombine = PhysicMaterialCombine.Average;
				this.playerPhysicMaterial.bounciness = 0.9f;
			}
			this.playerPhysicMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
		}
		else if (!this.Grounded)
		{
			this.playerPhysicMaterial.staticFriction = 0f;
			this.playerPhysicMaterial.dynamicFriction = 0f;
			this.playerPhysicMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
			this.playerPhysicMaterial.bounceCombine = PhysicMaterialCombine.Minimum;
			this.playerPhysicMaterial.bounciness = 0f;
		}
		else if (this.collFlags.groundAngleVal > this.extremeAngleGroundedLimit)
		{
			this.playerPhysicMaterial.staticFriction = 0f;
			this.playerPhysicMaterial.dynamicFriction = 0f;
			this.playerPhysicMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
			this.playerPhysicMaterial.bounceCombine = PhysicMaterialCombine.Minimum;
			this.playerPhysicMaterial.bounciness = 0f;
		}
		else if (this.inputVelocity.magnitude < 0.05f)
		{
			this.playerPhysicMaterial.staticFriction = 1f;
			this.playerPhysicMaterial.dynamicFriction = 1f;
			this.playerPhysicMaterial.frictionCombine = PhysicMaterialCombine.Average;
			this.playerPhysicMaterial.bounceCombine = PhysicMaterialCombine.Minimum;
			this.playerPhysicMaterial.bounciness = 0f;
		}
		else
		{
			this.playerPhysicMaterial.staticFriction = 0.2f;
			this.playerPhysicMaterial.dynamicFriction = 0.2f;
			this.playerPhysicMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
			this.playerPhysicMaterial.bounceCombine = PhysicMaterialCombine.Minimum;
			this.playerPhysicMaterial.bounciness = 0f;
		}
	}

	private void HandleHeightAdjustments()
	{
		if (Clock.planecrash || LocalPlayer.AnimControl.introCutScene)
		{
			this.capsule.center = new Vector3(this.capsule.center.x, this.originalYPos, this.capsule.center.z);
			return;
		}
		int terrainSurface = LocalPlayer.AnimControl.getTerrainSurface();
		bool flag = terrainSurface == 4 && LocalPlayer.Transform.position.y - Terrain.activeTerrain.SampleHeight(LocalPlayer.Transform.position) < 2.4f && !Clock.planecrash && !LocalPlayer.AnimControl.introCutScene && !LocalPlayer.AnimControl.doShellRideMode;
		if (this.Locked)
		{
			this.capsule.center = new Vector3(this.capsule.center.x, this.capsule.center.y, this.capsule.center.z);
		}
		else if (flag && !LocalPlayer.AnimControl.doShellRideMode)
		{
			this.inSand = true;
			if (!this.crouching)
			{
				this.yChange = this.capsule.center.y;
				this.capsule.center = new Vector3(this.capsule.center.x, Mathf.Lerp(this.yChange, this.originalYPos + 0.25f, Time.deltaTime), this.capsule.center.z);
			}
		}
		else if (LocalPlayer.Stats.IsInNorthColdArea() && !this.snowFlotation && LocalPlayer.Transform.position.y - Terrain.activeTerrain.SampleHeight(LocalPlayer.Transform.position) < 2.4f && !Clock.planecrash && !LocalPlayer.AnimControl.introCutScene)
		{
			this.inSnow = true;
			if (!this.crouching)
			{
				this.yChange = this.capsule.center.y;
				this.capsule.center = new Vector3(this.capsule.center.x, Mathf.Lerp(this.yChange, this.originalYPos + 0.25f, Time.deltaTime), this.capsule.center.z);
			}
		}
		else if (this.yChange != this.originalYPos)
		{
			this.inSand = false;
			this.inSnow = false;
			if (!this.crouching)
			{
				this.yChange = this.capsule.center.y;
				this.capsule.center = new Vector3(this.capsule.center.x, Mathf.Lerp(this.yChange, this.originalYPos, Time.deltaTime * 3f), this.capsule.center.z);
			}
		}
	}

	public void HandleLanded()
	{
		LocalPlayer.CamFollowHead.stopAllCameraShake();
		this.fallShakeBlock = false;
		base.StopCoroutine("startJumpTimer");
		this.jumpTimerStarted = false;
		float num = 28f;
		bool flag = false;
		if ((LocalPlayer.AnimControl.doShellRideMode || LocalPlayer.AnimControl.flyingGlider) && this.prevVelocityXZ.magnitude > 32f)
		{
			flag = true;
		}
		if (this.prevVelocity > num && !flag && this.allowFallDamage && this.jumpingTimer > 0.75f)
		{
			if (!this.jumpLand && !Clock.planecrash)
			{
				this.jumpCoolDown = true;
				this.jumpLand = true;
				float num2 = this.prevVelocity * 0.9f * (this.prevVelocity / 27.5f);
				int damage = (int)num2;
				float num3 = 3.8f;
				if (LocalPlayer.AnimControl.doShellRideMode)
				{
					num3 = 5f;
				}
				bool flag2 = false;
				if (this.jumpingTimer > num3 && !LocalPlayer.AnimControl.flyingGlider)
				{
					damage = 1000;
					flag2 = true;
				}
				if (LocalPlayer.AnimControl.doShellRideMode && !flag2)
				{
					damage = 17;
				}
				if (LocalPlayer.AnimControl.disconnectFromGlider)
				{
					damage = 12;
					LocalPlayer.SpecialActions.SendMessage("DropGlider", true);
					this.enforceHighDrag = true;
					base.Invoke("disableHighDrag", 0.65f);
				}
				this.Stats.Hit(damage, true, PlayerStats.DamageType.Physical);
				LocalPlayer.Animator.SetBoolReflected("jumpBool", false);
				if (this.Stats.Health > 0f)
				{
					if (!LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("doingJumpAttack").Value && !LocalPlayer.AnimControl.doShellRideMode)
					{
						LocalPlayer.Animator.SetIntegerReflected("jumpType", 1);
						LocalPlayer.Animator.SetTrigger("landHeavyTrigger");
						LocalPlayer.Animator.SetBoolReflected("jumpBool", false);
						this.CanJump = false;
						LocalPlayer.HitReactions.StartCoroutine("doHardfallRoutine");
						this.prevMouseXSpeed = LocalPlayer.MainRotator.rotationSpeed;
						LocalPlayer.MainRotator.rotationSpeed = 0.55f;
						LocalPlayer.Animator.SetLayerWeightReflected(4, 0f);
						LocalPlayer.Animator.SetLayerWeightReflected(0, 1f);
						LocalPlayer.Animator.SetLayerWeightReflected(1, 0f);
						LocalPlayer.Animator.SetLayerWeightReflected(2, 0f);
						LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
						base.Invoke("resetAnimSpine", 1f);
					}
					else
					{
						this.jumpLand = false;
						this.jumpCoolDown = false;
					}
				}
				else
				{
					this.jumpCoolDown = false;
					this.jumpLand = false;
				}
			}
			this.blockJumpAttack();
		}
		this.jumping = false;
		base.CancelInvoke("setAnimatorJump");
		if (!this.jumpCoolDown)
		{
			LocalPlayer.Animator.SetIntegerReflected("jumpType", 0);
			LocalPlayer.Animator.SetBoolReflected("jumpBool", false);
			LocalPlayer.ScriptSetup.pmControl.SendEvent("toWait");
			this.blockJumpAttack();
		}
		base.CancelInvoke("fallDamageTimer");
		this.allowFallDamage = false;
	}

	private void HandleStartJumping()
	{
		this.jumping = true;
		base.Invoke("disableAllowJump", 0.25f);
		base.Invoke("fallDamageTimer", 0.35f);
		base.StartCoroutine("startJumpTimer");
		if (!LocalPlayer.AnimControl.useRootMotion && !LocalPlayer.AnimControl.doSledPushMode && !LocalPlayer.AnimControl.onRope && !this.fsmClimbBool.Value && !this.Diving && !this.crouching)
		{
			base.Invoke("setAnimatorJump", 0.2f);
		}
	}

	private Vector3 DetermineVelocityChange()
	{
		Vector3 vector = new Vector3(TheForest.Utils.Input.GetAxis("Horizontal") + this.InputHack.x, 0f, TheForest.Utils.Input.GetAxis("Vertical") + this.InputHack.y);
		if (vector.magnitude < 0.1f)
		{
			vector = Vector3.zero;
		}
		vector = Vector3.ClampMagnitude(vector, 1.1f);
		if (this.Locked)
		{
			vector = Vector3.zero;
		}
		this.inputVelocity = vector;
		if (this.PushingSled || LocalPlayer.AnimControl.doShellRideMode || LocalPlayer.AnimControl.flyingGlider)
		{
			vector.x = 0f;
		}
		if (LocalPlayer.AnimControl.doShellRideMode || LocalPlayer.AnimControl.flyingGlider)
		{
			vector.z = 0f;
		}
		vector = base.transform.TransformDirection(vector);
		vector *= this.speed;
		this.velocity = this.rb.velocity;
		Vector3 result = vector - this.velocity;
		this.storeTargetVelocity = result;
		result.x = Mathf.Clamp(result.x, -this.maxVelocityChange, this.maxVelocityChange) * this.clampInputVal;
		result.z = Mathf.Clamp(result.z, -this.maxVelocityChange, this.maxVelocityChange) * this.clampInputVal;
		result.y = 0f;
		return result;
	}

	private void HandleSwimmingSpeed(Vector3 velocityChange)
	{
		LocalPlayer.CamFollowHead.stopAllCameraShake();
		this.fallShakeBlock = false;
		Vector3 vector = Vector3.zero;
		vector = Vector3.Slerp(vector, velocityChange, Time.deltaTime * 8f);
		this.rb.AddForce(vector, ForceMode.VelocityChange);
		if (LocalPlayer.AnimControl.swimming && !this.Diving && !LocalPlayer.WaterViz.InWater && this.collFlags.collisionFlags != CollisionFlags.Sides && !this.allowWaterJump)
		{
			this.maxDiveVelocity = this.maxSwimVelocity;
			if (this.rb.velocity.magnitude > this.maxDiveVelocity)
			{
				Vector3 a = this.rb.velocity.normalized;
				a *= this.maxDiveVelocity;
				this.rb.velocity = a;
			}
		}
		else
		{
			this.maxDiveVelocity = 7f;
		}
	}

	private void ClampVelocity()
	{
		if (this.hitByEnemy)
		{
			this.maximumVelocity = 3f;
		}
		else if (this.setNearEnemyVelocity)
		{
			this.maximumVelocity = 5.5f;
		}
		else
		{
			this.maximumVelocity = this.storeMaxVelocity;
		}
		if (this.rb.velocity.magnitude > this.maximumVelocity)
		{
			Vector3 a = this.rb.velocity.normalized;
			a *= this.maximumVelocity;
			this.rb.velocity = a;
		}
	}

	private void ApplyGroundingForce()
	{
		if (this.inputVelocity.magnitude > 0.05f && !this.swimming && this.Grounded && this.allowJump)
		{
			if ((!LocalPlayer.AnimControl.doShellRideMode || !this.buoyancy.InWater) && !LocalPlayer.AnimControl.flyingGlider)
			{
				float num = this.rb.velocity.magnitude * this.rb.mass * this.groundStableForce;
				this.rb.AddForce(new Vector3(0f, -num, 0f));
			}
		}
	}

	private void HandleJumpSpeed(Vector3 velocityChange)
	{
		Vector3 vector = Vector3.zero;
		if (!this.hitByEnemy)
		{
			this.maximumVelocity = this.storeMaxVelocity;
		}
		this.clampAirTouch = Mathf.Lerp(this.clampAirTouch, 0f, Time.deltaTime);
		vector = Vector3.Slerp(vector, velocityChange * 0.9f, Time.deltaTime * 10f);
		vector.x *= this.clampAirTouch;
		vector.z *= this.clampAirTouch;
		vector = Vector3.Slerp(vector, velocityChange * 0.9f, Time.deltaTime * 10f);
		vector.y = velocityChange.y;
		if (this._doingExitVelocity)
		{
			this.rb.AddForce(vector, ForceMode.Force);
			Vector3 forceDir = LocalPlayer.SpecialActions.GetComponent<playerZipLineAction>()._forceDir;
			this.rb.AddForce(forceDir * 10f, ForceMode.Acceleration);
		}
		else if (!LocalPlayer.AnimControl.doShellRideMode && !LocalPlayer.AnimControl.flyingGlider)
		{
			this.rb.AddForce(vector, ForceMode.VelocityChange);
		}
		float magnitude = this.rb.velocity.magnitude;
		if (magnitude > this.maximumVelocity)
		{
			Vector3 a = this.rb.velocity.normalized;
			a *= this.maximumVelocity;
			this.rb.velocity = a;
		}
		if (magnitude > 26f && !this.fallShakeBlock && !LocalPlayer.AnimControl.flyingGlider)
		{
			LocalPlayer.CamFollowHead.StartCoroutine("startFallingShake");
			LocalPlayer.AnimControl.disconnectFromObject();
			this.fallShakeBlock = true;
		}
	}

	private void HandleRunningStaminaAndSpeed()
	{
		if (this.Stats.Stamina < 10.2f && !this.forceStopRun)
		{
			base.StartCoroutine("doForceStopRun");
		}
		if (!LocalPlayer.AnimControl.doShellRideMode && !LocalPlayer.AnimControl.flyingGlider)
		{
			this.Stats.Stamina -= this.staminaCostPerSec * Time.fixedDeltaTime * LocalPlayer.Stats.Skills.RunStaminaRatio;
			LocalPlayer.Stats.Skills.TotalRunDuration += Time.fixedDeltaTime;
		}
		if (this.swimming && !this.Grounded)
		{
			this.speed = this.swimmingSpeed * 2.2f;
		}
		else if (this.Diving)
		{
			this.speed = this.swimmingSpeed;
		}
		else if (this.crouching)
		{
			this.speed = this.crouchSpeed * 1.75f * Mathf.Min(1f + (float)LocalPlayer.Stats.Skills.AthleticismSkillLevel * 0.01f, 1.85f);
		}
		else if (this.isChainSawAttack())
		{
			this.speed = this.walkSpeed;
		}
		else
		{
			this.speed = this.runSpeed * Mathf.Min(1f + (float)LocalPlayer.Stats.Skills.AthleticismSkillLevel * 0.05f, 1.3f);
		}
	}

	private void HandleWalkingSpeedOptions()
	{
		if (this.swimming && !this.Grounded)
		{
			this.speed = this.swimmingSpeed;
		}
		else if (this.Diving)
		{
			this.speed = this.swimmingSpeed * 0.8f;
		}
		else if (this.crouching)
		{
			this.speed = this.crouchSpeed;
		}
		else if (this.inSand && !this.PushingSled)
		{
			this.speed = this.walkSpeed * GameSettings.Survival.SandWalkSpeedRatio;
		}
		else if (this.inSnow && !this.PushingSled)
		{
			this.speed = this.walkSpeed * GameSettings.Survival.SnowWalkSpeedRatio;
		}
		else
		{
			this.speed = this.walkSpeed;
		}
	}

	private void ApplySwimmingForce()
	{
		float num = LocalPlayer.AnimControl.absCamX;
		num = Mathf.Clamp(num, -1f, 1f) * this.inputVelocity.normalized.magnitude;
		float num2 = num;
		if (num2 > 0f)
		{
			num *= 13.5f;
		}
		else
		{
			num *= 18f;
		}
		if (num2 < 0f)
		{
			num2 *= -1f;
		}
		num2 = 1f - num2;
		this.inputVelocity = this.inputVelocity.normalized;
		if ((double)this.rb.velocity.magnitude > 0.05)
		{
			if (this.inputVelocity.z < 0f)
			{
				num *= -1f;
			}
			this.rb.AddForce(new Vector3(0f, -num * this.rb.mass, 0f));
			if (this.rb.velocity.magnitude > this.maxDiveVelocity)
			{
				Vector3 a = this.rb.velocity.normalized;
				a *= this.maxDiveVelocity;
				this.rb.velocity = a;
			}
		}
		if (this.inputVelocity.magnitude < 0.1f)
		{
			this.rb.AddForce(new Vector3(0f, -this.rb.velocity.y * 50f, 0f));
		}
		else
		{
			this.rb.AddForce(new Vector3(0f, -this.rb.velocity.y * 50f * num2, 0f));
		}
	}

	private void ResetVariables()
	{
		if (this.Grounded)
		{
			this.fauxGroundedTimer = Time.time + 0.21f;
		}
		this.Grounded = false;
		this.standingOnRaft = false;
		this.terrainContact = false;
		this.onSlipperySlope = false;
		this.StandingOnDynamicObject = false;
		this.setup.targetInfo.onStructure = false;
	}

	private void disableAllowJump()
	{
		this.allowJump = false;
	}

	public void OnCollisionEnterProxied(Collision coll)
	{
		this.prevVelocity = coll.relativeVelocity.y;
		this.prevVelocityXZ = coll.relativeVelocity;
		this.prevVelocityXZ.y = 0f;
		if (coll.contacts.Length == 0)
		{
			return;
		}
		if (coll.contacts[0].point.y > base.transform.position.y && coll.contacts[0].thisCollider == this.capsule)
		{
			LocalPlayer.Stats.CheckCollisionFromAbove(coll);
		}
	}

	public void OnCollisionStayProxied(Collision col)
	{
		for (int i = 0; i < col.contacts.Length; i++)
		{
			if (BoltNetwork.isClient && this.recentlyDisabledSled && col.contacts[i].otherCollider.gameObject.CompareTag("Multisled"))
			{
				this.multisledContact = true;
			}
			getWalkableSurface component = col.contacts[i].otherCollider.GetComponent<getWalkableSurface>();
			if (component)
			{
				float num = 30f;
				this.modExtremeAngleLimit = 0f;
				if (component.CustomSlopeLimit > 0f)
				{
					num = component.CustomSlopeLimit;
					this.modExtremeAngleLimit = component.CustomSlopeLimit;
				}
				if (component._type == getWalkableSurface.walkableType.slippery && this.collFlags.groundAngleVal > num && this.PrevVelocity < 28.5f)
				{
					this.onSlipperySlope = true;
				}
				else
				{
					this.onSlipperySlope = false;
				}
			}
			if (col.contacts[i].otherCollider.GetType() == typeof(TerrainCollider))
			{
				this.terrainContact = true;
			}
			else if (col.contacts[i].otherCollider.GetType() != typeof(TerrainCollider))
			{
				float num2 = col.contacts[i].point.y - base.transform.position.y;
				if (num2 < 0.5f)
				{
					this.terrainContact = true;
				}
			}
			else
			{
				this.terrainContact = false;
			}
			if (col.contacts[i].otherCollider.GetType() == typeof(CapsuleCollider))
			{
				if (!this.prevGrounded)
				{
					if (57.29578f * Mathf.Asin(col.contacts[i].normal.y) > 45f && this.velocity.y <= 3f && this.collFlags.groundAngleVal < this.extremeAngleGroundedLimit)
					{
						base.CancelInvoke("disableAllowJump");
						this.Grounded = true;
						this.allowJump = true;
					}
				}
				else if (57.29578f * Mathf.Asin(col.contacts[i].normal.y) > 45f && this.collFlags.groundAngleVal < this.extremeAngleGroundedLimit)
				{
					base.CancelInvoke("disableAllowJump");
					this.Grounded = true;
					this.allowJump = true;
				}
			}
			if (col.contacts[i].otherCollider.GetType() == typeof(MeshCollider) && LocalPlayer.AnimControl.swimming)
			{
				float num3 = 57.29578f * Mathf.Asin(col.contacts[i].normal.y);
				float num4 = col.contacts[i].point.y - base.transform.position.y;
				if (num3 > 0f && num4 < 0.5f)
				{
					this.allowWaterJump = true;
				}
				else
				{
					this.allowWaterJump = false;
				}
			}
			else
			{
				this.allowWaterJump = false;
			}
		}
		if (!col.collider)
		{
			return;
		}
		if (this.collFlags.anyPointBelow)
		{
			if (!this.prevGrounded)
			{
				if ((this.velocity.y <= 3f || this.terrainContact) && !this.Diving)
				{
					this.setGroundedParams(col);
				}
			}
			else if (!this.Diving)
			{
				this.setGroundedParams(col);
			}
		}
		else if (this.collFlags.collisionFlags == CollisionFlags.Below)
		{
			this.allowFauxGrounded = false;
			for (int j = 0; j < col.contacts.Length; j++)
			{
				if (col.contacts[j].thisCollider.GetType() == typeof(CapsuleCollider))
				{
					if (!this.Diving)
					{
						this.setGroundedParams(col);
					}
					break;
				}
				if (col.contacts[j].thisCollider.GetType() == typeof(SphereCollider))
				{
					float num5 = this.HeadBlock.bounds.center.y - col.contacts[j].point.y;
					if (!this.prevGrounded && this.jumpingTimer < 3.5f && num5 > 0.3f)
					{
						this.allowFauxGrounded = true;
					}
				}
			}
			if (!this.prevGrounded)
			{
				if ((this.velocity.y <= 3f || this.terrainContact) && !this.Diving && !this.allowFauxGrounded && !this.Diving)
				{
					this.setGroundedParams(col);
				}
			}
			else if (!this.Diving && !this.allowFauxGrounded && !this.Diving)
			{
				this.setGroundedParams(col);
			}
		}
	}

	private void setGroundedParams(Collision col)
	{
		if (col.gameObject.CompareTag("structure") || col.gameObject.CompareTag("UnderfootWood"))
		{
			this.setup.targetInfo.onStructure = true;
		}
		if (col.rigidbody && !col.rigidbody.isKinematic)
		{
			this.StandingOnDynamicObject = true;
		}
		if ((col.gameObject.GetComponentInParent<Buoyancy>() || col.gameObject.GetComponent<DynamicFloor>() || col.gameObject.GetComponent<DynamicFloorProxy>()) && !this.Diving)
		{
			this.standingOnRaft = true;
		}
		base.CancelInvoke("disableAllowJump");
		this.allowJump = true;
		this.Grounded = true;
	}

	private void resetJumpInputBlock()
	{
		this.jumpInputBlock = false;
	}

	private void StickToGroundHelper()
	{
		RaycastHit raycastHit;
		if (Physics.SphereCast(base.transform.position, this.capsule.radius, Vector3.down, out raycastHit, this.capsule.height / 2f - this.capsule.radius + this.stickToGroundHelperDistance) && Mathf.Abs(Vector3.Angle(raycastHit.normal, Vector3.up)) < 85f)
		{
			this.rb.velocity -= Vector3.Project(this.rb.velocity, raycastHit.normal);
		}
	}

	public void ResetRotations()
	{
	}

	private void fallDamageTimer()
	{
		this.allowFallDamage = true;
	}

	private float CalculateJumpVerticalSpeed()
	{
		float num = this.jumpHeight;
		if (this.onSlipperySlope && this.collFlags.groundAngleVal > this.extremeAngleGroundedLimit)
		{
			num /= 9f;
		}
		return Mathf.Sqrt(2f * num * this.gravity);
	}

	private float CalculateWaterJumpVerticalSpeed()
	{
		return Mathf.Sqrt(this.jumpHeight / 2f * this.gravity);
	}

	public void CoveredInMud()
	{
		this.targets.coveredInMud = true;
	}

	public void NotCoveredInMud()
	{
		this.targets.coveredInMud = false;
	}

	public void ScaleCapsuleForCrouching(float alpha)
	{
		if (LocalPlayer.AnimControl.doingGroundChop)
		{
			return;
		}
		float b = this.crouchCapsuleCenter;
		if ((this.inSand || this.inSnow) && !LocalPlayer.AnimControl.doShellRideMode)
		{
			b = this.crouchCapsuleCenter + this.getCapsuleY;
		}
		else
		{
			this.getCapsuleY = 0f;
		}
		this.capsule.height = Mathf.Lerp(this.originalHeight, this.crouchHeight, alpha);
		this.capsule.center = new Vector3(0f, Mathf.Lerp(this.getCapsuleY, b, alpha), 0f);
		this.HeadBlock.center = new Vector3(0f, Mathf.Lerp(1.76f, -0.1f, alpha), this.HeadBlock.center.z);
	}

	private void ResetCapsule()
	{
		this.ScaleCapsuleForCrouching(0f);
	}

	private IEnumerator EnableCrouch()
	{
		base.StopCoroutine("DisableCrouch");
		this.fsmCrouchBool.Value = true;
		this.crouchHeightBlocked = false;
		this.animator.SetBoolReflected("crouchIdle", true);
		LocalPlayer.TargetFunctions.sendVisionRange(12f);
		LocalPlayer.TargetFunctions.enableCrouchLayers();
		float val = 0f;
		float initVal = this.animator.GetFloat("crouch");
		float t = 0f;
		this.getCapsuleY = this.capsule.center.y;
		while (t < 1f)
		{
			t += Time.deltaTime / 0.18f;
			val = Mathf.SmoothStep(initVal, 10f, t);
			this.animator.SetFloatReflected("crouch", val);
			this.ScaleCapsuleForCrouching(val / 10f);
			yield return null;
		}
		yield break;
	}

	private IEnumerator DisableCrouch()
	{
		base.StopCoroutine("EnableCrouch");
		float smoothVelocity = 0f;
		float halfCapsuleHeight = this.originalHeight / 2f;
		float @float = this.animator.GetFloat("crouch");
		float val = @float;
		this.animator.SetBoolReflected("crouchIdle", false);
		while (val > 0.01f)
		{
			float target = 0f;
			if (this.crouchHeightBlocked)
			{
				target = 2.75f - (this.hitInfo.point - base.transform.position).sqrMagnitude / (halfCapsuleHeight * halfCapsuleHeight);
				target = Mathf.Min(target * 1.25f, 1f) * 10f;
				if (val < target)
				{
					smoothVelocity = 1f;
				}
			}
			val = Mathf.SmoothDamp(val, target, ref smoothVelocity, 0.1f);
			this.animator.SetFloatReflected("crouch", val);
			this.ScaleCapsuleForCrouching(val);
			yield return null;
		}
		this.animator.SetFloatReflected("crouch", 0f);
		this.ScaleCapsuleForCrouching(0f);
		this.fsmCrouchBool.Value = false;
		LocalPlayer.TargetFunctions.sendVisionRange(0f);
		LocalPlayer.TargetFunctions.disableCrouchLayers();
		this.crouchHeightBlocked = false;
		this.crouching = false;
		yield break;
	}

	private void NormalSpeed()
	{
	}

	private void Slow()
	{
	}

	private bool isChainSawAttack()
	{
		return LocalPlayer.AnimControl.currLayerState1.shortNameHash == LocalPlayer.AnimControl.chainSawAttackHash;
	}

	public void LockView(bool rigidbodyLock = true)
	{
		if (!BoltNetwork.isRunning && rigidbodyLock && this.Grounded)
		{
			this.rb.Sleep();
			this.rb.isKinematic = true;
			this.rb.useGravity = false;
		}
		this.Locked = true;
		this.CanJump = false;
		TheForest.Utils.Input.UnLockMouse();
	}

	public void UnLockView()
	{
		this.Locked = false;
		this.CanJump = true;
		TheForest.Utils.Input.LockMouse();
		this.rb.isKinematic = false;
		this.rb.useGravity = true;
		this.rb.WakeUp();
	}

	public void OnRaft()
	{
		this.SailingRaft = true;
		this.Grounded = false;
		LocalPlayer.AnimControl.onRaft = true;
		LocalPlayer.SpecialItems.SendMessage("stopLightHeldFire", SendMessageOptions.DontRequireReceiver);
		this.animator.SetBoolReflected("paddleIdleBool", true);
		this.animator.SetLayerWeightReflected(2, 1f);
		this.animator.SetLayerWeightReflected(1, 0f);
		this.animator.SetLayerWeightReflected(4, 0f);
		if (LocalPlayer.PlayerBase.GetComponent<LockLocalPosition>() == null)
		{
			LocalPlayer.PlayerBase.AddComponent<LockLocalPosition>();
		}
	}

	public void OffRaft()
	{
		this.SailingRaft = false;
		this.Grounded = true;
		LocalPlayer.AnimControl.onRaft = false;
		this.animator.SetBoolReflected("paddleIdleBool", false);
		this.animator.SetBoolReflected("paddleBool", false);
		LocalPlayer.Transform.localEulerAngles = new Vector3(0f, LocalPlayer.Transform.localEulerAngles.y, 0f);
		UnityEngine.Object.Destroy(LocalPlayer.PlayerBase.GetComponentInChildren<LockLocalPosition>());
	}

	public void enablePaddleOnRaft(bool set)
	{
		this.animator.SetBoolReflected("paddleBool", set);
	}

	public void OnSled()
	{
		this.PushingSled = true;
	}

	public void OffSled()
	{
		this.PushingSled = false;
	}

	public bool IsAboveWaistDeep()
	{
		return this.CalculateWaterDepth() >= 7.5f;
	}

	public bool IsInWater()
	{
		return this.buoyancy.WaterLevel > this.capsule.bounds.min.y;
	}

	public float CalculateWaterDepth()
	{
		float num = this.buoyancy.WaterLevel - this.capsule.bounds.min.y;
		if (num >= 0f)
		{
			return Mathf.Clamp01(num / this.capsule.height) * 5f + 5f;
		}
		return WaterOnTerrain.Instance.RainIntensity * 1f;
	}

	public float CalculateSpeedParameter(float flatVelocity)
	{
		if (flatVelocity > this.walkSpeed)
		{
			return Mathf.Clamp01((flatVelocity - this.walkSpeed) / (this.runSpeed - this.walkSpeed));
		}
		float num = this.walkSpeed - this.crouchSpeed;
		return Mathf.Clamp01((flatVelocity - this.crouchSpeed) / num) - 1f;
	}

	private IEnumerator resetFallDamage()
	{
		yield return YieldPresets.WaitOnePointFiveSeconds;
		this.jumpCoolDown = false;
		yield break;
	}

	private void setAnimatorJump()
	{
		LocalPlayer.Animator.SetBoolReflected("jumpBool", true);
	}

	private void resetAnimSpine()
	{
		this.jumpLand = false;
		base.StartCoroutine("smoothEnableSpine");
	}

	private void allowJumpingAttack()
	{
		LocalPlayer.AnimControl.setup.pmControl.FsmVariables.GetFsmBool("allowJumpAttack").Value = true;
	}

	private void blockJumpAttack()
	{
		LocalPlayer.AnimControl.setup.pmControl.FsmVariables.GetFsmBool("allowJumpAttack").Value = false;
	}

	private IEnumerator smoothEnableSpine()
	{
		float t = 0f;
		float val = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime * 2f;
			val = Mathf.Lerp(0f, 1f, t);
			if (!LocalPlayer.Animator.GetBool("drawBowBool"))
			{
				this.animator.SetLayerWeightReflected(4, val);
			}
			this.animator.SetLayerWeightReflected(1, val);
			yield return null;
		}
		this.jumpCoolDown = false;
		this.CanJump = true;
		LocalPlayer.HitReactions.disableControllerFreeze();
		LocalPlayer.MainRotator.rotationSpeed = 5f;
		yield break;
	}

	public void resetPhysicMaterial()
	{
		this.playerPhysicMaterial.dynamicFriction = 1f;
		this.playerPhysicMaterial.staticFriction = 1f;
		this.playerPhysicMaterial.frictionCombine = PhysicMaterialCombine.Average;
		this.playerPhysicMaterial.bounceCombine = PhysicMaterialCombine.Minimum;
		this.playerPhysicMaterial.bounciness = 0f;
	}

	private void disableHighDrag()
	{
		this.enforceHighDrag = false;
	}

	private IEnumerator enableStamRechargeDelay()
	{
		this.stamRechargeDelay = true;
		float t = 0f;
		while (t < 2.5f || TheForest.Utils.Input.GetButton("Run"))
		{
			this.Stats.Stamina = 9.9f;
			t += Time.deltaTime;
			yield return null;
		}
		this.stamRechargeDelay = false;
		yield break;
	}

	private IEnumerator doForceStopRun()
	{
		this.forceStopRun = true;
		yield return YieldPresets.WaitPointNineSeconds;
		this.CantRun = true;
		this.forceStopRun = false;
		this.recoveringFromRun = this.timeToRecoverFromRun;
		if (PlayerPreferences.UseSprintToggle)
		{
			this.run = false;
		}
		yield break;
	}

	public void clampVelocityTowardEnemy(Vector3 otherPos)
	{
		Vector3 normalized = this.storeTargetVelocity.normalized;
		normalized.y = 0f;
		Vector3 normalized2 = (otherPos - base.transform.position).normalized;
		normalized2.y = 0f;
		float num = Vector3.Angle(normalized, normalized2);
		if (num > -60f && num < 60f)
		{
			this.clampInputVal = Mathf.Clamp(num / 90f, 0f, 1f);
			this.clampInputVal *= this.clampInputVal;
			if (this.clampInputVal < 0.2f)
			{
				Vector3 zero = Vector3.zero;
				zero.y = this.rb.velocity.y;
				this.rb.velocity = zero;
				this.clampInputVal = 0f;
			}
			this.doingClampInput = true;
		}
		else
		{
			this.clampInputVal = 1f;
			this.doingClampInput = false;
		}
	}

	public void resetClampInput()
	{
		this.clampInputVal = 1f;
		this.doingClampInput = false;
	}

	private IEnumerator doClampVelocity()
	{
		if (this.setNearEnemyVelocity)
		{
			yield break;
		}
		float t = 0f;
		while (t < 0.65f)
		{
			this.setNearEnemyVelocity = true;
			t += Time.deltaTime;
			yield return null;
		}
		this.setNearEnemyVelocity = false;
		yield break;
	}

	private IEnumerator DisconnectedSledRoutine()
	{
		this.recentlyDisabledSled = true;
		yield return YieldPresets.WaitOnePointFiveSeconds;
		this.recentlyDisabledSled = false;
		yield break;
	}

	public void setEnemyContact(bool set)
	{
		if (set)
		{
			this.contactWithEnemy = true;
		}
		else
		{
			this.contactWithEnemy = false;
			this.doingClampInput = false;
		}
	}

	private IEnumerator startJumpTimer()
	{
		if (this.jumpTimerStarted)
		{
			yield break;
		}
		this.jumpingTimer = 0f;
		this.jumpTimerStarted = true;
		for (;;)
		{
			this.jumpingTimer += Time.deltaTime;
			yield return null;
		}
		yield break;
	}

	private IEnumerator clampInput()
	{
		this.doingClampInput = true;
		this.clampInputVal = 0f;
		float timer = 0f;
		while (timer < this.clampInputDelay)
		{
			timer += Time.deltaTime;
			yield return null;
		}
		this.clampInputVal = 1f;
		this.doingClampInput = false;
		yield break;
	}

	private void resetBlockWaterJump()
	{
		this.blockWaterJump = false;
	}

	private void resetFauxJump()
	{
		this.blockFauxJump = false;
	}

	public Vector2 InputHack;

	public LayerMask CollisionLayers;

	public float walkSpeed = 3f;

	public float runSpeed = 8f;

	public float strafeSpeed = 4f;

	public float crouchSpeed = 2f;

	public float swimmingSpeed = 3f;

	public float staminaCostPerSec = 4f;

	public float minCamRotationRange = 155f;

	public bool SailingRaft;

	public bool PushingSled;

	public bool Diving;

	public bool standingOnRaft;

	public bool StandingOnDynamicObject;

	public bool Locked;

	public bool MovementLocked;

	public Transform Inventory;

	public Transform SurvivalBook;

	public SphereCollider HeadBlock;

	public float gravity = 10f;

	public float maxVelocityChange = 10f;

	public float maximumVelocity = 25f;

	public bool CanJump = true;

	public float jumpHeight = 2f;

	public float stickToGroundHelperDistance = 0.05f;

	public float groundStableForce;

	public float extremeAngleGroundedLimit = 65f;

	public float maxDiveVelocity = 6.5f;

	public float maxSwimVelocity = 3f;

	public bool drinking;

	public bool run;

	public bool running;

	public bool snowFlotation;

	public bool crouchHeightBlocked;

	public bool _doingExitVelocity;

	private bool _buildInputActive;

	public const float HARD_FALL_THRESHOLD = 28.5f;

	public RigidBodyCollisionFlags collFlags;

	public PhysicMaterial playerPhysicMaterial;

	public PhysicMaterial playerPhysicMaterial2;

	private PlayerStats Stats;

	private Buoyancy buoyancy;

	private playerScriptSetup setup;

	private playerTargetFunctions targets;

	private Animator animator;

	private VRPlayerAdapter vrAdapter;

	private SphereCollider headCollider;

	public CapsuleCollider capsule;

	public Rigidbody rb;

	public bool prevGrounded = true;

	public bool allowFallDamage;

	public bool jumpCoolDown;

	public bool jumping;

	public bool allowJump;

	public bool checkGrounded;

	private bool validateGrounded;

	public bool checkSwimming;

	private float prevVelocity;

	public bool terrainContact;

	private Vector2 input;

	public Vector3 inputVelocity;

	public Vector3 velocity;

	private bool crouchBlock;

	private FsmBool fsmCrouchBool;

	private FsmBool fsmCrouchBlockedBool;

	public bool crouching;

	public bool standingUp;

	public bool jumpingAttack;

	private bool jumpLand;

	private bool jumpFuzzyDelay;

	private float speed;

	public bool crouch;

	public bool standUp;

	public float originalHeight;

	public float originalYPos;

	private float yChange;

	public float getCapsuleY;

	private bool inSand;

	public bool inSnow;

	public float crouchCapsuleCenter;

	public float crouchHeight = 3f;

	public bool CantRun;

	private float prevMouseXSpeed;

	private FsmBool fsmClimbBool;

	private FsmBool fsmGrounded;

	private const float jumpRayLength = 0.7f;

	private float lastUpdateTime;

	private bool stamRechargeDelay;

	public bool allowWaterJump;

	private bool jumpInputBlock;

	public bool contactWithEnemy;

	private float storeMaxVelocity;

	public bool hitByEnemy;

	public bool setNearEnemyVelocity;

	public float jumpingTimer;

	public float clampInputVal = 1f;

	public bool doingClampInput;

	private bool fallShakeBlock;

	public float clampInputDelay = 0.2f;

	public float defaultMass;

	private bool forceStopRun;

	private bool blockWaterJump;

	private bool FauxGrounded;

	private bool blockFauxJump;

	private float fauxGroundedTimer;

	private float prevYPos;

	private Vector3 storeTargetVelocity;

	private float clampAirTouch;

	private float getSlopeLimit;

	public float timeToRecoverFromRun = 0.4f;

	private Vector3 prevVelocityXZ;

	private bool jumpTimerStarted;

	public bool onSlipperySlope;

	private RaycastHit hitInfo;

	private bool recentlyDisabledSled;

	private bool allowFauxGrounded;

	private bool multisledContact;

	public bool enforceHighDrag;

	private float modExtremeAngleLimit;

	private const float WET_GROUND_END = 1f;

	private const float IMMERSION_START = 5f;

	private const float IMMERSION_END = 10f;

	private const float IMMERSION_RANGE = 5f;
}
