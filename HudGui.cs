using System;
using System.Collections;
using System.Collections.Generic;
using PathologicalGames;
using TheForest.Buildings.Creation;
using TheForest.Buildings.World;
using TheForest.Items;
using TheForest.Items.Craft;
using TheForest.Items.Inventory;
using TheForest.Items.UI;
using TheForest.Modding.Bridge.Interfaces;
using TheForest.UI;
using TheForest.UI.Anim;
using TheForest.UI.Crafting;
using TheForest.UI.Multiplayer;
using TheForest.Utils;
using UniLinq;
using UnityEngine;


public class HudGui : MonoBehaviour
{
	
	
	
	public MonoBehaviour DelayedActionIconController { get; set; }

	
	private void Start()
	{
		this.BookCam.eventMask = 0;
		this.GuiCamC.eventMask = 0;
		this.ActionIconCams.eventMask = 0;
		try
		{
			this.CheckHudState();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		this._screenResolutionHash = this.GetScreenResolutionHash();
		this._inventoryItemsInfoCache = this._inventoryItemsInfo.ToDictionary((HudGui.InventoryItemInfo iii) => iii._itemId);
		this._cantCarryItemViewGOs = new Dictionary<int, HudGui.TimedGameObject>();
		this._gotItemViewGOs = new Dictionary<int, HudGui.TimedGameObject>();
		this._foundPassengerViewGOs = new Dictionary<int, HudGui.TimedGameObject>();
		this._todoListMessagesGOs = new Dictionary<int, HudGui.TimedGameObject>();
		this._tgoPool = new Queue<HudGui.TimedGameObject>();
		this._buildMissionQueue = new Queue<BuildMissionWidget>();
		this._buildMissionDisplayed = new Dictionary<int, BuildMissionWidget>();
		base.enabled = false;
		if (!CoopPeerStarter.DedicatedHost)
		{
			Scene.ActiveMB.StartCoroutine(this.WaitForGameStart());
		}
	}

	
	private void Update()
	{
		this.CheckTimedGOs(this._cantCarryItemViewGOs);
		this.CheckTimedGOs(this._gotItemViewGOs);
		this.CheckTimedGOs(this._foundPassengerViewGOs);
		this.CheckTimedGOs(this._todoListMessagesGOs);
		if (BoltNetwork.isRunning && !CoopPeerStarter.DedicatedHost && TheForest.Utils.Input.GetButtonDown("PlayerList"))
		{
			this.ToggleMpPlayerList();
		}
		if (this._screenResolutionHash != this.GetScreenResolutionHash())
		{
			this._screenResolutionHash = this.GetScreenResolutionHash();
			Scene.ActiveMB.StartCoroutine(this.RefreshHud());
		}
		if (this._buildMissionQueue.Count > 0 && this._nextBuildMissionDisplay <= Time.realtimeSinceStartup)
		{
			this._buildMissionQueue.Dequeue().Show();
			this._nextBuildMissionDisplay = Time.realtimeSinceStartup + 0.15f;
		}
		this.CheckDelayedActionController();
	}

	
	private IEnumerator WaitForGameStart()
	{
		while (!Scene.FinishGameLoad || !PoolManager.Pools.ContainsKey("misc"))
		{
			yield return null;
		}
		this.InitPrefabPool(this._cantCarryItemView.transform);
		this.InitPrefabPool(this._cantCarryItemWeightView.transform);
		this.InitPrefabPool(this._gotItemView.transform);
		this.InitPrefabPool(this._foundPassengerView.transform);
		this.InitPrefabPool(this._todoListMessageView.transform);
		this.InitPrefabPoolLarge(this.BuildMissionWidget.transform);
		this.Chatbox.gameObject.SetActive(true);
		this.Grid.gameObject.SetActive(true);
		this.Grid.Reposition();
		base.enabled = !CoopPeerStarter.DedicatedHost;
		BuildMission.ActiveMissions.Values.ForEach(delegate(BuildMission bm)
		{
			bm.RefreshAmount();
		});
		yield break;
	}

	
	private void InitPrefabPool(Transform prefab)
	{
		try
		{
			PrefabPool prefabPool = PoolManager.Pools["misc"].GetPrefabPool(prefab);
			if (prefabPool == null)
			{
				prefabPool = new PrefabPool(prefab);
				prefabPool.cullAbove = 5;
				prefabPool.cullDelay = 10;
				prefabPool.cullMaxPerPass = 10;
				prefabPool.cullDespawned = true;
				PoolManager.Pools["misc"].CreatePrefabPool(prefabPool);
			}
			else
			{
				prefabPool.cullAbove = 5;
				prefabPool.cullDelay = 10;
				prefabPool.cullMaxPerPass = 10;
				prefabPool.cullDespawned = true;
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	
	private void InitPrefabPoolLarge(Transform prefab)
	{
		try
		{
			PrefabPool prefabPool = PoolManager.Pools["misc"].GetPrefabPool(prefab);
			if (prefabPool == null)
			{
				prefabPool = new PrefabPool(prefab);
				prefabPool.preloadAmount = 20;
				prefabPool.cullDespawned = false;
				PoolManager.Pools["misc"].CreatePrefabPool(prefabPool);
			}
			else
			{
				prefabPool.preloadAmount = 20;
				prefabPool.cullDespawned = false;
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	
	public void ShowMpPlayerList()
	{
		this.MpPlayerListCamGo.SetActive(true);
		this.MpPlayerList.EnableList();
	}

	
	public void HideMpPlayerList()
	{
		this.MpPlayerList.DisableList();
		this.MpPlayerListCamGo.SetActive(false);
	}

	
	public void ClearMpPlayerList()
	{
		if (BoltNetwork.isRunning && !CoopPeerStarter.DedicatedHost && this.MpPlayerListCamGo.activeInHierarchy)
		{
			this.HideMpPlayerList();
		}
	}

	
	public void ToggleMpPlayerList()
	{
		if (this.MpPlayerListCamGo.activeInHierarchy)
		{
			this.HideMpPlayerList();
		}
		else if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World)
		{
			this.ShowMpPlayerList();
		}
	}

	
	private void OnApplicationFocus(bool focusStatus)
	{
		Debug.Log("OnApplicationFocus: focusStatus=" + focusStatus);
		if (Scene.ActiveMB)
		{
			Scene.ActiveMB.StartCoroutine(this.RefreshHud());
		}
	}

	
	private IEnumerator RefreshHud()
	{
		if (Scene.Atmosphere)
		{
			Scene.Atmosphere.ForceSunRotationUpdate = true;
		}
		yield return null;
		if (Scene.HudGui)
		{
			Scene.HudGui.gameObject.SetActive(false);
		}
		yield return null;
		yield return null;
		if (Scene.HudGui)
		{
			Scene.HudGui.gameObject.SetActive(true);
		}
		yield break;
	}

	
	private int GetScreenResolutionHash()
	{
		return Screen.width * 100000 + Screen.height;
	}

	
	public void CheckHudState()
	{
		if (!SteamDSConfig.isDedicatedServer)
		{
			bool flag = !CoopPeerStarter.DedicatedHost && LocalPlayer.Inventory && LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Pause && !Scene.Atmosphere.Sleeping && this.hudCameraOffCounter <= 0;
			if (this.GuiCamC.enabled != flag)
			{
				this.GuiCamC.enabled = flag;
			}
			bool flag2 = LocalPlayer.AnimControl && LocalPlayer.AnimControl.upsideDown;
			bool flag3 = LocalPlayer.Inventory && LocalPlayer.Inventory.CurrentView < PlayerInventory.PlayerViews.WakingUp;
			this.ActionIconCams.enabled = ((flag || flag2) && (PlayerPreferences.ShowHud || flag3));
			if (this.ToggleableHud.activeSelf != PlayerPreferences.ShowHud)
			{
				this.ToggleableHud.SetActive(PlayerPreferences.ShowHud);
			}
		}
	}

	
	public void ShowHud(bool on)
	{
		if (on)
		{
			this.hudCameraOffCounter = Mathf.Max(0, this.hudCameraOffCounter - 1);
		}
		else
		{
			this.hudCameraOffCounter++;
		}
		this.CheckHudState();
	}

	
	public void ToggleAllHud(bool on)
	{
		this.ActionIconCams.enabled = on;
		this.ShowHud(on);
	}

	
	private void CheckTimedGOs(Dictionary<int, HudGui.TimedGameObject> gos)
	{
		foreach (KeyValuePair<int, HudGui.TimedGameObject> keyValuePair in gos)
		{
			HudGui.TimedGameObject value = keyValuePair.Value;
			if (value._endTime < Time.time)
			{
				if (PoolManager.Pools["misc"].IsSpawned(value._GO.transform))
				{
					PoolManager.Pools["misc"].Despawn(value._GO.transform);
				}
				else
				{
					UnityEngine.Object.Destroy(value._GO);
				}
				value._GO = null;
				value._label = null;
				this._tgoPool.Enqueue(value);
				gos.Remove(value._itemId);
				this.Grid.repositionNow = true;
				break;
			}
		}
	}

	
	public void TogglePauseMenu()
	{
		this.TogglePauseMenu(!this.PauseMenu.activeSelf);
	}

	
	public void TogglePauseMenu(bool on)
	{
		if (on)
		{
			VirtualCursor.Instance.SetCursorType(VirtualCursor.CursorTypes.None);
			this.PauseMenu.SetActive(true);
			this.ShowHud(false);
			if (LocalPlayer.GameObject)
			{
				if (!BoltNetwork.isRunning)
				{
					Time.timeScale = 0f;
				}
				LocalPlayer.FpCharacter.LockView(true);
				LocalPlayer.Create.CloseBuildMode();
				LocalPlayer.PauseMenuBlur.enabled = true;
				LocalPlayer.PauseMenuBlurPsCam.enabled = true;
				if (this.PauseMenuCoordsLabel)
				{
					this.PauseMenuCoordsLabel.text = LocalPlayer.Transform.position.ToString();
				}
			}
		}
		else
		{
			this.ShowHud(true);
			this.CheckHudState();
			this.PauseMenu.SetActive(false);
			this.CheckHudState();
			if (LocalPlayer.GameObject)
			{
				Time.timeScale = 1f;
				LocalPlayer.FpCharacter.UnLockView();
				LocalPlayer.PauseMenuBlur.enabled = false;
				LocalPlayer.PauseMenuBlurPsCam.enabled = false;
			}
		}
	}

	
	public void SetDelayedIconController(MonoBehaviour mb)
	{
		if (this.DelayedActionIconController != mb)
		{
			this.DelayedActionIconController = mb;
			this.CheckDelayedActionController();
		}
	}

	
	public void UnsetDelayedIconController(MonoBehaviour mb)
	{
		if (this.DelayedActionIconController == mb)
		{
			this.SetDelayedIconController(null);
		}
	}

	
	public void CheckDelayedActionController()
	{
		if (this.DelayedActionIconController && this.DelayedActionIconController.enabled && this.DelayedActionIconController.gameObject.activeSelf)
		{
			if (!this.DelayedActionIcon.activeSelf)
			{
				this.DelayedActionIcon.SetActive(true);
			}
		}
		else if (this.DelayedActionIcon.activeSelf)
		{
			this.DelayedActionIcon.SetActive(false);
			this.DelayedActionIconController = null;
		}
	}

	
	public string GetItemName(int itemId, bool plural, bool caps)
	{
		string text = null;
		HudGui.InventoryItemInfo inventoryItemInfo = this._inventoryItemsInfoCache[itemId];
		if (plural)
		{
			text = UiTranslationDatabase.TranslateKey(inventoryItemInfo._titlePluralTextTranslationKey, inventoryItemInfo._titlePluralText, caps);
		}
		if (!plural || string.IsNullOrEmpty(text))
		{
			text = UiTranslationDatabase.TranslateKey(inventoryItemInfo._titleTextTranslationKey, inventoryItemInfo._titleText, caps);
		}
		return text;
	}

	
	private string GetDecayingItemName(HudGui.InventoryItemInfo iii, InventoryItemView view, bool plural, bool caps)
	{
		string text = null;
		if (view)
		{
			DecayingInventoryItemView.DecayStates state = ((DecayingInventoryItemView)view).DecayProperties._state;
			string text2 = null;
			string format;
			if (state < DecayingInventoryItemView.DecayStates.DriedFresh)
			{
				format = UiTranslationDatabase.TranslateKey("RAWMEAT_0_1", "Raw {0} {1}", caps);
			}
			else
			{
				format = "{0} {1}";
			}
			string arg;
			switch (state)
			{
			case DecayingInventoryItemView.DecayStates.Edible:
				arg = UiTranslationDatabase.TranslateKey("EDIBLE", "Edible", false);
				break;
			case DecayingInventoryItemView.DecayStates.Spoilt:
				arg = UiTranslationDatabase.TranslateKey("SPOILT", "Spoilt", false);
				break;
			case DecayingInventoryItemView.DecayStates.DriedFresh:
				arg = UiTranslationDatabase.TranslateKey("DRIED_FRESH", "Dried Fresh", false);
				break;
			case DecayingInventoryItemView.DecayStates.DriedEdible:
				arg = UiTranslationDatabase.TranslateKey("DRIED_EDIBLE", "Dried Edible", false);
				break;
			case DecayingInventoryItemView.DecayStates.DriedSpoilt:
				arg = UiTranslationDatabase.TranslateKey("DRIED_SPOILT", "Dried Spoilt", false);
				break;
			default:
				arg = UiTranslationDatabase.TranslateKey("FRESH", "Fresh", caps);
				break;
			}
			if (plural)
			{
				text2 = UiTranslationDatabase.TranslateKey(iii._titlePluralTextTranslationKey, iii._titlePluralText, caps);
			}
			if (!plural || string.IsNullOrEmpty(text2))
			{
				text2 = UiTranslationDatabase.TranslateKey(iii._titleTextTranslationKey, iii._titleText, caps);
			}
			text = string.Format(format, arg, text2);
		}
		else
		{
			if (plural)
			{
				text = UiTranslationDatabase.TranslateKey(iii._titlePluralTextTranslationKey, iii._titlePluralText, caps);
			}
			if (!plural || string.IsNullOrEmpty(text))
			{
				text = UiTranslationDatabase.TranslateKey(iii._titleTextTranslationKey, iii._titleText, caps);
			}
		}
		return text;
	}

	
	private string GetLeftClickActionName(HudGui.InventoryItemInfo iii)
	{
		switch (iii._leftClick)
		{
		case HudGui.InventoryItemInfo.LeftClickCommands.equip:
			return UiTranslationDatabase.TranslateKey("EQUIP", "equip", false);
		case HudGui.InventoryItemInfo.LeftClickCommands.play:
			return UiTranslationDatabase.TranslateKey("PLAY", "play", false);
		case HudGui.InventoryItemInfo.LeftClickCommands.drink:
			return UiTranslationDatabase.TranslateKey("DRINK", "drink", false);
		case HudGui.InventoryItemInfo.LeftClickCommands.read:
			return UiTranslationDatabase.TranslateKey("READ", "read", false);
		case HudGui.InventoryItemInfo.LeftClickCommands.take:
			return UiTranslationDatabase.TranslateKey("TAKE", "take", false);
		case HudGui.InventoryItemInfo.LeftClickCommands.Charge_Flashlight:
			return UiTranslationDatabase.TranslateKey("CHARGE", "charge", false);
		case HudGui.InventoryItemInfo.LeftClickCommands.wear:
			return UiTranslationDatabase.TranslateKey("WEAR", "wear", false);
		case HudGui.InventoryItemInfo.LeftClickCommands.eat:
			return UiTranslationDatabase.TranslateKey("EAT", "eat", false);
		case HudGui.InventoryItemInfo.LeftClickCommands.select:
			return UiTranslationDatabase.TranslateKey("SELECT", "select", false);
		case HudGui.InventoryItemInfo.LeftClickCommands.use:
			return UiTranslationDatabase.TranslateKey("USE", "use", false);
		default:
			return "None";
		}
	}

	
	private string GetRightClickActionName(HudGui.InventoryItemInfo iii)
	{
		switch (iii._rightClick)
		{
		case HudGui.InventoryItemInfo.RightClickCommands.combine:
			return UiTranslationDatabase.TranslateKey("COMBINE", "combine", false);
		case HudGui.InventoryItemInfo.RightClickCommands.remove:
			return UiTranslationDatabase.TranslateKey("REMOVE", "remove", false);
		case HudGui.InventoryItemInfo.RightClickCommands.unequip:
			return UiTranslationDatabase.TranslateKey("UNEQUIP", "unequip", false);
		default:
			return "None";
		}
	}

	
	public void ShowItemInfoView(InventoryItemView itemView, Vector3 viewportPos, bool isCraft)
	{
		if (LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Inventory)
		{
			return;
		}
		if (this._inventoryItemInfoView._itemId == itemView._itemId && this._inventoryItemInfoView.IsCraft == isCraft)
		{
			this._inventoryItemInfoView.ViewCounter++;
		}
		else if (!isCraft)
		{
			if (this._inventoryItemsInfoCache.ContainsKey(itemView._itemId))
			{
				this._inventoryItemInfoView.ViewCounter = 1;
				this._inventoryItemInfoView._itemId = itemView._itemId;
				this._inventoryItemInfoView.IsCraft = false;
				if (viewportPos.y > 0.6f)
				{
					viewportPos.y -= 0.1f;
				}
				else
				{
					viewportPos.y += 0.1f;
				}
				if (viewportPos.x > 0.7f)
				{
					viewportPos.x -= 0.075f;
				}
				else if (viewportPos.x < 0.3f)
				{
					viewportPos.x += 0.075f;
				}
				viewportPos.x = Mathf.Clamp(viewportPos.x, 0.15f, 0.85f);
				viewportPos.y = Mathf.Clamp(viewportPos.y, 0.15f, 0.85f);
				viewportPos.z = 2f;
				Vector3 position = this.ActionIconCams.ViewportToWorldPoint(viewportPos);
				this._inventoryItemInfoView._root.transform.position = position;
				HudGui.InventoryItemInfo inventoryItemInfo = this._inventoryItemsInfoCache[itemView._itemId];
				this._inventoryItemInfoView._icon.spriteName = inventoryItemInfo._icon.name;
				if (itemView is DecayingInventoryItemView)
				{
					this._inventoryItemInfoView._title.text = this.GetDecayingItemName(inventoryItemInfo, itemView, false, false);
				}
				else
				{
					this._inventoryItemInfoView._title.text = UiTranslationDatabase.TranslateKey(inventoryItemInfo._titleTextTranslationKey, inventoryItemInfo._titleText, false);
				}
				this._inventoryItemInfoView._effect.text = UiTranslationDatabase.TranslateKey(inventoryItemInfo._effectTextTranslationKey, inventoryItemInfo._effectText, false);
				this._inventoryItemInfoView._description.text = UiTranslationDatabase.TranslateKey(inventoryItemInfo._descriptionTextTranslationKey, inventoryItemInfo._descriptionText, false);
				this._inventoryItemInfoView._effect.gameObject.SetActive(true);
				this._inventoryItemInfoView._description.gameObject.SetActive(true);
				switch (inventoryItemInfo._amountDisplay)
				{
				case HudGui.InventoryItemInfo.AmountDisplay.none:
					this._inventoryItemInfoView._amountText._label.enabled = false;
					break;
				case HudGui.InventoryItemInfo.AmountDisplay.Amount:
					this._inventoryItemInfoView._amountText._label.enabled = true;
					this._inventoryItemInfoView._amountText._itemId = itemView._itemId;
					break;
				case HudGui.InventoryItemInfo.AmountDisplay.Pedometer:
					this._inventoryItemInfoView._amountText._label.enabled = false;
					this._inventoryItemInfoView._description.text = UiTranslationDatabase.TranslateKey(inventoryItemInfo._descriptionTextTranslationKey, inventoryItemInfo._descriptionText, false).Replace("%", LocalPlayer.Stats.PedometerSteps.ToString());
					break;
				case HudGui.InventoryItemInfo.AmountDisplay.Battery:
					this._inventoryItemInfoView._amountText._label.enabled = false;
					this._inventoryItemInfoView._description.text = UiTranslationDatabase.TranslateKey(inventoryItemInfo._descriptionTextTranslationKey, inventoryItemInfo._descriptionText, false).Replace("%", Mathf.FloorToInt(LocalPlayer.Stats.BatteryCharge) + "%");
					break;
				case HudGui.InventoryItemInfo.AmountDisplay.Air:
					this._inventoryItemInfoView._amountText._label.enabled = false;
					this._inventoryItemInfoView._description.text = UiTranslationDatabase.TranslateKey(inventoryItemInfo._descriptionTextTranslationKey, inventoryItemInfo._descriptionText, false).Replace("%", Mathf.FloorToInt(LocalPlayer.Stats.AirBreathing.CurrentRebreatherAir) + "s");
					if (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.Chest, inventoryItemInfo._itemId))
					{
						UILabel description = this._inventoryItemInfoView._description;
						description.text += UiTranslationDatabase.TranslateKey("__EQUIPPED_", " (Equiped)", false);
					}
					break;
				case HudGui.InventoryItemInfo.AmountDisplay.WaterFill:
				{
					this._inventoryItemInfoView._amountText._label.enabled = false;
					WeaponStatUpgrade.Types activeBonus = itemView.ActiveBonus;
					string newValue;
					if (activeBonus != WeaponStatUpgrade.Types.DirtyWater)
					{
						if (activeBonus != WeaponStatUpgrade.Types.CleanWater)
						{
							newValue = "empty";
						}
						else
						{
							newValue = UiTranslationDatabase.TranslateKey("CLEAN_WATER", "clean water", false) + " (" + string.Format("{0:P0}", itemView.Properties.ActiveBonusValue / 2f) + ")";
						}
					}
					else
					{
						newValue = UiTranslationDatabase.TranslateKey("POLLUTED_WATER", "polluted water", false) + " (" + string.Format("{0:P0}", itemView.Properties.ActiveBonusValue / 2f) + ")";
					}
					this._inventoryItemInfoView._description.text = UiTranslationDatabase.TranslateKey(inventoryItemInfo._descriptionTextTranslationKey, inventoryItemInfo._descriptionText, false).Replace("%", newValue);
					break;
				}
				case HudGui.InventoryItemInfo.AmountDisplay.Fuel:
					this._inventoryItemInfoView._amountText._label.enabled = false;
					this._inventoryItemInfoView._description.text = UiTranslationDatabase.TranslateKey(inventoryItemInfo._descriptionTextTranslationKey, inventoryItemInfo._descriptionText, false).Replace("%", string.Format("{0:P0}", LocalPlayer.Stats.Fuel.CurrentFuel / LocalPlayer.Stats.Fuel.MaxFuelCapacity));
					break;
				case HudGui.InventoryItemInfo.AmountDisplay.Ammo:
					this._inventoryItemInfoView._amountText._label.enabled = true;
					this._inventoryItemInfoView._amountText._itemId = itemView.ItemCache._ammoItemId;
					break;
				}
				this._inventoryItemInfoView._weight.enabled = false;
				if (this._inventoryItemInfoView._upgradeCounterGrid)
				{
					if (itemView.ItemCache.MatchType(Item.Types.Equipment))
					{
						this._inventoryItemInfoView._upgradeCounterGrid.gameObject.SetActive(true);
						for (int i = 0; i < this._inventoryItemInfoView._upgradeCounterViews.Count; i++)
						{
							HudGui.UpgradeCounterView upgradeCounterView = this._inventoryItemInfoView._upgradeCounterViews[i];
							int amountOfUpgrades = LocalPlayer.Inventory.GetAmountOfUpgrades(itemView._itemId, upgradeCounterView._itemId);
							if (amountOfUpgrades > 0)
							{
								upgradeCounterView._root.SetActive(true);
								upgradeCounterView._label.text = amountOfUpgrades.ToString();
							}
							else
							{
								upgradeCounterView._root.SetActive(false);
							}
						}
						this._inventoryItemInfoView._upgradeCounterGrid.Reposition();
					}
					else
					{
						this._inventoryItemInfoView._upgradeCounterGrid.gameObject.SetActive(false);
					}
				}
				if (this._inventoryItemInfoView._upgradeBonusGrid)
				{
					if (!inventoryItemInfo._dontShowWeaponInfo && itemView.ItemCache.MatchType(Item.Types.Equipment) && itemView.ItemCache.MatchType(Item.Types.Weapon) && itemView._heldWeaponInfo)
					{
						this._inventoryItemInfoView._effect.gameObject.SetActive(false);
						this._inventoryItemInfoView._description.gameObject.SetActive(false);
						this._inventoryItemInfoView._upgradeBonusGrid.gameObject.SetActive(true);
						this._inventoryItemInfoView._speedBonusView._root.SetActive(true);
						float fillAmount = itemView._heldWeaponInfo.weaponSpeed / 12f;
						this._inventoryItemInfoView._speedBonusView._amount.fillAmount = fillAmount;
						this._inventoryItemInfoView._damageBonusView._root.SetActive(true);
						float fillAmount2 = itemView._heldWeaponInfo.weaponDamage / 12f;
						this._inventoryItemInfoView._damageBonusView._amount.fillAmount = fillAmount2;
						this._inventoryItemInfoView._blockView._root.SetActive(true);
						float fillAmount3 = 1f - itemView._heldWeaponInfo.blockDamagePercent / 1f;
						this._inventoryItemInfoView._blockView._amount.fillAmount = fillAmount3;
						this._inventoryItemInfoView._upgradeBonusGrid.Reposition();
					}
					else
					{
						this._inventoryItemInfoView._upgradeBonusGrid.gameObject.SetActive(false);
					}
				}
				for (int j = 0; j < this._inventoryItemInfoView._usableUpgradeViews.Count; j++)
				{
					HudGui.UsableUpgradeView usableUpgradeView = this._inventoryItemInfoView._usableUpgradeViews[j];
					if (usableUpgradeView._stat == itemView.ActiveBonus != usableUpgradeView._root.activeSelf)
					{
						usableUpgradeView._root.SetActive(!usableUpgradeView._root.activeSelf);
					}
				}
				bool canUseWithPrimary = itemView.CanUseWithPrimary;
				if (inventoryItemInfo._leftClick == HudGui.InventoryItemInfo.LeftClickCommands.none || (!itemView.ItemCache.MatchType(Item.Types.Equipment) && itemView.ItemCache.MatchType(Item.Types.Edible) && !canUseWithPrimary))
				{
					this._inventoryItemInfoView._leftClickIcon.enabled = false;
					this._inventoryItemInfoView._leftClickText.enabled = false;
				}
				else
				{
					this._inventoryItemInfoView._leftClickIcon.enabled = true;
					this._inventoryItemInfoView._leftClickText.enabled = true;
					if (canUseWithPrimary && (itemView.ActiveBonus == WeaponStatUpgrade.Types.CleanWater || itemView.ActiveBonus == WeaponStatUpgrade.Types.DirtyWater))
					{
						this._inventoryItemInfoView._leftClickText.text = UiTranslationDatabase.TranslateKey("DRINK", "Drink", false);
					}
					else
					{
						this._inventoryItemInfoView._leftClickText.text = this.GetLeftClickActionName(inventoryItemInfo);
					}
				}
				if (LocalPlayer.Inventory.CurrentStorage == LocalPlayer.Inventory._craftingCog)
				{
					if (itemView._canDropFromInventory)
					{
						this._inventoryItemInfoView._rightClickIcon.enabled = true;
						this._inventoryItemInfoView._rightClickText.enabled = true;
						this._inventoryItemInfoView._rightClickText.text = UiTranslationDatabase.TranslateKey("DROP", "Drop", false);
					}
					else if (itemView.CanUseWithSecondary)
					{
						this._inventoryItemInfoView._rightClickIcon.enabled = true;
						this._inventoryItemInfoView._rightClickText.enabled = true;
						if (itemView.ActiveBonus == WeaponStatUpgrade.Types.CleanWater || itemView.ActiveBonus == WeaponStatUpgrade.Types.DirtyWater)
						{
							this._inventoryItemInfoView._rightClickText.text = UiTranslationDatabase.TranslateKey("DRINK", "Drink", false);
						}
						else if (itemView.ActiveBonus == WeaponStatUpgrade.Types.DriedFood)
						{
							this._inventoryItemInfoView._rightClickText.text = UiTranslationDatabase.TranslateKey("EAT", "Eat", false);
						}
						else
						{
							this._inventoryItemInfoView._rightClickText.text = UiTranslationDatabase.TranslateKey("USE", "Use", false);
						}
					}
					else if (itemView.CanBeHotkeyed)
					{
						this._inventoryItemInfoView._rightClickIcon.enabled = true;
						this._inventoryItemInfoView._rightClickText.enabled = true;
						this._inventoryItemInfoView._rightClickText.text = UiTranslationDatabase.TranslateKey("COMBINE", "combine", false);
					}
					else if (inventoryItemInfo._rightClick == HudGui.InventoryItemInfo.RightClickCommands.none)
					{
						this._inventoryItemInfoView._rightClickIcon.enabled = false;
						this._inventoryItemInfoView._rightClickText.enabled = false;
					}
					else
					{
						this._inventoryItemInfoView._rightClickIcon.enabled = true;
						this._inventoryItemInfoView._rightClickText.enabled = true;
						this._inventoryItemInfoView._rightClickText.text = this.GetRightClickActionName(inventoryItemInfo);
					}
				}
				else if (itemView.CanBeStored)
				{
					this._inventoryItemInfoView._rightClickIcon.enabled = true;
					this._inventoryItemInfoView._rightClickText.enabled = true;
					this._inventoryItemInfoView._rightClickText.text = UiTranslationDatabase.TranslateKey("STORE", "Store", false);
				}
				else
				{
					this._inventoryItemInfoView._rightClickIcon.enabled = false;
					this._inventoryItemInfoView._rightClickText.enabled = false;
				}
				this._inventoryItemInfoView._root.SetActive(true);
			}
			else
			{
				Debug.LogWarning("Missing Inventory Item Info data in HUDGui for " + itemView.ItemCache._name);
			}
		}
		else
		{
			if (!this.ClickToRemoveInfo.activeSelf)
			{
				this._inventoryItemInfoView.ViewCounter = 1;
				this._inventoryItemInfoView._itemId = itemView._itemId;
				this._inventoryItemInfoView.IsCraft = true;
				this.ClickToRemoveInfo.SetActive(true);
			}
			if (this.ClickToEquipInfo.activeSelf != itemView._canEquipFromCraft)
			{
				this.ClickToEquipInfo.SetActive(itemView._canEquipFromCraft);
			}
		}
	}

	
	public void HideItemInfoView(int itemId, bool isCraft)
	{
		if (this._inventoryItemInfoView._itemId == itemId && this._inventoryItemInfoView.IsCraft == isCraft)
		{
			this._inventoryItemInfoView.ViewCounter--;
			if (this._inventoryItemInfoView.ViewCounter == 0)
			{
				this.ClickToRemoveInfo.SetActive(false);
				this.ClickToEquipInfo.SetActive(false);
				if (this._inventoryItemInfoView._root.activeSelf)
				{
					this._inventoryItemInfoView._root.SetActive(false);
				}
				this._inventoryItemInfoView._itemId = 0;
			}
		}
	}

	
	public void ShowCarriedWeightInfo(Vector3 viewportPos)
	{
	}

	
	public void HideCarriedWeightInfo()
	{
		this._carriedWeightInfoView._root.SetActive(false);
	}

	
	public void ShowQuickSelectInfo(Vector3 viewportPos)
	{
		if (viewportPos.y > 0.75f)
		{
			viewportPos.y -= 0.1f;
		}
		else
		{
			viewportPos.y += 0.1f;
		}
		if (viewportPos.x > 0.8f)
		{
			viewportPos.x -= 0.075f;
		}
		else if (viewportPos.x < 0.2f)
		{
			viewportPos.x += 0.075f;
		}
		viewportPos.z = 2f;
		Vector3 position = this.GuiCamC.ViewportToWorldPoint(viewportPos);
		this._quickSelectTooltipView._root.SetActive(true);
		this._quickSelectTooltipView._root.transform.position = position;
		for (int i = 0; i < this._quickSelectTooltipView._slots.Length; i++)
		{
			int num = LocalPlayer.Inventory.QuickSelectItemIds[i];
			if (num > 0 && this._inventoryItemsInfoCache.ContainsKey(num))
			{
				HudGui.InventoryItemInfo inventoryItemInfo = this._inventoryItemsInfoCache[num];
				if (inventoryItemInfo != null)
				{
					this._quickSelectTooltipView._slots[i].text = UiTranslationDatabase.TranslateKey(inventoryItemInfo._titleTextTranslationKey, inventoryItemInfo._titleText, false);
				}
			}
		}
	}

	
	public void HideQuickSelectInfo()
	{
		this._quickSelectTooltipView._root.SetActive(false);
	}

	
	private HudGui.TimedGameObject SpawnTimedGameObject(int itemId, int amount, float endTime, GameObject go, UILabel label)
	{
		HudGui.TimedGameObject timedGameObject;
		if (this._tgoPool.Count > 0)
		{
			timedGameObject = this._tgoPool.Dequeue();
		}
		else
		{
			timedGameObject = new HudGui.TimedGameObject();
		}
		timedGameObject._itemId = itemId;
		timedGameObject._amount = amount;
		timedGameObject._endTime = endTime;
		timedGameObject._GO = go;
		timedGameObject._label = label;
		return timedGameObject;
	}

	
	public void ToggleFullCapacityHud(int itemId)
	{
		if (!CoopPeerStarter.DedicatedHost && base.gameObject.activeSelf && this.Grid.gameObject.activeSelf && this.GuiCamC.enabled && this._inventoryItemsInfoCache.ContainsKey(itemId) && this._inventoryItemsInfoCache[itemId]._showCantCarryMoreItem && this.GuiCam.activeSelf && this._cantCarryItemViewGOs.Count < 10 && PoolManager.Pools.ContainsKey("misc") && PlayerPreferences.ShowHud)
		{
			if (!this._cantCarryItemViewGOs.ContainsKey(itemId))
			{
				base.enabled = true;
				GameObject gameObject = PoolManager.Pools["misc"].Spawn(this._cantCarryItemView.transform, this._cantCarryItemView.transform.position, this._cantCarryItemView.transform.rotation).gameObject;
				gameObject.transform.parent = this.Grid.transform;
				gameObject.transform.localScale = this._cantCarryItemView.transform.localScale;
				gameObject.transform.SetSiblingIndex(this._gotItemView.transform.GetSiblingIndex() + 1);
				gameObject.SetActive(true);
				UILabel componentInChildren = gameObject.GetComponentInChildren<UILabel>();
				if (componentInChildren)
				{
					componentInChildren.text = this.GetDecayingItemName(this._inventoryItemsInfoCache[itemId], null, true, true);
				}
				this.Grid.Reposition();
				this._cantCarryItemViewGOs[itemId] = this.SpawnTimedGameObject(itemId, 0, Time.time + 5f, gameObject, null);
			}
			else
			{
				this._cantCarryItemViewGOs[itemId]._endTime = Time.time + 5f;
				this._cantCarryItemViewGOs[itemId]._GO.SetActive(false);
				this.Grid.Reposition();
				this._cantCarryItemViewGOs[itemId]._GO.SetActive(true);
				this.Grid.Reposition();
			}
		}
	}

	
	public void ToggleFullWeightHud(int itemId)
	{
		if (this.GuiCam.activeSelf && !CoopPeerStarter.DedicatedHost && base.enabled && PoolManager.Pools.ContainsKey("misc"))
		{
			if (!this._cantCarryItemViewGOs.ContainsKey(itemId))
			{
				base.enabled = true;
				GameObject gameObject = PoolManager.Pools["misc"].Spawn(this._cantCarryItemWeightView.transform, this._cantCarryItemWeightView.transform.position, this._cantCarryItemWeightView.transform.rotation).gameObject;
				gameObject.transform.parent = this.Grid.transform;
				gameObject.transform.localScale = this._cantCarryItemWeightView.transform.localScale;
				gameObject.SetActive(true);
				UILabel componentInChildren = gameObject.GetComponentInChildren<UILabel>();
				if (componentInChildren)
				{
					componentInChildren.text = componentInChildren.text.Replace("{ITEM}", string.IsNullOrEmpty(this._inventoryItemsInfoCache[itemId]._titlePluralText) ? this._inventoryItemsInfoCache[itemId]._titleText : this._inventoryItemsInfoCache[itemId]._titlePluralText);
				}
				this.Grid.Reposition();
				this._cantCarryItemViewGOs[itemId] = this.SpawnTimedGameObject(itemId, 0, Time.time + 5f, gameObject, null);
			}
			else
			{
				this._cantCarryItemViewGOs[itemId]._endTime = Time.time + 5f;
				this._cantCarryItemViewGOs[itemId]._GO.SetActive(false);
				this.Grid.Reposition();
				this._cantCarryItemViewGOs[itemId]._GO.SetActive(true);
				this.Grid.Reposition();
			}
		}
	}

	
	public void ToggleGotItemHud(int itemId, int amount)
	{
		if (!CoopPeerStarter.DedicatedHost && base.gameObject.activeSelf && this.Grid.gameObject.activeSelf && this.GuiCamC.enabled && this._inventoryItemsInfoCache.ContainsKey(itemId) && this._inventoryItemsInfoCache[itemId]._showGotItem && this.GuiCam.activeSelf && this._gotItemViewGOs.Count < 10 && PoolManager.Pools.ContainsKey("misc") && PlayerPreferences.ShowHud)
		{
			if (!this._gotItemViewGOs.ContainsKey(itemId))
			{
				base.enabled = true;
				GameObject gameObject = PoolManager.Pools["misc"].Spawn(this._gotItemView.transform, this._gotItemView.transform.position, this._gotItemView.transform.rotation).gameObject;
				gameObject.transform.parent = this.Grid.transform;
				gameObject.transform.localScale = this._gotItemView.transform.localScale;
				gameObject.transform.localScale = this.BuildMissionWidget.transform.localScale;
				gameObject.transform.SetSiblingIndex(this._gotItemView.transform.GetSiblingIndex() + 1);
				gameObject.SetActive(true);
				HudGui.InventoryItemInfo inventoryItemInfo = this._inventoryItemsInfoCache[itemId];
				UILabel componentInChildren = gameObject.GetComponentInChildren<UILabel>();
				if (componentInChildren)
				{
					componentInChildren.text = this.GotItemText(inventoryItemInfo, amount);
					UISprite componentInChildren2 = gameObject.GetComponentInChildren<UISprite>();
					if (componentInChildren2)
					{
						componentInChildren2.spriteName = inventoryItemInfo._icon.name;
					}
					this.Grid.Reposition();
					this._gotItemViewGOs[itemId] = this.SpawnTimedGameObject(itemId, amount, Time.time + 2f, gameObject, componentInChildren);
				}
				else
				{
					PoolManager.Pools["misc"].Despawn(gameObject.transform);
				}
			}
			else
			{
				this._gotItemViewGOs[itemId]._amount += amount;
				this._gotItemViewGOs[itemId]._endTime = Time.time + 2f;
				if (this._gotItemViewGOs[itemId]._label)
				{
					this._gotItemViewGOs[itemId]._label.text = this.GotItemText(this._inventoryItemsInfoCache[itemId], this._gotItemViewGOs[itemId]._amount);
				}
				this._gotItemViewGOs[itemId]._GO.SetActive(false);
				this.Grid.Reposition();
				this._gotItemViewGOs[itemId]._GO.SetActive(true);
				this.Grid.Reposition();
			}
		}
	}

	
	public void ShowFoundPassenger(int passengerId, string displayName)
	{
		if (!CoopPeerStarter.DedicatedHost && base.gameObject.activeSelf && this.Grid.gameObject.activeSelf && this.GuiCamC.enabled && !this._foundPassengerViewGOs.ContainsKey(passengerId) && this._foundPassengerViewGOs.Count < 10 && PoolManager.Pools.ContainsKey("misc") && PlayerPreferences.ShowHud)
		{
			base.enabled = true;
			GameObject gameObject = PoolManager.Pools["misc"].Spawn(this._foundPassengerView.transform, this._foundPassengerView.transform.position, this._foundPassengerView.transform.rotation).gameObject;
			gameObject.transform.parent = this.Grid.transform;
			gameObject.transform.localScale = this._foundPassengerView.transform.localScale;
			gameObject.SetActive(true);
			UILabel componentInChildren = gameObject.GetComponentInChildren<UILabel>();
			if (componentInChildren)
			{
				componentInChildren.text = UiTranslationDatabase.TranslateKey("FOUND_PASSENGER_SEAT_", "Found passenger, seat ", true) + displayName;
			}
			this.Grid.Reposition();
			this._foundPassengerViewGOs[passengerId] = this.SpawnTimedGameObject(passengerId, 0, Time.time + 10f, gameObject, componentInChildren);
		}
	}

	
	public void ShowTodoListMessage(string message)
	{
		if (!CoopPeerStarter.DedicatedHost && base.gameObject.activeSelf && this.Grid.gameObject.activeSelf && this.GuiCamC.enabled && !string.IsNullOrEmpty(message) && !this._todoListMessagesGOs.Any((KeyValuePair<int, HudGui.TimedGameObject> tlm) => tlm.Value._label.text.Equals(message)) && this._todoListMessagesGOs.Count < 10 && PoolManager.Pools.ContainsKey("misc") && PlayerPreferences.ShowHud)
		{
			base.enabled = true;
			GameObject gameObject = PoolManager.Pools["misc"].Spawn(this._todoListMessageView.transform, this._todoListMessageView.transform.position, this._todoListMessageView.transform.rotation).gameObject;
			gameObject.transform.parent = this.Grid.transform;
			gameObject.transform.localScale = this._todoListMessageView.transform.localScale;
			gameObject.SetActive(true);
			UILabel componentInChildren = gameObject.GetComponentInChildren<UILabel>();
			if (componentInChildren)
			{
				componentInChildren.text = message;
			}
			this.Grid.Reposition();
			int num = (this._todoListMessagesGOs.Keys.Count != 0) ? (this._todoListMessagesGOs.Keys.Max() + 1) : 0;
			this._todoListMessagesGOs[num] = this.SpawnTimedGameObject(num, 0, Time.time + 16f, gameObject, componentInChildren);
		}
	}

	
	private string GotItemText(HudGui.InventoryItemInfo iii, int amount)
	{
		string text;
		if (amount > 1)
		{
			text = UiTranslationDatabase.TranslateKey(iii._titlePluralTextTranslationKey, iii._titlePluralText, true);
			if (string.IsNullOrEmpty(text))
			{
				text = UiTranslationDatabase.TranslateKey(iii._titleTextTranslationKey, iii._titleText, true);
			}
			text = amount + " " + text;
		}
		else
		{
			text = UiTranslationDatabase.TranslateKey(iii._titleTextTranslationKey, iii._titleText, true);
		}
		if (iii._showCollectedItem)
		{
			text += UiTranslationDatabase.TranslateKey("_COLLECTED", " collected", true);
		}
		else
		{
			text += UiTranslationDatabase.TranslateKey("ADDED_TO_BACKPACK", " added to backpack", true);
		}
		return text;
	}

	
	public void ShowValidCraftingRecipes(IEnumerable<Receipe> receipes)
	{
		int i = 0;
		if (receipes != null)
		{
			foreach (Receipe receipe in receipes)
			{
				HudGui.InventoryItemInfo iii;
				if (receipe._type == Receipe.Types.Extension)
				{
					iii = ((!this._inventoryItemsInfoCache.ContainsKey(receipe._ingredients[0]._itemID)) ? null : this._inventoryItemsInfoCache[receipe._ingredients[0]._itemID]);
				}
				else
				{
					iii = ((!this._inventoryItemsInfoCache.ContainsKey(receipe._productItemID)) ? null : this._inventoryItemsInfoCache[receipe._productItemID]);
				}
				if (this._receipeProductViews[i].ShowReceipe(receipe, iii))
				{
					if (!this._receipeProductViews[i].gameObject.activeSelf)
					{
						this._receipeProductViews[i].gameObject.SetActive(true);
					}
					if (++i == this._receipeProductViews.Length)
					{
						break;
					}
				}
				else if (this._receipeProductViews[i].gameObject.activeSelf)
				{
					this._receipeProductViews[i].gameObject.SetActive(false);
				}
			}
		}
		while (i < this._receipeProductViews.Length)
		{
			this._receipeProductViews[i].gameObject.SetActive(false);
			i++;
		}
	}

	
	public void ShowUpgradesDistribution(int itemId, int addingItemid, int addingAmount)
	{
		weaponInfo heldWeaponInfo = LocalPlayer.Inventory.InventoryItemViewsCache[itemId][0]._heldWeaponInfo;
		if (heldWeaponInfo)
		{
			WeaponStatUpgrade[] weaponStatUpgradeForIngredient = LocalPlayer.Inventory._craftingCog.GetWeaponStatUpgradeForIngredient(addingItemid);
			if (weaponStatUpgradeForIngredient != null)
			{
				int maxUpgradesAmount = ItemDatabase.ItemById(itemId)._maxUpgradesAmount;
				this._upgradePreview._upgradesDistributionBacking.SetActive(true);
				for (int i = 0; i < this._upgradePreview._upgradesDistributionViews.Length; i++)
				{
					float num = (float)LocalPlayer.Inventory.GetAmountOfUpgrades(itemId, this._upgradePreview._upgradesDistributionViews[i]._itemId);
					if (this._upgradePreview._upgradesDistributionViews[i]._itemId == addingItemid)
					{
						num += (float)addingAmount;
					}
					if (num > 0f)
					{
						this._upgradePreview._upgradesDistributionViews[i]._icon.SetActive(true);
						this._upgradePreview._upgradesDistributionViews[i]._sprite.localScale = new Vector3(num / (float)maxUpgradesAmount, 0.5f, 1f);
					}
					else
					{
						this._upgradePreview._upgradesDistributionViews[i]._icon.SetActive(false);
						this._upgradePreview._upgradesDistributionViews[i]._sprite.localScale = Vector3.zero;
					}
				}
				float num2 = 0f;
				float num3 = 0f;
				float num4 = 0f;
				float num5 = 0f;
				for (int j = 0; j < weaponStatUpgradeForIngredient.Length; j++)
				{
					WeaponStatUpgrade.Types type = weaponStatUpgradeForIngredient[j]._type;
					switch (type)
					{
					case WeaponStatUpgrade.Types.weaponDamage:
						num3 += LocalPlayer.Inventory._craftingCog.GetUpgradeBonusAmount(itemId, addingItemid, weaponStatUpgradeForIngredient[j], addingAmount);
						break;
					default:
						if (type == WeaponStatUpgrade.Types.blockStaminaDrain)
						{
							num5 += LocalPlayer.Inventory._craftingCog.GetUpgradeBonusAmount(itemId, addingItemid, weaponStatUpgradeForIngredient[j], addingAmount);
						}
						break;
					case WeaponStatUpgrade.Types.weaponSpeed:
						num2 += LocalPlayer.Inventory._craftingCog.GetUpgradeBonusAmount(itemId, addingItemid, weaponStatUpgradeForIngredient[j], addingAmount);
						break;
					}
				}
				this._inventoryItemInfoView._speedBonusView._root.SetActive(true);
				float num6;
				float num7;
				if (num2 >= 0f)
				{
					num6 = heldWeaponInfo.weaponSpeed / 12f;
					num7 = num2 / 12f;
					this._upgradePreview._speedBonusPreview._amountRight.color = this._upgradePreview._speedBonusPreview._positive;
				}
				else
				{
					num6 = (heldWeaponInfo.weaponSpeed + num2) / 12f;
					num7 = Mathf.Abs(num2) / 12f;
					this._upgradePreview._speedBonusPreview._amountRight.color = this._upgradePreview._speedBonusPreview._negative;
				}
				this._upgradePreview._speedBonusPreview._amountLeft.fillAmount = num6;
				this._upgradePreview._speedBonusPreview._amountRight.fillAmount = num7;
				this._upgradePreview._speedBonusPreview._amountRight.transform.localPosition = new Vector3((num6 + num7) * (float)this._upgradePreview._speedBonusPreview._amountRight.width, 0f, 0f);
				float num8;
				float num9;
				if (num3 >= 0f)
				{
					num8 = heldWeaponInfo.weaponDamage / 12f;
					num9 = num3 / 12f;
					this._upgradePreview._damageBonusView._amountRight.color = this._upgradePreview._damageBonusView._positive;
				}
				else
				{
					num8 = (heldWeaponInfo.weaponDamage + num3) / 12f;
					num9 = Mathf.Abs(num3) / 12f;
					this._upgradePreview._damageBonusView._amountRight.color = this._upgradePreview._damageBonusView._negative;
				}
				this._upgradePreview._damageBonusView._amountLeft.fillAmount = num8;
				this._upgradePreview._damageBonusView._amountRight.fillAmount = num9;
				this._upgradePreview._damageBonusView._amountRight.transform.localPosition = new Vector3((num8 + num9) * (float)this._upgradePreview._damageBonusView._amountRight.width, 0f, 0f);
				float num10;
				float num11;
				if (num3 >= 0f)
				{
					num10 = 1f - heldWeaponInfo.blockDamagePercent / 1f;
					num11 = num4 / 1f;
					this._upgradePreview._blockBonusView._amountRight.color = this._upgradePreview._blockBonusView._positive;
				}
				else
				{
					num10 = 1f - (heldWeaponInfo.blockDamagePercent + num4) / 1f;
					num11 = Mathf.Abs(num4) / 1f;
					this._upgradePreview._blockBonusView._amountRight.color = this._upgradePreview._blockBonusView._negative;
				}
				this._upgradePreview._blockBonusView._amountLeft.fillAmount = num10;
				this._upgradePreview._blockBonusView._amountRight.fillAmount = num11;
				this._upgradePreview._blockBonusView._amountRight.transform.localPosition = new Vector3((num10 + num11) * (float)this._upgradePreview._blockBonusView._amountRight.width, 0f, 0f);
			}
			else
			{
				this._upgradePreview._upgradesDistributionBacking.SetActive(false);
			}
		}
		else
		{
			Debug.Log("Missing weaponInfo reference for: " + LocalPlayer.Inventory.InventoryItemViewsCache[itemId][0].ItemCache._name);
			this._upgradePreview._upgradesDistributionBacking.SetActive(false);
		}
	}

	
	public void HideUpgradesDistribution()
	{
		this._upgradePreview._upgradesDistributionBacking.SetActive(false);
	}

	
	public BuildMissionWidget GetBuildMissionWidget(int itemId)
	{
		if (this._buildMissionDisplayed.ContainsKey(itemId))
		{
			return this._buildMissionDisplayed[itemId];
		}
		if (PoolManager.Pools.ContainsKey("misc"))
		{
			Transform transform = PoolManager.Pools["misc"].Spawn(this.BuildMissionWidget.transform, this.Grid.transform);
			transform.transform.localPosition = Vector3.zero;
			transform.transform.localScale = this.BuildMissionWidget.transform.localScale;
			transform.SetSiblingIndex(this.BuildMissionWidget.transform.GetSiblingIndex() + 1);
			BuildMissionWidget component = transform.GetComponent<BuildMissionWidget>();
			component.Init(itemId);
			this._buildMissionQueue.Enqueue(component);
			this.SetBuildMissionWidgetForItem(itemId, component);
			return component;
		}
		return null;
	}

	
	public void SetBuildMissionWidgetForItem(int itemId, BuildMissionWidget bmw)
	{
		if (bmw)
		{
			this._buildMissionDisplayed[itemId] = bmw;
		}
		else if (this._buildMissionDisplayed.ContainsKey(itemId))
		{
			this._buildMissionDisplayed.Remove(itemId);
		}
	}

	
	
	public Dictionary<int, HudGui.InventoryItemInfo> InventoryItemsInfoCache
	{
		get
		{
			return this._inventoryItemsInfoCache;
		}
	}

	
	private const float MaxWeaponSpeed = 12f;

	
	private const float MaxWeaponDamage = 12f;

	
	private const float MaxWeaponBlockPercent = 1f;

	
	private const float MaxWeaponBlockStaminDrain = 26f;

	
	public GameObject PauseMenu;

	
	[Header("Survival Hud")]
	public GameObject StomachOutline;

	
	public UISprite Stomach;

	
	public UISprite StomachStarvation;

	
	public TweenColor StomachStarvationTween;

	
	public GameObject ThirstOutline;

	
	public UISprite Hydration;

	
	public UISprite ThirstDamageTimer;

	
	public TweenColor ThirstDamageTimerTween;

	
	public GameObject TimmyStomachOutline;

	
	public UISprite TimmyStomach;

	
	public UISprite HealthBar;

	
	public UISprite HealthBarTarget;

	
	public UISprite ArmorBar;

	
	public UISprite ColdArmorBar;

	
	public UISprite[] ArmorNibbles;

	
	public UISprite StaminaBar;

	
	public UISprite EnergyBar;

	
	public GameObject EnergyFlash;

	
	public GameObject HealthBarOutline;

	
	public GameObject StaminaBarOutline;

	
	public GameObject EnergyBarOutline;

	
	public UISprite AirReserve;

	
	public GameObject AirReserveOutline;

	
	public UISprite FuelReserve;

	
	public GameObject FuelReserveEmpty;

	
	[Space(15f)]
	public UIGrid Grid;

	
	public GameObject PlaneBodiesMsg;

	
	public UILabel PlaneBodiesLabel;

	
	[Header("Multiplayer")]
	public ChatBox Chatbox;

	
	public UILabel MpRespawnLabel;

	
	public UISprite MpRespawnMaxTimer;

	
	public PlayerOverlay PlayerOverlay;

	
	public GameObject MpPlayerListCamGo;

	
	public MpPlayerList MpPlayerList;

	
	public MpBannedPlayerList MpBannedPlayerList;

	
	[Header("Icons")]
	public GameObject MapIcon;

	
	public GameObject EyeIcon;

	
	public UISprite EyeIconFill1;

	
	public UISprite EyeIconFill2;

	
	public RoundStepProgressBarWidget RepairIcon;

	
	public RoundStepProgressBarWidget RepairLogIcon;

	
	public GameObject RepairActionIcon;

	
	public GameObject RepairMissingToolIcon;

	
	public UiFollowTarget SleepDelayIcon;

	
	public UILabel MpSleepLabel;

	
	public UILabel MpEndCrashLabel;

	
	public GameObject ToggleArrowBonusIcon;

	
	public GameObject RewindCamcorderIcon;

	
	[Header("Construction")]
	public ConstructionIcons WallConstructionIcons;

	
	public ConstructionIcons DefensiveWallConstructionIcons;

	
	public ConstructionIcons RoofConstructionIcons;

	
	public ConstructionIcons FoundationConstructionIcons;

	
	public GameObject PlaceIcon;

	
	public GameObject AddIcon;

	
	public GameObject AddBuildingIngredientIcon;

	
	public GameObject CantPlaceIcon;

	
	public GameObject BatchPlaceIcon;

	
	public GameObject RotateIcon;

	
	public GameObject SnapIcon;

	
	public GameObject DestroyIcon;

	
	public GameObject ToggleVariationIcon;

	
	public GameObject ToggleWallIcon;

	
	public GameObject ToggleDefensiveWallIcon;

	
	public GameObject ToggleDoor1Icon;

	
	public GameObject ToggleDoor2Icon;

	
	public GameObject ToggleWindowIcon;

	
	public GameObject ToggleGate1Icon;

	
	public GameObject ToggleGate2Icon;

	
	public UIGrid BuildingIngredientsGrid;

	
	public UiFollowTarget BuildingIngredientsFollow;

	
	public Color BuildingIngredientOwned;

	
	public Color BuildingIngredientMissing;

	
	public Color BuildingIngredientCompleted;

	
	public GameObject BuildMissionLogsParent;

	
	public BuildMissionWidget BuildMissionWidget;

	
	public HudGui.BuildingIngredient[] _buildingIngredients;

	
	[Header("Buildings")]
	public GameObject PlacePartIcon;

	
	public ItemListWidget FireWidget;

	
	public ItemListWidget FireStandWidget;

	
	public ItemListWidget DryingRackWidget;

	
	public ItemListWidget StewWidget;

	
	[NameFromEnumIndex(typeof(Garden.DirtPileTypes))]
	public ItemListWidget[] GardenWidgets;

	
	[NameFromEnumIndex(typeof(RackTypes))]
	public ItemListWidget[] RackWidgets;

	
	[NameFromEnumIndex(typeof(HolderTypes))]
	public ItemDualActionWidget[] HolderWidgets;

	
	[NameFromEnumIndex(typeof(OverlayIconTypes))]
	public ItemListWidget[] OverlayIconWidgets;

	
	public ItemListWidget MultiThrowerAddWidget;

	
	public ItemListWidget MultiThrowerTakeWidget;

	
	public ItemListWidget MultiSledAddWidget;

	
	[Header("Cameras")]
	public GameObject bookUICam;

	
	public GameObject bookTutorial;

	
	public GameObject GuiCam;

	
	public GameObject PedCam;

	
	public HudGui.LoadingScreen Loading;

	
	public Camera GuiCamC;

	
	public Camera PauseCamC;

	
	public Camera BookCam;

	
	public Camera ActionIconCams;

	
	public GameObject SaveSlotSelectionScreen;

	
	public GameObject EndgameScreen;

	
	public UILabel PauseMenuCoordsLabel;

	
	public GameObject ToggleableHud;

	
	[Header("Tutorials")]
	public GameObject Tut_Lighter;

	
	public GameObject Tut_Shelter;

	
	public GameObject Tut_OpenBag;

	
	public GameObject Tut_OpenBook;

	
	public GameObject Tut_DeathMP;

	
	public GameObject Tut_ReviveMP;

	
	public GameObject Tut_Health;

	
	public GameObject Tut_Energy;

	
	public GameObject Tut_Bloody;

	
	public GameObject Tut_Cold;

	
	public GameObject Tut_ColdDamage;

	
	public GameObject Tut_Hungry;

	
	public GameObject Tut_Starvation;

	
	public GameObject Tut_ThirstDamage;

	
	public GameObject Tut_Thirsty;

	
	public GameObject Tut_BookStage1;

	
	public GameObject Tut_BookStage2;

	
	public GameObject Tut_BookStage3;

	
	public GameObject Tut_Axe;

	
	public GameObject Tut_Crafting;

	
	public GameObject Tut_AnchorAccessibleBuildings;

	
	public GameObject Tut_StoryClue;

	
	public GameObject Tut_NoInventoryUnderwater;

	
	public GameObject Tut_MolotovTutorial;

	
	[Space(15f)]
	public GameObject DropButton;

	
	public GameObject DelayedActionIcon;

	
	[Header("Crafting")]
	public GameObject CraftingMessage;

	
	public UISprite CraftingReceipeProgress;

	
	public UISprite CraftingReceipeBacking;

	
	public GameObject ClickToCombineInfo;

	
	public GameObject ClickToRemoveInfo;

	
	public GameObject ClickToEquipInfo;

	
	public GameObject DisassembleInfo;

	
	public GameObject ManualUpgradingInfo;

	
	public GameObject TalkyWalkyInfo;

	
	public GameObject DropToRemoveAllInfo;

	
	public UITexture StorageFill;

	
	[Header("Story/tasks")]
	public GameObject[] StoryMapIcons;

	
	public GameObject[] StarIcons;

	
	public GameObject _foundPassengerView;

	
	public GameObject _todoListMessageView;

	
	[Header("Mods")]
	public HudGui.ModTimerView ModTimer;

	
	[Header("Gizmos")]
	public SupportPlacementGizmo SupportPlacementGizmo;

	
	public MultipointShapeGizmo MultipointShapeGizmo;

	
	public Transform UpgradePlacementGizmo;

	
	public LineRenderer RangedWeaponTrajectory;

	
	public GameObject RangedWeaponHitTarget;

	
	public GameObject RangedWeaponHitGroundTarget;

	
	public Transform MouseSprite;

	
	public UIBasicSprite MouseSpriteHand;

	
	public UIBasicSprite MouseSpriteInventoryInner;

	
	public UIBasicSprite MouseSpriteInventoryOuter;

	
	public UIBasicSprite MouseSpriteInventoryArrow;

	
	[Space(15f)]
	[NameFromProperty("_titleText", 0)]
	public List<HudGui.InventoryItemInfo> _inventoryItemsInfo;

	
	public HudGui.InventoryItemInfoView _inventoryItemInfoView;

	
	public HudGui.CarriedWeightInfoView _carriedWeightInfoView;

	
	public HudGui.QuickSelectTooltipView _quickSelectTooltipView;

	
	public HudGui.QuickSelectInfoView _quickSelectInfoView;

	
	public GameObject _cantCarryItemView;

	
	public GameObject _cantCarryItemWeightView;

	
	public GameObject _gotItemView;

	
	public ReceipeProductView[] _receipeProductViews;

	
	public GameObject _upgradesDistributionBacking;

	
	public HudGui.UpgradesDistributionView[] _upgradesDistributionViews;

	
	public HudGui.UpgradePreview _upgradePreview;

	
	public UiTranslationDatabase _translationDb;

	
	private Dictionary<int, HudGui.InventoryItemInfo> _inventoryItemsInfoCache;

	
	private Dictionary<int, HudGui.TimedGameObject> _cantCarryItemViewGOs;

	
	private Dictionary<int, HudGui.TimedGameObject> _gotItemViewGOs;

	
	private Dictionary<int, HudGui.TimedGameObject> _foundPassengerViewGOs;

	
	private Dictionary<int, HudGui.TimedGameObject> _todoListMessagesGOs;

	
	private Queue<HudGui.TimedGameObject> _tgoPool;

	
	private Queue<BuildMissionWidget> _buildMissionQueue;

	
	private Dictionary<int, BuildMissionWidget> _buildMissionDisplayed;

	
	private float _nextBuildMissionDisplay;

	
	[Header("Controls")]
	public bool _exportTranslationData;

	
	private int hudCameraOffCounter;

	
	private int _screenResolutionHash;

	
	[Serializable]
	public class InventoryItemInfo
	{
		
		[ItemIdPicker]
		public int _itemId;

		
		public Texture _icon;

		
		public string _titleText;

		
		public string _titlePluralText;

		
		[Multiline(1)]
		public string _effectText;

		
		[Multiline(1)]
		public string _descriptionText;

		
		public string _titleTextTranslationKey;

		
		public string _titlePluralTextTranslationKey;

		
		public string _effectTextTranslationKey;

		
		public string _descriptionTextTranslationKey;

		
		public HudGui.InventoryItemInfo.LeftClickCommands _leftClick;

		
		public HudGui.InventoryItemInfo.RightClickCommands _rightClick;

		
		public HudGui.InventoryItemInfo.AmountDisplay _amountDisplay;

		
		public bool _showGotItem;

		
		public bool _showCollectedItem;

		
		public bool _showCantCarryMoreItem;

		
		public bool _dontShowWeaponInfo;

		
		public enum LeftClickCommands
		{
			
			none,
			
			equip,
			
			play,
			
			drink,
			
			read,
			
			take,
			
			Charge_Flashlight,
			
			wear,
			
			eat,
			
			select,
			
			use
		}

		
		public enum RightClickCommands
		{
			
			none,
			
			combine,
			
			remove,
			
			unequip
		}

		
		public enum AmountDisplay
		{
			
			none,
			
			Amount,
			
			Pedometer,
			
			Battery,
			
			Air,
			
			WaterFill,
			
			Fuel,
			
			Ammo
		}
	}

	
	[Serializable]
	public class InventoryItemInfoView
	{
		
		
		
		public int ViewCounter { get; set; }

		
		
		
		public bool IsCraft { get; set; }

		
		[HideInInspector]
		public int _itemId;

		
		public GameObject _root;

		
		public UISprite _icon;

		
		public UILabel _title;

		
		public UILabel _effect;

		
		public UILabel _description;

		
		public UILabel _weight;

		
		public ItemAmountLabel _amountText;

		
		public UISprite _leftClickIcon;

		
		public UISprite _rightClickIcon;

		
		public UILabel _leftClickText;

		
		public UILabel _rightClickText;

		
		public UiTranslationLabel _titleUtl;

		
		public UiTranslationLabel _effectUtl;

		
		public UiTranslationLabel _descriptionUtl;

		
		public UiTranslationLabel _weightUtl;

		
		public UiTranslationLabel _leftClickTextUtl;

		
		public UiTranslationLabel _rightClickTextUtl;

		
		public UIGrid _upgradeCounterGrid;

		
		[NameFromProperty("_root", 0)]
		public List<HudGui.UpgradeCounterView> _upgradeCounterViews;

		
		[NameFromProperty("_stat", 0)]
		public List<HudGui.UsableUpgradeView> _usableUpgradeViews;

		
		public UIGrid _upgradeBonusGrid;

		
		public HudGui.UpgradeBonusView _speedBonusView;

		
		public HudGui.UpgradeBonusView _damageBonusView;

		
		public HudGui.UpgradeBonusView _blockView;
	}

	
	[Serializable]
	public class UpgradeCounterView
	{
		
		[ItemIdPicker]
		public int _itemId;

		
		public GameObject _root;

		
		public UILabel _label;
	}

	
	[Serializable]
	public class UsableUpgradeView
	{
		
		public WeaponStatUpgrade.Types _stat;

		
		public GameObject _root;
	}

	
	[Serializable]
	public class UpgradeBonusView
	{
		
		public GameObject _root;

		
		public UISprite _amount;
	}

	
	[Serializable]
	public class UpgradeBonusPreview
	{
		
		public GameObject _root;

		
		public UISprite _amountLeft;

		
		public UISprite _amountRight;

		
		public Color _base;

		
		public Color _negative;

		
		public Color _positive;
	}

	
	private class TimedGameObject
	{
		
		public int _itemId;

		
		public int _amount;

		
		public float _endTime;

		
		public GameObject _GO;

		
		public UILabel _label;
	}

	
	[Serializable]
	public class UpgradePreview
	{
		
		public GameObject _upgradesDistributionBacking;

		
		public HudGui.UpgradesDistributionView[] _upgradesDistributionViews;

		
		public HudGui.UpgradeBonusPreview _speedBonusPreview;

		
		public HudGui.UpgradeBonusPreview _damageBonusView;

		
		public HudGui.UpgradeBonusPreview _blockBonusView;
	}

	
	[Serializable]
	public class UpgradesDistributionView
	{
		
		[ItemIdPicker(Item.Types.CraftingMaterial)]
		public int _itemId;

		
		public Transform _sprite;

		
		public GameObject _icon;
	}

	
	[Serializable]
	public class CarriedWeightInfoView
	{
		
		public GameObject _root;

		
		public UILabel _weightOverMax;

		
		public UILabel _weightPercentage;
	}

	
	[Serializable]
	public class QuickSelectTooltipView
	{
		
		public GameObject _root;

		
		public UILabel[] _slots;
	}

	
	[Serializable]
	public class QuickSelectInfoView
	{
		
		public GameObject _root;

		
		public GameObject _selectButton;

		
		public GameObject _exitButton;
	}

	
	[Serializable]
	public class ModTimerView : IModTimerView
	{
		
		public void Toggle(bool onOff)
		{
			if (this._root.activeSelf != onOff)
			{
				this._root.SetActive(onOff);
			}
		}

		
		public void SetTitle(bool onOff, string text = null)
		{
			if (onOff)
			{
				this._title.text = text;
			}
			if (this._title.gameObject.activeSelf != onOff)
			{
				this._title.gameObject.SetActive(onOff);
			}
		}

		
		public void SetTimer(bool onOff, string text = null)
		{
			if (onOff)
			{
				this._timer.text = text;
			}
			if (this._timer.gameObject.activeSelf != onOff)
			{
				this._timer.gameObject.SetActive(onOff);
			}
		}

		
		public void SetSubtitle(bool onOff, string text = null)
		{
			if (onOff)
			{
				this._subtitle.text = text;
			}
			if (this._subtitle.gameObject.activeSelf != onOff)
			{
				this._subtitle.gameObject.SetActive(onOff);
			}
		}

		
		public GameObject _root;

		
		public UILabel _title;

		
		public UILabel _timer;

		
		public UILabel _subtitle;
	}

	
	[Serializable]
	public class LoadingScreen
	{
		
		public GameObject _cam;

		
		public GameObject _message;

		
		public TweenAlpha _backgroundTween;
	}

	
	[Serializable]
	public class BuildingIngredient
	{
		
		
		
		public int DisplayedPresent { get; set; }

		
		
		
		public int DisplayedNeeded { get; set; }

		
		[ItemIdPicker]
		public int _itemId;

		
		public GameObject _iconGo;

		
		public UISprite _iconSprite;

		
		public GUITexture _icon;

		
		public GUIText _text;

		
		public UILabel _label;

		
		public UISprite _border;
	}
}
