using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Network)]
	[Tooltip("Get the network OnFailedToConnect or MasterServer OnFailedToConnectToMasterServer connection error message.")]
	public class NetworkGetOnFailedToConnectProperties : FsmStateAction
	{
		
		public override void Reset()
		{
			this.errorLabel = null;
			this.NoErrorEvent = null;
			this.RSAPublicKeyMismatchEvent = null;
			this.InvalidPasswordEvent = null;
			this.ConnectionFailedEvent = null;
			this.TooManyConnectedPlayersEvent = null;
			this.ConnectionBannedEvent = null;
			this.AlreadyConnectedToServerEvent = null;
			this.AlreadyConnectedToAnotherServerEvent = null;
			this.CreateSocketOrThreadFailureEvent = null;
			this.IncorrectParametersEvent = null;
			this.EmptyConnectTargetEvent = null;
			this.InternalDirectConnectFailedEvent = null;
			this.NATTargetNotConnectedEvent = null;
			this.NATTargetConnectionLostEvent = null;
			this.NATPunchthroughFailedEvent = null;
		}

		
		public override void OnEnter()
		{
			this.doGetNetworkErrorInfo();
			base.Finish();
		}

		
		private void doGetNetworkErrorInfo()
		{
			NetworkConnectionError connectionError = Fsm.EventData.ConnectionError;
			this.errorLabel.Value = connectionError.ToString();
			switch (connectionError + 5)
			{
			case NetworkConnectionError.NoError:
				if (this.InternalDirectConnectFailedEvent != null)
				{
					base.Fsm.Event(this.InternalDirectConnectFailedEvent);
				}
				break;
			case (NetworkConnectionError)1:
				if (this.EmptyConnectTargetEvent != null)
				{
					base.Fsm.Event(this.EmptyConnectTargetEvent);
				}
				break;
			case (NetworkConnectionError)2:
				if (this.IncorrectParametersEvent != null)
				{
					base.Fsm.Event(this.IncorrectParametersEvent);
				}
				break;
			case (NetworkConnectionError)3:
				if (this.CreateSocketOrThreadFailureEvent != null)
				{
					base.Fsm.Event(this.CreateSocketOrThreadFailureEvent);
				}
				break;
			case (NetworkConnectionError)4:
				if (this.AlreadyConnectedToAnotherServerEvent != null)
				{
					base.Fsm.Event(this.AlreadyConnectedToAnotherServerEvent);
				}
				break;
			case (NetworkConnectionError)5:
				if (this.NoErrorEvent != null)
				{
					base.Fsm.Event(this.NoErrorEvent);
				}
				break;
			default:
				switch (connectionError)
				{
				case NetworkConnectionError.ConnectionFailed:
					if (this.ConnectionFailedEvent != null)
					{
						base.Fsm.Event(this.ConnectionFailedEvent);
					}
					break;
				case NetworkConnectionError.AlreadyConnectedToServer:
					if (this.AlreadyConnectedToServerEvent != null)
					{
						base.Fsm.Event(this.AlreadyConnectedToServerEvent);
					}
					break;
				default:
					switch (connectionError)
					{
					case NetworkConnectionError.NATTargetNotConnected:
						if (this.NATTargetNotConnectedEvent != null)
						{
							base.Fsm.Event(this.NATTargetNotConnectedEvent);
						}
						break;
					case NetworkConnectionError.NATTargetConnectionLost:
						if (this.NATTargetConnectionLostEvent != null)
						{
							base.Fsm.Event(this.NATTargetConnectionLostEvent);
						}
						break;
					case NetworkConnectionError.NATPunchthroughFailed:
						if (this.NATPunchthroughFailedEvent != null)
						{
							base.Fsm.Event(this.NoErrorEvent);
						}
						break;
					}
					break;
				case NetworkConnectionError.TooManyConnectedPlayers:
					if (this.TooManyConnectedPlayersEvent != null)
					{
						base.Fsm.Event(this.TooManyConnectedPlayersEvent);
					}
					break;
				case NetworkConnectionError.RSAPublicKeyMismatch:
					if (this.RSAPublicKeyMismatchEvent != null)
					{
						base.Fsm.Event(this.RSAPublicKeyMismatchEvent);
					}
					break;
				case NetworkConnectionError.ConnectionBanned:
					if (this.ConnectionBannedEvent != null)
					{
						base.Fsm.Event(this.ConnectionBannedEvent);
					}
					break;
				case NetworkConnectionError.InvalidPassword:
					if (this.InvalidPasswordEvent != null)
					{
						base.Fsm.Event(this.InvalidPasswordEvent);
					}
					break;
				}
				break;
			}
		}

		
		[Tooltip("Error label")]
		[UIHint(UIHint.Variable)]
		public FsmString errorLabel;

		
		[Tooltip("No error occurred.")]
		public FsmEvent NoErrorEvent;

		
		[Tooltip("We presented an RSA public key which does not match what the system we connected to is using.")]
		public FsmEvent RSAPublicKeyMismatchEvent;

		
		[Tooltip("The server is using a password and has refused our connection because we did not set the correct password.")]
		public FsmEvent InvalidPasswordEvent;

		
		[Tooltip("onnection attempt failed, possibly because of internal connectivity problems.")]
		public FsmEvent ConnectionFailedEvent;

		
		[Tooltip("The server is at full capacity, failed to connect.")]
		public FsmEvent TooManyConnectedPlayersEvent;

		
		[Tooltip("We are banned from the system we attempted to connect to (likely temporarily).")]
		public FsmEvent ConnectionBannedEvent;

		
		[Tooltip("We are already connected to this particular server (can happen after fast disconnect/reconnect).")]
		public FsmEvent AlreadyConnectedToServerEvent;

		
		[Tooltip("Cannot connect to two servers at once. Close the connection before connecting again.")]
		public FsmEvent AlreadyConnectedToAnotherServerEvent;

		
		[Tooltip("Internal error while attempting to initialize network interface. Socket possibly already in use.")]
		public FsmEvent CreateSocketOrThreadFailureEvent;

		
		[Tooltip("Incorrect parameters given to Connect function.")]
		public FsmEvent IncorrectParametersEvent;

		
		[Tooltip("No host target given in Connect.")]
		public FsmEvent EmptyConnectTargetEvent;

		
		[Tooltip("Client could not connect internally to same network NAT enabled server.")]
		public FsmEvent InternalDirectConnectFailedEvent;

		
		[Tooltip("The NAT target we are trying to connect to is not connected to the facilitator server.")]
		public FsmEvent NATTargetNotConnectedEvent;

		
		[Tooltip("Connection lost while attempting to connect to NAT target.")]
		public FsmEvent NATTargetConnectionLostEvent;

		
		[Tooltip("NAT punchthrough attempt has failed. The cause could be a too restrictive NAT implementation on either endpoints.")]
		public FsmEvent NATPunchthroughFailedEvent;
	}
}
