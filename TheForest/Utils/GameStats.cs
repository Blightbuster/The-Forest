using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using TheForest.Buildings.Creation;
using TheForest.TaskSystem;
using TheForest.Tools;
using UnityEngine;
using UnityEngine.Events;

namespace TheForest.Utils
{
	
	public class GameStats : MonoBehaviour
	{
		
		private void Awake()
		{
			EventRegistry.Enemy.Subscribe(TfEvent.KilledEnemy, new EventRegistry.SubscriberCallback(this.OnEnemyKilled));
			EventRegistry.Player.Subscribe(typeof(BuildingCondition), new EventRegistry.SubscriberCallback(this.OnStructureBuilt));
			EventRegistry.Player.Subscribe(typeof(StoryCondition), new EventRegistry.SubscriberCallback(this.OnStoryProgress));
			EventRegistry.Player.Subscribe(TfEvent.UsedItem, new EventRegistry.SubscriberCallback(this.OnEdibleItemUsed));
			EventRegistry.Player.Subscribe(TfEvent.FoundPassenger, new EventRegistry.SubscriberCallback(this.OnPassengerFound));
			EventRegistry.Animal.Subscribe(TfEvent.KilledRabbit, new EventRegistry.SubscriberCallback(this.RabbitKilled));
			EventRegistry.Animal.Subscribe(TfEvent.KilledLizard, new EventRegistry.SubscriberCallback(this.LizardKilled));
			EventRegistry.Animal.Subscribe(TfEvent.KilledRaccoon, new EventRegistry.SubscriberCallback(this.RaccoonKilled));
			EventRegistry.Animal.Subscribe(TfEvent.KilledDeer, new EventRegistry.SubscriberCallback(this.DeerKilled));
			EventRegistry.Animal.Subscribe(TfEvent.KilledTurtle, new EventRegistry.SubscriberCallback(this.TurtleKilled));
			EventRegistry.Animal.Subscribe(TfEvent.KilledBird, new EventRegistry.SubscriberCallback(this.BirdKilled));
			EventRegistry.Player.Subscribe(TfEvent.CutTree, new EventRegistry.SubscriberCallback(this.OnTreeCutDown));
			EventRegistry.Player.Subscribe(TfEvent.CraftedItem, new EventRegistry.SubscriberCallback(this.OnCraftedItem));
			GameStats.CookedFood.AddListener(delegate
			{
				this._stats._cookedFood = this._stats._cookedFood + 1;
			});
			GameStats.BurntFood.AddListener(delegate
			{
				this._stats._burntFood = this._stats._burntFood + 1;
			});
			GameStats.CancelledStructure.AddListener(delegate
			{
				this._stats._cancelledStructures = this._stats._cancelledStructures + 1;
			});
			GameStats.DestroyedStructure.AddListener(delegate
			{
				this._stats._destroyedStructures = this._stats._destroyedStructures + 1;
			});
			GameStats.RepairedStructure.AddListener(delegate
			{
				this._stats._repairedStructures = this._stats._repairedStructures + 1;
			});
			GameStats.UpgradesAdded.AddListener(delegate(int amount)
			{
				this._stats._upgradesAdded = this._stats._upgradesAdded + amount;
			});
			GameStats.ArrowFired.AddListener(delegate
			{
				this._stats._arrowsFired = this._stats._arrowsFired + 1;
			});
			GameStats.LitArrow.AddListener(delegate
			{
				this._stats._litArrows = this._stats._litArrows + 1;
			});
			GameStats.LitWeapon.AddListener(delegate
			{
				this._stats._litWeapons = this._stats._litWeapons + 1;
			});
			GameStats.BurntEnemy.AddListener(delegate
			{
				this._stats._burntEnemies = this._stats._burntEnemies + 1;
			});
			GameStats.OpenedSuitcase.AddListener(delegate
			{
				this._stats._openedSuitcases = this._stats._openedSuitcases + 1;
			});
			GameStats.Infected.AddListener(delegate
			{
				this._stats._infections = this._stats._infections + 1;
			});
		}

		
		private void OnDestroy()
		{
			EventRegistry.Enemy.Unsubscribe(TfEvent.KilledEnemy, new EventRegistry.SubscriberCallback(this.OnEnemyKilled));
			EventRegistry.Player.Unsubscribe(typeof(BuildingCondition), new EventRegistry.SubscriberCallback(this.OnStructureBuilt));
			EventRegistry.Player.Unsubscribe(typeof(StoryCondition), new EventRegistry.SubscriberCallback(this.OnStoryProgress));
			EventRegistry.Player.Unsubscribe(TfEvent.UsedItem, new EventRegistry.SubscriberCallback(this.OnEdibleItemUsed));
			EventRegistry.Player.Unsubscribe(TfEvent.FoundPassenger, new EventRegistry.SubscriberCallback(this.OnPassengerFound));
			EventRegistry.Animal.Unsubscribe(TfEvent.KilledRabbit, new EventRegistry.SubscriberCallback(this.RabbitKilled));
			EventRegistry.Animal.Unsubscribe(TfEvent.KilledLizard, new EventRegistry.SubscriberCallback(this.LizardKilled));
			EventRegistry.Animal.Unsubscribe(TfEvent.KilledRaccoon, new EventRegistry.SubscriberCallback(this.RaccoonKilled));
			EventRegistry.Animal.Unsubscribe(TfEvent.KilledDeer, new EventRegistry.SubscriberCallback(this.DeerKilled));
			EventRegistry.Animal.Unsubscribe(TfEvent.KilledTurtle, new EventRegistry.SubscriberCallback(this.TurtleKilled));
			EventRegistry.Animal.Unsubscribe(TfEvent.KilledBird, new EventRegistry.SubscriberCallback(this.BirdKilled));
		}

		
		private void OnSerializing()
		{
			this._stats._day = Clock.Day;
			string localSlotPath = SaveSlotUtils.GetLocalSlotPath();
			string path = localSlotPath + "info";
			string filename = SaveSlotUtils.GetCloudSlotPath() + "info";
			if (!Directory.Exists(localSlotPath))
			{
				Directory.CreateDirectory(localSlotPath);
			}
			IFormatter formatter = new BinaryFormatter();
			byte[] array;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				formatter.Serialize(memoryStream, this._stats);
				array = memoryStream.ToArray();
			}
			File.WriteAllBytes(path, array);
			CoopSteamCloud.CloudSave(filename, array);
		}

		
		private void RabbitKilled(object o)
		{
			this._stats._rabbitKilled = this._stats._rabbitKilled + 1;
		}

		
		private void LizardKilled(object o)
		{
			this._stats._lizardKilled = this._stats._lizardKilled + 1;
		}

		
		private void RaccoonKilled(object o)
		{
			this._stats._raccoonKilled = this._stats._raccoonKilled + 1;
		}

		
		private void DeerKilled(object o)
		{
			this._stats._deerKilled = this._stats._deerKilled + 1;
		}

		
		private void TurtleKilled(object o)
		{
			this._stats._turtleKilled = this._stats._turtleKilled + 1;
		}

		
		private void BirdKilled(object o)
		{
			this._stats._birdKilled = this._stats._birdKilled + 1;
		}

		
		private void OnStructureBuilt(object o)
		{
			this._stats._builtStructures = this._stats._builtStructures + 1;
		}

		
		private void OnEnemyKilled(object o)
		{
			this._stats._enemiesKilled = this._stats._enemiesKilled + 1;
		}

		
		private void OnEdibleItemUsed(object o)
		{
			this._stats._edibleItemsUsed = this._stats._edibleItemsUsed + 1;
		}

		
		private void OnPassengerFound(object o)
		{
			this._stats._passengersFound = (int)o;
		}

		
		private void OnTreeCutDown(object o)
		{
			this._stats._treeCutDown = this._stats._treeCutDown + 1;
		}

		
		private void OnCraftedItem(object o)
		{
			this._stats._itemsCrafted = this._stats._itemsCrafted + 1;
		}

		
		private void OnStoryProgress(object o)
		{
			GameStats.StoryElements storyElements = (GameStats.StoryElements)((int)o);
			this._stats._storyElements = (this._stats._storyElements | storyElements);
		}

		
		public GameStats.Stats _stats;

		
		public static UnityEvent CookedFood = new UnityEvent();

		
		public static UnityEvent BurntFood = new UnityEvent();

		
		public static UnityEvent CancelledStructure = new UnityEvent();

		
		public static UnityEvent DestroyedStructure = new UnityEvent();

		
		public static UnityEvent RepairedStructure = new UnityEvent();

		
		public static GameStats.UpgradeEvent UpgradesAdded = new GameStats.UpgradeEvent();

		
		public static UnityEvent ArrowFired = new UnityEvent();

		
		public static UnityEvent LitArrow = new UnityEvent();

		
		public static UnityEvent LitWeapon = new UnityEvent();

		
		public static UnityEvent BurntEnemy = new UnityEvent();

		
		public static UnityEvent OpenedSuitcase = new UnityEvent();

		
		public static UnityEvent Infected = new UnityEvent();

		
		public class ItemEvent : UnityEvent<int>
		{
		}

		
		public class UpgradeEvent : UnityEvent<int>
		{
		}

		
		public class BuildEvent : UnityEvent<BuildingTypes>
		{
		}

		
		public class StoryEvent : UnityEvent<GameStats.StoryElements>
		{
		}

		
		[Serializable]
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct Stats
		{
			
			public static GameStats.Stats LoadFromBytes(byte[] bytes)
			{
				GameStats.Stats result;
				try
				{
					IFormatter formatter = new BinaryFormatter();
					using (MemoryStream memoryStream = new MemoryStream(bytes))
					{
						result = (GameStats.Stats)formatter.Deserialize(memoryStream);
					}
				}
				catch (Exception message)
				{
					Debug.LogError(message);
					result = default(GameStats.Stats);
				}
				return result;
			}

			
			public int _day;

			
			public int _treeCutDown;

			
			public int _enemiesKilled;

			
			public int _rabbitKilled;

			
			public int _lizardKilled;

			
			public int _raccoonKilled;

			
			public int _deerKilled;

			
			public int _turtleKilled;

			
			public int _birdKilled;

			
			public int _cookedFood;

			
			public int _burntFood;

			
			public int _cancelledStructures;

			
			public int _builtStructures;

			
			public int _destroyedStructures;

			
			public int _repairedStructures;

			
			public int _edibleItemsUsed;

			
			public int _itemsCrafted;

			
			public int _upgradesAdded;

			
			public int _arrowsFired;

			
			public int _litArrows;

			
			public int _litWeapons;

			
			public int _burntEnemies;

			
			public int _explodedEnemies;

			
			public int _openedSuitcases;

			
			public int _passengersFound;

			
			public GameStats.StoryElements _storyElements;

			
			public int _infections;
		}

		
		[Flags]
		public enum StoryElements
		{
			
			HangingScene = 1,
			
			RedManOnYacht = 32,
			
			FoundClimbWall = 256,
			
			TimmyFound = 512,
			
			MeganFound = 1024
		}
	}
}
