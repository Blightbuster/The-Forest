﻿using System;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using UniLinq;
using UnityEngine;

namespace TheForest.Utils
{
	
	public class InputMappingAction : MonoBehaviour
	{
		
		private void Start()
		{
			if (!this._mappingManager)
			{
				this._mappingManager = UnityEngine.Object.FindObjectOfType<InputMapping>();
			}
			this._actionRowMappingCount = new Dictionary<InputActionRow, int>();
			this._knownActionMaps = new Dictionary<ActionElementMap, InputActionButton>();
			this.ShowUserAssignableActions();
			this._nextChangeTimer = Time.realtimeSinceStartup + this._interChangeDelay;
			base.enabled = false;
			ReInput.ControllerConnectedEvent -= this.OnControllerConnectedEvent;
			ReInput.ControllerConnectedEvent += this.OnControllerConnectedEvent;
			ReInput.ControllerDisconnectedEvent -= this.OnControllerConnectedEvent;
			ReInput.ControllerDisconnectedEvent += this.OnControllerConnectedEvent;
			if (Input.player != null)
			{
				Input.player.controllers.maps.SetMapsEnabled(true, ControllerType.Joystick, "Menu");
			}
		}

		
		private void Update()
		{
			if (this._pollInput)
			{
				if (this._autoCancelTimer < Time.realtimeSinceStartup)
				{
					this.Cancel();
				}
				else
				{
					this._selectionScreenTimer.text = Mathf.RoundToInt(this._autoCancelTimer - Time.realtimeSinceStartup) + "s";
					this.PollControllerForAssignment();
				}
			}
			else if (Input.GetButtonDown("Esc"))
			{
				this.CancelConflictingMapping();
				this.Cancel();
			}
		}

		
		private void FixedUpdate()
		{
			this.Update();
		}

		
		private void OnGUI()
		{
			this.Update();
		}

		
		private void OnDisable()
		{
			if (base.enabled)
			{
				this._mappingManager.LoadAllMaps();
			}
		}

		
		private void OnDestroy()
		{
			ReInput.ControllerConnectedEvent -= this.OnControllerConnectedEvent;
			ReInput.ControllerDisconnectedEvent -= this.OnControllerConnectedEvent;
		}

		
		private void OnControllerConnectedEvent(ControllerStatusChangedEventArgs obj)
		{
			if (this)
			{
				this.ResetUI();
			}
		}

		
		public void SelectController_Keyboard()
		{
			this._controllerType = ControllerType.Keyboard;
			this.ResetUI();
		}

		
		public void SelectController_Mouse()
		{
			this._controllerType = ControllerType.Mouse;
			this.ResetUI();
		}

		
		public void SelectController_Joystick()
		{
			this._controllerType = ControllerType.Joystick;
			this.ResetUI();
		}

		
		public void SaveChanges()
		{
			this._mappingManager.SaveAllMaps();
			this._mappingManager.LoadAllMaps();
		}

		
		public void CancelChanges()
		{
			this._mappingManager.LoadAllMaps();
			this.ResetUI();
		}

		
		public void ResetUI()
		{
			this.ClearUI();
			this.Start();
		}

		
		public void RestoreDefaults()
		{
			this._mappingManager.ClearAllMaps();
			Input input = UnityEngine.Object.FindObjectOfType<Input>();
			if (input)
			{
				UnityEngine.Object.DestroyImmediate(input.gameObject);
			}
			UnityEngine.Object.FindObjectOfType<RewiredSpawner>().SendMessage("Awake");
			this._mappingManager = UnityEngine.Object.FindObjectOfType<InputMapping>();
			this._mappingManager.LoadAllMaps();
			this.ResetUI();
		}

		
		private IEnumerator StartPollInputDelayed()
		{
			if (!this._delayedPollInput && !this._pollInput)
			{
				this._delayedPollInput = true;
				if (Input.IsGamePad)
				{
					while (!Input.GetButtonUp("Fire1"))
					{
						yield return null;
					}
					yield return null;
				}
				this._delayedPollInput = false;
				this.StartPollInput();
			}
			yield break;
		}

		
		private void StartPollInput()
		{
			if (!base.enabled && this._nextChangeTimer < Time.realtimeSinceStartup)
			{
				this._autoCancelTimer = Time.realtimeSinceStartup + this._inputSelectionDuration;
				this._selectionScreenTimer.gameObject.SetActive(true);
				base.enabled = true;
				this._pollInput = true;
				if (LocalPlayer.Inventory)
				{
					LocalPlayer.Inventory.enabled = false;
				}
				if (Input.player != null)
				{
					Input.player.controllers.maps.SetMapsEnabled(false, ControllerType.Joystick, "Menu");
				}
			}
		}

		
		private void StopPollInput()
		{
			this._pollInput = false;
			base.enabled = false;
			if (LocalPlayer.Inventory)
			{
				LocalPlayer.Inventory.enabled = true;
			}
			if (Input.player != null)
			{
				Input.player.controllers.maps.SetMapsEnabled(true, ControllerType.Joystick, "Menu");
			}
		}

		
		private void CheckMappingConflictAndConfirm()
		{
			if (base.enabled)
			{
				this._pollInput = false;
				this._selectionScreenTimer.gameObject.SetActive(false);
				bool flag = false;
				bool flag2 = false;
				string text = string.Empty;
				foreach (ElementAssignmentConflictInfo elementAssignmentConflictInfo in ReInput.controllers.conflictChecking.ElementAssignmentConflicts(this._entry.ToElementAssignmentConflictCheck()))
				{
					flag = true;
					ControllerMap controllerMap = Input.player.controllers.maps.GetAllMaps(elementAssignmentConflictInfo.controllerType).First<ControllerMap>();
					if (controllerMap != null)
					{
						ActionElementMap elementMap = controllerMap.GetElementMap(elementAssignmentConflictInfo.elementMapId);
						if (elementMap != null)
						{
							int actionId = elementMap.actionId;
							if (this._entry.actionId == actionId)
							{
								this.Cancel();
								return;
							}
							InputAction action = ReInput.mapping.GetAction(actionId);
							string text2 = (!(action.descriptiveName != string.Empty)) ? action.name : action.descriptiveName;
							if (!elementAssignmentConflictInfo.isUserAssignable || !action.userAssignable)
							{
								flag2 = true;
								text = text2;
								break;
							}
							text = text + text2 + ", ";
						}
					}
				}
				if (flag)
				{
					if (flag2)
					{
						string message = this._entry.elementName + " is already in use and is protected from reassignment. You cannot remove the protected assignment, but you can still assign the action to this element. If you do so, the element will trigger multiple actions when activated.";
						Debug.Log(message);
						this._mappingSystemConflictUI.text = this._entry.elementName.ToUpperInvariant();
						this._mappingSystemConflictUI.transform.parent.gameObject.SetActive(true);
					}
					else
					{
						string message2 = this._entry.elementName + " is already in use. You may replace the other conflicting assignments, add this assignment anyway which will leave multiple actions assigned to this element, or cancel this assignment.";
						Debug.Log(message2);
						text = text.TrimEnd(new char[]
						{
							' ',
							','
						});
						this._mappingConflictResolutionActionLabel.text = text;
						this._mappingConflictResolutionKeyLabel.text = this._entry.elementName.ToUpperInvariant();
						this._mappingConflictResolutionKeyLabel.transform.parent.gameObject.SetActive(true);
					}
					this._nextChangeTimer = float.MaxValue;
				}
				else
				{
					this.Confirm(InputMappingAction.ConflictResolution.DoNothing);
				}
			}
		}

		
		public void ConfirmReplacingConflicts()
		{
			this.Confirm(InputMappingAction.ConflictResolution.Replace);
			this.ResetUI();
		}

		
		public void ConfirmKeepingConflicts()
		{
			this.Confirm(InputMappingAction.ConflictResolution.DoNothing);
			this.ResetUI();
		}

		
		public void CancelConflictingMapping()
		{
			this._selectionScreenTimer.gameObject.SetActive(false);
			this._mappingConflictResolutionKeyLabel.transform.parent.gameObject.SetActive(false);
			this._mappingSystemConflictUI.transform.parent.gameObject.SetActive(false);
			this._nextChangeTimer = Time.realtimeSinceStartup + this._interChangeDelay;
			this.StopPollInput();
			this.ResetUI();
		}

		
		private void Confirm(InputMappingAction.ConflictResolution conflictResolution)
		{
			if (conflictResolution > InputMappingAction.ConflictResolution.Pending)
			{
				if (conflictResolution == InputMappingAction.ConflictResolution.Replace)
				{
					ElementAssignmentConflictInfo info;
					foreach (ElementAssignmentConflictInfo info2 in ReInput.controllers.conflictChecking.ElementAssignmentConflicts(this._entry.ToElementAssignmentConflictCheck()))
					{
						info = info2;
						if (this._knownActionMaps.Any((KeyValuePair<ActionElementMap, InputActionButton> m) => m.Key.id == info.elementMapId))
						{
							ActionElementMap elementMap = this._knownActionMaps.First((KeyValuePair<ActionElementMap, InputActionButton> m) => m.Key.id == info.elementMapId).Key;
							UnityEngine.Object.Destroy(this._knownActionMaps[elementMap].gameObject);
							this._knownActionMaps.Remove(elementMap);
							InputActionRow key = this._actionRowMappingCount.First((KeyValuePair<InputActionRow, int> r) => r.Key._action.id == elementMap.actionId).Key;
							Dictionary<InputActionRow, int> actionRowMappingCount;
							Dictionary<InputActionRow, int> dictionary = actionRowMappingCount = this._actionRowMappingCount;
							InputActionRow key3;
							InputActionRow key2 = key3 = key;
							int num = actionRowMappingCount[key3];
							dictionary[key2] = num - 1;
						}
					}
					ReInput.controllers.conflictChecking.RemoveElementAssignmentConflicts(this._entry.ToElementAssignmentConflictCheck());
				}
				if (this._entry.changeType != InputMappingAction.ElementAssignmentChangeType.Add)
				{
					this._entry.controllerMapPrevious.DeleteElementMap(this._entry.actionElementMapId);
					this._entry.controllerMapPrevious = null;
					this._knownActionMaps.Remove(this._entry.uiButton._actionElementMap);
					UnityEngine.Object.Destroy(this._entry.uiButton.gameObject);
					if (this._entry.uiRow)
					{
						Dictionary<InputActionRow, int> actionRowMappingCount2;
						Dictionary<InputActionRow, int> dictionary2 = actionRowMappingCount2 = this._actionRowMappingCount;
						InputActionRow key3;
						InputActionRow key4 = key3 = this._entry.uiRow;
						int num = actionRowMappingCount2[key3];
						dictionary2[key4] = num - 1;
					}
					this._entry.changeType = InputMappingAction.ElementAssignmentChangeType.Add;
				}
				this._entry.ReplaceOrCreateActionElementMap(false);
				if (this._entry.changeType == InputMappingAction.ElementAssignmentChangeType.Add)
				{
					ActionElementMap actionElementMap = this._entry.controllerMap.AllMaps.First((ActionElementMap m) => m.actionId == this._entry.actionId && !this._knownActionMaps.ContainsKey(m));
					bool showInvert = this._entry.actionType == InputActionType.Axis && actionElementMap.axisType == AxisType.Normal && this._entry.controllerType != ControllerType.Keyboard;
					this.AddActionAssignmentButton(this._entry.uiRow, Input.player.id, ReInput.mapping.GetAction(this._entry.actionId), actionElementMap.axisContribution, this._entry.controllerMap, false, actionElementMap, showInvert);
					this.HideAddActionMapButton(this._entry.uiRow, this._entry.controllerType);
				}
				this._selectionScreenTimer.gameObject.SetActive(false);
				this._mappingConflictResolutionKeyLabel.transform.parent.gameObject.SetActive(false);
				this._mappingSystemConflictUI.transform.parent.gameObject.SetActive(false);
				this._nextChangeTimer = Time.realtimeSinceStartup + this._interChangeDelay;
				this.ResetUI();
				this.StopPollInput();
			}
		}

		
		private void Cancel()
		{
			if (base.enabled)
			{
				this._selectionScreenTimer.gameObject.SetActive(false);
				this._nextChangeTimer = Time.realtimeSinceStartup + this._interChangeDelay;
				this.StopPollInput();
			}
		}

		
		private void AddNewUIActionCategory(string name)
		{
			UILabel uilabel = UnityEngine.Object.Instantiate<UILabel>(this._inputActionCategoryPrefab);
			uilabel.transform.parent = this._table.transform;
			uilabel.transform.localPosition = Vector3.zero;
			uilabel.transform.localScale = Vector3.one;
			uilabel.text = name.ToUpperInvariant();
		}

		
		private InputActionRow GetNewUIRow(InputAction action, string name)
		{
			InputActionRow inputActionRow = UnityEngine.Object.Instantiate<InputActionRow>(this._inputActionRowPrefab);
			inputActionRow.transform.parent = this._table.transform;
			inputActionRow.transform.localPosition = Vector3.zero;
			inputActionRow.transform.localScale = Vector3.one;
			inputActionRow._action = action;
			inputActionRow._label.text = name.ToUpperInvariant();
			this._actionRowMappingCount[inputActionRow] = 0;
			return inputActionRow;
		}

		
		private void ShowUserAssignableActions()
		{
			foreach (InputCategory inputCategory in ReInput.mapping.ActionCategories)
			{
				if (ReInput.mapping.ActionsInCategory(inputCategory.id).Count<InputAction>() > 0)
				{
					this.AddNewUIActionCategory(inputCategory.name);
					InputAction action;
					foreach (InputAction action2 in from a in ReInput.mapping.ActionsInCategory(inputCategory.id)
					where a.userAssignable
					select a)
					{
						action = action2;
						string name = (!(action.descriptiveName != string.Empty)) ? action.name : action.descriptiveName;
						InputActionRow newUIRow = this.GetNewUIRow(action, name);
						if (action.type == InputActionType.Button)
						{
							this.InitAddActionMapButton(newUIRow, Input.player.id, action, Pole.Positive, true);
							foreach (ControllerMap controllerMap in Input.player.controllers.maps.GetAllMaps())
							{
								foreach (ActionElementMap elementMap in controllerMap.AllMaps.Where((ActionElementMap m) => m.actionId == action.id))
								{
									this.AddActionAssignmentButton(newUIRow, Input.player.id, action, Pole.Positive, controllerMap, true, elementMap, false);
								}
							}
						}
						else if (action.type == InputActionType.Axis)
						{
							this.InitAddActionMapButton(newUIRow, Input.player.id, action, Pole.Positive, true);
							foreach (ControllerMap controllerMap2 in Input.player.controllers.maps.GetAllMaps())
							{
								foreach (ActionElementMap actionElementMap in controllerMap2.AllMaps.Where((ActionElementMap m) => m.actionId == action.id))
								{
									if (actionElementMap.elementType != ControllerElementType.Button)
									{
										if (actionElementMap.axisType != AxisType.Split)
										{
											this.AddActionAssignmentButton(newUIRow, Input.player.id, action, Pole.Positive, controllerMap2, true, actionElementMap, true);
										}
									}
								}
							}
							string str = (!(action.positiveDescriptiveName != string.Empty)) ? (action.descriptiveName + " +") : action.positiveDescriptiveName;
							newUIRow = this.GetNewUIRow(action, "    " + str);
							this.InitAddActionMapButton(newUIRow, Input.player.id, action, Pole.Positive, false);
							foreach (ControllerMap controllerMap3 in Input.player.controllers.maps.GetAllMaps())
							{
								foreach (ActionElementMap actionElementMap2 in controllerMap3.AllMaps.Where((ActionElementMap m) => m.actionId == action.id))
								{
									if (actionElementMap2.axisContribution == Pole.Positive)
									{
										if (actionElementMap2.axisType != AxisType.Normal)
										{
											this.AddActionAssignmentButton(newUIRow, Input.player.id, action, Pole.Positive, controllerMap3, false, actionElementMap2, false);
										}
									}
								}
							}
							string str2 = (!(action.negativeDescriptiveName != string.Empty)) ? (action.descriptiveName + " -") : action.negativeDescriptiveName;
							newUIRow = this.GetNewUIRow(action, "    " + str2);
							this.InitAddActionMapButton(newUIRow, Input.player.id, action, Pole.Negative, false);
							foreach (ControllerMap controllerMap4 in Input.player.controllers.maps.GetAllMaps())
							{
								foreach (ActionElementMap actionElementMap3 in controllerMap4.AllMaps.Where((ActionElementMap m) => m.actionId == action.id))
								{
									if (actionElementMap3.axisContribution == Pole.Negative)
									{
										if (actionElementMap3.axisType != AxisType.Normal)
										{
											this.AddActionAssignmentButton(newUIRow, Input.player.id, action, Pole.Negative, controllerMap4, false, actionElementMap3, false);
										}
									}
								}
							}
						}
					}
				}
			}
			this._table.repositionNow = true;
		}

		
		private void ClearUI()
		{
			if (this._table)
			{
				int i = this._table.transform.childCount;
				while (i > 0)
				{
					UnityEngine.Object.Destroy(this._table.transform.GetChild(--i).gameObject);
				}
			}
		}

		
		private void InitAddActionMapButton(InputActionRow uiRow, int playerId, InputAction action, Pole actionAxisContribution, bool assignFullAxis)
		{
			ControllerType controllerType = ControllerType.Keyboard;
			UIButton[] addButtons = uiRow._addButtons;
			for (int i = 0; i < addButtons.Length; i++)
			{
				UIButton uibutton = addButtons[i];
				ControllerType currentType = controllerType;
				uibutton.onClick.Add(new EventDelegate(delegate
				{
					if (UICamera.currentTouchID == -1 && !this.enabled)
					{
						this._controllerType = currentType;
						this._replaceElementMap = false;
						this._entry = new InputMappingAction.ElementAssignmentChange(playerId, InputMappingAction.ElementAssignmentChangeType.Add, -1, action.id, actionAxisContribution, action.type, assignFullAxis, false);
						this._entry.uiRow = uiRow;
						this.StartPollInput();
					}
				}));
				controllerType++;
			}
		}

		
		private void HideAddActionMapButton(InputActionRow uiRow, ControllerType controllerType)
		{
			uiRow._addButtons[(int)controllerType].gameObject.SetActive(false);
		}

		
		private void ShowActionMappingCount(InputActionRow uiRow, ControllerType controllerType)
		{
			uiRow._addButtons[(int)controllerType].gameObject.SetActive(true);
		}

		
		private void AddActionAssignmentButton(InputActionRow uiRow, int playerId, InputAction action, Pole actionAxisContribution, ControllerMap controllerMap, bool assignFullAxis, ActionElementMap elementMap, bool showInvert = false)
		{
			if (uiRow._addButtons[(int)controllerMap.controllerType].gameObject.activeSelf)
			{
				InputActionButton uiButton;
				if (!showInvert)
				{
					uiButton = UnityEngine.Object.Instantiate<InputActionButton>(this._inputActionButtonPrefab);
				}
				else
				{
					uiButton = UnityEngine.Object.Instantiate<InputActionButton>(this._inputAxisActionButtonPrefab);
				}
				uiButton._label.text = elementMap.elementIdentifierName.ToUpperInvariant();
				uiButton._actionElementMap = elementMap;
				uiButton._button.onClick.Add(new EventDelegate(delegate
				{
					if (!this.enabled && this._nextChangeTimer < Time.realtimeSinceStartup && !this._delayedPollInput && !this._pollInput)
					{
						if ((Input.IsGamePad && Input.GetButtonDown("Fire1")) || (!Input.IsGamePad && UICamera.currentTouchID == -1))
						{
							this._controllerType = controllerMap.controllerType;
							this._replaceElementMap = true;
							this._entry = new InputMappingAction.ElementAssignmentChange(playerId, InputMappingAction.ElementAssignmentChangeType.ReassignOrRemove, elementMap.id, action.id, actionAxisContribution, action.type, assignFullAxis, elementMap.invert);
							this._entry.controllerMap = controllerMap;
							this._entry.controllerMapPrevious = controllerMap;
							this._entry.uiButton = uiButton;
							this._entry.uiRow = uiRow;
							this.StartCoroutine(this.StartPollInputDelayed());
						}
						else if ((Input.IsGamePad && Input.GetButtonDown("AltFire")) || (!Input.IsGamePad && UICamera.currentTouchID == -2))
						{
							this._controllerType = controllerMap.controllerType;
							this._entry = new InputMappingAction.ElementAssignmentChange(playerId, InputMappingAction.ElementAssignmentChangeType.Remove, elementMap.id, action.id, actionAxisContribution, action.type, assignFullAxis, elementMap.invert);
							controllerMap.DeleteElementMap(this._entry.actionElementMapId);
							this._knownActionMaps.Remove(elementMap);
							UnityEngine.Object.Destroy(uiButton.gameObject);
							Dictionary<InputActionRow, int> actionRowMappingCount2;
							Dictionary<InputActionRow, int> dictionary2 = actionRowMappingCount2 = this._actionRowMappingCount;
							InputActionRow uiRow3;
							InputActionRow key2 = uiRow3 = uiRow;
							int num2 = actionRowMappingCount2[uiRow3];
							dictionary2[key2] = num2 - 1;
							this.ShowActionMappingCount(uiRow, controllerMap.controllerType);
							this._nextChangeTimer = Time.realtimeSinceStartup + this._interChangeDelay;
							this.ResetUI();
						}
					}
				}));
				if (showInvert)
				{
					uiButton._invertAxisToggle.gameObject.SetActive(true);
					uiButton._invertAxisToggle.value = uiButton._actionElementMap.invert;
					uiButton._invertAxisToggle.onChange.Add(new EventDelegate(delegate
					{
						if (!this.enabled && this._nextChangeTimer < Time.realtimeSinceStartup)
						{
							uiButton._actionElementMap.invert = uiButton._invertAxisToggle.value;
						}
					}));
				}
				UIButtonColor component = uiButton.GetComponent<UIButtonColor>();
				UIButtonColor component2 = uiRow._addButtons[(int)controllerMap.controllerType].GetComponent<UIButtonColor>();
				if (component && component2)
				{
					component.tweenTarget = uiRow._addButtons[(int)controllerMap.controllerType].GetComponent<UIButtonColor>().tweenTarget;
				}
				else
				{
					Debug.LogWarning("Missing tweener for element: " + elementMap.elementIdentifierName);
				}
				uiButton.transform.parent = uiRow._addButtons[(int)controllerMap.controllerType].transform.parent;
				uiButton.transform.localPosition = Vector3.zero;
				uiButton.transform.localScale = Vector3.one;
				this.HideAddActionMapButton(uiRow, controllerMap.controllerType);
				this._knownActionMaps.Add(elementMap, uiButton);
				Dictionary<InputActionRow, int> actionRowMappingCount;
				Dictionary<InputActionRow, int> dictionary = actionRowMappingCount = this._actionRowMappingCount;
				InputActionRow uiRow2;
				InputActionRow key = uiRow2 = uiRow;
				int num = actionRowMappingCount[uiRow2];
				dictionary[key] = num + 1;
			}
		}

		
		private void PollControllerForAssignment()
		{
			if (base.enabled)
			{
				if (this._pollInput && this._controllerType == ControllerType.Keyboard)
				{
					this.PollKeyboardForAssignment();
				}
				if (this._pollInput && this._controllerType == ControllerType.Joystick)
				{
					this.PollJoystickForAssignment();
				}
				if (this._pollInput && this._controllerType == ControllerType.Mouse)
				{
					this.PollMouseForAssignment();
				}
			}
		}

		
		private void PollKeyboardForAssignment()
		{
			int num = 0;
			ControllerPollingInfo pollingInfo = default(ControllerPollingInfo);
			ControllerPollingInfo pollingInfo2 = default(ControllerPollingInfo);
			ModifierKeyFlags modifierKeyFlags = ModifierKeyFlags.None;
			foreach (ControllerPollingInfo controllerPollingInfo in ReInput.controllers.Keyboard.PollForAllKeys())
			{
				KeyCode keyboardKey = controllerPollingInfo.keyboardKey;
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
			if (pollingInfo.keyboardKey == KeyCode.None)
			{
				if (num > 0)
				{
					if (num == 1)
					{
						if (ReInput.controllers.Keyboard.GetKeyTimePressed(pollingInfo2.keyboardKey) > 1f)
						{
							this._entry.pollingInfo = pollingInfo2;
							this._entry.controllerId = 0;
							this._entry.controllerType = ControllerType.Keyboard;
							this._entry.controllerMap = Input.player.controllers.maps.GetMap(ControllerType.Keyboard, 0, 0, 1);
							this.CheckMappingConflictAndConfirm();
							return;
						}
						this._selectionScreenTimer.text = Keyboard.GetKeyName(pollingInfo2.keyboardKey).ToUpperInvariant();
					}
					else
					{
						this._selectionScreenTimer.text = Keyboard.ModifierKeyFlagsToString(modifierKeyFlags).ToUpperInvariant();
					}
				}
				return;
			}
			if (num == 0)
			{
				this._entry.pollingInfo = pollingInfo;
				this._entry.controllerId = 0;
				this._entry.controllerType = ControllerType.Keyboard;
				this._entry.controllerMap = Input.player.controllers.maps.GetMap(ControllerType.Keyboard, 0, 0, 1);
				this.CheckMappingConflictAndConfirm();
				return;
			}
			this._entry.pollingInfo = pollingInfo;
			this._entry.modifierKeyFlags = modifierKeyFlags;
			this._entry.controllerId = 0;
			this._entry.controllerType = ControllerType.Keyboard;
			this._entry.controllerMap = Input.player.controllers.maps.GetMap(ControllerType.Keyboard, 0, 0, 1);
			this.CheckMappingConflictAndConfirm();
		}

		
		private void PollJoystickForAssignment()
		{
			Player player = ReInput.players.GetPlayer(this._entry.playerId);
			if (player == null)
			{
				this.Cancel();
				return;
			}
			foreach (Rewired.Joystick joystick in ReInput.controllers.Joysticks)
			{
				this._entry.pollingInfo = player.controllers.polling.PollControllerForFirstElementDown(ControllerType.Joystick, joystick.id);
				if (this._entry.pollingInfo.success)
				{
					this._entry.controllerId = joystick.id;
					this._entry.controllerType = ControllerType.Joystick;
					this._entry.controllerMap = Input.player.controllers.maps.GetMap(ControllerType.Joystick, joystick.id, 0, 0);
					this.CheckMappingConflictAndConfirm();
					break;
				}
			}
		}

		
		private void PollMouseForAssignment()
		{
			Player player = ReInput.players.GetPlayer(this._entry.playerId);
			if (player == null)
			{
				this.Cancel();
				return;
			}
			this._entry.pollingInfo = player.controllers.polling.PollControllerForFirstElementDown(ControllerType.Mouse, 0);
			if (this._entry.pollingInfo.success && ((this._entry.pollingInfo.elementType == ControllerElementType.Axis && this._entry.actionType == InputActionType.Axis) || this._entry.pollingInfo.elementType == ControllerElementType.Button))
			{
				this._entry.controllerId = 0;
				this._entry.controllerType = ControllerType.Mouse;
				this._entry.controllerMap = Input.player.controllers.maps.GetMap(ControllerType.Mouse, 0, 0, 0);
				this.CheckMappingConflictAndConfirm();
			}
		}

		
		public InputMapping _mappingManager;

		
		public UILabel _inputActionCategoryPrefab;

		
		public InputActionRow _inputActionRowPrefab;

		
		public InputActionButton _inputActionButtonPrefab;

		
		public InputActionButton _inputAxisActionButtonPrefab;

		
		public UITable _table;

		
		public UILabel _selectionScreenTimer;

		
		public UILabel _mappingConflictResolutionKeyLabel;

		
		public UILabel _mappingConflictResolutionActionLabel;

		
		public UILabel _mappingSystemConflictUI;

		
		public GameObject _cancelButton;

		
		public float _inputSelectionDuration = 5f;

		
		public float _interChangeDelay = 0.5f;

		
		private bool _delayedPollInput;

		
		private bool _pollInput;

		
		private bool _replaceElementMap;

		
		private ControllerType _controllerType;

		
		private InputMappingAction.ElementAssignmentChange _entry;

		
		private float _autoCancelTimer;

		
		private float _nextChangeTimer;

		
		private Dictionary<InputActionRow, int> _actionRowMappingCount;

		
		private Dictionary<ActionElementMap, InputActionButton> _knownActionMaps;

		
		private class ControllerSelection
		{
			
			public ControllerSelection()
			{
				this.Clear();
			}

			
			
			
			public int id
			{
				get
				{
					return this._id;
				}
				set
				{
					this._idPrev = this._id;
					this._id = value;
				}
			}

			
			
			
			public ControllerType type
			{
				get
				{
					return this._type;
				}
				set
				{
					this._typePrev = this._type;
					this._type = value;
				}
			}

			
			
			public int idPrev
			{
				get
				{
					return this._idPrev;
				}
			}

			
			
			public ControllerType typePrev
			{
				get
				{
					return this._typePrev;
				}
			}

			
			
			public bool hasSelection
			{
				get
				{
					return this._id >= 0;
				}
			}

			
			public void Set(int id, ControllerType type)
			{
				this.id = id;
				this.type = type;
			}

			
			public void Clear()
			{
				this._id = -1;
				this._idPrev = -1;
				this._type = ControllerType.Joystick;
				this._typePrev = ControllerType.Joystick;
			}

			
			private int _id;

			
			private int _idPrev;

			
			private ControllerType _type;

			
			private ControllerType _typePrev;
		}

		
		public class ElementAssignmentChange
		{
			
			public ElementAssignmentChange(int playerId, InputMappingAction.ElementAssignmentChangeType changeType, int actionElementMapId, int actionId, Pole actionAxisContribution, InputActionType actionType, bool assignFullAxis, bool invert)
			{
				this.playerId = playerId;
				this.changeType = changeType;
				this.actionElementMapId = actionElementMapId;
				this.actionId = actionId;
				this.actionAxisContribution = actionAxisContribution;
				this.actionType = actionType;
				this.assignFullAxis = assignFullAxis;
				this.invert = invert;
			}

			
			public ElementAssignmentChange(InputMappingAction.ElementAssignmentChange source)
			{
				this.playerId = source.playerId;
				this.controllerId = source.controllerId;
				this.controllerType = source.controllerType;
				this.controllerMap = source.controllerMap;
				this.changeType = source.changeType;
				this.actionElementMapId = source.actionElementMapId;
				this.actionId = source.actionId;
				this.actionAxisContribution = source.actionAxisContribution;
				this.actionType = source.actionType;
				this.assignFullAxis = source.assignFullAxis;
				this.invert = source.invert;
				this.pollingInfo = source.pollingInfo;
				this.modifierKeyFlags = source.modifierKeyFlags;
			}

			
			
			
			public int playerId { get; private set; }

			
			
			
			public int controllerId { get; set; }

			
			
			
			public ControllerType controllerType { get; set; }

			
			
			
			public ControllerMap controllerMapPrevious { get; set; }

			
			
			
			public ControllerMap controllerMap { get; set; }

			
			
			
			public int actionElementMapId { get; private set; }

			
			
			
			public int actionId { get; private set; }

			
			
			
			public Pole actionAxisContribution { get; private set; }

			
			
			
			public InputActionType actionType { get; private set; }

			
			
			
			public bool assignFullAxis { get; private set; }

			
			
			
			public bool invert { get; private set; }

			
			
			
			public InputMappingAction.ElementAssignmentChangeType changeType { get; set; }

			
			
			
			public ControllerPollingInfo pollingInfo { get; set; }

			
			
			
			public ModifierKeyFlags modifierKeyFlags { get; set; }

			
			
			public AxisRange AssignedAxisRange
			{
				get
				{
					if (!this.pollingInfo.success)
					{
						return AxisRange.Positive;
					}
					ControllerElementType elementType = this.pollingInfo.elementType;
					Pole axisPole = this.pollingInfo.axisPole;
					AxisRange result = AxisRange.Positive;
					if (elementType == ControllerElementType.Axis)
					{
						if (this.actionType == InputActionType.Axis)
						{
							if (this.assignFullAxis)
							{
								result = AxisRange.Full;
							}
							else
							{
								result = ((axisPole != Pole.Positive) ? AxisRange.Negative : AxisRange.Positive);
							}
						}
						else
						{
							result = ((axisPole != Pole.Positive) ? AxisRange.Negative : AxisRange.Positive);
						}
					}
					return result;
				}
			}

			
			
			public string elementName
			{
				get
				{
					if (this.controllerType == ControllerType.Keyboard && this.modifierKeyFlags != ModifierKeyFlags.None)
					{
						return string.Format("{0} + {1}", Keyboard.ModifierKeyFlagsToString(this.modifierKeyFlags), this.pollingInfo.elementIdentifierName);
					}
					return this.pollingInfo.elementIdentifierName;
				}
			}

			
			public void ReplaceOrCreateActionElementMap(bool replaceElementMap)
			{
				ElementAssignment elementAssignment = this.ToElementAssignment();
				if (replaceElementMap)
				{
					this.controllerMap.ReplaceElementMap(elementAssignment);
				}
				else
				{
					this.controllerMap.ReplaceOrCreateElementMap(elementAssignment);
				}
			}

			
			public ElementAssignmentConflictCheck ToElementAssignmentConflictCheck()
			{
				return new ElementAssignmentConflictCheck(this.playerId, this.controllerType, this.controllerId, this.controllerMap.id, this.pollingInfo.elementType, this.pollingInfo.elementIdentifierId, this.AssignedAxisRange, this.pollingInfo.keyboardKey, this.modifierKeyFlags, this.actionId, this.actionAxisContribution, this.invert, this.actionElementMapId);
			}

			
			public ElementAssignment ToElementAssignment()
			{
				return new ElementAssignment(this.controllerType, this.pollingInfo.elementType, this.pollingInfo.elementIdentifierId, this.AssignedAxisRange, this.pollingInfo.keyboardKey, this.modifierKeyFlags, this.actionId, this.actionAxisContribution, this.invert, this.actionElementMapId);
			}

			
			public InputActionRow uiRow;

			
			public InputActionButton uiButton;
		}

		
		public enum ElementAssignmentChangeType
		{
			
			Add,
			
			Replace,
			
			Remove,
			
			ReassignOrRemove,
			
			ConflictCheck
		}

		
		private enum ConflictResolution
		{
			
			None,
			
			Pending,
			
			DoNothing,
			
			Replace
		}
	}
}
