using System;
using UnityEngine;

public class StickyBomb : MonoBehaviour
{
	private void OnCollisionEnter(Collision other)
	{
		if (this.doneStick)
		{
			return;
		}
		if (other.gameObject.CompareTag("Player"))
		{
			return;
		}
		FollowTarget followTarget = base.gameObject.AddComponent<FollowTarget>();
		followTarget.target = other.transform;
		followTarget.offset = base.transform.position - other.transform.position;
		CapsuleCollider component = base.transform.GetComponent<CapsuleCollider>();
		if (component)
		{
			component.isTrigger = true;
		}
		base.GetComponent<Rigidbody>().isKinematic = true;
		this.doneStick = true;
		UnityEngine.Object.Destroy(this);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("enemyCollide") || other.gameObject.CompareTag("animalRoot") || other.gameObject.CompareTag("animalCollide"))
		{
			if (this.doneStick)
			{
				return;
			}
			arrowStickToTarget arrowStickToTarget = other.GetComponent<arrowStickToTarget>();
			if (arrowStickToTarget == null)
			{
				arrowStickToTarget = other.GetComponentInChildren<arrowStickToTarget>();
			}
			if (arrowStickToTarget)
			{
				Transform transform = arrowStickToTarget.returnNearestJoint(base.transform);
				if (transform)
				{
					FollowTarget followTarget = base.gameObject.AddComponent<FollowTarget>();
					followTarget.target = transform;
					followTarget.offset = (base.transform.position - transform.position) / 3.5f;
					followTarget.followRotation = true;
					CapsuleCollider component = base.transform.GetComponent<CapsuleCollider>();
					if (component)
					{
						component.isTrigger = true;
					}
					base.GetComponent<Rigidbody>().isKinematic = true;
					this.doneStick = true;
					UnityEngine.Object.Destroy(this);
				}
			}
		}
	}

	private bool doneStick;
}
