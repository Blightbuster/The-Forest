using System;
using System.Collections;
using System.Net;
using Steamworks;
using TheForest.Commons.Enums;
using TheForest.Utils;
using UdpKit;
using UniLinq;
using UnityEngine;

namespace TheForest.Networking
{
	
	public class AutoJoinAfterMPInvite : MonoBehaviour
	{
		
		private IEnumerator Start()
		{
			while (!SteamManager.Initialized || string.IsNullOrEmpty(SteamUser.GetSteamID().ToString()))
			{
				yield return null;
			}
			if (CoopLobby.IsInLobby)
			{
				CoopLobby.LeaveActive();
			}
			if (!AutoJoinAfterMPInvite.Done)
			{
				AutoJoinAfterMPInvite.Done = true;
				string[] commandLineArgs = Environment.GetCommandLineArgs();
				if (commandLineArgs.Contains("+connect_lobby"))
				{
					int num = commandLineArgs.IndexOf("+connect_lobby");
					this.invitedTo = ulong.Parse(commandLineArgs[num + 1]);
					this.SetInvitedToGameId(this.invitedTo);
					yield break;
				}
				if (commandLineArgs.Contains("+connect"))
				{
					int num2 = commandLineArgs.IndexOf("+connect");
					this.invitedToIP = commandLineArgs[num2 + 1];
					if (commandLineArgs.Contains("+password"))
					{
						int num3 = commandLineArgs.IndexOf("+password");
						this.invitedToPassword = commandLineArgs[num3 + 1];
					}
					else
					{
						this.invitedToPassword = string.Empty;
					}
					this.SetInvitedToServer(this.invitedToIP);
					yield break;
				}
			}
			AutoJoinAfterMPInvite.LobbyID = null;
			yield break;
		}

		
		public void SetInvitedToGameId(ulong gameId)
		{
			CSteamID y = new CSteamID(gameId);
			if (y.IsValid() && (CoopLobby.Instance == null || CoopLobby.Instance.Info.LobbyId != y))
			{
				base.StartCoroutine(this.DelayedInviteReceived(new CoopLobbyInfo(gameId)));
			}
		}

		
		public void SetInvitedToServer(string ip)
		{
			this.invitedToIP = ip;
			string[] array = ip.Split(new char[]
			{
				':'
			});
			string ipString = array[0];
			ushort port = ushort.Parse(array[1]);
			IPAddress ipaddress = IPAddress.Parse(ipString);
			byte[] addressBytes = ipaddress.GetAddressBytes();
			uint num = (uint)((uint)addressBytes[0] << 24);
			num += (uint)((uint)addressBytes[1] << 16);
			num += (uint)((uint)addressBytes[2] << 8);
			num += (uint)addressBytes[3];
			base.StartCoroutine(this.DelayedServerInviteReceived(num, port));
		}

		
		private IEnumerator DelayedInviteReceived(CoopLobbyInfo lobby)
		{
			lobby.RequestData();
			while (!lobby.Destroyed && (string.IsNullOrEmpty(lobby.Name) || string.IsNullOrEmpty(lobby.Guid) || this._inviteReceivedScreen._screen.activeSelf))
			{
				lobby.UpdateData();
				yield return null;
			}
			if (lobby.Destroyed)
			{
				this._inviteReceivedScreen._screen.SetActive(false);
				this._menu.SetActive(true);
				yield break;
			}
			yield return YieldPresets.WaitPointFiveSeconds;
			lobby.UpdateData();
			Debug.Log(string.Concat(new string[]
			{
				"Received MP invite for lobby name='",
				lobby.Name,
				"', guid='",
				lobby.Guid,
				"'"
			}));
			this._inviteReceivedScreen._lobby = lobby;
			this._inviteReceivedScreen._continueSaveButton.isEnabled = SaveSlotUtils.GetPreviouslyPlayedServers().Contains(lobby.Guid);
			this._inviteReceivedScreen._gameName.text = lobby.Name;
			this._inviteReceivedScreen._screen.SetActive(true);
			this._menu.SetActive(false);
			UICamera.hoveredObject = null;
			yield break;
		}

		
		private void ServerResponded(gameserveritem_t server)
		{
			Debug.Log("AutoJoin: Server ping succeeded");
			Debug.Log("Starting dedicated client");
			SteamDSConfig.isDedicatedServer = false;
			CoopPeerStarter.DedicatedHost = false;
			SteamClientDSConfig.isDedicatedClient = true;
			string[] array = server.GetGameTags().Split(new char[]
			{
				';'
			});
			string guid = (array == null || array.Length <= 0) ? string.Empty : array[0];
			SteamClientDSConfig.Guid = guid;
			SteamClientDSConfig.Server = server;
			SteamClientDSConfig.serverAddress = server.m_NetAdr.GetConnectionAddressString();
			if (!string.IsNullOrEmpty(this.invitedToPassword))
			{
				SteamClientDSConfig.password = SteamDSConfig.PasswordToHash(this.invitedToPassword);
			}
			else
			{
				SteamClientDSConfig.password = string.Empty;
			}
			SteamClientDSConfig.adminPassword = string.Empty;
			SteamClientDSConfig.EndPoint = UdpEndPoint.Parse(SteamClientDSConfig.serverAddress);
		}

		
		private void ServerFailedToRespond1()
		{
			Debug.Log("AutoJoin: Server ping failed " + this.invitedToIP + ", trying with port 27016");
			ISteamMatchmakingPingResponse pRequestServersResponse = new ISteamMatchmakingPingResponse(new ISteamMatchmakingPingResponse.ServerResponded(this.ServerResponded), new ISteamMatchmakingPingResponse.ServerFailedToRespond(this.ServerFailedToRespond2));
			HServerQuery hserverQuery = SteamMatchmakingServers.PingServer(this.QueryingIp, 27016, pRequestServersResponse);
		}

		
		private void ServerFailedToRespond2()
		{
			Debug.Log("AutoJoin: Server ping failed again on port 27016 " + this.invitedToIP + ", aborting");
			this.invitedToIP = null;
		}

		
		private IEnumerator DelayedServerInviteReceived(uint ip, ushort port)
		{
			this.QueryingIp = ip;
			ISteamMatchmakingPingResponse response = new ISteamMatchmakingPingResponse(new ISteamMatchmakingPingResponse.ServerResponded(this.ServerResponded), new ISteamMatchmakingPingResponse.ServerFailedToRespond(this.ServerFailedToRespond1));
			HServerQuery query = SteamMatchmakingServers.PingServer(ip, port, response);
			while (SteamClientDSConfig.Server == null)
			{
				if (string.IsNullOrEmpty(this.invitedToIP))
				{
					yield break;
				}
				yield return null;
			}
			yield return YieldPresets.WaitPointFiveSeconds;
			Debug.Log("Received DS invite for server name='" + SteamClientDSConfig.Server.GetServerName() + "'");
			this._inviteReceivedScreen._continueSaveButton.isEnabled = SaveSlotUtils.GetPreviouslyPlayedServers().Contains(SteamClientDSConfig.Guid);
			this._inviteReceivedScreen._gameName.text = SteamClientDSConfig.Server.GetServerName();
			this._inviteReceivedScreen._screen.SetActive(true);
			this._menu.SetActive(false);
			UICamera.hoveredObject = null;
			yield break;
		}

		
		public void OnClientCancel()
		{
			this._menu.SetActive(true);
			this._inviteReceivedScreen._screen.SetActive(false);
			this._inviteReceivedScreen._lobby = null;
			this.invitedToIP = null;
			this.invitedToPassword = null;
			this.QueryingIp = 0u;
			SteamClientDSConfig.Clear();
		}

		
		public void OnClientNewGame()
		{
			GameSetup.SetInitType(InitTypes.New);
			if (this._inviteReceivedScreen._lobby != null)
			{
				this.invitedTo = this._inviteReceivedScreen._lobby.LobbyId.m_SteamID;
				AutoJoinAfterMPInvite.LobbyID = this.invitedTo.ToString();
				this._titleScreen.OnCoOp();
				this._titleScreen.OnStartMpClient();
			}
			else if (SteamClientDSConfig.Server != null)
			{
				Application.LoadLevel("SteamStartSceneDedicatedServer_Client");
			}
		}

		
		public void OnClientContinueGame()
		{
			if (this._inviteReceivedScreen._lobby != null)
			{
				this.invitedTo = this._inviteReceivedScreen._lobby.LobbyId.m_SteamID;
				AutoJoinAfterMPInvite.LobbyID = this.invitedTo.ToString();
				this._titleScreen.OnCoOp();
				this._titleScreen.OnLoad();
				this._titleScreen.OnStartMpClient();
			}
			else if (SteamClientDSConfig.Server != null)
			{
				GameSetup.SetInitType(InitTypes.Continue);
				Application.LoadLevel("SteamStartSceneDedicatedServer_Client");
			}
		}

		
		
		
		public static string LobbyID { get; set; }

		
		
		
		public ulong invitedTo { get; private set; }

		
		
		
		public uint QueryingIp { get; private set; }

		
		
		
		public string invitedToIP { get; private set; }

		
		
		
		public string invitedToPassword { get; set; }

		
		
		
		private static bool Done { get; set; }

		
		public AutoJoinAfterMPInvite.InviteReceivedScreen _inviteReceivedScreen;

		
		public TitleScreen _titleScreen;

		
		public GameObject _menu;

		
		private Texture2D _textureOverlay;

		
		[Serializable]
		public class InviteReceivedScreen
		{
			
			public CoopLobbyInfo _lobby;

			
			public GameObject _screen;

			
			public UILabel _gameName;

			
			public UIButton _continueSaveButton;
		}
	}
}
