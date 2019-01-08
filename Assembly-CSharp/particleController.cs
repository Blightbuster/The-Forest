using System;
using UnityEngine;

public class particleController : MonoBehaviour
{
	private void Start()
	{
		this.p = base.transform.GetComponent<ParticleSystem>();
		this.velocityTarget = base.transform;
		this.lastPos = this.velocityTarget.position;
		this.particles = new ParticleSystem.Particle[this.p.particleCount];
	}

	private void OnEnable()
	{
		this.velocityTarget = base.transform;
		this.lastPos = this.velocityTarget.position;
	}

	private void LateUpdate()
	{
		if (this.p == null)
		{
			return;
		}
		if (this.particles == null || this.particles.Length < this.p.particleCount)
		{
			this.particles = new ParticleSystem.Particle[(int)((float)this.p.particleCount * 1.25f)];
		}
		if (this.applyLocalDrift)
		{
			Vector3 vector = (this.velocityTarget.position - this.lastPos) * -1f;
			vector = this.velocityTarget.InverseTransformDirection(vector);
			int num = this.p.GetParticles(this.particles);
			for (int i = 0; i < num; i++)
			{
				ParticleSystem.Particle[] array = this.particles;
				int num2 = i;
				array[num2].velocity = array[num2].velocity + vector * this.driftAmount;
			}
			this.p.SetParticles(this.particles, num);
			this.lastPos = this.velocityTarget.position;
		}
	}

	public bool applyLocalDrift = true;

	public float driftAmount = 1f;

	private Transform velocityTarget;

	private Vector3 lastPos;

	private ParticleSystem p;

	private ParticleSystem.Particle[] particles;
}
