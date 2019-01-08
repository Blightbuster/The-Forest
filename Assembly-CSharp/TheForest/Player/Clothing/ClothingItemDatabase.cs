using System;
using System.Collections.Generic;
using TheForest.Utils;
using UniLinq;
using UnityEngine;

namespace TheForest.Player.Clothing
{
	public class ClothingItemDatabase : ScriptableObject
	{
		public void OnEnable()
		{
			base.hideFlags = HideFlags.None;
			if (ClothingItemDatabase._instance == null)
			{
				ClothingItemDatabase._instance = this;
				this._itemCollection = this._items.AsEnumerable<ClothingItem>();
				this._itemsCache = this._items.ToDictionary((ClothingItem item) => item._id);
			}
		}

		public static ClothingItem[] Items
		{
			get
			{
				return ClothingItemDatabase._instance._items;
			}
		}

		public static ClothingItem ClothingItemById(int id)
		{
			return ClothingItemDatabase._instance._itemsCache[id];
		}

		public static IEnumerable<ClothingItem> ItemsByType(ClothingItem.DisplayTypes typeMask)
		{
			if (ClothingItemDatabase._instance._itemCollection != null)
			{
				return from i in ClothingItemDatabase._instance._itemCollection
				where (i._displayType & typeMask) != (ClothingItem.DisplayTypes)0
				select i;
			}
			return from i in ClothingItemDatabase._instance._items
			where (i._displayType & typeMask) != (ClothingItem.DisplayTypes)0
			select i;
		}

		private static Dictionary<ClothingItem, int> GetPonderatedClothingByTypeChance(ClothingItem.DisplayTypes typeMask, ClothingItem.DisplayTypes ignoreMask = (ClothingItem.DisplayTypes)0)
		{
			return (from ci in ClothingItemDatabase.ItemsByType(typeMask)
			where ci._displayType != ignoreMask
			select ci).ToDictionary((ClothingItem c) => c, (ClothingItem c) => Mathf.Max(c._baseRollChance - LocalPlayer.Clothing.AmountOf(c._id) * c._ownedRollChanceReduction, 0));
		}

		public static List<int> GetRandomOutfit(bool allowBeanie)
		{
			List<int> list = new List<int>();
			Dictionary<ClothingItem, int> ponderatedClothingByTypeChance = ClothingItemDatabase.GetPonderatedClothingByTypeChance(ClothingItem.DisplayTypes.TShirt, (ClothingItem.DisplayTypes)0);
			Dictionary<ClothingItem, int> ponderatedClothingByTypeChance2 = ClothingItemDatabase.GetPonderatedClothingByTypeChance(ClothingItem.DisplayTypes.Pants, ClothingItem.DisplayTypes.FullBody);
			Dictionary<ClothingItem, int> ponderatedClothingByTypeChance3 = ClothingItemDatabase.GetPonderatedClothingByTypeChance(ClothingItem.DisplayTypes.TopPartial_Hands | ClothingItem.DisplayTypes.TopPartial_Arms | ClothingItem.DisplayTypes.TopFull_Hands | ClothingItem.DisplayTypes.TopFull_Arms, (ClothingItem.DisplayTypes)0);
			int max = ponderatedClothingByTypeChance.Sum((KeyValuePair<ClothingItem, int> s) => s.Value);
			int max2 = ponderatedClothingByTypeChance2.Sum((KeyValuePair<ClothingItem, int> s) => s.Value);
			int max3 = (int)((float)ponderatedClothingByTypeChance3.Sum((KeyValuePair<ClothingItem, int> s) => s.Value) * 1.5f);
			int tshirtRand = UnityEngine.Random.Range(0, max);
			int pantsRand = UnityEngine.Random.Range(0, max2);
			int topRand = UnityEngine.Random.Range(0, max3);
			KeyValuePair<ClothingItem, int> keyValuePair = ponderatedClothingByTypeChance.SkipWhile(delegate(KeyValuePair<ClothingItem, int> c)
			{
				tshirtRand -= c.Value;
				return tshirtRand > 0;
			}).FirstOrDefault<KeyValuePair<ClothingItem, int>>();
			KeyValuePair<ClothingItem, int> keyValuePair2 = ponderatedClothingByTypeChance2.SkipWhile(delegate(KeyValuePair<ClothingItem, int> c)
			{
				pantsRand -= c.Value;
				return pantsRand > 0;
			}).FirstOrDefault<KeyValuePair<ClothingItem, int>>();
			KeyValuePair<ClothingItem, int> keyValuePair3 = ponderatedClothingByTypeChance3.SkipWhile(delegate(KeyValuePair<ClothingItem, int> c)
			{
				topRand -= c.Value;
				return topRand > 0;
			}).FirstOrDefault<KeyValuePair<ClothingItem, int>>();
			ClothingItem key = keyValuePair.Key;
			ClothingItem key2 = keyValuePair2.Key;
			ClothingItem key3 = keyValuePair3.Key;
			if (key3 != null && (key3._id != 9 || (AccountInfo.StoryCompleted && LocalPlayer.Clothing.AmountOf(9) == 0)))
			{
				if (key3._displayType != ClothingItem.DisplayTypes.FullBody)
				{
					allowBeanie = false;
				}
				list.Add(key3._id);
			}
			if (key3 == null || key3._displayType != ClothingItem.DisplayTypes.FullBody)
			{
				if (key != null)
				{
					ClothingItemDatabase.CombineClothingItemWithOutfit(list, key);
				}
				if (key2 != null)
				{
					ClothingItemDatabase.CombineClothingItemWithOutfit(list, key2);
				}
			}
			if (allowBeanie)
			{
				IEnumerable<ClothingItem> source = ClothingItemDatabase.ItemsByType(ClothingItem.DisplayTypes.Hat);
				int num = ponderatedClothingByTypeChance.Count<KeyValuePair<ClothingItem, int>>();
				int hatRand = UnityEngine.Random.Range(0, num * 3);
				if (hatRand < num)
				{
					ClothingItem clothingItem = source.Where((ClothingItem ci, int i) => i == hatRand).FirstOrDefault<ClothingItem>();
					if (clothingItem != null)
					{
						ClothingItemDatabase.CombineClothingItemWithOutfit(list, clothingItem);
					}
				}
			}
			return list;
		}

		public static bool CombineClothingItemWithOutfit(List<int> outfitItemIds, ClothingItem addingCloth)
		{
			for (int i = outfitItemIds.Count - 1; i >= 0; i--)
			{
				ClothingItem clothingItem = ClothingItemDatabase.ClothingItemById(outfitItemIds[i]);
				if (clothingItem == null)
				{
					Debug.LogError("Null worn cloth id =" + outfitItemIds[i]);
				}
				else if (!addingCloth.CompatibleWith(clothingItem._slotType) && (addingCloth._slotType != ClothingItem.SlotTypes.Exclusive || (clothingItem._slotType != ClothingItem.SlotTypes.Bottom && clothingItem._slotType != ClothingItem.SlotTypes.Top)))
				{
					return false;
				}
			}
			outfitItemIds.Add(addingCloth._id);
			return true;
		}

		[NameFromProperty("_name", 50)]
		public ClothingItem[] _items;

		private static ClothingItemDatabase _instance;

		private Dictionary<int, ClothingItem> _itemsCache;

		private IEnumerable<ClothingItem> _itemCollection;
	}
}
