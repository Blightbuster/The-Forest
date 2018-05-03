using System;
using System.Collections;
using TheForest.Buildings.Creation;
using TheForest.Commons.Enums;
using TheForest.Items.Inventory;
using TheForest.TaskSystem;
using TheForest.Tools;
using TheForest.UI;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Player
{
	
	[DoNotSerializePublic]
	public class SurvivalBookTodo : MonoBehaviour
	{
		
		private void Awake()
		{
			base.enabled = false;
			if (GameSetup.Init == InitTypes.New)
			{
				base.StartCoroutine(this.DelayedAwake());
			}
		}

		
		private IEnumerator DelayedAwake()
		{
			if (!this._initialized)
			{
				this._initialized = true;
				while (!Scene.SceneTracker || LevelSerializer.IsDeserializing)
				{
					yield return null;
				}
				yield return null;
				this._son.Prepare(this._sonGOs, new Action(this.OnStatusChange));
				this._camp.Prepare(this._campGOs, new Action(this.OnStatusChange));
				this._food.Prepare(this._foodGOs, new Action(this.OnStatusChange));
				this._defenses.Prepare(this._defensesGOs, (!this._defenses._available) ? new Action(this.OnDefenseTaskAvailable) : new Action(this.OnStatusChange));
				this._redman.Prepare(this._redmanGOs, new Action(this.OnStatusChange));
				this._cave1.Prepare(this._cave1GOs, new Action(this.OnCaveTaskStatusChange));
				this._cave2.Prepare(this._cave2GOs, new Action(this.OnCaveTaskStatusChange));
				this._cave3.Prepare(this._cave3GOs, new Action(this.OnCaveTaskStatusChange));
				this._cave4.Prepare(this._cave4GOs, new Action(this.OnCaveTaskStatusChange));
				this._cave5.Prepare(this._cave5GOs, new Action(this.OnCaveTaskStatusChange));
				this._cave6.Prepare(this._cave6GOs, new Action(this.OnCaveTaskStatusChange));
				this._cave7.Prepare(this._cave7GOs, new Action(this.OnCaveTaskStatusChange));
				this._cave8.Prepare(this._cave8GOs, new Action(this.OnCaveTaskStatusChange));
				this._cave9.Prepare(this._cave9GOs, new Action(this.OnCaveTaskStatusChange));
				this._cave10.Prepare(this._cave10GOs, new Action(this.OnCaveTaskStatusChange));
				this._findClimbingAxe.Prepare(this._findClimbingAxeGOs, new Action(this.OnStatusChange));
				this._findRebreather.Prepare(this._findRebreatherGOs, new Action(this.OnStatusChange));
				this._sinkHole.Prepare(this._sinkHoleGOs, new Action(this.OnStatusChange));
				this._passengers.Prepare(this._passengersGOs, new Action(this.OnStatusChange));
				this._megan.Prepare(this._meganGOs, new Action(this.OnStatusChange));
				this._sacrifice.Prepare(this._sacrificeGOs, new Action(this.OnStatusChange));
				if (GameSetup.Init == InitTypes.New)
				{
					while (LocalPlayer.Inventory.CurrentView < PlayerInventory.PlayerViews.World)
					{
						yield return null;
					}
					yield return YieldPresets.WaitTenSeconds;
					this._son.SetAvailable();
					this._camp.SetAvailable();
					this._todoListTab.Highlight(null);
					this._statsListTab.Highlight(null);
				}
				base.enabled = true;
			}
			yield break;
		}

		
		public void OnEnable()
		{
			int displayedElementNum = 0;
			displayedElementNum = this.ToggleDisplay(this._son, displayedElementNum);
			displayedElementNum = this.ToggleDisplay(this._camp, displayedElementNum);
			displayedElementNum = this.ToggleDisplay(this._food, displayedElementNum);
			displayedElementNum = this.ToggleDisplay(this._defenses, displayedElementNum);
			displayedElementNum = this.ToggleDisplay(this._redman, displayedElementNum);
			displayedElementNum = this.ToggleDisplay(this._findClimbingAxe, displayedElementNum);
			displayedElementNum = this.ToggleDisplay(this._findRebreather, displayedElementNum);
			displayedElementNum = this.ToggleDisplay(this._sinkHole, displayedElementNum);
			displayedElementNum = this.ToggleDisplay(this._passengers, displayedElementNum);
			displayedElementNum = this.ToggleDisplay(this._megan, displayedElementNum);
			this.ToggleDisplay(this._sacrifice, displayedElementNum);
			displayedElementNum = 0;
			displayedElementNum = this.ToggleDisplay(this._cave1, displayedElementNum);
			displayedElementNum = this.ToggleDisplay(this._cave2, displayedElementNum);
			displayedElementNum = this.ToggleDisplay(this._cave3, displayedElementNum);
			displayedElementNum = this.ToggleDisplay(this._cave4, displayedElementNum);
			displayedElementNum = this.ToggleDisplay(this._cave5, displayedElementNum);
			displayedElementNum = this.ToggleDisplay(this._cave6, displayedElementNum);
			displayedElementNum = this.ToggleDisplay(this._cave7, displayedElementNum);
			displayedElementNum = this.ToggleDisplay(this._cave8, displayedElementNum);
			displayedElementNum = this.ToggleDisplay(this._cave9, displayedElementNum);
			displayedElementNum = this.ToggleDisplay(this._cave10, displayedElementNum);
		}

		
		private void OnDestroy()
		{
			this._son.Clear();
			this._camp.Clear();
			this._food.Clear();
			this._defenses.Clear();
			this._redman.Clear();
			this._cave1.Clear();
			this._cave2.Clear();
			this._cave5.Clear();
			this._findClimbingAxe.Clear();
			this._findRebreather.Clear();
			this._sinkHole.Clear();
			this._passengers.Clear();
			this._megan.Clear();
			this._sacrifice.Clear();
		}

		
		private IEnumerator OnDeserialized()
		{
			yield return null;
			base.StartCoroutine(this.DelayedAwake());
			if (LocalPlayer.SavedData.ExitedEndgame)
			{
				this.StrikeThroughAll();
			}
			yield break;
		}

		
		public void Clone(SurvivalBookTodo other)
		{
			this._son.Clone(other._son);
			this._camp.Clone(other._camp);
			this._food.Clone(other._food);
			this._defenses.Clone(other._defenses);
			this._redman.Clone(other._redman);
			this._cave1.Clone(other._cave1);
			this._cave2.Clone(other._cave2);
			this._cave5.Clone(other._cave5);
			this._findClimbingAxe.Clone(other._findClimbingAxe);
			this._findRebreather.Clone(other._findRebreather);
			this._sinkHole.Clone(other._sinkHole);
			this._passengers.Clone(other._passengers);
			this._megan.Clone(other._megan);
			this._sacrifice.Clone(other._sacrifice);
			base.StartCoroutine(this.DelayedAwake());
		}

		
		public void StrikeThroughAll()
		{
			this._son._done = true;
			this._camp._done = true;
			this._food._done = true;
			this._defenses._done = true;
			this._redman._done = true;
			this._cave1._done = true;
			this._cave2._done = true;
			this._cave5._done = true;
			this._findClimbingAxe._done = true;
			this._findRebreather._done = true;
			this._sinkHole._done = true;
			this._passengers._done = true;
			this._megan._done = true;
			this._sacrifice._done = true;
		}

		
		private int ToggleDisplay(SurvivalBookTodo.TodoTask elem, int displayedElementNum)
		{
			if (elem._available)
			{
				if (elem.DisplayedNum != displayedElementNum)
				{
					elem.DisplayedNum = displayedElementNum;
					Vector3 localPosition = elem.GOs._text.transform.localPosition;
					localPosition.y = this._displayOffset * (float)displayedElementNum;
					elem.GOs._text.transform.localPosition = localPosition;
				}
				if (!elem.GOs._text.activeSelf)
				{
					elem.GOs._text.SetActive(true);
				}
				if (elem._done)
				{
					if (!elem.GOs._done.activeSelf)
					{
						elem.GOs._done.SetActive(true);
					}
				}
				else if (elem.GOs._done.activeSelf)
				{
					elem.GOs._done.SetActive(false);
				}
				displayedElementNum += ((!elem.GOs._text.GetComponent<TextMesh>().text.Contains("\n")) ? 1 : 2);
			}
			else if (elem.GOs._text.activeSelf)
			{
				elem.GOs._text.SetActive(false);
			}
			return displayedElementNum;
		}

		
		private void OnStatusChange()
		{
			this._todoListTab.Highlight(null);
		}

		
		private void OnCaveTaskStatusChange()
		{
			int num = 0;
			if (this._cave1._done)
			{
				num++;
			}
			if (this._cave2._done)
			{
				num++;
			}
			if (this._cave3._done)
			{
				num++;
			}
			if (this._cave4._done)
			{
				num++;
			}
			if (this._cave5._done)
			{
				num++;
			}
			if (this._cave6._done)
			{
				num++;
			}
			if (this._cave7._done)
			{
				num++;
			}
			if (this._cave8._done)
			{
				num++;
			}
			if (this._cave9._done)
			{
				num++;
			}
			if (this._cave10._done)
			{
				num++;
			}
			EventRegistry.Achievements.Publish(TfEvent.Achievements.DoneCaveTasks, num);
			this.OnStatusChange();
		}

		
		private void OnDefenseTaskAvailable()
		{
			this._defenses.OnStatusChange = new Action(this.OnStatusChange);
			this.OnStatusChange();
			this._trapsPageTab.Highlight(null);
		}

		
		public SelectPageNumber _todoListTab;

		
		public SelectPageNumber _statsListTab;

		
		public SelectPageNumber _trapsPageTab;

		
		[SerializeThis]
		public SurvivalBookTodo.FindTimmyTodoTask _son;

		
		public SurvivalBookTodo.TodoEntryGOs _sonGOs;

		
		[SerializeThis]
		public SurvivalBookTodo.CampTodoTask _camp;

		
		public SurvivalBookTodo.TodoEntryGOs _campGOs;

		
		[SerializeThis]
		public SurvivalBookTodo.FoodTodoTask _food;

		
		public SurvivalBookTodo.TodoEntryGOs _foodGOs;

		
		[SerializeThis]
		public SurvivalBookTodo.DefensesTodoTask _defenses;

		
		public SurvivalBookTodo.TodoEntryGOs _defensesGOs;

		
		[SerializeThis]
		public SurvivalBookTodo.TodoTask _redman;

		
		public SurvivalBookTodo.TodoEntryGOs _redmanGOs;

		
		[SerializeThis]
		public SurvivalBookTodo.Cave1TodoTask _cave1;

		
		public SurvivalBookTodo.TodoEntryGOs _cave1GOs;

		
		[SerializeThis]
		public SurvivalBookTodo.Cave2TodoTask _cave2;

		
		public SurvivalBookTodo.TodoEntryGOs _cave2GOs;

		
		[SerializeThis]
		public SurvivalBookTodo.Cave3TodoTask _cave3;

		
		public SurvivalBookTodo.TodoEntryGOs _cave3GOs;

		
		[SerializeThis]
		public SurvivalBookTodo.Cave4TodoTask _cave4;

		
		public SurvivalBookTodo.TodoEntryGOs _cave4GOs;

		
		[SerializeThis]
		public SurvivalBookTodo.Cave5TodoTask _cave5;

		
		public SurvivalBookTodo.TodoEntryGOs _cave5GOs;

		
		[SerializeThis]
		public SurvivalBookTodo.Cave6TodoTask _cave6;

		
		public SurvivalBookTodo.TodoEntryGOs _cave6GOs;

		
		[SerializeThis]
		public SurvivalBookTodo.Cave7TodoTask _cave7;

		
		public SurvivalBookTodo.TodoEntryGOs _cave7GOs;

		
		[SerializeThis]
		public SurvivalBookTodo.Cave8TodoTask _cave8;

		
		public SurvivalBookTodo.TodoEntryGOs _cave8GOs;

		
		[SerializeThis]
		public SurvivalBookTodo.Cave9TodoTask _cave9;

		
		public SurvivalBookTodo.TodoEntryGOs _cave9GOs;

		
		[SerializeThis]
		public SurvivalBookTodo.Cave10TodoTask _cave10;

		
		public SurvivalBookTodo.TodoEntryGOs _cave10GOs;

		
		[SerializeThis]
		public SurvivalBookTodo.FindClimbingAxeTodoTask _findClimbingAxe;

		
		public SurvivalBookTodo.TodoEntryGOs _findClimbingAxeGOs;

		
		[SerializeThis]
		public SurvivalBookTodo.FindRebreatherTodoTask _findRebreather;

		
		public SurvivalBookTodo.TodoEntryGOs _findRebreatherGOs;

		
		[SerializeThis]
		public SurvivalBookTodo.SinkHoleTodoTask _sinkHole;

		
		public SurvivalBookTodo.TodoEntryGOs _sinkHoleGOs;

		
		[SerializeThis]
		public SurvivalBookTodo.PassengersTodoTask _passengers;

		
		public SurvivalBookTodo.TodoEntryGOs _passengersGOs;

		
		[SerializeThis]
		public SurvivalBookTodo.FindMeganTodoTask _megan;

		
		public SurvivalBookTodo.TodoEntryGOs _meganGOs;

		
		[SerializeThis]
		public SurvivalBookTodo.SacrificeTodoTask _sacrifice;

		
		public SurvivalBookTodo.TodoEntryGOs _sacrificeGOs;

		
		public float _displayOffset = 1f;

		
		private bool _initialized;

		
		[DoNotSerializePublic]
		[Serializable]
		public class TodoEntryGOs
		{
			
			public GameObject _text;

			
			public GameObject _done;
		}

		
		[DoNotSerializePublic]
		[Serializable]
		public class TodoTask : Task
		{
			
			
			
			public SurvivalBookTodo.TodoEntryGOs GOs { get; set; }

			
			
			
			public int DisplayedNum { get; set; }

			
			public void Prepare(SurvivalBookTodo.TodoEntryGOs gos, Action onStatusChange)
			{
				this.GOs = gos;
				this.FixSerializer();
				if (this._allowInMp || !BoltNetwork.isRunning)
				{
					this.OnStatusChange = onStatusChange;
					this.Init();
				}
				this.DisplayedNum = -1;
			}

			
			public virtual void FixSerializer()
			{
			}

			
			public override void SetAvailable()
			{
				if (!this._available)
				{
					this.AvailableMessage();
					base.SetAvailable();
				}
			}

			
			public override void SetDone()
			{
				if (!this._done)
				{
					this.DoneMessage();
					base.SetDone();
				}
			}

			
			public void LogMessage(string message)
			{
				Scene.HudGui.ShowTodoListMessage(message);
			}

			
			public void AvailableMessage()
			{
				this.LogMessage(UiTranslationDatabase.TranslateKey("TODO_LIST_AVAILABLE_MESSAGE", "new item added to todo list", true));
				LocalPlayer.Sfx.PlayTaskAvailable();
			}

			
			public void DoneMessage()
			{
				this.LogMessage(UiTranslationDatabase.TranslateKey("TODO_LIST_DONE_MESSAGE", "to do list updated", true));
				LocalPlayer.Sfx.PlayTaskCompleted();
			}

			
			public virtual void Clone(SurvivalBookTodo.TodoTask other)
			{
				this.DisplayedNum = other.DisplayedNum;
				base.Clone(other);
			}
		}

		
		[DoNotSerializePublic]
		[Serializable]
		public class CampTodoTask : SurvivalBookTodo.TodoTask
		{
			
			
			public override ACondition _completeCondition
			{
				get
				{
					return this._completeConditionStorage;
				}
			}

			
			public override void FixSerializer()
			{
				if (this._completeConditionStorage == null || this._completeConditionStorage._buildings == null)
				{
					this._completeConditionStorage = new BuildingCondition
					{
						_buildings = new BuildingTypeList[]
						{
							new BuildingTypeList
							{
								_types = new BuildingTypes[]
								{
									BuildingTypes.LeafShelter,
									BuildingTypes.Shelter,
									BuildingTypes.LogCabinMed,
									BuildingTypes.LogCabin
								}
							}
						}
					};
				}
				base.FixSerializer();
			}

			
			public virtual void Clone(SurvivalBookTodo.CampTodoTask other)
			{
				this._completeConditionStorage = other._completeConditionStorage;
				base.Clone(other);
			}

			
			[SerializeThis]
			public BuildingCondition _completeConditionStorage;
		}

		
		[DoNotSerializePublic]
		[Serializable]
		public class FoodTodoTask : SurvivalBookTodo.TodoTask
		{
			
			
			public override ACondition _availableCondition
			{
				get
				{
					return this._availableConditionStorage;
				}
			}

			
			
			public override ACondition _completeCondition
			{
				get
				{
					return this._completeConditionStorage;
				}
			}

			
			public override void FixSerializer()
			{
				bool done = this._availableConditionStorage != null && this._availableConditionStorage._done;
				if (this._availableConditionStorage == null || this._availableConditionStorage._buildings == null)
				{
					this._availableConditionStorage = new BuildingCondition
					{
						_buildings = new BuildingTypeList[]
						{
							new BuildingTypeList
							{
								_types = new BuildingTypes[]
								{
									BuildingTypes.Fire,
									BuildingTypes.FireRockPit,
									BuildingTypes.BonFire
								}
							}
						},
						_done = done
					};
				}
				if (this._completeConditionStorage == null)
				{
					this._completeConditionStorage = new CookFoodCondition();
				}
				base.FixSerializer();
			}

			
			public virtual void Clone(SurvivalBookTodo.FoodTodoTask other)
			{
				this._availableConditionStorage = other._availableConditionStorage;
				this._completeConditionStorage = other._completeConditionStorage;
				base.Clone(other);
			}

			
			[SerializeThis]
			public BuildingCondition _availableConditionStorage;

			
			[SerializeThis]
			public CookFoodCondition _completeConditionStorage;
		}

		
		[DoNotSerializePublic]
		[Serializable]
		public class DefensesTodoTask : SurvivalBookTodo.TodoTask
		{
			
			
			public override ACondition _availableCondition
			{
				get
				{
					return this._availableConditionStorage;
				}
			}

			
			
			public override ACondition _completeCondition
			{
				get
				{
					return this._completeConditionStorage;
				}
			}

			
			public override void FixSerializer()
			{
				if (this._availableConditionStorage == null)
				{
					this._availableConditionStorage = new EnemyContactCondition();
				}
				if (this._completeConditionStorage == null || this._completeConditionStorage._buildings == null)
				{
					this._completeConditionStorage = new BuildingCondition
					{
						_buildings = new BuildingTypeList[]
						{
							new BuildingTypeList
							{
								_types = new BuildingTypes[]
								{
									BuildingTypes.TrapDeadfall,
									BuildingTypes.TrapPole,
									BuildingTypes.TrapRope,
									BuildingTypes.TrapSimple,
									BuildingTypes.TrapSpikedWall,
									BuildingTypes.TrapTripWireExplosive,
									BuildingTypes.TrapTripWireMolotov,
									BuildingTypes.TrapLeafPile,
									BuildingTypes.TrapSwingingRock
								}
							}
						}
					};
				}
				base.FixSerializer();
			}

			
			public virtual void Clone(SurvivalBookTodo.DefensesTodoTask other)
			{
				this._availableConditionStorage = other._availableConditionStorage;
				this._completeConditionStorage = other._completeConditionStorage;
				base.Clone(other);
			}

			
			[SerializeThis]
			public EnemyContactCondition _availableConditionStorage;

			
			[SerializeThis]
			public BuildingCondition _completeConditionStorage;
		}

		
		[DoNotSerializePublic]
		[Serializable]
		public class RedmanTodoTask : SurvivalBookTodo.TodoTask
		{
			
			
			public override ACondition _availableCondition
			{
				get
				{
					return this._availableConditionStorage;
				}
			}

			
			public override void FixSerializer()
			{
				bool done = this._availableConditionStorage != null && this._availableConditionStorage._done;
				if (this._availableConditionStorage == null || this._availableConditionStorage._type == (GameStats.StoryElements)0)
				{
					this._availableConditionStorage = new StoryCondition
					{
						_type = GameStats.StoryElements.RedManOnYacht,
						_done = done
					};
				}
				base.FixSerializer();
			}

			
			public virtual void Clone(SurvivalBookTodo.RedmanTodoTask other)
			{
				this._availableConditionStorage = other._availableConditionStorage;
				base.Clone(other);
			}

			
			[SerializeThis]
			public StoryCondition _availableConditionStorage;
		}

		
		[DoNotSerializePublic]
		[Serializable]
		public class FindTimmyTodoTask : SurvivalBookTodo.TodoTask
		{
			
			
			public override ACondition _completeCondition
			{
				get
				{
					return this._completeConditionStorage;
				}
			}

			
			public override void FixSerializer()
			{
				if (this._completeConditionStorage == null || this._completeConditionStorage._type == (GameStats.StoryElements)0)
				{
					this._completeConditionStorage = new StoryCondition
					{
						_type = GameStats.StoryElements.TimmyFound
					};
				}
				base.FixSerializer();
			}

			
			public virtual void Clone(SurvivalBookTodo.FindTimmyTodoTask other)
			{
				this._completeConditionStorage = other._completeConditionStorage;
				base.Clone(other);
			}

			
			[SerializeThis]
			public StoryCondition _completeConditionStorage;
		}

		
		[DoNotSerializePublic]
		[Serializable]
		public class FindMeganTodoTask : SurvivalBookTodo.TodoTask
		{
			
			
			public override ACondition _availableCondition
			{
				get
				{
					return this._availableConditionStorage;
				}
			}

			
			
			public override ACondition _completeCondition
			{
				get
				{
					return this._completeConditionStorage;
				}
			}

			
			public override void FixSerializer()
			{
				bool done = this._availableConditionStorage != null && this._availableConditionStorage._done;
				if (this._availableConditionStorage == null || this._availableConditionStorage._type == (GameStats.StoryElements)0)
				{
					this._availableConditionStorage = new StoryCondition
					{
						_type = GameStats.StoryElements.TimmyFound,
						_done = done
					};
				}
				if (this._completeConditionStorage == null || this._completeConditionStorage._type == (GameStats.StoryElements)0)
				{
					this._completeConditionStorage = new StoryCondition
					{
						_type = GameStats.StoryElements.MeganFound
					};
				}
				base.FixSerializer();
			}

			
			public virtual void Clone(SurvivalBookTodo.FindMeganTodoTask other)
			{
				this._availableConditionStorage = other._availableConditionStorage;
				this._completeConditionStorage = other._completeConditionStorage;
				base.Clone(other);
			}

			
			[SerializeThis]
			public StoryCondition _availableConditionStorage;

			
			[SerializeThis]
			public StoryCondition _completeConditionStorage;
		}

		
		[DoNotSerializePublic]
		[Serializable]
		public class SacrificeTodoTask : SurvivalBookTodo.TodoTask
		{
			
			
			public override ACondition _availableCondition
			{
				get
				{
					return this._availableConditionStorage;
				}
			}

			
			public override void FixSerializer()
			{
				bool done = this._availableConditionStorage != null && this._availableConditionStorage._done;
				if (this._availableConditionStorage == null || this._availableConditionStorage._type == (GameStats.StoryElements)0)
				{
					this._availableConditionStorage = new StoryCondition
					{
						_type = GameStats.StoryElements.MeganFound,
						_done = done
					};
				}
				base.FixSerializer();
			}

			
			public virtual void Clone(SurvivalBookTodo.SacrificeTodoTask other)
			{
				this._availableConditionStorage = other._availableConditionStorage;
				base.Clone(other);
			}

			
			[SerializeThis]
			public StoryCondition _availableConditionStorage;
		}

		
		[DoNotSerializePublic]
		[Serializable]
		public class Cave1TodoTask : SurvivalBookTodo.TodoTask
		{
			
			
			public override ACondition _availableCondition
			{
				get
				{
					return this._availableConditionStorage;
				}
			}

			
			
			public override ACondition _completeCondition
			{
				get
				{
					return this._completeConditionStorage;
				}
			}

			
			public override void FixSerializer()
			{
				bool done = this._availableConditionStorage != null && this._availableConditionStorage._done;
				if (this._availableConditionStorage == null || string.IsNullOrEmpty(this._availableConditionStorage._objectTag))
				{
					this._availableConditionStorage = new ProximityCondition
					{
						_objectTag = "Cave1Door",
						_isTag = false,
						_use2dDistance = true,
						_distance = 10f,
						_done = done
					};
				}
				if (this._completeConditionStorage == null || this._completeConditionStorage._caveNumber == -1)
				{
					this._completeConditionStorage = new ExploredCaveCondition
					{
						_caveNumber = 0
					};
				}
				base.FixSerializer();
			}

			
			public virtual void Clone(SurvivalBookTodo.Cave1TodoTask other)
			{
				this._availableConditionStorage = other._availableConditionStorage;
				this._completeConditionStorage = other._completeConditionStorage;
				base.Clone(other);
			}

			
			[SerializeThis]
			public ProximityCondition _availableConditionStorage;

			
			[SerializeThis]
			public ExploredCaveCondition _completeConditionStorage;
		}

		
		[DoNotSerializePublic]
		[Serializable]
		public class Cave2TodoTask : SurvivalBookTodo.TodoTask
		{
			
			
			public override ACondition _availableCondition
			{
				get
				{
					return this._availableConditionStorage;
				}
			}

			
			
			public override ACondition _completeCondition
			{
				get
				{
					return this._completeConditionStorage;
				}
			}

			
			public override void FixSerializer()
			{
				bool done = this._availableConditionStorage != null && this._availableConditionStorage._done;
				if (this._availableConditionStorage == null || string.IsNullOrEmpty(this._availableConditionStorage._objectTag))
				{
					this._availableConditionStorage = new ProximityCondition
					{
						_objectTag = "Cave2Door",
						_isTag = false,
						_use2dDistance = true,
						_distance = 10f,
						_done = done
					};
				}
				if (this._completeConditionStorage == null || this._completeConditionStorage._caveNumber == -1)
				{
					this._completeConditionStorage = new ExploredCaveCondition
					{
						_caveNumber = 1
					};
				}
				base.FixSerializer();
			}

			
			public virtual void Clone(SurvivalBookTodo.Cave2TodoTask other)
			{
				this._availableConditionStorage = other._availableConditionStorage;
				this._completeConditionStorage = other._completeConditionStorage;
				base.Clone(other);
			}

			
			[SerializeThis]
			public ProximityCondition _availableConditionStorage;

			
			[SerializeThis]
			public ExploredCaveCondition _completeConditionStorage;
		}

		
		[DoNotSerializePublic]
		[Serializable]
		public class Cave3TodoTask : SurvivalBookTodo.TodoTask
		{
			
			
			public override ACondition _availableCondition
			{
				get
				{
					return this._availableConditionStorage;
				}
			}

			
			
			public override ACondition _completeCondition
			{
				get
				{
					return this._completeConditionStorage;
				}
			}

			
			public override void FixSerializer()
			{
				bool done = this._availableConditionStorage != null && this._availableConditionStorage._done;
				if (this._availableConditionStorage == null || string.IsNullOrEmpty(this._availableConditionStorage._objectTag))
				{
					this._availableConditionStorage = new ProximityCondition
					{
						_objectTag = "Cave6Door3",
						_isTag = false,
						_use2dDistance = true,
						_distance = 20f,
						_inCaveOnly = true,
						_done = done
					};
				}
				if (this._completeConditionStorage == null || this._completeConditionStorage._caveNumber == -1)
				{
					this._completeConditionStorage = new ExploredCaveCondition
					{
						_caveNumber = 2
					};
				}
				base.FixSerializer();
			}

			
			public virtual void Clone(SurvivalBookTodo.Cave3TodoTask other)
			{
				this._availableConditionStorage = other._availableConditionStorage;
				this._completeConditionStorage = other._completeConditionStorage;
				base.Clone(other);
			}

			
			[SerializeThis]
			public ProximityCondition _availableConditionStorage;

			
			[SerializeThis]
			public ExploredCaveCondition _completeConditionStorage;
		}

		
		[DoNotSerializePublic]
		[Serializable]
		public class Cave4TodoTask : SurvivalBookTodo.TodoTask
		{
			
			
			public override ACondition _availableCondition
			{
				get
				{
					return this._availableConditionStorage;
				}
			}

			
			
			public override ACondition _completeCondition
			{
				get
				{
					return this._completeConditionStorage;
				}
			}

			
			public override void FixSerializer()
			{
				bool done = this._availableConditionStorage != null && this._availableConditionStorage._done;
				if (this._availableConditionStorage == null || string.IsNullOrEmpty(this._availableConditionStorage._objectTag))
				{
					this._availableConditionStorage = new ProximityCondition
					{
						_objectTag = "Cave4ClimbEntrance_Altexit",
						_isTag = false,
						_use2dDistance = true,
						_distance = 10f,
						_inCaveOnly = true,
						_done = done
					};
				}
				if (this._completeConditionStorage == null || this._completeConditionStorage._caveNumber == -1)
				{
					this._completeConditionStorage = new ExploredCaveCondition
					{
						_caveNumber = 3
					};
				}
				base.FixSerializer();
			}

			
			public virtual void Clone(SurvivalBookTodo.Cave4TodoTask other)
			{
				this._availableConditionStorage = other._availableConditionStorage;
				this._completeConditionStorage = other._completeConditionStorage;
				base.Clone(other);
			}

			
			[SerializeThis]
			public ProximityCondition _availableConditionStorage;

			
			[SerializeThis]
			public ExploredCaveCondition _completeConditionStorage;
		}

		
		[DoNotSerializePublic]
		[Serializable]
		public class Cave5TodoTask : SurvivalBookTodo.TodoTask
		{
			
			
			public override ACondition _availableCondition
			{
				get
				{
					return this._availableConditionStorage;
				}
			}

			
			
			public override ACondition _completeCondition
			{
				get
				{
					return this._completeConditionStorage;
				}
			}

			
			public override void FixSerializer()
			{
				bool done = this._availableConditionStorage != null && this._availableConditionStorage._done;
				if (this._availableConditionStorage == null || string.IsNullOrEmpty(this._availableConditionStorage._objectTag))
				{
					this._availableConditionStorage = new ProximityCondition
					{
						_objectTag = "Cave5Door",
						_isTag = false,
						_use2dDistance = true,
						_distance = 10f,
						_done = done
					};
				}
				if (this._completeConditionStorage == null || this._completeConditionStorage._caveNumber == -1)
				{
					this._completeConditionStorage = new ExploredCaveCondition
					{
						_caveNumber = 4
					};
				}
				base.FixSerializer();
			}

			
			public virtual void Clone(SurvivalBookTodo.Cave5TodoTask other)
			{
				this._availableConditionStorage = other._availableConditionStorage;
				this._completeConditionStorage = other._completeConditionStorage;
				base.Clone(other);
			}

			
			[SerializeThis]
			public ProximityCondition _availableConditionStorage;

			
			[SerializeThis]
			public ExploredCaveCondition _completeConditionStorage;
		}

		
		[DoNotSerializePublic]
		[Serializable]
		public class Cave6TodoTask : SurvivalBookTodo.TodoTask
		{
			
			
			public override ACondition _availableCondition
			{
				get
				{
					return this._availableConditionStorage;
				}
			}

			
			
			public override ACondition _completeCondition
			{
				get
				{
					return this._completeConditionStorage;
				}
			}

			
			public override void FixSerializer()
			{
				bool done = this._availableConditionStorage != null && this._availableConditionStorage._done;
				if (this._availableConditionStorage == null || string.IsNullOrEmpty(this._availableConditionStorage._objectTag))
				{
					this._availableConditionStorage = new ProximityCondition
					{
						_objectTag = "Cave6Door",
						_isTag = false,
						_use2dDistance = true,
						_distance = 10f,
						_done = done
					};
				}
				if (this._completeConditionStorage == null || this._completeConditionStorage._caveNumber == -1)
				{
					this._completeConditionStorage = new ExploredCaveCondition
					{
						_caveNumber = 5
					};
				}
				base.FixSerializer();
			}

			
			public virtual void Clone(SurvivalBookTodo.Cave6TodoTask other)
			{
				this._availableConditionStorage = other._availableConditionStorage;
				this._completeConditionStorage = other._completeConditionStorage;
				base.Clone(other);
			}

			
			[SerializeThis]
			public ProximityCondition _availableConditionStorage;

			
			[SerializeThis]
			public ExploredCaveCondition _completeConditionStorage;
		}

		
		[DoNotSerializePublic]
		[Serializable]
		public class Cave7TodoTask : SurvivalBookTodo.TodoTask
		{
			
			
			public override ACondition _availableCondition
			{
				get
				{
					return this._availableConditionStorage;
				}
			}

			
			
			public override ACondition _completeCondition
			{
				get
				{
					return this._completeConditionStorage;
				}
			}

			
			public override void FixSerializer()
			{
				bool done = this._availableConditionStorage != null && this._availableConditionStorage._done;
				if (this._availableConditionStorage == null || string.IsNullOrEmpty(this._availableConditionStorage._objectTag))
				{
					this._availableConditionStorage = new ProximityCondition
					{
						_objectTag = "Cave7Door1",
						_isTag = false,
						_use2dDistance = true,
						_distance = 10f,
						_done = done
					};
				}
				if (this._completeConditionStorage == null || this._completeConditionStorage._caveNumber == -1)
				{
					this._completeConditionStorage = new ExploredCaveCondition
					{
						_caveNumber = 6
					};
				}
				base.FixSerializer();
			}

			
			public virtual void Clone(SurvivalBookTodo.Cave7TodoTask other)
			{
				this._availableConditionStorage = other._availableConditionStorage;
				this._completeConditionStorage = other._completeConditionStorage;
				base.Clone(other);
			}

			
			[SerializeThis]
			public ProximityCondition _availableConditionStorage;

			
			[SerializeThis]
			public ExploredCaveCondition _completeConditionStorage;
		}

		
		[DoNotSerializePublic]
		[Serializable]
		public class Cave8TodoTask : SurvivalBookTodo.TodoTask
		{
			
			
			public override ACondition _availableCondition
			{
				get
				{
					return this._availableConditionStorage;
				}
			}

			
			
			public override ACondition _completeCondition
			{
				get
				{
					return this._completeConditionStorage;
				}
			}

			
			public override void FixSerializer()
			{
				bool done = this._availableConditionStorage != null && this._availableConditionStorage._done;
				if (this._availableConditionStorage == null || string.IsNullOrEmpty(this._availableConditionStorage._objectTag))
				{
					this._availableConditionStorage = new ProximityCondition
					{
						_objectTag = "Cave7Door1",
						_isTag = false,
						_use2dDistance = true,
						_distance = 10f,
						_done = done
					};
				}
				if (this._completeConditionStorage == null || this._completeConditionStorage._caveNumber == -1)
				{
					this._completeConditionStorage = new ExploredCaveCondition
					{
						_caveNumber = 7
					};
				}
				base.FixSerializer();
			}

			
			public virtual void Clone(SurvivalBookTodo.Cave8TodoTask other)
			{
				this._availableConditionStorage = other._availableConditionStorage;
				this._completeConditionStorage = other._completeConditionStorage;
				base.Clone(other);
			}

			
			[SerializeThis]
			public ProximityCondition _availableConditionStorage;

			
			[SerializeThis]
			public ExploredCaveCondition _completeConditionStorage;
		}

		
		[DoNotSerializePublic]
		[Serializable]
		public class Cave9TodoTask : SurvivalBookTodo.TodoTask
		{
			
			
			public override ACondition _availableCondition
			{
				get
				{
					return this._availableConditionStorage;
				}
			}

			
			
			public override ACondition _completeCondition
			{
				get
				{
					return this._completeConditionStorage;
				}
			}

			
			public override void FixSerializer()
			{
				bool done = this._availableConditionStorage != null && this._availableConditionStorage._done;
				if (this._availableConditionStorage == null || string.IsNullOrEmpty(this._availableConditionStorage._objectTag))
				{
					this._availableConditionStorage = new ProximityCondition
					{
						_objectTag = "Cave9RopeLedgeEntrance",
						_isTag = false,
						_use2dDistance = true,
						_distance = 10f,
						_done = done
					};
				}
				if (this._completeConditionStorage == null || this._completeConditionStorage._caveNumber == -1)
				{
					this._completeConditionStorage = new ExploredCaveCondition
					{
						_caveNumber = 8
					};
				}
				base.FixSerializer();
			}

			
			public virtual void Clone(SurvivalBookTodo.Cave9TodoTask other)
			{
				this._availableConditionStorage = other._availableConditionStorage;
				this._completeConditionStorage = other._completeConditionStorage;
				base.Clone(other);
			}

			
			[SerializeThis]
			public ProximityCondition _availableConditionStorage;

			
			[SerializeThis]
			public ExploredCaveCondition _completeConditionStorage;
		}

		
		[DoNotSerializePublic]
		[Serializable]
		public class Cave10TodoTask : SurvivalBookTodo.TodoTask
		{
			
			
			public override ACondition _availableCondition
			{
				get
				{
					return this._availableConditionStorage;
				}
			}

			
			
			public override ACondition _completeCondition
			{
				get
				{
					return this._completeConditionStorage;
				}
			}

			
			public override void FixSerializer()
			{
				bool done = this._availableConditionStorage != null && this._availableConditionStorage._done;
				if (this._availableConditionStorage == null || string.IsNullOrEmpty(this._availableConditionStorage._objectTag))
				{
					this._availableConditionStorage = new ProximityCondition
					{
						_objectTag = "Cave10ClimbEntrance",
						_isTag = false,
						_use2dDistance = true,
						_distance = 20f,
						_done = done
					};
				}
				if (this._completeConditionStorage == null || this._completeConditionStorage._caveNumber == -1)
				{
					this._completeConditionStorage = new ExploredCaveCondition
					{
						_caveNumber = 9
					};
				}
				base.FixSerializer();
			}

			
			public virtual void Clone(SurvivalBookTodo.Cave10TodoTask other)
			{
				this._availableConditionStorage = other._availableConditionStorage;
				this._completeConditionStorage = other._completeConditionStorage;
				base.Clone(other);
			}

			
			[SerializeThis]
			public ProximityCondition _availableConditionStorage;

			
			[SerializeThis]
			public ExploredCaveCondition _completeConditionStorage;
		}

		
		[DoNotSerializePublic]
		[Serializable]
		public class FindClimbingAxeTodoTask : SurvivalBookTodo.TodoTask
		{
			
			
			public override ACondition _availableCondition
			{
				get
				{
					return this._availableConditionStorage;
				}
			}

			
			
			public override ACondition _completeCondition
			{
				get
				{
					return this._completeConditionStorage;
				}
			}

			
			public override void FixSerializer()
			{
				bool done = this._availableConditionStorage != null && this._availableConditionStorage._done;
				if (this._availableConditionStorage == null || this._availableConditionStorage._type == (GameStats.StoryElements)0)
				{
					this._availableConditionStorage = new StoryCondition
					{
						_type = GameStats.StoryElements.FoundClimbWall,
						_done = done
					};
				}
				if (this._completeConditionStorage == null || this._completeConditionStorage._items == null)
				{
					this._completeConditionStorage = new ItemCondition
					{
						_items = new ItemIdList[]
						{
							new ItemIdList
							{
								_itemIds = new int[]
								{
									138
								}
							}
						}
					};
				}
				base.FixSerializer();
			}

			
			public virtual void Clone(SurvivalBookTodo.FindClimbingAxeTodoTask other)
			{
				this._availableConditionStorage = other._availableConditionStorage;
				this._completeConditionStorage = other._completeConditionStorage;
				base.Clone(other);
			}

			
			[SerializeThis]
			public StoryCondition _availableConditionStorage;

			
			[SerializeThis]
			public ItemCondition _completeConditionStorage;
		}

		
		[DoNotSerializePublic]
		[Serializable]
		public class FindRebreatherTodoTask : SurvivalBookTodo.TodoTask
		{
			
			
			public override ACondition _availableCondition
			{
				get
				{
					return this._availableConditionStorage;
				}
			}

			
			
			public override ACondition _completeCondition
			{
				get
				{
					return this._completeConditionStorage;
				}
			}

			
			public override void FixSerializer()
			{
				if (this._airBreathingCondition == null || this._airBreathingCondition._threshold == 0f)
				{
					this._airBreathingCondition = new AirBreathingCondition
					{
						_threshold = 0.4f
					};
				}
				if (this._completeConditionStorage == null || this._completeConditionStorage._items == null || this._completeConditionStorage._items.Length == 0)
				{
					this._completeConditionStorage = new ItemCondition
					{
						_items = new ItemIdList[]
						{
							new ItemIdList
							{
								_itemIds = new int[]
								{
									143
								}
							}
						}
					};
				}
				bool done = this._availableConditionStorage != null && this._availableConditionStorage._done;
				if (this._availableConditionStorage == null || this._availableConditionStorage._conditions == null || this._availableConditionStorage._conditions.Length == 0)
				{
					this._availableConditionStorage = new ListAnyCondition
					{
						_conditions = new ACondition[]
						{
							this._airBreathingCondition,
							this._completeConditionStorage
						},
						_done = done
					};
				}
				base.FixSerializer();
			}

			
			public virtual void Clone(SurvivalBookTodo.FindRebreatherTodoTask other)
			{
				this._airBreathingCondition = other._airBreathingCondition;
				this._availableConditionStorage = other._availableConditionStorage;
				this._completeConditionStorage = other._completeConditionStorage;
				base.Clone(other);
			}

			
			[SerializeThis]
			public AirBreathingCondition _airBreathingCondition;

			
			[SerializeThis]
			public ListAnyCondition _availableConditionStorage;

			
			[SerializeThis]
			public ItemCondition _completeConditionStorage;
		}

		
		[DoNotSerializePublic]
		[Serializable]
		public class SinkHoleTodoTask : SurvivalBookTodo.TodoTask
		{
			
			
			public override ACondition _availableCondition
			{
				get
				{
					return this._availableConditionStorage;
				}
			}

			
			
			public override ACondition _completeCondition
			{
				get
				{
					return this._completeConditionStorage;
				}
			}

			
			public override void FixSerializer()
			{
				if (this._availableConditionStorage == null || string.IsNullOrEmpty(this._availableConditionStorage._objectTag))
				{
					this._availableConditionStorage = new ProximityCondition
					{
						_objectTag = "SinkHoleCenter",
						_isTag = true,
						_use2dDistance = true,
						_distance = 250f
					};
				}
				if (this._completeConditionStorage == null || string.IsNullOrEmpty(this._completeConditionStorage._objectTag))
				{
					this._completeConditionStorage = new ProximityCondition
					{
						_objectTag = "SinkHoleCenter",
						_isTag = true,
						_use2dDistance = false,
						_distance = 135f
					};
				}
				base.FixSerializer();
			}

			
			public virtual void Clone(SurvivalBookTodo.SinkHoleTodoTask other)
			{
				this._availableConditionStorage = other._availableConditionStorage;
				this._completeConditionStorage = other._completeConditionStorage;
				base.Clone(other);
			}

			
			[SerializeThis]
			public ProximityCondition _availableConditionStorage;

			
			[SerializeThis]
			public ProximityCondition _completeConditionStorage;
		}

		
		[DoNotSerializePublic]
		[Serializable]
		public class PassengersTodoTask : SurvivalBookTodo.TodoTask
		{
			
			
			public override ACondition _availableCondition
			{
				get
				{
					return this._availableConditionStorage;
				}
			}

			
			
			public override ACondition _completeCondition
			{
				get
				{
					return this._completeConditionStorage;
				}
			}

			
			public override void FixSerializer()
			{
				bool done = this._availableConditionStorage != null && this._availableConditionStorage._done;
				if (this._availableConditionStorage == null || this._availableConditionStorage._items == null)
				{
					this._availableConditionStorage = new ItemCondition
					{
						_items = new ItemIdList[]
						{
							new ItemIdList
							{
								_itemIds = new int[]
								{
									197
								}
							}
						},
						_done = done
					};
				}
				if (this._completeConditionStorage == null)
				{
					this._completeConditionStorage = new PassengersCondition();
				}
				base.FixSerializer();
			}

			
			public virtual void Clone(SurvivalBookTodo.PassengersTodoTask other)
			{
				this._availableConditionStorage = other._availableConditionStorage;
				this._completeConditionStorage = other._completeConditionStorage;
				base.Clone(other);
			}

			
			[SerializeThis]
			public ItemCondition _availableConditionStorage;

			
			[SerializeThis]
			public PassengersCondition _completeConditionStorage;
		}
	}
}
