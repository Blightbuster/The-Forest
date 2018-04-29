using System;
using System.Collections;
using UnityEngine;


public class Explosive : MonoBehaviour
{
	
	private void Start()
	{
	}

	
	private IEnumerator OnCollisionEnter(Collision col)
	{
		if (base.enabled && col.contacts.Length > 0)
		{
			float velocityAlongCollisionNormal = Vector3.Project(col.relativeVelocity, col.contacts[0].normal).magnitude;
			if ((velocityAlongCollisionNormal > this.detonationImpactVelocity || this.exploded) && !this.exploded)
			{
				UnityEngine.Object.Instantiate(this.explosionPrefab, col.contacts[0].point, Quaternion.LookRotation(col.contacts[0].normal));
				this.exploded = true;
				base.SendMessage("Immobilize");
				if (this.reset)
				{
					base.GetComponent<ObjectResetter>().DelayedReset(this.resetTimeDelay);
				}
			}
		}
		yield return null;
		yield break;
	}

	
	public void Reset()
	{
		this.exploded = false;
	}

	
	public Transform explosionPrefab;

	
	private bool exploded;

	
	public float detonationImpactVelocity = 10f;

	
	public float sizeMultiplier = 1f;

	
	public bool reset = true;

	
	public float resetTimeDelay = 10f;
}
