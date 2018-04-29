﻿using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Logs the value of a Float Variable in the PlayMaker Log Window.")]
	[ActionCategory(ActionCategory.Debug)]
	public class DebugFloat : FsmStateAction
	{
		
		public override void Reset()
		{
			this.logLevel = LogLevel.Info;
			this.floatVariable = null;
		}

		
		public override void OnEnter()
		{
			string text = "None";
			if (!this.floatVariable.IsNone)
			{
				text = this.floatVariable.Name + ": " + this.floatVariable.Value;
			}
			ActionHelpers.DebugLog(base.Fsm, this.logLevel, text);
			base.Finish();
		}

		
		[Tooltip("Info, Warning, or Error.")]
		public LogLevel logLevel;

		
		[Tooltip("Prints the value of a Float variable in the PlayMaker log window.")]
		[UIHint(UIHint.Variable)]
		public FsmFloat floatVariable;
	}
}
