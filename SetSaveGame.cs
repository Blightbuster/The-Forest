using System;
using System.Collections;
using Steamworks;
using TheForest.Commons.Enums;
using TheForest.Player;
using UnityEngine;


public class SetSaveGame : MonoBehaviour
{
	
	private IEnumerator Start()
	{
		if (!CoopPeerStarter.DedicatedHost)
		{
			while (!SteamManager.Initialized || string.IsNullOrEmpty(SteamUser.GetSteamID().ToString()))
			{
				yield return null;
			}
		}
		if (SteamManager.Initialized && AccountInfo.Init())
		{
			while (!AccountInfo.InitDone)
			{
				yield return null;
			}
			AccountInfo.Load();
		}
		yield return null;
		int playerVersion = PlayerPrefs.GetInt("CurrentVersion", this.Current);
		try
		{
			if (playerVersion >= this.MinVersion)
			{
				if (playerVersion < 32)
				{
					PlayerPrefsFile.ConvertToSlotSystem("__RESUME__", PlayerModes.SinglePlayer);
					PlayerPrefsFile.ConvertToSlotSystem("__RESUME__", PlayerModes.Multiplayer);
				}
				if (CoopSteamCloud.ShouldUseCloud())
				{
					foreach (object obj in Enum.GetValues(typeof(Slots)))
					{
						Slots slot = (Slots)((int)obj);
						PlayerPrefsFile.SyncWithCloud("__RESUME__", PlayerModes.SinglePlayer, slot);
						PlayerPrefsFile.SyncWithCloud("info", PlayerModes.SinglePlayer, slot);
						PlayerPrefsFile.SyncWithCloud("thumb.png", PlayerModes.SinglePlayer, slot);
						PlayerPrefsFile.SyncWithCloud("__RESUME__", PlayerModes.Multiplayer, slot);
						PlayerPrefsFile.SyncWithCloud("info", PlayerModes.Multiplayer, slot);
						PlayerPrefsFile.SyncWithCloud("thumb.png", PlayerModes.Multiplayer, slot);
						PlayerPrefsFile.SyncWithCloud("guid", PlayerModes.Multiplayer, slot);
					}
					Debug.Log("Cloud Sync Done");
				}
				else
				{
					Debug.Log("Skipped cloud sync");
				}
			}
		}
		catch (Exception ex2)
		{
			Exception ex = ex2;
			Debug.LogException(ex);
		}
		finally
		{
			PlayerPrefs.SetInt("CurrentVersion", this.Current);
			PlayerPrefs.Save();
		}
		yield break;
	}

	
	public int Current = 1;

	
	public int MinVersion = 31;
}
