using System;
using UnityEngine;

public class followWeaponFire : MonoBehaviour
{
	private void Awake()
	{
		this.tr = base.transform;
		this.startX = this.tr.localPosition.x;
	}

	private void OnEnable()
	{
		this.pos = this.driftReferenceTarget.transform.position;
	}

	private void LateUpdate()
	{
		this.pos = Vector3.Slerp(this.pos, this.driftReferenceTarget.position + this.driftReferenceTarget.transform.right * -this.driftYOffset, Time.deltaTime * this.smoothDrift);
		this.driftFollowTarget.position = this.pos;
		Vector3 b = this.driftFollowTarget.position - this.pos;
		if (b.magnitude > this.followDistance)
		{
			this.driftFollowTarget.position = this.pos + b;
		}
		Vector3 localPosition = Vector3.Slerp(this.tr.localPosition, this.driftFollowTarget.localPosition, Time.deltaTime * this.smoothTime);
		if (localPosition.y > this.maxY)
		{
			localPosition.y = this.maxY;
		}
		else if (localPosition.y < this.minY)
		{
			localPosition.y = this.minY;
		}
		this.tr.localPosition = localPosition;
	}

	public Transform driftFollowTarget;

	public Transform driftReferenceTarget;

	private Transform tr;

	private Vector3 pos;

	public float smoothTime = 15f;

	public float followDistance = 0.4f;

	public float smoothDrift = 12f;

	public float driftYOffset = 0.6f;

	public float minY;

	public float maxY;

	private float startX;

	public bool net;
}
