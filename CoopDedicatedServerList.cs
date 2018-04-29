using System;
using System.Collections.Generic;
using Steamworks;
using TheForest.UI;
using UnityEngine;


public class CoopDedicatedServerList : MonoBehaviour
{
	
	private void Awake()
	{
		bool initialized = SteamManager.Initialized;
		this.ToggleSortingOption(CoopDedicatedServerList.SortingOptions.NoVersionMismatch, true);
		this.ToggleSortingOption(CoopDedicatedServerList.SortingOptions.PreviouslyPlayed, true);
	}

	
	private void OnEnable()
	{
		if (this._response == null)
		{
			this._listSteamServer = new List<CoopDedicatedServerList.SteamServerData>();
			this._previusInstantiateServerRowIsFinished = true;
			this._dragUI.scrollView.ResetPosition();
			this.OnDisable();
			this._mainUI.RefreshBrowserOverride = new Action(this.ForceRefresh);
			this._dragUI.gameObject.SetActive(false);
			this.listCount = 0;
			if (this._spinner)
			{
				this._spinner.SetActive(true);
			}
			CoopDedicatedServerList.Source source = this._source;
			if (source != CoopDedicatedServerList.Source.Internet)
			{
				if (source != CoopDedicatedServerList.Source.Lan)
				{
					if (source == CoopDedicatedServerList.Source.Favorites)
					{
						this.FetchFavoritesServers();
					}
				}
				else
				{
					this.FetchLanServers();
				}
			}
			else
			{
				this.FetchInternetServers();
			}
		}
	}

	
	private void Update()
	{
		if (!this._skipFrame)
		{
			if (this._listSteamServer.Count > 0 && this._previusInstantiateServerRowIsFinished)
			{
				this._skipFrame = true;
				this._previusInstantiateServerRowIsFinished = false;
				this.ProcessServerResponded(this._listSteamServer[0]._hRequest, this._listSteamServer[0]._iServer);
				this._listSteamServer.RemoveAt(0);
			}
		}
		else
		{
			this._skipFrame = false;
		}
	}

	
	private void LateUpdate()
	{
		bool flag = false;
		bool flag2 = !string.IsNullOrEmpty(this._mainUI._gameBrowserScreenDS._filter.value);
		string value = this._mainUI._gameBrowserScreenDS._filter.value.ToLowerInvariant();
		foreach (MpGameRow mpGameRow in this._rows)
		{
			if (flag2)
			{
				bool flag3 = mpGameRow._gameName.text.ToLowerInvariant().Contains(value);
				if (mpGameRow.gameObject.activeSelf != flag3)
				{
					mpGameRow.transform.parent = ((!flag3) ? this._mainUI._gameBrowserScreenDS._grid.transform.parent : this._mainUI._gameBrowserScreenDS._grid.transform);
					mpGameRow.gameObject.SetActive(flag3);
					flag = true;
				}
			}
			else if (!mpGameRow.gameObject.activeSelf)
			{
				mpGameRow.transform.parent = this._mainUI._gameBrowserScreenDS._grid.transform;
				mpGameRow.gameObject.SetActive(true);
				flag = true;
			}
		}
		if (flag)
		{
			this._mainUI._gameBrowserScreenDS._grid.repositionNow = true;
			this._mainUI._gameBrowserScreenDS._scrollview.UpdateScrollbars();
		}
	}

	
	private void OnDisable()
	{
		if (this._mainUI.RefreshBrowserOverride == new Action(this.ForceRefresh))
		{
			this._mainUI.RefreshBrowserOverride = null;
		}
		if (this._rows != null)
		{
			foreach (MpGameRow mpGameRow in this._rows)
			{
				UnityEngine.Object.Destroy(mpGameRow.gameObject);
			}
			this._rows.Clear();
		}
		this.FinalizeRequest(true);
	}

	
	private void CheckValidScrollBar()
	{
		Debug.Log("serverCount " + this.listCount);
		if (this.listCount <= 8)
		{
			this._dragUI.gameObject.SetActive(false);
			this._mainUI._gameBrowserScreenDS._scrollview.verticalScrollBar.gameObject.SetActive(false);
		}
		else
		{
			this._mainUI._gameBrowserScreenDS._scrollview.verticalScrollBar.gameObject.SetActive(true);
			this._dragUI.gameObject.SetActive(true);
			this._mainUI._gameBrowserScreenDS._scrollview.UpdateScrollbars();
		}
	}

	
	public void TurnOn()
	{
		base.gameObject.SetActive(true);
	}

	
	public void TurnOff()
	{
		base.gameObject.SetActive(false);
	}

	
	private void FetchInternetServers()
	{
		this._response = new ISteamMatchmakingServerListResponse(new ISteamMatchmakingServerListResponse.ServerResponded(this.ServerResponded), new ISteamMatchmakingServerListResponse.ServerFailedToRespond(this.ServerFailedToRespond), new ISteamMatchmakingServerListResponse.RefreshComplete(this.RefreshComplete));
		this._request = SteamMatchmakingServers.RequestInternetServerList(new AppId_t(242760u), new MatchMakingKeyValuePair_t[]
		{
			new MatchMakingKeyValuePair_t
			{
				m_szKey = "gamedir",
				m_szValue = "TheForest"
			}
		}, 0u, this._response);
	}

	
	private void FetchFavoritesServers()
	{
		this._response = new ISteamMatchmakingServerListResponse(new ISteamMatchmakingServerListResponse.ServerResponded(this.ServerResponded), new ISteamMatchmakingServerListResponse.ServerFailedToRespond(this.ServerFailedToRespond), new ISteamMatchmakingServerListResponse.RefreshComplete(this.RefreshComplete));
		this._request = SteamMatchmakingServers.RequestFavoritesServerList(new AppId_t(242760u), new MatchMakingKeyValuePair_t[]
		{
			new MatchMakingKeyValuePair_t
			{
				m_szKey = "gamedir",
				m_szValue = "TheForest"
			}
		}, 0u, this._response);
	}

	
	private void FetchLanServers()
	{
		this._response = new ISteamMatchmakingServerListResponse(new ISteamMatchmakingServerListResponse.ServerResponded(this.ServerResponded), new ISteamMatchmakingServerListResponse.ServerFailedToRespond(this.ServerFailedToRespond), new ISteamMatchmakingServerListResponse.RefreshComplete(this.RefreshComplete));
		this._request = SteamMatchmakingServers.RequestLANServerList(new AppId_t(242760u), this._response);
	}

	
	private void ServerResponded(HServerListRequest hRequest, int iServer)
	{
		this._listSteamServer.Add(new CoopDedicatedServerList.SteamServerData(hRequest, iServer));
	}

	
	private void ProcessServerResponded(HServerListRequest hRequest, int iServer)
	{
		this.listCount++;
		gameserveritem_t serverDetails = SteamMatchmakingServers.GetServerDetails(hRequest, iServer);
		if (serverDetails.m_nAppID == 242760u || serverDetails.m_nAppID == 556450u)
		{
			MpDedicatedServerRow mpDedicatedServerRow = UnityEngine.Object.Instantiate<MpDedicatedServerRow>(this._rowPrefab);
			mpDedicatedServerRow.transform.parent = this._mainUI._gameBrowserScreenDS._grid.transform;
			mpDedicatedServerRow.transform.localScale = this._rowPrefab.transform.localScale;
			mpDedicatedServerRow.gameObject.SetActive(true);
			this._previusInstantiateServerRowIsFinished = true;
			mpDedicatedServerRow._gameName.text = serverDetails.GetServerName();
			mpDedicatedServerRow._ip.text = serverDetails.m_NetAdr.GetConnectionAddressString();
			mpDedicatedServerRow._ping.text = serverDetails.m_nPing + "ms";
			mpDedicatedServerRow._VACProtected.SetActive(serverDetails.m_bSecure);
			mpDedicatedServerRow._passwordProtected.SetActive(serverDetails.m_bPassword);
			mpDedicatedServerRow.Server = serverDetails;
			mpDedicatedServerRow._playerLimit.text = string.Format("{0} / {1}", serverDetails.m_nPlayers, serverDetails.m_nMaxPlayers);
			this._rows.Add(mpDedicatedServerRow);
			string[] array = serverDetails.GetGameTags().Split(new char[]
			{
				';'
			});
			string item = (array == null || array.Length <= 0) ? string.Empty : array[0];
			string a = (array == null || array.Length <= 1) ? "-1" : array[1];
			bool flag = a != "__F486E3E06B8E13E0388571BE0FDC8A35182D8BE83E9256BA53BC5FBBDBCF23BC";
			mpDedicatedServerRow._versionMismatch = flag;
			mpDedicatedServerRow._prefabDbVersionMissmatch.SetActive(flag);
			mpDedicatedServerRow._previousPlayed = this._mainUI.PreviouslyPlayedServers.Contains(item);
			mpDedicatedServerRow.RefreshName(this._sortOptions);
			bool flag2 = !string.IsNullOrEmpty(this._mainUI._gameBrowserScreenDS._filter.value);
			if (flag2)
			{
				string value = this._mainUI._gameBrowserScreenDS._filter.value.ToLowerInvariant();
				bool flag3 = mpDedicatedServerRow._gameName.text.ToLowerInvariant().Contains(value);
				if (mpDedicatedServerRow.gameObject.activeSelf != flag3)
				{
					mpDedicatedServerRow.transform.parent = ((!flag3) ? this._mainUI._gameBrowserScreenDS._grid.transform.parent : this._mainUI._gameBrowserScreenDS._grid.transform);
					mpDedicatedServerRow.gameObject.SetActive(flag3);
				}
			}
			else if (!mpDedicatedServerRow.gameObject.activeSelf)
			{
				mpDedicatedServerRow.transform.parent = this._mainUI._gameBrowserScreenDS._grid.transform;
				mpDedicatedServerRow.gameObject.SetActive(true);
			}
			this._mainUI._gameBrowserScreenDS._grid.repositionNow = true;
			this._mainUI._gameBrowserScreenDS._scrollview.UpdateScrollbars();
		}
	}

	
	private void ServerFailedToRespond(HServerListRequest hRequest, int iServer)
	{
		Debug.LogWarning("CoopDedicatedServerList::ServerFailedToRespond");
		try
		{
		}
		catch (Exception ex)
		{
			Debug.LogError("CoopDedicatedServerList::ServerFailedToRespond " + ex.Message);
			Debug.LogError("CoopDedicatedServerList::ServerFailedToRespond " + ex.StackTrace);
		}
	}

	
	private void RefreshComplete(HServerListRequest hRequest, EMatchMakingServerResponse response)
	{
		try
		{
			this.serverCount = SteamMatchmakingServers.GetServerCount(hRequest);
			this.FinalizeRequest(false);
		}
		catch
		{
		}
		finally
		{
			if (this._spinner)
			{
				this._spinner.SetActive(false);
			}
		}
	}

	
	private void FinalizeRequest(bool forDisable = false)
	{
		SteamMatchmakingServers.ReleaseRequest(this._request);
		this._response = null;
		if (this._spinner)
		{
			this._spinner.SetActive(false);
		}
		if (!forDisable)
		{
			foreach (MpGameRow mpGameRow in this._rows)
			{
				MpDedicatedServerRow mpDedicatedServerRow = (MpDedicatedServerRow)mpGameRow;
				if (mpDedicatedServerRow)
				{
					UITweener componentInChildren = mpDedicatedServerRow._VACProtected.GetComponentInChildren<UITweener>();
					if (componentInChildren)
					{
						componentInChildren.enabled = true;
					}
				}
			}
			this._mainUI._gameBrowserScreenDS._grid.repositionNow = true;
			this._mainUI._gameBrowserScreenDS._scrollview.UpdateScrollbars();
			this.CheckValidScrollBar();
		}
	}

	
	private void ForceRefresh()
	{
		if (this._response == null)
		{
			this.FinalizeRequest(true);
			this.OnEnable();
		}
	}

	
	~CoopDedicatedServerList()
	{
		try
		{
			this.FinalizeRequest(true);
		}
		catch
		{
		}
	}

	
	private void SortByPreviouslyPlayed(MpGameRow row)
	{
		if (row._previousPlayed)
		{
			row.name += "0";
		}
		else
		{
			row.name += "1";
		}
	}

	
	private void SortByNotFull(MpGameRow row)
	{
		MpDedicatedServerRow mpDedicatedServerRow = (MpDedicatedServerRow)row;
		if (mpDedicatedServerRow)
		{
			if (mpDedicatedServerRow.Server.m_nMaxPlayers - mpDedicatedServerRow.Server.m_nPlayers != 0)
			{
				MpDedicatedServerRow mpDedicatedServerRow2 = mpDedicatedServerRow;
				mpDedicatedServerRow2.name += "0";
			}
			else
			{
				MpDedicatedServerRow mpDedicatedServerRow3 = mpDedicatedServerRow;
				mpDedicatedServerRow3.name += "1";
			}
		}
	}

	
	private void SortByHasPlayers(MpGameRow row)
	{
		MpDedicatedServerRow mpDedicatedServerRow = (MpDedicatedServerRow)row;
		if (mpDedicatedServerRow)
		{
			if (mpDedicatedServerRow.Server.m_nPlayers != 0)
			{
				MpDedicatedServerRow mpDedicatedServerRow2 = mpDedicatedServerRow;
				mpDedicatedServerRow2.name += "0";
			}
			else
			{
				MpDedicatedServerRow mpDedicatedServerRow3 = mpDedicatedServerRow;
				mpDedicatedServerRow3.name += "1";
			}
		}
	}

	
	private void SortByNoPlayers(MpGameRow row)
	{
		MpDedicatedServerRow mpDedicatedServerRow = (MpDedicatedServerRow)row;
		if (mpDedicatedServerRow)
		{
			if (mpDedicatedServerRow.Server.m_nPlayers == 0)
			{
				MpDedicatedServerRow mpDedicatedServerRow2 = mpDedicatedServerRow;
				mpDedicatedServerRow2.name += "0";
			}
			else
			{
				MpDedicatedServerRow mpDedicatedServerRow3 = mpDedicatedServerRow;
				mpDedicatedServerRow3.name += "1";
			}
		}
	}

	
	private void SortByNoPassword(MpGameRow row)
	{
		MpDedicatedServerRow mpDedicatedServerRow = (MpDedicatedServerRow)row;
		if (mpDedicatedServerRow)
		{
			if (!mpDedicatedServerRow._passwordProtected)
			{
				MpDedicatedServerRow mpDedicatedServerRow2 = mpDedicatedServerRow;
				mpDedicatedServerRow2.name += "0";
			}
			else
			{
				MpDedicatedServerRow mpDedicatedServerRow3 = mpDedicatedServerRow;
				mpDedicatedServerRow3.name += "1";
			}
		}
	}

	
	private void SortByPasswordProtected(MpGameRow row)
	{
		MpDedicatedServerRow mpDedicatedServerRow = (MpDedicatedServerRow)row;
		if (mpDedicatedServerRow)
		{
			if (mpDedicatedServerRow._passwordProtected)
			{
				MpDedicatedServerRow mpDedicatedServerRow2 = mpDedicatedServerRow;
				mpDedicatedServerRow2.name += "0";
			}
			else
			{
				MpDedicatedServerRow mpDedicatedServerRow3 = mpDedicatedServerRow;
				mpDedicatedServerRow3.name += "1";
			}
		}
	}

	
	private void SortByNoVersionMismatch(MpGameRow row)
	{
		MpDedicatedServerRow mpDedicatedServerRow = (MpDedicatedServerRow)row;
		if (mpDedicatedServerRow)
		{
			if (!mpDedicatedServerRow._versionMismatch)
			{
				MpDedicatedServerRow mpDedicatedServerRow2 = mpDedicatedServerRow;
				mpDedicatedServerRow2.name += "0";
			}
			else
			{
				MpDedicatedServerRow mpDedicatedServerRow3 = mpDedicatedServerRow;
				mpDedicatedServerRow3.name += "1";
			}
		}
	}

	
	private void SortBySecure(MpGameRow row)
	{
		MpDedicatedServerRow mpDedicatedServerRow = (MpDedicatedServerRow)row;
		if (mpDedicatedServerRow)
		{
			if (mpDedicatedServerRow._VACProtected)
			{
				MpDedicatedServerRow mpDedicatedServerRow2 = mpDedicatedServerRow;
				mpDedicatedServerRow2.name += "0";
			}
			else
			{
				MpDedicatedServerRow mpDedicatedServerRow3 = mpDedicatedServerRow;
				mpDedicatedServerRow3.name += "1";
			}
		}
	}

	
	private void SortByNotSecure(MpGameRow row)
	{
		MpDedicatedServerRow mpDedicatedServerRow = (MpDedicatedServerRow)row;
		if (mpDedicatedServerRow)
		{
			if (!mpDedicatedServerRow._VACProtected)
			{
				MpDedicatedServerRow mpDedicatedServerRow2 = mpDedicatedServerRow;
				mpDedicatedServerRow2.name += "0";
			}
			else
			{
				MpDedicatedServerRow mpDedicatedServerRow3 = mpDedicatedServerRow;
				mpDedicatedServerRow3.name += "1";
			}
		}
	}

	
	public void ToggleSortingOption(CoopDedicatedServerList.SortingOptions option, bool onoff)
	{
		switch (option)
		{
		case CoopDedicatedServerList.SortingOptions.PreviouslyPlayed:
			this.PushSortingOptionFunction(new Action<MpGameRow>(this.SortByPreviouslyPlayed), onoff);
			break;
		case CoopDedicatedServerList.SortingOptions.Notfull:
			this.PushSortingOptionFunction(new Action<MpGameRow>(this.SortByHasPlayers), false);
			this.PushSortingOptionFunction(new Action<MpGameRow>(this.SortByNoPlayers), false);
			this.PushSortingOptionFunction(new Action<MpGameRow>(this.SortByNotFull), onoff);
			break;
		case CoopDedicatedServerList.SortingOptions.HasPlayers:
			this.PushSortingOptionFunction(new Action<MpGameRow>(this.SortByNotFull), false);
			this.PushSortingOptionFunction(new Action<MpGameRow>(this.SortByNoPlayers), false);
			this.PushSortingOptionFunction(new Action<MpGameRow>(this.SortByHasPlayers), onoff);
			break;
		case CoopDedicatedServerList.SortingOptions.NoPlayers:
			this.PushSortingOptionFunction(new Action<MpGameRow>(this.SortByHasPlayers), false);
			this.PushSortingOptionFunction(new Action<MpGameRow>(this.SortByNotFull), false);
			this.PushSortingOptionFunction(new Action<MpGameRow>(this.SortByNoPlayers), onoff);
			break;
		case CoopDedicatedServerList.SortingOptions.NoPassword:
			this.PushSortingOptionFunction(new Action<MpGameRow>(this.SortByPasswordProtected), false);
			this.PushSortingOptionFunction(new Action<MpGameRow>(this.SortByNoPassword), onoff);
			break;
		case CoopDedicatedServerList.SortingOptions.PasswordProtected:
			this.PushSortingOptionFunction(new Action<MpGameRow>(this.SortByNoPassword), false);
			this.PushSortingOptionFunction(new Action<MpGameRow>(this.SortByPasswordProtected), onoff);
			break;
		case CoopDedicatedServerList.SortingOptions.NoVersionMismatch:
			this.PushSortingOptionFunction(new Action<MpGameRow>(this.SortByNoVersionMismatch), onoff);
			break;
		case CoopDedicatedServerList.SortingOptions.Secure:
			this.PushSortingOptionFunction(new Action<MpGameRow>(this.SortByNotSecure), false);
			this.PushSortingOptionFunction(new Action<MpGameRow>(this.SortBySecure), onoff);
			break;
		case CoopDedicatedServerList.SortingOptions.NotSecure:
			this.PushSortingOptionFunction(new Action<MpGameRow>(this.SortBySecure), false);
			this.PushSortingOptionFunction(new Action<MpGameRow>(this.SortByNotSecure), onoff);
			break;
		}
		for (int i = 0; i < this._rows.Count; i++)
		{
			this._rows[i].RefreshName(this._sortOptions);
		}
		this._mainUI._gameBrowserScreenDS._grid.repositionNow = true;
		this._mainUI._gameBrowserScreenDS._scrollview.UpdateScrollbars();
		this.CheckValidScrollBar();
	}

	
	private void PushSortingOptionFunction(Action<MpGameRow> action, bool onoff)
	{
		if (this._sortOptions.Contains(action))
		{
			this._sortOptions.Remove(action);
		}
		if (onoff)
		{
			this._sortOptions.Add(action);
		}
	}

	
	public CoopDedicatedServerList.Source _source;

	
	public MpDedicatedServerRow _rowPrefab;

	
	public CoopSteamNGUI _mainUI;

	
	public GameObject _spinner;

	
	public UIDragScrollView _dragUI;

	
	private List<MpDedicatedServerRow> _rows = new List<MpDedicatedServerRow>();

	
	private HServerListRequest _request;

	
	private ISteamMatchmakingServerListResponse _response;

	
	private int serverCount;

	
	private int listCount;

	
	private List<Action<MpGameRow>> _sortOptions = new List<Action<MpGameRow>>();

	
	private List<CoopDedicatedServerList.SteamServerData> _listSteamServer = new List<CoopDedicatedServerList.SteamServerData>();

	
	private bool _previusInstantiateServerRowIsFinished = true;

	
	private bool _skipFrame;

	
	private class SteamServerData
	{
		
		public SteamServerData(HServerListRequest hRequest, int iServer)
		{
			this._hRequest = hRequest;
			this._iServer = iServer;
		}

		
		public HServerListRequest _hRequest;

		
		public int _iServer;
	}

	
	public enum Source
	{
		
		Internet,
		
		Favorites,
		
		Lan
	}

	
	public enum SortingOptions
	{
		
		PreviouslyPlayed,
		
		Notfull,
		
		HasPlayers,
		
		NoPlayers,
		
		NoPassword,
		
		PasswordProtected,
		
		NoVersionMismatch,
		
		Secure,
		
		NotSecure
	}
}
