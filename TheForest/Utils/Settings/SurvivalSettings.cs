using System;

namespace TheForest.Utils.Settings
{
	
	public class SurvivalSettings
	{
		
		public SurvivalSettings()
		{
			this.Refresh();
		}

		
		public void Refresh()
		{
			if (GameSetup.IsHardMode)
			{
				this.PoisonDamageRatio = 1f;
				this.PolutedWaterDamageRatio = 20f;
				this.PolutedWaterThirstRatio = 0.25f;
				this.FrostSpeedRatio = 3f;
				this.DefrostSpeedRatio = 0.5f;
				this.FrostDamageRatio = 3f;
				this.FireDamageRatio = 2f;
				this.HealthRegenPerSecond = 0.5f;
				this.FullnessMealReplenishmentRatio = 1f;
				this.FullnessLimbReplenishmentRatio = 1f;
				this.EnergyMealReplenishmentRatio = 1f;
				this.EnergyLimbReplenishmentRatio = 1f;
				this.HealthMealReplenishmentRatio = 1f;
				this.HealthLimbReplenishmentRatio = 1f;
				this.StealthRatio = 1f;
				this.ThirstRatio = 1f;
				this.ThirstDamageRatio = 1f;
				this.ItemUsedThirstGainRatio = 1f;
				this.ItemUsedThirstLossRatio = 1f;
				this.LungMaxCapacityRatio = 1f;
				this.InfectionChance = 1f;
				this.InfectionEffectRatio = 1f;
				this.MeatDecayDurationRatio = 1f;
				this.HitFoodDamageRatio = 1;
				this.DriedMeatThirstRatio = 1f;
				this.SandWalkSpeedRatio = 0.85f;
				this.SnowWalkSpeedRatio = 0.85f;
				this.SuitcaseCountRatio = 1f;
				this.GreebleRegrowthMinDelay = 1f;
			}
			else if (GameSetup.IsHardSurvivalMode)
			{
				this.PoisonDamageRatio = 15f;
				this.PolutedWaterDamageRatio = 20f;
				this.PolutedWaterThirstRatio = 0.25f;
				this.FrostSpeedRatio = 5f;
				this.DefrostSpeedRatio = 0.2f;
				this.FrostDamageRatio = 5f;
				this.FireDamageRatio = 5f;
				this.HealthRegenPerSecond = 0.5f;
				this.FullnessMealReplenishmentRatio = 1f;
				this.FullnessLimbReplenishmentRatio = 0.15f;
				this.EnergyMealReplenishmentRatio = 0.35f;
				this.EnergyLimbReplenishmentRatio = 0.05f;
				this.HealthMealReplenishmentRatio = 0.35f;
				this.HealthLimbReplenishmentRatio = 0.35f;
				this.StealthRatio = 0.5f;
				this.ThirstRatio = 1f;
				this.ThirstDamageRatio = 2.5f;
				this.ItemUsedThirstGainRatio = 1f;
				this.ItemUsedThirstLossRatio = 0.5f;
				this.LungMaxCapacityRatio = 1f;
				this.InfectionChance = 2f;
				this.InfectionEffectRatio = 0.888f;
				this.MeatDecayDurationRatio = 0.75f;
				this.HitFoodDamageRatio = 5;
				this.DriedMeatThirstRatio = 4f;
				this.SandWalkSpeedRatio = 0.85f;
				this.SnowWalkSpeedRatio = 0.85f;
				this.SuitcaseCountRatio = 0.25f;
				this.GreebleRegrowthMinDelay = 4f;
			}
			else
			{
				this.PoisonDamageRatio = 1f;
				this.PolutedWaterDamageRatio = 1f;
				this.PolutedWaterThirstRatio = 0.5f;
				this.FrostSpeedRatio = 1f;
				this.DefrostSpeedRatio = 1f;
				this.FrostDamageRatio = 1f;
				this.FireDamageRatio = 1f;
				this.HealthRegenPerSecond = 2f;
				this.FullnessMealReplenishmentRatio = 1f;
				this.FullnessLimbReplenishmentRatio = 1f;
				this.EnergyMealReplenishmentRatio = 1f;
				this.EnergyLimbReplenishmentRatio = 1f;
				this.HealthMealReplenishmentRatio = 1f;
				this.HealthLimbReplenishmentRatio = 1f;
				this.StealthRatio = 1f;
				this.ThirstRatio = 1f;
				this.ThirstDamageRatio = 1f;
				this.ItemUsedThirstGainRatio = 1f;
				this.ItemUsedThirstLossRatio = 1f;
				this.LungMaxCapacityRatio = 1f;
				this.InfectionChance = 1f;
				this.InfectionEffectRatio = 1f;
				this.MeatDecayDurationRatio = 1f;
				this.HitFoodDamageRatio = 1;
				this.DriedMeatThirstRatio = 1f;
				this.SandWalkSpeedRatio = 0.85f;
				this.SnowWalkSpeedRatio = 0.85f;
				this.SuitcaseCountRatio = 1f;
				this.GreebleRegrowthMinDelay = 1f;
			}
		}

		
		public float PoisonDamageRatio;

		
		public float PolutedWaterDamageRatio;

		
		public float PolutedWaterThirstRatio;

		
		public float FrostSpeedRatio;

		
		public float DefrostSpeedRatio;

		
		public float FrostDamageRatio;

		
		public float FireDamageRatio;

		
		public float HealthRegenPerSecond;

		
		public float FullnessMealReplenishmentRatio;

		
		public float FullnessLimbReplenishmentRatio;

		
		public float EnergyMealReplenishmentRatio;

		
		public float EnergyLimbReplenishmentRatio;

		
		public float HealthMealReplenishmentRatio;

		
		public float HealthLimbReplenishmentRatio;

		
		public float StealthRatio;

		
		public float ThirstRatio;

		
		public float ThirstDamageRatio;

		
		public float ItemUsedThirstGainRatio;

		
		public float ItemUsedThirstLossRatio;

		
		public float LungMaxCapacityRatio;

		
		public float InfectionChance;

		
		public float InfectionEffectRatio;

		
		public float MeatDecayDurationRatio;

		
		public int HitFoodDamageRatio;

		
		public float DriedMeatThirstRatio;

		
		public float SandWalkSpeedRatio;

		
		public float SnowWalkSpeedRatio;

		
		public float SuitcaseCountRatio;

		
		public float GreebleRegrowthMinDelay;
	}
}
