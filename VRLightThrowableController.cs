using System;
using TheForest.Utils;
using UnityEngine;


public class VRLightThrowableController : MonoBehaviour
{
	
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("fire"))
		{
			this.InFireTrigger = true;
		}
	}

	
	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.CompareTag("fire"))
		{
			this.InFireTrigger = false;
		}
	}

	
	private void Update()
	{
		if (this.InFireTrigger)
		{
			if (Time.time > this.nextFireCheck && LocalPlayer.Inventory.RightHand._held.activeSelf)
			{
				LocalPlayer.Inventory.RightHand._held.SendMessage("setIsLit", SendMessageOptions.DontRequireReceiver);
				LocalPlayer.Inventory.RightHand._held.SendMessage("enableFire", SendMessageOptions.DontRequireReceiver);
				LocalPlayer.Inventory.UseAltWorldPrefab = false;
			}
		}
		else
		{
			this.nextFireCheck = Time.time + 1.3f;
		}
	}

	
	public bool InFireTrigger;

	
	private float nextFireCheck;
}
