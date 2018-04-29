using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Creates a rotation which rotates from fromDirection to toDirection. Usually you use this to rotate a transform so that one of its axes eg. the y-axis - follows a target direction toDirection in world space.")]
	[ActionCategory("Quaternion")]
	[HelpUrl("https:
	public class GetQuaternionFromRotation : FsmStateAction
	{
		
		public override void Reset()
		{
			this.fromDirection = null;
			this.toDirection = null;
			this.result = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoQuatFromRotation();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoQuatFromRotation();
		}

		
		private void DoQuatFromRotation()
		{
			this.result.Value = Quaternion.FromToRotation(this.fromDirection.Value, this.toDirection.Value);
		}

		
		[RequiredField]
		[Tooltip("the 'from' direction")]
		public FsmVector3 fromDirection;

		
		[Tooltip("the 'to' direction")]
		[RequiredField]
		public FsmVector3 toDirection;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("the resulting quaternion")]
		[RequiredField]
		public FsmQuaternion result;

		
		[Tooltip("Repeat every frame.")]
		public bool everyFrame;
	}
}
