using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Quaternion")]
	[Tooltip("Returns a rotation that rotates z degrees around the z axis, x degrees around the x axis, and y degrees around the y axis (in that order).")]
	[HelpUrl("https://hutonggames.fogbugz.com/default.asp?W1094")]
	public class QuaternionEuler : FsmStateAction
	{
		public override void Reset()
		{
			this.eulerAngles = null;
			this.result = null;
			this.everyFrame = true;
		}

		public override void OnEnter()
		{
			this.DoQuatEuler();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		public override void OnUpdate()
		{
			this.DoQuatEuler();
		}

		private void DoQuatEuler()
		{
			this.result.Value = Quaternion.Euler(this.eulerAngles.Value);
		}

		[RequiredField]
		[Tooltip("The Euler angles.")]
		public FsmVector3 eulerAngles;

		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the euler angles of this quaternion variable.")]
		public FsmQuaternion result;

		[Tooltip("Repeat every frame. Useful if any of the values are changing.")]
		public bool everyFrame;
	}
}
