using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rewired;
using Rewired.ControllerExtensions;
using TheForest.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheForest.Utils
{
	public class Input : MonoBehaviour
	{
		public static bool UsingDualshock { get; private set; }

		private void Awake()
		{
			if (Input.player == null)
			{
				Input.player = ReInput.players.GetPlayer(0);
				foreach (Rewired.Joystick joystick in Input.player.controllers.Joysticks)
				{
					Input.DS4 = joystick.GetExtension<DualShock4Extension>();
				}
				SceneManager.activeSceneChanged += this.SceneManager_activeSceneChanged;
			}
			else
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		private void LateUpdate()
		{
			if (ReInput.isReady)
			{
				Controller lastActiveController = Input.player.controllers.GetLastActiveController();
				if (lastActiveController != null)
				{
					Input.WasGamePad = Input.IsGamePad;
					bool flag = lastActiveController != Input.player.controllers.Keyboard && lastActiveController != Input.player.controllers.Mouse;
					Input.IsGamePad = flag;
					this.isGamePad = flag;
				}
				if (lastActiveController != this.prevController)
				{
					Input.DS4 = ((!Input.IsGamePad || lastActiveController == null) ? null : lastActiveController.GetExtension<DualShock4Extension>());
					Input.UsingDualshock = (Input.IsGamePad && lastActiveController != null && lastActiveController.name.Contains("DualShock"));
					this.prevController = lastActiveController;
				}
			}
			if (Input.DelayedActionIsDown && !Input.DelayedActionWasUpdated)
			{
				Input.GetButtonAfterDelay(Input.DelayedActionName, 0.5f, false);
			}
		}

		private void OnDestroy()
		{
			Input.player = null;
		}

		private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
		{
			int buildIndex = arg1.buildIndex;
			if (buildIndex != 1 && buildIndex != 3)
			{
				Input.InMenuScene = false;
			}
			else
			{
				Input.InMenuScene = true;
			}
		}

		public static bool GetState(InputState state)
		{
			return Input.States != null && Input.States.ContainsKey(state) && Input.States[state];
		}

		public static void SetState(InputState state, bool enabled)
		{
			if (!Input.States.ContainsKey(InputState.World))
			{
				Input.States.Add(InputState.World, false);
			}
			if (Input.GetState(state) == enabled)
			{
				return;
			}
			if (!Input.States.ContainsKey(state))
			{
				Input.States.Add(state, false);
			}
			Input.States[state] = enabled;
			if (!Input.GetState(InputState.World))
			{
				bool flag = false;
				foreach (KeyValuePair<InputState, bool> keyValuePair in Input.States)
				{
					if (keyValuePair.Value)
					{
						flag = true;
					}
				}
				if (!flag)
				{
					Input.States[InputState.World] = true;
				}
			}
			string text = "Input States: ";
			foreach (KeyValuePair<InputState, bool> keyValuePair2 in Input.States)
			{
				string text2 = text;
				text = string.Concat(new object[]
				{
					text2,
					keyValuePair2.Key,
					" : ",
					keyValuePair2.Value,
					"\n"
				});
			}
			Debug.Log(text);
			Input.ForceRefreshState();
		}

		public static void ForceRefreshState()
		{
			if (ForestVR.Enabled)
			{
				VRControllerDisplayManager.ForceRefresh();
			}
			if (Input.GetState(InputState.SavingMaps))
			{
				Input.SetMappingExclusive(KeyMapCategory.Default);
				InputMappingIcons.RefreshMappings();
				return;
			}
			if (Input.GetState(InputState.Locked))
			{
				Input.SetMappingExclusive(KeyMapCategory.None);
				InputMappingIcons.RefreshMappings();
				return;
			}
			if (Input.GetState(InputState.Chat))
			{
				Input.SetMappingExclusive(KeyMapCategory.Chat);
				InputMappingIcons.RefreshMappings();
				return;
			}
			if (Input.GetState(InputState.Menu))
			{
				Input.SetMappingExclusive(KeyMapCategory.Menu);
				InputMappingIcons.RefreshMappings();
				return;
			}
			if (Input.GetState(InputState.RadialWorld))
			{
				Input.SetMappingExclusive(KeyMapCategory.RadialWorld);
				InputMappingIcons.RefreshMappings();
				return;
			}
			if (Input.GetState(InputState.Book))
			{
				Input.SetMappingExclusive(KeyMapCategory.Book);
				InputMappingIcons.RefreshMappings();
				return;
			}
			if (Input.GetState(InputState.Inventory))
			{
				Input.SetMappingExclusive(KeyMapCategory.Inventory);
				InputMappingIcons.RefreshMappings();
				return;
			}
			Input.SetMappingExclusive(KeyMapCategory.Default);
			InputMappingIcons.RefreshMappings();
		}

		private static void SetMappingExclusive(KeyMapCategory exclusiveCategory)
		{
			ControllerType[] array = new ControllerType[3];
			RuntimeHelpers.InitializeArray(array, fieldof(<PrivateImplementationDetails>.$field-3101DD90CA740F799946B28E93662E644AFCB518).FieldHandle);
			ControllerType[] types = array;
			Input.SetMapping(KeyMapCategory.Chat, types, exclusiveCategory == KeyMapCategory.Chat);
			Input.SetMapping(KeyMapCategory.Default, types, exclusiveCategory == KeyMapCategory.Default);
			Input.SetMapping(KeyMapCategory.Menu, types, exclusiveCategory == KeyMapCategory.Menu);
			Input.SetMapping(KeyMapCategory.Inventory, types, exclusiveCategory == KeyMapCategory.Inventory);
			Input.SetMapping(KeyMapCategory.Book, types, exclusiveCategory == KeyMapCategory.Book);
			Input.SetMapping(KeyMapCategory.RadialWorld, types, exclusiveCategory == KeyMapCategory.RadialWorld);
		}

		public static bool IsGamePad { get; private set; }

		public static bool WasGamePad { get; private set; }

		public static bool IsMouseLocked { get; private set; }

		public static bool anyKeyDown
		{
			get
			{
				return Input.player.controllers.Keyboard.GetAnyButton();
			}
		}

		public static bool GetButtonDown(string button)
		{
			return Input.player.GetButtonDown(button);
		}

		public static bool GetButtonUp(string button)
		{
			return Input.player.GetButtonUp(button);
		}

		public static bool GetButtonPress(string button)
		{
			return Input.player.GetButtonUp(button) && Input.player.GetButtonTimePressed(button) < 0.2f;
		}

		public static bool IsPastButtonPress(string button)
		{
			return Input.player.GetButtonTimePressed(button) > 0.2f;
		}

		public static bool GetKeyDown(KeyCode key)
		{
			return Input.GetKeyDown(key);
		}

		public static bool GetKeyUp(KeyCode key)
		{
			return Input.GetKeyUp(key);
		}

		public static bool GetKey(KeyCode key)
		{
			return Input.GetKey(key);
		}

		public static bool GetMouseButtonDown(int index)
		{
			return Input.GetMouseButtonDown(index);
		}

		public static bool GetMouseButtonUp(int index)
		{
			return Input.GetMouseButtonUp(index);
		}

		public static bool GetMouseButton(int index)
		{
			return Input.GetMouseButton(index);
		}

		public static bool GetButton(string button)
		{
			return Input.player.GetButton(button);
		}

		public static bool GetButtonAfterDelay(string button, float delay, bool autoRepeat = false)
		{
			if (!Input.DelayedActionIsDown)
			{
				if ((!autoRepeat) ? Input.player.GetButtonDown(button) : Input.player.GetButton(button))
				{
					Input.DelayedActionStartTime = Time.realtimeSinceStartup;
					Input.DelayedActionAlpha = 0f;
					Input.DelayedActionIsDown = true;
					Input.DelayedActionWasUpdated = true;
					Input.DelayedActionName = button;
				}
			}
			else
			{
				if (Input.DelayedActionName != button)
				{
					return false;
				}
				if (Input.player.GetButton(button) && Input.DelayedActionAlpha < 1f)
				{
					Input.DelayedActionAlpha = Mathf.Clamp01((Time.realtimeSinceStartup - Input.DelayedActionStartTime) / delay);
					if (Mathf.Approximately(Input.DelayedActionAlpha, 1f))
					{
						Input.DelayedActionAlpha = 0f;
						Input.DelayedActionIsDown = false;
						Input.DelayedActionWasUpdated = true;
						return true;
					}
					Input.DelayedActionWasUpdated = true;
				}
				else if (Input.DelayedActionWasUpdated)
				{
					Input.DelayedActionWasUpdated = false;
				}
				else
				{
					Input.DelayedActionAlpha = 0f;
					Input.DelayedActionIsDown = false;
					Input.DelayedActionWasUpdated = false;
				}
			}
			return false;
		}

		public static void ResetDelayedAction()
		{
			Input.DelayedActionAlpha = 0f;
			Input.DelayedActionIsDown = false;
			Input.DelayedActionWasUpdated = false;
			Input.DelayedActionName = string.Empty;
		}

		private static void SetMapping(KeyMapCategory category, IEnumerable<ControllerType> types, bool enabled)
		{
			if (types == null)
			{
				return;
			}
			foreach (ControllerType type in types)
			{
				Input.SetMapping(category, type, enabled);
			}
		}

		private static void SetMapping(KeyMapCategory category, ControllerType type, bool enabled)
		{
			if (Input.player == null || Input.player.controllers == null || Input.player.controllers.maps == null)
			{
				return;
			}
			Input.player.controllers.maps.SetMapsEnabled(enabled, type, category.ToString());
		}

		public static void LogDebugInfo()
		{
			Debug.Log(string.Concat(new object[]
			{
				"DelayedActionStartTime = ",
				Input.DelayedActionStartTime,
				"DelayedActionAlpha = ",
				Input.DelayedActionAlpha,
				"DelayedActionIsDown = ",
				Input.DelayedActionIsDown,
				"DelayedActionWasUpdated = ",
				Input.DelayedActionWasUpdated,
				"DelayedActionName = ",
				Input.DelayedActionName
			}));
		}

		public static float GetAxis(string axis)
		{
			return Input.player.GetAxis(axis);
		}

		public static float GetAxisDown(string axis)
		{
			return (Input.player.GetAxisPrev(axis) != 0f) ? 0f : Input.player.GetAxis(axis);
		}

		public static Vector3 mousePosition
		{
			get
			{
				if (ForestVR.Enabled && LocalPlayer.IsInInventory)
				{
					return LocalPlayer.InventoryCam.WorldToScreenPoint(LocalPlayer.RightHandTrVR.position);
				}
				if (ForestVR.Enabled && LocalPlayer.IsInBook)
				{
					return LocalPlayer.MainCam.WorldToScreenPoint(LocalPlayer.RightHandTrVR.position);
				}
				if (ForestVR.Enabled && (LocalPlayer.IsInPauseMenu || Input.InMenuScene))
				{
					return Input.VRMouseScreenPos;
				}
				if (VirtualCursor.Instance && VirtualCursor.Instance.UseVirtualPosition)
				{
					return VirtualCursor.Instance.Position;
				}
				if (Input.player != null && Input.player.controllers != null && Input.player.controllers.Mouse != null)
				{
					return Input.player.controllers.Mouse.screenPosition;
				}
				return Input.mousePosition;
			}
		}

		public static void LockMouse()
		{
			if (!CoopPeerStarter.DedicatedHost)
			{
				Input.IsMouseLocked = true;
			}
		}

		public static void UnLockMouse()
		{
			Input.IsMouseLocked = false;
		}

		public static int touchCount
		{
			get
			{
				return Input.touchCount;
			}
		}

		public static Vector3 VRMouseScreenPos { get; internal set; }

		public static Touch GetTouch(int index)
		{
			return Input.GetTouch(index);
		}

		public bool isGamePad;

		private Controller prevController;

		public static Player player = null;

		public static DualShock4Extension DS4;

		public static float DelayedActionStartTime = 0f;

		public static float DelayedActionAlpha;

		public static string DelayedActionName = string.Empty;

		public static bool DelayedActionIsDown = false;

		public static bool DelayedActionWasUpdated = false;

		private static bool InMenuScene;

		public static Dictionary<InputState, bool> States = new Dictionary<InputState, bool>();
	}
}
