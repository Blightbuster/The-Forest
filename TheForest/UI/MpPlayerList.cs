using System;
using System.Collections.Generic;
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
			int playerCount = 0;
			int num = 0;
			if (SteamClientDSConfig.isDedicatedClient)
			{
				if (SteamClientDSConfig.Server != null)
				{
					for (int i = this._grid.transform.childCount - 1; i >= 0; i--)
					{
						UnityEngine.Object.Destroy(this._grid.GetChild(i).gameObject);
					}
					SteamClientDSConfig.playerCount = 0;
					num = SteamClientDSConfig.Server.m_nMaxPlayers;
					IPlayerState state = LocalPlayer.Entity.GetState<IPlayerState>();
					this.AddDSPlayerToList(state.name);
					GameObject[] array = GameObject.FindGameObjectsWithTag("PlayerNet");
					if (array.Length > 0)
					{
						foreach (GameObject gameObject in array)
						{
							BoltEntity component = gameObject.GetComponent<BoltEntity>();
							if (component != null)
							{
								IPlayerState state2 = component.GetState<IPlayerState>();
								if (state2 != null)
								{
									SteamClientDSConfig.playerCount++;
									this.AddPlayerRow(state2.name, null, CoopSteamClientStarter.IsAdmin && state2.name != LocalPlayer.State.name);
								}
							}
						}
					}
				}
				else
				{
					Debug.LogError("There is no server information stored on this client.");
				}
			}
			else
			{
				playerCount = 0;
				for (int k = this._grid.transform.childCount - 1; k >= 0; k--)
				{
					UnityEngine.Object.Destroy(this._grid.GetChild(k).gameObject);
				}
				IEnumerable<ulong> source = (from c in BoltNetwork.clients
				select c.RemoteEndPoint.SteamId.Id).Union(from m in CoopLobby.Instance.AllMembers
				select m.m_SteamID);
				source.ForEach(delegate(ulong x)
				{
					CSteamID steamIDFriend = new CSteamID(x);
					string friendPersonaName = SteamFriends.GetFriendPersonaName(steamIDFriend);
					BoltEntity entityFromSteamID = MpPlayerList.GetEntityFromSteamID(x);
					this.AddPlayerRow(friendPersonaName, x, entityFromSteamID, CoopLobby.Instance.Info.IsOwner && CoopLobby.Instance.Info.OwnerSteamId.m_SteamID != x);
					playerCount++;
				});
				num = CoopLobby.Instance.Info.MemberLimit;
			}
			this._grid.repositionNow = true;
			if (SteamClientDSConfig.isDedicatedClient)
			{
				this._playerCount.text = SteamClientDSConfig.playerCount + "/" + SteamClientDSConfig.Server.m_nMaxPlayers;
			}
			else
			{
				this._playerCount.text = playerCount + "/" + num;
			}
		}

		
		public void Kick(ulong steamid)
		{
			if (CoopPeerStarter.Dedicated && CoopSteamClientStarter.IsAdmin)
			{
				CoopAdminCommand.Send("kick", steamid.ToString());
			}
			else
			{
				BoltEntity entityFromSteamID = MpPlayerList.GetEntityFromSteamID(steamid);
				if (entityFromSteamID)
				{
					CoopKick.KickPlayer(entityFromSteamID, -1, "HOST_KICKED_YOU");
					base.Invoke("Refresh", 0.1f);
				}
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

		
		public void Ban(ulong steamid)
		{
			if (CoopPeerStarter.Dedicated && CoopSteamClientStarter.IsAdmin)
			{
				CoopAdminCommand.Send("ban", steamid.ToString());
			}
			else
			{
				BoltEntity entityFromSteamID = MpPlayerList.GetEntityFromSteamID(steamid);
				if (entityFromSteamID)
				{
					CoopKick.BanPlayer(entityFromSteamID);
					base.Invoke("Refresh", 0.1f);
				}
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

		
		public static BoltEntity GetEntityFromSteamID(ulong steamid)
		{
			return Scene.SceneTracker.allPlayerEntities.FirstOrDefault((BoltEntity e) => e.source.RemoteEndPoint.SteamId.Id == steamid);
		}

		
		public static BoltEntity GetEntityFromName(string name, BoltConnection except)
		{
			return Scene.SceneTracker.allPlayerEntities.FirstOrDefault((BoltEntity e) => e.source != except && e.GetState<IPlayerState>().name == name);
		}

		
		public static BoltEntity GetEntityFromBoltConnexion(BoltConnection connexion)
		{
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

		
		private PlayerInventory.PlayerViews previousView = PlayerInventory.PlayerViews.World;

		
		private ISteamMatchmakingPlayersResponse _DsPlayersResponse;
	}
}
