using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("Animator")]
	[Tooltip("If true, additionnal layers affects the mass center")]
	[HelpUrl("https:
	public class SetAnimatorLayersAffectMassCenter : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.affectMassCenter = null;
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
			this.SetAffectMassCenter();
			base.Finish();
		}

		
		private void SetAffectMassCenter()
		{
			if (this._animator == null)
			{
				return;
			}
			this._animator.layersAffectMassCenter = this.affectMassCenter.Value;
		}

		
		[RequiredField]
		[CheckForComponent(typeof(Animator))]
		[Tooltip("The Target. An Animator component is required")]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("If true, additionnal layers affects the mass center")]
		public FsmBool affectMassCenter;

		
		private Animator _animator;
	}
}
