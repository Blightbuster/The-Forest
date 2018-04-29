using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.Special
{
	
	public class TalkyWalkyControler : SpecialItemControlerBase
	{
		
		public override bool ToggleSpecial(bool enable)
		{
			if (Scene.HudGui.TalkyWalkyInfo.activeSelf != enable)
			{
				Scene.HudGui.TalkyWalkyInfo.SetActive(enable);
			}
			return true;
		}

		
		protected override void OnActivating()
		{
			if (!LocalPlayer.Animator.GetBool("drawBowBool") && !LocalPlayer.AnimControl.endGameCutScene)
			{
				LocalPlayer.Inventory.Equip(this._itemId, false);
			}
		}

		
		protected override void OnDeactivating()
		{
			base.StartCoroutine(this.DelayedStop(false));
		}

		
		private IEnumerator DelayedStop(bool equipPrevious)
		{
			this.ToggleSpecial(false);
			LocalPlayer.Sfx.PlayItemCustomSfx(this._itemId, true);
			LocalPlayer.Animator.SetBoolReflected("pedHeld", false);
			yield return new WaitForSeconds(1f);
			LocalPlayer.Inventory.SkipNextAddItemWoosh = true;
			if (equipPrevious && LocalPlayer.Inventory.EquipmentSlotsPrevious[1] != null)
			{
				LocalPlayer.Inventory.EquipPreviousUtility(false);
			}
			else
			{
				LocalPlayer.Inventory.StashLeftHand();
			}
			yield break;
		}
	}
}
