﻿using System;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Debug)]
	[Tooltip("Logs the value of a Bool Variable in the PlayMaker Log Window.")]
	public class DebugBool : FsmStateAction
	{
		public override void Reset()
		{
			this.logLevel = LogLevel.Info;
			this.boolVariable = null;
		}

		public override void OnEnter()
		{
			string text = "None";
			if (!this.boolVariable.IsNone)
			{
				text = this.boolVariable.Name + ": " + this.boolVariable.Value;
			}
			ActionHelpers.DebugLog(base.Fsm, this.logLevel, text);
			base.Finish();
		}

		[Tooltip("Info, Warning, or Error.")]
		public LogLevel logLevel;

		[UIHint(UIHint.Variable)]
		[Tooltip("Prints the value of a Bool variable in the PlayMaker log window.")]
		public FsmBool boolVariable;
	}
}
