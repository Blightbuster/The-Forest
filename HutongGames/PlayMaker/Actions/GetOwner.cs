﻿using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.GameObject)]
	[Tooltip("Gets the Game Object that owns the FSM and stores it in a game object variable.")]
	public class GetOwner : FsmStateAction
	{
		
		public override void Reset()
		{
			this.storeGameObject = null;
		}

		
		public override void OnEnter()
		{
			this.storeGameObject.Value = base.Owner;
			base.Finish();
		}

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmGameObject storeGameObject;
	}
}
