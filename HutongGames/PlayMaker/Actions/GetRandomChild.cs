using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Gets a Random Child of a Game Object.")]
	[ActionCategory(ActionCategory.GameObject)]
	public class GetRandomChild : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.storeResult = null;
		}

		
		public override void OnEnter()
		{
			this.DoGetRandomChild();
			base.Finish();
		}

		
		private void DoGetRandomChild()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (ownerDefaultTarget == null)
			{
				return;
			}
			int childCount = ownerDefaultTarget.transform.childCount;
			if (childCount == 0)
			{
				return;
			}
			this.storeResult.Value = ownerDefaultTarget.transform.GetChild(UnityEngine.Random.Range(0, childCount)).gameObject;
		}

		
		[RequiredField]
		public FsmOwnerDefault gameObject;

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmGameObject storeResult;
	}
}
