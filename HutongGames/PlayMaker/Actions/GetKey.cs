using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Gets the pressed state of a Key.")]
	[ActionCategory(ActionCategory.Input)]
	public class GetKey : FsmStateAction
	{
		
		public override void Reset()
		{
			this.key = KeyCode.None;
			this.storeResult = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoGetKey();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoGetKey();
		}

		
		private void DoGetKey()
		{
			this.storeResult.Value = Input.GetKey(this.key);
		}

		
		[RequiredField]
		[Tooltip("The key to test.")]
		public KeyCode key;

		
		[Tooltip("Store if the key is down (True) or up (False).")]
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmBool storeResult;

		
		[Tooltip("Repeat every frame. Useful if you're waiting for a key press/release.")]
		public bool everyFrame;
	}
}
