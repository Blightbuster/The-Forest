using System;
using System.Collections.Generic;
using System.Diagnostics;
using Steamworks;
using TheForest.UI;
using UniLinq;
using UnityEngine;

public class CoopDedicatedServerList : MonoBehaviour
{
	private void Awake()
	{
		bool initialized = SteamManager.Initialized;
		this.ToggleSortingOption(CoopDedicatedServerList.SortingOptions.NoVersionMismatch, true);
		this.ToggleSortingOption(CoopDedicatedServerList.SortingOptions.PreviouslyPlayed, true);
		this._rowPrefab._continueButtonLabel.transform.parent.gameObject.SetActive(false);
		this._rowPrefab.transform.localPosition = new Vector3(-1000f, 0f, 0f);
		this._offGridRows = new GameObject();
		this._offGridRows.SetActive(false);
		this._offGridRows.transform.localPosition = new Vector3(3f, 0f, 0f);
		this._offGridRows.transform.localScale = this._mainUI._gameBrowserScreenDS._grid.transform.lossyScale;
	}

	private void OnEnable()
	{
		if (this._response == null)
		{
			this._requestSteamServers.Clear();
			this._steamServerData.Clear();
			this._mainUI._gameBrowserScreenDS._scrollview.UpdateScrollbars();
			this._dragUI.scrollView.ResetPosition();
			this.OnDisable();
			this._mainUI.RefreshBrowserOverride = new Action(this.ForceRefresh);
			this._dragUI.gameObject.SetActive(false);
			if (this._fillSprite && this._fillSprite.gameObject.activeSelf)
			{
				this._fillSprite.gameObject.SetActive(false);
			}
			this._totalServerRequested = 0;
			this._validServers = 0;
			this._processedServers = 0;
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
		float num = (float)((!ForestVR.Enabled) ? 30 : 50);
		this._timer.Reset();
		this._timer.Start();
		while (this._requestSteamServers.Count > 0)
		{
			this.ProcessServerResponded(this._requestSteamServers[0]._hRequest, this._requestSteamServers[0]._iServer);
			this._requestSteamServers.RemoveAt(0);
			if ((float)this._timer.ElapsedMilliseconds >= 1000f / num)
			{
				break;
			}
		}
		int num2 = this._steamServerData.Count<gameserveritem_t>();
		if (num2 > 0 && (this._response == null || num2 >= this._spawnThreshold))
		{
			num2 = Mathf.Clamp(num2, 0, this._spawnThreshold);
			while (num2-- > 0 && (float)this._timer.ElapsedMilliseconds < 1000f / num)
			{
				this.ProcessServerData(this._steamServerData[0]);
				this._steamServerData.RemoveAt(0);
			}
			if (this._nextGridRefresh < Time.realtimeSinceStartup || this._response == null)
			{
				this._mainUI._gameBrowserScreenDS._grid.repositionNow = true;
				this.CheckValidScrollBar();
				this._nextGridRefresh = Time.realtimeSinceStartup + this._gridRefreshDelay;
			}
			if (this._steamServerData.Count<gameserveritem_t>() == 0)
			{
				UnityEngine.Debug.Log(string.Concat(new object[]
				{
					"serverCount ",
					this._validServers,
					"/",
					this._totalServerRequested
				}));
			}
		}
		if (this._fillSprite)
		{
			if (this._response != null && this._totalServerRequested > 0)
			{
				this._fillSprite.fillAmount = (float)this._processedServers / (float)this._totalServerRequested;
				if (!this._fillSprite.gameObject.activeSelf)
				{
					this._fillSprite.gameObject.SetActive(true);
				}
			}
			else if (this._fillSprite.gameObject.activeSelf)
			{
				this._fillSprite.gameObject.SetActive(false);
			}
		}
		this._timer.Stop();
	}

	private void LateUpdate()
	{
		bool flag = !string.IsNullOrEmpty(this._mainUI._gameBrowserScreenDS._filter.value);
		string text = this._mainUI._gameBrowserScreenDS._filter.value.ToLowerInvariant();
		if (this._lastFilterValue != text)
		{
			this._lastFilterValue = text;
			this._nextFilterRefresh = Time.realtimeSinceStartup + this._filterRefreshDelay;
		}
		if (this._nextFilterRefresh < Time.realtimeSinceStartup)
		{
			this._nextFilterRefresh = float.MaxValue;
			bool flag2 = false;
			foreach (MpGameRow mpGameRow in this._rows)
			{
				if (flag)
				{
					bool flag3 = mpGameRow._gameName.text.ToLowerInvariant().Contains(text);
					if (mpGameRow.gameObject.activeInHierarchy != flag3)
					{
						mpGameRow.transform.parent = ((!flag3) ? this._offGridRows.transform : this._mainUI._gameBrowserScreenDS._grid.transform);
						flag2 = true;
					}
				}
				else if (!mpGameRow.gameObject.activeInHierarchy)
				{
					mpGameRow.transform.parent = this._mainUI._gameBrowserScreenDS._grid.transform;
					flag2 = true;
				}
			}
			if (flag2)
			{
				this._mainUI._gameBrowserScreenDS._grid.repositionNow = true;
				this._mainUI._gameBrowserScreenDS._scrollview.UpdateScrollbars();
			}
		}
	}

	private void OnDisable()
	{
		if (this._fillSprite && this._fillSprite.gameObject.activeSelf)
		{
			this._fillSprite.gameObject.SetActive(false);
		}
		if (this._mainUI.RefreshBrowserOverride == new Action(this.ForceRefresh))
		{
			this._mainUI.RefreshBrowserOverride = null;
		}
		if (this._rows != null)
		{
			foreach (MpDedicatedServerRow mpDedicatedServerRow in this._rows)
			{
				UnityEngine.Object.Destroy(mpDedicatedServerRow.gameObject);
			}
			this._rows.Clear();
		}
		this.FinalizeRequest(true);
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
		this._totalServerRequested = SteamMatchmakingServers.GetServerCount(this._request);
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
		this._totalServerRequested = SteamMatchmakingServers.GetServerCount(this._request);
	}

	private void FetchLanServers()
	{
		this._response = new ISteamMatchmakingServerListResponse(new ISteamMatchmakingServerListResponse.ServerResponded(this.ServerResponded), new ISteamMatchmakingServerListResponse.ServerFailedToRespond(this.ServerFailedToRespond), new ISteamMatchmakingServerListResponse.RefreshComplete(this.RefreshComplete));
		this._request = SteamMatchmakingServers.RequestLANServerList(new AppId_t(242760u), this._response);
		this._totalServerRequested = SteamMatchmakingServers.GetServerCount(this._request);
	}

	private void ServerResponded(HServerListRequest hRequest, int iServer)
	{
		this._requestSteamServers.Add(new CoopDedicatedServerList.RequestSteamServer(hRequest, iServer));
		if (this._totalServerRequested == 0)
		{
			this._totalServerRequested = SteamMatchmakingServers.GetServerCount(hRequest);
		}
	}

	private void ProcessServerResponded(HServerListRequest hRequest, int iServer)
	{
		gameserveritem_t serverDetails = SteamMatchmakingServers.GetServerDetails(hRequest, iServer);
		if (serverDetails.m_nAppID == 242760u || serverDetails.m_nAppID == 556450u)
		{
			this._validServers++;
			this._steamServerData.Add(serverDetails);
		}
	}

	private void ProcessServerData(gameserveritem_t server)
	{
		this._processedServers++;
		MpDedicatedServerRow row = this.GetRow();
		row._gameName.text = server.GetServerName();
		row._ping.text = server.m_nPing + "ms";
		if (row._passwordProtected.activeSelf != server.m_bPassword)
		{
			row._passwordProtected.SetActive(server.m_bPassword);
		}
		row.Server = server;
		row._playerLimit.text = string.Format("{0} / {1}", server.m_nPlayers, server.m_nMaxPlayers);
		this._rows.Add(row);
		string[] array = server.GetGameTags().Split(new char[]
		{
			';'
		});
		string item = (array == null || array.Length <= 0) ? string.Empty : array[0];
		string a = (array == null || array.Length <= 1) ? "-1" : array[1];
		bool flag = a != "__E3C26D06F07B6AB14EC25F4823E9A30D6B4ED0450527C1E768739D96C9F061AE";
		row._versionMismatch = flag;
		if (row._prefabDbVersionMissmatch.activeSelf != flag)
		{
			row._prefabDbVersionMissmatch.SetActive(flag);
		}
		row._previousPlayed = this._mainUI.PreviouslyPlayedServers.Contains(item);
		row.RefreshName(this._sortOptions);
		bool flag2 = !string.IsNullOrEmpty(this._lastFilterValue);
		if (flag2)
		{
			bool flag3 = row._gameName.text.ToLowerInvariant().Contains(this._lastFilterValue);
			if (row.gameObject.activeInHierarchy != flag3)
			{
				row.transform.SetParent((!flag3) ? this._offGridRows.transform : this._mainUI._gameBrowserScreenDS._grid.transform);
			}
		}
		else if (!row.gameObject.activeInHierarchy)
		{
			row.transform.SetParent(this._mainUI._gameBrowserScreenDS._grid.transform);
		}
	}

	private void ServerFailedToRespond(HServerListRequest hRequest, int iServer)
	{
		UnityEngine.Debug.LogWarning("CoopDedicatedServerList::ServerFailedToRespond");
		try
		{
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError("CoopDedicatedServerList::ServerFailedToRespond " + ex.Message);
			UnityEngine.Debug.LogError("CoopDedicatedServerList::ServerFailedToRespond " + ex.StackTrace);
		}
	}

	private void RefreshComplete(HServerListRequest hRequest, EMatchMakingServerResponse response)
	{
		try
		{
			this._totalServerRequested = SteamMatchmakingServers.GetServerCount(hRequest);
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

	private MpDedicatedServerRow GetRow()
	{
		if (this._rowsPool.Count > 0)
		{
			MpDedicatedServerRow mpDedicatedServerRow = this._rowsPool.Dequeue();
			if (mpDedicatedServerRow)
			{
				return mpDedicatedServerRow;
			}
		}
		return UnityEngine.Object.Instantiate<MpDedicatedServerRow>(this._rowPrefab, this._mainUI._gameBrowserScreenDS._grid.transform);
	}

	private void CheckValidScrollBar()
	{
		if (this._validServers <= 8)
		{
			if (this._dragUI.gameObject.activeSelf)
			{
				this._dragUI.gameObject.SetActive(false);
			}
			if (this._mainUI._gameBrowserScreenDS._scrollview.verticalScrollBar.gameObject.activeSelf)
			{
				this._mainUI._gameBrowserScreenDS._scrollview.verticalScrollBar.gameObject.SetActive(false);
			}
		}
		else
		{
			if (!this._mainUI._gameBrowserScreenDS._scrollview.verticalScrollBar.gameObject.activeSelf)
			{
				this._mainUI._gameBrowserScreenDS._scrollview.verticalScrollBar.gameObject.SetActive(true);
			}
			if (!this._dragUI.gameObject.activeSelf)
			{
				this._dragUI.gameObject.SetActive(true);
			}
			this._mainUI._gameBrowserScreenDS._scrollview.UpdateScrollbars();
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

	public UISprite _fillSprite;

	public UIDragScrollView _dragUI;

	public GameObject _offGridRows;

	public int _spawnThreshold = 50;

	public float _gridRefreshDelay = 3f;

	public float _filterRefreshDelay = 1.5f;

	private List<MpDedicatedServerRow> _rows = new List<MpDedicatedServerRow>();

	private Queue<MpDedicatedServerRow> _rowsPool = new Queue<MpDedicatedServerRow>();

	private HServerListRequest _request;

	private ISteamMatchmakingServerListResponse _response;

	private int _totalServerRequested;

	private int _validServers;

	private int _processedServers;

	private List<Action<MpGameRow>> _sortOptions = new List<Action<MpGameRow>>();

	private List<CoopDedicatedServerList.RequestSteamServer> _requestSteamServers = new List<CoopDedicatedServerList.RequestSteamServer>();

	private List<gameserveritem_t> _steamServerData = new List<gameserveritem_t>();

	private Stopwatch _timer = new Stopwatch();

	private float _nextGridRefresh;

	private float _nextFilterRefresh;

	private string _lastFilterValue = string.Empty;

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

	[Serializable]
	private class RequestSteamServer
	{
		public RequestSteamServer(HServerListRequest hRequest, int iServer)
		{
			this._hRequest = hRequest;
			this._iServer = iServer;
		}

		public HServerListRequest _hRequest;

		public int _iServer;
	}
}
