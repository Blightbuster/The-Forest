using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Clamps the value of Float Variable to a Min/Max range.")]
	[ActionCategory(ActionCategory.Math)]
	public class FloatClamp : FsmStateAction
	{
		
		public override void Reset()
		{
			this.floatVariable = null;
			this.minValue = null;
			this.maxValue = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoClamp();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoClamp();
		}

		
		private void DoClamp()
		{
			this.floatVariable.Value = Mathf.Clamp(this.floatVariable.Value, this.minValue.Value, this.maxValue.Value);
		}

		
		[RequiredField]
		[Tooltip("Float variable to clamp.")]
		[UIHint(UIHint.Variable)]
		public FsmFloat floatVariable;

		
		[RequiredField]
		[Tooltip("The minimum value.")]
		public FsmFloat minValue;

		
		[Tooltip("The maximum value.")]
		[RequiredField]
		public FsmFloat maxValue;

		
		[Tooltip("Repeate every frame. Useful if the float variable is changing.")]
		public bool everyFrame;
	}
}
