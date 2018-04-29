using System;
using TheForest.Items;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.UI
{
	
	public class ItemListWidgetEntry : MonoBehaviour
	{
		
		private void Reset()
		{
			this._icon = base.GetComponent<UIWidget>();
			this._amountLabel = base.GetComponentInChildren<UILabel>();
		}

		
		public bool MatchItemId(int itemid)
		{
			foreach (int num in this._itemIds)
			{
				if (itemid == num)
				{
					return true;
				}
			}
			return false;
		}

		
		public bool OwnsAny()
		{
			foreach (int num in this._itemIds)
			{
				if ((num == -1 && LocalPlayer.AnimControl.carry) || LocalPlayer.Inventory.Owns(num, false))
				{
					return true;
				}
			}
			return false;
		}

		
		[ItemIdPicker]
		public int[] _itemIds;

		
		public UIWidget _icon;

		
		public GameObject _actionIcon;

		
		public UILabel _amountLabel;

		
		public int _lastAmountDisplayed = -1;

		
		public UIWidget _takeIcon;

		
		public UIWidget _craftIcon;

		
		public bool _useFillSprite;
	}
}
