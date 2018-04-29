using System;
using UnityEngine;


public class enableNitrogenRelay : MonoBehaviour
{
	
	private void Start()
	{
	}

	
	private void enableNitrogenExplosion()
	{
		if (this.nitrogenScript)
		{
			this.nitrogenScript.enableNitrogenExplosion();
		}
	}

	
	public nitrogenExplode nitrogenScript;
}
