using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Device)]
	[Tooltip("Gets the number of Touches.")]
	public class GetTouchCount : FsmStateAction
	{
		
		public override void Reset()
		{
			this.storeCount = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoGetTouchCount();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoGetTouchCount();
		}

		
		private void DoGetTouchCount()
		{
			this.storeCount.Value = Input.touchCount;
		}

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmInt storeCount;

		
		public bool everyFrame;
	}
}
