using System;
using UnityEngine;


public class spawnParticleController : MonoBehaviour
{
	
	private void Start()
	{
	}

	
	private void OnEnable()
	{
		this.spawnedPrefab = (UnityEngine.Object.Instantiate(this.go, base.transform.position, base.transform.rotation) as GameObject);
		this.spawnedPrefab.transform.parent = base.transform;
	}

	
	private void OnDisable()
	{
		if (this.spawnedPrefab)
		{
			UnityEngine.Object.Destroy(this.spawnedPrefab);
		}
	}

	
	public GameObject go;

	
	private GameObject spawnedPrefab;
}
