using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Math)]
	[Tooltip("Subtracts a value from a Float Variable.")]
	public class FloatSubtract : FsmStateAction
	{
		
		public override void Reset()
		{
			this.floatVariable = null;
			this.subtract = null;
			this.everyFrame = false;
			this.perSecond = false;
		}

		
		public override void OnEnter()
		{
			this.DoFloatSubtract();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoFloatSubtract();
		}

		
		private void DoFloatSubtract()
		{
			if (!this.perSecond)
			{
				this.floatVariable.Value -= this.subtract.Value;
			}
			else
			{
				this.floatVariable.Value -= this.subtract.Value * Time.deltaTime;
			}
		}

		
		[RequiredField]
		[Tooltip("The float variable to subtract from.")]
		[UIHint(UIHint.Variable)]
		public FsmFloat floatVariable;

		
		[RequiredField]
		[Tooltip("Value to subtract from the float variable.")]
		public FsmFloat subtract;

		
		[Tooltip("Repeat every frame while the state is active.")]
		public bool everyFrame;

		
		[Tooltip("Used with Every Frame. Adds the value over one second to make the operation frame rate independent.")]
		public bool perSecond;
	}
}
