﻿using System;
using UnityEngine;

public class pushRigidBody : MonoBehaviour
{
	private void Start()
	{
		this.setup = base.transform.root.GetComponentInChildren<mutantScriptSetup>();
	}

	private void OnTriggerEnter(Collider hit)
	{
		if (hit.gameObject.GetComponent<coopChoppedPartsReplicator>())
		{
			return;
		}
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
				if (!this.dontBreakCrates && hit.gameObject.CompareTag("BreakableWood") && this.setup && this.setup.animator.GetFloat("Speed") > 0.9f && !flag2)
				{
					hit.gameObject.SendMessage("Hit", 60, SendMessageOptions.DontRequireReceiver);
				}
				if (attachedRigidbody == null || attachedRigidbody.isKinematic)
				{
					return;
				}
				if (!this.setup)
				{
					this.setup = base.transform.root.GetComponentInChildren<mutantScriptSetup>();
				}
				float num = 1500f;
				if (this.DummyType)
				{
					num = this.pushPower;
				}
				if (this.DummyType)
				{
					attachedRigidbody.AddExplosionForce(num * attachedRigidbody.mass, base.transform.root.position, 20f);
				}
				else
				{
					attachedRigidbody.AddExplosionForce(num, base.transform.position, 20f);
				}
				if (this.DummyType && !flag2)
				{
					hit.gameObject.SendMessage("Hit", 60, SendMessageOptions.DontRequireReceiver);
				}
				else if (this.setup && this.setup.animator.GetFloat("Speed") > 0.9f && !flag2)
				{
					hit.gameObject.SendMessage("Hit", 60, SendMessageOptions.DontRequireReceiver);
				}
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
				float num2 = 2000f;
				if (flag3)
				{
					num2 = 2000f;
				}
				attachedRigidbody2.AddExplosionForce(num2 * attachedRigidbody2.mass, base.transform.position, 20f);
			}
		}
	}

	public bool regularMutant;

	public bool DummyType;

	public bool dontBreakCrates;

	public float pushPower;

	public float weight;

	public float minPushDistance;

	private mutantScriptSetup setup;
}
