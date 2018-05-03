using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using FMOD.Studio;
using TheForest.Items.Core;
using TheForest.Items.Craft.Interfaces;
using TheForest.Items.Inventory;
using TheForest.Items.Special;
using TheForest.Items.World;
using TheForest.Player;
using TheForest.Tools;
using TheForest.Utils;
using UniLinq;
using UnityEngine;

namespace TheForest.Items.Craft
{
	
	[DoNotSerializePublic]
	[AddComponentMenu("Items/Craft/Crafting Cog")]
	public class CraftingCog : MonoBehaviour, IItemStorage
	{
		
		public void Awake()
		{
			if (!this._initialized)
			{
				this._initialized = true;
				this._validRecipeFill = 0f;
				this._validRecipe = null;
				this.ShowCogRenderer(false);
				this._normalMaterial = this._targetCogRenderer.sharedMaterial;
				this._upgradeCog = base.GetComponent<UpgradeCog>();
				this._ingredients = new HashSet<ReceipeIngredient>();
				this.BuildViewCache();
				if (this._inventory && Scene.HudGui)
				{
					this._clickToCombineButton = Scene.HudGui.ClickToCombineInfo;
				}
				if (this._craftSfx)
				{
					this._craftSfxEmitter = this._craftSfx.GetComponent<FMOD_StudioEventEmitter>();
				}
				if (this._craftSfx2)
				{
					this._craftSfx2Emitter = this._craftSfx2.GetComponent<FMOD_StudioEventEmitter>();
				}
				for (int i = 0; i < this._itemViews.Length; i++)
				{
					this._itemViews[i].Init();
				}
			}
		}

		
		private void BuildViewCache()
		{
			this._itemViewsCache = (from iv in this._itemViews
			where !ItemDatabase.ItemById(iv._itemId).MatchType(Item.Types.Extension)
			select iv).ToDictionary((InventoryItemView iv) => iv._itemId, (InventoryItemView iv) => iv);
			this._itemExtensionViewsCache = (from iv in this._itemViews
			where ItemDatabase.ItemById(iv._itemId).MatchType(Item.Types.Extension)
			select iv).ToDictionary((InventoryItemView iv) => iv._itemId, (InventoryItemView iv) => iv);
		}

		
		private void Start()
		{
			this._lambdaMultiView = new GameObject("LambdaMultiview").AddComponent<InventoryItemView>();
			this._lambdaMultiView.transform.parent = base.transform;
			this._lambdaMultiView.transform.localPosition = new Vector3(-2.5f, 2.5f, 2.5f);
			this._lambdaMultiView._isCraft = true;
			this._lambdaMultiView._allowMultiView = true;
			if (!LevelSerializer.IsDeserializing)
			{
				this.IngredientCleanUp();
			}
			if (FMOD_StudioSystem.instance && !CoopPeerStarter.DedicatedHost)
			{
				this.CogRotateEventInstance = FMOD_StudioSystem.instance.GetEvent("event:/ui/ingame/ui_cog_spin");
				if (this.CogRotateEventInstance != null)
				{
					UnityUtil.ERRCHECK(this.CogRotateEventInstance.getCue("KeyOff", out this.CogRotateEventKeyoff));
				}
			}
		}

		
		public void OnDeserialized()
		{
			this.Awake();
		}

		
		private void OnEnable()
		{
			foreach (KeyValuePair<int, InventoryItemView> keyValuePair in this._itemViewsCache)
			{
				SpecialItemControlerBase component = keyValuePair.Value.GetComponent<SpecialItemControlerBase>();
				if (!(component == null))
				{
					MetalTinTrayControler metalTinTrayControler = component as MetalTinTrayControler;
					if (!(metalTinTrayControler == null))
					{
						if (!(metalTinTrayControler._storage == null) && !metalTinTrayControler._storage.IsEmpty)
						{
							metalTinTrayControler.EmptyToInventory();
						}
					}
				}
			}
			InventoryItemView rightHand = this._inventory.RightHand;
			ItemStorageProxy itemStorageProxy = (!(rightHand == null)) ? rightHand.GetComponent<ItemStorageProxy>() : null;
			if (itemStorageProxy != null && itemStorageProxy._storage != null)
			{
				this.Storage = itemStorageProxy._storage;
				this.Storage.Open();
				int num = this._inventory.CurrentStorage.Add(rightHand._itemId, 1, rightHand.Properties);
				this._inventory.RemoveItem(rightHand._itemId, 1 - num, true, true);
			}
		}

		
		private void OnDisable()
		{
			if (this._clickToCombineButton.activeSelf)
			{
				this._clickToCombineButton.SetActive(false);
				this._targetCogRenderer.sharedMaterial = this._normalMaterial;
			}
			if (Scene.HudGui && LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Inventory)
			{
				Scene.HudGui.ShowValidCraftingRecipes(null);
				Scene.HudGui.HideUpgradesDistribution();
			}
		}

		
		private void OnMouseExitCollider()
		{
			if (this.CogRotateEventKeyoff != null && this.CogRotateEventKeyoff.isValid())
			{
				UnityUtil.ERRCHECK(this.CogRotateEventKeyoff.trigger());
			}
			this._hovered = false;
			this.OnDisable();
		}

		
		private void OnMouseEnterCollider()
		{
			bool flag = this.CraftOverride != null || this.CanStore;
			bool hasValideRecipe = this.HasValideRecipe;
			if (!this._hovered && !this._upgradeCog.enabled && ((hasValideRecipe && (this._validRecipe._type != Receipe.Types.Upgrade || this._upgradeCount > 0)) || flag))
			{
				this._hovered = true;
				this._targetCogRenderer.sharedMaterial = this._selectedMaterial;
				if (this._clickToCombineButton.activeSelf != (hasValideRecipe || flag))
				{
					this._clickToCombineButton.SetActive(hasValideRecipe || flag);
				}
			}
			UnityUtil.ERRCHECK(this.CogRotateEventInstance.set3DAttributes(UnityUtil.to3DAttributes(LocalPlayer.MainCamTr.gameObject, null)));
			UnityUtil.ERRCHECK(this.CogRotateEventInstance.start());
		}

		
		private void Update()
		{
			if (!this._upgradeCog.enabled && (this.CraftOverride == null || this.CraftOverride.CanCombine()))
			{
				if ((this._hovered && (TheForest.Utils.Input.GetButtonDown("Combine") || TheForest.Utils.Input.GetButtonDown("Build"))) || TheForest.Utils.Input.GetButtonDown("Jump"))
				{
					if (this.CraftOverride != null)
					{
						this.CraftOverride.Combine();
					}
					else if (this.CanStore)
					{
						this.DoStorage();
					}
					else if (this.CanCraft)
					{
						this.DoCraft();
					}
				}
				else if (TheForest.Utils.Input.GetButtonDown("Drop"))
				{
					if (this._ingredients != null && this._ingredients.Count > 0)
					{
						LocalPlayer.Sfx.PlayWhoosh();
					}
					this.Close();
				}
				bool flag = this._ingredients.Count > 0 && !this._upgradeCog.enabled;
				if (Scene.HudGui.DropToRemoveAllInfo.activeSelf != flag)
				{
					Scene.HudGui.DropToRemoveAllInfo.SetActive(flag);
				}
			}
			this.UpdateCogRotation();
		}

		
		private void UpdateCogRotation()
		{
			float b = 0f;
			if (this._hovered)
			{
				b = 2f;
			}
			if (this._targetCogRenderer.enabled)
			{
				this._targetCogRotate = Mathf.Lerp(this._targetCogRotate, b, Time.unscaledDeltaTime * 2.5f);
			}
			else
			{
				this._targetCogRotate = 0f;
			}
			this._targetCogRenderer.transform.localEulerAngles += new Vector3(0f, this._targetCogRotate, 0f);
		}

		
		public bool IsValidItem(Item item)
		{
			if (this.Storage != null)
			{
				return this.Storage.IsValidItem(item);
			}
			return this.MatchType(this._acceptedTypes, item._type);
		}

		
		private bool MatchType(Item.Types mask, Item.Types type)
		{
			return (mask & type) != (Item.Types)0;
		}

		
		
		public bool IsEmpty
		{
			get
			{
				return this._ingredients.Count == 0;
			}
		}

		
		public int Add(int itemId, int amount = 1, ItemProperties properties = null)
		{
			if (amount > 0)
			{
				if (this.Storage != null)
				{
					this.RemoveExcessStorage(itemId);
				}
				ReceipeIngredient receipeIngredient = this.TryGetIngredient(itemId);
				int num;
				if (this.Storage || !this.InItemViewsCache(itemId))
				{
					num = int.MaxValue;
				}
				else if (this._itemViewsCache[itemId]._allowMultiView)
				{
					num = this._itemViewsCache[itemId]._maxMultiViews;
				}
				else
				{
					num = 1;
				}
				if (receipeIngredient == null)
				{
					receipeIngredient = new ReceipeIngredient
					{
						_itemID = itemId
					};
					this._ingredients.Add(receipeIngredient);
				}
				int result = Mathf.Max(receipeIngredient._amount + amount - num, 0);
				receipeIngredient._amount = Mathf.Min(receipeIngredient._amount + amount, num);
				this.ToggleItemInventoryView(itemId, properties);
				this.CheckForValidRecipe();
				return result;
			}
			return 0;
		}

		
		private void RemoveExcessStorage(int incomingItemId)
		{
			List<ReceipeIngredient> list = new List<ReceipeIngredient>();
			foreach (ReceipeIngredient receipeIngredient in this._ingredients)
			{
				int itemID = receipeIngredient._itemID;
				Item item = ItemDatabase.ItemById(itemID);
				if (item._maxAmount > 0 && (item._maxAmount <= 100 || itemID != incomingItemId))
				{
					bool flag = this.InItemViewsCache(itemID);
					ItemStorageProxy itemStorageProxy = (!flag) ? null : this._itemViewsCache[itemID].GetComponent<ItemStorageProxy>();
					if (!(itemStorageProxy != null) || !(itemStorageProxy._storage == this.Storage))
					{
						list.Add(receipeIngredient);
					}
				}
			}
			for (int i = 0; i < list.Count - (this.Storage._slotCount - 1); i++)
			{
				ItemProperties properties = ItemProperties.Any;
				if (this.InItemViewsCache(list[i]._itemID))
				{
					properties = this._itemViewsCache[list[i]._itemID].Properties;
				}
				this._inventory.AddItem(list[i]._itemID, list[i]._amount, true, true, properties);
				this.Remove(list[i]._itemID, list[i]._amount, properties);
			}
		}

		
		public int Remove(int itemId, int amount = 1, ItemProperties properties = null)
		{
			if (this._upgradeCog.enabled)
			{
				this._upgradeCog.Shutdown();
			}
			ReceipeIngredient receipeIngredient = this.TryGetIngredient(itemId);
			if (receipeIngredient != null)
			{
				bool flag = this.InItemViewsCache(itemId);
				InventoryItemView inventoryItemView = (!flag) ? null : this._itemViewsCache[itemId];
				if (flag && (inventoryItemView.ItemCache._maxAmount == 0 || inventoryItemView.ItemCache._maxAmount > LocalPlayer.Inventory.InventoryItemViewsCache[itemId].Count || !inventoryItemView._allowMultiView))
				{
					if (properties == ItemProperties.Any || inventoryItemView.Properties.Match(properties))
					{
						int result = Mathf.Max(amount - receipeIngredient._amount, 0);
						if ((receipeIngredient._amount -= amount) <= 0)
						{
							this._ingredients.Remove(receipeIngredient);
						}
						this.CheckForValidRecipe();
						this.ToggleItemInventoryView(itemId, properties);
						return result;
					}
				}
				else
				{
					if (flag)
					{
						int num = inventoryItemView.AmountOfMultiviewWithProperties(itemId, properties);
						int num2 = Mathf.Max(amount - num, 0);
						if ((receipeIngredient._amount -= amount - num2) <= 0)
						{
							this._ingredients.Remove(receipeIngredient);
						}
						this.CheckForValidRecipe();
						inventoryItemView.RemovedMultiViews(itemId, amount, properties, false);
						this.SelectItemViewProxyTarget();
						return num2;
					}
					if (LocalPlayer.Inventory.InventoryItemViewsCache.ContainsKey(itemId))
					{
						int num3 = this._lambdaMultiView.AmountOfMultiviewWithProperties(itemId, properties);
						int num4 = Mathf.Max(amount - num3, 0);
						if ((receipeIngredient._amount -= amount - num4) <= 0)
						{
							this._ingredients.Remove(receipeIngredient);
						}
						this.CheckForValidRecipe();
						this._lambdaMultiView.RemovedMultiViews(itemId, amount - num4, properties, false);
						this.SelectItemViewProxyTarget();
						return num4;
					}
					this._completedItemViewProxy.Unset();
				}
			}
			return amount;
		}

		
		public ItemProperties GetPropertiesOf(int itemId)
		{
			ReceipeIngredient receipeIngredient = this.TryGetIngredient(itemId);
			if (receipeIngredient != null)
			{
				InventoryItemView inventoryItemView = null;
				if (this._itemViewsCache.TryGetValue(itemId, out inventoryItemView) && (inventoryItemView.ItemCache._maxAmount == 0 || inventoryItemView.ItemCache._maxAmount > LocalPlayer.Inventory.InventoryItemViewsCache[itemId].Count || !inventoryItemView._allowMultiView))
				{
					return inventoryItemView.Properties;
				}
				if (inventoryItemView != null)
				{
					return inventoryItemView.GetFirstViewProperties();
				}
				if (LocalPlayer.Inventory.InventoryItemViewsCache.ContainsKey(itemId))
				{
					return this._lambdaMultiView.GetFirstViewPropertiesForItem(itemId);
				}
			}
			return null;
		}

		
		public void Open()
		{
		}

		
		public void Close()
		{
			if (this._upgradeCog.enabled)
			{
				this._upgradeCog.Shutdown();
			}
			if (this.Storage != null)
			{
				this.EmptyStorageToInventory();
			}
			foreach (ReceipeIngredient receipeIngredient in this._ingredients)
			{
				if (LocalPlayer.Inventory.InventoryItemViewsCache[receipeIngredient._itemID][0].ItemCache.MatchType(Item.Types.Special))
				{
					LocalPlayer.Inventory.SpecialItemsControlers[receipeIngredient._itemID].ToggleSpecialCraft(false);
				}
				if (this.InItemViewsCache(receipeIngredient._itemID))
				{
					InventoryItemView inventoryItemView = this._itemViewsCache[receipeIngredient._itemID];
					if (!inventoryItemView._allowMultiView)
					{
						this._inventory.AddItem(receipeIngredient._itemID, receipeIngredient._amount, true, true, inventoryItemView.Properties);
					}
					else
					{
						int i = receipeIngredient._amount;
						while (i > 0)
						{
							ItemProperties firstViewProperties = inventoryItemView.GetFirstViewProperties();
							int num = inventoryItemView.AmountOfMultiviewWithProperties(receipeIngredient._itemID, firstViewProperties);
							inventoryItemView.RemovedMultiViews(receipeIngredient._itemID, num, firstViewProperties, false);
							this._inventory.AddItem(receipeIngredient._itemID, num, true, true, firstViewProperties);
							if (num > 0)
							{
								i -= num;
							}
							else if (firstViewProperties == ItemProperties.Any)
							{
								break;
							}
						}
						if (i > 0)
						{
							this._inventory.AddItem(receipeIngredient._itemID, receipeIngredient._amount, true, true, ItemProperties.Any);
						}
					}
				}
				else if (LocalPlayer.Inventory.InventoryItemViewsCache.ContainsKey(receipeIngredient._itemID))
				{
					this._lambdaMultiView.SetAnyMultiViewAmount(LocalPlayer.Inventory.InventoryItemViewsCache[receipeIngredient._itemID][0], this._lambdaMultiView.transform, 0, ItemProperties.Any, true);
				}
				else
				{
					this._inventory.AddItem(receipeIngredient._itemID, receipeIngredient._amount, true, true, ItemProperties.Any);
				}
			}
			this.IngredientCleanUp();
			this.CheckForValidRecipe();
			Scene.HudGui.ShowValidCraftingRecipes(null);
			Scene.HudGui.HideUpgradesDistribution();
			Scene.HudGui.DropToRemoveAllInfo.SetActive(false);
		}

		
		private void EmptyStorageToInventory()
		{
			for (int i = 0; i < this.Storage.UsedSlots.Count; i++)
			{
				LocalPlayer.Inventory.AddItem(this.Storage.UsedSlots[i]._itemId, this.Storage.UsedSlots[i]._amount, true, true, this.Storage.UsedSlots[i]._properties);
			}
			this.Storage.Close();
			this.Storage.UsedSlots.Clear();
			this.Storage.UpdateContentVersion();
		}

		
		private bool CanCarryProduct(Receipe recipe)
		{
			int maxAmountOf = LocalPlayer.Inventory.GetMaxAmountOf(recipe._productItemID);
			return this._inventory.AmountOf(recipe._productItemID, false) < maxAmountOf;
		}

		
		private bool ShouldList(Receipe recipe)
		{
			return !recipe._hidden;
		}

		
		private bool CanCarryUpgradeProduct(Receipe recipe)
		{
			if (recipe._forceUnique && this.HasExistingUpgradeBonus(recipe))
			{
				return false;
			}
			if (recipe._productItemID == recipe._ingredients[0]._itemID)
			{
				return true;
			}
			int maxAmountOf = LocalPlayer.Inventory.GetMaxAmountOf(recipe._productItemID);
			int num = this._inventory.AmountOf(recipe._productItemID, false);
			return num < maxAmountOf || this._ingredients.Any((ReceipeIngredient i) => i._itemID == recipe._productItemID);
		}

		
		private bool HasExistingUpgradeBonus(Receipe recipe)
		{
			if (recipe._type != Receipe.Types.Upgrade)
			{
				return false;
			}
			if (this._inventory.AmountOf(recipe._productItemID, false) == 0 && this.Ingredients.All((ReceipeIngredient ingredient) => ingredient._itemID != recipe._productItemID))
			{
				return false;
			}
			if (recipe._weaponStatUpgrades == null || recipe._weaponStatUpgrades.Length == 0)
			{
				return false;
			}
			int productItemID = recipe._productItemID;
			WeaponStatUpgrade.Types activeBonus = this._itemViewsCache[productItemID].ActiveBonus;
			if (activeBonus == (WeaponStatUpgrade.Types)(-1))
			{
				return false;
			}
			foreach (WeaponStatUpgrade weaponStatUpgrade in recipe._weaponStatUpgrades)
			{
				if (activeBonus == weaponStatUpgrade._type)
				{
					return true;
				}
			}
			return false;
		}

		
		private bool CheckStorage()
		{
			if (this.Storage)
			{
				this._validRecipe = null;
				Scene.HudGui.CraftingReceipeBacking.gameObject.SetActive(false);
				Scene.HudGui.ShowValidCraftingRecipes(null);
				Scene.HudGui.HideUpgradesDistribution();
				this.ShowCogRenderer(this._ingredients.Count > 1);
				return true;
			}
			if (this._validRecipe == null)
			{
				this.ShowCogRenderer(false);
			}
			return false;
		}

		
		private void ShowCogRenderer(bool enabledValue)
		{
			this._targetCogRenderer.enabled = enabledValue;
			base.gameObject.GetComponent<Collider>().enabled = enabledValue;
		}

		
		private void DoStorage()
		{
			List<ReceipeIngredient> list = new List<ReceipeIngredient>();
			InventoryItemView inventoryItemView = null;
			bool flag = false;
			this._craftSfxEmitter.Play();
			foreach (ReceipeIngredient receipeIngredient in this._ingredients)
			{
				bool flag2 = this.InItemViewsCache(receipeIngredient._itemID);
				ItemStorageProxy itemStorageProxy = (!flag2) ? null : this._itemViewsCache[receipeIngredient._itemID].GetComponent<ItemStorageProxy>();
				if (!flag2)
				{
					while (this._lambdaMultiView.ContainsMultiView(receipeIngredient._itemID))
					{
						ItemProperties firstViewPropertiesForItem = this._lambdaMultiView.GetFirstViewPropertiesForItem(receipeIngredient._itemID);
						int num = this._lambdaMultiView.AmountOfMultiviewWithProperties(receipeIngredient._itemID, firstViewPropertiesForItem);
						int num2 = this.Storage.Add(receipeIngredient._itemID, num, firstViewPropertiesForItem);
						this._lambdaMultiView.RemovedMultiViews(receipeIngredient._itemID, num - num2, firstViewPropertiesForItem, false);
						if (num2 == 0)
						{
							list.Add(receipeIngredient);
						}
						else
						{
							if (num2 == num)
							{
								break;
							}
							receipeIngredient._amount = num2;
						}
					}
				}
				else if (itemStorageProxy == null)
				{
					int num3 = 0;
					if (!this._itemViewsCache[receipeIngredient._itemID]._allowMultiView)
					{
						num3 = this.Storage.Add(receipeIngredient._itemID, receipeIngredient._amount, this._itemViewsCache[receipeIngredient._itemID].Properties);
					}
					else
					{
						int i = receipeIngredient._amount;
						while (i > 0)
						{
							ItemProperties firstViewProperties = this._itemViewsCache[receipeIngredient._itemID].GetFirstViewProperties();
							int num4 = this._itemViewsCache[receipeIngredient._itemID].AmountOfMultiviewWithProperties(receipeIngredient._itemID, firstViewProperties);
							int num5 = this.Storage.Add(receipeIngredient._itemID, num4, firstViewProperties);
							if (num4 == num5)
							{
								num3 = i;
								break;
							}
							num4 -= num5;
							this._itemViewsCache[receipeIngredient._itemID].RemovedMultiViews(receipeIngredient._itemID, num4, firstViewProperties, false);
							if (num4 > 0)
							{
								i -= num4;
							}
							else if (firstViewProperties == ItemProperties.Any)
							{
								num3 = i;
								break;
							}
						}
					}
					if (num3 == 0)
					{
						list.Add(receipeIngredient);
					}
					else
					{
						receipeIngredient._amount = num3;
					}
				}
				else if (itemStorageProxy._storage == this.Storage)
				{
					inventoryItemView = this._itemViewsCache[receipeIngredient._itemID];
					flag = true;
				}
			}
			foreach (ReceipeIngredient receipeIngredient2 in list)
			{
				this._ingredients.Remove(receipeIngredient2);
				this.ToggleItemInventoryView(receipeIngredient2._itemID, ItemProperties.Any);
			}
			this.Storage.UpdateContentVersion();
			this.CheckStorage();
			this._completedItemViewProxy._targetView = inventoryItemView;
			if (flag)
			{
				int itemId = inventoryItemView._itemId;
				this._inventory.AddItem(itemId, 1, true, true, inventoryItemView.Properties);
				this._inventory.Equip(itemId, false);
				this._inventory.CurrentStorage.Remove(itemId, 1, null);
				this._inventory.Close();
			}
		}

		
		private bool InItemViewsCache(int itemId)
		{
			return this._itemViewsCache.ContainsKey(itemId);
		}

		
		private void DoCraft()
		{
			Receipe validRecipe = this._validRecipe;
			this._craftSfxEmitter.Play();
			UnityEngine.Object.Instantiate<GameObject>(this._craftParticle1, this._craftParticleSpawnPos.position, this._craftParticleSpawnPos.rotation);
			int num = validRecipe._type.Equals(Receipe.Types.Upgrade) ? this._upgradeCount : 1;
			ItemProperties itemProperties = ItemProperties.Any;
			for (int i = 0; i < validRecipe._ingredients.Length; i++)
			{
				ReceipeIngredient receipeIngredient = validRecipe._ingredients[i];
				ReceipeIngredient receipeIngredient2 = this.TryGetIngredient(receipeIngredient._itemID);
				int num2;
				if (i == 0)
				{
					if (validRecipe._type.Equals(Receipe.Types.Upgrade))
					{
						num2 = receipeIngredient._amount;
					}
					else if (validRecipe._type.Equals(Receipe.Types.Extension))
					{
						num2 = 0;
					}
					else
					{
						num2 = receipeIngredient._amount * num;
					}
				}
				else
				{
					num2 = receipeIngredient._amount * num;
				}
				receipeIngredient2._amount -= num2;
				if (i == 1)
				{
					itemProperties = ((!this._itemViewsCache[receipeIngredient2._itemID]._allowMultiView) ? this._itemViewsCache[receipeIngredient2._itemID].Properties : this._itemViewsCache[receipeIngredient2._itemID].GetFirstViewProperties());
				}
				if (receipeIngredient2._amount <= 0)
				{
					this._ingredients.Remove(receipeIngredient2);
				}
				else if (receipeIngredient._amount == 0)
				{
					this._inventory.AddItem(receipeIngredient._itemID, receipeIngredient2._amount, true, true, itemProperties);
					this.Remove(receipeIngredient._itemID, receipeIngredient2._amount, null);
				}
				this.ToggleItemInventoryView(receipeIngredient._itemID, ItemProperties.Any);
			}
			int upgradeCount = num;
			if (!validRecipe._type.Equals(Receipe.Types.Extension))
			{
				this.Add(validRecipe._productItemID, validRecipe._productItemAmount, ItemProperties.Any);
			}
			else
			{
				LocalPlayer.Inventory.AddItem(validRecipe._productItemID, validRecipe._productItemAmount, true, true, null);
				this.CheckForValidRecipe();
			}
			if (validRecipe._type.Equals(Receipe.Types.Upgrade))
			{
				this.ApplyUpgrade(validRecipe, itemProperties, upgradeCount);
			}
			if (validRecipe._ingredients[0]._itemID != validRecipe._productItemID)
			{
				EventRegistry.Player.Publish(TfEvent.CraftedItem, validRecipe._productItemID);
			}
			InventoryItemView itemView = this.GetItemView(validRecipe._productItemID);
			this._completedItemViewProxy._targetView = itemView;
			if (!this._upgradeCog.enabled && itemView)
			{
				base.StartCoroutine(this.animateCraftedItemRoutine(itemView.transform));
			}
		}

		
		private void CheckCraftOverride()
		{
			Scene.HudGui.HideUpgradesDistribution();
			bool flag = this.CraftOverride.CanCombine();
			if (flag)
			{
				this._craftSfx2Emitter.Play();
			}
			this.ShowCogRenderer(flag);
		}

		
		public void CheckForValidRecipe()
		{
			if (this._legacyCraftingSystem)
			{
				this.CheckForValidRecipeLegacy();
				return;
			}
			if (this.CheckStorage())
			{
				return;
			}
			this._validRecipeFill = 0f;
			this._validRecipe = null;
			this._upgradeCount = 0;
			this._validRecipeFill = 0f;
			this._validRecipeFull = false;
			if (this.CraftOverride != null)
			{
				this.HideRecipeDisplay();
				this.CheckCraftOverride();
				return;
			}
			if (this._ingredients.Count == 0)
			{
				this.HideRecipeDisplay();
				return;
			}
			int i;
			List<Receipe> list = (from ar in this._receipeBook.AvailableReceipesCache
			where this._ingredients.All((ReceipeIngredient i) => ar._ingredients.Any((ReceipeIngredient i2) => i._itemID == i2._itemID))
			select ar).ToList<Receipe>();
			list.AddRange(from ar in this._receipeBook.AvailableUpgradeCache
			where this._ingredients.All((ReceipeIngredient i) => ar._ingredients.Any((ReceipeIngredient i2) => i._itemID == i2._itemID))
			select ar);
			if (list.Count == 0)
			{
				this.HideRecipeDisplay();
				return;
			}
			Scene.HudGui.CraftingReceipeBacking.gameObject.SetActive(true);
			list.ForEach(delegate(Receipe ar)
			{
				ar.CanCarryProduct = this.CanCarryUpgradeProduct(ar);
			});
			bool flag = false;
			bool flag2 = false;
			list = (from eachRecipe in list
			orderby eachRecipe.CanCarryProduct descending, eachRecipe._ingredients.Length
			select eachRecipe).ThenBy((Receipe r) => r._type).ToList<Receipe>();
			for (i = 0; i < list.Count; i++)
			{
				Receipe receipe = list[i];
				if (receipe.CanCarryProduct)
				{
					flag2 |= (receipe._type == Receipe.Types.Craft | receipe._type == Receipe.Types.Extension);
					int num = receipe._ingredients.Sum((ReceipeIngredient ingredient) => Mathf.Max(ingredient._amount, 1));
					int matchedIngredientSum = CraftingCog.GetMatchedIngredientSum(receipe, this._ingredients);
					float num2 = (float)matchedIngredientSum / (float)num;
					if (receipe._hidden)
					{
						num2 = 0f;
					}
					if (!flag && num2 > this._validRecipeFill)
					{
						this._validRecipe = receipe;
						this._validRecipeFill = Mathf.Max(num2, this._validRecipeFill);
						flag = (num == matchedIngredientSum);
					}
				}
			}
			if (flag && flag2)
			{
				if (this._validRecipe._type == Receipe.Types.Upgrade)
				{
					this._upgradeCount = ItemDatabase.ItemById(this._validRecipe._productItemID)._maxUpgradesAmount;
				}
				Scene.HudGui.CraftingReceipeBacking.gameObject.SetActive(false);
				Scene.HudGui.HideUpgradesDistribution();
				this._craftSfx2Emitter.Play();
				this.ShowCogRenderer(true);
				return;
			}
			CraftingCog.ShowFilteredRecipes(list);
			this.ShowCogRenderer(false);
			float validRecipeFill = this._validRecipeFill;
			this._validRecipe = null;
			this.CheckForValidUpgrade(false);
			if (this._validRecipe == null)
			{
				this._validRecipeFill = validRecipeFill;
				Scene.HudGui.CraftingReceipeBacking.gameObject.SetActive(true);
				this.ShowCogRenderer(false);
			}
			Scene.HudGui.CraftingReceipeProgress.fillAmount = this._validRecipeFill;
		}

		
		private void HideRecipeDisplay()
		{
			this.ShowCogRenderer(false);
			Scene.HudGui.ShowValidCraftingRecipes(null);
			Scene.HudGui.CraftingReceipeBacking.gameObject.SetActive(false);
		}

		
		private static void ShowFilteredRecipes(List<Receipe> foundRecipes)
		{
			IEnumerable<Receipe> receipes = from eachRecipe in foundRecipes.Distinct(new CraftingCog.CompareRecipeProduct())
			where !eachRecipe._hidden
			select eachRecipe;
			Scene.HudGui.ShowValidCraftingRecipes(receipes);
		}

		
		[Obsolete("Use CheckForValidRecipe")]
		public void CheckForValidRecipeLegacy()
		{
			if (this.CheckStorage())
			{
				return;
			}
			Receipe receipe = null;
			IOrderedEnumerable<Receipe> orderedEnumerable = null;
			IOrderedEnumerable<Receipe> orderedEnumerable2 = null;
			int num = 0;
			int num2 = 0;
			int i;
			if (this._ingredients.Count > 0 && this.CraftOverride == null)
			{
				orderedEnumerable = from ar in this._receipeBook.AvailableReceipesCache
				where !ar._hidden
				where this._ingredients.All((ReceipeIngredient i) => ar._ingredients.Any((ReceipeIngredient i2) => i._itemID == i2._itemID))
				orderby ar._ingredients.Sum((ReceipeIngredient ari) => ari._amount), ar._ingredients.Length
				select ar;
				num = orderedEnumerable.Count<Receipe>();
				orderedEnumerable.ForEach(delegate(Receipe ar)
				{
					ar.CanCarryProduct = this.CanCarryUpgradeProduct(ar);
				});
				orderedEnumerable2 = from ar in this._receipeBook.AvailableUpgradeCache
				where !ar._hidden
				where this._ingredients.All((ReceipeIngredient i) => ar._ingredients.Any((ReceipeIngredient i2) => i._itemID == i2._itemID))
				orderby ar._ingredients.Length
				select ar;
				num2 = orderedEnumerable2.Count<Receipe>();
				orderedEnumerable2.ForEach(delegate(Receipe ar)
				{
					ar.CanCarryProduct = this.CanCarryUpgradeProduct(ar);
				});
				receipe = orderedEnumerable.FirstOrDefault((Receipe ar) => ar.CanCarryProduct);
				Receipe receipe2 = orderedEnumerable2.FirstOrDefault((Receipe ar) => ar.CanCarryProduct);
				if (receipe2 != null && receipe != null && receipe._ingredients.Length > receipe2._ingredients.Length)
				{
					receipe = null;
				}
			}
			bool flag = receipe != null;
			bool skipUpgrade2DFillingCog = flag;
			this._validRecipeFill = 0f;
			if (flag)
			{
				this._validRecipe = null;
				flag = false;
				Receipe receipe3 = null;
				float num3 = 0f;
				List<Receipe> list = new List<Receipe>();
				foreach (Receipe receipe4 in orderedEnumerable)
				{
					int num4 = 0;
					int num5 = receipe4._ingredients.Sum((ReceipeIngredient i) => i._amount);
					using (HashSet<ReceipeIngredient>.Enumerator enumerator2 = this._ingredients.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							ReceipeIngredient cogIngredients = enumerator2.Current;
							ReceipeIngredient receipeIngredient = receipe4._ingredients.First((ReceipeIngredient i) => i._itemID == cogIngredients._itemID);
							num4 += ((cogIngredients._amount <= receipeIngredient._amount) ? cogIngredients._amount : receipeIngredient._amount);
							if (receipeIngredient._amount == 0 && cogIngredients._amount > 0)
							{
								num4++;
								num5++;
							}
						}
					}
					this._validRecipeFull = false;
					this._validRecipeFill = (float)num4 / (float)num5;
					if (num4 != num5)
					{
						if (this._validRecipeFill > num3)
						{
							num3 = this._validRecipeFill;
							receipe3 = receipe4;
							list.Insert(0, receipe4);
						}
						else
						{
							bool flag2 = false;
							for (i = list.Count - 1; i >= 0; i--)
							{
								if (list[i]._ingredients.Length < receipe4._ingredients.Length)
								{
									list.Insert(i + 1, receipe4);
									flag2 = true;
									break;
								}
							}
							if (!flag2)
							{
								list.Add(receipe4);
							}
						}
					}
					else
					{
						list.Insert(0, receipe4);
						this._validRecipe = receipe4;
						flag = true;
						receipe3 = null;
						num3 = 1f;
					}
				}
				skipUpgrade2DFillingCog = flag;
				this._validRecipeFill = num3;
				if (receipe3 != null && !flag)
				{
					Scene.HudGui.CraftingReceipeBacking.gameObject.SetActive(true);
					Scene.HudGui.CraftingReceipeProgress.fillAmount = num3;
				}
				else
				{
					Scene.HudGui.CraftingReceipeBacking.gameObject.SetActive(false);
				}
				Scene.HudGui.ShowValidCraftingRecipes(from r in list.Concat(orderedEnumerable2).Distinct(new CraftingCog.CompareRecipeProduct())
				orderby r.CanCarryProduct descending, r._type
				select r);
			}
			else
			{
				List<Receipe> list2 = new List<Receipe>();
				if (orderedEnumerable != null && num > 0)
				{
					list2.AddRange(orderedEnumerable);
				}
				if (orderedEnumerable2 != null && num2 > 0)
				{
					list2.AddRange(orderedEnumerable2);
				}
				Scene.HudGui.ShowValidCraftingRecipes(from r in list2.Distinct(new CraftingCog.CompareRecipeProduct())
				orderby r.CanCarryProduct descending, r._ingredients.Length
				select r);
				Scene.HudGui.CraftingReceipeBacking.gameObject.SetActive(false);
			}
			if (this.CraftOverride == null)
			{
				if (flag)
				{
					Scene.HudGui.HideUpgradesDistribution();
					this._craftSfx2Emitter.Play();
					this._targetCogRenderer.enabled = true;
					base.gameObject.GetComponent<Collider>().enabled = true;
				}
				if (!flag || !this.CanCraft)
				{
					this.CheckForValidUpgrade(skipUpgrade2DFillingCog);
				}
			}
			else
			{
				Scene.HudGui.HideUpgradesDistribution();
				if (this.CraftOverride.CanCombine())
				{
					this._craftSfx2Emitter.Play();
					this._targetCogRenderer.enabled = true;
					base.gameObject.GetComponent<Collider>().enabled = true;
				}
				else
				{
					this._targetCogRenderer.enabled = false;
					base.gameObject.GetComponent<Collider>().enabled = false;
				}
			}
		}

		
		private static int GetMatchedIngredientSum(Receipe validRecipe, HashSet<ReceipeIngredient> suppliedIngredients)
		{
			int num = 0;
			if (validRecipe == null || suppliedIngredients == null)
			{
				return num;
			}
			foreach (ReceipeIngredient receipeIngredient in validRecipe._ingredients)
			{
				foreach (ReceipeIngredient receipeIngredient2 in suppliedIngredients)
				{
					if (receipeIngredient2._itemID == receipeIngredient._itemID)
					{
						int a = Mathf.Max(receipeIngredient._amount, 1);
						num += Mathf.Min(a, receipeIngredient2._amount);
					}
				}
			}
			return num;
		}

		
		private void CheckForValidUpgrade(bool skipUpgrade2DFillingCog = false)
		{
			Receipe receipe = null;
			if (this._ingredients.Count > 0)
			{
				IOrderedEnumerable<Receipe> source = from ar in this._receipeBook.AvailableUpgradeCache
				where this._ingredients.All(new Func<ReceipeIngredient, bool>(ar.HasIngredient))
				where this.CanCarryUpgradeProduct(ar)
				orderby ar._ingredients.Length
				select ar;
				receipe = source.FirstOrDefault<Receipe>();
			}
			if (receipe != null)
			{
				if (!this.CanCarryProduct(receipe) && receipe._ingredients[0]._itemID != receipe._productItemID)
				{
					this._validRecipeFull = true;
					receipe = null;
				}
				else
				{
					IEnumerable<ReceipeIngredient> source2 = from vri in receipe._ingredients
					join i in this._ingredients on vri._itemID equals i._itemID
					select i;
					ReceipeIngredient[] array = source2.ToArray<ReceipeIngredient>();
					if (array.Length > 0)
					{
						this._upgradeCount = ItemDatabase.ItemById(receipe._productItemID)._maxUpgradesAmount;
						int itemID = receipe._ingredients[1]._itemID;
						if (this._upgradeCog.SupportedItemsCache.ContainsKey(itemID) && this._upgradeCog.SupportedItemsCache[itemID]._pattern != UpgradeCog.Patterns.NoView)
						{
							this._upgradeCount -= LocalPlayer.Inventory.GetAmountOfUpgrades(receipe._productItemID);
						}
						bool flag = false;
						bool flag2 = this._upgradeCount == 0;
						int num = 0;
						int num2 = receipe._ingredients.Sum((ReceipeIngredient i) => (i._amount != 0) ? i._amount : 1);
						for (int j = 0; j < receipe._ingredients.Length; j++)
						{
							ReceipeIngredient receipeIngredient = receipe._ingredients[j];
							int num3 = 0;
							while (num3 < array.Length && array[num3]._itemID != receipeIngredient._itemID)
							{
								num3++;
							}
							if (num3 >= array.Length)
							{
								flag = true;
							}
							else if (receipeIngredient._amount > 0)
							{
								int num4 = array[num3]._amount / receipeIngredient._amount;
								if (j > 0 && num4 < this._upgradeCount)
								{
									this._upgradeCount = num4;
								}
								num += Mathf.Min(array[num3]._amount, receipeIngredient._amount);
							}
							else
							{
								this._upgradeCount = 1;
								num++;
								flag2 = true;
							}
						}
						if (!skipUpgrade2DFillingCog)
						{
							float num5 = (!receipe.CanCarryProduct) ? 0f : ((float)num / (float)num2);
							if (num5 > this._validRecipeFill)
							{
								this._validRecipeFill = num5;
							}
						}
						if ((this._upgradeCount <= 0 && !flag2) || flag)
						{
							receipe = null;
						}
						else
						{
							this._validRecipeFull = false;
						}
					}
					else
					{
						receipe = null;
					}
				}
			}
			bool flag3 = receipe != null;
			if (flag3)
			{
				this._validRecipe = receipe;
				Scene.HudGui.CraftingReceipeBacking.gameObject.SetActive(false);
				if (receipe._weaponStatUpgrades.Length == 0)
				{
					Scene.HudGui.ShowValidCraftingRecipes(null);
					Scene.HudGui.ShowUpgradesDistribution(this._validRecipe._productItemID, this._validRecipe._ingredients[1]._itemID, this._upgradeCount);
				}
			}
			else
			{
				if (!skipUpgrade2DFillingCog)
				{
					Scene.HudGui.CraftingReceipeBacking.gameObject.SetActive(this._validRecipeFill > 0f);
					Scene.HudGui.CraftingReceipeProgress.fillAmount = this._validRecipeFill;
				}
				Scene.HudGui.HideUpgradesDistribution();
				this._upgradeCount = 0;
			}
			if (this._targetCogRenderer.enabled != this._upgradeCount > 0)
			{
				if (!this._targetCogRenderer.enabled)
				{
					this._craftSfx2Emitter.Play();
				}
				this.ShowCogRenderer(this._upgradeCount > 0);
			}
		}

		
		private void ApplyUpgrade(Receipe craftedRecipe, ItemProperties lastIngredientProperties, int upgradeCount)
		{
			bool flag = this._itemViewsCache[craftedRecipe._productItemID]._held && !this._itemViewsCache[craftedRecipe._productItemID]._held.activeInHierarchy;
			if (flag)
			{
				if (this._itemViewsCache[craftedRecipe._productItemID]._heldWeaponInfo)
				{
					this._itemViewsCache[craftedRecipe._productItemID]._heldWeaponInfo.enabled = false;
				}
				this._itemViewsCache[craftedRecipe._productItemID]._held.SetActive(true);
			}
			if (!this.ApplyWeaponStatsUpgrades(craftedRecipe._productItemID, craftedRecipe._ingredients[1]._itemID, craftedRecipe._weaponStatUpgrades, craftedRecipe._batchUpgrade, upgradeCount, lastIngredientProperties))
			{
				this._upgradeCog.ApplyUpgradeRecipe(this._itemViewsCache[craftedRecipe._productItemID], craftedRecipe, upgradeCount);
			}
			if (flag)
			{
				this._itemViewsCache[craftedRecipe._productItemID]._held.SetActive(false);
				if (this._itemViewsCache[craftedRecipe._productItemID]._heldWeaponInfo)
				{
					this._itemViewsCache[craftedRecipe._productItemID]._heldWeaponInfo.enabled = true;
				}
			}
		}

		
		public WeaponStatUpgrade[] GetWeaponStatUpgradeForIngredient(int ingredientId)
		{
			UpgradeCogItems upgradeCogItems;
			if (this._upgradeCog.SupportedItemsCache.TryGetValue(ingredientId, out upgradeCogItems))
			{
				return upgradeCogItems._weaponStatUpgrades;
			}
			return null;
		}

		
		public bool ApplyWeaponStatsUpgrades(int productItemId, int ingredientItemId, WeaponStatUpgrade[] bonuses, bool batched, int upgradeCount, ItemProperties lastIngredientProperties = null)
		{
			InventoryItemView inventoryItemView = this._itemViewsCache[productItemId];
			bool flag = false;
			int i = 0;
			while (i < bonuses.Length)
			{
				switch (bonuses[i]._type)
				{
				case WeaponStatUpgrade.Types.BurningWeapon:
				{
					BurnableCloth componentInChildren = inventoryItemView._held.GetComponentInChildren<BurnableCloth>();
					if (componentInChildren)
					{
						inventoryItemView.ActiveBonus = WeaponStatUpgrade.Types.BurningWeapon;
						componentInChildren.EnableBurnableCloth();
					}
					flag = true;
					break;
				}
				case WeaponStatUpgrade.Types.StickyProjectile:
					if (batched && inventoryItemView._allowMultiView)
					{
						inventoryItemView.SetMultiviewsBonus(WeaponStatUpgrade.Types.StickyProjectile);
					}
					else
					{
						inventoryItemView.ActiveBonus = WeaponStatUpgrade.Types.StickyProjectile;
					}
					flag = true;
					break;
				case WeaponStatUpgrade.Types.WalkmanTrack:
					WalkmanControler.LoadCassette(ingredientItemId);
					flag = true;
					break;
				case WeaponStatUpgrade.Types.BurningAmmo:
					if (batched && inventoryItemView._allowMultiView)
					{
						inventoryItemView.SetMultiviewsBonus(WeaponStatUpgrade.Types.BurningAmmo);
					}
					else
					{
						inventoryItemView.ActiveBonus = WeaponStatUpgrade.Types.BurningAmmo;
					}
					flag = true;
					break;
				case WeaponStatUpgrade.Types.Paint_Green:
				{
					EquipmentPainting componentInChildren2 = inventoryItemView._held.GetComponentInChildren<EquipmentPainting>();
					if (componentInChildren2)
					{
						componentInChildren2.PaintInGreen();
					}
					flag = true;
					break;
				}
				case WeaponStatUpgrade.Types.Paint_Orange:
				{
					EquipmentPainting componentInChildren3 = inventoryItemView._held.GetComponentInChildren<EquipmentPainting>();
					if (componentInChildren3)
					{
						componentInChildren3.PaintInOrange();
					}
					flag = true;
					break;
				}
				case WeaponStatUpgrade.Types.DirtyWater:
				case WeaponStatUpgrade.Types.CleanWater:
				case WeaponStatUpgrade.Types.Cooked:
				case WeaponStatUpgrade.Types.blockStaminaDrain:
				case WeaponStatUpgrade.Types.RawFood:
				case WeaponStatUpgrade.Types.DriedFood:
					goto IL_3C7;
				case WeaponStatUpgrade.Types.ItemPart:
				{
					IItemPartInventoryView itemPartInventoryView = (IItemPartInventoryView)inventoryItemView;
					itemPartInventoryView.AddPiece(Mathf.RoundToInt(bonuses[i]._amount), true);
					flag = true;
					break;
				}
				case WeaponStatUpgrade.Types.BatteryCharge:
					LocalPlayer.Stats.BatteryCharge = Mathf.Clamp(LocalPlayer.Stats.BatteryCharge + bonuses[i]._amount, 0f, 100f);
					flag = true;
					break;
				case WeaponStatUpgrade.Types.FlareGunAmmo:
					LocalPlayer.Inventory.AddItem(ItemDatabase.ItemByName("FlareGunAmmo")._id, Mathf.RoundToInt(bonuses[i]._amount * (float)upgradeCount), false, false, null);
					flag = true;
					break;
				case WeaponStatUpgrade.Types.SetWeaponAmmoBonus:
					inventoryItemView.Properties.Copy((lastIngredientProperties != ItemProperties.Any) ? lastIngredientProperties : this._itemViewsCache[ingredientItemId].Properties);
					LocalPlayer.Inventory.SortInventoryViewsByBonus(LocalPlayer.Inventory.InventoryItemViewsCache[ingredientItemId][0], inventoryItemView.ActiveBonus, false);
					flag = true;
					break;
				case WeaponStatUpgrade.Types.PoisonnedAmmo:
					if (batched && inventoryItemView._allowMultiView)
					{
						inventoryItemView.SetMultiviewsBonus(WeaponStatUpgrade.Types.PoisonnedAmmo);
					}
					else
					{
						inventoryItemView.ActiveBonus = WeaponStatUpgrade.Types.PoisonnedAmmo;
					}
					flag = true;
					break;
				case WeaponStatUpgrade.Types.BurningWeaponExtra:
				{
					BurnableCloth componentInChildren4 = inventoryItemView._held.GetComponentInChildren<BurnableCloth>();
					if (componentInChildren4 && inventoryItemView.ActiveBonus == WeaponStatUpgrade.Types.BurningWeapon)
					{
						inventoryItemView.ActiveBonus = WeaponStatUpgrade.Types.BurningWeaponExtra;
						componentInChildren4.EnableBurnableClothExtra();
					}
					flag = true;
					break;
				}
				case WeaponStatUpgrade.Types.Incendiary:
					inventoryItemView.ActiveBonus = WeaponStatUpgrade.Types.Incendiary;
					flag = true;
					break;
				case WeaponStatUpgrade.Types.BoneAmmo:
					if (batched && inventoryItemView._allowMultiView)
					{
						inventoryItemView.SetMultiviewsBonus(WeaponStatUpgrade.Types.BoneAmmo);
					}
					else
					{
						inventoryItemView.ActiveBonus = WeaponStatUpgrade.Types.BoneAmmo;
					}
					flag = true;
					break;
				case WeaponStatUpgrade.Types.CamCorderTape:
					CamCorderControler.LoadTape(ingredientItemId);
					flag = true;
					break;
				case WeaponStatUpgrade.Types.PoisonnedWeapon:
					if (inventoryItemView._heldWeaponInfo.bonus)
					{
						inventoryItemView.ActiveBonus = WeaponStatUpgrade.Types.PoisonnedWeapon;
						inventoryItemView._heldWeaponInfo.bonus._bonusType = WeaponBonus.BonusTypes.Poison;
						inventoryItemView._heldWeaponInfo.bonus.enabled = true;
						RandomWeaponUpgradeVisual component = inventoryItemView._heldWeaponInfo.bonus.GetComponent<RandomWeaponUpgradeVisual>();
						if (component)
						{
							component.OnEnable();
						}
					}
					flag = true;
					break;
				case WeaponStatUpgrade.Types.ModernAmmo:
					if (batched && inventoryItemView._allowMultiView)
					{
						inventoryItemView.SetMultiviewsBonus(WeaponStatUpgrade.Types.ModernAmmo);
					}
					else
					{
						inventoryItemView.ActiveBonus = WeaponStatUpgrade.Types.ModernAmmo;
					}
					flag = true;
					break;
				case WeaponStatUpgrade.Types.TapedLight:
					LocalPlayer.Inventory.AddItem(Mathf.RoundToInt(bonuses[i]._amount), 1, false, false, null);
					flag = true;
					break;
				default:
					goto IL_3C7;
				}
				IL_410:
				i++;
				continue;
				IL_3C7:
				flag = this.ApplyWeaponBonus(bonuses[i], productItemId, ingredientItemId, upgradeCount);
				if (flag)
				{
					GameStats.UpgradesAdded.Invoke(upgradeCount);
				}
				else
				{
					Debug.LogError("Attempting to upgrade " + inventoryItemView.ItemCache._name + " which doesn't reference its weaponInfo component.");
				}
				goto IL_410;
			}
			return flag;
		}

		
		public bool ApplyWeaponBonus(WeaponStatUpgrade bonus, int weaponItemId, int upgradeItemId, int upgradeCount)
		{
			weaponInfo heldWeaponInfo = this._itemViewsCache[weaponItemId]._heldWeaponInfo;
			if (heldWeaponInfo)
			{
				FieldInfo field = typeof(weaponInfo).GetField(bonus._type.ToString(), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				float upgradeBonusAmount = this.GetUpgradeBonusAmount(weaponItemId, upgradeItemId, bonus, upgradeCount);
				if (field.FieldType == typeof(float))
				{
					field.SetValue(heldWeaponInfo, (float)field.GetValue(heldWeaponInfo) + upgradeBonusAmount);
				}
				else if (field.FieldType == typeof(int))
				{
					field.SetValue(heldWeaponInfo, (int)field.GetValue(heldWeaponInfo) + Mathf.RoundToInt(upgradeBonusAmount));
				}
				return true;
			}
			return false;
		}

		
		public float GetUpgradeBonusAmount(int productItemId, int ingredientItemId, WeaponStatUpgrade bonus, int upgradeCount)
		{
			AnimationCurve bonusDecayCurve = this._upgradeCog.SupportedItemsCache[ingredientItemId]._bonusDecayCurve;
			float num = 0f;
			if (bonusDecayCurve == null)
			{
				num = bonus._amount * (float)upgradeCount;
			}
			else
			{
				float num2 = (float)LocalPlayer.Inventory.GetAmountOfUpgrades(productItemId, ingredientItemId);
				float num3 = (float)ItemDatabase.ItemById(productItemId)._maxUpgradesAmount;
				for (int i = 0; i < upgradeCount; i++)
				{
					num += bonus._amount * bonusDecayCurve.Evaluate((num2 + (float)i) / num3);
				}
			}
			return num;
		}

		
		private ReceipeIngredient TryGetIngredient(int itemId)
		{
			foreach (ReceipeIngredient receipeIngredient in this._ingredients)
			{
				if (receipeIngredient._itemID == itemId)
				{
					return receipeIngredient;
				}
			}
			return null;
		}

		
		private void ToggleItemInventoryView(int itemId, ItemProperties properties = null)
		{
			ReceipeIngredient receipeIngredient = this.TryGetIngredient(itemId);
			int num = (receipeIngredient == null) ? 0 : receipeIngredient._amount;
			bool flag = num > 0;
			if (this.InItemViewsCache(itemId))
			{
				if (this._itemViewsCache[itemId]._allowMultiView)
				{
					if (properties == ItemProperties.Any)
					{
						this._itemViewsCache[itemId].SetMultiViewAmount(num, properties);
					}
					else
					{
						this._itemViewsCache[itemId].SetMultiViewAmount(num - this._itemViewsCache[itemId].AmountOfMultiviewWithoutProperties(itemId, properties), properties);
					}
				}
				else
				{
					if (properties != ItemProperties.Any)
					{
						this._itemViewsCache[itemId].Properties.Copy(properties);
					}
					if (this._itemViewsCache[itemId].gameObject.activeSelf != flag)
					{
						this._itemViewsCache[itemId].gameObject.SetActive(flag);
					}
				}
			}
			else if (LocalPlayer.Inventory.InventoryItemViewsCache.ContainsKey(itemId))
			{
				this._lambdaMultiView.SetAnyMultiViewAmount(LocalPlayer.Inventory.InventoryItemViewsCache[itemId][0], this._lambdaMultiView.transform, num, properties, false);
			}
			this.SelectItemViewProxyTarget();
		}

		
		private void SelectItemViewProxyTarget()
		{
			if (this._ingredients.Count == 1)
			{
				InventoryItemView itemView = this.GetItemView(this._ingredients.First<ReceipeIngredient>()._itemID);
				this._completedItemViewProxy._targetView = itemView;
			}
			else
			{
				this._completedItemViewProxy.Unset();
			}
		}

		
		private InventoryItemView GetItemView(int itemId)
		{
			if (this.InItemViewsCache(itemId))
			{
				if (this._itemViewsCache[itemId]._allowMultiView)
				{
					return this._itemViewsCache[itemId].GetFirstView();
				}
				return this._itemViewsCache[itemId];
			}
			else
			{
				if (LocalPlayer.Inventory.InventoryItemViewsCache.ContainsKey(itemId))
				{
					return this._lambdaMultiView.GetFirstViewForItem(itemId);
				}
				return null;
			}
		}

		
		private void IngredientCleanUp()
		{
			this._ingredients.Clear();
			IEnumerable<Item> enumerable = ItemDatabase.ItemsByType(Item.Types.CraftingMaterial | Item.Types.Craftable | Item.Types.Edible);
			foreach (Item item in enumerable)
			{
				this.ToggleItemInventoryView(item._id, ItemProperties.Any);
			}
			this._lambdaMultiView.ClearMultiViews();
		}

		
		private void EnableInventorySnapshot()
		{
			if (FMOD_StudioSystem.instance && this.InventorySnapShotInstance == null)
			{
				this.InventorySnapShotInstance = FMOD_StudioSystem.instance.GetEvent("snapshot:/inventory");
				if (this.InventorySnapShotInstance != null)
				{
					UnityUtil.ERRCHECK(this.InventorySnapShotInstance.start());
				}
			}
		}

		
		private void DisableInventorySnapshot()
		{
			if (this.InventorySnapShotInstance != null && this.InventorySnapShotInstance.isValid())
			{
				UnityUtil.ERRCHECK(this.InventorySnapShotInstance.stop(STOP_MODE.IMMEDIATE));
				UnityUtil.ERRCHECK(this.InventorySnapShotInstance.release());
				this.InventorySnapShotInstance = null;
			}
		}

		
		private IEnumerator animateCraftedItemRoutine(Transform tr)
		{
			Transform currentParent = tr.parent;
			Vector3 currentPos = tr.localPosition;
			Quaternion currentRot = tr.localRotation;
			tr.parent = this._craftAnimateParent;
			this._craftAnim.SetTrigger("craftTrigger");
			yield return YieldPresets.WaitOnePointThreeSeconds;
			tr.parent = currentParent;
			tr.localPosition = currentPos;
			tr.localRotation = currentRot;
			yield break;
		}

		
		
		public HashSet<ReceipeIngredient> Ingredients
		{
			get
			{
				return this._ingredients;
			}
		}

		
		
		public float RecipeFill
		{
			get
			{
				return this._validRecipeFill;
			}
		}

		
		
		public bool RecipeProductFull
		{
			get
			{
				return this._validRecipeFull;
			}
		}

		
		
		public bool CanStore
		{
			get
			{
				return this.CheckStorage() && this._ingredients.Count > 0;
			}
		}

		
		
		public bool CanCraft
		{
			get
			{
				return this._validRecipe != null && !this._validRecipeFull && Mathf.Approximately(this._validRecipeFill, 1f);
			}
		}

		
		
		public Dictionary<int, InventoryItemView> ItemViewsCache
		{
			get
			{
				return this._itemViewsCache;
			}
		}

		
		
		public Dictionary<int, InventoryItemView> ItemExtensionViewsCache
		{
			get
			{
				return this._itemExtensionViewsCache;
			}
		}

		
		
		
		public ItemStorage Storage { get; set; }

		
		
		
		public ICraftOverride CraftOverride { get; set; }

		
		
		public bool HasValideRecipe
		{
			get
			{
				return this._validRecipe != null;
			}
		}

		
		public bool _legacyCraftingSystem;

		
		[EnumFlags]
		public Item.Types _acceptedTypes;

		
		public ReceipeBook _receipeBook;

		
		public PlayerInventory _inventory;

		
		public Material _selectedMaterial;

		
		public MeshRenderer _targetCogRenderer;

		
		public GameObject _craftSfx;

		
		public GameObject _craftSfx2;

		
		public GameObject _craftParticle1;

		
		public Transform _craftParticleSpawnPos;

		
		public Animator _craftAnim;

		
		public Transform _craftAnimateParent;

		
		public InventoryItemView[] _itemViews;

		
		public UpgradeCog _upgradeCog;

		
		public InventoryItemViewProxy _completedItemViewProxy;

		
		private GameObject _clickToCombineButton;

		
		private Material _normalMaterial;

		
		private HashSet<ReceipeIngredient> _ingredients;

		
		private Dictionary<int, InventoryItemView> _itemViewsCache;

		
		private Dictionary<int, InventoryItemView> _itemExtensionViewsCache;

		
		private bool _validRecipeFull;

		
		private float _validRecipeFill;

		
		private Receipe _validRecipe;

		
		private int _upgradeCount;

		
		private FMOD_StudioEventEmitter _craftSfxEmitter;

		
		private FMOD_StudioEventEmitter _craftSfx2Emitter;

		
		private bool _initialized;

		
		private bool _hovered;

		
		private InventoryItemView _lambdaMultiView;

		
		private float _targetCogRotate;

		
		private EventInstance InventorySnapShotInstance;

		
		private EventInstance CogRotateEventInstance;

		
		private CueInstance CogRotateEventKeyoff;

		
		private class CompareRecipeProduct : IEqualityComparer<Receipe>
		{
			
			public bool Equals(Receipe x, Receipe y)
			{
				if (x._id != y._id)
				{
					if (x._type == Receipe.Types.Extension && y._type == Receipe.Types.Extension && x._ingredients.Length == y._ingredients.Length)
					{
						for (int i = 1; i < x._ingredients.Length; i++)
						{
							if (x._ingredients[i]._itemID != y._ingredients[i]._itemID)
							{
								return false;
							}
						}
					}
					else
					{
						if (x._productItemAmount._min != y._productItemAmount._min || x._productItemAmount._max != y._productItemAmount._max || x._weaponStatUpgrades.Length != y._weaponStatUpgrades.Length)
						{
							return false;
						}
						if (x._weaponStatUpgrades.Length > 0)
						{
							for (int j = 0; j < x._weaponStatUpgrades.Length; j++)
							{
								if (x._weaponStatUpgrades[j]._type != y._weaponStatUpgrades[j]._type)
								{
									return false;
								}
							}
						}
					}
				}
				return true;
			}

			
			public int GetHashCode(Receipe obj)
			{
				if (obj._type == Receipe.Types.Upgrade)
				{
					if (obj._weaponStatUpgrades.Length > 0)
					{
						return (int)(obj._weaponStatUpgrades[0]._type + (int)(obj._type * (Receipe.Types)10000));
					}
					if (obj._ingredients.Length == 3)
					{
						return (int)(obj._ingredients[1]._itemID * 1000 + obj._ingredients[2]._itemID * 500000 + obj._type * (Receipe.Types)10000000);
					}
					if (obj._ingredients.Length == 2)
					{
						return (int)(obj._ingredients[1]._itemID * 1000 + obj._type * (Receipe.Types)10000000);
					}
				}
				return (int)(obj._productItemID + obj._type * (Receipe.Types)100000);
			}
		}
	}
}
