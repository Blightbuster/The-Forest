using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using TheForest.Commons.Enums;
using TheForest.Utils;
using UniLinq;
using UnityEngine;

namespace TheForest.UI
{
	
	public class LoadSaveSlotInfo : MonoBehaviour
	{
		
		private void OnEnable()
		{
			this._slotNum = (int)this._slot;
			this.LoadStats();
		}

		
		private void LoadStats()
		{
			string localSlotPath = SaveSlotUtils.GetLocalSlotPath(this._slot);
			string path = localSlotPath + "info";
			if (!File.Exists(localSlotPath + "__RESUME__"))
			{
				this._labelSlot.text = UiTranslationDatabase.TranslateKey("SLOT_" + this._slotNum, "Slot " + this._slotNum, false);
				if (this._labelStat)
				{
					this._labelStat.gameObject.SetActive(false);
				}
				if (this._labelDateTime)
				{
					this._labelDateTime.gameObject.SetActive(false);
				}
				if (Application.loadedLevelName.Equals("TitleScene"))
				{
					base.transform.parent.GetComponent<Collider>().enabled = false;
				}
			}
			else
			{
				base.transform.parent.GetComponent<Collider>().enabled = true;
				try
				{
					if (File.Exists(path))
					{
						GameStats.Stats gameStats = GameStats.Stats.LoadFromBytes(File.ReadAllBytes(path));
						this._labelSlot.text = UiTranslationDatabase.TranslateKey("SLOT_" + this._slotNum, "Slot " + this._slotNum, false) + UiTranslationDatabase.TranslateKey("_DAY_", ": day ", false) + gameStats._day;
						if (this._labelStat)
						{
							FieldInfo[] array = (from f in gameStats.GetType().GetFields()
							where (int)f.GetValue(gameStats) > 0
							select f).ToArray<FieldInfo>();
							if (array != null && array.Length > 0)
							{
								int num = UnityEngine.Random.Range(0, array.Length);
								string name = array[num].Name;
								string text;
								switch (name)
								{
								case "_treeCutDown":
									text = "Trees Cut Down: ";
									goto IL_4BC;
								case "_enemiesKilled":
									text = "Enemies Killed: ";
									goto IL_4BC;
								case "_rabbitKilled":
									text = "Rabbits Killed: ";
									goto IL_4BC;
								case "_lizardKilled":
									text = "Lizards Killed: ";
									goto IL_4BC;
								case "_raccoonKilled":
									text = "Raccoons Killed: ";
									goto IL_4BC;
								case "_deerKilled":
									text = "Deer Killed: ";
									goto IL_4BC;
								case "_turtleKilled":
									text = "Turtles Killed: ";
									goto IL_4BC;
								case "_birdKilled":
									text = "Birds Killed: ";
									goto IL_4BC;
								case "_cookedFood":
									text = "Cooked Food: ";
									goto IL_4BC;
								case "_burntFood":
									text = "Burnt Food: ";
									goto IL_4BC;
								case "_cancelledStructures":
									text = "Cancelled Structures: ";
									goto IL_4BC;
								case "_builtStructures":
									text = "Built Structures: ";
									goto IL_4BC;
								case "_destroyedStructures":
									text = "Destroyed Structures: ";
									goto IL_4BC;
								case "_repairedStructures":
									text = "Repaired Structures: ";
									goto IL_4BC;
								case "_edibleItemsUsed":
									text = "Edible Items Used: ";
									goto IL_4BC;
								case "_itemsCrafted":
									text = "Items Crafted: ";
									goto IL_4BC;
								case "_upgradesAdded":
									text = "Upgrades Added: ";
									goto IL_4BC;
								case "_arrowsFired":
									text = "Arrows Fired: ";
									goto IL_4BC;
								case "_litArrows":
									text = "Lit Arrows: ";
									goto IL_4BC;
								case "_litWeapons":
									text = "Lit Weapons: ";
									goto IL_4BC;
								case "_burntEnemies":
									text = "Burnt Enemies: ";
									goto IL_4BC;
								case "_explodedEnemies":
									text = "Exploded Enemies: ";
									goto IL_4BC;
								}
								text = string.Empty;
								IL_4BC:
								if (string.IsNullOrEmpty(text))
								{
									this._labelStat.gameObject.SetActive(false);
								}
								else
								{
									this._labelStat.gameObject.SetActive(true);
									this._labelStat.text = text + array[num].GetValue(gameStats);
								}
							}
							else
							{
								this._labelStat.gameObject.SetActive(false);
							}
						}
					}
					else
					{
						this._labelSlot.text = "Slot " + this._slotNum;
					}
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
				UILabel labelSlot = this._labelSlot;
				labelSlot.text += "\n";
				try
				{
					string path2 = localSlotPath + "difficulty";
					if (File.Exists(path2))
					{
						string text2 = File.ReadAllText(path2);
						if (text2 != null)
						{
							if (text2 == "Peaceful" || text2 == "Hard" || text2 == "HardSurvival" || text2 == "Creative" || text2 == "Normal")
							{
								string text3 = text2.ToUpper();
								UILabel labelSlot2 = this._labelSlot;
								labelSlot2.text += UiTranslationDatabase.TranslateKey(text3, text3, false);
							}
						}
					}
				}
				catch (Exception ex)
				{
				}
				if (this._labelDateTime)
				{
					this._labelDateTime.text = File.GetLastWriteTime(localSlotPath + "__RESUME__").ToString(CultureInfo.CurrentCulture.DateTimeFormat);
					this._labelDateTime.gameObject.SetActive(true);
				}
			}
		}

		
		public UILabel _labelSlot;

		
		public UILabel _labelStat;

		
		public UILabel _labelDateTime;

		
		public Slots _slot;

		
		[HideInInspector]
		public int _slotNum;
	}
}
