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
		
		private void Start()
		{
			InputMappingIcons.TextIconBacking = this._textIconBacking;
			InputMappingIcons.TexturesByName = this._textures.ToDictionary((Texture2D t) => t.name);
			InputMappingIcons.Version = 1;
			InputMappingIcons.RefreshMappings();
		}

		
		private void Update()
		{
			if (TheForest.Utils.Input.IsGamePad)
			{
				Controller lastActiveController = TheForest.Utils.Input.player.controllers.GetLastActiveController();
				if (lastActiveController != InputMappingIcons.LastController)
				{
					InputMappingIcons.LastController = lastActiveController;
					InputMappingIcons.RefreshMappings();
					InputMappingIcons.Version++;
				}
			}
			if (this._usingGamePadVersion != TheForest.Utils.Input.IsGamePad)
			{
				this._usingGamePadVersion = TheForest.Utils.Input.IsGamePad;
				InputMappingIcons.Version++;
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
				if (InputMappingIcons.<>f__switch$map16 == null)
				{
					InputMappingIcons.<>f__switch$map16 = new Dictionary<string, int>(4)
					{
						{
							"Right_Mouse_Button",
							0
						},
						{
							"Left_Mouse_Button",
							0
						},
						{
							"Mouse_Horizontal",
							0
						},
						{
							"Mouse_Vertical",
							0
						}
					};
				}
				int num;
				if (InputMappingIcons.<>f__switch$map16.TryGetValue(text, out num))
				{
					if (num == 0)
					{
						return false;
					}
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
				if (InputMappingIcons.<>f__switch$map17 == null)
				{
					InputMappingIcons.<>f__switch$map17 = new Dictionary<string, int>(4)
					{
						{
							"Right_Mouse_Button",
							0
						},
						{
							"Left_Mouse_Button",
							0
						},
						{
							"Mouse_Horizontal",
							0
						},
						{
							"Mouse_Vertical",
							0
						}
					};
				}
				int num;
				if (InputMappingIcons.<>f__switch$map17.TryGetValue(text, out num))
				{
					if (num == 0)
					{
						return InputMappingIcons.TexturesByName[InputMappingIcons.KeyboardMappings[(int)action]];
					}
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
			return InputMappingIcons.KeyboardMappings[(int)action];
		}

		
		public static void RefreshMappings()
		{
			Debug.Log("Refreshing Input Mapping Icons");
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
								if (InputMappingIcons.<>f__switch$map18 == null)
								{
									InputMappingIcons.<>f__switch$map18 = new Dictionary<string, int>(2)
									{
										{
											"Left Control",
											0
										},
										{
											"Right Control",
											1
										}
									};
								}
								int num;
								if (InputMappingIcons.<>f__switch$map18.TryGetValue(elementIdentifierName, out num))
								{
									if (num == 0)
									{
										text2 = "LCtrl";
										goto IL_22B;
									}
									if (num == 1)
									{
										text2 = "RCtrl";
										goto IL_22B;
									}
								}
							}
							text2 = actionElementMap2.elementIdentifierName;
							IL_22B:
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
									string elementIdentifierName = actionElementMap3.elementIdentifierName;
									string text3;
									switch (elementIdentifierName)
									{
									case "View":
										text3 = "Back";
										goto IL_4B4;
									case "Right Stick X":
									case "Right Stick Y":
										text3 = "Right_Stick_Button";
										goto IL_4B4;
									case "Left Stick X":
									case "Left Stick Y":
										text3 = "Left_Stick_Button";
										goto IL_4B4;
									}
									text3 = actionElementMap3.elementIdentifierName.Replace(" X", string.Empty).Replace(" Y", string.Empty).TrimEnd(new char[]
									{
										' ',
										'+',
										'-'
									}).Replace(' ', '_');
									IL_4B4:
									InputAction action3 = ReInput.mapping.GetAction(actionElementMap3.actionId);
									if (flag)
									{
										text3 = "PS4_" + text3;
										if (action3.type == InputActionType.Axis)
										{
											InputMappingIcons.GamepadMappings[(int)Enum.Parse(typeof(InputMappingIcons.Actions), ((actionElementMap3.axisContribution != Pole.Positive) ? action3.negativeDescriptiveName : action3.positiveDescriptiveName).Replace(' ', '_'))] = text3;
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
			
			ScrollX
		}
	}
}
