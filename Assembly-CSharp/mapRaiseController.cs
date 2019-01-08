using System;
using TheForest.Items;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

public class mapRaiseController : MonoBehaviour
{
	private void OnDisable()
	{
		LocalPlayer.Animator.SetBool("attack", false);
	}

	private void Update()
	{
		if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World)
		{
			if (UnityEngine.Input.GetButton("Fire1") && LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._mapId))
			{
				LocalPlayer.Animator.SetBool("attack", true);
			}
			else
			{
				LocalPlayer.Animator.SetBool("attack", false);
			}
		}
	}

	[ItemIdPicker]
	public int _mapId;
}
