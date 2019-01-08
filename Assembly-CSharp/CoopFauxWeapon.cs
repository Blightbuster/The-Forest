using System;
using TheForest.Buildings.World;
using TheForest.World;
using UnityEngine;

public class CoopFauxWeapon : MonoBehaviour
{
	private void OnTriggerEnter(Collider c)
	{
		if (c.CompareTag("SmallTree") || c.CompareTag("EnemyBodyPart") || c.CompareTag("jumpObject") || c.CompareTag("UnderfootWood") || c.CompareTag("BreakableRock") || c.CompareTag("BreakableWood") || c.CompareTag("corpseProp"))
		{
			ignoreFauxHits component = c.GetComponent<ignoreFauxHits>();
			if (component)
			{
				return;
			}
			if (c.GetComponent<BuildingHealthHitRelay>())
			{
				return;
			}
			if (c.CompareTag("corpseProp"))
			{
				c.SendMessageUpwards("Hit", this.Damage, SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				c.SendMessage("Hit", this.Damage, SendMessageOptions.DontRequireReceiver);
				c.SendMessage("LocalizedHit", new LocalizedHitData(base.transform.position, (float)this.Damage), SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	private void FixedUpdate()
	{
		if (++this.fixedUpdate > 10)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private int fixedUpdate;

	[HideInInspector]
	public int Damage;
}
