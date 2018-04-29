﻿using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("Animator")]
	[Tooltip("Sets the value of a float parameter")]
	[HelpUrl("https:
	public class SetAnimatorFloat : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.parameter = null;
			this.dampTime = null;
			this.Value = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			this._animatorGo = ownerDefaultTarget;
			if (ownerDefaultTarget == null)
			{
				base.Finish();
				return;
			}
			if (!this._animator)
			{
				this._animator = ownerDefaultTarget.GetComponent<Animator>();
			}
			if (this._animator == null)
			{
				base.Finish();
				return;
			}
			if (!this._animatorProxy)
			{
				this._animatorProxy = ownerDefaultTarget.GetComponent<PlayMakerAnimatorMoveProxy>();
			}
			if (this._animatorProxy != null)
			{
				this._animatorProxy.OnAnimatorMoveEvent += this.OnAnimatorMoveEvent;
			}
			if (this._paramID == 0)
			{
				this._paramID = Animator.StringToHash(this.parameter.Value);
			}
			this.SetParameter();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public void OnAnimatorMoveEvent()
		{
			if (this._animatorProxy != null)
			{
				this.SetParameter();
			}
		}

		
		public override void OnUpdate()
		{
			if (this._animatorProxy == null)
			{
				this.SetParameter();
			}
		}

		
		private void SetParameter()
		{
			if (this._animator != null && this._animatorGo.activeSelf)
			{
				if (this.dampTime.Value > 0f)
				{
					this._animator.SetFloatReflected(this._paramID, this.Value.Value, this.dampTime.Value, Time.deltaTime);
				}
				else
				{
					this._animator.SetFloatReflected(this.parameter.Value, this.Value.Value);
				}
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

		
		[Tooltip("The float value to assign to the animator parameter")]
		public FsmFloat Value;

		
		[Tooltip("Optional: The time allowed to parameter to reach the value. Requires everyFrame Checked on")]
		public FsmFloat dampTime;

		
		[Tooltip("Repeat every frame. Useful when changing over time.")]
		public bool everyFrame;

		
		private PlayMakerAnimatorMoveProxy _animatorProxy;

		
		private Animator _animator;

		
		private int _paramID;

		
		private GameObject _animatorGo;
	}
}
