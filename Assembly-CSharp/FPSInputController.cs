using System;
using UnityEngine;

[RequireComponent(typeof(CharacterMotor))]
[AddComponentMenu("Character/FPS Input Controller")]
[Serializable]
public class FPSInputController : MonoBehaviour
{
	public virtual void Awake()
	{
		this.motor = (CharacterMotor)base.GetComponent(typeof(CharacterMotor));
	}

	public virtual void Update()
	{
		Vector3 vector = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
		if (vector != Vector3.zero)
		{
			float num = vector.magnitude;
			vector /= num;
			num = Mathf.Min(1f, num);
			num *= num;
			vector *= num;
		}
		this.motor.inputMoveDirection = base.transform.rotation * vector;
		this.motor.inputJump = Input.GetButton("Jump");
	}

	private CharacterMotor motor;
}
