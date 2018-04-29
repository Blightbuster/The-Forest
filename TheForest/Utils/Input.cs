using System;
using Rewired;
using Rewired.ControllerExtensions;
using TheForest.Items.Inventory;
using TheForest.UI;
using UnityEngine;

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

		
		public static void UpdateControlMapping()
		{
			if (Input.player == null)
			{
				return;
			}
			if (LocalPlayer.Inventory == null)
			{
				return;
			}
			PlayerInventory.PlayerViews currentView = LocalPlayer.Inventory.CurrentView;
			if (currentView != PlayerInventory.PlayerViews.Pause)
			{
				Input.SetMenuMapping(false);
			}
			else
			{
				Input.SetMenuMapping(true);
			}
		}

		
		public static void SetDefaultMapping(bool enabled)
		{
			Input.player.controllers.maps.SetMapsEnabled(enabled, ControllerType.Keyboard, "Default");
			Input.player.controllers.maps.SetMapsEnabled(enabled, ControllerType.Mouse, "Default");
			Input.player.controllers.maps.SetMapsEnabled(enabled, ControllerType.Joystick, "Default");
		}

		
		public static void SetMenuMapping(bool enabled)
		{
			Input.player.controllers.maps.SetMapsEnabled(enabled, ControllerType.Keyboard, "Menu");
			Input.player.controllers.maps.SetMapsEnabled(enabled, ControllerType.Joystick, "Menu");
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

		
		public static Touch GetTouch(int index)
		{
			return Input.GetTouch(index);
		}

		
		private const string MenuInputMap = "Menu";

		
		private const string DefaultInputMap = "Default";

		
		public bool isGamePad;

		
		private Controller prevController;

		
		public static Player player;

		
		public static DualShock4Extension DS4;

		
		public static float DelayedActionStartTime;

		
		public static float DelayedActionAlpha;

		
		public static string DelayedActionName = string.Empty;

		
		public static bool DelayedActionIsDown;

		
		public static bool DelayedActionWasUpdated;
	}
}
