using System;
using System.Collections;
using TheForest.Tools;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.Special
{
	
	public class CompassControler : SpecialItemControlerBase
	{
		
		public override bool ToggleSpecial(bool enable)
		{
			if (enable && LocalPlayer.Inventory.LastUtility != this)
			{
				LocalPlayer.Inventory.StashLeftHand();
				LocalPlayer.Inventory.LastUtility = this;
				EventRegistry.Achievements.Publish(TfEvent.Achievements.UsingCompass, null);
			}
			return true;
		}

		
		protected override void OnActivating()
		{
			if (LocalPlayer.Inventory.LastUtility == this && !LocalPlayer.Animator.GetBool("drawBowBool"))
			{
				LocalPlayer.Inventory.TurnOnLastUtility(Item.EquipmentSlot.LeftHand);
			}
		}

		
		protected override void OnDeactivating()
		{
			if (LocalPlayer.Inventory.LastUtility == this)
			{
				base.StartCoroutine(this.DelayedStop());
			}
		}

		
		private IEnumerator DelayedStop()
		{
			this.ToggleSpecial(false);
			LocalPlayer.Sfx.PlayWhoosh();
			LocalPlayer.Animator.SetBoolReflected("pedHeld", false);
			yield return new WaitForSeconds(1f);
			if (this.IsActive)
			{
				LocalPlayer.Inventory.SkipNextAddItemWoosh = true;
				LocalPlayer.Inventory.StashLeftHand();
			}
			yield break;
		}

		
		
		protected override bool IsActive
		{
			get
			{
				return LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.LeftHand, LocalPlayer.Inventory.LastUtility._itemId);
			}
		}
	}
}
