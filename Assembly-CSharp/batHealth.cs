using System;
using PathologicalGames;
using UnityEngine;

public class batHealth : MonoBehaviour
{
	private void Start()
	{
		this.cls = base.transform.GetComponent<clsragdollify>();
	}

	private void OnEnable()
	{
		this.health = 2;
		if (this.fire)
		{
			this.fire.SetActive(false);
		}
		if (this.cls)
		{
			this.cls.burning = false;
		}
	}

	private void OnDisable()
	{
		base.CancelInvoke("hitFire");
	}

	private void Burn()
	{
		if (this.fire)
		{
			this.fire.SetActive(true);
		}
		base.InvokeRepeating("hitFire", 3f, 3f);
	}

	private void hitFire()
	{
		this.Hit(1);
	}

	private void Hit(int damage)
	{
		this.health -= damage;
		if (this.health <= 0)
		{
			this.die();
		}
	}

	private void Explosion()
	{
		this.Hit(10);
	}

	private void die()
	{
		base.CancelInvoke("hitFire");
		if (this.fire.activeSelf)
		{
			this.cls.burning = true;
		}
		this.cls.metgoragdoll(default(Vector3));
		if (PoolManager.Pools["creatures"].IsSpawned(base.transform.parent))
		{
			PoolManager.Pools["creatures"].Despawn(base.transform.parent);
		}
		else
		{
			base.transform.parent.gameObject.SetActive(false);
		}
	}

	public int health = 2;

	public GameObject fire;

	private clsragdollify cls;
}
