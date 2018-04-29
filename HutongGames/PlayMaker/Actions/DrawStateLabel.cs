﻿using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Debug)]
	[Tooltip("Draws a state label for this FSM in the Game View. The label is drawn on the game object that owns the FSM. Use this to override the global setting in the PlayMaker Debug menu.")]
	public class DrawStateLabel : FsmStateAction
	{
		
		public override void Reset()
		{
			this.showLabel = true;
		}

		
		public override void OnEnter()
		{
			base.Fsm.ShowStateLabel = this.showLabel.Value;
			base.Finish();
		}

		
		[RequiredField]
		[Tooltip("Set to True to show State labels, or Fals to hide them.")]
		public FsmBool showLabel;
	}
}
