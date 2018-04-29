using System;
using UnityEngine;

namespace TheForest.UI
{
	
	public class ActionIcon : MonoBehaviour
	{
		
		
		
		public int StartHeight { get; private set; }

		
		private void Awake()
		{
			if (this._sprite)
			{
				this.StartHeight = this._sprite.height;
			}
		}

		
		public UiFollowTarget _follow;

		
		public UISprite _sprite;

		
		public UILabel _label;

		
		public UISprite _fillSprite;

		
		public DelayedAction _fillSpriteAction;

		
		public UISprite _sideUpArrowIcon;

		
		public UISprite _middleUpArrowIcon;

		
		public enum SideIconTypes
		{
			
			None = -1,
			
			UpArrow,
			
			MiddleUpArrow
		}
	}
}
