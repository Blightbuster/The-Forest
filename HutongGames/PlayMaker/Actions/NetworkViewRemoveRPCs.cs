using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Remove the RPC function calls accociated with a Game Object.\n\nNOTE: The Game Object must have a NetworkView component attached.")]
	[ActionCategory(ActionCategory.Network)]
	public class NetworkViewRemoveRPCs : ComponentAction<NetworkView>
	{
		
		public override void Reset()
		{
			this.gameObject = null;
		}

		
		public override void OnEnter()
		{
			this.DoRemoveRPCsFromViewID();
			base.Finish();
		}

		
		private void DoRemoveRPCsFromViewID()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (base.UpdateCache(ownerDefaultTarget))
			{
				Network.RemoveRPCs(base.networkView.viewID);
			}
		}

		
		[CheckForComponent(typeof(NetworkView))]
		[Tooltip("Remove the RPC function calls accociated with this Game Object.\n\nNOTE: The GameObject must have a NetworkView component attached.")]
		[RequiredField]
		public FsmOwnerDefault gameObject;
	}
}
