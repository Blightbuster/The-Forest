using System;
using System.Runtime.CompilerServices;
using Steamworks;
using TheForest.Tools;
using UnityEngine;

namespace TheForest.Player
{
	
	internal class SteamAccountInfo : IAccountInfo
	{
		
		public bool Init()
		{
			Debug.Log("# Init Steam account info, polling for user stats and achievements");
			bool result;
			try
			{
				EventRegistry endgame = EventRegistry.Endgame;
				object completed = TfEvent.Endgame.Completed;
				if (SteamAccountInfo.<>f__mg$cache0 == null)
				{
					SteamAccountInfo.<>f__mg$cache0 = new EventRegistry.SubscriberCallback(AccountInfo.SetStoryCompleted);
				}
				endgame.Subscribe(completed, SteamAccountInfo.<>f__mg$cache0);
				this._statReceivedCallback = Callback<UserStatsReceived_t>.Create(new Callback<UserStatsReceived_t>.DispatchDelegate(this.OnReceivedStats));
				result = SteamUserStats.RequestCurrentStats();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				result = false;
			}
			return result;
		}

		
		private void OnReceivedStats(UserStatsReceived_t info)
		{
			this._initDone = true;
			this._statReceivedCallback = null;
		}

		
		public void Load()
		{
			Debug.Log("# Init steam account info done, loading data...");
			int num;
			if (SteamUserStats.GetStat("StoryCompleted", out num))
			{
				this._storyCompleted = (num == 1);
			}
			if (!this._storyCompleted)
			{
				SteamUserStats.GetAchievement("ACH_SURVIVE_THE_FOREST", out this._storyCompleted);
			}
			if (!this._storyCompleted && PlayerPrefs.GetInt("StoryCompleted", 0) == 1)
			{
				this._storyCompleted = true;
				this.SetStoryCompleted();
			}
		}

		
		public bool ResetAchievement(string achName)
		{
			return SteamUserStats.ClearAchievement(achName);
		}

		
		public bool ResetAllAchievements()
		{
			return SteamUserStats.ResetAllStats(true);
		}

		
		public bool SetStoryCompleted()
		{
			this._storyCompleted = true;
			PlayerPrefs.SetInt("StoryCompleted", 1);
			PlayerPrefs.Save();
			if (SteamUserStats.SetStat("StoryCompleted", 1))
			{
				SteamUserStats.StoreStats();
				return true;
			}
			return false;
		}

		
		
		public bool InitDone
		{
			get
			{
				return this._initDone;
			}
		}

		
		
		public bool StoryCompleted
		{
			get
			{
				return this._storyCompleted;
			}
		}

		
		private bool SetStatProgress(string statKey, uint current, uint max)
		{
			return SteamUserStats.IndicateAchievementProgress(statKey, current, max);
		}

		
		public bool IsAchievementUnlocked(AchievementData ach)
		{
			bool flag;
			if (ach.AchHandle != null)
			{
				return SteamUserStats.GetAchievement(ach.AchHandle, out flag) && flag;
			}
			return SteamUserStats.GetAchievement(ach.Key, out flag) && flag;
		}

		
		public bool UnlockAchievement(AchievementData ach)
		{
			if (ach.AchHandle != null)
			{
				if (SteamUserStats.SetAchievement(ach.AchHandle) || SteamUserStats.SetAchievement(ach.Key))
				{
					return SteamUserStats.StoreStats();
				}
			}
			else if (SteamUserStats.SetAchievement(ach.Key))
			{
				return SteamUserStats.StoreStats();
			}
			return false;
		}

		
		public int GetIntStat(AchievementData ach)
		{
			int result;
			if (ach.StatHandle != null)
			{
				SteamUserStats.GetStat(ach.StatHandle, out result);
			}
			else
			{
				SteamUserStats.GetStat(ach.StatKey, out result);
			}
			return result;
		}

		
		public bool SetIntStat(AchievementData ach, int value)
		{
			if (value > ach.CurrentValue)
			{
				if (ach.UnlockValues != null && ach.UnlockValues.Count > 0 && value > ach.UnlockValues[ach.UnlockValues.Count - 1])
				{
					value = ach.UnlockValues[ach.UnlockValues.Count - 1];
				}
				if (ach.StatHandle != null)
				{
					if (!SteamUserStats.SetStat(ach.StatHandle, value))
					{
						return false;
					}
				}
				else if (!SteamUserStats.SetStat(ach.StatKey, value))
				{
					return false;
				}
				ach.CurrentValue = value;
				if (ach.UnlockValues != null && ach.UnlockValues.Count > 0 && ach.CurrentValue >= ach.UnlockValues[0] && SteamUserStats.StoreStats())
				{
					ach.UnlockValues.RemoveAt(0);
				}
				return true;
			}
			return false;
		}

		
		private bool _initDone;

		
		private bool _storyCompleted;

		
		private Callback<UserStatsReceived_t> _statReceivedCallback;

		
		[CompilerGenerated]
		private static EventRegistry.SubscriberCallback <>f__mg$cache0;
	}
}
