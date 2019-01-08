using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Animator")]
	[Tooltip("Set Apply Root Motion: If true, Root is controlled by animations")]
	[HelpUrl("https://hutonggames.fogbugz.com/default.asp?W1061")]
	public class SetAnimatorApplyRootMotion : FsmStateAction
	{
		public override void Reset()
		{
			this.gameObject = null;
			this.applyRootMotion = null;
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
			this.DoApplyRootMotion();
			base.Finish();
		}

		private void DoApplyRootMotion()
		{
			if (this._animator == null)
			{
				return;
			}
			this._animator.applyRootMotion = this.applyRootMotion.Value;
		}

		[RequiredField]
		[CheckForComponent(typeof(Animator))]
		[Tooltip("The Target. An Animator component is required")]
		public FsmOwnerDefault gameObject;

		[Tooltip("If true, Root is controlled by animations")]
		public FsmBool applyRootMotion;

		private Animator _animator;
	}
}
