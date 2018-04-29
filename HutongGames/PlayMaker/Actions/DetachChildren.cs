using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.GameObject)]
	[Tooltip("Unparents all children from the Game Object.")]
	public class DetachChildren : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
		}

		
		public override void OnEnter()
		{
			DetachChildren.DoDetachChildren(base.Fsm.GetOwnerDefaultTarget(this.gameObject));
			base.Finish();
		}

		
		private static void DoDetachChildren(GameObject go)
		{
			if (go != null)
			{
				go.transform.DetachChildren();
			}
		}

		
		[RequiredField]
		[Tooltip("GameObject to unparent children from.")]
		public FsmOwnerDefault gameObject;
	}
}
