using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.UI
{
	[AddComponentMenu("Items/UI/Item Amount Label")]
	public class ItemAmountLabel : MonoBehaviour
	{
		private void LateUpdate()
		{
			if (this._itemId > 0 && this._label.enabled && ItemDatabase.IsItemidValid(this._itemId))
			{
				int num = LocalPlayer.Inventory.AmountOf(this._itemId, false);
				if (num != this._displayedAmount)
				{
					this._displayedAmount = num;
					this._label.text = num.ToString();
				}
			}
		}

		[ItemIdPicker]
		public int _itemId;

		public UILabel _label;

		private int _displayedAmount = -1;
	}
}
