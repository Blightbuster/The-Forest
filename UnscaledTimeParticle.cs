using System;
using UnityEngine;


public class UnscaledTimeParticle : MonoBehaviour
{
	
	private void Start()
	{
		this.pSystem = base.transform.GetComponent<ParticleSystem>();
	}

	
	private void Update()
	{
		if (Time.timeScale < 0.01f)
		{
			this.pSystem.Simulate(Time.unscaledDeltaTime, true, false);
		}
	}

	
	private ParticleSystem pSystem;
}
