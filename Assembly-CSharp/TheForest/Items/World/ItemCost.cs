using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.World
{
	public class ItemCost : MonoBehaviour
	{
		private void OnEnable()
		{
			if (PlayerPreferences.ShowHud && !this.IsCompleted)
			{
				Scene.HudGui.TrapReArmIngredientsFollow.SetTarget(base.transform, null);
			}
		}

		private void OnDisable()
		{
			Scene.HudGui.TrapReArmIngredientsFollow.Shutdown();
		}

		private void Update()
		{
			bool flag = this._currentAmount < this._totalRequiredAmount;
			bool flag2 = flag && LocalPlayer.Inventory.Owns(this._itemId, true);
			if (!flag || flag2)
			{
				Scene.HudGui.TrapReArmIngredients.SetAvailableIngredientColor();
			}
			else
			{
				Scene.HudGui.TrapReArmIngredients.SetMissingIngredientColor();
			}
			if (flag2 && TheForest.Utils.Input.GetButtonDown("Take") && LocalPlayer.Inventory.RemoveItem(this._itemId, 1, false, true))
			{
				this._currentAmount++;
				LocalPlayer.Sfx.PlayItemCustomSfx(this._itemId, true);
				if (this._currentAmount >= this._totalRequiredAmount)
				{
					base.enabled = false;
				}
			}
		}

		public bool IsCompleted
		{
			get
			{
				return this._currentAmount >= this._totalRequiredAmount;
			}
		}

		[ItemIdPicker]
		public int _itemId;

		public int _currentAmount;

		public int _totalRequiredAmount = 1;

		private int _displayedCount;

		private int _displayedTotalCount;
	}
}
