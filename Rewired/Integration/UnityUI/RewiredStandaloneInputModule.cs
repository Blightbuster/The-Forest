using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Rewired.Integration.UnityUI
{
	
	[AddComponentMenu("Event/Rewired Standalone Input Module")]
	public class RewiredStandaloneInputModule : PointerInputModule
	{
		
		protected RewiredStandaloneInputModule()
		{
		}

		
		
		
		public bool UseAllRewiredGamePlayers
		{
			get
			{
				return this.useAllRewiredGamePlayers;
			}
			set
			{
				bool flag = value != this.useAllRewiredGamePlayers;
				this.useAllRewiredGamePlayers = value;
				if (flag)
				{
					this.SetupRewiredVars();
				}
			}
		}

		
		
		
		public bool UseRewiredSystemPlayer
		{
			get
			{
				return this.useRewiredSystemPlayer;
			}
			set
			{
				bool flag = value != this.useRewiredSystemPlayer;
				this.useRewiredSystemPlayer = value;
				if (flag)
				{
					this.SetupRewiredVars();
				}
			}
		}

		
		
		
		public int[] RewiredPlayerIds
		{
			get
			{
				return (int[])this.rewiredPlayerIds.Clone();
			}
			set
			{
				this.rewiredPlayerIds = ((value == null) ? new int[0] : ((int[])value.Clone()));
				this.SetupRewiredVars();
			}
		}

		
		
		
		public bool UsePlayingPlayersOnly
		{
			get
			{
				return this.usePlayingPlayersOnly;
			}
			set
			{
				this.usePlayingPlayersOnly = value;
			}
		}

		
		
		
		public bool MoveOneElementPerAxisPress
		{
			get
			{
				return this.moveOneElementPerAxisPress;
			}
			set
			{
				this.moveOneElementPerAxisPress = value;
			}
		}

		
		
		
		public bool allowMouseInput
		{
			get
			{
				return this.m_allowMouseInput;
			}
			set
			{
				this.m_allowMouseInput = value;
			}
		}

		
		
		
		public bool allowMouseInputIfTouchSupported
		{
			get
			{
				return this.m_allowMouseInputIfTouchSupported;
			}
			set
			{
				this.m_allowMouseInputIfTouchSupported = value;
			}
		}

		
		
		private bool isMouseSupported
		{
			get
			{
				return Input.mousePresent && this.m_allowMouseInput && (!this.isTouchSupported || this.m_allowMouseInputIfTouchSupported);
			}
		}

		
		
		
		[Obsolete("allowActivationOnMobileDevice has been deprecated. Use forceModuleActive instead")]
		public bool allowActivationOnMobileDevice
		{
			get
			{
				return this.m_ForceModuleActive;
			}
			set
			{
				this.m_ForceModuleActive = value;
			}
		}

		
		
		
		public bool forceModuleActive
		{
			get
			{
				return this.m_ForceModuleActive;
			}
			set
			{
				this.m_ForceModuleActive = value;
			}
		}

		
		
		
		public float inputActionsPerSecond
		{
			get
			{
				return this.m_InputActionsPerSecond;
			}
			set
			{
				this.m_InputActionsPerSecond = value;
			}
		}

		
		
		
		public float repeatDelay
		{
			get
			{
				return this.m_RepeatDelay;
			}
			set
			{
				this.m_RepeatDelay = value;
			}
		}

		
		
		
		public string horizontalAxis
		{
			get
			{
				return this.m_HorizontalAxis;
			}
			set
			{
				this.m_HorizontalAxis = value;
			}
		}

		
		
		
		public string verticalAxis
		{
			get
			{
				return this.m_VerticalAxis;
			}
			set
			{
				this.m_VerticalAxis = value;
			}
		}

		
		
		
		public string submitButton
		{
			get
			{
				return this.m_SubmitButton;
			}
			set
			{
				this.m_SubmitButton = value;
			}
		}

		
		
		
		public string cancelButton
		{
			get
			{
				return this.m_CancelButton;
			}
			set
			{
				this.m_CancelButton = value;
			}
		}

		
		protected override void Awake()
		{
			base.Awake();
			this.isTouchSupported = Input.touchSupported;
			TouchInputModule component = base.GetComponent<TouchInputModule>();
			if (component != null)
			{
				component.enabled = false;
			}
			this.InitializeRewired();
		}

		
		public override void UpdateModule()
		{
			this.CheckEditorRecompile();
			if (this.recompiling)
			{
				return;
			}
			if (!ReInput.isReady)
			{
				return;
			}
			if (!this.m_HasFocus && this.ShouldIgnoreEventsOnNoFocus())
			{
				return;
			}
			if (this.isMouseSupported)
			{
				this.m_LastMousePosition = this.m_MousePosition;
				this.m_MousePosition = Input.mousePosition;
			}
		}

		
		public override bool IsModuleSupported()
		{
			return true;
		}

		
		public override bool ShouldActivateModule()
		{
			if (!base.ShouldActivateModule())
			{
				return false;
			}
			if (this.recompiling)
			{
				return false;
			}
			if (!ReInput.isReady)
			{
				return false;
			}
			bool flag = this.m_ForceModuleActive;
			for (int i = 0; i < this.playerIds.Length; i++)
			{
				Player player = ReInput.players.GetPlayer(this.playerIds[i]);
				if (player != null)
				{
					if (!this.usePlayingPlayersOnly || player.isPlaying)
					{
						flag |= player.GetButtonDown(this.m_SubmitButton);
						flag |= player.GetButtonDown(this.m_CancelButton);
						if (this.moveOneElementPerAxisPress)
						{
							flag |= (player.GetButtonDown(this.m_HorizontalAxis) || player.GetNegativeButtonDown(this.m_HorizontalAxis));
							flag |= (player.GetButtonDown(this.m_VerticalAxis) || player.GetNegativeButtonDown(this.m_VerticalAxis));
						}
						else
						{
							flag |= !Mathf.Approximately(player.GetAxisRaw(this.m_HorizontalAxis), 0f);
							flag |= !Mathf.Approximately(player.GetAxisRaw(this.m_VerticalAxis), 0f);
						}
					}
				}
			}
			if (this.isMouseSupported)
			{
				flag |= ((this.m_MousePosition - this.m_LastMousePosition).sqrMagnitude > 0f);
				flag |= Input.GetMouseButtonDown(0);
			}
			if (this.isTouchSupported)
			{
				for (int j = 0; j < Input.touchCount; j++)
				{
					Touch touch = Input.GetTouch(j);
					flag |= (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary);
				}
			}
			return flag;
		}

		
		public override void ActivateModule()
		{
			if (!this.m_HasFocus && this.ShouldIgnoreEventsOnNoFocus())
			{
				return;
			}
			base.ActivateModule();
			if (this.isMouseSupported)
			{
				Vector2 vector = Input.mousePosition;
				this.m_MousePosition = vector;
				this.m_LastMousePosition = vector;
			}
			GameObject gameObject = base.eventSystem.currentSelectedGameObject;
			if (gameObject == null)
			{
				gameObject = base.eventSystem.firstSelectedGameObject;
			}
			base.eventSystem.SetSelectedGameObject(gameObject, this.GetBaseEventData());
		}

		
		public override void DeactivateModule()
		{
			base.DeactivateModule();
			base.ClearSelection();
		}

		
		public override void Process()
		{
			if (!ReInput.isReady)
			{
				return;
			}
			if (!this.m_HasFocus && this.ShouldIgnoreEventsOnNoFocus())
			{
				return;
			}
			bool flag = this.SendUpdateEventToSelectedObject();
			if (base.eventSystem.sendNavigationEvents)
			{
				if (!flag)
				{
					flag |= this.SendMoveEventToSelectedObject();
				}
				if (!flag)
				{
					this.SendSubmitEventToSelectedObject();
				}
			}
			if (!this.ProcessTouchEvents() && this.isMouseSupported)
			{
				this.ProcessMouseEvent();
			}
		}

		
		private bool ProcessTouchEvents()
		{
			if (!this.isTouchSupported)
			{
				return false;
			}
			for (int i = 0; i < Input.touchCount; i++)
			{
				Touch touch = Input.GetTouch(i);
				if (touch.type != TouchType.Indirect)
				{
					bool pressed;
					bool flag;
					PointerEventData touchPointerEventData = base.GetTouchPointerEventData(touch, out pressed, out flag);
					this.ProcessTouchPress(touchPointerEventData, pressed, flag);
					if (!flag)
					{
						this.ProcessMove(touchPointerEventData);
						this.ProcessDrag(touchPointerEventData);
					}
					else
					{
						base.RemovePointerData(touchPointerEventData);
					}
				}
			}
			return Input.touchCount > 0;
		}

		
		private void ProcessTouchPress(PointerEventData pointerEvent, bool pressed, bool released)
		{
			GameObject gameObject = pointerEvent.pointerCurrentRaycast.gameObject;
			if (pressed)
			{
				pointerEvent.eligibleForClick = true;
				pointerEvent.delta = Vector2.zero;
				pointerEvent.dragging = false;
				pointerEvent.useDragThreshold = true;
				pointerEvent.pressPosition = pointerEvent.position;
				pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;
				base.DeselectIfSelectionChanged(gameObject, pointerEvent);
				if (pointerEvent.pointerEnter != gameObject)
				{
					base.HandlePointerExitAndEnter(pointerEvent, gameObject);
					pointerEvent.pointerEnter = gameObject;
				}
				GameObject gameObject2 = ExecuteEvents.ExecuteHierarchy<IPointerDownHandler>(gameObject, pointerEvent, ExecuteEvents.pointerDownHandler);
				if (gameObject2 == null)
				{
					gameObject2 = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
				}
				float unscaledTime = Time.unscaledTime;
				if (gameObject2 == pointerEvent.lastPress)
				{
					float num = unscaledTime - pointerEvent.clickTime;
					if (num < 0.3f)
					{
						pointerEvent.clickCount++;
					}
					else
					{
						pointerEvent.clickCount = 1;
					}
					pointerEvent.clickTime = unscaledTime;
				}
				else
				{
					pointerEvent.clickCount = 1;
				}
				pointerEvent.pointerPress = gameObject2;
				pointerEvent.rawPointerPress = gameObject;
				pointerEvent.clickTime = unscaledTime;
				pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject);
				if (pointerEvent.pointerDrag != null)
				{
					ExecuteEvents.Execute<IInitializePotentialDragHandler>(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
				}
			}
			if (released)
			{
				ExecuteEvents.Execute<IPointerUpHandler>(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
				GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
				if (pointerEvent.pointerPress == eventHandler && pointerEvent.eligibleForClick)
				{
					ExecuteEvents.Execute<IPointerClickHandler>(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
				}
				else if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
				{
					ExecuteEvents.ExecuteHierarchy<IDropHandler>(gameObject, pointerEvent, ExecuteEvents.dropHandler);
				}
				pointerEvent.eligibleForClick = false;
				pointerEvent.pointerPress = null;
				pointerEvent.rawPointerPress = null;
				if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
				{
					ExecuteEvents.Execute<IEndDragHandler>(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
				}
				pointerEvent.dragging = false;
				pointerEvent.pointerDrag = null;
				if (pointerEvent.pointerDrag != null)
				{
					ExecuteEvents.Execute<IEndDragHandler>(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
				}
				pointerEvent.pointerDrag = null;
				ExecuteEvents.ExecuteHierarchy<IPointerExitHandler>(pointerEvent.pointerEnter, pointerEvent, ExecuteEvents.pointerExitHandler);
				pointerEvent.pointerEnter = null;
			}
		}

		
		protected bool SendSubmitEventToSelectedObject()
		{
			if (base.eventSystem.currentSelectedGameObject == null)
			{
				return false;
			}
			if (this.recompiling)
			{
				return false;
			}
			BaseEventData baseEventData = this.GetBaseEventData();
			for (int i = 0; i < this.playerIds.Length; i++)
			{
				Player player = ReInput.players.GetPlayer(this.playerIds[i]);
				if (player != null)
				{
					if (!this.usePlayingPlayersOnly || player.isPlaying)
					{
						if (player.GetButtonDown(this.m_SubmitButton))
						{
							ExecuteEvents.Execute<ISubmitHandler>(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.submitHandler);
							break;
						}
						if (player.GetButtonDown(this.m_CancelButton))
						{
							ExecuteEvents.Execute<ICancelHandler>(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.cancelHandler);
							break;
						}
					}
				}
			}
			return baseEventData.used;
		}

		
		private Vector2 GetRawMoveVector()
		{
			if (this.recompiling)
			{
				return Vector2.zero;
			}
			Vector2 zero = Vector2.zero;
			bool flag = false;
			bool flag2 = false;
			for (int i = 0; i < this.playerIds.Length; i++)
			{
				Player player = ReInput.players.GetPlayer(this.playerIds[i]);
				if (player != null)
				{
					if (!this.usePlayingPlayersOnly || player.isPlaying)
					{
						if (this.moveOneElementPerAxisPress)
						{
							float num = 0f;
							if (player.GetButtonDown(this.m_HorizontalAxis))
							{
								num = 1f;
							}
							else if (player.GetNegativeButtonDown(this.m_HorizontalAxis))
							{
								num = -1f;
							}
							float num2 = 0f;
							if (player.GetButtonDown(this.m_VerticalAxis))
							{
								num2 = 1f;
							}
							else if (player.GetNegativeButtonDown(this.m_VerticalAxis))
							{
								num2 = -1f;
							}
							zero.x += num;
							zero.y += num2;
						}
						else
						{
							zero.x += player.GetAxisRaw(this.m_HorizontalAxis);
							zero.y += player.GetAxisRaw(this.m_VerticalAxis);
						}
						flag |= (player.GetButtonDown(this.m_HorizontalAxis) || player.GetNegativeButtonDown(this.m_HorizontalAxis));
						flag2 |= (player.GetButtonDown(this.m_VerticalAxis) || player.GetNegativeButtonDown(this.m_VerticalAxis));
					}
				}
			}
			if (flag)
			{
				if (zero.x < 0f)
				{
					zero.x = -1f;
				}
				if (zero.x > 0f)
				{
					zero.x = 1f;
				}
			}
			if (flag2)
			{
				if (zero.y < 0f)
				{
					zero.y = -1f;
				}
				if (zero.y > 0f)
				{
					zero.y = 1f;
				}
			}
			return zero;
		}

		
		protected bool SendMoveEventToSelectedObject()
		{
			if (this.recompiling)
			{
				return false;
			}
			float unscaledTime = Time.unscaledTime;
			Vector2 rawMoveVector = this.GetRawMoveVector();
			if (Mathf.Approximately(rawMoveVector.x, 0f) && Mathf.Approximately(rawMoveVector.y, 0f))
			{
				this.m_ConsecutiveMoveCount = 0;
				return false;
			}
			bool flag = Vector2.Dot(rawMoveVector, this.m_LastMoveVector) > 0f;
			bool flag2 = this.CheckButtonOrKeyMovement(unscaledTime);
			bool flag3 = flag2;
			if (!flag3)
			{
				if (this.m_RepeatDelay > 0f)
				{
					if (flag && this.m_ConsecutiveMoveCount == 1)
					{
						flag3 = (unscaledTime > this.m_PrevActionTime + this.m_RepeatDelay);
					}
					else
					{
						flag3 = (unscaledTime > this.m_PrevActionTime + 1f / this.m_InputActionsPerSecond);
					}
				}
				else
				{
					flag3 = (unscaledTime > this.m_PrevActionTime + 1f / this.m_InputActionsPerSecond);
				}
			}
			if (!flag3)
			{
				return false;
			}
			AxisEventData axisEventData = this.GetAxisEventData(rawMoveVector.x, rawMoveVector.y, 0.6f);
			if (axisEventData.moveDir == MoveDirection.None)
			{
				return false;
			}
			ExecuteEvents.Execute<IMoveHandler>(base.eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
			if (!flag)
			{
				this.m_ConsecutiveMoveCount = 0;
			}
			this.m_ConsecutiveMoveCount++;
			this.m_PrevActionTime = unscaledTime;
			this.m_LastMoveVector = rawMoveVector;
			return axisEventData.used;
		}

		
		private bool CheckButtonOrKeyMovement(float time)
		{
			bool flag = false;
			for (int i = 0; i < this.playerIds.Length; i++)
			{
				Player player = ReInput.players.GetPlayer(this.playerIds[i]);
				if (player != null)
				{
					if (!this.usePlayingPlayersOnly || player.isPlaying)
					{
						flag |= (player.GetButtonDown(this.m_HorizontalAxis) || player.GetNegativeButtonDown(this.m_HorizontalAxis));
						flag |= (player.GetButtonDown(this.m_VerticalAxis) || player.GetNegativeButtonDown(this.m_VerticalAxis));
					}
				}
			}
			return flag;
		}

		
		protected void ProcessMouseEvent()
		{
			this.ProcessMouseEvent(0);
		}

		
		protected void ProcessMouseEvent(int id)
		{
			PointerInputModule.MouseState mousePointerEventData = this.GetMousePointerEventData(id);
			PointerInputModule.MouseButtonEventData eventData = mousePointerEventData.GetButtonState(PointerEventData.InputButton.Left).eventData;
			this.ProcessMousePress(eventData);
			this.ProcessMove(eventData.buttonData);
			this.ProcessDrag(eventData.buttonData);
			this.ProcessMousePress(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Right).eventData);
			this.ProcessDrag(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
			this.ProcessMousePress(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Middle).eventData);
			this.ProcessDrag(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);
			if (!Mathf.Approximately(eventData.buttonData.scrollDelta.sqrMagnitude, 0f))
			{
				GameObject eventHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(eventData.buttonData.pointerCurrentRaycast.gameObject);
				ExecuteEvents.ExecuteHierarchy<IScrollHandler>(eventHandler, eventData.buttonData, ExecuteEvents.scrollHandler);
			}
		}

		
		protected bool SendUpdateEventToSelectedObject()
		{
			if (base.eventSystem.currentSelectedGameObject == null)
			{
				return false;
			}
			BaseEventData baseEventData = this.GetBaseEventData();
			ExecuteEvents.Execute<IUpdateSelectedHandler>(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.updateSelectedHandler);
			return baseEventData.used;
		}

		
		protected void ProcessMousePress(PointerInputModule.MouseButtonEventData data)
		{
			PointerEventData buttonData = data.buttonData;
			GameObject gameObject = buttonData.pointerCurrentRaycast.gameObject;
			if (data.PressedThisFrame())
			{
				buttonData.eligibleForClick = true;
				buttonData.delta = Vector2.zero;
				buttonData.dragging = false;
				buttonData.useDragThreshold = true;
				buttonData.pressPosition = buttonData.position;
				buttonData.pointerPressRaycast = buttonData.pointerCurrentRaycast;
				base.DeselectIfSelectionChanged(gameObject, buttonData);
				GameObject gameObject2 = ExecuteEvents.ExecuteHierarchy<IPointerDownHandler>(gameObject, buttonData, ExecuteEvents.pointerDownHandler);
				if (gameObject2 == null)
				{
					gameObject2 = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
				}
				float unscaledTime = Time.unscaledTime;
				if (gameObject2 == buttonData.lastPress)
				{
					float num = unscaledTime - buttonData.clickTime;
					if (num < 0.3f)
					{
						buttonData.clickCount++;
					}
					else
					{
						buttonData.clickCount = 1;
					}
					buttonData.clickTime = unscaledTime;
				}
				else
				{
					buttonData.clickCount = 1;
				}
				buttonData.pointerPress = gameObject2;
				buttonData.rawPointerPress = gameObject;
				buttonData.clickTime = unscaledTime;
				buttonData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject);
				if (buttonData.pointerDrag != null)
				{
					ExecuteEvents.Execute<IInitializePotentialDragHandler>(buttonData.pointerDrag, buttonData, ExecuteEvents.initializePotentialDrag);
				}
			}
			if (data.ReleasedThisFrame())
			{
				ExecuteEvents.Execute<IPointerUpHandler>(buttonData.pointerPress, buttonData, ExecuteEvents.pointerUpHandler);
				GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
				if (buttonData.pointerPress == eventHandler && buttonData.eligibleForClick)
				{
					ExecuteEvents.Execute<IPointerClickHandler>(buttonData.pointerPress, buttonData, ExecuteEvents.pointerClickHandler);
				}
				else if (buttonData.pointerDrag != null && buttonData.dragging)
				{
					ExecuteEvents.ExecuteHierarchy<IDropHandler>(gameObject, buttonData, ExecuteEvents.dropHandler);
				}
				buttonData.eligibleForClick = false;
				buttonData.pointerPress = null;
				buttonData.rawPointerPress = null;
				if (buttonData.pointerDrag != null && buttonData.dragging)
				{
					ExecuteEvents.Execute<IEndDragHandler>(buttonData.pointerDrag, buttonData, ExecuteEvents.endDragHandler);
				}
				buttonData.dragging = false;
				buttonData.pointerDrag = null;
				if (gameObject != buttonData.pointerEnter)
				{
					base.HandlePointerExitAndEnter(buttonData, null);
					base.HandlePointerExitAndEnter(buttonData, gameObject);
				}
			}
		}

		
		protected virtual void OnApplicationFocus(bool hasFocus)
		{
			this.m_HasFocus = hasFocus;
		}

		
		private bool ShouldIgnoreEventsOnNoFocus()
		{
			return !ReInput.isReady || ReInput.configuration.ignoreInputWhenAppNotInFocus;
		}

		
		private void InitializeRewired()
		{
			if (!ReInput.isReady)
			{
				Debug.LogError("Rewired is not initialized! Are you missing a Rewired Input Manager in your scene?");
				return;
			}
			ReInput.EditorRecompileEvent += this.OnEditorRecompile;
			this.SetupRewiredVars();
		}

		
		private void SetupRewiredVars()
		{
			if (this.useAllRewiredGamePlayers)
			{
				IList<Player> list = (!this.useRewiredSystemPlayer) ? ReInput.players.Players : ReInput.players.AllPlayers;
				this.playerIds = new int[list.Count];
				for (int i = 0; i < list.Count; i++)
				{
					this.playerIds[i] = list[i].id;
				}
			}
			else
			{
				int num = this.rewiredPlayerIds.Length + ((!this.useRewiredSystemPlayer) ? 0 : 1);
				this.playerIds = new int[num];
				for (int j = 0; j < this.rewiredPlayerIds.Length; j++)
				{
					this.playerIds[j] = ReInput.players.GetPlayer(this.rewiredPlayerIds[j]).id;
				}
				if (this.useRewiredSystemPlayer)
				{
					this.playerIds[num - 1] = ReInput.players.GetSystemPlayer().id;
				}
			}
		}

		
		private void CheckEditorRecompile()
		{
			if (!this.recompiling)
			{
				return;
			}
			if (!ReInput.isReady)
			{
				return;
			}
			this.recompiling = false;
			this.InitializeRewired();
		}

		
		private void OnEditorRecompile()
		{
			this.recompiling = true;
			this.ClearRewiredVars();
		}

		
		private void ClearRewiredVars()
		{
			Array.Clear(this.playerIds, 0, this.playerIds.Length);
		}

		
		private const string DEFAULT_ACTION_MOVE_HORIZONTAL = "UIHorizontal";

		
		private const string DEFAULT_ACTION_MOVE_VERTICAL = "UIVertical";

		
		private const string DEFAULT_ACTION_SUBMIT = "UISubmit";

		
		private const string DEFAULT_ACTION_CANCEL = "UICancel";

		
		private int[] playerIds;

		
		private bool recompiling;

		
		private bool isTouchSupported;

		
		[SerializeField]
		[Tooltip("Use all Rewired game Players to control the UI. This does not include the System Player. If enabled, this setting overrides individual Player Ids set in Rewired Player Ids.")]
		private bool useAllRewiredGamePlayers;

		
		[SerializeField]
		[Tooltip("Allow the Rewired System Player to control the UI.")]
		private bool useRewiredSystemPlayer;

		
		[SerializeField]
		[Tooltip("A list of Player Ids that are allowed to control the UI. If Use All Rewired Game Players = True, this list will be ignored.")]
		private int[] rewiredPlayerIds = new int[1];

		
		[SerializeField]
		[Tooltip("Allow only Players with Player.isPlaying = true to control the UI.")]
		private bool usePlayingPlayersOnly;

		
		[SerializeField]
		[Tooltip("Makes an axis press always move only one UI selection. Enable if you do not want to allow scrolling through UI elements by holding an axis direction.")]
		private bool moveOneElementPerAxisPress;

		
		private float m_PrevActionTime;

		
		private Vector2 m_LastMoveVector;

		
		private int m_ConsecutiveMoveCount;

		
		private Vector2 m_LastMousePosition;

		
		private Vector2 m_MousePosition;

		
		private bool m_HasFocus = true;

		
		[SerializeField]
		private string m_HorizontalAxis = "UIHorizontal";

		
		[SerializeField]
		[Tooltip("Name of the vertical axis for movement (if axis events are used).")]
		private string m_VerticalAxis = "UIVertical";

		
		[SerializeField]
		[Tooltip("Name of the action used to submit.")]
		private string m_SubmitButton = "UISubmit";

		
		[SerializeField]
		[Tooltip("Name of the action used to cancel.")]
		private string m_CancelButton = "UICancel";

		
		[SerializeField]
		[Tooltip("Number of selection changes allowed per second when a movement button/axis is held in a direction.")]
		private float m_InputActionsPerSecond = 10f;

		
		[SerializeField]
		[Tooltip("Delay in seconds before vertical/horizontal movement starts repeating continouously when a movement direction is held.")]
		private float m_RepeatDelay;

		
		[SerializeField]
		[Tooltip("Allows the mouse to be used to select elements.")]
		private bool m_allowMouseInput = true;

		
		[SerializeField]
		[Tooltip("Allows the mouse to be used to select elements if the device also supports touch control.")]
		private bool m_allowMouseInputIfTouchSupported = true;

		
		[SerializeField]
		[FormerlySerializedAs("m_AllowActivationOnMobileDevice")]
		[Tooltip("Forces the module to always be active.")]
		private bool m_ForceModuleActive;
	}
}
