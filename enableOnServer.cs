using System;
using UnityEngine;


public class enableOnServer : MonoBehaviour
{
	
	private void Start()
	{
		if (BoltNetwork.isRunning && BoltNetwork.isServer)
		{
			foreach (GameObject gameObject in this.enableGo)
			{
				if (gameObject)
				{
					gameObject.SetActive(true);
				}
			}
		}
	}

	
	public GameObject[] enableGo;
}
