using System;
using TheForest.Items.Inventory;
using UnityEngine;

namespace TheForest.Buildings.World
{
	[DoNotSerializePublic]
	public class StorageItemBonusManager : MonoBehaviour
	{
		public void Init(int storageSize)
		{
			this._itemsProperties = new StorageItemBonusManager.ItemData[storageSize];
		}

		public void SetItemProperties(int index, int itemId, ItemProperties prop)
		{
			this._itemsProperties[index] = new StorageItemBonusManager.ItemData
			{
				_itemid = itemId,
				_properties = prop.Clone()
			};
		}

		public void UnsetItemProperties(int index)
		{
			this._itemsProperties[index] = null;
		}

		public ItemProperties GetItemProperties(int index)
		{
			return (this._itemsProperties[index] != null) ? this._itemsProperties[index]._properties : ItemProperties.Any;
		}

		[SerializeThis]
		public StorageItemBonusManager.ItemData[] _itemsProperties;

		[Serializable]
		public class ItemData
		{
			public int _itemid;

			public ItemProperties _properties;
		}
	}
}
