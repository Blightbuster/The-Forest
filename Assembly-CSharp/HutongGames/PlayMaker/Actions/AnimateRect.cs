using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("AnimateVariables")]
	[Tooltip("Animates the value of a Rect Variable using an Animation Curve.")]
	public class AnimateRect : AnimateFsmAction
	{
		public override void Reset()
		{
			base.Reset();
			this.rectVariable = new FsmRect
			{
				UseVariable = true
			};
		}

		public override void OnEnter()
		{
			base.OnEnter();
			this.finishInNextStep = false;
			this.resultFloats = new float[4];
			this.fromFloats = new float[4];
			this.fromFloats[0] = ((!this.rectVariable.IsNone) ? this.rectVariable.Value.x : 0f);
			this.fromFloats[1] = ((!this.rectVariable.IsNone) ? this.rectVariable.Value.y : 0f);
			this.fromFloats[2] = ((!this.rectVariable.IsNone) ? this.rectVariable.Value.width : 0f);
			this.fromFloats[3] = ((!this.rectVariable.IsNone) ? this.rectVariable.Value.height : 0f);
			this.curves = new AnimationCurve[4];
			this.curves[0] = this.curveX.curve;
			this.curves[1] = this.curveY.curve;
			this.curves[2] = this.curveW.curve;
			this.curves[3] = this.curveH.curve;
			this.calculations = new AnimateFsmAction.Calculation[4];
			this.calculations[0] = this.calculationX;
			this.calculations[1] = this.calculationY;
			this.calculations[2] = this.calculationW;
			this.calculations[3] = this.calculationH;
			base.Init();
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			if (!this.rectVariable.IsNone && this.isRunning)
			{
				this.rct = new Rect(this.resultFloats[0], this.resultFloats[1], this.resultFloats[2], this.resultFloats[3]);
				this.rectVariable.Value = this.rct;
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
				if (!this.rectVariable.IsNone)
				{
					this.rct = new Rect(this.resultFloats[0], this.resultFloats[1], this.resultFloats[2], this.resultFloats[3]);
					this.rectVariable.Value = this.rct;
				}
				this.finishInNextStep = true;
			}
		}

		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmRect rectVariable;

		[RequiredField]
		public FsmAnimationCurve curveX;

		[Tooltip("Calculation lets you set a type of curve deformation that will be applied to rectVariable.x.")]
		public AnimateFsmAction.Calculation calculationX;

		[RequiredField]
		public FsmAnimationCurve curveY;

		[Tooltip("Calculation lets you set a type of curve deformation that will be applied to rectVariable.y.")]
		public AnimateFsmAction.Calculation calculationY;

		[RequiredField]
		public FsmAnimationCurve curveW;

		[Tooltip("Calculation lets you set a type of curve deformation that will be applied to rectVariable.width.")]
		public AnimateFsmAction.Calculation calculationW;

		[RequiredField]
		public FsmAnimationCurve curveH;

		[Tooltip("Calculation lets you set a type of curve deformation that will be applied to rectVariable.height.")]
		public AnimateFsmAction.Calculation calculationH;

		private bool finishInNextStep;

		private Rect rct;
	}
}
