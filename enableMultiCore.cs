using System;
using UnityEngine;


public class enableMultiCore : MonoBehaviour
{
	
	private void Start()
	{
		base.Invoke("enableThreading", 1f);
	}

	
	private void enableThreading()
	{
		this.Enabled = !this.Enabled;
	}

	
	private bool Enabled;
}
