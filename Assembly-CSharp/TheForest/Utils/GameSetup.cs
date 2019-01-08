using System;
using TheForest.Commons.Enums;
using TheForest.Modding.Bridge.Interfaces;
using TheForest.Tools;
using UnityEngine;

namespace TheForest.Utils
{
	public static class GameSetup
	{
		public static GameTypes Game { get; private set; }

		public static PlayerModes Mode { get; private set; }

		public static DifficultyModes Difficulty { get; private set; }

		public static InitTypes Init { get; private set; }

		public static MpTypes MpType { get; private set; }

		public static Slots Slot { get; private set; }

		public static string SaveUserId { get; private set; }

		public static void SetGameType(GameTypes game)
		{
			GameSetup.Game = game;
			EventRegistry.Game.Publish(TfEvent.GameTypeSet, null);
		}

		public static void SetPlayerMode(PlayerModes mode)
		{
			GameSetup.Mode = mode;
		}

		public static void SetDifficulty(DifficultyModes difficulty)
		{
			GameSetup.Difficulty = difficulty;
			EventRegistry.Game.Publish(TfEvent.DifficultySet, null);
		}

		public static void SetInitType(InitTypes init)
		{
			GameSetup.Init = init;
		}

		public static void SetMpType(MpTypes mpType)
		{
			GameSetup.MpType = mpType;
		}

		public static void SetSlot(Slots slot)
		{
			GameSetup.Slot = (Slots)Mathf.RoundToInt((float)Mathf.Clamp((int)slot, 1, 5));
			GameSetup.SaveUserId = SaveSlotUtils.UserId;
		}

		public static bool IsStandardGame
		{
			get
			{
				return GameSetup.Game == GameTypes.Standard;
			}
		}

		public static bool IsModGame
		{
			get
			{
				return GameSetup.Game == GameTypes.Mod;
			}
		}

		public static bool IsCreativeGame
		{
			get
			{
				return GameSetup.Game == GameTypes.Creative;
			}
		}

		public static bool IsSinglePlayer
		{
			get
			{
				return GameSetup.Mode == PlayerModes.SinglePlayer;
			}
		}

		public static bool IsMultiplayer
		{
			get
			{
				return GameSetup.Mode == PlayerModes.Multiplayer;
			}
		}

		public static bool IsPeacefulMode
		{
			get
			{
				return GameSetup.Difficulty == DifficultyModes.Peaceful;
			}
		}

		public static bool IsNormalMode
		{
			get
			{
				return GameSetup.Difficulty == DifficultyModes.Normal;
			}
		}

		public static bool IsHardMode
		{
			get
			{
				return GameSetup.Difficulty == DifficultyModes.Hard;
			}
		}

		public static bool IsHardSurvivalMode
		{
			get
			{
				return GameSetup.Difficulty == DifficultyModes.HardSurvival;
			}
		}

		public static bool IsNewGame
		{
			get
			{
				return GameSetup.Init == InitTypes.New;
			}
		}

		public static bool IsSavedGame
		{
			get
			{
				return GameSetup.Init == InitTypes.Continue;
			}
		}

		public static bool IsMpServer
		{
			get
			{
				return GameSetup.MpType == MpTypes.Server;
			}
		}

		public static bool IsMpClient
		{
			get
			{
				return GameSetup.MpType == MpTypes.Client;
			}
		}

		public static GameSetup.GameSetupBridge Bridge = new GameSetup.GameSetupBridge();

		public class GameSetupBridge : IGameSetupBridge
		{
			public void SetGameType(GameTypes game)
			{
				GameSetup.Game = game;
				EventRegistry.Game.Publish(TfEvent.GameTypeSet, null);
			}

			public void SetPlayerMode(PlayerModes mode)
			{
				GameSetup.Mode = mode;
			}

			public void SetDifficulty(DifficultyModes difficulty)
			{
				GameSetup.Difficulty = difficulty;
				EventRegistry.Game.Publish(TfEvent.DifficultySet, null);
			}

			public void SetInitType(InitTypes init)
			{
				GameSetup.Init = init;
			}

			public void SetMpType(MpTypes mpType)
			{
				GameSetup.MpType = mpType;
			}

			public void SetSlot(Slots slot)
			{
				GameSetup.Slot = (Slots)Mathf.RoundToInt((float)Mathf.Clamp((int)slot, 1, 5));
			}
		}
	}
}
