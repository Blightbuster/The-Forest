using System;
using TheForest.Items.Craft;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.Inventory
{
	
	[AddComponentMenu("Items/Inventory/WaterSkin Inventory View")]
	[DoNotSerializePublic]
	public class WaterSkinInventoryItemView : InventoryItemView
	{
		
		protected void Awake()
		{
			this.Init();
		}

		
		public override void UseEdible()
		{
			InventoryItemView.CombiningItemId = -1;
			if (LocalPlayer.Stats.Thirst > 0.05f)
			{
				if (this._properties.ActiveBonus == WeaponStatUpgrade.Types.DirtyWater)
				{
					LocalPlayer.Stats.HitWaterDelayed(1);
				}
				float num = Mathf.Min(this.Properties.ActiveBonusValue, LocalPlayer.Stats.Thirst);
				LocalPlayer.Stats.Thirst -= num * LocalPlayer.Stats.FoodPoisoning.EffectRatio;
				this.Properties.ActiveBonusValue -= num;
				if (this.Properties.ActiveBonusValue < 0.25f)
				{
					this.ActiveBonus = (WeaponStatUpgrade.Types)(-1);
				}
				if (base.ItemCache._usedSFX != Item.SFXCommands.None)
				{
					this._inventory.SendMessage("PlayInventorySound", base.ItemCache._usedSFX);
				}
				Scene.HudGui.HideItemInfoView(this._itemId, this._isCraft);
				Scene.HudGui.ShowItemInfoViewDelayed(this, (this._renderers.Length <= 0) ? null : this._renderers[0]._renderer, this._isCraft);
			}
			else
			{
				LocalPlayer.Sfx.PlayItemCustomSfx(base.ItemCache, true);
			}
		}
	}
}
