using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Animator")]
	[Tooltip("Gets the position and rotation of the target specified by SetTarget(AvatarTarget targetIndex, float targetNormalizedTime)).\nThe position and rotation are only valid when a frame has being evaluated after the SetTarget call")]
	[HelpUrl("https://hutonggames.fogbugz.com/default.asp?W1058")]
	public class GetAnimatorTarget : FsmStateAction
	{
		public override void Reset()
		{
			this.gameObject = null;
			this.targetPosition = null;
			this.targetRotation = null;
			this.targetGameObject = null;
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
			GameObject value = this.targetGameObject.Value;
			if (value != null)
			{
				this._transform = value.transform;
			}
			this.DoGetTarget();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		public override void OnUpdate()
		{
			if (this._animatorProxy == null)
			{
				this.DoGetTarget();
			}
		}

		public void OnAnimatorMoveEvent()
		{
			this.DoGetTarget();
		}

		private void DoGetTarget()
		{
			if (this._animator == null)
			{
				return;
			}
			this.targetPosition.Value = this._animator.targetPosition;
			this.targetRotation.Value = this._animator.targetRotation;
			if (this._transform != null)
			{
				this._transform.position = this._animator.targetPosition;
				this._transform.rotation = this._animator.targetRotation;
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

		[Tooltip("Repeat every frame. Useful when value is subject to change over time.")]
		public bool everyFrame;

		[ActionSection("Results")]
		[UIHint(UIHint.Variable)]
		[Tooltip("The target position")]
		public FsmVector3 targetPosition;

		[UIHint(UIHint.Variable)]
		[Tooltip("The target rotation")]
		public FsmQuaternion targetRotation;

		[Tooltip("If set, apply the position and rotation to this gameObject")]
		public FsmGameObject targetGameObject;

		private PlayMakerAnimatorMoveProxy _animatorProxy;

		private Animator _animator;

		private Transform _transform;
	}
}
