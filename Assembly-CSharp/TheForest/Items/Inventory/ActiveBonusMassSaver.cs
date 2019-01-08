using System;
using System.Collections;
using System.Collections.Generic;
using TheForest.Items.Craft;
using TheForest.Utils;
using UniLinq;
using UnityEngine;

namespace TheForest.Items.Inventory
{
	[DoNotSerializePublic]
	public class ActiveBonusMassSaver : MonoBehaviour
	{
		private void OnDestroy()
		{
			this._itemsData = null;
		}

		private IEnumerator OnSerializing()
		{
			this._itemsProperties = (from iv in LocalPlayer.Inventory._itemViews
			where iv && iv.Properties.ActiveBonus > (WeaponStatUpgrade.Types)(-1) && iv.gameObject.activeSelf
			group iv by iv._itemId).Select(delegate(IGrouping<int, InventoryItemView> g)
			{
				ActiveBonusMassSaver.ItemData2 itemData = new ActiveBonusMassSaver.ItemData2();
				itemData._itemid = g.Key;
				itemData._viewsData = (from iv in g
				select iv.Properties).ToArray<ItemProperties>();
				return itemData;
			}).ToArray<ActiveBonusMassSaver.ItemData2>();
			yield return null;
			this._itemsData = null;
			yield break;
		}

		private IEnumerator OnDeserialized()
		{
			yield return null;
			if (this._itemsData != null)
			{
				foreach (ActiveBonusMassSaver.ItemData itemData in this._itemsData)
				{
					List<InventoryItemView> list = LocalPlayer.Inventory.InventoryItemViewsCache[itemData._itemid];
					int j = 0;
					int num = Mathf.Min(itemData._viewsData.Length, list.Count);
					int count = list.Count;
					while (j < num)
					{
						list[j].Properties.ActiveBonusValue = itemData._viewsData[j]._activeBonusValue;
						if (list[j] is ISaveableView)
						{
							(list[j] as ISaveableView).ApplySavedData(itemData._viewsData[j]);
						}
						list[j].ActiveBonus = itemData._viewsData[j]._activeBonus;
						j++;
					}
					while (j < count)
					{
						list[j].Properties.ActiveBonusValue = 0f;
						if (list[j] is ISaveableView)
						{
							(list[j] as ISaveableView).ApplySavedData(null);
						}
						list[j].ActiveBonus = (WeaponStatUpgrade.Types)(-1);
						j++;
					}
				}
			}
			this._itemsData = null;
			if (this._itemsProperties != null)
			{
				foreach (ActiveBonusMassSaver.ItemData2 itemData2 in this._itemsProperties)
				{
					List<InventoryItemView> list2 = LocalPlayer.Inventory.InventoryItemViewsCache[itemData2._itemid];
					int l = 0;
					int num2 = Mathf.Min(itemData2._viewsData.Length, list2.Count);
					int count2 = list2.Count;
					while (l < num2)
					{
						list2[l].Properties.Copy(itemData2._viewsData[l]);
						l++;
					}
					while (l < count2)
					{
						list2[l].Properties.Copy(ItemProperties.Any);
						l++;
					}
				}
			}
			this._itemsProperties = null;
			yield break;
		}

		[SerializeThis]
		public ActiveBonusMassSaver.ItemData[] _itemsData;

		[SerializeThis]
		public ActiveBonusMassSaver.ItemData2[] _itemsProperties;

		[Serializable]
		public class ItemData
		{
			public int _itemid;

			public ActiveBonusMassSaver.ViewData[] _viewsData;
		}

		[Serializable]
		public class ViewData
		{
			[SerializeThis]
			public WeaponStatUpgrade.Types _activeBonus = (WeaponStatUpgrade.Types)(-1);

			[SerializeThis]
			public float _activeBonusValue;

			[SerializeThis]
			public int _intValue1;

			[SerializeThis]
			public float _floatValue1;
		}

		[Serializable]
		public class ItemData2
		{
			public int _itemid;

			public ItemProperties[] _viewsData;
		}
	}
}
