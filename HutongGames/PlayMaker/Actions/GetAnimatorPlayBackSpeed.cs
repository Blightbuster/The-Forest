using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[HelpUrl("https:
	[Tooltip("Gets the playback speed of the Animator. 1 is normal playback speed")]
	[ActionCategory("Animator")]
	public class GetAnimatorPlayBackSpeed : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.playBackSpeed = null;
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
			this.GetPlayBackSpeed();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.GetPlayBackSpeed();
		}

		
		private void GetPlayBackSpeed()
		{
			if (this._animator != null)
			{
				this.playBackSpeed.Value = this._animator.speed;
			}
		}

		
		[CheckForComponent(typeof(Animator))]
		[Tooltip("The Target. An Animator component is required")]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("The playBack speed of the animator. 1 is normal playback speed")]
		[RequiredField]
		public FsmFloat playBackSpeed;

		
		[Tooltip("Repeat every frame. Useful when value is subject to change over time.")]
		public bool everyFrame;

		
		private Animator _animator;
	}
}
