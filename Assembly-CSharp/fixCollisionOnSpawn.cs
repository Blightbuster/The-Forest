using System;
using System.Collections;
using UnityEngine;

public class fixCollisionOnSpawn : MonoBehaviour
{
	private void OnEnable()
	{
		if (!this.thisCollider)
		{
			this.thisCollider = base.GetComponent<MeshCollider>();
		}
		if (this.fixGarden)
		{
			base.StartCoroutine(this.fixGardenCollision());
		}
	}

	private IEnumerator fixGardenCollision()
	{
		Rigidbody rb = base.gameObject.AddComponent<Rigidbody>();
		rb.useGravity = false;
		rb.isKinematic = true;
		this.thisCollider.isTrigger = true;
		yield return YieldPresets.WaitForFixedUpdate;
		this.thisCollider.isTrigger = false;
		yield return YieldPresets.WaitPointTwoSeconds;
		if (rb)
		{
			UnityEngine.Object.Destroy(rb);
		}
		UnityEngine.Object.Destroy(this);
		yield break;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("PlayerNet") || other.gameObject.CompareTag("Player"))
		{
			return;
		}
		Rigidbody component = other.gameObject.GetComponent<Rigidbody>();
		if (component && component.useGravity && !component.isKinematic)
		{
			base.StartCoroutine(this.repositionOtherRigidBody(component));
		}
	}

	private IEnumerator repositionOtherRigidBody(Rigidbody targetRb)
	{
		float storeDrag = targetRb.drag;
		float storeAngDrag = targetRb.angularDrag;
		targetRb.drag = 20f;
		targetRb.angularDrag = 20f;
		targetRb.transform.position += new Vector3(0f, 1f, 0f);
		yield return null;
		yield return null;
		yield return null;
		if (targetRb)
		{
			targetRb.drag = storeDrag;
			targetRb.angularDrag = storeAngDrag;
		}
		yield break;
	}

	public bool fixGarden;

	private MeshCollider thisCollider;
}
