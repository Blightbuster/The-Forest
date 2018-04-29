using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[HelpUrl("https:
	[ActionCategory("Quaternion")]
	[Tooltip("Get the quaternion from a quaternion multiplied by a quaternion.")]
	public class GetQuaternionMultipliedByQuaternion : FsmStateAction
	{
		
		public override void Reset()
		{
			this.quaternionA = null;
			this.quaternionB = null;
			this.result = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoQuatMult();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoQuatMult();
		}

		
		private void DoQuatMult()
		{
			this.result.Value = this.quaternionA.Value * this.quaternionB.Value;
		}

		
		[Tooltip("The first quaternion to multiply")]
		[RequiredField]
		public FsmQuaternion quaternionA;

		
		[Tooltip("The second quaternion to multiply")]
		[RequiredField]
		public FsmQuaternion quaternionB;

		
		[Tooltip("The resulting quaternion")]
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmQuaternion result;

		
		[Tooltip("Repeat every frame.")]
		public bool everyFrame;
	}
}
