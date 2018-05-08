using System;
using System.Collections;
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
							if (actionElementMap.elementType == ControllerElementType.Button || actionElementMap.elementType == ControllerElementType.Axis)
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

		
		public void Update()
		{
			if (this._state == MappingButtonState.Idle)
			{
				return;
			}
			this.PollForKeyboardInput();
		}

		
		public void OnEnable()
		{
			this.Refresh();
		}

		
		private void OnDisable()
		{
			this._foundRewiredMaps = null;
			this._foundRewiredAction = null;
			this._controllerMap = null;
			this._pollingInfo = default(ControllerPollingInfo);
			this._modifierKeyFlags = ModifierKeyFlags.None;
			this._state = MappingButtonState.Idle;
			this.ShowWaiting(false);
		}

		
		public IEnumerator EnableButtonAfterDelay(float delay)
		{
			yield return new WaitForSeconds(delay);
			yield return 0;
			if (this._button == null)
			{
				yield break;
			}
			this._button.enabled = true;
			yield break;
		}

		
		public void Refresh()
		{
			this._foundRewiredAction = null;
			this._foundRewiredMaps = null;
			this._controllerType = ControllerType.Keyboard;
			this._controllerMap = null;
			this._pollingInfo = default(ControllerPollingInfo);
			this._pollingControllerType = ControllerType.Keyboard;
			this._pollingControllerMap = null;
			this.CollectRewiredRepresentations();
			this.UpdateAssignedInput();
			this.UpdateActionLabel();
			this.UpdateGameObjectNames();
		}

		
		private void UpdateActionLabel()
		{
			if (this._foundRewiredAction == null)
			{
				this._actionLabel.text = "NULL";
			}
			else
			{
				this._actionLabel.text = InputMappingButton.GetActionUIText(this._foundRewiredAction, this._negativeAxis).ToUpperInvariant();
			}
		}

		
		private void UpdateAssignedInput()
		{
			if (this._foundRewiredMaps.SafeCount<ActionElementMap>() == 0)
			{
				this._actionInput = null;
			}
			else
			{
				this._actionInput = this._foundRewiredMaps[0].elementIdentifierName.ToUpperInvariant();
			}
			this._inputLabel.text = this._actionInput;
		}

		
		public void OnClick()
		{
			if (UICamera.currentTouchID == -1)
			{
				this.PollingStart();
			}
			else if (UICamera.currentTouchID == -2)
			{
				this.DeleteCurrentElementMap();
				this.Refresh();
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
				this._pollingControllerMap = Input.player.controllers.maps.GetMap(ControllerType.Mouse, 0, 0, 0);
				this._pollingControllerType = ControllerType.Mouse;
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
					this._pollingControllerMap = Input.player.controllers.maps.GetMap(ControllerType.Keyboard, 0, 0, 1);
					this._pollingControllerType = ControllerType.Keyboard;
					this.AssignInput();
					return;
				}
				this._pollingInfo = pollingInfo;
				this._modifierKeyFlags = modifierKeyFlags;
				this._pollingControllerMap = Input.player.controllers.maps.GetMap(ControllerType.Keyboard, 0, 0, 1);
				this._pollingControllerType = ControllerType.Keyboard;
				this.AssignInput();
				return;
			}
			else if (num > 0 && num == 1 && ReInput.controllers.Keyboard.GetKeyTimePressed(pollingInfo2.keyboardKey) > 1f)
			{
				this._pollingInfo = pollingInfo2;
				this._pollingControllerMap = Input.player.controllers.maps.GetMap(ControllerType.Keyboard, 0, 0, 1);
				this._pollingControllerType = ControllerType.Keyboard;
				this.AssignInput();
				return;
			}
		}

		
		private void CancelAssignInput()
		{
			this.PollingStop();
		}

		
		private void AssignInput()
		{
			this.PollingStop();
			if (this._foundRewiredMaps.NullOrEmpty<ActionElementMap>() || this._foundRewiredMaps[0] == null)
			{
				this.CreateElementMap();
			}
			else
			{
				this.ReplaceElementMap();
			}
			this.CollectRewiredRepresentations();
			this.UpdateAssignedInput();
		}

		
		private void CollectRewiredRepresentations()
		{
			this._foundRewiredAction = InputMappingButton.GetAction(this._actionName);
			this._foundRewiredMaps = InputMappingButton.GetElementMap(this._foundRewiredAction, this._negativeAxis);
			this._controllerMap = null;
			this._controllerType = ControllerType.Keyboard;
			if (!this._foundRewiredMaps.NullOrEmpty<ActionElementMap>() && this._foundRewiredMaps[0] != null)
			{
				this._controllerMap = this._foundRewiredMaps[0].controllerMap;
				this._controllerType = this._controllerMap.controllerType;
			}
		}

		
		private void DeleteCurrentElementMap()
		{
			if (this._foundRewiredMaps.NullOrEmpty<ActionElementMap>())
			{
				return;
			}
			if (this._foundRewiredMaps[0] == null || this._controllerMap == null)
			{
				return;
			}
			try
			{
				if (!this._controllerMap.DeleteElementMap(this._foundRewiredMaps[0].id))
				{
					throw new InvalidOperationException("Rewire reported failure");
				}
				Debug.Log(string.Format("Delete Map {0} {1} + {2}", this._foundRewiredAction.name, this._pollingInfo.keyboardKey, this._modifierKeyFlags));
			}
			catch (Exception ex)
			{
				Debug.Log(string.Format("Delete Map failed! {0}", ex.Message));
			}
			this._foundRewiredMaps = null;
		}

		
		private void ReplaceElementMap()
		{
			this.DeleteCurrentElementMap();
			this.CreateElementMap();
		}

		
		private void CreateElementMap()
		{
			try
			{
				ElementAssignment elementAssignment = new ElementAssignment
				{
					actionId = this._foundRewiredAction.id,
					axisContribution = ((!this._negativeAxis) ? Pole.Positive : Pole.Negative),
					keyboardKey = this._pollingInfo.keyboardKey,
					elementIdentifierId = this._pollingInfo.elementIdentifierId,
					modifierKeyFlags = this._modifierKeyFlags
				};
				if (!this._pollingControllerMap.CreateElementMap(elementAssignment))
				{
					throw new InvalidOperationException("Rewire reported failure");
				}
				this._controllerMap = this._pollingControllerMap;
				this._controllerType = this._pollingControllerType;
				Debug.Log(string.Format("Created Map {0} {1} + {2}", this._foundRewiredAction.name, this._pollingInfo.keyboardKey, this._modifierKeyFlags));
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

		
		public string _actionName;

		
		public InputActionType _actionType = InputActionType.Button;

		
		public AxisType _axisType;

		
		public bool _negativeAxis;

		
		public UIButton _button;

		
		public UILabel _actionLabel;

		
		public UILabel _inputLabel;

		
		public MappingButtonState _state;

		
		public GameObject _pulseObject;

		
		private string _actionInput;

		
		private ActionElementMap[] _foundRewiredMaps;

		
		private InputAction _foundRewiredAction;

		
		private ControllerMap _controllerMap;

		
		private ControllerType _controllerType;

		
		private ControllerPollingInfo _pollingInfo;

		
		private ControllerMap _pollingControllerMap;

		
		private ControllerType _pollingControllerType;

		
		private ModifierKeyFlags _modifierKeyFlags;
	}
}
