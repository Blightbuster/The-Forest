using System;
using UnityEngine;


public class pushRigidBody : MonoBehaviour
{
	
	private void Start()
	{
		this.setup = base.transform.root.GetComponentInChildren<mutantScriptSetup>();
	}

	
	private void OnTriggerEnter(Collider hit)
	{
		if (hit.gameObject.GetComponent<ignorePushCollider>())
		{
			return;
		}
		if (hit.transform.root.GetComponent<ignorePushCollider>())
		{
			return;
		}
		if (this.regularMutant)
		{
			bool flag = false;
			if (hit.gameObject.CompareTag("UnderfootWood"))
			{
				enemyCanPush component = hit.gameObject.GetComponent<enemyCanPush>();
				if (component)
				{
					flag = true;
				}
			}
			if (hit.gameObject.CompareTag("Float") || hit.gameObject.CompareTag("suitCase") || hit.gameObject.CompareTag("pushable") || hit.gameObject.CompareTag("BreakableWood") || hit.gameObject.CompareTag("hanging") || flag || hit.gameObject.layer == 28 || hit.gameObject.layer == 21)
			{
				Rigidbody attachedRigidbody = hit.attachedRigidbody;
				bool flag2 = false;
				if (hit.gameObject.CompareTag("Float") || hit.gameObject.CompareTag("suitCase"))
				{
					flag2 = true;
				}
				if (attachedRigidbody == null || attachedRigidbody.isKinematic)
				{
					return;
				}
				if (!this.setup)
				{
					this.setup = base.transform.root.GetComponentInChildren<mutantScriptSetup>();
				}
				if (this.setup.animator.GetFloat("Speed") > 0.9f && !flag2)
				{
					hit.gameObject.SendMessage("Hit", 10, SendMessageOptions.DontRequireReceiver);
				}
				float explosionForce = 2000f;
				if (flag)
				{
					explosionForce = 2000f;
				}
				if (flag2)
				{
					explosionForce = 8000f;
				}
				attachedRigidbody.AddExplosionForce(explosionForce, base.transform.position, 20f);
			}
		}
		else
		{
			bool flag3 = false;
			enableNitrogenRelay component2 = hit.transform.GetComponent<enableNitrogenRelay>();
			if (component2)
			{
				flag3 = true;
				Rigidbody component3 = component2.transform.GetComponent<Rigidbody>();
				if (component3)
				{
					component3.isKinematic = false;
					component3.useGravity = true;
				}
			}
			if (hit.gameObject.CompareTag("UnderfootWood"))
			{
				enemyCanPush component4 = hit.gameObject.GetComponent<enemyCanPush>();
				if (component4)
				{
					flag3 = true;
				}
			}
			if (hit.gameObject.CompareTag("Float") || hit.gameObject.CompareTag("suitCase") || hit.gameObject.CompareTag("pushable") || hit.gameObject.CompareTag("BreakableWood") || hit.gameObject.CompareTag("BreakableRock") || hit.gameObject.CompareTag("hanging") || hit.gameObject.CompareTag("corpseProp") || flag3 || hit.gameObject.layer == 28 || hit.gameObject.layer == 21)
			{
				if (hit.gameObject.CompareTag("BreakableRock"))
				{
					hit.gameObject.SendMessage("Explosion", SendMessageOptions.DontRequireReceiver);
				}
				Rigidbody attachedRigidbody2 = hit.GetComponent<Collider>().attachedRigidbody;
				if (!this.dontBreakCrates && hit.gameObject.CompareTag("BreakableWood"))
				{
					hit.gameObject.SendMessage("Hit", 60, SendMessageOptions.DontRequireReceiver);
				}
				if (attachedRigidbody2 == null || attachedRigidbody2.isKinematic)
				{
					return;
				}
				float explosionForce2 = 2000f;
				if (flag3)
				{
					explosionForce2 = 2000f;
				}
				attachedRigidbody2.AddExplosionForce(explosionForce2, base.transform.position, 20f);
			}
		}
	}

	
	public bool regularMutant;

	
	public bool dontBreakCrates;

	
	public float pushPower;

	
	public float weight;

	
	public float minPushDistance;

	
	private mutantScriptSetup setup;
}
