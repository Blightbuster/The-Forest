using System;
using System.Collections;
using System.Collections.Generic;
using TheForest.Items.Craft;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UniLinq;
using UnityEngine;

namespace TheForest.Items.Core
{
	
	[DoNotSerializePublic]
	[AddComponentMenu("Buildings/World/Item Stash")]
	public class ItemStorage : MonoBehaviour, IItemStorage
	{
		
		private void Start()
		{
			base.enabled = false;
		}

		
		private void OnSerializing()
		{
			this._usedSlots.Capacity = this._usedSlots.Count;
			this._usedSlotsCount = this._usedSlots.Count;
		}

		
		private void OnDeserialized()
		{
			this._usedSlots.RemoveRange(this._usedSlotsCount, this._usedSlots.Count - this._usedSlotsCount);
		}

		
		private void UpdateFillIcon()
		{
			Scene.HudGui.StorageFill.fillAmount = (float)this._usedSlots.Count / (float)this._slotCount;
		}

		
		public void UpdateContentVersion()
		{
			this.ContentVersion = Mathf.FloorToInt(Mathf.Repeat((float)(this.ContentVersion + 1), 13371337f));
		}

		
		
		public bool IsEmpty
		{
			get
			{
				return this._usedSlots.Count == 0;
			}
		}

		
		public int Add(int itemId, int amount = 1, ItemProperties properties = null)
		{
			if (!this.IsValidItem(itemId))
			{
				return amount;
			}
			float num = 0f;
			int maxAmount = (num != 0f) ? Mathf.FloorToInt(1f / num) : int.MaxValue;
			ItemStorage.InventoryItemX inventoryItemX = this._usedSlots.LastOrDefault((ItemStorage.InventoryItemX s) => s._itemId == itemId && s._properties.Match(properties));
			if (inventoryItemX != null)
			{
				this.UpdateContentVersion();
				amount = inventoryItemX.Add(amount, false);
			}
			while (amount > 0 && (this._usedSlots.Count < this._slotCount || this._slotCount == 0))
			{
				inventoryItemX = new ItemStorage.InventoryItemX
				{
					_itemId = itemId,
					_maxAmount = maxAmount,
					_properties = ((properties == null) ? new ItemProperties
					{
						ActiveBonus = (WeaponStatUpgrade.Types)(-1)
					} : properties.Clone())
				};
				amount = inventoryItemX.Add(amount, false);
				this._usedSlots.Add(inventoryItemX);
				this.UpdateContentVersion();
			}
			this.UpdateFillIcon();
			return amount;
		}

		
		public bool IsValidItem(int itemId)
		{
			Item item = ItemDatabase.ItemById(itemId);
			return item != null && this.IsValidItem(item);
		}

		
		public bool IsValidItem(Item item)
		{
			return this._whiteList.Contains(item._id) || (!this._blacklist.Contains(item._id) && this.IsWhiteListedType(item) && !this.IsBlackListedType(item));
		}

		
		private bool IsWhiteListedType(Item item)
		{
			return this.MatchType(this._acceptedTypes, item._type);
		}

		
		private bool IsBlackListedType(Item item)
		{
			return this.MatchType(this._blackListedTypes, item._type);
		}

		
		private bool MatchType(Item.Types mask, Item.Types type)
		{
			return (mask & type) != (Item.Types)0;
		}

		
		public int Remove(int itemId, int amount = 1, ItemProperties properties = null)
		{
			ItemStorage.InventoryItemX inventoryItemX;
			while (amount > 0 && (inventoryItemX = this._usedSlots.LastOrDefault((ItemStorage.InventoryItemX s) => s._itemId == itemId && s._properties.Match(properties))) != null)
			{
				amount = inventoryItemX.RemoveOverflow(amount);
				if (inventoryItemX._amount == 0)
				{
					int index = this._usedSlots.IndexOf(inventoryItemX);
					this._usedSlots.RemoveAt(index);
					this.UpdateContentVersion();
				}
			}
			this.UpdateFillIcon();
			return amount;
		}

		
		public void Open()
		{
			base.StartCoroutine(this.DelayedOpen());
		}

		
		private IEnumerator DelayedOpen()
		{
			yield return null;
			this.UpdateFillIcon();
			yield break;
		}

		
		public virtual void Close()
		{
		}

		
		
		
		public int ContentVersion { get; set; }

		
		
		public List<ItemStorage.InventoryItemX> UsedSlots
		{
			get
			{
				return this._usedSlots;
			}
		}

		
		[EnumFlags]
		public Item.Types _acceptedTypes;

		
		[EnumFlags]
		public Item.Types _blackListedTypes;

		
		public int _slotCount;

		
		[ItemIdPicker]
		public int[] _blacklist = new int[0];

		
		public int[] _whiteList = new int[0];

		
		[SerializeThis]
		protected List<ItemStorage.InventoryItemX> _usedSlots = new List<ItemStorage.InventoryItemX>();

		
		[SerializeThis]
		private int _usedSlotsCount;

		
		[Serializable]
		public class InventoryItemX : InventoryItem
		{
			
			public ItemProperties _properties;
		}
	}
}
