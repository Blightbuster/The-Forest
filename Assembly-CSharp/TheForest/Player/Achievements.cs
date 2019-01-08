using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
			AchievementData[] array = new AchievementData[40];
			array[0] = new AchievementData("Major Cannibalism", "Eat an entire family", "ACH_MAJOR_CANNIBALISM", "STAT_CANNIBALISM", new List<int>
			{
				4,
				24
			}, EventRegistry.Achievements, TfEvent.Achievements.AteLimb, null, null);
			array[1] = new AchievementData("Be Nice", "Share a food or drink item in MP", "ACH_BE_NICE", null, EventRegistry.Achievements, TfEvent.Achievements.SharedEdible, null, null);
			array[2] = new AchievementData("Be Extremely nice", "Share a weapon in MP", "ACH_BE_EXTREMELY_NICE", null, EventRegistry.Achievements, TfEvent.Achievements.SharedWeapon, null, null);
			int num = 3;
			string name = "Crafty";
			string description = "Craft all items";
			string key = "ACH_CRAFTY";
			string statKey = "STAT_UNIQUE_CRAFTED_ITEMS";
			List<int> unlockValues = new List<int>
			{
				26
			};
			EventRegistry achievements = EventRegistry.Achievements;
			object craftedItemCount = TfEvent.Achievements.CraftedItemCount;
			if (Achievements.<>f__mg$cache0 == null)
			{
				Achievements.<>f__mg$cache0 = new Func<AchievementData, object, bool>(Achievements.IntHighScore);
			}
			Func<AchievementData, object, bool> customAction = Achievements.<>f__mg$cache0;
			if (Achievements.<>f__mg$cache1 == null)
			{
				Achievements.<>f__mg$cache1 = new Func<AchievementData, object, bool>(Achievements.SetupAchCrafty);
			}
			array[num] = new AchievementData(name, description, key, statKey, unlockValues, achievements, craftedItemCount, customAction, Achievements.<>f__mg$cache1);
			int num2 = 4;
			string name2 = "Bad father";
			string description2 = "Survive 100 days without finding your son";
			string key2 = "ACH_BAD_FATHER";
			string statKey2 = "STAT_DAYS_SURVIVED";
			List<int> unlockValues2 = new List<int>
			{
				1,
				5,
				100
			};
			EventRegistry player = EventRegistry.Player;
			object survivedDay = TfEvent.SurvivedDay;
			if (Achievements.<>f__mg$cache2 == null)
			{
				Achievements.<>f__mg$cache2 = new Func<AchievementData, object, bool>(Achievements.DaySurvivedAction);
			}
			array[num2] = new AchievementData(name2, description2, key2, statKey2, unlockValues2, player, survivedDay, Achievements.<>f__mg$cache2, null);
			int num3 = 5;
			string name3 = "Trophy Hunter";
			string description3 = "Kill all animal types and display heads";
			string key3 = "ACH_TROPHY_HUNTER";
			string statKey3 = "STAT_TROPHY_CREATED";
			List<int> unlockValues3 = new List<int>
			{
				11
			};
			EventRegistry achievements2 = EventRegistry.Achievements;
			object trophyCount = TfEvent.Achievements.TrophyCount;
			if (Achievements.<>f__mg$cache3 == null)
			{
				Achievements.<>f__mg$cache3 = new Func<AchievementData, object, bool>(Achievements.IntHighScore);
			}
			Func<AchievementData, object, bool> customAction2 = Achievements.<>f__mg$cache3;
			if (Achievements.<>f__mg$cache4 == null)
			{
				Achievements.<>f__mg$cache4 = new Func<AchievementData, object, bool>(Achievements.SetupAchTrophyHunter);
			}
			array[num3] = new AchievementData(name3, description3, key3, statKey3, unlockValues3, achievements2, trophyCount, customAction2, Achievements.<>f__mg$cache4);
			int num4 = 6;
			string name4 = "Gardener";
			string description4 = "Grow all plant types";
			string key4 = "ACH_GARDENER";
			string statKey4 = "STAT_PLANTED_SEEDS";
			List<int> unlockValues4 = new List<int>
			{
				3
			};
			EventRegistry achievements3 = EventRegistry.Achievements;
			object seedCount = TfEvent.Achievements.SeedCount;
			if (Achievements.<>f__mg$cache5 == null)
			{
				Achievements.<>f__mg$cache5 = new Func<AchievementData, object, bool>(Achievements.IntHighScore);
			}
			Func<AchievementData, object, bool> customAction3 = Achievements.<>f__mg$cache5;
			if (Achievements.<>f__mg$cache6 == null)
			{
				Achievements.<>f__mg$cache6 = new Func<AchievementData, object, bool>(Achievements.SetupAchGardener);
			}
			array[num4] = new AchievementData(name4, description4, key4, statKey4, unlockValues4, achievements3, seedCount, customAction3, Achievements.<>f__mg$cache6);
			int num5 = 7;
			string name5 = "Vegan";
			string description5 = "play through entire game without killing or eating animals";
			string key5 = "ACH_VEGAN";
			string statKey5 = null;
			EventRegistry endgame = EventRegistry.Endgame;
			object completed = TfEvent.Endgame.Completed;
			if (Achievements.<>f__mg$cache7 == null)
			{
				Achievements.<>f__mg$cache7 = new Func<AchievementData, object, bool>(Achievements.CheckVegan);
			}
			Func<AchievementData, object, bool> customAction4 = Achievements.<>f__mg$cache7;
			if (Achievements.<>f__mg$cache8 == null)
			{
				Achievements.<>f__mg$cache8 = new Func<AchievementData, object, bool>(Achievements.SetupAchVegan);
			}
			array[num5] = new AchievementData(name5, description5, key5, statKey5, endgame, completed, customAction4, Achievements.<>f__mg$cache8);
			int num6 = 8;
			string name6 = "Naturopath";
			string description6 = "craft 10 medicine items";
			string key6 = "ACH_NATUROPATH";
			string statKey6 = "STAT_CRAFTED_MEDS";
			List<int> unlockValues5 = new List<int>
			{
				10
			};
			EventRegistry achievements4 = EventRegistry.Achievements;
			object craftedMeds = TfEvent.Achievements.CraftedMeds;
			Func<AchievementData, object, bool> customAction5 = null;
			if (Achievements.<>f__mg$cache9 == null)
			{
				Achievements.<>f__mg$cache9 = new Func<AchievementData, object, bool>(Achievements.SetupAchNaturopath);
			}
			array[num6] = new AchievementData(name6, description6, key6, statKey6, unlockValues5, achievements4, craftedMeds, customAction5, Achievements.<>f__mg$cache9);
			array[9] = new AchievementData("5 Star hotel", "Sleep on yacht", "ACH_5_STAR_HOTEL", null, EventRegistry.Achievements, TfEvent.Achievements.SleptInYacht, null, null);
			int num7 = 10;
			string name7 = "Spelunker";
			string description7 = "Explore all caves";
			string key7 = "ACH_SPELUNKER";
			string statKey7 = "STAT_COMPLETED_CAVE_TASKS";
			List<int> unlockValues6 = new List<int>
			{
				10
			};
			EventRegistry achievements5 = EventRegistry.Achievements;
			object doneCaveTasks = TfEvent.Achievements.DoneCaveTasks;
			if (Achievements.<>f__mg$cacheA == null)
			{
				Achievements.<>f__mg$cacheA = new Func<AchievementData, object, bool>(Achievements.IntHighScore);
			}
			array[num7] = new AchievementData(name7, description7, key7, statKey7, unlockValues6, achievements5, doneCaveTasks, Achievements.<>f__mg$cacheA, null);
			array[11] = new AchievementData("Medic", "Revive 10 unique coop players", "ACH_MEDIC", "STAT_REVIVED_PLAYERS", new List<int>
			{
				1,
				10
			}, EventRegistry.Achievements, TfEvent.Achievements.RevivedPlayer, null, null);
			int num8 = 12;
			string name8 = "You should be looking for timmy";
			string description8 = "Build a gazebo";
			string key8 = "ACH_BUILD_GAZEBO";
			string statKey8 = null;
			EventRegistry player2 = EventRegistry.Player;
			object builtStructure = TfEvent.BuiltStructure;
			if (Achievements.<>f__mg$cacheB == null)
			{
				Achievements.<>f__mg$cacheB = new Func<AchievementData, object, bool>(Achievements.CheckBuildGazebo);
			}
			array[num8] = new AchievementData(name8, description8, key8, statKey8, player2, builtStructure, Achievements.<>f__mg$cacheB, null);
			array[13] = new AchievementData("Boy Scout", "Use the compass", "ACH_BOY_SCOOT", null, EventRegistry.Achievements, TfEvent.Achievements.UsingCompass, null, null);
			int num9 = 14;
			string name9 = "campout";
			string description9 = "Sleep with another player (group sleep)";
			string key9 = "ACH_GROUP_SLEEP";
			string statKey9 = null;
			EventRegistry player3 = EventRegistry.Player;
			object slept = TfEvent.Slept;
			if (Achievements.<>f__mg$cacheC == null)
			{
				Achievements.<>f__mg$cacheC = new Func<AchievementData, object, bool>(Achievements.CheckSeveralPlayersAction);
			}
			array[num9] = new AchievementData(name9, description9, key9, statKey9, player3, slept, Achievements.<>f__mg$cacheC, null);
			int num10 = 15;
			string name10 = "splatter";
			string description10 = "Killed downed enemy with rock";
			string key10 = "ACH_KILL_DOWNED_ENEMY";
			string statKey10 = null;
			EventRegistry achievements6 = EventRegistry.Achievements;
			object downedEnemyRockKill = TfEvent.Achievements.DownedEnemyRockKill;
			Func<AchievementData, object, bool> customAction6 = null;
			if (Achievements.<>f__mg$cacheD == null)
			{
				Achievements.<>f__mg$cacheD = new Func<AchievementData, object, bool>(Achievements.SetupAchSplatter);
			}
			array[num10] = new AchievementData(name10, description10, key10, statKey10, achievements6, downedEnemyRockKill, customAction6, Achievements.<>f__mg$cacheD);
			int num11 = 16;
			string name11 = "longest wall";
			string description11 = "Build a super long wall";
			string key11 = "ACH_LONG_WALL";
			string statKey11 = "STAT_LENGTHIEST_WALL";
			List<int> unlockValues7 = new List<int>
			{
				250
			};
			EventRegistry achievements7 = EventRegistry.Achievements;
			object placedWall = TfEvent.Achievements.PlacedWall;
			if (Achievements.<>f__mg$cacheE == null)
			{
				Achievements.<>f__mg$cacheE = new Func<AchievementData, object, bool>(Achievements.IntHighScore);
			}
			array[num11] = new AchievementData(name11, description11, key11, statKey11, unlockValues7, achievements7, placedWall, Achievements.<>f__mg$cacheE, null);
			array[17] = new AchievementData("choppy chop", "Chop up 50 bodies", "ACH_CHOP_BODIES", "STAT_CHOPPED_BODIES", new List<int>
			{
				50
			}, EventRegistry.Enemy, TfEvent.CutLimb, null, null);
			int num12 = 18;
			string name12 = "unseen";
			string description12 = "Make and wear stealth armour";
			string key12 = "ACH_STEALTH_ARMOR";
			string statKey12 = null;
			EventRegistry achievements8 = EventRegistry.Achievements;
			object usedStealthArmor = TfEvent.Achievements.UsedStealthArmor;
			Func<AchievementData, object, bool> customAction7 = null;
			if (Achievements.<>f__mg$cacheF == null)
			{
				Achievements.<>f__mg$cacheF = new Func<AchievementData, object, bool>(Achievements.SetupAchStealthArmor);
			}
			array[num12] = new AchievementData(name12, description12, key12, statKey12, achievements8, usedStealthArmor, customAction7, Achievements.<>f__mg$cacheF);
			int num13 = 19;
			string name13 = "fisherman";
			string description13 = "kill 6 or more fish with a stick of dynamite";
			string key13 = "ACH_FISH_DYNAMITED";
			string statKey13 = "STAT_FISH_DYNAMITED";
			List<int> unlockValues8 = new List<int>
			{
				6
			};
			EventRegistry achievements9 = EventRegistry.Achievements;
			object fishDynamited = TfEvent.Achievements.FishDynamited;
			if (Achievements.<>f__mg$cache10 == null)
			{
				Achievements.<>f__mg$cache10 = new Func<AchievementData, object, bool>(Achievements.IntHighScore);
			}
			array[num13] = new AchievementData(name13, description13, key13, statKey13, unlockValues8, achievements9, fishDynamited, Achievements.<>f__mg$cache10, null);
			array[20] = new AchievementData("Survive the forest", "Finish Game", "ACH_SURVIVE_THE_FOREST", null, EventRegistry.Endgame, TfEvent.Endgame.Completed, null, null);
			array[21] = new AchievementData("daily grind", "Drink from coffee machine", "ACH_DRINK_COFFEE", null, EventRegistry.Achievements, TfEvent.Achievements.DrinkCoffee, null, null);
			array[22] = new AchievementData("gross", "Drink water from cooler that has the lawyer head in it", "ACH_GROSS_DRINK", null, EventRegistry.Achievements, TfEvent.Achievements.GrossDrink, null, null);
			array[23] = new AchievementData("serial killer", "Kill 100 cannibals", "ACH_KILL_CANNIBALS", "STAT_KILLED_CANNIBALS", new List<int>
			{
				100
			}, EventRegistry.Enemy, TfEvent.KilledEnemy, null, null);
			array[24] = new AchievementData("make it rain", "Set off sprinklers in end game", "ACH_SPRINKLERS", null, EventRegistry.Endgame, TfEvent.Endgame.FireDetected, null, null);
			int num14 = 25;
			string name14 = "get closure";
			string description14 = "Find all missing passengers";
			string key14 = "ACH_FIND_PASSENGERS";
			string statKey14 = "STAT_FOUND_PASSENGERS";
			List<int> unlockValues9 = new List<int>
			{
				43
			};
			EventRegistry player4 = EventRegistry.Player;
			object foundPassenger = TfEvent.FoundPassenger;
			if (Achievements.<>f__mg$cache11 == null)
			{
				Achievements.<>f__mg$cache11 = new Func<AchievementData, object, bool>(Achievements.IntHighScore);
			}
			array[num14] = new AchievementData(name14, description14, key14, statKey14, unlockValues9, player4, foundPassenger, Achievements.<>f__mg$cache11, null);
			int num15 = 26;
			string name15 = "big spender";
			string description15 = "Buy a soda and candy from vending machine";
			string key15 = "ACH_VENDING_MACHINES";
			string statKey15 = null;
			EventRegistry achievements10 = EventRegistry.Achievements;
			object bigSpender = TfEvent.Achievements.BigSpender;
			if (Achievements.<>f__mg$cache12 == null)
			{
				Achievements.<>f__mg$cache12 = new Func<AchievementData, object, bool>(Achievements.CheckSpendings);
			}
			Func<AchievementData, object, bool> customAction8 = Achievements.<>f__mg$cache12;
			if (Achievements.<>f__mg$cache13 == null)
			{
				Achievements.<>f__mg$cache13 = new Func<AchievementData, object, bool>(Achievements.SetupAchVendingMachines);
			}
			array[num15] = new AchievementData(name15, description15, key15, statKey15, achievements10, bigSpender, customAction8, Achievements.<>f__mg$cache13);
			array[27] = new AchievementData("Bite me!", "Kill shark", "ACH_KILL_SHARK", null, EventRegistry.Animal, TfEvent.KilledShark, null, null);
			array[28] = new AchievementData("Monster", "Kill bunny", "ACH_KILL_BUNNY", null, EventRegistry.Animal, TfEvent.KilledRabbit, null, null);
			int num16 = 29;
			string name16 = "gabe fan";
			string description16 = "Collect all cassette tapes!";
			string key16 = "ACH_COLLECT_CASSETTES";
			string statKey16 = "STAT_COLLECTED_CASSETTES";
			List<int> unlockValues10 = new List<int>
			{
				5
			};
			EventRegistry achievements11 = EventRegistry.Achievements;
			object cassettes = TfEvent.Achievements.Cassettes;
			if (Achievements.<>f__mg$cache14 == null)
			{
				Achievements.<>f__mg$cache14 = new Func<AchievementData, object, bool>(Achievements.IntHighScore);
			}
			Func<AchievementData, object, bool> customAction9 = Achievements.<>f__mg$cache14;
			if (Achievements.<>f__mg$cache15 == null)
			{
				Achievements.<>f__mg$cache15 = new Func<AchievementData, object, bool>(Achievements.SetupAchCollectCassettes);
			}
			array[num16] = new AchievementData(name16, description16, key16, statKey16, unlockValues10, achievements11, cassettes, customAction9, Achievements.<>f__mg$cache15);
			int num17 = 30;
			string name17 = "You are a fun guy";
			string description17 = "Eat all mushroom types";
			string key17 = "ACH_EAT_ALL_MUSHROOMS";
			string statKey17 = "STAT_ATE_MUSHROOMS_TYPES";
			List<int> unlockValues11 = new List<int>
			{
				6
			};
			EventRegistry achievements12 = EventRegistry.Achievements;
			object ateMushrooms = TfEvent.Achievements.AteMushrooms;
			if (Achievements.<>f__mg$cache16 == null)
			{
				Achievements.<>f__mg$cache16 = new Func<AchievementData, object, bool>(Achievements.IntHighScore);
			}
			Func<AchievementData, object, bool> customAction10 = Achievements.<>f__mg$cache16;
			if (Achievements.<>f__mg$cache17 == null)
			{
				Achievements.<>f__mg$cache17 = new Func<AchievementData, object, bool>(Achievements.SetupAchEatAllMushrooms);
			}
			array[num17] = new AchievementData(name17, description17, key17, statKey17, unlockValues11, achievements12, ateMushrooms, customAction10, Achievements.<>f__mg$cache17);
			array[31] = new AchievementData("Archer", "kill bird with arrow", "ACH_SHOOT_BIRDS", null, EventRegistry.Achievements, TfEvent.Achievements.BirdArrowKill, null, null);
			int num18 = 32;
			string name18 = "fitbits";
			string description18 = "pass 50 000 steps";
			string key18 = "ACH_MARATHON_STEPS";
			string statKey18 = "STAT_TOTAL_WALKED_STEPS";
			List<int> unlockValues12 = new List<int>
			{
				50000
			};
			EventRegistry player5 = EventRegistry.Player;
			object walkedSteps = TfEvent.WalkedSteps;
			if (Achievements.<>f__mg$cache18 == null)
			{
				Achievements.<>f__mg$cache18 = new Func<AchievementData, object, bool>(Achievements.IntHighScore);
			}
			array[num18] = new AchievementData(name18, description18, key18, statKey18, unlockValues12, player5, walkedSteps, Achievements.<>f__mg$cache18, null);
			array[33] = new AchievementData("demolition man ", "Set off 20 bombs", "ACH_USED_BOMBS", "STAT_USED_BOMBS", new List<int>
			{
				20
			}, EventRegistry.Player, TfEvent.UsedBomb, null, null);
			array[34] = new AchievementData("handyman", "Repair a shelter", "ACH_REPAIR_SHELTER", null, EventRegistry.Achievements, TfEvent.Achievements.RepairedShelter, null, null);
			int num19 = 35;
			string name19 = "Good Father";
			string description19 = "Good Father";
			string key19 = "ACH_COLLECT_ROBOT";
			string statKey19 = "STAT_HIGHIEST_ROBOT_PIECES";
			List<int> unlockValues13 = new List<int>
			{
				6
			};
			EventRegistry achievements13 = EventRegistry.Achievements;
			object robotPieces = TfEvent.Achievements.RobotPieces;
			if (Achievements.<>f__mg$cache19 == null)
			{
				Achievements.<>f__mg$cache19 = new Func<AchievementData, object, bool>(Achievements.IntHighScore);
			}
			array[num19] = new AchievementData(name19, description19, key19, statKey19, unlockValues13, achievements13, robotPieces, Achievements.<>f__mg$cache19, null);
			array[36] = new AchievementData("dont save the forest", "chop down 1000 trees", "ACH_CHOP_TREES_1000", "STAT_CHOPPED_TREES", new List<int>
			{
				100,
				1000
			}, EventRegistry.Player, TfEvent.CutTree, null, null);
			int num20 = 37;
			string name20 = "Pacifist";
			string description20 = "dont kill any cannibals for more than 10 days in a row";
			string key20 = "ACH_DAYS_WITHOUT_KILLS";
			string statKey20 = "STAT_DAYS_WITHOUT_KILLS";
			List<int> unlockValues14 = new List<int>
			{
				10
			};
			EventRegistry player6 = EventRegistry.Player;
			object survivedDay2 = TfEvent.SurvivedDay;
			if (Achievements.<>f__mg$cache1A == null)
			{
				Achievements.<>f__mg$cache1A = new Func<AchievementData, object, bool>(Achievements.CountDaysWithoutKill);
			}
			Func<AchievementData, object, bool> customAction11 = Achievements.<>f__mg$cache1A;
			if (Achievements.<>f__mg$cache1B == null)
			{
				Achievements.<>f__mg$cache1B = new Func<AchievementData, object, bool>(Achievements.SetupAchPacifist);
			}
			array[num20] = new AchievementData(name20, description20, key20, statKey20, unlockValues14, player6, survivedDay2, customAction11, Achievements.<>f__mg$cache1B);
			int num21 = 38;
			string name21 = "Demolition expert";
			string description21 = "knock down 6 or more trees with 1 stick of dynamite";
			string key21 = "ACH_TREES_DYNAMITED";
			string statKey21 = "STAT_HIGHIEST_TREES_DYNAMITED";
			List<int> unlockValues15 = new List<int>
			{
				6
			};
			EventRegistry achievements14 = EventRegistry.Achievements;
			object treeDynamited = TfEvent.Achievements.TreeDynamited;
			if (Achievements.<>f__mg$cache1C == null)
			{
				Achievements.<>f__mg$cache1C = new Func<AchievementData, object, bool>(Achievements.IntHighScore);
			}
			array[num21] = new AchievementData(name21, description21, key21, statKey21, unlockValues15, achievements14, treeDynamited, Achievements.<>f__mg$cache1C, null);
			array[39] = new AchievementData("fisherman", "Catch fish with a trap", "ACH_TRAP_FISH", null, EventRegistry.Achievements, TfEvent.Achievements.FishTrapped, null, null);
			Achievements.Data = array;
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
			if ((BuildingTypes)o == BuildingTypes.Gazeebo)
			{
				AccountInfo.UnlockAchievement(ach);
				return true;
			}
			return false;
		}

		private static bool CheckBuildTreeHouse(AchievementData ach, object o)
		{
			BuildingTypes buildingTypes = (BuildingTypes)o;
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
			GameStats.StoryElements storyElements = (GameStats.StoryElements)o;
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

		[CompilerGenerated]
		private static Func<AchievementData, object, bool> <>f__mg$cache0;

		[CompilerGenerated]
		private static Func<AchievementData, object, bool> <>f__mg$cache1;

		[CompilerGenerated]
		private static Func<AchievementData, object, bool> <>f__mg$cache2;

		[CompilerGenerated]
		private static Func<AchievementData, object, bool> <>f__mg$cache3;

		[CompilerGenerated]
		private static Func<AchievementData, object, bool> <>f__mg$cache4;

		[CompilerGenerated]
		private static Func<AchievementData, object, bool> <>f__mg$cache5;

		[CompilerGenerated]
		private static Func<AchievementData, object, bool> <>f__mg$cache6;

		[CompilerGenerated]
		private static Func<AchievementData, object, bool> <>f__mg$cache7;

		[CompilerGenerated]
		private static Func<AchievementData, object, bool> <>f__mg$cache8;

		[CompilerGenerated]
		private static Func<AchievementData, object, bool> <>f__mg$cache9;

		[CompilerGenerated]
		private static Func<AchievementData, object, bool> <>f__mg$cacheA;

		[CompilerGenerated]
		private static Func<AchievementData, object, bool> <>f__mg$cacheB;

		[CompilerGenerated]
		private static Func<AchievementData, object, bool> <>f__mg$cacheC;

		[CompilerGenerated]
		private static Func<AchievementData, object, bool> <>f__mg$cacheD;

		[CompilerGenerated]
		private static Func<AchievementData, object, bool> <>f__mg$cacheE;

		[CompilerGenerated]
		private static Func<AchievementData, object, bool> <>f__mg$cacheF;

		[CompilerGenerated]
		private static Func<AchievementData, object, bool> <>f__mg$cache10;

		[CompilerGenerated]
		private static Func<AchievementData, object, bool> <>f__mg$cache11;

		[CompilerGenerated]
		private static Func<AchievementData, object, bool> <>f__mg$cache12;

		[CompilerGenerated]
		private static Func<AchievementData, object, bool> <>f__mg$cache13;

		[CompilerGenerated]
		private static Func<AchievementData, object, bool> <>f__mg$cache14;

		[CompilerGenerated]
		private static Func<AchievementData, object, bool> <>f__mg$cache15;

		[CompilerGenerated]
		private static Func<AchievementData, object, bool> <>f__mg$cache16;

		[CompilerGenerated]
		private static Func<AchievementData, object, bool> <>f__mg$cache17;

		[CompilerGenerated]
		private static Func<AchievementData, object, bool> <>f__mg$cache18;

		[CompilerGenerated]
		private static Func<AchievementData, object, bool> <>f__mg$cache19;

		[CompilerGenerated]
		private static Func<AchievementData, object, bool> <>f__mg$cache1A;

		[CompilerGenerated]
		private static Func<AchievementData, object, bool> <>f__mg$cache1B;

		[CompilerGenerated]
		private static Func<AchievementData, object, bool> <>f__mg$cache1C;
	}
}
