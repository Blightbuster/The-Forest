using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Gets the Width of the Screen in pixels.")]
	[ActionCategory(ActionCategory.Application)]
	public class GetScreenWidth : FsmStateAction
	{
		
		public override void Reset()
		{
			this.storeScreenWidth = null;
		}

		
		public override void OnEnter()
		{
			this.storeScreenWidth.Value = (float)Screen.width;
			base.Finish();
		}

		
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmFloat storeScreenWidth;
	}
}
