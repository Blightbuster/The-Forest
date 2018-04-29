using System;
using UnityEngine;


public class animalHitReceiver : MonoBehaviour
{
	
	private void Awake()
	{
		this.health = base.transform.root.GetComponentInChildren<animalHealth>();
		if (!this.health)
		{
			this.health = base.transform.root.GetComponent<animalHealth>();
		}
		if (this.goose)
		{
			base.transform.GetComponentInParent<gooseHealth>();
		}
	}

	
	private void Start()
	{
		this.disableBodyCollisions();
	}

	
	private void OnEnable()
	{
		this.disableBodyCollisions();
	}

	
	private void Hit(int damage)
	{
		if (this.health)
		{
			this.health.Hit(damage);
		}
		if (this.goose)
		{
			this.gHealth.Hit(10);
		}
	}

	
	public void disableBodyCollisions()
	{
		if (this.ignoreCollisionFixes)
		{
			return;
		}
		if (!this.col)
		{
			Collider[] components = base.transform.GetComponents<Collider>();
			foreach (Collider collider in components)
			{
				if (!collider.isTrigger)
				{
					this.col = collider;
				}
			}
		}
		if (!this.rootCol)
		{
			this.rootCol = base.transform.root.GetComponent<Collider>();
		}
		if (!this.tCol)
		{
			this.tCol = Terrain.activeTerrain.GetComponent<TerrainCollider>();
		}
		if (this.col)
		{
			if (this.tCol && this.tCol.enabled && this.col.gameObject.activeInHierarchy && this.col.enabled)
			{
				Physics.IgnoreCollision(this.col, this.tCol, true);
			}
			if (this.rootCol && this.col.enabled && this.rootCol.gameObject.activeSelf && this.rootCol.enabled)
			{
				Physics.IgnoreCollision(this.rootCol, this.col, true);
			}
		}
	}

	
	public void Burn()
	{
		if (!this.burning && this.health)
		{
			this.health.Burn();
			this.burning = true;
			base.Invoke("resetBurning", 5f);
		}
		if (this.goose)
		{
			this.gHealth.Hit(10);
		}
	}

	
	private void resetBurning()
	{
		this.burning = false;
	}

	
	private animalHealth health;

	
	private gooseHealth gHealth;

	
	public bool goose;

	
	private bool burning;

	
	private Collider col;

	
	private Collider tCol;

	
	private Collider rootCol;

	
	public bool ignoreCollisionFixes;
}
