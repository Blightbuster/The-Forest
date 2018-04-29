using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Tests if a GameObject is a Child of another GameObject.")]
	[ActionCategory(ActionCategory.Logic)]
	public class GameObjectIsChildOf : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.isChildOf = null;
			this.trueEvent = null;
			this.falseEvent = null;
			this.storeResult = null;
		}

		
		public override void OnEnter()
		{
			this.DoIsChildOf(base.Fsm.GetOwnerDefaultTarget(this.gameObject));
			base.Finish();
		}

		
		private void DoIsChildOf(GameObject go)
		{
			if (go == null || this.isChildOf == null)
			{
				return;
			}
			bool flag = go.transform.IsChildOf(this.isChildOf.Value.transform);
			this.storeResult.Value = flag;
			base.Fsm.Event((!flag) ? this.falseEvent : this.trueEvent);
		}

		
		[RequiredField]
		[Tooltip("GameObject to test.")]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Is it a child of this GameObject?")]
		[RequiredField]
		public FsmGameObject isChildOf;

		
		[Tooltip("Event to send if GameObject is a child.")]
		public FsmEvent trueEvent;

		
		[Tooltip("Event to send if GameObject is NOT a child.")]
		public FsmEvent falseEvent;

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Store result in a bool variable")]
		public FsmBool storeResult;
	}
}
