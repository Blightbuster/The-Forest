using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Check if this machine has a public IP address.")]
	[ActionCategory(ActionCategory.Network)]
	public class NetworkHavePublicIpAddress : FsmStateAction
	{
		
		public override void Reset()
		{
			this.havePublicIpAddress = null;
			this.publicIpAddressFoundEvent = null;
			this.publicIpAddressNotFoundEvent = null;
		}

		
		public override void OnEnter()
		{
			bool flag = Network.HavePublicAddress();
			this.havePublicIpAddress.Value = flag;
			if (flag && this.publicIpAddressFoundEvent != null)
			{
				base.Fsm.Event(this.publicIpAddressFoundEvent);
			}
			else if (!flag && this.publicIpAddressNotFoundEvent != null)
			{
				base.Fsm.Event(this.publicIpAddressNotFoundEvent);
			}
			base.Finish();
		}

		
		[Tooltip("True if this machine has a public IP address")]
		[UIHint(UIHint.Variable)]
		public FsmBool havePublicIpAddress;

		
		[Tooltip("Event to send if this machine has a public IP address")]
		public FsmEvent publicIpAddressFoundEvent;

		
		[Tooltip("Event to send if this machine has no public IP address")]
		public FsmEvent publicIpAddressNotFoundEvent;
	}
}
