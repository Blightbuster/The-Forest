using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	
	public class Balloon : MonoBehaviour
	{
		
		private void Start()
		{
			this.destructTime = Time.time + this.lifetime + UnityEngine.Random.value;
			this.hand = base.GetComponentInParent<Hand>();
			this.balloonRigidbody = base.GetComponent<Rigidbody>();
		}

		
		private void Update()
		{
			if (this.destructTime != 0f && Time.time > this.destructTime)
			{
				if (this.burstOnLifetimeEnd)
				{
					this.SpawnParticles(this.lifetimeEndParticlePrefab, this.lifetimeEndSound);
				}
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		
		private void SpawnParticles(GameObject particlePrefab, SoundPlayOneshot sound)
		{
			if (this.bParticlesSpawned)
			{
				return;
			}
			this.bParticlesSpawned = true;
			if (particlePrefab != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(particlePrefab, base.transform.position, base.transform.rotation);
				gameObject.GetComponent<ParticleSystem>().Play();
				UnityEngine.Object.Destroy(gameObject, 2f);
			}
			if (sound != null)
			{
				float num = Time.time - Balloon.s_flLastDeathSound;
				if (num < 0.1f)
				{
					sound.volMax *= 0.25f;
					sound.volMin *= 0.25f;
				}
				sound.Play();
				Balloon.s_flLastDeathSound = Time.time;
			}
		}

		
		private void FixedUpdate()
		{
			if (this.balloonRigidbody.velocity.sqrMagnitude > this.maxVelocity)
			{
				this.balloonRigidbody.velocity *= 0.97f;
			}
		}

		
		private void ApplyDamage()
		{
			this.SpawnParticles(this.popPrefab, null);
			UnityEngine.Object.Destroy(base.gameObject);
		}

		
		private void OnCollisionEnter(Collision collision)
		{
			if (this.bParticlesSpawned)
			{
				return;
			}
			Hand x = null;
			BalloonHapticBump component = collision.gameObject.GetComponent<BalloonHapticBump>();
			if (component != null && component.physParent != null)
			{
				x = component.physParent.GetComponentInParent<Hand>();
			}
			if (Time.time > this.lastSoundTime + this.soundDelay)
			{
				if (x != null)
				{
					if (Time.time > this.releaseTime + this.soundDelay)
					{
						this.collisionSound.Play();
						this.lastSoundTime = Time.time;
					}
				}
				else
				{
					this.collisionSound.Play();
					this.lastSoundTime = Time.time;
				}
			}
			if (this.destructTime > 0f)
			{
				return;
			}
			if (this.balloonRigidbody.velocity.magnitude > this.maxVelocity * 10f)
			{
				this.balloonRigidbody.velocity = this.balloonRigidbody.velocity.normalized * this.maxVelocity;
			}
			if (this.hand != null)
			{
				ushort durationMicroSec = (ushort)Mathf.Clamp(Util.RemapNumber(collision.relativeVelocity.magnitude, 0f, 3f, 500f, 800f), 500f, 800f);
				this.hand.controller.TriggerHapticPulse(durationMicroSec, EVRButtonId.k_EButton_Axis0);
			}
		}

		
		public void SetColor(Balloon.BalloonColor color)
		{
			base.GetComponentInChildren<MeshRenderer>().material.color = this.BalloonColorToRGB(color);
		}

		
		private Color BalloonColorToRGB(Balloon.BalloonColor balloonColorVar)
		{
			Color result = new Color(255f, 0f, 0f);
			switch (balloonColorVar)
			{
			case Balloon.BalloonColor.Red:
				return new Color(237f, 29f, 37f, 255f) / 255f;
			case Balloon.BalloonColor.OrangeRed:
				return new Color(241f, 91f, 35f, 255f) / 255f;
			case Balloon.BalloonColor.Orange:
				return new Color(245f, 140f, 31f, 255f) / 255f;
			case Balloon.BalloonColor.YellowOrange:
				return new Color(253f, 185f, 19f, 255f) / 255f;
			case Balloon.BalloonColor.Yellow:
				return new Color(254f, 243f, 0f, 255f) / 255f;
			case Balloon.BalloonColor.GreenYellow:
				return new Color(172f, 209f, 54f, 255f) / 255f;
			case Balloon.BalloonColor.Green:
				return new Color(0f, 167f, 79f, 255f) / 255f;
			case Balloon.BalloonColor.BlueGreen:
				return new Color(108f, 202f, 189f, 255f) / 255f;
			case Balloon.BalloonColor.Blue:
				return new Color(0f, 119f, 178f, 255f) / 255f;
			case Balloon.BalloonColor.VioletBlue:
				return new Color(82f, 80f, 162f, 255f) / 255f;
			case Balloon.BalloonColor.Violet:
				return new Color(102f, 46f, 143f, 255f) / 255f;
			case Balloon.BalloonColor.RedViolet:
				return new Color(182f, 36f, 102f, 255f) / 255f;
			case Balloon.BalloonColor.LightGray:
				return new Color(192f, 192f, 192f, 255f) / 255f;
			case Balloon.BalloonColor.DarkGray:
				return new Color(128f, 128f, 128f, 255f) / 255f;
			case Balloon.BalloonColor.Random:
			{
				int balloonColorVar2 = UnityEngine.Random.Range(0, 12);
				return this.BalloonColorToRGB((Balloon.BalloonColor)balloonColorVar2);
			}
			default:
				return result;
			}
		}

		
		private Hand hand;

		
		public GameObject popPrefab;

		
		public float maxVelocity = 5f;

		
		public float lifetime = 15f;

		
		public bool burstOnLifetimeEnd;

		
		public GameObject lifetimeEndParticlePrefab;

		
		public SoundPlayOneshot lifetimeEndSound;

		
		private float destructTime;

		
		private float releaseTime = 99999f;

		
		public SoundPlayOneshot collisionSound;

		
		private float lastSoundTime;

		
		private float soundDelay = 0.2f;

		
		private Rigidbody balloonRigidbody;

		
		private bool bParticlesSpawned;

		
		private static float s_flLastDeathSound;

		
		public enum BalloonColor
		{
			
			Red,
			
			OrangeRed,
			
			Orange,
			
			YellowOrange,
			
			Yellow,
			
			GreenYellow,
			
			Green,
			
			BlueGreen,
			
			Blue,
			
			VioletBlue,
			
			Violet,
			
			RedViolet,
			
			LightGray,
			
			DarkGray,
			
			Random
		}
	}
}
