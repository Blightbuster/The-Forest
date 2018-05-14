using System;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;
using Valve.VR.InteractionSystem;


public class VRPlayerControl : MonoBehaviour
{
	
	private void Start()
	{
		this.ikSolve = LocalPlayer.PlayerBase.GetComponents<simpleIkSolver>();
		if (ForestVR.Enabled)
		{
			foreach (simpleIkSolver simpleIkSolver in this.ikSolve)
			{
				simpleIkSolver.enabled = true;
			}
			this.canSolveIk = true;
			this.VRLighterHeldGo.SetActive(false);
			this.updateCameraCentre();
		}
	}

	
	private void Update()
	{
		if (ForestVR.Enabled)
		{
			this.UpdateFlick();
			this.UpdateDebugMenu();
			this.updateCameraCentre();
		}
	}

	
	private void LateUpdate()
	{
		if (ForestVR.Enabled)
		{
			this.updateSpineRotation();
			this.canSolveIk = !LocalPlayer.vrPlayerControl.useGhostMode;
			if (this.canSolveIk)
			{
				for (int i = 0; i < this.ikSolve.Length; i++)
				{
					this.ikSolve[i].Solve();
				}
			}
		}
	}

	
	private void updateBodyVisibility()
	{
	}

	
	private void updateCameraCentre()
	{
		if (LocalPlayer.vrPlayerControl.useGhostMode)
		{
			return;
		}
		Transform transform = this.VRCameraOrigin;
		float num = 1f;
		float num2 = 0.5f;
		if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.PlaneCrash)
		{
			transform = Scene.TriggerCutScene.playerSeatPosVR;
			num = 1f;
		}
		float num3 = Vector3.Distance(this.VRCamera.position, transform.position);
		float num4 = this.VRCamera.position.y - transform.position.y;
		if (num3 > num)
		{
			if (Time.time > this.distanceResetTimer)
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
			transform = Scene.TriggerCutScene.playerSeatPosVR;
		}
		Vector3 b = this.VRCamera.position - transform.position;
		this.VRCameraRig.position = this.VRCameraRig.position - b;
		if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.PlaneCrash)
		{
			this.VRCameraRig.rotation = Scene.TriggerCutScene.playerSeatPosVR.rotation;
			this.VRCameraRig.localEulerAngles = new Vector3(0f, this.VRCameraRig.localEulerAngles.y, 0f);
		}
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
		vector.x = Mathf.Clamp(vector.x, -this.xClampValue, this.xClampValue) * this.spineBendAmount / 2f;
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
		if (this.HorizontalFlickable && Mathf.Abs(vector.x) >= this.HorizontalDeadzone && Mathf.Abs(this.lastAim.x) < this.HorizontalDeadzone)
		{
			float angle = Mathf.Sign(vector.x) * this.FlickRotateDegrees;
			this.RotatableTransform.Rotate(Vector3.up, angle);
		}
		if (this.VerticalFlickable && Mathf.Abs(vector.y) >= this.VerticalDeadzone && Mathf.Abs(this.lastAim.y) < this.VerticalDeadzone)
		{
			this.RotatableTransform.Rotate(Vector3.up, 180f);
		}
		this.lastAim = vector;
	}

	
	private void UpdateDebugMenu()
	{
	}

	
	private simpleIkSolver[] ikSolve;

	
	public GameObject VRLighterHeldGo;

	
	public GameObject playerLighterHeldGo;

	
	public GameObject leftHandHeldGo;

	
	public SkinnedMeshRenderer targetArmsRenderer;

	
	public SkinnedMeshRenderer VRArms;

	
	public SkinnedMeshRenderer DefaultArms;

	
	public Transform RotatableTransform;

	
	public Transform VRCamera;

	
	public Transform VRCameraOrigin;

	
	public Transform VROffsetTransform;

	
	public Transform VRCameraRig;

	
	public Hand LeftHand;

	
	public Hand RightHand;

	
	public bool HorizontalFlickable;

	
	public float HorizontalDeadzone = 0.5f;

	
	public float FlickRotateDegrees;

	
	public bool VerticalFlickable;

	
	public float VerticalDeadzone = 0.5f;

	
	private bool switchArmsCheck;

	
	private bool canSolveIk;

	
	public bool useGhostMode;

	
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

	
	private Vector3 velRef;

	
	private Vector3 smoothPosition;

	
	private Vector3 smoothClampedTargetPosition;

	
	private Vector2 lastAim = Vector2.zero;
}
