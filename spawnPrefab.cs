using System;
using UnityEngine;


public class spawnPrefab : MonoBehaviour
{
	
	private void Start()
	{
		if (!this.defaultBehaviour)
		{
			this.init = true;
		}
	}

	
	private void OnEnable()
	{
		if (!this.init)
		{
			return;
		}
		if (this.defaultBehaviour)
		{
			UnityEngine.Object.Instantiate(this.go, base.transform.position, base.transform.rotation);
		}
		else
		{
			UnityEngine.Object.Instantiate(Resources.Load("effects/WoodChopBurst"), base.transform.position, base.transform.rotation);
		}
	}

	
	public GameObject go;

	
	private bool init;

	
	public bool defaultBehaviour;
}
