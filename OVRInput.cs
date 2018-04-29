using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


public static class OVRInput
{
	
	static OVRInput()
	{
		OVRInput.controllers = new List<OVRInput.OVRControllerBase>
		{
			new OVRInput.OVRControllerGamepadPC(),
			new OVRInput.OVRControllerTouch(),
			new OVRInput.OVRControllerLTouch(),
			new OVRInput.OVRControllerRTouch(),
			new OVRInput.OVRControllerRemote()
		};
	}

	
	
	private static bool pluginSupportsActiveController
	{
		get
		{
			if (!OVRInput._pluginSupportsActiveControllerCached)
			{
				bool flag = true;
				OVRInput._pluginSupportsActiveController = (flag && OVRPlugin.version >= OVRInput._pluginSupportsActiveControllerMinVersion);
				OVRInput._pluginSupportsActiveControllerCached = true;
			}
			return OVRInput._pluginSupportsActiveController;
		}
	}

	
	public static void Update()
	{
		OVRInput.connectedControllerTypes = OVRInput.Controller.None;
		OVRInput.stepType = OVRPlugin.Step.Render;
		OVRInput.fixedUpdateCount = 0;
		for (int i = 0; i < OVRInput.controllers.Count; i++)
		{
			OVRInput.OVRControllerBase ovrcontrollerBase = OVRInput.controllers[i];
			OVRInput.connectedControllerTypes |= ovrcontrollerBase.Update();
			if ((OVRInput.connectedControllerTypes & ovrcontrollerBase.controllerType) != OVRInput.Controller.None)
			{
				OVRInput.RawButton rawMask = ~OVRInput.RawButton.Back;
				OVRInput.RawTouch rawMask2 = OVRInput.RawTouch.Any;
				if (OVRInput.Get(rawMask, ovrcontrollerBase.controllerType) || OVRInput.Get(rawMask2, ovrcontrollerBase.controllerType))
				{
					OVRInput.activeControllerType = ovrcontrollerBase.controllerType;
				}
			}
		}
		if (OVRInput.activeControllerType == OVRInput.Controller.LTouch || OVRInput.activeControllerType == OVRInput.Controller.RTouch)
		{
			OVRInput.activeControllerType = OVRInput.Controller.Touch;
		}
		if ((OVRInput.connectedControllerTypes & OVRInput.activeControllerType) == OVRInput.Controller.None)
		{
			OVRInput.activeControllerType = OVRInput.Controller.None;
		}
		if (OVRInput.activeControllerType == OVRInput.Controller.None)
		{
			if ((OVRInput.connectedControllerTypes & OVRInput.Controller.RTrackedRemote) != OVRInput.Controller.None)
			{
				OVRInput.activeControllerType = OVRInput.Controller.RTrackedRemote;
			}
			else if ((OVRInput.connectedControllerTypes & OVRInput.Controller.LTrackedRemote) != OVRInput.Controller.None)
			{
				OVRInput.activeControllerType = OVRInput.Controller.LTrackedRemote;
			}
		}
		if (OVRInput.pluginSupportsActiveController)
		{
			OVRInput.connectedControllerTypes = (OVRInput.Controller)OVRPlugin.GetConnectedControllers();
			OVRInput.activeControllerType = (OVRInput.Controller)OVRPlugin.GetActiveController();
		}
	}

	
	public static void FixedUpdate()
	{
		OVRInput.stepType = OVRPlugin.Step.Physics;
		double predictionSeconds = (double)OVRInput.fixedUpdateCount * (double)Time.fixedDeltaTime / (double)Mathf.Max(Time.timeScale, 1E-06f);
		OVRInput.fixedUpdateCount++;
		OVRPlugin.UpdateNodePhysicsPoses(0, predictionSeconds);
	}

	
	public static bool GetControllerOrientationTracked(OVRInput.Controller controllerType)
	{
		if (controllerType != OVRInput.Controller.LTouch)
		{
			if (controllerType != OVRInput.Controller.RTouch)
			{
				if (controllerType == OVRInput.Controller.LTrackedRemote)
				{
					goto IL_29;
				}
				if (controllerType != OVRInput.Controller.RTrackedRemote)
				{
					return false;
				}
			}
			return OVRPlugin.GetNodeOrientationTracked(OVRPlugin.Node.HandRight);
		}
		IL_29:
		return OVRPlugin.GetNodeOrientationTracked(OVRPlugin.Node.HandLeft);
	}

	
	public static bool GetControllerPositionTracked(OVRInput.Controller controllerType)
	{
		if (controllerType != OVRInput.Controller.LTouch)
		{
			if (controllerType != OVRInput.Controller.RTouch)
			{
				if (controllerType == OVRInput.Controller.LTrackedRemote)
				{
					goto IL_29;
				}
				if (controllerType != OVRInput.Controller.RTrackedRemote)
				{
					return false;
				}
			}
			return OVRPlugin.GetNodePositionTracked(OVRPlugin.Node.HandRight);
		}
		IL_29:
		return OVRPlugin.GetNodePositionTracked(OVRPlugin.Node.HandLeft);
	}

	
	public static Vector3 GetLocalControllerPosition(OVRInput.Controller controllerType)
	{
		if (controllerType != OVRInput.Controller.LTouch)
		{
			if (controllerType != OVRInput.Controller.RTouch)
			{
				if (controllerType == OVRInput.Controller.LTrackedRemote)
				{
					goto IL_29;
				}
				if (controllerType != OVRInput.Controller.RTrackedRemote)
				{
					return Vector3.zero;
				}
			}
			return OVRPlugin.GetNodePose(OVRPlugin.Node.HandRight, OVRInput.stepType).ToOVRPose().position;
		}
		IL_29:
		return OVRPlugin.GetNodePose(OVRPlugin.Node.HandLeft, OVRInput.stepType).ToOVRPose().position;
	}

	
	public static Vector3 GetLocalControllerVelocity(OVRInput.Controller controllerType)
	{
		if (controllerType != OVRInput.Controller.LTouch)
		{
			if (controllerType != OVRInput.Controller.RTouch)
			{
				if (controllerType == OVRInput.Controller.LTrackedRemote)
				{
					goto IL_29;
				}
				if (controllerType != OVRInput.Controller.RTrackedRemote)
				{
					return Vector3.zero;
				}
			}
			return OVRPlugin.GetNodeVelocity(OVRPlugin.Node.HandRight, OVRInput.stepType).FromFlippedZVector3f();
		}
		IL_29:
		return OVRPlugin.GetNodeVelocity(OVRPlugin.Node.HandLeft, OVRInput.stepType).FromFlippedZVector3f();
	}

	
	public static Vector3 GetLocalControllerAcceleration(OVRInput.Controller controllerType)
	{
		if (controllerType != OVRInput.Controller.LTouch)
		{
			if (controllerType != OVRInput.Controller.RTouch)
			{
				if (controllerType == OVRInput.Controller.LTrackedRemote)
				{
					goto IL_29;
				}
				if (controllerType != OVRInput.Controller.RTrackedRemote)
				{
					return Vector3.zero;
				}
			}
			return OVRPlugin.GetNodeAcceleration(OVRPlugin.Node.HandRight, OVRInput.stepType).FromFlippedZVector3f();
		}
		IL_29:
		return OVRPlugin.GetNodeAcceleration(OVRPlugin.Node.HandLeft, OVRInput.stepType).FromFlippedZVector3f();
	}

	
	public static Quaternion GetLocalControllerRotation(OVRInput.Controller controllerType)
	{
		if (controllerType != OVRInput.Controller.LTouch)
		{
			if (controllerType != OVRInput.Controller.RTouch)
			{
				if (controllerType == OVRInput.Controller.LTrackedRemote)
				{
					goto IL_29;
				}
				if (controllerType != OVRInput.Controller.RTrackedRemote)
				{
					return Quaternion.identity;
				}
			}
			return OVRPlugin.GetNodePose(OVRPlugin.Node.HandRight, OVRInput.stepType).ToOVRPose().orientation;
		}
		IL_29:
		return OVRPlugin.GetNodePose(OVRPlugin.Node.HandLeft, OVRInput.stepType).ToOVRPose().orientation;
	}

	
	public static Vector3 GetLocalControllerAngularVelocity(OVRInput.Controller controllerType)
	{
		if (controllerType != OVRInput.Controller.LTouch)
		{
			if (controllerType != OVRInput.Controller.RTouch)
			{
				if (controllerType == OVRInput.Controller.LTrackedRemote)
				{
					goto IL_29;
				}
				if (controllerType != OVRInput.Controller.RTrackedRemote)
				{
					return Vector3.zero;
				}
			}
			return OVRPlugin.GetNodeAngularVelocity(OVRPlugin.Node.HandRight, OVRInput.stepType).FromFlippedZVector3f();
		}
		IL_29:
		return OVRPlugin.GetNodeAngularVelocity(OVRPlugin.Node.HandLeft, OVRInput.stepType).FromFlippedZVector3f();
	}

	
	public static Vector3 GetLocalControllerAngularAcceleration(OVRInput.Controller controllerType)
	{
		if (controllerType != OVRInput.Controller.LTouch)
		{
			if (controllerType != OVRInput.Controller.RTouch)
			{
				if (controllerType == OVRInput.Controller.LTrackedRemote)
				{
					goto IL_29;
				}
				if (controllerType != OVRInput.Controller.RTrackedRemote)
				{
					return Vector3.zero;
				}
			}
			return OVRPlugin.GetNodeAngularAcceleration(OVRPlugin.Node.HandRight, OVRInput.stepType).FromFlippedZVector3f();
		}
		IL_29:
		return OVRPlugin.GetNodeAngularAcceleration(OVRPlugin.Node.HandLeft, OVRInput.stepType).FromFlippedZVector3f();
	}

	
	public static bool Get(OVRInput.Button virtualMask, OVRInput.Controller controllerMask = OVRInput.Controller.Active)
	{
		return OVRInput.GetResolvedButton(virtualMask, OVRInput.RawButton.None, controllerMask);
	}

	
	public static bool Get(OVRInput.RawButton rawMask, OVRInput.Controller controllerMask = OVRInput.Controller.Active)
	{
		return OVRInput.GetResolvedButton(OVRInput.Button.None, rawMask, controllerMask);
	}

	
	private static bool GetResolvedButton(OVRInput.Button virtualMask, OVRInput.RawButton rawMask, OVRInput.Controller controllerMask)
	{
		if ((controllerMask & OVRInput.Controller.Active) != OVRInput.Controller.None)
		{
			controllerMask |= OVRInput.activeControllerType;
		}
		for (int i = 0; i < OVRInput.controllers.Count; i++)
		{
			OVRInput.OVRControllerBase ovrcontrollerBase = OVRInput.controllers[i];
			if (OVRInput.ShouldResolveController(ovrcontrollerBase.controllerType, controllerMask))
			{
				OVRInput.RawButton rawButton = rawMask | ovrcontrollerBase.ResolveToRawMask(virtualMask);
				if ((ovrcontrollerBase.currentState.Buttons & (uint)rawButton) != 0u)
				{
					return true;
				}
			}
		}
		return false;
	}

	
	public static bool GetDown(OVRInput.Button virtualMask, OVRInput.Controller controllerMask = OVRInput.Controller.Active)
	{
		return OVRInput.GetResolvedButtonDown(virtualMask, OVRInput.RawButton.None, controllerMask);
	}

	
	public static bool GetDown(OVRInput.RawButton rawMask, OVRInput.Controller controllerMask = OVRInput.Controller.Active)
	{
		return OVRInput.GetResolvedButtonDown(OVRInput.Button.None, rawMask, controllerMask);
	}

	
	private static bool GetResolvedButtonDown(OVRInput.Button virtualMask, OVRInput.RawButton rawMask, OVRInput.Controller controllerMask)
	{
		bool result = false;
		if ((controllerMask & OVRInput.Controller.Active) != OVRInput.Controller.None)
		{
			controllerMask |= OVRInput.activeControllerType;
		}
		for (int i = 0; i < OVRInput.controllers.Count; i++)
		{
			OVRInput.OVRControllerBase ovrcontrollerBase = OVRInput.controllers[i];
			if (OVRInput.ShouldResolveController(ovrcontrollerBase.controllerType, controllerMask))
			{
				OVRInput.RawButton rawButton = rawMask | ovrcontrollerBase.ResolveToRawMask(virtualMask);
				if ((ovrcontrollerBase.previousState.Buttons & (uint)rawButton) != 0u)
				{
					return false;
				}
				if ((ovrcontrollerBase.currentState.Buttons & (uint)rawButton) != 0u && (ovrcontrollerBase.previousState.Buttons & (uint)rawButton) == 0u)
				{
					result = true;
				}
			}
		}
		return result;
	}

	
	public static bool GetUp(OVRInput.Button virtualMask, OVRInput.Controller controllerMask = OVRInput.Controller.Active)
	{
		return OVRInput.GetResolvedButtonUp(virtualMask, OVRInput.RawButton.None, controllerMask);
	}

	
	public static bool GetUp(OVRInput.RawButton rawMask, OVRInput.Controller controllerMask = OVRInput.Controller.Active)
	{
		return OVRInput.GetResolvedButtonUp(OVRInput.Button.None, rawMask, controllerMask);
	}

	
	private static bool GetResolvedButtonUp(OVRInput.Button virtualMask, OVRInput.RawButton rawMask, OVRInput.Controller controllerMask)
	{
		bool result = false;
		if ((controllerMask & OVRInput.Controller.Active) != OVRInput.Controller.None)
		{
			controllerMask |= OVRInput.activeControllerType;
		}
		for (int i = 0; i < OVRInput.controllers.Count; i++)
		{
			OVRInput.OVRControllerBase ovrcontrollerBase = OVRInput.controllers[i];
			if (OVRInput.ShouldResolveController(ovrcontrollerBase.controllerType, controllerMask))
			{
				OVRInput.RawButton rawButton = rawMask | ovrcontrollerBase.ResolveToRawMask(virtualMask);
				if ((ovrcontrollerBase.currentState.Buttons & (uint)rawButton) != 0u)
				{
					return false;
				}
				if ((ovrcontrollerBase.currentState.Buttons & (uint)rawButton) == 0u && (ovrcontrollerBase.previousState.Buttons & (uint)rawButton) != 0u)
				{
					result = true;
				}
			}
		}
		return result;
	}

	
	public static bool Get(OVRInput.Touch virtualMask, OVRInput.Controller controllerMask = OVRInput.Controller.Active)
	{
		return OVRInput.GetResolvedTouch(virtualMask, OVRInput.RawTouch.None, controllerMask);
	}

	
	public static bool Get(OVRInput.RawTouch rawMask, OVRInput.Controller controllerMask = OVRInput.Controller.Active)
	{
		return OVRInput.GetResolvedTouch(OVRInput.Touch.None, rawMask, controllerMask);
	}

	
	private static bool GetResolvedTouch(OVRInput.Touch virtualMask, OVRInput.RawTouch rawMask, OVRInput.Controller controllerMask)
	{
		if ((controllerMask & OVRInput.Controller.Active) != OVRInput.Controller.None)
		{
			controllerMask |= OVRInput.activeControllerType;
		}
		for (int i = 0; i < OVRInput.controllers.Count; i++)
		{
			OVRInput.OVRControllerBase ovrcontrollerBase = OVRInput.controllers[i];
			if (OVRInput.ShouldResolveController(ovrcontrollerBase.controllerType, controllerMask))
			{
				OVRInput.RawTouch rawTouch = rawMask | ovrcontrollerBase.ResolveToRawMask(virtualMask);
				if ((ovrcontrollerBase.currentState.Touches & (uint)rawTouch) != 0u)
				{
					return true;
				}
			}
		}
		return false;
	}

	
	public static bool GetDown(OVRInput.Touch virtualMask, OVRInput.Controller controllerMask = OVRInput.Controller.Active)
	{
		return OVRInput.GetResolvedTouchDown(virtualMask, OVRInput.RawTouch.None, controllerMask);
	}

	
	public static bool GetDown(OVRInput.RawTouch rawMask, OVRInput.Controller controllerMask = OVRInput.Controller.Active)
	{
		return OVRInput.GetResolvedTouchDown(OVRInput.Touch.None, rawMask, controllerMask);
	}

	
	private static bool GetResolvedTouchDown(OVRInput.Touch virtualMask, OVRInput.RawTouch rawMask, OVRInput.Controller controllerMask)
	{
		bool result = false;
		if ((controllerMask & OVRInput.Controller.Active) != OVRInput.Controller.None)
		{
			controllerMask |= OVRInput.activeControllerType;
		}
		for (int i = 0; i < OVRInput.controllers.Count; i++)
		{
			OVRInput.OVRControllerBase ovrcontrollerBase = OVRInput.controllers[i];
			if (OVRInput.ShouldResolveController(ovrcontrollerBase.controllerType, controllerMask))
			{
				OVRInput.RawTouch rawTouch = rawMask | ovrcontrollerBase.ResolveToRawMask(virtualMask);
				if ((ovrcontrollerBase.previousState.Touches & (uint)rawTouch) != 0u)
				{
					return false;
				}
				if ((ovrcontrollerBase.currentState.Touches & (uint)rawTouch) != 0u && (ovrcontrollerBase.previousState.Touches & (uint)rawTouch) == 0u)
				{
					result = true;
				}
			}
		}
		return result;
	}

	
	public static bool GetUp(OVRInput.Touch virtualMask, OVRInput.Controller controllerMask = OVRInput.Controller.Active)
	{
		return OVRInput.GetResolvedTouchUp(virtualMask, OVRInput.RawTouch.None, controllerMask);
	}

	
	public static bool GetUp(OVRInput.RawTouch rawMask, OVRInput.Controller controllerMask = OVRInput.Controller.Active)
	{
		return OVRInput.GetResolvedTouchUp(OVRInput.Touch.None, rawMask, controllerMask);
	}

	
	private static bool GetResolvedTouchUp(OVRInput.Touch virtualMask, OVRInput.RawTouch rawMask, OVRInput.Controller controllerMask)
	{
		bool result = false;
		if ((controllerMask & OVRInput.Controller.Active) != OVRInput.Controller.None)
		{
			controllerMask |= OVRInput.activeControllerType;
		}
		for (int i = 0; i < OVRInput.controllers.Count; i++)
		{
			OVRInput.OVRControllerBase ovrcontrollerBase = OVRInput.controllers[i];
			if (OVRInput.ShouldResolveController(ovrcontrollerBase.controllerType, controllerMask))
			{
				OVRInput.RawTouch rawTouch = rawMask | ovrcontrollerBase.ResolveToRawMask(virtualMask);
				if ((ovrcontrollerBase.currentState.Touches & (uint)rawTouch) != 0u)
				{
					return false;
				}
				if ((ovrcontrollerBase.currentState.Touches & (uint)rawTouch) == 0u && (ovrcontrollerBase.previousState.Touches & (uint)rawTouch) != 0u)
				{
					result = true;
				}
			}
		}
		return result;
	}

	
	public static bool Get(OVRInput.NearTouch virtualMask, OVRInput.Controller controllerMask = OVRInput.Controller.Active)
	{
		return OVRInput.GetResolvedNearTouch(virtualMask, OVRInput.RawNearTouch.None, controllerMask);
	}

	
	public static bool Get(OVRInput.RawNearTouch rawMask, OVRInput.Controller controllerMask = OVRInput.Controller.Active)
	{
		return OVRInput.GetResolvedNearTouch(OVRInput.NearTouch.None, rawMask, controllerMask);
	}

	
	private static bool GetResolvedNearTouch(OVRInput.NearTouch virtualMask, OVRInput.RawNearTouch rawMask, OVRInput.Controller controllerMask)
	{
		if ((controllerMask & OVRInput.Controller.Active) != OVRInput.Controller.None)
		{
			controllerMask |= OVRInput.activeControllerType;
		}
		for (int i = 0; i < OVRInput.controllers.Count; i++)
		{
			OVRInput.OVRControllerBase ovrcontrollerBase = OVRInput.controllers[i];
			if (OVRInput.ShouldResolveController(ovrcontrollerBase.controllerType, controllerMask))
			{
				OVRInput.RawNearTouch rawNearTouch = rawMask | ovrcontrollerBase.ResolveToRawMask(virtualMask);
				if ((ovrcontrollerBase.currentState.NearTouches & (uint)rawNearTouch) != 0u)
				{
					return true;
				}
			}
		}
		return false;
	}

	
	public static bool GetDown(OVRInput.NearTouch virtualMask, OVRInput.Controller controllerMask = OVRInput.Controller.Active)
	{
		return OVRInput.GetResolvedNearTouchDown(virtualMask, OVRInput.RawNearTouch.None, controllerMask);
	}

	
	public static bool GetDown(OVRInput.RawNearTouch rawMask, OVRInput.Controller controllerMask = OVRInput.Controller.Active)
	{
		return OVRInput.GetResolvedNearTouchDown(OVRInput.NearTouch.None, rawMask, controllerMask);
	}

	
	private static bool GetResolvedNearTouchDown(OVRInput.NearTouch virtualMask, OVRInput.RawNearTouch rawMask, OVRInput.Controller controllerMask)
	{
		bool result = false;
		if ((controllerMask & OVRInput.Controller.Active) != OVRInput.Controller.None)
		{
			controllerMask |= OVRInput.activeControllerType;
		}
		for (int i = 0; i < OVRInput.controllers.Count; i++)
		{
			OVRInput.OVRControllerBase ovrcontrollerBase = OVRInput.controllers[i];
			if (OVRInput.ShouldResolveController(ovrcontrollerBase.controllerType, controllerMask))
			{
				OVRInput.RawNearTouch rawNearTouch = rawMask | ovrcontrollerBase.ResolveToRawMask(virtualMask);
				if ((ovrcontrollerBase.previousState.NearTouches & (uint)rawNearTouch) != 0u)
				{
					return false;
				}
				if ((ovrcontrollerBase.currentState.NearTouches & (uint)rawNearTouch) != 0u && (ovrcontrollerBase.previousState.NearTouches & (uint)rawNearTouch) == 0u)
				{
					result = true;
				}
			}
		}
		return result;
	}

	
	public static bool GetUp(OVRInput.NearTouch virtualMask, OVRInput.Controller controllerMask = OVRInput.Controller.Active)
	{
		return OVRInput.GetResolvedNearTouchUp(virtualMask, OVRInput.RawNearTouch.None, controllerMask);
	}

	
	public static bool GetUp(OVRInput.RawNearTouch rawMask, OVRInput.Controller controllerMask = OVRInput.Controller.Active)
	{
		return OVRInput.GetResolvedNearTouchUp(OVRInput.NearTouch.None, rawMask, controllerMask);
	}

	
	private static bool GetResolvedNearTouchUp(OVRInput.NearTouch virtualMask, OVRInput.RawNearTouch rawMask, OVRInput.Controller controllerMask)
	{
		bool result = false;
		if ((controllerMask & OVRInput.Controller.Active) != OVRInput.Controller.None)
		{
			controllerMask |= OVRInput.activeControllerType;
		}
		for (int i = 0; i < OVRInput.controllers.Count; i++)
		{
			OVRInput.OVRControllerBase ovrcontrollerBase = OVRInput.controllers[i];
			if (OVRInput.ShouldResolveController(ovrcontrollerBase.controllerType, controllerMask))
			{
				OVRInput.RawNearTouch rawNearTouch = rawMask | ovrcontrollerBase.ResolveToRawMask(virtualMask);
				if ((ovrcontrollerBase.currentState.NearTouches & (uint)rawNearTouch) != 0u)
				{
					return false;
				}
				if ((ovrcontrollerBase.currentState.NearTouches & (uint)rawNearTouch) == 0u && (ovrcontrollerBase.previousState.NearTouches & (uint)rawNearTouch) != 0u)
				{
					result = true;
				}
			}
		}
		return result;
	}

	
	public static float Get(OVRInput.Axis1D virtualMask, OVRInput.Controller controllerMask = OVRInput.Controller.Active)
	{
		return OVRInput.GetResolvedAxis1D(virtualMask, OVRInput.RawAxis1D.None, controllerMask);
	}

	
	public static float Get(OVRInput.RawAxis1D rawMask, OVRInput.Controller controllerMask = OVRInput.Controller.Active)
	{
		return OVRInput.GetResolvedAxis1D(OVRInput.Axis1D.None, rawMask, controllerMask);
	}

	
	private static float GetResolvedAxis1D(OVRInput.Axis1D virtualMask, OVRInput.RawAxis1D rawMask, OVRInput.Controller controllerMask)
	{
		float num = 0f;
		if ((controllerMask & OVRInput.Controller.Active) != OVRInput.Controller.None)
		{
			controllerMask |= OVRInput.activeControllerType;
		}
		for (int i = 0; i < OVRInput.controllers.Count; i++)
		{
			OVRInput.OVRControllerBase ovrcontrollerBase = OVRInput.controllers[i];
			if (OVRInput.ShouldResolveController(ovrcontrollerBase.controllerType, controllerMask))
			{
				OVRInput.RawAxis1D rawAxis1D = rawMask | ovrcontrollerBase.ResolveToRawMask(virtualMask);
				if ((OVRInput.RawAxis1D.LIndexTrigger & rawAxis1D) != OVRInput.RawAxis1D.None)
				{
					float num2 = ovrcontrollerBase.currentState.LIndexTrigger;
					if (ovrcontrollerBase.shouldApplyDeadzone)
					{
						num2 = OVRInput.CalculateDeadzone(num2, OVRInput.AXIS_DEADZONE_THRESHOLD);
					}
					num = OVRInput.CalculateAbsMax(num, num2);
				}
				if ((OVRInput.RawAxis1D.RIndexTrigger & rawAxis1D) != OVRInput.RawAxis1D.None)
				{
					float num3 = ovrcontrollerBase.currentState.RIndexTrigger;
					if (ovrcontrollerBase.shouldApplyDeadzone)
					{
						num3 = OVRInput.CalculateDeadzone(num3, OVRInput.AXIS_DEADZONE_THRESHOLD);
					}
					num = OVRInput.CalculateAbsMax(num, num3);
				}
				if ((OVRInput.RawAxis1D.LHandTrigger & rawAxis1D) != OVRInput.RawAxis1D.None)
				{
					float num4 = ovrcontrollerBase.currentState.LHandTrigger;
					if (ovrcontrollerBase.shouldApplyDeadzone)
					{
						num4 = OVRInput.CalculateDeadzone(num4, OVRInput.AXIS_DEADZONE_THRESHOLD);
					}
					num = OVRInput.CalculateAbsMax(num, num4);
				}
				if ((OVRInput.RawAxis1D.RHandTrigger & rawAxis1D) != OVRInput.RawAxis1D.None)
				{
					float num5 = ovrcontrollerBase.currentState.RHandTrigger;
					if (ovrcontrollerBase.shouldApplyDeadzone)
					{
						num5 = OVRInput.CalculateDeadzone(num5, OVRInput.AXIS_DEADZONE_THRESHOLD);
					}
					num = OVRInput.CalculateAbsMax(num, num5);
				}
			}
		}
		return num;
	}

	
	public static Vector2 Get(OVRInput.Axis2D virtualMask, OVRInput.Controller controllerMask = OVRInput.Controller.Active)
	{
		return OVRInput.GetResolvedAxis2D(virtualMask, OVRInput.RawAxis2D.None, controllerMask);
	}

	
	public static Vector2 Get(OVRInput.RawAxis2D rawMask, OVRInput.Controller controllerMask = OVRInput.Controller.Active)
	{
		return OVRInput.GetResolvedAxis2D(OVRInput.Axis2D.None, rawMask, controllerMask);
	}

	
	private static Vector2 GetResolvedAxis2D(OVRInput.Axis2D virtualMask, OVRInput.RawAxis2D rawMask, OVRInput.Controller controllerMask)
	{
		Vector2 vector = Vector2.zero;
		if ((controllerMask & OVRInput.Controller.Active) != OVRInput.Controller.None)
		{
			controllerMask |= OVRInput.activeControllerType;
		}
		for (int i = 0; i < OVRInput.controllers.Count; i++)
		{
			OVRInput.OVRControllerBase ovrcontrollerBase = OVRInput.controllers[i];
			if (OVRInput.ShouldResolveController(ovrcontrollerBase.controllerType, controllerMask))
			{
				OVRInput.RawAxis2D rawAxis2D = rawMask | ovrcontrollerBase.ResolveToRawMask(virtualMask);
				if ((OVRInput.RawAxis2D.LThumbstick & rawAxis2D) != OVRInput.RawAxis2D.None)
				{
					Vector2 vector2 = new Vector2(ovrcontrollerBase.currentState.LThumbstick.x, ovrcontrollerBase.currentState.LThumbstick.y);
					if (ovrcontrollerBase.shouldApplyDeadzone)
					{
						vector2 = OVRInput.CalculateDeadzone(vector2, OVRInput.AXIS_DEADZONE_THRESHOLD);
					}
					vector = OVRInput.CalculateAbsMax(vector, vector2);
				}
				if ((OVRInput.RawAxis2D.LTouchpad & rawAxis2D) != OVRInput.RawAxis2D.None)
				{
					Vector2 b = new Vector2(ovrcontrollerBase.currentState.LTouchpad.x, ovrcontrollerBase.currentState.LTouchpad.y);
					vector = OVRInput.CalculateAbsMax(vector, b);
				}
				if ((OVRInput.RawAxis2D.RThumbstick & rawAxis2D) != OVRInput.RawAxis2D.None)
				{
					Vector2 vector3 = new Vector2(ovrcontrollerBase.currentState.RThumbstick.x, ovrcontrollerBase.currentState.RThumbstick.y);
					if (ovrcontrollerBase.shouldApplyDeadzone)
					{
						vector3 = OVRInput.CalculateDeadzone(vector3, OVRInput.AXIS_DEADZONE_THRESHOLD);
					}
					vector = OVRInput.CalculateAbsMax(vector, vector3);
				}
				if ((OVRInput.RawAxis2D.RTouchpad & rawAxis2D) != OVRInput.RawAxis2D.None)
				{
					Vector2 b2 = new Vector2(ovrcontrollerBase.currentState.RTouchpad.x, ovrcontrollerBase.currentState.RTouchpad.y);
					vector = OVRInput.CalculateAbsMax(vector, b2);
				}
			}
		}
		return vector;
	}

	
	public static OVRInput.Controller GetConnectedControllers()
	{
		return OVRInput.connectedControllerTypes;
	}

	
	public static bool IsControllerConnected(OVRInput.Controller controller)
	{
		return (OVRInput.connectedControllerTypes & controller) == controller;
	}

	
	public static OVRInput.Controller GetActiveController()
	{
		return OVRInput.activeControllerType;
	}

	
	public static void SetControllerVibration(float frequency, float amplitude, OVRInput.Controller controllerMask = OVRInput.Controller.Active)
	{
		if ((controllerMask & OVRInput.Controller.Active) != OVRInput.Controller.None)
		{
			controllerMask |= OVRInput.activeControllerType;
		}
		for (int i = 0; i < OVRInput.controllers.Count; i++)
		{
			OVRInput.OVRControllerBase ovrcontrollerBase = OVRInput.controllers[i];
			if (OVRInput.ShouldResolveController(ovrcontrollerBase.controllerType, controllerMask))
			{
				ovrcontrollerBase.SetControllerVibration(frequency, amplitude);
			}
		}
	}

	
	public static void RecenterController(OVRInput.Controller controllerMask = OVRInput.Controller.Active)
	{
		if ((controllerMask & OVRInput.Controller.Active) != OVRInput.Controller.None)
		{
			controllerMask |= OVRInput.activeControllerType;
		}
		for (int i = 0; i < OVRInput.controllers.Count; i++)
		{
			OVRInput.OVRControllerBase ovrcontrollerBase = OVRInput.controllers[i];
			if (OVRInput.ShouldResolveController(ovrcontrollerBase.controllerType, controllerMask))
			{
				ovrcontrollerBase.RecenterController();
			}
		}
	}

	
	public static bool GetControllerWasRecentered(OVRInput.Controller controllerMask = OVRInput.Controller.Active)
	{
		if ((controllerMask & OVRInput.Controller.Active) != OVRInput.Controller.None)
		{
			controllerMask |= OVRInput.activeControllerType;
		}
		bool flag = false;
		for (int i = 0; i < OVRInput.controllers.Count; i++)
		{
			OVRInput.OVRControllerBase ovrcontrollerBase = OVRInput.controllers[i];
			if (OVRInput.ShouldResolveController(ovrcontrollerBase.controllerType, controllerMask))
			{
				flag |= ovrcontrollerBase.WasRecentered();
			}
		}
		return flag;
	}

	
	public static byte GetControllerRecenterCount(OVRInput.Controller controllerMask = OVRInput.Controller.Active)
	{
		if ((controllerMask & OVRInput.Controller.Active) != OVRInput.Controller.None)
		{
			controllerMask |= OVRInput.activeControllerType;
		}
		byte result = 0;
		for (int i = 0; i < OVRInput.controllers.Count; i++)
		{
			OVRInput.OVRControllerBase ovrcontrollerBase = OVRInput.controllers[i];
			if (OVRInput.ShouldResolveController(ovrcontrollerBase.controllerType, controllerMask))
			{
				result = ovrcontrollerBase.GetRecenterCount();
				break;
			}
		}
		return result;
	}

	
	public static byte GetControllerBatteryPercentRemaining(OVRInput.Controller controllerMask = OVRInput.Controller.Active)
	{
		if ((controllerMask & OVRInput.Controller.Active) != OVRInput.Controller.None)
		{
			controllerMask |= OVRInput.activeControllerType;
		}
		byte result = 0;
		for (int i = 0; i < OVRInput.controllers.Count; i++)
		{
			OVRInput.OVRControllerBase ovrcontrollerBase = OVRInput.controllers[i];
			if (OVRInput.ShouldResolveController(ovrcontrollerBase.controllerType, controllerMask))
			{
				result = ovrcontrollerBase.GetBatteryPercentRemaining();
				break;
			}
		}
		return result;
	}

	
	private static Vector2 CalculateAbsMax(Vector2 a, Vector2 b)
	{
		float sqrMagnitude = a.sqrMagnitude;
		float sqrMagnitude2 = b.sqrMagnitude;
		if (sqrMagnitude >= sqrMagnitude2)
		{
			return a;
		}
		return b;
	}

	
	private static float CalculateAbsMax(float a, float b)
	{
		float num = (a < 0f) ? (-a) : a;
		float num2 = (b < 0f) ? (-b) : b;
		if (num >= num2)
		{
			return a;
		}
		return b;
	}

	
	private static Vector2 CalculateDeadzone(Vector2 a, float deadzone)
	{
		if (a.sqrMagnitude <= deadzone * deadzone)
		{
			return Vector2.zero;
		}
		a *= (a.magnitude - deadzone) / (1f - deadzone);
		if (a.sqrMagnitude > 1f)
		{
			return a.normalized;
		}
		return a;
	}

	
	private static float CalculateDeadzone(float a, float deadzone)
	{
		float num = (a < 0f) ? (-a) : a;
		if (num <= deadzone)
		{
			return 0f;
		}
		a *= (num - deadzone) / (1f - deadzone);
		if (a * a > 1f)
		{
			return (a < 0f) ? -1f : 1f;
		}
		return a;
	}

	
	private static bool ShouldResolveController(OVRInput.Controller controllerType, OVRInput.Controller controllerMask)
	{
		bool result = false;
		if ((controllerType & controllerMask) == controllerType)
		{
			result = true;
		}
		if ((controllerMask & OVRInput.Controller.Touch) == OVRInput.Controller.Touch && (controllerType & OVRInput.Controller.Touch) != OVRInput.Controller.None && (controllerType & OVRInput.Controller.Touch) != OVRInput.Controller.Touch)
		{
			result = false;
		}
		return result;
	}

	
	private static readonly float AXIS_AS_BUTTON_THRESHOLD = 0.5f;

	
	private static readonly float AXIS_DEADZONE_THRESHOLD = 0.2f;

	
	private static List<OVRInput.OVRControllerBase> controllers;

	
	private static OVRInput.Controller activeControllerType = OVRInput.Controller.None;

	
	private static OVRInput.Controller connectedControllerTypes = OVRInput.Controller.None;

	
	private static OVRPlugin.Step stepType = OVRPlugin.Step.Render;

	
	private static int fixedUpdateCount = 0;

	
	private static bool _pluginSupportsActiveController = false;

	
	private static bool _pluginSupportsActiveControllerCached = false;

	
	private static Version _pluginSupportsActiveControllerMinVersion = new Version(1, 9, 0);

	
	[Flags]
	public enum Button
	{
		
		None = 0,
		
		One = 1,
		
		Two = 2,
		
		Three = 4,
		
		Four = 8,
		
		Start = 256,
		
		Back = 512,
		
		PrimaryShoulder = 4096,
		
		PrimaryIndexTrigger = 8192,
		
		PrimaryHandTrigger = 16384,
		
		PrimaryThumbstick = 32768,
		
		PrimaryThumbstickUp = 65536,
		
		PrimaryThumbstickDown = 131072,
		
		PrimaryThumbstickLeft = 262144,
		
		PrimaryThumbstickRight = 524288,
		
		PrimaryTouchpad = 1024,
		
		SecondaryShoulder = 1048576,
		
		SecondaryIndexTrigger = 2097152,
		
		SecondaryHandTrigger = 4194304,
		
		SecondaryThumbstick = 8388608,
		
		SecondaryThumbstickUp = 16777216,
		
		SecondaryThumbstickDown = 33554432,
		
		SecondaryThumbstickLeft = 67108864,
		
		SecondaryThumbstickRight = 134217728,
		
		SecondaryTouchpad = 2048,
		
		DpadUp = 16,
		
		DpadDown = 32,
		
		DpadLeft = 64,
		
		DpadRight = 128,
		
		Up = 268435456,
		
		Down = 536870912,
		
		Left = 1073741824,
		
		Right = -2147483648,
		
		Any = -1
	}

	
	[Flags]
	public enum RawButton
	{
		
		None = 0,
		
		A = 1,
		
		B = 2,
		
		X = 256,
		
		Y = 512,
		
		Start = 1048576,
		
		Back = 2097152,
		
		LShoulder = 2048,
		
		LIndexTrigger = 268435456,
		
		LHandTrigger = 536870912,
		
		LThumbstick = 1024,
		
		LThumbstickUp = 16,
		
		LThumbstickDown = 32,
		
		LThumbstickLeft = 64,
		
		LThumbstickRight = 128,
		
		LTouchpad = 1073741824,
		
		RShoulder = 8,
		
		RIndexTrigger = 67108864,
		
		RHandTrigger = 134217728,
		
		RThumbstick = 4,
		
		RThumbstickUp = 4096,
		
		RThumbstickDown = 8192,
		
		RThumbstickLeft = 16384,
		
		RThumbstickRight = 32768,
		
		RTouchpad = -2147483648,
		
		DpadUp = 65536,
		
		DpadDown = 131072,
		
		DpadLeft = 262144,
		
		DpadRight = 524288,
		
		Any = -1
	}

	
	[Flags]
	public enum Touch
	{
		
		None = 0,
		
		One = 1,
		
		Two = 2,
		
		Three = 4,
		
		Four = 8,
		
		PrimaryIndexTrigger = 8192,
		
		PrimaryThumbstick = 32768,
		
		PrimaryThumbRest = 4096,
		
		PrimaryTouchpad = 1024,
		
		SecondaryIndexTrigger = 2097152,
		
		SecondaryThumbstick = 8388608,
		
		SecondaryThumbRest = 1048576,
		
		SecondaryTouchpad = 2048,
		
		Any = -1
	}

	
	[Flags]
	public enum RawTouch
	{
		
		None = 0,
		
		A = 1,
		
		B = 2,
		
		X = 256,
		
		Y = 512,
		
		LIndexTrigger = 4096,
		
		LThumbstick = 1024,
		
		LThumbRest = 2048,
		
		LTouchpad = 1073741824,
		
		RIndexTrigger = 16,
		
		RThumbstick = 4,
		
		RThumbRest = 8,
		
		RTouchpad = -2147483648,
		
		Any = -1
	}

	
	[Flags]
	public enum NearTouch
	{
		
		None = 0,
		
		PrimaryIndexTrigger = 1,
		
		PrimaryThumbButtons = 2,
		
		SecondaryIndexTrigger = 4,
		
		SecondaryThumbButtons = 8,
		
		Any = -1
	}

	
	[Flags]
	public enum RawNearTouch
	{
		
		None = 0,
		
		LIndexTrigger = 1,
		
		LThumbButtons = 2,
		
		RIndexTrigger = 4,
		
		RThumbButtons = 8,
		
		Any = -1
	}

	
	[Flags]
	public enum Axis1D
	{
		
		None = 0,
		
		PrimaryIndexTrigger = 1,
		
		PrimaryHandTrigger = 4,
		
		SecondaryIndexTrigger = 2,
		
		SecondaryHandTrigger = 8,
		
		Any = -1
	}

	
	[Flags]
	public enum RawAxis1D
	{
		
		None = 0,
		
		LIndexTrigger = 1,
		
		LHandTrigger = 4,
		
		RIndexTrigger = 2,
		
		RHandTrigger = 8,
		
		Any = -1
	}

	
	[Flags]
	public enum Axis2D
	{
		
		None = 0,
		
		PrimaryThumbstick = 1,
		
		PrimaryTouchpad = 4,
		
		SecondaryThumbstick = 2,
		
		SecondaryTouchpad = 8,
		
		Any = -1
	}

	
	[Flags]
	public enum RawAxis2D
	{
		
		None = 0,
		
		LThumbstick = 1,
		
		LTouchpad = 4,
		
		RThumbstick = 2,
		
		RTouchpad = 8,
		
		Any = -1
	}

	
	[Flags]
	public enum Controller
	{
		
		None = 0,
		
		LTouch = 1,
		
		RTouch = 2,
		
		Touch = 3,
		
		Remote = 4,
		
		Gamepad = 16,
		
		Touchpad = 134217728,
		
		LTrackedRemote = 16777216,
		
		RTrackedRemote = 33554432,
		
		Active = -2147483648,
		
		All = -1
	}

	
	private abstract class OVRControllerBase
	{
		
		public OVRControllerBase()
		{
			this.ConfigureButtonMap();
			this.ConfigureTouchMap();
			this.ConfigureNearTouchMap();
			this.ConfigureAxis1DMap();
			this.ConfigureAxis2DMap();
		}

		
		public virtual OVRInput.Controller Update()
		{
			OVRPlugin.ControllerState4 controllerState = OVRPlugin.GetControllerState4((uint)this.controllerType);
			if (controllerState.LIndexTrigger >= OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				controllerState.Buttons |= 268435456u;
			}
			if (controllerState.LHandTrigger >= OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				controllerState.Buttons |= 536870912u;
			}
			if (controllerState.LThumbstick.y >= OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				controllerState.Buttons |= 16u;
			}
			if (controllerState.LThumbstick.y <= -OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				controllerState.Buttons |= 32u;
			}
			if (controllerState.LThumbstick.x <= -OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				controllerState.Buttons |= 64u;
			}
			if (controllerState.LThumbstick.x >= OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				controllerState.Buttons |= 128u;
			}
			if (controllerState.RIndexTrigger >= OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				controllerState.Buttons |= 67108864u;
			}
			if (controllerState.RHandTrigger >= OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				controllerState.Buttons |= 134217728u;
			}
			if (controllerState.RThumbstick.y >= OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				controllerState.Buttons |= 4096u;
			}
			if (controllerState.RThumbstick.y <= -OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				controllerState.Buttons |= 8192u;
			}
			if (controllerState.RThumbstick.x <= -OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				controllerState.Buttons |= 16384u;
			}
			if (controllerState.RThumbstick.x >= OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				controllerState.Buttons |= 32768u;
			}
			this.previousState = this.currentState;
			this.currentState = controllerState;
			return (OVRInput.Controller)(this.currentState.ConnectedControllers & (uint)this.controllerType);
		}

		
		public virtual void SetControllerVibration(float frequency, float amplitude)
		{
			OVRPlugin.SetControllerVibration((uint)this.controllerType, frequency, amplitude);
		}

		
		public virtual void RecenterController()
		{
			OVRPlugin.RecenterTrackingOrigin(OVRPlugin.RecenterFlags.Controllers);
		}

		
		public virtual bool WasRecentered()
		{
			return false;
		}

		
		public virtual byte GetRecenterCount()
		{
			return 0;
		}

		
		public virtual byte GetBatteryPercentRemaining()
		{
			return 0;
		}

		
		public abstract void ConfigureButtonMap();

		
		public abstract void ConfigureTouchMap();

		
		public abstract void ConfigureNearTouchMap();

		
		public abstract void ConfigureAxis1DMap();

		
		public abstract void ConfigureAxis2DMap();

		
		public OVRInput.RawButton ResolveToRawMask(OVRInput.Button virtualMask)
		{
			return this.buttonMap.ToRawMask(virtualMask);
		}

		
		public OVRInput.RawTouch ResolveToRawMask(OVRInput.Touch virtualMask)
		{
			return this.touchMap.ToRawMask(virtualMask);
		}

		
		public OVRInput.RawNearTouch ResolveToRawMask(OVRInput.NearTouch virtualMask)
		{
			return this.nearTouchMap.ToRawMask(virtualMask);
		}

		
		public OVRInput.RawAxis1D ResolveToRawMask(OVRInput.Axis1D virtualMask)
		{
			return this.axis1DMap.ToRawMask(virtualMask);
		}

		
		public OVRInput.RawAxis2D ResolveToRawMask(OVRInput.Axis2D virtualMask)
		{
			return this.axis2DMap.ToRawMask(virtualMask);
		}

		
		public OVRInput.Controller controllerType;

		
		public OVRInput.OVRControllerBase.VirtualButtonMap buttonMap = new OVRInput.OVRControllerBase.VirtualButtonMap();

		
		public OVRInput.OVRControllerBase.VirtualTouchMap touchMap = new OVRInput.OVRControllerBase.VirtualTouchMap();

		
		public OVRInput.OVRControllerBase.VirtualNearTouchMap nearTouchMap = new OVRInput.OVRControllerBase.VirtualNearTouchMap();

		
		public OVRInput.OVRControllerBase.VirtualAxis1DMap axis1DMap = new OVRInput.OVRControllerBase.VirtualAxis1DMap();

		
		public OVRInput.OVRControllerBase.VirtualAxis2DMap axis2DMap = new OVRInput.OVRControllerBase.VirtualAxis2DMap();

		
		public OVRPlugin.ControllerState4 previousState = default(OVRPlugin.ControllerState4);

		
		public OVRPlugin.ControllerState4 currentState = default(OVRPlugin.ControllerState4);

		
		public bool shouldApplyDeadzone = true;

		
		public class VirtualButtonMap
		{
			
			public OVRInput.RawButton ToRawMask(OVRInput.Button virtualMask)
			{
				OVRInput.RawButton rawButton = OVRInput.RawButton.None;
				if (virtualMask == OVRInput.Button.None)
				{
					return OVRInput.RawButton.None;
				}
				if ((virtualMask & OVRInput.Button.One) != OVRInput.Button.None)
				{
					rawButton |= this.One;
				}
				if ((virtualMask & OVRInput.Button.Two) != OVRInput.Button.None)
				{
					rawButton |= this.Two;
				}
				if ((virtualMask & OVRInput.Button.Three) != OVRInput.Button.None)
				{
					rawButton |= this.Three;
				}
				if ((virtualMask & OVRInput.Button.Four) != OVRInput.Button.None)
				{
					rawButton |= this.Four;
				}
				if ((virtualMask & OVRInput.Button.Start) != OVRInput.Button.None)
				{
					rawButton |= this.Start;
				}
				if ((virtualMask & OVRInput.Button.Back) != OVRInput.Button.None)
				{
					rawButton |= this.Back;
				}
				if ((virtualMask & OVRInput.Button.PrimaryShoulder) != OVRInput.Button.None)
				{
					rawButton |= this.PrimaryShoulder;
				}
				if ((virtualMask & OVRInput.Button.PrimaryIndexTrigger) != OVRInput.Button.None)
				{
					rawButton |= this.PrimaryIndexTrigger;
				}
				if ((virtualMask & OVRInput.Button.PrimaryHandTrigger) != OVRInput.Button.None)
				{
					rawButton |= this.PrimaryHandTrigger;
				}
				if ((virtualMask & OVRInput.Button.PrimaryThumbstick) != OVRInput.Button.None)
				{
					rawButton |= this.PrimaryThumbstick;
				}
				if ((virtualMask & OVRInput.Button.PrimaryThumbstickUp) != OVRInput.Button.None)
				{
					rawButton |= this.PrimaryThumbstickUp;
				}
				if ((virtualMask & OVRInput.Button.PrimaryThumbstickDown) != OVRInput.Button.None)
				{
					rawButton |= this.PrimaryThumbstickDown;
				}
				if ((virtualMask & OVRInput.Button.PrimaryThumbstickLeft) != OVRInput.Button.None)
				{
					rawButton |= this.PrimaryThumbstickLeft;
				}
				if ((virtualMask & OVRInput.Button.PrimaryThumbstickRight) != OVRInput.Button.None)
				{
					rawButton |= this.PrimaryThumbstickRight;
				}
				if ((virtualMask & OVRInput.Button.PrimaryTouchpad) != OVRInput.Button.None)
				{
					rawButton |= this.PrimaryTouchpad;
				}
				if ((virtualMask & OVRInput.Button.SecondaryShoulder) != OVRInput.Button.None)
				{
					rawButton |= this.SecondaryShoulder;
				}
				if ((virtualMask & OVRInput.Button.SecondaryIndexTrigger) != OVRInput.Button.None)
				{
					rawButton |= this.SecondaryIndexTrigger;
				}
				if ((virtualMask & OVRInput.Button.SecondaryHandTrigger) != OVRInput.Button.None)
				{
					rawButton |= this.SecondaryHandTrigger;
				}
				if ((virtualMask & OVRInput.Button.SecondaryThumbstick) != OVRInput.Button.None)
				{
					rawButton |= this.SecondaryThumbstick;
				}
				if ((virtualMask & OVRInput.Button.SecondaryThumbstickUp) != OVRInput.Button.None)
				{
					rawButton |= this.SecondaryThumbstickUp;
				}
				if ((virtualMask & OVRInput.Button.SecondaryThumbstickDown) != OVRInput.Button.None)
				{
					rawButton |= this.SecondaryThumbstickDown;
				}
				if ((virtualMask & OVRInput.Button.SecondaryThumbstickLeft) != OVRInput.Button.None)
				{
					rawButton |= this.SecondaryThumbstickLeft;
				}
				if ((virtualMask & OVRInput.Button.SecondaryThumbstickRight) != OVRInput.Button.None)
				{
					rawButton |= this.SecondaryThumbstickRight;
				}
				if ((virtualMask & OVRInput.Button.SecondaryTouchpad) != OVRInput.Button.None)
				{
					rawButton |= this.SecondaryTouchpad;
				}
				if ((virtualMask & OVRInput.Button.DpadUp) != OVRInput.Button.None)
				{
					rawButton |= this.DpadUp;
				}
				if ((virtualMask & OVRInput.Button.DpadDown) != OVRInput.Button.None)
				{
					rawButton |= this.DpadDown;
				}
				if ((virtualMask & OVRInput.Button.DpadLeft) != OVRInput.Button.None)
				{
					rawButton |= this.DpadLeft;
				}
				if ((virtualMask & OVRInput.Button.DpadRight) != OVRInput.Button.None)
				{
					rawButton |= this.DpadRight;
				}
				if ((virtualMask & OVRInput.Button.Up) != OVRInput.Button.None)
				{
					rawButton |= this.Up;
				}
				if ((virtualMask & OVRInput.Button.Down) != OVRInput.Button.None)
				{
					rawButton |= this.Down;
				}
				if ((virtualMask & OVRInput.Button.Left) != OVRInput.Button.None)
				{
					rawButton |= this.Left;
				}
				if ((virtualMask & OVRInput.Button.Right) != OVRInput.Button.None)
				{
					rawButton |= this.Right;
				}
				return rawButton;
			}

			
			public OVRInput.RawButton None;

			
			public OVRInput.RawButton One;

			
			public OVRInput.RawButton Two;

			
			public OVRInput.RawButton Three;

			
			public OVRInput.RawButton Four;

			
			public OVRInput.RawButton Start;

			
			public OVRInput.RawButton Back;

			
			public OVRInput.RawButton PrimaryShoulder;

			
			public OVRInput.RawButton PrimaryIndexTrigger;

			
			public OVRInput.RawButton PrimaryHandTrigger;

			
			public OVRInput.RawButton PrimaryThumbstick;

			
			public OVRInput.RawButton PrimaryThumbstickUp;

			
			public OVRInput.RawButton PrimaryThumbstickDown;

			
			public OVRInput.RawButton PrimaryThumbstickLeft;

			
			public OVRInput.RawButton PrimaryThumbstickRight;

			
			public OVRInput.RawButton PrimaryTouchpad;

			
			public OVRInput.RawButton SecondaryShoulder;

			
			public OVRInput.RawButton SecondaryIndexTrigger;

			
			public OVRInput.RawButton SecondaryHandTrigger;

			
			public OVRInput.RawButton SecondaryThumbstick;

			
			public OVRInput.RawButton SecondaryThumbstickUp;

			
			public OVRInput.RawButton SecondaryThumbstickDown;

			
			public OVRInput.RawButton SecondaryThumbstickLeft;

			
			public OVRInput.RawButton SecondaryThumbstickRight;

			
			public OVRInput.RawButton SecondaryTouchpad;

			
			public OVRInput.RawButton DpadUp;

			
			public OVRInput.RawButton DpadDown;

			
			public OVRInput.RawButton DpadLeft;

			
			public OVRInput.RawButton DpadRight;

			
			public OVRInput.RawButton Up;

			
			public OVRInput.RawButton Down;

			
			public OVRInput.RawButton Left;

			
			public OVRInput.RawButton Right;
		}

		
		public class VirtualTouchMap
		{
			
			public OVRInput.RawTouch ToRawMask(OVRInput.Touch virtualMask)
			{
				OVRInput.RawTouch rawTouch = OVRInput.RawTouch.None;
				if (virtualMask == OVRInput.Touch.None)
				{
					return OVRInput.RawTouch.None;
				}
				if ((virtualMask & OVRInput.Touch.One) != OVRInput.Touch.None)
				{
					rawTouch |= this.One;
				}
				if ((virtualMask & OVRInput.Touch.Two) != OVRInput.Touch.None)
				{
					rawTouch |= this.Two;
				}
				if ((virtualMask & OVRInput.Touch.Three) != OVRInput.Touch.None)
				{
					rawTouch |= this.Three;
				}
				if ((virtualMask & OVRInput.Touch.Four) != OVRInput.Touch.None)
				{
					rawTouch |= this.Four;
				}
				if ((virtualMask & OVRInput.Touch.PrimaryIndexTrigger) != OVRInput.Touch.None)
				{
					rawTouch |= this.PrimaryIndexTrigger;
				}
				if ((virtualMask & OVRInput.Touch.PrimaryThumbstick) != OVRInput.Touch.None)
				{
					rawTouch |= this.PrimaryThumbstick;
				}
				if ((virtualMask & OVRInput.Touch.PrimaryThumbRest) != OVRInput.Touch.None)
				{
					rawTouch |= this.PrimaryThumbRest;
				}
				if ((virtualMask & OVRInput.Touch.PrimaryTouchpad) != OVRInput.Touch.None)
				{
					rawTouch |= this.PrimaryTouchpad;
				}
				if ((virtualMask & OVRInput.Touch.SecondaryIndexTrigger) != OVRInput.Touch.None)
				{
					rawTouch |= this.SecondaryIndexTrigger;
				}
				if ((virtualMask & OVRInput.Touch.SecondaryThumbstick) != OVRInput.Touch.None)
				{
					rawTouch |= this.SecondaryThumbstick;
				}
				if ((virtualMask & OVRInput.Touch.SecondaryThumbRest) != OVRInput.Touch.None)
				{
					rawTouch |= this.SecondaryThumbRest;
				}
				if ((virtualMask & OVRInput.Touch.SecondaryTouchpad) != OVRInput.Touch.None)
				{
					rawTouch |= this.SecondaryTouchpad;
				}
				return rawTouch;
			}

			
			public OVRInput.RawTouch None;

			
			public OVRInput.RawTouch One;

			
			public OVRInput.RawTouch Two;

			
			public OVRInput.RawTouch Three;

			
			public OVRInput.RawTouch Four;

			
			public OVRInput.RawTouch PrimaryIndexTrigger;

			
			public OVRInput.RawTouch PrimaryThumbstick;

			
			public OVRInput.RawTouch PrimaryThumbRest;

			
			public OVRInput.RawTouch PrimaryTouchpad;

			
			public OVRInput.RawTouch SecondaryIndexTrigger;

			
			public OVRInput.RawTouch SecondaryThumbstick;

			
			public OVRInput.RawTouch SecondaryThumbRest;

			
			public OVRInput.RawTouch SecondaryTouchpad;
		}

		
		public class VirtualNearTouchMap
		{
			
			public OVRInput.RawNearTouch ToRawMask(OVRInput.NearTouch virtualMask)
			{
				OVRInput.RawNearTouch rawNearTouch = OVRInput.RawNearTouch.None;
				if (virtualMask == OVRInput.NearTouch.None)
				{
					return OVRInput.RawNearTouch.None;
				}
				if ((virtualMask & OVRInput.NearTouch.PrimaryIndexTrigger) != OVRInput.NearTouch.None)
				{
					rawNearTouch |= this.PrimaryIndexTrigger;
				}
				if ((virtualMask & OVRInput.NearTouch.PrimaryThumbButtons) != OVRInput.NearTouch.None)
				{
					rawNearTouch |= this.PrimaryThumbButtons;
				}
				if ((virtualMask & OVRInput.NearTouch.SecondaryIndexTrigger) != OVRInput.NearTouch.None)
				{
					rawNearTouch |= this.SecondaryIndexTrigger;
				}
				if ((virtualMask & OVRInput.NearTouch.SecondaryThumbButtons) != OVRInput.NearTouch.None)
				{
					rawNearTouch |= this.SecondaryThumbButtons;
				}
				return rawNearTouch;
			}

			
			public OVRInput.RawNearTouch None;

			
			public OVRInput.RawNearTouch PrimaryIndexTrigger;

			
			public OVRInput.RawNearTouch PrimaryThumbButtons;

			
			public OVRInput.RawNearTouch SecondaryIndexTrigger;

			
			public OVRInput.RawNearTouch SecondaryThumbButtons;
		}

		
		public class VirtualAxis1DMap
		{
			
			public OVRInput.RawAxis1D ToRawMask(OVRInput.Axis1D virtualMask)
			{
				OVRInput.RawAxis1D rawAxis1D = OVRInput.RawAxis1D.None;
				if (virtualMask == OVRInput.Axis1D.None)
				{
					return OVRInput.RawAxis1D.None;
				}
				if ((virtualMask & OVRInput.Axis1D.PrimaryIndexTrigger) != OVRInput.Axis1D.None)
				{
					rawAxis1D |= this.PrimaryIndexTrigger;
				}
				if ((virtualMask & OVRInput.Axis1D.PrimaryHandTrigger) != OVRInput.Axis1D.None)
				{
					rawAxis1D |= this.PrimaryHandTrigger;
				}
				if ((virtualMask & OVRInput.Axis1D.SecondaryIndexTrigger) != OVRInput.Axis1D.None)
				{
					rawAxis1D |= this.SecondaryIndexTrigger;
				}
				if ((virtualMask & OVRInput.Axis1D.SecondaryHandTrigger) != OVRInput.Axis1D.None)
				{
					rawAxis1D |= this.SecondaryHandTrigger;
				}
				return rawAxis1D;
			}

			
			public OVRInput.RawAxis1D None;

			
			public OVRInput.RawAxis1D PrimaryIndexTrigger;

			
			public OVRInput.RawAxis1D PrimaryHandTrigger;

			
			public OVRInput.RawAxis1D SecondaryIndexTrigger;

			
			public OVRInput.RawAxis1D SecondaryHandTrigger;
		}

		
		public class VirtualAxis2DMap
		{
			
			public OVRInput.RawAxis2D ToRawMask(OVRInput.Axis2D virtualMask)
			{
				OVRInput.RawAxis2D rawAxis2D = OVRInput.RawAxis2D.None;
				if (virtualMask == OVRInput.Axis2D.None)
				{
					return OVRInput.RawAxis2D.None;
				}
				if ((virtualMask & OVRInput.Axis2D.PrimaryThumbstick) != OVRInput.Axis2D.None)
				{
					rawAxis2D |= this.PrimaryThumbstick;
				}
				if ((virtualMask & OVRInput.Axis2D.PrimaryTouchpad) != OVRInput.Axis2D.None)
				{
					rawAxis2D |= this.PrimaryTouchpad;
				}
				if ((virtualMask & OVRInput.Axis2D.SecondaryThumbstick) != OVRInput.Axis2D.None)
				{
					rawAxis2D |= this.SecondaryThumbstick;
				}
				if ((virtualMask & OVRInput.Axis2D.SecondaryTouchpad) != OVRInput.Axis2D.None)
				{
					rawAxis2D |= this.SecondaryTouchpad;
				}
				return rawAxis2D;
			}

			
			public OVRInput.RawAxis2D None;

			
			public OVRInput.RawAxis2D PrimaryThumbstick;

			
			public OVRInput.RawAxis2D PrimaryTouchpad;

			
			public OVRInput.RawAxis2D SecondaryThumbstick;

			
			public OVRInput.RawAxis2D SecondaryTouchpad;
		}
	}

	
	private class OVRControllerTouch : OVRInput.OVRControllerBase
	{
		
		public OVRControllerTouch()
		{
			this.controllerType = OVRInput.Controller.Touch;
		}

		
		public override void ConfigureButtonMap()
		{
			this.buttonMap.None = OVRInput.RawButton.None;
			this.buttonMap.One = OVRInput.RawButton.A;
			this.buttonMap.Two = OVRInput.RawButton.B;
			this.buttonMap.Three = OVRInput.RawButton.X;
			this.buttonMap.Four = OVRInput.RawButton.Y;
			this.buttonMap.Start = OVRInput.RawButton.Start;
			this.buttonMap.Back = OVRInput.RawButton.None;
			this.buttonMap.PrimaryShoulder = OVRInput.RawButton.None;
			this.buttonMap.PrimaryIndexTrigger = OVRInput.RawButton.LIndexTrigger;
			this.buttonMap.PrimaryHandTrigger = OVRInput.RawButton.LHandTrigger;
			this.buttonMap.PrimaryThumbstick = OVRInput.RawButton.LThumbstick;
			this.buttonMap.PrimaryThumbstickUp = OVRInput.RawButton.LThumbstickUp;
			this.buttonMap.PrimaryThumbstickDown = OVRInput.RawButton.LThumbstickDown;
			this.buttonMap.PrimaryThumbstickLeft = OVRInput.RawButton.LThumbstickLeft;
			this.buttonMap.PrimaryThumbstickRight = OVRInput.RawButton.LThumbstickRight;
			this.buttonMap.PrimaryTouchpad = OVRInput.RawButton.None;
			this.buttonMap.SecondaryShoulder = OVRInput.RawButton.None;
			this.buttonMap.SecondaryIndexTrigger = OVRInput.RawButton.RIndexTrigger;
			this.buttonMap.SecondaryHandTrigger = OVRInput.RawButton.RHandTrigger;
			this.buttonMap.SecondaryThumbstick = OVRInput.RawButton.RThumbstick;
			this.buttonMap.SecondaryThumbstickUp = OVRInput.RawButton.RThumbstickUp;
			this.buttonMap.SecondaryThumbstickDown = OVRInput.RawButton.RThumbstickDown;
			this.buttonMap.SecondaryThumbstickLeft = OVRInput.RawButton.RThumbstickLeft;
			this.buttonMap.SecondaryThumbstickRight = OVRInput.RawButton.RThumbstickRight;
			this.buttonMap.SecondaryTouchpad = OVRInput.RawButton.None;
			this.buttonMap.DpadUp = OVRInput.RawButton.None;
			this.buttonMap.DpadDown = OVRInput.RawButton.None;
			this.buttonMap.DpadLeft = OVRInput.RawButton.None;
			this.buttonMap.DpadRight = OVRInput.RawButton.None;
			this.buttonMap.Up = OVRInput.RawButton.LThumbstickUp;
			this.buttonMap.Down = OVRInput.RawButton.LThumbstickDown;
			this.buttonMap.Left = OVRInput.RawButton.LThumbstickLeft;
			this.buttonMap.Right = OVRInput.RawButton.LThumbstickRight;
		}

		
		public override void ConfigureTouchMap()
		{
			this.touchMap.None = OVRInput.RawTouch.None;
			this.touchMap.One = OVRInput.RawTouch.A;
			this.touchMap.Two = OVRInput.RawTouch.B;
			this.touchMap.Three = OVRInput.RawTouch.X;
			this.touchMap.Four = OVRInput.RawTouch.Y;
			this.touchMap.PrimaryIndexTrigger = OVRInput.RawTouch.LIndexTrigger;
			this.touchMap.PrimaryThumbstick = OVRInput.RawTouch.LThumbstick;
			this.touchMap.PrimaryThumbRest = OVRInput.RawTouch.LThumbRest;
			this.touchMap.PrimaryTouchpad = OVRInput.RawTouch.None;
			this.touchMap.SecondaryIndexTrigger = OVRInput.RawTouch.RIndexTrigger;
			this.touchMap.SecondaryThumbstick = OVRInput.RawTouch.RThumbstick;
			this.touchMap.SecondaryThumbRest = OVRInput.RawTouch.RThumbRest;
			this.touchMap.SecondaryTouchpad = OVRInput.RawTouch.None;
		}

		
		public override void ConfigureNearTouchMap()
		{
			this.nearTouchMap.None = OVRInput.RawNearTouch.None;
			this.nearTouchMap.PrimaryIndexTrigger = OVRInput.RawNearTouch.LIndexTrigger;
			this.nearTouchMap.PrimaryThumbButtons = OVRInput.RawNearTouch.LThumbButtons;
			this.nearTouchMap.SecondaryIndexTrigger = OVRInput.RawNearTouch.RIndexTrigger;
			this.nearTouchMap.SecondaryThumbButtons = OVRInput.RawNearTouch.RThumbButtons;
		}

		
		public override void ConfigureAxis1DMap()
		{
			this.axis1DMap.None = OVRInput.RawAxis1D.None;
			this.axis1DMap.PrimaryIndexTrigger = OVRInput.RawAxis1D.LIndexTrigger;
			this.axis1DMap.PrimaryHandTrigger = OVRInput.RawAxis1D.LHandTrigger;
			this.axis1DMap.SecondaryIndexTrigger = OVRInput.RawAxis1D.RIndexTrigger;
			this.axis1DMap.SecondaryHandTrigger = OVRInput.RawAxis1D.RHandTrigger;
		}

		
		public override void ConfigureAxis2DMap()
		{
			this.axis2DMap.None = OVRInput.RawAxis2D.None;
			this.axis2DMap.PrimaryThumbstick = OVRInput.RawAxis2D.LThumbstick;
			this.axis2DMap.PrimaryTouchpad = OVRInput.RawAxis2D.None;
			this.axis2DMap.SecondaryThumbstick = OVRInput.RawAxis2D.RThumbstick;
			this.axis2DMap.SecondaryTouchpad = OVRInput.RawAxis2D.None;
		}
	}

	
	private class OVRControllerLTouch : OVRInput.OVRControllerBase
	{
		
		public OVRControllerLTouch()
		{
			this.controllerType = OVRInput.Controller.LTouch;
		}

		
		public override void ConfigureButtonMap()
		{
			this.buttonMap.None = OVRInput.RawButton.None;
			this.buttonMap.One = OVRInput.RawButton.X;
			this.buttonMap.Two = OVRInput.RawButton.Y;
			this.buttonMap.Three = OVRInput.RawButton.None;
			this.buttonMap.Four = OVRInput.RawButton.None;
			this.buttonMap.Start = OVRInput.RawButton.Start;
			this.buttonMap.Back = OVRInput.RawButton.None;
			this.buttonMap.PrimaryShoulder = OVRInput.RawButton.None;
			this.buttonMap.PrimaryIndexTrigger = OVRInput.RawButton.LIndexTrigger;
			this.buttonMap.PrimaryHandTrigger = OVRInput.RawButton.LHandTrigger;
			this.buttonMap.PrimaryThumbstick = OVRInput.RawButton.LThumbstick;
			this.buttonMap.PrimaryThumbstickUp = OVRInput.RawButton.LThumbstickUp;
			this.buttonMap.PrimaryThumbstickDown = OVRInput.RawButton.LThumbstickDown;
			this.buttonMap.PrimaryThumbstickLeft = OVRInput.RawButton.LThumbstickLeft;
			this.buttonMap.PrimaryThumbstickRight = OVRInput.RawButton.LThumbstickRight;
			this.buttonMap.PrimaryTouchpad = OVRInput.RawButton.None;
			this.buttonMap.SecondaryShoulder = OVRInput.RawButton.None;
			this.buttonMap.SecondaryIndexTrigger = OVRInput.RawButton.None;
			this.buttonMap.SecondaryHandTrigger = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstick = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstickUp = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstickDown = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstickLeft = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstickRight = OVRInput.RawButton.None;
			this.buttonMap.SecondaryTouchpad = OVRInput.RawButton.None;
			this.buttonMap.DpadUp = OVRInput.RawButton.None;
			this.buttonMap.DpadDown = OVRInput.RawButton.None;
			this.buttonMap.DpadLeft = OVRInput.RawButton.None;
			this.buttonMap.DpadRight = OVRInput.RawButton.None;
			this.buttonMap.Up = OVRInput.RawButton.LThumbstickUp;
			this.buttonMap.Down = OVRInput.RawButton.LThumbstickDown;
			this.buttonMap.Left = OVRInput.RawButton.LThumbstickLeft;
			this.buttonMap.Right = OVRInput.RawButton.LThumbstickRight;
		}

		
		public override void ConfigureTouchMap()
		{
			this.touchMap.None = OVRInput.RawTouch.None;
			this.touchMap.One = OVRInput.RawTouch.X;
			this.touchMap.Two = OVRInput.RawTouch.Y;
			this.touchMap.Three = OVRInput.RawTouch.None;
			this.touchMap.Four = OVRInput.RawTouch.None;
			this.touchMap.PrimaryIndexTrigger = OVRInput.RawTouch.LIndexTrigger;
			this.touchMap.PrimaryThumbstick = OVRInput.RawTouch.LThumbstick;
			this.touchMap.PrimaryThumbRest = OVRInput.RawTouch.LThumbRest;
			this.touchMap.PrimaryTouchpad = OVRInput.RawTouch.None;
			this.touchMap.SecondaryIndexTrigger = OVRInput.RawTouch.None;
			this.touchMap.SecondaryThumbstick = OVRInput.RawTouch.None;
			this.touchMap.SecondaryThumbRest = OVRInput.RawTouch.None;
			this.touchMap.SecondaryTouchpad = OVRInput.RawTouch.None;
		}

		
		public override void ConfigureNearTouchMap()
		{
			this.nearTouchMap.None = OVRInput.RawNearTouch.None;
			this.nearTouchMap.PrimaryIndexTrigger = OVRInput.RawNearTouch.LIndexTrigger;
			this.nearTouchMap.PrimaryThumbButtons = OVRInput.RawNearTouch.LThumbButtons;
			this.nearTouchMap.SecondaryIndexTrigger = OVRInput.RawNearTouch.None;
			this.nearTouchMap.SecondaryThumbButtons = OVRInput.RawNearTouch.None;
		}

		
		public override void ConfigureAxis1DMap()
		{
			this.axis1DMap.None = OVRInput.RawAxis1D.None;
			this.axis1DMap.PrimaryIndexTrigger = OVRInput.RawAxis1D.LIndexTrigger;
			this.axis1DMap.PrimaryHandTrigger = OVRInput.RawAxis1D.LHandTrigger;
			this.axis1DMap.SecondaryIndexTrigger = OVRInput.RawAxis1D.None;
			this.axis1DMap.SecondaryHandTrigger = OVRInput.RawAxis1D.None;
		}

		
		public override void ConfigureAxis2DMap()
		{
			this.axis2DMap.None = OVRInput.RawAxis2D.None;
			this.axis2DMap.PrimaryThumbstick = OVRInput.RawAxis2D.LThumbstick;
			this.axis2DMap.PrimaryTouchpad = OVRInput.RawAxis2D.None;
			this.axis2DMap.SecondaryThumbstick = OVRInput.RawAxis2D.None;
			this.axis2DMap.SecondaryTouchpad = OVRInput.RawAxis2D.None;
		}
	}

	
	private class OVRControllerRTouch : OVRInput.OVRControllerBase
	{
		
		public OVRControllerRTouch()
		{
			this.controllerType = OVRInput.Controller.RTouch;
		}

		
		public override void ConfigureButtonMap()
		{
			this.buttonMap.None = OVRInput.RawButton.None;
			this.buttonMap.One = OVRInput.RawButton.A;
			this.buttonMap.Two = OVRInput.RawButton.B;
			this.buttonMap.Three = OVRInput.RawButton.None;
			this.buttonMap.Four = OVRInput.RawButton.None;
			this.buttonMap.Start = OVRInput.RawButton.None;
			this.buttonMap.Back = OVRInput.RawButton.None;
			this.buttonMap.PrimaryShoulder = OVRInput.RawButton.None;
			this.buttonMap.PrimaryIndexTrigger = OVRInput.RawButton.RIndexTrigger;
			this.buttonMap.PrimaryHandTrigger = OVRInput.RawButton.RHandTrigger;
			this.buttonMap.PrimaryThumbstick = OVRInput.RawButton.RThumbstick;
			this.buttonMap.PrimaryThumbstickUp = OVRInput.RawButton.RThumbstickUp;
			this.buttonMap.PrimaryThumbstickDown = OVRInput.RawButton.RThumbstickDown;
			this.buttonMap.PrimaryThumbstickLeft = OVRInput.RawButton.RThumbstickLeft;
			this.buttonMap.PrimaryThumbstickRight = OVRInput.RawButton.RThumbstickRight;
			this.buttonMap.PrimaryTouchpad = OVRInput.RawButton.None;
			this.buttonMap.SecondaryShoulder = OVRInput.RawButton.None;
			this.buttonMap.SecondaryIndexTrigger = OVRInput.RawButton.None;
			this.buttonMap.SecondaryHandTrigger = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstick = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstickUp = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstickDown = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstickLeft = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstickRight = OVRInput.RawButton.None;
			this.buttonMap.SecondaryTouchpad = OVRInput.RawButton.None;
			this.buttonMap.DpadUp = OVRInput.RawButton.None;
			this.buttonMap.DpadDown = OVRInput.RawButton.None;
			this.buttonMap.DpadLeft = OVRInput.RawButton.None;
			this.buttonMap.DpadRight = OVRInput.RawButton.None;
			this.buttonMap.Up = OVRInput.RawButton.RThumbstickUp;
			this.buttonMap.Down = OVRInput.RawButton.RThumbstickDown;
			this.buttonMap.Left = OVRInput.RawButton.RThumbstickLeft;
			this.buttonMap.Right = OVRInput.RawButton.RThumbstickRight;
		}

		
		public override void ConfigureTouchMap()
		{
			this.touchMap.None = OVRInput.RawTouch.None;
			this.touchMap.One = OVRInput.RawTouch.A;
			this.touchMap.Two = OVRInput.RawTouch.B;
			this.touchMap.Three = OVRInput.RawTouch.None;
			this.touchMap.Four = OVRInput.RawTouch.None;
			this.touchMap.PrimaryIndexTrigger = OVRInput.RawTouch.RIndexTrigger;
			this.touchMap.PrimaryThumbstick = OVRInput.RawTouch.RThumbstick;
			this.touchMap.PrimaryThumbRest = OVRInput.RawTouch.RThumbRest;
			this.touchMap.PrimaryTouchpad = OVRInput.RawTouch.None;
			this.touchMap.SecondaryIndexTrigger = OVRInput.RawTouch.None;
			this.touchMap.SecondaryThumbstick = OVRInput.RawTouch.None;
			this.touchMap.SecondaryThumbRest = OVRInput.RawTouch.None;
			this.touchMap.SecondaryTouchpad = OVRInput.RawTouch.None;
		}

		
		public override void ConfigureNearTouchMap()
		{
			this.nearTouchMap.None = OVRInput.RawNearTouch.None;
			this.nearTouchMap.PrimaryIndexTrigger = OVRInput.RawNearTouch.RIndexTrigger;
			this.nearTouchMap.PrimaryThumbButtons = OVRInput.RawNearTouch.RThumbButtons;
			this.nearTouchMap.SecondaryIndexTrigger = OVRInput.RawNearTouch.None;
			this.nearTouchMap.SecondaryThumbButtons = OVRInput.RawNearTouch.None;
		}

		
		public override void ConfigureAxis1DMap()
		{
			this.axis1DMap.None = OVRInput.RawAxis1D.None;
			this.axis1DMap.PrimaryIndexTrigger = OVRInput.RawAxis1D.RIndexTrigger;
			this.axis1DMap.PrimaryHandTrigger = OVRInput.RawAxis1D.RHandTrigger;
			this.axis1DMap.SecondaryIndexTrigger = OVRInput.RawAxis1D.None;
			this.axis1DMap.SecondaryHandTrigger = OVRInput.RawAxis1D.None;
		}

		
		public override void ConfigureAxis2DMap()
		{
			this.axis2DMap.None = OVRInput.RawAxis2D.None;
			this.axis2DMap.PrimaryThumbstick = OVRInput.RawAxis2D.RThumbstick;
			this.axis2DMap.PrimaryTouchpad = OVRInput.RawAxis2D.None;
			this.axis2DMap.SecondaryThumbstick = OVRInput.RawAxis2D.None;
			this.axis2DMap.SecondaryTouchpad = OVRInput.RawAxis2D.None;
		}
	}

	
	private class OVRControllerRemote : OVRInput.OVRControllerBase
	{
		
		public OVRControllerRemote()
		{
			this.controllerType = OVRInput.Controller.Remote;
		}

		
		public override void ConfigureButtonMap()
		{
			this.buttonMap.None = OVRInput.RawButton.None;
			this.buttonMap.One = OVRInput.RawButton.Start;
			this.buttonMap.Two = OVRInput.RawButton.Back;
			this.buttonMap.Three = OVRInput.RawButton.None;
			this.buttonMap.Four = OVRInput.RawButton.None;
			this.buttonMap.Start = OVRInput.RawButton.Start;
			this.buttonMap.Back = OVRInput.RawButton.Back;
			this.buttonMap.PrimaryShoulder = OVRInput.RawButton.None;
			this.buttonMap.PrimaryIndexTrigger = OVRInput.RawButton.None;
			this.buttonMap.PrimaryHandTrigger = OVRInput.RawButton.None;
			this.buttonMap.PrimaryThumbstick = OVRInput.RawButton.None;
			this.buttonMap.PrimaryThumbstickUp = OVRInput.RawButton.None;
			this.buttonMap.PrimaryThumbstickDown = OVRInput.RawButton.None;
			this.buttonMap.PrimaryThumbstickLeft = OVRInput.RawButton.None;
			this.buttonMap.PrimaryThumbstickRight = OVRInput.RawButton.None;
			this.buttonMap.PrimaryTouchpad = OVRInput.RawButton.None;
			this.buttonMap.SecondaryShoulder = OVRInput.RawButton.None;
			this.buttonMap.SecondaryIndexTrigger = OVRInput.RawButton.None;
			this.buttonMap.SecondaryHandTrigger = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstick = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstickUp = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstickDown = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstickLeft = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstickRight = OVRInput.RawButton.None;
			this.buttonMap.SecondaryTouchpad = OVRInput.RawButton.None;
			this.buttonMap.DpadUp = OVRInput.RawButton.DpadUp;
			this.buttonMap.DpadDown = OVRInput.RawButton.DpadDown;
			this.buttonMap.DpadLeft = OVRInput.RawButton.DpadLeft;
			this.buttonMap.DpadRight = OVRInput.RawButton.DpadRight;
			this.buttonMap.Up = OVRInput.RawButton.DpadUp;
			this.buttonMap.Down = OVRInput.RawButton.DpadDown;
			this.buttonMap.Left = OVRInput.RawButton.DpadLeft;
			this.buttonMap.Right = OVRInput.RawButton.DpadRight;
		}

		
		public override void ConfigureTouchMap()
		{
			this.touchMap.None = OVRInput.RawTouch.None;
			this.touchMap.One = OVRInput.RawTouch.None;
			this.touchMap.Two = OVRInput.RawTouch.None;
			this.touchMap.Three = OVRInput.RawTouch.None;
			this.touchMap.Four = OVRInput.RawTouch.None;
			this.touchMap.PrimaryIndexTrigger = OVRInput.RawTouch.None;
			this.touchMap.PrimaryThumbstick = OVRInput.RawTouch.None;
			this.touchMap.PrimaryThumbRest = OVRInput.RawTouch.None;
			this.touchMap.PrimaryTouchpad = OVRInput.RawTouch.None;
			this.touchMap.SecondaryIndexTrigger = OVRInput.RawTouch.None;
			this.touchMap.SecondaryThumbstick = OVRInput.RawTouch.None;
			this.touchMap.SecondaryThumbRest = OVRInput.RawTouch.None;
			this.touchMap.SecondaryTouchpad = OVRInput.RawTouch.None;
		}

		
		public override void ConfigureNearTouchMap()
		{
			this.nearTouchMap.None = OVRInput.RawNearTouch.None;
			this.nearTouchMap.PrimaryIndexTrigger = OVRInput.RawNearTouch.None;
			this.nearTouchMap.PrimaryThumbButtons = OVRInput.RawNearTouch.None;
			this.nearTouchMap.SecondaryIndexTrigger = OVRInput.RawNearTouch.None;
			this.nearTouchMap.SecondaryThumbButtons = OVRInput.RawNearTouch.None;
		}

		
		public override void ConfigureAxis1DMap()
		{
			this.axis1DMap.None = OVRInput.RawAxis1D.None;
			this.axis1DMap.PrimaryIndexTrigger = OVRInput.RawAxis1D.None;
			this.axis1DMap.PrimaryHandTrigger = OVRInput.RawAxis1D.None;
			this.axis1DMap.SecondaryIndexTrigger = OVRInput.RawAxis1D.None;
			this.axis1DMap.SecondaryHandTrigger = OVRInput.RawAxis1D.None;
		}

		
		public override void ConfigureAxis2DMap()
		{
			this.axis2DMap.None = OVRInput.RawAxis2D.None;
			this.axis2DMap.PrimaryThumbstick = OVRInput.RawAxis2D.None;
			this.axis2DMap.PrimaryTouchpad = OVRInput.RawAxis2D.None;
			this.axis2DMap.SecondaryThumbstick = OVRInput.RawAxis2D.None;
			this.axis2DMap.SecondaryTouchpad = OVRInput.RawAxis2D.None;
		}
	}

	
	private class OVRControllerGamepadPC : OVRInput.OVRControllerBase
	{
		
		public OVRControllerGamepadPC()
		{
			this.controllerType = OVRInput.Controller.Gamepad;
		}

		
		public override void ConfigureButtonMap()
		{
			this.buttonMap.None = OVRInput.RawButton.None;
			this.buttonMap.One = OVRInput.RawButton.A;
			this.buttonMap.Two = OVRInput.RawButton.B;
			this.buttonMap.Three = OVRInput.RawButton.X;
			this.buttonMap.Four = OVRInput.RawButton.Y;
			this.buttonMap.Start = OVRInput.RawButton.Start;
			this.buttonMap.Back = OVRInput.RawButton.Back;
			this.buttonMap.PrimaryShoulder = OVRInput.RawButton.LShoulder;
			this.buttonMap.PrimaryIndexTrigger = OVRInput.RawButton.LIndexTrigger;
			this.buttonMap.PrimaryHandTrigger = OVRInput.RawButton.None;
			this.buttonMap.PrimaryThumbstick = OVRInput.RawButton.LThumbstick;
			this.buttonMap.PrimaryThumbstickUp = OVRInput.RawButton.LThumbstickUp;
			this.buttonMap.PrimaryThumbstickDown = OVRInput.RawButton.LThumbstickDown;
			this.buttonMap.PrimaryThumbstickLeft = OVRInput.RawButton.LThumbstickLeft;
			this.buttonMap.PrimaryThumbstickRight = OVRInput.RawButton.LThumbstickRight;
			this.buttonMap.PrimaryTouchpad = OVRInput.RawButton.None;
			this.buttonMap.SecondaryShoulder = OVRInput.RawButton.RShoulder;
			this.buttonMap.SecondaryIndexTrigger = OVRInput.RawButton.RIndexTrigger;
			this.buttonMap.SecondaryHandTrigger = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstick = OVRInput.RawButton.RThumbstick;
			this.buttonMap.SecondaryThumbstickUp = OVRInput.RawButton.RThumbstickUp;
			this.buttonMap.SecondaryThumbstickDown = OVRInput.RawButton.RThumbstickDown;
			this.buttonMap.SecondaryThumbstickLeft = OVRInput.RawButton.RThumbstickLeft;
			this.buttonMap.SecondaryThumbstickRight = OVRInput.RawButton.RThumbstickRight;
			this.buttonMap.SecondaryTouchpad = OVRInput.RawButton.None;
			this.buttonMap.DpadUp = OVRInput.RawButton.DpadUp;
			this.buttonMap.DpadDown = OVRInput.RawButton.DpadDown;
			this.buttonMap.DpadLeft = OVRInput.RawButton.DpadLeft;
			this.buttonMap.DpadRight = OVRInput.RawButton.DpadRight;
			this.buttonMap.Up = OVRInput.RawButton.LThumbstickUp;
			this.buttonMap.Down = OVRInput.RawButton.LThumbstickDown;
			this.buttonMap.Left = OVRInput.RawButton.LThumbstickLeft;
			this.buttonMap.Right = OVRInput.RawButton.LThumbstickRight;
		}

		
		public override void ConfigureTouchMap()
		{
			this.touchMap.None = OVRInput.RawTouch.None;
			this.touchMap.One = OVRInput.RawTouch.None;
			this.touchMap.Two = OVRInput.RawTouch.None;
			this.touchMap.Three = OVRInput.RawTouch.None;
			this.touchMap.Four = OVRInput.RawTouch.None;
			this.touchMap.PrimaryIndexTrigger = OVRInput.RawTouch.None;
			this.touchMap.PrimaryThumbstick = OVRInput.RawTouch.None;
			this.touchMap.PrimaryThumbRest = OVRInput.RawTouch.None;
			this.touchMap.PrimaryTouchpad = OVRInput.RawTouch.None;
			this.touchMap.SecondaryIndexTrigger = OVRInput.RawTouch.None;
			this.touchMap.SecondaryThumbstick = OVRInput.RawTouch.None;
			this.touchMap.SecondaryThumbRest = OVRInput.RawTouch.None;
			this.touchMap.SecondaryTouchpad = OVRInput.RawTouch.None;
		}

		
		public override void ConfigureNearTouchMap()
		{
			this.nearTouchMap.None = OVRInput.RawNearTouch.None;
			this.nearTouchMap.PrimaryIndexTrigger = OVRInput.RawNearTouch.None;
			this.nearTouchMap.PrimaryThumbButtons = OVRInput.RawNearTouch.None;
			this.nearTouchMap.SecondaryIndexTrigger = OVRInput.RawNearTouch.None;
			this.nearTouchMap.SecondaryThumbButtons = OVRInput.RawNearTouch.None;
		}

		
		public override void ConfigureAxis1DMap()
		{
			this.axis1DMap.None = OVRInput.RawAxis1D.None;
			this.axis1DMap.PrimaryIndexTrigger = OVRInput.RawAxis1D.LIndexTrigger;
			this.axis1DMap.PrimaryHandTrigger = OVRInput.RawAxis1D.None;
			this.axis1DMap.SecondaryIndexTrigger = OVRInput.RawAxis1D.RIndexTrigger;
			this.axis1DMap.SecondaryHandTrigger = OVRInput.RawAxis1D.None;
		}

		
		public override void ConfigureAxis2DMap()
		{
			this.axis2DMap.None = OVRInput.RawAxis2D.None;
			this.axis2DMap.PrimaryThumbstick = OVRInput.RawAxis2D.LThumbstick;
			this.axis2DMap.PrimaryTouchpad = OVRInput.RawAxis2D.None;
			this.axis2DMap.SecondaryThumbstick = OVRInput.RawAxis2D.RThumbstick;
			this.axis2DMap.SecondaryTouchpad = OVRInput.RawAxis2D.None;
		}
	}

	
	private class OVRControllerGamepadMac : OVRInput.OVRControllerBase
	{
		
		public OVRControllerGamepadMac()
		{
			this.controllerType = OVRInput.Controller.Gamepad;
			this.initialized = OVRInput.OVRControllerGamepadMac.OVR_GamepadController_Initialize();
		}

		
		~OVRControllerGamepadMac()
		{
			if (this.initialized)
			{
				OVRInput.OVRControllerGamepadMac.OVR_GamepadController_Destroy();
			}
		}

		
		public override OVRInput.Controller Update()
		{
			if (!this.initialized)
			{
				return OVRInput.Controller.None;
			}
			OVRPlugin.ControllerState4 currentState = default(OVRPlugin.ControllerState4);
			bool flag = OVRInput.OVRControllerGamepadMac.OVR_GamepadController_Update();
			if (flag)
			{
				currentState.ConnectedControllers = 16u;
			}
			if (OVRInput.OVRControllerGamepadMac.OVR_GamepadController_GetButton(0))
			{
				currentState.Buttons |= 1u;
			}
			if (OVRInput.OVRControllerGamepadMac.OVR_GamepadController_GetButton(1))
			{
				currentState.Buttons |= 2u;
			}
			if (OVRInput.OVRControllerGamepadMac.OVR_GamepadController_GetButton(2))
			{
				currentState.Buttons |= 256u;
			}
			if (OVRInput.OVRControllerGamepadMac.OVR_GamepadController_GetButton(3))
			{
				currentState.Buttons |= 512u;
			}
			if (OVRInput.OVRControllerGamepadMac.OVR_GamepadController_GetButton(4))
			{
				currentState.Buttons |= 65536u;
			}
			if (OVRInput.OVRControllerGamepadMac.OVR_GamepadController_GetButton(5))
			{
				currentState.Buttons |= 131072u;
			}
			if (OVRInput.OVRControllerGamepadMac.OVR_GamepadController_GetButton(6))
			{
				currentState.Buttons |= 262144u;
			}
			if (OVRInput.OVRControllerGamepadMac.OVR_GamepadController_GetButton(7))
			{
				currentState.Buttons |= 524288u;
			}
			if (OVRInput.OVRControllerGamepadMac.OVR_GamepadController_GetButton(8))
			{
				currentState.Buttons |= 1048576u;
			}
			if (OVRInput.OVRControllerGamepadMac.OVR_GamepadController_GetButton(9))
			{
				currentState.Buttons |= 2097152u;
			}
			if (OVRInput.OVRControllerGamepadMac.OVR_GamepadController_GetButton(10))
			{
				currentState.Buttons |= 1024u;
			}
			if (OVRInput.OVRControllerGamepadMac.OVR_GamepadController_GetButton(11))
			{
				currentState.Buttons |= 4u;
			}
			if (OVRInput.OVRControllerGamepadMac.OVR_GamepadController_GetButton(12))
			{
				currentState.Buttons |= 2048u;
			}
			if (OVRInput.OVRControllerGamepadMac.OVR_GamepadController_GetButton(13))
			{
				currentState.Buttons |= 8u;
			}
			currentState.LThumbstick.x = OVRInput.OVRControllerGamepadMac.OVR_GamepadController_GetAxis(0);
			currentState.LThumbstick.y = OVRInput.OVRControllerGamepadMac.OVR_GamepadController_GetAxis(1);
			currentState.RThumbstick.x = OVRInput.OVRControllerGamepadMac.OVR_GamepadController_GetAxis(2);
			currentState.RThumbstick.y = OVRInput.OVRControllerGamepadMac.OVR_GamepadController_GetAxis(3);
			currentState.LIndexTrigger = OVRInput.OVRControllerGamepadMac.OVR_GamepadController_GetAxis(4);
			currentState.RIndexTrigger = OVRInput.OVRControllerGamepadMac.OVR_GamepadController_GetAxis(5);
			if (currentState.LIndexTrigger >= OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				currentState.Buttons |= 268435456u;
			}
			if (currentState.LHandTrigger >= OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				currentState.Buttons |= 536870912u;
			}
			if (currentState.LThumbstick.y >= OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				currentState.Buttons |= 16u;
			}
			if (currentState.LThumbstick.y <= -OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				currentState.Buttons |= 32u;
			}
			if (currentState.LThumbstick.x <= -OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				currentState.Buttons |= 64u;
			}
			if (currentState.LThumbstick.x >= OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				currentState.Buttons |= 128u;
			}
			if (currentState.RIndexTrigger >= OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				currentState.Buttons |= 67108864u;
			}
			if (currentState.RHandTrigger >= OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				currentState.Buttons |= 134217728u;
			}
			if (currentState.RThumbstick.y >= OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				currentState.Buttons |= 4096u;
			}
			if (currentState.RThumbstick.y <= -OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				currentState.Buttons |= 8192u;
			}
			if (currentState.RThumbstick.x <= -OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				currentState.Buttons |= 16384u;
			}
			if (currentState.RThumbstick.x >= OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				currentState.Buttons |= 32768u;
			}
			this.previousState = this.currentState;
			this.currentState = currentState;
			return (OVRInput.Controller)(this.currentState.ConnectedControllers & (uint)this.controllerType);
		}

		
		public override void ConfigureButtonMap()
		{
			this.buttonMap.None = OVRInput.RawButton.None;
			this.buttonMap.One = OVRInput.RawButton.A;
			this.buttonMap.Two = OVRInput.RawButton.B;
			this.buttonMap.Three = OVRInput.RawButton.X;
			this.buttonMap.Four = OVRInput.RawButton.Y;
			this.buttonMap.Start = OVRInput.RawButton.Start;
			this.buttonMap.Back = OVRInput.RawButton.Back;
			this.buttonMap.PrimaryShoulder = OVRInput.RawButton.LShoulder;
			this.buttonMap.PrimaryIndexTrigger = OVRInput.RawButton.LIndexTrigger;
			this.buttonMap.PrimaryHandTrigger = OVRInput.RawButton.None;
			this.buttonMap.PrimaryThumbstick = OVRInput.RawButton.LThumbstick;
			this.buttonMap.PrimaryThumbstickUp = OVRInput.RawButton.LThumbstickUp;
			this.buttonMap.PrimaryThumbstickDown = OVRInput.RawButton.LThumbstickDown;
			this.buttonMap.PrimaryThumbstickLeft = OVRInput.RawButton.LThumbstickLeft;
			this.buttonMap.PrimaryThumbstickRight = OVRInput.RawButton.LThumbstickRight;
			this.buttonMap.PrimaryTouchpad = OVRInput.RawButton.None;
			this.buttonMap.SecondaryShoulder = OVRInput.RawButton.RShoulder;
			this.buttonMap.SecondaryIndexTrigger = OVRInput.RawButton.RIndexTrigger;
			this.buttonMap.SecondaryHandTrigger = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstick = OVRInput.RawButton.RThumbstick;
			this.buttonMap.SecondaryThumbstickUp = OVRInput.RawButton.RThumbstickUp;
			this.buttonMap.SecondaryThumbstickDown = OVRInput.RawButton.RThumbstickDown;
			this.buttonMap.SecondaryThumbstickLeft = OVRInput.RawButton.RThumbstickLeft;
			this.buttonMap.SecondaryThumbstickRight = OVRInput.RawButton.RThumbstickRight;
			this.buttonMap.SecondaryTouchpad = OVRInput.RawButton.None;
			this.buttonMap.DpadUp = OVRInput.RawButton.DpadUp;
			this.buttonMap.DpadDown = OVRInput.RawButton.DpadDown;
			this.buttonMap.DpadLeft = OVRInput.RawButton.DpadLeft;
			this.buttonMap.DpadRight = OVRInput.RawButton.DpadRight;
			this.buttonMap.Up = OVRInput.RawButton.LThumbstickUp;
			this.buttonMap.Down = OVRInput.RawButton.LThumbstickDown;
			this.buttonMap.Left = OVRInput.RawButton.LThumbstickLeft;
			this.buttonMap.Right = OVRInput.RawButton.LThumbstickRight;
		}

		
		public override void ConfigureTouchMap()
		{
			this.touchMap.None = OVRInput.RawTouch.None;
			this.touchMap.One = OVRInput.RawTouch.None;
			this.touchMap.Two = OVRInput.RawTouch.None;
			this.touchMap.Three = OVRInput.RawTouch.None;
			this.touchMap.Four = OVRInput.RawTouch.None;
			this.touchMap.PrimaryIndexTrigger = OVRInput.RawTouch.None;
			this.touchMap.PrimaryThumbstick = OVRInput.RawTouch.None;
			this.touchMap.PrimaryThumbRest = OVRInput.RawTouch.None;
			this.touchMap.PrimaryTouchpad = OVRInput.RawTouch.None;
			this.touchMap.SecondaryIndexTrigger = OVRInput.RawTouch.None;
			this.touchMap.SecondaryThumbstick = OVRInput.RawTouch.None;
			this.touchMap.SecondaryThumbRest = OVRInput.RawTouch.None;
			this.touchMap.SecondaryTouchpad = OVRInput.RawTouch.None;
		}

		
		public override void ConfigureNearTouchMap()
		{
			this.nearTouchMap.None = OVRInput.RawNearTouch.None;
			this.nearTouchMap.PrimaryIndexTrigger = OVRInput.RawNearTouch.None;
			this.nearTouchMap.PrimaryThumbButtons = OVRInput.RawNearTouch.None;
			this.nearTouchMap.SecondaryIndexTrigger = OVRInput.RawNearTouch.None;
			this.nearTouchMap.SecondaryThumbButtons = OVRInput.RawNearTouch.None;
		}

		
		public override void ConfigureAxis1DMap()
		{
			this.axis1DMap.None = OVRInput.RawAxis1D.None;
			this.axis1DMap.PrimaryIndexTrigger = OVRInput.RawAxis1D.LIndexTrigger;
			this.axis1DMap.PrimaryHandTrigger = OVRInput.RawAxis1D.None;
			this.axis1DMap.SecondaryIndexTrigger = OVRInput.RawAxis1D.RIndexTrigger;
			this.axis1DMap.SecondaryHandTrigger = OVRInput.RawAxis1D.None;
		}

		
		public override void ConfigureAxis2DMap()
		{
			this.axis2DMap.None = OVRInput.RawAxis2D.None;
			this.axis2DMap.PrimaryThumbstick = OVRInput.RawAxis2D.LThumbstick;
			this.axis2DMap.PrimaryTouchpad = OVRInput.RawAxis2D.None;
			this.axis2DMap.SecondaryThumbstick = OVRInput.RawAxis2D.RThumbstick;
			this.axis2DMap.SecondaryTouchpad = OVRInput.RawAxis2D.None;
		}

		
		public override void SetControllerVibration(float frequency, float amplitude)
		{
			int node = 0;
			float frequency2 = frequency * 200f;
			OVRInput.OVRControllerGamepadMac.OVR_GamepadController_SetVibration(node, amplitude, frequency2);
		}

		
		[DllImport("OVRGamepad", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool OVR_GamepadController_Initialize();

		
		[DllImport("OVRGamepad", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool OVR_GamepadController_Destroy();

		
		[DllImport("OVRGamepad", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool OVR_GamepadController_Update();

		
		[DllImport("OVRGamepad", CallingConvention = CallingConvention.Cdecl)]
		private static extern float OVR_GamepadController_GetAxis(int axis);

		
		[DllImport("OVRGamepad", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool OVR_GamepadController_GetButton(int button);

		
		[DllImport("OVRGamepad", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool OVR_GamepadController_SetVibration(int node, float strength, float frequency);

		
		private bool initialized;

		
		private const string DllName = "OVRGamepad";

		
		private enum AxisGPC
		{
			
			None = -1,
			
			LeftXAxis,
			
			LeftYAxis,
			
			RightXAxis,
			
			RightYAxis,
			
			LeftTrigger,
			
			RightTrigger,
			
			DPad_X_Axis,
			
			DPad_Y_Axis,
			
			Max
		}

		
		public enum ButtonGPC
		{
			
			None = -1,
			
			A,
			
			B,
			
			X,
			
			Y,
			
			Up,
			
			Down,
			
			Left,
			
			Right,
			
			Start,
			
			Back,
			
			LStick,
			
			RStick,
			
			LeftShoulder,
			
			RightShoulder,
			
			Max
		}
	}

	
	private class OVRControllerGamepadAndroid : OVRInput.OVRControllerBase
	{
		
		public OVRControllerGamepadAndroid()
		{
			this.controllerType = OVRInput.Controller.Gamepad;
		}

		
		private bool ShouldUpdate()
		{
			if (!this.joystickCheckInitialized || Time.realtimeSinceStartup - this.joystickCheckTime > this.joystickCheckInterval)
			{
				this.joystickCheckInitialized = true;
				this.joystickCheckTime = Time.realtimeSinceStartup;
				this.joystickDetected = false;
				string[] joystickNames = Input.GetJoystickNames();
				for (int i = 0; i < joystickNames.Length; i++)
				{
					if (joystickNames[i] != string.Empty && joystickNames[i].IndexOf("<0x", StringComparison.OrdinalIgnoreCase) == -1 && joystickNames[i] != "manufacturer: Oculus HMD" && joystickNames[i] != "Oculus HMD" && joystickNames[i] != "Oculus Tracked Remote - Right" && joystickNames[i] != "Oculus Tracked Remote - Left")
					{
						this.joystickDetected = true;
						break;
					}
				}
			}
			return this.joystickDetected;
		}

		
		public override OVRInput.Controller Update()
		{
			if (!this.ShouldUpdate())
			{
				return OVRInput.Controller.None;
			}
			OVRPlugin.ControllerState4 currentState = default(OVRPlugin.ControllerState4);
			currentState.ConnectedControllers = 16u;
			if (Input.GetKey(OVRInput.OVRControllerGamepadAndroid.AndroidButtonNames.A))
			{
				currentState.Buttons |= 1u;
			}
			if (Input.GetKey(OVRInput.OVRControllerGamepadAndroid.AndroidButtonNames.B))
			{
				currentState.Buttons |= 2u;
			}
			if (Input.GetKey(OVRInput.OVRControllerGamepadAndroid.AndroidButtonNames.X))
			{
				currentState.Buttons |= 256u;
			}
			if (Input.GetKey(OVRInput.OVRControllerGamepadAndroid.AndroidButtonNames.Y))
			{
				currentState.Buttons |= 512u;
			}
			if (Input.GetKey(OVRInput.OVRControllerGamepadAndroid.AndroidButtonNames.Start))
			{
				currentState.Buttons |= 1048576u;
			}
			if (Input.GetKey(OVRInput.OVRControllerGamepadAndroid.AndroidButtonNames.Back) || Input.GetKey(KeyCode.Escape))
			{
				currentState.Buttons |= 2097152u;
			}
			if (Input.GetKey(OVRInput.OVRControllerGamepadAndroid.AndroidButtonNames.LThumbstick))
			{
				currentState.Buttons |= 1024u;
			}
			if (Input.GetKey(OVRInput.OVRControllerGamepadAndroid.AndroidButtonNames.RThumbstick))
			{
				currentState.Buttons |= 4u;
			}
			if (Input.GetKey(OVRInput.OVRControllerGamepadAndroid.AndroidButtonNames.LShoulder))
			{
				currentState.Buttons |= 2048u;
			}
			if (Input.GetKey(OVRInput.OVRControllerGamepadAndroid.AndroidButtonNames.RShoulder))
			{
				currentState.Buttons |= 8u;
			}
			currentState.LThumbstick.x = Input.GetAxisRaw(OVRInput.OVRControllerGamepadAndroid.AndroidAxisNames.LThumbstickX);
			currentState.LThumbstick.y = Input.GetAxisRaw(OVRInput.OVRControllerGamepadAndroid.AndroidAxisNames.LThumbstickY);
			currentState.RThumbstick.x = Input.GetAxisRaw(OVRInput.OVRControllerGamepadAndroid.AndroidAxisNames.RThumbstickX);
			currentState.RThumbstick.y = Input.GetAxisRaw(OVRInput.OVRControllerGamepadAndroid.AndroidAxisNames.RThumbstickY);
			currentState.LIndexTrigger = Input.GetAxisRaw(OVRInput.OVRControllerGamepadAndroid.AndroidAxisNames.LIndexTrigger);
			currentState.RIndexTrigger = Input.GetAxisRaw(OVRInput.OVRControllerGamepadAndroid.AndroidAxisNames.RIndexTrigger);
			if (currentState.LIndexTrigger >= OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				currentState.Buttons |= 268435456u;
			}
			if (currentState.LHandTrigger >= OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				currentState.Buttons |= 536870912u;
			}
			if (currentState.LThumbstick.y >= OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				currentState.Buttons |= 16u;
			}
			if (currentState.LThumbstick.y <= -OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				currentState.Buttons |= 32u;
			}
			if (currentState.LThumbstick.x <= -OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				currentState.Buttons |= 64u;
			}
			if (currentState.LThumbstick.x >= OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				currentState.Buttons |= 128u;
			}
			if (currentState.RIndexTrigger >= OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				currentState.Buttons |= 67108864u;
			}
			if (currentState.RHandTrigger >= OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				currentState.Buttons |= 134217728u;
			}
			if (currentState.RThumbstick.y >= OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				currentState.Buttons |= 4096u;
			}
			if (currentState.RThumbstick.y <= -OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				currentState.Buttons |= 8192u;
			}
			if (currentState.RThumbstick.x <= -OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				currentState.Buttons |= 16384u;
			}
			if (currentState.RThumbstick.x >= OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				currentState.Buttons |= 32768u;
			}
			float axisRaw = Input.GetAxisRaw(OVRInput.OVRControllerGamepadAndroid.AndroidAxisNames.DpadX);
			float axisRaw2 = Input.GetAxisRaw(OVRInput.OVRControllerGamepadAndroid.AndroidAxisNames.DpadY);
			if (axisRaw <= -OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				currentState.Buttons |= 262144u;
			}
			if (axisRaw >= OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				currentState.Buttons |= 524288u;
			}
			if (axisRaw2 <= -OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				currentState.Buttons |= 131072u;
			}
			if (axisRaw2 >= OVRInput.AXIS_AS_BUTTON_THRESHOLD)
			{
				currentState.Buttons |= 65536u;
			}
			this.previousState = this.currentState;
			this.currentState = currentState;
			return (OVRInput.Controller)(this.currentState.ConnectedControllers & (uint)this.controllerType);
		}

		
		public override void ConfigureButtonMap()
		{
			this.buttonMap.None = OVRInput.RawButton.None;
			this.buttonMap.One = OVRInput.RawButton.A;
			this.buttonMap.Two = OVRInput.RawButton.B;
			this.buttonMap.Three = OVRInput.RawButton.X;
			this.buttonMap.Four = OVRInput.RawButton.Y;
			this.buttonMap.Start = OVRInput.RawButton.Start;
			this.buttonMap.Back = OVRInput.RawButton.Back;
			this.buttonMap.PrimaryShoulder = OVRInput.RawButton.LShoulder;
			this.buttonMap.PrimaryIndexTrigger = OVRInput.RawButton.LIndexTrigger;
			this.buttonMap.PrimaryHandTrigger = OVRInput.RawButton.None;
			this.buttonMap.PrimaryThumbstick = OVRInput.RawButton.LThumbstick;
			this.buttonMap.PrimaryThumbstickUp = OVRInput.RawButton.LThumbstickUp;
			this.buttonMap.PrimaryThumbstickDown = OVRInput.RawButton.LThumbstickDown;
			this.buttonMap.PrimaryThumbstickLeft = OVRInput.RawButton.LThumbstickLeft;
			this.buttonMap.PrimaryThumbstickRight = OVRInput.RawButton.LThumbstickRight;
			this.buttonMap.PrimaryTouchpad = OVRInput.RawButton.None;
			this.buttonMap.SecondaryShoulder = OVRInput.RawButton.RShoulder;
			this.buttonMap.SecondaryIndexTrigger = OVRInput.RawButton.RIndexTrigger;
			this.buttonMap.SecondaryHandTrigger = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstick = OVRInput.RawButton.RThumbstick;
			this.buttonMap.SecondaryThumbstickUp = OVRInput.RawButton.RThumbstickUp;
			this.buttonMap.SecondaryThumbstickDown = OVRInput.RawButton.RThumbstickDown;
			this.buttonMap.SecondaryThumbstickLeft = OVRInput.RawButton.RThumbstickLeft;
			this.buttonMap.SecondaryThumbstickRight = OVRInput.RawButton.RThumbstickRight;
			this.buttonMap.SecondaryTouchpad = OVRInput.RawButton.None;
			this.buttonMap.DpadUp = OVRInput.RawButton.DpadUp;
			this.buttonMap.DpadDown = OVRInput.RawButton.DpadDown;
			this.buttonMap.DpadLeft = OVRInput.RawButton.DpadLeft;
			this.buttonMap.DpadRight = OVRInput.RawButton.DpadRight;
			this.buttonMap.Up = OVRInput.RawButton.LThumbstickUp;
			this.buttonMap.Down = OVRInput.RawButton.LThumbstickDown;
			this.buttonMap.Left = OVRInput.RawButton.LThumbstickLeft;
			this.buttonMap.Right = OVRInput.RawButton.LThumbstickRight;
		}

		
		public override void ConfigureTouchMap()
		{
			this.touchMap.None = OVRInput.RawTouch.None;
			this.touchMap.One = OVRInput.RawTouch.None;
			this.touchMap.Two = OVRInput.RawTouch.None;
			this.touchMap.Three = OVRInput.RawTouch.None;
			this.touchMap.Four = OVRInput.RawTouch.None;
			this.touchMap.PrimaryIndexTrigger = OVRInput.RawTouch.None;
			this.touchMap.PrimaryThumbstick = OVRInput.RawTouch.None;
			this.touchMap.PrimaryThumbRest = OVRInput.RawTouch.None;
			this.touchMap.PrimaryTouchpad = OVRInput.RawTouch.None;
			this.touchMap.SecondaryIndexTrigger = OVRInput.RawTouch.None;
			this.touchMap.SecondaryThumbstick = OVRInput.RawTouch.None;
			this.touchMap.SecondaryThumbRest = OVRInput.RawTouch.None;
			this.touchMap.SecondaryTouchpad = OVRInput.RawTouch.None;
		}

		
		public override void ConfigureNearTouchMap()
		{
			this.nearTouchMap.None = OVRInput.RawNearTouch.None;
			this.nearTouchMap.PrimaryIndexTrigger = OVRInput.RawNearTouch.None;
			this.nearTouchMap.PrimaryThumbButtons = OVRInput.RawNearTouch.None;
			this.nearTouchMap.SecondaryIndexTrigger = OVRInput.RawNearTouch.None;
			this.nearTouchMap.SecondaryThumbButtons = OVRInput.RawNearTouch.None;
		}

		
		public override void ConfigureAxis1DMap()
		{
			this.axis1DMap.None = OVRInput.RawAxis1D.None;
			this.axis1DMap.PrimaryIndexTrigger = OVRInput.RawAxis1D.LIndexTrigger;
			this.axis1DMap.PrimaryHandTrigger = OVRInput.RawAxis1D.None;
			this.axis1DMap.SecondaryIndexTrigger = OVRInput.RawAxis1D.RIndexTrigger;
			this.axis1DMap.SecondaryHandTrigger = OVRInput.RawAxis1D.None;
		}

		
		public override void ConfigureAxis2DMap()
		{
			this.axis2DMap.None = OVRInput.RawAxis2D.None;
			this.axis2DMap.PrimaryThumbstick = OVRInput.RawAxis2D.LThumbstick;
			this.axis2DMap.PrimaryTouchpad = OVRInput.RawAxis2D.None;
			this.axis2DMap.SecondaryThumbstick = OVRInput.RawAxis2D.RThumbstick;
			this.axis2DMap.SecondaryTouchpad = OVRInput.RawAxis2D.None;
		}

		
		public override void SetControllerVibration(float frequency, float amplitude)
		{
		}

		
		private bool joystickDetected;

		
		private bool joystickCheckInitialized;

		
		private float joystickCheckInterval = 1f;

		
		private float joystickCheckTime;

		
		private static class AndroidButtonNames
		{
			
			public static readonly KeyCode A = KeyCode.JoystickButton0;

			
			public static readonly KeyCode B = KeyCode.JoystickButton1;

			
			public static readonly KeyCode X = KeyCode.JoystickButton2;

			
			public static readonly KeyCode Y = KeyCode.JoystickButton3;

			
			public static readonly KeyCode Start = KeyCode.JoystickButton10;

			
			public static readonly KeyCode Back = KeyCode.JoystickButton11;

			
			public static readonly KeyCode LThumbstick = KeyCode.JoystickButton8;

			
			public static readonly KeyCode RThumbstick = KeyCode.JoystickButton9;

			
			public static readonly KeyCode LShoulder = KeyCode.JoystickButton4;

			
			public static readonly KeyCode RShoulder = KeyCode.JoystickButton5;
		}

		
		private static class AndroidAxisNames
		{
			
			public static readonly string LThumbstickX = "Oculus_GearVR_LThumbstickX";

			
			public static readonly string LThumbstickY = "Oculus_GearVR_LThumbstickY";

			
			public static readonly string RThumbstickX = "Oculus_GearVR_RThumbstickX";

			
			public static readonly string RThumbstickY = "Oculus_GearVR_RThumbstickY";

			
			public static readonly string LIndexTrigger = "Oculus_GearVR_LIndexTrigger";

			
			public static readonly string RIndexTrigger = "Oculus_GearVR_RIndexTrigger";

			
			public static readonly string DpadX = "Oculus_GearVR_DpadX";

			
			public static readonly string DpadY = "Oculus_GearVR_DpadY";
		}
	}

	
	private class OVRControllerTouchpad : OVRInput.OVRControllerBase
	{
		
		public OVRControllerTouchpad()
		{
			this.controllerType = OVRInput.Controller.Touchpad;
		}

		
		public override OVRInput.Controller Update()
		{
			OVRInput.Controller result = base.Update();
			if (OVRInput.GetDown(OVRInput.RawTouch.LTouchpad, OVRInput.Controller.Touchpad))
			{
				this.moveAmount = this.currentState.LTouchpad;
			}
			if (OVRInput.GetUp(OVRInput.RawTouch.LTouchpad, OVRInput.Controller.Touchpad))
			{
				this.moveAmount.x = this.previousState.LTouchpad.x - this.moveAmount.x;
				this.moveAmount.y = this.previousState.LTouchpad.y - this.moveAmount.y;
				Vector2 vector = new Vector2(this.moveAmount.x, this.moveAmount.y);
				float magnitude = vector.magnitude;
				if (magnitude < this.maxTapMagnitude)
				{
					this.currentState.Buttons = (this.currentState.Buttons | 1048576u);
					this.currentState.Buttons = (this.currentState.Buttons | 1073741824u);
				}
				else if (magnitude >= this.minMoveMagnitude)
				{
					vector.Normalize();
					if (Mathf.Abs(vector.x) > Mathf.Abs(vector.y))
					{
						if (vector.x < 0f)
						{
							this.currentState.Buttons = (this.currentState.Buttons | 262144u);
						}
						else
						{
							this.currentState.Buttons = (this.currentState.Buttons | 524288u);
						}
					}
					else if (vector.y < 0f)
					{
						this.currentState.Buttons = (this.currentState.Buttons | 131072u);
					}
					else
					{
						this.currentState.Buttons = (this.currentState.Buttons | 65536u);
					}
				}
			}
			return result;
		}

		
		public override void ConfigureButtonMap()
		{
			this.buttonMap.None = OVRInput.RawButton.None;
			this.buttonMap.One = OVRInput.RawButton.LTouchpad;
			this.buttonMap.Two = OVRInput.RawButton.Back;
			this.buttonMap.Three = OVRInput.RawButton.None;
			this.buttonMap.Four = OVRInput.RawButton.None;
			this.buttonMap.Start = OVRInput.RawButton.Start;
			this.buttonMap.Back = OVRInput.RawButton.Back;
			this.buttonMap.PrimaryShoulder = OVRInput.RawButton.None;
			this.buttonMap.PrimaryIndexTrigger = OVRInput.RawButton.None;
			this.buttonMap.PrimaryHandTrigger = OVRInput.RawButton.None;
			this.buttonMap.PrimaryThumbstick = OVRInput.RawButton.None;
			this.buttonMap.PrimaryThumbstickUp = OVRInput.RawButton.None;
			this.buttonMap.PrimaryThumbstickDown = OVRInput.RawButton.None;
			this.buttonMap.PrimaryThumbstickLeft = OVRInput.RawButton.None;
			this.buttonMap.PrimaryThumbstickRight = OVRInput.RawButton.None;
			this.buttonMap.PrimaryTouchpad = OVRInput.RawButton.LTouchpad;
			this.buttonMap.SecondaryShoulder = OVRInput.RawButton.None;
			this.buttonMap.SecondaryIndexTrigger = OVRInput.RawButton.None;
			this.buttonMap.SecondaryHandTrigger = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstick = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstickUp = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstickDown = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstickLeft = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstickRight = OVRInput.RawButton.None;
			this.buttonMap.SecondaryTouchpad = OVRInput.RawButton.None;
			this.buttonMap.DpadUp = OVRInput.RawButton.DpadUp;
			this.buttonMap.DpadDown = OVRInput.RawButton.DpadDown;
			this.buttonMap.DpadLeft = OVRInput.RawButton.DpadLeft;
			this.buttonMap.DpadRight = OVRInput.RawButton.DpadRight;
			this.buttonMap.Up = OVRInput.RawButton.DpadUp;
			this.buttonMap.Down = OVRInput.RawButton.DpadDown;
			this.buttonMap.Left = OVRInput.RawButton.DpadLeft;
			this.buttonMap.Right = OVRInput.RawButton.DpadRight;
		}

		
		public override void ConfigureTouchMap()
		{
			this.touchMap.None = OVRInput.RawTouch.None;
			this.touchMap.One = OVRInput.RawTouch.LTouchpad;
			this.touchMap.Two = OVRInput.RawTouch.None;
			this.touchMap.Three = OVRInput.RawTouch.None;
			this.touchMap.Four = OVRInput.RawTouch.None;
			this.touchMap.PrimaryIndexTrigger = OVRInput.RawTouch.None;
			this.touchMap.PrimaryThumbstick = OVRInput.RawTouch.None;
			this.touchMap.PrimaryThumbRest = OVRInput.RawTouch.None;
			this.touchMap.PrimaryTouchpad = OVRInput.RawTouch.LTouchpad;
			this.touchMap.SecondaryIndexTrigger = OVRInput.RawTouch.None;
			this.touchMap.SecondaryThumbstick = OVRInput.RawTouch.None;
			this.touchMap.SecondaryThumbRest = OVRInput.RawTouch.None;
			this.touchMap.SecondaryTouchpad = OVRInput.RawTouch.None;
		}

		
		public override void ConfigureNearTouchMap()
		{
			this.nearTouchMap.None = OVRInput.RawNearTouch.None;
			this.nearTouchMap.PrimaryIndexTrigger = OVRInput.RawNearTouch.None;
			this.nearTouchMap.PrimaryThumbButtons = OVRInput.RawNearTouch.None;
			this.nearTouchMap.SecondaryIndexTrigger = OVRInput.RawNearTouch.None;
			this.nearTouchMap.SecondaryThumbButtons = OVRInput.RawNearTouch.None;
		}

		
		public override void ConfigureAxis1DMap()
		{
			this.axis1DMap.None = OVRInput.RawAxis1D.None;
			this.axis1DMap.PrimaryIndexTrigger = OVRInput.RawAxis1D.None;
			this.axis1DMap.PrimaryHandTrigger = OVRInput.RawAxis1D.None;
			this.axis1DMap.SecondaryIndexTrigger = OVRInput.RawAxis1D.None;
			this.axis1DMap.SecondaryHandTrigger = OVRInput.RawAxis1D.None;
		}

		
		public override void ConfigureAxis2DMap()
		{
			this.axis2DMap.None = OVRInput.RawAxis2D.None;
			this.axis2DMap.PrimaryThumbstick = OVRInput.RawAxis2D.None;
			this.axis2DMap.PrimaryTouchpad = OVRInput.RawAxis2D.LTouchpad;
			this.axis2DMap.SecondaryThumbstick = OVRInput.RawAxis2D.None;
			this.axis2DMap.SecondaryTouchpad = OVRInput.RawAxis2D.None;
		}

		
		private OVRPlugin.Vector2f moveAmount;

		
		private float maxTapMagnitude = 0.1f;

		
		private float minMoveMagnitude = 0.15f;
	}

	
	private class OVRControllerLTrackedRemote : OVRInput.OVRControllerBase
	{
		
		public OVRControllerLTrackedRemote()
		{
			this.controllerType = OVRInput.Controller.LTrackedRemote;
		}

		
		public override void ConfigureButtonMap()
		{
			this.buttonMap.None = OVRInput.RawButton.None;
			this.buttonMap.One = OVRInput.RawButton.LTouchpad;
			this.buttonMap.Two = OVRInput.RawButton.Back;
			this.buttonMap.Three = OVRInput.RawButton.None;
			this.buttonMap.Four = OVRInput.RawButton.None;
			this.buttonMap.Start = OVRInput.RawButton.Start;
			this.buttonMap.Back = OVRInput.RawButton.Back;
			this.buttonMap.PrimaryShoulder = OVRInput.RawButton.None;
			this.buttonMap.PrimaryIndexTrigger = OVRInput.RawButton.LIndexTrigger;
			this.buttonMap.PrimaryHandTrigger = OVRInput.RawButton.LHandTrigger;
			this.buttonMap.PrimaryThumbstick = OVRInput.RawButton.None;
			this.buttonMap.PrimaryThumbstickUp = OVRInput.RawButton.None;
			this.buttonMap.PrimaryThumbstickDown = OVRInput.RawButton.None;
			this.buttonMap.PrimaryThumbstickLeft = OVRInput.RawButton.None;
			this.buttonMap.PrimaryThumbstickRight = OVRInput.RawButton.None;
			this.buttonMap.PrimaryTouchpad = OVRInput.RawButton.LTouchpad;
			this.buttonMap.SecondaryShoulder = OVRInput.RawButton.None;
			this.buttonMap.SecondaryIndexTrigger = OVRInput.RawButton.None;
			this.buttonMap.SecondaryHandTrigger = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstick = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstickUp = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstickDown = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstickLeft = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstickRight = OVRInput.RawButton.None;
			this.buttonMap.SecondaryTouchpad = OVRInput.RawButton.None;
			this.buttonMap.DpadUp = OVRInput.RawButton.DpadUp;
			this.buttonMap.DpadDown = OVRInput.RawButton.DpadDown;
			this.buttonMap.DpadLeft = OVRInput.RawButton.DpadLeft;
			this.buttonMap.DpadRight = OVRInput.RawButton.DpadRight;
			this.buttonMap.Up = OVRInput.RawButton.DpadUp;
			this.buttonMap.Down = OVRInput.RawButton.DpadDown;
			this.buttonMap.Left = OVRInput.RawButton.DpadLeft;
			this.buttonMap.Right = OVRInput.RawButton.DpadRight;
		}

		
		public override void ConfigureTouchMap()
		{
			this.touchMap.None = OVRInput.RawTouch.None;
			this.touchMap.One = OVRInput.RawTouch.LTouchpad;
			this.touchMap.Two = OVRInput.RawTouch.None;
			this.touchMap.Three = OVRInput.RawTouch.None;
			this.touchMap.Four = OVRInput.RawTouch.None;
			this.touchMap.PrimaryIndexTrigger = OVRInput.RawTouch.LIndexTrigger;
			this.touchMap.PrimaryThumbstick = OVRInput.RawTouch.None;
			this.touchMap.PrimaryThumbRest = OVRInput.RawTouch.None;
			this.touchMap.PrimaryTouchpad = OVRInput.RawTouch.LTouchpad;
			this.touchMap.SecondaryIndexTrigger = OVRInput.RawTouch.None;
			this.touchMap.SecondaryThumbstick = OVRInput.RawTouch.None;
			this.touchMap.SecondaryThumbRest = OVRInput.RawTouch.None;
			this.touchMap.SecondaryTouchpad = OVRInput.RawTouch.None;
		}

		
		public override void ConfigureNearTouchMap()
		{
			this.nearTouchMap.None = OVRInput.RawNearTouch.None;
			this.nearTouchMap.PrimaryIndexTrigger = OVRInput.RawNearTouch.None;
			this.nearTouchMap.PrimaryThumbButtons = OVRInput.RawNearTouch.None;
			this.nearTouchMap.SecondaryIndexTrigger = OVRInput.RawNearTouch.None;
			this.nearTouchMap.SecondaryThumbButtons = OVRInput.RawNearTouch.None;
		}

		
		public override void ConfigureAxis1DMap()
		{
			this.axis1DMap.None = OVRInput.RawAxis1D.None;
			this.axis1DMap.PrimaryIndexTrigger = OVRInput.RawAxis1D.LIndexTrigger;
			this.axis1DMap.PrimaryHandTrigger = OVRInput.RawAxis1D.LHandTrigger;
			this.axis1DMap.SecondaryIndexTrigger = OVRInput.RawAxis1D.None;
			this.axis1DMap.SecondaryHandTrigger = OVRInput.RawAxis1D.None;
		}

		
		public override void ConfigureAxis2DMap()
		{
			this.axis2DMap.None = OVRInput.RawAxis2D.None;
			this.axis2DMap.PrimaryThumbstick = OVRInput.RawAxis2D.None;
			this.axis2DMap.PrimaryTouchpad = OVRInput.RawAxis2D.LTouchpad;
			this.axis2DMap.SecondaryThumbstick = OVRInput.RawAxis2D.None;
			this.axis2DMap.SecondaryTouchpad = OVRInput.RawAxis2D.None;
		}

		
		public override OVRInput.Controller Update()
		{
			OVRInput.Controller result = base.Update();
			if (OVRInput.GetDown(OVRInput.RawTouch.LTouchpad, OVRInput.Controller.LTrackedRemote))
			{
				this.emitSwipe = true;
				this.moveAmount = this.currentState.LTouchpad;
			}
			if (OVRInput.GetDown(OVRInput.RawButton.LTouchpad, OVRInput.Controller.LTrackedRemote))
			{
				this.emitSwipe = false;
			}
			if (OVRInput.GetUp(OVRInput.RawTouch.LTouchpad, OVRInput.Controller.LTrackedRemote) && this.emitSwipe)
			{
				this.emitSwipe = false;
				this.moveAmount.x = this.previousState.LTouchpad.x - this.moveAmount.x;
				this.moveAmount.y = this.previousState.LTouchpad.y - this.moveAmount.y;
				Vector2 vector = new Vector2(this.moveAmount.x, this.moveAmount.y);
				if (vector.magnitude >= this.minMoveMagnitude)
				{
					vector.Normalize();
					if (Mathf.Abs(vector.x) > Mathf.Abs(vector.y))
					{
						if (vector.x < 0f)
						{
							this.currentState.Buttons = (this.currentState.Buttons | 262144u);
						}
						else
						{
							this.currentState.Buttons = (this.currentState.Buttons | 524288u);
						}
					}
					else if (vector.y < 0f)
					{
						this.currentState.Buttons = (this.currentState.Buttons | 131072u);
					}
					else
					{
						this.currentState.Buttons = (this.currentState.Buttons | 65536u);
					}
				}
			}
			return result;
		}

		
		public override bool WasRecentered()
		{
			return this.currentState.LRecenterCount != this.previousState.LRecenterCount;
		}

		
		public override byte GetRecenterCount()
		{
			return this.currentState.LRecenterCount;
		}

		
		public override byte GetBatteryPercentRemaining()
		{
			return this.currentState.LBatteryPercentRemaining;
		}

		
		private bool emitSwipe;

		
		private OVRPlugin.Vector2f moveAmount;

		
		private float minMoveMagnitude = 0.3f;
	}

	
	private class OVRControllerRTrackedRemote : OVRInput.OVRControllerBase
	{
		
		public OVRControllerRTrackedRemote()
		{
			this.controllerType = OVRInput.Controller.RTrackedRemote;
		}

		
		public override void ConfigureButtonMap()
		{
			this.buttonMap.None = OVRInput.RawButton.None;
			this.buttonMap.One = OVRInput.RawButton.RTouchpad;
			this.buttonMap.Two = OVRInput.RawButton.Back;
			this.buttonMap.Three = OVRInput.RawButton.None;
			this.buttonMap.Four = OVRInput.RawButton.None;
			this.buttonMap.Start = OVRInput.RawButton.Start;
			this.buttonMap.Back = OVRInput.RawButton.Back;
			this.buttonMap.PrimaryShoulder = OVRInput.RawButton.None;
			this.buttonMap.PrimaryIndexTrigger = OVRInput.RawButton.RIndexTrigger;
			this.buttonMap.PrimaryHandTrigger = OVRInput.RawButton.RHandTrigger;
			this.buttonMap.PrimaryThumbstick = OVRInput.RawButton.None;
			this.buttonMap.PrimaryThumbstickUp = OVRInput.RawButton.None;
			this.buttonMap.PrimaryThumbstickDown = OVRInput.RawButton.None;
			this.buttonMap.PrimaryThumbstickLeft = OVRInput.RawButton.None;
			this.buttonMap.PrimaryThumbstickRight = OVRInput.RawButton.None;
			this.buttonMap.PrimaryTouchpad = OVRInput.RawButton.RTouchpad;
			this.buttonMap.SecondaryShoulder = OVRInput.RawButton.None;
			this.buttonMap.SecondaryIndexTrigger = OVRInput.RawButton.None;
			this.buttonMap.SecondaryHandTrigger = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstick = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstickUp = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstickDown = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstickLeft = OVRInput.RawButton.None;
			this.buttonMap.SecondaryThumbstickRight = OVRInput.RawButton.None;
			this.buttonMap.SecondaryTouchpad = OVRInput.RawButton.None;
			this.buttonMap.DpadUp = OVRInput.RawButton.DpadUp;
			this.buttonMap.DpadDown = OVRInput.RawButton.DpadDown;
			this.buttonMap.DpadLeft = OVRInput.RawButton.DpadLeft;
			this.buttonMap.DpadRight = OVRInput.RawButton.DpadRight;
			this.buttonMap.Up = OVRInput.RawButton.DpadUp;
			this.buttonMap.Down = OVRInput.RawButton.DpadDown;
			this.buttonMap.Left = OVRInput.RawButton.DpadLeft;
			this.buttonMap.Right = OVRInput.RawButton.DpadRight;
		}

		
		public override void ConfigureTouchMap()
		{
			this.touchMap.None = OVRInput.RawTouch.None;
			this.touchMap.One = OVRInput.RawTouch.RTouchpad;
			this.touchMap.Two = OVRInput.RawTouch.None;
			this.touchMap.Three = OVRInput.RawTouch.None;
			this.touchMap.Four = OVRInput.RawTouch.None;
			this.touchMap.PrimaryIndexTrigger = OVRInput.RawTouch.RIndexTrigger;
			this.touchMap.PrimaryThumbstick = OVRInput.RawTouch.None;
			this.touchMap.PrimaryThumbRest = OVRInput.RawTouch.None;
			this.touchMap.PrimaryTouchpad = OVRInput.RawTouch.RTouchpad;
			this.touchMap.SecondaryIndexTrigger = OVRInput.RawTouch.None;
			this.touchMap.SecondaryThumbstick = OVRInput.RawTouch.None;
			this.touchMap.SecondaryThumbRest = OVRInput.RawTouch.None;
			this.touchMap.SecondaryTouchpad = OVRInput.RawTouch.None;
		}

		
		public override void ConfigureNearTouchMap()
		{
			this.nearTouchMap.None = OVRInput.RawNearTouch.None;
			this.nearTouchMap.PrimaryIndexTrigger = OVRInput.RawNearTouch.None;
			this.nearTouchMap.PrimaryThumbButtons = OVRInput.RawNearTouch.None;
			this.nearTouchMap.SecondaryIndexTrigger = OVRInput.RawNearTouch.None;
			this.nearTouchMap.SecondaryThumbButtons = OVRInput.RawNearTouch.None;
		}

		
		public override void ConfigureAxis1DMap()
		{
			this.axis1DMap.None = OVRInput.RawAxis1D.None;
			this.axis1DMap.PrimaryIndexTrigger = OVRInput.RawAxis1D.RIndexTrigger;
			this.axis1DMap.PrimaryHandTrigger = OVRInput.RawAxis1D.RHandTrigger;
			this.axis1DMap.SecondaryIndexTrigger = OVRInput.RawAxis1D.None;
			this.axis1DMap.SecondaryHandTrigger = OVRInput.RawAxis1D.None;
		}

		
		public override void ConfigureAxis2DMap()
		{
			this.axis2DMap.None = OVRInput.RawAxis2D.None;
			this.axis2DMap.PrimaryThumbstick = OVRInput.RawAxis2D.None;
			this.axis2DMap.PrimaryTouchpad = OVRInput.RawAxis2D.RTouchpad;
			this.axis2DMap.SecondaryThumbstick = OVRInput.RawAxis2D.None;
			this.axis2DMap.SecondaryTouchpad = OVRInput.RawAxis2D.None;
		}

		
		public override OVRInput.Controller Update()
		{
			OVRInput.Controller result = base.Update();
			if (OVRInput.GetDown(OVRInput.RawTouch.RTouchpad, OVRInput.Controller.RTrackedRemote))
			{
				this.emitSwipe = true;
				this.moveAmount = this.currentState.RTouchpad;
			}
			if (OVRInput.GetDown(OVRInput.RawButton.RTouchpad, OVRInput.Controller.RTrackedRemote))
			{
				this.emitSwipe = false;
			}
			if (OVRInput.GetUp(OVRInput.RawTouch.RTouchpad, OVRInput.Controller.RTrackedRemote) && this.emitSwipe)
			{
				this.emitSwipe = false;
				this.moveAmount.x = this.previousState.RTouchpad.x - this.moveAmount.x;
				this.moveAmount.y = this.previousState.RTouchpad.y - this.moveAmount.y;
				Vector2 vector = new Vector2(this.moveAmount.x, this.moveAmount.y);
				if (vector.magnitude >= this.minMoveMagnitude)
				{
					vector.Normalize();
					if (Mathf.Abs(vector.x) > Mathf.Abs(vector.y))
					{
						if (vector.x < 0f)
						{
							this.currentState.Buttons = (this.currentState.Buttons | 262144u);
						}
						else
						{
							this.currentState.Buttons = (this.currentState.Buttons | 524288u);
						}
					}
					else if (vector.y < 0f)
					{
						this.currentState.Buttons = (this.currentState.Buttons | 131072u);
					}
					else
					{
						this.currentState.Buttons = (this.currentState.Buttons | 65536u);
					}
				}
			}
			return result;
		}

		
		public override bool WasRecentered()
		{
			return this.currentState.RRecenterCount != this.previousState.RRecenterCount;
		}

		
		public override byte GetRecenterCount()
		{
			return this.currentState.RRecenterCount;
		}

		
		public override byte GetBatteryPercentRemaining()
		{
			return this.currentState.RBatteryPercentRemaining;
		}

		
		private bool emitSwipe;

		
		private OVRPlugin.Vector2f moveAmount;

		
		private float minMoveMagnitude = 0.3f;
	}
}
