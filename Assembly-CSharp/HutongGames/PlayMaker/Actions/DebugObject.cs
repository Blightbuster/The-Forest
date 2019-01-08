using System;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Debug)]
	[Tooltip("Logs the value of an Object Variable in the PlayMaker Log Window.")]
	public class DebugObject : FsmStateAction
	{
		public override void Reset()
		{
			this.logLevel = LogLevel.Info;
			this.fsmObject = null;
		}

		public override void OnEnter()
		{
			string text = "None";
			if (!this.fsmObject.IsNone)
			{
				text = this.fsmObject.Name + ": " + this.fsmObject;
			}
			ActionHelpers.DebugLog(base.Fsm, this.logLevel, text);
			base.Finish();
		}

		[Tooltip("Info, Warning, or Error.")]
		public LogLevel logLevel;

		[UIHint(UIHint.Variable)]
		[Tooltip("Prints the value of an Object variable in the PlayMaker log window.")]
		public FsmObject fsmObject;
	}
}
