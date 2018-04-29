using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("Quaternion")]
	[Tooltip("Get the vector3 from a quaternion multiplied by a vector.")]
	[HelpUrl("https:
	public class GetQuaternionMultipliedByVector : FsmStateAction
	{
		
		public override void Reset()
		{
			this.quaternion = null;
			this.vector3 = null;
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
			this.result.Value = this.quaternion.Value * this.vector3.Value;
		}

		
		[RequiredField]
		[Tooltip("The quaternion to multiply")]
		public FsmQuaternion quaternion;

		
		[RequiredField]
		[Tooltip("The vector3 to multiply")]
		public FsmVector3 vector3;

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The resulting vector3")]
		public FsmVector3 result;

		
		[Tooltip("Repeat every frame.")]
		public bool everyFrame;
	}
}
