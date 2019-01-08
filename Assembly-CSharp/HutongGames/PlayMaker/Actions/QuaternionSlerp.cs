using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Quaternion")]
	[Tooltip("Spherically interpolates between from and to by t.")]
	[HelpUrl("https://hutonggames.fogbugz.com/default.asp?W1093")]
	public class QuaternionSlerp : FsmStateAction
	{
		public override void Reset()
		{
			this.fromQuaternion = new FsmQuaternion
			{
				UseVariable = true
			};
			this.toQuaternion = new FsmQuaternion
			{
				UseVariable = true
			};
			this.amount = 0.1f;
			this.storeResult = null;
			this.everyFrame = true;
		}

		public override void OnEnter()
		{
			this.DoQuatSlerp();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		public override void OnUpdate()
		{
			this.DoQuatSlerp();
		}

		private void DoQuatSlerp()
		{
			this.storeResult.Value = Quaternion.Slerp(this.fromQuaternion.Value, this.toQuaternion.Value, this.amount.Value);
		}

		[RequiredField]
		[Tooltip("From Quaternion.")]
		public FsmQuaternion fromQuaternion;

		[RequiredField]
		[Tooltip("To Quaternion.")]
		public FsmQuaternion toQuaternion;

		[RequiredField]
		[Tooltip("Interpolate between fromQuaternion and toQuaternion by this amount. Value is clamped to 0-1 range. 0 = fromQuaternion; 1 = toQuaternion; 0.5 = half way between.")]
		[HasFloatSlider(0f, 1f)]
		public FsmFloat amount;

		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the result in this quaternion variable.")]
		public FsmQuaternion storeResult;

		[Tooltip("Repeat every frame. Useful if any of the values are changing. SHOULD BE ALWAYS TRUE")]
		public bool everyFrame;
	}
}
