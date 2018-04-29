﻿using System;
using System.Collections.Generic;
using Steamworks;
using TheForest.Utils;
using UniLinq;
using UnityEngine;


public static class CoopLobbyManager
{
	
	
	public static IEnumerable<CoopLobbyInfo> Lobbies
	{
		get
		{
			return CoopLobbyManager.LobbyMatchList_Result.AsEnumerable<CoopLobbyInfo>();
		}
	}

	
	public static void QueryList()
	{
		CoopLobbyManager.Initialize();
		CoopLobbyManager.LobbyMatchList_Result = (from x in CoopLobbyManager.LobbyMatchList_Result
		where x.IsOwner
		select x).ToList<CoopLobbyInfo>();
		SteamMatchmaking.AddRequestLobbyListResultCountFilter(100);
		SteamMatchmaking.RequestLobbyList();
	}

	
	public static void Create(string name, int memberLimit, bool friendsOnly, Action callback, Action callbackFail)
	{
		CoopLobbyManager.Initialize();
		CoopLobbyManager.createCallback = callback;
		CoopLobbyManager.createFailCallback = callbackFail;
		CoopLobbyManager.createValues = new CoopLobbyInfo(0UL);
		CoopLobbyManager.createValues.Name = name;
		CoopLobbyManager.createValues.MemberLimit = memberLimit;
		SteamMatchmaking.CreateLobby((!friendsOnly) ? ELobbyType.k_ELobbyTypePublic : ELobbyType.k_ELobbyTypeFriendsOnly, memberLimit);
	}

	
	public static void Join(CoopLobbyInfo info, Action callback, Action<string> callbackFail)
	{
		CoopLobbyManager.Initialize();
		CoopLobby.LeaveActive();
		if (info.LobbyId.IsValid())
		{
			CoopLobbyManager.enterCallback = callback;
			CoopLobbyManager.enterFailCallback = callbackFail;
			SteamMatchmaking.JoinLobby(info.LobbyId);
		}
	}

	
	public static CoopLobbyInfo FindLobby(ulong id)
	{
		CoopLobbyManager.Initialize();
		return CoopLobbyManager.FindLobby(new CSteamID(id));
	}

	
	public static CoopLobbyInfo FindLobby(CSteamID id)
	{
		CoopLobbyManager.Initialize();
		return CoopLobbyManager.LobbyMatchList_Result.FirstOrDefault((CoopLobbyInfo x) => x.LobbyId == id);
	}

	
	private static void LobbyEnter(LobbyEnter_t param)
	{
		Debug.Log("LobbyEnter");
		try
		{
			CSteamID id = new CSteamID(param.m_ulSteamIDLobby);
			if (id.IsValid() && param.m_EChatRoomEnterResponse == 1u)
			{
				CoopLobbyInfo lobbyInfo = CoopLobbyManager.GetLobbyInfo(id);
				lobbyInfo.UpdateData();
				CoopLobby.SetActive(lobbyInfo);
				if (CoopLobbyManager.enterCallback != null)
				{
					CoopLobbyManager.enterCallback();
				}
			}
			else
			{
				string obj = string.Empty;
				if (id.IsValid())
				{
					if (param.m_EChatRoomEnterResponse == 4u)
					{
						obj = "FULL";
					}
					else
					{
						CoopLobbyInfo lobbyInfo2 = CoopLobbyManager.GetLobbyInfo(id);
						if (lobbyInfo2 != null && lobbyInfo2.CurrentMembers >= lobbyInfo2.MemberLimit)
						{
							obj = "FULL";
						}
					}
				}
				if (CoopLobbyManager.enterFailCallback != null)
				{
					Debug.Log("calling fail callback");
					CoopLobbyManager.enterFailCallback(obj);
				}
				else
				{
					Debug.Log("no fail callback");
				}
			}
		}
		finally
		{
			CoopLobbyManager.enterCallback = null;
			CoopLobbyManager.enterFailCallback = null;
		}
	}

	
	private static void LobbyCreated(LobbyCreated_t param)
	{
		Debug.Log(string.Concat(new object[]
		{
			"LobbyCreated param.m_eResult=",
			param.m_eResult,
			", lobbyId=",
			param.m_ulSteamIDLobby
		}));
		try
		{
			if (param.m_eResult == EResult.k_EResultOK)
			{
				CSteamID csteamID = new CSteamID(param.m_ulSteamIDLobby);
				if (csteamID.IsValid())
				{
					CoopLobbyInfo coopLobbyInfo = new CoopLobbyInfo(param.m_ulSteamIDLobby);
					coopLobbyInfo.IsOwner = true;
					coopLobbyInfo.Name = CoopLobbyManager.createValues.Name;
					coopLobbyInfo.MemberLimit = CoopLobbyManager.createValues.MemberLimit;
					CoopLobbyManager.LobbyMatchList_Result = new List<CoopLobbyInfo>();
					CoopLobbyManager.LobbyMatchList_Result.Add(coopLobbyInfo);
					CoopLobby.SetActive(coopLobbyInfo);
					if (GameSetup.IsSavedGame)
					{
						SaveSlotUtils.LoadHostGameGUID();
					}
					if (CoopLobbyManager.createCallback != null)
					{
						try
						{
							CoopLobbyManager.createCallback();
						}
						catch (Exception ex)
						{
						}
					}
					goto IL_106;
				}
			}
			if (CoopLobbyManager.createFailCallback != null)
			{
				try
				{
					CoopLobbyManager.createFailCallback();
				}
				catch (Exception ex2)
				{
				}
			}
			IL_106:;
		}
		finally
		{
			CoopLobbyManager.createValues = null;
			CoopLobbyManager.createCallback = null;
			CoopLobbyManager.createFailCallback = null;
		}
	}

	
	private static void LobbyDataUpdate(LobbyDataUpdate_t param)
	{
		if (param.m_bSuccess == 1)
		{
			CoopLobbyInfo coopLobbyInfo = CoopLobbyManager.FindLobby(param.m_ulSteamIDLobby);
			if (coopLobbyInfo != null)
			{
				coopLobbyInfo.UpdateData();
			}
		}
	}

	
	private static void LobbyMatchList(LobbyMatchList_t param)
	{
		int num = 0;
		while ((long)num < (long)((ulong)param.m_nLobbiesMatching))
		{
			CoopLobbyManager.GetLobbyInfo(SteamMatchmaking.GetLobbyByIndex(num));
			num++;
		}
	}

	
	private static CoopLobbyInfo GetLobbyInfo(CSteamID id)
	{
		CoopLobbyInfo coopLobbyInfo = CoopLobbyManager.FindLobby(id);
		if (coopLobbyInfo == null)
		{
			coopLobbyInfo = new CoopLobbyInfo(id);
			coopLobbyInfo.UpdateData();
			CoopLobbyManager.LobbyMatchList_Result.Add(coopLobbyInfo);
		}
		return coopLobbyInfo;
	}

	
	public static void Initialize()
	{
		if (CoopLobbyManager.runInit)
		{
			CoopLobbyManager.runInit = false;
			CoopLobbyManager.LobbyMatchList_Callback = Callback<LobbyMatchList_t>.Create(new Callback<LobbyMatchList_t>.DispatchDelegate(CoopLobbyManager.LobbyMatchList));
			CoopLobbyManager.LobbyDataUpdate_Callback = Callback<LobbyDataUpdate_t>.Create(new Callback<LobbyDataUpdate_t>.DispatchDelegate(CoopLobbyManager.LobbyDataUpdate));
			CoopLobbyManager.LobbyCreated_Callback = Callback<LobbyCreated_t>.Create(new Callback<LobbyCreated_t>.DispatchDelegate(CoopLobbyManager.LobbyCreated));
			CoopLobbyManager.LobbyEnter_Callback = Callback<LobbyEnter_t>.Create(new Callback<LobbyEnter_t>.DispatchDelegate(CoopLobbyManager.LobbyEnter));
		}
	}

	
	public static void Shutdown()
	{
		CoopLobbyManager.LobbyMatchList_Result = new List<CoopLobbyInfo>();
		CoopLobbyManager.LobbyEnter_Callback = null;
		CoopLobbyManager.LobbyCreated_Callback = null;
		CoopLobbyManager.LobbyMatchList_Callback = null;
		CoopLobbyManager.LobbyDataUpdate_Callback = null;
		CoopLobbyManager.createValues = null;
		CoopLobbyManager.createCallback = null;
		CoopLobbyManager.runInit = true;
	}

	
	public const string FULL_KEY = "FULL";

	
	private static bool runInit = true;

	
	private static Action enterCallback;

	
	private static Action<string> enterFailCallback;

	
	private static Action createCallback;

	
	private static Action createFailCallback;

	
	private static CoopLobbyInfo createValues;

	
	private static Callback<LobbyMatchList_t> LobbyMatchList_Callback;

	
	private static Callback<LobbyDataUpdate_t> LobbyDataUpdate_Callback;

	
	private static Callback<LobbyCreated_t> LobbyCreated_Callback;

	
	private static Callback<LobbyEnter_t> LobbyEnter_Callback;

	
	private static List<CoopLobbyInfo> LobbyMatchList_Result = new List<CoopLobbyInfo>();
}
