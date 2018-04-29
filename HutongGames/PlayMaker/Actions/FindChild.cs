using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.GameObject)]
	[Tooltip("Finds the Child of a GameObject by Name.\nNote, you can specify a path to the child, e.g., LeftShoulder/Arm/Hand/Finger. If you need to specify a tag, use GetChild.")]
	public class FindChild : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.childName = string.Empty;
			this.storeResult = null;
		}

		
		public override void OnEnter()
		{
			this.DoFindChild();
			base.Finish();
		}

		
		private void DoFindChild()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (ownerDefaultTarget == null)
			{
				return;
			}
			Transform transform = ownerDefaultTarget.transform.Find(this.childName.Value);
			this.storeResult.Value = ((!(transform != null)) ? null : transform.gameObject);
		}

		
		[RequiredField]
		[Tooltip("The GameObject to search.")]
		public FsmOwnerDefault gameObject;

		
		[RequiredField]
		[Tooltip("The name of the child. Note, you can specify a path to the child, e.g., LeftShoulder/Arm/Hand/Finger")]
		public FsmString childName;

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the child in a GameObject variable.")]
		public FsmGameObject storeResult;
	}
}
