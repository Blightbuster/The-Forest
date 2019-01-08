﻿using System;
using TheForest.Utils;

namespace TheForest.Items.Inventory
{
	public class InventoryItemFilter_Unlimited : IInventoryItemFilter
	{
		public bool AddItem(int itemId, int amount, bool preventAutoEquip, bool fromCraftingCog, ItemProperties properties)
		{
			return LocalPlayer.Inventory.AddItemNF(itemId, amount, preventAutoEquip, fromCraftingCog, properties);
		}

		public int AmountOf(int itemId, bool allowFallback)
		{
			return 100000;
		}

		public bool Owns(int itemId, bool allowfallback = true)
		{
			return true;
		}

		public bool RemoveItem(int itemId, int amount, bool allowAmountOverflow)
		{
			return true;
		}
	}
}
