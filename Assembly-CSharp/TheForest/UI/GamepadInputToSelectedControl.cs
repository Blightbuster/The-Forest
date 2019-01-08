using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.UI
{
	public class GamepadInputToSelectedControl : MonoBehaviour
	{
		private void Update()
		{
			if (UICamera.hoveredObject.IsNull())
			{
				return;
			}
			if (ForestVR.Enabled)
			{
				if (TheForest.Utils.Input.GetButtonDown("Take"))
				{
					if (UIPopupList.current)
					{
						UICamera.hoveredObject.SendMessage("OnPress", true, SendMessageOptions.DontRequireReceiver);
					}
					else
					{
						UICamera.hoveredObject.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
					}
				}
			}
			else if (TheForest.Utils.Input.UsingDualshock && UICamera.controller != null && TheForest.Utils.Input.GetButtonDown("Take"))
			{
				if (UIPopupList.current)
				{
					UICamera.hoveredObject.SendMessage("OnPress", true, SendMessageOptions.DontRequireReceiver);
				}
				else
				{
					UICamera.hoveredObject.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
				}
			}
		}
	}
}
