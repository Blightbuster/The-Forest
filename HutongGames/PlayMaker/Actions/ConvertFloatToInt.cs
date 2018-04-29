using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Converts a Float value to an Integer value.")]
	[ActionCategory(ActionCategory.Convert)]
	public class ConvertFloatToInt : FsmStateAction
	{
		
		public override void Reset()
		{
			this.floatVariable = null;
			this.intVariable = null;
			this.rounding = ConvertFloatToInt.FloatRounding.Nearest;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoConvertFloatToInt();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoConvertFloatToInt();
		}

		
		private void DoConvertFloatToInt()
		{
			switch (this.rounding)
			{
			case ConvertFloatToInt.FloatRounding.RoundDown:
				this.intVariable.Value = Mathf.FloorToInt(this.floatVariable.Value);
				break;
			case ConvertFloatToInt.FloatRounding.RoundUp:
				this.intVariable.Value = Mathf.CeilToInt(this.floatVariable.Value);
				break;
			case ConvertFloatToInt.FloatRounding.Nearest:
				this.intVariable.Value = Mathf.RoundToInt(this.floatVariable.Value);
				break;
			}
		}

		
		[RequiredField]
		[Tooltip("The Float variable to convert to an integer.")]
		[UIHint(UIHint.Variable)]
		public FsmFloat floatVariable;

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the result in an Integer variable.")]
		public FsmInt intVariable;

		
		public ConvertFloatToInt.FloatRounding rounding;

		
		public bool everyFrame;

		
		public enum FloatRounding
		{
			
			RoundDown,
			
			RoundUp,
			
			Nearest
		}
	}
}
