using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Network)]
	[Tooltip("Test if the Network View is controlled by a GameObject.")]
	public class NetworkViewIsMine : FsmStateAction
	{
		
		private void _getNetworkView()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (ownerDefaultTarget == null)
			{
				return;
			}
			this._networkView = ownerDefaultTarget.GetComponent<NetworkView>();
		}

		
		public override void Reset()
		{
			this.gameObject = null;
			this.isMine = null;
			this.isMineEvent = null;
			this.isNotMineEvent = null;
		}

		
		public override void OnEnter()
		{
			this._getNetworkView();
			this.checkIsMine();
			base.Finish();
		}

		
		private void checkIsMine()
		{
			if (this._networkView == null)
			{
				return;
			}
			bool flag = this._networkView.isMine;
			this.isMine.Value = flag;
			if (flag)
			{
				if (this.isMineEvent != null)
				{
					base.Fsm.Event(this.isMineEvent);
				}
			}
			else if (this.isNotMineEvent != null)
			{
				base.Fsm.Event(this.isNotMineEvent);
			}
		}

		
		[RequiredField]
		[CheckForComponent(typeof(NetworkView))]
		[Tooltip("The Game Object with the NetworkView attached.")]
		public FsmOwnerDefault gameObject;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("True if the network view is controlled by this object.")]
		public FsmBool isMine;

		
		[Tooltip("Send this event if the network view controlled by this object.")]
		public FsmEvent isMineEvent;

		
		[Tooltip("Send this event if the network view is NOT controlled by this object.")]
		public FsmEvent isNotMineEvent;

		
		private NetworkView _networkView;
	}
}
