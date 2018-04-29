using System;

namespace TheForest.Utils.Settings
{
	
	public class AnimalSettings
	{
		
		public AnimalSettings()
		{
			this.Refresh();
		}

		
		public void Refresh()
		{
			if (GameSetup.IsHardMode)
			{
				this.FishRespawnDelay = 5f;
				this.FishMaxAmountRatio = 0.66f;
				this.RespawnDelayRatio = 1f;
				this.MaxAmountRatio = 0.4f;
			}
			else if (GameSetup.IsHardSurvivalMode)
			{
				this.FishRespawnDelay = 10f;
				this.FishMaxAmountRatio = 0.2f;
				this.RespawnDelayRatio = 5f;
				this.MaxAmountRatio = 0.1f;
			}
			else
			{
				this.FishRespawnDelay = 2f;
				this.FishMaxAmountRatio = 1f;
				this.RespawnDelayRatio = 1f;
				this.MaxAmountRatio = 0.6f;
			}
		}

		
		public float FishRespawnDelay;

		
		public float FishMaxAmountRatio;

		
		public float RespawnDelayRatio;

		
		public float MaxAmountRatio;
	}
}
