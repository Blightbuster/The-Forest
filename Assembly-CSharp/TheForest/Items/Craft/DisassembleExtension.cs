using System;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.Craft
{
	public class DisassembleExtension : MonoBehaviour
	{
		private void Update()
		{
			if (!LocalPlayer.Inventory._craftingCog._upgradeCog.enabled && LocalPlayer.Inventory._craftingCog.Ingredients.Count == 1 && this._host._isCraft)
			{
				if (TheForest.Utils.Input.GetButtonDown("Craft"))
				{
					LocalPlayer.Sfx.PlayWhoosh();
					if (LocalPlayer.Inventory.RemoveItem(this._extension._itemId, 1, false, false))
					{
						LocalPlayer.Inventory._craftingCog.Add(this._returnedItemId, 1, null);
						base.gameObject.SetActive(false);
						Scene.HudGui.DisassembleInfo.SetActive(false);
						return;
					}
				}
				if (!Scene.HudGui.DisassembleInfo.activeSelf)
				{
					Scene.HudGui.DisassembleInfo.SetActive(true);
				}
			}
		}

		private void OnDisable()
		{
			if (this._host._isCraft && Scene.HudGui.DisassembleInfo.activeSelf)
			{
				Scene.HudGui.DisassembleInfo.SetActive(false);
			}
		}

		public InventoryItemView _host;

		public InventoryItemView _extension;

		[ItemIdPicker]
		public int _returnedItemId;
	}
}
