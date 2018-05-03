using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rewired;
using TheForest.UI;
using UnityEngine;

namespace TheForest.Utils
{
	
	public class InputMappingButton : MonoBehaviour
	{
		
		private static void GetAllActions()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (InputAction inputAction in ReInput.mapping.Actions)
			{
				stringBuilder.AppendLine(string.Concat(new object[]
				{
					inputAction.name,
					" ",
					inputAction.id,
					" ",
					inputAction.type,
					" ",
					inputAction.userAssignable
				}));
			}
			Debug.Log(stringBuilder);
		}

		
		private void UpdateGameObjectNames()
		{
		}

		
		public void OnEnable()
		{
			this.Refresh();
		}

		
		public void Refresh()
		{
			this.UpdateElementMap();
			this.UpdateAssignedInput();
			this.UpdateActionLabel();
			this.UpdateGameObjectNames();
		}

		
		private void UpdateElementMap()
		{
			this._foundAction = InputMappingButton.GetAction(this._actionName);
			this._foundMaps = InputMappingButton.GetElementMap(this._foundAction, this._negativeAxis);
			if (this._foundMaps.NullOrEmpty<ActionElementMap>() || this._foundMaps[0] == null)
			{
				this._controllerType = ControllerType.Keyboard;
			}
			else
			{
				this._controllerType = this._foundMaps[0].controllerMap.controllerType;
			}
		}

		
		private void UpdateActionLabel()
		{
			if (this._foundAction == null)
			{
				this._actionLabel.text = "NULL";
			}
			else
			{
				this._actionLabel.text = InputMappingButton.GetActionUIText(this._foundAction, this._negativeAxis).ToUpperInvariant();
			}
		}

		
		private void UpdateAssignedInput()
		{
			if (this._foundMaps.SafeCount<ActionElementMap>() == 0)
			{
				this._actionInput = null;
			}
			else
			{
				this._actionInput = this._foundMaps[0].elementIdentifierName.ToUpperInvariant();
			}
			this._inputLabel.text = this._actionInput;
		}

		
		private static string GetActionUIText(InputAction action, bool negativeAxis = false)
		{
			if (action == null)
			{
				return "NULL";
			}
			string text = action.descriptiveName;
			if (action.type == InputActionType.Axis)
			{
				text = "____" + ((!negativeAxis) ? action.positiveDescriptiveName : action.negativeDescriptiveName);
			}
			if (text.NullOrEmpty())
			{
				text = action.name;
			}
			return UiTranslationDatabase.TranslateKey(text.ToUpper(), text.ToUpperInvariant(), true).Trim();
		}

		
		private static InputAction GetAction(string actionName)
		{
			return ReInput.mapping.GetAction(actionName);
		}

		
		private static ActionElementMap[] GetElementMap(InputAction action, bool negativeAxis)
		{
			if (action == null)
			{
				return null;
			}
			ActionElementMap[] array = new ActionElementMap[1];
			List<ControllerMap> list = Input.player.controllers.maps.GetAllMaps().ToList<ControllerMap>();
			foreach (ControllerMap controllerMap in list)
			{
				if (controllerMap.controllerType == ControllerType.Mouse || controllerMap.controllerType == ControllerType.Keyboard)
				{
					List<ActionElementMap> list2 = (from m in controllerMap.AllMaps
					where m.actionId == action.id
					select m).ToList<ActionElementMap>();
					foreach (ActionElementMap actionElementMap in list2)
					{
						bool flag = actionElementMap.axisContribution == Pole.Negative;
						if (flag == negativeAxis)
						{
							if (actionElementMap.elementType == ControllerElementType.Button)
							{
								if (array[0] != null)
								{
									Debug.Log(string.Concat(new object[]
									{
										"Multiple maps! ",
										array[0].keyCode,
										" & ",
										actionElementMap.keyCode
									}));
								}
								array[0] = actionElementMap;
							}
						}
					}
				}
			}
			if (array[0] == null)
			{
				return null;
			}
			return array;
		}

		
		public void Update()
		{
			if (this._state == MappingButtonState.Idle)
			{
				return;
			}
			this.PollForKeyboardInput();
		}

		
		private void OnDisable()
		{
			this._foundMaps = null;
			this._foundAction = null;
			this._controllerMap = null;
			this._pollingInfo = default(ControllerPollingInfo);
			this._modifierKeyFlags = ModifierKeyFlags.None;
			this._state = MappingButtonState.Idle;
			this.ShowWaiting(false);
		}

		
		private void CancelAssignInput()
		{
			this.PollingStop();
		}

		
		private void ClearMapping()
		{
			if (this._foundMaps.NullOrEmpty<ActionElementMap>())
			{
				return;
			}
			this._controllerMap = Input.player.controllers.maps.GetMap(this._foundMaps[0].controllerMap.controllerType, 0, 0, 1);
			if (this._foundMaps[0] == null || this._controllerMap == null)
			{
				return;
			}
			if (!this._foundMaps[0].controllerMap.DeleteElementMap(this._foundMaps[0].id))
			{
				Debug.Log("Clear Map Failed!");
			}
			else
			{
				this._foundMaps = null;
			}
			this.Refresh();
		}

		
		public void AssignInput()
		{
			this.PollingStop();
			if (this._foundMaps.NullOrEmpty<ActionElementMap>() || this._foundMaps[0] == null)
			{
				this.CreateElementMap(this._pollingInfo);
			}
			else
			{
				this.ReplaceElementMap(this._pollingInfo);
			}
			this.UpdateElementMap();
			this.UpdateAssignedInput();
		}

		
		private void ReplaceElementMap(ControllerPollingInfo pollingInfo)
		{
			try
			{
				ElementAssignment elementAssignment = new ElementAssignment
				{
					elementMapId = this._foundMaps[0].id,
					actionId = this._foundAction.id,
					axisContribution = ((!this._negativeAxis) ? Pole.Positive : Pole.Negative),
					keyboardKey = pollingInfo.keyboardKey,
					modifierKeyFlags = this._modifierKeyFlags
				};
				if (!this._controllerMap.ReplaceElementMap(elementAssignment))
				{
					throw new InvalidOperationException("Rewire reported failure");
				}
				Debug.Log(string.Format("Replaced Map {0} {1} + {2}", this._foundAction.name, pollingInfo.keyboardKey, this._modifierKeyFlags));
			}
			catch (Exception ex)
			{
				Debug.Log(string.Format("Replace Map failed! {0}", ex.Message));
			}
		}

		
		private void CreateElementMap(ControllerPollingInfo pollingInfo)
		{
			try
			{
				ElementAssignment elementAssignment = new ElementAssignment
				{
					actionId = this._foundAction.id,
					axisContribution = ((!this._negativeAxis) ? Pole.Positive : Pole.Negative),
					keyboardKey = this._pollingInfo.keyboardKey,
					modifierKeyFlags = this._modifierKeyFlags
				};
				if (!this._controllerMap.CreateElementMap(elementAssignment))
				{
					throw new InvalidOperationException("Rewire reported failure");
				}
				Debug.Log(string.Format("Created Map {0} {1} + {2}", this._foundAction.name, pollingInfo.keyboardKey, this._modifierKeyFlags));
			}
			catch (Exception ex)
			{
				Debug.Log(string.Format("Create Map failed! {0}", ex.Message));
			}
		}

		
		private void ShowWaiting(bool showValue)
		{
			this._pulseObject.SetActive(showValue);
		}

		
		private void PollForMouseInput()
		{
			Player player = Input.player;
			if (player == null)
			{
				return;
			}
			this._pollingInfo = player.controllers.polling.PollControllerForFirstElementDown(ControllerType.Mouse, 0);
			if (this._pollingInfo.success && ((this._pollingInfo.elementType == ControllerElementType.Axis && this._actionType == InputActionType.Axis) || this._pollingInfo.elementType == ControllerElementType.Button))
			{
				this._controllerMap = Input.player.controllers.maps.GetMap(ControllerType.Mouse, 0, 0, 0);
				this._controllerType = ControllerType.Mouse;
				this.AssignInput();
			}
		}

		
		private void PollForKeyboardInput()
		{
			int num = 0;
			ControllerPollingInfo pollingInfo = default(ControllerPollingInfo);
			ControllerPollingInfo pollingInfo2 = default(ControllerPollingInfo);
			ModifierKeyFlags modifierKeyFlags = ModifierKeyFlags.None;
			foreach (ControllerPollingInfo controllerPollingInfo in ReInput.controllers.Keyboard.PollForAllKeys())
			{
				KeyCode keyboardKey = controllerPollingInfo.keyboardKey;
				if (keyboardKey == KeyCode.Escape)
				{
					this.CancelAssignInput();
					return;
				}
				if (keyboardKey != KeyCode.AltGr)
				{
					if (Keyboard.IsModifierKey(controllerPollingInfo.keyboardKey))
					{
						if (num == 0)
						{
							pollingInfo2 = controllerPollingInfo;
						}
						modifierKeyFlags |= Keyboard.KeyCodeToModifierKeyFlags(keyboardKey);
						num++;
					}
					else if (pollingInfo.keyboardKey == KeyCode.None)
					{
						pollingInfo = controllerPollingInfo;
					}
				}
			}
			if (pollingInfo.keyboardKey != KeyCode.None)
			{
				if (num == 0)
				{
					this._pollingInfo = pollingInfo;
					this._controllerMap = Input.player.controllers.maps.GetMap(ControllerType.Keyboard, 0, 0, 1);
					this._controllerType = ControllerType.Keyboard;
					this.AssignInput();
					return;
				}
				this._pollingInfo = pollingInfo;
				this._modifierKeyFlags = modifierKeyFlags;
				this._controllerMap = Input.player.controllers.maps.GetMap(ControllerType.Keyboard, 0, 0, 1);
				this._controllerType = ControllerType.Keyboard;
				this.AssignInput();
				return;
			}
			else if (num > 0 && num == 1 && ReInput.controllers.Keyboard.GetKeyTimePressed(pollingInfo2.keyboardKey) > 1f)
			{
				this._pollingInfo = pollingInfo2;
				this._controllerMap = Input.player.controllers.maps.GetMap(ControllerType.Keyboard, 0, 0, 1);
				this._controllerType = ControllerType.Keyboard;
				this.AssignInput();
				return;
			}
		}

		
		public void OnClick()
		{
			if (UICamera.currentTouchID == -1)
			{
				this.PollingStart();
			}
			else if (UICamera.currentTouchID == -2)
			{
				this.ClearMapping();
			}
		}

		
		private void PollingStart()
		{
			this._state = MappingButtonState.Waiting;
			this._modifierKeyFlags = ModifierKeyFlags.None;
			InputMappingButtonManager.PollingStarted(this);
			this.ShowWaiting(true);
		}

		
		private void PollingStop()
		{
			this._state = MappingButtonState.Idle;
			InputMappingButtonManager.PollingStopped(this);
			this.ShowWaiting(false);
		}

		
		public string _actionName;

		
		public InputActionType _actionType = InputActionType.Button;

		
		public AxisType _axisType;

		
		public bool _negativeAxis;

		
		private ControllerType _controllerType;

		
		public UIButton _button;

		
		public UILabel _actionLabel;

		
		public UILabel _inputLabel;

		
		public MappingButtonState _state;

		
		public GameObject _pulseObject;

		
		private string _actionInput;

		
		private ActionElementMap[] _foundMaps;

		
		private InputAction _foundAction;

		
		private ControllerMap _controllerMap;

		
		private ControllerPollingInfo _pollingInfo;

		
		private ModifierKeyFlags _modifierKeyFlags;
	}
}
