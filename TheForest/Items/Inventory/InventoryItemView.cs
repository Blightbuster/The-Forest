using System;
using System.Collections;
using System.Collections.Generic;
using TheForest.Items.Craft;
using TheForest.Items.Special;
using TheForest.Items.Utils;
using TheForest.Tools;
using TheForest.UI;
using TheForest.UI.Interfaces;
using TheForest.Utils;
using UniLinq;
using UnityEngine;

namespace TheForest.Items.Inventory
{
	
	[AddComponentMenu("Items/Inventory/Item Inventory View")]
	[DoNotSerializePublic]
	public class InventoryItemView : MonoBehaviour, IVirtualCursorSnapNodeGroupTester, IScreenSizeRatio
	{
		
		private void Start()
		{
			if (base.transform.parent)
			{
				this._itemSpread = base.transform.parent.GetComponent<inventoryItemSpreadSetup>();
			}
			if (this._isCraft && this._allowMultiView)
			{
				this._multiViews = new List<InventoryItemView>();
			}
			if (!this._hovered)
			{
				base.enabled = false;
			}
		}

		
		private void OnDisable()
		{
			if (this._hovered)
			{
				if (Scene.HudGui)
				{
					Scene.HudGui.HideItemInfoView(this, this._isCraft);
				}
				this.Highlight(false);
				this._hovered = false;
			}
			base.enabled = false;
		}

		
		public void OnMouseExitCollider()
		{
			if (base.enabled)
			{
				base.enabled = false;
				VirtualCursor.Instance.SetCursorType(VirtualCursor.CursorTypes.Inventory);
			}
			if (this._itemSpread && this._itemSpread.savingCompatibilityMode && !this._ignoreItemSpread)
			{
				this.disableItemSpread();
			}
		}

		
		public void OnMouseEnterCollider()
		{
			if (!base.enabled && !this._inventory._craftingCog._upgradeCog.enabled && (this._inventory._craftingCog.CraftOverride == null || !this._inventory._craftingCog.CraftOverride.IsCombining))
			{
				this.InitCombineTimer();
				this._hovered = true;
				base.enabled = true;
				VirtualCursor.Instance.SetCursorType(VirtualCursor.CursorTypes.InventoryHover);
				this.Highlight(true);
				Scene.HudGui.ShowItemInfoViewDelayed(this, (this._renderers.Length <= 0) ? null : this._renderers[0]._renderer, this._isCraft);
			}
			if (this._itemSpread && this._itemSpread.savingCompatibilityMode && !this._ignoreItemSpread)
			{
				this.enableItemSpread();
			}
		}

		
		private void Update()
		{
			if (this._inventory._craftingCog._upgradeCog.enabled || (this._inventory._craftingCog.CraftOverride != null && this._inventory._craftingCog.CraftOverride.IsCombining))
			{
				return;
			}
			bool buttonDown = TheForest.Utils.Input.GetButtonDown("Combine");
			bool flag = (InventoryItemView.CombiningItemId != this._itemId || !this.Properties.Match(InventoryItemView.CombiningItemProperty)) ? buttonDown : TheForest.Utils.Input.GetButton("Combine");
			if (flag)
			{
				InventoryItemView.CombiningItemId = this._itemId;
				InventoryItemView.CombiningItemProperty = this.Properties;
			}
			else
			{
				InventoryItemView.CombiningItemId = -1;
				InventoryItemView.CombiningItemProperty = ItemProperties.Any;
			}
			bool flag2 = this._item.MatchType(Item.Types.Special);
			SpecialItemControlerBase specialItemControlerBase = flag2 ? LocalPlayer.Inventory.SpecialItemsControlers[this._itemId] : null;
			if (this._isCraft)
			{
				if (flag)
				{
					if (flag2)
					{
						specialItemControlerBase.ToggleSpecialCraft(false);
					}
					if (this._allowMultiView && this.MultiViewOwner != null)
					{
						Scene.HudGui.HideItemInfoView(this, this._isCraft);
						this.MultiViewOwner.BubbleDownMultiview(this);
					}
					if (buttonDown)
					{
						LocalPlayer.Inventory.SkipNextAddItemWoosh = true;
						LocalPlayer.Sfx.PlayInventorySound(Item.SFXCommands.PlayWhoosh);
					}
					int num = (!TheForest.Utils.Input.GetButton("Batch")) ? this.GetCombineAmount() : 10;
					if (this._addMaxToCraft)
					{
						num = int.MaxValue;
					}
					if (num == 0)
					{
						return;
					}
					if (this._inventory._craftingCog.Storage && base.GetComponentInParent<ItemStorageProxy>())
					{
						num -= this._inventory._craftingCog.Storage.Remove(this._itemId, num, this._properties);
					}
					else
					{
						num -= this._inventory.CurrentStorage.Remove(this._itemId, num, this._properties);
					}
					this._inventory.AddItem(this._itemId, num, true, true, this._properties);
					this.ActiveBonus = (WeaponStatUpgrade.Types)(-1);
					if (LocalPlayer.Inventory.CurrentStorage is CraftingCog)
					{
						((CraftingCog)LocalPlayer.Inventory.CurrentStorage).CheckForValidRecipe();
					}
				}
				if (this._canEquipFromCraft && TheForest.Utils.Input.GetButtonUp("Equip") && this._item.MatchType(Item.Types.Equipment))
				{
					this._inventory.AddItem(this._itemId, 1, true, true, this._properties);
					this._inventory.Equip(this._itemId, false);
					this._inventory.CurrentStorage.Remove(this._itemId, 1, null);
					this.ActiveBonus = (WeaponStatUpgrade.Types)(-1);
					if (!this.ItemCache._preventClosingInventoryAfterEquip)
					{
						this._inventory.Close();
					}
				}
			}
			else
			{
				if (flag)
				{
					if (!this.CanUseWithPrimary && this.CanUseWithSecondary && LocalPlayer.Inventory.CurrentStorage is CraftingCog)
					{
						if (buttonDown)
						{
							this.UseEdible();
						}
					}
					else if (this.CanBeStored && this._item._equipmentSlot < Item.EquipmentSlot.Chest)
					{
						if (!flag2 || specialItemControlerBase.ToggleSpecialCraft(true))
						{
							if (this._allowMultiView && this.MultiViewOwner != null)
							{
								Scene.HudGui.HideItemInfoView(this, this._isCraft);
								this.MultiViewOwner.RemovedMultiView(this);
							}
							if (buttonDown)
							{
								LocalPlayer.Inventory.SkipNextAddItemWoosh = true;
								LocalPlayer.Sfx.PlayInventorySound(Item.SFXCommands.PlayWhoosh);
							}
							if (this._inventory.HasInSlot(this._item._equipmentSlot, this._itemId) && this._inventory.AmountOf(this._itemId, false) == 1)
							{
								this._inventory.MemorizeItem(this._item._equipmentSlot);
							}
							int num2;
							if (this._addMaxToCraft)
							{
								num2 = this._inventory.AmountOfItemWithBonus(this._itemId, this.ActiveBonus);
							}
							else if (TheForest.Utils.Input.GetButton("Batch"))
							{
								num2 = Mathf.Min(this._inventory.AmountOfItemWithBonus(this._itemId, this.ActiveBonus), 10);
							}
							else
							{
								num2 = Mathf.Min(this._inventory.AmountOfItemWithBonus(this._itemId, this.ActiveBonus), this.GetCombineAmount());
							}
							if (num2 == 0)
							{
								return;
							}
							if (num2 > 1)
							{
								LocalPlayer.Inventory.SortInventoryViewsByBonus(this, this.ActiveBonus, true);
							}
							else
							{
								this._inventory.BubbleUpInventoryView(this);
							}
							int num3 = this._inventory.CurrentStorage.Add(this._itemId, num2, this._properties);
							this._inventory.RemoveItem(this._itemId, num2 - num3, true, true);
						}
					}
					else if (buttonDown)
					{
						if (this._canDropFromInventory && this._item.MatchType(Item.Types.Droppable))
						{
							FakeParent component = this._held.GetComponent<FakeParent>();
							GameObject worldGo;
							if (!BoltNetwork.isRunning || !this.ItemCache._pickupPrefabMP)
							{
								worldGo = UnityEngine.Object.Instantiate<GameObject>((!this.ItemCache._pickupPrefab) ? this._worldPrefab : this.ItemCache._pickupPrefab.gameObject, (!component) ? this._held.transform.position : component.RealPosition, (!component) ? this._held.transform.rotation : component.RealRotation);
							}
							else
							{
								worldGo = BoltNetwork.Instantiate(this.ItemCache._pickupPrefabMP.gameObject, (!component) ? this._held.transform.position : component.RealPosition, (!component) ? this._held.transform.rotation : component.RealRotation).gameObject;
							}
							this.OnItemDropped(worldGo);
							this._inventory.BubbleUpInventoryView(this);
							this._inventory.RemoveItem(this._itemId, 1, false, true);
						}
						else if (this._item._equipmentSlot >= Item.EquipmentSlot.Chest && LocalPlayer.Inventory.EquipmentSlots[(int)this._item._equipmentSlot] == this)
						{
							LocalPlayer.Inventory.UnequipItemAtSlot(this._item._equipmentSlot, false, true, false);
						}
					}
				}
				if (TheForest.Utils.Input.GetButtonUp("Equip"))
				{
					if (this.CanUseWithPrimary)
					{
						this.UseEdible();
					}
					else if (this._item.MatchType(Item.Types.Equipment))
					{
						if (!this._item._inventoryTooltipOnly)
						{
							this._inventory.BubbleUpInventoryView(this);
							if (!this.ItemCache._preventClosingInventoryAfterEquip)
							{
								Time.timeScale = 1f;
							}
							this._inventory.Equip(this._itemId, false);
							if (!this.ItemCache._preventClosingInventoryAfterEquip)
							{
								this._inventory.Close();
							}
						}
					}
					else if (flag2)
					{
						this._inventory.SpecialItemsControlers[this._itemId].ToggleSpecial(true);
						if (this._item._equipedSFX != Item.SFXCommands.None)
						{
							this._inventory.SendMessage(this._item._equipedSFX.ToString());
						}
						if (!this.ItemCache._preventClosingInventoryAfterEquip)
						{
							this._inventory.Close();
						}
					}
				}
			}
		}

		
		private void OnDestroy()
		{
			if (this._renderers != null)
			{
				foreach (InventoryItemView.RendererDefinition rendererDefinition in this._renderers)
				{
					rendererDefinition.Clear();
				}
			}
			this._renderers = null;
		}

		
		public virtual void OnDeserialized()
		{
			if (base.GetComponent<EmptyObjectIdentifier>())
			{
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
		}

		
		public virtual void OnSerializing()
		{
		}

		
		public virtual void OnItemAdded()
		{
		}

		
		public virtual void OnItemRemoved()
		{
		}

		
		public virtual void OnItemDropped(GameObject worldGo)
		{
		}

		
		public virtual void OnItemEquipped()
		{
		}

		
		public virtual void OnMultiviewSpawned()
		{
		}

		
		public virtual void OnMultiviewDespawning()
		{
		}

		
		public void Highlight(bool onoff)
		{
			if (this._renderers != null)
			{
				foreach (InventoryItemView.RendererDefinition rendererDefinition in this._renderers)
				{
					rendererDefinition.Highlight(onoff, this._itemId);
				}
			}
		}

		
		public void Sheen(bool onoff)
		{
			if (this._renderers != null)
			{
				foreach (InventoryItemView.RendererDefinition rendererDefinition in this._renderers)
				{
					rendererDefinition.Sheen(onoff);
				}
			}
		}

		
		public virtual void Init()
		{
			if (this._itemId > 0)
			{
				this._item = ItemDatabase.ItemById(this._itemId);
				Renderer component = base.gameObject.GetComponent<Renderer>();
				if (component && component.enabled && !base.GetComponent<VirtualCursorSnapNode>())
				{
					VirtualCursorSnapNode virtualCursorSnapNode = base.gameObject.AddComponent<VirtualCursorSnapNode>();
					virtualCursorSnapNode._layer = LocalPlayer.Inventory._craftingCog.GetComponent<VirtualCursorSnapNode>()._layer;
					virtualCursorSnapNode.enabled = false;
					virtualCursorSnapNode.enabled = true;
					virtualCursorSnapNode.Refresh();
				}
				this._canEquipFromCraft = this._item.MatchType(Item.Types.Equipment);
				return;
			}
			Debug.Log("InventoryItemView not setup : " + base.name);
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(base.gameObject, 0.5f);
			}
		}

		
		private void InitCombineTimer()
		{
			if (InventoryItemView.CombiningItemId != this._itemId || !this.Properties.Match(InventoryItemView.CombiningItemProperty))
			{
				float buttonTimePressed = TheForest.Utils.Input.player.GetButtonTimePressed("Combine");
				float num = 0.35f;
				int lastCombineTick = Mathf.CeilToInt(buttonTimePressed / num);
				InventoryItemView.LastCombineTick = lastCombineTick;
			}
		}

		
		private int GetCombineAmount()
		{
			float buttonTimePressed = TheForest.Utils.Input.player.GetButtonTimePressed("Combine");
			float num = 0.35f;
			int num2 = Mathf.CeilToInt(buttonTimePressed / num);
			int result;
			if (num2 > InventoryItemView.LastCombineTick)
			{
				result = Mathf.RoundToInt(Mathf.Pow((float)num2, 2f));
			}
			else if (buttonTimePressed == 0f)
			{
				result = 1;
				num2 = 1;
			}
			else
			{
				result = 0;
			}
			InventoryItemView.LastCombineTick = num2;
			return result;
		}

		
		public virtual void UseEdible()
		{
			InventoryItemView.CombiningItemId = -1;
			InventoryItemView.CombiningItemProperty = ItemProperties.Any;
			if (this.ActiveBonus == WeaponStatUpgrade.Types.DirtyWater)
			{
				LocalPlayer.Stats.HitWaterDelayed(1);
			}
			this.ActiveBonus = (WeaponStatUpgrade.Types)(-1);
			this._inventory.BubbleUpInventoryView(this);
			if (this._item.MatchType(Item.Types.Armor))
			{
				this._inventory.RemoveItem(this._itemId, 1, false, true);
				if (!ItemUtils.ApplyEffectsToStats(this._item._usedStatEffect, true, 1))
				{
					this._inventory.AddItemNF(this._itemId, 1, false, false, null);
					LocalPlayer.Sfx.PlayItemCustomSfx(this.ItemCache, true);
					return;
				}
				EventRegistry.Player.Publish(TfEvent.UsedItem, this._itemId);
			}
			else if (!this._item.MatchType(Item.Types.Equipment))
			{
				if (!ItemUtils.ApplyEffectsToStats(this._item._usedStatEffect, true, 1))
				{
					LocalPlayer.Sfx.PlayWhoosh();
					return;
				}
				this._inventory.RemoveItem(this._itemId, 1, false, true);
				EventRegistry.Player.Publish(TfEvent.UsedItem, this._itemId);
			}
			else
			{
				ItemUtils.ApplyEffectsToStats(this._item._usedStatEffect, true, 1);
			}
			if (this._item._usedSFX != Item.SFXCommands.None)
			{
				LocalPlayer.Sfx.PlayInventorySound(this._item._usedSFX);
			}
		}

		
		public void ApplyEquipmentEffect(bool forward)
		{
			ItemUtils.ApplyEffectsToStats(this._item._equipedStatEffect, forward, 1);
		}

		
		public InventoryItemView GetFirstView()
		{
			if (this._multiViews != null && this._multiViews.Count > 0)
			{
				return this._multiViews[0];
			}
			return this;
		}

		
		public InventoryItemView GetFirstViewForItem(int itemId)
		{
			if (this._multiViews != null && this._multiViews.Count > 0)
			{
				for (int i = 0; i < this._multiViews.Count; i++)
				{
					if (this._multiViews[i]._itemId == itemId)
					{
						return this._multiViews[i];
					}
				}
				return null;
			}
			return (this._itemId != itemId) ? null : this;
		}

		
		public ItemProperties GetFirstViewProperties()
		{
			if (this._multiViews != null && this._multiViews.Count > 0)
			{
				return this._multiViews[0].Properties;
			}
			return ItemProperties.Any;
		}

		
		public ItemProperties GetFirstViewPropertiesForItem(int itemId)
		{
			if (this._multiViews != null && this._multiViews.Count > 0)
			{
				for (int i = 0; i < this._multiViews.Count; i++)
				{
					if (this._multiViews[i]._itemId == itemId)
					{
						return this._multiViews[i].Properties;
					}
				}
			}
			return ItemProperties.Any;
		}

		
		public int AmountOfMultiviewWithProperties(int itemId, ItemProperties properties)
		{
			if (this._allowMultiView && this._multiViews != null)
			{
				int num = 0;
				for (int i = this._multiViews.Count - 1; i >= 0; i--)
				{
					if (this._multiViews[i]._itemId == itemId && (properties == ItemProperties.Any || this._multiViews[i].Properties.Match(properties)))
					{
						num++;
					}
				}
				return num;
			}
			return 0;
		}

		
		public int AmountOfMultiviewWithoutProperties(int itemId, ItemProperties properties)
		{
			if (this._allowMultiView && this._multiViews != null)
			{
				int num = 0;
				for (int i = this._multiViews.Count - 1; i >= 0; i--)
				{
					if (this._multiViews[i]._itemId == itemId && !this._multiViews[i].Properties.Match(properties))
					{
						num++;
					}
				}
				return num;
			}
			return 0;
		}

		
		public void SetMultiViewAmount(int amount, ItemProperties properties)
		{
			if (this._allowMultiView)
			{
				int i = Mathf.Clamp(amount, 0, this._maxMultiViews);
				if (this._multiViews == null)
				{
					this._multiViews = new List<InventoryItemView>();
				}
				base.gameObject.SetActive(true);
				int num = (properties != ItemProperties.Any) ? this.AmountOfMultiviewWithProperties(this._itemId, properties) : this._multiViews.Count;
				while (i > num)
				{
					num++;
					this.SpawnAnyMultiview(this, base.transform.parent, true, properties);
				}
				if (i < num)
				{
					this.RemovedMultiViews(this._itemId, num - i, properties, false);
				}
				base.gameObject.SetActive(false);
			}
		}

		
		public void SetAnyMultiViewAmount(InventoryItemView source, Transform parent, int amount, ItemProperties properties, bool returnExtrasToInventory)
		{
			if (this._allowMultiView)
			{
				int i = Mathf.Clamp(amount, 0, this._maxMultiViews);
				if (this._multiViews == null)
				{
					this._multiViews = new List<InventoryItemView>();
				}
				base.gameObject.SetActive(true);
				int num = (properties != ItemProperties.Any) ? this.AmountOfMultiviewWithProperties(source._itemId, properties) : this._multiViews.Count;
				i -= this.AmountOfMultiviewWithProperties(source._itemId, ItemProperties.Any) - num;
				while (i > num)
				{
					num++;
					this.SpawnAnyMultiview(source, parent, false, properties);
				}
				if (i < num)
				{
					this.RemovedMultiViews(source._itemId, num - i, properties, returnExtrasToInventory);
				}
			}
		}

		
		public void SpawnAnyMultiview(InventoryItemView source, Transform parent, bool randomRotation, ItemProperties properties)
		{
			Vector3 euler = new Vector3(0f, (float)UnityEngine.Random.Range(-50, 50), 0f);
			bool flag = source.Equals(this);
			Collider component = source.GetComponent<Collider>();
			Vector3 size;
			if (component is BoxCollider)
			{
				size = ((BoxCollider)component).size;
			}
			else if (component is CapsuleCollider)
			{
				CapsuleCollider capsuleCollider = (CapsuleCollider)component;
				int direction = capsuleCollider.direction;
				if (direction != 0)
				{
					if (direction != 1)
					{
						size = new Vector3(capsuleCollider.radius * 2f, capsuleCollider.radius * 2f, capsuleCollider.height);
					}
					else
					{
						size = new Vector3(capsuleCollider.radius * 2f, capsuleCollider.height, capsuleCollider.radius * 2f);
					}
				}
				else
				{
					size = new Vector3(capsuleCollider.height, capsuleCollider.radius * 2f, capsuleCollider.radius * 2f);
				}
			}
			else
			{
				size = component.bounds.size;
			}
			Vector3 vector = UnityEngine.Random.insideUnitSphere;
			if (!flag)
			{
				vector *= 3f;
			}
			if (size.x * 2f < size.z)
			{
				vector /= 15f;
				vector *= size.x * 0.75f;
				vector.x /= 2f;
			}
			else if (size.z * 2f < size.x)
			{
				vector /= 15f;
				vector *= size.z * 0.75f;
				vector.z /= 2f;
			}
			else
			{
				vector /= 8f;
				bool flag2 = size.z * 2f > size.y;
				bool flag3 = size.x * 2f > size.y;
				if (flag2 && !flag3)
				{
					euler.z = (float)UnityEngine.Random.Range(-45, 45);
				}
				if (flag3 && !flag2)
				{
					euler.x = (float)UnityEngine.Random.Range(-45, 45);
				}
			}
			vector.y = 0f;
			InventoryItemView inventoryItemView = UnityEngine.Object.Instantiate<InventoryItemView>(source, base.transform.position + vector, (!randomRotation) ? source.transform.rotation : (source.transform.rotation * Quaternion.Euler(euler)));
			inventoryItemView.transform.parent = parent;
			inventoryItemView.transform.localScale = source.transform.localScale;
			inventoryItemView._isCraft = true;
			inventoryItemView.MultiViewOwner = this;
			inventoryItemView.enabled = true;
			inventoryItemView.gameObject.SetActive(true);
			inventoryItemView.Init();
			inventoryItemView.Highlight(false);
			inventoryItemView.OnMultiviewSpawned();
			if (properties != ItemProperties.Any)
			{
				inventoryItemView.Properties.Copy(properties);
			}
			if (this._bonusListener)
			{
				this._bonusListener.ToggleItemViewMat(inventoryItemView);
			}
			this._multiViews.Add(inventoryItemView);
		}

		
		public void SetMultiviewsBonus(WeaponStatUpgrade.Types bonus)
		{
			if (this._multiViews != null && this._multiViews.Count > 0)
			{
				for (int i = 0; i < this._multiViews.Count; i++)
				{
					this._multiViews[i].ActiveBonus = bonus;
				}
			}
		}

		
		public List<WeaponStatUpgrade.Types> GetMultiviewsBonus()
		{
			if (this._multiViews == null)
			{
				return null;
			}
			List<WeaponStatUpgrade.Types> list = new List<WeaponStatUpgrade.Types>();
			foreach (InventoryItemView inventoryItemView in this._multiViews)
			{
				list.Add(inventoryItemView.ActiveBonus);
			}
			return list;
		}

		
		public bool ContainsMultiView(int itemId)
		{
			return this._multiViews.Any((InventoryItemView mv) => mv._itemId == itemId);
		}

		
		private void BubbleDownMultiview(InventoryItemView iiv)
		{
			if (this._multiViews.Contains(iiv))
			{
				this._multiViews.Remove(iiv);
				this._multiViews.Insert(0, iiv);
			}
		}

		
		public int AmountOfMultiView(int itemId)
		{
			return this._multiViews.Count((InventoryItemView mv) => mv._itemId == itemId);
		}

		
		private void RemovedMultiView(InventoryItemView view)
		{
			if (this._multiViews.Contains(view))
			{
				this._multiViews.Remove(view);
				UnityEngine.Object.Destroy(view.gameObject);
			}
		}

		
		public void RemovedMultiViews(int itemId, int amount, ItemProperties properties, bool returnToInventory = false)
		{
			if (this._multiViews == null)
			{
				this._multiViews = new List<InventoryItemView>();
			}
			int num = this._multiViews.FindIndex((InventoryItemView mv) => mv._itemId == itemId && mv.Properties.Match(properties));
			while (num >= 0 && amount > 0)
			{
				amount--;
				if (this._multiViews[num])
				{
					if (returnToInventory)
					{
						LocalPlayer.Inventory.AddItem(this._multiViews[num]._itemId, 1, true, true, this._multiViews[num].Properties);
					}
					this._multiViews[num].OnMultiviewDespawning();
					UnityEngine.Object.Destroy(this._multiViews[num].gameObject);
				}
				this._multiViews.RemoveAt(num);
				num = this._multiViews.FindIndex((InventoryItemView mv) => mv._itemId == itemId && mv.Properties.Match(properties));
			}
		}

		
		public void ClearMultiViews()
		{
			if (this._multiViews == null)
			{
				this._multiViews = new List<InventoryItemView>();
			}
			while (this._multiViews.Count > 0)
			{
				if (this._multiViews[0])
				{
					UnityEngine.Object.Destroy(this._multiViews[0].gameObject);
				}
				this._multiViews.RemoveAt(0);
			}
		}

		
		public void ClearActiveBonus()
		{
			this.ActiveBonus = (WeaponStatUpgrade.Types)(-1);
		}

		
		private bool isNeighbourActive()
		{
			return (!this._itemSpread.singleItemMode && !this._itemSpread.spreadHoveredItemOnlyMode) || true;
		}

		
		private void enableItemSpread()
		{
			int num = 0;
			if (this._itemSpread.minSpreadTargetAmount > 0)
			{
				for (int i = 0; i < this._itemSpread.sourceObjects.Count; i++)
				{
					if (this._itemSpread.sourceObjects[i].activeSelf)
					{
						num++;
					}
				}
				if (num < this._itemSpread.minSpreadTargetAmount)
				{
					return;
				}
			}
			if (this._itemSpread.spreadHoveredItemOnlyMode)
			{
				LocalPlayer.Sfx.PlayItemCustomSfx(this.ItemCache, PlayerInventory.SfxListenerSpacePosition(base.transform.position), false);
				base.StartCoroutine("enableSingleItemSpreadRoutine");
				return;
			}
			this._itemSpread.spreadActive = true;
			if (!this._itemSpread.doingItemSpread && this.isNeighbourActive())
			{
				LocalPlayer.Sfx.PlayItemCustomSfx(this.ItemCache, PlayerInventory.SfxListenerSpacePosition(base.transform.position), false);
				this._itemSpread.StartCoroutine("enableItemSpreadRoutine");
			}
		}

		
		private void disableItemSpread()
		{
			if (this._itemSpread.spreadHoveredItemOnlyMode)
			{
				base.StartCoroutine("disableSingleItemSpreadRoutine");
				return;
			}
			this._itemSpread.spreadActive = false;
			if (this._itemSpread.singleItemMode || this._itemSpread.spreadHoveredItemOnlyMode)
			{
				base.StartCoroutine(this.resetSpread(0.1f));
			}
			else
			{
				base.StartCoroutine(this.resetSpread(0.25f));
			}
		}

		
		private IEnumerator resetSpread(float delay)
		{
			float t = 0f;
			while (t < delay)
			{
				t += Time.unscaledDeltaTime;
				yield return null;
			}
			if (this._itemSpread.doingItemSpread && !this._itemSpread.spreadActive && base.gameObject.activeInHierarchy)
			{
				this._itemSpread.StartCoroutine("disableItemSpreadRoutine");
			}
			yield break;
		}

		
		private IEnumerator enableSingleItemSpreadRoutine()
		{
			float t = 0f;
			base.StopCoroutine("disableSingleItemSpreadRoutine");
			while (t < 1f)
			{
				if (this._itemSpread.offsetRenderersMode)
				{
					this._itemSpread.offsetSourceObjects[this._itemSpreadIndex].transform.localPosition = Vector3.Lerp(this._itemSpread.offsetSourceObjects[this._itemSpreadIndex].transform.localPosition, this._itemSpread.targetPositions[this._itemSpreadIndex], t);
					this._itemSpread.offsetSourceObjects[this._itemSpreadIndex].transform.localRotation = Quaternion.Lerp(this._itemSpread.offsetSourceObjects[this._itemSpreadIndex].transform.localRotation, this._itemSpread.targetRotations[this._itemSpreadIndex], t);
				}
				else
				{
					base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, this._itemSpread.targetPositions[this._itemSpreadIndex], t);
					base.transform.localRotation = Quaternion.Lerp(base.transform.localRotation, this._itemSpread.targetRotations[this._itemSpreadIndex], t);
				}
				t += Time.unscaledDeltaTime * 2f;
				yield return null;
			}
			yield break;
		}

		
		private IEnumerator disableSingleItemSpreadRoutine()
		{
			float t = 0f;
			while (t < 0.15f)
			{
				t += Time.unscaledDeltaTime;
				yield return null;
			}
			base.StopCoroutine("enableSingleItemSpreadRoutine");
			t = 0f;
			while (t < 1f)
			{
				if (this._itemSpread.offsetRenderersMode)
				{
					this._itemSpread.offsetSourceObjects[this._itemSpreadIndex].transform.localPosition = Vector3.Lerp(this._itemSpread.offsetSourceObjects[this._itemSpreadIndex].transform.localPosition, this._itemSpread.startPositions[this._itemSpreadIndex], t);
					this._itemSpread.offsetSourceObjects[this._itemSpreadIndex].transform.localRotation = Quaternion.Lerp(this._itemSpread.offsetSourceObjects[this._itemSpreadIndex].transform.localRotation, this._itemSpread.startRotations[this._itemSpreadIndex], t);
				}
				else
				{
					base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, this._itemSpread.startPositions[this._itemSpreadIndex], t);
					base.transform.localRotation = Quaternion.Lerp(base.transform.localRotation, this._itemSpread.startRotations[this._itemSpreadIndex], t);
				}
				t += Time.unscaledDeltaTime * 2f;
				yield return null;
			}
			yield break;
		}

		
		public void PlayCustomSFX()
		{
			LocalPlayer.Sfx.PlayItemCustomSfx(this.ItemCache, PlayerInventory.SfxListenerSpacePosition(base.transform.position), false);
		}

		
		
		public Item ItemCache
		{
			get
			{
				return this._item;
			}
		}

		
		
		public bool CanBeHotkeyed
		{
			get
			{
				return this._inventory._craftingCog.CraftOverride != null && (this._item.MatchType(this._inventory._craftingCog.CraftOverride.AcceptedTypes) || this._item._allowQuickSelect);
			}
		}

		
		
		public bool CanBeStored
		{
			get
			{
				return LocalPlayer.Inventory.CurrentStorage.IsValidItem(this._item) || this.CanBeHotkeyed;
			}
		}

		
		
		public bool CanUseWithPrimary
		{
			get
			{
				return this._item.MatchType(Item.Types.Edible) && (this.ActiveBonus == this._item._edibleCondition || this.ActiveBonus == this._item._altEdibleCondition) && (this._item._rechargeOfItemId == 0 || LocalPlayer.Inventory.Owns(this._item._rechargeOfItemId, true));
			}
		}

		
		
		public bool CanUseWithSecondary
		{
			get
			{
				return this._item.MatchType(Item.Types.Edible) && (this.ActiveBonus == this._item._secondaryEdibleCondition || this.ActiveBonus == this._item._secondaryAltEdibleCondition) && (this._item._rechargeOfItemId == 0 || LocalPlayer.Inventory.Owns(this._item._rechargeOfItemId, true));
			}
		}

		
		
		public bool IsHeldOnly
		{
			get
			{
				return this._item != null && this._item._maxAmount < 0;
			}
		}

		
		
		
		public Material NormalMaterial
		{
			get
			{
				return this._renderers[0]._defaultMaterial;
			}
			set
			{
				this._renderers[0]._defaultMaterial = value;
			}
		}

		
		
		
		public InventoryItemView MultiViewOwner { get; set; }

		
		
		
		public virtual WeaponStatUpgrade.Types ActiveBonus
		{
			get
			{
				return this.Properties.ActiveBonus;
			}
			set
			{
				this.Properties.ActiveBonus = value;
				if (this._bonusListener)
				{
					this._bonusListener.ToggleItemViewMat(this);
				}
			}
		}

		
		
		public virtual ItemProperties Properties
		{
			get
			{
				if (this._properties == null)
				{
					this._properties = new ItemProperties
					{
						ActiveBonus = (WeaponStatUpgrade.Types)(-1)
					};
				}
				return this._properties;
			}
		}

		
		
		public float ScreenSizeRatio
		{
			get
			{
				return this._onScreenSizeRatio;
			}
		}

		
		public bool BelongWith(IVirtualCursorSnapNodeGroupTester group)
		{
			InventoryItemView inventoryItemView = group as InventoryItemView;
			return inventoryItemView && inventoryItemView._itemId == this._itemId && this.Properties.Match(inventoryItemView.Properties) && this._isCraft == inventoryItemView._isCraft;
		}

		
		[ItemIdPicker]
		public int _itemId;

		
		public bool _isCraft;

		
		public bool _canEquipFromCraft;

		
		public bool _canDropFromInventory;

		
		public bool _addMaxToCraft;

		
		public bool _allowMultiView;

		
		public int _maxMultiViews = 20;

		
		public InventoryItemView.RendererDefinition[] _renderers;

		
		public GameObject _held;

		
		public weaponInfo _heldWeaponInfo;

		
		public GameObject _worldPrefab;

		
		public GameObject _altWorldPrefab;

		
		[HideInInspector]
		public PlayerInventory _inventory;

		
		public ActiveBonusListenerMat _bonusListener;

		
		public Transform _modelOffsetTr;

		
		public Material _materialWhenAttachedToPack;

		
		public int _itemSpreadIndex;

		
		public bool _ignoreItemSpread;

		
		public float _onScreenSizeRatio = 1f;

		
		protected bool _hovered;

		
		protected bool _hasUpgrades;

		
		private List<InventoryItemView> _multiViews;

		
		protected Item _item;

		
		protected ItemProperties _properties;

		
		private inventoryItemSpreadSetup _itemSpread;

		
		protected static int CombiningItemId;

		
		protected static ItemProperties CombiningItemProperty = ItemProperties.Any;

		
		protected static int LastCombineTick;

		
		[Serializable]
		public class RendererDefinition
		{
			
			public void Highlight(bool onoff, int itemId)
			{
				if (onoff)
				{
					if (this._defaultMaterial != this._renderer.sharedMaterial && this._renderer.sharedMaterial != this._selectedMaterial && this._renderer.sharedMaterial != this._sheenMaterial)
					{
						this._defaultMaterial = this._renderer.sharedMaterial;
					}
					if (this._renderer.sharedMaterial == this._sheenMaterial)
					{
						LocalPlayer.Inventory.SheenItem(itemId, ItemProperties.Any, false);
					}
				}
				this._renderer.sharedMaterial = ((!onoff) ? this._defaultMaterial : this._selectedMaterial);
			}

			
			public void Sheen(bool onoff)
			{
				if (this._sheenMaterial && onoff)
				{
					if (this._defaultMaterial != this._renderer.sharedMaterial && this._renderer.sharedMaterial != this._selectedMaterial && this._renderer.sharedMaterial != this._sheenMaterial)
					{
						this._defaultMaterial = this._renderer.sharedMaterial;
					}
					this._renderer.sharedMaterial = this._sheenMaterial;
				}
				if (this._defaultMaterial && !onoff)
				{
					this._renderer.sharedMaterial = this._defaultMaterial;
				}
			}

			
			public void Clear()
			{
				this._renderer = null;
				this._defaultMaterial = null;
				this._selectedMaterial = null;
			}

			
			public Renderer _renderer;

			
			public Material _defaultMaterial;

			
			public Material _selectedMaterial;

			
			public Material _sheenMaterial;
		}
	}
}
