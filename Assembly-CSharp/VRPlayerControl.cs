using System;
using System.Collections;
using TheForest.Items;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class VRPlayerControl : MonoBehaviour
{
	private void OnDestroy()
	{
		if (this.VROffsetTransform != null)
		{
			UnityEngine.Object.Destroy(this.VROffsetTransform.gameObject);
		}
	}

	private void Awake()
	{
		this.shellLocalPos = this.ShellHeldTransform.localPosition;
		this.shellLocalRot = this.ShellHeldTransform.localRotation;
		this.OriginHeight = this.VRCameraOrigin.localPosition.y;
		this.coopSync = base.transform.GetComponent<VRCoopPlayerSync>();
	}

	private void Start()
	{
		this.ikSolve = LocalPlayer.vrAdapter.HeightScaleGo.GetComponents<InverseKinematics>();
		if (ForestVR.Enabled)
		{
			this.watchParentLocalPos = LocalPlayer.vrAdapter.VRWatchParentGo.transform.localPosition;
			this.watchParentLocalRot = LocalPlayer.vrAdapter.VRWatchParentGo.transform.localRotation;
			this.watchParentLocalScale = LocalPlayer.vrAdapter.VRWatchParentGo.transform.localScale;
			foreach (InverseKinematics inverseKinematics in this.ikSolve)
			{
				inverseKinematics.enabled = true;
			}
			this.canSolveIk = true;
			this.VRLighterHeldGo.SetActive(false);
			this.updateCameraCentre();
			this.laserPointerGo.SetActive(false);
		}
	}

	private void Update()
	{
		if (ForestVR.Enabled)
		{
			if (VRPlayerControl.canUpdateMovement())
			{
				this.UpdateFlick();
				this.UpdateCrouchHeight();
			}
			this.updateCameraCentre();
			this.updateWeaponVis();
			this.UpdateHandAttachments();
			this.UpdateWeaponHandedness();
			this.UpdateIconCameraVisibility();
		}
	}

	private void LateUpdate()
	{
		if (ForestVR.Enabled)
		{
			if (VRPlayerControl.canUpdateMovement())
			{
				this.UpdatePlayerAngleVr();
				this.RoomScaleCameraAlign();
				this.updateSpineRotation();
			}
			this.canSolveIk = !LocalPlayer.vrPlayerControl.useGhostMode;
			if (this.canSolveIk)
			{
				for (int i = 0; i < this.coopSync.shoulders.Length; i++)
				{
				}
				for (int j = 0; j < this.coopSync.elbows.Length; j++)
				{
				}
				for (int k = 0; k < this.ikSolve.Length; k++)
				{
					this.ikSolve[k].Solve();
				}
			}
		}
	}

	private void UpdateIconCameraVisibility()
	{
		if (Grabber.IsFocused || LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.World)
		{
			if (!Scene.HudGui.VRIconCam.enabled)
			{
				Scene.HudGui.VRIconCam.enabled = true;
			}
		}
		else if (Scene.HudGui.VRIconCam.enabled)
		{
			Scene.HudGui.VRIconCam.enabled = false;
		}
	}

	private static bool canUpdateMovement()
	{
		return !LocalPlayer.FpCharacter.MovementLocked && !LocalPlayer.FpCharacter.Locked && !LocalPlayer.AnimControl.onRope && LocalPlayer.CurrentView == PlayerInventory.PlayerViews.World && !LocalPlayer.AnimControl.onRockThrower && !LocalPlayer.AnimControl.sitting && !LocalPlayer.FpCharacter.PushingSled && LocalPlayer.FpCharacter.enabled && !LocalPlayer.AnimControl.flyingGlider;
	}

	private void UpdateHandAttachments()
	{
		this.RightHand.hoverSphereRadius = ((!LocalPlayer.Inventory.IsRightHandEmpty()) ? 0.001f : 0.08f);
		if (LocalPlayer.AnimControl.doShellRideMode)
		{
			if (this.ShellHeldTransform.parent != this.ShellOrientTransform)
			{
				this.ShellHeldTransform.parent = this.ShellOrientTransform;
				this.ShellHeldTransform.localPosition = Vector3.zero;
				this.ShellHeldTransform.localRotation = Quaternion.identity;
			}
		}
		else if (this.ShellHeldTransform.parent != LocalPlayer.ScriptSetup.rightHandHeld)
		{
			this.ShellHeldTransform.parent = LocalPlayer.ScriptSetup.rightHandHeld;
			this.ShellHeldTransform.localPosition = this.shellLocalPos;
			this.ShellHeldTransform.localRotation = this.shellLocalRot;
		}
	}

	private void UpdateCrouchHeight()
	{
		if (this.useGhostMode || LocalPlayer.CurrentView != PlayerInventory.PlayerViews.World)
		{
			return;
		}
		if (PlayerPreferences.VRUsePhysicalCrouching)
		{
			if (this.VRCameraRig.InverseTransformPoint(this.VRCamera.position).y < 1.26f)
			{
				this.toggleCrouchFromVrHeight = true;
			}
			else
			{
				this.toggleCrouchFromVrHeight = false;
			}
			return;
		}
		if ((LocalPlayer.Animator.GetFloat("crouch") > 5f || LocalPlayer.AnimControl.doShellRideMode) && !this.doingCrouch)
		{
			this.VRCameraOrigin.localPosition = new Vector3(this.VRCameraOrigin.localPosition.x, this.OriginHeight / 1.45f, this.VRCameraOrigin.localPosition.z);
			this.centerVrSpaceAroundHead();
			this.doingCrouch = true;
		}
		else if (LocalPlayer.Animator.GetFloat("crouch") < 5f && this.doingCrouch && !LocalPlayer.AnimControl.doShellRideMode)
		{
			this.VRCameraOrigin.localPosition = new Vector3(this.VRCameraOrigin.localPosition.x, this.OriginHeight, this.VRCameraOrigin.localPosition.z);
			this.centerVrSpaceAroundHead();
			this.doingCrouch = false;
		}
	}

	private void updateWeaponVis()
	{
		bool flag = false;
		if (LocalPlayer.IsInInventory != this.inventoryOpen)
		{
			flag = true;
			this.inventoryOpen = LocalPlayer.IsInInventory;
		}
		else if (LocalPlayer.Create.CreateMode != this.createGhostMode)
		{
			flag = true;
			this.createGhostMode = LocalPlayer.Create.CreateMode;
		}
		else if (LocalPlayer.IsInPauseMenu != this.pauseMenuMode)
		{
			flag = true;
			this.pauseMenuMode = LocalPlayer.IsInPauseMenu;
		}
		if (flag)
		{
			if (this.leftHandControllerGo.activeSelf != this.pauseMenuMode)
			{
				this.leftHandControllerGo.SetActive(this.pauseMenuMode);
			}
			if (this.RightHandControllerGo.activeSelf != this.pauseMenuMode)
			{
				this.RightHandControllerGo.SetActive(this.pauseMenuMode);
			}
			if (LocalPlayer.ScriptSetup.rightHandHeld.gameObject.activeSelf != !this.inventoryOpen)
			{
				LocalPlayer.ScriptSetup.rightHandHeld.gameObject.SetActive(!this.inventoryOpen);
			}
			bool flag2 = this.inventoryOpen | this.createGhostMode | this.pauseMenuMode;
			if (this.laserPointerGo.activeSelf != flag2)
			{
				this.laserPointerGo.SetActive(flag2);
			}
			LocalPlayer.Animator.SetBool("laserHeld", flag2);
		}
	}

	private void updateCameraCentre()
	{
		if (LocalPlayer.vrPlayerControl.useGhostMode || LocalPlayer.AnimControl.introCutScene)
		{
			return;
		}
		if (PlayerPreferences.VRUsePhysicalCrouching && LocalPlayer.CurrentView != PlayerInventory.PlayerViews.PlaneCrash)
		{
			return;
		}
		Transform transform = this.VRCameraOrigin;
		float num = 1f;
		float num2 = 0.5f;
		if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.PlaneCrash)
		{
			if (BoltNetwork.isClient)
			{
				transform = Scene.TriggerCutScene.ClientSeatPosVR;
			}
			else
			{
				transform = Scene.TriggerCutScene.playerSeatPosVR;
			}
			num = 1f;
		}
		if (transform == null)
		{
			return;
		}
		float num3 = Vector3.Distance(this.VRCamera.position, transform.position);
		float num4 = this.VRCamera.position.y - transform.position.y;
		if (num3 > num)
		{
			if (Time.time > this.distanceResetTimer && LocalPlayer.CurrentView == PlayerInventory.PlayerViews.PlaneCrash)
			{
				this.centerVrSpaceAroundHead();
			}
		}
		else
		{
			this.distanceResetTimer = Time.time + 1f;
		}
		if (num4 < -num2 || num4 > num2)
		{
			if (Time.time > this.heightResetTimer)
			{
				this.centerVrSpaceAroundHead();
			}
		}
		else
		{
			this.heightResetTimer = Time.time + 2f;
		}
	}

	public void centerVrSpaceAroundHead()
	{
		Transform transform = this.VRCameraOrigin;
		if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.PlaneCrash)
		{
			if (BoltNetwork.isClient)
			{
				transform = Scene.TriggerCutScene.ClientSeatPosVR;
			}
			else
			{
				transform = Scene.TriggerCutScene.playerSeatPosVR;
			}
		}
		Vector3 b = this.VRCamera.position - transform.position;
		this.VRCameraRig.position = this.VRCameraRig.position - b;
		if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.PlaneCrash)
		{
			this.VRCameraRig.rotation = transform.rotation;
			this.VRCameraRig.localEulerAngles = new Vector3(0f, transform.localEulerAngles.y, 0f);
		}
	}

	public IEnumerator centerVrSpaceAroundHeadAndMovePlayer()
	{
		Transform origin = this.VRCameraOrigin;
		if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.PlaneCrash)
		{
			if (BoltNetwork.isClient)
			{
				origin = Scene.TriggerCutScene.ClientSeatPosVR;
			}
			else
			{
				origin = Scene.TriggerCutScene.playerSeatPosVR;
			}
		}
		Vector3 camPos = this.VRCamera.position;
		camPos.y = origin.position.y;
		Vector3 offsetRigDir = camPos - origin.position;
		if (offsetRigDir.magnitude < 1f)
		{
			yield break;
		}
		this.VRCameraRig.position = this.VRCameraRig.position - offsetRigDir;
		if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.PlaneCrash)
		{
			this.VRCameraRig.rotation = origin.rotation;
			this.VRCameraRig.localEulerAngles = new Vector3(0f, this.VRCameraRig.localEulerAngles.y, 0f);
		}
		Vector3 finalPos = LocalPlayer.Rigidbody.position + offsetRigDir;
		LocalPlayer.ScriptSetup.forceLocalPos.forcePositionUpdate = true;
		LocalPlayer.Rigidbody.position = finalPos;
		yield return YieldPresets.WaitForEndOfFrame;
		yield return YieldPresets.WaitForEndOfFrame;
		LocalPlayer.ScriptSetup.forceLocalPos.forcePositionUpdate = false;
		yield break;
	}

	public void MovePlayerRelativeToVrPosition()
	{
		Transform transform = this.VRCameraOrigin;
		if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.PlaneCrash)
		{
			if (BoltNetwork.isClient)
			{
				transform = Scene.TriggerCutScene.ClientSeatPosVR;
			}
			else
			{
				transform = Scene.TriggerCutScene.playerSeatPosVR;
			}
		}
		Vector3 position = this.VRCamera.position;
		position.y = transform.position.y;
		Vector3 vector = position - transform.position;
		if (vector.magnitude < this.MovePlayerThreshold || vector.magnitude > 1f)
		{
			if (this.BlockedEffect != null)
			{
				this.BlockedEffect.HeadMoved = false;
			}
			return;
		}
		vector -= vector.normalized * this.MovePlayerThreshold;
		Vector3 vector2 = LocalPlayer.Rigidbody.position + vector;
		LocalPlayer.Rigidbody.position = vector2;
		if (this.BlockedEffect != null)
		{
			this.BlockedEffect.HeadMoved = true;
			this.BlockedEffect.UpdateGoalPosition(vector2);
		}
		this.VRCameraRig.position = this.VRCameraRig.position - vector;
	}

	public void rotateVrSpaceToPlayer()
	{
		this.VRCameraRig.rotation = LocalPlayer.PlayerBase.transform.rotation;
		this.VRCameraRig.localEulerAngles = new Vector3(0f, LocalPlayer.PlayerBase.transform.localEulerAngles.y, 0f);
	}

	private void updateSpineRotation()
	{
		if (LocalPlayer.vrPlayerControl.useGhostMode)
		{
			return;
		}
		Vector3 vector = this.VRCameraOrigin.InverseTransformPoint(this.followTarget.position);
		vector.x = Mathf.Clamp(vector.x, -this.xClampValue, this.xClampValue) * this.spineBendAmount;
		vector.z = Mathf.Clamp(vector.z, -this.zClampValue, this.zClampValue) * this.spineBendAmount;
		vector = this.VRCameraOrigin.TransformPoint(vector);
		vector.y = this.VRCameraOrigin.position.y;
		this.smoothPosition = Vector3.SmoothDamp(this.smoothPosition, vector, ref this.velRef, 0.025f);
		Vector3 vector2 = this.smoothPosition - this.spineBones[0].position;
		Debug.DrawRay(this.spineBones[0].position, vector2 * 5f, Color.red);
		Quaternion rotation = Quaternion.LookRotation(vector2, base.transform.forward) * Quaternion.Euler(-90f, 90f, 90f);
		if (!this.disableSpineInput())
		{
			this.spineBones[0].rotation = rotation;
		}
		Vector3 vector3 = this.headTransform.InverseTransformPoint(this.followTarget.position + this.followTarget.forward * 10f);
		float num = Mathf.Atan2(vector3.x, vector3.z) * 57.29578f;
		num /= 110f * this.spineTwistAmount;
		num = Mathf.Clamp(num, -1f, 1f);
		this.smoothTargetAngle = Mathf.Lerp(this.smoothTargetAngle, num, Time.deltaTime * 13f);
		if (!this.disableSpineInput())
		{
			this.spineBones[0].transform.RotateAround(vector2, this.smoothTargetAngle);
		}
	}

	private bool disableSpineInput()
	{
		return LocalPlayer.AnimControl.useRootMotion || LocalPlayer.AnimControl.doingGroundChop || LocalPlayer.CamFollowHead.followAnim;
	}

	private void UpdateFlick()
	{
		if (LocalPlayer.vrPlayerControl.useGhostMode)
		{
			return;
		}
		Vector2 vector = new Vector2(TheForest.Utils.Input.GetAxis("Mouse X"), TheForest.Utils.Input.GetAxis("Mouse Y"));
		if (this.HorizontalFlickable)
		{
			bool flag = PlayerPreferences.VRTurnSnap == 0;
			if (Mathf.Abs(vector.x) >= this.HorizontalDeadzone && (Mathf.Abs(this.lastAim.x) < this.HorizontalDeadzone || flag))
			{
				float angle;
				if (flag)
				{
					float num = Mathf.Lerp(0.25f, 3f, PlayerPreferences.MouseSensitivityX);
					float num2 = Time.deltaTime / 0.0222222228f;
					angle = vector.x * num2 * num;
				}
				else
				{
					angle = Mathf.Sign(vector.x) * (float)Mathf.Max(PlayerPreferences.VRTurnSnap, 5);
				}
				this.RotatableTransform.Rotate(Vector3.up, angle);
			}
		}
		if (this.VerticalFlickable && Mathf.Abs(vector.y) >= this.VerticalDeadzone && Mathf.Abs(this.lastAim.y) < this.VerticalDeadzone)
		{
			this.RotatableTransform.Rotate(Vector3.up, 180f);
		}
		this.lastAim = vector;
	}

	public void UpdatePlayerAngleVr()
	{
		float num = LocalPlayer.FpCharacter.inputVelocity.x * LocalPlayer.FpCharacter.inputVelocity.x + LocalPlayer.FpCharacter.inputVelocity.z * LocalPlayer.FpCharacter.inputVelocity.z;
		if (num > 0.01f)
		{
			Vector3 vector;
			if (PlayerPreferences.VRUseCameraDirectedForwardMovement)
			{
				vector = LocalPlayer.vrAdapter.VREyeCamera.transform.forward;
			}
			else if (PlayerPreferences.VRUseControllerDirectedForwardMovement)
			{
				vector = LocalPlayer.vrPlayerControl.LeftHand.transform.forward;
			}
			else
			{
				vector = LocalPlayer.vrPlayerControl.VROffsetTransform.forward;
			}
			vector.y = 0f;
			vector = vector.normalized;
			float num2 = Vector3.Angle(vector, LocalPlayer.Transform.forward);
			if (Vector3.Cross(vector, LocalPlayer.Transform.forward).y > 0f)
			{
				num2 = -num2;
			}
			if (!LocalPlayer.vrAdapter.TheatreController.theatreOn && !LocalPlayer.AnimControl.onRope && LocalPlayer.CurrentView == PlayerInventory.PlayerViews.World)
			{
				if (PlayerPreferences.VRForwardMovement != PlayerPreferences.VRForwardDirectionTypes.PLAYER)
				{
					LocalPlayer.Transform.Rotate(Vector3.up, num2);
					LocalPlayer.vrPlayerControl.VROffsetTransform.Rotate(Vector3.up, -num2);
				}
				if (!this._startedMoving)
				{
					LocalPlayer.vrPlayerControl.StartCoroutine(LocalPlayer.vrPlayerControl.centerVrSpaceAroundHeadAndMovePlayer());
				}
			}
			if (!LocalPlayer.vrAdapter.TheatreController.theatreOn && !LocalPlayer.AnimControl.onRope && LocalPlayer.CurrentView == PlayerInventory.PlayerViews.World)
			{
				LocalPlayer.vrPlayerControl.MovePlayerRelativeToVrPosition();
			}
			if (LocalPlayer.vrPlayerControl.BlockedEffect != null)
			{
				LocalPlayer.vrPlayerControl.BlockedEffect.ClearOffsets();
				LocalPlayer.vrPlayerControl.BlockedEffect.HeadMoved = false;
			}
			if (!this._startedMoving)
			{
				LocalPlayer.ScriptSetup.forceLocalPos.forcePositionUpdate = false;
			}
			this._startedMoving = true;
		}
		else if (num < 0.01f && LocalPlayer.ScriptSetup.forceLocalPos.offsetDistance < 0.01f)
		{
			if (this._startedMoving)
			{
				LocalPlayer.ScriptSetup.forceLocalPos.forcePositionUpdate = true;
			}
			if (!LocalPlayer.vrAdapter.TheatreController.theatreOn && !LocalPlayer.AnimControl.onRope && LocalPlayer.CurrentView == PlayerInventory.PlayerViews.World)
			{
				LocalPlayer.vrPlayerControl.MovePlayerRelativeToVrPosition();
			}
			this._startedMoving = false;
		}
	}

	public void RoomScaleCameraAlign()
	{
		Vector3 vector = LocalPlayer.vrAdapter.VREyeCamera.transform.forward;
		vector.y = 0f;
		vector = vector.normalized;
		float num = Vector3.Angle(vector, LocalPlayer.Transform.forward);
		if (Vector3.Cross(vector, LocalPlayer.Transform.forward).y > 0f)
		{
			num = -num;
		}
		if ((num > 105f || num < -105f) && !LocalPlayer.vrAdapter.TheatreController.theatreOn && !LocalPlayer.AnimControl.onRope && LocalPlayer.CurrentView == PlayerInventory.PlayerViews.World)
		{
			LocalPlayer.Transform.Rotate(Vector3.up, num);
			LocalPlayer.vrPlayerControl.VROffsetTransform.Rotate(Vector3.up, -num);
		}
	}

	public void UpdateWeaponHandedness()
	{
		bool flag = (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, LocalPlayer.AnimControl._bowRecurveId) || LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, LocalPlayer.AnimControl._bowId) || LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, LocalPlayer.AnimControl._slingShotId)) && PlayerPreferences.VRUseRightHandedBow;
		bool flag2 = !PlayerPreferences.VRUseRightHandedWeapon && !LocalPlayer.Animator.GetBool("flintLockHeld") && !LocalPlayer.Animator.GetBool("chainSawHeld") && !LocalPlayer.Animator.GetBool("flaregunHeld") && !LocalPlayer.Inventory.IsRightHandEmpty();
		if (LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.World)
		{
			flag = false;
			flag2 = false;
		}
		if ((flag || flag2) && !this.RightHandedActive)
		{
			LocalPlayer.ScriptSetup.targetInfo.RightHandActive = true;
			LocalPlayer.vrAdapter.VRPlayerHands.transform.localScale = new Vector3(-1f, 1f, 1f);
			this.LeftForearmTransform.localScale = new Vector3(-1f, 1f, 1f);
			this.RightForearmTransform.localScale = new Vector3(-1f, 1f, 1f);
			this.RightHand.startingHandType = Hand.HandType.Left;
			this.LeftHand.startingHandType = Hand.HandType.Right;
			this.LeftHandIkScaleTransform.localScale = new Vector3(-1f, 1f, 1f);
			this.RightHandIkScaleTransform.localScale = new Vector3(-1f, 1f, 1f);
			Vector3 localScale = LocalPlayer.vrAdapter.VRWatchParentGo.transform.localScale;
			localScale.x *= -1f;
			LocalPlayer.vrAdapter.VRWatchParentGo.transform.localScale = localScale;
			LocalPlayer.vrAdapter.VRWatchParentGo.transform.localEulerAngles = new Vector3(-73.97f, -178.63f, -118.86f);
			this.LeftActionButtons.SetParent(this.RightHand.transform, false);
			this.RightActionButtons.SetParent(this.LeftHand.transform, false);
			this.RightHandedActive = true;
		}
		else if (!flag && !flag2 && this.RightHandedActive)
		{
			LocalPlayer.ScriptSetup.targetInfo.RightHandActive = false;
			LocalPlayer.vrAdapter.VRPlayerHands.transform.localScale = new Vector3(1f, 1f, 1f);
			this.LeftForearmTransform.localScale = new Vector3(1f, 1f, 1f);
			this.RightForearmTransform.localScale = new Vector3(1f, 1f, 1f);
			this.RightHand.startingHandType = Hand.HandType.Right;
			this.LeftHand.startingHandType = Hand.HandType.Left;
			this.LeftHandIkScaleTransform.localScale = new Vector3(1f, 1f, 1f);
			this.RightHandIkScaleTransform.localScale = new Vector3(1f, 1f, 1f);
			LocalPlayer.vrAdapter.VRWatchParentGo.transform.localRotation = this.watchParentLocalRot;
			LocalPlayer.vrAdapter.VRWatchParentGo.transform.localScale = this.watchParentLocalScale;
			this.LeftActionButtons.SetParent(this.LeftHand.transform, false);
			this.RightActionButtons.SetParent(this.RightHand.transform, false);
			this.RightHandedActive = false;
		}
	}

	public void ShowRespawnPrompt(bool onoff)
	{
		VRControllerDisplayManager componentInChildren = this.RightHand.GetComponentInChildren<VRControllerDisplayManager>();
		if (componentInChildren == null)
		{
			return;
		}
		if (componentInChildren.RespawnButton)
		{
			componentInChildren.RespawnButton.SetActive(onoff);
		}
	}

	public void ToggleVrControllers(bool onoff)
	{
		if (onoff)
		{
			this.leftHandControllerGo.SetActive(true);
			this.RightHandControllerGo.SetActive(true);
			Transform[] componentsInChildren = this.leftHandControllerGo.GetComponentsInChildren<Transform>();
			foreach (Transform transform in componentsInChildren)
			{
				transform.gameObject.layer = 18;
			}
			Transform[] componentsInChildren2 = this.RightHandControllerGo.GetComponentsInChildren<Transform>();
			foreach (Transform transform2 in componentsInChildren2)
			{
				transform2.gameObject.layer = 18;
			}
		}
		else
		{
			Transform[] componentsInChildren3 = this.leftHandControllerGo.GetComponentsInChildren<Transform>();
			foreach (Transform transform3 in componentsInChildren3)
			{
				transform3.gameObject.layer = 8;
			}
			Transform[] componentsInChildren4 = this.RightHandControllerGo.GetComponentsInChildren<Transform>();
			foreach (Transform transform4 in componentsInChildren4)
			{
				transform4.gameObject.layer = 8;
			}
			this.leftHandControllerGo.SetActive(false);
			this.RightHandControllerGo.SetActive(false);
		}
	}

	private InverseKinematics[] ikSolve;

	private VRCoopPlayerSync coopSync;

	public Transform ShellHeldTransform;

	public Transform ShellOrientTransform;

	public GameObject VRLighterHeldGo;

	public GameObject playerLighterHeldGo;

	public GameObject leftHandHeldGo;

	public GameObject laserPointerGo;

	public GameObject leftHandControllerGo;

	public GameObject RightHandControllerGo;

	public SkinnedMeshRenderer targetArmsRenderer;

	public SkinnedMeshRenderer VRArms;

	public SkinnedMeshRenderer DefaultArms;

	public Transform RotatableTransform;

	public Transform VRCamera;

	public Transform VRCameraOrigin;

	public Transform VROffsetTransform;

	public Transform VRCameraRig;

	public GameObject VRLightableTrigger;

	public Hand LeftHand;

	public Hand RightHand;

	public Transform RightIkOffset;

	public Transform LeftForearmTransform;

	public Transform RightForearmTransform;

	public Transform LeftHandIkScaleTransform;

	public Transform RightHandIkScaleTransform;

	public Transform LeftActionButtons;

	public Transform RightActionButtons;

	public bool HorizontalFlickable;

	public float HorizontalDeadzone = 0.5f;

	public bool VerticalFlickable;

	public float VerticalDeadzone = 0.5f;

	private bool switchArmsCheck;

	private bool canSolveIk;

	public bool useGhostMode;

	private bool inventoryOpen;

	private bool createGhostMode;

	private bool pauseMenuMode;

	private float OriginHeight;

	private Vector3 watchParentLocalPos;

	private Quaternion watchParentLocalRot;

	private Vector3 watchParentLocalScale;

	private Vector3 shellLocalPos;

	private Quaternion shellLocalRot;

	private bool doingCrouch;

	public bool _startedMoving = true;

	[HideInInspector]
	public bool toggleCrouchFromVrHeight;

	public Transform[] spineBones;

	public Transform followTarget;

	public Transform headTransform;

	public float xClampValue;

	public float zClampValue;

	public float spineBendAmount = 0.75f;

	public float spineTwistAmount = 1f;

	public float fudgeOffsetCameraValue;

	private float heightResetTimer;

	private float distanceResetTimer;

	private float smoothTargetAngle;

	private float spineBendClamp;

	public float MovePlayerThreshold = 0.4f;

	public VRPlayerBlockedEffect BlockedEffect;

	private Vector3 velRef;

	private Vector3 smoothPosition;

	private Vector3 smoothClampedTargetPosition;

	private Vector2 lastAim = Vector2.zero;

	public bool RightHandedActive;
}
