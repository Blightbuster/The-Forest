using System;
using System.Collections.Generic;
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
								string text = array[num].Name;
								string text2;
								switch (text)
								{
								case "_treeCutDown":
									text2 = "Trees Cut Down: ";
									goto IL_4BE;
								case "_enemiesKilled":
									text2 = "Enemies Killed: ";
									goto IL_4BE;
								case "_rabbitKilled":
									text2 = "Rabbits Killed: ";
									goto IL_4BE;
								case "_lizardKilled":
									text2 = "Lizards Killed: ";
									goto IL_4BE;
								case "_raccoonKilled":
									text2 = "Raccoons Killed: ";
									goto IL_4BE;
								case "_deerKilled":
									text2 = "Deer Killed: ";
									goto IL_4BE;
								case "_turtleKilled":
									text2 = "Turtles Killed: ";
									goto IL_4BE;
								case "_birdKilled":
									text2 = "Birds Killed: ";
									goto IL_4BE;
								case "_cookedFood":
									text2 = "Cooked Food: ";
									goto IL_4BE;
								case "_burntFood":
									text2 = "Burnt Food: ";
									goto IL_4BE;
								case "_cancelledStructures":
									text2 = "Cancelled Structures: ";
									goto IL_4BE;
								case "_builtStructures":
									text2 = "Built Structures: ";
									goto IL_4BE;
								case "_destroyedStructures":
									text2 = "Destroyed Structures: ";
									goto IL_4BE;
								case "_repairedStructures":
									text2 = "Repaired Structures: ";
									goto IL_4BE;
								case "_edibleItemsUsed":
									text2 = "Edible Items Used: ";
									goto IL_4BE;
								case "_itemsCrafted":
									text2 = "Items Crafted: ";
									goto IL_4BE;
								case "_upgradesAdded":
									text2 = "Upgrades Added: ";
									goto IL_4BE;
								case "_arrowsFired":
									text2 = "Arrows Fired: ";
									goto IL_4BE;
								case "_litArrows":
									text2 = "Lit Arrows: ";
									goto IL_4BE;
								case "_litWeapons":
									text2 = "Lit Weapons: ";
									goto IL_4BE;
								case "_burntEnemies":
									text2 = "Burnt Enemies: ";
									goto IL_4BE;
								case "_explodedEnemies":
									text2 = "Exploded Enemies: ";
									goto IL_4BE;
								}
								text2 = string.Empty;
								IL_4BE:
								if (string.IsNullOrEmpty(text2))
								{
									this._labelStat.gameObject.SetActive(false);
								}
								else
								{
									this._labelStat.gameObject.SetActive(true);
									this._labelStat.text = text2 + array[num].GetValue(gameStats);
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
						string text3 = File.ReadAllText(path2);
						string text = text3;
						if (text != null)
						{
							if (LoadSaveSlotInfo.<>f__switch$map1B == null)
							{
								LoadSaveSlotInfo.<>f__switch$map1B = new Dictionary<string, int>(5)
								{
									{
										"Peaceful",
										0
									},
									{
										"Hard",
										0
									},
									{
										"HardSurvival",
										0
									},
									{
										"Creative",
										0
									},
									{
										"Normal",
										0
									}
								};
							}
							int num2;
							if (LoadSaveSlotInfo.<>f__switch$map1B.TryGetValue(text, out num2))
							{
								if (num2 == 0)
								{
									string text4 = text3.ToUpper();
									UILabel labelSlot2 = this._labelSlot;
									labelSlot2.text += UiTranslationDatabase.TranslateKey(text4, text4, false);
								}
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
