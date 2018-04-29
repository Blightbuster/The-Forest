using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.GUILayout)]
	[Tooltip("Begins a ScrollView. Use GUILayoutEndScrollView at the end of the block.")]
	public class GUILayoutBeginScrollView : GUILayoutAction
	{
		
		public override void Reset()
		{
			base.Reset();
			this.scrollPosition = null;
			this.horizontalScrollbar = null;
			this.verticalScrollbar = null;
			this.useCustomStyle = null;
			this.horizontalStyle = null;
			this.verticalStyle = null;
			this.backgroundStyle = null;
		}

		
		public override void OnGUI()
		{
			if (this.useCustomStyle.Value)
			{
				this.scrollPosition.Value = GUILayout.BeginScrollView(this.scrollPosition.Value, this.horizontalScrollbar.Value, this.verticalScrollbar.Value, this.horizontalStyle.Value, this.verticalStyle.Value, this.backgroundStyle.Value, base.LayoutOptions);
			}
			else
			{
				this.scrollPosition.Value = GUILayout.BeginScrollView(this.scrollPosition.Value, this.horizontalScrollbar.Value, this.verticalScrollbar.Value, base.LayoutOptions);
			}
		}

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Assign a Vector2 variable to store the scroll position of this view.")]
		public FsmVector2 scrollPosition;

		
		[Tooltip("Always show the horizontal scrollbars.")]
		public FsmBool horizontalScrollbar;

		
		[Tooltip("Always show the vertical scrollbars.")]
		public FsmBool verticalScrollbar;

		
		[Tooltip("Define custom styles below. NOTE: You have to define all the styles if you check this option.")]
		public FsmBool useCustomStyle;

		
		[Tooltip("Named style in the active GUISkin for the horizontal scrollbars.")]
		public FsmString horizontalStyle;

		
		[Tooltip("Named style in the active GUISkin for the vertical scrollbars.")]
		public FsmString verticalStyle;

		
		[Tooltip("Named style in the active GUISkin for the background.")]
		public FsmString backgroundStyle;
	}
}
