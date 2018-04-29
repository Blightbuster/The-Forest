using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Register this server on the master server.\n\nIf the master server address information has not been changed the default Unity master server will be used.")]
	[ActionCategory(ActionCategory.Network)]
	public class MasterServerRegisterHost : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameTypeName = null;
			this.gameName = null;
			this.comment = null;
		}

		
		public override void OnEnter()
		{
			this.DoMasterServerRegisterHost();
			base.Finish();
		}

		
		private void DoMasterServerRegisterHost()
		{
			MasterServer.RegisterHost(this.gameTypeName.Value, this.gameName.Value, this.comment.Value);
		}

		
		[Tooltip("The unique game type name.")]
		[RequiredField]
		public FsmString gameTypeName;

		
		[RequiredField]
		[Tooltip("The game name.")]
		public FsmString gameName;

		
		[Tooltip("Optional comment")]
		public FsmString comment;
	}
}
