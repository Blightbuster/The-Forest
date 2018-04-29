using System;
using Bolt;
using TheForest.Player;
using UnityEngine;


public class CoopWeaponArrowFire : EntityBehaviour<IDynamicPickup>
{
	
	private void Start()
	{
		this.ad = base.transform.GetComponentInChildren<ArrowDamage>();
		this.ts = base.transform.GetComponentInParent<targetStats>();
	}

	
	public override void Attached()
	{
		this.ad = base.transform.GetComponentInChildren<ArrowDamage>();
	}

	
	private void Update()
	{
		if (this.onArrowSync)
		{
			if (BoltNetwork.isRunning && this.entity && this.entity.isAttached && this.entity.isOwner && this.ad.Live && base.transform.GetComponentInChildren<WeaponBonus>())
			{
				arrowFireSync arrowFireSync = arrowFireSync.Create(GlobalTargets.Everyone);
				arrowFireSync.Target = this.entity;
				arrowFireSync.Send();
				base.enabled = false;
			}
		}
		else if (this.ts.arrowFire != this.prevState)
		{
			this.arrowFireGo.SetActive(this.ts.arrowFire);
			this.prevState = this.ts.arrowFire;
		}
	}

	
	private void enableArrowFire()
	{
		this.arrowFireGo.SetActive(true);
	}

	
	private targetStats ts;

	
	private ArrowDamage ad;

	
	public GameObject arrowFireGo;

	
	public bool onArrowSync;

	
	private bool prevState;
}
