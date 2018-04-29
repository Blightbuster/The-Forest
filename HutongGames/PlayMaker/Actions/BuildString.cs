using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.String)]
	[Tooltip("Builds a String from other Strings.")]
	public class BuildString : FsmStateAction
	{
		
		public override void Reset()
		{
			this.stringParts = new FsmString[3];
			this.separator = null;
			this.storeResult = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoBuildString();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoBuildString();
		}

		
		private void DoBuildString()
		{
			if (this.storeResult == null)
			{
				return;
			}
			this.result = string.Empty;
			foreach (FsmString arg in this.stringParts)
			{
				this.result += arg;
				this.result += this.separator.Value;
			}
			this.storeResult.Value = this.result;
		}

		
		[Tooltip("Array of Strings to combine.")]
		[RequiredField]
		public FsmString[] stringParts;

		
		[Tooltip("Separator to insert between each String. E.g. space character.")]
		public FsmString separator;

		
		[Tooltip("Store the final String in a variable.")]
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmString storeResult;

		
		[Tooltip("Repeat every frame while the state is active.")]
		public bool everyFrame;

		
		private string result;
	}
}
