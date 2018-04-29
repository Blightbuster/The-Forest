using System;
using System.Collections.Generic;
using Rewired;
using Rewired.ControllerExtensions;
using TheForest.Utils;
using UniLinq;
using UnityEngine;

namespace TheForest.UI
{
	
	public class InputMappingIcons : MonoBehaviour
	{
		
		private void Awake()
		{
			InputMappingIcons.TextIconBacking = this._textIconBacking;
			InputMappingIcons.TexturesByName = (from t in this._textures
			where t != null
			select t).ToDictionary((Texture2D t) => t.name);
			InputMappingIcons.Version = 1;
		}

		
		private void Start()
		{
			InputMappingIcons.RefreshMappings();
		}

		
		private void Update()
		{
			if (TheForest.Utils.Input.IsGamePad)
			{
				Controller lastActiveController = TheForest.Utils.Input.player.controllers.GetLastActiveController();
				if (lastActiveController != InputMappingIcons.LastController)
				{
					if (InputMappingIcons.LastController != null)
					{
						Debug.Log(string.Concat(new object[]
						{
							"===> Changing gamepad icons: currentController=",
							lastActiveController.name,
							", LastController=",
							InputMappingIcons.LastController
						}));
					}
					else
					{
						Debug.Log("===> Init gamepad icons: currentController=" + lastActiveController.name);
					}
					InputMappingIcons.LastController = lastActiveController;
					InputMappingIcons.RefreshMappings();
					InputMappingIcons.Version++;
				}
			}
			if (this._usingGamePadVersion != TheForest.Utils.Input.IsGamePad)
			{
				this._usingGamePadVersion = TheForest.Utils.Input.IsGamePad;
				InputMappingIcons.Version++;
				if (TheForest.Utils.Input.IsGamePad)
				{
					Debug.Log("===> Switching from KM to gamepad     Version=" + InputMappingIcons.Version);
				}
				else
				{
					Debug.Log("===> Switching from gamepad to KM     Version = " + InputMappingIcons.Version);
				}
			}
		}

		
		public static bool UsesText(InputMappingIcons.Actions action)
		{
			if (TheForest.Utils.Input.IsGamePad)
			{
				return false;
			}
			string text = InputMappingIcons.KeyboardMappings[(int)action];
			if (text != null)
			{
				if (text == "Right_Mouse_Button" || text == "Left_Mouse_Button" || text == "Mouse_Horizontal" || text == "Mouse_Vertical")
				{
					return false;
				}
			}
			return true;
		}

		
		public static Texture GetTextureFor(InputMappingIcons.Actions action)
		{
			if (TheForest.Utils.Input.IsGamePad)
			{
				return InputMappingIcons.TexturesByName[InputMappingIcons.GetMappingFor(action)];
			}
			string text = InputMappingIcons.KeyboardMappings[(int)action];
			if (text != null)
			{
				if (text == "Right_Mouse_Button" || text == "Left_Mouse_Button" || text == "Mouse_Horizontal" || text == "Mouse_Vertical")
				{
					return InputMappingIcons.TexturesByName[InputMappingIcons.KeyboardMappings[(int)action]];
				}
			}
			return InputMappingIcons.TextIconBacking;
		}

		
		public static string GetMappingFor(InputMappingIcons.Actions action)
		{
			if (TheForest.Utils.Input.IsGamePad)
			{
				return InputMappingIcons.GamepadMappings[(int)action];
			}
			string text = InputMappingIcons.KeyboardMappings[(int)action];
			if (text != null)
			{
				if (text == "Space")
				{
					return UiTranslationDatabase.TranslateKey("SPACE", "SPACE", true);
				}
			}
			return InputMappingIcons.KeyboardMappings[(int)action];
		}

		
		public static string GetBackingFor(InputMappingIcons.Actions action)
		{
			if (TheForest.Utils.Input.IsGamePad)
			{
				return InputMappingIcons.GamepadMappings[(int)action];
			}
			string text = InputMappingIcons.KeyboardMappings[(int)action];
			if (text != null)
			{
				if (text == "Space")
				{
					return "space_button";
				}
			}
			return InputMappingIcons.TextIconBacking.name;
		}

		
		public static void RefreshMappings()
		{
			if (SteamDSConfig.isDedicatedServer)
			{
				return;
			}
			foreach (ControllerMap controllerMap in ReInput.players.GetPlayer(0).controllers.maps.GetAllMaps(ControllerType.Mouse))
			{
				foreach (ActionElementMap actionElementMap in controllerMap.AllMaps)
				{
					if (!string.IsNullOrEmpty(actionElementMap.elementIdentifierName))
					{
						try
						{
							string text = actionElementMap.elementIdentifierName.Replace(' ', '_');
							InputAction action = ReInput.mapping.GetAction(actionElementMap.actionId);
							if (action.type == InputActionType.Axis)
							{
								InputMappingIcons.KeyboardMappings[(int)Enum.Parse(typeof(InputMappingIcons.Actions), ((actionElementMap.axisContribution != Pole.Positive) ? action.negativeDescriptiveName : action.positiveDescriptiveName).Replace(' ', '_'))] = text;
							}
							InputMappingIcons.KeyboardMappings[(int)Enum.Parse(typeof(InputMappingIcons.Actions), action.name.Replace(' ', '_'))] = text;
						}
						catch
						{
						}
					}
				}
			}
			foreach (ControllerMap controllerMap2 in ReInput.players.GetPlayer(0).controllers.maps.GetAllMaps(ControllerType.Keyboard))
			{
				foreach (ActionElementMap actionElementMap2 in controllerMap2.AllMaps)
				{
					if (!string.IsNullOrEmpty(actionElementMap2.elementIdentifierName))
					{
						try
						{
							string elementIdentifierName = actionElementMap2.elementIdentifierName;
							string text2;
							if (elementIdentifierName != null)
							{
								if (elementIdentifierName == "Left Control")
								{
									text2 = "LCtrl";
									goto IL_1FD;
								}
								if (elementIdentifierName == "Right Control")
								{
									text2 = "RCtrl";
									goto IL_1FD;
								}
							}
							text2 = actionElementMap2.elementIdentifierName;
							IL_1FD:
							InputAction action2 = ReInput.mapping.GetAction(actionElementMap2.actionId);
							if (action2.type == InputActionType.Axis)
							{
								InputMappingIcons.KeyboardMappings[(int)Enum.Parse(typeof(InputMappingIcons.Actions), ((actionElementMap2.axisContribution != Pole.Positive) ? action2.negativeDescriptiveName : action2.positiveDescriptiveName).Replace(' ', '_'))] = text2;
							}
							InputMappingIcons.KeyboardMappings[(int)Enum.Parse(typeof(InputMappingIcons.Actions), action2.name.Replace(' ', '_'))] = text2;
						}
						catch
						{
						}
					}
				}
			}
			if (InputMappingIcons.LastController != null)
			{
				bool flag = InputMappingIcons.LastController.name.Contains("DualShock") || InputMappingIcons.LastController.GetExtension<DualShock4Extension>() != null;
				foreach (ControllerMap controllerMap3 in ReInput.players.GetPlayer(0).controllers.maps.GetAllMaps(ControllerType.Joystick))
				{
					if (controllerMap3.controllerId == InputMappingIcons.LastController.id)
					{
						foreach (ActionElementMap actionElementMap3 in controllerMap3.AllMaps)
						{
							if (!string.IsNullOrEmpty(actionElementMap3.elementIdentifierName))
							{
								try
								{
									string elementIdentifierName2 = actionElementMap3.elementIdentifierName;
									string text3;
									if (elementIdentifierName2 != null)
									{
										if (elementIdentifierName2 == "View")
										{
											text3 = "Back";
											goto IL_45B;
										}
										if (elementIdentifierName2 == "Right Stick X" || elementIdentifierName2 == "Right Stick Y")
										{
											text3 = "Right_Stick_Button";
											goto IL_45B;
										}
										if (elementIdentifierName2 == "Left Stick X" || elementIdentifierName2 == "Left Stick Y")
										{
											text3 = "Left_Stick_Button";
											goto IL_45B;
										}
									}
									text3 = actionElementMap3.elementIdentifierName.Replace(" X", string.Empty).Replace(" Y", string.Empty).TrimEnd(new char[]
									{
										' ',
										'+',
										'-'
									}).Replace(' ', '_');
									IL_45B:
									InputAction action3 = ReInput.mapping.GetAction(actionElementMap3.actionId);
									if (flag)
									{
										text3 = "PS4_" + text3;
										if (action3.type == InputActionType.Axis)
										{
											if (!string.IsNullOrEmpty(action3.positiveDescriptiveName) || !string.IsNullOrEmpty(action3.negativeDescriptiveName))
											{
												InputMappingIcons.GamepadMappings[(int)Enum.Parse(typeof(InputMappingIcons.Actions), ((actionElementMap3.axisContribution != Pole.Positive) ? action3.negativeDescriptiveName : action3.positiveDescriptiveName).Replace(' ', '_'))] = text3;
											}
											else
											{
												InputMappingIcons.GamepadMappings[(int)Enum.Parse(typeof(InputMappingIcons.Actions), action3.name.Replace(' ', '_'))] = text3;
											}
										}
										InputMappingIcons.GamepadMappings[(int)Enum.Parse(typeof(InputMappingIcons.Actions), action3.name.Replace(' ', '_'))] = text3;
									}
									else
									{
										text3 = "360_" + text3;
										if (action3.type == InputActionType.Axis)
										{
											InputMappingIcons.GamepadMappings[(int)Enum.Parse(typeof(InputMappingIcons.Actions), ((actionElementMap3.axisContribution != Pole.Positive) ? action3.negativeDescriptiveName : action3.positiveDescriptiveName).Replace(' ', '_'))] = text3;
										}
										InputMappingIcons.GamepadMappings[(int)Enum.Parse(typeof(InputMappingIcons.Actions), action3.name.Replace(' ', '_'))] = text3;
									}
								}
								catch
								{
								}
							}
						}
					}
				}
			}
			InputMappingIcons.Version++;
		}

		
		public Texture2D[] _textures;

		
		public Texture2D _textIconBacking;

		
		private bool _usingGamePadVersion;

		
		public static int Version;

		
		public static Texture2D TextIconBacking;

		
		private static Dictionary<string, Texture2D> TexturesByName;

		
		private static string[] KeyboardMappings = new string[Enum.GetNames(typeof(InputMappingIcons.Actions)).Length];

		
		private static string[] GamepadMappings = new string[Enum.GetNames(typeof(InputMappingIcons.Actions)).Length];

		
		private static Controller LastController;

		
		public enum Actions
		{
			
			None = -1,
			
			Inventory,
			
			SurvivalBook,
			
			Horizontal,
			
			Right,
			
			Left,
			
			Vertical,
			
			Up,
			
			Down,
			
			Jump,
			
			Mouse_X,
			
			LookRight,
			
			LookLeft,
			
			Mouse_Y,
			
			LookUp,
			
			LookDown,
			
			Run,
			
			Crouch,
			
			Fire1,
			
			AltFire,
			
			Take,
			
			Drop,
			
			RestKey,
			
			Save,
			
			Lighter,
			
			Craft,
			
			Utility,
			
			WalkyTalky,
			
			Rotate,
			
			Batch,
			
			OpenChat,
			
			Submit,
			
			Esc,
			
			Build,
			
			Combine,
			
			Equip,
			
			PreviousChapter,
			
			NextChapter,
			
			PreviousPage,
			
			NextPage,
			
			Back,
			
			Map,
			
			ScrollY,
			
			ItemSlot1,
			
			ItemSlot2,
			
			ItemSlot3,
			
			ItemSlot4,
			
			ScrollX,
			
			Rebind,
			
			SetOption,
			
			RestoreDefaults,
			
			SaveBindings
		}
	}
}
