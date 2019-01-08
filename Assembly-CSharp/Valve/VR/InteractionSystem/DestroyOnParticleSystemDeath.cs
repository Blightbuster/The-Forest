using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	[RequireComponent(typeof(ParticleSystem))]
	public class DestroyOnParticleSystemDeath : MonoBehaviour
	{
		private void Awake()
		{
			this.particles = base.GetComponent<ParticleSystem>();
			base.InvokeRepeating("CheckParticleSystem", 0.1f, 0.1f);
		}

		private void CheckParticleSystem()
		{
			if (!this.particles.IsAlive())
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		private ParticleSystem particles;
	}
}
