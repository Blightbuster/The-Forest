using System;
using System.Collections;
using UnityEngine;


public class ExplosionFireAndDebris : MonoBehaviour
{
	
	private IEnumerator Start()
	{
		float multiplier = base.GetComponent<ParticleSystemMultiplier>().multiplier;
		int i = 0;
		while ((float)i < (float)this.numDebrisPieces * multiplier)
		{
			Transform prefab = this.debrisPrefabs[UnityEngine.Random.Range(0, this.debrisPrefabs.Length)];
			Vector3 pos = base.transform.position + UnityEngine.Random.insideUnitSphere * 3f * multiplier;
			Quaternion rot = UnityEngine.Random.rotation;
			UnityEngine.Object.Instantiate(prefab, pos, rot);
			i++;
		}
		yield return null;
		float r = 10f * multiplier;
		Collider[] cols = Physics.OverlapSphere(base.transform.position, r);
		foreach (Collider col in cols)
		{
			if (this.numFires > 0)
			{
				Ray fireRay = new Ray(base.transform.position, col.transform.position - base.transform.position);
				RaycastHit fireHit;
				if (col.Raycast(fireRay, out fireHit, r))
				{
					this.AddFire(col.transform, fireHit.point, fireHit.normal);
					this.numFires--;
				}
			}
		}
		float testR = 0f;
		while (this.numFires > 0 && testR < r)
		{
			Ray fireRay2 = new Ray(base.transform.position + Vector3.up, UnityEngine.Random.onUnitSphere);
			RaycastHit fireHit2;
			if (Physics.Raycast(fireRay2, out fireHit2, testR))
			{
				this.AddFire(null, fireHit2.point, fireHit2.normal);
				this.numFires--;
			}
			testR += r * 0.1f;
		}
		yield break;
	}

	
	private void AddFire(Transform t, Vector3 pos, Vector3 normal)
	{
		pos += normal * 0.5f;
		Transform transform = (Transform)UnityEngine.Object.Instantiate(this.firePrefab, pos, Quaternion.identity);
		transform.parent = t;
	}

	
	public Transform[] debrisPrefabs;

	
	public Transform firePrefab;

	
	public int numDebrisPieces;

	
	public int numFires;
}
