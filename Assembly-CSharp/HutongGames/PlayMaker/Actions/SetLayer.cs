using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.GameObject)]
	[Tooltip("Sets a Game Object's Layer.")]
	public class SetLayer : FsmStateAction
	{
		public override void Reset()
		{
			this.gameObject = null;
			this.layer = 0;
		}

		public override void OnEnter()
		{
			this.DoSetLayer();
			base.Finish();
		}

		private void DoSetLayer()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (ownerDefaultTarget == null)
			{
				return;
			}
			ownerDefaultTarget.layer = this.layer;
		}

		[RequiredField]
		public FsmOwnerDefault gameObject;

		[UIHint(UIHint.Layer)]
		public int layer;
	}
}
