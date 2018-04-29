using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Animates the value of a Float Variable using an Animation Curve.")]
	[ActionCategory(ActionCategory.AnimateVariables)]
	public class AnimateFloatV2 : AnimateFsmAction
	{
		
		public override void Reset()
		{
			base.Reset();
			this.floatVariable = new FsmFloat
			{
				UseVariable = true
			};
		}

		
		public override void OnEnter()
		{
			base.OnEnter();
			this.finishInNextStep = false;
			this.resultFloats = new float[1];
			this.fromFloats = new float[1];
			this.fromFloats[0] = ((!this.floatVariable.IsNone) ? this.floatVariable.Value : 0f);
			this.calculations = new AnimateFsmAction.Calculation[1];
			this.calculations[0] = this.calculation;
			this.curves = new AnimationCurve[1];
			this.curves[0] = this.animCurve.curve;
			base.Init();
		}

		
		public override void OnExit()
		{
		}

		
		public override void OnUpdate()
		{
			base.OnUpdate();
			if (!this.floatVariable.IsNone && this.isRunning)
			{
				this.floatVariable.Value = this.resultFloats[0];
			}
			if (this.finishInNextStep && !this.looping)
			{
				base.Finish();
				if (this.finishEvent != null)
				{
					base.Fsm.Event(this.finishEvent);
				}
			}
			if (this.finishAction && !this.finishInNextStep)
			{
				if (!this.floatVariable.IsNone)
				{
					this.floatVariable.Value = this.resultFloats[0];
				}
				this.finishInNextStep = true;
			}
		}

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmFloat floatVariable;

		
		[RequiredField]
		public FsmAnimationCurve animCurve;

		
		[Tooltip("Calculation lets you set a type of curve deformation that will be applied to floatVariable")]
		public AnimateFsmAction.Calculation calculation;

		
		private bool finishInNextStep;
	}
}
