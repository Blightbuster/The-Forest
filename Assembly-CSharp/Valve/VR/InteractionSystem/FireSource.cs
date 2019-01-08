using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	public class FireSource : MonoBehaviour
	{
		private void Start()
		{
			if (this.startActive)
			{
				this.StartBurning();
			}
		}

		private void Update()
		{
			if (this.burnTime != 0f && Time.time > this.ignitionTime + this.burnTime && this.isBurning)
			{
				this.isBurning = false;
				if (this.customParticles != null)
				{
					this.customParticles.Stop();
				}
				else
				{
					UnityEngine.Object.Destroy(this.fireObject);
				}
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (this.isBurning && this.canSpreadFromThisSource)
			{
				other.SendMessageUpwards("FireExposure", SendMessageOptions.DontRequireReceiver);
			}
		}

		private void FireExposure()
		{
			if (this.fireObject == null)
			{
				base.Invoke("StartBurning", this.ignitionDelay);
			}
			if (this.hand = base.GetComponentInParent<Hand>())
			{
				this.hand.controller.TriggerHapticPulse(1000, EVRButtonId.k_EButton_Axis0);
			}
		}

		private void StartBurning()
		{
			this.isBurning = true;
			this.ignitionTime = Time.time;
			if (this.ignitionSound != null)
			{
				this.ignitionSound.Play();
			}
			if (this.customParticles != null)
			{
				this.customParticles.Play();
			}
			else if (this.fireParticlePrefab != null)
			{
				this.fireObject = UnityEngine.Object.Instantiate<GameObject>(this.fireParticlePrefab, base.transform.position, base.transform.rotation);
				this.fireObject.transform.parent = base.transform;
			}
		}

		public GameObject fireParticlePrefab;

		public bool startActive;

		private GameObject fireObject;

		public ParticleSystem customParticles;

		public bool isBurning;

		public float burnTime;

		public float ignitionDelay;

		private float ignitionTime;

		private Hand hand;

		public AudioSource ignitionSound;

		public bool canSpreadFromThisSource = true;
	}
}
