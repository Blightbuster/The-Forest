using System;
using Bolt;
using UnityEngine;


public class DamageCorpseSimple : EntityBehaviour
{
	
	public void DoLocalCut(int health)
	{
		if (health >= 20)
		{
			return;
		}
		UnityEngine.Object.Destroy(UnityEngine.Object.Instantiate<GameObject>(this.BloodSplat, base.transform.position, Quaternion.identity), 0.5f);
		if (this.MyGore)
		{
			this.MyGore.SetActive(true);
		}
		if (this.cutBody)
		{
			this.cutBody.gameObject.SetActive(true);
		}
		if (this.cutHead)
		{
			this.cutHead.gameObject.SetActive(true);
		}
		if (this.baseBody)
		{
			this.baseBody.enabled = false;
		}
		this.Health = health;
		if (health <= 0)
		{
			this.CutDown();
		}
	}

	
	private void ignoreCutting()
	{
		this.ignoreHit = true;
	}

	
	private void Hit(int damage)
	{
		if (this.ignoreHit)
		{
			this.ignoreHit = false;
			return;
		}
		if (base.entity.IsAttached() && BoltNetwork.isRunning)
		{
			HitCorpse hitCorpse = HitCorpse.Create(GlobalTargets.Everyone);
			hitCorpse.Entity = base.entity;
			hitCorpse.Damage = this.Health - damage;
			hitCorpse.creepyCorpse = true;
			hitCorpse.Send();
		}
		this.DoLocalCut(this.Health - damage);
	}

	
	private void CutDown()
	{
		if (this.cutBody)
		{
			this.cutBody.gameObject.SetActive(true);
		}
		if (this.cutHead)
		{
			this.cutHead.gameObject.SetActive(false);
		}
		if (this.baseBody)
		{
			this.baseBody.enabled = false;
		}
		if (!BoltNetwork.isClient)
		{
			this.MyCut.SetActive(true);
		}
		if (!this.creepySetup)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	
	private void Explosion(float dist)
	{
		base.Invoke("CutDown", 1f);
	}

	
	public int Health = 20;

	
	public GameObject BloodSplat;

	
	public GameObject MyCut;

	
	public GameObject MyGore;

	
	public Renderer cutBody;

	
	public Renderer cutHead;

	
	public Renderer baseBody;

	
	private bool ignoreHit;

	
	public bool creepySetup;
}
