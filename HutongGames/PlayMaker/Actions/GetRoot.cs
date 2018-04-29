using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Gets the top most parent of the Game Object.\nIf the game object has no parent, returns itself.")]
	[ActionCategory(ActionCategory.GameObject)]
	public class GetRoot : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.storeRoot = null;
		}

		
		public override void OnEnter()
		{
			this.DoGetRoot();
			base.Finish();
		}

		
		private void DoGetRoot()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (ownerDefaultTarget == null)
			{
				return;
			}
			this.storeRoot.Value = ownerDefaultTarget.transform.root.gameObject;
		}

		
		[RequiredField]
		public FsmOwnerDefault gameObject;

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmGameObject storeRoot;
	}
}
