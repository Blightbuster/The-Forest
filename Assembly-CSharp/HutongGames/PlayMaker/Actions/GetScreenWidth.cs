using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Application)]
	[Tooltip("Gets the Width of the Screen in pixels.")]
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

		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmFloat storeScreenWidth;
	}
}
