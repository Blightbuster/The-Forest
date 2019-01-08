using System;
using System.Collections;
using TheForest.Items.Core;
using TheForest.Items.Craft;
using TheForest.Items.Inventory;
using TheForest.Utils;

namespace TheForest.Items.Special
{
	public class MetalTinTrayControler : SpecialItemControlerBase
	{
		public override bool ToggleSpecial(bool enable)
		{
			if (enable)
			{
				LocalPlayer.Inventory._craftingCog.Storage = null;
			}
			else if (!this._storage.Equals(LocalPlayer.Inventory._craftingCog.Storage))
			{
				this.EmptyToInventory();
			}
			return true;
		}

		public override bool ToggleSpecialCraft(bool enable)
		{
			CraftingCog craftingCog = LocalPlayer.Inventory._craftingCog;
			if (enable)
			{
				if (craftingCog.Storage == null || this._storage.Equals(craftingCog.Storage))
				{
					craftingCog.Storage = this._storage;
					this._storage.Open();
					return true;
				}
				return false;
			}
			else
			{
				if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Inventory && this._storage.Equals(craftingCog.Storage) && !LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._itemId) && !LocalPlayer.Inventory.HasInNextSlot(Item.EquipmentSlot.RightHand, this._itemId))
				{
					this.EmptyToInventory();
					craftingCog.Storage = null;
					return true;
				}
				if (this._storage.Equals(craftingCog.Storage))
				{
					craftingCog.Storage = null;
				}
				return false;
			}
		}

		private IEnumerator DelayedToggleSpecialCraft()
		{
			yield return null;
			if (!this._storage.Equals(LocalPlayer.Inventory.CurrentStorage))
			{
				LocalPlayer.Inventory.Open(this._storage);
			}
			yield break;
		}

		protected override void OnActivating()
		{
			if (!LocalPlayer.Animator.GetBool("drawBowBool"))
			{
				LocalPlayer.Inventory.Equip(this._itemId, false);
			}
		}

		protected override void OnDeactivating()
		{
			LocalPlayer.Inventory.StashEquipedWeapon(true);
		}

		protected override bool IsActive
		{
			get
			{
				return LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._itemId);
			}
		}

		public void EmptyToInventory()
		{
			for (int i = 0; i < this._storage.UsedSlots.Count; i++)
			{
				LocalPlayer.Inventory.AddItem(this._storage.UsedSlots[i]._itemId, this._storage.UsedSlots[i]._amount, true, true, this._storage.UsedSlots[i]._properties);
			}
			this._storage.Close();
			this._storage.UsedSlots.Clear();
			this._storage.UpdateContentVersion();
		}

		public ItemStorage _storage;
	}
}
