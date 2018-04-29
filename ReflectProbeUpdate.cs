using System;
using UnityEngine;


public class ReflectProbeUpdate : MonoBehaviour
{
	
	private void Start()
	{
		base.InvokeRepeating("UpdateProbe", 1f, 10f);
	}

	
	private void UpdateProbe()
	{
		this.Reflecty.RenderProbe();
	}

	
	public ReflectionProbe Reflecty;
}
