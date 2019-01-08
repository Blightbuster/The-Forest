using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Animator")]
	[Tooltip("If true, automaticaly stabilize feet during transition and blending")]
	[HelpUrl("https://hutonggames.fogbugz.com/default.asp?W1074")]
	public class SetAnimatorStabilizeFeet : FsmStateAction
	{
		public override void Reset()
		{
			this.gameObject = null;
			this.stabilizeFeet = null;
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
			this.DoStabilizeFeet();
			base.Finish();
		}

		private void DoStabilizeFeet()
		{
			if (this._animator == null)
			{
				return;
			}
			this._animator.stabilizeFeet = this.stabilizeFeet.Value;
		}

		[RequiredField]
		[CheckForComponent(typeof(Animator))]
		[Tooltip("The Target. An Animator component is required")]
		public FsmOwnerDefault gameObject;

		[Tooltip("If true, automaticaly stabilize feet during transition and blending")]
		public FsmBool stabilizeFeet;

		private Animator _animator;
	}
}
