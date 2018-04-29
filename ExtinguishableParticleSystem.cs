using System;
using UnityEngine;


public class ExtinguishableParticleSystem : MonoBehaviour
{
	
	private void Start()
	{
		this.systems = base.GetComponentsInChildren<ParticleSystem>();
	}

	
	public void Extinguish()
	{
		foreach (ParticleSystem particleSystem in this.systems)
		{
			particleSystem.enableEmission = false;
		}
	}

	
	public float multiplier = 1f;

	
	private ParticleSystem[] systems;
}
