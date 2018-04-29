using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("Animator")]
	[Tooltip("Returns if additionnal layers affects the mass center")]
	[HelpUrl("https:
	public class GetAnimatorLayersAffectMassCenter : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.affectMassCenter = null;
			this.affectMassCenterEvent = null;
			this.doNotAffectMassCenterEvent = null;
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
			this.CheckAffectMassCenter();
			base.Finish();
		}

		
		private void CheckAffectMassCenter()
		{
			if (this._animator == null)
			{
				return;
			}
			bool layersAffectMassCenter = this._animator.layersAffectMassCenter;
			this.affectMassCenter.Value = layersAffectMassCenter;
			if (layersAffectMassCenter)
			{
				base.Fsm.Event(this.affectMassCenterEvent);
			}
			else
			{
				base.Fsm.Event(this.doNotAffectMassCenterEvent);
			}
		}

		
		[RequiredField]
		[CheckForComponent(typeof(Animator))]
		[Tooltip("The Target. An Animator component is required")]
		public FsmOwnerDefault gameObject;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("If true, additionnal layers affects the mass center")]
		[ActionSection("Results")]
		[RequiredField]
		public FsmBool affectMassCenter;

		
		[Tooltip("Event send if additionnal layers affects the mass center")]
		public FsmEvent affectMassCenterEvent;

		
		[Tooltip("Event send if additionnal layers do no affects the mass center")]
		public FsmEvent doNotAffectMassCenterEvent;

		
		private Animator _animator;
	}
}
