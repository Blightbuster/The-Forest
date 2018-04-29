using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.GameObject)]
	[Tooltip("Sets a Game Object's Name.")]
	public class SetName : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.name = null;
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
			ownerDefaultTarget.name = this.name.Value;
		}

		
		[RequiredField]
		public FsmOwnerDefault gameObject;

		
		[RequiredField]
		public FsmString name;
	}
}
