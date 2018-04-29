using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Converts a Bool value to a Float value.")]
	[ActionCategory(ActionCategory.Convert)]
	public class ConvertBoolToFloat : FsmStateAction
	{
		
		public override void Reset()
		{
			this.boolVariable = null;
			this.floatVariable = null;
			this.falseValue = 0f;
			this.trueValue = 1f;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoConvertBoolToFloat();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoConvertBoolToFloat();
		}

		
		private void DoConvertBoolToFloat()
		{
			this.floatVariable.Value = ((!this.boolVariable.Value) ? this.falseValue.Value : this.trueValue.Value);
		}

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The Bool variable to test.")]
		public FsmBool boolVariable;

		
		[Tooltip("The Float variable to set based on the Bool variable value.")]
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmFloat floatVariable;

		
		[Tooltip("Float value if Bool variable is false.")]
		public FsmFloat falseValue;

		
		[Tooltip("Float value if Bool variable is true.")]
		public FsmFloat trueValue;

		
		[Tooltip("Repeat every frame. Useful if the Bool variable is changing.")]
		public bool everyFrame;
	}
}
