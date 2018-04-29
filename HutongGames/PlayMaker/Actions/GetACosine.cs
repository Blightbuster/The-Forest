using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("Trigonometry")]
	[Tooltip("Get the Arc Cosine. You can get the result in degrees, simply check on the RadToDeg conversion")]
	public class GetACosine : FsmStateAction
	{
		
		public override void Reset()
		{
			this.angle = null;
			this.RadToDeg = true;
			this.everyFrame = false;
			this.Value = null;
		}

		
		public override void OnEnter()
		{
			this.DoACosine();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoACosine();
		}

		
		private void DoACosine()
		{
			float num = Mathf.Acos(this.Value.Value);
			if (this.RadToDeg.Value)
			{
				num *= 57.29578f;
			}
			this.angle.Value = num;
		}

		
		[Tooltip("The value of the cosine")]
		[RequiredField]
		public FsmFloat Value;

		
		[Tooltip("The resulting angle. Note:If you want degrees, simply check RadToDeg")]
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmFloat angle;

		
		[Tooltip("Check on if you want the angle expressed in degrees.")]
		public FsmBool RadToDeg;

		
		[Tooltip("Repeat every frame.")]
		public bool everyFrame;
	}
}
