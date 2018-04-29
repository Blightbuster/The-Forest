using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("Trigonometry")]
	[Tooltip("Get the Arc Tangent. You can get the result in degrees, simply check on the RadToDeg conversion")]
	public class GetAtan : FsmStateAction
	{
		
		public override void Reset()
		{
			this.Value = null;
			this.RadToDeg = true;
			this.everyFrame = false;
			this.angle = null;
		}

		
		public override void OnEnter()
		{
			this.DoATan();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoATan();
		}

		
		private void DoATan()
		{
			float num = Mathf.Atan(this.Value.Value);
			if (this.RadToDeg.Value)
			{
				num *= 57.29578f;
			}
			this.angle.Value = num;
		}

		
		[RequiredField]
		[Tooltip("The value of the tan")]
		public FsmFloat Value;

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The resulting angle. Note:If you want degrees, simply check RadToDeg")]
		public FsmFloat angle;

		
		[Tooltip("Check on if you want the angle expressed in degrees.")]
		public FsmBool RadToDeg;

		
		public bool everyFrame;
	}
}
