using System;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.World
{
	
	[AddComponentMenu("Items/World/Story PickUp")]
	public class StoryPickUp : PickUp
	{
		
		protected override bool MainEffect()
		{
			bool flag = LocalPlayer.Stats.CheckItem(Item.EquipmentSlot.RightHand);
			if (flag)
			{
				LocalPlayer.Inventory.MemorizeItem(Item.EquipmentSlot.RightHand);
			}
			InventoryItemView inventoryItemView = LocalPlayer.Inventory.EquipmentSlotsPrevious[0];
			if (this._forceAutoEquip)
			{
				LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.RightHand);
			}
			LocalPlayer.Inventory.Equip(this._itemId, LocalPlayer.Inventory.HasRoomFor(this._itemId, 1));
			LocalPlayer.Inventory.EquipmentSlotsPrevious[0] = inventoryItemView;
			if (FMOD_StudioSystem.instance)
			{
				FMOD_StudioSystem.instance.PlayOneShot("event:/music/toy_pickup", LocalPlayer.Transform.position, null);
			}
			return true;
		}

		
		protected const string PICKUP_EVENT = "event:/music/toy_pickup";
	}
}
