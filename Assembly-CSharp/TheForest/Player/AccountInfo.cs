using System;
using UnityEngine;

namespace TheForest.Player
{
	public class AccountInfo : MonoBehaviour
	{
		private static IAccountInfo Instance
		{
			get
			{
				if (AccountInfo._instance == null)
				{
					AccountInfo._instance = new SteamAccountInfo();
				}
				return AccountInfo._instance;
			}
		}

		public static bool Init()
		{
			return AccountInfo.Instance != null && AccountInfo.Instance.Init();
		}

		public static void Load()
		{
			if (AccountInfo.Instance != null)
			{
				AccountInfo.Instance.Load();
				Achievements.Reset();
			}
		}

		public static bool InitDone
		{
			get
			{
				return AccountInfo.Instance != null && AccountInfo.Instance.InitDone;
			}
		}

		public static bool StoryCompleted
		{
			get
			{
				return AccountInfo.Instance != null && AccountInfo.Instance.StoryCompleted;
			}
		}

		public static void SetStoryCompleted(object o)
		{
			if (AccountInfo.Instance != null)
			{
				AccountInfo.Instance.SetStoryCompleted();
			}
		}

		public static bool ResetAchievement(string name)
		{
			return AccountInfo.Instance != null && AccountInfo.Instance.ResetAchievement(name);
		}

		public static bool ResetAllAchievements()
		{
			return AccountInfo.Instance != null && AccountInfo.Instance.ResetAllAchievements();
		}

		public static bool IsAchievementUnlocked(AchievementData ach)
		{
			return AccountInfo.Instance != null && AccountInfo.Instance.IsAchievementUnlocked(ach);
		}

		public static bool UnlockAchievement(AchievementData ach)
		{
			if (AccountInfo.Instance != null)
			{
				bool flag = AccountInfo.Instance.UnlockAchievement(ach);
				if (!flag)
				{
					Debug.LogError(string.Concat(new object[]
					{
						"# Achievement '",
						ach.Key,
						"' unlock failed:, handle=",
						ach.AchHandle
					}));
				}
				return flag;
			}
			return false;
		}

		public static int GetIntStat(AchievementData ach)
		{
			return (AccountInfo.Instance == null) ? -1 : AccountInfo.Instance.GetIntStat(ach);
		}

		public static bool SetIntStat(AchievementData ach, int value)
		{
			return AccountInfo.Instance != null && AccountInfo.Instance.SetIntStat(ach, value);
		}

		private static IAccountInfo _instance;
	}
}
