using System;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using TheForest.UI;
using UniLinq;
using UnityEngine;

namespace TheForest.Utils
{
	
	public class InputMappingAction : MonoBehaviour
	{
		
		private void Start()
		{
			this.InitializeUI(true);
		}

		
		private void Update()
		{
			if (this._pollInput)
			{
				if (Input.GetButtonDown("Esc"))
				{
					this.Cancel();
				}
				else
				{
					this.PollControllerForAssignment();
				}
			}
			else if (Input.GetButtonDown("Esc"))
			{
				this.CancelConflictingMapping();
				this.Cancel();
			}
			bool flag = this.IsModalWindowActive();
			for (int i = 0; i < this._actionTriggerEventToLock.Length; i++)
			{
				this._actionTriggerEventToLock[i].enabled = !flag;
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
				this.InitializeUI(false);
			}
		}

		
		public void SelectController_Keyboard()
		{
			this._controllerType = ControllerType.Keyboard;
			this.InitializeUI(true);
		}

		
		public void SelectController_Mouse()
		{
			this._controllerType = ControllerType.Mouse;
			this.InitializeUI(true);
		}

		
		public void SelectController_Joystick()
		{
			this._controllerType = ControllerType.Joystick;
			this.InitializeUI(true);
		}

		
		public void SaveChanges()
		{
			this._mappingManager.SaveAllMaps();
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
			this.InitializeUI(true);
		}

		
		private IEnumerator StartPollInputDelayed()
		{
			if (!this._delayedPollInput && !this._pollInput)
			{
				this._delayedPollInput = true;
				if (Input.IsGamePad)
				{
					while (!Input.GetButtonDown("Rebind"))
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
				base.enabled = true;
				this._pollInput = true;
				if (LocalPlayer.Inventory)
				{
					LocalPlayer.Inventory.enabled = false;
				}
				if (Input.player != null)
				{
					InputMappingAction.SetJoystickMenuMap(false);
				}
			}
		}

		
		private static void SetJoystickMenuMap(bool enabledValue)
		{
			if (Input.player == null)
			{
				return;
			}
			Input.player.controllers.maps.SetMapsEnabled(enabledValue, ControllerType.Joystick, "Menu");
			Input.player.controllers.maps.SetMapsEnabled(enabledValue, ControllerType.Joystick, "Default");
			Input.player.controllers.maps.SetMapsEnabled(enabledValue, ControllerType.Keyboard, "Default");
			Input.player.controllers.maps.SetMapsEnabled(enabledValue, ControllerType.Mouse, "Default");
		}

		
		private void StopPollInput()
		{
			this._pollInput = false;
			base.enabled = false;
			if (LocalPlayer.Inventory)
			{
				LocalPlayer.Inventory.enabled = true;
			}
			InputMappingAction.SetJoystickMenuMap(true);
		}

		
		private void CheckMappingConflictAndConfirm()
		{
			if (!base.enabled)
			{
				return;
			}
			this._pollInput = false;
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
						string text2 = action.descriptiveName.IfNullOrEmpty(action.name);
						text2 = UiTranslationDatabase.TranslateKey(text2.ToUpper(), text2, true);
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
					this.CancelConflictingMapping();
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
					this.ConfirmKeepingConflicts();
				}
				this._nextChangeTimer = Time.realtimeSinceStartup + this._interChangeDelay;
			}
			else
			{
				this.Confirm(InputMappingAction.ConflictResolution.DoNothing);
			}
		}

		
		public void ConfirmReplacingConflicts()
		{
			this.Confirm(InputMappingAction.ConflictResolution.Replace);
			this.InitializeUI(false);
		}

		
		public void ConfirmKeepingConflicts()
		{
			this.Confirm(InputMappingAction.ConflictResolution.DoNothing);
			this.InitializeUI(false);
		}

		
		public void CancelConflictingMapping()
		{
			this._nextChangeTimer = Time.realtimeSinceStartup + this._interChangeDelay;
			this.StopPollInput();
			this.InitializeUI(false);
		}

		
		public bool IsModalWindowActive()
		{
			return false;
		}

		
		private void Confirm(InputMappingAction.ConflictResolution conflictResolution)
		{
			if (conflictResolution > InputMappingAction.ConflictResolution.Pending)
			{
				if (conflictResolution == InputMappingAction.ConflictResolution.Replace)
				{
					using (IEnumerator<ElementAssignmentConflictInfo> enumerator = ReInput.controllers.conflictChecking.ElementAssignmentConflicts(this._entry.ToElementAssignmentConflictCheck()).GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							ElementAssignmentConflictInfo info = enumerator.Current;
							if (this._knownActionMaps.Any((KeyValuePair<ActionElementMap, InputActionButton> m) => m.Key.id == info.elementMapId))
							{
								ActionElementMap elementMap = this._knownActionMaps.First((KeyValuePair<ActionElementMap, InputActionButton> m) => m.Key.id == info.elementMapId).Key;
								UnityEngine.Object.Destroy(this._knownActionMaps[elementMap].gameObject);
								this._knownActionMaps.Remove(elementMap);
								InputActionRow key = this._actionRowMappingCount.First((KeyValuePair<InputActionRow, int> r) => r.Key._action.id == elementMap.actionId).Key;
								Dictionary<InputActionRow, int> actionRowMappingCount;
								InputActionRow key2;
								(actionRowMappingCount = this._actionRowMappingCount)[key2 = key] = actionRowMappingCount[key2] - 1;
							}
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
						Dictionary<InputActionRow, int> actionRowMappingCount;
						InputActionRow uiRow;
						(actionRowMappingCount = this._actionRowMappingCount)[uiRow = this._entry.uiRow] = actionRowMappingCount[uiRow] - 1;
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
				this._nextChangeTimer = Time.realtimeSinceStartup + this._interChangeDelay;
				this.InitializeUI(false);
				this.StopPollInput();
			}
		}

		
		private void Cancel()
		{
			if (base.enabled)
			{
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
			if (this._table.transform.childCount == 1)
			{
				uilabel.height = 40;
			}
			name = name.ToUpperInvariant();
			name = UiTranslationDatabase.TranslateKey(name, name, true);
			uilabel.text = name;
		}

		
		private InputActionRow GetNewUIRow(InputAction action, string name)
		{
			InputActionRow inputActionRow = UnityEngine.Object.Instantiate<InputActionRow>(this._inputActionRowPrefab);
			inputActionRow.transform.parent = this._table.transform;
			inputActionRow.transform.localPosition = Vector3.zero;
			inputActionRow.transform.localScale = Vector3.one;
			inputActionRow._action = action;
			name = name.ToUpperInvariant();
			name = UiTranslationDatabase.TranslateKey(name, name, true);
			inputActionRow._label.text = name;
			this._actionRowMappingCount[inputActionRow] = 0;
			return inputActionRow;
		}

		
		private void ShowUserAssignableActions()
		{
			List<InputActionButton> list = new List<InputActionButton>();
			foreach (InputCategory inputCategory in ReInput.mapping.ActionCategories)
			{
				InputAction[] array = (from a in ReInput.mapping.ActionsInCategory(inputCategory.id)
				where a.userAssignable
				select a).ToArray<InputAction>();
				if (array.Length != 0)
				{
					this.AddNewUIActionCategory(inputCategory.name);
					InputAction[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						InputAction action = array2[i];
						string name = action.descriptiveName.IfNullOrEmpty(action.name);
						InputActionRow newUIRow = this.GetNewUIRow(action, name);
						if (action.type == InputActionType.Button)
						{
							this.InitAddActionMapButton(newUIRow, Input.player.id, action, Pole.Positive, true);
							foreach (ControllerMap controllerMap in Input.player.controllers.maps.GetAllMaps())
							{
								foreach (ActionElementMap elementMap in from m in controllerMap.AllMaps
								where m.actionId == action.id
								select m)
								{
									InputActionButton inputActionButton = this.AddActionAssignmentButton(newUIRow, Input.player.id, action, Pole.Positive, controllerMap, true, elementMap, false);
									if (inputActionButton != null && controllerMap.controllerType == ControllerType.Joystick)
									{
										list.Add(inputActionButton);
									}
								}
							}
						}
						else if (action.type == InputActionType.Axis)
						{
							this.InitAddActionMapButton(newUIRow, Input.player.id, action, Pole.Positive, true);
							foreach (ControllerMap controllerMap2 in Input.player.controllers.maps.GetAllMaps())
							{
								foreach (ActionElementMap actionElementMap in from m in controllerMap2.AllMaps
								where m.actionId == action.id
								select m)
								{
									if (actionElementMap.elementType != ControllerElementType.Button)
									{
										if (actionElementMap.axisType != AxisType.Split)
										{
											InputActionButton inputActionButton2 = this.AddActionAssignmentButton(newUIRow, Input.player.id, action, Pole.Positive, controllerMap2, true, actionElementMap, false);
											if (inputActionButton2 != null && controllerMap2.controllerType == ControllerType.Joystick)
											{
												list.Add(inputActionButton2);
											}
										}
									}
								}
							}
							if (!action.positiveDescriptiveName.IsEmpty())
							{
								string positiveDescriptiveName = action.positiveDescriptiveName;
								newUIRow = this.GetNewUIRow(action, "____" + positiveDescriptiveName);
								this.InitAddActionMapButton(newUIRow, Input.player.id, action, Pole.Positive, false);
								foreach (ControllerMap controllerMap3 in Input.player.controllers.maps.GetAllMaps())
								{
									foreach (ActionElementMap actionElementMap2 in from m in controllerMap3.AllMaps
									where m.actionId == action.id
									select m)
									{
										if (actionElementMap2.axisContribution == Pole.Positive)
										{
											if (actionElementMap2.axisType != AxisType.Normal)
											{
												InputActionButton inputActionButton3 = this.AddActionAssignmentButton(newUIRow, Input.player.id, action, Pole.Positive, controllerMap3, false, actionElementMap2, false);
												if (inputActionButton3 != null && controllerMap3.controllerType == ControllerType.Joystick)
												{
													list.Add(inputActionButton3);
												}
											}
										}
									}
								}
							}
							if (!action.negativeDescriptiveName.IsEmpty())
							{
								string negativeDescriptiveName = action.negativeDescriptiveName;
								newUIRow = this.GetNewUIRow(action, "____" + negativeDescriptiveName);
								this.InitAddActionMapButton(newUIRow, Input.player.id, action, Pole.Negative, false);
								foreach (ControllerMap controllerMap4 in Input.player.controllers.maps.GetAllMaps())
								{
									foreach (ActionElementMap actionElementMap3 in from m in controllerMap4.AllMaps
									where m.actionId == action.id
									select m)
									{
										if (actionElementMap3.axisContribution == Pole.Negative)
										{
											if (actionElementMap3.axisType != AxisType.Normal)
											{
												InputActionButton inputActionButton4 = this.AddActionAssignmentButton(newUIRow, Input.player.id, action, Pole.Negative, controllerMap4, false, actionElementMap3, false);
												if (inputActionButton4 != null && controllerMap4.controllerType == ControllerType.Joystick)
												{
													list.Add(inputActionButton4);
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
			InputActionButton inputActionButton5 = null;
			for (int j = 0; j < list.Count; j++)
			{
				InputActionButton inputActionButton6 = list[j];
				UIKeyNavigation uiKeyNavigation = inputActionButton6._UiKeyNavigation;
				if (j == 0)
				{
					uiKeyNavigation.startsSelected = true;
				}
				if (inputActionButton5 != null)
				{
					uiKeyNavigation.onUp = inputActionButton5.gameObject;
					inputActionButton5._UiKeyNavigation.onDown = inputActionButton6.gameObject;
				}
				inputActionButton5 = inputActionButton6;
			}
			this._table.Reposition();
		}

		
		private void InitializeUI(bool resetScrollView)
		{
			this.ClearUI(resetScrollView);
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
			InputMappingAction.SetJoystickMenuMap(true);
			if (resetScrollView)
			{
				this._scrollView.ResetPosition();
				this._scrollbar.value = 0f;
			}
		}

		
		private void ClearUI(bool resetScrollView)
		{
			if (this._table == null)
			{
				return;
			}
			if (resetScrollView)
			{
				this._scrollView.ResetPosition();
			}
			foreach (Transform transform in this._table.GetChildList())
			{
				transform.parent = null;
				transform.gameObject.SetActive(false);
				UnityEngine.Object.Destroy(transform.gameObject);
			}
			this._table.Reposition();
			this._scrollView.UpdateScrollbars();
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
			ControllerType controllerType2 = controllerType;
			if (controllerType2 == ControllerType.Mouse)
			{
				controllerType2 = ControllerType.Keyboard;
			}
			uiRow._addButtons[(int)controllerType2].gameObject.SetActive(false);
		}

		
		private void ShowActionMappingCount(InputActionRow uiRow, ControllerType controllerType)
		{
			ControllerType controllerType2 = controllerType;
			if (controllerType2 == ControllerType.Mouse)
			{
				controllerType2 = ControllerType.Keyboard;
			}
			uiRow._addButtons[(int)controllerType2].gameObject.SetActive(true);
		}

		
		private InputActionButton AddActionAssignmentButton(InputActionRow uiRow, int playerId, InputAction action, Pole actionAxisContribution, ControllerMap controllerMap, bool assignFullAxis, ActionElementMap elementMap, bool showInvert = false)
		{
			ControllerType controllerType = controllerMap.controllerType;
			if (controllerType == ControllerType.Mouse)
			{
				controllerType = ControllerType.Keyboard;
			}
			if (!uiRow._addButtons[(int)controllerType].gameObject.activeSelf)
			{
				return null;
			}
			InputActionButton uiButton;
			if (!showInvert)
			{
				uiButton = UnityEngine.Object.Instantiate<InputActionButton>(this._inputActionButtonPrefab);
			}
			else
			{
				uiButton = UnityEngine.Object.Instantiate<InputActionButton>(this._inputAxisActionButtonPrefab);
			}
			uiButton.name = string.Format("actBtn_{0}_{1}_{2}", controllerMap.controllerType, elementMap.elementType, new string[]
			{
				action.descriptiveName,
				action.positiveDescriptiveName,
				action.negativeDescriptiveName,
				action.name,
				"UNKNOWN"
			}.FirstNotNullOrEmpty(null));
			uiButton._label.text = elementMap.elementIdentifierName.ToUpperInvariant();
			uiButton._actionElementMap = elementMap;
			uiButton._button.onClick.Add(new EventDelegate(delegate
			{
				if (!this.enabled && this._nextChangeTimer < Time.realtimeSinceStartup && !this._delayedPollInput && !this._pollInput)
				{
					if ((Input.IsGamePad && Input.GetButtonDown("Rebind")) || (!Input.IsGamePad && UICamera.currentTouchID == -1))
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
						InputActionRow uiRow3;
						(actionRowMappingCount2 = this._actionRowMappingCount)[uiRow3 = uiRow] = actionRowMappingCount2[uiRow3] - 1;
						this.ShowActionMappingCount(uiRow, controllerMap.controllerType);
						this._nextChangeTimer = Time.realtimeSinceStartup + this._interChangeDelay;
						this.InitializeUI(false);
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
			uiButton.transform.parent = uiRow._addButtons[(int)controllerType].transform.parent;
			uiButton.transform.localPosition = Vector3.zero;
			uiButton.transform.localScale = Vector3.one;
			this.HideAddActionMapButton(uiRow, controllerMap.controllerType);
			this._knownActionMaps.Add(elementMap, uiButton);
			Dictionary<InputActionRow, int> actionRowMappingCount;
			InputActionRow uiRow2;
			(actionRowMappingCount = this._actionRowMappingCount)[uiRow2 = uiRow] = actionRowMappingCount[uiRow2] + 1;
			return uiButton;
		}

		
		private void PollControllerForAssignment()
		{
			if (base.enabled)
			{
				if (this._controllerType == ControllerType.Keyboard || this._controllerType == ControllerType.Mouse)
				{
					if (this._pollInput)
					{
						this.PollKeyboardForAssignment();
					}
					if (this._pollInput)
					{
						this.PollMouseForAssignment();
					}
				}
				if (this._pollInput && this._controllerType == ControllerType.Joystick)
				{
					this.PollJoystickForAssignment();
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
				if (num > 0 && num == 1)
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

		
		public UIScrollView _scrollView;

		
		public UIScrollBar _scrollbar;

		
		public UILabel _selectionScreenTimer;

		
		public UILabel _mappingConflictResolutionKeyLabel;

		
		public UILabel _mappingConflictResolutionActionLabel;

		
		public UILabel _mappingSystemConflictUI;

		
		public GameObject _cancelButton;

		
		public float _inputSelectionDuration = 5f;

		
		public float _interChangeDelay = 0.5f;

		
		public ActionTriggerEvent[] _actionTriggerEventToLock;

		
		private bool _delayedPollInput;

		
		private bool _pollInput;

		
		private bool _replaceElementMap;

		
		private ControllerType _controllerType;

		
		private InputMappingAction.ElementAssignmentChange _entry;

		
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
