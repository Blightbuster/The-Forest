using System;
using UnityEngine;


public class CoopDisabledOtherOnEnable : MonoBehaviour
{
	
	private void OnEnable()
	{
		if (BoltNetwork.isRunning)
		{
			if (this.OnlyOnClient && BoltNetwork.isServer)
			{
				return;
			}
			this.Other.gameObject.SetActive(false);
		}
	}

	
	public bool OnlyOnClient;

	
	public GameObject Other;
}
