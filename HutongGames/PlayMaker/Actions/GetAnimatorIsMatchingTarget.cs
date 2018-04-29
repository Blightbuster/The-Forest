﻿using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("Animator")]
	[Tooltip("Returns true if automatic matching is active. Can also send events")]
	public class GetAnimatorIsMatchingTarget : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.isMatchingActive = null;
			this.matchingActivatedEvent = null;
			this.matchingDeactivedEvent = null;
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
			this.DoCheckIsMatchingActive();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			if (this._animatorProxy == null)
			{
				this.DoCheckIsMatchingActive();
			}
		}

		
		public void OnAnimatorMoveEvent()
		{
			this.DoCheckIsMatchingActive();
		}

		
		private void DoCheckIsMatchingActive()
		{
			if (this._animator == null)
			{
				return;
			}
			bool isMatchingTarget = this._animator.isMatchingTarget;
			this.isMatchingActive.Value = isMatchingTarget;
			if (isMatchingTarget)
			{
				base.Fsm.Event(this.matchingActivatedEvent);
			}
			else
			{
				base.Fsm.Event(this.matchingDeactivedEvent);
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

		
		[Tooltip("Repeat every frame. Warning: do not use the events in this action if you set everyFrame to true")]
		public bool everyFrame;

		
		[ActionSection("Results")]
		[UIHint(UIHint.Variable)]
		[Tooltip("True if automatic matching is active")]
		public FsmBool isMatchingActive;

		
		[Tooltip("Event send if automatic matching is active")]
		public FsmEvent matchingActivatedEvent;

		
		[Tooltip("Event send if automatic matching is not active")]
		public FsmEvent matchingDeactivedEvent;

		
		private PlayMakerAnimatorMoveProxy _animatorProxy;

		
		private Animator _animator;
	}
}
