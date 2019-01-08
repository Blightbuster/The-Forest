using System;
using TheForest.Items.World;
using TheForest.Utils;
using UnityEngine;

public class VRLightThrowableController : MonoBehaviour
{
	private void OnDisable()
	{
		this.InFireTrigger = false;
	}

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
			if (Time.time > this.nextFireCheck && LocalPlayer.Inventory.RightHand._held != null && LocalPlayer.Inventory.RightHand._held.activeSelf)
			{
				if (this.burnableWeapon)
				{
					BurnableCloth componentInChildren = LocalPlayer.Inventory.RightHand._held.GetComponentInChildren<BurnableCloth>();
					if (componentInChildren && !componentInChildren._player.IsWeaponBurning)
					{
						componentInChildren.SendMessage("Light", SendMessageOptions.DontRequireReceiver);
					}
				}
				else
				{
					LocalPlayer.Inventory.RightHand._held.SendMessage("setIsLit", SendMessageOptions.DontRequireReceiver);
					LocalPlayer.Inventory.RightHand._held.SendMessage("enableFire", SendMessageOptions.DontRequireReceiver);
					LocalPlayer.Inventory.UseAltWorldPrefab = false;
				}
			}
		}
		else
		{
			this.nextFireCheck = Time.time + 1.3f;
		}
	}

	public bool burnableWeapon;

	private bool InFireTrigger;

	private float nextFireCheck;
}
