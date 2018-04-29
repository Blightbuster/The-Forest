using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("Quaternion")]
	[Tooltip("Creates a rotation which rotates angle degrees around axis.")]
	[HelpUrl("https:
	public class QuaternionAngleAxis : FsmStateAction
	{
		
		public override void Reset()
		{
			this.angle = null;
			this.axis = null;
			this.result = null;
			this.everyFrame = true;
		}

		
		public override void OnEnter()
		{
			this.DoQuatAngleAxis();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoQuatAngleAxis();
		}

		
		private void DoQuatAngleAxis()
		{
			this.result.Value = Quaternion.AngleAxis(this.angle.Value, this.axis.Value);
		}

		
		[RequiredField]
		[Tooltip("The angle.")]
		public FsmFloat angle;

		
		[RequiredField]
		[Tooltip("The rotation axis.")]
		public FsmVector3 axis;

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the rotation of this quaternion variable.")]
		public FsmQuaternion result;

		
		[Tooltip("Repeat every frame. Useful if any of the values are changing.")]
		public bool everyFrame;
	}
}
