using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Convert)]
	[Tooltip("Converts a Float value to an Integer value.")]
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
			ConvertFloatToInt.FloatRounding floatRounding = this.rounding;
			if (floatRounding != ConvertFloatToInt.FloatRounding.Nearest)
			{
				if (floatRounding != ConvertFloatToInt.FloatRounding.RoundDown)
				{
					if (floatRounding == ConvertFloatToInt.FloatRounding.RoundUp)
					{
						this.intVariable.Value = Mathf.CeilToInt(this.floatVariable.Value);
					}
				}
				else
				{
					this.intVariable.Value = Mathf.FloorToInt(this.floatVariable.Value);
				}
			}
			else
			{
				this.intVariable.Value = Mathf.RoundToInt(this.floatVariable.Value);
			}
		}

		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The Float variable to convert to an integer.")]
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
