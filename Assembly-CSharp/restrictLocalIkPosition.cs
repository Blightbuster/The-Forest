using System;
using UnityEngine;

[ExecuteInEditMode]
public class restrictLocalIkPosition : MonoBehaviour
{
	private void LateUpdate()
	{
		if (!Application.isPlaying)
		{
			this.SolveLimits();
		}
	}

	public void SolveLimits()
	{
		float num = float.PositiveInfinity;
		Vector3 a = Vector3.zero;
		Vector3 vector = Vector3.zero;
		for (int i = 0; i < this.Blockers.Length; i++)
		{
			Vector3 vector2 = base.transform.position - this.Blockers[i].position;
			float magnitude = vector2.magnitude;
			if (magnitude < num)
			{
				num = magnitude;
				vector = vector2;
				a = this.Blockers[i].position;
			}
		}
		if (num < 0.6f)
		{
			base.transform.position = a + vector.normalized * 0.6f;
		}
		Vector3 vector3 = this.LimitPosTr1.position - this.Shoulder.position;
		Vector3 vector4 = base.transform.position - this.Shoulder.position;
		this.armDistance = vector4.magnitude;
		if (this.armDistance > this.MaxArmDistance)
		{
			base.transform.position = this.Shoulder.position + vector4.normalized * this.MaxArmDistance;
		}
	}

	public Transform[] Blockers;

	public Transform LimitPosTr1;

	public Transform Shoulder;

	public float MaxArmDistance = 1.34f;

	private float limitAngle;

	private float armDistance;

	public bool RightHand;
}
