using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("Quaternion")]
	[Tooltip("Inverse a quaternion")]
	public class QuaternionInverse : FsmStateAction
	{
		
		public override void Reset()
		{
			this.rotation = null;
			this.result = null;
			this.everyFrame = true;
		}

		
		public override void OnEnter()
		{
			this.DoQuatInverse();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoQuatInverse();
		}

		
		private void DoQuatInverse()
		{
			this.result.Value = Quaternion.Inverse(this.rotation.Value);
		}

		
		[RequiredField]
		[Tooltip("the rotation")]
		public FsmQuaternion rotation;

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the inverse of the rotation variable.")]
		public FsmQuaternion result;

		
		[Tooltip("Repeat every frame. Useful if any of the values are changing.")]
		public bool everyFrame;
	}
}
