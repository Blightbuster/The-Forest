using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Forces a Game Object's Rigid Body to wake up.")]
	[ActionCategory(ActionCategory.Physics)]
	public class WakeUp : ComponentAction<Rigidbody>
	{
		
		public override void Reset()
		{
			this.gameObject = null;
		}

		
		public override void OnEnter()
		{
			this.DoWakeUp();
			base.Finish();
		}

		
		private void DoWakeUp()
		{
			GameObject go = (this.gameObject.OwnerOption != OwnerDefaultOption.UseOwner) ? this.gameObject.GameObject.Value : base.Owner;
			if (base.UpdateCache(go))
			{
				base.rigidbody.WakeUp();
			}
		}

		
		[CheckForComponent(typeof(Rigidbody))]
		[RequiredField]
		public FsmOwnerDefault gameObject;
	}
}
