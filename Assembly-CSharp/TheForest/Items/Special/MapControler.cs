using System;
using TheForest.Utils;

namespace TheForest.Items.Special
{
	public class MapControler : SpecialItemControlerBase
	{
		public override bool ToggleSpecial(bool enable)
		{
			if (enable)
			{
				enable = (!LocalPlayer.FpCharacter.PushingSled && !LocalPlayer.WaterViz.InWater && !LocalPlayer.AnimControl.WaterBlock && !LocalPlayer.AnimControl.onRope && !LocalPlayer.AnimControl.endGameCutScene && !LocalPlayer.AnimControl.useRootMotion);
				Scene.HudGui.MapIcon.SetActive(enable);
			}
			else
			{
				Scene.HudGui.MapIcon.SetActive(false);
			}
			return true;
		}

		protected override void OnActivating()
		{
			if (!LocalPlayer.Animator.GetBool("drawBowBool") && !LocalPlayer.Create.CreateMode && !LocalPlayer.AnimControl.endGameCutScene && !LocalPlayer.AnimControl.useRootMotion)
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
	}
}
