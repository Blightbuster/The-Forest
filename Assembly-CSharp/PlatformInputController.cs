using System;
using UnityEngine;

[RequireComponent(typeof(CharacterMotor))]
[AddComponentMenu("Character/Platform Input Controller")]
[Serializable]
public class PlatformInputController : MonoBehaviour
{
	public PlatformInputController()
	{
		this.autoRotate = true;
		this.maxRotationSpeed = 360f;
	}

	public virtual void Awake()
	{
		this.motor = (CharacterMotor)base.GetComponent(typeof(CharacterMotor));
	}

	public virtual void Update()
	{
		Vector3 vector = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0f);
		if (vector != Vector3.zero)
		{
			float num = vector.magnitude;
			vector /= num;
			num = Mathf.Min(1f, num);
			num *= num;
			vector *= num;
		}
		vector = Camera.main.transform.rotation * vector;
		Quaternion rotation = Quaternion.FromToRotation(-Camera.main.transform.forward, base.transform.up);
		vector = rotation * vector;
		this.motor.inputMoveDirection = vector;
		this.motor.inputJump = Input.GetButton("Jump");
		if (this.autoRotate && vector.sqrMagnitude > 0.01f)
		{
			Vector3 vector2 = this.ConstantSlerp(base.transform.forward, vector, this.maxRotationSpeed * Time.deltaTime);
			vector2 = this.ProjectOntoPlane(vector2, base.transform.up);
			base.transform.rotation = Quaternion.LookRotation(vector2, base.transform.up);
		}
	}

	public virtual Vector3 ProjectOntoPlane(Vector3 v, Vector3 normal)
	{
		return v - Vector3.Project(v, normal);
	}

	public virtual Vector3 ConstantSlerp(Vector3 from, Vector3 to, float angle)
	{
		float t = Mathf.Min(1f, angle / Vector3.Angle(from, to));
		return Vector3.Slerp(from, to, t);
	}

	public bool autoRotate;

	public float maxRotationSpeed;

	private CharacterMotor motor;
}
