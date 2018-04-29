using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.UI
{
	
	public class ActionIconWorld : MonoBehaviour
	{
		
		
		
		public UISprite FillSprite { get; set; }

		
		private void Reset()
		{
			ActionTriggerEvent component = base.GetComponent<ActionTriggerEvent>();
			if (component)
			{
				this._action = component._action;
			}
			if (base.GetComponentInParent<SurvivalBook>())
			{
				this._currentViewOption = ActionIconSystem.CurrentViewOptions.AllowInBook;
			}
		}

		
		public void OnEnable()
		{
			if (this._action != InputMappingIcons.Actions.None && (!this._gamepadOnly || TheForest.Utils.Input.IsGamePad) && (!this._mouseOnly || !TheForest.Utils.Input.IsGamePad))
			{
				this.FillSprite = ActionIconSystem.RegisterIcon(base.transform, this._action, this._sideIcon, this._currentViewOption);
				if (this._overrideDepth || this._overrideHeight || this._currentViewOption == ActionIconSystem.CurrentViewOptions.AllowInBook || this._currentViewOption == ActionIconSystem.CurrentViewOptions.AllowInInventory)
				{
					ActionIcon actionIcon = ActionIconSystem.GetActionIcon(base.transform);
					if (actionIcon)
					{
						if (this._overrideDepth)
						{
							if (this._currentViewOption != ActionIconSystem.CurrentViewOptions.AllowInBook)
							{
								this._oldDepth = actionIcon._follow._minDepth;
								actionIcon._follow._minDepth = this._depth;
							}
						}
						if (this._overrideHeight)
						{
							if (this._currentViewOption != ActionIconSystem.CurrentViewOptions.AllowInBook)
							{
								this._oldHeight = actionIcon._follow._worldOffset.y;
								actionIcon._follow._worldOffset.y = this._height;
							}
						}
						if (this._currentViewOption == ActionIconSystem.CurrentViewOptions.AllowInBook || this._currentViewOption == ActionIconSystem.CurrentViewOptions.AllowInInventory)
						{
							actionIcon._follow._viewportOffsetBook = this._viewportOffset;
						}
					}
				}
			}
		}

		
		public void OnDisable()
		{
			if (this._action != InputMappingIcons.Actions.None)
			{
				ActionIcon actionIcon = ActionIconSystem.UnregisterIcon(base.transform);
				if (actionIcon)
				{
					if (this._overrideDepth)
					{
						if (this._currentViewOption != ActionIconSystem.CurrentViewOptions.AllowInBook)
						{
							actionIcon._follow._minDepth = this._oldDepth;
						}
					}
					if (this._overrideHeight)
					{
						if (this._currentViewOption != ActionIconSystem.CurrentViewOptions.AllowInBook)
						{
							actionIcon._follow._worldOffset.y = this._oldHeight;
						}
					}
				}
			}
		}

		
		public InputMappingIcons.Actions _action;

		
		public ActionIconSystem.CurrentViewOptions _currentViewOption;

		
		public ActionIcon.SideIconTypes _sideIcon = ActionIcon.SideIconTypes.None;

		
		public bool _overrideDepth;

		
		public float _depth;

		
		public bool _overrideHeight;

		
		public float _height;

		
		public Vector3 _viewportOffset;

		
		public bool _gamepadOnly;

		
		public bool _mouseOnly;

		
		public float _oldDepth;

		
		public float _oldHeight;
	}
}
