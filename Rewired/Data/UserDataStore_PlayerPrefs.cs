using System;
using System.Collections;
using System.Collections.Generic;
using Rewired.Utils.Libraries.TinyJson;
using UnityEngine;

namespace Rewired.Data
{
	
	public class UserDataStore_PlayerPrefs : UserDataStore
	{
		
		
		
		public bool IsEnabled
		{
			get
			{
				return this.isEnabled;
			}
			set
			{
				this.isEnabled = value;
			}
		}

		
		
		
		public bool LoadDataOnStart
		{
			get
			{
				return this.loadDataOnStart;
			}
			set
			{
				this.loadDataOnStart = value;
			}
		}

		
		
		
		public bool LoadJoystickAssignments
		{
			get
			{
				return this.loadJoystickAssignments;
			}
			set
			{
				this.loadJoystickAssignments = value;
			}
		}

		
		
		
		public bool LoadKeyboardAssignments
		{
			get
			{
				return this.loadKeyboardAssignments;
			}
			set
			{
				this.loadKeyboardAssignments = value;
			}
		}

		
		
		
		public bool LoadMouseAssignments
		{
			get
			{
				return this.loadMouseAssignments;
			}
			set
			{
				this.loadMouseAssignments = value;
			}
		}

		
		
		
		public string PlayerPrefsKeyPrefix
		{
			get
			{
				return this.playerPrefsKeyPrefix;
			}
			set
			{
				this.playerPrefsKeyPrefix = value;
			}
		}

		
		
		private string playerPrefsKey_controllerAssignments
		{
			get
			{
				return string.Format("{0}_{1}", this.playerPrefsKeyPrefix, "ControllerAssignments");
			}
		}

		
		
		private bool loadControllerAssignments
		{
			get
			{
				return this.loadKeyboardAssignments || this.loadMouseAssignments || this.loadJoystickAssignments;
			}
		}

		
		public override void Save()
		{
			if (!this.isEnabled)
			{
				Debug.LogWarning("Rewired: UserDataStore_PlayerPrefs is disabled and will not save any data.", this);
				return;
			}
			this.SaveAll();
		}

		
		public override void SaveControllerData(int playerId, ControllerType controllerType, int controllerId)
		{
			if (!this.isEnabled)
			{
				Debug.LogWarning("Rewired: UserDataStore_PlayerPrefs is disabled and will not save any data.", this);
				return;
			}
			this.SaveControllerDataNow(playerId, controllerType, controllerId);
		}

		
		public override void SaveControllerData(ControllerType controllerType, int controllerId)
		{
			if (!this.isEnabled)
			{
				Debug.LogWarning("Rewired: UserDataStore_PlayerPrefs is disabled and will not save any data.", this);
				return;
			}
			this.SaveControllerDataNow(controllerType, controllerId);
		}

		
		public override void SavePlayerData(int playerId)
		{
			if (!this.isEnabled)
			{
				Debug.LogWarning("Rewired: UserDataStore_PlayerPrefs is disabled and will not save any data.", this);
				return;
			}
			this.SavePlayerDataNow(playerId);
		}

		
		public override void SaveInputBehavior(int playerId, int behaviorId)
		{
			if (!this.isEnabled)
			{
				Debug.LogWarning("Rewired: UserDataStore_PlayerPrefs is disabled and will not save any data.", this);
				return;
			}
			this.SaveInputBehaviorNow(playerId, behaviorId);
		}

		
		public override void Load()
		{
			if (!this.isEnabled)
			{
				Debug.LogWarning("Rewired: UserDataStore_PlayerPrefs is disabled and will not load any data.", this);
				return;
			}
			int num = this.LoadAll();
		}

		
		public override void LoadControllerData(int playerId, ControllerType controllerType, int controllerId)
		{
			if (!this.isEnabled)
			{
				Debug.LogWarning("Rewired: UserDataStore_PlayerPrefs is disabled and will not load any data.", this);
				return;
			}
			int num = this.LoadControllerDataNow(playerId, controllerType, controllerId);
		}

		
		public override void LoadControllerData(ControllerType controllerType, int controllerId)
		{
			if (!this.isEnabled)
			{
				Debug.LogWarning("Rewired: UserDataStore_PlayerPrefs is disabled and will not load any data.", this);
				return;
			}
			int num = this.LoadControllerDataNow(controllerType, controllerId);
		}

		
		public override void LoadPlayerData(int playerId)
		{
			if (!this.isEnabled)
			{
				Debug.LogWarning("Rewired: UserDataStore_PlayerPrefs is disabled and will not load any data.", this);
				return;
			}
			int num = this.LoadPlayerDataNow(playerId);
		}

		
		public override void LoadInputBehavior(int playerId, int behaviorId)
		{
			if (!this.isEnabled)
			{
				Debug.LogWarning("Rewired: UserDataStore_PlayerPrefs is disabled and will not load any data.", this);
				return;
			}
			int num = this.LoadInputBehaviorNow(playerId, behaviorId);
		}

		
		protected override void OnInitialize()
		{
			if (this.loadDataOnStart)
			{
				this.Load();
				if (this.loadControllerAssignments && ReInput.controllers.joystickCount > 0)
				{
					this.SaveControllerAssignments();
				}
			}
		}

		
		protected override void OnControllerConnected(ControllerStatusChangedEventArgs args)
		{
			if (!this.isEnabled)
			{
				return;
			}
			if (args.controllerType == ControllerType.Joystick)
			{
				int num = this.LoadJoystickData(args.controllerId);
				if (this.loadDataOnStart && this.loadJoystickAssignments && !this.wasJoystickEverDetected)
				{
					base.StartCoroutine(this.LoadJoystickAssignmentsDeferred());
				}
				if (this.loadJoystickAssignments && !this.deferredJoystickAssignmentLoadPending)
				{
					this.SaveControllerAssignments();
				}
				this.wasJoystickEverDetected = true;
			}
		}

		
		protected override void OnControllerPreDiscconnect(ControllerStatusChangedEventArgs args)
		{
			if (!this.isEnabled)
			{
				return;
			}
			if (args.controllerType == ControllerType.Joystick)
			{
				this.SaveJoystickData(args.controllerId);
			}
		}

		
		protected override void OnControllerDisconnected(ControllerStatusChangedEventArgs args)
		{
			if (!this.isEnabled)
			{
				return;
			}
			if (this.loadControllerAssignments)
			{
				this.SaveControllerAssignments();
			}
		}

		
		private int LoadAll()
		{
			int num = 0;
			if (this.loadControllerAssignments && this.LoadControllerAssignmentsNow())
			{
				num++;
			}
			IList<Player> allPlayers = ReInput.players.AllPlayers;
			for (int i = 0; i < allPlayers.Count; i++)
			{
				num += this.LoadPlayerDataNow(allPlayers[i]);
			}
			return num + this.LoadAllJoystickCalibrationData();
		}

		
		private int LoadPlayerDataNow(int playerId)
		{
			return this.LoadPlayerDataNow(ReInput.players.GetPlayer(playerId));
		}

		
		private int LoadPlayerDataNow(Player player)
		{
			if (player == null)
			{
				return 0;
			}
			int num = 0;
			num += this.LoadInputBehaviors(player.id);
			num += this.LoadControllerMaps(player.id, ControllerType.Keyboard, 0);
			num += this.LoadControllerMaps(player.id, ControllerType.Mouse, 0);
			foreach (Joystick joystick in player.controllers.Joysticks)
			{
				num += this.LoadControllerMaps(player.id, ControllerType.Joystick, joystick.id);
			}
			return num;
		}

		
		private int LoadAllJoystickCalibrationData()
		{
			int num = 0;
			IList<Joystick> joysticks = ReInput.controllers.Joysticks;
			for (int i = 0; i < joysticks.Count; i++)
			{
				num += this.LoadJoystickCalibrationData(joysticks[i]);
			}
			return num;
		}

		
		private int LoadJoystickCalibrationData(Joystick joystick)
		{
			if (joystick == null)
			{
				return 0;
			}
			return (!joystick.ImportCalibrationMapFromXmlString(this.GetJoystickCalibrationMapXml(joystick))) ? 0 : 1;
		}

		
		private int LoadJoystickCalibrationData(int joystickId)
		{
			return this.LoadJoystickCalibrationData(ReInput.controllers.GetJoystick(joystickId));
		}

		
		private int LoadJoystickData(int joystickId)
		{
			int num = 0;
			IList<Player> allPlayers = ReInput.players.AllPlayers;
			for (int i = 0; i < allPlayers.Count; i++)
			{
				Player player = allPlayers[i];
				if (player.controllers.ContainsController(ControllerType.Joystick, joystickId))
				{
					num += this.LoadControllerMaps(player.id, ControllerType.Joystick, joystickId);
				}
			}
			return num + this.LoadJoystickCalibrationData(joystickId);
		}

		
		private int LoadControllerDataNow(int playerId, ControllerType controllerType, int controllerId)
		{
			int num = 0;
			num += this.LoadControllerMaps(playerId, controllerType, controllerId);
			return num + this.LoadControllerDataNow(controllerType, controllerId);
		}

		
		private int LoadControllerDataNow(ControllerType controllerType, int controllerId)
		{
			int num = 0;
			if (controllerType == ControllerType.Joystick)
			{
				num += this.LoadJoystickCalibrationData(controllerId);
			}
			return num;
		}

		
		private int LoadControllerMaps(int playerId, ControllerType controllerType, int controllerId)
		{
			int num = 0;
			Player player = ReInput.players.GetPlayer(playerId);
			if (player == null)
			{
				return num;
			}
			Controller controller = ReInput.controllers.GetController(controllerType, controllerId);
			if (controller == null)
			{
				return num;
			}
			List<UserDataStore_PlayerPrefs.SavedControllerMapData> allControllerMapsXml = this.GetAllControllerMapsXml(player, true, controller);
			if (allControllerMapsXml.Count == 0)
			{
				return num;
			}
			num += player.controllers.maps.AddMapsFromXml(controllerType, controllerId, UserDataStore_PlayerPrefs.SavedControllerMapData.GetXmlStringList(allControllerMapsXml));
			this.AddDefaultMappingsForNewActions(player, allControllerMapsXml, controllerType, controllerId);
			return num;
		}

		
		private int LoadInputBehaviors(int playerId)
		{
			Player player = ReInput.players.GetPlayer(playerId);
			if (player == null)
			{
				return 0;
			}
			int num = 0;
			IList<InputBehavior> inputBehaviors = ReInput.mapping.GetInputBehaviors(player.id);
			for (int i = 0; i < inputBehaviors.Count; i++)
			{
				num += this.LoadInputBehaviorNow(player, inputBehaviors[i]);
			}
			return num;
		}

		
		private int LoadInputBehaviorNow(int playerId, int behaviorId)
		{
			Player player = ReInput.players.GetPlayer(playerId);
			if (player == null)
			{
				return 0;
			}
			InputBehavior inputBehavior = ReInput.mapping.GetInputBehavior(playerId, behaviorId);
			if (inputBehavior == null)
			{
				return 0;
			}
			return this.LoadInputBehaviorNow(player, inputBehavior);
		}

		
		private int LoadInputBehaviorNow(Player player, InputBehavior inputBehavior)
		{
			if (player == null || inputBehavior == null)
			{
				return 0;
			}
			string inputBehaviorXml = this.GetInputBehaviorXml(player, inputBehavior.id);
			if (inputBehaviorXml == null || inputBehaviorXml == string.Empty)
			{
				return 0;
			}
			return (!inputBehavior.ImportXmlString(inputBehaviorXml)) ? 0 : 1;
		}

		
		private bool LoadControllerAssignmentsNow()
		{
			try
			{
				UserDataStore_PlayerPrefs.ControllerAssignmentSaveInfo controllerAssignmentSaveInfo = this.LoadControllerAssignmentData();
				if (controllerAssignmentSaveInfo == null)
				{
					return false;
				}
				if (this.loadKeyboardAssignments || this.loadMouseAssignments)
				{
					this.LoadKeyboardAndMouseAssignmentsNow(controllerAssignmentSaveInfo);
				}
				if (this.loadJoystickAssignments)
				{
					this.LoadJoystickAssignmentsNow(controllerAssignmentSaveInfo);
				}
			}
			catch
			{
			}
			return true;
		}

		
		private bool LoadKeyboardAndMouseAssignmentsNow(UserDataStore_PlayerPrefs.ControllerAssignmentSaveInfo data)
		{
			try
			{
				if (data == null && (data = this.LoadControllerAssignmentData()) == null)
				{
					return false;
				}
				foreach (Player player in ReInput.players.AllPlayers)
				{
					if (data.ContainsPlayer(player.id))
					{
						UserDataStore_PlayerPrefs.ControllerAssignmentSaveInfo.PlayerInfo playerInfo = data.players[data.IndexOfPlayer(player.id)];
						if (this.loadKeyboardAssignments)
						{
							player.controllers.hasKeyboard = playerInfo.hasKeyboard;
						}
						if (this.loadMouseAssignments)
						{
							player.controllers.hasMouse = playerInfo.hasMouse;
						}
					}
				}
			}
			catch
			{
			}
			return true;
		}

		
		private bool LoadJoystickAssignmentsNow(UserDataStore_PlayerPrefs.ControllerAssignmentSaveInfo data)
		{
			try
			{
				if (ReInput.controllers.joystickCount == 0)
				{
					return false;
				}
				if (data == null && (data = this.LoadControllerAssignmentData()) == null)
				{
					return false;
				}
				foreach (Player player in ReInput.players.AllPlayers)
				{
					player.controllers.ClearControllersOfType(ControllerType.Joystick);
				}
				List<UserDataStore_PlayerPrefs.JoystickAssignmentHistoryInfo> list = (!this.loadJoystickAssignments) ? null : new List<UserDataStore_PlayerPrefs.JoystickAssignmentHistoryInfo>();
				foreach (Player player2 in ReInput.players.AllPlayers)
				{
					if (data.ContainsPlayer(player2.id))
					{
						UserDataStore_PlayerPrefs.ControllerAssignmentSaveInfo.PlayerInfo playerInfo = data.players[data.IndexOfPlayer(player2.id)];
						for (int i = 0; i < playerInfo.joystickCount; i++)
						{
							UserDataStore_PlayerPrefs.ControllerAssignmentSaveInfo.JoystickInfo joystickInfo2 = playerInfo.joysticks[i];
							if (joystickInfo2 != null)
							{
								Joystick joystick = this.FindJoystickPrecise(joystickInfo2);
								if (joystick != null)
								{
									if (list.Find((UserDataStore_PlayerPrefs.JoystickAssignmentHistoryInfo x) => x.joystick == joystick) == null)
									{
										list.Add(new UserDataStore_PlayerPrefs.JoystickAssignmentHistoryInfo(joystick, joystickInfo2.id));
									}
									player2.controllers.AddController(joystick, false);
								}
							}
						}
					}
				}
				if (this.allowImpreciseJoystickAssignmentMatching)
				{
					foreach (Player player3 in ReInput.players.AllPlayers)
					{
						if (data.ContainsPlayer(player3.id))
						{
							UserDataStore_PlayerPrefs.ControllerAssignmentSaveInfo.PlayerInfo playerInfo2 = data.players[data.IndexOfPlayer(player3.id)];
							for (int j = 0; j < playerInfo2.joystickCount; j++)
							{
								UserDataStore_PlayerPrefs.ControllerAssignmentSaveInfo.JoystickInfo joystickInfo = playerInfo2.joysticks[j];
								if (joystickInfo != null)
								{
									Joystick joystick2 = null;
									int num = list.FindIndex((UserDataStore_PlayerPrefs.JoystickAssignmentHistoryInfo x) => x.oldJoystickId == joystickInfo.id);
									if (num >= 0)
									{
										joystick2 = list[num].joystick;
									}
									else
									{
										List<Joystick> list2;
										if (!this.TryFindJoysticksImprecise(joystickInfo, out list2))
										{
											goto IL_30F;
										}
										using (List<Joystick>.Enumerator enumerator4 = list2.GetEnumerator())
										{
											while (enumerator4.MoveNext())
											{
												Joystick match = enumerator4.Current;
												if (list.Find((UserDataStore_PlayerPrefs.JoystickAssignmentHistoryInfo x) => x.joystick == match) == null)
												{
													joystick2 = match;
													break;
												}
											}
										}
										if (joystick2 == null)
										{
											goto IL_30F;
										}
										list.Add(new UserDataStore_PlayerPrefs.JoystickAssignmentHistoryInfo(joystick2, joystickInfo.id));
									}
									player3.controllers.AddController(joystick2, false);
								}
								IL_30F:;
							}
						}
					}
				}
			}
			catch
			{
			}
			if (ReInput.configuration.autoAssignJoysticks)
			{
				ReInput.controllers.AutoAssignJoysticks();
			}
			return true;
		}

		
		private UserDataStore_PlayerPrefs.ControllerAssignmentSaveInfo LoadControllerAssignmentData()
		{
			UserDataStore_PlayerPrefs.ControllerAssignmentSaveInfo result;
			try
			{
				if (!PlayerPrefs.HasKey(this.playerPrefsKey_controllerAssignments))
				{
					result = null;
				}
				else
				{
					string @string = PlayerPrefs.GetString(this.playerPrefsKey_controllerAssignments);
					if (string.IsNullOrEmpty(@string))
					{
						result = null;
					}
					else
					{
						UserDataStore_PlayerPrefs.ControllerAssignmentSaveInfo controllerAssignmentSaveInfo = JsonParser.FromJson<UserDataStore_PlayerPrefs.ControllerAssignmentSaveInfo>(@string);
						if (controllerAssignmentSaveInfo == null || controllerAssignmentSaveInfo.playerCount == 0)
						{
							result = null;
						}
						else
						{
							result = controllerAssignmentSaveInfo;
						}
					}
				}
			}
			catch
			{
				result = null;
			}
			return result;
		}

		
		private IEnumerator LoadJoystickAssignmentsDeferred()
		{
			this.deferredJoystickAssignmentLoadPending = true;
			yield return new WaitForEndOfFrame();
			if (!ReInput.isReady)
			{
				yield break;
			}
			if (this.LoadJoystickAssignmentsNow(null))
			{
			}
			this.SaveControllerAssignments();
			this.deferredJoystickAssignmentLoadPending = false;
			yield break;
		}

		
		private void SaveAll()
		{
			IList<Player> allPlayers = ReInput.players.AllPlayers;
			for (int i = 0; i < allPlayers.Count; i++)
			{
				this.SavePlayerDataNow(allPlayers[i]);
			}
			this.SaveAllJoystickCalibrationData();
			if (this.loadControllerAssignments)
			{
				this.SaveControllerAssignments();
			}
			PlayerPrefs.Save();
		}

		
		private void SavePlayerDataNow(int playerId)
		{
			this.SavePlayerDataNow(ReInput.players.GetPlayer(playerId));
			PlayerPrefs.Save();
		}

		
		private void SavePlayerDataNow(Player player)
		{
			if (player == null)
			{
				return;
			}
			PlayerSaveData saveData = player.GetSaveData(true);
			this.SaveInputBehaviors(player, saveData);
			this.SaveControllerMaps(player, saveData);
		}

		
		private void SaveAllJoystickCalibrationData()
		{
			IList<Joystick> joysticks = ReInput.controllers.Joysticks;
			for (int i = 0; i < joysticks.Count; i++)
			{
				this.SaveJoystickCalibrationData(joysticks[i]);
			}
		}

		
		private void SaveJoystickCalibrationData(int joystickId)
		{
			this.SaveJoystickCalibrationData(ReInput.controllers.GetJoystick(joystickId));
		}

		
		private void SaveJoystickCalibrationData(Joystick joystick)
		{
			if (joystick == null)
			{
				return;
			}
			JoystickCalibrationMapSaveData calibrationMapSaveData = joystick.GetCalibrationMapSaveData();
			string joystickCalibrationMapPlayerPrefsKey = this.GetJoystickCalibrationMapPlayerPrefsKey(joystick);
			PlayerPrefs.SetString(joystickCalibrationMapPlayerPrefsKey, calibrationMapSaveData.map.ToXmlString());
		}

		
		private void SaveJoystickData(int joystickId)
		{
			IList<Player> allPlayers = ReInput.players.AllPlayers;
			for (int i = 0; i < allPlayers.Count; i++)
			{
				Player player = allPlayers[i];
				if (player.controllers.ContainsController(ControllerType.Joystick, joystickId))
				{
					this.SaveControllerMaps(player.id, ControllerType.Joystick, joystickId);
				}
			}
			this.SaveJoystickCalibrationData(joystickId);
		}

		
		private void SaveControllerDataNow(int playerId, ControllerType controllerType, int controllerId)
		{
			this.SaveControllerMaps(playerId, controllerType, controllerId);
			this.SaveControllerDataNow(controllerType, controllerId);
			PlayerPrefs.Save();
		}

		
		private void SaveControllerDataNow(ControllerType controllerType, int controllerId)
		{
			if (controllerType == ControllerType.Joystick)
			{
				this.SaveJoystickCalibrationData(controllerId);
			}
			PlayerPrefs.Save();
		}

		
		private void SaveControllerMaps(Player player, PlayerSaveData playerSaveData)
		{
			foreach (ControllerMapSaveData saveData in playerSaveData.AllControllerMapSaveData)
			{
				this.SaveControllerMap(player, saveData);
			}
		}

		
		private void SaveControllerMaps(int playerId, ControllerType controllerType, int controllerId)
		{
			Player player = ReInput.players.GetPlayer(playerId);
			if (player == null)
			{
				return;
			}
			if (!player.controllers.ContainsController(controllerType, controllerId))
			{
				return;
			}
			ControllerMapSaveData[] mapSaveData = player.controllers.maps.GetMapSaveData(controllerType, controllerId, true);
			if (mapSaveData == null)
			{
				return;
			}
			for (int i = 0; i < mapSaveData.Length; i++)
			{
				this.SaveControllerMap(player, mapSaveData[i]);
			}
		}

		
		private void SaveControllerMap(Player player, ControllerMapSaveData saveData)
		{
			string key = this.GetControllerMapPlayerPrefsKey(player, saveData.controller, saveData.categoryId, saveData.layoutId);
			PlayerPrefs.SetString(key, saveData.map.ToXmlString());
			key = this.GetControllerMapKnownActionIdsPlayerPrefsKey(player, saveData.controller, saveData.categoryId, saveData.layoutId);
			PlayerPrefs.SetString(key, this.GetAllActionIdsString());
		}

		
		private void SaveInputBehaviors(Player player, PlayerSaveData playerSaveData)
		{
			if (player == null)
			{
				return;
			}
			InputBehavior[] inputBehaviors = playerSaveData.inputBehaviors;
			for (int i = 0; i < inputBehaviors.Length; i++)
			{
				this.SaveInputBehaviorNow(player, inputBehaviors[i]);
			}
		}

		
		private void SaveInputBehaviorNow(int playerId, int behaviorId)
		{
			Player player = ReInput.players.GetPlayer(playerId);
			if (player == null)
			{
				return;
			}
			InputBehavior inputBehavior = ReInput.mapping.GetInputBehavior(playerId, behaviorId);
			if (inputBehavior == null)
			{
				return;
			}
			this.SaveInputBehaviorNow(player, inputBehavior);
			PlayerPrefs.Save();
		}

		
		private void SaveInputBehaviorNow(Player player, InputBehavior inputBehavior)
		{
			if (player == null || inputBehavior == null)
			{
				return;
			}
			string inputBehaviorPlayerPrefsKey = this.GetInputBehaviorPlayerPrefsKey(player, inputBehavior.id);
			PlayerPrefs.SetString(inputBehaviorPlayerPrefsKey, inputBehavior.ToXmlString());
		}

		
		private bool SaveControllerAssignments()
		{
			try
			{
				UserDataStore_PlayerPrefs.ControllerAssignmentSaveInfo controllerAssignmentSaveInfo = new UserDataStore_PlayerPrefs.ControllerAssignmentSaveInfo(ReInput.players.allPlayerCount);
				for (int i = 0; i < ReInput.players.allPlayerCount; i++)
				{
					Player player = ReInput.players.AllPlayers[i];
					UserDataStore_PlayerPrefs.ControllerAssignmentSaveInfo.PlayerInfo playerInfo = new UserDataStore_PlayerPrefs.ControllerAssignmentSaveInfo.PlayerInfo();
					controllerAssignmentSaveInfo.players[i] = playerInfo;
					playerInfo.id = player.id;
					playerInfo.hasKeyboard = player.controllers.hasKeyboard;
					playerInfo.hasMouse = player.controllers.hasMouse;
					UserDataStore_PlayerPrefs.ControllerAssignmentSaveInfo.JoystickInfo[] array = new UserDataStore_PlayerPrefs.ControllerAssignmentSaveInfo.JoystickInfo[player.controllers.joystickCount];
					playerInfo.joysticks = array;
					for (int j = 0; j < player.controllers.joystickCount; j++)
					{
						Joystick joystick = player.controllers.Joysticks[j];
						array[j] = new UserDataStore_PlayerPrefs.ControllerAssignmentSaveInfo.JoystickInfo
						{
							instanceGuid = joystick.deviceInstanceGuid,
							id = joystick.id,
							hardwareIdentifier = joystick.hardwareIdentifier
						};
					}
				}
				PlayerPrefs.SetString(this.playerPrefsKey_controllerAssignments, JsonWriter.ToJson(controllerAssignmentSaveInfo));
				PlayerPrefs.Save();
			}
			catch
			{
			}
			return true;
		}

		
		private bool ControllerAssignmentSaveDataExists()
		{
			if (!PlayerPrefs.HasKey(this.playerPrefsKey_controllerAssignments))
			{
				return false;
			}
			string @string = PlayerPrefs.GetString(this.playerPrefsKey_controllerAssignments);
			return !string.IsNullOrEmpty(@string);
		}

		
		private string GetBasePlayerPrefsKey(Player player)
		{
			string str = this.playerPrefsKeyPrefix;
			return str + "|playerName=" + player.name;
		}

		
		private string GetControllerMapPlayerPrefsKey(Player player, Controller controller, int categoryId, int layoutId)
		{
			string text = this.GetBasePlayerPrefsKey(player);
			text += "|dataType=ControllerMap";
			text = text + "|controllerMapType=" + controller.mapTypeString;
			string text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"|categoryId=",
				categoryId,
				"|layoutId=",
				layoutId
			});
			text = text + "|hardwareIdentifier=" + controller.hardwareIdentifier;
			if (controller.type == ControllerType.Joystick)
			{
				text = text + "|hardwareGuid=" + ((Joystick)controller).hardwareTypeGuid.ToString();
			}
			return text;
		}

		
		private string GetControllerMapKnownActionIdsPlayerPrefsKey(Player player, Controller controller, int categoryId, int layoutId)
		{
			string text = this.GetBasePlayerPrefsKey(player);
			text += "|dataType=ControllerMap_KnownActionIds";
			text = text + "|controllerMapType=" + controller.mapTypeString;
			string text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"|categoryId=",
				categoryId,
				"|layoutId=",
				layoutId
			});
			text = text + "|hardwareIdentifier=" + controller.hardwareIdentifier;
			if (controller.type == ControllerType.Joystick)
			{
				text = text + "|hardwareGuid=" + ((Joystick)controller).hardwareTypeGuid.ToString();
			}
			return text;
		}

		
		private string GetJoystickCalibrationMapPlayerPrefsKey(Joystick joystick)
		{
			string str = this.playerPrefsKeyPrefix;
			str += "|dataType=CalibrationMap";
			str = str + "|controllerType=" + joystick.type.ToString();
			str = str + "|hardwareIdentifier=" + joystick.hardwareIdentifier;
			return str + "|hardwareGuid=" + joystick.hardwareTypeGuid.ToString();
		}

		
		private string GetInputBehaviorPlayerPrefsKey(Player player, int inputBehaviorId)
		{
			string text = this.GetBasePlayerPrefsKey(player);
			text += "|dataType=InputBehavior";
			return text + "|id=" + inputBehaviorId;
		}

		
		private string GetControllerMapXml(Player player, Controller controller, int categoryId, int layoutId)
		{
			string controllerMapPlayerPrefsKey = this.GetControllerMapPlayerPrefsKey(player, controller, categoryId, layoutId);
			if (!PlayerPrefs.HasKey(controllerMapPlayerPrefsKey))
			{
				return string.Empty;
			}
			return PlayerPrefs.GetString(controllerMapPlayerPrefsKey);
		}

		
		private List<int> GetControllerMapKnownActionIds(Player player, Controller controller, int categoryId, int layoutId)
		{
			List<int> list = new List<int>();
			string controllerMapKnownActionIdsPlayerPrefsKey = this.GetControllerMapKnownActionIdsPlayerPrefsKey(player, controller, categoryId, layoutId);
			if (!PlayerPrefs.HasKey(controllerMapKnownActionIdsPlayerPrefsKey))
			{
				return list;
			}
			string @string = PlayerPrefs.GetString(controllerMapKnownActionIdsPlayerPrefsKey);
			if (string.IsNullOrEmpty(@string))
			{
				return list;
			}
			string[] array = @string.Split(new char[]
			{
				','
			});
			for (int i = 0; i < array.Length; i++)
			{
				if (!string.IsNullOrEmpty(array[i]))
				{
					int item;
					if (int.TryParse(array[i], out item))
					{
						list.Add(item);
					}
				}
			}
			return list;
		}

		
		private List<UserDataStore_PlayerPrefs.SavedControllerMapData> GetAllControllerMapsXml(Player player, bool userAssignableMapsOnly, Controller controller)
		{
			List<UserDataStore_PlayerPrefs.SavedControllerMapData> list = new List<UserDataStore_PlayerPrefs.SavedControllerMapData>();
			IList<InputMapCategory> mapCategories = ReInput.mapping.MapCategories;
			for (int i = 0; i < mapCategories.Count; i++)
			{
				InputMapCategory inputMapCategory = mapCategories[i];
				if (!userAssignableMapsOnly || inputMapCategory.userAssignable)
				{
					IList<InputLayout> list2 = ReInput.mapping.MapLayouts(controller.type);
					for (int j = 0; j < list2.Count; j++)
					{
						InputLayout inputLayout = list2[j];
						string controllerMapXml = this.GetControllerMapXml(player, controller, inputMapCategory.id, inputLayout.id);
						if (!(controllerMapXml == string.Empty))
						{
							List<int> controllerMapKnownActionIds = this.GetControllerMapKnownActionIds(player, controller, inputMapCategory.id, inputLayout.id);
							list.Add(new UserDataStore_PlayerPrefs.SavedControllerMapData(controllerMapXml, controllerMapKnownActionIds));
						}
					}
				}
			}
			return list;
		}

		
		private string GetJoystickCalibrationMapXml(Joystick joystick)
		{
			string joystickCalibrationMapPlayerPrefsKey = this.GetJoystickCalibrationMapPlayerPrefsKey(joystick);
			if (!PlayerPrefs.HasKey(joystickCalibrationMapPlayerPrefsKey))
			{
				return string.Empty;
			}
			return PlayerPrefs.GetString(joystickCalibrationMapPlayerPrefsKey);
		}

		
		private string GetInputBehaviorXml(Player player, int id)
		{
			string inputBehaviorPlayerPrefsKey = this.GetInputBehaviorPlayerPrefsKey(player, id);
			if (!PlayerPrefs.HasKey(inputBehaviorPlayerPrefsKey))
			{
				return string.Empty;
			}
			return PlayerPrefs.GetString(inputBehaviorPlayerPrefsKey);
		}

		
		private void AddDefaultMappingsForNewActions(Player player, List<UserDataStore_PlayerPrefs.SavedControllerMapData> savedData, ControllerType controllerType, int controllerId)
		{
			if (player == null || savedData == null)
			{
				return;
			}
			List<int> allActionIds = this.GetAllActionIds();
			for (int i = 0; i < savedData.Count; i++)
			{
				UserDataStore_PlayerPrefs.SavedControllerMapData savedControllerMapData = savedData[i];
				if (savedControllerMapData != null)
				{
					if (savedControllerMapData.knownActionIds != null && savedControllerMapData.knownActionIds.Count != 0)
					{
						ControllerMap controllerMap = ControllerMap.CreateFromXml(controllerType, savedData[i].xml);
						if (controllerMap != null)
						{
							ControllerMap map = player.controllers.maps.GetMap(controllerType, controllerId, controllerMap.categoryId, controllerMap.layoutId);
							if (map != null)
							{
								ControllerMap controllerMapInstance = ReInput.mapping.GetControllerMapInstance(ReInput.controllers.GetController(controllerType, controllerId), controllerMap.categoryId, controllerMap.layoutId);
								if (controllerMapInstance != null)
								{
									List<int> list = new List<int>();
									foreach (int item in allActionIds)
									{
										if (!savedControllerMapData.knownActionIds.Contains(item))
										{
											list.Add(item);
										}
									}
									if (list.Count != 0)
									{
										foreach (ActionElementMap actionElementMap in controllerMapInstance.AllMaps)
										{
											if (list.Contains(actionElementMap.actionId))
											{
												if (!map.DoesElementAssignmentConflict(actionElementMap))
												{
													ElementAssignment elementAssignment = new ElementAssignment(controllerType, actionElementMap.elementType, actionElementMap.elementIdentifierId, actionElementMap.axisRange, actionElementMap.keyCode, actionElementMap.modifierKeyFlags, actionElementMap.actionId, actionElementMap.axisContribution, actionElementMap.invert);
													map.CreateElementMap(elementAssignment);
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		
		private List<int> GetAllActionIds()
		{
			List<int> list = new List<int>();
			IList<InputAction> actions = ReInput.mapping.Actions;
			for (int i = 0; i < actions.Count; i++)
			{
				list.Add(actions[i].id);
			}
			return list;
		}

		
		private string GetAllActionIdsString()
		{
			string text = string.Empty;
			List<int> allActionIds = this.GetAllActionIds();
			for (int i = 0; i < allActionIds.Count; i++)
			{
				if (i > 0)
				{
					text += ",";
				}
				text += allActionIds[i];
			}
			return text;
		}

		
		private Joystick FindJoystickPrecise(UserDataStore_PlayerPrefs.ControllerAssignmentSaveInfo.JoystickInfo joystickInfo)
		{
			if (joystickInfo == null)
			{
				return null;
			}
			if (joystickInfo.instanceGuid == Guid.Empty)
			{
				return null;
			}
			IList<Joystick> joysticks = ReInput.controllers.Joysticks;
			for (int i = 0; i < joysticks.Count; i++)
			{
				if (joysticks[i].deviceInstanceGuid == joystickInfo.instanceGuid)
				{
					return joysticks[i];
				}
			}
			return null;
		}

		
		private bool TryFindJoysticksImprecise(UserDataStore_PlayerPrefs.ControllerAssignmentSaveInfo.JoystickInfo joystickInfo, out List<Joystick> matches)
		{
			matches = null;
			if (joystickInfo == null)
			{
				return false;
			}
			if (string.IsNullOrEmpty(joystickInfo.hardwareIdentifier))
			{
				return false;
			}
			IList<Joystick> joysticks = ReInput.controllers.Joysticks;
			for (int i = 0; i < joysticks.Count; i++)
			{
				if (string.Equals(joysticks[i].hardwareIdentifier, joystickInfo.hardwareIdentifier, StringComparison.OrdinalIgnoreCase))
				{
					if (matches == null)
					{
						matches = new List<Joystick>();
					}
					matches.Add(joysticks[i]);
				}
			}
			return matches != null;
		}

		
		private const string thisScriptName = "UserDataStore_PlayerPrefs";

		
		private const string editorLoadedMessage = "\nIf unexpected input issues occur, the loaded XML data may be outdated or invalid. Clear PlayerPrefs using the inspector option on the UserDataStore_PlayerPrefs component.";

		
		private const string playerPrefsKeySuffix_controllerAssignments = "ControllerAssignments";

		
		[Tooltip("Should this script be used? If disabled, nothing will be saved or loaded.")]
		[SerializeField]
		private bool isEnabled = true;

		
		[Tooltip("Should saved data be loaded on start?")]
		[SerializeField]
		private bool loadDataOnStart = true;

		
		[Tooltip("Should Player Joystick assignments be saved and loaded? This is not totally reliable for all Joysticks on all platforms. Some platforms/input sources do not provide enough information to reliably save assignments from session to session and reboot to reboot.")]
		[SerializeField]
		private bool loadJoystickAssignments = true;

		
		[Tooltip("Should Player Keyboard assignments be saved and loaded?")]
		[SerializeField]
		private bool loadKeyboardAssignments = true;

		
		[Tooltip("Should Player Mouse assignments be saved and loaded?")]
		[SerializeField]
		private bool loadMouseAssignments = true;

		
		[Tooltip("The PlayerPrefs key prefix. Change this to change how keys are stored in PlayerPrefs. Changing this will make saved data already stored with the old key no longer accessible.")]
		[SerializeField]
		private string playerPrefsKeyPrefix = "RewiredSaveData";

		
		private bool allowImpreciseJoystickAssignmentMatching = true;

		
		private bool deferredJoystickAssignmentLoadPending;

		
		private bool wasJoystickEverDetected;

		
		private class SavedControllerMapData
		{
			
			public SavedControllerMapData(string xml, List<int> knownActionIds)
			{
				this.xml = xml;
				this.knownActionIds = knownActionIds;
			}

			
			public static List<string> GetXmlStringList(List<UserDataStore_PlayerPrefs.SavedControllerMapData> data)
			{
				List<string> list = new List<string>();
				if (data == null)
				{
					return list;
				}
				for (int i = 0; i < data.Count; i++)
				{
					if (data[i] != null)
					{
						if (!string.IsNullOrEmpty(data[i].xml))
						{
							list.Add(data[i].xml);
						}
					}
				}
				return list;
			}

			
			public string xml;

			
			public List<int> knownActionIds;
		}

		
		private class ControllerAssignmentSaveInfo
		{
			
			public ControllerAssignmentSaveInfo()
			{
			}

			
			public ControllerAssignmentSaveInfo(int playerCount)
			{
				this.players = new UserDataStore_PlayerPrefs.ControllerAssignmentSaveInfo.PlayerInfo[playerCount];
				for (int i = 0; i < playerCount; i++)
				{
					this.players[i] = new UserDataStore_PlayerPrefs.ControllerAssignmentSaveInfo.PlayerInfo();
				}
			}

			
			
			public int playerCount
			{
				get
				{
					return (this.players == null) ? 0 : this.players.Length;
				}
			}

			
			public int IndexOfPlayer(int playerId)
			{
				for (int i = 0; i < this.playerCount; i++)
				{
					if (this.players[i] != null)
					{
						if (this.players[i].id == playerId)
						{
							return i;
						}
					}
				}
				return -1;
			}

			
			public bool ContainsPlayer(int playerId)
			{
				return this.IndexOfPlayer(playerId) >= 0;
			}

			
			public UserDataStore_PlayerPrefs.ControllerAssignmentSaveInfo.PlayerInfo[] players;

			
			public class PlayerInfo
			{
				
				
				public int joystickCount
				{
					get
					{
						return (this.joysticks == null) ? 0 : this.joysticks.Length;
					}
				}

				
				public int IndexOfJoystick(int joystickId)
				{
					for (int i = 0; i < this.joystickCount; i++)
					{
						if (this.joysticks[i] != null)
						{
							if (this.joysticks[i].id == joystickId)
							{
								return i;
							}
						}
					}
					return -1;
				}

				
				public bool ContainsJoystick(int joystickId)
				{
					return this.IndexOfJoystick(joystickId) >= 0;
				}

				
				public int id;

				
				public bool hasKeyboard;

				
				public bool hasMouse;

				
				public UserDataStore_PlayerPrefs.ControllerAssignmentSaveInfo.JoystickInfo[] joysticks;
			}

			
			public class JoystickInfo
			{
				
				public Guid instanceGuid;

				
				public string hardwareIdentifier;

				
				public int id;
			}
		}

		
		private class JoystickAssignmentHistoryInfo
		{
			
			public JoystickAssignmentHistoryInfo(Joystick joystick, int oldJoystickId)
			{
				if (joystick == null)
				{
					throw new ArgumentNullException("joystick");
				}
				this.joystick = joystick;
				this.oldJoystickId = oldJoystickId;
			}

			
			public readonly Joystick joystick;

			
			public readonly int oldJoystickId;
		}
	}
}
