using System;
using System.Collections.Generic;
using TheForest.Buildings.Creation;
using TheForest.Tools;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Player
{
	
	public static class Achievements
	{
		
		
		
		public static AchievementData Crafty { get; private set; }

		
		
		
		public static AchievementData TrophyHunter { get; private set; }

		
		
		
		public static AchievementData Gardener { get; private set; }

		
		
		
		public static AchievementData Naturopath { get; private set; }

		
		
		
		public static AchievementData Splatter { get; private set; }

		
		
		
		public static AchievementData Vegan { get; private set; }

		
		
		
		public static AchievementData StealthArmor { get; private set; }

		
		
		
		public static AchievementData VendingMachines { get; private set; }

		
		
		
		public static AchievementData CollectCassettes { get; private set; }

		
		
		
		public static AchievementData EatMushrooms { get; private set; }

		
		
		
		public static AchievementData Pacifist { get; private set; }

		
		public static void Reset()
		{
			if (Achievements.Data != null)
			{
				foreach (AchievementData achievementData in Achievements.Data)
				{
					if (achievementData != null)
					{
						achievementData.Clear();
					}
				}
			}
			Achievements.Crafty = null;
			Achievements.TrophyHunter = null;
			Achievements.Gardener = null;
			Achievements.Naturopath = null;
			Achievements.Splatter = null;
			Achievements.Vegan = null;
			Achievements.StealthArmor = null;
			Achievements.VendingMachines = null;
			Achievements.CollectCassettes = null;
			Achievements.EatMushrooms = null;
			Achievements.Pacifist = null;
			Achievements.Data = new AchievementData[]
			{
				new AchievementData("Major Cannibalism", "Eat an entire family", "ACH_MAJOR_CANNIBALISM", "STAT_CANNIBALISM", new List<int>
				{
					4,
					24
				}, EventRegistry.Achievements, TfEvent.Achievements.AteLimb, null, null),
				new AchievementData("Be Nice", "Share a food or drink item in MP", "ACH_BE_NICE", null, EventRegistry.Achievements, TfEvent.Achievements.SharedEdible, null, null),
				new AchievementData("Be Extremely nice", "Share a weapon in MP", "ACH_BE_EXTREMELY_NICE", null, EventRegistry.Achievements, TfEvent.Achievements.SharedWeapon, null, null),
				new AchievementData("Crafty", "Craft all items", "ACH_CRAFTY", "STAT_UNIQUE_CRAFTED_ITEMS", new List<int>
				{
					26
				}, EventRegistry.Achievements, TfEvent.Achievements.CraftedItemCount, new Func<AchievementData, object, bool>(Achievements.IntHighScore), new Func<AchievementData, object, bool>(Achievements.SetupAchCrafty)),
				new AchievementData("Bad father", "Survive 100 days without finding your son", "ACH_BAD_FATHER", "STAT_DAYS_SURVIVED", new List<int>
				{
					1,
					5,
					100
				}, EventRegistry.Player, TfEvent.SurvivedDay, new Func<AchievementData, object, bool>(Achievements.DaySurvivedAction), null),
				new AchievementData("Trophy Hunter", "Kill all animal types and display heads", "ACH_TROPHY_HUNTER", "STAT_TROPHY_CREATED", new List<int>
				{
					11
				}, EventRegistry.Achievements, TfEvent.Achievements.TrophyCount, new Func<AchievementData, object, bool>(Achievements.IntHighScore), new Func<AchievementData, object, bool>(Achievements.SetupAchTrophyHunter)),
				new AchievementData("Gardener", "Grow all plant types", "ACH_GARDENER", "STAT_PLANTED_SEEDS", new List<int>
				{
					3
				}, EventRegistry.Achievements, TfEvent.Achievements.SeedCount, new Func<AchievementData, object, bool>(Achievements.IntHighScore), new Func<AchievementData, object, bool>(Achievements.SetupAchGardener)),
				new AchievementData("Vegan", "play through entire game without killing or eating animals", "ACH_VEGAN", null, EventRegistry.Endgame, TfEvent.Endgame.Completed, new Func<AchievementData, object, bool>(Achievements.CheckVegan), new Func<AchievementData, object, bool>(Achievements.SetupAchVegan)),
				new AchievementData("Naturopath", "craft 10 medicine items", "ACH_NATUROPATH", "STAT_CRAFTED_MEDS", new List<int>
				{
					10
				}, EventRegistry.Achievements, TfEvent.Achievements.CraftedMeds, null, new Func<AchievementData, object, bool>(Achievements.SetupAchNaturopath)),
				new AchievementData("5 Star hotel", "Sleep on yacht", "ACH_5_STAR_HOTEL", null, EventRegistry.Achievements, TfEvent.Achievements.SleptInYacht, null, null),
				new AchievementData("Spelunker", "Explore all caves", "ACH_SPELUNKER", "STAT_COMPLETED_CAVE_TASKS", new List<int>
				{
					10
				}, EventRegistry.Achievements, TfEvent.Achievements.DoneCaveTasks, new Func<AchievementData, object, bool>(Achievements.IntHighScore), null),
				new AchievementData("Medic", "Revive 10 unique coop players", "ACH_MEDIC", "STAT_REVIVED_PLAYERS", new List<int>
				{
					1,
					10
				}, EventRegistry.Achievements, TfEvent.Achievements.RevivedPlayer, null, null),
				new AchievementData("You should be looking for timmy", "Build a gazebo", "ACH_BUILD_GAZEBO", null, EventRegistry.Player, TfEvent.BuiltStructure, new Func<AchievementData, object, bool>(Achievements.CheckBuildGazebo), null),
				new AchievementData("Boy Scout", "Use the compass", "ACH_BOY_SCOOT", null, EventRegistry.Achievements, TfEvent.Achievements.UsingCompass, null, null),
				new AchievementData("campout", "Sleep with another player (group sleep)", "ACH_GROUP_SLEEP", null, EventRegistry.Player, TfEvent.Slept, new Func<AchievementData, object, bool>(Achievements.CheckSeveralPlayersAction), null),
				new AchievementData("splatter", "Killed downed enemy with rock", "ACH_KILL_DOWNED_ENEMY", null, EventRegistry.Achievements, TfEvent.Achievements.DownedEnemyRockKill, null, new Func<AchievementData, object, bool>(Achievements.SetupAchSplatter)),
				new AchievementData("longest wall", "Build a super long wall", "ACH_LONG_WALL", "STAT_LENGTHIEST_WALL", new List<int>
				{
					250
				}, EventRegistry.Achievements, TfEvent.Achievements.PlacedWall, new Func<AchievementData, object, bool>(Achievements.IntHighScore), null),
				new AchievementData("choppy chop", "Chop up 50 bodies", "ACH_CHOP_BODIES", "STAT_CHOPPED_BODIES", new List<int>
				{
					50
				}, EventRegistry.Enemy, TfEvent.CutLimb, null, null),
				new AchievementData("unseen", "Make and wear stealth armour", "ACH_STEALTH_ARMOR", null, EventRegistry.Achievements, TfEvent.Achievements.UsedStealthArmor, null, new Func<AchievementData, object, bool>(Achievements.SetupAchStealthArmor)),
				new AchievementData("fisherman", "kill 6 or more fish with a stick of dynamite", "ACH_FISH_DYNAMITED", "STAT_FISH_DYNAMITED", new List<int>
				{
					6
				}, EventRegistry.Achievements, TfEvent.Achievements.FishDynamited, new Func<AchievementData, object, bool>(Achievements.IntHighScore), null),
				new AchievementData("Survive the forest", "Finish Game", "ACH_SURVIVE_THE_FOREST", null, EventRegistry.Endgame, TfEvent.Endgame.Completed, null, null),
				new AchievementData("daily grind", "Drink from coffee machine", "ACH_DRINK_COFFEE", null, EventRegistry.Achievements, TfEvent.Achievements.DrinkCoffee, null, null),
				new AchievementData("gross", "Drink water from cooler that has the lawyer head in it", "ACH_GROSS_DRINK", null, EventRegistry.Achievements, TfEvent.Achievements.GrossDrink, null, null),
				new AchievementData("serial killer", "Kill 100 cannibals", "ACH_KILL_CANNIBALS", "STAT_KILLED_CANNIBALS", new List<int>
				{
					100
				}, EventRegistry.Enemy, TfEvent.KilledEnemy, null, null),
				new AchievementData("make it rain", "Set off sprinklers in end game", "ACH_SPRINKLERS", null, EventRegistry.Endgame, TfEvent.Endgame.FireDetected, null, null),
				new AchievementData("get closure", "Find all missing passengers", "ACH_FIND_PASSENGERS", "STAT_FOUND_PASSENGERS", new List<int>
				{
					43
				}, EventRegistry.Player, TfEvent.FoundPassenger, new Func<AchievementData, object, bool>(Achievements.IntHighScore), null),
				new AchievementData("big spender", "Buy a soda and candy from vending machine", "ACH_VENDING_MACHINES", null, EventRegistry.Achievements, TfEvent.Achievements.BigSpender, new Func<AchievementData, object, bool>(Achievements.CheckSpendings), new Func<AchievementData, object, bool>(Achievements.SetupAchVendingMachines)),
				new AchievementData("Bite me!", "Kill shark", "ACH_KILL_SHARK", null, EventRegistry.Animal, TfEvent.KilledShark, null, null),
				new AchievementData("Monster", "Kill bunny", "ACH_KILL_BUNNY", null, EventRegistry.Animal, TfEvent.KilledRabbit, null, null),
				new AchievementData("gabe fan", "Collect all cassette tapes!", "ACH_COLLECT_CASSETTES", "STAT_COLLECTED_CASSETTES", new List<int>
				{
					5
				}, EventRegistry.Achievements, TfEvent.Achievements.Cassettes, new Func<AchievementData, object, bool>(Achievements.IntHighScore), new Func<AchievementData, object, bool>(Achievements.SetupAchCollectCassettes)),
				new AchievementData("You are a fun guy", "Eat all mushroom types", "ACH_EAT_ALL_MUSHROOMS", "STAT_ATE_MUSHROOMS_TYPES", new List<int>
				{
					6
				}, EventRegistry.Achievements, TfEvent.Achievements.AteMushrooms, new Func<AchievementData, object, bool>(Achievements.IntHighScore), new Func<AchievementData, object, bool>(Achievements.SetupAchEatAllMushrooms)),
				new AchievementData("Archer", "kill bird with arrow", "ACH_SHOOT_BIRDS", null, EventRegistry.Achievements, TfEvent.Achievements.BirdArrowKill, null, null),
				new AchievementData("fitbits", "pass 50 000 steps", "ACH_MARATHON_STEPS", "STAT_TOTAL_WALKED_STEPS", new List<int>
				{
					50000
				}, EventRegistry.Player, TfEvent.WalkedSteps, new Func<AchievementData, object, bool>(Achievements.IntHighScore), null),
				new AchievementData("demolition man ", "Set off 20 bombs", "ACH_USED_BOMBS", "STAT_USED_BOMBS", new List<int>
				{
					20
				}, EventRegistry.Player, TfEvent.UsedBomb, null, null),
				new AchievementData("handyman", "Repair a shelter", "ACH_REPAIR_SHELTER", null, EventRegistry.Achievements, TfEvent.Achievements.RepairedShelter, null, null),
				new AchievementData("Good Father", "Good Father", "ACH_COLLECT_ROBOT", "STAT_HIGHIEST_ROBOT_PIECES", new List<int>
				{
					6
				}, EventRegistry.Achievements, TfEvent.Achievements.RobotPieces, new Func<AchievementData, object, bool>(Achievements.IntHighScore), null),
				new AchievementData("dont save the forest", "chop down 1000 trees", "ACH_CHOP_TREES_1000", "STAT_CHOPPED_TREES", new List<int>
				{
					100,
					1000
				}, EventRegistry.Player, TfEvent.CutTree, null, null),
				new AchievementData("Pacifist", "dont kill any cannibals for more than 10 days in a row", "ACH_DAYS_WITHOUT_KILLS", "STAT_DAYS_WITHOUT_KILLS", new List<int>
				{
					10
				}, EventRegistry.Player, TfEvent.SurvivedDay, new Func<AchievementData, object, bool>(Achievements.CountDaysWithoutKill), new Func<AchievementData, object, bool>(Achievements.SetupAchPacifist)),
				new AchievementData("Demolition expert", "knock down 6 or more trees with 1 stick of dynamite", "ACH_TREES_DYNAMITED", "STAT_HIGHIEST_TREES_DYNAMITED", new List<int>
				{
					6
				}, EventRegistry.Achievements, TfEvent.Achievements.TreeDynamited, new Func<AchievementData, object, bool>(Achievements.IntHighScore), null),
				new AchievementData("fisherman", "Catch fish with a trap", "ACH_TRAP_FISH", null, EventRegistry.Achievements, TfEvent.Achievements.FishTrapped, null, null)
			};
		}

		
		private static bool DaySurvivedAction(AchievementData ach, object o)
		{
			if (LocalPlayer.Stats)
			{
				AccountInfo.SetIntStat(ach, Mathf.FloorToInt(LocalPlayer.Stats.DaySurvived));
				return true;
			}
			return false;
		}

		
		private static bool CheckVegan(AchievementData ach, object o)
		{
			if (!AchievementsManager.FailedVegan)
			{
				AccountInfo.UnlockAchievement(ach);
				return true;
			}
			return false;
		}

		
		private static bool CheckBuildGazebo(AchievementData ach, object o)
		{
			if ((int)o == 370)
			{
				AccountInfo.UnlockAchievement(ach);
				return true;
			}
			return false;
		}

		
		private static bool CheckBuildTreeHouse(AchievementData ach, object o)
		{
			BuildingTypes buildingTypes = (BuildingTypes)((int)o);
			if (buildingTypes == BuildingTypes.TreeHouse || buildingTypes == BuildingTypes.TreeHouseAnchor || buildingTypes == BuildingTypes.TreeHouseChatel || buildingTypes == BuildingTypes.TreeHouseChatelAnchor)
			{
				AccountInfo.UnlockAchievement(ach);
				return true;
			}
			return false;
		}

		
		private static bool CheckMpAction(AchievementData ach, object o)
		{
			if (BoltNetwork.isRunning)
			{
				AccountInfo.UnlockAchievement(ach);
				return true;
			}
			return false;
		}

		
		private static bool CheckSeveralPlayersAction(AchievementData ach, object o)
		{
			if (BoltNetwork.isRunning && Scene.SceneTracker.allPlayerEntities.Count > 0)
			{
				AccountInfo.UnlockAchievement(ach);
				return true;
			}
			return false;
		}

		
		private static bool CheckHangingCutScene(AchievementData ach, object o)
		{
			GameStats.StoryElements storyElements = (GameStats.StoryElements)((int)o);
			if (storyElements == GameStats.StoryElements.HangingScene)
			{
				AccountInfo.UnlockAchievement(ach);
				return true;
			}
			return false;
		}

		
		private static bool IntHighScore(AchievementData ach, object o)
		{
			AccountInfo.SetIntStat(ach, (int)o);
			return true;
		}

		
		private static bool CheckSpendings(AchievementData ach, object o)
		{
			if (LocalPlayer.Achievements.BoughtSoda && LocalPlayer.Achievements.BoughtSnacks)
			{
				AccountInfo.UnlockAchievement(ach);
				return true;
			}
			return false;
		}

		
		private static bool CountDaysWithoutKill(AchievementData ach, object o)
		{
			int num = Mathf.FloorToInt(LocalPlayer.Stats.DaySurvived) - LocalPlayer.Achievements.LastKillSurvivedDays;
			AccountInfo.SetIntStat(ach, num);
			return num != 0;
		}

		
		private static bool SetupAchCrafty(AchievementData ach, object o)
		{
			Achievements.Crafty = ach;
			return true;
		}

		
		private static bool SetupAchTrophyHunter(AchievementData ach, object o)
		{
			Achievements.TrophyHunter = ach;
			return true;
		}

		
		private static bool SetupAchGardener(AchievementData ach, object o)
		{
			Achievements.Gardener = ach;
			return true;
		}

		
		private static bool SetupAchVegan(AchievementData ach, object o)
		{
			Achievements.Vegan = ach;
			return true;
		}

		
		private static bool SetupAchNaturopath(AchievementData ach, object o)
		{
			Achievements.Naturopath = ach;
			return true;
		}

		
		private static bool SetupAchSplatter(AchievementData ach, object o)
		{
			Achievements.Splatter = ach;
			return true;
		}

		
		private static bool SetupAchStealthArmor(AchievementData ach, object o)
		{
			Achievements.StealthArmor = ach;
			return true;
		}

		
		private static bool SetupAchVendingMachines(AchievementData ach, object o)
		{
			Achievements.VendingMachines = ach;
			return true;
		}

		
		private static bool SetupAchCollectCassettes(AchievementData ach, object o)
		{
			Achievements.CollectCassettes = ach;
			return true;
		}

		
		private static bool SetupAchEatAllMushrooms(AchievementData ach, object o)
		{
			Achievements.EatMushrooms = ach;
			return true;
		}

		
		private static bool SetupAchPacifist(AchievementData ach, object o)
		{
			Achievements.Pacifist = ach;
			return true;
		}

		
		public static bool ShowLogs;

		
		public static bool ShowExceptions = true;

		
		private static AchievementData[] Data;
	}
}
