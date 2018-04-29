using System;
using System.Collections.Generic;
using UnityEngine;


public class raftFoamController : MonoBehaviour
{
	
	private void OnEnable()
	{
		this.offTimer = Time.time + 3f;
		this.lastPos = base.transform.root.position;
	}

	
	private void Start()
	{
		foreach (ParticleSystem particleSystem in this.splashParticles)
		{
			this.splashEmit.Add(particleSystem.emissionRate);
		}
		foreach (ParticleSystem particleSystem2 in this.foamParticles)
		{
			this.foamEmit.Add(particleSystem2.emissionRate);
		}
		this._particles = new ParticleSystem.Particle[this.foamParticles.Length][];
		for (int k = 0; k < this.foamParticles.Length; k++)
		{
			this._particles[k] = new ParticleSystem.Particle[this.foamParticles[k].particleCount];
		}
		this.rootTr = base.transform.root;
	}

	
	private void LateUpdate()
	{
		if (!this.bcy.enabled)
		{
			this.bcy.enabled = true;
		}
		if (this.currVel < this.maxVelocity / 2f)
		{
			this.currVel = 0f;
		}
		float num = this.currVel / this.maxVelocity;
		if (this.currVel < this.maxVelocity / 2f && Time.time > this.offTimer && this.particlesGo.activeSelf)
		{
			this.particlesGo.SetActive(false);
		}
		else if (!this.particlesGo.activeSelf && this.bcy.InWater && this.bcy && this.currVel > this.maxVelocity / 2f)
		{
			this.particlesGo.SetActive(true);
		}
		else
		{
			this.offTimer = Time.time + 3f;
		}
		num = Mathf.Clamp(num, 0f, 1f);
		this.smoothDamp = Mathf.Lerp(this.smoothDamp, num, Time.deltaTime * 3f);
		for (int i = 0; i < this.splashParticles.Length; i++)
		{
			this.splashParticles[i].emissionRate = this.splashEmit[i] * num;
		}
		for (int j = 0; j < this.foamParticles.Length; j++)
		{
			this.foamParticles[j].emissionRate = this.foamEmit[j] * this.smoothDamp;
		}
		this.waterHeight = 0f;
		if (this.bcy.InWater && this.bcy.WaterCollider)
		{
			this.waterHeight = this.bcy.WaterCollider.bounds.center.y + this.bcy.WaterCollider.bounds.extents.y + 0.4f;
		}
		for (int k = 0; k < this.foamParticles.Length; k++)
		{
			ParticleSystem.Particle[] array = this._particles[k];
			int particles = this.foamParticles[k].GetParticles(array);
			for (int l = 0; l < particles; l++)
			{
				array[l].position = new Vector3(array[l].position.x, this.waterHeight, array[l].position.z);
			}
			this.foamParticles[k].SetParticles(array, particles);
		}
	}

	
	private void FixedUpdate()
	{
		this.currVel = (this.rootTr.position - this.lastPos).magnitude * 100f;
		this.lastPos = this.rootTr.position;
	}

	
	public GameObject particlesGo;

	
	public Buoyancy bcy;

	
	public ParticleSystem[] splashParticles;

	
	public ParticleSystem[] foamParticles;

	
	private List<float> splashEmit = new List<float>();

	
	private List<float> foamEmit = new List<float>();

	
	private Transform rootTr;

	
	private ParticleSystem.Particle[][] _particles;

	
	private Vector3 lastPos;

	
	public float currVel;

	
	private float smoothDamp;

	
	public float maxVelocity = 8f;

	
	public float waterHeight;

	
	private float offTimer;

	
	private bool doParticles;
}
