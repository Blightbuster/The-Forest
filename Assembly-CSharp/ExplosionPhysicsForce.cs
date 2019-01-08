using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionPhysicsForce : MonoBehaviour
{
	private IEnumerator Start()
	{
		yield return null;
		float multiplier = base.GetComponent<ParticleSystemMultiplier>().multiplier;
		float r = 10f * multiplier;
		Collider[] cols = Physics.OverlapSphere(base.transform.position, r);
		List<Rigidbody> rigidbodies = new List<Rigidbody>();
		foreach (Collider collider in cols)
		{
			if (collider.attachedRigidbody != null && !rigidbodies.Contains(collider.attachedRigidbody))
			{
				rigidbodies.Add(collider.attachedRigidbody);
			}
		}
		foreach (Rigidbody rigidbody in rigidbodies)
		{
			rigidbody.AddExplosionForce(this.explosionForce * multiplier, base.transform.position, r, 1f * multiplier, ForceMode.Impulse);
		}
		yield break;
	}

	public float explosionForce = 4f;
}
