using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Convert)]
	[Tooltip("Converts a Float value to a String value with optional format.")]
	public class ConvertFloatToString : FsmStateAction
	{
		
		public override void Reset()
		{
			this.floatVariable = null;
			this.stringVariable = null;
			this.everyFrame = false;
			this.format = null;
		}

		
		public override void OnEnter()
		{
			this.DoConvertFloatToString();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoConvertFloatToString();
		}

		
		private void DoConvertFloatToString()
		{
			if (this.format.IsNone || string.IsNullOrEmpty(this.format.Value))
			{
				this.stringVariable.Value = this.floatVariable.Value.ToString();
			}
			else
			{
				this.stringVariable.Value = this.floatVariable.Value.ToString(this.format.Value);
			}
		}

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The float variable to convert.")]
		public FsmFloat floatVariable;

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("A string variable to store the converted value.")]
		public FsmString stringVariable;

		
		[Tooltip("Optional Format, allows for leading zeroes. E.g., 0000")]
		public FsmString format;

		
		[Tooltip("Repeat every frame. Useful if the float variable is changing.")]
		public bool everyFrame;
	}
}
