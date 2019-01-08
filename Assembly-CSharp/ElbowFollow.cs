using System;
using UnityEngine;

[ExecuteInEditMode]
public class ElbowFollow : MonoBehaviour
{
	private void LateUpdate()
	{
		if (!Application.isPlaying)
		{
			this.SolveElbow();
		}
	}

	public void SolveElbow()
	{
		this.OrientTransform.position = this.Shoulder.position;
		this.OrientTransform.LookAt(this.Hand.position, this.Shoulder.right);
		Vector3 vector = this.angleTargetTransform1.position - this.Shoulder.position;
		Vector3 from = this.angleTargetTransform2.position - this.Shoulder.position;
		Vector3 from2 = this.angleTargetTransform3.position - this.Shoulder.position;
		Vector3 from3 = this.angleTargetTransform4.position - this.Shoulder.position;
		Vector3 from4 = this.angleTargetTransform5.position - this.Shoulder.position;
		Debug.DrawRay(this.Shoulder.position, vector, Color.blue);
		Vector3 to = base.transform.position - this.Shoulder.position;
		float num = Vector3.Angle(vector, to);
		num /= 90f;
		num = Mathf.Clamp(num, 0f, 1f);
		float num2 = Vector3.Angle(from, to);
		num2 /= 90f;
		num2 = Mathf.Clamp(num2, 0f, 1f);
		float num3 = Vector3.Angle(from2, to);
		num3 /= 90f;
		num3 = Mathf.Clamp(num3, 0f, 1f);
		float num4 = Vector3.Angle(from3, to);
		num4 /= 90f;
		num4 = Mathf.Clamp(num4, 0f, 1f);
		float num5 = Vector3.Angle(from4, to);
		num5 /= 75f;
		num5 = Mathf.Clamp(num5, 0f, 1f);
		Vector3 localPosition = this.DefaultLocalPos + this.TargetAnglePos1 * (1f - num) + this.TargetAnglePos2 * (1f - num2) + this.TargetAnglePos3 * (1f - num3) + this.TargetAnglePos4 * (1f - num4) + this.TargetAnglePos5 * (1f - num5);
		this.elbow.localPosition = localPosition;
	}

	public Transform OrientTransform;

	public Transform Shoulder;

	public Transform Hand;

	public Transform elbow;

	public Transform angleTargetTransform1;

	public Transform angleTargetTransform2;

	public Transform angleTargetTransform3;

	public Transform angleTargetTransform4;

	public Transform angleTargetTransform5;

	public Vector3 TargetAnglePos1;

	public Vector3 TargetAnglePos2;

	public Vector3 TargetAnglePos3;

	public Vector3 TargetAnglePos4;

	public Vector3 TargetAnglePos5;

	public Vector3 DefaultLocalPos = new Vector3(0.44f, 0.27f, 0.3f);
}
