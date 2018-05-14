using System;

namespace TheForest.Tools
{
	
	public class TfEvent
	{
		
		public static object Get(TfEvent.Types type)
		{
			switch (type)
			{
			case TfEvent.Types.EnterEndgame:
				return TfEvent.EnterEndgame;
			case TfEvent.Types.ExitEndgame:
				return TfEvent.ExitEndgame;
			case TfEvent.Types.EnterOverlookArea:
				return TfEvent.EnterOverlookArea;
			case TfEvent.Types.ExitOverlookArea:
				return TfEvent.ExitOverlookArea;
			case TfEvent.Types.DifficultySet:
				return TfEvent.DifficultySet;
			case TfEvent.Types.BuiltStructure:
				return TfEvent.BuiltStructure;
			case TfEvent.Types.EnemyInSight:
				return TfEvent.EnemyInSight;
			case TfEvent.Types.EnemyContact:
				return TfEvent.EnemyContact;
			case TfEvent.Types.StoryProgress:
				return TfEvent.StoryProgress;
			case TfEvent.Types.FoundStarLocation:
				return TfEvent.FoundStarLocation;
			case TfEvent.Types.AddedItem:
				return TfEvent.AddedItem;
			case TfEvent.Types.UsedItem:
				return TfEvent.UsedItem;
			case TfEvent.Types.TickedOffEntry:
				return TfEvent.TickedOffEntry;
			case TfEvent.Types.InspectedAnimal:
				return TfEvent.InspectedAnimal;
			case TfEvent.Types.Endgame_FireDetected:
				return TfEvent.Endgame.FireDetected;
			case TfEvent.Types.Endgame_Completed:
				return TfEvent.Endgame.Completed;
			case TfEvent.Types.RegrowModeSet:
				return TfEvent.RegrowModeSet;
			case TfEvent.Types.NoDestructionModeSet:
				return TfEvent.NoDestrutionModeSet;
			case TfEvent.Types.Slept:
				return TfEvent.Slept;
			case TfEvent.Types.AllowEnemiesSet:
				return TfEvent.AllowEnemiesSet;
			case TfEvent.Types.LanguageSet:
				return TfEvent.LanguageSet;
			case TfEvent.Types.SurvivedDay:
				return TfEvent.SurvivedDay;
			case TfEvent.Types.EquippedItem:
				return TfEvent.EquippedItem;
			case TfEvent.Types.RemovedItem:
				return TfEvent.RemovedItem;
			case TfEvent.Types.KilledEnemy:
				return TfEvent.KilledEnemy;
			case TfEvent.Types.CutLimb:
				return TfEvent.CutLimb;
			case TfEvent.Types.Achievements_DrinkCoffee:
				return TfEvent.Achievements.DrinkCoffee;
			case TfEvent.Types.Achievements_BoughtSnacks:
				return TfEvent.Achievements.BoughtSnacks;
			case TfEvent.Types.Achievements_BoughtSoda:
				return TfEvent.Achievements.BoughtSoda;
			case TfEvent.Types.CutTree:
				return TfEvent.CutTree;
			case TfEvent.Types.Achievements_GrossDrink:
				return TfEvent.Achievements.GrossDrink;
			case TfEvent.Types.Achievements_SleptInYacht:
				return TfEvent.Achievements.SleptInYacht;
			case TfEvent.Types.ExploredCaveArea:
				return TfEvent.ExploredCaveArea;
			case TfEvent.Types.EnterSnowCave:
				return TfEvent.EnterSnowCave;
			case TfEvent.Types.ExitSnowCave:
				return TfEvent.ExitSnowCave;
			case TfEvent.Types.InspectedPlant:
				return TfEvent.InspectedPlant;
			case TfEvent.Types.Endgame_Shutdown2ndArtifact:
				return TfEvent.Endgame.Shutdown2ndArtifact;
			case TfEvent.Types.CheatAllowedSet:
				return TfEvent.CheatAllowedSet;
			default:
				return null;
			}
		}

		
		public static readonly object EnterEndgame = new object();

		
		public static readonly object ExitEndgame = new object();

		
		public static readonly object EnterOverlookArea = new object();

		
		public static readonly object ExitOverlookArea = new object();

		
		public static readonly object DifficultySet = new object();

		
		public static readonly object GameTypeSet = new object();

		
		public static readonly object RegrowModeSet = new object();

		
		public static readonly object NoDestrutionModeSet = new object();

		
		public static readonly object BuiltStructure = new object();

		
		public static readonly object EnemyInSight = new object();

		
		public static readonly object EnemyContact = new object();

		
		public static readonly object StoryProgress = new object();

		
		public static readonly object FoundStarLocation = new object();

		
		public static readonly object EquippedItem = new object();

		
		public static readonly object AddedItem = new object();

		
		public static readonly object RemovedItem = new object();

		
		public static readonly object UsedItem = new object();

		
		public static readonly object TickedOffEntry = new object();

		
		public static readonly object InspectedAnimal = new object();

		
		public static readonly object Slept = new object();

		
		public static readonly object AllowEnemiesSet = new object();

		
		public static readonly object LanguageSet = new object();

		
		public static readonly object SurvivedDay = new object();

		
		public static readonly object KilledEnemy = new object();

		
		public static readonly object CutLimb = new object();

		
		public static readonly object FoundPassenger = new object();

		
		public static readonly object KilledRabbit = new object();

		
		public static readonly object KilledLizard = new object();

		
		public static readonly object KilledRaccoon = new object();

		
		public static readonly object KilledDeer = new object();

		
		public static readonly object KilledTurtle = new object();

		
		public static readonly object KilledBird = new object();

		
		public static readonly object KilledShark = new object();

		
		public static readonly object CutTree = new object();

		
		public static readonly object WalkedSteps = new object();

		
		public static readonly object UsedBomb = new object();

		
		public static readonly object CraftedItem = new object();

		
		public static readonly object ExploredCaveArea = new object();

		
		public static readonly object EnterSnowCave = new object();

		
		public static readonly object ExitSnowCave = new object();

		
		public static readonly object InspectedPlant = new object();

		
		public static readonly object CheatAllowedSet = new object();

		
		public class Achievements
		{
			
			public static readonly object AteLimb = new object();

			
			public static readonly object SharedWeapon = new object();

			
			public static readonly object SharedEdible = new object();

			
			public static readonly object RevivedPlayer = new object();

			
			public static readonly object UsingCompass = new object();

			
			public static readonly object EnemyTrapKill = new object();

			
			public static readonly object TreeDynamited = new object();

			
			public static readonly object BirdArrowKill = new object();

			
			public static readonly object CraftedItemCount = new object();

			
			public static readonly object CreatedTrophy = new object();

			
			public static readonly object TrophyCount = new object();

			
			public static readonly object PlantedSeed = new object();

			
			public static readonly object SeedCount = new object();

			
			public static readonly object SleptInYacht = new object();

			
			public static readonly object CraftedMeds = new object();

			
			public static readonly object DownedEnemyRockKill = new object();

			
			public static readonly object PlacedWall = new object();

			
			public static readonly object UsedStealthArmor = new object();

			
			public static readonly object FishDynamited = new object();

			
			public static readonly object FishTrapped = new object();

			
			public static readonly object DrinkCoffee = new object();

			
			public static readonly object BoughtSoda = new object();

			
			public static readonly object BoughtSnacks = new object();

			
			public static readonly object BigSpender = new object();

			
			public static readonly object RepairedShelter = new object();

			
			public static readonly object Cassettes = new object();

			
			public static readonly object RobotPieces = new object();

			
			public static readonly object DoneCaveTasks = new object();

			
			public static readonly object GrossDrink = new object();

			
			public static readonly object AteMushrooms = new object();

			
			public static readonly object AteMeat = new object();
		}

		
		public class Endgame
		{
			
			public static readonly object FireDetected = new object();

			
			public static readonly object Completed = new object();

			
			public static readonly object Shutdown2ndArtifact = new object();
		}

		
		public enum Types
		{
			
			EnterEndgame,
			
			ExitEndgame,
			
			EnterOverlookArea,
			
			ExitOverlookArea,
			
			DifficultySet,
			
			BuiltStructure,
			
			EnemyInSight,
			
			EnemyContact,
			
			StoryProgress,
			
			FoundStarLocation,
			
			AddedItem,
			
			UsedItem,
			
			TickedOffEntry,
			
			InspectedAnimal,
			
			Endgame_FireDetected,
			
			Endgame_Completed,
			
			RegrowModeSet,
			
			NoDestructionModeSet,
			
			Slept,
			
			AllowEnemiesSet,
			
			LanguageSet,
			
			SurvivedDay,
			
			EquippedItem,
			
			RemovedItem,
			
			KilledEnemy,
			
			CutLimb,
			
			Achievements_DrinkCoffee,
			
			Achievements_BoughtSnacks,
			
			Achievements_BoughtSoda,
			
			CutTree,
			
			Achievements_GrossDrink,
			
			Achievements_SleptInYacht,
			
			ExploredCaveArea,
			
			EnterSnowCave,
			
			ExitSnowCave,
			
			InspectedPlant,
			
			Endgame_Shutdown2ndArtifact,
			
			CheatAllowedSet
		}
	}
}
