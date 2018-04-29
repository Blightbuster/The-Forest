using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Gets the value of a curve at a given time and stores it in a Float Variable. NOTE: This can be used for more than just animation! It's a general way to transform an input number into an output number using a curve (e.g., linear input -> bell curve).")]
	[ActionCategory(ActionCategory.Math)]
	public class SampleCurve : FsmStateAction
	{
		
		public override void Reset()
		{
			this.curve = null;
			this.sampleAt = null;
			this.storeValue = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoSampleCurve();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoSampleCurve();
		}

		
		private void DoSampleCurve()
		{
			if (this.curve == null || this.curve.curve == null || this.storeValue == null)
			{
				return;
			}
			this.storeValue.Value = this.curve.curve.Evaluate(this.sampleAt.Value);
		}

		
		[RequiredField]
		public FsmAnimationCurve curve;

		
		[RequiredField]
		public FsmFloat sampleAt;

		
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmFloat storeValue;

		
		public bool everyFrame;
	}
}
