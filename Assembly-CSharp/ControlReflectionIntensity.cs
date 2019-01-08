using System;
using UnityEngine;

public class ControlReflectionIntensity : MonoBehaviour
{
	private void Start()
	{
		this.MyIntensity = base.gameObject.GetComponent<ReflectionProbe>();
	}

	private void LateUpdate()
	{
		this.MyIntensity.intensity = TheForestAtmosphere.ReflectionAmount;
	}

	private ReflectionProbe MyIntensity;
}
