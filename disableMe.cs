using System;
using UnityEngine;


public class disableMe : MonoBehaviour
{
	
	private void Start()
	{
		base.Invoke("doDisable", this.delay);
	}

	
	private void doDisable()
	{
		base.gameObject.SetActive(false);
	}

	
	public float delay;
}
