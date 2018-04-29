using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("Quaternion")]
	[Tooltip("Get the quaternion from a quaternion multiplied by a quaternion.")]
	[HelpUrl("https:
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

		
		[RequiredField]
		[Tooltip("The first quaternion to multiply")]
		public FsmQuaternion quaternionA;

		
		[RequiredField]
		[Tooltip("The second quaternion to multiply")]
		public FsmQuaternion quaternionB;

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The resulting quaternion")]
		public FsmQuaternion result;

		
		[Tooltip("Repeat every frame.")]
		public bool everyFrame;
	}
}
