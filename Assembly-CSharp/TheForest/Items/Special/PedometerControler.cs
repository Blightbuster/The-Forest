using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.Special
{
	public class PedometerControler : SpecialItemControlerBase
	{
		private void Awake()
		{
			LocalPlayer.Inventory.LastUtility = this;
			PedometerControler.HasPedometer = false;
		}

		public override bool ToggleSpecial(bool enable)
		{
			if (enable && LocalPlayer.Inventory.LastUtility != this)
			{
				LocalPlayer.Inventory.StashLeftHand();
				LocalPlayer.Inventory.LastUtility = this;
			}
			if (Scene.HudGui.PedCam.activeSelf != enable)
			{
				Scene.HudGui.PedCam.SetActive(enable);
			}
			return true;
		}

		public override void PickedUpSpecialItem(int itemId)
		{
			if (itemId == this._itemId)
			{
				PedometerControler.HasPedometer = true;
				if (this._button != SpecialItemControlerBase.Buttons.None)
				{
					base.enabled = true;
				}
			}
		}

		public override void LostSpecialItem(int itemId)
		{
			if (itemId == this._itemId)
			{
				this._checkRemoval = true;
				PedometerControler.HasPedometer = false;
			}
		}

		protected override void OnActivating()
		{
			if (LocalPlayer.Inventory.LastUtility == this && !LocalPlayer.Animator.GetBool("drawBowBool") && !LocalPlayer.AnimControl.endGameCutScene)
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
			LocalPlayer.Sfx.PlayItemCustomSfx(this._itemId, true);
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

		public static bool HasPedometer { get; private set; }
	}
}
