using System;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using Steamworks;
using TheForest.Commons.Enums;
using TheForest.Networking;
using TheForest.UI;
using TheForest.Utils;
using UdpKit;
using UniLinq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.ImageEffects;


public class CoopSteamNGUI : MonoBehaviour
{
	
	
	public string BrowserFilter
	{
		get
		{
			return this._browserFilter;
		}
	}

	
	
	public HashSet<string> PreviouslyPlayedServers
	{
		get
		{
			return this._previouslyPlayedServers;
		}
	}

	
	
	
	public Action RefreshBrowserOverride { get; set; }

	
	private void Awake()
	{
		this.ResetDSFlags();
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		if (LoadAsync.Scenery)
		{
			Blur blur = (!LoadAsync.Scenery) ? null : LoadAsync.Scenery.GetComponentInChildren<Blur>();
			if (blur)
			{
				blur.enabled = true;
			}
		}
		BoltLauncher.SetUdpPlatform(new SteamPlatform());
		TheForest.Utils.Input.player.controllers.maps.SetMapsEnabled(false, ControllerType.Keyboard, "Default");
		TheForest.Utils.Input.player.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "Menu");
		TheForest.Utils.Input.player.controllers.maps.SetMapsEnabled(true, ControllerType.Joystick, "Menu");
		if (!SteamManager.Initialized)
		{
			if (GameSetup.IsMpServer)
			{
				this.OpenScreen(CoopSteamNGUI.Screens.LobbySetup);
			}
			else
			{
				this.OpenScreen(CoopSteamNGUI.Screens.GameBrowser);
			}
			this.SetLoadingText(UiTranslationDatabase.TranslateKey("STEAM_NOT_INITIALIZED", "Steam not initialized", this._allCapsTexts));
			return;
		}
		this.RefreshUI();
		if (GameSetup.IsMpServer)
		{
			this._hostGameName = PlayerPrefs.GetString("MpGameName", this._hostGameName);
			this._hostMaxPlayers = PlayerPrefs.GetInt("MpGamePlayerCount", this.GetHostPlayersMax()).ToString();
			this._hostFriendsOnly = (PlayerPrefs.GetInt("MpGameFriendsOnly", (!this._hostFriendsOnly) ? 0 : 1) == 1);
			this._lobbySetupScreen._gameNameInput.value = this._hostGameName;
			this._lobbySetupScreen._playerCountInput.value = this._hostMaxPlayers;
			this._lobbySetupScreen._privateOnlyToggle.value = this._hostFriendsOnly;
			this.OpenScreen(CoopSteamNGUI.Screens.LobbySetup);
		}
		else
		{
			CoopLobbyManager.QueryList(this._showFriendGames);
			this.OpenScreen(CoopSteamNGUI.Screens.GameBrowser);
		}
		if (AutoJoinAfterMPInvite.LobbyID != null && (CoopLobby.Instance == null || CoopLobby.Instance.Info.LobbyId.ToString() != AutoJoinAfterMPInvite.LobbyID))
		{
			CoopLobbyInfo lobby = new CoopLobbyInfo(ulong.Parse(AutoJoinAfterMPInvite.LobbyID));
			AutoJoinAfterMPInvite.LobbyID = null;
			if (GameSetup.IsSavedGame)
			{
				this.OnClientContinueGame(lobby);
			}
			else
			{
				this.OnClientNewGame(lobby);
			}
		}
	}

	
	private void LateUpdate()
	{
		CoopSteamNGUI.Screens screens = (this._currentScreen != CoopSteamNGUI.Screens.ModalScreen) ? this._currentScreen : this._prevScreen;
		if (screens != CoopSteamNGUI.Screens.LobbySetup)
		{
			if (screens != CoopSteamNGUI.Screens.Lobby)
			{
				if (screens == CoopSteamNGUI.Screens.GameBrowser)
				{
					this.UpdateGameBrowser();
				}
			}
			else
			{
				this.UpdateLobby();
			}
		}
	}

	
	private void OnDestroy()
	{
		TheForest.Utils.Input.player.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "Default");
	}

	
	public void OnHostChangeGameName()
	{
	}

	
	public void OnHostLobbySetup()
	{
		this._hostGameName = this._lobbySetupScreen._gameNameInput.value;
		this._hostMaxPlayers = this._lobbySetupScreen._playerCountInput.value;
		this._hostFriendsOnly = this._lobbySetupScreen._privateOnlyToggle.value;
		PlayerPrefs.SetString("MpGameName", this._hostGameName);
		PlayerPrefs.SetInt("MpGamePlayerCount", this.GetHostPlayersMax());
		PlayerPrefs.SetInt("MpGameFriendsOnly", (!this._hostFriendsOnly) ? 0 : 1);
		PlayerPrefs.Save();
		bool flag = CoopSteamServer.Start(delegate
		{
			this.SetLoadingText(UiTranslationDatabase.TranslateKey("CREATING_LOBBY___", "Creating Lobby...", this._allCapsTexts));
			CoopLobbyManager.Create(this.GetHostGameName(), this.GetHostPlayersMax(), this._hostFriendsOnly, delegate
			{
				if (string.IsNullOrEmpty(CoopLobby.Instance.Info.Guid) && GameSetup.IsNewGame)
				{
					CoopLobby.Instance.SetGuid(Guid.NewGuid().ToString());
				}
				this._lobbyScreen._gameNameLabel.text = StringEx.TryFormat(UiTranslationDatabase.TranslateKey("LOBBY_GAME_NAME", "LOBBY: {0}", this._allCapsTexts), new object[]
				{
					CoopLobby.Instance.Info.Name.ToUpperInvariant()
				});
				this.OpenScreen(CoopSteamNGUI.Screens.Lobby);
			}, delegate
			{
				this.ClearLoadingAndError();
				this.SetErrorText(UiTranslationDatabase.TranslateKey("COULD_NOT_CREATE_STEAM_LOBBY", "Could not create Steam lobby.", this._allCapsTexts));
			});
		}, delegate
		{
			this.SetErrorText(UiTranslationDatabase.TranslateKey("COULD_NOT_CONNECT_TO_STEAM_MASTER_SERVER", "Could not connect to Steam master server.", this._allCapsTexts));
		});
		if (flag)
		{
			this.SetLoadingText(UiTranslationDatabase.TranslateKey("TALKING_TO_STEAM___", "Talking To Steam...", this._allCapsTexts));
		}
		else
		{
			this.SetErrorText(UiTranslationDatabase.TranslateKey("COULD_NOT_START_STEAM_GAME_SERVER", "Could not start Steam game server.", this._allCapsTexts));
		}
	}

	
	public void OnHostInviteFriends()
	{
		SteamFriends.ActivateGameOverlayInviteDialog(CoopLobby.Instance.Info.LobbyId);
	}

	
	public void OnHostStartGame()
	{
		this.SetLoadingText(UiTranslationDatabase.TranslateKey("STARTING_SERVER___", "Starting Server...", this._allCapsTexts));
		if (GameSetup.IsNewGame)
		{
			PlaneCrashAudioState.Spawn();
		}
		base.gameObject.AddComponent<CoopSteamServerStarter>().gui = this;
	}

	
	public void OnGamepadSearch()
	{
	}

	
	public void OnShowFriendGames(bool onOff)
	{
		this._showFriendGames = onOff;
		this.OnClientRefreshGameList();
	}

	
	public void OnClientUpdateFilter()
	{
		this._browserFilter = this._gameBrowserScreen._filter.value.ToLowerInvariant();
	}

	
	public void OnClientRefreshGameList()
	{
		if (this.RefreshBrowserOverride != null)
		{
			this.RefreshBrowserOverride();
		}
		else
		{
			this._lobbies = new List<CoopLobbyInfo>(0);
			foreach (MpGameRow mpGameRow in this._gameRows.Values)
			{
				UnityEngine.Object.Destroy(mpGameRow.gameObject);
			}
			this._gameRows.Clear();
			CoopLobbyManager.QueryList(this._showFriendGames);
		}
	}

	
	public void OnClientContinueGame(CoopLobbyInfo lobby)
	{
		if (CoopLobby.IsInLobby)
		{
			CoopLobby.LeaveActive();
		}
		GameSetup.SetInitType(InitTypes.Continue);
		GameSetup.SetMpType(MpTypes.Client);
		this.RefreshUI();
		if (this._currentScreen == CoopSteamNGUI.Screens.InviteReceivedScreen)
		{
			this._currentScreen = this._prevScreen;
			this.OpenScreen(CoopSteamNGUI.Screens.GameBrowser);
		}
		else if (this._currentScreen == CoopSteamNGUI.Screens.JoinP2P)
		{
			this._currentScreen = this._prevScreen;
			this.OpenScreen(CoopSteamNGUI.Screens.GameBrowser);
		}
		try
		{
			this.SetLoadingText(StringEx.TryFormat(UiTranslationDatabase.TranslateKey("JOINING_LOBBY_0____", "Joining Lobby {0}...", this._allCapsTexts), new object[]
			{
				lobby.Name
			}));
		}
		catch
		{
		}
		lobby.UpdateData();
		CoopLobbyManager.Join(lobby, delegate
		{
			lobby.UpdateData();
			this.ClearLoadingAndError();
		}, new Action<string>(this.OnFailedEnterLobby));
	}

	
	public void OnClientNewGame(CoopLobbyInfo lobby)
	{
		if (CoopLobby.IsInLobby)
		{
			CoopLobby.LeaveActive();
		}
		GameSetup.SetInitType(InitTypes.New);
		GameSetup.SetMpType(MpTypes.Client);
		this.RefreshUI();
		if (this._currentScreen == CoopSteamNGUI.Screens.InviteReceivedScreen)
		{
			this._currentScreen = this._prevScreen;
			this.OpenScreen(CoopSteamNGUI.Screens.GameBrowser);
		}
		else if (this._currentScreen == CoopSteamNGUI.Screens.JoinP2P)
		{
			this._currentScreen = this._prevScreen;
			this.OpenScreen(CoopSteamNGUI.Screens.GameBrowser);
		}
		try
		{
			this.SetLoadingText(StringEx.TryFormat(UiTranslationDatabase.TranslateKey("JOINING_LOBBY_0____", "Joining Lobby {0}...", this._allCapsTexts), new object[]
			{
				lobby.Name
			}));
		}
		catch
		{
		}
		lobby.UpdateData();
		CoopLobbyManager.Join(lobby, delegate
		{
			lobby.UpdateData();
			this.ClearLoadingAndError();
		}, new Action<string>(this.OnFailedEnterLobby));
	}

	
	private void OnFailedEnterLobby(string reason)
	{
		this.ClearLoadingAndError();
		this.OnBack();
		if (reason == "FULL")
		{
			this._lobbyFullMessage.SetActive(true);
		}
		else
		{
			this.SetErrorText(UiTranslationDatabase.TranslateKey("COULD_NOT_JOIN_STEAM_LOBBY", "Could not join Steam lobby.", this._allCapsTexts));
		}
	}

	
	public void OnClientConnectDs(gameserveritem_t server)
	{
		bool flag = server.m_nPlayers >= server.m_nMaxPlayers;
		string[] array = server.GetGameTags().Split(new char[]
		{
			';'
		});
		string item = (array == null || array.Length <= 0) ? string.Empty : array[0];
		string a = (array == null || array.Length <= 1) ? "-1" : array[1];
		bool flag2 = a != "__F486E3E06B8E13E0388571BE0FDC8A35182D8BE83E9256BA53BC5FBBDBCF23BC";
		if (flag2 || flag)
		{
			this._joinDsScreen._adminPassword.gameObject.SetActive(false);
			this._joinDsScreen._password.gameObject.SetActive(false);
			this._joinDsScreen._newButtonLabel.transform.parent.gameObject.SetActive(false);
			this._joinDsScreen._continueButtonLabel.transform.parent.gameObject.SetActive(false);
		}
		else
		{
			this._joinDsScreen._newButtonLabel.transform.parent.gameObject.SetActive(true);
			if (this.PreviouslyPlayedServers.Contains(item))
			{
				this._joinDsScreen._continueButtonLabel.transform.parent.gameObject.SetActive(true);
				this._joinDsScreen._continueButtonLabel.text = UiTranslationDatabase.TranslateKey("CONTINUE", "Continue", this._allCapsTexts);
			}
			else
			{
				this._joinDsScreen._continueButtonLabel.transform.parent.gameObject.SetActive(false);
			}
			this._joinDsScreen._adminPassword.gameObject.SetActive(true);
			this._joinDsScreen._password.gameObject.SetActive(server.m_bPassword);
		}
		this._joinDsScreen._prefabDbVersionMissmatch.SetActive(flag2);
		this._joinDsScreen._prefabServerIsFull.SetActive(flag && !flag2);
		this._joinDsScreen._serverName.text = server.GetServerName();
		this.JoiningServer = server;
		this._joinDsScreen._ip.text = server.m_NetAdr.GetConnectionAddressString();
		if (this._joinDsScreen._ping)
		{
			this._joinDsScreen._ping.text = server.m_nPing + "ms";
		}
		this._joinDsScreen._playerLimit.text = StringEx.TryFormat("{0} / {1}", new object[]
		{
			server.m_nPlayers,
			server.m_nMaxPlayers
		});
		this._joinDsScreen._password.value = string.Empty;
		this._joinDsScreen._adminPassword.value = string.Empty;
		this.OpenScreen(CoopSteamNGUI.Screens.JoinDS);
	}

	
	public void OnClientNewGameDS(gameserveritem_t server)
	{
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
		if (!string.IsNullOrEmpty(this._joinDsScreen._password.value))
		{
			SteamClientDSConfig.password = SteamDSConfig.PasswordToHash(this._joinDsScreen._password.value);
			this._joinDsScreen._password.value = string.Empty;
		}
		else
		{
			SteamClientDSConfig.password = string.Empty;
		}
		if (!string.IsNullOrEmpty(this._joinDsScreen._adminPassword.value))
		{
			SteamClientDSConfig.adminPassword = SteamDSConfig.PasswordToHash(this._joinDsScreen._adminPassword.value);
			this._joinDsScreen._adminPassword.value = string.Empty;
		}
		else
		{
			SteamClientDSConfig.adminPassword = string.Empty;
		}
		SteamClientDSConfig.EndPoint = UdpEndPoint.Parse(SteamClientDSConfig.serverAddress);
		GameSetup.SetInitType(InitTypes.New);
		UnityEngine.Object.Destroy(base.gameObject);
		SceneManager.LoadScene("SteamStartSceneDedicatedServer_Client");
	}

	
	public void OnClientContinueGameDS(gameserveritem_t server)
	{
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
		if (!string.IsNullOrEmpty(this._joinDsScreen._password.value))
		{
			SteamClientDSConfig.password = SteamDSConfig.PasswordToHash(this._joinDsScreen._password.value);
			this._joinDsScreen._password.value = string.Empty;
		}
		else
		{
			SteamClientDSConfig.password = string.Empty;
		}
		if (!string.IsNullOrEmpty(this._joinDsScreen._adminPassword.value))
		{
			SteamClientDSConfig.adminPassword = SteamDSConfig.PasswordToHash(this._joinDsScreen._adminPassword.value);
			this._joinDsScreen._adminPassword.value = string.Empty;
		}
		else
		{
			SteamClientDSConfig.adminPassword = string.Empty;
		}
		SteamClientDSConfig.EndPoint = UdpEndPoint.Parse(SteamClientDSConfig.serverAddress);
		GameSetup.SetInitType(InitTypes.Continue);
		UnityEngine.Object.Destroy(base.gameObject);
		SceneManager.LoadScene("SteamStartSceneDedicatedServer_Client");
	}

	
	public void OnBack()
	{
		switch (this._currentScreen)
		{
		case CoopSteamNGUI.Screens.ModalScreen:
			if (GameSetup.IsMpClient)
			{
				CoopLobby.LeaveActive();
				if (this._prevScreen == CoopSteamNGUI.Screens.Lobby || this._prevScreen == CoopSteamNGUI.Screens.ModalScreen)
				{
					this._prevScreen = CoopSteamNGUI.Screens.GameBrowser;
					this.OnClientRefreshGameList();
				}
			}
			else
			{
				if (CoopLobby.Instance != null)
				{
					CoopLobby.Instance.Destroy();
				}
				CoopLobby.LeaveActive();
				CoopSteamServer.Shutdown();
				if (this._prevScreen == CoopSteamNGUI.Screens.Lobby || this._prevScreen == CoopSteamNGUI.Screens.ModalScreen)
				{
					this._prevScreen = CoopSteamNGUI.Screens.LobbySetup;
				}
			}
			this.OpenScreen(this._prevScreen);
			return;
		case CoopSteamNGUI.Screens.LobbySetup:
			this.ClearScenery();
			CoopSteamClient.Shutdown();
			UnityEngine.Object.Destroy(base.gameObject);
			SceneManager.LoadScene("TitleScene", LoadSceneMode.Single);
			return;
		case CoopSteamNGUI.Screens.Lobby:
			if (GameSetup.IsMpClient)
			{
				CoopLobby.LeaveActive();
				if (this._prevScreen == CoopSteamNGUI.Screens.GameBrowser)
				{
					this.OpenScreen(CoopSteamNGUI.Screens.GameBrowser);
					this.OnClientRefreshGameList();
				}
				else if (this._prevScreen == CoopSteamNGUI.Screens.LobbySetup)
				{
					this.OpenScreen(CoopSteamNGUI.Screens.LobbySetup);
				}
				else
				{
					this.ClearScenery();
					UnityEngine.Object.Destroy(base.gameObject);
					SceneManager.LoadScene("TitleScene", LoadSceneMode.Single);
				}
			}
			else
			{
				CoopLobby.Instance.Destroy();
				CoopLobby.LeaveActive();
				CoopSteamServer.Shutdown();
				this.OpenScreen(CoopSteamNGUI.Screens.LobbySetup);
			}
			return;
		case CoopSteamNGUI.Screens.JoinDS:
		case CoopSteamNGUI.Screens.JoinP2P:
			this.JoiningServer = null;
			this.OpenScreen(this._prevScreen);
			return;
		}
		this.ClearScenery();
		UnityEngine.Object.Destroy(base.gameObject);
		SceneManager.LoadScene("TitleScene", LoadSceneMode.Single);
	}

	
	private void ClearScenery()
	{
		if (LoadAsync.Scenery)
		{
			UnityEngine.Object.Destroy(LoadAsync.Scenery);
			LoadAsync.Scenery = null;
		}
	}

	
	private void ResetDSFlags()
	{
		CoopPeerStarter.Dedicated = false;
		CoopPeerStarter.DedicatedHost = false;
		SteamClientDSConfig.isDedicatedClient = false;
		SteamClientDSConfig.isDSFirstClient = false;
		SteamDSConfig.isDedicatedServer = false;
	}

	
	private void RefreshUI()
	{
		foreach (GameObject gameObject in this._hostOnlyGOs)
		{
			if (gameObject)
			{
				gameObject.SetActive(GameSetup.IsMpServer);
			}
		}
		foreach (GameObject gameObject2 in this._clientOnlyGOs)
		{
			if (gameObject2)
			{
				gameObject2.SetActive(GameSetup.IsMpClient);
			}
		}
	}

	
	public void OpenScreen(CoopSteamNGUI.Screens screen)
	{
		this.ClearLoadingAndError();
		this._prevScreen = this._currentScreen;
		this._modalScreen._screen.SetActive(screen == CoopSteamNGUI.Screens.ModalScreen);
		this._inviteReceivedScreen._screen.SetActive(screen == CoopSteamNGUI.Screens.InviteReceivedScreen);
		this._lobbySetupScreen._screen.SetActive(screen == CoopSteamNGUI.Screens.LobbySetup);
		this._lobbyScreen._screen.SetActive(screen == CoopSteamNGUI.Screens.Lobby);
		this._gameBrowserScreen._screen.SetActive(screen == CoopSteamNGUI.Screens.GameBrowser);
		if (this._gameBrowserScreenDS._screen)
		{
			this._gameBrowserScreenDS._screen.SetActive(screen == CoopSteamNGUI.Screens.GameBrowserDS);
		}
		this._gameBrowserScreen._sources.SetActive(screen == CoopSteamNGUI.Screens.GameBrowser || screen == CoopSteamNGUI.Screens.GameBrowserDS);
		if (this._joinDsScreen._screen)
		{
			this._joinDsScreen._screen.SetActive(screen == CoopSteamNGUI.Screens.JoinDS);
		}
		if (this._joinP2PScreen._screen)
		{
			this._joinP2PScreen._screen.SetActive(screen == CoopSteamNGUI.Screens.JoinP2P);
		}
		this._currentScreen = screen;
		if (screen == CoopSteamNGUI.Screens.GameBrowser || screen == CoopSteamNGUI.Screens.GameBrowserDS)
		{
			this._previouslyPlayedServers = SaveSlotUtils.GetPreviouslyPlayedServers();
		}
	}

	
	private string GetHostGameName()
	{
		if (this._hostGameName == null || this._hostGameName.Trim().Length == 0)
		{
			return SteamFriends.GetPersonaName() + "'s game";
		}
		return this._hostGameName.Trim() + " (" + SteamFriends.GetPersonaName() + ")";
	}

	
	private int GetHostPlayersMax()
	{
		int result;
		try
		{
			result = Mathf.Clamp(int.Parse(this._hostMaxPlayers), 2, 8);
		}
		catch
		{
			result = 4;
		}
		return result;
	}

	
	public void SetJoinText(ulong steamIDLobby)
	{
		base.StartCoroutine(this.DelayedInviteReceived(new CoopLobbyInfo(steamIDLobby)));
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
		this._currentInvitelobby = lobby;
		this._inviteReceivedScreen._continueSaveButton.isEnabled = SaveSlotUtils.GetPreviouslyPlayedServers().Contains(lobby.Guid);
		this._inviteReceivedScreen._gameName.text = lobby.Name;
		this.OpenScreen(CoopSteamNGUI.Screens.InviteReceivedScreen);
		yield break;
	}

	
	private void SetLoadingText(string text)
	{
		this._modalScreen._text.text = text;
		this.OpenScreen(CoopSteamNGUI.Screens.ModalScreen);
	}

	
	public void SetErrorText(string text)
	{
		if (this._errorLabel == null)
		{
			return;
		}
		this._errorLabel.SetActiveSelfSafe(!text.NullOrEmpty());
		this._errorLabel.text = text;
	}

	
	private void ClearLoadingAndError()
	{
		this._modalScreen._screen.SetActive(false);
		this._errorLabel.text = string.Empty;
	}

	
	private void UpdateLobby()
	{
		if (!CoopLobby.IsInLobby)
		{
			this.OnBack();
			return;
		}
		if (CoopLobby.Instance == null || CoopLobby.Instance.Info == null || CoopLobby.Instance.Info.Destroyed)
		{
			this.SetErrorText(UiTranslationDatabase.TranslateKey("LOBBY_DESTROYED", "Lobby Destroyed", this._allCapsTexts));
			this.OnBack();
			CoopLobby.LeaveActive();
			return;
		}
		if (!CoopLobby.Instance.Info.IsOwner && CoopLobby.Instance.Info.ServerId.IsValid())
		{
			if (!BoltNetwork.isClient && !base.gameObject.GetComponent<CoopSteamClientStarter>())
			{
				base.gameObject.AddComponent<CoopSteamClientStarter>().gui = this;
				if (GameSetup.IsNewGame)
				{
					PlaneCrashAudioState.Spawn();
				}
				this.SetLoadingText(UiTranslationDatabase.TranslateKey("STARTING_CLIENT___", "Starting Client...", this._allCapsTexts));
			}
		}
		else
		{
			bool foundHost = false;
			ulong ownerId = SteamMatchmaking.GetLobbyOwner(CoopLobby.Instance.Info.LobbyId).m_SteamID;
			this._lobbyScreen._playerCountLabel.text = StringEx.TryFormat(UiTranslationDatabase.TranslateKey("PLAYER_CURRENT_OVER_MAX", "PLAYERS: {0} / {1}", this._allCapsTexts), new object[]
			{
				CoopLobby.Instance.MemberCount,
				CoopLobby.Instance.Info.MemberLimit
			});
			this._lobbyScreen._playerListLabel.text = CoopLobby.Instance.AllMembers.Select(delegate(CSteamID x)
			{
				string text = SteamFriends.GetFriendPersonaName(x);
				bool flag = x.m_SteamID == ownerId;
				if (flag)
				{
					text += " (Host)";
					foundHost = true;
				}
				return text;
			}).Aggregate((string a, string b) => a + "\n" + b);
			if (!foundHost)
			{
				this.OnBack();
			}
		}
	}

	
	private void UpdateGameBrowser()
	{
		bool flag = false;
		if (CoopLobby.IsInLobby)
		{
			this.OpenScreen(CoopSteamNGUI.Screens.Lobby);
			this._lobbyScreen._gameNameLabel.text = StringEx.TryFormat(UiTranslationDatabase.TranslateKey("LOBBY_GAME_NAME", "LOBBY: {0}", this._allCapsTexts), new object[]
			{
				CoopLobby.Instance.Info.Name.ToUpperInvariant()
			});
			return;
		}
		this._lobbies = (from l in this._lobbies
		where l != null && CoopLobbyManager.Lobbies.Any((CoopLobbyInfo al) => al.LobbyId.m_SteamID == l.LobbyId.m_SteamID)
		select l).ToList<CoopLobbyInfo>();
		IEnumerable<CoopLobbyInfo> enumerable = (from nl in CoopLobbyManager.Lobbies
		where !string.IsNullOrEmpty(nl.Name) && (this._allowLegacyGames || !string.IsNullOrEmpty(nl.Guid)) && !nl.IsOwner && !this._lobbies.Any((CoopLobbyInfo l) => nl.LobbyId.m_SteamID == l.LobbyId.m_SteamID)
		select nl).Take(5);
		if (enumerable != null && enumerable.Any<CoopLobbyInfo>())
		{
			Vector3 localScale = this._gameBrowserScreen._rowPrefab.transform.localScale;
			foreach (CoopLobbyInfo coopLobbyInfo in enumerable)
			{
				MpGameRow mpGameRow = UnityEngine.Object.Instantiate<MpGameRow>(this._gameBrowserScreen._rowPrefab);
				mpGameRow.transform.parent = this._gameBrowserScreen._grid.transform;
				mpGameRow.transform.localScale = localScale;
				mpGameRow._gameName.text = coopLobbyInfo.Name;
				mpGameRow._lobby = coopLobbyInfo;
				mpGameRow._playerLimit.text = StringEx.TryFormat("{0} / {1}", new object[]
				{
					coopLobbyInfo.CurrentMembers,
					coopLobbyInfo.MemberLimit
				});
				this._gameRows[coopLobbyInfo] = mpGameRow;
				mpGameRow._previousPlayed = this._previouslyPlayedServers.Contains(mpGameRow._lobby.Guid);
				if (mpGameRow._previousPlayed)
				{
					mpGameRow.name = "0";
					mpGameRow._newButtonLabel.transform.parent.gameObject.SetActive(true);
					mpGameRow._continueButtonLabel.text = "Continue";
				}
				else
				{
					mpGameRow._newButtonLabel.transform.parent.gameObject.SetActive(false);
					mpGameRow.name = "1";
				}
				MpGameRow mpGameRow2 = mpGameRow;
				mpGameRow2.name += coopLobbyInfo.Name.Substring(0, 6);
			}
			this._lobbies = this._lobbies.Union(enumerable).ToList<CoopLobbyInfo>();
			flag = true;
		}
		bool flag2 = !string.IsNullOrEmpty(this._browserFilter);
		foreach (MpGameRow mpGameRow3 in this._gameRows.Values)
		{
			if (flag2)
			{
				bool flag3 = mpGameRow3._lobby.Name.ToLowerInvariant().Contains(this._browserFilter);
				if (mpGameRow3.gameObject.activeSelf != flag3)
				{
					mpGameRow3.transform.parent = ((!flag3) ? this._gameBrowserScreen._grid.transform.parent : this._gameBrowserScreen._grid.transform);
					mpGameRow3.gameObject.SetActive(flag3);
					flag = true;
				}
			}
			else if (!mpGameRow3.gameObject.activeSelf)
			{
				mpGameRow3.transform.parent = this._gameBrowserScreen._grid.transform;
				mpGameRow3.gameObject.SetActive(true);
				flag = true;
			}
		}
		if (flag)
		{
			this._gameBrowserScreen._grid.Reposition();
			this._gameBrowserScreen._scrollview.UpdateScrollbars();
			this._gameBrowserScreen._scrollview.verticalScrollBar.value = 1f;
			this._gameBrowserScreen._scrollview.verticalScrollBar.value = 0f;
			if (this._gameBrowserScreen._firstSelectControl && this._gameBrowserScreen._grid.transform.childCount > 0)
			{
				this._gameBrowserScreen._firstSelectControl.ObjectToBeSelected = this._gameBrowserScreen._grid.GetChild(0).GetComponent<MpGameRow>()._continueButtonLabel.transform.parent.gameObject;
			}
		}
	}

	
	public bool _allowLegacyGames;

	
	public LoadAsync _async;

	
	public UILabel _errorLabel;

	
	public CoopSteamNGUI.ModalScreen _modalScreen;

	
	public CoopSteamNGUI.InviteReceivedScreen _inviteReceivedScreen;

	
	public CoopSteamNGUI.LobbySetupScreen _lobbySetupScreen;

	
	public CoopSteamNGUI.LobbyScreen _lobbyScreen;

	
	public CoopSteamNGUI.GameBrowserScreen _gameBrowserScreen;

	
	public CoopSteamNGUI.GameBrowserScreen _gameBrowserScreenDS;

	
	public CoopSteamNGUI.JoinServerScreen _joinDsScreen;

	
	public CoopSteamNGUI.JoinP2PScreen _joinP2PScreen;

	
	public GameObject[] _hostOnlyGOs;

	
	public GameObject[] _clientOnlyGOs;

	
	public CoopLobbyInfo _currentInvitelobby;

	
	public gameserveritem_t JoiningServer;

	
	public GameObject _lobbyFullMessage;

	
	public bool _allCapsTexts = true;

	
	private bool _hostFriendsOnly;

	
	private string _hostGameName = "The Forest Game";

	
	private string _hostMaxPlayers = "4";

	
	private string _browserFilter = string.Empty;

	
	private CoopSteamNGUI.Screens _currentScreen;

	
	private CoopSteamNGUI.Screens _prevScreen;

	
	private IEnumerable<CoopLobbyInfo> _lobbies = new List<CoopLobbyInfo>(0);

	
	private Dictionary<CoopLobbyInfo, MpGameRow> _gameRows = new Dictionary<CoopLobbyInfo, MpGameRow>();

	
	private HashSet<string> _previouslyPlayedServers;

	
	private bool _showFriendGames;

	
	public enum Screens
	{
		
		ModalScreen,
		
		LobbySetup,
		
		Lobby,
		
		GameBrowser,
		
		InviteReceivedScreen,
		
		GameBrowserDS,
		
		JoinDS,
		
		JoinP2P
	}

	
	[Serializable]
	public class ModalScreen
	{
		
		public GameObject _screen;

		
		public UILabel _text;
	}

	
	[Serializable]
	public class InviteReceivedScreen
	{
		
		public GameObject _screen;

		
		public UILabel _gameName;

		
		public UIButton _continueSaveButton;
	}

	
	[Serializable]
	public class LobbySetupScreen
	{
		
		public GameObject _screen;

		
		public UIInput _gameNameInput;

		
		public UIPopupList _playerCountInput;

		
		public UIToggle _privateOnlyToggle;
	}

	
	[Serializable]
	public class LobbyScreen
	{
		
		public GameObject _screen;

		
		public UILabel _gameNameLabel;

		
		public UILabel _playerCountLabel;

		
		public UILabel _playerListLabel;
	}

	
	[Serializable]
	public class GameBrowserScreen
	{
		
		public GameObject _screen;

		
		public MpGameRow _rowPrefab;

		
		public UIScrollView _scrollview;

		
		public UIGrid _grid;

		
		public UIInput _filter;

		
		public GameObject _sources;

		
		public FirstSelectControl _firstSelectControl;
	}

	
	[Serializable]
	public class JoinServerScreen
	{
		
		public GameObject _screen;

		
		public UILabel _serverName;

		
		public UILabel _ip;

		
		public UILabel _ping;

		
		public UILabel _playerLimit;

		
		public GameObject _prefabDbVersionMissmatch;

		
		public GameObject _prefabServerIsFull;

		
		public UIInput _password;

		
		public UIInput _adminPassword;

		
		public UILabel _continueButtonLabel;

		
		public UILabel _newButtonLabel;
	}

	
	[Serializable]
	public class JoinP2PScreen
	{
		
		public GameObject _screen;

		
		public UILabel _gameNameLabel;
	}
}
