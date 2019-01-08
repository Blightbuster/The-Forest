using System;
using UnityEngine;

public class VROffsetTransform : MonoBehaviour
{
	private void Awake()
	{
		this.initialEulerRotation = base.transform.localEulerAngles;
		this.initialLocalPosition = base.transform.localPosition;
	}

	private void OnEnable()
	{
		if (!ForestVR.Enabled)
		{
			return;
		}
		base.transform.localEulerAngles = this.initialEulerRotation + this.rotationOffset;
		base.transform.localPosition = this.initialEulerRotation + this.rotationOffset;
	}

	private void Update()
	{
		if (!ForestVR.Enabled)
		{
			return;
		}
		base.transform.localEulerAngles = this.initialEulerRotation + this.rotationOffset;
		base.transform.localPosition = this.initialLocalPosition + this.positionOffset;
	}

	private void OnDisable()
	{
		if (!ForestVR.Enabled)
		{
			return;
		}
		base.transform.localEulerAngles = this.initialEulerRotation;
		base.transform.localPosition = this.initialLocalPosition;
	}

	public Vector3 rotationOffset;

	public Vector3 positionOffset;

	private Vector3 initialEulerRotation;

	private Vector3 initialLocalPosition;
}
