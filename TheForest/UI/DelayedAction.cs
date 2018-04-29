using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.UI
{
	
	public class DelayedAction : MonoBehaviour
	{
		
		private void Update()
		{
			this._fillIcon.fillAmount = TheForest.Utils.Input.DelayedActionAlpha;
		}

		
		public UISprite _fillIcon;
	}
}
