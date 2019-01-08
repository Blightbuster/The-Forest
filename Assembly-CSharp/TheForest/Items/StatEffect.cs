﻿using System;

namespace TheForest.Items
{
	[Serializable]
	public class StatEffect
	{
		public StatEffect.Types _type;

		public float _amount;

		[ItemIdPicker]
		public int _itemId;

		public enum Types
		{
			Stamina,
			Health,
			Energy,
			Armor,
			Hunger,
			InfectionChance,
			BleedChance,
			Ate,
			ColdAmt,
			Fullness,
			Tired,
			EnergyEx,
			BatteryCharge,
			VisibleLizardSkinArmor,
			EnergyTemp,
			Method_PoisonMe,
			Method_HitFood,
			VisibleDeerSkinArmor,
			ColdArmor,
			VisibleStealthArmor,
			Stealth,
			Thirst,
			AirRecharge,
			Method_UseRebreather,
			Flammable,
			CureFoodPoisoning,
			CureBloodInfection,
			OvereatingPoints,
			UndereatingPoints,
			SnowFlotation,
			SoundRangeDampFactor,
			VisibleBoneArmor,
			ResetFrost,
			FuelRecharge,
			EatenCalories,
			VisibleWarmsuit,
			hairSprayFuelRecharge,
			VisibleCreepyArmor,
			MaxAmountBonus = 1000001
		}
	}
}
