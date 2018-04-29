using System;
using System.Collections;
using UnityEngine;


public class ExplosionFireAndDebris : MonoBehaviour
{
	
	private IEnumerator Start()
	{
		float multiplier = base.GetComponent<ParticleSystemMultiplier>().multiplier;
		int num = 0;
		while ((float)num < (float)this.numDebrisPieces * multiplier)
		{
			Transform original = this.debrisPrefabs[UnityEngine.Random.Range(0, this.debrisPrefabs.Length)];
			Vector3 position = base.transform.position + UnityEngine.Random.insideUnitSphere * 3f * multiplier;
			Quaternion rotation = UnityEngine.Random.rotation;
			UnityEngine.Object.Instantiate<Transform>(original, position, rotation);
			num++;
		}
		yield return null;
		float r = 10f * multiplier;
		Collider[] cols = Physics.OverlapSphere(base.transform.position, r);
		foreach (Collider collider in cols)
		{
			if (this.numFires > 0)
			{
				Ray ray = new Ray(base.transform.position, collider.transform.position - base.transform.position);
				RaycastHit raycastHit;
				if (collider.Raycast(ray, out raycastHit, r))
				{
					this.AddFire(collider.transform, raycastHit.point, raycastHit.normal);
					this.numFires--;
				}
			}
		}
		float testR = 0f;
		while (this.numFires > 0 && testR < r)
		{
			Ray ray2 = new Ray(base.transform.position + Vector3.up, UnityEngine.Random.onUnitSphere);
			RaycastHit raycastHit2;
			if (Physics.Raycast(ray2, out raycastHit2, testR))
			{
				this.AddFire(null, raycastHit2.point, raycastHit2.normal);
				this.numFires--;
			}
			testR += r * 0.1f;
		}
		yield break;
	}

	
	private void AddFire(Transform t, Vector3 pos, Vector3 normal)
	{
		pos += normal * 0.5f;
		Transform transform = UnityEngine.Object.Instantiate<Transform>(this.firePrefab, pos, Quaternion.identity);
		transform.parent = t;
	}

	
	public Transform[] debrisPrefabs;

	
	public Transform firePrefab;

	
	public int numDebrisPieces;

	
	public int numFires;
}
