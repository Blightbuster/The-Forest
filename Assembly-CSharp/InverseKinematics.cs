using System;
using UnityEngine;

[ExecuteInEditMode]
public class InverseKinematics : MonoBehaviour
{
	private void LateUpdate()
	{
		if (!Application.isPlaying && this.debug)
		{
			this.Solve();
		}
	}

	public void Solve()
	{
		if (this.upperArm == null || this.forearm == null || this.hand == null || this.elbow == null || this.target == null)
		{
			return;
		}
		if (!Application.isPlaying && this.debug)
		{
			this.upperArm.localRotation = Quaternion.identity;
			this.forearm.localRotation = Quaternion.identity;
			this.hand.localRotation = Quaternion.identity;
		}
		float d = 1f;
		if (this.RightSide)
		{
			d = -1f;
		}
		Vector3 forward = Vector3.Lerp(this.upperArm.up * d, this.target.position - this.upperArm.position, this.BlendOn);
		Vector3 upwards = Vector3.Lerp(this.upperArm.forward * d, this.elbow.position - this.upperArm.position, this.BlendOn);
		this.upperArm.rotation = Quaternion.LookRotation(forward, upwards);
		this.upperArm.Rotate(this.uppperArm_OffsetRotation);
		Vector3 b = Vector3.Cross(this.elbow.position - this.upperArm.position, this.forearm.position - this.upperArm.position);
		this.upperArm_Length = Vector3.Distance(this.upperArm.position, this.forearm.position);
		this.forearm_Length = Vector3.Distance(this.forearm.position, this.hand.position);
		this.arm_Length = this.upperArm_Length + this.forearm_Length;
		this.targetDistance = Vector3.Distance(this.upperArm.position, this.target.position);
		this.targetDistance = Mathf.Min(this.targetDistance, this.arm_Length - this.arm_Length * 0.001f);
		this.adyacent = (this.upperArm_Length * this.upperArm_Length - this.forearm_Length * this.forearm_Length + this.targetDistance * this.targetDistance) / (2f * this.targetDistance);
		this.angle = Mathf.Acos(Mathf.Clamp(this.adyacent / this.upperArm_Length, -1f, 1f)) * 57.29578f;
		Vector3 axis = Vector3.Lerp(this.forearm.position - this.upperArm.position, b, this.BlendOn);
		this.upperArm.RotateAround(this.upperArm.position, axis, -this.angle * this.BlendOn);
		if (this.debug)
		{
			Debug.DrawRay(this.forearm.position, this.forearm.up * d, Color.blue);
			Debug.DrawRay(this.forearm.position, -this.forearm.right, Color.green);
		}
		Vector3 forward2 = Vector3.Lerp(this.forearm.up * d, this.target.position - this.forearm.position, this.BlendOn);
		Vector3 upwards2 = Vector3.Lerp(-this.forearm.right, b, this.BlendOn);
		this.forearm.rotation = Quaternion.LookRotation(forward2, upwards2);
		this.forearm.Rotate(this.forearm_OffsetRotation);
		if (this.handMatchesTargetRotation)
		{
			Quaternion rotation = Quaternion.Lerp(this.hand.rotation, this.target.rotation, this.BlendOn);
			this.hand.rotation = rotation;
			this.hand.Rotate(this.hand_OffsetRotation * this.BlendOn);
			if (this.applyHandRotationLimits)
			{
				this.ApplyRotationLimits();
			}
		}
		if (this.handMatchesTargetTransform)
		{
			this.hand.position = this.target.position;
		}
		if (this.debug)
		{
			this.debugForearmEuler = this.forearm.localEulerAngles;
			this.debugForearmQuat = this.forearm.rotation;
			if (!(this.forearm != null) || this.elbow != null)
			{
			}
			if (!(this.upperArm != null) || this.target != null)
			{
			}
		}
	}

	private void ApplyRotationLimits()
	{
		Vector3 eulerAngles = this.hand.localRotation.eulerAngles;
		if (eulerAngles.x > 180f)
		{
			eulerAngles.x -= 360f;
		}
		eulerAngles.x = Mathf.Clamp(eulerAngles.x, -27f, 27f);
		if (eulerAngles.y > 180f)
		{
			eulerAngles.y -= 360f;
		}
		eulerAngles.y = Mathf.Clamp(eulerAngles.y, -125f, 125f);
		if (eulerAngles.z > 180f)
		{
			eulerAngles.z -= 360f;
		}
		eulerAngles.z = Mathf.Clamp(eulerAngles.z, -80f, 80f);
		this.hand.localEulerAngles = eulerAngles;
	}

	private void ApplyForearmRotationLimits()
	{
		Vector3 eulerAngles = this.forearm.localRotation.eulerAngles;
		if (eulerAngles.x > 180f)
		{
			eulerAngles.x -= 360f;
		}
		if (eulerAngles.y > 180f)
		{
			eulerAngles.y -= 360f;
		}
		if (eulerAngles.z > 180f)
		{
			eulerAngles.z -= 360f;
		}
		this.forearm.localEulerAngles = eulerAngles;
	}

	private void OnDrawGizmos()
	{
		if (this.debug && this.upperArm != null && this.elbow != null && this.hand != null && this.target != null && this.elbow != null)
		{
			return;
		}
	}

	public Transform upperArm;

	public Transform forearm;

	public Transform hand;

	public Transform elbow;

	public Transform target;

	[Space(20f)]
	public Vector3 uppperArm_OffsetRotation;

	public Vector3 forearm_OffsetRotation;

	public Vector3 hand_OffsetRotation;

	[Space(20f)]
	public bool handMatchesTargetRotation = true;

	public bool handMatchesTargetTransform;

	[Space(20f)]
	public bool debug;

	public Quaternion debugForearmQuat;

	public Vector3 debugForearmEuler;

	private float angle;

	private float upperArm_Length;

	private float forearm_Length;

	private float arm_Length;

	private float targetDistance;

	private float adyacent;

	public bool RightSide;

	public float BlendOn = 1f;

	public bool applyHandRotationLimits;
}
