using System;
using UnityEngine;


public class endPlaneAnimEvents : MonoBehaviour
{
	
	private void enableWingExplosion()
	{
		this.explosion.SetActive(true);
	}

	
	public GameObject explosion;
}
