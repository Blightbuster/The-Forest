using System;
using UnityEngine;

public class trapHit : MonoBehaviour
{
	private void Start()
	{
		if (this.trigger.largeSpike)
		{
			this.trapType = 0;
		}
		if (this.trigger.largeDeadfall)
		{
			this.trapType = 1;
		}
		if (this.trigger.largeNoose)
		{
			this.trapType = 2;
		}
		if (this.trigger.largeSwingingRock)
		{
			this.trapType = 3;
			this.rb = base.transform.GetComponent<Rigidbody>();
		}
	}

	private void OnEnable()
	{
		this.disable = false;
		if (!this.trigger.largeSwingingRock && !this.disable)
		{
			base.Invoke("disableCollision", 2f);
		}
	}

	private void OnDisable()
	{
		base.CancelInvoke("disableCollision");
		this.disable = false;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag("Player"))
		{
			this.registerTrapHit(collision.collider);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		this.registerTrapHit(other);
	}

	private void registerTrapHit(Collider other)
	{
		if (other.GetComponent<creepyHitReactions>() && !this.disable && this.trigger.largeSpike)
		{
			this.sendCreepyDamageFromRoot(other);
			if (!this.disable)
			{
				base.Invoke("disableCollision", 0.8f);
			}
			return;
		}
		if (other.gameObject.CompareTag("enemyCollide") || other.gameObject.CompareTag("playerHitDetect") || other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("animalCollide"))
		{
			if (other.gameObject.CompareTag("enemyCollide"))
			{
				mutantHitReceiver component = other.transform.GetComponent<mutantHitReceiver>();
				if (component && component.inNooseTrap)
				{
					return;
				}
				explodeDummy component2 = other.GetComponent<explodeDummy>();
				if (component2)
				{
					return;
				}
			}
			if (!this.disable)
			{
				if (this.trigger.largeSpike)
				{
					other.gameObject.SendMessageUpwards("setTrapLookat", base.transform.root.gameObject, SendMessageOptions.DontRequireReceiver);
					base.gameObject.SendMessage("addTrappedMutant", other.transform.root.gameObject, SendMessageOptions.DontRequireReceiver);
					other.gameObject.SendMessageUpwards("setCurrentTrap", this.trigger.gameObject, SendMessageOptions.DontRequireReceiver);
					this.sendCreepyDamage(other);
					if (other.gameObject.CompareTag("Player"))
					{
						other.gameObject.SendMessage("HitFromTrap", 35, SendMessageOptions.DontRequireReceiver);
					}
				}
				else if (this.trigger.largeDeadfall)
				{
					if (other.gameObject.CompareTag("playerHitDetect"))
					{
						other.gameObject.SendMessageUpwards("HitFromTrap", 35, SendMessageOptions.DontRequireReceiver);
					}
					if (other.gameObject.CompareTag("enemyCollide"))
					{
						this.sendCreepyDamage(other);
					}
				}
				if (this.trigger.largeSwingingRock)
				{
					if (this.rb.velocity.magnitude > 11f)
					{
						other.gameObject.SendMessageUpwards("Explosion", -1, SendMessageOptions.DontRequireReceiver);
						other.gameObject.SendMessage("lookAtExplosion", base.transform.position, SendMessageOptions.DontRequireReceiver);
						other.gameObject.SendMessageUpwards("DieTrap", this.trapType, SendMessageOptions.DontRequireReceiver);
					}
				}
				else
				{
					other.gameObject.SendMessageUpwards("DieTrap", this.trapType, SendMessageOptions.DontRequireReceiver);
					if (other.gameObject.CompareTag("enemyCollide") && this.trigger.largeSpike)
					{
						Vector3 vector = this.PutOnSpikes(other.transform.root.gameObject);
						other.gameObject.SendMessageUpwards("setPositionAtSpikes", vector, SendMessageOptions.DontRequireReceiver);
					}
				}
				if (!this.disable && !this.trigger.largeSwingingRock)
				{
					base.Invoke("disableCollision", 0.8f);
				}
			}
		}
	}

	public float RelativeToSpikes(Vector3 enemyPosition)
	{
		Vector3 a = base.transform.root.position - this.trigger.transform.position;
		Vector3 b = 0.85f * a + this.trigger.transform.position;
		Vector3 vector = enemyPosition - b;
		Vector3 lhs = Vector3.Cross(a.normalized, base.transform.root.up);
		float num = Vector3.Dot(lhs, vector.normalized);
		return num * vector.magnitude;
	}

	public Vector3 RelativeFromSpikes(float position)
	{
		Vector3 a = base.transform.root.position - this.trigger.transform.position;
		Vector3 b = 0.85f * a + this.trigger.transform.position;
		Vector3 a2 = Vector3.Cross(a.normalized, base.transform.root.up);
		return position * a2 + b;
	}

	public Vector3 PutOnSpikes(GameObject enemy)
	{
		if (this.trigger.largeSpike)
		{
			Vector3 vector = this.spikePosTr.InverseTransformPoint(enemy.transform.position);
			vector.z = 0f;
			vector.y = 0f;
			vector = this.spikePosTr.TransformPoint(vector);
			return vector + this.spikePosTr.forward * -0.65f;
		}
		return Vector3.zero;
	}

	private void sendCreepyDamage(Collider other)
	{
		mutantTargetSwitching component = other.GetComponent<mutantTargetSwitching>();
		if (component && (component.typeBabyCreepy || component.typeFatCreepy || component.typeFemaleCreepy || component.typeMaleCreepy))
		{
			other.SendMessageUpwards("staggerFromHit", SendMessageOptions.DontRequireReceiver);
			other.SendMessageUpwards("Hit", 150, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void sendCreepyDamageFromRoot(Collider other)
	{
		EnemyHealth componentInChildren = other.GetComponentInChildren<EnemyHealth>();
		if (componentInChildren)
		{
			other.SendMessageUpwards("staggerFromHit", SendMessageOptions.DontRequireReceiver);
			other.SendMessageUpwards("Hit", 150);
		}
	}

	private void disableCollision()
	{
		this.disable = true;
	}

	public trapTrigger trigger;

	private int trapType;

	public bool disable;

	private Rigidbody rb;

	public Transform spikePosTr;
}
