using System;
using UnityEngine;

namespace TheForest.UI
{
	public class VRActionIcon : MonoBehaviour
	{
		private void OnEnable()
		{
			if (ForestVR.Enabled)
			{
				VRControllerDisplayManager.AutoShowAction(this._action, true, null, ActionIcon.SideIconTypes.None, this._actionTextOverride, null, false);
			}
		}

		private void OnDisable()
		{
			if (ForestVR.Enabled)
			{
				VRControllerDisplayManager.AutoShowAction(this._action, false, null, ActionIcon.SideIconTypes.None, null, null, false);
			}
		}

		public InputMappingIcons.Actions _action;

		public string _actionTextOverride;
	}
}
