using System;
using TheForest.Buildings.Creation;
using UnityEngine;

public class arrowPassThroughManager : MonoBehaviour
{
	private void Start()
	{
		this.thisTrigger = base.transform.GetComponent<Collider>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!other.isTrigger)
		{
			this.ignoreCollisionEvents(other);
		}
	}

	private bool ignoreCollisionEvents(Collider col)
	{
		if (!col)
		{
			return false;
		}
		if (col.isTrigger)
		{
			return false;
		}
		bool flag = false;
		if (col.transform.GetComponent<girlMutantColliderSetup>() || col.transform.GetComponent<CoopMutantFX>())
		{
			flag = true;
		}
		if (col.transform.parent && (col.transform.parent.GetComponent<StickFenceChunkArchitect>() || col.transform.parent.GetComponent<BoneFenceChunkArchitect>()))
		{
			flag = true;
		}
		if (flag && this.thisCollisionCollider && col && this.thisCollisionCollider.enabled && col.enabled && col.gameObject.activeInHierarchy)
		{
			Physics.IgnoreCollision(this.thisCollisionCollider, col, true);
			return true;
		}
		return false;
	}

	private Collider thisTrigger;

	public Collider thisCollisionCollider;
}
