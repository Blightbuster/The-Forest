using System;
using TheForest.Tools;

namespace TheForest.Utils.Settings
{
	
	public static class GameSettings
	{
		
		public static void Init()
		{
			if (!GameSettings.InitDone)
			{
				GameSettings.InitDone = true;
				GameSettings.Survival = new SurvivalSettings();
				GameSettings.Animals = new AnimalSettings();
				GameSettings.Ai = new AiSettings();
			}
			EventRegistry.Game.Subscribe(TfEvent.DifficultySet, new EventRegistry.SubscriberCallback(GameSettings.OnDifficultySet));
		}

		
		private static void OnDifficultySet(object param)
		{
			GameSettings.Survival.Refresh();
			GameSettings.Animals.Refresh();
			GameSettings.Ai.Refresh();
		}

		
		private static bool InitDone;

		
		public static SurvivalSettings Survival;

		
		public static AnimalSettings Animals;

		
		public static AiSettings Ai;
	}
}
