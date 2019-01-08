using System;
using UnityEngine;

public class CausticLightIntensity : MonoBehaviour
{
	private void Awake()
	{
		this.MyIntensity = base.gameObject.GetComponent<Light>();
		this.MyIntensity.intensity = 0f;
	}

	private void Update()
	{
		this.MyIntensity.intensity = TheForestAtmosphere.ReflectionAmount;
	}

	private Light MyIntensity;
}
