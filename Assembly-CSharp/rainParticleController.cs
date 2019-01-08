using System;
using TheForest.Utils;
using UnityEngine;

public class rainParticleController : MonoBehaviour
{
	private void Start()
	{
		if (CoopPeerStarter.DedicatedHost)
		{
			return;
		}
		this._rainParticles = base.transform.GetComponent<ParticleSystem>();
		LocalPlayer.MainRotator.enabled = false;
		LocalPlayer.MainRotator.resetOriginalRotation = true;
		LocalPlayer.MainRotator.enabled = true;
	}

	public void SetHighRain()
	{
		if (CoopPeerStarter.DedicatedHost)
		{
			return;
		}
		if (!this._rainParticles)
		{
			this._rainParticles = base.transform.GetComponent<ParticleSystem>();
		}
		ParticleSystem.MainModule main = this._rainParticles.main;
		main.duration = 2f;
		main.startLifetimeMultiplier = 2f;
		main.maxParticles = this._defaultMaxParticles * 3;
		this._rainParticles.shape.radius = 3f;
		ParticleSystem.CollisionModule collision = this._rainParticles.collision;
		if (LocalPlayer.ScriptSetup.targetInfo.inYacht)
		{
			collision.quality = ParticleSystemCollisionQuality.High;
		}
		else
		{
			collision.quality = ParticleSystemCollisionQuality.Medium;
		}
	}

	public void SetDefaultRain()
	{
		if (CoopPeerStarter.DedicatedHost)
		{
			return;
		}
		if (!this._rainParticles)
		{
			this._rainParticles = base.transform.GetComponent<ParticleSystem>();
		}
		ParticleSystem.MainModule main = this._rainParticles.main;
		main.duration = this._defaultDuration;
		main.startLifetimeMultiplier = 1f;
		main.maxParticles = this._defaultMaxParticles;
		this._rainParticles.shape.radius = this._defaultRadius;
		ParticleSystem.CollisionModule collision = this._rainParticles.collision;
		if (LocalPlayer.ScriptSetup.targetInfo.inYacht)
		{
			collision.quality = ParticleSystemCollisionQuality.High;
		}
		else
		{
			collision.quality = ParticleSystemCollisionQuality.Medium;
		}
	}

	public ParticleSystem _rainParticles;

	public float _defaultDuration = 1f;

	public float _defaultRadius = 2.5f;

	public int _defaultMaxParticles = 600;
}
