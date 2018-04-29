using System;

namespace TheForest.Player
{
	
	internal interface IAccountInfo
	{
		
		bool Init();

		
		void Load();

		
		bool ResetAchievement(string name);

		
		bool ResetAllAchievements();

		
		bool SetStoryCompleted();

		
		bool IsAchievementUnlocked(AchievementData ach);

		
		bool UnlockAchievement(AchievementData ach);

		
		int GetIntStat(AchievementData ach);

		
		bool SetIntStat(AchievementData ach, int value);

		
		
		bool InitDone { get; }

		
		
		bool StoryCompleted { get; }
	}
}
