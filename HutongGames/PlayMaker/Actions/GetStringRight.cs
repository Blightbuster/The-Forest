using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Gets the Right n characters from a String.")]
	[ActionCategory(ActionCategory.String)]
	public class GetStringRight : FsmStateAction
	{
		
		public override void Reset()
		{
			this.stringVariable = null;
			this.charCount = 0;
			this.storeResult = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoGetStringRight();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoGetStringRight();
		}

		
		private void DoGetStringRight()
		{
			if (this.stringVariable == null)
			{
				return;
			}
			if (this.storeResult == null)
			{
				return;
			}
			string value = this.stringVariable.Value;
			this.storeResult.Value = value.Substring(value.Length - this.charCount.Value, this.charCount.Value);
		}

		
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmString stringVariable;

		
		public FsmInt charCount;

		
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmString storeResult;

		
		public bool everyFrame;
	}
}
