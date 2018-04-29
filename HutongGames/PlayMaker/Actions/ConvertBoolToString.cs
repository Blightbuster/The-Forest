using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Converts a Bool value to a String value.")]
	[ActionCategory(ActionCategory.Convert)]
	public class ConvertBoolToString : FsmStateAction
	{
		
		public override void Reset()
		{
			this.boolVariable = null;
			this.stringVariable = null;
			this.falseString = "False";
			this.trueString = "True";
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoConvertBoolToString();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoConvertBoolToString();
		}

		
		private void DoConvertBoolToString()
		{
			this.stringVariable.Value = ((!this.boolVariable.Value) ? this.falseString.Value : this.trueString.Value);
		}

		
		[Tooltip("The Bool variable to test.")]
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmBool boolVariable;

		
		[Tooltip("The String variable to set based on the Bool variable value.")]
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmString stringVariable;

		
		[Tooltip("String value if Bool variable is false.")]
		public FsmString falseString;

		
		[Tooltip("String value if Bool variable is true.")]
		public FsmString trueString;

		
		[Tooltip("Repeat every frame. Useful if the Bool variable is changing.")]
		public bool everyFrame;
	}
}
