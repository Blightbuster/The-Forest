using System;
using System.Collections.Generic;
using Rewired;
using Rewired.ControllerExtensions;
using TheForest.Items.Inventory;
using TheForest.UI;
using UnityEngine;

namespace TheForest.Utils
{
	public class InputMapping : MonoBehaviour
	{
		private void Awake()
		{
			InputMapping.instance = this;
			ReInput.ControllerConnectedEvent += this.OnControllerConnectedEvent;
			InputMapping.InitControllers();
		}

		private void OnDestroy()
		{
			ReInput.ControllerConnectedEvent -= this.OnControllerConnectedEvent;
		}

		public static void InitControllers()
		{
			InputMapping.holdLoadingMaps = true;
			foreach (Rewired.Joystick joystick in ReInput.controllers.Joysticks)
			{
				InputMapping.instance.OnControllerConnectedEvent(new ControllerStatusChangedEventArgs(joystick.name, joystick.id, joystick.type));
			}
			InputMapping.holdLoadingMaps = false;
			if (LocalPlayer.Inventory && LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World)
			{
				InputMapping.instance.Invoke("LoadAllMaps", 0.5f);
			}
			else
			{
				InputMapping.instance.LoadAllMaps();
			}
		}

		private void OnControllerConnectedEvent(ControllerStatusChangedEventArgs obj)
		{
			bool flag = false;
			if (LocalPlayer.Create)
			{
				LocalPlayer.Create.BeginCoolDown();
			}
			Debug.Log("OnControllerConnectedEvent");
			IList<Player> allPlayers = ReInput.players.AllPlayers;
			for (int i = 1; i < allPlayers.Count; i++)
			{
				Player player = allPlayers[i];
				bool flag2 = player.controllers.ContainsController(obj.controllerType, obj.controllerId);
				bool flag3 = InputDeviceManager.UseDevice(ReInput.controllers.GetController(obj.controllerType, obj.controllerId).hardwareIdentifier);
				if (!flag2 && flag3)
				{
					flag = true;
					Debug.Log(string.Concat(new object[]
					{
						"Adding controller to player ",
						i,
						" (",
						player.name,
						")"
					}));
					player.controllers.AddController(obj.controllerType, obj.controllerId, false);
				}
				else if (flag2 && !flag3)
				{
					flag = true;
					Debug.Log(string.Concat(new object[]
					{
						"Removing controller to player ",
						i,
						" (",
						player.name,
						")"
					}));
					player.controllers.RemoveController(obj.controllerType, obj.controllerId);
				}
			}
			if (flag && !InputMapping.holdLoadingMaps)
			{
				if (LocalPlayer.Inventory && LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World)
				{
					base.Invoke("LoadAllMaps", 0.5f);
				}
				else
				{
					this.LoadAllMaps();
				}
			}
		}

		public void ClearAllMaps()
		{
			IList<Player> allPlayers = ReInput.players.AllPlayers;
			for (int i = 0; i < allPlayers.Count; i++)
			{
				Player player = allPlayers[i];
				IList<InputBehavior> inputBehaviors = ReInput.mapping.GetInputBehaviors(player.id);
				for (int j = 0; j < inputBehaviors.Count; j++)
				{
					this.ClearInputBehaviorXml(player, inputBehaviors[j].id);
				}
				this.ClearAllControllerMapsXml(player, true, ControllerType.Keyboard, ReInput.controllers.Keyboard);
				this.ClearAllControllerMapsXml(player, true, ControllerType.Mouse, ReInput.controllers.Mouse);
				foreach (Rewired.Joystick controller in player.controllers.Joysticks)
				{
					this.ClearAllControllerMapsXml(player, true, ControllerType.Joystick, controller);
				}
				player.controllers.maps.ClearMaps(ControllerType.Keyboard, true);
				player.controllers.maps.ClearMaps(ControllerType.Joystick, true);
				player.controllers.maps.ClearMaps(ControllerType.Mouse, true);
			}
			foreach (Rewired.Joystick joystick in ReInput.controllers.Joysticks)
			{
				this.ClearJoystickCalibrationMapXml(joystick);
			}
		}

		public void LoadAllMaps()
		{
			Debug.Log("Reloading Input Mapping");
			if (!ReInput.isReady)
			{
				return;
			}
			if (ReInput.players == null || ReInput.players.AllPlayers == null)
			{
				return;
			}
			IList<Player> allPlayers = ReInput.players.AllPlayers;
			for (int i = 0; i < allPlayers.Count; i++)
			{
				Player player = allPlayers[i];
				IList<InputBehavior> inputBehaviors = ReInput.mapping.GetInputBehaviors(player.id);
				for (int j = 0; j < inputBehaviors.Count; j++)
				{
					string inputBehaviorXml = this.GetInputBehaviorXml(player, inputBehaviors[j].id);
					if (inputBehaviorXml != null && !(inputBehaviorXml == string.Empty))
					{
						inputBehaviors[j].ImportXmlString(inputBehaviorXml);
					}
				}
				List<string> allControllerMapsXml = this.GetAllControllerMapsXml(player, true, ControllerType.Keyboard, ReInput.controllers.Keyboard);
				List<string> allControllerMapsXml2 = this.GetAllControllerMapsXml(player, true, ControllerType.Mouse, ReInput.controllers.Mouse);
				bool flag = false;
				List<List<string>> list = new List<List<string>>();
				Debug.Log("Joystick count= " + player.controllers.Joysticks.Count);
				foreach (Rewired.Joystick controller in player.controllers.Joysticks)
				{
					List<string> allControllerMapsXml3 = this.GetAllControllerMapsXml(player, true, ControllerType.Joystick, controller);
					list.Add(allControllerMapsXml3);
					if (allControllerMapsXml3.Count > 0)
					{
						flag = true;
					}
				}
				if (allControllerMapsXml.Count > 0)
				{
					if (!allControllerMapsXml[0].Contains("<actionId>48</actionId>"))
					{
						allControllerMapsXml[0] = allControllerMapsXml[0].Replace("</buttonMaps>", "<ActionElementMap><actionCategoryId>0</actionCategoryId><actionId>48</actionId><elementType>1</elementType><elementIdentifierId>-1</elementIdentifierId><keyboardKeyCode>49</keyboardKeyCode></ActionElementMap></buttonMaps>");
					}
					if (!allControllerMapsXml[0].Contains("<actionId>49</actionId>"))
					{
						allControllerMapsXml[0] = allControllerMapsXml[0].Replace("</buttonMaps>", "<ActionElementMap><actionCategoryId>0</actionCategoryId><actionId>49</actionId><elementType>1</elementType><elementIdentifierId>-1</elementIdentifierId><keyboardKeyCode>50</keyboardKeyCode></ActionElementMap></buttonMaps>");
					}
					if (!allControllerMapsXml[0].Contains("<actionId>50</actionId>"))
					{
						allControllerMapsXml[0] = allControllerMapsXml[0].Replace("</buttonMaps>", "<ActionElementMap><actionCategoryId>0</actionCategoryId><actionId>50</actionId><elementType>1</elementType><elementIdentifierId>-1</elementIdentifierId><keyboardKeyCode>51</keyboardKeyCode></ActionElementMap></buttonMaps>");
					}
					if (!allControllerMapsXml[0].Contains("<actionId>51</actionId>"))
					{
						allControllerMapsXml[0] = allControllerMapsXml[0].Replace("</buttonMaps>", "<ActionElementMap><actionCategoryId>0</actionCategoryId><actionId>51</actionId><elementType>1</elementType><elementIdentifierId>-1</elementIdentifierId><keyboardKeyCode>52</keyboardKeyCode></ActionElementMap></buttonMaps>");
					}
				}
				if (allControllerMapsXml.Count > 0)
				{
					InputMapCategory mapCategory = ReInput.mapping.GetMapCategory(0);
					Dictionary<int[], List<ActionElementMap>> elementSourceActionLinks;
					Dictionary<int, ActionElementMap> storedElementMaps;
					InputMapping.CaptureExistingActionElementRelationships(mapCategory, player, out elementSourceActionLinks, out storedElementMaps);
					player.controllers.maps.ClearMaps(ControllerType.Keyboard, true);
					player.controllers.maps.AddMapsFromXml(ControllerType.Keyboard, 0, allControllerMapsXml);
					InputMapping.UpdateActionElementRelationships(elementSourceActionLinks, storedElementMaps, mapCategory, player);
				}
				if (flag)
				{
					player.controllers.maps.ClearMaps(ControllerType.Joystick, true);
				}
				int num = 0;
				foreach (Rewired.Joystick joystick in player.controllers.Joysticks)
				{
					bool flag2 = joystick.name.Contains("DualShock") || joystick.GetExtension<DualShock4Extension>() != null;
					if (list[num].Count > 0)
					{
						if (flag2)
						{
							if (!list[num][0].Contains("<actionId>48</actionId>"))
							{
								list[num][0] = list[num][0].Replace("</buttonMaps>", "<ActionElementMap><actionCategoryId>0</actionCategoryId><actionId>48</actionId><elementType>1</elementType><elementIdentifierId>21</elementIdentifierId><axisRange>1</axisRange><axisContribution>0</axisContribution><keyboardKeyCode>0</keyboardKeyCode></ActionElementMap></buttonMaps>");
							}
							if (!list[num][0].Contains("<actionId>49</actionId>"))
							{
								list[num][0] = list[num][0].Replace("</buttonMaps>", "<ActionElementMap><actionCategoryId>0</actionCategoryId><actionId>49</actionId><elementType>1</elementType><elementIdentifierId>20</elementIdentifierId><axisRange>1</axisRange><axisContribution>0</axisContribution><keyboardKeyCode>0</keyboardKeyCode></ActionElementMap></buttonMaps>");
							}
							if (!list[num][0].Contains("<actionId>50</actionId>"))
							{
								list[num][0] = list[num][0].Replace("</buttonMaps>", "<ActionElementMap><actionCategoryId>0</actionCategoryId><actionId>50</actionId><elementType>1</elementType><elementIdentifierId>19</elementIdentifierId><axisRange>1</axisRange><axisContribution>0</axisContribution><keyboardKeyCode>0</keyboardKeyCode></ActionElementMap></buttonMaps>");
							}
							if (!list[num][0].Contains("<actionId>51</actionId>"))
							{
								list[num][0] = list[num][0].Replace("</buttonMaps>", "<ActionElementMap><actionCategoryId>0</actionCategoryId><actionId>51</actionId><elementType>1</elementType><elementIdentifierId>18</elementIdentifierId><axisRange>1</axisRange><axisContribution>0</axisContribution><keyboardKeyCode>0</keyboardKeyCode></ActionElementMap></buttonMaps>");
							}
							if (!list[num][0].Contains("<actionId>53</actionId>"))
							{
								list[num][0] = list[num][0].Replace("</buttonMaps>", "<ActionElementMap><actionCategoryId>2</actionCategoryId><actionId>53</actionId><elementType>1</elementType><elementIdentifierId>4</elementIdentifierId><axisRange>0</axisRange><axisContribution>0</axisContribution><keyboardKeyCode>0</keyboardKeyCode></ActionElementMap></buttonMaps>");
							}
							if (!list[num][0].Contains("<actionId>54</actionId>"))
							{
								list[num][0] = list[num][0].Replace("</buttonMaps>", "<ActionElementMap><actionCategoryId>2</actionCategoryId><actionId>54</actionId><elementType>1</elementType><elementIdentifierId>6</elementIdentifierId><axisRange>1</axisRange><axisContribution>0</axisContribution><keyboardKeyCode>0</keyboardKeyCode></ActionElementMap></buttonMaps>");
							}
						}
						else
						{
							if (!list[num][0].Contains("<actionId>48</actionId>"))
							{
								list[num][0] = list[num][0].Replace("</buttonMaps>", "<ActionElementMap><actionCategoryId>0</actionCategoryId><actionId>48</actionId><elementType>1</elementType><elementIdentifierId>19</elementIdentifierId><axisRange>1</axisRange><axisContribution>0</axisContribution><keyboardKeyCode>0</keyboardKeyCode></ActionElementMap></buttonMaps>");
							}
							if (!list[num][0].Contains("<actionId>49</actionId>"))
							{
								list[num][0] = list[num][0].Replace("</buttonMaps>", "<ActionElementMap><actionCategoryId>0</actionCategoryId><actionId>49</actionId><elementType>1</elementType><elementIdentifierId>18</elementIdentifierId><axisRange>1</axisRange><axisContribution>0</axisContribution><keyboardKeyCode>0</keyboardKeyCode></ActionElementMap></buttonMaps>");
							}
							if (!list[num][0].Contains("<actionId>50</actionId>"))
							{
								list[num][0] = list[num][0].Replace("</buttonMaps>", "<ActionElementMap><actionCategoryId>0</actionCategoryId><actionId>50</actionId><elementType>1</elementType><elementIdentifierId>17</elementIdentifierId><axisRange>1</axisRange><axisContribution>0</axisContribution><keyboardKeyCode>0</keyboardKeyCode></ActionElementMap></buttonMaps>");
							}
							if (!list[num][0].Contains("<actionId>51</actionId>"))
							{
								list[num][0] = list[num][0].Replace("</buttonMaps>", "<ActionElementMap><actionCategoryId>0</actionCategoryId><actionId>51</actionId><elementType>1</elementType><elementIdentifierId>16</elementIdentifierId><axisRange>1</axisRange><axisContribution>0</axisContribution><keyboardKeyCode>0</keyboardKeyCode></ActionElementMap></buttonMaps>");
							}
							if (!list[num][0].Contains("<actionId>53</actionId>"))
							{
								list[num][0] = list[num][0].Replace("</buttonMaps>", "<ActionElementMap><actionCategoryId>2</actionCategoryId><actionId>53</actionId><elementType>1</elementType><elementIdentifierId>4</elementIdentifierId><axisRange>0</axisRange><axisContribution>0</axisContribution><keyboardKeyCode>0</keyboardKeyCode></ActionElementMap></buttonMaps>");
							}
							if (!list[num][0].Contains("<actionId>54</actionId>"))
							{
								list[num][0] = list[num][0].Replace("</buttonMaps>", "<ActionElementMap><actionCategoryId>2</actionCategoryId><actionId>54</actionId><elementType>1</elementType><elementIdentifierId>6</elementIdentifierId><axisRange>1</axisRange><axisContribution>0</axisContribution><keyboardKeyCode>0</keyboardKeyCode></ActionElementMap></buttonMaps>");
							}
						}
					}
					player.controllers.maps.AddMapsFromXml(ControllerType.Joystick, joystick.id, list[num]);
					num++;
				}
				if (allControllerMapsXml2.Count > 0)
				{
					player.controllers.maps.ClearMaps(ControllerType.Mouse, true);
				}
				player.controllers.maps.AddMapsFromXml(ControllerType.Mouse, 0, allControllerMapsXml2);
				if (ForestVR.Enabled)
				{
					this.ProcessVRInputMappings(player);
				}
			}
			foreach (Rewired.Joystick joystick2 in ReInput.controllers.Joysticks)
			{
				joystick2.ImportCalibrationMapFromXmlString(this.GetJoystickCalibrationMapXml(joystick2));
			}
			Input.ForceRefreshState();
			InputMappingIcons.RefreshMappings();
		}

		private void ProcessVRInputMappings(Player player)
		{
			VRControllerDisplayManager.VRControllerType activeControllerType = VRControllerDisplayManager.GetActiveControllerType();
			if (activeControllerType != VRControllerDisplayManager.VRControllerType.Vive)
			{
				if (activeControllerType == VRControllerDisplayManager.VRControllerType.OculusTouch)
				{
					this.ProcessVROculusInputMappings(player);
				}
			}
			else
			{
				this.ProcessVRViveInputMappings(player);
			}
		}

		private void ProcessVRViveInputMappings(Player player)
		{
			Player.ControllerHelper controllers = player.controllers;
			IEnumerable<ControllerMap> enumerable = controllers.maps.GetAllMaps(ControllerType.Joystick);
			if (enumerable == null)
			{
				enumerable = new List<ControllerMap>();
			}
			foreach (ControllerMap controllerMap in enumerable)
			{
				List<ElementAssignment> list = new List<ElementAssignment>();
				IList<ActionElementMap> allMaps = controllerMap.AllMaps;
				foreach (ActionElementMap actionElementMap in allMaps)
				{
					if (actionElementMap.elementIdentifierId == 2 && actionElementMap.elementIdentifierName.Equals("Index Trigger"))
					{
						ElementAssignment item = new ElementAssignment(ControllerType.Joystick, ControllerElementType.Button, 7, AxisRange.Full, KeyCode.None, ModifierKeyFlags.None, actionElementMap.actionId, Pole.Positive, false, actionElementMap.id);
						list.Add(item);
					}
				}
				foreach (ElementAssignment elementAssignment in list)
				{
					ActionElementMap actionElementMap2;
					bool flag = controllerMap.ReplaceElementMap(elementAssignment, out actionElementMap2);
				}
			}
		}

		private void ProcessVROculusInputMappings(Player player)
		{
		}

		private static void UpdateActionElementRelationships(Dictionary<int[], List<ActionElementMap>> elementSourceActionLinks, Dictionary<int, ActionElementMap> storedElementMaps, InputMapCategory sourceCategory, Player player)
		{
			foreach (KeyValuePair<int[], List<ActionElementMap>> keyValuePair in elementSourceActionLinks)
			{
				if (keyValuePair.Value.SafeCount<ActionElementMap>() != 0)
				{
					int key = keyValuePair.Key[2];
					ActionElementMap oldElementMap = storedElementMaps[key];
					KeyboardMap firstMapInCategory = player.controllers.maps.GetFirstMapInCategory<KeyboardMap>(0, sourceCategory.id);
					if (firstMapInCategory == null)
					{
						Debug.LogError("newControllerMap is null!");
					}
					else
					{
						ActionElementMap firstElementMapMatch = firstMapInCategory.GetFirstElementMapMatch((ActionElementMap searchMap) => searchMap.actionId == oldElementMap.actionId);
						if (firstElementMapMatch == null)
						{
							Debug.LogError("sourceMap is null!");
						}
						else
						{
							foreach (ActionElementMap actionElementMap in keyValuePair.Value)
							{
								ControllerMap controllerMap = actionElementMap.controllerMap;
								ElementAssignment elementAssignment = new ElementAssignment(ControllerType.Keyboard, firstElementMapMatch.elementType, firstElementMapMatch.elementIdentifierId, firstElementMapMatch.axisRange, firstElementMapMatch.keyCode, firstElementMapMatch.modifierKeyFlags, firstElementMapMatch.actionId, firstElementMapMatch.axisContribution, firstElementMapMatch.invert, actionElementMap.id);
								bool flag = controllerMap.DeleteElementMap(actionElementMap.id);
								ActionElementMap actionElementMap2;
								if (!flag || !controllerMap.CreateElementMap(elementAssignment, out actionElementMap2))
								{
									Debug.Log("Failed to remap secondary actions");
								}
							}
						}
					}
				}
			}
		}

		private static void CaptureExistingActionElementRelationships(InputMapCategory sourceCategory, Player player, out Dictionary<int[], List<ActionElementMap>> elementSourceActionLinks, out Dictionary<int, ActionElementMap> storedElementMaps)
		{
			Player.ControllerHelper controllers = player.controllers;
			elementSourceActionLinks = new Dictionary<int[], List<ActionElementMap>>();
			storedElementMaps = new Dictionary<int, ActionElementMap>();
			IEnumerable<ControllerMap> enumerable = controllers.maps.GetAllMaps(ControllerType.Keyboard);
			if (enumerable == null)
			{
				enumerable = new List<ControllerMap>();
			}
			foreach (ControllerMap controllerMap in enumerable)
			{
				if (controllerMap.categoryId == sourceCategory.id)
				{
					IList<ActionElementMap> allMaps = controllerMap.AllMaps;
					foreach (ActionElementMap actionElementMap in allMaps)
					{
						elementSourceActionLinks.Add(new int[]
						{
							actionElementMap.elementIdentifierId,
							actionElementMap.actionId,
							actionElementMap.id
						}, new List<ActionElementMap>());
						if (!storedElementMaps.ContainsKey(actionElementMap.id))
						{
							storedElementMaps.Add(actionElementMap.id, actionElementMap);
						}
					}
				}
			}
			foreach (ControllerMap controllerMap2 in enumerable)
			{
				bool flag = controllerMap2.categoryId == sourceCategory.id;
				if (!flag)
				{
					IList<ActionElementMap> allMaps2 = controllerMap2.AllMaps;
					foreach (ActionElementMap actionElementMap2 in allMaps2)
					{
						foreach (int[] array in elementSourceActionLinks.Keys)
						{
							int num = array[0];
							int num2 = array[1];
							bool flag2 = num2 == actionElementMap2.actionId;
							bool flag3 = num == actionElementMap2.elementIdentifierId;
							if (flag3 && flag2)
							{
								elementSourceActionLinks[array].Add(actionElementMap2);
							}
						}
					}
				}
			}
		}

		public void SaveAllMaps(bool enableBeforeSave = true)
		{
			IList<Player> allPlayers = ReInput.players.AllPlayers;
			for (int i = 0; i < allPlayers.Count; i++)
			{
				Player player = allPlayers[i];
				PlayerSaveData saveData = player.GetSaveData(true);
				foreach (InputBehavior inputBehavior in saveData.inputBehaviors)
				{
					string inputBehaviorPlayerPrefsKey = this.GetInputBehaviorPlayerPrefsKey(player, inputBehavior);
					PlayerPrefs.SetString(inputBehaviorPlayerPrefsKey, inputBehavior.ToXmlString());
				}
				foreach (ControllerMapSaveData controllerMapSaveData in saveData.AllControllerMapSaveData)
				{
					bool enabled = controllerMapSaveData.map.enabled;
					if (!enabled && enableBeforeSave)
					{
						controllerMapSaveData.map.enabled = true;
					}
					string controllerMapPlayerPrefsKey = this.GetControllerMapPlayerPrefsKey(player, controllerMapSaveData);
					PlayerPrefs.SetString(controllerMapPlayerPrefsKey, controllerMapSaveData.map.ToXmlString());
					if (!enabled)
					{
						controllerMapSaveData.map.enabled = false;
					}
				}
			}
			foreach (Rewired.Joystick joystick in ReInput.controllers.Joysticks)
			{
				JoystickCalibrationMapSaveData calibrationMapSaveData = joystick.GetCalibrationMapSaveData();
				string joystickCalibrationMapPlayerPrefsKey = this.GetJoystickCalibrationMapPlayerPrefsKey(calibrationMapSaveData);
				PlayerPrefs.SetString(joystickCalibrationMapPlayerPrefsKey, calibrationMapSaveData.map.ToXmlString());
			}
			PlayerPrefs.Save();
			InputMappingIcons.RefreshMappings();
		}

		private string GetBasePlayerPrefsKey(Player player)
		{
			string str = "UserRemapping_v3";
			return str + "&playerName=" + player.name;
		}

		private string GetControllerMapPlayerPrefsKey(Player player, ControllerMapSaveData saveData)
		{
			string text = this.GetBasePlayerPrefsKey(player);
			text += "&dataType=ControllerMap";
			text = text + "&controllerMapType=" + saveData.mapTypeString;
			string text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"&categoryId=",
				saveData.map.categoryId,
				"&layoutId=",
				saveData.map.layoutId
			});
			text = text + "&hardwareIdentifier=" + saveData.controllerHardwareIdentifier;
			if (saveData.mapType == typeof(JoystickMap))
			{
				text = text + "&hardwareGuid=" + ((JoystickMapSaveData)saveData).joystickHardwareTypeGuid.ToString();
			}
			return text;
		}

		private string GetControllerMapXml(Player player, ControllerType controllerType, int categoryId, int layoutId, Controller controller)
		{
			string text = this.GetBasePlayerPrefsKey(player);
			text += "&dataType=ControllerMap";
			text = text + "&controllerMapType=" + controller.mapTypeString;
			string text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"&categoryId=",
				categoryId,
				"&layoutId=",
				layoutId
			});
			text = text + "&hardwareIdentifier=" + controller.hardwareIdentifier;
			if (controllerType == ControllerType.Joystick)
			{
				Rewired.Joystick joystick = (Rewired.Joystick)controller;
				text = text + "&hardwareGuid=" + joystick.hardwareTypeGuid.ToString();
			}
			if (!PlayerPrefs.HasKey(text))
			{
				return string.Empty;
			}
			return PlayerPrefs.GetString(text);
		}

		private void ClearControllerMapXml(Player player, ControllerType controllerType, int categoryId, int layoutId, Controller controller)
		{
			string text = this.GetBasePlayerPrefsKey(player);
			text += "&dataType=ControllerMap";
			text = text + "&controllerMapType=" + controller.mapTypeString;
			string text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"&categoryId=",
				categoryId,
				"&layoutId=",
				layoutId
			});
			text = text + "&hardwareIdentifier=" + controller.hardwareIdentifier;
			if (controllerType == ControllerType.Joystick)
			{
				Rewired.Joystick joystick = (Rewired.Joystick)controller;
				text = text + "&hardwareGuid=" + joystick.hardwareTypeGuid.ToString();
			}
			if (PlayerPrefs.HasKey(text))
			{
				PlayerPrefs.DeleteKey(text);
			}
		}

		private List<string> GetAllControllerMapsXml(Player player, bool userAssignableMapsOnly, ControllerType controllerType, Controller controller)
		{
			List<string> list = new List<string>();
			IList<InputMapCategory> mapCategories = ReInput.mapping.MapCategories;
			for (int i = 0; i < mapCategories.Count; i++)
			{
				InputMapCategory inputMapCategory = mapCategories[i];
				if (!userAssignableMapsOnly || inputMapCategory.userAssignable)
				{
					IList<InputLayout> list2 = ReInput.mapping.MapLayouts(controllerType);
					for (int j = 0; j < list2.Count; j++)
					{
						InputLayout inputLayout = list2[j];
						string controllerMapXml = this.GetControllerMapXml(player, controllerType, inputMapCategory.id, inputLayout.id, controller);
						if (!(controllerMapXml == string.Empty))
						{
							list.Add(controllerMapXml);
						}
					}
				}
			}
			return list;
		}

		private void ClearAllControllerMapsXml(Player player, bool userAssignableMapsOnly, ControllerType controllerType, Controller controller)
		{
			List<string> list = new List<string>();
			IList<InputMapCategory> mapCategories = ReInput.mapping.MapCategories;
			for (int i = 0; i < mapCategories.Count; i++)
			{
				InputMapCategory inputMapCategory = mapCategories[i];
				if (!userAssignableMapsOnly || inputMapCategory.userAssignable)
				{
					IList<InputLayout> list2 = ReInput.mapping.MapLayouts(controllerType);
					for (int j = 0; j < list2.Count; j++)
					{
						InputLayout inputLayout = list2[j];
						this.ClearControllerMapXml(player, controllerType, inputMapCategory.id, inputLayout.id, controller);
					}
				}
			}
		}

		private string GetJoystickCalibrationMapPlayerPrefsKey(JoystickCalibrationMapSaveData saveData)
		{
			string str = "UserRemapping_v3";
			str += "&dataType=CalibrationMap";
			str = str + "&controllerType=" + saveData.controllerType.ToString();
			str = str + "&hardwareIdentifier=" + saveData.hardwareIdentifier;
			return str + "&hardwareGuid=" + saveData.joystickHardwareTypeGuid.ToString();
		}

		private string GetJoystickCalibrationMapXml(Rewired.Joystick joystick)
		{
			string text = "UserRemapping_v3";
			text += "&dataType=CalibrationMap";
			text = text + "&controllerType=" + joystick.type.ToString();
			text = text + "&hardwareIdentifier=" + joystick.hardwareIdentifier;
			text = text + "&hardwareGuid=" + joystick.hardwareTypeGuid.ToString();
			if (!PlayerPrefs.HasKey(text))
			{
				return string.Empty;
			}
			return PlayerPrefs.GetString(text);
		}

		private void ClearJoystickCalibrationMapXml(Rewired.Joystick joystick)
		{
			string text = "UserRemapping_v3";
			text += "&dataType=CalibrationMap";
			text = text + "&controllerType=" + joystick.type.ToString();
			text = text + "&hardwareIdentifier=" + joystick.hardwareIdentifier;
			text = text + "&hardwareGuid=" + joystick.hardwareTypeGuid.ToString();
			if (PlayerPrefs.HasKey(text))
			{
				PlayerPrefs.DeleteKey(text);
			}
		}

		private string GetInputBehaviorPlayerPrefsKey(Player player, InputBehavior saveData)
		{
			string text = this.GetBasePlayerPrefsKey(player);
			text += "&dataType=InputBehavior";
			return text + "&id=" + saveData.id;
		}

		private string GetInputBehaviorXml(Player player, int id)
		{
			string text = this.GetBasePlayerPrefsKey(player);
			text += "&dataType=InputBehavior";
			text = text + "&id=" + id;
			if (!PlayerPrefs.HasKey(text))
			{
				return string.Empty;
			}
			return PlayerPrefs.GetString(text);
		}

		private void ClearInputBehaviorXml(Player player, int id)
		{
			string text = this.GetBasePlayerPrefsKey(player);
			text += "&dataType=InputBehavior";
			text = text + "&id=" + id;
			if (PlayerPrefs.HasKey(text))
			{
				PlayerPrefs.DeleteKey(text);
			}
		}

		private static InputMapping instance;

		private static bool holdLoadingMaps;

		private const string playerPrefsBaseKey = "UserRemapping_v3";

		private const int IndexTriggerElementId = 2;

		private const string IndexTriggerElementName = "Index Trigger";

		private const int IndexTriggerTouchElementId = 7;

		private const string IndexTriggerTouchElementName = "Index Trigger Touch";
	}
}
