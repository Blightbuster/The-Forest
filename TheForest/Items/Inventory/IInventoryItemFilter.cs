﻿using System;

namespace TheForest.Items.Inventory
{
	
	public interface IInventoryItemFilter
	{
		
		bool Owns(int itemId, bool allowFallback);

		
		int AmountOf(int itemId, bool allowFallback);

		
		bool AddItem(int itemId, int amount, bool preventAutoEquip, bool fromCraftingCog, ItemProperties properties);

		
		bool RemoveItem(int itemId, int amount, bool allowAmountOverflow);
	}
}
