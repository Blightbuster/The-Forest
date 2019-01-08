using System;
using UnityEngine;

public class AutoDestructParticleSystems : MonoBehaviour
{
	private void Awake()
	{
		if (this.ChildParticleSystems)
		{
			this.allParticleSystems = base.GetComponentsInChildren<ParticleSystem>();
		}
	}

	private void LateUpdate()
	{
		if (this.ChildParticleSystems)
		{
			foreach (ParticleSystem particleSystem in this.allParticleSystems)
			{
				if (particleSystem.IsAlive())
				{
					return;
				}
			}
		}
		else if (base.GetComponent<ParticleSystem>().IsAlive())
		{
			return;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public bool ChildParticleSystems;

	private ParticleSystem[] allParticleSystems;
}
