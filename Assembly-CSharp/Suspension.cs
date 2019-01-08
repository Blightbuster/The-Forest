using System;
using UnityEngine;

public class Suspension : MonoBehaviour
{
	private void Start()
	{
		this.originalPosition = base.transform.localPosition;
	}

	private void Update()
	{
		base.transform.localPosition = this.originalPosition + this.wheel.suspensionSpringPos * base.transform.up;
	}

	public Wheel wheel;

	private Vector3 originalPosition;
}
