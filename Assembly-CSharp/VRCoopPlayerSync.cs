using System;
using Bolt;
using TheForest.Utils;
using UnityEngine;

[ExecuteInEditMode]
public class VRCoopPlayerSync : EntityBehaviour<IPlayerState>
{
	private void Start()
	{
		this.ikSolvers = base.transform.GetComponents<InverseKinematics>();
		if (!Application.isPlaying && this.DebugHost)
		{
			this.UpdateSpineRotation();
		}
	}

	public override void Attached()
	{
		if (base.entity != null && base.entity.isOwner && !ForestVR.Enabled)
		{
			return;
		}
		base.state.HeadVR.SetTransforms(this.TrackedHead);
		base.state.LeftHandVR.SetTransforms(this.TrackedLeftHand);
		base.state.RightHandVR.SetTransforms(this.TrackedRightHand);
	}

	private void LateUpdate()
	{
		if (BoltNetwork.isRunning)
		{
			if (base.entity == null || !base.entity.isAttached)
			{
				return;
			}
			if (base.entity.isOwner)
			{
				if (ForestVR.Enabled)
				{
					this.UpdateOwnerPositions();
					this.UpdateIkRigPosition();
					if (this.DisableTrackingFromMinMovement())
					{
						LocalPlayer.vrAdapter.TheatreController.ghostFromIkDisable = true;
					}
					else
					{
						LocalPlayer.vrAdapter.TheatreController.ghostFromIkDisable = false;
					}
				}
			}
			else
			{
				if (!this.Stats.VREnabled)
				{
					return;
				}
				this.allowArmBlend = !this.NetAnimSetup.DisableVrTrackedArms();
				if (this._animator.GetBool("ghostMode") || this.NetAnimSetup.DisableVrAnimSync())
				{
					this.allowArmBlend = false;
					this.allowHeadBlend = false;
				}
				else
				{
					this.allowHeadBlend = true;
				}
				this.UpdateSpineRotation();
				this.UpdateClientPositions();
				this.UpdateClientRigPosition();
				this.blendIkValue = Mathf.Lerp(this.blendIkValue, (float)((!this.allowArmBlend) ? 0 : 1), Time.deltaTime * 3f);
				this.blendHeadValue = Mathf.Lerp(this.blendHeadValue, (float)((!this.allowHeadBlend) ? 0 : 1), Time.deltaTime * 3f);
				if (this.shoulders.Length > 0)
				{
					for (int i = 0; i < this.shoulders.Length; i++)
					{
						this.shoulders[i].SolveShoulder();
						this.shoulders[i].blendAmount = this.blendIkValue;
					}
				}
				if (this.ikLimits.Length > 0)
				{
					for (int j = 0; j < this.ikLimits.Length; j++)
					{
						this.ikLimits[j].SolveLimits();
					}
				}
				if (this.elbows.Length > 0)
				{
					for (int k = 0; k < this.elbows.Length; k++)
					{
						this.elbows[k].SolveElbow();
					}
				}
				for (int l = 0; l < this.ikSolvers.Length; l++)
				{
					this.ikSolvers[l].Solve();
					this.ikSolvers[l].BlendOn = this.blendIkValue;
				}
				this.NetAnimSetup.doVrBowSync();
			}
		}
		if (this.DebugClient && !Application.isPlaying)
		{
			this.UpdateSpineRotation();
			this.UpdateClientPositions();
		}
	}

	private void UpdateIkRigPosition()
	{
		Vector3 position = this.IkRigTransform.transform.position;
		position = (LocalPlayer.vrPlayerControl.VRCamera.position + this.IkRigTransform.position) / 2f;
		position.y = this.IkRigTransform.position.y;
		this.IkRigOffsetTransform.position = position;
		Vector3 localPosition = this.IkRigHeadOffsetTransform.parent.InverseTransformPoint(LocalPlayer.vrAdapter.VREyeCamera.transform.position);
		localPosition.x = 0f;
		localPosition.z = 0f;
		this.IkRigHeadOffsetTransform.localPosition = localPosition;
		Vector3 eulerAngles = this.TrackedHead.localRotation.eulerAngles;
		if (eulerAngles.y > 180f)
		{
			eulerAngles.y -= 360f;
		}
		float num = eulerAngles.y / 90f;
		num = Mathf.Clamp(num, 0f, 1f);
		eulerAngles.x = 0f;
		eulerAngles.z = 0f;
		eulerAngles.y /= 1.5f;
		this.IkRigOffsetTransform.localEulerAngles = eulerAngles * num;
	}

	private void UpdateClientRigPosition()
	{
		Vector3 localPosition = this.IkRigHeadOffsetTransform.parent.InverseTransformPoint(this.NetHeadJoint.position);
		localPosition.x = 0f;
		localPosition.z = 0f;
		this.IkRigHeadOffsetTransform.localPosition = localPosition;
	}

	private void UpdateOwnerPositions()
	{
		this.TrackedHead.SetPositionAndRotation(this.RigHead.position, this.RigHead.rotation);
		if (LocalPlayer.vrPlayerControl.RightHandedActive)
		{
			this.TrackedLeftHand.SetPositionAndRotation(this.RigRightHand.position, this.RigRightHand.rotation);
			this.TrackedRightHand.SetPositionAndRotation(this.RigLeftHand.position, this.RigLeftHand.rotation);
			Vector3 vector = this.TrackedLeftHand.localEulerAngles;
			vector.x += this.mirrOffset.x;
			vector.y += this.mirrOffset.y;
			vector.z += this.mirrOffset.z;
			vector.x *= this.mirrorMult.x;
			vector.z *= this.mirrorMult.y;
			vector.z *= this.mirrorMult.z;
			this.TrackedLeftHand.localEulerAngles = vector;
			vector = this.TrackedRightHand.localEulerAngles;
			vector += this.mirrOffset;
			vector.x *= this.mirrorMult.x;
			vector.z *= this.mirrorMult.y;
			vector.z *= this.mirrorMult.z;
			this.TrackedRightHand.localEulerAngles = vector;
		}
		else
		{
			this.TrackedLeftHand.SetPositionAndRotation(this.RigLeftHand.position, this.RigLeftHand.rotation);
			this.TrackedRightHand.SetPositionAndRotation(this.RigRightHand.position, this.RigRightHand.rotation);
		}
	}

	private void UpdateClientPositions()
	{
		Quaternion rotation = Quaternion.Lerp(this.RigHead.rotation, this.TrackedHead.rotation, this.blendHeadValue);
		this.RigHead.rotation = rotation;
		Vector3 eulerAngles = this.RigHead.localRotation.eulerAngles;
		if (eulerAngles.x > 180f)
		{
			eulerAngles.x -= 360f;
		}
		eulerAngles.x = Mathf.Clamp(eulerAngles.x, -50f, 25f);
		if (eulerAngles.y > 180f)
		{
			eulerAngles.y -= 360f;
		}
		eulerAngles.y = Mathf.Clamp(eulerAngles.y, -60f, 60f);
		if (eulerAngles.z > 180f)
		{
			eulerAngles.z -= 360f;
		}
		eulerAngles.z = Mathf.Clamp(eulerAngles.z, -25f, 25f);
		this.RigHead.localEulerAngles = eulerAngles;
	}

	private void UpdateSpineRotation()
	{
		if (Application.isPlaying && LocalPlayer.vrPlayerControl.useGhostMode)
		{
			return;
		}
		Vector3 localPosition = this.RigHead.localPosition;
		Vector3 eulerAngles = this.TrackedHead.localRotation.eulerAngles;
		if (eulerAngles.x > 180f)
		{
			eulerAngles.x -= 360f;
		}
		if (eulerAngles.z > 180f)
		{
			eulerAngles.z -= 360f;
		}
		localPosition.x = Mathf.Clamp(eulerAngles.z * -0.01f * this.spineBendAmount, -this.xClampValue, this.xClampValue);
		localPosition.z = Mathf.Clamp(eulerAngles.x * 0.025f * this.spineBendAmount, -this.zClampValue, this.zClampValue);
		localPosition.y = 1.65f;
		this.smoothSpinePosition = Vector3.SmoothDamp(this.smoothSpinePosition, localPosition, ref this.velRef, 0.025f);
		Vector3 a = base.transform.TransformPoint(this.smoothSpinePosition);
		Vector3 vector = Vector3.Lerp(this.spineBones[1].position - this.spineBones[0].position, a - this.spineBones[0].position, this.blendHeadValue);
		Quaternion a2 = Quaternion.LookRotation(this.spineBones[0].forward, this.spineBones[0].up);
		Quaternion rotation = Quaternion.Lerp(a2, Quaternion.LookRotation(vector, base.transform.forward) * Quaternion.Euler(-90f, 90f, 90f), this.blendHeadValue);
		this.spineBones[0].rotation = rotation;
		Vector3 vector2 = this.TrackedHead.forward;
		vector2.y = 0f;
		vector2 = vector2.normalized;
		float num = Vector3.Angle(vector2, base.transform.forward);
		if (Vector3.Cross(vector2, base.transform.forward).y > 0f)
		{
			num = -num;
		}
		num /= 160f * this.spineTwistAmount;
		num = Mathf.Clamp(num, -0.75f, 0.75f);
		this.smoothTargetAngle = Mathf.Lerp(this.smoothTargetAngle, num, Time.deltaTime * 13f) * this.blendHeadValue;
		this.spineBones[0].transform.RotateAround(vector, this.smoothTargetAngle);
	}

	private bool DisableTrackingFromMinMovement()
	{
		float num = LocalPlayer.vrPlayerControl.LeftHand.GetTrackedObjectAngularVelocity().magnitude + LocalPlayer.vrPlayerControl.RightHand.GetTrackedObjectAngularVelocity().magnitude + LocalPlayer.vrPlayerControl.LeftHand.GetTrackedObjectVelocity().magnitude + LocalPlayer.vrPlayerControl.RightHand.GetTrackedObjectVelocity().magnitude;
		if (num < 0.2f)
		{
			if (Time.time > this.disableTrackingTimer)
			{
				return true;
			}
		}
		else
		{
			this.disableTrackingTimer = Time.time + 2f;
		}
		return false;
	}

	public Animator _animator;

	public targetStats Stats;

	public netAnimatorSetup NetAnimSetup;

	public Transform NetHeadJoint;

	public Transform IkRigTransform;

	public Transform IkRigOffsetTransform;

	public Transform IkRigHeadOffsetTransform;

	public Transform RigHead;

	public Transform RigLeftHand;

	public Transform RigRightHand;

	public Transform TrackedHead;

	public Transform TrackedLeftHand;

	public Transform TrackedRightHand;

	public Transform[] spineBones;

	private InverseKinematics[] ikSolvers;

	public shoulderFollow[] shoulders;

	public ElbowFollow[] elbows;

	public restrictLocalIkPosition[] ikLimits;

	public float xClampValue;

	public float zClampValue;

	public float spineBendAmount = 0.75f;

	public float spineTwistAmount = 1f;

	private Vector3 smoothSpinePosition;

	private Vector3 velRef;

	private float smoothTargetAngle;

	public Vector3 mirrOffset;

	public Vector3 mirrorMult = new Vector3(1f, 1f, 1f);

	private bool allowArmBlend;

	private bool allowHeadBlend;

	public float blendIkValue;

	public float blendHeadValue;

	public bool DebugHost;

	public bool DebugClient;

	public bool NetPlayer;

	private float disableTrackingTimer;
}
