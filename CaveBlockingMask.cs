using System;
using TheForest.Utils;
using UnityEngine;


public class CaveBlockingMask : MonoBehaviour
{
	
	private void Update()
	{
		if (LocalPlayer.IsInCaves)
		{
			base.gameObject.GetComponent<Renderer>().enabled = true;
		}
		else
		{
			base.gameObject.GetComponent<Renderer>().enabled = false;
		}
	}
}
