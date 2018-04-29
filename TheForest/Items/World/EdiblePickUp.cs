﻿using System;
using System.Collections.Generic;
using TheForest.Items.Utils;
using TheForest.Tools;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.World
{
	
	[AddComponentMenu("Items/World/Edible PickUp")]
	public class EdiblePickUp : PickUp
	{
		
		protected override bool MainEffect()
		{
			if (!this.ForcePickup && this._playerStatConditionList.IsValid(LocalPlayer.Stats))
			{
				EventRegistry.Player.Publish(TfEvent.UsedItem, this._itemId);
				Item item = ItemDatabase.ItemById(this._itemId);
				ItemUtils.ApplyEffectsToStats(item._usedStatEffect, true, 1);
				if (item._usedSFX != Item.SFXCommands.None)
				{
					LocalPlayer.GameObject.SendMessage(item._usedSFX.ToString());
				}
				return true;
			}
			return base.MainEffect();
		}

		
		
		
		public bool ForcePickup { get; set; }

		
		public EdiblePickUp.PlayerStatConditionList _playerStatConditionList;

		
		[Serializable]
		public class PlayerStatCondition
		{
			
			public bool IsValid(PlayerStats playerStats)
			{
				float num;
				switch (this._stat)
				{
				case EdiblePickUp.PlayerStatCondition.Stats.Stamina:
					num = playerStats.Stamina;
					break;
				case EdiblePickUp.PlayerStatCondition.Stats.Health:
					num = playerStats.Health;
					break;
				case EdiblePickUp.PlayerStatCondition.Stats.Energy:
					num = playerStats.Energy;
					break;
				case EdiblePickUp.PlayerStatCondition.Stats.Armor:
					num = (float)playerStats.Armor;
					break;
				case EdiblePickUp.PlayerStatCondition.Stats.Fullness:
					num = playerStats.Fullness;
					break;
				case EdiblePickUp.PlayerStatCondition.Stats.BatteryCharge:
					num = playerStats.BatteryCharge;
					break;
				case EdiblePickUp.PlayerStatCondition.Stats.RebreatherAir:
					num = playerStats.AirBreathing.CurrentRebreatherAir;
					break;
				case EdiblePickUp.PlayerStatCondition.Stats.HealthTarget:
					num = playerStats.HealthTarget;
					break;
				default:
					return false;
				}
				switch (this._op)
				{
				case EdiblePickUp.PlayerStatCondition.Operators.Superior:
					return num > this._value;
				case EdiblePickUp.PlayerStatCondition.Operators.SuperiorOrEqual:
					return num >= this._value;
				case EdiblePickUp.PlayerStatCondition.Operators.Equal:
					return Mathf.Approximately(num, this._value);
				case EdiblePickUp.PlayerStatCondition.Operators.Inferior:
					return num <= this._value;
				case EdiblePickUp.PlayerStatCondition.Operators.InferiorOrEqual:
					return num < this._value;
				case EdiblePickUp.PlayerStatCondition.Operators.Different:
					return !Mathf.Approximately(num, this._value);
				default:
					return false;
				}
			}

			
			public EdiblePickUp.PlayerStatCondition.Stats _stat;

			
			public EdiblePickUp.PlayerStatCondition.Operators _op;

			
			public float _value;

			
			public enum Operators
			{
				
				Superior,
				
				SuperiorOrEqual,
				
				Equal,
				
				Inferior,
				
				InferiorOrEqual,
				
				Different
			}

			
			public enum Stats
			{
				
				Stamina,
				
				Health,
				
				Energy,
				
				Armor,
				
				Fullness,
				
				BatteryCharge,
				
				RebreatherAir,
				
				HealthTarget
			}
		}

		
		[Serializable]
		public class PlayerStatConditionList
		{
			
			public bool IsValid(PlayerStats playerStats)
			{
				if (this._conditions != null && this._conditions.Count > 0)
				{
					foreach (EdiblePickUp.PlayerStatCondition playerStatCondition in this._conditions)
					{
						bool flag = playerStatCondition.IsValid(playerStats);
						EdiblePickUp.PlayerStatConditionList.ValidationTypes validationType = this._validationType;
						if (validationType != EdiblePickUp.PlayerStatConditionList.ValidationTypes.AnyTrue)
						{
							if (validationType == EdiblePickUp.PlayerStatConditionList.ValidationTypes.AllTrue)
							{
								if (!flag)
								{
									return false;
								}
							}
						}
						else if (flag)
						{
							return true;
						}
					}
					return this._validationType == EdiblePickUp.PlayerStatConditionList.ValidationTypes.AllTrue;
				}
				return true;
			}

			
			public EdiblePickUp.PlayerStatConditionList.ValidationTypes _validationType;

			
			[NameFromProperty("_stat", 0)]
			public List<EdiblePickUp.PlayerStatCondition> _conditions;

			
			public enum ValidationTypes
			{
				
				AnyTrue,
				
				AllTrue
			}
		}
	}
}
