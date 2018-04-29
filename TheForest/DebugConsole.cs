using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Bolt;
using PathologicalGames;
using Steamworks;
using TheForest.Buildings.Creation;
using TheForest.Buildings.World;
using TheForest.Commons.Enums;
using TheForest.Items;
using TheForest.Items.Craft;
using TheForest.Items.Inventory;
using TheForest.Items.Utils;
using TheForest.Items.World;
using TheForest.Player;
using TheForest.Utils;
using TheForest.World;
using TheForest.World.Areas;
using UniLinq;
using UnityEngine;

namespace TheForest
{
	
	public class DebugConsole : MonoBehaviour
	{
		
		private void _toggleWorkScheduler(string param)
		{
			if (string.IsNullOrEmpty(param) || param == "toggle")
			{
				param = ((!Scene.WorkScheduler.enabled) ? "on" : "off");
			}
			else
			{
				param = param.ToLower();
			}
			if (param == "on")
			{
				Scene.WorkScheduler.enabled = true;
				Debug.Log("$> Turned Work Scheduler ON");
			}
			else if (param == "off")
			{
				Scene.WorkScheduler.enabled = false;
				Debug.Log("$> Turned Work Scheduler OFF");
			}
			else
			{
				Debug.Log("$> usage: toggleWorkScheduler <on|off|toggle|>");
			}
		}

		
		private void _toggleCullingGrid(string param)
		{
			if (string.IsNullOrEmpty(param) || param == "toggle")
			{
				param = ((!CullingGrid.Instance.enabled) ? "on" : "off");
			}
			else
			{
				param = param.ToLower();
			}
			if (param == "on")
			{
				CullingGrid.Instance.enabled = true;
				Debug.Log("$> Turned Culling Grid  ON");
			}
			else if (param == "off")
			{
				CullingGrid.Instance.enabled = false;
				Debug.Log("$> Turned Culling Grid OFF");
			}
			else
			{
				Debug.Log("$> usage: toggleCullingGrid <on|off|toggle|>");
			}
		}

		
		private void _wsscaling(string onoff)
		{
			if (onoff == "off")
			{
				if (Scene.WorkScheduler.ScaleWithFPS)
				{
					this.wsMaxMsValue = Scene.WorkScheduler.MaxMilliseconds;
					Scene.WorkScheduler.ScaleWithFPS = false;
					Scene.WorkScheduler.MaxMilliseconds = 1f;
					Debug.Log("$> Disabled WS fps scaling");
				}
				else
				{
					Debug.Log("$> WS fps scaling already disabled");
				}
			}
			else if (onoff == "on")
			{
				Scene.WorkScheduler.ScaleWithFPS = true;
				Scene.WorkScheduler.MaxMilliseconds = this.wsMaxMsValue;
				Debug.Log("$> Enabled WS fps scaling");
			}
			else
			{
				Debug.Log("$> usage: wsscaling <on|off>");
			}
		}

		
		private void _LODManagerScaling(string onoff)
		{
			if (string.IsNullOrEmpty(onoff) || onoff.ToLower() == "toggle")
			{
				onoff = ((!LOD_Manager.Instance.FpsQualityScaling) ? "on" : "off");
			}
			onoff = onoff.ToLower();
			if (onoff == "off")
			{
				if (LOD_Manager.Instance.FpsQualityScaling)
				{
					LOD_Manager.Instance.FpsQualityScaling = false;
					Debug.Log("$> Disabled LODManager fps scaling");
				}
				else
				{
					Debug.Log("$> LODManager fps scaling already disabled");
				}
			}
			else if (onoff == "on")
			{
				if (!LOD_Manager.Instance.FpsQualityScaling)
				{
					LOD_Manager.Instance.FpsQualityScaling = true;
					Debug.Log("$> Enabled LODManager fps scaling");
				}
				else
				{
					Debug.Log("$> LODManager fps scaling already enabled");
				}
			}
			else
			{
				Debug.Log("$> usage: LODManagerScaling <on|off>");
			}
		}

		
		private void Awake()
		{
			if (CoopPeerStarter.DedicatedHost || (DebugConsole.Instance && DebugConsole.Instance != this) || !Cheats.DebugConsole)
			{
				UnityEngine.Object.Destroy(this);
				return;
			}
			if (!DebugConsole.Instance)
			{
				DebugConsole.Instance = this;
				this._availableConsoleMethods = (from m in typeof(DebugConsole).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod)
				where m.Name[0] == '_'
				select m).ToDictionary((MethodInfo m) => m.Name.Substring(1).ToLower());
				this._help(null);
				this._logs = new Queue<LogContent>();
				this._logRowStyle = new GUIStyle();
				this._alphaNum = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
				this._logRowStyle.normal.background = new Texture2D(1, 1, TextureFormat.RGBA32, false);
				this._logRowStyle.normal.background.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.25f));
				this._logRowStyle.normal.background.Apply();
				this._logRowStyle.normal.background.wrapMode = TextureWrapMode.Repeat;
				this._logRowStyle.margin = new RectOffset(0, 0, 2, 0);
				this._logRowStyle.padding = new RectOffset(3, 1, 1, 1);
				this._logRowStyle.normal.textColor = Color.white;
				this._logRowStyle.fixedHeight = 12f;
				this._logRowStyle.fontSize = 10;
				this._consoleRowStyle = new GUIStyle(this._logRowStyle);
				this._consoleRowStyle.fixedHeight = 20f;
				this._textStyle = new GUIStyle();
				this._textStyle.normal.textColor = Color.white;
				this._textStyle.fontSize = 10;
				this._textStyle.margin = new RectOffset(0, 0, 0, 0);
				this._textStyle.padding = new RectOffset(0, 0, 0, 0);
				this._maxLogs = Mathf.FloorToInt(500f / (this._logRowStyle.fixedHeight + 2f));
				DebugConsole.Counters = (this._counters = new Dictionary<Type, int>());
				Application.logMessageReceived += this.LogCallback;
				this._gamepadWheelEntries = new Dictionary<string, string>();
				this._gamepadWheelEntries.Add("☍\nToggle Overlay", "toggleoverlay");
				this._gamepadWheelEntries.Add("☍\nToggle Stats", "toggleplayerstats");
				this._gamepadWheelEntries.Add("☞\nReport errors", "reporterrorsnow");
				this._gamepadWheelEntries.Add("⊕\nMemory snapshot", "profilersnapshot");
				this._gamepadWheelEntries.Add("∞\nBuild\nHack", "buildhack toggle");
				this._gamepadWheelEntries.Add("Ω\nGodmode", "godmode toggle");
				this._gamepadWheelEntries.Add("♕\nAll items", "addallitems");
				this._gamepadWheelEntries.Add("☠\nKill all\nenemies", "killallenemies");
				this._gamepadWheelEntries.Add("☠\nKill closest\nenemy", "killclosestenemy");
				this._gamepadWheelEntries.Add("☢\nKill local\nplayer", "killlocalplayer");
				this._gamepadWheelEntries.Add("✈\nGoto\nHull", "goto Hull");
				this._gamepadWheelEntries.Add("☮\nToggle\nEnemies", "enemies toggle");
				this._gamepadWheelEntries.Add("♘\nToggle\nAnimals", "animals toggle");
				this._gamepadWheelEntries.Add("❦\nToggle\nNatureSpawned", "togglego Nature_Spawned");
				this._gamepadWheelEntries.Add("❦\nToggle\nNaturePlaced", "togglego Nature_Placed");
				this._gamepadWheelEntries.Add("❦\nToggle\nCaves", "togglego Caves");
				this._gamepadWheelEntries.Add("⟳\nToggle\nWorkScheduler", "toggleWorkScheduler toggle");
				this._gamepadWheelEntries.Add("▦\nToggle\nCullingGrid", "toggleCullingGrid toggle");
				this.CheckDisplayState();
				UnityEngine.Object.DontDestroyOnLoad(this);
			}
			else
			{
				UnityEngine.Object.Destroy(this);
			}
		}

		
		private void OnLevelWasLoaded()
		{
			if (Application.loadedLevel == 0 && DebugConsole.Counters != null)
			{
				DebugConsole.Counters.Clear();
			}
		}

		
		private void OnDestroy()
		{
			Application.logMessageReceived -= this.LogCallback;
			if (DebugConsole.Counters == this._counters)
			{
				DebugConsole.Counters = null;
			}
			if (DebugConsole.Instance == this)
			{
				DebugConsole.Instance = this;
			}
		}

		
		private void Update()
		{
			this._fps = Mathf.Lerp(this._fps, 1f / Time.deltaTime, 0.05f);
			if (float.IsNaN(this._fps) || this._fps == 0f)
			{
				this._fps = 1f;
			}
			if (TheForest.Utils.Input.GetButtonDown("Debug") || this.CheckPS4Debug())
			{
				this.ToggleGamePadWheel();
			}
			if (TheForest.Utils.Input.GetKeyDown(KeyCode.F4))
			{
				Debug.Break();
			}
		}

		
		private void OnGUI()
		{
			Color color = GUI.color;
			if (this.HandleInput())
			{
				return;
			}
			if (this._showConsole)
			{
				GUI.skin.label.fontSize = 12;
				GUILayout.BeginHorizontal(this._consoleRowStyle, new GUILayoutOption[]
				{
					GUILayout.Width((float)Screen.width),
					GUILayout.Height(20f)
				});
				GUILayout.Label("$> ", GUI.skin.label, new GUILayoutOption[0]);
				GUI.color = Color.gray;
				GUILayout.Label(this._autocomplete, new GUILayoutOption[]
				{
					GUILayout.MinWidth((float)(Screen.width * 3 / 4))
				});
				GUI.color = color;
				GUI.SetNextControlName("debugConsole");
				this._consoleInput = GUI.TextField(GUILayoutUtility.GetLastRect(), this._consoleInput, GUI.skin.label);
				if (this._focusConsoleField)
				{
					this._focusConsoleField = false;
					GUI.FocusControl("debugConsole");
				}
				if (this._selectConsoleText)
				{
					GUI.FocusControl("debugConsole");
					this._selectConsoleText = false;
					this._focusConsoleField = true;
					TextEditor textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
					textEditor.MoveLineEnd();
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
			if (this._showOverlay)
			{
				GUILayout.BeginHorizontal(new GUILayoutOption[]
				{
					GUILayout.Width((float)Screen.width),
					GUILayout.Height(24f)
				});
				GUILayout.FlexibleSpace();
				GUILayout.Label("FPS: " + (int)this._fps, GUI.skin.button, new GUILayoutOption[0]);
				GUILayout.BeginVertical(new GUILayoutOption[0]);
				GUILayout.Label("Total Alloc: " + Profiler.GetTotalAllocatedMemory() / 1000u / 1000u + "MB", GUI.skin.button, new GUILayoutOption[]
				{
					GUILayout.MinWidth(140f)
				});
				GUILayout.Label("Total Reserved: " + Profiler.GetTotalReservedMemory() / 1000u / 1000u + "MB", GUI.skin.button, new GUILayoutOption[]
				{
					GUILayout.MinWidth(140f)
				});
				GUILayout.Label("Heap Size: " + Profiler.GetMonoHeapSize() / 1000u / 1000u + "MB", GUI.skin.button, new GUILayoutOption[]
				{
					GUILayout.MinWidth(140f)
				});
				GUILayout.Label("Used Size: " + Profiler.GetMonoUsedSize() / 1000u / 1000u + "MB", GUI.skin.button, new GUILayoutOption[]
				{
					GUILayout.MinWidth(140f)
				});
				GUILayout.Label("GC: " + GC.GetTotalMemory(false) / 1000L / 1000L + "MB", GUI.skin.button, new GUILayoutOption[]
				{
					GUILayout.MinWidth(100f)
				});
				GUILayout.EndVertical();
				GUILayout.BeginVertical(new GUILayoutOption[0]);
				GUILayout.Label(string.Concat(new object[]
				{
					(int)FMOD_StudioEventEmitter.HoursSinceMidnight,
					"h",
					(int)((FMOD_StudioEventEmitter.HoursSinceMidnight - (float)((int)FMOD_StudioEventEmitter.HoursSinceMidnight)) * 60f),
					(!Clock.Dark) ? " (d)" : " (n)"
				}), GUI.skin.button, new GUILayoutOption[]
				{
					GUILayout.Width(80f)
				});
				GUILayout.Label((!LocalPlayer.IsInCaves) ? "Not in cave" : "In cave", GUI.skin.button, new GUILayoutOption[]
				{
					GUILayout.Width(80f)
				});
				if (LocalPlayer.Inventory)
				{
					GUILayout.Label(string.Concat(new object[]
					{
						"x: ",
						LocalPlayer.Transform.position.x,
						"\ny: ",
						LocalPlayer.Transform.position.y,
						"\nz: ",
						LocalPlayer.Transform.position.z
					}), GUI.skin.button, new GUILayoutOption[]
					{
						GUILayout.Width(80f)
					});
				}
				GUILayout.EndVertical();
				GUILayout.BeginVertical(new GUILayoutOption[0]);
				this._showPlayerStats = GUILayout.Toggle(this._showPlayerStats, "Player Stats", GUI.skin.button, new GUILayoutOption[0]);
				GUILayout.Label("Occlusion: " + LOD_Manager.TreeOcclusionBonusRatio.ToString("0.00"), GUI.skin.button, new GUILayoutOption[0]);
				GUILayout.EndVertical();
				foreach (KeyValuePair<Type, int> keyValuePair in DebugConsole.Counters)
				{
					if (GUILayout.Button(keyValuePair.Key.Name + ": " + keyValuePair.Value, new GUILayoutOption[0]))
					{
						this.CheckAmount(keyValuePair.Key, keyValuePair.Value);
					}
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
			if (this._showPlayerStats && LocalPlayer.Stats)
			{
				GUILayout.BeginArea(new Rect((float)(Screen.width - 250), (float)(Screen.height / 2 - 200), 250f, 400f), GUI.skin.textArea);
				GUILayout.BeginVertical(new GUILayoutOption[0]);
				GUILayout.Label(string.Concat(new object[]
				{
					"+ Athleticism real:",
					LocalPlayer.Stats.Skills.AthleticismSkillLevel,
					", display:",
					LocalPlayer.Stats.Skills.AthleticismSkillLevelProgressApprox
				}), this._textStyle, new GUILayoutOption[0]);
				GUILayout.Label(string.Format("|- Sprint: {0:F0} / {1:F0} = {2:F0} ", LocalPlayer.Stats.Skills.TotalRunDuration, LocalPlayer.Stats.Skills.RunSkillLevelDuration, LocalPlayer.Stats.Skills.TotalRunDuration / LocalPlayer.Stats.Skills.RunSkillLevelDuration), this._textStyle, new GUILayoutOption[0]);
				GUILayout.Label(string.Format("|- Diving: {0:F0} / {1:F0} = {2:F0} ", LocalPlayer.Stats.Skills.TotalLungBreathingDuration, LocalPlayer.Stats.Skills.BreathingSkillLevelDuration, LocalPlayer.Stats.Skills.TotalLungBreathingDuration / LocalPlayer.Stats.Skills.BreathingSkillLevelDuration), this._textStyle, new GUILayoutOption[0]);
				GUILayout.Space(20f);
				GUILayout.Label(string.Format("+ Weight {0:F3}lbs", LocalPlayer.Stats.PhysicalStrength.CurrentWeight), this._textStyle, new GUILayoutOption[0]);
				GUILayout.Label(string.Format("|- Current Calories Burnt: {0:F3}", LocalPlayer.Stats.Calories.CurrentCaloriesBurntCount), this._textStyle, new GUILayoutOption[0]);
				GUILayout.Label(string.Format("|- Current Calories Eaten: {0:F3}", LocalPlayer.Stats.Calories.CurrentCaloriesEatenCount), this._textStyle, new GUILayoutOption[0]);
				GUILayout.Label(string.Format("|- Excess Calories Final: {0}", LocalPlayer.Stats.Calories.GetExcessCaloriesFinal()), this._textStyle, new GUILayoutOption[0]);
				GUILayout.Label(string.Format("|- Time to next resolution: {0:F3} Hours (IG)", LocalPlayer.Stats.Calories.TimeToNextResolution()), this._textStyle, new GUILayoutOption[0]);
				int excessCaloriesFinal = LocalPlayer.Stats.Calories.GetExcessCaloriesFinal();
				GUILayout.Label(string.Format("|- Weight change at resolution: {0:F3} lbs", (float)excessCaloriesFinal * ((excessCaloriesFinal <= 0) ? LocalPlayer.Stats.Calories.WeightLossPerMissingCalory : LocalPlayer.Stats.Calories.WeightGainPerExcessCalory)), this._textStyle, new GUILayoutOption[0]);
				GUILayout.Space(20f);
				GUILayout.Space(20f);
				GUILayout.Label(string.Format("+ Strength {0:F4} ({1})", LocalPlayer.Stats.PhysicalStrength.CurrentStrength, (excessCaloriesFinal <= 0) ? "Losing" : "Gaining"), this._textStyle, new GUILayoutOption[0]);
				GUILayout.Space(20f);
				GUILayout.EndVertical();
				GUILayout.EndArea();
			}
			if (this._showLog)
			{
				GUILayout.BeginArea(new Rect(5f, 40f, 600f, 500f));
				GUILayout.FlexibleSpace();
				foreach (LogContent logContent in this._logs)
				{
					GUILayout.BeginHorizontal(this._logRowStyle, new GUILayoutOption[0]);
					switch (logContent.type)
					{
					case LogType.Error:
						GUI.color = Color.red;
						break;
					case LogType.Warning:
						GUI.color = Color.yellow;
						break;
					case LogType.Log:
						GUI.color = color;
						break;
					case LogType.Exception:
						GUI.color = Color.magenta;
						break;
					}
					GUILayout.Label(logContent.type + ((logContent.amount <= 1) ? string.Empty : (" x" + logContent.amount)), this._textStyle, new GUILayoutOption[]
					{
						GUILayout.Width(80f)
					});
					GUILayout.Label(logContent.content, this._textStyle, new GUILayoutOption[0]);
					GUILayout.EndHorizontal();
				}
				GUILayout.EndArea();
				GUI.color = color;
				if (!string.IsNullOrEmpty(GUI.tooltip))
				{
					GUIStyle guistyle = new GUIStyle(this._textStyle);
					guistyle.normal.background = this._logRowStyle.normal.background;
					guistyle.wordWrap = true;
					GUI.Label(new Rect(610f, 100f, 600f, 500f), GUI.tooltip, guistyle);
				}
			}
			if (this._showGamePadWheel)
			{
				this.ShowGamepadWheel();
			}
		}

		
		private void ToggleConsole()
		{
			if (Scene.HudGui && Scene.HudGui.Chatbox)
			{
				Scene.HudGui.Chatbox.enabled = this._showConsole;
			}
			this._showConsole = !this._showConsole;
			if (LocalPlayer.Inventory)
			{
				LocalPlayer.Inventory.enabled = !this._showConsole;
			}
			if (this._showConsole)
			{
				this._focusConsoleField = true;
			}
			this._consoleInput = string.Empty;
			this.CheckDisplayState();
		}

		
		private void FinalizeConsoleInput()
		{
			if (Scene.HudGui && Scene.HudGui.Chatbox)
			{
				Scene.HudGui.Chatbox.enabled = true;
			}
			if (LocalPlayer.Inventory)
			{
				LocalPlayer.Inventory.enabled = true;
			}
			this._showConsole = false;
			this.HandleConsoleInput();
			this._consoleInput = string.Empty;
			this.CheckDisplayState();
		}

		
		private void ToggleOverlay()
		{
			if (!this._showOverlay)
			{
				this._showOverlay = true;
			}
			else if (!this._showLog)
			{
				this._showLog = true;
			}
			else
			{
				this._showOverlay = false;
				this._showLog = false;
			}
			this.CheckDisplayState();
		}

		
		private void TogglePlayerStats()
		{
			this._showPlayerStats = !this._showPlayerStats;
			this.CheckDisplayState();
		}

		
		private void ToggleGamePadWheel()
		{
			this._showGamePadWheel = !this._showGamePadWheel;
			if (LocalPlayer.FpCharacter)
			{
				if (this._showGamePadWheel)
				{
					LocalPlayer.FpCharacter.LockView(true);
				}
				else
				{
					LocalPlayer.FpCharacter.UnLockView();
				}
			}
			this.CheckDisplayState();
		}

		
		private void ShowGamepadWheel()
		{
			float num = 70f;
			float num2 = num / 2f;
			int count = this._gamepadWheelEntries.Count;
			float f = 360f / (float)count * 0.0174532924f;
			Vector2 vector = new Vector2((float)(Screen.width / 2), (float)(Screen.height / 2));
			Vector2 a = vector;
			Vector2 vector2 = new Vector2(TheForest.Utils.Input.GetAxis("Horizontal"), -TheForest.Utils.Input.GetAxis("Vertical"));
			Vector2 point = a + vector2.normalized * (float)(15 * count);
			Rect position = new Rect(vector.x - num2, vector.y - num2, num, num);
			GUIStyle guistyle = new GUIStyle(GUI.skin.label);
			GUIStyle guistyle2 = new GUIStyle(GUI.skin.box);
			guistyle.alignment = TextAnchor.MiddleCenter;
			guistyle.wordWrap = true;
			guistyle.fontStyle = FontStyle.Bold;
			guistyle2.alignment = TextAnchor.MiddleCenter;
			guistyle2.wordWrap = true;
			guistyle2.fontStyle = FontStyle.Bold;
			bool flag = position.Contains(point);
			if (flag && TheForest.Utils.Input.GetButtonDown("Take"))
			{
				this.ToggleGamePadWheel();
			}
			position.y += (float)(15 * count);
			foreach (KeyValuePair<string, string> keyValuePair in this._gamepadWheelEntries)
			{
				float num3 = (position.x - vector.x + num2) * Mathf.Cos(f) - (position.y - vector.y + num2) * Mathf.Sin(f);
				float num4 = (position.x - vector.x + num2) * Mathf.Sin(f) + (position.y - vector.y + num2) * Mathf.Cos(f);
				position.x = num3 + vector.x - num2;
				position.y = num4 + vector.y - num2;
				flag = position.Contains(point);
				GUI.Label(position, keyValuePair.Key, (!flag) ? guistyle : guistyle2);
				if (flag && TheForest.Utils.Input.GetButtonDown("Take"))
				{
					this._consoleInput = keyValuePair.Value;
					this.HandleConsoleInput();
					this.ToggleGamePadWheel();
				}
			}
			position.x = point.x - 10f;
			position.y = point.y - 10f;
			position.width = 20f;
			position.height = 20f;
			GUI.Button(position, "X");
		}

		
		private bool HandleInput()
		{
			if (UnityEngine.Event.current.type == EventType.KeyDown)
			{
				KeyCode keyCode = UnityEngine.Event.current.keyCode;
				switch (keyCode)
				{
				case KeyCode.UpArrow:
					if (this._showConsole)
					{
						if (this._historyCurrent == -1)
						{
							this._historyCurrent = this._historyEnd;
						}
						else
						{
							int num = this._historyCurrent - 1;
							num = (num + this._history.Length) % this._history.Length;
							if (num != this._historyEnd && this._history[num] != null)
							{
								this._historyCurrent = num;
							}
						}
						if (this._historyCurrent != -1)
						{
							this._consoleInput = this._history[this._historyCurrent];
						}
					}
					return true;
				case KeyCode.DownArrow:
					if (this._showConsole)
					{
						if (this._historyCurrent != -1 && this._historyCurrent != this._historyEnd)
						{
							this._historyCurrent = (this._historyCurrent + 1) % this._history.Length;
						}
						if (this._historyCurrent != -1)
						{
							this._consoleInput = this._history[this._historyCurrent];
						}
						this._selectConsoleText = true;
					}
					return true;
				case KeyCode.RightArrow:
					if (!string.IsNullOrEmpty(this._autocomplete))
					{
						this._consoleInput = this._autocomplete;
						this._selectConsoleText = true;
					}
					return true;
				default:
					if (keyCode == KeyCode.Return)
					{
						if (this._showConsole)
						{
							this.FinalizeConsoleInput();
						}
						return true;
					}
					if (keyCode != KeyCode.Quote && keyCode != KeyCode.BackQuote)
					{
						if (keyCode != KeyCode.RightControl)
						{
							return false;
						}
						this._logs.Clear();
						return true;
					}
					break;
				case KeyCode.F1:
					this.ToggleConsole();
					return true;
				case KeyCode.F2:
					break;
				case KeyCode.F3:
					this.TogglePlayerStats();
					return true;
				}
				this.ToggleOverlay();
				return true;
			}
			if (UnityEngine.Event.current.type == EventType.KeyUp)
			{
				this._consoleInput = this._consoleInput.TrimStart(new char[]
				{
					' '
				});
				this._autocomplete = this._availableConsoleMethods.Keys.FirstOrDefault((string acm) => acm.IndexOf(this._consoleInput) == 0);
			}
			return false;
		}

		
		private void CheckDisplayState()
		{
			bool flag = this._showConsole || this._showLog || this._showOverlay || this._showGamePadWheel || this._showPlayerStats;
			if (flag)
			{
				if (!base.enabled)
				{
					base.StopCoroutine(this._inputRoutine);
					base.enabled = true;
				}
			}
			else if (base.enabled)
			{
				if (!this._routineMB)
				{
					this._routineMB = base.gameObject.AddComponent<DebugConsoleRoutine>();
				}
				this._inputRoutine = this._routineMB.StartCoroutine(this.SilentInput());
				base.enabled = false;
			}
		}

		
		private bool CheckPS4Debug()
		{
			return TheForest.Utils.Input.DS4 != null && TheForest.Utils.Input.DS4.IsTouching(0);
		}

		
		private IEnumerator SilentInput()
		{
			yield return null;
			for (;;)
			{
				if (UnityEngine.Input.anyKeyDown)
				{
					if (UnityEngine.Input.GetKeyDown(KeyCode.F1))
					{
						yield return null;
						this.ToggleConsole();
					}
					else if (UnityEngine.Input.GetKeyDown(KeyCode.Quote) || UnityEngine.Input.GetKeyDown(KeyCode.BackQuote) || UnityEngine.Input.GetKeyDown(KeyCode.F2))
					{
						yield return null;
						this.ToggleOverlay();
					}
					if (UnityEngine.Input.GetKeyDown(KeyCode.F3))
					{
						yield return null;
						this.TogglePlayerStats();
					}
				}
				if (TheForest.Utils.Input.GetButtonDown("Debug") || this.CheckPS4Debug())
				{
					this.ToggleGamePadWheel();
				}
				yield return null;
			}
			yield break;
		}

		
		public static void AddCounter(Type t)
		{
			if (DebugConsole.Counters == null)
			{
				DebugConsole.Counters = new Dictionary<Type, int>();
			}
			if (!DebugConsole.Counters.ContainsKey(t))
			{
				DebugConsole.Counters.Add(t, 1);
			}
			else
			{
				Dictionary<Type, int> counters;
				Dictionary<Type, int> dictionary = counters = DebugConsole.Counters;
				int num = counters[t];
				dictionary[t] = num + 1;
			}
		}

		
		public static void RemoveCounter(Type t)
		{
			if (DebugConsole.Counters != null)
			{
				if (!DebugConsole.Counters.ContainsKey(t))
				{
					Debug.Log("Removing more " + t + " than there was added to DebugConsole counters");
					DebugConsole.Counters[t] = -1;
				}
				else
				{
					Dictionary<Type, int> counters;
					Dictionary<Type, int> dictionary = counters = DebugConsole.Counters;
					int num = counters[t];
					dictionary[t] = num - 1;
				}
			}
		}

		
		private void CheckAmount(Type t, int amount)
		{
			UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(t);
			int num = (array == null) ? 0 : array.Length;
			Debug.Log(string.Concat(new object[]
			{
				"GameObject.FindObjectsOfType<",
				t,
				">().Length = ",
				num,
				" (",
				num == amount,
				")"
			}));
		}

		
		public void LogCallback(string condition, string stackTrace, LogType type)
		{
			LogContent logContent = this._lastLog;
			if (logContent == null || logContent.type != type || !logContent.content.text.Equals(condition))
			{
				if (this._logs.Count == this._maxLogs)
				{
					logContent = this._logs.Dequeue();
				}
				else
				{
					logContent = new LogContent
					{
						content = new GUIContent()
					};
				}
				logContent.content.text = condition;
				logContent.content.tooltip = condition + "\n\n--------------\n" + stackTrace;
				logContent.type = type;
				logContent.amount = 1;
				this._logs.Enqueue(logContent);
				this._lastLog = logContent;
			}
			else if (logContent != null)
			{
				logContent.amount++;
			}
		}

		
		private IEnumerator Report(string message)
		{
			string formated = string.Format(this._reportFormat, string.Concat(new object[]
			{
				Environment.UserName,
				" (",
				SteamManager.BetaName,
				" ",
				SteamManager.BuildId,
				") ",
				BoltNetwork.isRunning ? ((!BoltNetwork.isServer) ? "Client" : "Host") : "SP",
				" --",
				UnityEngine.Random.Range(0, 1337)
			}), message.Replace("\"", "\\\""));
			formated = formated.Replace("\r", string.Empty).Replace("\n", "\\n");
			WWW w = new WWW(this._reportUrl, Encoding.ASCII.GetBytes(formated));
			yield return w;
			yield break;
		}

		
		public bool IsAlphaNum(string s)
		{
			return s.IndexOfAny(this._alphaNum) >= 0;
		}

		
		private void HandleConsoleInput()
		{
			if (this._historyEnd == -1)
			{
				this._historyEnd = 0;
				this._history[0] = this._consoleInput;
			}
			else if (this._consoleInput != this._history[this._historyEnd])
			{
				this._historyEnd = (this._historyEnd + 1) % this._history.Length;
				this._history[this._historyEnd] = this._consoleInput;
			}
			this._historyCurrent = -1;
			List<string> list = this._consoleInput.Split(new char[]
			{
				' '
			}).ToList<string>();
			string text = list[0].ToLower();
			if (this._availableConsoleMethods.ContainsKey(text))
			{
				list.RemoveAt(0);
				int num = 1;
				if (list.Any((string a) => a.StartsWith("--")))
				{
					string text2 = list.First((string a) => a.StartsWith("--"));
					list.Remove(text2);
					num = int.Parse(text2.Substring(2));
				}
				string text3 = (list.Count <= 0) ? null : string.Join(" ", list.ToArray(), 0, list.Count);
				if (num > 1)
				{
					Debug.Log(string.Concat(new object[]
					{
						"$> Being repeat command '",
						text,
						" ",
						text3,
						"'",
						num,
						" times"
					}));
				}
				while (num-- > 0)
				{
					try
					{
						this._availableConsoleMethods[text].Invoke((!this._availableConsoleMethods[text].IsStatic) ? this : null, new object[]
						{
							text3
						});
					}
					catch (Exception ex)
					{
						Debug.Log(string.Concat(new object[]
						{
							"$> '",
							text,
							" ",
							text3,
							"' #",
							num,
							" exception:\n",
							ex
						}));
					}
				}
			}
			else if (!string.IsNullOrEmpty(list[0]))
			{
				Debug.Log("$> Unknown console command '" + list[0] + "'");
			}
		}

		
		private void _help(object o)
		{
			string text = string.Empty;
			foreach (string str in this._availableConsoleMethods.Keys)
			{
				text = text + "$> " + str + "\n";
			}
			Debug.Log("$> DebugConsole Commands Help:\n" + text);
		}

		
		private void _clear(object o)
		{
			this._logs.Clear();
			Debug.Log("$> Cleared DebugConsole logs");
		}

		
		private void _togglePlayerStats(object o)
		{
			this.TogglePlayerStats();
		}

		
		private void _toggleOverlay(object o)
		{
			this.ToggleOverlay();
		}

		
		private void _reporterrorsnow(string message)
		{
			if (this._logs.Count > 0)
			{
				string str = (from l in this._logs
				where l.type == LogType.Error || l.type == LogType.Exception
				select l.content.tooltip).Join("\n\n***\n\n");
				base.StartCoroutine(this.Report("[" + message + "]\n\n" + str));
				Debug.Log("$> Reported current DebugConsole error and exceptions logs");
			}
			else
			{
				Debug.Log("$> Nothing to report in DebugConsole");
			}
		}

		
		private void _reportwarningsnow(string message)
		{
			if (this._logs.Count > 0)
			{
				string str = (from l in this._logs
				where l.type == LogType.Warning
				select l.content.tooltip).Join("\n\n***\n\n");
				base.StartCoroutine(this.Report("[" + message + "]\n\n" + str));
				Debug.Log("$> Reported current DebugConsole error and exceptions logs");
			}
			else
			{
				Debug.Log("$> Nothing to report in DebugConsole");
			}
		}

		
		private void _reportlogsnow(string message)
		{
			if (this._logs.Count > 0)
			{
				string str = (from l in this._logs
				where l.type == LogType.Log
				select l.content.tooltip).Join("\n\n***\n\n");
				base.StartCoroutine(this.Report("[" + message + "]\n\n" + str));
				Debug.Log("$> Reported current DebugConsole error and exceptions logs");
			}
			else
			{
				Debug.Log("$> Nothing to report in DebugConsole");
			}
		}

		
		private void _spawnitem(string id)
		{
			int num = 0;
			if (!int.TryParse(id, out num))
			{
				Debug.Log("$> usage: invalid item id. Must be an integer number!");
				return;
			}
			if (!ItemDatabase.IsItemidValid(num))
			{
				Debug.Log("$> usage: no item number (" + id + ") found!");
				return;
			}
			Vector3 position = LocalPlayer.Transform.position + 2f * LocalPlayer.Transform.forward;
			ItemUtils.SpawnItem(num, position, Quaternion.identity, false);
		}

		
		private void _spawnanimal(string type)
		{
			this.spawnAnAnimal(type, false);
		}

		
		private void _spawnanimalquiet(string type)
		{
			this.spawnAnAnimal(type, true);
		}

		
		private void spawnAnAnimal(string type, bool trapped)
		{
			List<string> list = new List<string>
			{
				"rabbit",
				"lizard",
				"deer",
				"turtle",
				"tortoise",
				"raccoon",
				"squirrel",
				"boar",
				"crocodile"
			};
			string prefabName = type + "Go";
			if (!list.Contains(type) || !PoolManager.Pools["creatures"].prefabs.ContainsKey(prefabName))
			{
				string text = "$> usage: spawnanimal <";
				foreach (string str in list)
				{
					text = text + str + " | ";
				}
				text += ">";
				Debug.Log(text);
				return;
			}
			Transform transform = PoolManager.Pools["creatures"].Spawn(prefabName, LocalPlayer.Transform.position + 2f * LocalPlayer.Transform.forward, Quaternion.identity);
			if (transform)
			{
				if (BoltNetwork.isServer && transform.gameObject.GetComponent<CoopAnimalServer>())
				{
					AnimalSpawnController.AttachAnimalToNetwork(null, transform.gameObject);
				}
				if (trapped)
				{
					GameObject gameObject = new GameObject();
					gameObject.transform.position = LocalPlayer.Transform.position + 2f * LocalPlayer.Transform.forward;
					animalHealth componentInChildren = transform.GetComponentInChildren<animalHealth>();
					if (componentInChildren != null)
					{
						componentInChildren.Trap = gameObject;
					}
					transform.SendMessage("startUpdateSpawn");
					transform.gameObject.SendMessageUpwards("setTrapped", gameObject, SendMessageOptions.DontRequireReceiver);
				}
			}
		}

		
		private void _spawnmutant(string type)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("instantMutantSpawner"), LocalPlayer.Transform.position + new Vector3(0f, 1f, 2f), Quaternion.identity) as GameObject;
			switch (type)
			{
			case "male":
				gameObject.GetComponent<spawnMutants>().amount_male = 1;
				break;
			case "female":
				gameObject.GetComponent<spawnMutants>().amount_female = 1;
				break;
			case "female_skinny":
				gameObject.GetComponent<spawnMutants>().amount_female_skinny = 1;
				break;
			case "male_skinny":
				gameObject.GetComponent<spawnMutants>().amount_male_skinny = 1;
				break;
			case "pale":
				gameObject.GetComponent<spawnMutants>().amount_pale = 1;
				break;
			case "pale_skinny":
				gameObject.GetComponent<spawnMutants>().amount_skinny_pale = 1;
				break;
			case "fireman":
				gameObject.GetComponent<spawnMutants>().amount_fireman = 1;
				break;
			case "vags":
				gameObject.GetComponent<spawnMutants>().amount_vags = 1;
				break;
			case "armsy":
				gameObject.GetComponent<spawnMutants>().amount_armsy = 1;
				break;
			case "baby":
				gameObject.GetComponent<spawnMutants>().amount_baby = 1;
				break;
			case "fat":
				gameObject.GetComponent<spawnMutants>().amount_fat = 1;
				break;
			case "skinnedMale":
				gameObject.GetComponent<spawnMutants>().amount_pale = 1;
				gameObject.GetComponent<spawnMutants>().skinnedTribe = true;
				gameObject.GetComponent<spawnMutants>().pale = true;
				break;
			}
		}

		
		private void _spawnRegularFamily(Transform target)
		{
			if (BoltNetwork.isClient)
			{
				debugCommand debugCommand = debugCommand.Create(GlobalTargets.OnlyServer);
				debugCommand.input = "_spawnRegularFamily";
				debugCommand.target = LocalPlayer.Transform.GetComponent<BoltEntity>();
				debugCommand.Send();
				return;
			}
			if (target == null)
			{
				target = LocalPlayer.Transform;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("instantMutantSpawner"), target.position + new Vector3(0f, 1f, 2f), Quaternion.identity) as GameObject;
			spawnMutants component = gameObject.GetComponent<spawnMutants>();
			component.amount_armsy = 0;
			component.amount_baby = 0;
			component.amount_female = 1;
			component.amount_female_skinny = 0;
			component.amount_fireman = 0;
			component.amount_male = 4;
			component.amount_pale = 0;
			component.amount_male_skinny = 0;
			component.amount_vags = 0;
			component.amount_fat = 0;
			component.amount_armsy = 0;
			component.leader = true;
			component.pale = false;
		}

		
		private void _spawnPaintedFamily(Transform target)
		{
			if (BoltNetwork.isClient)
			{
				debugCommand debugCommand = debugCommand.Create(GlobalTargets.OnlyServer);
				debugCommand.input = "_spawnPaintedFamily";
				debugCommand.target = LocalPlayer.Transform.GetComponent<BoltEntity>();
				debugCommand.Send();
				return;
			}
			if (target == null)
			{
				target = LocalPlayer.Transform;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("instantMutantSpawner"), target.position + new Vector3(0f, 1f, 2f), Quaternion.identity) as GameObject;
			spawnMutants component = gameObject.GetComponent<spawnMutants>();
			component.amount_armsy = 0;
			component.amount_baby = 0;
			component.amount_female = 1;
			component.amount_female_skinny = 0;
			component.amount_fireman = 0;
			component.amount_male = 4;
			component.amount_pale = 0;
			component.amount_male_skinny = 0;
			component.amount_vags = 0;
			component.amount_fat = 0;
			component.amount_armsy = 0;
			component.leader = true;
			component.paintedTribe = true;
			component.pale = false;
		}

		
		private void _spawnSkinnedFamily(Transform target)
		{
			if (BoltNetwork.isClient)
			{
				debugCommand debugCommand = debugCommand.Create(GlobalTargets.OnlyServer);
				debugCommand.input = "_spawnSkinnedFamily";
				debugCommand.target = LocalPlayer.Transform.GetComponent<BoltEntity>();
				debugCommand.Send();
				return;
			}
			if (target == null)
			{
				target = LocalPlayer.Transform;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("instantMutantSpawner"), target.position + new Vector3(0f, 1f, 2f), Quaternion.identity) as GameObject;
			spawnMutants component = gameObject.GetComponent<spawnMutants>();
			component.amount_armsy = 0;
			component.amount_baby = 0;
			component.amount_female = 0;
			component.amount_female_skinny = 0;
			component.amount_fireman = 0;
			component.amount_male = 0;
			component.amount_pale = 2;
			component.amount_skinny_pale = 3;
			component.amount_male_skinny = 0;
			component.amount_vags = 0;
			component.amount_fat = 0;
			component.amount_armsy = 0;
			component.leader = true;
			component.paintedTribe = false;
			component.pale = true;
			component.skinnedTribe = true;
		}

		
		private void _spawnSkinnyFamily(Transform target)
		{
			if (BoltNetwork.isClient)
			{
				debugCommand debugCommand = debugCommand.Create(GlobalTargets.OnlyServer);
				debugCommand.input = "_spawnSkinnyFamily";
				debugCommand.target = LocalPlayer.Transform.GetComponent<BoltEntity>();
				debugCommand.Send();
				return;
			}
			if (target == null)
			{
				target = LocalPlayer.Transform;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("instantMutantSpawner"), target.position + new Vector3(0f, 1f, 2f), Quaternion.identity) as GameObject;
			spawnMutants component = gameObject.GetComponent<spawnMutants>();
			component.amount_armsy = 0;
			component.amount_baby = 0;
			component.amount_female = 0;
			component.amount_female_skinny = 2;
			component.amount_fireman = 0;
			component.amount_male = 0;
			component.amount_pale = 0;
			component.amount_male_skinny = 3;
			component.amount_vags = 0;
			component.amount_fat = 0;
			component.amount_armsy = 0;
			component.leader = true;
			component.pale = false;
		}

		
		private void _count(string name)
		{
			if (!string.IsNullOrEmpty(name))
			{
				UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(Type.GetType(name));
				Debug.Log(string.Concat(new object[]
				{
					"$> Found ",
					(array == null) ? 0 : array.Length,
					" ",
					name
				}));
			}
			else
			{
				Debug.Log("$> usage: count <ComponentName>");
			}
		}

		
		private void _counttag(string name)
		{
			if (!string.IsNullOrEmpty(name))
			{
				GameObject[] array = GameObject.FindGameObjectsWithTag(name);
				Debug.Log(string.Concat(new object[]
				{
					"$> Found ",
					(array == null) ? 0 : array.Length,
					" GameObjects with tag: ",
					name
				}));
			}
			else
			{
				Debug.Log("$> usage: counttag <tag>");
			}
		}

		
		private void _enablego(string name)
		{
			if (!string.IsNullOrEmpty(name))
			{
				GameObject gameObject = this._disabledGOs.FirstOrDefault((GameObject g) => g.name == name);
				if (gameObject)
				{
					gameObject.SetActive(true);
					this._disabledGOs.Remove(gameObject);
					Debug.Log("$> enabling GameObject: " + name);
				}
				else
				{
					Debug.Log("$> enablego: didn't find any GameObject named '" + name + "'");
				}
			}
			else
			{
				Debug.Log("$> usage: enablego <GameObjectName>");
			}
		}

		
		private void _disablego(string name)
		{
			if (!string.IsNullOrEmpty(name))
			{
				GameObject gameObject = GameObject.Find(name);
				if (gameObject)
				{
					gameObject.SetActive(false);
					if (!this._disabledGOs.Contains(gameObject))
					{
						this._disabledGOs.Add(gameObject);
					}
					Debug.Log("$> disabling GameObject: " + name);
				}
				else
				{
					Debug.Log("$> disablego: didn't find any GameObject named '" + name + "'");
				}
			}
			else
			{
				Debug.Log("$> usage: disablego <GameObjectName>");
			}
		}

		
		private void _togglego(string name)
		{
			if (!string.IsNullOrEmpty(name))
			{
				GameObject gameObject = this._disabledGOs.FirstOrDefault((GameObject g) => g.name == name);
				if (gameObject)
				{
					gameObject.SetActive(true);
					this._disabledGOs.Remove(gameObject);
					Debug.Log("$> enabling GameObject: " + name);
				}
				else
				{
					GameObject gameObject2 = GameObject.Find(name);
					if (gameObject2)
					{
						gameObject2.SetActive(false);
						if (!this._disabledGOs.Contains(gameObject2))
						{
							this._disabledGOs.Add(gameObject2);
						}
						Debug.Log("$> disabling GameObject: " + name);
					}
					else
					{
						Debug.Log("$> togglego: didn't find any GameObject named '" + name + "'");
					}
				}
			}
			else
			{
				Debug.Log("$> usage: togglego <GameObjectName>");
			}
		}

		
		private void _destroy(string name)
		{
			if (!string.IsNullOrEmpty(name))
			{
				GameObject gameObject = GameObject.Find(name);
				if (gameObject)
				{
					Debug.Log("$> Found " + name + ", destroying now");
					UnityEngine.Object.Destroy(gameObject);
				}
				else
				{
					Debug.Log("$> " + name + " not found, unable to destroy");
				}
			}
			else
			{
				Debug.Log("$> usage: destroy <GameObjectName>");
			}
		}

		
		private void _save(object o)
		{
			LocalPlayer.Stats.JustSave();
			Debug.Log("$> Saved game");
		}

		
		private void _listitems(object o)
		{
			string text = "$> ";
			foreach (Item item in from i in ItemDatabase.Items
			orderby i._name
			select i)
			{
				string text2 = text;
				text = string.Concat(new object[]
				{
					text2,
					item._name,
					" [",
					item._id,
					"], "
				});
			}
			Debug.Log("$> Item names [id]:\n" + text);
		}

		
		private void _additem(string nameOrId)
		{
			if (!string.IsNullOrEmpty(nameOrId))
			{
				Item item;
				int itemId;
				if (int.TryParse(nameOrId, out itemId))
				{
					item = ItemDatabase.Items.FirstOrDefault((Item i) => i._id == itemId);
				}
				else
				{
					string lowerName = nameOrId.ToLower();
					item = ItemDatabase.Items.FirstOrDefault((Item i) => i._name.ToLower() == lowerName);
				}
				if (item != null)
				{
					Debug.Log("$> Found item " + item._name + ", adding now");
					LocalPlayer.Inventory.AddItem(item._id, 100000, false, false, null);
				}
				else
				{
					Debug.Log("$> Item " + nameOrId + " not found, please check command 'listitems'");
				}
			}
			else
			{
				Debug.Log("$> usage: additem <itemName|itemId>");
			}
		}

		
		private void _removeitem(string nameOrId)
		{
			if (!string.IsNullOrEmpty(nameOrId))
			{
				Item item;
				int itemId;
				if (int.TryParse(nameOrId, out itemId))
				{
					item = ItemDatabase.Items.FirstOrDefault((Item i) => i._id == itemId);
				}
				else
				{
					string lowerName = nameOrId.ToLower();
					item = ItemDatabase.Items.FirstOrDefault((Item i) => i._name.ToLower() == lowerName);
				}
				if (item != null)
				{
					Debug.Log("$> Found item " + item._name + ", removing now");
					LocalPlayer.Inventory.RemoveItem(item._id, 100000, true, true);
				}
				else
				{
					Debug.Log("$> Item " + nameOrId + " not found, please check command 'listitems'");
				}
			}
			else
			{
				Debug.Log("$> usage: additem <itemName|itemId>");
			}
		}

		
		private void _spawnpickup(string nameOrId)
		{
			if (!string.IsNullOrEmpty(nameOrId))
			{
				Item item;
				int itemId;
				if (int.TryParse(nameOrId, out itemId))
				{
					item = ItemDatabase.Items.FirstOrDefault((Item i) => i._id == itemId);
				}
				else
				{
					string lowerName = nameOrId.ToLower();
					item = ItemDatabase.Items.FirstOrDefault((Item i) => i._name.ToLower() == lowerName);
				}
				if (item != null)
				{
					Debug.Log("$> Found item " + item._name + ", spawning prefab now");
					LocalPlayer.Inventory.FakeDrop(item._id, null);
				}
				else
				{
					Debug.Log("$> Item " + nameOrId + " not found, please check command 'listitems'");
				}
			}
			else
			{
				Debug.Log("$> usage: spawnpickup <itemName|itemId>");
			}
		}

		
		private void _spawnAllPickups(object o)
		{
			Item item = null;
			Debug.Log("$> Begin spawning all items with fake drop now");
			foreach (int num in LocalPlayer.Inventory.InventoryItemViewsCache.Keys)
			{
				try
				{
					item = ItemDatabase.ItemById(num);
					LocalPlayer.Inventory.FakeDrop(num, null);
				}
				catch (Exception ex)
				{
					Debug.Log(string.Concat(new object[]
					{
						"Error spawning ",
						(item != null) ? item._name : num.ToString(),
						"\n\n",
						ex
					}));
				}
			}
			Debug.Log("$> Spawning all items done");
		}

		
		private void _addAllItems(object o)
		{
			foreach (Item item in ItemDatabase.Items)
			{
				try
				{
					if (item._maxAmount >= 0 && !item.MatchType(Item.Types.Story) && LocalPlayer.Inventory.InventoryItemViewsCache.ContainsKey(item._id))
					{
						LocalPlayer.Inventory.AddItem(item._id, 100000, true, false, null);
					}
				}
				catch (Exception ex)
				{
				}
			}
			Debug.Log("$> Added all non-story items");
		}

		
		private void _addAllStoryItems(object o)
		{
			foreach (Item item in ItemDatabase.Items)
			{
				try
				{
					if (item._maxAmount >= 0 && item.MatchType(Item.Types.Story) && LocalPlayer.Inventory.InventoryItemViewsCache.ContainsKey(item._id))
					{
						LocalPlayer.Inventory.AddItem(item._id, 100000, true, false, null);
					}
				}
				catch (Exception ex)
				{
				}
			}
			Debug.Log("$> Added all story items");
		}

		
		private void _decaynextmeat(object o)
		{
			if (LocalPlayer.GameObject)
			{
				ItemDecayMachine componentInChildren = LocalPlayer.GameObject.GetComponentInChildren<ItemDecayMachine>();
				if (componentInChildren)
				{
					if (componentInChildren.DecayNext())
					{
						Debug.Log("$> Decayed meat");
					}
					else
					{
						Debug.Log("$> No more meat to decay");
					}
				}
			}
		}

		
		private void _veganmode(string onoff)
		{
			if (onoff == "on")
			{
				Cheats.NoEnemies = true;
				Debug.Log("$> Vegan Mode enabled (reloading is required to be effective)");
			}
			else if (onoff == "off")
			{
				Cheats.NoEnemies = false;
				Debug.Log("$> Vegan Mode disabled (reloading is required to be effective)");
			}
			else
			{
				Debug.Log("$> usage: veganmode <on|off>");
			}
		}

		
		private void _loghack(string onoff)
		{
			if (onoff == "on")
			{
				LocalPlayer.Inventory.Logs._infiniteLogHack = true;
				Debug.Log("$> Log Hack enabled");
				int id = ItemDatabase.ItemByName("Log")._id;
				if (!LocalPlayer.Inventory.Owns(id, true))
				{
					LocalPlayer.Inventory.AddItem(id, 1, false, false, null);
				}
			}
			else if (onoff == "off")
			{
				LocalPlayer.Inventory.Logs._infiniteLogHack = false;
				Debug.Log("$> Log Hack disabled");
			}
			else
			{
				Debug.Log("$> usage: loghack <on|off>");
			}
		}

		
		private void _itemhack(string onoff)
		{
			if (onoff == "on")
			{
				LocalPlayer.Inventory.ItemFilter = new InventoryItemFilter_Unlimited();
				Debug.Log("$> Item Hack enabled");
			}
			else if (onoff == "off")
			{
				LocalPlayer.Inventory.ItemFilter = null;
				Debug.Log("$> Item Hack disabled");
			}
			else
			{
				Debug.Log("$> usage: itemhack <on|off>");
			}
		}

		
		private void _energyhack(string onoff)
		{
			if (onoff == "on")
			{
				Cheats.InfiniteEnergy = true;
				Debug.Log("$> Energy Hack enabled");
			}
			else if (onoff == "off")
			{
				Cheats.InfiniteEnergy = false;
				Debug.Log("$> Energy Hack disabled");
			}
			else
			{
				Debug.Log("$> usage: energyhack <on|off>");
			}
		}

		
		private void _godmode(string onoff)
		{
			if (onoff == "toggle")
			{
				onoff = ((!Cheats.GodMode) ? "on" : "off");
			}
			if (onoff == "on")
			{
				this._setstat("full");
				this._survival("off");
				this._energyhack("on");
				Cheats.GodMode = true;
				Debug.Log("$> God Mode enabled");
			}
			else if (onoff == "off")
			{
				this._survival("on");
				this._energyhack("off");
				Cheats.GodMode = false;
				Debug.Log("$> God Mode disabled");
			}
			else
			{
				Debug.Log("$> usage: godmode <on|off>");
			}
		}

		
		private void _cancelallghosts(object o)
		{
			Craft_Structure[] array = UnityEngine.Object.FindObjectsOfType<Craft_Structure>();
			if (array != null && array.Length > 0)
			{
				foreach (Craft_Structure craft_Structure in array)
				{
					craft_Structure.CancelBlueprint();
				}
			}
		}

		
		private void _buildallghosts(object o)
		{
			Scene.ActiveMB.StartCoroutine(this.BuildAllGhostsRoutine());
		}

		
		private IEnumerator BuildAllGhostsRoutine()
		{
			Craft_Structure[] css = UnityEngine.Object.FindObjectsOfType<Craft_Structure>();
			if (css != null && css.Length > 0)
			{
				Debug.Log("$> Begin build all " + css.Length + " ghosts");
				foreach (Craft_Structure cs in css)
				{
					ReceipeIngredient[] presentAll = cs.GetPresentIngredients();
					List<Craft_Structure.BuildIngredients> requiredAll = cs._requiredIngredients;
					int i = 0;
					while (i < requiredAll.Count && i < presentAll.Length)
					{
						if (requiredAll[i]._amount != presentAll[i]._amount)
						{
							Craft_Structure.BuildIngredients needed = requiredAll[i];
							ReceipeIngredient present = presentAll[i];
							for (int j = needed._amount - present._amount; j > 0; j--)
							{
								cs.SendMessage("AddIngredient", i);
							}
							if (BoltNetwork.isRunning)
							{
								yield return null;
							}
						}
						i++;
					}
					yield return null;
				}
				Debug.Log("$> Done building all " + css.Length + " ghosts");
			}
			else
			{
				Debug.Log("$> found no ghost buildings to complete");
			}
			yield break;
		}

		
		private void _placebuiltobjects(string input)
		{
			string[] array = input.Split(new char[]
			{
				' '
			});
			string name;
			int num;
			try
			{
				name = array[0];
				num = int.Parse(array[1]);
			}
			catch
			{
				Debug.Log("$> Usage: placebuiltobjects [name] [number]");
				return;
			}
			Scene.ActiveMB.StartCoroutine(this.PlaceBuiltObjectsRoutine(name, num));
		}

		
		private IEnumerator PlaceBuiltObjectsRoutine(string name, int num)
		{
			int layers = LayerMask.GetMask(new string[]
			{
				"Terrain",
				"Prop",
				"ReflectBig",
				"Cave"
			});
			float spread = 6f;
			float gridSize = Mathf.Floor(Mathf.Sqrt((float)num));
			Vector3 pos = LocalPlayer.Transform.position;
			Vector3 playerForward = LocalPlayer.Transform.forward;
			Vector3 playerRight = LocalPlayer.Transform.right;
			LOD_Trees[] treeLods = UnityEngine.Object.FindObjectsOfType<LOD_Trees>();
			bool foundObject = false;
			pos += playerForward * spread - playerRight * (gridSize / 2f * spread);
			for (int i = 0; i < Prefabs.Instance.Constructions._blueprints.Count; i++)
			{
				BuildingBlueprint bp = Prefabs.Instance.Constructions._blueprints[i];
				if (bp._builtPrefab && bp._builtPrefab.name == name)
				{
					foundObject = true;
					for (int j = 0; j < num; j++)
					{
						RaycastHit hit;
						if (Physics.SphereCast(pos + Vector3.up * 500f, spread / 2f, Vector3.down, out hit, 1000f, layers))
						{
							try
							{
								(from tl in treeLods
								where Vector3.Distance(tl.transform.position, this.<hit>__11.point) < this.<spread>__1 * 2f
								select tl).ForEach(delegate(LOD_Trees tl)
								{
									tl.enabled = false;
								});
								GameObject built = UnityEngine.Object.Instantiate<GameObject>(bp._builtPrefab);
								built.transform.position = hit.point;
								built.transform.rotation = ((!bp._allowInTree) ? Quaternion.FromToRotation(Vector3.up, hit.normal) : Quaternion.identity);
							}
							catch
							{
							}
						}
						yield return null;
						Vector3 prevPos = pos;
						if ((float)j % gridSize == gridSize - 1f)
						{
							pos += playerForward * spread - playerRight * ((gridSize - 1f) * spread);
						}
						else
						{
							pos += playerRight * spread;
						}
						Debug.DrawLine(pos, prevPos, Color.cyan, 20f, false);
					}
				}
			}
			if (!foundObject)
			{
				Debug.Log("$> placebuiltobjects called with unknown object " + name);
			}
			yield break;
		}

		
		private void _placeallghosts(object o)
		{
			Scene.ActiveMB.StartCoroutine(this.PlaceAllGhostsRoutine());
		}

		
		private IEnumerator PlaceAllGhostsRoutine()
		{
			int layers = LayerMask.GetMask(new string[]
			{
				"Terrain",
				"Prop",
				"ReflectBig",
				"Cave"
			});
			float spread = 12f;
			float gridSize = 9f;
			Vector3 pos = LocalPlayer.Transform.position;
			Vector3 playerForward = LocalPlayer.Transform.forward;
			Vector3 playerRight = LocalPlayer.Transform.right;
			LOD_Trees[] treeLods = UnityEngine.Object.FindObjectsOfType<LOD_Trees>();
			pos += playerForward * spread - playerRight * (gridSize / 2f * spread);
			Debug.Log("$> Begin place all " + Prefabs.Instance.Constructions._blueprints.Count + " blueprint ghosts");
			for (int i = 0; i < Prefabs.Instance.Constructions._blueprints.Count; i++)
			{
				BuildingBlueprint bp = Prefabs.Instance.Constructions._blueprints[i];
				if (bp._ghostPrefab && bp._ghostPrefab.GetComponent<ICoopStructure>() == null && !bp._ghostPrefab.GetComponent<WallArchitect>() && !bp._ghostPrefab.GetComponent<WallDefensiveChunkReinforcement>())
				{
					RaycastHit hit;
					if (Physics.SphereCast(pos + Vector3.up * 500f, spread / 2f, Vector3.down, out hit, 1000f, layers))
					{
						try
						{
							(from tl in treeLods
							where Vector3.Distance(tl.transform.position, this.<hit>__9.point) < this.<spread>__1 * 2f
							select tl).ForEach(delegate(LOD_Trees tl)
							{
								tl.enabled = false;
							});
							GameObject ghost = UnityEngine.Object.Instantiate<GameObject>(bp._ghostPrefab);
							Craft_Structure cs = ghost.GetComponentInChildren<Craft_Structure>();
							if (cs)
							{
								LocalPlayer.Create.InitPlacer(bp);
								ghost.transform.position = hit.point + LocalPlayer.Create.GetGhostOffsetWithPlacer(ghost);
								ghost.transform.rotation = ((!bp._allowInTree) ? Quaternion.FromToRotation(Vector3.up, hit.normal) : Quaternion.identity);
								ghost.SendMessage("OnPlaced", SendMessageOptions.DontRequireReceiver);
								cs.Initialize();
								cs.GrabExit();
							}
							else
							{
								UnityEngine.Object.Destroy(ghost);
							}
						}
						catch
						{
						}
					}
					yield return null;
				}
				Vector3 prevPos = pos;
				if ((float)i % gridSize == gridSize - 1f)
				{
					pos += playerForward * spread - playerRight * ((gridSize - 1f) * spread);
				}
				else
				{
					pos += playerRight * spread;
				}
				Debug.DrawLine(pos, prevPos, Color.cyan, 20f, false);
			}
			Debug.Log("$> Done placing all " + Prefabs.Instance.Constructions._blueprints.Count + " blueprint ghosts");
			yield break;
		}

		
		private void _buildhack(string onoff)
		{
			if (onoff == "toggle")
			{
				onoff = ((!Cheats.Creative) ? "on" : "off");
			}
			if (onoff == "on")
			{
				Debug.Log("$> Build hack enabled");
				Cheats.Creative = true;
			}
			else if (onoff == "off")
			{
				Cheats.Creative = false;
				Debug.Log("$> Build hack disabled");
			}
			else
			{
				Debug.Log("$> usage: buildhack <on|off>");
			}
		}

		
		private void _survival(string onoff)
		{
			if (onoff == "on")
			{
				Debug.Log("$> Survival features enabled");
				Cheats.NoSurvival = false;
			}
			else if (onoff == "off")
			{
				Cheats.NoSurvival = true;
				Debug.Log("$> Survival features disabled");
			}
			else
			{
				Debug.Log("$> usage: survival <on|off>");
			}
		}

		
		private void _workscheduler(string trycatch)
		{
			if (trycatch == "try")
			{
				WorkScheduler.ToggleTryCatchWork(true);
				Debug.Log("$> WorkSchedulerBatch TryCatch is now used");
			}
			else if (trycatch == "notry")
			{
				WorkScheduler.ToggleTryCatchWork(false);
				Debug.Log("$> WorkSchedulerBatch TryCatch is no longer used");
			}
			else
			{
				Debug.Log("$> usage: workscheduler <try|notry>");
			}
		}

		
		private void _pmActiveStateLabels(string onoff)
		{
			PlayMakerGUI playMakerGUI = Resources.FindObjectsOfTypeAll<PlayMakerGUI>().FirstOrDefault<PlayMakerGUI>();
			if (onoff == "on" && playMakerGUI)
			{
				playMakerGUI.gameObject.SetActive(true);
				playMakerGUI.drawStateLabels = true;
				Debug.Log("$> Draw Active State Labels enabled");
			}
			else if (onoff == "off" && playMakerGUI)
			{
				playMakerGUI.gameObject.SetActive(false);
				playMakerGUI.drawStateLabels = false;
				Debug.Log("$> Draw Active State Labels disabled");
			}
			else
			{
				Debug.Log("$> usage: pmActiveStateLabels <on|off>");
			}
		}

		
		private void _timescale(string scaleStr)
		{
			if (!string.IsNullOrEmpty(scaleStr))
			{
				float num;
				if (float.TryParse(scaleStr, out num))
				{
					Time.timeScale = num;
					Debug.Log("$> Time Scale set to " + num);
				}
				else
				{
					Debug.Log("$> '" + scaleStr + "' is not a valid float value for timescale");
				}
			}
			else
			{
				Debug.Log("$> usage: timescale <scale>");
			}
		}

		
		private void _gametimescale(string scaleStr)
		{
			if (!string.IsNullOrEmpty(scaleStr))
			{
				float num;
				if (float.TryParse(scaleStr, out num))
				{
					TheForestAtmosphere.GameTimeScale = num;
					Debug.Log("$> Game Time Scale set to " + num);
				}
				else
				{
					Debug.Log("$> '" + scaleStr + "' is not a valid float value for gametimescale");
				}
			}
			else
			{
				Debug.Log("$> usage: gametimescale <scale>");
			}
		}

		
		private void _hitlocalplayer(string damageStr)
		{
			if (LocalPlayer.Stats)
			{
				int damage;
				if (int.TryParse(damageStr, out damage))
				{
					LocalPlayer.Stats.Hit(damage, false, PlayerStats.DamageType.Physical);
					Debug.Log("$> Hit local player with " + damageStr + " damage (not ignoring armor)");
				}
				else
				{
					Debug.Log("$> usage: hitlocalplayer <amount>");
				}
			}
			else
			{
				Debug.Log("$> Local player not found");
			}
		}

		
		private void _killlocalplayer(object o)
		{
			if (LocalPlayer.Stats)
			{
				LocalPlayer.Stats.Hit(1000, true, PlayerStats.DamageType.Physical);
				Debug.Log("$> Hit local player (1000 damage hit + ignore armor)");
			}
			else
			{
				Debug.Log("$> Local player not found");
			}
		}

		
		private void _killmefast(object o)
		{
			if (LocalPlayer.Stats)
			{
				LocalPlayer.Stats.KillMeFast();
			}
			else
			{
				Debug.Log("$> Local player not found");
			}
		}

		
		private void _revivelocalplayer(object o)
		{
			if (LocalPlayer.Stats)
			{
				if (LocalPlayer.Stats.Dead)
				{
					Item item = ItemDatabase.ItemByName("meds");
					ItemUtils.ApplyEffectsToStats(item._usedStatEffect, true, 1);
					if (item._usedSFX != Item.SFXCommands.None)
					{
						LocalPlayer.Inventory.SendMessage(item._usedSFX.ToString());
					}
					LocalPlayer.Stats.HealedMp();
					Debug.Log("$> Revived local player");
				}
				else
				{
					Debug.Log("$> Local player is alive, cannot be revived");
				}
			}
			else
			{
				Debug.Log("$> Local player not found");
			}
		}

		
		private void _setstat(string arg)
		{
			if (arg.ToLower() == "full")
			{
				LocalPlayer.Stats.Health = 100f;
				LocalPlayer.Stats.Energy = 100f;
				LocalPlayer.Stats.Stamina = 100f;
				LocalPlayer.Stats.Fullness = 100f;
				LocalPlayer.Stats.Thirst = 0f;
				Debug.Log("$> Local player's Health/Energy/Stamina/Fullness/Thirst all reset to max values");
			}
			else
			{
				string[] array = arg.Split(new char[]
				{
					' '
				});
				if (!string.IsNullOrEmpty(arg) && array.Length == 2)
				{
					Type typeFromHandle = typeof(PlayerStats);
					FieldInfo field = typeFromHandle.GetField(array[0], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					float num;
					if (float.TryParse(array[1], out num) && field != null)
					{
						if (field.FieldType == typeof(float))
						{
							field.SetValue(LocalPlayer.Stats, num);
						}
						else if (field.FieldType == typeof(int))
						{
							field.SetValue(LocalPlayer.Stats, (int)num);
						}
						Debug.Log(string.Concat(new object[]
						{
							"$> Local player's ",
							array[0],
							" set to ",
							num
						}));
					}
					else if (field != null)
					{
						Debug.Log(string.Concat(new object[]
						{
							"$> '",
							num,
							"' is not a valid value for player stat '",
							array[0],
							"'"
						}));
					}
					else
					{
						Debug.Log("$> '" + array[0] + "' is not a valid stat, should be one of: Health/Stamina/Energy/Fullness/BatteryCharge (Case Sensistive)");
					}
				}
				else
				{
					Debug.Log("$> usage: setstat <Health/Stamina/Energy/Fullness/BatteryCharge> <amount>");
				}
			}
		}

		
		private void _StewComboVerbose(string arg)
		{
			if (arg == "on")
			{
				StewCombo.Verbose = true;
				Debug.Log("$> StewCombo Verbose enabled");
			}
			else if (arg == "off")
			{
				StewCombo.Verbose = false;
				Debug.Log("$> StewCombo Verbose disabled");
			}
			else
			{
				Debug.Log("$> usage: StewComboVerbose <on|off>");
			}
		}

		
		private void _setrainbonus(string arg)
		{
			if (!string.IsNullOrEmpty(arg))
			{
				int num;
				if (int.TryParse(arg, out num))
				{
					RainEffigy.RainAdd = num;
					Debug.Log("$> RainEffigy bonus set to " + num);
				}
				else
				{
					Debug.Log("$> '" + arg + "' is not a valid float value for setRainBonus");
				}
			}
			else
			{
				Debug.Log("$> usage: setrainbonus <value> (current=" + RainEffigy.RainAdd + ")");
			}
		}

		
		private void _setGameMode(string arg)
		{
			if (!string.IsNullOrEmpty(arg))
			{
				string text = arg.ToLower();
				switch (text)
				{
				case "standard":
					GameSetup.SetGameType(GameTypes.Standard);
					Debug.Log("$> start game mode set to Standard");
					goto IL_CA;
				case "mod":
					GameSetup.SetGameType(GameTypes.Mod);
					Debug.Log("$> start game mode set to horde");
					goto IL_CA;
				case "creative":
					GameSetup.SetGameType(GameTypes.Creative);
					Debug.Log("$> start game mode set to horde");
					goto IL_CA;
				}
				Debug.Log("$> usage: setgamemode <standard|creative|mod>");
				IL_CA:;
			}
			else
			{
				Debug.Log("$> usage: setgamemode <standard|creative|mod>");
			}
		}

		
		private void _setDifficultyMode(string arg)
		{
			if (!string.IsNullOrEmpty(arg))
			{
				string text = arg.ToLower();
				switch (text)
				{
				case "peaceful":
					GameSetup.SetDifficulty(DifficultyModes.Peaceful);
					Debug.Log("$> Difficulty mode set to Peaceful");
					goto IL_EF;
				case "normal":
					GameSetup.SetDifficulty(DifficultyModes.Normal);
					Debug.Log("$> Difficulty mode set to Normal");
					goto IL_EF;
				case "hard":
					GameSetup.SetDifficulty(DifficultyModes.Hard);
					Debug.Log("$> Difficulty mode set to Hard");
					goto IL_EF;
				case "hardsurvival":
					GameSetup.SetDifficulty(DifficultyModes.HardSurvival);
					Debug.Log("$> Difficulty mode set to HardSurvival");
					goto IL_EF;
				}
				Debug.Log("$> usage: setdifficultymode <peaceful|normal|hard|hardsurvival>");
				IL_EF:;
			}
			else
			{
				Debug.Log("$> usage: setdifficultymode <peaceful|normal|hard|hardsurvival>");
			}
		}

		
		private void _setPlayerVariation(string s)
		{
			int num;
			if (int.TryParse(s, out num))
			{
				CoopPlayerVariations component = LocalPlayer.GameObject.GetComponent<CoopPlayerVariations>();
				try
				{
					LocalPlayer.Stats.PlayerVariation = num;
					component.SetVariation(LocalPlayer.Stats.PlayerVariation, LocalPlayer.Stats.PlayerVariationTShirtType, LocalPlayer.Stats.PlayerVariationTShirtMat, LocalPlayer.Stats.PlayerVariationPantsType, LocalPlayer.Stats.PlayerVariationPantsMat, LocalPlayer.Stats.PlayerVariationHair, LocalPlayer.Stats.PlayerVariationExtras, LocalPlayer.Stats.PlayerClothingVariation);
					component.UpdateSkinVariation(LocalPlayer.Stats.IsBloody, LocalPlayer.Stats.IsMuddy, LocalPlayer.Stats.IsRed, LocalPlayer.Stats.IsCold);
					if (BoltNetwork.isRunning)
					{
						IPlayerState state = LocalPlayer.Entity.GetState<IPlayerState>();
						state.PlayerVariation = LocalPlayer.Stats.PlayerVariation;
						state.PlayerVariationTShirtType = LocalPlayer.Stats.PlayerVariationTShirtType;
						state.PlayerVariationTShirtMat = LocalPlayer.Stats.PlayerVariationTShirtMat;
						state.PlayerVariationPantsType = LocalPlayer.Stats.PlayerVariationPantsType;
						state.PlayerVariationPantsMat = LocalPlayer.Stats.PlayerVariationPantsMat;
					}
					Debug.Log("$> Player variation set to variation #" + num);
				}
				catch
				{
					Debug.Log("$> Invalid player variation number");
				}
			}
			else
			{
				Debug.Log("$> usage: setPlayerVariation <VariationNum>");
			}
		}

		
		private void _setPlayerTShirtMat(string s)
		{
			int num;
			if (int.TryParse(s, out num))
			{
				CoopPlayerVariations component = LocalPlayer.GameObject.GetComponent<CoopPlayerVariations>();
				try
				{
					LocalPlayer.Stats.PlayerVariationTShirtMat = num;
					component.SetVariation(LocalPlayer.Stats.PlayerVariation, LocalPlayer.Stats.PlayerVariationTShirtType, LocalPlayer.Stats.PlayerVariationTShirtMat, LocalPlayer.Stats.PlayerVariationPantsType, LocalPlayer.Stats.PlayerVariationPantsMat, LocalPlayer.Stats.PlayerVariationHair, LocalPlayer.Stats.PlayerVariationExtras, LocalPlayer.Stats.PlayerClothingVariation);
					component.UpdateSkinVariation(LocalPlayer.Stats.IsBloody, LocalPlayer.Stats.IsMuddy, LocalPlayer.Stats.IsRed, LocalPlayer.Stats.IsCold);
					if (BoltNetwork.isRunning)
					{
						IPlayerState state = LocalPlayer.Entity.GetState<IPlayerState>();
						state.PlayerVariation = LocalPlayer.Stats.PlayerVariation;
						state.PlayerVariationTShirtType = LocalPlayer.Stats.PlayerVariationTShirtType;
						state.PlayerVariationTShirtMat = LocalPlayer.Stats.PlayerVariationTShirtMat;
						state.PlayerVariationPantsType = LocalPlayer.Stats.PlayerVariationPantsType;
						state.PlayerVariationPantsMat = LocalPlayer.Stats.PlayerVariationPantsMat;
					}
					Debug.Log("$> Player tshirt set to mat #" + num);
				}
				catch
				{
					Debug.Log("$> Invalid player tshirt mat number");
				}
			}
			else
			{
				Debug.Log("$> usage: setPlayerTShirtMat <BodyNum>");
			}
		}

		
		private void _setVariationExtra(string s)
		{
			try
			{
				PlayerCloting playerCloting = (PlayerCloting)((int)Enum.Parse(typeof(PlayerCloting), s));
				CoopPlayerVariations component = LocalPlayer.GameObject.GetComponent<CoopPlayerVariations>();
				try
				{
					component.SetVariation(LocalPlayer.Stats.PlayerVariation, LocalPlayer.Stats.PlayerVariationTShirtType, LocalPlayer.Stats.PlayerVariationTShirtMat, LocalPlayer.Stats.PlayerVariationPantsType, LocalPlayer.Stats.PlayerVariationPantsMat, LocalPlayer.Stats.PlayerVariationHair, playerCloting, LocalPlayer.Stats.PlayerClothingVariation);
					component.UpdateSkinVariation(LocalPlayer.Stats.IsBloody, LocalPlayer.Stats.IsMuddy, LocalPlayer.Stats.IsRed, LocalPlayer.Stats.IsCold);
					LocalPlayer.Stats.PlayerVariationExtras = playerCloting;
					if (BoltNetwork.isRunning)
					{
						IPlayerState state = LocalPlayer.Entity.GetState<IPlayerState>();
						state.PlayerVariation = LocalPlayer.Stats.PlayerVariation;
						state.PlayerVariationTShirtType = LocalPlayer.Stats.PlayerVariationTShirtType;
						state.PlayerVariationTShirtMat = LocalPlayer.Stats.PlayerVariationTShirtMat;
						state.PlayerVariationPantsType = LocalPlayer.Stats.PlayerVariationPantsType;
						state.PlayerVariationPantsMat = LocalPlayer.Stats.PlayerVariationPantsMat;
						state.PlayerClothing = (int)LocalPlayer.Stats.PlayerVariationExtras;
						state.PlayerClothingVariation = LocalPlayer.Stats.PlayerClothingVariation;
					}
					Debug.Log("$> Player extra variation set to #" + playerCloting);
				}
				catch
				{
					Debug.Log("$> Invalid player variation extra");
				}
			}
			catch
			{
				Debug.Log("$> usage: setVariationExtra <None|Jacket>");
			}
		}

		
		private void _findPassenger(string arg)
		{
			int num;
			if (int.TryParse(arg, out num))
			{
				if (LocalPlayer.PassengerManifest.FoundPassenger(num))
				{
					Debug.Log("$> Successfully found passenger " + num);
				}
				else
				{
					Debug.Log("$> Find passenger " + num + " failed");
				}
			}
			else
			{
				Debug.Log("$> usage: findPassenger <ID>");
			}
		}

		
		private void GotoCave(bool inCave)
		{
			if (inCave && !LocalPlayer.IsInCaves)
			{
				Debug.Log("$> goto -> entering cave");
				LocalPlayer.GameObject.SendMessage("InACave");
			}
			else if (!inCave && LocalPlayer.IsInCaves)
			{
				Debug.Log("$> goto -> exiting cave");
				LocalPlayer.GameObject.SendMessage("NotInACave");
			}
		}

		
		private void GotoArea(Area area)
		{
			if (area)
			{
				area.OnEnter(null);
			}
		}

		
		private void _goto(string arg)
		{
			if (!string.IsNullOrEmpty(arg))
			{
				GameObject gameObject = GameObject.Find(arg);
				if (gameObject)
				{
					bool flag = Terrain.activeTerrain.SampleHeight(gameObject.transform.position) - gameObject.transform.position.y > (float)((!LocalPlayer.IsInCaves) ? 7 : 3);
					Area componentInParent = gameObject.GetComponentInParent<Area>();
					if (!flag && !LocalPlayer.IsInCaves)
					{
						RaycastHit raycastHit;
						if (Physics.SphereCast(gameObject.transform.position + Vector3.up * 25f + Vector3.forward * 2f, 2f, Vector3.down, out raycastHit, 60f))
						{
							this.GotoCave(flag);
							this.GotoArea(componentInParent);
							LocalPlayer.Rigidbody.velocity = Vector3.zero;
							LocalPlayer.Transform.position = raycastHit.point + Vector3.up * 2.5f;
							Debug.Log("$> going to " + arg);
						}
						else
						{
							Debug.Log("$> didn't find a suitable landing spot raycasting down on '" + arg + "', cancelling goto");
						}
					}
					else
					{
						this.GotoCave(flag);
						this.GotoArea(componentInParent);
						LocalPlayer.Rigidbody.velocity = Vector3.zero;
						LocalPlayer.Transform.position = gameObject.transform.position + Vector3.up;
						GameObject gameObject2 = GameObject.FindWithTag("EndgameLoader");
						if (gameObject2)
						{
							SceneLoadTrigger component = gameObject2.GetComponent<SceneLoadTrigger>();
							Vector3 rhs = LocalPlayer.Transform.position - component.transform.position;
							if (Vector3.Dot(component.transform.forward, rhs) > 0f && rhs.magnitude < 150f)
							{
								component._onCrossingForwards.Invoke();
							}
						}
						Debug.Log("$> going to " + arg);
					}
				}
				else
				{
					string[] array = arg.Split(new char[]
					{
						' '
					});
					if (array.Length == 3)
					{
						float numVal;
						if (array.All((string c) => float.TryParse(c, out numVal)))
						{
							Vector3 vector = new Vector3(float.Parse(array[0]), Mathf.Ceil(float.Parse(array[1])), float.Parse(array[2]));
							bool inCave = Terrain.activeTerrain.SampleHeight(vector) - vector.y > (float)((!LocalPlayer.IsInCaves) ? 6 : 3);
							this.GotoCave(inCave);
							LocalPlayer.Rigidbody.velocity = Vector3.zero;
							LocalPlayer.Transform.position = vector;
							Debug.Log("$> going to " + vector);
							return;
						}
					}
					Debug.Log("$> '" + arg + "' not found, cancelling goto");
				}
			}
			else
			{
				Debug.Log("$> usage: goto <GameObjectName>");
			}
		}

		
		private void _gototag(string arg)
		{
			if (!string.IsNullOrEmpty(arg))
			{
				GameObject gameObject = GameObject.FindGameObjectWithTag(arg);
				if (gameObject)
				{
					this._goto(gameObject.name);
				}
				else
				{
					Debug.Log("$> No object with tag '" + arg + "' was found, cancelling gototag");
				}
			}
			else
			{
				Debug.Log("$> usage: gototag <GameObjectTag>");
			}
		}

		
		private void _gotoEnemy(object o)
		{
			if (Scene.MutantControler && Scene.MutantControler.activeCannibals.Count > 0)
			{
				this._goto(Scene.MutantControler.activeCannibals[0].name);
			}
			else
			{
				Debug.Log("$> No enemy to target with goto, gotoEnemy failed");
			}
		}

		
		private void _sendMessageTo(string arg)
		{
			if (!string.IsNullOrEmpty(arg))
			{
				string[] array = arg.Trim().Split(new char[]
				{
					' '
				});
				if (array.Length == 2)
				{
					GameObject gameObject;
					if (array[0].ToLower() == "player")
					{
						gameObject = LocalPlayer.GameObject;
					}
					else
					{
						gameObject = GameObject.Find(array[0]);
						if (!gameObject)
						{
							gameObject = GameObject.FindWithTag(array[0]);
						}
					}
					if (!gameObject)
					{
						Debug.Log("$> sendMessageTo target '" + array[0] + "' not found, aborting");
						return;
					}
					try
					{
						Debug.Log(string.Concat(new string[]
						{
							"$> Sending message '",
							array[1],
							"' to target '",
							array[0],
							"'"
						}));
						gameObject.SendMessage(array[1], SendMessageOptions.DontRequireReceiver);
					}
					finally
					{
						Debug.Log(string.Concat(new string[]
						{
							"$> Done sending message '",
							array[1],
							"' to target '",
							array[0],
							"'"
						}));
					}
				}
			}
			Debug.Log("$> usage: sendMessageTo <targetName> <message>");
		}

		
		private void _profilersnapshot(object arg)
		{
			Debug.Log("----------[ Profiler Snapshot ]----------");
			string format = "{0}: {1:N0} MB";
			Debug.Log(string.Format(format, "Textures", Resources.FindObjectsOfTypeAll(typeof(Texture)).Sum((UnityEngine.Object r) => Profiler.GetRuntimeMemorySize(r)) / 1048576));
			Debug.Log(string.Format(format, "AudioClip", Resources.FindObjectsOfTypeAll(typeof(AudioClip)).Sum((UnityEngine.Object r) => Profiler.GetRuntimeMemorySize(r)) / 1048576));
			Debug.Log(string.Format(format, "Mesh", Resources.FindObjectsOfTypeAll(typeof(Mesh)).Sum((UnityEngine.Object r) => Profiler.GetRuntimeMemorySize(r)) / 1048576));
			Debug.Log(string.Format(format, "Material", Resources.FindObjectsOfTypeAll(typeof(Material)).Sum((UnityEngine.Object r) => Profiler.GetRuntimeMemorySize(r)) / 1048576));
			Debug.Log(string.Format(format, "GameObject", Resources.FindObjectsOfTypeAll(typeof(GameObject)).Sum((UnityEngine.Object r) => Profiler.GetRuntimeMemorySize(r)) / 1048576));
			Debug.Log(string.Format(format, "Component", Resources.FindObjectsOfTypeAll(typeof(Component)).Sum((UnityEngine.Object r) => Profiler.GetRuntimeMemorySize(r)) / 1048576));
			Debug.Log("----------[ EoS ]----------");
		}

		
		private void _profilersample(string onoff)
		{
			if (onoff == "on")
			{
				DateTime now = DateTime.Now;
				Profiler.logFile = string.Concat(new object[]
				{
					"ProfilerSample_",
					Environment.UserName,
					"_",
					now.Month,
					"-",
					now.Day,
					"-",
					now.Year,
					"_",
					now.Minute,
					"-",
					now.Hour,
					".log"
				});
				Profiler.enableBinaryLog = true;
				Profiler.enabled = true;
				Debug.Log("$> Profiler Sampling enabled in " + Profiler.logFile);
			}
			else if (onoff == "off")
			{
				Profiler.enableBinaryLog = false;
				Profiler.enabled = false;
				Debug.Log("$> Profiler Sampling disabled");
			}
			else
			{
				Debug.Log("$> usage: profilersample <on|off>");
			}
		}

		
		private void _unloadUnusedAssets(object o)
		{
			Debug.Log("$> Unloading unused assets");
			Resources.UnloadUnusedAssets();
		}

		
		private void _filteraudio(string filter)
		{
			if (filter == null)
			{
				FMOD_Listener.PathFilter = new string[0];
				Debug.Log("$> Audio path filter cleared.");
			}
			else
			{
				FMOD_Listener.PathFilter = filter.ToLowerInvariant().Split(new char[]
				{
					' '
				});
				Debug.LogFormat("$> Audio path filter set to ({0}).", new object[]
				{
					string.Join(" AND ", FMOD_Listener.PathFilter)
				});
			}
		}

		
		private void _eval(string sCSCode)
		{
			try
			{
				ICodeCompiler codeCompiler = CodeDomProvider.CreateProvider("CSharp").CreateCompiler();
				CompilerParameters compilerParameters = new CompilerParameters();
				compilerParameters.ReferencedAssemblies.Add("system.dll");
				compilerParameters.CompilerOptions = "/t:library";
				compilerParameters.GenerateInMemory = true;
				StringBuilder stringBuilder = new StringBuilder(string.Empty);
				stringBuilder.Append("using System;\n");
				stringBuilder.Append("using TheForest;\n");
				stringBuilder.Append("using TheForest.Utils;\n");
				stringBuilder.Append("using TheForest.Items;\n");
				stringBuilder.Append("using TheForest.Items.Inventory;\n");
				stringBuilder.Append("using TheForest.Items.World;\n");
				stringBuilder.Append("using TheForest.Buildings;\n");
				stringBuilder.Append("using TheForest.Buildings.Creation;\n");
				stringBuilder.Append("using TheForest.Buildings.World;\n");
				stringBuilder.Append("namespace TheForest{ \n");
				stringBuilder.Append("public class CSCodeEvaler{ \n");
				stringBuilder.Append("public object EvalCode(){\n");
				stringBuilder.Append(sCSCode + " \n");
				stringBuilder.Append("} \n");
				stringBuilder.Append("} \n");
				stringBuilder.Append("}\n");
				CompilerResults compilerResults = codeCompiler.CompileAssemblyFromSource(compilerParameters, stringBuilder.ToString());
				if (compilerResults.Errors.Count > 0)
				{
					Debug.LogError("$> eval error: " + compilerResults.Errors[0].ErrorText);
				}
				else
				{
					Assembly compiledAssembly = compilerResults.CompiledAssembly;
					object obj = compiledAssembly.CreateInstance("CSCodeEvaler.CSCodeEvaler");
					Type type = obj.GetType();
					MethodInfo method = type.GetMethod("EvalCode");
					object arg = method.Invoke(obj, null);
					Debug.Log("$> Eval succes: " + arg);
				}
			}
			catch (Exception arg2)
			{
				Debug.LogError("$> eval exception: " + arg2);
			}
		}

		
		private void setLastLocalTarget(Transform tr)
		{
			this.lastLocalTarget = tr;
		}

		
		private void _spawnenemy(string mutantName)
		{
			if (BoltNetwork.isClient)
			{
				debugCommand debugCommand = debugCommand.Create(GlobalTargets.OnlyServer);
				debugCommand.input = "_spawnenemy";
				debugCommand.target = LocalPlayer.Transform.GetComponent<BoltEntity>();
				debugCommand.input2 = mutantName;
				debugCommand.Send();
				return;
			}
			Vector3 vector;
			if (this.lastLocalTarget == null)
			{
				vector = LocalPlayer.Transform.position + LocalPlayer.Transform.forward * 4f;
			}
			else
			{
				vector = this.lastLocalTarget.position + this.lastLocalTarget.forward * 4f;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("instantMutantSpawner"), vector + new Vector3(0f, 1f, 2f), Quaternion.identity) as GameObject;
			string text = mutantName;
			if (text != null)
			{
				if (DebugConsole.<>f__switch$map20 == null)
				{
					DebugConsole.<>f__switch$map20 = new Dictionary<string, int>(12)
					{
						{
							"male_skinny",
							0
						},
						{
							"female_skinny",
							0
						},
						{
							"skinny_pale",
							0
						},
						{
							"male",
							0
						},
						{
							"female",
							0
						},
						{
							"fireman",
							0
						},
						{
							"dynamiteman",
							0
						},
						{
							"pale",
							0
						},
						{
							"armsy",
							0
						},
						{
							"vags",
							0
						},
						{
							"baby",
							0
						},
						{
							"fat",
							0
						}
					};
				}
				int num;
				if (DebugConsole.<>f__switch$map20.TryGetValue(text, out num))
				{
					if (num == 0)
					{
						spawnMutants spawnMutants = UnityEngine.Object.Instantiate<spawnMutants>(Resources.Load<spawnMutants>("instantMutantSpawnerDC"));
						spawnMutants.transform.position = vector;
						if (mutantName == "dynamiteman")
						{
							mutantName = "fireman";
							spawnMutants.useDynamiteMan = true;
						}
						if (mutantName == "pale" || mutantName == "skinny_pale")
						{
							spawnMutants.pale = true;
						}
						else
						{
							spawnMutants.pale = false;
						}
						FieldInfo field = spawnMutants.GetType().GetField("amount_" + mutantName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						field.SetValue(spawnMutants, 1);
						Debug.Log("$> spawned 1 " + mutantName);
						goto IL_266;
					}
				}
			}
			Debug.Log("$> usage: spawnenemy <male_skinny | female_skinny | skinny_pale | male | female | fireman | pale | armsy | vags | baby | fat>");
			IL_266:
			this.lastLocalTarget = null;
		}

		
		private void _killclosestenemy(string onoff)
		{
			List<GameObject> list = new List<GameObject>(Scene.MutantControler.activeCannibals);
			foreach (GameObject item in Scene.MutantControler.activeInstantSpawnedCannibals)
			{
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
			list.RemoveAll((GameObject o) => o == null);
			list.RemoveAll((GameObject o) => o != o.activeSelf);
			if (list.Count > 0)
			{
				list.Sort((GameObject c1, GameObject c2) => Vector3.Distance(LocalPlayer.Transform.position, c1.transform.position).CompareTo(Vector3.Distance(LocalPlayer.Transform.position, c2.transform.position)));
				list[0].SendMessage("killThisEnemy", SendMessageOptions.DontRequireReceiver);
				Debug.Log("$> " + list[0].gameObject.name + " was killed");
			}
			else
			{
				Debug.Log("$> no more enemies left to kill");
			}
		}

		
		private void _knockDownclosestenemy(string onoff)
		{
			List<GameObject> list = new List<GameObject>(Scene.MutantControler.activeCannibals);
			foreach (GameObject item in Scene.MutantControler.activeInstantSpawnedCannibals)
			{
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
			list.RemoveAll((GameObject o) => o == null);
			list.RemoveAll((GameObject o) => o != o.activeSelf);
			if (list.Count > 0)
			{
				list.Sort((GameObject c1, GameObject c2) => Vector3.Distance(LocalPlayer.Transform.position, c1.transform.position).CompareTo(Vector3.Distance(LocalPlayer.Transform.position, c2.transform.position)));
				list[0].SendMessage("knockDownThisEnemy", SendMessageOptions.DontRequireReceiver);
				Debug.Log("$> " + list[0].gameObject.name + " was knocked down");
			}
			else
			{
				Debug.Log("$> no more enemies left to knock down!");
			}
		}

		
		private void _killclosestanimal(string onoff)
		{
			List<GameObject> list = new List<GameObject>();
			for (int i = 0; i < PoolManager.Pools["creatures"].Count; i++)
			{
				GameObject gameObject = PoolManager.Pools["creatures"][i].gameObject;
				if (gameObject.activeSelf)
				{
					list.Add(gameObject);
				}
			}
			list.RemoveAll((GameObject o) => o == null);
			if (list.Count > 0)
			{
				list.Sort((GameObject c1, GameObject c2) => Vector3.Distance(LocalPlayer.Transform.position, c1.transform.position).CompareTo(Vector3.Distance(LocalPlayer.Transform.position, c2.transform.position)));
				list[0].SendMessage("HitReal", 1000, SendMessageOptions.DontRequireReceiver);
				Debug.Log("$> " + list[0].gameObject.name + " was killed");
			}
			else
			{
				Debug.Log("$> no more animals left to kill");
			}
		}

		
		private void _greebledRocksCollision(string onoff)
		{
			if (onoff == "on")
			{
				GameObject gameObject = GameObject.Find("greeebleRocksExport");
				if (gameObject)
				{
					Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>();
					foreach (Transform transform in componentsInChildren)
					{
						if (!transform.GetComponent<MeshCollider>())
						{
							transform.gameObject.AddComponent<MeshCollider>();
						}
					}
					Debug.Log("$> enabled static greeble rock collision");
				}
			}
			if (onoff == "off")
			{
				GameObject gameObject2 = GameObject.Find("greeebleRocksExport");
				if (gameObject2)
				{
					Transform[] componentsInChildren2 = gameObject2.GetComponentsInChildren<Transform>();
					foreach (Transform transform2 in componentsInChildren2)
					{
						MeshCollider component = transform2.GetComponent<MeshCollider>();
						if (component)
						{
							UnityEngine.Object.Destroy(component);
						}
					}
					Debug.Log("$> disabled static greeble rock collision");
				}
			}
		}

		
		private void _fakeHitPlayer(string onoff)
		{
			LocalPlayer.Transform.SendMessage("hitFromEnemy", 1, SendMessageOptions.DontRequireReceiver);
		}

		
		private void _killallenemies(string onoff)
		{
			if (BoltNetwork.isClient)
			{
				debugCommand debugCommand = debugCommand.Create(GlobalTargets.OnlyServer);
				debugCommand.input = "_killallenemies";
				debugCommand.Send();
				return;
			}
			base.StartCoroutine(this.doKillAllEnemies());
		}

		
		private IEnumerator doKillAllEnemies()
		{
			List<GameObject> allEnemy = new List<GameObject>(Scene.MutantControler.activeCannibals);
			List<GameObject> allBaby = new List<GameObject>(Scene.MutantControler.activeBabies);
			List<GameObject> allInstant = new List<GameObject>(Scene.MutantControler.activeInstantSpawnedCannibals);
			allEnemy.RemoveAll((GameObject o) => o == null);
			allBaby.RemoveAll((GameObject o) => o == null);
			allInstant.RemoveAll((GameObject o) => o == null);
			foreach (GameObject go in allEnemy)
			{
				go.SendMessage("killThisEnemy", SendMessageOptions.DontRequireReceiver);
				Debug.Log("$> " + go.gameObject.name + " was killed");
				yield return YieldPresets.WaitPointOneSeconds;
			}
			foreach (GameObject go2 in allInstant)
			{
				go2.SendMessage("killThisEnemy", SendMessageOptions.DontRequireReceiver);
				Debug.Log("$> " + go2.gameObject.name + " was killed");
				yield return YieldPresets.WaitPointOneSeconds;
			}
			foreach (GameObject go3 in allBaby)
			{
				go3.SendMessage("killThisEnemy", SendMessageOptions.DontRequireReceiver);
				Debug.Log("$> " + go3.gameObject.name + " was killed");
				yield return YieldPresets.WaitPointOneSeconds;
			}
			Debug.Log("$> no more enemies left to kill");
			yield break;
		}

		
		private void _killallanimals(string onoff)
		{
			animalHealth[] array = UnityEngine.Object.FindObjectsOfType<animalHealth>();
			foreach (animalHealth animalHealth in array)
			{
				if (animalHealth.gameObject.activeInHierarchy)
				{
					animalHealth.SendMessage("Die");
				}
			}
			Debug.Log("$> Killed " + array.Length + " animals");
		}

		
		private void _killEndBoss(string onoff)
		{
			GameObject gameObject = GameObject.Find("girl_base");
			if (gameObject)
			{
				gameObject.transform.parent.SendMessage("killThisEnemy", SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				Debug.Log("$> no boss to kill?");
			}
			Debug.Log("$> Killed the Boss");
		}

		
		private void _resetAllEnemies(string onoff)
		{
			if (BoltNetwork.isClient)
			{
				debugCommand debugCommand = debugCommand.Create(GlobalTargets.OnlyServer);
				debugCommand.input = "_resetAllEnemies";
				debugCommand.Send();
				Debug.Log("$> respawning all enemies based on current day amounts");
				return;
			}
			base.StartCoroutine(this.doResetAllEnemies());
		}

		
		private IEnumerator doResetAllEnemies()
		{
			base.StartCoroutine(this.doKillAllEnemies());
			yield return YieldPresets.WaitSevenSeconds;
			Scene.MutantControler.spawnManager.offsetCreepy = 0;
			Scene.MutantControler.spawnManager.offsetPainted = 0;
			Scene.MutantControler.spawnManager.offsetPale = 0;
			Scene.MutantControler.spawnManager.offsetRegular = 0;
			Scene.MutantControler.spawnManager.offsetSkinned = 0;
			Scene.MutantControler.spawnManager.offsetSkinny = 0;
			Scene.MutantControler.spawnManager.offsetSkinnyPale = 0;
			Scene.SceneTracker.updateMutantSpawners();
			Debug.Log("$> all enemies now respawned based on current day amounts..");
			yield break;
		}

		
		private void _speedyrun(string onoff)
		{
			if (onoff == "on")
			{
				base.StartCoroutine("setFastRun");
			}
			else if (onoff == "off")
			{
				base.StopCoroutine("setFastRun");
				LocalPlayer.FpCharacter.runSpeed = 9f;
			}
		}

		
		private IEnumerator setFastRun()
		{
			for (;;)
			{
				LocalPlayer.FpCharacter.runSpeed = 60f;
				yield return null;
			}
			yield break;
		}

		
		private void _poison(string onoff)
		{
			LocalPlayer.Stats.Poison();
			Debug.Log("$> Poisonned player");
		}

		
		private void _enemies(string onoff)
		{
			if (Scene.MutantControler && onoff == "toggle")
			{
				onoff = ((!Scene.MutantControler.enabled) ? "on" : "off");
			}
			if (onoff == "off")
			{
				Scene.MutantControler.StartCoroutine("removeAllEnemies");
				base.Invoke("disableMutantController", 0.5f);
				Debug.Log("$> Disabled all enemies");
			}
			else if (onoff == "on")
			{
				Scene.MutantControler.enabled = true;
				Scene.MutantControler.startSetupFamilies();
				Debug.Log("$> Enabled enemies");
			}
			else
			{
				Debug.Log("$> usage: enemies <on|off>");
			}
		}

		
		private void _setShadowLevel(string val)
		{
			switch (int.Parse(val))
			{
			case 0:
				TheForestQualitySettings.UserSettings.ShadowLevel = TheForestQualitySettings.ShadowLevels.Fastest;
				Debug.Log("$> shadow level  = " + TheForestQualitySettings.UserSettings.ShadowLevel);
				return;
			case 1:
				TheForestQualitySettings.UserSettings.ShadowLevel = TheForestQualitySettings.ShadowLevels.UltraLow;
				Debug.Log("$> shadow level  = " + TheForestQualitySettings.UserSettings.ShadowLevel);
				return;
			case 2:
				TheForestQualitySettings.UserSettings.ShadowLevel = TheForestQualitySettings.ShadowLevels.Low;
				Debug.Log("$> shadow level  = " + TheForestQualitySettings.UserSettings.ShadowLevel);
				return;
			case 3:
				TheForestQualitySettings.UserSettings.ShadowLevel = TheForestQualitySettings.ShadowLevels.Medium;
				Debug.Log("$> shadow level  = " + TheForestQualitySettings.UserSettings.ShadowLevel);
				return;
			case 4:
				TheForestQualitySettings.UserSettings.ShadowLevel = TheForestQualitySettings.ShadowLevels.High;
				Debug.Log("$> shadow level  = " + TheForestQualitySettings.UserSettings.ShadowLevel);
				return;
			default:
				return;
			}
		}

		
		private void _setDrawDistance(string val)
		{
			switch (int.Parse(val))
			{
			case 0:
				TheForestQualitySettings.UserSettings.DrawDistance = TheForestQualitySettings.DrawDistances.UltraLow;
				Debug.Log("$> draw distance  = " + TheForestQualitySettings.UserSettings.DrawDistance);
				return;
			case 1:
				TheForestQualitySettings.UserSettings.DrawDistance = TheForestQualitySettings.DrawDistances.Low;
				Debug.Log("$> draw distance  = " + TheForestQualitySettings.UserSettings.DrawDistance);
				return;
			case 2:
				TheForestQualitySettings.UserSettings.DrawDistance = TheForestQualitySettings.DrawDistances.Medium;
				Debug.Log("$> draw distance  = " + TheForestQualitySettings.UserSettings.DrawDistance);
				return;
			case 3:
				TheForestQualitySettings.UserSettings.DrawDistance = TheForestQualitySettings.DrawDistances.High;
				Debug.Log("$> draw distance  = " + TheForestQualitySettings.UserSettings.DrawDistance);
				return;
			case 4:
				TheForestQualitySettings.UserSettings.DrawDistance = TheForestQualitySettings.DrawDistances.VeryHigh;
				Debug.Log("$> draw distance  = " + TheForestQualitySettings.UserSettings.DrawDistance);
				return;
			case 5:
				TheForestQualitySettings.UserSettings.DrawDistance = TheForestQualitySettings.DrawDistances.Ultra;
				Debug.Log("$> draw distance  = " + TheForestQualitySettings.UserSettings.DrawDistance);
				return;
			default:
				return;
			}
		}

		
		private void _terrainPixelError(string val)
		{
			Terrain.activeTerrain.heightmapPixelError = float.Parse(val);
			Debug.Log("$> terrain pixel error = " + Terrain.activeTerrain.heightmapPixelError);
		}

		
		private void _toggleVSync(string val)
		{
			if (QualitySettings.vSyncCount == 0)
			{
				QualitySettings.vSyncCount = 1;
				Debug.Log("$> vsync enabled");
			}
			else
			{
				QualitySettings.vSyncCount = 0;
				Debug.Log("$> vsync disabled");
			}
		}

		
		private void _toggleFPSDisplay(string val)
		{
			GameObject gameObject = GameObject.Find("BlitDebug");
			if (gameObject)
			{
				gameObject.SendMessage("ToggleFPSCounter", SendMessageOptions.DontRequireReceiver);
			}
		}

		
		private void _toggleOcclusionCulling(string val)
		{
			if (Camera.main.useOcclusionCulling)
			{
				Camera.main.useOcclusionCulling = false;
				Debug.Log("$> Disabled occlusion culling");
			}
			else
			{
				Camera.main.useOcclusionCulling = true;
				Debug.Log("$> enabled occlusion culling");
			}
		}

		
		private void _terrainRender(string onoff)
		{
			if (onoff == "off")
			{
				Terrain.activeTerrain.drawHeightmap = false;
				Terrain.activeTerrain.drawTreesAndFoliage = false;
				Debug.Log("$> Disabled terrain rendering");
			}
			else if (onoff == "on")
			{
				Terrain.activeTerrain.drawHeightmap = true;
				Terrain.activeTerrain.drawTreesAndFoliage = true;
				Debug.Log("$> Enabled terrain rendering");
			}
			else
			{
				Debug.Log("$> usage: terrainRender <on|off>");
			}
		}

		
		private void _physics30Fps(string onoff)
		{
			if (onoff == "on")
			{
				PlayerPreferences.SetLowQualityPhysics(true);
				Debug.Log("$> physics update set to 30 fps");
			}
			else if (onoff == "off")
			{
				PlayerPreferences.SetLowQualityPhysics(false);
				Debug.Log("$> physics update rate set to 60 fps");
			}
			else
			{
				Debug.Log("$> usage: terrainRender <on|off>");
			}
		}

		
		private void _targetFrameRate(string rate)
		{
			Application.targetFrameRate = int.Parse(rate);
		}

		
		private void _invisible(string onoff)
		{
			if (onoff == "off")
			{
				LocalPlayer.GameObject.layer = 18;
				Debug.Log("$> Player can be seen by enemies");
			}
			else if (onoff == "on")
			{
				LocalPlayer.GameObject.layer = 31;
				Debug.Log("$> Player is now invisible to enemies");
			}
		}

		
		private void disableMutantController()
		{
			Scene.MutantControler.enabled = false;
		}

		
		private void _playernetanimator(string onoff)
		{
			if (onoff == "off")
			{
				GameObject[] array = GameObject.FindGameObjectsWithTag("PlayerNet");
				foreach (GameObject gameObject in array)
				{
					Animator componentInChildren = gameObject.GetComponentInChildren<Animator>();
					if (componentInChildren)
					{
						componentInChildren.enabled = false;
					}
				}
				Debug.Log("$> Disabled " + array.Length + " player_net animators");
			}
			else if (onoff == "on")
			{
				GameObject[] array3 = GameObject.FindGameObjectsWithTag("PlayerNet");
				foreach (GameObject gameObject2 in array3)
				{
					Animator componentInChildren2 = gameObject2.GetComponentInChildren<Animator>();
					if (componentInChildren2)
					{
						componentInChildren2.enabled = true;
					}
				}
				Debug.Log("$> Enabled " + array3.Length + " player_net animators");
			}
			else
			{
				Debug.Log("$> usage: playernetanimator <on|off>");
			}
		}

		
		private void _capsulemode(string onoff)
		{
			if (onoff == "on")
			{
				GameObject[] array = GameObject.FindGameObjectsWithTag("PlayerNet");
				foreach (GameObject gameObject in array)
				{
					Animator componentInChildren = gameObject.GetComponentInChildren<Animator>();
					if (componentInChildren)
					{
						Renderer[] componentsInChildren = componentInChildren.transform.GetComponentsInChildren<Renderer>();
						foreach (Renderer renderer in componentsInChildren)
						{
							renderer.enabled = false;
						}
					}
					gameObject.transform.Find("Capsule").GetComponent<Renderer>().enabled = true;
				}
				Debug.Log("$> Enabled capsule mode on " + array.Length + " player_net");
			}
			else if (onoff == "off")
			{
				GameObject[] array4 = GameObject.FindGameObjectsWithTag("PlayerNet");
				foreach (GameObject gameObject2 in array4)
				{
					Animator[] componentsInChildren2 = gameObject2.GetComponentsInChildren<Animator>(true);
					if (componentsInChildren2[0] && componentsInChildren2[0])
					{
						Renderer[] componentsInChildren3 = componentsInChildren2[0].transform.GetComponentsInChildren<Renderer>();
						foreach (Renderer renderer2 in componentsInChildren3)
						{
							renderer2.enabled = true;
						}
					}
					gameObject2.transform.Find("Capsule").GetComponent<Renderer>().enabled = false;
				}
				Debug.Log("$> Disabled capsule mode on " + array4.Length + " player_net");
			}
			else
			{
				Debug.Log("$> usage: capsulemode <on|off>");
			}
		}

		
		private void _animals(string onoff)
		{
			if (!this.CoopAnimalController)
			{
				this.CoopAnimalController = GameObject.Find("CoopAnimalSpawner");
			}
			if (this.CoopAnimalController && onoff == "toggle")
			{
				onoff = ((!this.CoopAnimalController.activeSelf) ? "on" : "off");
			}
			if (onoff == "off")
			{
				this.CoopAnimalController.SetActive(false);
				PoolManager.Pools["creatures"].DespawnAll();
				Debug.Log("$> Disabled animal controller & despawned all animals");
				animalController[] componentsInChildren = GameObject.Find("_creatureSetup").GetComponentsInChildren<animalController>();
				foreach (animalController animalController in componentsInChildren)
				{
					animalController.enabled = false;
				}
				GameObject gameObject = GameObject.Find("gooseControllerGo");
				if (gameObject)
				{
					gooseController[] componentsInChildren2 = gameObject.GetComponentsInChildren<gooseController>(true);
					foreach (gooseController gooseController in componentsInChildren2)
					{
						gooseController.disableGeese();
						gooseController.enabled = false;
					}
				}
			}
			else if (onoff == "on")
			{
				if (this.CoopAnimalController)
				{
					this.CoopAnimalController.SetActive(true);
				}
				animalController[] componentsInChildren3 = GameObject.Find("_creatureSetup").GetComponentsInChildren<animalController>(true);
				foreach (animalController animalController2 in componentsInChildren3)
				{
					animalController2.enabled = true;
				}
				GameObject gameObject2 = GameObject.Find("gooseControllerGo");
				if (gameObject2)
				{
					gooseController[] componentsInChildren4 = gameObject2.GetComponentsInChildren<gooseController>(true);
					foreach (gooseController gooseController2 in componentsInChildren4)
					{
						gooseController2.enabled = true;
						gooseController2.SendMessage("Start");
					}
				}
				Debug.Log("$> Enabled animal controller");
			}
			else
			{
				Debug.Log("$> usage: animals <on|off>");
			}
		}

		
		private void _birds(string onoff)
		{
			GameObject gameObject = GameObject.Find("_livingBirdsController");
			if (gameObject)
			{
				if (onoff == "off")
				{
					gameObject.SendMessage("despawnAll");
					gameObject.GetComponent<lb_BirdController>().enabled = false;
					Debug.Log("$> Disabled bird controller & despawned all birds");
				}
				else if (onoff == "on")
				{
					gameObject.GetComponent<lb_BirdController>().enabled = true;
					Debug.Log("$> Enabled bird controller");
				}
				else
				{
					Debug.Log("$> usage: birds <on|off>");
				}
			}
		}

		
		private void _astar(string onoff)
		{
			if (onoff == "off")
			{
				GameObject gameObject = GameObject.Find("Astar");
				if (gameObject)
				{
					gameObject.GetComponent<AstarPath>().enabled = false;
					Debug.Log("$> disabled astar component");
				}
			}
			else if (onoff == "on")
			{
				GameObject gameObject2 = GameObject.Find("Astar");
				if (gameObject2)
				{
					gameObject2.GetComponent<AstarPath>().enabled = true;
					Debug.Log("$> enabled astar component");
				}
			}
			else
			{
				Debug.Log("$> usage: astar <on|off>");
			}
		}

		
		private void _useRigidBodyRotation(string onoff)
		{
			if (onoff == "on")
			{
				LocalPlayer.MainRotator.useRigidbody = true;
				Debug.Log("$> enabled camera rotation using Rigid Body rotations");
			}
			else if (onoff == "off")
			{
				LocalPlayer.MainRotator.useRigidbody = false;
				Debug.Log("$> disabled camera rotation using Rigid Body rotations");
			}
			else
			{
				Debug.Log("$> usage: useRigidBodyRotation <on|off>");
			}
		}

		
		private void _setCurrentDay(string num)
		{
			int num2 = int.Parse(num);
			if (BoltNetwork.isClient)
			{
				debugCommand debugCommand = debugCommand.Create(GlobalTargets.OnlyServer);
				debugCommand.input = "_setCurrentDay";
				debugCommand.input2 = num;
				debugCommand.Send();
				Debug.Log("$> current day = " + num2);
				return;
			}
			if (num2 > 0)
			{
				Clock.Day = num2;
			}
			Debug.Log("$> current day = " + num2);
		}

		
		private void _advanceday(string onoff)
		{
			if (!Clock.Dark)
			{
				Scene.Atmosphere.TimeOfDay = 155f;
			}
			else
			{
				Scene.Atmosphere.TimeOfDay = 269f;
			}
			Scene.Atmosphere.ForceSunRotationUpdate = true;
			base.Invoke("_checkDay", 1f);
		}

		
		private void _checkDay(string onoff)
		{
			Debug.Log("$> current day = " + Clock.Day);
		}

		
		private void _forcerain(string arg)
		{
			int num;
			if (string.IsNullOrEmpty(arg))
			{
				num = UnityEngine.Random.Range(2, 5);
			}
			else if (!int.TryParse(arg, out num))
			{
				arg = arg.ToLower().Trim();
				if (arg.Contains("lig"))
				{
					num = 2;
				}
				else if (arg.Contains("med"))
				{
					num = 3;
				}
				else if (arg.Contains("hea"))
				{
					num = 4;
				}
				else if (arg.Contains("clo"))
				{
					num = 5;
				}
				else
				{
					if (!arg.Contains("sun") && !arg.Contains("sto"))
					{
						Debug.Log("$> usage: forcerain <[rainDice]|light|medium|heavy|cloud|sunny>");
						return;
					}
					num = 6;
				}
			}
			Debug.Log("$> forcing rain with rainDice=" + num);
			Scene.WeatherSystem.ForceRain(num);
		}

		
		private void _showgamestats(object arg)
		{
			GameStats gameStats = UnityEngine.Object.FindObjectOfType<GameStats>();
			if (gameStats)
			{
				GameStats.Stats stats = gameStats._stats;
				FieldInfo[] fields = stats.GetType().GetFields();
				string text = string.Empty;
				if (fields != null && fields.Length > 0)
				{
					for (int i = 0; i < fields.Length; i++)
					{
						string text2 = text;
						text = string.Concat(new object[]
						{
							text2,
							fields[i].Name,
							": ",
							fields[i].GetValue(stats),
							"\n"
						});
					}
				}
				Debug.Log("$> GameStats:\n" + text);
			}
			else
			{
				Debug.Log("$> showgamestats failed, GameStats component not found.");
			}
		}

		
		private void _plantallgardens(object arg)
		{
			Garden[] array = UnityEngine.Object.FindObjectsOfType<Garden>();
			if (array != null && array.Length > 0)
			{
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Test = true;
					for (int j = 0; j < array[i].GrowSpots.Length; j++)
					{
						array[i].PlantSeed();
					}
					array[i].Test = false;
				}
				Debug.Log("$> Found " + array.Length + " gardens to grow.");
			}
			else
			{
				Debug.Log("$> plantallgardens failed, didn't find any garden.");
			}
		}

		
		private void _growalldirtpiles(object arg)
		{
			GardenDirtPile[] array = UnityEngine.Object.FindObjectsOfType<GardenDirtPile>();
			if (array != null && array.Length > 0)
			{
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Growth();
				}
				Debug.Log("$> Found " + array.Length + " garden dirt pile to grow.");
			}
			else
			{
				Debug.Log("$> growalldirtpiles failed, didn't find any garden dirt pile.");
			}
		}

		
		private void _togglesheenbillboards(string onoff)
		{
			if (onoff == "on")
			{
				SheenBillboard[] array = UnityEngine.Object.FindObjectsOfType<SheenBillboard>();
				for (int i = 0; i < array.Length; i++)
				{
					array[i].enabled = true;
				}
				Debug.Log("$> Enabled all " + array.Length + " SheenBillboards");
			}
			else if (onoff == "off")
			{
				SheenBillboard[] array2 = UnityEngine.Object.FindObjectsOfType<SheenBillboard>();
				for (int j = 0; j < array2.Length; j++)
				{
					array2[j].enabled = false;
					array2[j].GetComponent<Renderer>().enabled = true;
				}
				Debug.Log("$> Disabled all " + array2.Length + " SheenBillboards and enabled all their renderers");
			}
			else
			{
				Debug.Log("$> usage: togglesheenbillboards <on|off>");
			}
		}

		
		private void _inspectgo(string name)
		{
			if (!string.IsNullOrEmpty(name))
			{
				GameObject gameObject = GameObject.Find(name);
				if (gameObject)
				{
					StringBuilder sb = new StringBuilder();
					sb.AppendLine("inspectgo: " + name);
					Action<Transform> browseRec = null;
					int depth = 0;
					int maxDepth = 10;
					browseRec = delegate(Transform t)
					{
						try
						{
							if (++depth < maxDepth)
							{
								Component[] components = t.gameObject.GetComponents<Behaviour>();
								string text = string.Concat(new object[]
								{
									"\n=> ",
									t.name,
									" -> ",
									t.gameObject.activeSelf,
									", pos=",
									t.position,
									", rot=",
									t.eulerAngles,
									", scale=",
									t.localScale
								});
								sb.AppendLine(text.PadLeft(text.Length + depth, '\t'));
								foreach (Component component in components)
								{
									string text2 = "+ " + component.GetType().Name;
									if (component is Behaviour)
									{
										text2 += ((!((Behaviour)component).enabled) ? "(disabled)" : "(enabled)");
									}
									sb.AppendLine(text2.PadLeft(text2.Length + depth + 1, '\t'));
									foreach (FieldInfo fieldInfo in component.GetType().GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty))
									{
										object value = fieldInfo.GetValue(component);
										string text3 = "|- " + fieldInfo.Name + ": ";
										if (value is IList && !(value is string))
										{
											sb.AppendLine(text3.PadLeft(text3.Length + depth + 1, '\t'));
											foreach (object obj in ((IList)value))
											{
												if (obj == null)
												{
													sb.AppendLine("null".PadLeft("null".Length + depth + 2, '\t'));
												}
												else
												{
													sb.AppendLine(obj.ToString().PadLeft(obj.ToString().Length + depth + 2, '\t'));
												}
											}
										}
										else
										{
											text3 += value;
											sb.AppendLine(text3.PadLeft(text3.Length + depth + 1, '\t'));
										}
									}
								}
								foreach (object obj2 in t)
								{
									Transform obj3 = (Transform)obj2;
									browseRec(obj3);
								}
								depth--;
							}
						}
						catch (Exception exception)
						{
							Debug.LogException(exception);
						}
					};
					browseRec(gameObject.transform);
					File.WriteAllText(Application.dataPath + "/../inspectgo_" + name + ".txt", sb.ToString());
					Debug.Log(string.Concat(new string[]
					{
						"$> inspectgo: ",
						name,
						" wrote output to '",
						Application.dataPath,
						"/../inspectgo_",
						name,
						".txt'"
					}));
				}
				else
				{
					Debug.Log("$> inspectgo: " + name + " not found");
				}
			}
			else
			{
				Debug.Log("$> usage: inspectgo <gameObjectName>");
			}
		}

		
		private void _loadDebugConsoleMod(string filename)
		{
			try
			{
				byte[] rawAssembly = File.ReadAllBytes(filename);
				Assembly assembly = Assembly.Load(rawAssembly);
				foreach (Type type in assembly.GetExportedTypes())
				{
					foreach (MethodInfo methodInfo in type.GetMethods())
					{
						if (methodInfo.Name.StartsWith("_") && methodInfo.IsStatic)
						{
							string text = methodInfo.Name.Substring(1).ToLower();
							this._availableConsoleMethods[text] = methodInfo;
							Debug.Log("$> loadDebugMod added '" + text + "' debug console command");
						}
					}
				}
				Debug.Log("$> loadDebugMod done on file: '" + filename + "'");
			}
			catch (Exception ex)
			{
				Debug.Log(string.Concat(new object[]
				{
					"$> loadDebugMod error on file: '",
					filename,
					"':\n",
					ex
				}));
			}
		}

		
		private void _getlayerculldistance(string args)
		{
			int num = LayerMask.NameToLayer(args);
			if (num >= 0 && num < 32 && CullDistanceManager.Instance)
			{
				Debug.Log(string.Concat(new object[]
				{
					"$> '",
					args,
					"' cull distance == ",
					CullDistanceManager.Instance.LayerCullDistances[num],
					" units"
				}));
				return;
			}
			Debug.Log("$> usage: getlayerculldistance <layerName>");
		}

		
		private void _setlayerculldistance(string args)
		{
			string[] array = args.Split(new char[]
			{
				' '
			});
			if (array.Length == 2)
			{
				int num = LayerMask.NameToLayer(array[0]);
				int num2;
				if (num >= 0 && num < 32 && int.TryParse(array[1], out num2) && CullDistanceManager.Instance)
				{
					CullDistanceManager.Instance.LayerCullDistances[num] = (float)num2;
					Debug.Log(string.Concat(new object[]
					{
						"$> set '",
						array[0],
						"' layer cull distance set to ",
						num2,
						" units"
					}));
					return;
				}
			}
			Debug.Log("$> usage: setlayerculldistance <layerName> <distance>");
		}

		
		private void _showworldposfor(string componentName)
		{
			if (!string.IsNullOrEmpty(componentName))
			{
				Type type = Type.GetType(componentName);
				if (type != null)
				{
					UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(type);
					if (array != null)
					{
						int num = 0;
						foreach (UnityEngine.Object @object in array)
						{
							GameObject gameObject = ((MonoBehaviour)@object).gameObject;
							if (!gameObject.GetComponent<DBH_WorldPosToGuiTexture>())
							{
								gameObject.AddComponent<DBH_WorldPosToGuiTexture>();
								num++;
							}
						}
						Debug.Log(string.Concat(new object[]
						{
							"$> added world position display to ",
							num,
							" ",
							componentName
						}));
						return;
					}
					Debug.Log("$> showworldposfor: found 0 object of type " + componentName);
				}
				else
				{
					Debug.Log("$> showworldposfor: unknown type " + componentName);
				}
			}
			else
			{
				Debug.Log("$> usage: showworldposfor <ObjectType>");
			}
		}

		
		private void _hideworldposfor(string componentName)
		{
			if (!string.IsNullOrEmpty(componentName))
			{
				Type type = Type.GetType(componentName);
				if (type != null)
				{
					UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(type);
					if (array != null)
					{
						int num = 0;
						foreach (UnityEngine.Object @object in array)
						{
							GameObject gameObject = ((MonoBehaviour)@object).gameObject;
							DBH_WorldPosToGuiTexture component = gameObject.GetComponent<DBH_WorldPosToGuiTexture>();
							if (component)
							{
								UnityEngine.Object.Destroy(component);
								num++;
							}
						}
						Debug.Log(string.Concat(new object[]
						{
							"$> removed world position display to ",
							num,
							" ",
							componentName
						}));
						return;
					}
					Debug.Log("$> hideworldposfor: found 0 object of type " + componentName);
				}
				else
				{
					Debug.Log("$> hideworldposfor: unknown type " + componentName);
				}
			}
			else
			{
				Debug.Log("$> usage: hideworldposfor <ObjectType>");
			}
		}

		
		private void _clearsaveslot(string slotNumArg)
		{
			if (!string.IsNullOrEmpty(slotNumArg))
			{
				if (slotNumArg == "all")
				{
					foreach (object obj in Enum.GetValues(typeof(Slots)))
					{
						Slots slot = (Slots)((int)obj);
						SaveSlotUtils.DeleteSlot(PlayerModes.SinglePlayer, slot);
						SaveSlotUtils.DeleteSlot(PlayerModes.Multiplayer, slot);
					}
					Debug.Log("$> Cleared all save slots");
					return;
				}
				string[] array = slotNumArg.Split(new char[]
				{
					' '
				});
				array[0] = array[0].ToLower();
				int num;
				if (int.TryParse(array[1], out num) && num > 0 && num <= Enum.GetValues(typeof(Slots)).Length)
				{
					if (array[0] == "sp")
					{
						SaveSlotUtils.DeleteSlot(PlayerModes.SinglePlayer, (Slots)num);
						Debug.Log(string.Concat(new object[]
						{
							"$> Cleared ",
							array[0],
							" save slot #",
							num
						}));
						return;
					}
					if (array[0] == "mp")
					{
						SaveSlotUtils.DeleteSlot(PlayerModes.Multiplayer, (Slots)num);
						Debug.Log(string.Concat(new object[]
						{
							"$> Cleared ",
							array[0],
							" save slot #",
							num
						}));
						return;
					}
				}
			}
			Debug.Log("$> usage: clearsaveslot <sp|mp|all> <slotNum>");
		}

		
		private void _cutdowntrees(string arg)
		{
			if (arg == "count")
			{
				CoopTreeId[] array = UnityEngine.Object.FindObjectsOfType<CoopTreeId>();
				List<CoopTreeId> list = (from t in array
				where t.lod && !t.lod.enabled && t.lod.CurrentView == null
				select t).ToList<CoopTreeId>();
				List<CoopTreeId> list2 = (from t in array
				where !t.lod
				select t).ToList<CoopTreeId>();
				Debug.Log(string.Format("$> Removed/Cut down/Total trees:  {0}({1:P0}) / {2}({3:P0}) / {4}", new object[]
				{
					list2.Count,
					(float)list2.Count / (float)array.Length,
					list.Count,
					(float)list.Count / (float)array.Length,
					array.Length
				}));
			}
			else if (!BoltNetwork.isClient)
			{
				CoopTreeId[] array2 = UnityEngine.Object.FindObjectsOfType<CoopTreeId>();
				List<CoopTreeId> list3 = (from t in array2
				where t.lod && !t.lod.enabled && t.lod.CurrentView == null
				select t).ToList<CoopTreeId>();
				int count = list3.Count;
				int num2;
				if (arg.EndsWith("%"))
				{
					arg = arg.TrimEnd(new char[]
					{
						'%'
					});
					int num;
					if (!int.TryParse(arg, out num))
					{
						Debug.Log("$> usage: cutdowntrees <count|[percent]|[amount]>");
						return;
					}
					num2 = (int)((float)num / 100f * (float)array2.Length);
				}
				else
				{
					int num3;
					if (!int.TryParse(arg, out num3))
					{
						Debug.Log("$> usage: cutdowntrees <count|[percent]|[amount]>");
						return;
					}
					num2 = num3;
				}
				TreeLodGrid treeLodGrid = UnityEngine.Object.FindObjectOfType<TreeLodGrid>();
				if (num2 > 0)
				{
					List<CoopTreeId> list4 = (from t in array2
					where t.lod != null && t.lod.enabled
					select t).ToList<CoopTreeId>();
					int count2 = list4.Count;
					int num4 = num2 - Mathf.Max(num2 - (array2.Length - count), 0);
					float num5 = Mathf.Max((float)count2 / (float)num4, 1f);
					int num6 = 0;
					for (float num7 = 0f; num7 < (float)count2; num7 += num5)
					{
						int index = (int)num7;
						if (list4[index])
						{
							if (BoltNetwork.isRunning)
							{
								list4[index].SendMessage("OnDestroyCallback");
								list4[index].lod.DontSpawn = true;
								list4[index].entity.Freeze(false);
							}
							foreach (object obj in list4[index].transform)
							{
								Transform transform = (Transform)obj;
								LOD_Stump component = transform.GetComponent<LOD_Stump>();
								if (component)
								{
									component.DespawnCurrent();
									component.CurrentView = null;
								}
								UnityEngine.Object.Destroy(transform.gameObject);
							}
							list4[index].lod.DespawnCurrent();
							list4[index].lod.enabled = false;
							list4[index].lod.CurrentView = null;
							list4[index].lod.SpawnStumpLod();
							if (treeLodGrid)
							{
								treeLodGrid.RegisterCutDownTree(list4[index].transform.position);
							}
							num6++;
						}
					}
					if (num6 != 0 && BoltNetwork.isRunning)
					{
						CoopTreeGrid.SweepGrid();
					}
					Debug.Log(string.Concat(new object[]
					{
						"Cut down ",
						num6,
						" new trees, total is now=",
						count + num6,
						string.Format("({0:P0})", ((float)count + (float)num6) / (float)array2.Length)
					}));
				}
				else if (num2 < 0)
				{
					num2 = Mathf.Abs(num2);
					int num8 = num2 - Mathf.Max(num2 - count, 0);
					float num9 = Mathf.Max((float)count / (float)num8, 1f);
					int num10 = 0;
					for (float num11 = 0f; num11 < (float)count; num11 += num9)
					{
						int index2 = (int)num11;
						if (BoltNetwork.isRunning)
						{
							CoopTreeId coopTreeId = list3[index2];
							if (coopTreeId)
							{
								coopTreeId.RegrowTree();
							}
							list3[index2].lod.DontSpawn = false;
						}
						list3[index2].lod.enabled = true;
						list3[index2].lod.RefreshLODs();
						if (treeLodGrid)
						{
							treeLodGrid.RegisterTreeRegrowth(list3[index2].transform.position);
						}
						foreach (object obj2 in list3[index2].transform)
						{
							Transform transform2 = (Transform)obj2;
							LOD_Stump component2 = transform2.GetComponent<LOD_Stump>();
							if (component2)
							{
								component2.DespawnCurrent();
								component2.CurrentView = null;
							}
							UnityEngine.Object.Destroy(transform2.gameObject);
						}
						num10++;
					}
					if (num10 != 0 && BoltNetwork.isRunning)
					{
						CoopTreeGrid.SweepGrid();
					}
					Debug.Log(string.Concat(new object[]
					{
						"$> Regrowth of ",
						num10,
						" cut down trees, total is now=",
						count - num10,
						string.Format("({0:P0})", ((float)count - (float)num10) / (float)array2.Length)
					}));
				}
			}
			else
			{
				Debug.Log("$> Mp clients cannot use this command to edit amount of cut down trees, only counting is allowed");
			}
		}

		
		private void _cutgrass(string radiusArg)
		{
			if (string.IsNullOrEmpty(radiusArg))
			{
				radiusArg = "1";
			}
			int num = int.Parse(radiusArg);
			NeoGrassCutter.Cut(LocalPlayer.Transform.position, (float)num, true);
			Debug.Log("$> Cut grass arround player with a radius of " + radiusArg);
		}

		
		private void _growgrass(string args)
		{
			if (!string.IsNullOrEmpty(args))
			{
				string[] array = args.Split(new char[]
				{
					' '
				});
				if (array.Length < 3)
				{
					Debug.Log("$> usage: growgrass <radius> <grassLayer> <grassAmount>");
				}
				else
				{
					int num = int.Parse(array[0]);
					int num2 = int.Parse(array[1]);
					int num3 = int.Parse(array[2]);
					NeoGrassCutter.Grow(LocalPlayer.Transform.position, (float)num, num2, num3, true);
					Debug.Log(string.Concat(new object[]
					{
						"$> Grown grass arround player with a radius=",
						num,
						", grassLayer=",
						num2,
						", grassAmount=",
						num3
					}));
				}
			}
			else
			{
				Debug.Log("$> usage: growgrass <radius> <grassLayer> <grassAmount>");
			}
		}

		
		private void _clearallsettings(object arg)
		{
			PlayerPrefs.DeleteAll();
			PlayerPrefs.Save();
			Debug.Log("$> Cleared all settings, restarting game without opening options is required to be fully effective");
		}

		
		private void _testeventmask(object arg)
		{
			foreach (Camera camera in Camera.allCameras)
			{
				camera.eventMask = 0;
			}
			Debug.Log("$> Cleared eventMask for " + Camera.allCameras.Length + " cameras");
		}

		
		private void _dumplobbyinfo(object arg)
		{
			if (CoopLobby.Instance != null)
			{
				P2PSessionState_t s;
				if (SteamNetworking.GetP2PSessionState((!CoopLobby.Instance.Info.ServerId.IsValid()) ? CoopLobby.Instance.Info.LobbyId : CoopLobby.Instance.Info.ServerId, out s))
				{
					Debug.Log("$> Dumping Steam lobby info:");
					CoopSteamManager.Dump("Server", s);
				}
				else
				{
					Debug.Log("$> Failed retrieving Steam lobby info");
				}
			}
			else
			{
				Debug.Log("$> Not in a Steam lobby, nothing to lookup");
			}
		}

		
		private void _gccollect(object o)
		{
			Debug.Log("$> Doing GC.Collect()");
			GC.Collect();
		}

		
		private void _loadlevel(string levelNumS)
		{
			if (!string.IsNullOrEmpty(levelNumS))
			{
				int num;
				if (int.TryParse(levelNumS, out num))
				{
					Debug.Log("$> Loading level " + num);
					Application.LoadLevel(num);
				}
				else
				{
					Debug.Log("$> usage: loadlevel <levelNum>");
				}
			}
			else
			{
				Debug.Log("$> usage: loadlevel <levelNum>");
			}
		}

		
		private void _addmemory(string amount)
		{
			if (amount == "clear")
			{
				JunkCreator.Instance.Clear();
			}
			else
			{
				int amount2 = int.Parse(amount);
				JunkCreator.Instance.AddJunk(amount2);
			}
		}

		
		private void _resetStatsAndAchievements(string param)
		{
			if (param.ToLower() == "all")
			{
				AccountInfo.ResetAllAchievements();
				Debug.Log("$> Reseted all account stats & achievements info");
			}
			else if (param.All((char c) => char.IsUpper(c)))
			{
				if (AccountInfo.ResetAchievement(param))
				{
					Debug.Log("$> Reseted the " + param + " achievement");
				}
				else
				{
					Debug.Log("$> Error reseting the " + param + " achievement");
				}
			}
			else
			{
				Debug.Log("$> usage: resetStatsAndAchievements <all|ACHIEVEMENT_NAME>");
			}
		}

		
		private void _achievementLogLevel(string param)
		{
			param = ((param != null) ? param.ToLower() : string.Empty);
			if (param == "none")
			{
				Achievements.ShowLogs = false;
				Achievements.ShowExceptions = false;
				Debug.Log("$> Set achievements log level to none");
			}
			else if (param == "log")
			{
				Achievements.ShowLogs = true;
				Achievements.ShowExceptions = false;
				Debug.Log("$> Set achievements log level to log");
			}
			else if (param == "error")
			{
				Achievements.ShowLogs = false;
				Achievements.ShowExceptions = true;
				Debug.Log("$> Set achievements log level to error");
			}
			else if (param == "all")
			{
				Achievements.ShowLogs = true;
				Achievements.ShowExceptions = true;
				Debug.Log("$> Set achievements log level to all");
			}
			else
			{
				Debug.Log("$> usage: achievementLogLevel <none|log|error|all>");
			}
		}

		
		private void _diagRenderers(string param)
		{
			bool flag = param == "all";
			Renderer[] array = UnityEngine.Object.FindObjectsOfType<Renderer>();
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Renderer renderer in array)
			{
				if (flag || renderer.isVisible)
				{
					MeshFilter component = renderer.GetComponent<MeshFilter>();
					if (component)
					{
						stringBuilder.AppendLine(renderer.name + "\t\t\t" + component.sharedMesh);
					}
					else if (renderer is SkinnedMeshRenderer)
					{
						stringBuilder.AppendLine(renderer.name + "(Skinned)\t\t\t" + (renderer as SkinnedMeshRenderer).sharedMesh);
					}
				}
			}
			File.WriteAllText(Application.dataPath + "/../diagRenderers_" + DateTime.Now.ToString().Replace('/', '-').Replace(' ', '_').Replace(':', '-') + ".txt", stringBuilder.ToString());
			Debug.Log(string.Concat(new string[]
			{
				"$> diagRenderers (",
				(!flag) ? "visible only" : "all",
				"): wrote output to '",
				Application.dataPath,
				"/../diagRenderers_",
				DateTime.Now.ToString(),
				".txt"
			}));
		}

		
		private void _toggleLowerShadowsAtExtremeLightAngles(string param)
		{
			if (string.IsNullOrEmpty(param))
			{
				Scene.Atmosphere.LowerShadowsAtExtremeLightAngles = !Scene.Atmosphere.LowerShadowsAtExtremeLightAngles;
				Debug.Log("$> Toggled LowerShadowsAtExtremeLightAngles " + ((!Scene.Atmosphere.LowerShadowsAtExtremeLightAngles) ? "off" : "on"));
			}
			else if (param.ToLower() == "on")
			{
				Scene.Atmosphere.LowerShadowsAtExtremeLightAngles = true;
				Debug.Log("$> Toggled LowerShadowsAtExtremeLightAngles " + ((!Scene.Atmosphere.LowerShadowsAtExtremeLightAngles) ? "off" : "on"));
			}
			else if (param.ToLower() == "off")
			{
				Scene.Atmosphere.LowerShadowsAtExtremeLightAngles = false;
				Debug.Log("$> Toggled LowerShadowsAtExtremeLightAngles " + ((!Scene.Atmosphere.LowerShadowsAtExtremeLightAngles) ? "off" : "on"));
			}
			else
			{
				Debug.Log("$> usage: toggleLowerShadowsAtExtremeLightAngles <on|off|> ");
			}
		}

		
		private void _lightingTimeOfDayOverride(string param)
		{
			param = ((!string.IsNullOrEmpty(param)) ? param.ToLower() : string.Empty);
			Func<string> func = delegate
			{
				float num2 = (Scene.Atmosphere.LightingTimeOfDayOverrideValue + 180f) % 360f * 0.06666667f;
				return (int)num2 + "h" + (int)((num2 - (float)((int)num2)) * 60f);
			};
			string text = param;
			if (text != null)
			{
				if (DebugConsole.<>f__switch$map21 == null)
				{
					DebugConsole.<>f__switch$map21 = new Dictionary<string, int>(5)
					{
						{
							"off",
							0
						},
						{
							"morning",
							1
						},
						{
							"noon",
							2
						},
						{
							"sunset",
							3
						},
						{
							"night",
							4
						}
					};
				}
				int num;
				if (DebugConsole.<>f__switch$map21.TryGetValue(text, out num))
				{
					switch (num)
					{
					case 0:
						if (Scene.Atmosphere.OverrideLightingTimeOfDay)
						{
							Scene.Atmosphere.OverrideLightingTimeOfDay = false;
							Debug.Log("$> Disabled lighting time of Day override");
						}
						else
						{
							Debug.Log("$> Lighting time of Day override is already disabled");
						}
						return;
					case 1:
						Scene.Atmosphere.OverrideLightingTimeOfDay = true;
						Scene.Atmosphere.LightingTimeOfDayOverrideValue = 275f;
						Debug.Log("$> Enabled lighting time of Day override at: morning [" + func() + "]");
						break;
					case 2:
						Scene.Atmosphere.OverrideLightingTimeOfDay = true;
						Scene.Atmosphere.LightingTimeOfDayOverrideValue = 0f;
						Debug.Log("$> Enabled lighting time of Day override at: noon [" + func() + "]");
						break;
					case 3:
						Scene.Atmosphere.OverrideLightingTimeOfDay = true;
						Scene.Atmosphere.LightingTimeOfDayOverrideValue = 90f;
						Debug.Log("$> Enabled lighting time of Day override at: sunset [" + func() + "]");
						break;
					case 4:
						Scene.Atmosphere.OverrideLightingTimeOfDay = true;
						Scene.Atmosphere.LightingTimeOfDayOverrideValue = 180f;
						Debug.Log("$> Enabled lighting time of Day override at: night [" + func() + "]");
						break;
					default:
						goto IL_1DF;
					}
					return;
				}
			}
			IL_1DF:
			Debug.Log("$> usage: lightingTimeOfDayOverride <off|morning|noon|sunset|night> (currently=" + ((!Scene.Atmosphere.OverrideLightingTimeOfDay) ? "off" : "on") + ")");
		}

		
		private float wsMaxMsValue;

		
		public bool _showOverlay;

		
		public bool _showLog;

		
		public bool _showConsole;

		
		public bool _showGamePadWheel;

		
		public bool _showPlayerStats;

		
		public GUIStyle _consoleRowStyle;

		
		public GUIStyle _logRowStyle;

		
		public GUIStyle _textStyle;

		
		[Multiline]
		public string _reportFormat = "payload={{\"username\": \"{0}\", \"text\": \"\n************************************\n{1}\"}}";

		
		public string _reportUrl = string.Empty;

		
		public static int BatchedTasksNear;

		
		public static int BatchedTasksFar;

		
		private Queue<LogContent> _logs;

		
		private LogContent _lastLog;

		
		private DebugConsoleRoutine _routineMB;

		
		private Coroutine _inputRoutine;

		
		private bool _destroyOnTitleSceneLoad;

		
		private int _maxLogs;

		
		private Vector2 _logsScrollPos;

		
		private string[] _history = new string[100];

		
		private int _historyEnd = -1;

		
		private int _historyCurrent = -1;

		
		private string _consoleInput = string.Empty;

		
		private string _autocomplete;

		
		private char[] _alphaNum;

		
		private float _fps = 60f;

		
		private bool _showWSDetail;

		
		private bool _focusConsoleField;

		
		private bool _selectConsoleText;

		
		private Dictionary<string, MethodInfo> _availableConsoleMethods;

		
		private Dictionary<Type, int> _counters;

		
		private Dictionary<string, string> _gamepadWheelEntries = new Dictionary<string, string>();

		
		private static Dictionary<Type, int> Counters;

		
		private static DebugConsole Instance;

		
		private Transform lastLocalTarget;

		
		private List<GameObject> _disabledGOs = new List<GameObject>();

		
		private GameObject CoopAnimalController;
	}
}
