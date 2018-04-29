using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.UI
{
	
	public class DelayedActionSheenBillboard : MonoBehaviour
	{
		
		private void GrabEnter()
		{
			TheForest.Utils.Input.ResetDelayedAction();
			this._icon.UseFillSprite = true;
			if (this._icon.gameObject.activeSelf)
			{
				if (!this._icon.FillSpriteAction.gameObject.activeSelf)
				{
					this._icon.FillSpriteAction.gameObject.SetActive(true);
				}
				this._icon.FillSpriteAction.SetAction(this._icon._action);
			}
		}

		
		private void GrabExit()
		{
			this._icon.UseFillSprite = false;
			if (this._icon.FillSpriteAction && this._icon.FillSpriteAction.gameObject.activeSelf)
			{
				this._icon.FillSpriteAction.gameObject.SetActive(false);
			}
		}

		
		public SheenBillboard _icon;
	}
}
