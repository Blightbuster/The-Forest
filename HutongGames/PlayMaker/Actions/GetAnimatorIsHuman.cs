using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Returns true if the current rig is humanoid, false if it is generic. Can also sends events")]
	[ActionCategory("Animator")]
	public class GetAnimatorIsHuman : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.isHuman = null;
			this.isHumanEvent = null;
			this.isGenericEvent = null;
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
			this.DoCheckIsHuman();
			base.Finish();
		}

		
		private void DoCheckIsHuman()
		{
			if (this._animator == null)
			{
				return;
			}
			bool flag = this._animator.isHuman;
			this.isHuman.Value = flag;
			if (flag)
			{
				base.Fsm.Event(this.isHumanEvent);
			}
			else
			{
				base.Fsm.Event(this.isGenericEvent);
			}
		}

		
		[Tooltip("The Target. An Animator component is required")]
		[CheckForComponent(typeof(Animator))]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("True if the current rig is humanoid, False if it is generic")]
		[UIHint(UIHint.Variable)]
		[ActionSection("Results")]
		public FsmBool isHuman;

		
		[Tooltip("Event send if rig is humanoid")]
		public FsmEvent isHumanEvent;

		
		[Tooltip("Event send if rig is generic")]
		public FsmEvent isGenericEvent;

		
		private Animator _animator;
	}
}
