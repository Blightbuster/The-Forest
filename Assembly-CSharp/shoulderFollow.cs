using System;
using UnityEngine;

[ExecuteInEditMode]
public class shoulderFollow : MonoBehaviour
{
	private void Start()
	{
	}

	private void LateUpdate()
	{
		if (!Application.isPlaying)
		{
			this.SolveShoulder();
		}
	}

	public void SolveShoulder()
	{
		if (!Application.isPlaying)
		{
			this.shoulder.localEulerAngles = this.restTransform.localEulerAngles;
		}
		this.restTransform.localEulerAngles = this.restLocalRotation;
		Vector3 localEulerAngles = this.restTransform.localEulerAngles;
		this.localPos = this.restTransform.InverseTransformPoint(this.followTarget.position);
		if (this.localPos.x < this.minX)
		{
			this.localPos.x = this.minX;
		}
		if (this.localPos.x > this.maxX)
		{
			this.localPos.x = this.maxX;
		}
		if (this.localPos.z < this.minZ)
		{
			this.localPos.z = this.minZ;
		}
		if (this.localPos.z > this.maxZ)
		{
			this.localPos.z = this.maxZ;
		}
		if (this.RightSide)
		{
			if (this.localPos.y < 0f)
			{
				this.localPos.y = 0f;
			}
		}
		else if (this.localPos.y > 0f)
		{
			this.localPos.y = 0f;
		}
		localEulerAngles.z += this.localPos.x * this.multiplyZ * this.blendAmount;
		localEulerAngles.y = localEulerAngles.y + this.localPos.z * this.multiplyX * this.blendAmount + this.localPos.y * this.multiplyY * this.blendAmount;
		this.autoShoulder.localEulerAngles = localEulerAngles;
		Vector3 forward = Vector3.Lerp(this.shoulder.forward, this.autoShoulder.forward, this.blendAmount);
		Vector3 upwards = Vector3.Lerp(this.shoulder.up, this.autoShoulder.up, this.blendAmount);
		this.shoulder.rotation = Quaternion.LookRotation(forward, upwards);
	}

	public Transform restTransform;

	public Transform autoShoulder;

	public Transform shoulder;

	public Transform followTarget;

	public Transform helperJoint1;

	public float minZ;

	public float maxZ;

	public float minX;

	public float maxX;

	public float multiplyZ;

	public float multiplyX;

	public float multiplyY;

	public Vector3 restLocalRotation;

	public Vector3 localPos;

	private Vector3 startLocalRot;

	public bool RightSide;

	public float blendAmount = 1f;

	public bool DebugEditor;
}
