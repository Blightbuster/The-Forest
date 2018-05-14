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

	
	private void OnEnable()
	{
		if (!CoopPeerStarter.DedicatedHost)
		{
			if (this.thisCollider == null)
			{
				this.thisCollider = base.transform.GetComponent<Collider>();
			}
			Physics.IgnoreCollision(this.thisCollider, LocalPlayer.AnimControl.playerCollider, true);
			Physics.IgnoreCollision(this.thisCollider, LocalPlayer.AnimControl.playerHeadCollider, true);
		}
	}

	
	private void OnCollisionEnter(Collision collision)
	{
		Debug.Log("arrow collided with " + collision.gameObject.name);
		if (collision.collider.gameObject.CompareTag("Player") && !CoopPeerStarter.DedicatedHost)
		{
			if (this.thisCollider == null)
			{
				this.thisCollider = base.transform.GetComponent<Collider>();
			}
			Physics.IgnoreCollision(this.thisCollider, LocalPlayer.AnimControl.playerCollider, true);
			Physics.IgnoreCollision(this.thisCollider, LocalPlayer.AnimControl.playerHeadCollider, true);
			return;
		}
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
