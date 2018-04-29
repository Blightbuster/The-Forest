using System;
using UnityEngine;


public class KillSpider : MonoBehaviour
{
	
	private void Hit()
	{
		this.KillMe();
	}

	
	private void KillMe()
	{
		UnityEngine.Object.Instantiate(this.Gore, base.transform.position, base.transform.rotation);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	
	public GameObject Gore;
}
