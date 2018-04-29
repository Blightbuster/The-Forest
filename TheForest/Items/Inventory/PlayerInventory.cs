using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Bolt;
using FMOD.Studio;
using TheForest.Items.Core;
using TheForest.Items.Craft;
using TheForest.Items.Special;
using TheForest.Items.Utils;
using TheForest.Items.World;
using TheForest.Tools;
using TheForest.UI;
using TheForest.Utils;
using UniLinq;
using UnityEngine;
using UnityEngine.Events;

namespace TheForest.Items.Inventory
{
	
	[AddComponentMenu("Items/Inventory/Player Inventory")]
	[DoNotSerializePublic]
	public class PlayerInventory : MonoBehaviour
	{
		
		private void Awake()
		{
			this.SpecialItemsControlers = new Dictionary<int, SpecialItemControlerBase>();
			this._equipmentSlots = new InventoryItemView[Enum.GetValues(typeof(Item.EquipmentSlot)).Length];
			this._equipmentSlotsPrevious = new InventoryItemView[this._equipmentSlots.Length];
			this._equipmentSlotsPreviousOverride = new InventoryItemView[this._equipmentSlots.Length];
			this._equipmentSlotsNext = new InventoryItemView[this._equipmentSlots.Length];
			this._equipmentSlotsLocked = new bool[this._equipmentSlots.Length];
			this._equipPreviousTime = float.MaxValue;
			this._noEquipedItem = this._inventoryGO.AddComponent<InventoryItemView>();
			this._noEquipedItem.enabled = false;
			for (int i = 0; i < this._equipmentSlots.Length; i++)
			{
				this._equipmentSlots[i] = this._noEquipedItem;
				this._equipmentSlotsPrevious[i] = this._noEquipedItem;
				this._equipmentSlotsPreviousOverride[i] = this._noEquipedItem;
				this._equipmentSlotsNext[i] = this._noEquipedItem;
			}
			this._quickSelectItemIds = new int[4];
			EventRegistry.Player.Subscribe(TfEvent.EquippedItem, new EventRegistry.SubscriberCallback(this.CheckQuickSelectAutoAdd));
			EventRegistry.Player.Subscribe(TfEvent.AddedItem, new EventRegistry.SubscriberCallback(this.CheckQuickSelectAutoAdd));
			this._itemAnimHash = new ItemAnimatorHashHelper();
			this.InitItemCache();
			if (!LevelSerializer.IsDeserializing)
			{
				foreach (QuickSelectViews quickSelectViews2 in LocalPlayer.QuickSelectViews)
				{
					quickSelectViews2.Awake();
				}
			}
		}

		
		public void Start()
		{
			if (ForestVR.Prototype)
			{
				base.enabled = false;
				return;
			}
			this._craftingCog._inventory = this;
			for (int i = 0; i < this._itemViews.Length; i++)
			{
				if (this._itemViews[i])
				{
					this._itemViews[i].Init();
				}
			}
			this._pm = base.GetComponentInChildren<playerScriptSetup>().pmControl;
			this._inventoryGO.SetActive(false);
			this._inventoryGO.transform.parent = null;
			this._inventoryGO.transform.eulerAngles = new Vector3(0f, this._inventoryGO.transform.eulerAngles.y, 0f);
			if (!LevelSerializer.IsDeserializing)
			{
				for (int j = 0; j < ItemDatabase.Items.Length; j++)
				{
					Item item = ItemDatabase.Items[j];
					this.ToggleInventoryItemView(item._id, false, null);
				}
				DecayingInventoryItemView.LastUsed = null;
			}
			else
			{
				base.enabled = false;
			}
		}

		
		public IEnumerator OnDeserialized()
		{
			this._possessedItems.RemoveRange(this._possessedItemsCount, this._possessedItems.Count - this._possessedItemsCount);
			this._possessedItemCache = this._possessedItems.ToDictionary((InventoryItem i) => i._itemId);
			yield return YieldPresets.WaitOnePointFiveSeconds;
			try
			{
				try
				{
					foreach (KeyValuePair<int, List<InventoryItemView>> keyValuePair in this._inventoryItemViewsCache)
					{
						for (int i2 = keyValuePair.Value.Count<InventoryItemView>() - 1; i2 >= 0; i2--)
						{
							if (!keyValuePair.Value[i2])
							{
								keyValuePair.Value.RemoveAt(i2);
							}
						}
					}
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
				try
				{
					if (!this.SpecialItemsControlers.ContainsKey(ItemDatabase.ItemByName("Compass")._id))
					{
						Debug.LogError("Adding compass controller");
						CompassControler compassControler = this._specialItems.AddComponent<CompassControler>();
						compassControler._itemId = ItemDatabase.ItemByName("Compass")._id;
						compassControler._button = SpecialItemControlerBase.Buttons.Utility;
					}
					if (!this.SpecialItemsControlers.ContainsKey(ItemDatabase.ItemByName("MetalTinTray")._id))
					{
						Debug.LogError("Adding metal tin tray controller");
						MetalTinTrayControler metalTinTrayControler = this._specialItems.AddComponent<MetalTinTrayControler>();
						metalTinTrayControler._itemId = ItemDatabase.ItemByName("MetalTinTray")._id;
						metalTinTrayControler._storage = this._specialItems.transform.Find("MetalTinTrayStorage").GetComponent<ItemStorage>();
					}
				}
				catch (Exception exception2)
				{
					Debug.LogException(exception2);
				}
				this.FixMaxAmountBonuses();
				try
				{
					if (!this._possessedItemCache.ContainsKey(this.DefaultLight._itemId) && !this._equipmentSlotsIds.Contains(this.DefaultLight._itemId))
					{
						this.AddItem(this.DefaultLight._itemId, 1, true, false, null);
					}
				}
				catch (Exception exception3)
				{
					Debug.LogException(exception3);
				}
				try
				{
					if (!this._possessedItemCache.ContainsKey(this._defaultWeaponItemId) && !this._equipmentSlotsIds.Contains(this._defaultWeaponItemId))
					{
						this.AddItem(this._defaultWeaponItemId, 1, true, false, null);
					}
				}
				catch (Exception exception4)
				{
					Debug.LogException(exception4);
				}
				try
				{
					if (!LocalPlayer.SavedData.ExitedEndgame)
					{
						int timmyPhotoItemId = ItemDatabase.ItemByName("PhotoTimmy")._id;
						if (!this._possessedItems.Any((InventoryItem pi) => pi._itemId == timmyPhotoItemId) && !this._equipmentSlotsIds.Contains(timmyPhotoItemId))
						{
							Debug.LogError("Adding timmy photo failsafe");
							this.AddItem(timmyPhotoItemId, 1, true, true, null);
						}
					}
				}
				catch (Exception exception5)
				{
					Debug.LogException(exception5);
				}
				try
				{
					this._craftingCog.OnDeserialized();
					this._craftingCog.GetComponent<UpgradeCog>().Awake();
				}
				catch (Exception exception6)
				{
					Debug.LogException(exception6);
				}
				try
				{
					for (int j = 0; j < ItemDatabase.Items.Length; j++)
					{
						Item item = ItemDatabase.Items[j];
						try
						{
							this.ToggleInventoryItemView(item._id, true, ItemProperties.Any);
							bool flag = this._possessedItemCache.ContainsKey(item._id);
							if (item.MatchType(Item.Types.Special) && (flag || this._equipmentSlotsIds.Contains(item._id)))
							{
								this._specialItems.SendMessage("PickedUpSpecialItem", item._id);
							}
							if (flag)
							{
								this._possessedItemCache[item._id]._maxAmount = ((item._maxAmount != 0) ? item._maxAmount : int.MaxValue);
								if (this._possessedItemCache[item._id]._amount > this._possessedItemCache[item._id].MaxAmount)
								{
									this._possessedItemCache[item._id]._amount = this._possessedItemCache[item._id].MaxAmount;
								}
								for (int k = 0; k < this._inventoryItemViewsCache[item._id].Count; k++)
								{
									if (this._inventoryItemViewsCache[item._id][k].gameObject.activeSelf)
									{
										this._inventoryItemViewsCache[item._id][k].OnDeserialized();
									}
								}
							}
						}
						catch (Exception)
						{
						}
					}
				}
				catch (Exception exception7)
				{
					Debug.LogException(exception7);
				}
				yield return null;
				try
				{
					if (this._equipmentSlotsIds != null)
					{
						this.HideAllEquiped(false, false);
						for (int l = 0; l < this._equipmentSlotsIds.Length; l++)
						{
							if (this._equipmentSlotsIds[l] > 0)
							{
								this.LockEquipmentSlot((Item.EquipmentSlot)l);
							}
						}
						for (int m = 0; m < this._equipmentSlotsIds.Length; m++)
						{
							if (this._equipmentSlotsIds[m] > 0)
							{
								int num = this._equipmentSlotsIds[m];
								ItemUtils.ApplyEffectsToStats(this._inventoryItemViewsCache[num][0].ItemCache._equipedStatEffect, false, 1);
								this._equipmentSlots[m] = null;
								this.UnlockEquipmentSlot((Item.EquipmentSlot)m);
								if (!this.Equip(num, true))
								{
									this.AddItem(num, 1, true, true, null);
								}
							}
						}
					}
				}
				catch (Exception exception8)
				{
					Debug.LogException(exception8);
				}
				try
				{
					if (this._upgradeCounters != null)
					{
						for (int n = 0; n < this._upgradeCountersCount; n++)
						{
							PlayerInventory.SerializableItemUpgradeCounters serializableItemUpgradeCounters = this._upgradeCounters[n];
							if (!this.ItemsUpgradeCounters.ContainsKey(serializableItemUpgradeCounters._itemId))
							{
								this.ItemsUpgradeCounters[serializableItemUpgradeCounters._itemId] = new PlayerInventory.UpgradeCounterDict();
							}
							for (int num2 = 0; num2 < serializableItemUpgradeCounters._count; num2++)
							{
								PlayerInventory.SerializableUpgradeCounter serializableUpgradeCounter = serializableItemUpgradeCounters._counters[num2];
								this._craftingCog.ApplyWeaponStatsUpgrades(serializableItemUpgradeCounters._itemId, serializableUpgradeCounter._upgradeItemId, this._craftingCog._upgradeCog.SupportedItemsCache[serializableUpgradeCounter._upgradeItemId]._weaponStatUpgrades, false, serializableUpgradeCounter._amount, null);
								this.ItemsUpgradeCounters[serializableItemUpgradeCounters._itemId][serializableUpgradeCounter._upgradeItemId] = serializableUpgradeCounter._amount;
							}
						}
					}
				}
				catch (Exception exception9)
				{
					Debug.LogException(exception9);
				}
				try
				{
					for (int num3 = 0; num3 < this._upgradeViewReceivers.Count; num3++)
					{
						this._upgradeViewReceivers[num3].OnDeserialized();
					}
				}
				catch (Exception exception10)
				{
					Debug.LogException(exception10);
				}
				try
				{
					foreach (QuickSelectViews quickSelectViews2 in LocalPlayer.QuickSelectViews)
					{
						quickSelectViews2.Awake();
						quickSelectViews2.ShowLocalPlayerViews();
					}
				}
				catch (Exception exception11)
				{
					Debug.LogException(exception11);
				}
			}
			finally
			{
				base.enabled = true;
				DecayingInventoryItemView.LastUsed = null;
			}
			yield break;
		}

		
		private void Update()
		{
			if (SteamDSConfig.isDedicatedServer)
			{
				return;
			}
			if ((this.CurrentView != PlayerInventory.PlayerViews.Pause && !this.BlockTogglingInventory && !LocalPlayer.AnimControl.useRootMotion && !LocalPlayer.FpCharacter.jumping && !LocalPlayer.FpCharacter.drinking && !LocalPlayer.AnimControl.endGameCutScene && !LocalPlayer.AnimControl.blockInventoryOpen && TheForest.Utils.Input.GetButtonDown("Inventory")) || (this.CurrentView == PlayerInventory.PlayerViews.Inventory && TheForest.Utils.Input.GetButtonDown("Esc")))
			{
				Scene.HudGui.ClearMpPlayerList();
				this.ToggleInventory();
			}
			else if ((TheForest.Utils.Input.GetButtonDown("Esc") && this.CurrentView != PlayerInventory.PlayerViews.Book && !LocalPlayer.Create.CreateMode && !this.QuickSelectGamepadSwitch) || (this.CurrentView == PlayerInventory.PlayerViews.Pause && !Scene.HudGui.IsNull() && !Scene.HudGui.PauseMenu.IsNull() && !Scene.HudGui.PauseMenu.activeSelf))
			{
				Scene.HudGui.ClearMpPlayerList();
				this.TogglePauseMenu();
			}
			else if (this.CurrentView == PlayerInventory.PlayerViews.World)
			{
				if (TheForest.Utils.Input.GetButtonDown("Fire1"))
				{
					this.Attack();
				}
				if (TheForest.Utils.Input.GetButtonUp("Fire1"))
				{
					this.ReleaseAttack();
				}
				else if (TheForest.Utils.Input.GetButtonDown("AltFire"))
				{
					this.Block();
				}
				else if (TheForest.Utils.Input.GetButtonUp("AltFire"))
				{
					this.UnBlock();
				}
				else if (TheForest.Utils.Input.GetButtonDown("Drop") && !LocalPlayer.AnimControl.upsideDown && !this.IsSlotEmpty(Item.EquipmentSlot.RightHand))
				{
					InventoryItemView inventoryItemView = this._equipmentSlots[0];
					Item itemCache = inventoryItemView.ItemCache;
					this.DropEquipedWeapon(itemCache.MatchType(Item.Types.Droppable) && !this.UseAltWorldPrefab);
				}
				if (this._equipPreviousTime < Time.time && !LocalPlayer.AnimControl.upsideDown && !LocalPlayer.Create.CreateMode)
				{
					this.EquipPreviousWeapon(false);
				}
				this.CheckQuickSelect();
			}
			else if (this.CurrentView == PlayerInventory.PlayerViews.ClosingInventory)
			{
				this.CurrentView = PlayerInventory.PlayerViews.World;
			}
			this.RefreshDropIcon();
		}

		
		private void OnDestroy()
		{
			foreach (QuickSelectViews quickSelectViews2 in LocalPlayer.QuickSelectViews)
			{
				if (quickSelectViews2)
				{
					quickSelectViews2.OnDestroy();
				}
			}
		}

		
		private void OnLevelWasLoaded()
		{
			if (Application.loadedLevelName == "TitleScene")
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		
		public static Vector3 SfxListenerSpacePosition(Vector3 worldPosition)
		{
			return LocalPlayer.MainCamTr.TransformPoint(LocalPlayer.InventoryCam.transform.InverseTransformPoint(worldPosition));
		}

		
		public void ToggleInventory()
		{
			if (this.CurrentView == PlayerInventory.PlayerViews.Inventory || LocalPlayer.Stats.Dead)
			{
				this.Close();
			}
			else if (!LocalPlayer.WaterViz.InWater)
			{
				this.Open(this._craftingCog);
			}
			else
			{
				LocalPlayer.Tuts.ShowNoInventoryUnderWater();
			}
		}

		
		public void Open(IItemStorage storage)
		{
			if (this.CurrentView != PlayerInventory.PlayerViews.Inventory || this.CurrentStorage != storage)
			{
				bool flag = this.CurrentView == PlayerInventory.PlayerViews.Book;
				if (this.CurrentView == PlayerInventory.PlayerViews.Book)
				{
					LocalPlayer.Create.CloseBookForInventory();
					base.enabled = true;
				}
				this.CurrentStorage = storage;
				this.CurrentStorage.Open();
				this.CurrentView = PlayerInventory.PlayerViews.Inventory;
				LocalPlayer.FpCharacter.LockView(true);
				if (!ForestVR.Enabled)
				{
					LocalPlayer.MainCam.enabled = false;
				}
				VirtualCursor.Instance.SetCursorType(VirtualCursor.CursorTypes.Inventory);
				Scene.HudGui.CheckHudState();
				Scene.HudGui.Grid.gameObject.SetActive(false);
				this._inventoryGO.tag = "open";
				this._inventoryGO.SetActive(true);
				if (ForestVR.Enabled)
				{
					this._inventoryGO.transform.position = LocalPlayer.InventoryPositionVR.transform.position;
					this._inventoryGO.transform.rotation = LocalPlayer.InventoryPositionVR.transform.rotation;
					LocalPlayer.InventoryMouseEventsVR.enabled = true;
				}
				LocalPlayer.Sfx.PlayOpenInventory();
				this.IsOpenningInventory = true;
				base.Invoke("PauseTimeInInventory", (!flag) ? 0.05f : 0.25f);
			}
		}

		
		private void PauseTimeInInventory()
		{
			if (this.CurrentView == PlayerInventory.PlayerViews.Inventory)
			{
				this.IsOpenningInventory = false;
				if (!BoltNetwork.isRunning && !GameSetup.IsHardMode && !GameSetup.IsHardSurvivalMode && !ForestVR.Enabled)
				{
					Time.timeScale = 0f;
				}
				if (!ForestVR.Enabled)
				{
					Application.targetFrameRate = 60;
				}
				this._pauseSnapshot = FMODCommon.PlayOneshot("snapshot:/Inventory Pause", Vector3.zero, new object[0]);
			}
		}

		
		public void Close()
		{
			if (this.CurrentView == PlayerInventory.PlayerViews.Inventory || this._inventoryGO.activeSelf)
			{
				this.CurrentView = PlayerInventory.PlayerViews.ClosingInventory;
				this.CurrentStorage.Close();
				this.CurrentStorage = null;
				this._inventoryGO.tag = "closed";
				this._inventoryGO.SetActive(false);
				Scene.HudGui.CheckHudState();
				Scene.HudGui.Grid.gameObject.SetActive(true);
				LocalPlayer.FpCharacter.UnLockView();
				LocalPlayer.MainCam.enabled = true;
				LocalPlayer.InventoryMouseEventsVR.enabled = false;
				PlayerPreferences.ApplyMaxFrameRate();
				VirtualCursor.Instance.SetCursorType(VirtualCursor.CursorTypes.Hand);
				if (!string.IsNullOrEmpty(this.PendingSendMessage))
				{
					base.SendMessage(this.PendingSendMessage);
					this.PendingSendMessage = null;
				}
				LocalPlayer.Sfx.PlayCloseInventory();
				Time.timeScale = 1f;
				this.IsOpenningInventory = false;
				if (this._pauseSnapshot != null && this._pauseSnapshot.isValid())
				{
					UnityUtil.ERRCHECK(this._pauseSnapshot.stop(STOP_MODE.ALLOWFADEOUT));
				}
			}
		}

		
		public void TogglePauseMenu()
		{
			this.BlockTogglingInventory = false;
			bool flag = this.CurrentView == PlayerInventory.PlayerViews.Pause;
			Scene.HudGui.TogglePauseMenu(!flag);
		}

		
		public void FixMaxAmountBonuses()
		{
			try
			{
				foreach (InventoryItem inventoryItem in this._possessedItemCache.Values)
				{
					Item item = ItemDatabase.ItemById(inventoryItem._itemId);
					if (inventoryItem._maxAmountBonus > 0)
					{
						inventoryItem._maxAmountBonus = 0;
					}
				}
				foreach (InventoryItem inventoryItem2 in this._possessedItemCache.Values)
				{
					Item item2 = ItemDatabase.ItemById(inventoryItem2._itemId);
					if (item2 != null && item2._ownedStatEffect != null && item2._ownedStatEffect.Length > 0)
					{
						ItemUtils.ApplyEffectsToStats(item2._ownedStatEffect, true, inventoryItem2._amount);
						inventoryItem2._amount = Mathf.Clamp(inventoryItem2._amount, 0, inventoryItem2.MaxAmount);
					}
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		
		private void CheckQuickSelect()
		{
			if (this.CurrentView == PlayerInventory.PlayerViews.World && !LocalPlayer.AnimControl.swimming && !LocalPlayer.AnimControl.onRope && !LocalPlayer.AnimControl.onRaft && !LocalPlayer.AnimControl.sitting && !LocalPlayer.AnimControl.upsideDown && !LocalPlayer.AnimControl.skinningAnimal && !LocalPlayer.AnimControl.endGameCutScene && !LocalPlayer.AnimControl.useRootMotion && !LocalPlayer.Create.CreateMode && !LocalPlayer.AnimControl.PlayerIsAttacking())
			{
				bool flag = !TheForest.Utils.Input.IsGamePad || this.QuickSelectGamepadSwitch;
				for (int i = 0; i < this._quickSelectItemIds.Length; i++)
				{
					if (this._quickSelectItemIds[i] > 0 && flag && TheForest.Utils.Input.GetButtonDown(this._quickSelectButtons[i]) && this.Owns(this._quickSelectItemIds[i], false))
					{
						Item item = ItemDatabase.ItemById(this._quickSelectItemIds[i]);
						if (item.MatchType(Item.Types.Equipment))
						{
							if (this.Equip(this._quickSelectItemIds[i], false))
							{
								this.UnBlock();
								this.Blocking(true);
								LocalPlayer.Sfx.PlayWhoosh();
							}
						}
						else if (item.MatchType(Item.Types.Edible))
						{
							this.InventoryItemViewsCache[this._quickSelectItemIds[i]][0].UseEdible();
							foreach (QuickSelectViews quickSelectViews2 in LocalPlayer.QuickSelectViews)
							{
								quickSelectViews2.ShowLocalPlayerViews();
							}
						}
					}
				}
			}
		}

		
		private void CheckQuickSelectAutoAdd(object o)
		{
			this.CheckQuickSelectAutoAdd((int)o);
		}

		
		private void CheckQuickSelectAutoAdd(int itemId)
		{
			if (this.CurrentView == PlayerInventory.PlayerViews.World)
			{
				Item item = ItemDatabase.ItemById(itemId);
				if (item != null && item.MatchType(Item.Types.Edible | Item.Types.Projectile | Item.Types.RangedWeapon | Item.Types.Weapon))
				{
					for (int i = 0; i < this._quickSelectItemIds.Length; i++)
					{
						if (this._quickSelectItemIds[i] == itemId)
						{
							return;
						}
					}
					for (int j = 0; j < this._quickSelectItemIds.Length; j++)
					{
						if (this._quickSelectItemIds[j] <= 0)
						{
							this._quickSelectItemIds[j] = itemId;
							return;
						}
					}
					EventRegistry.Player.Unsubscribe(TfEvent.EquippedItem, new EventRegistry.SubscriberCallback(this.CheckQuickSelectAutoAdd));
					EventRegistry.Player.Unsubscribe(TfEvent.AddedItem, new EventRegistry.SubscriberCallback(this.CheckQuickSelectAutoAdd));
				}
			}
		}

		
		public void LockEquipmentSlot(Item.EquipmentSlot slot)
		{
			this._equipmentSlotsLocked[(int)slot] = true;
		}

		
		public void UnlockEquipmentSlot(Item.EquipmentSlot slot)
		{
			this._equipmentSlotsLocked[(int)slot] = false;
		}

		
		public bool IsSlotLocked(Item.EquipmentSlot slot)
		{
			return this._equipmentSlotsLocked[(int)slot];
		}

		
		public bool Equip(int itemId, bool pickedUpFromWorld)
		{
			if (itemId == this.Logs._logItemId)
			{
				if (this.Logs.Lift())
				{
					EventRegistry.Player.Publish(TfEvent.EquippedItem, itemId);
					return true;
				}
				return false;
			}
			else
			{
				int num = this.AmountOf(itemId, false);
				if (pickedUpFromWorld)
				{
					num++;
					if (ItemDatabase.ItemById(itemId)._maxAmount > 0 && num > this.GetMaxAmountOf(itemId))
					{
						return false;
					}
				}
				if (num > 0 && this.Equip(this._inventoryItemViewsCache[itemId][Mathf.Min(num, this._inventoryItemViewsCache[itemId].Count) - 1], pickedUpFromWorld))
				{
					EventRegistry.Player.Publish(TfEvent.EquippedItem, itemId);
					return true;
				}
				return false;
			}
		}

		
		private bool Equip(InventoryItemView itemView, bool pickedUpFromWorld)
		{
			if (itemView != null)
			{
				this._equipPreviousTime = float.MaxValue;
				Item.EquipmentSlot equipmentSlot = itemView.ItemCache._equipmentSlot;
				if ((this.Logs.HasLogs && equipmentSlot != Item.EquipmentSlot.LeftHand) || (LocalPlayer.AnimControl.carry && equipmentSlot != Item.EquipmentSlot.LeftHand) || (LocalPlayer.Create.CreateMode && itemView.ItemCache.MatchType(Item.Types.Projectile | Item.Types.RangedWeapon | Item.Types.Weapon)) || (!(itemView != this._equipmentSlots[(int)equipmentSlot]) && !pickedUpFromWorld) || this.IsSlotLocked(equipmentSlot) || (itemView.ItemCache.MatchType(Item.Types.Special) && !this.SpecialItemsControlers[itemView._itemId].ToggleSpecial(true)))
				{
					return false;
				}
				if (pickedUpFromWorld || this.RemoveItem(itemView._itemId, 1, false, false))
				{
					this.LockEquipmentSlot(equipmentSlot);
					base.StartCoroutine(this.EquipSequence(equipmentSlot, itemView));
					return true;
				}
			}
			else
			{
				Debug.LogWarning(base.name + " is trying to equip a null object");
			}
			return false;
		}

		
		private IEnumerator EquipSequence(Item.EquipmentSlot slot, InventoryItemView itemView)
		{
			bool specialItemCheck = true;
			if (this._equipmentSlots[(int)slot] != null && this._equipmentSlots[(int)slot] != this._noEquipedItem)
			{
				this._pendingEquip = true;
				this._equipmentSlotsNext[(int)slot] = itemView;
				bool canStash = this._equipmentSlots[(int)slot].ItemCache._maxAmount >= 0;
				if (canStash)
				{
					this.MemorizeItem(slot);
				}
				this._itemAnimHash.ApplyAnimVars(this._equipmentSlots[(int)slot].ItemCache, false);
				int currentItemId = this._equipmentSlots[(int)slot]._itemId;
				if (Time.timeScale > 0f)
				{
					float durationCountdown = this._equipmentSlots[(int)slot].ItemCache._unequipDelay;
					while (this._pendingEquip && durationCountdown > 0f)
					{
						durationCountdown -= Time.deltaTime;
						yield return null;
					}
				}
				if (!this.HasInSlot(slot, currentItemId) || !this.HasInNextSlot(slot, itemView._itemId))
				{
					if (canStash)
					{
						this.AddItem(itemView._itemId, 1, true, true, null);
					}
					else
					{
						this.FakeDrop(itemView._itemId, null);
					}
					if (this._equipmentSlotsNext[(int)slot] == itemView)
					{
						this._equipmentSlotsNext[(int)slot] = this._noEquipedItem;
					}
					this._pendingEquip = false;
					yield break;
				}
				this.UnlockEquipmentSlot(slot);
				if (itemView.ItemCache.MatchType(Item.Types.Special))
				{
					specialItemCheck = this.SpecialItemsControlers[itemView._itemId].ToggleSpecial(true);
				}
				if (specialItemCheck)
				{
					this.UnequipItemAtSlot(slot, !canStash, canStash, false);
				}
				this._equipmentSlotsNext[(int)slot] = this._noEquipedItem;
			}
			else if (itemView.ItemCache.MatchType(Item.Types.Special))
			{
				specialItemCheck = this.SpecialItemsControlers[itemView._itemId].ToggleSpecial(true);
			}
			if (specialItemCheck)
			{
				if (itemView._held)
				{
					this._equipmentSlots[(int)slot] = itemView;
					itemView.OnItemEquipped();
					itemView._held.SetActive(true);
					itemView.ApplyEquipmentEffect(true);
					this._itemAnimHash.ApplyAnimVars(itemView.ItemCache, true);
					if (itemView.ItemCache._equipedSFX != Item.SFXCommands.None)
					{
						LocalPlayer.Sfx.SendMessage(itemView.ItemCache._equipedSFX.ToString(), SendMessageOptions.DontRequireReceiver);
					}
					if (itemView.ItemCache._maxAmount >= 0)
					{
						this.ToggleAmmo(itemView, true);
						this.ToggleInventoryItemView(itemView._itemId, false, null);
					}
					yield return (!itemView.ItemCache.MatchType(Item.Types.Projectile)) ? null : YieldPresets.WaitPointSevenSeconds;
				}
				else
				{
					Debug.LogError("Trying to equip item '" + itemView.ItemCache._name + "' which doesn't have a held reference in " + itemView.name);
				}
				this.UnlockEquipmentSlot(slot);
			}
			this._pendingEquip = false;
			yield break;
		}

		
		public void StopPendingEquip()
		{
			this._pendingEquip = false;
		}

		
		public void MemorizeItem(Item.EquipmentSlot slot)
		{
			if (Mathf.Approximately(this._equipPreviousTime, 3.40282347E+38f))
			{
				this._equipmentSlotsPrevious[(int)slot] = this._equipmentSlots[(int)slot];
			}
			else
			{
				this._equipPreviousTime = float.MaxValue;
			}
		}

		
		public void MemorizeOverrideItem(Item.EquipmentSlot slot)
		{
			if (Mathf.Approximately(this._equipPreviousTime, 3.40282347E+38f))
			{
				this._equipmentSlotsPreviousOverride[(int)slot] = this._equipmentSlots[(int)slot];
			}
			else
			{
				this._equipPreviousTime = float.MaxValue;
			}
		}

		
		public void EquipPreviousUtility(bool keepPrevious = false)
		{
			if (this._equipmentSlotsPrevious[1] != null && this._equipmentSlotsPrevious[1] != this._noEquipedItem)
			{
				this.Equip(this._equipmentSlotsPrevious[1]._itemId, false);
				if (!keepPrevious)
				{
					this._equipmentSlotsPrevious[1] = null;
				}
			}
		}

		
		public void EquipPreviousWeaponDelayed()
		{
			this._equipPreviousTime = Time.time + 0.45f;
		}

		
		public void CancelEquipPreviousWeaponDelayed()
		{
			this._equipPreviousTime = float.MaxValue;
		}

		
		public void EquipPreviousWeapon(bool fallbackToDefault = true)
		{
			if (!this.EquipPreviousOverride() && this._equipmentSlotsPrevious[0] != this._noEquipedItem)
			{
				this._equipPreviousTime = float.MaxValue;
				if (this._equipmentSlotsPrevious[0] != null)
				{
					if (this.Equip(this._equipmentSlotsPrevious[0]._itemId, false))
					{
						this._equipmentSlotsPrevious[0] = null;
					}
					else
					{
						this.Equip(this._defaultWeaponItemId, false);
						this._equipmentSlotsPrevious[0] = this._inventoryItemViewsCache[this._defaultWeaponItemId][0];
					}
				}
				else if (fallbackToDefault)
				{
					this.Equip(this._defaultWeaponItemId, false);
					this._equipmentSlotsPrevious[0] = this._inventoryItemViewsCache[this._defaultWeaponItemId][0];
				}
			}
		}

		
		private bool EquipPreviousOverride()
		{
			if (this._equipmentSlotsPreviousOverride[0] != this._noEquipedItem && this._equipmentSlotsPreviousOverride[0])
			{
				if (this.Equip(this._equipmentSlotsPreviousOverride[0]._itemId, false))
				{
					this._equipPreviousTime = float.MaxValue;
					this._equipmentSlotsPreviousOverride[0] = this._noEquipedItem;
					return true;
				}
				this._equipmentSlotsPreviousOverride[0] = this._noEquipedItem;
			}
			return false;
		}

		
		public void DropEquipedWeapon(bool equipPrevious)
		{
			this.DroppedRightHand.Invoke();
			LocalPlayer.Animator.SetBoolReflected("lookAtItemRight", false);
			this.UnequipItemAtSlot(Item.EquipmentSlot.RightHand, true, false, equipPrevious);
		}

		
		public void StashEquipedWeapon(bool equipPrevious)
		{
			this.StashedRightHand.Invoke();
			this.UnequipItemAtSlot(Item.EquipmentSlot.RightHand, false, true, equipPrevious);
		}

		
		public void StashLeftHand()
		{
			this.StashedLeftHand.Invoke();
			if (this.HasInSlot(Item.EquipmentSlot.LeftHand, this.DefaultLight._itemId))
			{
				this.DefaultLight.StashLighter();
			}
			else
			{
				this.UnequipItemAtSlot(Item.EquipmentSlot.LeftHand, false, true, false);
			}
		}

		
		public void HideAllEquiped(bool hideOnly = false, bool skipMemorize = false)
		{
			if (!hideOnly)
			{
				if (LocalPlayer.AnimControl.carry)
				{
					LocalPlayer.AnimControl.DropBody();
				}
				else if (this.Logs.HasLogs && this.Logs.PutDown(false, true, false, null))
				{
					this.Logs.PutDown(false, true, false, null);
				}
			}
			if (!this.IsSlotEmpty(Item.EquipmentSlot.LeftHand))
			{
				this.MemorizeItem(Item.EquipmentSlot.LeftHand);
			}
			if (!this.IsSlotEmpty(Item.EquipmentSlot.RightHand) && !skipMemorize)
			{
				this.MemorizeItem(Item.EquipmentSlot.RightHand);
			}
			this.StashLeftHand();
			this.StashEquipedWeapon(false);
		}

		
		public void ShowAllEquiped(bool fallbackToDefault = true)
		{
			this.EquipPreviousUtility(false);
			this.EquipPreviousWeapon(fallbackToDefault);
		}

		
		public void HideRightHand(bool hideOnly = false)
		{
			if (!hideOnly)
			{
				if (LocalPlayer.AnimControl.carry)
				{
					LocalPlayer.AnimControl.DropBody();
				}
				else if (this.Logs.HasLogs && this.Logs.PutDown(false, true, false, null))
				{
					this.Logs.PutDown(false, true, false, null);
				}
			}
			this.MemorizeItem(Item.EquipmentSlot.RightHand);
			this.StashEquipedWeapon(false);
		}

		
		public void ShowRightHand(bool fallbackToDefault = true)
		{
			this.EquipPreviousWeapon(fallbackToDefault);
		}

		
		public void Attack()
		{
			if (!this.IsRightHandEmpty() && !this._isThrowing && !this.IsReloading && !this.blockRangedAttack && !this.IsSlotLocked(Item.EquipmentSlot.RightHand) && !LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, LocalPlayer.AnimControl._slingShotId))
			{
				TheForest.Utils.Input.ResetDelayedAction();
				Item itemCache = this._equipmentSlots[0].ItemCache;
				if (itemCache.MatchType(Item.Types.RangedWeapon) && itemCache._lastAmmoAttackEvent != Item.FSMEvents.None && this.AmountOf(itemCache._ammoItemId, false) == 1)
				{
					LocalPlayer.Stats.UsedStick();
					this._pm.SendEvent(itemCache._lastAmmoAttackEvent.ToString());
				}
				else if (itemCache._attackEvent != Item.FSMEvents.None)
				{
					LocalPlayer.Stats.UsedStick();
					this._pm.SendEvent(itemCache._attackEvent.ToString());
				}
				if (itemCache._attackSFX != Item.SFXCommands.None)
				{
					LocalPlayer.Sfx.SendMessage(itemCache._attackSFX.ToString(), SendMessageOptions.DontRequireReceiver);
				}
				if (itemCache.MatchType(Item.Types.Projectile))
				{
					this._isThrowing = true;
					base.Invoke("ThrowProjectile", itemCache._projectileThrowDelay);
					LocalPlayer.TargetFunctions.Invoke("sendPlayerAttacking", 0.5f);
					LocalPlayer.SpecialItems.SendMessage("stopLightHeldFire", SendMessageOptions.DontRequireReceiver);
				}
				else if (itemCache.MatchType(Item.Types.RangedWeapon))
				{
					if (itemCache.MatchRangedStyle(Item.RangedStyle.Instantaneous))
					{
						base.Invoke("FireRangedWeapon", itemCache._projectileThrowDelay);
					}
					else
					{
						this._weaponChargeStartTime = Time.time;
					}
				}
				this.Attacked.Invoke();
			}
		}

		
		public void ReleaseAttack()
		{
			if (!this.IsRightHandEmpty() && !this._isThrowing && !this.blockRangedAttack && !LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, LocalPlayer.AnimControl._slingShotId))
			{
				Item itemCache = this._equipmentSlots[0].ItemCache;
				if (this.CancelNextChargedAttack)
				{
					this.CancelNextChargedAttack = false;
					return;
				}
				if (itemCache.MatchType(Item.Types.RangedWeapon) && itemCache.MatchRangedStyle(Item.RangedStyle.Charged))
				{
					if (itemCache._attackReleaseEvent != Item.FSMEvents.None)
					{
						LocalPlayer.Stats.UsedStick();
						this._pm.SendEvent(itemCache._attackReleaseEvent.ToString());
					}
					this._isThrowing = true;
					base.Invoke("FireRangedWeapon", itemCache._projectileThrowDelay);
					this.ReleasedAttack.Invoke();
				}
			}
		}

		
		public void Block()
		{
			this.Blocking(true);
			if (!this.IsRightHandEmpty() && !this._isThrowing)
			{
				Item itemCache = this._equipmentSlots[0].ItemCache;
				if (itemCache._blockEvent != Item.FSMEvents.None)
				{
					LocalPlayer.Stats.UsedStick();
					this._pm.SendEvent(itemCache._blockEvent.ToString());
					this.Blocked.Invoke();
				}
			}
		}

		
		public void UnBlock()
		{
			this.Blocking(false);
			if (!this.IsRightHandEmpty())
			{
				Item itemCache = this._equipmentSlots[0].ItemCache;
				if (itemCache._unblockEvent != Item.FSMEvents.None)
				{
					LocalPlayer.Stats.UsedStick();
					this._pm.SendEvent(itemCache._unblockEvent.ToString());
					this.Unblocked.Invoke();
				}
			}
		}

		
		private void Blocking(bool onoff)
		{
			if (TheForest.Utils.Input.IsGamePad)
			{
				this.BlockTogglingInventory = onoff;
				this.QuickSelectGamepadSwitch = onoff;
			}
			else if (this.QuickSelectGamepadSwitch)
			{
				this.BlockTogglingInventory = false;
				this.QuickSelectGamepadSwitch = false;
			}
		}

		
		public bool AddItem(int itemId, int amount = 1, bool preventAutoEquip = false, bool fromCraftingCog = false, ItemProperties properties = null)
		{
			if (this.ItemFilter != null)
			{
				return this.ItemFilter.AddItem(itemId, amount, preventAutoEquip, fromCraftingCog, properties);
			}
			return this.AddItemNF(itemId, amount, preventAutoEquip, fromCraftingCog, properties);
		}

		
		public bool AddItemNF(int itemId, int amount = 1, bool preventAutoEquip = false, bool fromCraftingCog = false, ItemProperties properties = null)
		{
			if (this.Logs != null && itemId == this.Logs._logItemId)
			{
				return this.Logs.Lift();
			}
			Item item = ItemDatabase.ItemById(itemId);
			if (item == null)
			{
				return false;
			}
			if (item._maxAmount >= 0)
			{
				if (amount < 1)
				{
					return true;
				}
				if (!Application.isPlaying || fromCraftingCog || Mathf.Approximately(item._weight, 0f) || item._weight + LocalPlayer.Stats.CarriedWeight.CurrentWeight <= 1f)
				{
					InventoryItem inventoryItem;
					if (!this._possessedItemCache.ContainsKey(itemId))
					{
						inventoryItem = new InventoryItem
						{
							_itemId = itemId,
							_amount = 0,
							_maxAmount = ((item._maxAmount != 0) ? item._maxAmount : int.MaxValue)
						};
						this._possessedItems.Add(inventoryItem);
						this._possessedItemCache[itemId] = inventoryItem;
					}
					else
					{
						inventoryItem = this._possessedItemCache[itemId];
					}
					if (Application.isPlaying)
					{
						int num = inventoryItem.Add(amount, this.HasInSlot(item._equipmentSlot, itemId));
						if (num < amount)
						{
							if (!fromCraftingCog)
							{
								this.RefreshCurrentWeight();
								Scene.HudGui.ToggleGotItemHud(itemId, amount - num);
							}
							ItemUtils.ApplyEffectsToStats(item._ownedStatEffect, true, amount - num);
						}
						if (num > 0)
						{
							Scene.HudGui.ToggleFullCapacityHud(itemId);
							if (num == amount && !fromCraftingCog)
							{
								return false;
							}
						}
						if (item.MatchType(Item.Types.Special) && this.SpecialItemsControlers.ContainsKey(itemId))
						{
							this.SpecialItemsControlers[itemId].PickedUpSpecialItem(itemId);
						}
						if (preventAutoEquip || LocalPlayer.AnimControl.swimming || !item.MatchType(Item.Types.Equipment) || (!(this._equipmentSlots[(int)item._equipmentSlot] == null) && !(this._equipmentSlots[0] == this._noEquipedItem)) || !this.Equip(itemId, false))
						{
							EventRegistry.Player.Publish(TfEvent.AddedItem, itemId);
						}
						this.ToggleInventoryItemView(itemId, false, properties);
						if (this.SkipNextAddItemWoosh)
						{
							this.SkipNextAddItemWoosh = false;
						}
						else
						{
							LocalPlayer.Sfx.PlayItemCustomSfx(item, (!Grabber.FocusedItem) ? (LocalPlayer.Transform.position + LocalPlayer.MainCamTr.forward) : Grabber.FocusedItem.transform.position, true);
						}
					}
					else
					{
						inventoryItem.Add(amount, false);
					}
					return true;
				}
				Scene.HudGui.ToggleFullWeightHud(itemId);
				return false;
			}
			else
			{
				if (!item.MatchType(Item.Types.Equipment) || LocalPlayer.AnimControl.swimming)
				{
					return false;
				}
				bool flag = this._equipmentSlots[(int)item._equipmentSlot] == null || this._equipmentSlots[0] == this._noEquipedItem || this._equipmentSlots[(int)item._equipmentSlot]._itemId != itemId;
				if (flag && !this.Logs.HasLogs && this.Equip(itemId, true))
				{
					EventRegistry.Player.Publish(TfEvent.AddedItem, itemId);
					return true;
				}
				return false;
			}
		}

		
		public void RefreshCurrentWeight()
		{
		}

		
		public bool RemoveItem(int itemId, int amount = 1, bool allowAmountOverflow = false, bool shouldEquipPrevious = true)
		{
			if (this.ItemFilter != null)
			{
				return this.ItemFilter.RemoveItem(itemId, amount, allowAmountOverflow);
			}
			return this.RemoveItemNF(itemId, amount, allowAmountOverflow, shouldEquipPrevious);
		}

		
		public bool RemoveItemNF(int itemId, int amount = 1, bool allowAmountOverflow = false, bool shouldEquipPrevious = true)
		{
			InventoryItem inventoryItem;
			if (this.Logs != null && itemId == this.Logs._logItemId)
			{
				if (this.Logs.PutDown(false, false, true, null))
				{
					EventRegistry.Player.Publish(TfEvent.RemovedItem, itemId);
					return true;
				}
			}
			else if (this._possessedItemCache.TryGetValue(itemId, out inventoryItem) && inventoryItem._amount > 0)
			{
				if (inventoryItem.Remove(amount))
				{
					if (Application.isPlaying)
					{
						this.RefreshCurrentWeight();
						this.ToggleInventoryItemView(itemId, false, null);
						ItemUtils.ApplyEffectsToStats(ItemDatabase.ItemById(itemId)._ownedStatEffect, false, amount);
					}
					EventRegistry.Player.Publish(TfEvent.RemovedItem, itemId);
					return true;
				}
				if (allowAmountOverflow)
				{
					if (this.HasInSlot(Item.EquipmentSlot.RightHand, itemId))
					{
						ItemUtils.ApplyEffectsToStats(ItemDatabase.ItemById(itemId)._ownedStatEffect, false, 1);
						this.UnequipItemAtSlot(Item.EquipmentSlot.RightHand, false, false, shouldEquipPrevious);
						EventRegistry.Player.Publish(TfEvent.RemovedItem, itemId);
					}
					if (inventoryItem._maxAmountBonus == 0)
					{
						this._possessedItems.Remove(inventoryItem);
						this._possessedItemCache.Remove(itemId);
					}
					this.ToggleInventoryItemView(itemId, false, null);
					this.RefreshCurrentWeight();
				}
			}
			else
			{
				if (amount == 1 && this.HasInSlot(Item.EquipmentSlot.RightHand, itemId))
				{
					this.UnequipItemAtSlot(Item.EquipmentSlot.RightHand, false, false, shouldEquipPrevious);
					EventRegistry.Player.Publish(TfEvent.RemovedItem, itemId);
					this.RefreshCurrentWeight();
					return true;
				}
				if (amount == 1 && this.HasInSlot(Item.EquipmentSlot.LeftHand, itemId))
				{
					this.UnequipItemAtSlot(Item.EquipmentSlot.LeftHand, false, false, shouldEquipPrevious);
					EventRegistry.Player.Publish(TfEvent.RemovedItem, itemId);
					this.RefreshCurrentWeight();
					return true;
				}
				Item item = ItemDatabase.ItemById(itemId);
				if (item._fallbackItemIds.Length > 0)
				{
					for (int i = 0; i < item._fallbackItemIds.Length; i++)
					{
						if (this.RemoveItem(item._fallbackItemIds[i], amount, allowAmountOverflow, true))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		
		public bool HasOwned(int itemId)
		{
			return (this.Logs != null && itemId == this.Logs._logItemId) || this._possessedItemCache.ContainsKey(itemId);
		}

		
		public bool IsSlotEmpty(Item.EquipmentSlot slot)
		{
			return this._equipmentSlots[(int)slot] == null || this._equipmentSlots[(int)slot] == this._noEquipedItem;
		}

		
		public bool IsSlotNextEmpty(Item.EquipmentSlot slot)
		{
			return this._equipmentSlotsNext[(int)slot] == null || this._equipmentSlotsNext[(int)slot] == this._noEquipedItem;
		}

		
		public bool IsSlotAndNextSlotEmpty(Item.EquipmentSlot slot)
		{
			return this.IsSlotEmpty(slot) && this.IsSlotNextEmpty(slot);
		}

		
		public bool IsRightHandEmpty()
		{
			return this.IsSlotEmpty(Item.EquipmentSlot.RightHand);
		}

		
		public bool IsLeftHandEmpty()
		{
			return this.IsSlotEmpty(Item.EquipmentSlot.LeftHand);
		}

		
		public bool HasInSlot(Item.EquipmentSlot slot, int itemId)
		{
			return this._equipmentSlots[(int)slot] != null && this._equipmentSlots[(int)slot]._itemId == itemId;
		}

		
		public bool HasInPreviousSlot(Item.EquipmentSlot slot, int itemId)
		{
			return this._equipmentSlotsPrevious[(int)slot] != null && this._equipmentSlotsPrevious[(int)slot]._itemId == itemId;
		}

		
		public bool HasInNextSlot(Item.EquipmentSlot slot, int itemId)
		{
			return this._equipmentSlotsNext[(int)slot] != null && this._equipmentSlotsNext[(int)slot]._itemId == itemId;
		}

		
		public bool HasInSlotOrNextSlot(Item.EquipmentSlot slot, int itemId)
		{
			return (this._equipmentSlots[(int)slot] != null && this._equipmentSlots[(int)slot]._itemId == itemId) || (this._equipmentSlotsNext[(int)slot] != null && this._equipmentSlotsNext[(int)slot]._itemId == itemId);
		}

		
		public bool Owns(int itemId, bool allowFallback = true)
		{
			if (this.ItemFilter != null)
			{
				return this.ItemFilter.Owns(itemId, allowFallback);
			}
			return this.OwnsNF(itemId, allowFallback);
		}

		
		public bool OwnsNF(int itemId, bool allowFallback = true)
		{
			if (this.Logs != null && itemId == this.Logs._logItemId)
			{
				return this.Logs.HasLogs;
			}
			bool flag = (this._possessedItemCache.ContainsKey(itemId) && this._possessedItemCache[itemId]._amount > 0) || this.HasInSlotOrNextSlot(Item.EquipmentSlot.RightHand, itemId) || this.HasInSlotOrNextSlot(Item.EquipmentSlot.LeftHand, itemId) || this.HasInSlotOrNextSlot(Item.EquipmentSlot.Chest, itemId) || this.HasInSlotOrNextSlot(Item.EquipmentSlot.Feet, itemId) || this.HasInSlotOrNextSlot(Item.EquipmentSlot.FullBody, itemId);
			if (!flag && allowFallback)
			{
				Item item = ItemDatabase.ItemById(itemId);
				if (item._fallbackItemIds.Length > 0)
				{
					for (int i = 0; i < item._fallbackItemIds.Length; i++)
					{
						if (this.Owns(item._fallbackItemIds[i], true))
						{
							return true;
						}
					}
				}
			}
			return flag;
		}

		
		public int OwnsWhich(int itemId, bool allowFallback = true)
		{
			if (this.Logs != null && itemId == this.Logs._logItemId)
			{
				return itemId;
			}
			bool flag = (this._possessedItemCache.ContainsKey(itemId) && this._possessedItemCache[itemId]._amount > 0) || this.HasInSlot(Item.EquipmentSlot.RightHand, itemId) || this.HasInSlot(Item.EquipmentSlot.LeftHand, itemId) || this.HasInSlot(Item.EquipmentSlot.Chest, itemId) || this.HasInSlot(Item.EquipmentSlot.Feet, itemId) || this.HasInSlot(Item.EquipmentSlot.FullBody, itemId);
			if (!flag && allowFallback)
			{
				Item item = ItemDatabase.ItemById(itemId);
				if (item._fallbackItemIds.Length > 0)
				{
					for (int i = 0; i < item._fallbackItemIds.Length; i++)
					{
						int num = this.OwnsWhich(item._fallbackItemIds[i], true);
						if (num > -1)
						{
							return num;
						}
					}
				}
			}
			return (!flag) ? -1 : itemId;
		}

		
		public int AmountOf(int itemId, bool allowFallback = true)
		{
			if (this.ItemFilter != null)
			{
				return this.ItemFilter.AmountOf(itemId, allowFallback);
			}
			return this.AmountOfNF(itemId, allowFallback);
		}

		
		public int AmountOfNF(int itemId, bool allowFallback = true)
		{
			if (this.Logs != null && itemId == this.Logs._logItemId)
			{
				return this.Logs.Amount;
			}
			int num = 0;
			if (this._possessedItemCache.ContainsKey(itemId))
			{
				num = this._possessedItemCache[itemId]._amount;
			}
			if (this.HasInSlot(Item.EquipmentSlot.RightHand, itemId) || this.HasInSlot(Item.EquipmentSlot.LeftHand, itemId) || this.HasInSlot(Item.EquipmentSlot.Chest, itemId) || this.HasInSlot(Item.EquipmentSlot.Feet, itemId) || this.HasInSlot(Item.EquipmentSlot.FullBody, itemId))
			{
				num++;
			}
			if (num == 0 && allowFallback)
			{
				Item item = ItemDatabase.ItemById(itemId);
				if (item._fallbackItemIds.Length > 0)
				{
					for (int i = 0; i < item._fallbackItemIds.Length; i++)
					{
						num += this.AmountOf(item._fallbackItemIds[i], false);
					}
				}
			}
			return num;
		}

		
		public bool HasRoomFor(int itemId, int amount = 1)
		{
			int num = this.AmountOf(itemId, false);
			int maxAmountOf = this.GetMaxAmountOf(itemId);
			return num + amount <= maxAmountOf;
		}

		
		public void AddMaxAmountBonus(int itemId, int amount)
		{
			if (this.Logs != null && itemId != this.Logs._logItemId)
			{
				InventoryItem inventoryItem;
				if (!this._possessedItemCache.ContainsKey(itemId))
				{
					Item item = ItemDatabase.ItemById(itemId);
					inventoryItem = new InventoryItem
					{
						_itemId = itemId,
						_amount = 0,
						_maxAmount = ((item._maxAmount != 0) ? item._maxAmount : int.MaxValue)
					};
					this._possessedItems.Add(inventoryItem);
					this._possessedItemCache[itemId] = inventoryItem;
				}
				else
				{
					inventoryItem = this._possessedItemCache[itemId];
				}
				inventoryItem._maxAmountBonus += amount;
			}
		}

		
		public void SetMaxAmountBonus(int itemId, int amount)
		{
			if (this.Logs != null && itemId != this.Logs._logItemId)
			{
				InventoryItem inventoryItem;
				if (!this._possessedItemCache.ContainsKey(itemId))
				{
					Item item = ItemDatabase.ItemById(itemId);
					inventoryItem = new InventoryItem
					{
						_itemId = itemId,
						_amount = 0,
						_maxAmount = ((item._maxAmount != 0) ? item._maxAmount : int.MaxValue)
					};
					this._possessedItems.Add(inventoryItem);
					this._possessedItemCache[itemId] = inventoryItem;
				}
				else
				{
					inventoryItem = this._possessedItemCache[itemId];
				}
				inventoryItem._maxAmountBonus = amount;
			}
		}

		
		public int GetMaxAmountOf(int itemId)
		{
			if (this._possessedItemCache.ContainsKey(itemId))
			{
				return this._possessedItemCache[itemId].MaxAmount;
			}
			int maxAmount = ItemDatabase.ItemById(itemId)._maxAmount;
			return (maxAmount != 0) ? maxAmount : int.MaxValue;
		}

		
		public void TurnOffLastLight()
		{
			if (this.HasInSlot(Item.EquipmentSlot.LeftHand, this.LastLight._itemId))
			{
				this.StashLeftHand();
			}
		}

		
		public void TurnOnLastLight()
		{
			if (!this.HasInSlot(Item.EquipmentSlot.LeftHand, this.LastLight._itemId))
			{
				if (!this.Equip(this.LastLight._itemId, false))
				{
					this.LastLight = this.DefaultLight;
					this.Equip(this.LastLight._itemId, false);
				}
				LocalPlayer.Tuts.HideLighter();
			}
		}

		
		public void TurnOffLastUtility(Item.EquipmentSlot slot = Item.EquipmentSlot.LeftHand)
		{
			if (this.HasInSlot(slot, this.LastUtility._itemId))
			{
				this.StashLeftHand();
			}
		}

		
		public void TurnOnLastUtility(Item.EquipmentSlot slot = Item.EquipmentSlot.LeftHand)
		{
			if (!this.HasInSlot(slot, this.LastUtility._itemId))
			{
				this.Equip(this.LastUtility._itemId, false);
			}
		}

		
		public void BloodyWeapon()
		{
			foreach (InventoryItemView inventoryItemView in this._equipmentSlots)
			{
				if (inventoryItemView != null && inventoryItemView._held != null)
				{
					inventoryItemView._held.SendMessage("GotBloody", SendMessageOptions.DontRequireReceiver);
				}
			}
		}

		
		public void CleanWeapon()
		{
			foreach (InventoryItemView inventoryItemView in this._equipmentSlots)
			{
				if (inventoryItemView != null && inventoryItemView._held != null)
				{
					inventoryItemView._held.SendMessage("GotClean", SendMessageOptions.DontRequireReceiver);
				}
			}
			foreach (Bloodify bloodify in LocalPlayer.HeldItemsData._weaponBlood)
			{
				if (bloodify)
				{
					bloodify.GotClean();
				}
			}
			foreach (BurnableCloth burnableCloth in LocalPlayer.HeldItemsData._weaponFire)
			{
				if (burnableCloth)
				{
					burnableCloth.GotClean();
				}
			}
			foreach (InventoryItemView inventoryItemView2 in this._equipmentSlots)
			{
				if (inventoryItemView2 != null && inventoryItemView2._held != null)
				{
					inventoryItemView2._held.SendMessage("GotClean", SendMessageOptions.DontRequireReceiver);
				}
			}
		}

		
		public void GotLeaf()
		{
			this.AddItem(this._leafItemId, UnityEngine.Random.Range(1, 3), false, false, null);
		}

		
		public void GotSap(int? amount = null)
		{
			int num = (amount == null) ? UnityEngine.Random.Range(-1, 3) : amount.Value;
			if (num > 0)
			{
				this.AddItem(this._sapItemId, num, false, false, null);
			}
		}

		
		public void HighlightItemGroup(InventoryItemView view, bool onoff)
		{
			this.HighlightItemGroup(view._itemId, view.Properties, onoff);
		}

		
		public void HighlightItemGroup(int itemId, ItemProperties properties, bool onoff)
		{
			if (this._inventoryItemViewsCache.ContainsKey(itemId))
			{
				List<InventoryItemView> list = this._inventoryItemViewsCache[itemId];
				int count = list.Count;
				for (int i = count - 1; i >= 0; i--)
				{
					InventoryItemView inventoryItemView = list[i];
					if (inventoryItemView && inventoryItemView.gameObject.activeSelf)
					{
						if (onoff)
						{
							if (inventoryItemView.Properties.Match(properties))
							{
								inventoryItemView.Highlight(true);
							}
						}
						else
						{
							inventoryItemView.Highlight(false);
						}
					}
				}
			}
		}

		
		public void SheenItem(int itemId, ItemProperties properties, bool onoff)
		{
			if (this._inventoryItemViewsCache.ContainsKey(itemId))
			{
				List<InventoryItemView> list = this._inventoryItemViewsCache[itemId];
				int count = list.Count;
				for (int i = count - 1; i >= 0; i--)
				{
					InventoryItemView inventoryItemView = list[i];
					if (inventoryItemView && inventoryItemView.Properties.Match(properties))
					{
						inventoryItemView.Sheen(onoff);
					}
				}
			}
		}

		
		public InventoryItemView GetLastActiveView(int itemId)
		{
			int num = this.AmountOf(itemId, false);
			if (num > 0)
			{
				return this._inventoryItemViewsCache[itemId][Mathf.Min(num, this._inventoryItemViewsCache[itemId].Count) - 1];
			}
			return null;
		}

		
		public void BubbleUpInventoryView(InventoryItemView view)
		{
			int num = Mathf.Max(this.AmountOf(view._itemId, false), 1);
			List<InventoryItemView> list = this._inventoryItemViewsCache[view._itemId];
			if (num <= list.Count)
			{
				list.Remove(view);
				list.Insert(num - 1, view);
			}
		}

		
		public void BubbleDownInventoryView(InventoryItemView view)
		{
			List<InventoryItemView> list = this._inventoryItemViewsCache[view._itemId];
			list.Remove(view);
			list.Insert(0, view);
		}

		
		public void ShuffleInventoryView(InventoryItemView view)
		{
			List<InventoryItemView> list = this._inventoryItemViewsCache[view._itemId];
			int num = Mathf.Min(Mathf.Max(this.AmountOf(view._itemId, false), 1), list.Count) - 1;
			int num2 = list.IndexOf(view);
			if (num2 == num)
			{
				list.Remove(view);
				if (num > 0)
				{
					list.Insert(0, view);
				}
				else
				{
					list.Insert(list.Count, view);
				}
				this.ToggleInventoryItemView(view._itemId, false, null);
			}
		}

		
		public bool ShuffleRemoveRightHandItem()
		{
			if (!this.IsRightHandEmpty())
			{
				if (LocalPlayer.Inventory.AmountOf(this.RightHand._itemId, false) == 1)
				{
					LocalPlayer.Inventory.UnequipItemAtSlot(Item.EquipmentSlot.RightHand, false, false, true);
				}
				else
				{
					LocalPlayer.Inventory.MemorizeItem(Item.EquipmentSlot.RightHand);
					LocalPlayer.Inventory.ShuffleInventoryView(LocalPlayer.Inventory.RightHand);
					LocalPlayer.Inventory.UnequipItemAtSlot(Item.EquipmentSlot.RightHand, false, false, true);
				}
				return true;
			}
			return false;
		}

		
		public void SortInventoryViewsByBonus(InventoryItemView view, WeaponStatUpgrade.Types activeBonus, bool setTargetViewFirst)
		{
			List<InventoryItemView> list = this._inventoryItemViewsCache[view._itemId];
			int num = Mathf.Max(this.AmountOf(view._itemId, false), 1);
			int num2 = 0;
			int count = list.Count;
			for (int i = count - 1; i >= 0; i--)
			{
				InventoryItemView inventoryItemView = list[i + num2];
				if (inventoryItemView && inventoryItemView.ActiveBonus != activeBonus && inventoryItemView.gameObject.activeSelf)
				{
					list.RemoveAt(i + num2++);
					list.Insert(0, inventoryItemView);
				}
			}
			if (setTargetViewFirst && num <= list.Count)
			{
				list.Remove(view);
				list.Insert(num - 1, view);
			}
		}

		
		public bool OwnsItemWithBonus(int itemId, WeaponStatUpgrade.Types bonus)
		{
			List<InventoryItemView> list = this._inventoryItemViewsCache[itemId];
			for (int i = list.Count - 1; i >= 0; i--)
			{
				InventoryItemView inventoryItemView = list[i];
				if (inventoryItemView && inventoryItemView.gameObject.activeSelf && inventoryItemView.ActiveBonus == bonus)
				{
					return true;
				}
			}
			return false;
		}

		
		public int AmountOfItemWithBonus(int itemId, WeaponStatUpgrade.Types bonus)
		{
			int num = 0;
			List<InventoryItemView> list = this._inventoryItemViewsCache[itemId];
			if (list[0].ItemCache._maxAmount > 0 && list[0].ItemCache._maxAmount <= list.Count)
			{
				for (int i = list.Count - 1; i >= 0; i--)
				{
					InventoryItemView inventoryItemView = list[i];
					if (inventoryItemView && inventoryItemView.gameObject.activeSelf && inventoryItemView.ActiveBonus == bonus)
					{
						num++;
					}
				}
			}
			else
			{
				num = this.AmountOf(itemId, false);
			}
			return num;
		}

		
		public void AddUpgradeToCounter(int itemId, int upgradeItemId, int amount)
		{
			if (!this.ItemsUpgradeCounters.ContainsKey(itemId))
			{
				this.ItemsUpgradeCounters[itemId] = new PlayerInventory.UpgradeCounterDict();
			}
			int num;
			this.ItemsUpgradeCounters[itemId].TryGetValue(upgradeItemId, out num);
			this.ItemsUpgradeCounters[itemId][upgradeItemId] = num + amount;
		}

		
		public int GetAmountOfUpgrades(int itemId)
		{
			if (this.ItemsUpgradeCounters.ContainsKey(itemId))
			{
				return this.ItemsUpgradeCounters[itemId].Values.Sum();
			}
			return 0;
		}

		
		public int GetAmountOfUpgrades(int itemId, int upgradeItemId)
		{
			if (this.ItemsUpgradeCounters.ContainsKey(itemId) && this.ItemsUpgradeCounters[itemId].ContainsKey(upgradeItemId))
			{
				return this.ItemsUpgradeCounters[itemId][upgradeItemId];
			}
			return 0;
		}

		
		public void GatherWater(bool clean)
		{
			InventoryItemView inventoryItemView = this._equipmentSlots[0];
			inventoryItemView.ActiveBonus = ((!clean) ? WeaponStatUpgrade.Types.DirtyWater : WeaponStatUpgrade.Types.CleanWater);
			inventoryItemView.Properties.ActiveBonusValue = 2f;
		}

		
		public void SetQuickSelectItemIds(int[] itemIds)
		{
			this._quickSelectItemIds = itemIds;
			foreach (QuickSelectViews quickSelectViews2 in LocalPlayer.QuickSelectViews)
			{
				quickSelectViews2.ShowLocalPlayerViews();
			}
		}

		
		private IEnumerator OnSerializing()
		{
			this._possessedItemsCount = this._possessedItems.Count;
			PlayerInventory.SerializableItemUpgradeCounters[] upgradeCounters;
			this._upgradeCounters = this.ItemsUpgradeCounters.Select(delegate(KeyValuePair<int, PlayerInventory.UpgradeCounterDict> itemUpgradeCounters)
			{
				PlayerInventory.SerializableItemUpgradeCounters serializableItemUpgradeCounters2 = new PlayerInventory.SerializableItemUpgradeCounters();
				serializableItemUpgradeCounters2._itemId = itemUpgradeCounters.Key;
				serializableItemUpgradeCounters2._counters = (from upgradeCounters in itemUpgradeCounters.Value
				select new PlayerInventory.SerializableUpgradeCounter
				{
					_upgradeItemId = upgradeCounters.Key,
					_amount = upgradeCounters.Value
				}).ToArray<PlayerInventory.SerializableUpgradeCounter>();
				return serializableItemUpgradeCounters2;
			}).ToArray<PlayerInventory.SerializableItemUpgradeCounters>();
			foreach (PlayerInventory.SerializableItemUpgradeCounters serializableItemUpgradeCounters in this._upgradeCounters)
			{
				serializableItemUpgradeCounters._count = serializableItemUpgradeCounters._counters.Length;
			}
			this._upgradeCountersCount = this._upgradeCounters.Length;
			this._equipmentSlotsIds = (from es in this._equipmentSlots
			select (!(es != null)) ? 0 : es._itemId).ToArray<int>();
			foreach (InventoryItemView inventoryItemView in this._itemViews)
			{
				inventoryItemView.OnSerializing();
			}
			this._inventoryGO.transform.parent = base.transform;
			yield return null;
			this._inventoryGO.transform.parent = null;
			yield break;
		}

		
		private void ToggleInventoryItemView(int itemId, bool forceInit = false, ItemProperties properties = null)
		{
			if (this._inventoryItemViewsCache.ContainsKey(itemId))
			{
				int num = this.AmountOf(itemId, false);
				List<InventoryItemView> list = this._inventoryItemViewsCache[itemId];
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i])
					{
						InventoryItemView inventoryItemView = list[i];
						GameObject gameObject = inventoryItemView.gameObject;
						bool flag = i < num;
						if (gameObject.activeSelf != flag || forceInit)
						{
							gameObject.SetActive(flag);
							if (flag)
							{
								if (properties != ItemProperties.Any)
								{
									list[i].Properties.Copy(properties);
								}
								list[i].OnItemAdded();
							}
							else
							{
								list[i].OnItemRemoved();
							}
						}
						if (inventoryItemView.ItemCache.MatchType(Item.Types.Extension))
						{
							if (inventoryItemView._held && inventoryItemView._held.activeSelf != flag)
							{
								inventoryItemView._held.SetActive(flag);
							}
							if (this._craftingCog.ItemExtensionViewsCache.TryGetValue(itemId, out inventoryItemView) && inventoryItemView.gameObject.activeSelf != flag)
							{
								inventoryItemView.gameObject.SetActive(flag);
							}
						}
					}
				}
			}
		}

		
		private void ThrowProjectile()
		{
			this._isThrowing = false;
			InventoryItemView inventoryItemView = this._equipmentSlots[0];
			if (inventoryItemView)
			{
				Item itemCache = inventoryItemView.ItemCache;
				bool flag = itemCache._maxAmount < 0;
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>((!this.UseAltWorldPrefab) ? inventoryItemView._worldPrefab : inventoryItemView._altWorldPrefab, inventoryItemView._held.transform.position, inventoryItemView._held.transform.rotation);
				Rigidbody component = gameObject.GetComponent<Rigidbody>();
				Collider component2 = gameObject.GetComponent<Collider>();
				if (BoltNetwork.isRunning)
				{
					BoltEntity component3 = gameObject.GetComponent<BoltEntity>();
					if (component3)
					{
						BoltNetwork.Attach(gameObject);
					}
				}
				if (inventoryItemView.ActiveBonus == WeaponStatUpgrade.Types.StickyProjectile)
				{
					if (component2)
					{
						gameObject.AddComponent<StickyBomb>();
						SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
						sphereCollider.isTrigger = true;
						sphereCollider.radius = 0.8f;
					}
					else
					{
						Collider componentInChildren = gameObject.GetComponentInChildren<Collider>();
						if (componentInChildren)
						{
							componentInChildren.gameObject.AddComponent<StickyBomb>();
						}
					}
				}
				component.AddForce((float)itemCache._projectileThrowForceRange * (0.016666f / Time.fixedDeltaTime) * LocalPlayer.MainCamTr.forward);
				inventoryItemView._held.SendMessage("OnProjectileThrown", gameObject, SendMessageOptions.DontRequireReceiver);
				inventoryItemView.ActiveBonus = (WeaponStatUpgrade.Types)(-1);
				if (itemCache._rangedStyle == Item.RangedStyle.Bell)
				{
					component.AddTorque((float)itemCache._projectileThrowTorqueRange * base.transform.forward);
				}
				else if (itemCache._rangedStyle == Item.RangedStyle.Forward)
				{
					component.AddTorque((float)itemCache._projectileThrowTorqueRange * LocalPlayer.MainCamTr.forward);
				}
				if (base.transform.GetComponent<Collider>().enabled && component2 && component2.enabled)
				{
					Physics.IgnoreCollision(base.transform.GetComponent<Collider>(), component2);
				}
				if (!flag)
				{
					this.MemorizeOverrideItem(Item.EquipmentSlot.RightHand);
				}
				bool equipPrevious = true;
				if (LocalPlayer.FpCharacter.Sitting || LocalPlayer.AnimControl.onRope || LocalPlayer.FpCharacter.SailingRaft)
				{
					equipPrevious = false;
				}
				this.UnequipItemAtSlot(itemCache._equipmentSlot, false, false, equipPrevious);
				LocalPlayer.Sfx.PlayThrow();
			}
		}

		
		private void FireRangedWeapon()
		{
			InventoryItemView inventoryItemView = this._equipmentSlots[0];
			Item itemCache = inventoryItemView.ItemCache;
			bool flag = itemCache._maxAmount < 0;
			bool flag2 = false;
			if (flag || this.RemoveItem(itemCache._ammoItemId, 1, false, true))
			{
				InventoryItemView inventoryItemView2 = this._inventoryItemViewsCache[itemCache._ammoItemId][0];
				Item itemCache2 = inventoryItemView2.ItemCache;
				FakeParent component = inventoryItemView2._held.GetComponent<FakeParent>();
				if (this.UseAltWorldPrefab)
				{
					Debug.Log(string.Concat(new object[]
					{
						"Firing ",
						itemCache._name,
						" with '",
						inventoryItemView.ActiveBonus,
						"' ammo (alt=",
						this.UseAltWorldPrefab,
						")"
					}));
				}
				GameObject gameObject;
				if (component && !component.gameObject.activeSelf)
				{
					gameObject = UnityEngine.Object.Instantiate<GameObject>(itemCache2._ammoPrefabs.GetPrefabForBonus(inventoryItemView.ActiveBonus, true).gameObject, component.RealPosition, component.RealRotation);
				}
				else
				{
					gameObject = UnityEngine.Object.Instantiate<GameObject>(itemCache2._ammoPrefabs.GetPrefabForBonus(inventoryItemView.ActiveBonus, true).gameObject, inventoryItemView2._held.transform.position, inventoryItemView2._held.transform.rotation);
				}
				if (gameObject.GetComponent<Rigidbody>())
				{
					if (itemCache.MatchRangedStyle(Item.RangedStyle.Shoot))
					{
						gameObject.GetComponent<Rigidbody>().AddForce(gameObject.transform.TransformDirection(Vector3.forward * (0.016666f / Time.fixedDeltaTime) * (float)itemCache._projectileThrowForceRange), ForceMode.VelocityChange);
					}
					else
					{
						float num = Time.time - this._weaponChargeStartTime;
						gameObject.GetComponent<Rigidbody>().AddForce(inventoryItemView2._held.transform.up * Mathf.Clamp01(num / itemCache._projectileMaxChargeDuration) * (0.016666f / Time.fixedDeltaTime) * (float)itemCache._projectileThrowForceRange);
						if (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, LocalPlayer.AnimControl._bowId))
						{
							gameObject.SendMessage("setCraftedBowDamage", SendMessageOptions.DontRequireReceiver);
						}
					}
					inventoryItemView._held.SendMessage("OnAmmoFired", gameObject, SendMessageOptions.DontRequireReceiver);
				}
				if (itemCache._attackReleaseSFX != Item.SFXCommands.None)
				{
					LocalPlayer.Sfx.SendMessage(itemCache._attackReleaseSFX.ToString(), SendMessageOptions.DontRequireReceiver);
				}
				Mood.HitRumble();
			}
			else
			{
				flag2 = true;
				if (itemCache._dryFireSFX != Item.SFXCommands.None)
				{
					LocalPlayer.Sfx.SendMessage(itemCache._dryFireSFX.ToString(), SendMessageOptions.DontRequireReceiver);
				}
			}
			if (flag)
			{
				this.UnequipItemAtSlot(itemCache._equipmentSlot, false, false, flag);
			}
			else
			{
				this.ToggleAmmo(inventoryItemView, true);
			}
			this._weaponChargeStartTime = 0f;
			this.SetReloadDelay((!flag2) ? itemCache._reloadDuration : itemCache._dryFireReloadDuration);
			this._isThrowing = false;
		}

		
		public float CalculateRemainingReloadDelay()
		{
			InventoryItemView inventoryItemView = this._equipmentSlots[0];
			if (inventoryItemView && inventoryItemView.ItemCache != null)
			{
				Item itemCache = inventoryItemView.ItemCache;
				return (this._reloadingEndTime - Time.time) / itemCache._reloadDuration;
			}
			return 0f;
		}

		
		public void ForceReloadDelay()
		{
			InventoryItemView inventoryItemView = this._equipmentSlots[0];
			if (inventoryItemView && inventoryItemView.ItemCache != null)
			{
				Item itemCache = inventoryItemView.ItemCache;
				bool flag = false;
				if (itemCache._ammoItemId < 1)
				{
					flag = true;
				}
				this.SetReloadDelay((!flag) ? itemCache._reloadDuration : itemCache._dryFireReloadDuration);
			}
		}

		
		public void SetReloadDelay(float delay)
		{
			this._reloadingEndTime = Time.time + delay;
		}

		
		public void CancelReloadDelay()
		{
			this._reloadingEndTime = 0f;
		}

		
		public void UnequipItemAtSlot(Item.EquipmentSlot slot, bool drop, bool stash, bool equipPrevious)
		{
			if (!this.IsSlotEmpty(slot))
			{
				InventoryItemView inventoryItemView = this._equipmentSlots[(int)slot];
				Item itemCache = inventoryItemView.ItemCache;
				this.UnlockEquipmentSlot(slot);
				bool useAltWorldPrefab = this.UseAltWorldPrefab;
				bool flag = inventoryItemView.ItemCache.MatchType(Item.Types.Special);
				if (drop && flag)
				{
					if (this.SpecialItemsControlers[inventoryItemView._itemId].ToggleSpecial(false))
					{
						inventoryItemView._held.SetActive(false);
					}
				}
				else
				{
					inventoryItemView._held.SetActive(false);
					if (inventoryItemView.ItemCache.MatchType(Item.Types.Special))
					{
						this.SpecialItemsControlers[inventoryItemView._itemId].ToggleSpecial(false);
					}
				}
				this._itemAnimHash.ApplyAnimVars(itemCache, false);
				this._equipmentSlots[(int)slot] = ((inventoryItemView._itemId == this._defaultWeaponItemId) ? this._noEquipedItem : null);
				inventoryItemView.ApplyEquipmentEffect(false);
				if ((drop && itemCache.MatchType(Item.Types.Droppable) && !useAltWorldPrefab) || (stash && inventoryItemView.IsHeldOnly))
				{
					FakeParent component = inventoryItemView._held.GetComponent<FakeParent>();
					Vector3 position;
					Quaternion rotation;
					if (component && !inventoryItemView._held.transform.parent)
					{
						position = component.RealPosition;
						rotation = component.RealRotation;
					}
					else
					{
						position = inventoryItemView._held.transform.position;
						rotation = inventoryItemView._held.transform.rotation;
					}
					GameObject gameObject;
					if (!BoltNetwork.isRunning || !itemCache._pickupPrefabMP)
					{
						gameObject = UnityEngine.Object.Instantiate<GameObject>((!useAltWorldPrefab && inventoryItemView._worldPrefab) ? inventoryItemView._worldPrefab : ((!itemCache._pickupPrefab) ? inventoryItemView._altWorldPrefab : itemCache._pickupPrefab.gameObject), position, rotation);
					}
					else
					{
						gameObject = BoltNetwork.Instantiate((!useAltWorldPrefab && inventoryItemView._worldPrefab && inventoryItemView._worldPrefab.GetComponent<BoltEntity>()) ? inventoryItemView._worldPrefab : itemCache._pickupPrefabMP.gameObject, position, rotation);
						if (!gameObject)
						{
							gameObject = UnityEngine.Object.Instantiate<GameObject>((!useAltWorldPrefab && inventoryItemView._worldPrefab && inventoryItemView._worldPrefab.GetComponent<BoltEntity>()) ? inventoryItemView._worldPrefab : itemCache._pickupPrefabMP.gameObject, position, rotation);
						}
					}
					inventoryItemView.OnItemDropped(gameObject);
				}
				else if ((stash && itemCache._maxAmount >= 0) || (drop && (!itemCache.MatchType(Item.Types.Droppable) || useAltWorldPrefab)))
				{
					this.AddItem(inventoryItemView._itemId, 1, true, true, null);
					if (inventoryItemView.ItemCache._stashSFX != Item.SFXCommands.None)
					{
						LocalPlayer.Sfx.SendMessage(inventoryItemView.ItemCache._stashSFX.ToString(), SendMessageOptions.DontRequireReceiver);
					}
				}
				if (inventoryItemView.ItemCache._maxAmount >= 0)
				{
					this.ToggleAmmo(inventoryItemView, false);
					this.ToggleInventoryItemView(inventoryItemView._itemId, false, null);
				}
				if (equipPrevious && slot == Item.EquipmentSlot.RightHand)
				{
					this.EquipPreviousWeaponDelayed();
				}
			}
		}

		
		public bool FakeDrop(int itemId, GameObject preSpawned = null)
		{
			if (itemId == this.Logs._logItemId)
			{
				return this.Logs.PutDown(true, true, true, preSpawned);
			}
			if (this._inventoryItemViewsCache.ContainsKey(itemId))
			{
				InventoryItemView itemView = this._inventoryItemViewsCache[itemId][0];
				return this.FakeDrop(itemView, false, preSpawned);
			}
			return false;
		}

		
		public bool FakeDrop(InventoryItemView itemView, bool sendOnDropEvent, GameObject preSpawned = null)
		{
			LocalPlayer.Sfx.PlayItemCustomSfx(itemView.ItemCache, (!preSpawned) ? (LocalPlayer.Transform.position + LocalPlayer.MainCamTr.forward) : preSpawned.transform.position, true);
			if (itemView.ItemCache._pickupPrefab || itemView._worldPrefab)
			{
				GameObject gameObject = (!itemView._held) ? this._inventoryItemViewsCache[this._defaultWeaponItemId][0]._held : itemView._held;
				FakeParent component = gameObject.GetComponent<FakeParent>();
				Vector3 position;
				if (component && !gameObject.transform.parent)
				{
					position = component.RealPosition + LocalPlayer.Transform.forward * 1.2f;
				}
				else
				{
					position = gameObject.transform.position + LocalPlayer.Transform.forward * 1.2f;
				}
				if (BoltNetwork.isRunning)
				{
					BoltEntity boltEntity = (!preSpawned) ? null : preSpawned.GetComponent<BoltEntity>();
					BoltEntity component2 = ((!itemView.ItemCache._pickupPrefabMP) ? ((!itemView.ItemCache._pickupPrefab) ? itemView._worldPrefab : itemView.ItemCache._pickupPrefab.gameObject) : itemView.ItemCache._pickupPrefabMP.gameObject).GetComponent<BoltEntity>();
					if (component2 && (!boltEntity || !boltEntity.isAttached || !boltEntity.isOwner))
					{
						DropItem dropItem = DropItem.Create(GlobalTargets.OnlyServer);
						dropItem.PrefabId = component2.prefabId;
						dropItem.Position = position;
						dropItem.Rotation = Quaternion.identity;
						dropItem.PreSpawned = boltEntity;
						dropItem.Send();
					}
					else if (preSpawned)
					{
						preSpawned.transform.position = position;
						preSpawned.transform.rotation = Quaternion.identity;
					}
					else
					{
						GameObject worldGo = UnityEngine.Object.Instantiate<GameObject>((!itemView.ItemCache._pickupPrefab) ? itemView._worldPrefab : itemView.ItemCache._pickupPrefab.gameObject, position, Quaternion.identity);
						if (sendOnDropEvent)
						{
							itemView.OnItemDropped(worldGo);
						}
					}
				}
				else if (preSpawned)
				{
					preSpawned.transform.position = position;
					preSpawned.transform.rotation = Quaternion.identity;
				}
				else
				{
					GameObject worldGo2 = UnityEngine.Object.Instantiate<GameObject>((!itemView.ItemCache._pickupPrefab) ? itemView._worldPrefab : itemView.ItemCache._pickupPrefab.gameObject, position, Quaternion.identity);
					if (sendOnDropEvent)
					{
						itemView.OnItemDropped(worldGo2);
					}
				}
				return true;
			}
			Debug.LogWarning(ItemDatabase.ItemById(itemView._itemId)._name + " doesn't have a proper pickup prefab reference and cannot be fake-dropped");
			return false;
		}

		
		public void ToggleAmmo(int ammoItemId, bool enable)
		{
			this._inventoryItemViewsCache[ammoItemId][0]._held.SetActive(enable && this.Owns(ammoItemId, true));
		}

		
		public void ToggleAmmo(InventoryItemView itemView, bool enable)
		{
			if (itemView.ItemCache.MatchType(Item.Types.RangedWeapon))
			{
				this._inventoryItemViewsCache[itemView.ItemCache._ammoItemId][0]._held.SetActive(enable && this.Owns(itemView.ItemCache._ammoItemId, true));
			}
		}

		
		public void InitItemCache()
		{
			this._itemDatabase.OnEnable();
			this._possessedItemCache = this._possessedItems.ToDictionary((InventoryItem i) => i._itemId);
			if (this._itemViews != null)
			{
				IEnumerable<IGrouping<int, InventoryItemView>> source = from i in this._itemViews
				where i && i._itemId > 0
				select i into iv
				group iv by iv._itemId;
				Func<IGrouping<int, InventoryItemView>, int> keySelector = (IGrouping<int, InventoryItemView> g) => g.Key;
				if (PlayerInventory.<>f__mg$cache0 == null)
				{
					PlayerInventory.<>f__mg$cache0 = new Func<IGrouping<int, InventoryItemView>, List<InventoryItemView>>(Enumerable.ToList<InventoryItemView>);
				}
				this._inventoryItemViewsCache = source.ToDictionary(keySelector, PlayerInventory.<>f__mg$cache0);
			}
			this.ItemsUpgradeCounters = new PlayerInventory.ItemsUpgradeCountersDict();
			if (Application.isPlaying)
			{
				this._craftingCog.Awake();
				this._craftingCog.GetComponent<UpgradeCog>().Awake();
			}
		}

		
		private void RefreshDropIcon()
		{
			if (Scene.HudGui.IsNull())
			{
				return;
			}
			bool flag = (this.Logs.Amount > 0 || LocalPlayer.AnimControl.carry || (!this.IsRightHandEmpty() && this._equipmentSlots[0].ItemCache._maxAmount < 0)) && !this.DontShowDrop && this.CurrentView == PlayerInventory.PlayerViews.World;
			if (Scene.HudGui.DropButton.activeSelf != flag)
			{
				Scene.HudGui.DropButton.SetActive(flag);
			}
		}

		
		
		public Dictionary<int, List<InventoryItemView>> InventoryItemViewsCache
		{
			get
			{
				return this._inventoryItemViewsCache;
			}
		}

		
		
		
		public Dictionary<int, SpecialItemControlerBase> SpecialItemsControlers { get; set; }

		
		
		public GameObject SpecialItems
		{
			get
			{
				return this._specialItems;
			}
		}

		
		
		public GameObject SpecialActions
		{
			get
			{
				return this._specialActions;
			}
		}

		
		
		public PlayMakerFSM PM
		{
			get
			{
				return this._pm;
			}
		}

		
		
		
		public LighterControler DefaultLight { get; set; }

		
		
		
		public SpecialItemControlerBase LastLight { get; set; }

		
		
		
		public SpecialItemControlerBase LastUtility { get; set; }

		
		
		public ItemAnimatorHashHelper ItemAnimHash
		{
			get
			{
				return this._itemAnimHash;
			}
		}

		
		
		public InventoryItemView[] EquipmentSlots
		{
			get
			{
				return this._equipmentSlots;
			}
		}

		
		
		public InventoryItemView[] EquipmentSlotsPrevious
		{
			get
			{
				return this._equipmentSlotsPrevious;
			}
		}

		
		
		public InventoryItemView LeftHand
		{
			get
			{
				return this._equipmentSlots[1];
			}
		}

		
		
		public InventoryItemView RightHand
		{
			get
			{
				return this._equipmentSlots[0];
			}
		}

		
		
		public InventoryItemView RightHandOrNext
		{
			get
			{
				return (!this.IsSlotNextEmpty(Item.EquipmentSlot.RightHand)) ? this._equipmentSlotsNext[0] : this._equipmentSlots[0];
			}
		}

		
		
		public int[] QuickSelectItemIds
		{
			get
			{
				return this._quickSelectItemIds;
			}
		}

		
		
		
		public LogControler Logs { get; set; }

		
		
		
		public PlayerInventory.ItemsUpgradeCountersDict ItemsUpgradeCounters { get; set; }

		
		
		
		public bool BlockTogglingInventory { get; set; }

		
		
		
		public bool QuickSelectGamepadSwitch { get; set; }

		
		
		
		public bool IsWeaponBurning { get; set; }

		
		
		
		public bool CancelNextChargedAttack { get; set; }

		
		
		
		public bool SkipNextAddItemWoosh { get; set; }

		
		
		
		public bool DontShowDrop { get; set; }

		
		
		
		public string PendingSendMessage { get; set; }

		
		
		public float WeaponChargeStartTime
		{
			get
			{
				return this._weaponChargeStartTime;
			}
		}

		
		
		public bool IsReloading
		{
			get
			{
				return this._reloadingEndTime > Time.time;
			}
		}

		
		
		public bool IsThrowing
		{
			get
			{
				return this._isThrowing;
			}
		}

		
		
		
		public bool UseAltWorldPrefab { get; set; }

		
		
		
		public bool IsOpenningInventory { get; private set; }

		
		
		
		public bool blockRangedAttack { get; set; }

		
		
		
		public IItemStorage CurrentStorage { get; private set; }

		
		
		
		public IInventoryItemFilter ItemFilter { get; set; }

		
		
		
		public PlayerInventory.PlayerViews CurrentView
		{
			get
			{
				return this._currentView;
			}
			set
			{
				if (LocalPlayer.Stats.Dead && value != PlayerInventory.PlayerViews.Pause)
				{
					this._currentView = ((value != PlayerInventory.PlayerViews.WakingUp) ? PlayerInventory.PlayerViews.Death : value);
				}
				else
				{
					this._currentView = value;
				}
				if (BoltNetwork.isRunning && LocalPlayer.Entity && LocalPlayer.Entity.IsAttached())
				{
					if (LocalPlayer.Stats.Dead)
					{
						LocalPlayer.Entity.GetState<IPlayerState>().CurrentView = (int)((value != PlayerInventory.PlayerViews.WakingUp) ? PlayerInventory.PlayerViews.Death : value);
					}
					else
					{
						LocalPlayer.Entity.GetState<IPlayerState>().CurrentView = (int)value;
					}
				}
			}
		}

		
		public ItemDatabase _itemDatabase;

		
		[SerializeThis]
		public List<InventoryItem> _possessedItems;

		
		[SerializeThis]
		public int _possessedItemsCount;

		
		public CraftingCog _craftingCog;

		
		public List<UpgradeViewReceiver> _upgradeViewReceivers;

		
		public InventoryItemView[] _itemViews;

		
		public GameObject _specialActions;

		
		public GameObject _specialItems;

		
		public GameObject _inventoryGO;

		
		[ItemIdPicker]
		public int _leafItemId;

		
		[ItemIdPicker]
		public int _seedItemId;

		
		[ItemIdPicker]
		public int _sapItemId;

		
		[ItemIdPicker(Item.Types.Equipment)]
		public int _defaultWeaponItemId;

		
		private PlayerInventory.PlayerViews _currentView;

		
		[SerializeThis]
		private int[] _equipmentSlotsIds;

		
		private bool[] _equipmentSlotsLocked;

		
		private InventoryItemView[] _equipmentSlots;

		
		private InventoryItemView[] _equipmentSlotsPrevious;

		
		private InventoryItemView[] _equipmentSlotsPreviousOverride;

		
		private InventoryItemView[] _equipmentSlotsNext;

		
		private InventoryItemView _noEquipedItem;

		
		[SerializeThis]
		private PlayerInventory.SerializableItemUpgradeCounters[] _upgradeCounters;

		
		[SerializeThis]
		private int _upgradeCountersCount;

		
		private Dictionary<int, InventoryItem> _possessedItemCache;

		
		private Dictionary<int, List<InventoryItemView>> _inventoryItemViewsCache;

		
		private PlayMakerFSM _pm;

		
		private ItemAnimatorHashHelper _itemAnimHash;

		
		private float _weaponChargeStartTime;

		
		private float _equipPreviousTime;

		
		public float _reloadingEndTime;

		
		private bool _isThrowing;

		
		private EventInstance _pauseSnapshot;

		
		[SerializeThis]
		[ItemIdPicker]
		private int[] _quickSelectItemIds;

		
		private string[] _quickSelectButtons = new string[]
		{
			"ItemSlot1",
			"ItemSlot2",
			"ItemSlot3",
			"ItemSlot4"
		};

		
		private bool _pendingEquip;

		
		public UnityEvent StashedLeftHand = new UnityEvent();

		
		public UnityEvent StashedRightHand = new UnityEvent();

		
		public UnityEvent DroppedRightHand = new UnityEvent();

		
		public UnityEvent Attacked = new UnityEvent();

		
		public UnityEvent AttackEnded = new UnityEvent();

		
		public UnityEvent ReleasedAttack = new UnityEvent();

		
		public UnityEvent Blocked = new UnityEvent();

		
		public UnityEvent Unblocked = new UnityEvent();

		
		[CompilerGenerated]
		private static Func<IGrouping<int, InventoryItemView>, List<InventoryItemView>> <>f__mg$cache0;

		
		public class ItemEvent : UnityEvent<int>
		{
		}

		
		public enum PlayerViews
		{
			
			PlaneCrash = -1,
			
			Loading,
			
			WakingUp,
			
			World,
			
			Inventory,
			
			ClosingInventory,
			
			Book,
			
			Pause,
			
			Death,
			
			Loot,
			
			Sleep,
			
			EndCrash,
			
			PlayerList
		}

		
		public class ItemsUpgradeCountersDict : Dictionary<int, PlayerInventory.UpgradeCounterDict>
		{
		}

		
		public class UpgradeCounterDict : Dictionary<int, int>
		{
		}

		
		[Serializable]
		public class SerializableItemUpgradeCounters
		{
			
			public int _itemId;

			
			public int _count;

			
			public PlayerInventory.SerializableUpgradeCounter[] _counters;
		}

		
		[Serializable]
		public class SerializableUpgradeCounter
		{
			
			public int _upgradeItemId;

			
			public int _amount;
		}
	}
}
