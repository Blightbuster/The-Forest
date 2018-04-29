using System;
using Bolt;
using TheForest.Commons.Enums;
using TheForest.Utils;

namespace TheForest.Player
{
	
	public class GameMode_Creative : EntityBehaviour<IGameModeState>
	{
		
		private void Awake()
		{
			if (BoltNetwork.isClient)
			{
				GameSetup.SetGameType(GameTypes.Creative);
			}
			Cheats.NoSurvival = true;
			Cheats.Creative = true;
			Cheats.GodMode = true;
			Cheats.InfiniteEnergy = true;
		}

		
		private void OnDestroy()
		{
			Cheats.NoSurvival = false;
			Cheats.Creative = false;
			Cheats.GodMode = false;
			Cheats.InfiniteEnergy = false;
		}
	}
}
