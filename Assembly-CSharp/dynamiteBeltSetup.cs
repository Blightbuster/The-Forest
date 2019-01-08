using System;
using UnityEngine;

public class dynamiteBeltSetup : MonoBehaviour
{
	private void Start()
	{
	}

	public void enableBeltExplosion()
	{
		foreach (GameObject gameObject in this.enableGo)
		{
			gameObject.SetActive(true);
		}
	}

	public GameObject[] enableGo;
}
