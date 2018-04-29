using System;
using Bolt;
using UnityEngine;


public class flameThrowerDamage : MonoBehaviour
{
	
	private void Awake()
	{
		this.rootTr = base.transform.root;
	}

	
	private void Start()
	{
		this.flameCollider = base.transform.GetComponent<Collider>();
	}

	
	private void OnEnable()
	{
		if (this.flameCollider)
		{
			this.flameCollider.enabled = true;
		}
	}

	
	private void OnTriggerEnter(Collider other)
	{
		if (this.rootTr != other.transform.root)
		{
			other.SendMessage("Douse", this, SendMessageOptions.DontRequireReceiver);
			other.SendMessage("Burn", this, SendMessageOptions.DontRequireReceiver);
			if (BoltNetwork.isClient)
			{
				BoltEntity component = other.transform.GetComponent<BoltEntity>();
				if (component == null)
				{
					other.GetComponentInParent<BoltEntity>();
				}
				if (component)
				{
					Burn burn = Burn.Create(GlobalTargets.OnlyServer);
					burn.Entity = component;
					burn.Send();
				}
			}
		}
	}

	
	private void FixedUpdate()
	{
		if (Time.time > this.updateRate)
		{
			this.flameCollider.enabled = false;
			this.updateRate = Time.time + 0.3f;
		}
		else if (!this.flameCollider.enabled)
		{
			this.flameCollider.enabled = true;
		}
	}

	
	private Collider flameCollider;

	
	private Transform rootTr;

	
	private float updateRate;
}
