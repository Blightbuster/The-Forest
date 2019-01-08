using System;
using UnityEngine;

public class LimitRotation : MonoBehaviour
{
	private void Awake()
	{
		if (this.m_RigidBody == null)
		{
			this.m_RigidBody = base.GetComponent<Rigidbody>();
		}
	}

	private void Update()
	{
		if (this.m_RigidBody == null)
		{
			UnityEngine.Object.Destroy(this);
		}
		else if (Vector3.Angle(this.m_RigidBody.transform.up, Vector3.up) >= this.m_MaxRotationAngle)
		{
			this.m_RigidBody.transform.eulerAngles = this.ClampRotation(this.m_RigidBody.rotation.eulerAngles);
			this.m_RigidBody.angularVelocity = default(Vector3);
		}
	}

	private Vector3 ClampRotation(Vector3 rotationAngle)
	{
		rotationAngle.x = Mathf.Clamp(rotationAngle.x, -this.m_MaxRotationAngle, this.m_MaxRotationAngle);
		rotationAngle.z = Mathf.Clamp(rotationAngle.z, -this.m_MaxRotationAngle, this.m_MaxRotationAngle);
		return rotationAngle;
	}

	[SerializeField]
	private float m_MaxRotationAngle = 45f;

	[SerializeField]
	private Rigidbody m_RigidBody;
}
