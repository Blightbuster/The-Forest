using System;
using UnityEngine;

public class VRTurnOn : MonoBehaviour
{
	private void Update()
	{
		if (ForestVR.Enabled)
		{
			this.ItemOn.SetActive(true);
		}
		else
		{
			this.ItemOn.SetActive(false);
		}
	}

	public GameObject ItemOn;
}
