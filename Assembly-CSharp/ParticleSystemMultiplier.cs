using System;
using UnityEngine;

public class ParticleSystemMultiplier : MonoBehaviour
{
	private void Start()
	{
		ParticleSystem[] componentsInChildren = base.GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particleSystem in componentsInChildren)
		{
			particleSystem.startSize *= this.multiplier;
			particleSystem.startSpeed *= this.multiplier;
			particleSystem.startLifetime *= Mathf.Lerp(this.multiplier, 1f, 0.5f);
			particleSystem.Clear();
			particleSystem.Play();
		}
	}

	public float multiplier = 1f;
}
