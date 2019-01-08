using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.Special
{
	public class FlashLightControler : SpecialItemControlerBase
	{
		public override bool ToggleSpecial(bool enable)
		{
			if (enable)
			{
				if (LocalPlayer.Stats.BatteryCharge <= 0f)
				{
					if (this._nextBatteryUpsurge >= Time.time)
					{
						return false;
					}
					LocalPlayer.Stats.BatteryCharge = this._batteryUpsurgeValue;
				}
				if (LocalPlayer.Inventory.LastLight != this)
				{
					LocalPlayer.Inventory.StashLeftHand();
					LocalPlayer.Inventory.LastLight = this;
				}
				LocalPlayer.Tuts.HideLighter();
			}
			return true;
		}

		public override void PickedUpSpecialItem(int itemId)
		{
			if (itemId == this._itemId)
			{
				base.enabled = true;
				if (LocalPlayer.Inventory.AmountOf(this._itemId, false) > 1)
				{
					LocalPlayer.Inventory.RemoveItem(this._itemId, 1, false, true);
					LocalPlayer.Inventory.AddItem(this._batteryItemId, 1, false, false, null);
				}
			}
		}

		protected override void OnActivating()
		{
			if (LocalPlayer.Inventory.LastLight == this && !LocalPlayer.Animator.GetBool("drawBowBool"))
			{
				LocalPlayer.Inventory.TurnOnLastLight();
			}
		}

		protected override void OnDeactivating()
		{
			if (LocalPlayer.Inventory.LastLight == this)
			{
				LocalPlayer.Inventory.StashLeftHand();
			}
		}

		protected override bool IsActive
		{
			get
			{
				return LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.LeftHand, LocalPlayer.Inventory.LastLight._itemId);
			}
		}

		[ItemIdPicker(Item.Types.Edible)]
		public int _batteryItemId;

		public float _batteryUpsurgeDelay = 10f;

		public float _batteryUpsurgeValue = 0.5f;

		private float _nextBatteryUpsurge;
	}
}
