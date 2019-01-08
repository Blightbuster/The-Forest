using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.UI
{
	public class ActionIconUISprite : MonoBehaviour
	{
		private void Awake()
		{
			if (this._sprite)
			{
				this._startHeight = this._sprite.height;
			}
			this._widget = base.GetComponent<UIWidget>();
		}

		public void OnEnable()
		{
			if (this._action != InputMappingIcons.Actions.None && (!this._gamepadOnly || TheForest.Utils.Input.IsGamePad || ForestVR.Enabled))
			{
				this._registeredAsBigIcon = (this._useBigIcon || (TheForest.Utils.Input.IsGamePad && this._useBigIconForGamepad));
				Transform transform = base.transform;
				InputMappingIcons.Actions action = this._action;
				ActionIcon.SideIconTypes sideIcon = this._sideIcon;
				ActionIconSystem.CurrentViewOptions currentViewOption = this._currentViewOption;
				bool useAltTextIcon = this._useAltTextIcon;
				bool registeredAsBigIcon = this._registeredAsBigIcon;
				string actionTextOverride = this._actionTextOverride;
				ActionIcon actionIcon = ActionIconSystem.RegisterIcon(transform, action, sideIcon, currentViewOption, useAltTextIcon, registeredAsBigIcon, null, actionTextOverride, false);
				if (actionIcon)
				{
					this._fillSprite = actionIcon._fillSprite;
					if (this._fillSprite)
					{
						if (this._fillSprite.gameObject.activeSelf != this._useFillSprite)
						{
							this._fillSprite.gameObject.SetActive(this._useFillSprite);
						}
						if (this._useFillSprite && actionIcon._fillSpriteAction)
						{
							actionIcon._fillSpriteAction.SetAction(this._action);
						}
					}
					if (actionIcon._sprite && this._widget && this._refreshSiblings)
					{
						Transform target = this._widget.leftAnchor.target;
						this._widget.leftAnchor.target = null;
						this._widget.width = Mathf.RoundToInt((float)actionIcon._sprite.width / base.transform.localScale.x);
						this._widget.leftAnchor.target = target;
						this._widget.SkipWidthUpdate = true;
						IEnumerator enumerator = base.transform.GetEnumerator();
						try
						{
							while (enumerator.MoveNext())
							{
								object obj = enumerator.Current;
								Transform transform2 = (Transform)obj;
								UILabel component = transform2.GetComponent<UILabel>();
								if (component)
								{
									component.MarkAsChanged();
									component.ProcessText();
								}
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
				}
			}
			if (this._sprite)
			{
				this._sprite.enabled = false;
			}
			if (this._label)
			{
				this._label.enabled = false;
			}
		}

		public void OnDisable()
		{
			if (this._action != InputMappingIcons.Actions.None)
			{
				ActionIconSystem.UnregisterIcon(base.transform, this._useAltTextIcon, this._registeredAsBigIcon);
			}
		}

		private void Update()
		{
			if (this._version != InputMappingIcons.Version)
			{
				this._version = InputMappingIcons.Version;
				this.OnDisable();
				this.OnEnable();
			}
		}

		public void ChangeAction(InputMappingIcons.Actions action, bool useFillSprite)
		{
			if (this._action != action || this._useFillSprite != useFillSprite)
			{
				this._useFillSprite = useFillSprite;
				this._action = action;
				this._version--;
				this.OnDisable();
				this.OnEnable();
			}
		}

		public InputMappingIcons.Actions _action;

		public string _actionTextOverride;

		public ActionIcon.SideIconTypes _sideIcon = ActionIcon.SideIconTypes.None;

		public ActionIconSystem.CurrentViewOptions _currentViewOption = ActionIconSystem.CurrentViewOptions.HudIcon;

		public UISprite _sprite;

		public UILabel _label;

		public bool _useFillSprite;

		public bool _useAltTextIcon;

		public bool _useBigIcon;

		public bool _useBigIconForGamepad;

		public bool _refreshSiblings;

		public bool _gamepadOnly;

		private bool _registeredAsBigIcon;

		private int _version;

		private int _startHeight;

		private UISprite _fillSprite;

		private UIWidget _widget;
	}
}
