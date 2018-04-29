using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.UI
{
	
	public class GamepadInputToSelectedControl : MonoBehaviour
	{
		
		private void Update()
		{
			if (TheForest.Utils.Input.IsGamePad && UICamera.controller != null && UICamera.hoveredObject && TheForest.Utils.Input.GetButtonDown("Take"))
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
