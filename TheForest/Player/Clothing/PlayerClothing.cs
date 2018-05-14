using System;
using System.Collections.Generic;
using System.Linq;
using TheForest.Items;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Player.Clothing
{
	
	[DoNotSerializePublic]
	public class PlayerClothing : MonoBehaviour
	{
		
		
		public List<List<int>> AvailableClothingOutfits
		{
			get
			{
				return this._availableClothingOutfits;
			}
		}

		
		private void Awake()
		{
			if (ForestVR.Prototype)
			{
				base.enabled = false;
				return;
			}
			this._playerVariation.Database.OnEnable();
			if (!LevelSerializer.IsDeserializing)
			{
				this.CheckInit();
				if (!BoltNetwork.isRunning)
				{
					this.AddClothingOutfit(new List<int>
					{
						4,
						24
					}, true);
					this.RefreshVisibleClothing();
				}
			}
		}

		
		public void CheckInit()
		{
			if (this._availableClothingOutfits == null)
			{
				this._availableClothingOutfits = new List<List<int>>();
			}
			if (this._wornClothingItems == null)
			{
				this._wornClothingItems = new List<int>();
			}
		}

		
		public int AmountOf(int clothingId)
		{
			int num = 0;
			for (int i = 0; i < this._wornClothingItems.SafeCount<int>(); i++)
			{
				if (this._wornClothingItems[i] == clothingId)
				{
					num++;
					break;
				}
			}
			List<List<int>> availableClothingOutfits = this._availableClothingOutfits;
			for (int j = 0; j < availableClothingOutfits.SafeCount<List<int>>(); j++)
			{
				List<int> list = availableClothingOutfits[j];
				for (int k = 0; k < list.SafeCount<int>(); k++)
				{
					if (list[k] == clothingId)
					{
						num++;
						break;
					}
				}
			}
			return num;
		}

		
		private void OnSerializing()
		{
			this.PackClothingIndiciesAndCount();
		}

		
		private void PackClothingIndiciesAndCount()
		{
			this._availableClothingOutfits = (from outfit in this._availableClothingOutfits
			where outfit.SafeCount<int>() > 0
			select outfit).ToList<List<int>>();
			this._availableClothingOutfitCount = this.AvailableClothingOutfits.Count;
		}

		
		private void OnDeserialized()
		{
			this.CheckInit();
			if (this._availableClothingOutfits.Count > this._availableClothingOutfitCount)
			{
				this._availableClothingOutfits.RemoveRange(this._availableClothingOutfitCount, this._availableClothingOutfits.Count - this._availableClothingOutfitCount);
			}
			if (this._wornClothingItems.Count > this._wornClothingItemsCount)
			{
				this._wornClothingItems.RemoveRange(this._wornClothingItemsCount, this._wornClothingItems.Count - this._wornClothingItemsCount);
			}
			if (this._availableClothingOutfits.Count == 0 && this._wornClothingItems.Count == 0)
			{
				if (!BoltNetwork.isRunning)
				{
					this.AddClothingOutfit(new List<int>
					{
						4,
						24
					}, true);
				}
				else
				{
					this.AddClothingOutfit(ClothingItemDatabase.GetRandomOutfit(LocalPlayer.Stats.PlayerVariation == 0), true);
				}
			}
			this.RefreshVisibleClothing();
		}

		
		public bool AddClothingOutfit(List<int> ids, bool autoEquip = true)
		{
			this.CheckInit();
			if (this._availableClothingOutfitCount >= 5)
			{
				this.DropOutfit(0);
			}
			this._availableClothingOutfits.Add(ids);
			this.PackClothingIndiciesAndCount();
			return !autoEquip || this.SetWornOutfit(this._availableClothingOutfits.Count - 1);
		}

		
		public bool DropOutfit(int outfitNum)
		{
			if (outfitNum < this._availableClothingOutfits.Count)
			{
				this.FakeDrop(outfitNum);
				this._availableClothingOutfits[outfitNum] = null;
				this.RefreshCount();
				this.ToggleClothingInventoryViews();
				return true;
			}
			return false;
		}

		
		private void RefreshCount()
		{
			this._availableClothingOutfitCount = this._availableClothingOutfits.Count((List<int> outfit) => outfit.SafeCount<int>() > 0);
		}

		
		private bool IsUnique(List<int> availableClothingOutfit)
		{
			foreach (int id in availableClothingOutfit)
			{
				ClothingItem clothingItem = ClothingItemDatabase.ClothingItemById(id);
				if (clothingItem != null)
				{
					if ((float)clothingItem._baseRollChance <= 0f)
					{
						return true;
					}
				}
			}
			return false;
		}

		
		public void DropAll(Vector3 dropPos, bool uniqueOnly = false, bool includeWearing = false)
		{
			for (int i = 0; i < this._availableClothingOutfits.Count; i++)
			{
				if (!uniqueOnly || this.IsUnique(this._availableClothingOutfits[i]))
				{
					Vector3 b = UnityEngine.Random.onUnitSphere * 0.5f;
					b.y = 0f;
					this.FakeDrop(this._availableClothingOutfits[i], dropPos + b);
				}
			}
			if (includeWearing && (!uniqueOnly || this.IsUnique(this._wornClothingItems)))
			{
				this.FakeDrop(this._wornClothingItems, dropPos);
			}
			this._availableClothingOutfits.Clear();
			this._availableClothingOutfitCount = 0;
		}

		
		public void DropAll(bool uniqueOnly = false, bool includeWearing = false)
		{
			Transform transform = LocalPlayer.GameObject.transform;
			Vector3 dropPos = transform.position + transform.forward * 2f + Vector3.up * 0.25f + UnityEngine.Random.insideUnitSphere * 0.5f;
			this.DropAll(dropPos, uniqueOnly, includeWearing);
		}

		
		private void FakeDrop(int outfitNum)
		{
			List<int> indexSafe = this._availableClothingOutfits.GetIndexSafe(outfitNum, null);
			if (indexSafe == null)
			{
				return;
			}
			this.FakeDrop(indexSafe);
		}

		
		private void FakeDrop(List<int> outfit)
		{
			Transform transform = LocalPlayer.GameObject.transform;
			Vector3 dropPos = transform.position + transform.forward * 2f + Vector3.up * 0.25f + UnityEngine.Random.insideUnitSphere * 0.5f;
			this.FakeDrop(outfit, dropPos);
		}

		
		private void FakeDrop(List<int> outfit, Vector3 dropPos)
		{
			if (outfit == null)
			{
				return;
			}
			List<int> list = (from eachValue in outfit
			where eachValue > 0
			select eachValue).ToList<int>();
			if (list.SafeCount<int>() == 0)
			{
				return;
			}
			if (this._fakeDropPrefabSource == null)
			{
				return;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this._fakeDropPrefabSource, dropPos, Quaternion.identity);
			ClothingPickup componentInChildren = gameObject.GetComponentInChildren<ClothingPickup>();
			componentInChildren._presetOutfitItemIds = list;
			gameObject.name = string.Format("{0}_fakeDrop_clothes_{1}", this._fakeDropPrefabSource.name, list.Join("_"));
			if (BoltNetwork.isRunning)
			{
				BoltEntity component = gameObject.GetComponent<BoltEntity>();
				BoltNetwork.Attach(component);
			}
			LocalPlayer.Sfx.PlayWhoosh();
		}

		
		public bool SetWornOutfit(int outfitNum)
		{
			LocalPlayer.Inventory.UnequipItemAtSlot(Item.EquipmentSlot.FullBody, false, true, false);
			this.CheckInit();
			LocalPlayer.Sfx.PlayInventorySound(Item.SFXCommands.PlayPutOnClothingSfx);
			List<int> wornClothingItems = this._wornClothingItems;
			this._wornClothingItems = this._availableClothingOutfits[outfitNum];
			this._wornClothingItemsCount = this._wornClothingItems.Count;
			this._availableClothingOutfits.RemoveAt(outfitNum);
			if (wornClothingItems != null && wornClothingItems.Count > 0)
			{
				this._availableClothingOutfits.Add(wornClothingItems);
			}
			this.RefreshCount();
			return true;
		}

		
		public void ToggleClothingInventoryViews()
		{
			for (int i = this._clothingInventoryViews.Count - 1; i >= 0; i--)
			{
				bool flag = i < this._availableClothingOutfits.Count && this._availableClothingOutfits[i].SafeCount<int>() > 0;
				if (flag)
				{
					this._clothingInventoryViews[i]._outfitId = i;
				}
				if (this._clothingInventoryViews[i].gameObject.activeSelf != flag)
				{
					this._clothingInventoryViews[i].gameObject.SetActive(flag);
				}
			}
		}

		
		public void RefreshVisibleClothing()
		{
			bool flag = !LocalPlayer.Inventory.IsSlotEmpty(Item.EquipmentSlot.FullBody);
			this._wornClothingItemsFinal.Clear();
			for (int i = 0; i < this._wornClothingItems.Count; i++)
			{
				if (flag)
				{
					ClothingItem clothingItem = ClothingItemDatabase.ClothingItemById(this._wornClothingItems[i]);
					if (clothingItem._displayType == ClothingItem.DisplayTypes.Hat)
					{
						this._wornClothingItemsFinal.Add(this._wornClothingItems[i]);
					}
				}
				else
				{
					this._wornClothingItemsFinal.Add(this._wornClothingItems[i]);
				}
			}
			if (flag)
			{
				this._wornClothingItemsFinal.Add(-1);
			}
			this._playerVariation.SetVariation(LocalPlayer.Stats.PlayerVariation, LocalPlayer.Stats.PlayerVariationHair, this._wornClothingItemsFinal);
			this._playerVariation.UpdateSkinVariation(LocalPlayer.Stats.IsBloody, LocalPlayer.Stats.IsMuddy, LocalPlayer.Stats.IsRed, LocalPlayer.Stats.IsCold);
			this.ToggleClothingInventoryViews();
			this.MpSync();
		}

		
		public void MpSync()
		{
			if (BoltNetwork.isRunning && LocalPlayer.Entity && LocalPlayer.Entity.isAttached)
			{
				for (int i = 0; i < 4; i++)
				{
					if (i < this._wornClothingItemsFinal.Count)
					{
						LocalPlayer.State.PlayerClothingIds[i] = this._wornClothingItemsFinal[i];
					}
					else
					{
						LocalPlayer.State.PlayerClothingIds[i] = 0;
					}
				}
			}
		}

		
		public List<ClothingInventoryView> _clothingInventoryViews;

		
		public CoopPlayerVariations _playerVariation;

		
		public GameObject _fakeDropPrefabSource;

		
		[SerializeThis]
		private List<List<int>> _availableClothingOutfits;

		
		[SerializeThis]
		private int _availableClothingOutfitCount;

		
		[SerializeThis]
		private List<int> _wornClothingItems;

		
		[SerializeThis]
		private int _wornClothingItemsCount;

		
		private List<int> _wornClothingItemsFinal = new List<int>();

		
		public const int DefaultPantsId = 4;

		
		public const int DefaultTopId = 24;
	}
}
