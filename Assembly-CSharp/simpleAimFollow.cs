using System;
using UnityEngine;
using Valve.VR.InteractionSystem;

[ExecuteInEditMode]
public class simpleAimFollow : MonoBehaviour
{
	private void Update()
	{
		if (!this.execute)
		{
			return;
		}
		base.transform.SetPositionAndRotation(this.BowFollow.position, this.BowFollow.rotation);
		Vector3 forward = this.AimRoot.position - this.AimTarget.position;
		this.AimRoot.rotation = Quaternion.LookRotation(forward, this.HandTransform.up);
		this.localNockPos = this.nockRestTransform.InverseTransformPoint(this.AimTarget.position);
		float num = -this.localNockPos.z;
		float value = Util.RemapNumberClamped(num, this.minPull, this.maxPull, 0f, 1f);
		this.bowDrawLinearMapping.value = value;
	}

	public Transform HandTransform;

	public Transform AimRoot;

	public Transform BowFollow;

	public Transform nockRestTransform;

	public LinearMapping bowDrawLinearMapping;

	public Transform AimTarget;

	public float minPull;

	public float maxPull = 3f;

	public bool execute;

	public Vector3 localNockPos;
}
