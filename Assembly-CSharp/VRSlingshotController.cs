using System;
using TheForest.Utils;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class VRSlingshotController : MonoBehaviour
{
	private void Awake()
	{
		this.restRotation = this.pivotTransform.localRotation;
		this.lerpStartRotation = this.pivotTransform.rotation;
	}

	private void Start()
	{
		if (!ForestVR.Enabled)
		{
			base.enabled = false;
		}
	}

	private void OnDisable()
	{
		this.controller._slingAnimator.SetBool("attack", false);
		this.controller._slingAnimator.SetBool("toAimVR", false);
		LocalPlayer.AnimControl.slingShotAim = false;
	}

	private void OnHandHoverBegin(Hand hand)
	{
		if (hand.startingHandType == Hand.HandType.Left || (PlayerPreferences.VRUseRightHandedBow && hand.startingHandType == Hand.HandType.Right))
		{
			this.slingShotFocus = true;
		}
	}

	private void OnHandHoverEnd(Hand hand)
	{
		if (hand.startingHandType == Hand.HandType.Left || (PlayerPreferences.VRUseRightHandedBow && hand.startingHandType == Hand.HandType.Right))
		{
			this.slingShotFocus = false;
		}
	}

	private void LateUpdate()
	{
		Hand leftHand = LocalPlayer.vrPlayerControl.LeftHand;
		Hand rightHand = LocalPlayer.vrPlayerControl.RightHand;
		if (ForestVR.Enabled)
		{
			if (PlayerPreferences.VRUseRightHandedBow)
			{
				this.controller._slingAnimator.SetFloat("leftHand", 0f);
			}
			else
			{
				this.controller._slingAnimator.SetFloat("leftHand", 0f);
			}
		}
		if (LocalPlayer.vrPlayerControl.LeftHand.GetStandardInteractionButton() && this.slingShotFocus)
		{
			this.slingShotAim = true;
			LocalPlayer.AnimControl.slingShotAim = true;
		}
		if (this.slingShotAim && !LocalPlayer.vrPlayerControl.LeftHand.GetStandardInteractionButton())
		{
			this.slingShotAim = false;
			LocalPlayer.AnimControl.slingShotAim = false;
		}
		if (this.slingShotAim)
		{
			this.pullDist = Util.RemapNumberClamped((LocalPlayer.vrPlayerControl.LeftHand.transform.position - this.pullRestTransform.position).magnitude, 0f, this.maxPull, 0f, 1f);
			float magnitude = (LocalPlayer.vrPlayerControl.LeftHand.transform.position - this.pivotTransform.position).magnitude;
			if (magnitude > this.minAttachDist)
			{
				this.controller._slingAnimator.SetBool("toAimVR", true);
				this.slingloaded = true;
				Vector3 vector = LocalPlayer.vrPlayerControl.LeftHand.transform.position;
				vector = this.pivotTransform.InverseTransformPoint(vector);
				vector.y -= this.aimDirOffset;
				vector = this.pivotTransform.TransformPoint(vector);
				Vector3 normalized = (vector - this.pivotTransform.position).normalized;
				Vector3 normalized2 = (this.handleTransform.position - this.pivotTransform.position).normalized;
				float d = 1f;
				if (LocalPlayer.vrPlayerControl.RightHandedActive)
				{
					d = -1f;
				}
				this.slingLeftVector = -Vector3.Cross(normalized2, normalized * d);
				this.pivotTransform.rotation = Quaternion.LookRotation(-normalized, this.slingLeftVector);
				float t = Util.RemapNumberClamped(Time.time, this.nockLerpStartTime, this.nockLerpStartTime + this.lerpDuration, 0f, 1f);
				this.pivotTransform.rotation = Quaternion.Lerp(this.LerpStartRotation, Quaternion.LookRotation(-normalized, this.slingLeftVector), t);
				this.controller._slingAnimator.Play(0, 0, this.pullDist);
			}
			else
			{
				this.controller._slingAnimator.SetBool("toAimVR", false);
				this.pivotTransform.localRotation = this.restRotation;
			}
		}
		else
		{
			if (this.pullDist > 0.1f && this.slingloaded)
			{
				this.controller.Invoke("fireProjectile", 0.15f);
				this.controller._slingAnimator.SetBool("attack", true);
				LocalPlayer.Sfx.PlayBowSnap();
				LocalPlayer.TargetFunctions.sendPlayerAttacking();
				base.Invoke("resetAttack", 0.3f);
				this.slingloaded = false;
			}
			this.controller._slingAnimator.SetBool("toAimVR", false);
		}
	}

	private void resetAttack()
	{
		this.controller._slingAnimator.SetBool("attack", false);
		this.pivotTransform.localRotation = this.restRotation;
	}

	public Transform pullRestTransform;

	public Transform aimDirTransform;

	public Transform pivotTransform;

	public Transform handleTransform;

	public Transform rootTransform;

	private Vector3 slingLeftVector;

	private float lerpStartTime;

	private float lerpDuration = 0.15f;

	private Quaternion lerpStartRotation;

	private float nockLerpStartTime;

	private Quaternion LerpStartRotation;

	private Hand hand;

	public slingShotController controller;

	public bool slingShotFocus;

	public bool slingShotAim;

	public bool slingloaded;

	private float pullDist;

	private Quaternion restRotation;

	public float maxPull;

	public float aimDirOffset = 0.4f;

	public float minAttachDist = 0.5f;

	private bool deferNewPoses;

	private Vector3 lateUpdatePos;

	private Quaternion lateUpdateRot;

	public float mirrorLook1 = 1f;

	public float mirrorLook2 = 1f;

	private SteamVR_Events.Action newPosesAppliedAction;
}
