using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Vector3)]
	[Tooltip("Linearly interpolates between 2 vectors.")]
	public class Vector3Lerp : FsmStateAction
	{
		
		public override void Reset()
		{
			this.fromVector = new FsmVector3
			{
				UseVariable = true
			};
			this.toVector = new FsmVector3
			{
				UseVariable = true
			};
			this.storeResult = null;
			this.everyFrame = true;
		}

		
		public override void OnEnter()
		{
			this.DoVector3Lerp();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoVector3Lerp();
		}

		
		private void DoVector3Lerp()
		{
			this.storeResult.Value = Vector3.Lerp(this.fromVector.Value, this.toVector.Value, this.amount.Value);
		}

		
		[Tooltip("First Vector.")]
		[RequiredField]
		public FsmVector3 fromVector;

		
		[RequiredField]
		[Tooltip("Second Vector.")]
		public FsmVector3 toVector;

		
		[RequiredField]
		[Tooltip("Interpolate between From Vector and ToVector by this amount. Value is clamped to 0-1 range. 0 = From Vector; 1 = To Vector; 0.5 = half way between.")]
		public FsmFloat amount;

		
		[Tooltip("Store the result in this vector variable.")]
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmVector3 storeResult;

		
		[Tooltip("Repeat every frame. Useful if any of the values are changing.")]
		public bool everyFrame;
	}
}
