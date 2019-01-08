using System;
using System.Collections.Generic;
using Bolt;
using Steamworks;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UniLinq;
using UnityEngine;

namespace TheForest.UI
{
	public class MpPlayerList : MonoBehaviour
	{
		public void EnableList()
		{
			this.previousView = LocalPlayer.Inventory.CurrentView;
			LocalPlayer.Inventory.CurrentView = PlayerInventory.PlayerViews.PlayerList;
			LocalPlayer.FpCharacter.LockView(true);
			if (BoltNetwork.isClient)
			{
				this._banListButton.SetActive(false);
			}
			this.Refresh();
		}

		public void DisableList()
		{
			LocalPlayer.Inventory.CurrentView = this.previousView;
			LocalPlayer.FpCharacter.UnLockView();
		}

		public void Refresh()
		{
			if (SteamClientDSConfig.isDedicatedClient)
			{
				this.RefreshFromDedicatedServer();
			}
			else
			{
				this.RefreshFromP2PServer();
			}
		}

		private void UpdatePlayerCountText(string message)
		{
			this._grid.repositionNow = true;
			this._playerCount.text = message;
		}

		private void RefreshFromP2PServer()
		{
			int playerCount = 0;
			playerCount = 0;
			for (int i = this._grid.transform.childCount - 1; i >= 0; i--)
			{
				UnityEngine.Object.Destroy(this._grid.GetChild(i).gameObject);
			}
			IEnumerable<BoltConnection> clients = BoltNetwork.clients;
			IEnumerable<CSteamID> allMembers = CoopLobby.Instance.AllMembers;
			ulong[] first = (from c in clients
			select c.RemoteEndPoint.SteamId.Id).ToArray<ulong>();
			ulong[] second = (from m in allMembers
			select m.m_SteamID).ToArray<ulong>();
			IEnumerable<ulong> source = first.Union(second);
			source.ForEach(delegate(ulong x)
			{
				CSteamID steamIDFriend = new CSteamID(x);
				string friendPersonaName = SteamFriends.GetFriendPersonaName(steamIDFriend);
				BoltEntity entityFromSteamID = MpPlayerList.GetEntityFromSteamID(x);
				this.AddPlayerRow(friendPersonaName, x, entityFromSteamID, CoopLobby.Instance.Info.IsOwner && CoopLobby.Instance.Info.OwnerSteamId.m_SteamID != x);
				playerCount++;
			});
			int memberLimit = CoopLobby.Instance.Info.MemberLimit;
			this.UpdatePlayerCountText(playerCount + "/" + memberLimit);
		}

		private void RefreshFromDedicatedServer()
		{
			if (SteamClientDSConfig.Server == null)
			{
				Debug.LogError("There is no server information stored on this client.");
				return;
			}
			for (int i = this._grid.transform.childCount - 1; i >= 0; i--)
			{
				UnityEngine.Object.Destroy(this._grid.GetChild(i).gameObject);
			}
			SteamClientDSConfig.playerCount = 0;
			int nMaxPlayers = SteamClientDSConfig.Server.m_nMaxPlayers;
			IPlayerState state = LocalPlayer.Entity.GetState<IPlayerState>();
			CoopServerInfo instance = CoopServerInfo.Instance;
			NetworkArray_Integer clients = instance.state.Clients;
			NetworkArray_String playerNames = instance.state.PlayerNames;
			for (int j = 0; j < playerNames.Length; j++)
			{
				if (!playerNames[j].NullOrEmpty())
				{
					SteamClientDSConfig.playerCount++;
					int connectionId = clients[j - 1];
					this.AddPlayerRow(playerNames[j], connectionId, CoopSteamClientStarter.IsAdmin && playerNames[j] != LocalPlayer.State.name);
				}
			}
			this.UpdatePlayerCountText(SteamClientDSConfig.playerCount + "/" + SteamClientDSConfig.Server.m_nMaxPlayers);
		}

		public void Kick(ulong steamid)
		{
			if (CoopPeerStarter.Dedicated && CoopSteamClientStarter.IsAdmin)
			{
				CoopAdminCommand.Send("kick", steamid.ToString());
			}
			else
			{
				CoopKick.KickPlayer(steamid, 1, "HOST_KICKED_YOU");
				base.Invoke("Refresh", 0.1f);
			}
		}

		public void KickByConnectionId(int connectionId)
		{
			if (CoopPeerStarter.Dedicated && CoopSteamClientStarter.IsAdmin)
			{
				CoopAdminCommand.Send("kickbycid", connectionId.ToString());
			}
		}

		public void KickByName(string name)
		{
			if (CoopPeerStarter.Dedicated && CoopSteamClientStarter.IsAdmin)
			{
				CoopAdminCommand.Send("kick", name);
			}
			else
			{
				BoltEntity entityFromName = MpPlayerList.GetEntityFromName(name, null);
				if (entityFromName)
				{
					CoopKick.KickPlayer(entityFromName, -1, "HOST_KICKED_YOU");
					base.Invoke("Refresh", 0.1f);
				}
			}
		}

		public void BanByConnectionId(int connectionId)
		{
			if (CoopPeerStarter.Dedicated && CoopSteamClientStarter.IsAdmin)
			{
				CoopAdminCommand.Send("banbycid", connectionId.ToString());
			}
		}

		public void Ban(ulong steamid)
		{
			if (CoopPeerStarter.Dedicated && CoopSteamClientStarter.IsAdmin)
			{
				CoopAdminCommand.Send("ban", steamid.ToString());
			}
			else
			{
				BoltConnection connection;
				BoltEntity playerEntity;
				MpPlayerList.GetConnectionAndEntity(steamid, out connection, out playerEntity);
				CoopKick.BanPlayer(connection, playerEntity);
				base.Invoke("Refresh", 0.1f);
			}
		}

		public void BanByName(string name)
		{
			if (CoopPeerStarter.Dedicated && CoopSteamClientStarter.IsAdmin)
			{
				CoopAdminCommand.Send("ban", name);
			}
			else
			{
				BoltEntity entityFromName = MpPlayerList.GetEntityFromName(name, null);
				if (entityFromName)
				{
					CoopKick.BanPlayer(entityFromName);
					base.Invoke("Refresh", 0.1f);
				}
			}
		}

		public void Profile(ulong steamid)
		{
			SteamFriends.ActivateGameOverlayToUser("steamid", new CSteamID(steamid));
		}

		public void Close()
		{
			Scene.HudGui.HideMpPlayerList();
		}

		public void ToBannedPlayerList()
		{
			Scene.HudGui.MpBannedPlayerList.gameObject.SetActive(true);
			base.gameObject.SetActive(false);
		}

		private void AddPlayerRow(string name, int connectionId, bool showButtons)
		{
			PlayerListRow playerListRow = UnityEngine.Object.Instantiate<PlayerListRow>(this._rowPrefab);
			playerListRow.transform.parent = this._grid.transform;
			playerListRow.transform.localPosition = Vector3.zero;
			playerListRow.transform.localScale = Vector3.one;
			playerListRow._entity = null;
			playerListRow._overlay._name.text = name;
			if (showButtons)
			{
				EventDelegate eventDelegate = new EventDelegate(this, "KickByConnectionId");
				eventDelegate.parameters[0] = new EventDelegate.Parameter(connectionId);
				playerListRow._kickButton.onClick.Add(eventDelegate);
				playerListRow._kickButton.gameObject.SetActive(true);
				EventDelegate eventDelegate2 = new EventDelegate(this, "BanByConnectionId");
				eventDelegate2.parameters[0] = new EventDelegate.Parameter(connectionId);
				playerListRow._banButton.onClick.Add(eventDelegate2);
				playerListRow._banButton.gameObject.SetActive(true);
			}
			else
			{
				playerListRow._kickButton.gameObject.SetActive(false);
				playerListRow._banButton.gameObject.SetActive(false);
			}
			playerListRow._profileButton.gameObject.SetActive(false);
		}

		private void AddPlayerRow(string name, ulong id, BoltEntity entity, bool showButtons)
		{
			PlayerListRow playerListRow = UnityEngine.Object.Instantiate<PlayerListRow>(this._rowPrefab);
			playerListRow.transform.parent = this._grid.transform;
			playerListRow.transform.localPosition = Vector3.zero;
			playerListRow.transform.localScale = Vector3.one;
			playerListRow._entity = entity;
			playerListRow._overlay._name.text = name;
			if (showButtons)
			{
				EventDelegate eventDelegate = new EventDelegate(this, "Kick");
				eventDelegate.parameters[0] = new EventDelegate.Parameter(id);
				playerListRow._kickButton.onClick.Add(eventDelegate);
				playerListRow._kickButton.gameObject.SetActive(true);
				EventDelegate eventDelegate2 = new EventDelegate(this, "Ban");
				eventDelegate2.parameters[0] = new EventDelegate.Parameter(id);
				playerListRow._banButton.onClick.Add(eventDelegate2);
				playerListRow._banButton.gameObject.SetActive(true);
			}
			else
			{
				playerListRow._kickButton.gameObject.SetActive(false);
				playerListRow._banButton.gameObject.SetActive(false);
			}
			if (entity && entity != LocalPlayer.Entity)
			{
				EventDelegate eventDelegate3 = new EventDelegate(this, "Profile");
				eventDelegate3.parameters[0] = new EventDelegate.Parameter(id);
				playerListRow._profileButton.onClick.Add(eventDelegate3);
				playerListRow._profileButton.gameObject.SetActive(true);
			}
			else
			{
				playerListRow._profileButton.gameObject.SetActive(false);
			}
		}

		private void AddPlayerRow(string name, BoltEntity entity, bool showButtons)
		{
			PlayerListRow playerListRow = UnityEngine.Object.Instantiate<PlayerListRow>(this._rowPrefab);
			playerListRow.transform.parent = this._grid.transform;
			playerListRow.transform.localPosition = Vector3.zero;
			playerListRow.transform.localScale = Vector3.one;
			playerListRow._entity = entity;
			playerListRow._overlay._name.text = name;
			if (showButtons)
			{
				EventDelegate eventDelegate = new EventDelegate(this, "KickByName");
				eventDelegate.parameters[0] = new EventDelegate.Parameter(name);
				playerListRow._kickButton.onClick.Add(eventDelegate);
				playerListRow._kickButton.gameObject.SetActive(true);
				EventDelegate eventDelegate2 = new EventDelegate(this, "BanByName");
				eventDelegate2.parameters[0] = new EventDelegate.Parameter(name);
				playerListRow._banButton.onClick.Add(eventDelegate2);
				playerListRow._banButton.gameObject.SetActive(true);
			}
			else
			{
				playerListRow._kickButton.gameObject.SetActive(false);
				playerListRow._banButton.gameObject.SetActive(false);
			}
			playerListRow._profileButton.gameObject.SetActive(false);
		}

		public static void UpdateKnownPlayers()
		{
			if (MpPlayerList._knownPlayers == null)
			{
				MpPlayerList._knownPlayers = new Dictionary<BoltConnection, MpPlayerList.KnownPlayerData>();
			}
			else
			{
				MpPlayerList._knownPlayers.Clear();
			}
			foreach (BoltConnection boltConnection in BoltNetwork.clients)
			{
				ulong id = boltConnection.RemoteEndPoint.SteamId.Id;
				CSteamID csteamID = new CSteamID(id);
				string friendPersonaName = SteamFriends.GetFriendPersonaName(csteamID);
				BoltEntity entityFromSteamID = MpPlayerList.GetEntityFromSteamID(id);
				MpPlayerList._knownPlayers.Add(boltConnection, new MpPlayerList.KnownPlayerData
				{
					Connection = boltConnection,
					SteamId = id,
					CSteamId = csteamID,
					Name = friendPersonaName,
					BoltEntity = entityFromSteamID
				});
			}
		}

		public static void GetConnectionAndEntity(ulong steamid, out BoltConnection foundConnection, out BoltEntity foundEntity)
		{
			foundConnection = MpPlayerList.GetConnectionFromSteamID(steamid);
			foundEntity = MpPlayerList.GetEntityFromSteamID(steamid);
		}

		public static BoltConnection GetConnectionFromConnectionId(int connectionId)
		{
			return BoltNetwork.clients.FirstOrDefault((BoltConnection eachClient) => (ulong)eachClient.ConnectionId == (ulong)((long)connectionId));
		}

		public static BoltEntity GetEntityFromSteamID(ulong steamid)
		{
			return Scene.SceneTracker.allPlayerEntities.FirstOrDefault((BoltEntity e) => e.source.RemoteEndPoint.SteamId.Id == steamid);
		}

		public static BoltConnection GetConnectionFromSteamID(ulong steamid)
		{
			return BoltNetwork.clients.FirstOrDefault((BoltConnection eachClient) => eachClient.RemoteEndPoint.SteamId.Id == steamid);
		}

		public static BoltEntity GetEntityFromName(string name, BoltConnection except)
		{
			return Scene.SceneTracker.allPlayerEntities.FirstOrDefault((BoltEntity e) => e.source != except && e.GetState<IPlayerState>().name == name);
		}

		public static BoltEntity GetEntityFromBoltConnexion(BoltConnection connexion)
		{
			if (Scene.SceneTracker == null)
			{
				return null;
			}
			return Scene.SceneTracker.allPlayerEntities.FirstOrDefault((BoltEntity e) => e.source == connexion);
		}

		private void AddDSPlayerToList(string pchName)
		{
			this.DsAddPlayerToList(pchName, 0, 0f);
		}

		private void DsAddPlayerToList(string pchName, int nScore, float flTimePlayed)
		{
			SteamClientDSConfig.playerCount++;
			this.AddPlayerRow(pchName, 0UL, null, CoopSteamClientStarter.IsAdmin && pchName != LocalPlayer.State.name);
		}

		private void DsPlayerListRequestFailed()
		{
			this._DsPlayersResponse = null;
		}

		private void DsPlayerListRequestFinished()
		{
			this._playerCount.text = SteamClientDSConfig.playerCount + "/" + SteamClientDSConfig.Server.m_nMaxPlayers;
			this._DsPlayersResponse = null;
		}

		public PlayerListRow _rowPrefab;

		public UIGrid _grid;

		public UILabel _playerCount;

		public GameObject _banListButton;

		private static Dictionary<BoltConnection, MpPlayerList.KnownPlayerData> _knownPlayers = new Dictionary<BoltConnection, MpPlayerList.KnownPlayerData>();

		private PlayerInventory.PlayerViews previousView = PlayerInventory.PlayerViews.World;

		private ISteamMatchmakingPlayersResponse _DsPlayersResponse;

		private class KnownPlayerData
		{
			internal ulong SteamId { get; set; }

			internal BoltConnection Connection { get; set; }

			internal string Name { get; set; }

			internal BoltEntity BoltEntity { get; set; }

			internal CSteamID CSteamId { get; set; }
		}
	}
}
