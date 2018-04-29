using System;
using UnityEngine;


public static class OVRTouchpad
{
	
	
	
	public static event EventHandler TouchHandler;

	
	public static void Create()
	{
	}

	
	public static void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			OVRTouchpad.moveAmountMouse = Input.mousePosition;
			OVRTouchpad.touchState = OVRTouchpad.TouchState.Down;
		}
		else if (Input.GetMouseButtonUp(0))
		{
			OVRTouchpad.moveAmountMouse -= Input.mousePosition;
			OVRTouchpad.HandleInputMouse(ref OVRTouchpad.moveAmountMouse);
			OVRTouchpad.touchState = OVRTouchpad.TouchState.Init;
		}
	}

	
	public static void OnDisable()
	{
	}

	
	private static void HandleInput(OVRTouchpad.TouchState state, ref Vector2 move)
	{
		if (move.magnitude >= OVRTouchpad.minMovMagnitude && OVRTouchpad.touchState != OVRTouchpad.TouchState.Stationary)
		{
			if (OVRTouchpad.touchState == OVRTouchpad.TouchState.Move)
			{
				move.Normalize();
				if (Mathf.Abs(move.x) > Mathf.Abs(move.y))
				{
					if (move.x > 0f)
					{
					}
				}
				else if (move.y > 0f)
				{
				}
			}
		}
	}

	
	private static void HandleInputMouse(ref Vector3 move)
	{
		if (move.magnitude < OVRTouchpad.minMovMagnitudeMouse)
		{
			if (OVRTouchpad.TouchHandler != null)
			{
				OVRTouchpad.TouchHandler(null, new OVRTouchpad.TouchArgs
				{
					TouchType = OVRTouchpad.TouchEvent.SingleTap
				});
			}
		}
		else
		{
			move.Normalize();
			if (Mathf.Abs(move.x) > Mathf.Abs(move.y))
			{
				if (move.x > 0f)
				{
					if (OVRTouchpad.TouchHandler != null)
					{
						OVRTouchpad.TouchHandler(null, new OVRTouchpad.TouchArgs
						{
							TouchType = OVRTouchpad.TouchEvent.Left
						});
					}
				}
				else if (OVRTouchpad.TouchHandler != null)
				{
					OVRTouchpad.TouchHandler(null, new OVRTouchpad.TouchArgs
					{
						TouchType = OVRTouchpad.TouchEvent.Right
					});
				}
			}
			else if (move.y > 0f)
			{
				if (OVRTouchpad.TouchHandler != null)
				{
					OVRTouchpad.TouchHandler(null, new OVRTouchpad.TouchArgs
					{
						TouchType = OVRTouchpad.TouchEvent.Down
					});
				}
			}
			else if (OVRTouchpad.TouchHandler != null)
			{
				OVRTouchpad.TouchHandler(null, new OVRTouchpad.TouchArgs
				{
					TouchType = OVRTouchpad.TouchEvent.Up
				});
			}
		}
	}

	
	private static OVRTouchpad.TouchState touchState = OVRTouchpad.TouchState.Init;

	
	private static float minMovMagnitude = 100f;

	
	private static Vector3 moveAmountMouse;

	
	private static float minMovMagnitudeMouse = 25f;

	
	private static OVRTouchpadHelper touchpadHelper = new GameObject("OVRTouchpadHelper").AddComponent<OVRTouchpadHelper>();

	
	public enum TouchEvent
	{
		
		SingleTap,
		
		Left,
		
		Right,
		
		Up,
		
		Down
	}

	
	public class TouchArgs : EventArgs
	{
		
		public OVRTouchpad.TouchEvent TouchType;
	}

	
	private enum TouchState
	{
		
		Init,
		
		Down,
		
		Stationary,
		
		Move,
		
		Up
	}
}
