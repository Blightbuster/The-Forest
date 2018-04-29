﻿using System;
using UnityEngine;

namespace TheForest.UI
{
	
	public class ActionIcon : MonoBehaviour
	{
		
		public UiFollowTarget _follow;

		
		public UISprite _sprite;

		
		public UILabel _label;

		
		public UISprite _fillSprite;

		
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