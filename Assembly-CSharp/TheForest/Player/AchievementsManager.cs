using System;
using System.Collections.Generic;
using TheForest.Items;
using TheForest.Tools;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Player
{
	[DoNotSerializePublic]
	public class AchievementsManager : MonoBehaviour
	{
		private void Start()
		{
			if (this._lastKillSurvivedDays < 0)
			{
				this._lastKillSurvivedDays = Mathf.CeilToInt(LocalPlayer.Stats.DaySurvived);
			}
			if (Achievements.Crafty != null)
			{
				if (this._doneCrafts == null)
				{
					this._doneCrafts = new List<int>(this._craftablesItemId.Count);
				}
				EventRegistry.Player.Subscribe(TfEvent.CraftedItem, new EventRegistry.SubscriberCallback(this.OnCraftedItem));
			}
			if (Achievements.TrophyHunter != null)
			{
				if (this._doneTrophies == null)
				{
					this._doneTrophies = new List<int>(Prefabs.Instance.TrophyPrefabs.Length);
				}
				EventRegistry.Achievements.Subscribe(TfEvent.Achievements.CreatedTrophy, new EventRegistry.SubscriberCallback(this.OnCreatedTrophy));
			}
			if (Achievements.Gardener != null)
			{
				if (this._donePlantTypes == null)
				{
					this._donePlantTypes = new List<int>(3);
				}
				EventRegistry.Achievements.Subscribe(TfEvent.Achievements.PlantedSeed, new EventRegistry.SubscriberCallback(this.OnPlantedSeed));
			}
			if (Achievements.Naturopath != null)
			{
				EventRegistry.Player.Subscribe(TfEvent.CraftedItem, new EventRegistry.SubscriberCallback(this.CheckCraftedMeds));
			}
			if (Achievements.Splatter != null)
			{
				EventRegistry.Enemy.Subscribe(TfEvent.KilledEnemy, new EventRegistry.SubscriberCallback(this.CheckKillDownedEnemyWithRock));
			}
			AchievementsManager.FailedVegan = this._failedVegan;
			if (Achievements.Vegan != null && !this._failedVegan)
			{
				EventRegistry.Animal.Subscribe(TfEvent.KilledRabbit, new EventRegistry.SubscriberCallback(this.SetFailedVegan));
				EventRegistry.Animal.Subscribe(TfEvent.KilledLizard, new EventRegistry.SubscriberCallback(this.SetFailedVegan));
				EventRegistry.Animal.Subscribe(TfEvent.KilledRaccoon, new EventRegistry.SubscriberCallback(this.SetFailedVegan));
				EventRegistry.Animal.Subscribe(TfEvent.KilledDeer, new EventRegistry.SubscriberCallback(this.SetFailedVegan));
				EventRegistry.Animal.Subscribe(TfEvent.KilledTurtle, new EventRegistry.SubscriberCallback(this.SetFailedVegan));
				EventRegistry.Animal.Subscribe(TfEvent.KilledBird, new EventRegistry.SubscriberCallback(this.SetFailedVegan));
				EventRegistry.Animal.Subscribe(TfEvent.KilledShark, new EventRegistry.SubscriberCallback(this.SetFailedVegan));
				EventRegistry.Achievements.Subscribe(TfEvent.Achievements.AteLimb, new EventRegistry.SubscriberCallback(this.SetFailedVegan));
				EventRegistry.Achievements.Subscribe(TfEvent.Achievements.AteMeat, new EventRegistry.SubscriberCallback(this.SetFailedVegan));
			}
			if (Achievements.StealthArmor != null)
			{
				EventRegistry.Player.Subscribe(TfEvent.UsedItem, new EventRegistry.SubscriberCallback(this.CheckStealthArmor));
			}
			if (Achievements.VendingMachines != null)
			{
				if (!this._boughtSoda)
				{
					EventRegistry.Achievements.Subscribe(TfEvent.Achievements.BoughtSoda, new EventRegistry.SubscriberCallback(this.SetBoughtSoda));
				}
				if (!this._boughtSnacks)
				{
					EventRegistry.Achievements.Subscribe(TfEvent.Achievements.BoughtSnacks, new EventRegistry.SubscriberCallback(this.SetBoughtSnacks));
				}
			}
			if (Achievements.CollectCassettes != null)
			{
				if (this._collectedCassettes == null)
				{
					this._collectedCassettes = new List<int>(this._cassettesItemId.Count);
				}
				EventRegistry.Player.Subscribe(TfEvent.AddedItem, new EventRegistry.SubscriberCallback(this.CheckCollectedCassette));
			}
			if (Achievements.EatMushrooms != null)
			{
				if (this._eatenMushrooms == null)
				{
					this._eatenMushrooms = new List<int>(this._mushroomsItemId.Count);
				}
				EventRegistry.Player.Subscribe(TfEvent.UsedItem, new EventRegistry.SubscriberCallback(this.CheckEatenMushroom));
			}
			if (Achievements.Pacifist != null)
			{
				EventRegistry.Enemy.Subscribe(TfEvent.KilledEnemy, new EventRegistry.SubscriberCallback(this.UpdateLastKillEnemyTime));
			}
		}

		private void OnDestroy()
		{
			EventRegistry.Player.Unsubscribe(TfEvent.CraftedItem, new EventRegistry.SubscriberCallback(this.OnCraftedItem));
			EventRegistry.Achievements.Unsubscribe(TfEvent.Achievements.CreatedTrophy, new EventRegistry.SubscriberCallback(this.OnCreatedTrophy));
			EventRegistry.Achievements.Unsubscribe(TfEvent.Achievements.PlantedSeed, new EventRegistry.SubscriberCallback(this.OnPlantedSeed));
			EventRegistry.Player.Unsubscribe(TfEvent.CraftedItem, new EventRegistry.SubscriberCallback(this.CheckCraftedMeds));
			EventRegistry.Animal.Unsubscribe(TfEvent.KilledRabbit, new EventRegistry.SubscriberCallback(this.SetFailedVegan));
			EventRegistry.Animal.Unsubscribe(TfEvent.KilledLizard, new EventRegistry.SubscriberCallback(this.SetFailedVegan));
			EventRegistry.Animal.Unsubscribe(TfEvent.KilledRaccoon, new EventRegistry.SubscriberCallback(this.SetFailedVegan));
			EventRegistry.Animal.Unsubscribe(TfEvent.KilledDeer, new EventRegistry.SubscriberCallback(this.SetFailedVegan));
			EventRegistry.Animal.Unsubscribe(TfEvent.KilledTurtle, new EventRegistry.SubscriberCallback(this.SetFailedVegan));
			EventRegistry.Animal.Unsubscribe(TfEvent.KilledBird, new EventRegistry.SubscriberCallback(this.SetFailedVegan));
			EventRegistry.Animal.Unsubscribe(TfEvent.KilledShark, new EventRegistry.SubscriberCallback(this.SetFailedVegan));
			EventRegistry.Achievements.Unsubscribe(TfEvent.Achievements.AteLimb, new EventRegistry.SubscriberCallback(this.SetFailedVegan));
			EventRegistry.Achievements.Unsubscribe(TfEvent.Achievements.AteMeat, new EventRegistry.SubscriberCallback(this.SetFailedVegan));
			EventRegistry.Player.Unsubscribe(TfEvent.UsedItem, new EventRegistry.SubscriberCallback(this.CheckStealthArmor));
			EventRegistry.Player.Unsubscribe(TfEvent.AddedItem, new EventRegistry.SubscriberCallback(this.CheckCollectedCassette));
			EventRegistry.Player.Unsubscribe(TfEvent.UsedItem, new EventRegistry.SubscriberCallback(this.CheckEatenMushroom));
		}

		public void OnDeserialized()
		{
			if (this._doneCrafts != null)
			{
				this._doneCrafts.RemoveRange(this._doneCraftsCount, this._doneCrafts.Count - this._doneCraftsCount);
			}
			if (this._doneTrophies != null)
			{
				this._doneTrophies.RemoveRange(this._doneTrophiesCount, this._doneTrophies.Count - this._doneTrophiesCount);
			}
			if (this._donePlantTypes != null)
			{
				this._donePlantTypes.RemoveRange(this._donePlantTypesCount, this._donePlantTypes.Count - this._donePlantTypesCount);
			}
			if (this._collectedCassettes != null)
			{
				this._collectedCassettes.RemoveRange(this._collectedCassettesCount, this._collectedCassettes.Count - this._collectedCassettesCount);
			}
			if (this._eatenMushrooms != null)
			{
				this._eatenMushrooms.RemoveRange(this._eatenMushroomsCount, this._eatenMushrooms.Count - this._eatenMushroomsCount);
			}
			AchievementsManager.FailedVegan = this._failedVegan;
		}

		public void Clone(AchievementsManager achMan)
		{
			this._revivedPlayerSteamIds = achMan._revivedPlayerSteamIds;
			this._failedVegan = achMan._failedVegan;
			this._boughtSoda = achMan._boughtSoda;
			this._boughtSnacks = achMan._boughtSnacks;
			this._lastKillSurvivedDays = (achMan._lastKillSurvivedDays = -1);
			this._doneCrafts = achMan._doneCrafts;
			this._doneCraftsCount = achMan._doneCraftsCount;
			this._doneTrophies = achMan._doneTrophies;
			this._doneTrophiesCount = achMan._doneTrophiesCount;
			this._donePlantTypes = achMan._donePlantTypes;
			this._donePlantTypesCount = achMan._donePlantTypesCount;
			this._collectedCassettes = achMan._collectedCassettes;
			this._collectedCassettesCount = achMan._collectedCassettesCount;
			this._eatenMushrooms = achMan._eatenMushrooms;
			this._eatenMushroomsCount = achMan._eatenMushroomsCount;
		}

		public void OnCraftedItem(object o)
		{
			int item = (int)o;
			if (!this._doneCrafts.Contains(item) && this._craftablesItemId.Contains(item))
			{
				this._doneCrafts.Add(item);
				this._doneCraftsCount = this._doneCrafts.Count;
				EventRegistry.Achievements.Publish(TfEvent.Achievements.CraftedItemCount, this._doneCraftsCount);
			}
		}

		public void OnCreatedTrophy(object o)
		{
			int item = (int)o;
			if (!this._doneTrophies.Contains(item))
			{
				this._doneTrophies.Add(item);
				this._doneTrophiesCount = this._doneTrophies.Count;
				EventRegistry.Achievements.Publish(TfEvent.Achievements.TrophyCount, this._doneTrophiesCount);
			}
		}

		public void OnPlantedSeed(object o)
		{
			int item = (int)o;
			if (!this._donePlantTypes.Contains(item))
			{
				this._donePlantTypes.Add(item);
				this._donePlantTypesCount = this._donePlantTypes.Count;
				EventRegistry.Achievements.Publish(TfEvent.Achievements.SeedCount, this._donePlantTypesCount);
			}
		}

		public void CheckCraftedMeds(object o)
		{
			int id = (int)o;
			Item item = ItemDatabase.ItemById(id);
			if (item != null && item._usedStatEffect != null && item._usedStatEffect.Length > 0 && item._usedStatEffect[0]._type == StatEffect.Types.Health)
			{
				EventRegistry.Achievements.Publish(TfEvent.Achievements.CraftedMeds, null);
			}
		}

		public void SetFailedVegan(object o)
		{
			Debug.Log("FAILED VEGAN");
			this._failedVegan = true;
			AchievementsManager.FailedVegan = this._failedVegan;
			EventRegistry.Animal.Unsubscribe(TfEvent.KilledRabbit, new EventRegistry.SubscriberCallback(this.SetFailedVegan));
			EventRegistry.Animal.Unsubscribe(TfEvent.KilledLizard, new EventRegistry.SubscriberCallback(this.SetFailedVegan));
			EventRegistry.Animal.Unsubscribe(TfEvent.KilledRaccoon, new EventRegistry.SubscriberCallback(this.SetFailedVegan));
			EventRegistry.Animal.Unsubscribe(TfEvent.KilledDeer, new EventRegistry.SubscriberCallback(this.SetFailedVegan));
			EventRegistry.Animal.Unsubscribe(TfEvent.KilledTurtle, new EventRegistry.SubscriberCallback(this.SetFailedVegan));
			EventRegistry.Animal.Unsubscribe(TfEvent.KilledBird, new EventRegistry.SubscriberCallback(this.SetFailedVegan));
			EventRegistry.Animal.Unsubscribe(TfEvent.KilledShark, new EventRegistry.SubscriberCallback(this.SetFailedVegan));
			EventRegistry.Achievements.Unsubscribe(TfEvent.Achievements.AteLimb, new EventRegistry.SubscriberCallback(this.SetFailedVegan));
			EventRegistry.Achievements.Unsubscribe(TfEvent.Achievements.AteMeat, new EventRegistry.SubscriberCallback(this.SetFailedVegan));
		}

		public void CheckStealthArmor(object o)
		{
			int num = (int)o;
			if (num == this._stealthArmorItemId)
			{
				EventRegistry.Achievements.Publish(TfEvent.Achievements.UsedStealthArmor, null);
				EventRegistry.Player.Unsubscribe(TfEvent.UsedItem, new EventRegistry.SubscriberCallback(this.CheckStealthArmor));
			}
		}

		public void SetBoughtSoda(object o)
		{
			this._boughtSoda = true;
			EventRegistry.Achievements.Unsubscribe(TfEvent.Achievements.BoughtSoda, new EventRegistry.SubscriberCallback(this.SetBoughtSoda));
			EventRegistry.Achievements.Publish(TfEvent.Achievements.BigSpender, null);
		}

		public void SetBoughtSnacks(object o)
		{
			this._boughtSnacks = true;
			EventRegistry.Achievements.Unsubscribe(TfEvent.Achievements.BoughtSnacks, new EventRegistry.SubscriberCallback(this.SetBoughtSnacks));
			EventRegistry.Achievements.Publish(TfEvent.Achievements.BigSpender, null);
		}

		public void CheckCollectedCassette(object o)
		{
			int item = (int)o;
			if (this._cassettesItemId.Contains(item) && !this._collectedCassettes.Contains(item))
			{
				this._collectedCassettes.Add(item);
				this._collectedCassettesCount = this._collectedCassettes.Count;
				EventRegistry.Achievements.Publish(TfEvent.Achievements.Cassettes, this._collectedCassettesCount);
			}
		}

		public void CheckEatenMushroom(object o)
		{
			int item = (int)o;
			if (this._mushroomsItemId.Contains(item) && !this._eatenMushrooms.Contains(item))
			{
				this._eatenMushrooms.Add(item);
				this._eatenMushroomsCount = this._eatenMushrooms.Count;
				EventRegistry.Achievements.Publish(TfEvent.Achievements.AteMushrooms, this._eatenMushroomsCount);
			}
		}

		private void CheckKillDownedEnemyWithRock(object o)
		{
			EnemyHealth enemyHealth = (EnemyHealth)o;
			if (enemyHealth && enemyHealth.targetSwitcher && enemyHealth.targetSwitcher.currentAttackerGo == LocalPlayer.GameObject && LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._rockItemId))
			{
				targetStats componentInParent = enemyHealth.GetComponentInParent<targetStats>();
				if (componentInParent && componentInParent.targetDown)
				{
					EventRegistry.Achievements.Publish(TfEvent.Achievements.DownedEnemyRockKill, null);
					EventRegistry.Enemy.Unsubscribe(TfEvent.KilledEnemy, new EventRegistry.SubscriberCallback(this.CheckKillDownedEnemyWithRock));
				}
			}
		}

		public void UpdateLastKillEnemyTime(object o)
		{
			this._lastKillSurvivedDays = Mathf.CeilToInt(LocalPlayer.Stats.DaySurvived);
		}

		public static bool FailedVegan { get; private set; }

		public int AnimalsKilled { get; private set; }

		public bool BoughtSoda
		{
			get
			{
				return this._boughtSoda;
			}
		}

		public bool BoughtSnacks
		{
			get
			{
				return this._boughtSnacks;
			}
		}

		public int LastKillSurvivedDays
		{
			get
			{
				return this._lastKillSurvivedDays;
			}
		}

		[ItemIdPicker]
		public int _medsItemId;

		[ItemIdPicker]
		public int _rockItemId;

		[ItemIdPicker]
		public int _stealthArmorItemId;

		[ItemIdPicker]
		public List<int> _craftablesItemId;

		[ItemIdPicker]
		public List<int> _cassettesItemId;

		[ItemIdPicker]
		public List<int> _mushroomsItemId;

		private ulong[] _revivedPlayerSteamIds;

		[SerializeThis]
		private bool _failedVegan;

		[SerializeThis]
		private bool _boughtSoda;

		[SerializeThis]
		private bool _boughtSnacks;

		[SerializeThis]
		private int _lastKillSurvivedDays = -1;

		[SerializeThis]
		private List<int> _doneCrafts;

		[SerializeThis]
		private int _doneCraftsCount;

		[SerializeThis]
		private List<int> _doneTrophies;

		[SerializeThis]
		private int _doneTrophiesCount;

		[SerializeThis]
		private List<int> _donePlantTypes;

		[SerializeThis]
		private int _donePlantTypesCount;

		[SerializeThis]
		private List<int> _collectedCassettes;

		[SerializeThis]
		private int _collectedCassettesCount;

		[SerializeThis]
		private List<int> _eatenMushrooms;

		[SerializeThis]
		private int _eatenMushroomsCount;
	}
}
