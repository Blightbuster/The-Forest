﻿using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Animator")]
	[Tooltip("Gets the value of a quaternion parameter")]
	[HelpUrl("https://hutonggames.fogbugz.com/default.asp?W1057")]
	public class GetAnimatorQuaternion : FsmStateAction
	{
		public override void Reset()
		{
			this.gameObject = null;
			this.parameter = null;
			this.result = null;
			this.everyFrame = false;
		}

		public override void OnEnter()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (ownerDefaultTarget == null)
			{
				base.Finish();
				return;
			}
			this._animator = ownerDefaultTarget.GetComponent<Animator>();
			if (this._animator == null)
			{
				base.Finish();
				return;
			}
			this._animatorProxy = ownerDefaultTarget.GetComponent<PlayMakerAnimatorMoveProxy>();
			if (this._animatorProxy != null)
			{
				this._animatorProxy.OnAnimatorMoveEvent += this.OnAnimatorMoveEvent;
			}
			this._paramID = Animator.StringToHash(this.parameter.Value);
			this.GetParameter();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		public void OnAnimatorMoveEvent()
		{
			if (this._animatorProxy != null)
			{
				this.GetParameter();
			}
		}

		public override void OnUpdate()
		{
			if (this._animatorProxy == null)
			{
				this.GetParameter();
			}
		}

		private void GetParameter()
		{
			if (this._animator != null)
			{
				this.result.Value = this._animator.GetQuaternion(this._paramID);
			}
		}

		public override void OnExit()
		{
			if (this._animatorProxy != null)
			{
				this._animatorProxy.OnAnimatorMoveEvent -= this.OnAnimatorMoveEvent;
			}
		}

		[RequiredField]
		[CheckForComponent(typeof(Animator))]
		[Tooltip("The target. An Animator component and a PlayMakerAnimatorProxy component are required")]
		public FsmOwnerDefault gameObject;

		[Tooltip("The animator parameter")]
		public FsmString parameter;

		[Tooltip("Repeat every frame. Useful when value is subject to change over time.")]
		public bool everyFrame;

		[ActionSection("Results")]
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The Quaternion value of the animator parameter")]
		public FsmQuaternion result;

		private PlayMakerAnimatorMoveProxy _animatorProxy;

		private Animator _animator;

		private int _paramID;
	}
}
