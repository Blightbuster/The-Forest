using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Sets whether a Game Object's Rigidy Body is affected by Gravity.")]
	[ActionCategory(ActionCategory.Physics)]
	public class UseGravity : ComponentAction<Rigidbody>
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.useGravity = true;
		}

		
		public override void OnEnter()
		{
			this.DoUseGravity();
			base.Finish();
		}

		
		private void DoUseGravity()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (base.UpdateCache(ownerDefaultTarget))
			{
				base.rigidbody.useGravity = this.useGravity.Value;
			}
		}

		
		[CheckForComponent(typeof(Rigidbody))]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		
		[RequiredField]
		public FsmBool useGravity;
	}
}
