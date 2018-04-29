using System;
using TheForest.Utils;
using UnityEngine;


public class sendCollisionEventsAsDamage : MonoBehaviour
{
	
	private void Start()
	{
		this.thisCollider = base.transform.GetComponent<Collider>();
		this.ad = base.transform.GetComponentInChildren<ArrowDamage>();
		this.at = base.transform.GetComponent<arrowTrajectory>();
	}

	
	private void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.gameObject != LocalPlayer.GameObject || (BoltNetwork.isRunning && this.at && this.at.entity && this.at.entity.isAttached && !this.at.entity.isOwner))
		{
			if (this.ad && this.ad.Live)
			{
				this.ad.CheckHit(collision.transform.position, collision.transform, collision.collider.isTrigger, collision.collider);
			}
			if (this.at)
			{
				this.at.forceDisable = true;
			}
		}
	}

	
	private ArrowDamage ad;

	
	private arrowTrajectory at;

	
	private Collider thisCollider;
}
