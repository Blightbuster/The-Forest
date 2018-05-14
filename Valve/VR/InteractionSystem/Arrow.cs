using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	
	public class Arrow : MonoBehaviour
	{
		
		private void Start()
		{
			if (Player.instance.headCollider != null)
			{
				Physics.IgnoreCollision(this.shaftRB.GetComponent<Collider>(), Player.instance.headCollider);
			}
		}

		
		private void FixedUpdate()
		{
			if (this.released && this.inFlight)
			{
				this.prevPosition = base.transform.position;
				this.prevRotation = base.transform.rotation;
				this.prevVelocity = base.GetComponent<Rigidbody>().velocity;
				this.prevHeadPosition = this.arrowHeadRB.transform.position;
				this.travelledFrames++;
			}
		}

		
		public void ArrowReleased(float inputVelocity)
		{
			this.inFlight = true;
			this.released = true;
			this.airReleaseSound.Play();
			if (this.glintParticle != null)
			{
				this.glintParticle.Play();
			}
			if (base.gameObject.GetComponentInChildren<FireSource>().isBurning)
			{
				this.fireReleaseSound.Play();
			}
			RaycastHit[] array = Physics.SphereCastAll(base.transform.position, 0.01f, base.transform.forward, 0.8f, -5, QueryTriggerInteraction.Ignore);
			foreach (RaycastHit raycastHit in array)
			{
				if (raycastHit.collider.gameObject != base.gameObject && raycastHit.collider.gameObject != this.arrowHeadRB.gameObject && raycastHit.collider != Player.instance.headCollider)
				{
					UnityEngine.Object.Destroy(base.gameObject);
					return;
				}
			}
			this.travelledFrames = 0;
			this.prevPosition = base.transform.position;
			this.prevRotation = base.transform.rotation;
			this.prevHeadPosition = this.arrowHeadRB.transform.position;
			this.prevVelocity = base.GetComponent<Rigidbody>().velocity;
			UnityEngine.Object.Destroy(base.gameObject, 30f);
		}

		
		private void OnCollisionEnter(Collision collision)
		{
			if (this.inFlight)
			{
				Rigidbody component = base.GetComponent<Rigidbody>();
				float sqrMagnitude = component.velocity.sqrMagnitude;
				bool flag = this.targetPhysMaterial != null && collision.collider.sharedMaterial == this.targetPhysMaterial && sqrMagnitude > 0.2f;
				bool flag2 = collision.collider.gameObject.GetComponent<Balloon>() != null;
				if (this.travelledFrames < 2 && !flag)
				{
					base.transform.position = this.prevPosition - this.prevVelocity * Time.deltaTime;
					base.transform.rotation = this.prevRotation;
					Vector3 a = Vector3.Reflect(this.arrowHeadRB.velocity, collision.contacts[0].normal);
					this.arrowHeadRB.velocity = a * 0.25f;
					this.shaftRB.velocity = a * 0.25f;
					this.travelledFrames = 0;
					return;
				}
				if (this.glintParticle != null)
				{
					this.glintParticle.Stop(true);
				}
				if (sqrMagnitude > 0.1f)
				{
					this.hitGroundSound.Play();
				}
				FireSource componentInChildren = base.gameObject.GetComponentInChildren<FireSource>();
				FireSource componentInParent = collision.collider.GetComponentInParent<FireSource>();
				if (componentInChildren != null && componentInChildren.isBurning && componentInParent != null)
				{
					if (!this.hasSpreadFire)
					{
						collision.collider.gameObject.SendMessageUpwards("FireExposure", base.gameObject, SendMessageOptions.DontRequireReceiver);
						this.hasSpreadFire = true;
					}
				}
				else if (sqrMagnitude > 0.1f || flag2)
				{
					collision.collider.gameObject.SendMessageUpwards("ApplyDamage", SendMessageOptions.DontRequireReceiver);
					base.gameObject.SendMessage("HasAppliedDamage", SendMessageOptions.DontRequireReceiver);
				}
				if (flag2)
				{
					base.transform.position = this.prevPosition;
					base.transform.rotation = this.prevRotation;
					this.arrowHeadRB.velocity = this.prevVelocity;
					Physics.IgnoreCollision(this.arrowHeadRB.GetComponent<Collider>(), collision.collider);
					Physics.IgnoreCollision(this.shaftRB.GetComponent<Collider>(), collision.collider);
				}
				if (flag)
				{
					this.StickInTarget(collision, this.travelledFrames < 2);
				}
				if (Player.instance && collision.collider == Player.instance.headCollider)
				{
					Player.instance.PlayerShotSelf();
				}
			}
		}

		
		private void StickInTarget(Collision collision, bool bSkipRayCast)
		{
			Vector3 direction = this.prevRotation * Vector3.forward;
			if (!bSkipRayCast)
			{
				RaycastHit[] array = Physics.RaycastAll(this.prevHeadPosition - this.prevVelocity * Time.deltaTime, direction, this.prevVelocity.magnitude * Time.deltaTime * 2f);
				bool flag = false;
				foreach (RaycastHit raycastHit in array)
				{
					if (raycastHit.collider == collision.collider)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return;
				}
			}
			UnityEngine.Object.Destroy(this.glintParticle);
			this.inFlight = false;
			this.shaftRB.velocity = Vector3.zero;
			this.shaftRB.angularVelocity = Vector3.zero;
			this.shaftRB.isKinematic = true;
			this.shaftRB.useGravity = false;
			this.shaftRB.transform.GetComponent<BoxCollider>().enabled = false;
			this.arrowHeadRB.velocity = Vector3.zero;
			this.arrowHeadRB.angularVelocity = Vector3.zero;
			this.arrowHeadRB.isKinematic = true;
			this.arrowHeadRB.useGravity = false;
			this.arrowHeadRB.transform.GetComponent<BoxCollider>().enabled = false;
			this.hitTargetSound.Play();
			this.scaleParentObject = new GameObject("Arrow Scale Parent");
			Transform transform = collision.collider.transform;
			ExplosionWobble component = collision.collider.gameObject.GetComponent<ExplosionWobble>();
			if (!component && transform.parent)
			{
				transform = transform.parent;
			}
			this.scaleParentObject.transform.parent = transform;
			base.transform.parent = this.scaleParentObject.transform;
			base.transform.rotation = this.prevRotation;
			base.transform.position = this.prevPosition;
			base.transform.position = collision.contacts[0].point - base.transform.forward * (0.75f - (Util.RemapNumberClamped(this.prevVelocity.magnitude, 0f, 10f, 0f, 0.1f) + UnityEngine.Random.Range(0f, 0.05f)));
		}

		
		private void OnDestroy()
		{
			if (this.scaleParentObject != null)
			{
				UnityEngine.Object.Destroy(this.scaleParentObject);
			}
		}

		
		public Transform arrowFollowTransform;

		
		public ParticleSystem glintParticle;

		
		public Rigidbody arrowHeadRB;

		
		public Rigidbody shaftRB;

		
		public PhysicMaterial targetPhysMaterial;

		
		private Vector3 prevPosition;

		
		private Quaternion prevRotation;

		
		private Vector3 prevVelocity;

		
		private Vector3 prevHeadPosition;

		
		public SoundPlayOneshot fireReleaseSound;

		
		public SoundPlayOneshot airReleaseSound;

		
		public SoundPlayOneshot hitTargetSound;

		
		public PlaySound hitGroundSound;

		
		private bool inFlight;

		
		private bool released;

		
		private bool hasSpreadFire;

		
		private int travelledFrames;

		
		private GameObject scaleParentObject;
	}
}
