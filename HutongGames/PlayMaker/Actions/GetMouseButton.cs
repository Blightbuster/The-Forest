using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Input)]
	[Tooltip("Gets the pressed state of the specified Mouse Button and stores it in a Bool Variable. See Unity Input Manager doc.")]
	public class GetMouseButton : FsmStateAction
	{
		
		public override void Reset()
		{
			this.button = MouseButton.Left;
			this.storeResult = null;
		}

		
		public override void OnUpdate()
		{
			this.storeResult.Value = Input.GetMouseButton((int)this.button);
		}

		
		[RequiredField]
		public MouseButton button;

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmBool storeResult;
	}
}
