using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("Animator")]
	[Tooltip("When turned on, animations will be executed in the physics loop. This is only useful in conjunction with kinematic rigidbodies.")]
	[HelpUrl("https:
	public class SetAnimatorAnimatePhysics : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.animatePhysics = null;
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
			this.DoAnimatePhysics();
			base.Finish();
		}

		
		private void DoAnimatePhysics()
		{
			if (this._animator == null)
			{
				return;
			}
			this._animator.animatePhysics = this.animatePhysics.Value;
		}

		
		[CheckForComponent(typeof(Animator))]
		[Tooltip("The Target. An Animator component is required")]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("If true, animations will be executed in the physics loop. This is only useful in conjunction with kinematic rigidbodies.")]
		public FsmBool animatePhysics;

		
		private Animator _animator;
	}
}
