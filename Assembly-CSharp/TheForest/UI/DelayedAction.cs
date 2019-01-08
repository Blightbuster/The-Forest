using System;
using System.Collections.Generic;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.UI
{
	public class DelayedAction : MonoBehaviour
	{
		private void Awake()
		{
			if (ForestVR.Enabled)
			{
				this._fillIcon.enabled = false;
			}
		}

		private void Update()
		{
			if (!DelayedAction.HasActiveCustomControl)
			{
				bool flag = string.IsNullOrEmpty(this._actionName) || TheForest.Utils.Input.DelayedActionName.Equals(this._actionName);
				this._fillIcon.fillAmount = ((!flag) ? 0f : TheForest.Utils.Input.DelayedActionAlpha);
			}
		}

		public void SetAction(InputMappingIcons.Actions action)
		{
			if (!DelayedAction.ActionNames.ContainsKey(action))
			{
				DelayedAction.ActionNames.Add(action, action.ToString());
			}
			this._actionName = DelayedAction.ActionNames[action];
		}

		public UISprite _fillIcon;

		public string _actionName;

		public bool _useFillSprite;

		private static Dictionary<InputMappingIcons.Actions, string> ActionNames = new Dictionary<InputMappingIcons.Actions, string>();

		public static bool HasActiveCustomControl;
	}
}
