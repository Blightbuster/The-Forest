using System;
using System.Runtime.CompilerServices;
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
			EventRegistry game = EventRegistry.Game;
			object difficultySet = TfEvent.DifficultySet;
			if (GameSettings.<>f__mg$cache0 == null)
			{
				GameSettings.<>f__mg$cache0 = new EventRegistry.SubscriberCallback(GameSettings.OnDifficultySet);
			}
			game.Subscribe(difficultySet, GameSettings.<>f__mg$cache0);
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

		[CompilerGenerated]
		private static EventRegistry.SubscriberCallback <>f__mg$cache0;
	}
}
