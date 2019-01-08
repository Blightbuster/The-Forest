using System;
using Bolt;
using TheForest.Commons.Enums;
using TheForest.Interfaces;
using TheForest.Utils;

namespace TheForest.Player
{
	public class GameMode_Creative : EntityBehaviour<IGameModeState>, IGameMode
	{
		private void Awake()
		{
			if (BoltNetwork.isClient)
			{
				GameSetup.SetGameType(GameTypes.Creative);
				CoopServerInfo.Instance.SetGameMode(this);
			}
			this.RestoreSettings();
		}

		private void OnDestroy()
		{
			Cheats.NoSurvival = false;
			Cheats.Creative = false;
			Cheats.GodMode = false;
			Cheats.InfiniteEnergy = false;
			if (CoopServerInfo.Instance)
			{
				CoopServerInfo.Instance.SetGameMode(null);
			}
		}

		public void RestoreSettings()
		{
			Cheats.NoSurvival = true;
			Cheats.Creative = true;
			Cheats.GodMode = true;
			Cheats.InfiniteEnergy = true;
		}
	}
}
