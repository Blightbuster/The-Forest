using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.World
{
	
	public class ItemCost : MonoBehaviour
	{
		
		private void Awake()
		{
			this._iconPos = this._icon.transform.localPosition;
			if (!PlayerPreferences.ShowHud)
			{
				this._icon.gameObject.SetActive(false);
			}
		}

		
		private void OnEnable()
		{
			this._icon.transform.position = this._iconPos;
		}

		
		private void Update()
		{
			bool flag = this._currentAmount < this._totalRequiredAmount;
			bool flag2 = flag && LocalPlayer.Inventory.Owns(this._itemId, true);
			this._icon.color = ((flag && !flag2) ? this._red : this._white);
			if (flag2 && TheForest.Utils.Input.GetButtonDown("Take") && LocalPlayer.Inventory.RemoveItem(this._itemId, 1, false, true))
			{
				this._currentAmount++;
				LocalPlayer.Sfx.PlayWhoosh();
			}
			this.UpdateDisplay(this._currentAmount, this._totalRequiredAmount);
		}

		
		public void UpdateDisplay(int amount, int total)
		{
			if (amount == total || !PlayerPreferences.ShowHud)
			{
				if (this._icon.gameObject.activeSelf)
				{
					this._icon.gameObject.SetActive(false);
				}
			}
			else if (this._displayedCount != amount || this._displayedTotalCount != total)
			{
				this._text.text = amount + "/" + total;
				this._displayedCount = amount;
				this._displayedTotalCount = total;
				if (!this._icon.gameObject.activeSelf)
				{
					this._icon.gameObject.SetActive(true);
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

		
		public GUITexture _icon;

		
		public GUIText _text;

		
		public Color _white;

		
		public Color _red;

		
		[ItemIdPicker]
		public int _itemId;

		
		public int _currentAmount;

		
		public int _totalRequiredAmount = 1;

		
		private Vector3 _iconPos;

		
		private int _displayedCount;

		
		private int _displayedTotalCount;
	}
}
