using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Physics)]
	[Tooltip("Gets the Mass of a Game Object's Rigid Body.")]
	public class GetMass : ComponentAction<Rigidbody>
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.storeResult = null;
		}

		
		public override void OnEnter()
		{
			this.DoGetMass();
			base.Finish();
		}

		
		private void DoGetMass()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (base.UpdateCache(ownerDefaultTarget))
			{
				this.storeResult.Value = base.rigidbody.mass;
			}
		}

		
		[RequiredField]
		[CheckForComponent(typeof(Rigidbody))]
		[Tooltip("The GameObject that owns the Rigidbody")]
		public FsmOwnerDefault gameObject;

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the mass in a float variable.")]
		public FsmFloat storeResult;
	}
}
