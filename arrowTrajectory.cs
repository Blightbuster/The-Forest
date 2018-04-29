using System;
using Bolt;
using UnityEngine;


public class arrowTrajectory : EntityBehaviour<IRigidbodyState>
{
	
	private void Awake()
	{
		this.col = base.transform.GetComponent<Collider>();
		this.rb = base.GetComponent<Rigidbody>();
		this._spearType = false;
		this.ad = base.transform.GetComponentInChildren<ArrowDamage>();
	}

	
	private void Start()
	{
		this.forceDisable = false;
	}

	
	public override void Attached()
	{
		this._boltEntity = base.entity;
	}

	
	private void Update()
	{
		if (this.forceDisable)
		{
			if (this.rb)
			{
				this.rb.AddTorque((float)UnityEngine.Random.Range(-1000, 1000), (float)UnityEngine.Random.Range(-1000, 1000), (float)UnityEngine.Random.Range(-1000, 1000));
			}
			base.enabled = false;
			base.Invoke("ActivatePickupOnly", 0.75f);
			return;
		}
		if (this.rb && !this.rb.isKinematic)
		{
			if (this.rb.velocity.sqrMagnitude > 1f)
			{
				base.transform.LookAt(base.transform.position + this.rb.velocity);
				base.transform.rotation *= Quaternion.Euler(90f, 0f, 0f);
			}
			else
			{
				this.ActivatePickup();
				base.enabled = false;
			}
		}
		else
		{
			base.Invoke("ActivatePickup", 0.75f);
			base.enabled = false;
		}
	}

	
	private void ActivatePickup()
	{
		if (!this._spearType)
		{
			Collider component = base.transform.GetComponent<Collider>();
			if (component && this.rb.isKinematic)
			{
				component.isTrigger = true;
			}
		}
		this._pickup.SetActive(true);
	}

	
	private void ActivatePickupOnly()
	{
		this._pickup.SetActive(true);
	}

	
	private void setCraftedBowDamage()
	{
		if (this.ad)
		{
			this.ad.damage = 14;
		}
	}

	
	public void setArrowDynamic()
	{
		if (this.rb)
		{
			this.rb.isKinematic = false;
			this.rb.useGravity = true;
			this.col.isTrigger = false;
		}
	}

	
	public GameObject _pickup;

	
	private Rigidbody rb;

	
	private Quaternion rot;

	
	public bool forceDisable;

	
	public Collider col;

	
	public bool _spearType;

	
	private ArrowDamage ad;

	
	public BoltEntity _boltEntity;
}
