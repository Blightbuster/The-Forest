using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Controls whether physics affects the Game Object.")]
	[ActionCategory(ActionCategory.Physics)]
	public class SetIsKinematic : ComponentAction<Rigidbody>
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.isKinematic = false;
		}

		
		public override void OnEnter()
		{
			this.DoSetIsKinematic();
			base.Finish();
		}

		
		private void DoSetIsKinematic()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (base.UpdateCache(ownerDefaultTarget))
			{
				base.rigidbody.isKinematic = this.isKinematic.Value;
			}
		}

		
		[RequiredField]
		[CheckForComponent(typeof(Rigidbody))]
		public FsmOwnerDefault gameObject;

		
		[RequiredField]
		public FsmBool isKinematic;
	}
}
