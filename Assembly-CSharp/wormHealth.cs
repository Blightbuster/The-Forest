using System;
using UnityEngine;

public class wormHealth : MonoBehaviour
{
	private void Start()
	{
		this.thisSkin = base.transform.GetComponentInChildren<SkinnedMeshRenderer>();
		this.controller = base.transform.GetComponent<wormSingleController>();
		this.skinDamage = base.transform.GetComponent<wormSkinDamage>();
	}

	public void Hit(int damage)
	{
		if (Time.time < this.hitCoolDown)
		{
			return;
		}
		this.hitCoolDown = Time.time + 0.25f;
		this.health -= damage;
		if (this.skinDamage)
		{
			this.skinDamage.setSkinDamage(0.5f);
		}
		if (this.health < 1)
		{
			this.Die();
		}
	}

	public void Burn()
	{
		if (Time.time < this.burnCoolDown)
		{
			return;
		}
		this.burnCoolDown = Time.time + 0.25f;
		this.onFire = true;
		this.fireGo.SetActive(true);
		base.InvokeRepeating("HitFire", 1f, 1f);
		if (base.IsInvoking("CancelHitFire"))
		{
			base.CancelInvoke("CancelHitFire");
		}
		base.Invoke("CancelHitFire", 8f);
	}

	private void HitFire()
	{
		this.health -= 3;
		if (this.health < 1)
		{
			this.Die();
		}
	}

	public void Die()
	{
		if (this.dead)
		{
			return;
		}
		this.dead = true;
		if (BoltNetwork.isClient)
		{
			return;
		}
		this.controller.setWormIdleLoop(false);
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.ragdollGo, base.transform.position, base.transform.rotation);
		gameObject.SendMessage("setBodyVariation", this.controller.bodyType);
		if (this.onFire)
		{
			gameObject.SendMessage("setOnFire");
		}
		this.controller.Detach();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void CancelHitFire()
	{
		base.CancelInvoke("HitFire");
		this.onFire = false;
		this.fireGo.SetActive(false);
	}

	private wormSingleController controller;

	private SkinnedMeshRenderer thisSkin;

	private wormSkinDamage skinDamage;

	private int health = 8;

	public GameObject ragdollGo;

	public GameObject fireGo;

	public bool onFire;

	private bool dead;

	private float hitCoolDown;

	private float burnCoolDown;
}
