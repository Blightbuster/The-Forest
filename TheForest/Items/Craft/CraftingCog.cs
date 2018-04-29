using System;
using System.Collections.Generic;
using System.Reflection;
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
	
	[AddComponentMenu("Items/Craft/Crafting Cog")]
	[DoNotSerializePublic]
	public class CraftingCog : MonoBehaviour, IItemStorage
	{
		
		public void Awake()
		{
			if (!this._initialized)
			{
				this._initialized = true;
				this._validRecipeFill = 0f;
				this._validRecipe = null;
				base.gameObject.GetComponent<Collider>().enabled = false;
				this._normalMaterial = base.gameObject.GetComponent<Renderer>().sharedMaterial;
				this._upgradeCog = base.GetComponent<UpgradeCog>();
				this._ingredients = new HashSet<ReceipeIngredient>();
				this._itemViewsCache = (from iv in this._itemViews
				where !ItemDatabase.ItemById(iv._itemId).MatchType(Item.Types.Extension)
				select iv).ToDictionary((InventoryItemView iv) => iv._itemId, (InventoryItemView iv) => iv);
				this._itemExtensionViewsCache = (from iv in this._itemViews
				where ItemDatabase.ItemById(iv._itemId).MatchType(Item.Types.Extension)
				select iv).ToDictionary((InventoryItemView iv) => iv._itemId, (InventoryItemView iv) => iv);
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
		}

		
		public void OnDeserialized()
		{
			this.Awake();
		}

		
		private void OnDisable()
		{
			if (this._clickToCombineButton.activeSelf)
			{
				this._clickToCombineButton.SetActive(false);
				base.gameObject.GetComponent<Renderer>().sharedMaterial = this._normalMaterial;
			}
			if (Scene.HudGui && LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Inventory)
			{
				Scene.HudGui.ShowValidCraftingRecipes(null);
				Scene.HudGui.HideUpgradesDistribution();
			}
		}

		
		private void OnMouseExitCollider()
		{
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
				base.gameObject.GetComponent<Renderer>().sharedMaterial = this._selectedMaterial;
				if (this._clickToCombineButton.activeSelf != (hasValideRecipe || flag))
				{
					this._clickToCombineButton.SetActive(hasValideRecipe || flag);
				}
			}
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
					LocalPlayer.Sfx.PlayWhoosh();
					this.Close();
				}
				bool flag = this._ingredients.Count > 0 && !this._upgradeCog.enabled;
				if (Scene.HudGui.DropToRemoveAllInfo.activeSelf != flag)
				{
					Scene.HudGui.DropToRemoveAllInfo.SetActive(flag);
				}
			}
		}

		
		
		public Item.Types AcceptedTypes
		{
			get
			{
				return (!this.Storage) ? this._acceptedTypes : this.Storage.AcceptedTypes;
			}
		}

		
		
		public Item.Types BlackListedTypes
		{
			get
			{
				return (!this.Storage) ? ((Item.Types)0) : this.Storage.BlackListedTypes;
			}
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
				ReceipeIngredient receipeIngredient = this._ingredients.FirstOrDefault((ReceipeIngredient i) => i._itemID == itemId);
				int num;
				if (this.Storage || !this._itemViewsCache.ContainsKey(itemId))
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
				this.CheckForValidRecipe();
				this.ToggleItemInventoryView(itemId, properties);
				return result;
			}
			return 0;
		}

		
		public int Remove(int itemId, int amount = 1, ItemProperties properties = null)
		{
			if (this._upgradeCog.enabled)
			{
				this._upgradeCog.Shutdown();
			}
			ReceipeIngredient receipeIngredient = this._ingredients.FirstOrDefault((ReceipeIngredient i) => i._itemID == itemId);
			if (receipeIngredient != null)
			{
				if (this._itemViewsCache.ContainsKey(itemId) && (this._itemViewsCache[itemId].ItemCache._maxAmount == 0 || this._itemViewsCache[itemId].ItemCache._maxAmount > LocalPlayer.Inventory.InventoryItemViewsCache[itemId].Count || !this._itemViewsCache[itemId]._allowMultiView))
				{
					if (properties == ItemProperties.Any || this._itemViewsCache[itemId].Properties.Match(properties))
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
					if (this._itemViewsCache.ContainsKey(itemId))
					{
						int num = this._itemViewsCache[itemId].AmountOfMultiviewWithProperties(itemId, properties);
						int num2 = Mathf.Max(amount - num, 0);
						if ((receipeIngredient._amount -= amount - num2) <= 0)
						{
							this._ingredients.Remove(receipeIngredient);
						}
						this.CheckForValidRecipe();
						this._itemViewsCache[itemId].RemovedMultiViews(itemId, amount, properties, false);
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
			ReceipeIngredient receipeIngredient = this._ingredients.FirstOrDefault((ReceipeIngredient i) => i._itemID == itemId);
			if (receipeIngredient != null)
			{
				if (this._itemViewsCache.ContainsKey(itemId) && (this._itemViewsCache[itemId].ItemCache._maxAmount == 0 || this._itemViewsCache[itemId].ItemCache._maxAmount > LocalPlayer.Inventory.InventoryItemViewsCache[itemId].Count || !this._itemViewsCache[itemId]._allowMultiView))
				{
					return this._itemViewsCache[itemId].Properties;
				}
				if (this._itemViewsCache.ContainsKey(itemId))
				{
					return this._itemViewsCache[itemId].GetFirstViewProperties();
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
			foreach (ReceipeIngredient receipeIngredient in this._ingredients)
			{
				if (LocalPlayer.Inventory.InventoryItemViewsCache[receipeIngredient._itemID][0].ItemCache.MatchType(Item.Types.Special))
				{
					LocalPlayer.Inventory.SpecialItemsControlers[receipeIngredient._itemID].ToggleSpecialCraft(false);
				}
				if (this._itemViewsCache.ContainsKey(receipeIngredient._itemID))
				{
					if (!this._itemViewsCache[receipeIngredient._itemID]._allowMultiView)
					{
						this._inventory.AddItem(receipeIngredient._itemID, receipeIngredient._amount, true, true, this._itemViewsCache[receipeIngredient._itemID].Properties);
					}
					else
					{
						int i = receipeIngredient._amount;
						while (i > 0)
						{
							ItemProperties firstViewProperties = this._itemViewsCache[receipeIngredient._itemID].GetFirstViewProperties();
							int num = this._itemViewsCache[receipeIngredient._itemID].AmountOfMultiviewWithProperties(receipeIngredient._itemID, firstViewProperties);
							this._itemViewsCache[receipeIngredient._itemID].RemovedMultiViews(receipeIngredient._itemID, num, firstViewProperties, false);
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

		
		private bool CanCarryProduct(Receipe recipe)
		{
			int maxAmountOf = LocalPlayer.Inventory.GetMaxAmountOf(recipe._productItemID);
			return this._inventory.AmountOf(recipe._productItemID, false) < maxAmountOf;
		}

		
		private bool CanCarryUpgradeProduct(Receipe recipe)
		{
			if (recipe._productItemID == recipe._ingredients[0]._itemID)
			{
				return true;
			}
			int maxAmountOf = LocalPlayer.Inventory.GetMaxAmountOf(recipe._productItemID);
			int num = this._inventory.AmountOf(recipe._productItemID, false);
			return num < maxAmountOf || this._ingredients.Any((ReceipeIngredient i) => i._itemID == recipe._productItemID);
		}

		
		private bool CheckStorage()
		{
			if (this.Storage)
			{
				this._validRecipe = null;
				Scene.HudGui.CraftingReceipeBacking.gameObject.SetActive(false);
				Scene.HudGui.ShowValidCraftingRecipes(null);
				Scene.HudGui.HideUpgradesDistribution();
				base.gameObject.GetComponent<Renderer>().enabled = (this._ingredients.Count > 1);
				base.gameObject.GetComponent<Collider>().enabled = (this._ingredients.Count > 1);
				return true;
			}
			if (this._validRecipe == null)
			{
				base.gameObject.GetComponent<Renderer>().enabled = false;
				base.gameObject.GetComponent<Collider>().enabled = false;
			}
			return false;
		}

		
		private void DoStorage()
		{
			List<ReceipeIngredient> list = new List<ReceipeIngredient>();
			InventoryItemView targetView = null;
			this._craftSfxEmitter.Play();
			foreach (ReceipeIngredient receipeIngredient in this._ingredients)
			{
				if (!this._itemViewsCache.ContainsKey(receipeIngredient._itemID))
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
				else if (!this._itemViewsCache[receipeIngredient._itemID].ItemCache.MatchType(Item.Types.Special))
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
				else
				{
					ItemStorageProxy component = this._itemViewsCache[receipeIngredient._itemID].GetComponent<ItemStorageProxy>();
					if (component && component._storage == this.Storage)
					{
						targetView = this._itemViewsCache[receipeIngredient._itemID];
					}
				}
			}
			foreach (ReceipeIngredient receipeIngredient2 in list)
			{
				this._ingredients.Remove(receipeIngredient2);
				this.ToggleItemInventoryView(receipeIngredient2._itemID, ItemProperties.Any);
			}
			this.Storage.UpdateContentVersion();
			this.CheckStorage();
			this._completedItemViewProxy._targetView = targetView;
		}

		
		private void DoCraft()
		{
			Receipe validRecipe = this._validRecipe;
			this._craftSfxEmitter.Play();
			LocalPlayer.Tuts.CloseRecipeTut();
			this._upgradeCount = 1;
			ItemProperties itemProperties = ItemProperties.Any;
			for (int i = 0; i < validRecipe._ingredients.Length; i++)
			{
				ReceipeIngredient recipeIngredient = validRecipe._ingredients[i];
				ReceipeIngredient receipeIngredient = this._ingredients.FirstOrDefault((ReceipeIngredient ig) => ig._itemID == recipeIngredient._itemID);
				int num = recipeIngredient._amount * ((i != 0 || !validRecipe._type.Equals(Receipe.Types.Upgrade)) ? this._upgradeCount : 1);
				receipeIngredient._amount -= num;
				if (i == 1)
				{
					itemProperties = ((!this._itemViewsCache[receipeIngredient._itemID]._allowMultiView) ? this._itemViewsCache[receipeIngredient._itemID].Properties : this._itemViewsCache[receipeIngredient._itemID].GetFirstViewProperties());
				}
				if (receipeIngredient._amount <= 0)
				{
					this._ingredients.Remove(receipeIngredient);
				}
				else if (recipeIngredient._amount == 0)
				{
					this._inventory.AddItem(recipeIngredient._itemID, receipeIngredient._amount, true, true, itemProperties);
					this.Remove(recipeIngredient._itemID, receipeIngredient._amount, null);
				}
				this.ToggleItemInventoryView(recipeIngredient._itemID, ItemProperties.Any);
			}
			int upgradeCount = this._upgradeCount;
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
		}

		
		public void CheckForValidRecipe()
		{
			if (this.CheckStorage())
			{
				return;
			}
			IOrderedEnumerable<Receipe> orderedEnumerable = null;
			IOrderedEnumerable<Receipe> orderedEnumerable2 = null;
			Receipe receipe;
			int i;
			if (this._ingredients.Count > 0 && this.CraftOverride == null)
			{
				orderedEnumerable = from ar in this._receipeBook.AvailableReceipesCache
				where this.CanCarryProduct(ar)
				where this._ingredients.All((ReceipeIngredient i) => ar._ingredients.Any((ReceipeIngredient i2) => i._itemID == i2._itemID))
				orderby ar._ingredients.Sum((ReceipeIngredient ari) => ari._amount)
				orderby ar._ingredients.Length
				select ar;
				orderedEnumerable2 = from ar in this._receipeBook.AvailableUpgradeCache
				where this.CanCarryUpgradeProduct(ar)
				where this._ingredients.All((ReceipeIngredient i) => ar._ingredients.Any((ReceipeIngredient i2) => i._itemID == i2._itemID))
				orderby ar._ingredients.Length
				select ar;
				receipe = orderedEnumerable.FirstOrDefault<Receipe>();
				Receipe receipe2 = orderedEnumerable2.FirstOrDefault<Receipe>();
				if (receipe2 != null && receipe != null && receipe._ingredients.Length > receipe2._ingredients.Length)
				{
					receipe = null;
				}
			}
			else
			{
				receipe = null;
			}
			bool flag = receipe != null;
			bool skipUpgrade2dFillingCog = flag;
			this._validRecipeFill = 0f;
			if (flag)
			{
				this._validRecipe = null;
				flag = false;
				Receipe receipe3 = null;
				float num = 0f;
				List<Receipe> list = new List<Receipe>();
				foreach (Receipe receipe4 in orderedEnumerable)
				{
					int num2 = 0;
					int num3 = receipe4._ingredients.Sum((ReceipeIngredient i) => i._amount);
					ReceipeIngredient cogIngredients;
					foreach (ReceipeIngredient cogIngredients2 in this._ingredients)
					{
						cogIngredients = cogIngredients2;
						ReceipeIngredient receipeIngredient = receipe4._ingredients.First((ReceipeIngredient i) => i._itemID == cogIngredients._itemID);
						num2 += ((cogIngredients._amount <= receipeIngredient._amount) ? cogIngredients._amount : receipeIngredient._amount);
						if (receipeIngredient._amount == 0 && cogIngredients._amount > 0)
						{
							num2++;
							num3++;
						}
					}
					this._validRecipeFull = false;
					this._validRecipeFill = (float)num2 / (float)num3;
					if (num2 != num3)
					{
						if (this._validRecipeFill > num)
						{
							num = this._validRecipeFill;
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
						num = 1f;
					}
				}
				skipUpgrade2dFillingCog = flag;
				this._validRecipeFill = num;
				if (receipe3 != null && !flag)
				{
					Scene.HudGui.CraftingReceipeBacking.gameObject.SetActive(true);
					Scene.HudGui.CraftingReceipeProgress.fillAmount = num;
				}
				else
				{
					Scene.HudGui.CraftingReceipeBacking.gameObject.SetActive(false);
				}
				Scene.HudGui.ShowValidCraftingRecipes(from r in list.Concat(orderedEnumerable2).Distinct(new CraftingCog.CompareRecipeProduct())
				orderby r._type
				select r);
			}
			else
			{
				if (orderedEnumerable2 != null)
				{
					if (orderedEnumerable != null)
					{
						Scene.HudGui.ShowValidCraftingRecipes(from r in orderedEnumerable.Concat(orderedEnumerable2).Distinct(new CraftingCog.CompareRecipeProduct())
						orderby r._type
						orderby r._ingredients.Length
						select r);
					}
					else
					{
						Scene.HudGui.ShowValidCraftingRecipes(from r in orderedEnumerable2.Distinct(new CraftingCog.CompareRecipeProduct())
						orderby r._ingredients.Length
						select r);
					}
				}
				else
				{
					HudGui hudGui = Scene.HudGui;
					IEnumerable<Receipe> receipes;
					if (orderedEnumerable != null)
					{
						IOrderedEnumerable<Receipe> orderedEnumerable3 = from r in orderedEnumerable.Distinct(new CraftingCog.CompareRecipeProduct())
						orderby r._ingredients.Length
						select r;
						receipes = orderedEnumerable3;
					}
					else
					{
						receipes = null;
					}
					hudGui.ShowValidCraftingRecipes(receipes);
				}
				Scene.HudGui.CraftingReceipeBacking.gameObject.SetActive(false);
			}
			if (this.CraftOverride == null)
			{
				if (flag)
				{
					Scene.HudGui.HideUpgradesDistribution();
					this._craftSfx2Emitter.Play();
					base.gameObject.GetComponent<Renderer>().enabled = true;
					base.gameObject.GetComponent<Collider>().enabled = true;
				}
				if (!flag || !this.CanCraft)
				{
					this.CheckForValidUpgrade(skipUpgrade2dFillingCog);
				}
			}
			else
			{
				Scene.HudGui.HideUpgradesDistribution();
				if (this.CraftOverride.CanCombine())
				{
					this._craftSfx2Emitter.Play();
					base.gameObject.GetComponent<Renderer>().enabled = true;
					base.gameObject.GetComponent<Collider>().enabled = true;
				}
				else
				{
					base.gameObject.GetComponent<Renderer>().enabled = false;
					base.gameObject.GetComponent<Collider>().enabled = false;
				}
			}
		}

		
		private void CheckForValidUpgrade(bool skipUpgrade2dFillingCog)
		{
			Receipe receipe = null;
			int i;
			if (this._ingredients.Count > 0)
			{
				IOrderedEnumerable<Receipe> source = from ar in this._receipeBook.AvailableUpgradeCache
				where this._ingredients.Any((ReceipeIngredient i) => i._itemID == ar._ingredients[0]._itemID) && this._ingredients.All((ReceipeIngredient i) => ar._ingredients.Any((ReceipeIngredient i2) => i._itemID == i2._itemID))
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
					if (array.Length > 1)
					{
						this._upgradeCount = ItemDatabase.ItemById(receipe._productItemID)._maxUpgradesAmount;
						int itemID = array[1]._itemID;
						if (this._upgradeCog.SupportedItemsCache.ContainsKey(itemID) && this._upgradeCog.SupportedItemsCache[itemID]._pattern != UpgradeCog.Patterns.NoView)
						{
							this._upgradeCount -= LocalPlayer.Inventory.GetAmountOfUpgrades(receipe._productItemID);
						}
						bool flag = false;
						bool flag2 = this._upgradeCount == 0;
						int num = receipe._ingredients[0]._amount;
						int num2 = receipe._ingredients.Sum((ReceipeIngredient i) => i._amount);
						for (i = 1; i < receipe._ingredients.Length; i++)
						{
							ReceipeIngredient receipeIngredient = receipe._ingredients[i];
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
								if (num4 < this._upgradeCount)
								{
									this._upgradeCount = num4;
								}
								num += Mathf.Min(array[num3]._amount, receipeIngredient._amount);
							}
							else
							{
								this._upgradeCount = 1;
								flag2 = true;
							}
						}
						if (!skipUpgrade2dFillingCog)
						{
							float num5 = (float)num / (float)num2;
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
				if (!skipUpgrade2dFillingCog)
				{
					Scene.HudGui.CraftingReceipeBacking.gameObject.SetActive(this._validRecipeFill > 0f);
					Scene.HudGui.CraftingReceipeProgress.fillAmount = this._validRecipeFill;
				}
				Scene.HudGui.HideUpgradesDistribution();
				this._upgradeCount = 0;
			}
			if (base.gameObject.GetComponent<Renderer>().enabled != this._upgradeCount > 0)
			{
				if (!base.gameObject.GetComponent<Renderer>().enabled)
				{
					this._craftSfx2Emitter.Play();
				}
				base.gameObject.GetComponent<Renderer>().enabled = (this._upgradeCount > 0);
				base.gameObject.GetComponent<Collider>().enabled = (this._upgradeCount > 0);
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
			bool flag = false;
			int i = 0;
			while (i < bonuses.Length)
			{
				switch (bonuses[i]._type)
				{
				case WeaponStatUpgrade.Types.BurningWeapon:
				{
					BurnableCloth componentInChildren = this._itemViewsCache[productItemId]._held.GetComponentInChildren<BurnableCloth>();
					if (componentInChildren)
					{
						this._itemViewsCache[productItemId].ActiveBonus = WeaponStatUpgrade.Types.BurningWeapon;
						componentInChildren.EnableBurnableCloth();
					}
					flag = true;
					break;
				}
				case WeaponStatUpgrade.Types.StickyProjectile:
					if (batched && this._itemViewsCache[productItemId]._allowMultiView)
					{
						this._itemViewsCache[productItemId].SetMultiviewsBonus(WeaponStatUpgrade.Types.StickyProjectile);
					}
					else
					{
						this._itemViewsCache[productItemId].ActiveBonus = WeaponStatUpgrade.Types.StickyProjectile;
					}
					flag = true;
					break;
				case WeaponStatUpgrade.Types.WalkmanTrack:
					WalkmanControler.LoadCassette(ingredientItemId);
					flag = true;
					break;
				case WeaponStatUpgrade.Types.BurningAmmo:
					if (batched && this._itemViewsCache[productItemId]._allowMultiView)
					{
						this._itemViewsCache[productItemId].SetMultiviewsBonus(WeaponStatUpgrade.Types.BurningAmmo);
					}
					else
					{
						this._itemViewsCache[productItemId].ActiveBonus = WeaponStatUpgrade.Types.BurningAmmo;
					}
					flag = true;
					break;
				case WeaponStatUpgrade.Types.Paint_Green:
				{
					EquipmentPainting componentInChildren2 = this._itemViewsCache[productItemId]._held.GetComponentInChildren<EquipmentPainting>();
					if (componentInChildren2)
					{
						componentInChildren2.PaintInGreen();
					}
					flag = true;
					break;
				}
				case WeaponStatUpgrade.Types.Paint_Orange:
				{
					EquipmentPainting componentInChildren3 = this._itemViewsCache[productItemId]._held.GetComponentInChildren<EquipmentPainting>();
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
					goto IL_50B;
				case WeaponStatUpgrade.Types.ItemPart:
				{
					IItemPartInventoryView itemPartInventoryView = (IItemPartInventoryView)this._itemViewsCache[productItemId];
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
					this._itemViewsCache[productItemId].Properties.Copy((lastIngredientProperties != ItemProperties.Any) ? lastIngredientProperties : this._itemViewsCache[ingredientItemId].Properties);
					LocalPlayer.Inventory.SortInventoryViewsByBonus(LocalPlayer.Inventory.InventoryItemViewsCache[ingredientItemId][0], this._itemViewsCache[productItemId].ActiveBonus, false);
					flag = true;
					break;
				case WeaponStatUpgrade.Types.PoisonnedAmmo:
					if (batched && this._itemViewsCache[productItemId]._allowMultiView)
					{
						this._itemViewsCache[productItemId].SetMultiviewsBonus(WeaponStatUpgrade.Types.PoisonnedAmmo);
					}
					else
					{
						this._itemViewsCache[productItemId].ActiveBonus = WeaponStatUpgrade.Types.PoisonnedAmmo;
					}
					flag = true;
					break;
				case WeaponStatUpgrade.Types.BurningWeaponExtra:
				{
					BurnableCloth componentInChildren4 = this._itemViewsCache[productItemId]._held.GetComponentInChildren<BurnableCloth>();
					if (componentInChildren4 && this._itemViewsCache[productItemId].ActiveBonus == WeaponStatUpgrade.Types.BurningWeapon)
					{
						this._itemViewsCache[productItemId].ActiveBonus = WeaponStatUpgrade.Types.BurningWeaponExtra;
						componentInChildren4.EnableBurnableClothExtra();
					}
					flag = true;
					break;
				}
				case WeaponStatUpgrade.Types.Incendiary:
					this._itemViewsCache[productItemId].ActiveBonus = WeaponStatUpgrade.Types.Incendiary;
					flag = true;
					break;
				case WeaponStatUpgrade.Types.BoneAmmo:
					if (batched && this._itemViewsCache[productItemId]._allowMultiView)
					{
						this._itemViewsCache[productItemId].SetMultiviewsBonus(WeaponStatUpgrade.Types.BoneAmmo);
					}
					else
					{
						this._itemViewsCache[productItemId].ActiveBonus = WeaponStatUpgrade.Types.BoneAmmo;
					}
					flag = true;
					break;
				case WeaponStatUpgrade.Types.CamCorderTape:
					CamCorderControler.LoadTape(ingredientItemId);
					flag = true;
					break;
				case WeaponStatUpgrade.Types.PoisonnedWeapon:
					if (this._itemViewsCache[productItemId]._heldWeaponInfo.bonus)
					{
						this._itemViewsCache[productItemId].ActiveBonus = WeaponStatUpgrade.Types.PoisonnedWeapon;
						this._itemViewsCache[productItemId]._heldWeaponInfo.bonus._bonusType = WeaponBonus.BonusTypes.Poison;
						this._itemViewsCache[productItemId]._heldWeaponInfo.bonus.enabled = true;
						RandomWeaponUpgradeVisual component = this._itemViewsCache[productItemId]._heldWeaponInfo.bonus.GetComponent<RandomWeaponUpgradeVisual>();
						if (component)
						{
							component.OnEnable();
						}
					}
					flag = true;
					break;
				case WeaponStatUpgrade.Types.ModernAmmo:
					if (batched && this._itemViewsCache[productItemId]._allowMultiView)
					{
						this._itemViewsCache[productItemId].SetMultiviewsBonus(WeaponStatUpgrade.Types.ModernAmmo);
					}
					else
					{
						this._itemViewsCache[productItemId].ActiveBonus = WeaponStatUpgrade.Types.ModernAmmo;
					}
					flag = true;
					break;
				case WeaponStatUpgrade.Types.TapedLight:
					LocalPlayer.Inventory.AddItem(Mathf.RoundToInt(bonuses[i]._amount), 1, false, false, null);
					flag = true;
					break;
				default:
					goto IL_50B;
				}
				IL_55F:
				i++;
				continue;
				IL_50B:
				flag = this.ApplyWeaponBonus(bonuses[i], productItemId, ingredientItemId, upgradeCount);
				if (flag)
				{
					GameStats.UpgradesAdded.Invoke(upgradeCount);
				}
				else
				{
					Debug.LogError("Attempting to upgrade " + this._itemViewsCache[productItemId].ItemCache._name + " which doesn't reference its weaponInfo component.");
				}
				goto IL_55F;
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

		
		private void ToggleItemInventoryView(int itemId, ItemProperties properties = null)
		{
			ReceipeIngredient receipeIngredient = this._ingredients.FirstOrDefault((ReceipeIngredient i) => i._itemID == itemId);
			int num = (receipeIngredient == null) ? 0 : receipeIngredient._amount;
			bool flag = num > 0;
			if (this._itemViewsCache.ContainsKey(itemId))
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
			if (this._itemViewsCache.ContainsKey(itemId))
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
			IEnumerable<Item> enumerable = ItemDatabase.ItemsByType(Item.Types.CraftingMaterial | Item.Types.Craftable);
			foreach (Item item in enumerable)
			{
				this.ToggleItemInventoryView(item._id, ItemProperties.Any);
			}
			this._lambdaMultiView.ClearMultiViews();
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

		
		[EnumFlags]
		public Item.Types _acceptedTypes;

		
		public ReceipeBook _receipeBook;

		
		public PlayerInventory _inventory;

		
		public Material _selectedMaterial;

		
		public GameObject _craftSfx;

		
		public GameObject _craftSfx2;

		
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
						if (x._productItemID != y._productItemID || x._productItemAmount._min != y._productItemAmount._min || x._productItemAmount._max != y._productItemAmount._max || x._weaponStatUpgrades.Length != y._weaponStatUpgrades.Length || x._weaponStatUpgrades.Length <= 0)
						{
							return false;
						}
						for (int j = 0; j < x._weaponStatUpgrades.Length; j++)
						{
							if (x._weaponStatUpgrades[j]._type != y._weaponStatUpgrades[j]._type)
							{
								return false;
							}
						}
					}
				}
				return true;
			}

			
			public int GetHashCode(Receipe obj)
			{
				return (int)(obj._productItemID + obj._type * (Receipe.Types)100000);
			}
		}
	}
}
