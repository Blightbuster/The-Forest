using System;
using System.Reflection;
using Bolt;
using TheForest.Items.Inventory;
using TheForest.Utils;
using TheForest.Utils.Settings;
using UnityEngine;

namespace TheForest.Items.Utils
{
	
	public static class ItemUtils
	{
		
		public static void FixItemPosition(GameObject itemObject, int itemId)
		{
			Item item = ItemDatabase.ItemById(itemId);
			Transform transform = (!(item._pickupPrefab != null)) ? ((!(item._pickupPrefabMP != null)) ? null : item._pickupPrefabMP) : item._pickupPrefab;
			if (transform)
			{
				FixRackPosition component = transform.GetComponent<FixRackPosition>();
				if (component)
				{
					itemObject.transform.localPosition += component.positionOffset;
					itemObject.transform.localRotation *= Quaternion.Euler(component.rotationOffset);
				}
			}
		}

		
		public static int ItemIdByName(string itemName)
		{
			return ItemDatabase.ItemIdByName(itemName);
		}

		
		public static GameObject SpawnItem(string itemName, Vector3 position, Quaternion rotation, bool avoidImpacts = false)
		{
			int itemId = ItemDatabase.ItemIdByName(itemName);
			return ItemUtils.SpawnItem(itemId, position, rotation, avoidImpacts);
		}

		
		public static GameObject SpawnItem(int itemId, Vector3 position, Quaternion rotation, bool avoidImpacts = false)
		{
			Item item = ItemDatabase.ItemById(itemId);
			GameObject gameObject = null;
			if (item != null)
			{
				if (BoltNetwork.isRunning)
				{
					if (item._pickupPrefabMP != null)
					{
						if (BoltNetwork.isServer)
						{
							Transform pickupPrefabMP = item._pickupPrefabMP;
							BoltEntity boltEntity = BoltNetwork.Instantiate(pickupPrefabMP.gameObject, position, rotation);
							gameObject = boltEntity.gameObject;
						}
						else
						{
							DropItem dropItem = DropItem.Create(GlobalTargets.OnlyServer);
							dropItem.PrefabId = item._pickupPrefabMP.gameObject.GetComponent<BoltEntity>().prefabId;
							dropItem.Position = position;
							dropItem.Rotation = rotation;
							dropItem.PreSpawned = null;
							dropItem.AvoidImpacts = avoidImpacts;
							dropItem.Send();
						}
					}
					else if (item._pickupPrefab != null)
					{
						Debug.LogError(string.Concat(new object[]
						{
							"Item #",
							item._id,
							" name ",
							item._name,
							" no MP version at itemdatabase."
						}));
					}
					else
					{
						Debug.LogError(string.Concat(new object[]
						{
							"Item #",
							item._id,
							" name ",
							item._name,
							" not found at itemdatabase."
						}));
					}
				}
				else
				{
					Transform transform = (!item._pickupPrefab) ? item._pickupPrefabMP : item._pickupPrefab;
					if (transform)
					{
						gameObject = UnityEngine.Object.Instantiate<GameObject>(transform.gameObject, position, rotation);
					}
					else
					{
						Debug.LogError(string.Concat(new object[]
						{
							"Item #",
							item._id,
							" name ",
							item._name,
							" not found at itemdatabase."
						}));
					}
				}
				if (gameObject != null && avoidImpacts && !gameObject.GetComponent<flyingObjectFixerFrame>())
				{
					flyingObjectFixerFrame flyingObjectFixerFrame = gameObject.AddComponent<flyingObjectFixerFrame>();
				}
			}
			return gameObject;
		}

		
		public static bool ApplyEffectsToStats(StatEffect effect, bool forward)
		{
			Type typeFromHandle = typeof(PlayerStats);
			PlayerStats stats = LocalPlayer.Stats;
			PlayerInventory inventory = LocalPlayer.Inventory;
			int num = (!forward) ? -1 : 1;
			StatEffect.Types type = effect._type;
			switch (type)
			{
			case StatEffect.Types.BatteryCharge:
				stats.BatteryCharge = Mathf.Clamp(stats.BatteryCharge + effect._amount * (float)num, 0f, 100f);
				break;
			case StatEffect.Types.VisibleLizardSkinArmor:
				stats.AddArmorVisible(PlayerStats.ArmorTypes.LizardSkin);
				inventory.PendingSendMessage = "CheckArmor";
				break;
			default:
				switch (type)
				{
				case StatEffect.Types.Stamina:
					stats.Stamina += effect._amount * (float)num;
					break;
				case StatEffect.Types.Health:
					stats.HealthChange(effect._amount * (float)num * LocalPlayer.Stats.FoodPoisoning.EffectRatio);
					LocalPlayer.Stats.CheckStats();
					break;
				case StatEffect.Types.Energy:
					stats.Energy += effect._amount * (float)num * LocalPlayer.Stats.FoodPoisoning.EffectRatio;
					break;
				case StatEffect.Types.Armor:
					stats.Armor = Mathf.Clamp(stats.Armor + (int)effect._amount * num, 0, 1000);
					break;
				default:
					if (type != StatEffect.Types.MaxAmountBonus)
					{
						FieldInfo field = typeFromHandle.GetField(effect._type.ToString(), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						if (field.FieldType == typeof(float))
						{
							field.SetValue(stats, (float)field.GetValue(stats) + effect._amount * (float)num);
						}
						else if (field.FieldType == typeof(int))
						{
							field.SetValue(stats, (int)((float)((int)field.GetValue(stats)) + effect._amount * (float)num));
						}
						LocalPlayer.Stats.CheckStats();
					}
					else
					{
						LocalPlayer.Inventory.AddMaxAmountBonus(effect._itemId, (int)(effect._amount * (float)num));
					}
					break;
				case StatEffect.Types.Fullness:
					stats.Fullness += effect._amount * (float)num * LocalPlayer.Stats.FoodPoisoning.EffectRatio;
					break;
				}
				break;
			case StatEffect.Types.Method_PoisonMe:
				if (effect._amount == 0f)
				{
					stats.PoisonMe();
				}
				else
				{
					stats.Invoke("PoisonMe", effect._amount);
				}
				break;
			case StatEffect.Types.Method_HitFood:
				if (effect._amount == 0f)
				{
					stats.HitFood();
				}
				else
				{
					stats.Invoke("HitFood", effect._amount);
				}
				break;
			case StatEffect.Types.VisibleDeerSkinArmor:
				stats.AddArmorVisible(PlayerStats.ArmorTypes.DeerSkin);
				inventory.PendingSendMessage = "CheckArmor";
				break;
			case StatEffect.Types.ColdArmor:
				stats.ColdArmor = Mathf.Clamp(stats.ColdArmor + effect._amount * (float)num, 0f, 2f);
				break;
			case StatEffect.Types.VisibleStealthArmor:
				stats.AddArmorVisible(PlayerStats.ArmorTypes.Leaves);
				inventory.PendingSendMessage = "CheckArmor";
				break;
			case StatEffect.Types.Stealth:
				LocalPlayer.Stats.Stealth += effect._amount * (float)num;
				break;
			case StatEffect.Types.Thirst:
				if (num > 0)
				{
					stats.Thirst += effect._amount * (float)num * LocalPlayer.Stats.FoodPoisoning.EffectRatio * GameSettings.Survival.ItemUsedThirstGainRatio;
				}
				else
				{
					stats.Thirst += effect._amount * (float)num * LocalPlayer.Stats.FoodPoisoning.EffectRatio * GameSettings.Survival.ItemUsedThirstLossRatio;
				}
				break;
			case StatEffect.Types.AirRecharge:
				if (stats.AirBreathing.CurrentRebreatherAir >= stats.AirBreathing.MaxRebreatherAirCapacity)
				{
					return false;
				}
				stats.AirBreathing.CurrentRebreatherAir = stats.AirBreathing.MaxRebreatherAirCapacity;
				break;
			case StatEffect.Types.Method_UseRebreather:
				stats.UseRebreather(forward);
				break;
			case StatEffect.Types.CureFoodPoisoning:
				LocalPlayer.Stats.FoodPoisoning.Cure();
				break;
			case StatEffect.Types.CureBloodInfection:
				LocalPlayer.Stats.BloodInfection.Cure();
				break;
			case StatEffect.Types.OvereatingPoints:
				Debug.LogError("Use of deprecated option please report");
				break;
			case StatEffect.Types.UndereatingPoints:
				Debug.LogError("Use of deprecated option please report");
				break;
			case StatEffect.Types.SnowFlotation:
				LocalPlayer.FpCharacter.snowFlotation = (num > 0);
				break;
			case StatEffect.Types.SoundRangeDampFactor:
				if (LocalPlayer.Stats.SoundRangeDampFactor < 0.69f)
				{
					LocalPlayer.Stats.SoundRangeDampFactor = 0.7f;
				}
				LocalPlayer.Stats.SoundRangeDampFactor += effect._amount * (float)num;
				break;
			case StatEffect.Types.VisibleBoneArmor:
				stats.AddArmorVisible(PlayerStats.ArmorTypes.Bone);
				inventory.PendingSendMessage = "CheckArmor";
				break;
			case StatEffect.Types.ResetFrost:
				LocalPlayer.Stats.ResetFrost();
				break;
			case StatEffect.Types.FuelRecharge:
				if (stats.Fuel.CurrentFuel >= stats.Fuel.MaxFuelCapacity)
				{
					return false;
				}
				stats.Fuel.CurrentFuel = stats.Fuel.MaxFuelCapacity;
				break;
			case StatEffect.Types.EatenCalories:
				LocalPlayer.Stats.Calories.OnAteFood(Mathf.RoundToInt(effect._amount * (float)num));
				break;
			case StatEffect.Types.VisibleWarmsuit:
				if (forward)
				{
					stats.AddArmorVisible(PlayerStats.ArmorTypes.Warmsuit);
				}
				else
				{
					stats.AddArmorVisible(PlayerStats.ArmorTypes.None);
				}
				inventory.PendingSendMessage = "CheckArmor";
				break;
			case StatEffect.Types.hairSprayFuelRecharge:
				if (stats.hairSprayFuel.CurrentFuel >= stats.hairSprayFuel.MaxFuelCapacity)
				{
					return false;
				}
				stats.hairSprayFuel.CurrentFuel = stats.hairSprayFuel.MaxFuelCapacity;
				break;
			case StatEffect.Types.VisibleCreepyArmor:
				stats.AddArmorVisible(PlayerStats.ArmorTypes.Creepy);
				inventory.PendingSendMessage = "CheckArmor";
				break;
			}
			return true;
		}

		
		public static bool ApplyEffectsToStats(StatEffect[] effects, bool forward, int times = 1)
		{
			bool result = false;
			if (effects != null && effects.Length > 0)
			{
				for (int i = 0; i < times; i++)
				{
					for (int j = 0; j < effects.Length; j++)
					{
						if (ItemUtils.ApplyEffectsToStats(effects[j], forward))
						{
							result = true;
						}
					}
				}
			}
			return result;
		}
	}
}
