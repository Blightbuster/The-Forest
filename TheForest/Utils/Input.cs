using System;
using Rewired;
using Rewired.ControllerExtensions;
using TheForest.UI;
using UnityEngine;

namespace TheForest.Utils
{
	
	public class Input : MonoBehaviour
	{
		
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
					Input.DS4 = lastActiveController.GetExtension<DualShock4Extension>();
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
