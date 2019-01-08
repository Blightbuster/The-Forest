using System;
using System.Collections;
using System.Collections.Generic;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.UI
{
	public class ActionIconSystem : MonoBehaviour
	{
		private void Awake()
		{
			ActionIconSystem.Instance = this;
		}

		private void Update()
		{
			if (LocalPlayer.Inventory)
			{
				bool flag = LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World || (LocalPlayer.Inventory.CurrentView >= PlayerInventory.PlayerViews.Sleep && LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.PlayerList) || LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.PlaneCrash;
				if (flag != this._iconHolderTr.gameObject.activeSelf)
				{
					this._iconHolderTr.gameObject.SetActive(flag);
				}
				if (!flag)
				{
					bool flag2 = LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Book;
					if (flag2 != this._iconHolderBookTr.gameObject.activeSelf)
					{
						this._iconHolderBookTr.gameObject.SetActive(flag2);
					}
				}
				if (this._gamePadMode != TheForest.Utils.Input.IsGamePad)
				{
					this._gamePadMode = TheForest.Utils.Input.IsGamePad;
				}
			}
		}

		private void OnDestroy()
		{
			if (ActionIconSystem.Instance == this)
			{
				ActionIconSystem.Instance = null;
			}
		}

		private ActionIcon GetActionIconSprite(bool big)
		{
			ActionIcon actionIcon;
			if (big)
			{
				if (ActionIconSystem.Instance._spriteIconPoolBig.Count > 0)
				{
					actionIcon = ActionIconSystem.Instance._spriteIconPoolBig.Dequeue();
					actionIcon.gameObject.SetActive(true);
				}
				else
				{
					actionIcon = UnityEngine.Object.Instantiate<ActionIcon>(ActionIconSystem.Instance._spriteIconPrefabBig);
					actionIcon.transform.parent = base.transform.parent;
					actionIcon.transform.localScale = ActionIconSystem.Instance._spriteIconPrefabBig.transform.localScale;
				}
			}
			else if (ActionIconSystem.Instance._spriteIconPool.Count > 0)
			{
				actionIcon = ActionIconSystem.Instance._spriteIconPool.Dequeue();
				actionIcon.gameObject.SetActive(true);
			}
			else
			{
				actionIcon = UnityEngine.Object.Instantiate<ActionIcon>(ActionIconSystem.Instance._spriteIconPrefab);
				actionIcon.transform.parent = base.transform.parent;
				actionIcon.transform.localScale = ActionIconSystem.Instance._spriteIconPrefab.transform.localScale;
			}
			return actionIcon;
		}

		private ActionIcon GetActionIconLabel(bool alt, bool big)
		{
			ActionIcon actionIcon;
			if (big)
			{
				if (((!alt) ? ActionIconSystem.Instance._textIconPoolBig : ActionIconSystem.Instance._textIconPoolAltBig).Count > 0)
				{
					actionIcon = ((!alt) ? ActionIconSystem.Instance._textIconPoolBig : ActionIconSystem.Instance._textIconPoolAltBig).Dequeue();
					actionIcon.gameObject.SetActive(true);
				}
				else
				{
					actionIcon = UnityEngine.Object.Instantiate<ActionIcon>((!alt) ? ActionIconSystem.Instance._textIconPrefabBig : ActionIconSystem.Instance._textIconPrefabAltBig);
					actionIcon.transform.parent = base.transform.parent;
					actionIcon.transform.localScale = ActionIconSystem.Instance._textIconPrefabBig.transform.localScale;
				}
			}
			else if (((!alt) ? ActionIconSystem.Instance._textIconPool : ActionIconSystem.Instance._textIconPoolAlt).Count > 0)
			{
				actionIcon = ((!alt) ? ActionIconSystem.Instance._textIconPool : ActionIconSystem.Instance._textIconPoolAlt).Dequeue();
				actionIcon.gameObject.SetActive(true);
			}
			else
			{
				actionIcon = UnityEngine.Object.Instantiate<ActionIcon>((!alt) ? ActionIconSystem.Instance._textIconPrefab : ActionIconSystem.Instance._textIconPrefabAlt);
				actionIcon.transform.parent = base.transform.parent;
				actionIcon.transform.localScale = ActionIconSystem.Instance._textIconPrefab.transform.localScale;
			}
			return actionIcon;
		}

		public static ActionIcon RegisterIcon(Transform target, InputMappingIcons.Actions action, ActionIcon.SideIconTypes sideIcon, ActionIconSystem.CurrentViewOptions currentViewOption = ActionIconSystem.CurrentViewOptions.AllowInWorld, bool useAltTextIcon = false, bool useBigIcon = false, Texture2D vrIcon = null, string actionStringOverride = null, bool useFillSprite = false)
		{
			ActionIcon actionIcon = ActionIconSystem.InitializeActionIcon(target, action, sideIcon, currentViewOption, useAltTextIcon, useBigIcon);
			if (ForestVR.Enabled && !ActionIconSystem.Instance._activeVRButtonActionIcons.ContainsKey(target))
			{
				VRControllerDisplayManager.AutoShowAction(action, true, vrIcon, sideIcon, actionStringOverride, (!actionIcon.IsNull()) ? actionIcon._fillSpriteAction : null, useFillSprite);
				ActionIconSystem.Instance._activeVRButtonActionIcons.Add(target, action);
			}
			return actionIcon;
		}

		private static ActionIcon InitializeActionIcon(Transform target, InputMappingIcons.Actions action, ActionIcon.SideIconTypes sideIcon, ActionIconSystem.CurrentViewOptions currentViewOption, bool useAltTextIcon, bool useBigIcon)
		{
			ActionIcon actionIcon = null;
			if (ActionIconSystem.Instance && !ActionIconSystem.Instance._activeIcons.ContainsKey(target))
			{
				if (!InputMappingIcons.UsesText(action))
				{
					actionIcon = ActionIconSystem.Instance.GetActionIconSprite(useBigIcon);
					if (!ForestVR.Enabled)
					{
						actionIcon._sprite.spriteName = InputMappingIcons.GetMappingFor(action);
						UISpriteData atlasSprite = actionIcon._sprite.GetAtlasSprite();
						if (atlasSprite == null)
						{
							ActionIconSystem.Instance.DisableActionIcon(actionIcon, useAltTextIcon, useBigIcon);
							return null;
						}
						actionIcon._sprite.width = Mathf.RoundToInt((float)atlasSprite.width / (float)atlasSprite.height * (float)actionIcon._sprite.height);
					}
					else
					{
						actionIcon._sprite.enabled = false;
					}
				}
				else
				{
					actionIcon = ActionIconSystem.Instance.GetActionIconLabel(useAltTextIcon, useBigIcon);
					actionIcon._label.text = InputMappingIcons.GetMappingFor(action);
					if (!useAltTextIcon)
					{
						if (actionIcon._sprite == null)
						{
							Debug.LogError(string.Concat(new object[]
							{
								"[ActionIcon] Invalid sprite for \"",
								action,
								"\" on ",
								actionIcon.gameObject.GetFullName()
							}));
							return null;
						}
						actionIcon._sprite.spriteName = InputMappingIcons.GetBackingFor(action);
						actionIcon._sprite.enabled = true;
						float num = (float)actionIcon._label.width * actionIcon._label.transform.localScale.x / (float)actionIcon.StartHeight;
						if (num > 1.5f)
						{
							actionIcon._sprite.width = Mathf.RoundToInt((float)actionIcon.StartHeight * (num * 1.2f));
						}
						else
						{
							actionIcon._sprite.width = actionIcon.StartHeight;
						}
					}
				}
				actionIcon._action = action;
				actionIcon._follow._target = target;
				actionIcon._fillSprite.gameObject.SetActive(false);
				actionIcon._follow._inHud = (currentViewOption == ActionIconSystem.CurrentViewOptions.HudIcon || currentViewOption == ActionIconSystem.CurrentViewOptions.DeathScreen);
				actionIcon._follow._inBook = (currentViewOption == ActionIconSystem.CurrentViewOptions.AllowInBook);
				actionIcon._follow._inInventory = (currentViewOption == ActionIconSystem.CurrentViewOptions.AllowInInventory);
				actionIcon._follow._inPlane = (currentViewOption == ActionIconSystem.CurrentViewOptions.AllowInPlane);
				if (actionIcon._sideUpArrowIcon)
				{
					actionIcon._sideUpArrowIcon.enabled = (sideIcon == ActionIcon.SideIconTypes.UpArrow);
				}
				if (actionIcon._middleUpArrowIcon)
				{
					actionIcon._middleUpArrowIcon.enabled = (sideIcon == ActionIcon.SideIconTypes.MiddleUpArrow);
				}
				ActionIconSystem.Instance.SetIconHolderTr(actionIcon.transform, currentViewOption);
				ActionIconSystem.Instance._activeIcons.Add(target, actionIcon);
			}
			return actionIcon;
		}

		public static ActionIcon UnregisterIcon(Transform target, bool useAltTextIcon = false, bool useBigIcon = false)
		{
			if (ForestVR.Enabled && ActionIconSystem.Instance != null && ActionIconSystem.Instance._activeVRButtonActionIcons.ContainsKey(target))
			{
				VRControllerDisplayManager.AutoShowAction(ActionIconSystem.Instance._activeVRButtonActionIcons[target], false, null, ActionIcon.SideIconTypes.None, null, null, false);
				ActionIconSystem.Instance._activeVRButtonActionIcons.Remove(target);
			}
			ActionIcon actionIcon;
			if (ActionIconSystem.Instance && ActionIconSystem.Instance._activeIcons.TryGetValue(target, out actionIcon))
			{
				actionIcon._follow._target2 = null;
				if (actionIcon._fillSpriteAction)
				{
					actionIcon._fillSpriteAction._actionName = null;
				}
				ActionIconSystem.Instance._activeIcons.Remove(target);
				ActionIconSystem.Instance.DisableActionIcon(actionIcon, useAltTextIcon, useBigIcon);
				return actionIcon;
			}
			return null;
		}

		private void DisableActionIcon(ActionIcon ai, bool useAltTextIcon, bool useBigIcon)
		{
			if (useBigIcon)
			{
				if (ai._label != null)
				{
					((!useAltTextIcon) ? ActionIconSystem.Instance._textIconPoolBig : ActionIconSystem.Instance._textIconPoolAltBig).Enqueue(ai);
				}
				else
				{
					ActionIconSystem.Instance._spriteIconPoolBig.Enqueue(ai);
				}
			}
			else if (ai._label != null)
			{
				((!useAltTextIcon) ? ActionIconSystem.Instance._textIconPool : ActionIconSystem.Instance._textIconPoolAlt).Enqueue(ai);
			}
			else
			{
				ActionIconSystem.Instance._spriteIconPool.Enqueue(ai);
			}
			ai.gameObject.SetActive(false);
		}

		public static ActionIcon GetActionIcon(Transform target)
		{
			ActionIcon result;
			if (ActionIconSystem.Instance && ActionIconSystem.Instance._activeIcons.TryGetValue(target, out result))
			{
				return result;
			}
			return null;
		}

		private void SetIconHolderTr(Transform t, ActionIconSystem.CurrentViewOptions currentViewOption)
		{
			switch (currentViewOption)
			{
			case ActionIconSystem.CurrentViewOptions.AllowInWorld:
				this.SetParentAndLayer(t, this._iconHolderTr);
				break;
			case ActionIconSystem.CurrentViewOptions.AllowInBook:
				this.SetParentAndLayer(t, this._iconHolderBookTr);
				break;
			case ActionIconSystem.CurrentViewOptions.AllowInPlane:
			case ActionIconSystem.CurrentViewOptions.DeathScreen:
				this.SetParentAndLayer(t, this._iconHolderPlaneTr);
				break;
			case ActionIconSystem.CurrentViewOptions.AllowInInventory:
				this.SetParentAndLayer(t, this._iconHolderInventoryTr);
				break;
			case ActionIconSystem.CurrentViewOptions.HudIcon:
				if (LocalPlayer.Inventory)
				{
					PlayerInventory.PlayerViews currentView = LocalPlayer.Inventory.CurrentView;
					switch (currentView)
					{
					case PlayerInventory.PlayerViews.Inventory:
						this.SetParentAndLayer(t, this._iconHolderInventoryTr);
						break;
					default:
						if (currentView != PlayerInventory.PlayerViews.PlaneCrash)
						{
							this.SetParentAndLayer(t, this._iconHolderTr);
						}
						else
						{
							this.SetParentAndLayer(t, this._iconHolderPlaneHudTr);
						}
						break;
					case PlayerInventory.PlayerViews.Book:
						this.SetParentAndLayer(t, this._iconHolderBookTr);
						break;
					case PlayerInventory.PlayerViews.Pause:
						this.SetParentAndLayer(t, this._iconHolderPauseTr);
						break;
					}
				}
				else
				{
					t.parent = base.transform;
				}
				break;
			}
		}

		private void SetParentAndLayer(Transform tr, Transform parent)
		{
			this.SetLayerRec(tr, parent.gameObject.layer);
			tr.parent = parent;
		}

		private void SetLayerRec(Transform t, int layer)
		{
			t.gameObject.layer = layer;
			IEnumerator enumerator = t.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform t2 = (Transform)obj;
					this.SetLayerRec(t2, layer);
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
		}

		public ActionIcon _textIconPrefab;

		public ActionIcon _textIconPrefabAlt;

		public ActionIcon _spriteIconPrefab;

		public ActionIcon _textIconPrefabBig;

		public ActionIcon _textIconPrefabAltBig;

		public ActionIcon _spriteIconPrefabBig;

		public Transform _iconHolderTr;

		public Transform _iconHolderBookTr;

		public Transform _iconHolderPlaneTr;

		public Transform _iconHolderPlaneHudTr;

		public Transform _iconHolderInventoryTr;

		public Transform _iconHolderPauseTr;

		public float _gamepadMasterDepthRatio = 0.5f;

		public float _gamepadMasterDepthOffset;

		private static ActionIconSystem Instance;

		private Queue<ActionIcon> _textIconPool = new Queue<ActionIcon>();

		private Queue<ActionIcon> _textIconPoolAlt = new Queue<ActionIcon>();

		private Queue<ActionIcon> _spriteIconPool = new Queue<ActionIcon>();

		private Queue<ActionIcon> _textIconPoolBig = new Queue<ActionIcon>();

		private Queue<ActionIcon> _textIconPoolAltBig = new Queue<ActionIcon>();

		private Queue<ActionIcon> _spriteIconPoolBig = new Queue<ActionIcon>();

		private Dictionary<Transform, ActionIcon> _activeIcons = new Dictionary<Transform, ActionIcon>();

		private Dictionary<Transform, InputMappingIcons.Actions> _activeVRButtonActionIcons = new Dictionary<Transform, InputMappingIcons.Actions>();

		private bool _gamePadMode;

		public enum CurrentViewOptions
		{
			AllowInWorld,
			AllowInBook,
			AllowInPlane,
			AllowInInventory,
			HudIcon,
			DeathScreen
		}
	}
}
