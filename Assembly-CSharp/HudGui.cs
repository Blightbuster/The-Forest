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
using TheForest.Player.Clothing;
using TheForest.UI;
using TheForest.UI.Anim;
using TheForest.UI.Crafting;
using TheForest.UI.Interfaces;
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
		if (ForestVR.Enabled)
		{
			this.Grid.cellHeight = 20f;
			this.GuiCamC.enabled = false;
			this.HealthHudWidget.enabled = false;
		}
	}

	private void Update()
	{
		this.CheckTimedGOs(this._cantCarryItemViewGOs);
		this.CheckTimedGOs(this._gotItemViewGOs);
		this.CheckTimedGOs(this._foundPassengerViewGOs);
		this.CheckTimedGOs(this._todoListMessagesGOs);
		if (BoltNetwork.isRunning && !CoopPeerStarter.DedicatedHost && !ForestVR.Enabled && TheForest.Utils.Input.GetButtonDown("PlayerList"))
		{
			this.ToggleMpPlayerList();
		}
		if (!ForestVR.Enabled && this._screenResolutionHash != this.GetScreenResolutionHash())
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
		this.CheckItemInfoViewCounter(false);
		if (this._nextItemInfoIIV && this._nextItemInfoDisplay < Time.realtimeSinceStartup)
		{
			this.ShowItemInfoView(this._nextItemInfoIIV, this._nextItemInfoRenderer, this._nextItemInfoIsCraft, this._nextItemInfoViewCounter);
			this._nextItemInfoIIV = null;
			this._nextItemInfoDisplay = float.MaxValue;
		}
		if (this._nextOutfitInfoIIV && this._nextOutfitInfoDisplay < Time.realtimeSinceStartup)
		{
			this.ShowClothingOutitInfoView(this._nextOutfitInfoIIV, this._nextOutfitInfoRenderer);
			this._nextOutfitInfoIIV = null;
			this._nextOutfitInfoDisplay = float.MaxValue;
		}
		if (this._nextQuickInfoDisplay < Time.realtimeSinceStartup)
		{
			this._nextQuickInfoDisplay = float.MaxValue;
			this.ShowQuickSelectInfo();
		}
		if (this._nextRecipeListDisplay < Time.realtimeSinceStartup)
		{
			this._nextRecipeListDisplay = float.MaxValue;
			this.ShowRecipeList();
		}
		if (ForestVR.Enabled)
		{
			this._fps = Mathf.Lerp(this._fps, 1f / Time.deltaTime, 0.05f);
			if (float.IsNaN(this._fps) || this._fps == 0f)
			{
				this._fps = 1f;
			}
			int num = (int)this._fps;
			this.FpsLabel.text = num.ToString();
		}
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
		this.InitPrefabPool(this._cantCarryClothingView.transform);
		this.InitPrefabPool(this._gotClothingView.transform);
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
		if (ForestVR.Prototype)
		{
			base.enabled = false;
			return;
		}
		if (!SteamDSConfig.isDedicatedServer)
		{
			bool flag = !CoopPeerStarter.DedicatedHost && LocalPlayer.Inventory && LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Pause && !Scene.Atmosphere.Sleeping && this._hudCameraOffCounter <= 0;
			bool flag2 = LocalPlayer.AnimControl && LocalPlayer.AnimControl.upsideDown;
			bool flag3 = LocalPlayer.Inventory && LocalPlayer.Inventory.CurrentView < PlayerInventory.PlayerViews.WakingUp;
			this.ActionIconCams.enabled = ((flag || flag2) && (PlayerPreferences.ShowHud || flag3));
			this.SetGUICamEnabled(flag);
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
			this._hudCameraOffCounter = Mathf.Max(0, this._hudCameraOffCounter - 1);
		}
		else
		{
			this._hudCameraOffCounter++;
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

	public void TogglePauseMenu(bool on)
	{
		LocalPlayer.Inventory.CurrentView = ((!on) ? PlayerInventory.PlayerViews.World : PlayerInventory.PlayerViews.Pause);
		TheForest.Utils.Input.SetState(InputState.Menu, on);
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
		if (ForestVR.Enabled && LocalPlayer.CurrentView != PlayerInventory.PlayerViews.Inventory)
		{
			if (on)
			{
				LocalPlayer.MainCam.cullingMask = LocalPlayer.vrPlayerControl.gameObject.GetComponent<VRTheatreController>().OptionsMenuLayerMask;
				base.StartCoroutine(this.RefreshHudCam());
				Scene.HudGui.ToggleableHud.SetActive(false);
			}
			else
			{
				LocalPlayer.MainCam.cullingMask = LocalPlayer.vrPlayerControl.gameObject.GetComponent<VRTheatreController>().DefaultLayerMask;
				Scene.HudGui.CheckHudState();
			}
		}
	}

	private IEnumerator RefreshHudCam()
	{
		yield return YieldPresets.WaitForEndOfFrame;
		yield return YieldPresets.WaitForEndOfFrame;
		Scene.HudGui.VRPauseMenuCam.enabled = false;
		Scene.HudGui.VRPauseMenuCam.enabled = true;
		Debug.Log("toggling vr pause menu");
		yield break;
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
			text = UiTranslationDatabase.TranslateKey(inventoryItemInfo._titlePluralTextTranslationKey, inventoryItemInfo._titlePluralText, true);
		}
		if (!plural || string.IsNullOrEmpty(text))
		{
			text = UiTranslationDatabase.TranslateKey(inventoryItemInfo._titleTextTranslationKey, inventoryItemInfo._titleText, true);
		}
		return text;
	}

	private string GetMeatItemName(HudGui.InventoryItemInfo iii, InventoryItemView view, bool plural, bool caps)
	{
		string text = null;
		if (view)
		{
			DecayingInventoryItemView.DecayStates state = ((DecayingInventoryItemView)view).DecayProperties._state;
			string text2 = null;
			string format;
			if (state < DecayingInventoryItemView.DecayStates.DriedFresh)
			{
				format = UiTranslationDatabase.TranslateKey("RAWMEAT_0", "Raw {0}", caps);
			}
			else
			{
				format = UiTranslationDatabase.TranslateKey("DRIEDMEAT_0", "Dried {0}", caps);
			}
			if (plural)
			{
				text2 = UiTranslationDatabase.TranslateKey(iii._titlePluralTextTranslationKey, iii._titlePluralText, caps);
			}
			if (!plural || string.IsNullOrEmpty(text2))
			{
				text2 = UiTranslationDatabase.TranslateKey(iii._titleTextTranslationKey, iii._titleText, caps);
			}
			text = StringEx.TryFormat(format, new object[]
			{
				text2
			});
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

	private string GetMeatDecay(HudGui.InventoryItemInfo iii, InventoryItemView view, bool plural, bool caps)
	{
		string result = string.Empty;
		if (view)
		{
			string text;
			switch (((DecayingInventoryItemView)view).DecayProperties._state)
			{
			case DecayingInventoryItemView.DecayStates.Edible:
			case DecayingInventoryItemView.DecayStates.DriedEdible:
				text = UiTranslationDatabase.TranslateKey("EDIBLE", "Edible", caps);
				goto IL_88;
			case DecayingInventoryItemView.DecayStates.Spoilt:
			case DecayingInventoryItemView.DecayStates.DriedSpoilt:
				text = UiTranslationDatabase.TranslateKey("SPOILT", "Spoilt", caps);
				goto IL_88;
			}
			text = UiTranslationDatabase.TranslateKey("FRESH", "Fresh", caps);
			IL_88:
			result = text;
		}
		return result;
	}

	private string GetLeftClickActionName(HudGui.InventoryItemInfo iii)
	{
		switch (iii._leftClick)
		{
		case HudGui.InventoryItemInfo.LeftClickCommands.equip:
			return UiTranslationDatabase.TranslateKey("EQUIP", "equip", true);
		case HudGui.InventoryItemInfo.LeftClickCommands.play:
			return UiTranslationDatabase.TranslateKey("PLAY", "play", true);
		case HudGui.InventoryItemInfo.LeftClickCommands.drink:
			return UiTranslationDatabase.TranslateKey("DRINK", "drink", true);
		case HudGui.InventoryItemInfo.LeftClickCommands.read:
			return UiTranslationDatabase.TranslateKey("READ", "read", true);
		case HudGui.InventoryItemInfo.LeftClickCommands.take:
			return UiTranslationDatabase.TranslateKey("TAKE", "take", true);
		case HudGui.InventoryItemInfo.LeftClickCommands.Charge_Flashlight:
			return UiTranslationDatabase.TranslateKey("CHARGE", "charge", true);
		case HudGui.InventoryItemInfo.LeftClickCommands.wear:
			return UiTranslationDatabase.TranslateKey("WEAR", "wear", true);
		case HudGui.InventoryItemInfo.LeftClickCommands.eat:
			return UiTranslationDatabase.TranslateKey("EAT", "eat", true);
		case HudGui.InventoryItemInfo.LeftClickCommands.select:
			return UiTranslationDatabase.TranslateKey("SELECT", "select", true);
		case HudGui.InventoryItemInfo.LeftClickCommands.use:
			return UiTranslationDatabase.TranslateKey("USE", "use", true);
		default:
			return "None";
		}
	}

	private string GetRightClickActionName(HudGui.InventoryItemInfo iii)
	{
		HudGui.InventoryItemInfo.RightClickCommands rightClick = iii._rightClick;
		if (rightClick == HudGui.InventoryItemInfo.RightClickCommands.combine)
		{
			return UiTranslationDatabase.TranslateKey("COMBINE", "combine", true);
		}
		if (rightClick == HudGui.InventoryItemInfo.RightClickCommands.remove)
		{
			return UiTranslationDatabase.TranslateKey("REMOVE", "remove", true);
		}
		if (rightClick != HudGui.InventoryItemInfo.RightClickCommands.unequip)
		{
			return "None";
		}
		return UiTranslationDatabase.TranslateKey("UNEQUIP", "unequip", true);
	}

	public Vector3 GetInventoryScreenPos(Renderer renderer, HudGui.AllowPositions allowPos = HudGui.AllowPositions.Right | HudGui.AllowPositions.Top | HudGui.AllowPositions.Bottom)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		Vector2 v = TheForest.Utils.Input.mousePosition;
		float num = (float)(Screen.width / 8);
		float num2 = (float)(Screen.height / 10);
		if (renderer)
		{
			Rect screenRectOf = LocalPlayer.InventoryCam.GetScreenRectOf(renderer);
			IScreenSizeRatio screenSizeRatio = renderer.GetComponent<IScreenSizeRatio>() ?? renderer.transform.parent.GetComponent<IScreenSizeRatio>();
			float num3 = 1f;
			if (screenSizeRatio != null)
			{
				num3 = screenSizeRatio.ScreenSizeRatio;
			}
			float num4 = screenRectOf.center.x / (float)Screen.width;
			float num5 = screenRectOf.center.y / (float)Screen.height;
			if ((allowPos & (HudGui.AllowPositions.Left | HudGui.AllowPositions.Right)) == (HudGui.AllowPositions)0 || ((allowPos & (HudGui.AllowPositions.Top | HudGui.AllowPositions.Bottom)) != (HudGui.AllowPositions)0 && Mathf.Abs(num4 - 0.5f) < Mathf.Abs(num5 - 0.5f) * (2.6f * (float)Screen.width / (float)Screen.height)))
			{
				if (num5 < 0.5f)
				{
					flag4 = true;
				}
				else
				{
					flag3 = true;
				}
			}
			else if (num4 < 0.5f || (allowPos & HudGui.AllowPositions.Left) == (HudGui.AllowPositions)0)
			{
				flag2 = true;
			}
			else
			{
				flag = true;
			}
			v.x = ((!flag) ? ((!flag2) ? screenRectOf.center.x : (screenRectOf.xMax + num * num3 * 0.5f)) : (screenRectOf.xMin - num * num3));
			v.y = ((!flag4) ? ((!flag3) ? screenRectOf.center.y : (screenRectOf.yMin - num2 * num3)) : (screenRectOf.yMax + num2 * num3));
			v.x = Mathf.Clamp(v.x, num, (float)Screen.width - num * 1.5f);
			v.y = Mathf.Clamp(v.y, num2 * 2f, (float)Screen.height - num2);
		}
		Vector3 position = LocalPlayer.InventoryCam.ScreenToViewportPoint(v);
		position.z = 3f;
		return this.ActionIconCams.ViewportToWorldPoint(position);
	}

	public void ShowItemInfoViewDelayed(InventoryItemView itemView, Renderer renderer, bool isCraft)
	{
		if (LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Inventory)
		{
			return;
		}
		if (this._inventoryItemInfoView._itemId == itemView._itemId && itemView.Properties.Match(this._inventoryItemInfoView._itemProperties) && this._inventoryItemInfoView.IsCraft == isCraft)
		{
			this._inventoryItemInfoView.ViewCounter++;
			if (isCraft)
			{
				if (!this.ClickToRemoveInfo.activeSelf)
				{
					this.ClickToRemoveInfo.SetActive(true);
				}
				if (this.ClickToEquipInfo.activeSelf != itemView._canEquipFromCraft)
				{
					this.ClickToEquipInfo.SetActive(itemView._canEquipFromCraft);
				}
			}
			else if (!this._inventoryItemInfoView._root.activeSelf && !isCraft)
			{
				this._inventoryItemInfoView._root.SetActive(true);
			}
		}
		else if (this._nextItemInfoIIV && this._nextItemInfoIIV._itemId == itemView._itemId && this._nextItemInfoIIV.Properties.Match(itemView.Properties) && this._nextItemInfoIsCraft == isCraft)
		{
			this._nextItemInfoViewCounter++;
		}
		else
		{
			this._nextItemInfoViewCounter = 1;
			this._nextItemInfoDisplay = Time.realtimeSinceStartup + 0.2f;
			this._nextItemInfoIIV = itemView;
			this._nextItemInfoRenderer = renderer;
			this._nextItemInfoIsCraft = isCraft;
		}
	}

	private void ShowItemInfoView(InventoryItemView itemView, Renderer renderer, bool isCraft, int viewCounter)
	{
		if (LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Inventory)
		{
			return;
		}
		if (!isCraft)
		{
			if (this._inventoryItemsInfoCache.ContainsKey(itemView._itemId))
			{
				this._inventoryItemInfoView.ViewCounter = viewCounter;
				this._inventoryItemInfoView._itemId = itemView._itemId;
				this._inventoryItemInfoView._itemProperties = itemView.Properties;
				this._inventoryItemInfoView.IsCraft = false;
				this._inventoryItemInfoView._root.transform.position = this.GetInventoryScreenPos(renderer, HudGui.AllowPositions.Right | HudGui.AllowPositions.Top | HudGui.AllowPositions.Bottom);
				HudGui.InventoryItemInfo inventoryItemInfo = this._inventoryItemsInfoCache[itemView._itemId];
				this._inventoryItemInfoView._icon.mainTexture = inventoryItemInfo._icon;
				HudGui.InventoryItemInfo.BackgroundSize backgroundSize = HudGui.InventoryItemInfo.BackgroundSize.Small;
				if (itemView is DecayingInventoryItemView)
				{
					this._inventoryItemInfoView._title.text = this.GetMeatItemName(inventoryItemInfo, itemView, false, true);
					this._inventoryItemInfoView._description.text = this.GetMeatDecay(inventoryItemInfo, itemView, false, true);
				}
				else
				{
					this._inventoryItemInfoView._title.text = UiTranslationDatabase.TranslateKey(inventoryItemInfo._titleTextTranslationKey, inventoryItemInfo._titleText, true);
					this._inventoryItemInfoView._description.text = ((!inventoryItemInfo.ShowDescription()) ? string.Empty : UiTranslationDatabase.TranslateKey(inventoryItemInfo._descriptionTextTranslationKey, inventoryItemInfo._descriptionText, true));
				}
				this._inventoryItemInfoView._effect.text = ((!inventoryItemInfo.ShowEffect()) ? string.Empty : UiTranslationDatabase.TranslateKey(inventoryItemInfo._effectTextTranslationKey, inventoryItemInfo._effectText, true));
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
					this._inventoryItemInfoView._description.text = UiTranslationDatabase.TranslateKey(inventoryItemInfo._descriptionTextTranslationKey, inventoryItemInfo._descriptionText, true).Replace("%", LocalPlayer.Stats.PedometerSteps.ToString());
					break;
				case HudGui.InventoryItemInfo.AmountDisplay.Battery:
					this._inventoryItemInfoView._amountText._label.enabled = false;
					this._inventoryItemInfoView._description.text = UiTranslationDatabase.TranslateKey(inventoryItemInfo._descriptionTextTranslationKey, inventoryItemInfo._descriptionText, true).Replace("%", Mathf.FloorToInt(LocalPlayer.Stats.BatteryCharge) + "%");
					break;
				case HudGui.InventoryItemInfo.AmountDisplay.Air:
					this._inventoryItemInfoView._amountText._label.enabled = false;
					this._inventoryItemInfoView._description.text = UiTranslationDatabase.TranslateKey(inventoryItemInfo._descriptionTextTranslationKey, inventoryItemInfo._descriptionText, true).Replace("%", Mathf.FloorToInt(LocalPlayer.Stats.AirBreathing.CurrentRebreatherAir) + "s");
					if (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.Chest, inventoryItemInfo._itemId))
					{
						UILabel description = this._inventoryItemInfoView._description;
						description.text += UiTranslationDatabase.TranslateKey("__EQUIPPED_", " (Equiped)", true);
					}
					break;
				case HudGui.InventoryItemInfo.AmountDisplay.WaterFill:
				{
					this._inventoryItemInfoView._amountText._label.enabled = false;
					WeaponStatUpgrade.Types activeBonus = itemView.ActiveBonus;
					string newValue;
					if (activeBonus != WeaponStatUpgrade.Types.CleanWater)
					{
						if (activeBonus != WeaponStatUpgrade.Types.DirtyWater)
						{
							newValue = UiTranslationDatabase.TranslateKey("EMPTY", "empty", true);
						}
						else
						{
							newValue = UiTranslationDatabase.TranslateKey("POLLUTED_WATER", "polluted water", true) + " (" + StringEx.TryFormat("{0:P0}", new object[]
							{
								itemView.Properties.ActiveBonusValue / 2f
							}) + ")";
						}
					}
					else
					{
						newValue = UiTranslationDatabase.TranslateKey("CLEAN_WATER", "clean water", true) + " (" + StringEx.TryFormat("{0:P0}", new object[]
						{
							itemView.Properties.ActiveBonusValue / 2f
						}) + ")";
					}
					this._inventoryItemInfoView._effect.text = UiTranslationDatabase.TranslateKey(inventoryItemInfo._effectTextTranslationKey, inventoryItemInfo._effectText, true).Replace("%", newValue);
					this._inventoryItemInfoView._effect.gameObject.SetActive(true);
					break;
				}
				case HudGui.InventoryItemInfo.AmountDisplay.Fuel:
					this._inventoryItemInfoView._amountText._label.enabled = false;
					this._inventoryItemInfoView._description.text = UiTranslationDatabase.TranslateKey(inventoryItemInfo._descriptionTextTranslationKey, inventoryItemInfo._descriptionText, true).Replace("%", StringEx.TryFormat("{0:P0}", new object[]
					{
						LocalPlayer.Stats.Fuel.CurrentFuel / LocalPlayer.Stats.Fuel.MaxFuelCapacity
					}));
					break;
				case HudGui.InventoryItemInfo.AmountDisplay.Ammo:
					this._inventoryItemInfoView._amountText._label.enabled = true;
					this._inventoryItemInfoView._amountText._itemId = itemView.ItemCache._ammoItemId;
					break;
				}
				this._inventoryItemInfoView._weight.enabled = false;
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
						backgroundSize = HudGui.InventoryItemInfo.BackgroundSize.Large;
					}
					else
					{
						this._inventoryItemInfoView._upgradeBonusGrid.gameObject.SetActive(false);
					}
				}
				bool canUseWithPrimary = itemView.CanUseWithPrimary;
				if (inventoryItemInfo._leftClick == HudGui.InventoryItemInfo.LeftClickCommands.none || (!itemView.ItemCache.MatchType(Item.Types.Equipment) && itemView.ItemCache.MatchType(Item.Types.Edible) && !canUseWithPrimary))
				{
					this._inventoryItemInfoView._leftClickIcon.SetActive(false);
				}
				else
				{
					if (canUseWithPrimary && (itemView.ActiveBonus == WeaponStatUpgrade.Types.CleanWater || itemView.ActiveBonus == WeaponStatUpgrade.Types.DirtyWater))
					{
						this._inventoryItemInfoView._leftClickText.text = UiTranslationDatabase.TranslateKey("DRINK", "Drink", true);
					}
					else
					{
						this._inventoryItemInfoView._leftClickText.text = this.GetLeftClickActionName(inventoryItemInfo);
					}
					this._inventoryItemInfoView._leftClickIcon.SetActive(true);
				}
				if (LocalPlayer.Inventory.CurrentStorage == LocalPlayer.Inventory._craftingCog)
				{
					if (itemView._canDropFromInventory)
					{
						this._inventoryItemInfoView._rightClickIcon.SetActive(true);
						this._inventoryItemInfoView._rightClickText.text = UiTranslationDatabase.TranslateKey("DROP", "Drop", true);
					}
					else if (itemView.CanUseWithSecondary)
					{
						this._inventoryItemInfoView._rightClickIcon.SetActive(true);
						if (itemView.ActiveBonus == WeaponStatUpgrade.Types.CleanWater || itemView.ActiveBonus == WeaponStatUpgrade.Types.DirtyWater)
						{
							this._inventoryItemInfoView._rightClickText.text = UiTranslationDatabase.TranslateKey("DRINK", "Drink", true);
						}
						else if (itemView.ActiveBonus == WeaponStatUpgrade.Types.DriedFood)
						{
							this._inventoryItemInfoView._rightClickText.text = UiTranslationDatabase.TranslateKey("EAT", "Eat", true);
						}
						else
						{
							this._inventoryItemInfoView._rightClickText.text = UiTranslationDatabase.TranslateKey("USE", "Use", true);
						}
					}
					else if (itemView.CanBeHotkeyed)
					{
						this._inventoryItemInfoView._rightClickIcon.SetActive(true);
						this._inventoryItemInfoView._rightClickText.text = UiTranslationDatabase.TranslateKey("COMBINE", "combine", true);
					}
					else if (inventoryItemInfo._rightClick == HudGui.InventoryItemInfo.RightClickCommands.none)
					{
						this._inventoryItemInfoView._rightClickIcon.SetActive(false);
					}
					else
					{
						this._inventoryItemInfoView._rightClickIcon.SetActive(true);
						this._inventoryItemInfoView._rightClickText.text = this.GetRightClickActionName(inventoryItemInfo);
					}
				}
				else if (itemView.CanBeStored)
				{
					this._inventoryItemInfoView._rightClickIcon.SetActive(true);
					this._inventoryItemInfoView._rightClickText.text = UiTranslationDatabase.TranslateKey("STORE", "Store", true);
				}
				else
				{
					this._inventoryItemInfoView._rightClickIcon.SetActive(false);
				}
				bool flag = this._inventoryItemInfoView._leftClickIcon.activeSelf || this._inventoryItemInfoView._rightClickIcon.activeSelf;
				bool flag2 = !string.IsNullOrEmpty(this._inventoryItemInfoView._description.text);
				bool flag3 = !string.IsNullOrEmpty(this._inventoryItemInfoView._effect.text);
				if (flag && (flag2 || flag3))
				{
					int num = (!flag2) ? 1 : Mathf.CeilToInt((float)this._inventoryItemInfoView._description.height / 18f);
					if (flag3)
					{
						num += Mathf.CeilToInt((float)this._inventoryItemInfoView._effect.height / 18f);
					}
					if (num > 2)
					{
						backgroundSize = HudGui.InventoryItemInfo.BackgroundSize.Large;
					}
					else if (backgroundSize < HudGui.InventoryItemInfo.BackgroundSize.Medium)
					{
						backgroundSize = HudGui.InventoryItemInfo.BackgroundSize.Medium;
					}
				}
				this._inventoryItemInfoView._background.height = this._inventoryItemInfoView._backgroundSizes[(int)backgroundSize];
				this._inventoryItemInfoView._background.width = Mathf.Max(460, this._inventoryItemInfoView._title.width + 30 + 130);
				Vector3 localPosition = this._inventoryItemInfoView._background.transform.localPosition;
				localPosition.x = (float)(38 + Mathf.Max(this._inventoryItemInfoView._background.width - 460, 0) / 2);
				this._inventoryItemInfoView._background.transform.localPosition = localPosition;
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

	public void HideItemInfoView(InventoryItemView itemView, bool isCraft)
	{
		if (this._inventoryItemInfoView._itemId == itemView._itemId && (isCraft || itemView.Properties.Match(this._inventoryItemInfoView._itemProperties)) && this._inventoryItemInfoView.IsCraft == isCraft)
		{
			this._inventoryItemInfoView.ViewCounter--;
		}
		else if (this._nextItemInfoIIV && this._nextItemInfoIIV._itemId == itemView._itemId && (isCraft || this._nextItemInfoIIV.Properties.Match(itemView.Properties)) && this._nextItemInfoIsCraft == isCraft)
		{
			this._nextItemInfoViewCounter--;
		}
	}

	public void CheckItemInfoViewCounter(bool forceReset = false)
	{
		if (this._inventoryItemInfoView.ViewCounter <= 0 && this._inventoryItemInfoView._itemId != 0)
		{
			this.ClickToRemoveInfo.SetActive(false);
			this.ClickToEquipInfo.SetActive(false);
			if (this._inventoryItemInfoView._root.activeSelf)
			{
				this._inventoryItemInfoView._root.SetActive(false);
			}
			if (LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Inventory || forceReset)
			{
				this._inventoryItemInfoView._itemId = 0;
				this._inventoryItemInfoView._itemProperties = ItemProperties.Any;
			}
			this._inventoryItemInfoView.ViewCounter = 0;
		}
		if (this._nextItemInfoViewCounter <= 0 && this._nextItemInfoIIV)
		{
			this._nextItemInfoIIV = null;
			this._nextItemInfoViewCounter = 0;
		}
	}

	public void ShowCarriedWeightInfo(Vector3 viewportPos)
	{
	}

	public void HideCarriedWeightInfo()
	{
		this._carriedWeightInfoView._root.SetActive(false);
	}

	public void ShowQuickSelectInfoDelayed(Renderer renderer)
	{
		this._quickSelectTooltipView._root.transform.position = this.GetInventoryScreenPos(renderer, HudGui.AllowPositions.Right | HudGui.AllowPositions.Top | HudGui.AllowPositions.Bottom);
		this._nextQuickInfoDisplay = Time.realtimeSinceStartup + 0.2f;
	}

	private void ShowQuickSelectInfo()
	{
		if (LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Inventory)
		{
			return;
		}
		this._quickSelectTooltipView._root.SetActive(true);
		for (int i = 0; i < this._quickSelectTooltipView._slots.Length; i++)
		{
			int num = LocalPlayer.Inventory.QuickSelectItemIds[i];
			if (num > 0)
			{
				if (this._inventoryItemsInfoCache.ContainsKey(num))
				{
					HudGui.InventoryItemInfo inventoryItemInfo = this._inventoryItemsInfoCache[num];
					if (inventoryItemInfo != null)
					{
						this._quickSelectTooltipView._slots[i].text = UiTranslationDatabase.TranslateKey(inventoryItemInfo._titleTextTranslationKey, inventoryItemInfo._titleText, true);
						this._quickSelectTooltipView._slots[i].enabled = true;
					}
				}
			}
			else
			{
				this._quickSelectTooltipView._slots[i].enabled = false;
			}
		}
	}

	public void HideQuickSelectInfo()
	{
		this._quickSelectTooltipView._root.SetActive(false);
		this._nextQuickInfoDisplay = float.MaxValue;
	}

	public void ShowQuickSelectCombineInfo()
	{
		if (LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Inventory)
		{
			return;
		}
		for (int i = 0; i < this._quickSelectInfoView._slots.Length; i++)
		{
			int num = LocalPlayer.Inventory.QuickSelectItemIds[i];
			if (num > 0)
			{
				if (this._inventoryItemsInfoCache.ContainsKey(num))
				{
					HudGui.InventoryItemInfo inventoryItemInfo = this._inventoryItemsInfoCache[num];
					if (inventoryItemInfo != null)
					{
						this._quickSelectInfoView._slots[i].text = UiTranslationDatabase.TranslateKey(inventoryItemInfo._titleTextTranslationKey, inventoryItemInfo._titleText, true);
						this._quickSelectInfoView._slots[i].enabled = true;
					}
				}
			}
			else
			{
				this._quickSelectInfoView._slots[i].enabled = false;
			}
		}
		if (!this._quickSelectInfoView._root.activeSelf)
		{
			this._quickSelectInfoView._root.SetActive(true);
		}
	}

	public void HideQuickSelectCombineInfo()
	{
		this._quickSelectInfoView._root.SetActive(false);
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
		if (!CoopPeerStarter.DedicatedHost && base.gameObject.activeSelf && this.Grid.gameObject.activeSelf && this.GuiCamC.enabled && this._inventoryItemsInfoCache.ContainsKey(itemId) && this._inventoryItemsInfoCache[itemId]._showCantCarryMoreItem && this.GuiCam.activeSelf && this._cantCarryItemViewGOs.Count < 5 && PoolManager.Pools.ContainsKey("misc") && PlayerPreferences.ShowHud)
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
					componentInChildren.text = this.GetMeatItemName(this._inventoryItemsInfoCache[itemId], null, true, true);
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

	public void ShowClothingOutitInfoViewDelayed(ClothingInventoryView itemView, Renderer renderer)
	{
		if (LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Inventory)
		{
			return;
		}
		this._nextOutfitInfoIIV = itemView;
		this._nextOutfitInfoRenderer = renderer;
		this._nextOutfitInfoDisplay = Time.realtimeSinceStartup + 0.2f;
	}

	private void ShowClothingOutitInfoView(ClothingInventoryView itemView, Renderer renderer)
	{
		if (LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Inventory)
		{
			return;
		}
		this._clothingItemInfoView._root.transform.position = this.GetInventoryScreenPos(renderer, HudGui.AllowPositions.Right | HudGui.AllowPositions.Top | HudGui.AllowPositions.Bottom);
		this._clothingItemInfoView._outfitId = itemView._outfitId;
		List<int> list = LocalPlayer.Clothing.AvailableClothingOutfits[itemView._outfitId];
		string text = string.Empty;
		for (int i = 0; i < list.Count; i++)
		{
			ClothingItem clothingItem = ClothingItemDatabase.ClothingItemById(list[i]);
			text += UiTranslationDatabase.TranslateKey(clothingItem._translationKey, clothingItem._displayName.ToUpper(), true);
			if (i + 2 < list.Count)
			{
				text += ", ";
			}
			else if (i + 1 < list.Count)
			{
				text += UiTranslationDatabase.TranslateKey("_AND_", " AND ", true);
			}
		}
		this._clothingItemInfoView._content.text = text;
		if (!this._clothingItemInfoView._root.activeSelf)
		{
			this._clothingItemInfoView._root.SetActive(true);
		}
	}

	public void HideClothingOutitInfoView(int itemId)
	{
		if (this._clothingItemInfoView._outfitId == itemId)
		{
			if (this._clothingItemInfoView._root.activeSelf)
			{
				this._clothingItemInfoView._root.SetActive(false);
			}
			this._clothingItemInfoView._outfitId = 0;
		}
		if (this._nextOutfitInfoIIV && this._nextOutfitInfoIIV._outfitId == itemId)
		{
			this._nextOutfitInfoIIV = null;
		}
	}

	public void ToggleGotClothingOutfitHud()
	{
		if (!CoopPeerStarter.DedicatedHost && base.gameObject.activeSelf && this.Grid.gameObject.activeSelf && this.GuiCamC.enabled && this.GuiCam.activeSelf && this._gotItemViewGOs.Count < 5 && PoolManager.Pools.ContainsKey("misc") && PlayerPreferences.ShowHud)
		{
			if (!this._gotItemViewGOs.ContainsKey(-1))
			{
				base.enabled = true;
				GameObject gameObject = PoolManager.Pools["misc"].Spawn(this._gotClothingView.transform, this._gotClothingView.transform.position, this._gotClothingView.transform.rotation).gameObject;
				gameObject.transform.parent = this.Grid.transform;
				gameObject.transform.localScale = this._gotClothingView.transform.localScale;
				gameObject.transform.localScale = this.BuildMissionWidget.transform.localScale;
				gameObject.transform.SetSiblingIndex(this._gotClothingView.transform.GetSiblingIndex() + 1);
				gameObject.SetActive(true);
				this.Grid.Reposition();
				this._gotItemViewGOs[-1] = this.SpawnTimedGameObject(-1, 1, Time.time + 2f, gameObject, gameObject.GetComponentInChildren<UILabel>());
			}
			else
			{
				this._gotItemViewGOs[-1]._amount++;
				this._gotItemViewGOs[-1]._endTime = Time.time + 2f;
				this._gotItemViewGOs[-1]._GO.SetActive(false);
				this.Grid.Reposition();
				this._gotItemViewGOs[-1]._GO.SetActive(true);
				this.Grid.Reposition();
			}
		}
	}

	public void ToggleFullClothingOutfitCapacityHud()
	{
		if (!CoopPeerStarter.DedicatedHost && base.gameObject.activeSelf && this.Grid.gameObject.activeSelf && this.GuiCamC.enabled && this.GuiCam.activeSelf && this._cantCarryItemViewGOs.Count < 5 && PoolManager.Pools.ContainsKey("misc") && PlayerPreferences.ShowHud)
		{
			if (!this._cantCarryItemViewGOs.ContainsKey(-1))
			{
				base.enabled = true;
				GameObject gameObject = PoolManager.Pools["misc"].Spawn(this._cantCarryClothingView.transform, this._cantCarryClothingView.transform.position, this._cantCarryClothingView.transform.rotation).gameObject;
				gameObject.transform.parent = this.Grid.transform;
				gameObject.transform.localScale = this._cantCarryClothingView.transform.localScale;
				gameObject.transform.SetSiblingIndex(this._gotItemView.transform.GetSiblingIndex() + 1);
				gameObject.SetActive(true);
				this.Grid.Reposition();
				this._cantCarryItemViewGOs[-1] = this.SpawnTimedGameObject(-1, 0, Time.time + 5f, gameObject, null);
			}
			else
			{
				this._cantCarryItemViewGOs[-1]._endTime = Time.time + 5f;
				this._cantCarryItemViewGOs[-1]._GO.SetActive(false);
				this.Grid.Reposition();
				this._cantCarryItemViewGOs[-1]._GO.SetActive(true);
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
		if (!CoopPeerStarter.DedicatedHost && base.gameObject.activeSelf && this.Grid.gameObject.activeSelf && this.GuiCamC.enabled && this._inventoryItemsInfoCache.ContainsKey(itemId) && this._inventoryItemsInfoCache[itemId]._showGotItem && this.GuiCam.activeSelf && this._gotItemViewGOs.Count < 5 && PoolManager.Pools.ContainsKey("misc") && PlayerPreferences.ShowHud)
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
					UITexture componentInChildren2 = gameObject.GetComponentInChildren<UITexture>();
					if (componentInChildren2)
					{
						componentInChildren2.mainTexture = inventoryItemInfo._icon;
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
		if (!CoopPeerStarter.DedicatedHost && base.gameObject.activeSelf && this.Grid.gameObject.activeSelf && this.GuiCamC.enabled && !this._foundPassengerViewGOs.ContainsKey(passengerId) && this._foundPassengerViewGOs.Count < 5 && PoolManager.Pools.ContainsKey("misc") && PlayerPreferences.ShowHud)
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
		if (!CoopPeerStarter.DedicatedHost && base.gameObject.activeSelf && this.Grid.gameObject.activeSelf && this.GuiCamC.enabled && !string.IsNullOrEmpty(message) && !this._todoListMessagesGOs.Any((KeyValuePair<int, HudGui.TimedGameObject> tlm) => tlm.Value._label.text.Equals(message)) && this._todoListMessagesGOs.Count < 5 && PoolManager.Pools.ContainsKey("misc") && PlayerPreferences.ShowHud)
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
			string text2 = UiTranslationDatabase.TranslateKey("_COLLECTED", " collected", true);
			text += ((!text2.StartsWith(" ")) ? (' ' + text2) : text2);
		}
		else
		{
			text += UiTranslationDatabase.TranslateKey("ADDED_TO_BACKPACK", " added to backpack", true);
		}
		return text;
	}

	public HudGui.InventoryItemInfo GetItemInfo(int itemId)
	{
		return (!this._inventoryItemsInfoCache.ContainsKey(itemId)) ? null : this._inventoryItemsInfoCache[itemId];
	}

	public void HideCraftingRecipes()
	{
	}

	public void ShowValidCraftingRecipes(IEnumerable<Receipe> receipes)
	{
		int i = 0;
		if (receipes != null)
		{
			foreach (Receipe receipe in receipes)
			{
				HudGui.InventoryItemInfo itemInfo = this.GetItemInfo((receipe._type != Receipe.Types.Extension) ? receipe._productItemID : receipe._ingredients[0]._itemID);
				if (this._receipeProductViews[i].ShowReceipe(receipe, itemInfo))
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

	public void ShowRecipeListDelayed()
	{
		this._nextRecipeListDisplay = Time.realtimeSinceStartup + 0.2f;
	}

	private void ShowRecipeList()
	{
		if (!this._receipeList.activeSelf)
		{
			this._receipeList.SetActive(true);
		}
		this._receipeList.GetComponent<UIGrid>().Reposition();
		base.StartCoroutine(this.UpdateRecipeListSize());
	}

	public void HideRecipeList()
	{
		if (this._receipeList.activeSelf)
		{
			this._receipeList.SetActive(false);
		}
		this._nextRecipeListDisplay = float.MaxValue;
	}

	public IEnumerator UpdateRecipeListSize()
	{
		yield return null;
		Bounds b = default(Bounds);
		bool set = false;
		for (int i = 0; i < this._receipeProductViews.Length; i++)
		{
			if (!this._receipeProductViews[i].gameObject.activeSelf)
			{
				break;
			}
			if (set)
			{
				b.Encapsulate(this._receipeProductViews[i]._title.CalculateBounds(this._receipeList.transform));
			}
			else
			{
				b = this._receipeProductViews[i]._title.CalculateBounds(this._receipeList.transform);
				set = true;
			}
			for (int j = this._receipeProductViews[i]._ingredientTextures.Length - 1; j > 0; j--)
			{
				if (!this._receipeProductViews[i]._ingredientTextures[j - 1].gameObject.activeSelf)
				{
					b.Encapsulate(this._receipeProductViews[i]._ingredientTextures[j].CalculateBounds(this._receipeList.transform));
					break;
				}
			}
		}
		this._receipeListSize.SetRect(b.min.x, b.min.y, b.max.x - b.min.x + 70f, b.max.y - b.min.y);
		yield break;
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
					if (type != WeaponStatUpgrade.Types.weaponSpeed)
					{
						if (type != WeaponStatUpgrade.Types.weaponDamage)
						{
							if (type == WeaponStatUpgrade.Types.blockStaminaDrain)
							{
								num5 += LocalPlayer.Inventory._craftingCog.GetUpgradeBonusAmount(itemId, addingItemid, weaponStatUpgradeForIngredient[j], addingAmount);
							}
						}
						else
						{
							num3 += LocalPlayer.Inventory._craftingCog.GetUpgradeBonusAmount(itemId, addingItemid, weaponStatUpgradeForIngredient[j], addingAmount);
						}
					}
					else
					{
						num2 += LocalPlayer.Inventory._craftingCog.GetUpgradeBonusAmount(itemId, addingItemid, weaponStatUpgradeForIngredient[j], addingAmount);
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
			transform.transform.localRotation = Quaternion.identity;
			transform.SetSiblingIndex(this.BuildMissionWidget.transform.GetSiblingIndex() + 1);
			BuildMissionWidget component = transform.GetComponent<BuildMissionWidget>();
			component.Init(itemId);
			this._buildMissionQueue.Enqueue(component);
			this.SetBuildMissionWidgetForItem(itemId, component);
			return component;
		}
		return null;
	}

	public void SetGUICamEnabled(bool enabledValue)
	{
		if (!ForestVR.Enabled)
		{
			this.GuiCamC.enabled = enabledValue;
		}
		else
		{
			this.VRGuiCamC.enabled = enabledValue;
			this.ActionIconCams.enabled = false;
		}
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

	public UISprite ArmorBar;

	public UISprite ColdArmorBar;

	public UISprite[] ArmorNibbles;

	public UISprite HealthBar;

	public UISprite HealthBarTarget;

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

	public UiFollowTarget EnterCavesIcon;

	public GameObject LoadingCavesInfo;

	public UISprite LoadingCavesFill;

	[Space(15f)]
	public GameObject DropButton;

	public GameObject PlaceArtifactButton;

	public GameObject DelayedActionIcon;

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

	[Header("Traps")]
	public UiFollowTarget TrapReArmIngredientsFollow;

	public HudGui.BuildingIngredient TrapReArmIngredients;

	[Header("Cameras")]
	public GameObject bookUICam;

	public GameObject GuiCam;

	public GameObject PedCam;

	public HudGui.LoadingScreen Loading;

	public Camera GuiCamC;

	public Camera VRGuiCamC;

	public Camera PauseCamC;

	public Camera BookCam;

	public Camera ActionIconCams;

	public GameObject SaveSlotSelectionScreen;

	public GameObject EndgameScreen;

	public UILabel PauseMenuCoordsLabel;

	public GameObject ToggleableHud;

	public Camera VRTutorialCam;

	public Camera VRIconCam;

	public Camera VRPauseMenuCam;

	[Header("Tutorials")]
	public GameObject Tut_Lighter;

	public GameObject Tut_Shelter;

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

	public GameObject Tut_StoryClue;

	public GameObject Tut_NoInventoryUnderwater;

	public GameObject Tut_MolotovTutorial;

	public GameObject Tut_NewBuildingsAvailable;

	public GameObject Tut_Sledding;

	public EnvelopContent VR_Tut_Backing;

	[Header("Crafting")]
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

	public GameObject _cantCarryClothingView;

	public GameObject _gotClothingView;

	public HudGui.ClothingItemTooltipView _clothingItemInfoView;

	public GameObject _receipeList;

	public UIWidget _receipeListSize;

	public ReceipeProductView[] _receipeProductViews;

	public GameObject _upgradesDistributionBacking;

	public HudGui.UpgradesDistributionView[] _upgradesDistributionViews;

	public HudGui.UpgradePreview _upgradePreview;

	public UiTranslationDatabase _translationDb;

	[Header("VR")]
	public UILabel FpsLabel;

	public UIWidget HealthHudWidget;

	private Dictionary<int, HudGui.InventoryItemInfo> _inventoryItemsInfoCache;

	private Dictionary<int, HudGui.TimedGameObject> _cantCarryItemViewGOs;

	private Dictionary<int, HudGui.TimedGameObject> _gotItemViewGOs;

	private Dictionary<int, HudGui.TimedGameObject> _foundPassengerViewGOs;

	private Dictionary<int, HudGui.TimedGameObject> _todoListMessagesGOs;

	private Queue<HudGui.TimedGameObject> _tgoPool;

	private Queue<BuildMissionWidget> _buildMissionQueue;

	private Dictionary<int, BuildMissionWidget> _buildMissionDisplayed;

	private float _nextBuildMissionDisplay;

	private int _hudCameraOffCounter;

	private float _nextItemInfoDisplay = float.MaxValue;

	private InventoryItemView _nextItemInfoIIV;

	private Renderer _nextItemInfoRenderer;

	private bool _nextItemInfoIsCraft;

	private int _nextItemInfoViewCounter;

	private float _nextOutfitInfoDisplay = float.MaxValue;

	private ClothingInventoryView _nextOutfitInfoIIV;

	private Renderer _nextOutfitInfoRenderer;

	private float _nextQuickInfoDisplay = float.MaxValue;

	private float _nextRecipeListDisplay = float.MaxValue;

	private float _fps;

	[Header("Controls")]
	public bool _exportTranslationData;

	private const float MaxWeaponSpeed = 12f;

	private const float MaxWeaponDamage = 12f;

	private const float MaxWeaponBlockPercent = 1f;

	private const float MaxWeaponBlockStaminDrain = 26f;

	private int _screenResolutionHash;

	[Flags]
	public enum AllowPositions
	{
		Left = 1,
		Right = 2,
		Top = 4,
		Bottom = 8
	}

	[Serializable]
	public class InventoryItemInfo
	{
		public bool ShowEffect()
		{
			HudGui.InventoryItemInfo.AllowedDifficulties allowedDifficulties = (HudGui.InventoryItemInfo.AllowedDifficulties)(1 << (int)GameSetup.Difficulty);
			return (this._effectShowFor & allowedDifficulties) != HudGui.InventoryItemInfo.AllowedDifficulties.None;
		}

		public bool ShowDescription()
		{
			HudGui.InventoryItemInfo.AllowedDifficulties allowedDifficulties = (HudGui.InventoryItemInfo.AllowedDifficulties)(1 << (int)GameSetup.Difficulty);
			return (this._descriptionShowFor & allowedDifficulties) != HudGui.InventoryItemInfo.AllowedDifficulties.None;
		}

		[ItemIdPicker]
		public int _itemId;

		public Texture _icon;

		public string _titleText;

		public string _titlePluralText;

		[Multiline(1)]
		public string _effectText;

		public HudGui.InventoryItemInfo.AllowedDifficulties _effectShowFor = HudGui.InventoryItemInfo.AllowedDifficulties.All;

		[Multiline(1)]
		public string _descriptionText;

		public HudGui.InventoryItemInfo.AllowedDifficulties _descriptionShowFor = HudGui.InventoryItemInfo.AllowedDifficulties.All;

		public string _titleTextTranslationKey;

		public string _titlePluralTextTranslationKey;

		public string _effectTextTranslationKey;

		public string _descriptionTextTranslationKey;

		public HudGui.InventoryItemInfo.BackgroundSize _backgroundSize = HudGui.InventoryItemInfo.BackgroundSize.Medium;

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

		public enum BackgroundSize
		{
			Small,
			Medium,
			Large
		}

		[Flags]
		public enum AllowedDifficulties
		{
			All = -1,
			None = 0,
			Peaceful = 1,
			Normal = 2,
			Hard = 4,
			HardSurvival = 8
		}
	}

	[Serializable]
	public class InventoryItemInfoView
	{
		public int ViewCounter { get; set; }

		public bool IsCraft { get; set; }

		[HideInInspector]
		public int _itemId;

		[HideInInspector]
		public ItemProperties _itemProperties;

		public GameObject _root;

		public UITexture _icon;

		public UISprite _background;

		[NameFromEnumIndex(typeof(HudGui.InventoryItemInfo.BackgroundSize))]
		public int[] _backgroundSizes;

		public UILabel _title;

		public UILabel _effect;

		public UILabel _description;

		public UILabel _weight;

		public ItemAmountLabel _amountText;

		public GameObject _leftClickIcon;

		public GameObject _rightClickIcon;

		public UILabel _leftClickText;

		public UILabel _rightClickText;

		public UiTranslationLabel _titleUtl;

		public UiTranslationLabel _effectUtl;

		public UiTranslationLabel _descriptionUtl;

		public UiTranslationLabel _weightUtl;

		public UiTranslationLabel _leftClickTextUtl;

		public UiTranslationLabel _rightClickTextUtl;

		public UIGrid _upgradeCounterGrid;

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
	public class ClothingItemTooltipView
	{
		[HideInInspector]
		public int _outfitId;

		public GameObject _root;

		public UILabel _content;
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

		public UILabel[] _slots;
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

		public void SetMissingIngredientColor()
		{
			this._iconTexture.color = Scene.HudGui.BuildingIngredientMissing;
			this._border.color = Scene.HudGui.BuildingIngredientMissing;
		}

		public void SetAvailableIngredientColor()
		{
			this._iconTexture.color = Scene.HudGui.BuildingIngredientOwned;
			this._border.color = Scene.HudGui.BuildingIngredientOwned;
		}

		[ItemIdPicker]
		public int _itemId;

		public GameObject _iconGo;

		public UITexture _iconTexture;

		public UILabel _label;

		public UISprite _border;
	}
}
