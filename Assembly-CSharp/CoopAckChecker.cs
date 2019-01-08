using System;
using Bolt;
using TheForest.Utils;
using UnityEngine;

public class CoopAckChecker : GlobalEventListener
{
	private void Awake()
	{
		if (CoopAckChecker.Instance)
		{
			UnityEngine.Object.Destroy(base.gameObject, 0.01f);
		}
		else
		{
			CoopAckChecker.ACKED = false;
			CoopAckChecker.Instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
	}

	public override void Connected(BoltConnection connection)
	{
		if (BoltNetwork.isServer)
		{
			if (CoopKick.IsBanned(connection.RemoteEndPoint.SteamId))
			{
				Debug.Log("TELL CLIENT HE WAS BANNED");
				connection.Disconnect(new CoopKickToken
				{
					Banned = true,
					KickMessage = "HOST_BANNED_YOU"
				});
			}
			else
			{
				ClientACK.Create(connection).Send();
				CoopTreeGrid.TodoPlayerSweeps.Add(connection);
			}
		}
		if (!CoopPeerStarter.DedicatedHost && Scene.HudGui && Scene.HudGui.MpPlayerList && Scene.HudGui.MpPlayerList.gameObject && Scene.HudGui.MpPlayerList.gameObject.activeInHierarchy)
		{
			Scene.HudGui.MpPlayerList.Refresh();
		}
	}

	public override void OnEvent(ClientACK evnt)
	{
		UnityEngine.Object.FindObjectOfType<CoopSteamClientStarter>().CancelInvoke("OnDisconnected");
		Debug.Log("ACKED, Waiting to receive plane position");
		CoopAckChecker.ACKED = true;
	}

	public static bool ACKED;

	public static CoopAckChecker Instance;
}
