using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Animation)]
	[Tooltip("Rewinds the named animation.")]
	public class RewindAnimation : ComponentAction<Animation>
	{
		public override void Reset()
		{
			this.gameObject = null;
			this.animName = null;
		}

		public override void OnEnter()
		{
			this.DoRewindAnimation();
			base.Finish();
		}

		private void DoRewindAnimation()
		{
			if (string.IsNullOrEmpty(this.animName.Value))
			{
				return;
			}
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (base.UpdateCache(ownerDefaultTarget))
			{
				base.animation.Rewind(this.animName.Value);
			}
		}

		[RequiredField]
		[CheckForComponent(typeof(Animation))]
		public FsmOwnerDefault gameObject;

		[UIHint(UIHint.Animation)]
		public FsmString animName;
	}
}
