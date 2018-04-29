using System;
using UnityEngine;


public class ragdollExplode : MonoBehaviour
{
	
	private void Explosion()
	{
		Debug.Log("doing explosion");
		if (this.explodedPrefab)
		{
			UnityEngine.Object.Instantiate<GameObject>(this.explodedPrefab, base.transform.position, base.transform.rotation);
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	
	public GameObject explodedPrefab;
}
