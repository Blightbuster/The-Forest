using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Gets the Mass of a Game Object's Rigid Body.")]
	[ActionCategory(ActionCategory.Physics)]
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
		[Tooltip("The GameObject that owns the Rigidbody")]
		[CheckForComponent(typeof(Rigidbody))]
		public FsmOwnerDefault gameObject;

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the mass in a float variable.")]
		public FsmFloat storeResult;
	}
}
