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
			where iv.Properties.ActiveBonus > (WeaponStatUpgrade.Types)(-1) && iv.gameObject.activeSelf
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
					List<InventoryItemView> views = LocalPlayer.Inventory.InventoryItemViewsCache[itemData._itemid];
					int i = 0;
					int setBonusMax = Mathf.Min(itemData._viewsData.Length, views.Count);
					int unsetBonusMax = views.Count;
					while (i < setBonusMax)
					{
						views[i].Properties.ActiveBonusValue = itemData._viewsData[i]._activeBonusValue;
						if (views[i] is ISaveableView)
						{
							(views[i] as ISaveableView).ApplySavedData(itemData._viewsData[i]);
						}
						views[i].ActiveBonus = itemData._viewsData[i]._activeBonus;
						i++;
					}
					while (i < unsetBonusMax)
					{
						views[i].Properties.ActiveBonusValue = 0f;
						if (views[i] is ISaveableView)
						{
							(views[i] as ISaveableView).ApplySavedData(null);
						}
						views[i].ActiveBonus = (WeaponStatUpgrade.Types)(-1);
						i++;
					}
				}
			}
			this._itemsData = null;
			if (this._itemsProperties != null)
			{
				foreach (ActiveBonusMassSaver.ItemData2 itemProperties in this._itemsProperties)
				{
					List<InventoryItemView> views2 = LocalPlayer.Inventory.InventoryItemViewsCache[itemProperties._itemid];
					int j = 0;
					int setBonusMax2 = Mathf.Min(itemProperties._viewsData.Length, views2.Count);
					int unsetBonusMax2 = views2.Count;
					while (j < setBonusMax2)
					{
						views2[j].Properties.Copy(itemProperties._viewsData[j]);
						j++;
					}
					while (j < unsetBonusMax2)
					{
						views2[j].Properties.Copy(ItemProperties.Any);
						j++;
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
