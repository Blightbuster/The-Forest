using System;
using UnityEngine;

[ExecuteInEditMode]
public class twistFollow : MonoBehaviour
{
	private void Start()
	{
	}

	private void LateUpdate()
	{
		for (int i = 0; i < this.twistJoints.Length; i++)
		{
			Vector3 localEulerAngles = this.twistJoints[i].localEulerAngles;
			Vector3 localEulerAngles2 = this.wristTarget.localEulerAngles;
			if (localEulerAngles2.y > 180f)
			{
				localEulerAngles2.y -= 360f;
			}
			localEulerAngles.y = localEulerAngles2.y * 0.33f * this.blendAmount + this.TwistOffset * this.blendAmount + localEulerAngles2.y * (1f - this.blendAmount);
			this.twistJoints[i].localEulerAngles = localEulerAngles;
		}
	}

	public Transform[] twistJoints;

	public Transform wristTarget;

	public float TwistOffset;

	public float blendAmount = 1f;
}
