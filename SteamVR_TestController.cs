using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;


public class SteamVR_TestController : MonoBehaviour
{
	
	public SteamVR_TestController()
	{
		EVRButtonId[] array = new EVRButtonId[4];
		RuntimeHelpers.InitializeArray(array, fieldof(<PrivateImplementationDetails>.$field-8236C0CC36A2E3B682228FAF16E4A74BE79F3DE1).FieldHandle);
		this.buttonIds = array;
		this.axisIds = new EVRButtonId[]
		{
			EVRButtonId.k_EButton_Axis0,
			EVRButtonId.k_EButton_Axis1
		};
		base..ctor();
	}

	
	private void OnDeviceConnected(int index, bool connected)
	{
		CVRSystem system = OpenVR.System;
		if (system == null || system.GetTrackedDeviceClass((uint)index) != ETrackedDeviceClass.Controller)
		{
			return;
		}
		if (connected)
		{
			Debug.Log(string.Format("Controller {0} connected.", index));
			this.PrintControllerStatus(index);
			this.controllerIndices.Add(index);
		}
		else
		{
			Debug.Log(string.Format("Controller {0} disconnected.", index));
			this.PrintControllerStatus(index);
			this.controllerIndices.Remove(index);
		}
	}

	
	private void OnEnable()
	{
		SteamVR_Events.DeviceConnected.Listen(new UnityAction<int, bool>(this.OnDeviceConnected));
	}

	
	private void OnDisable()
	{
		SteamVR_Events.DeviceConnected.Remove(new UnityAction<int, bool>(this.OnDeviceConnected));
	}

	
	private void PrintControllerStatus(int index)
	{
		SteamVR_Controller.Device device = SteamVR_Controller.Input(index);
		Debug.Log("index: " + device.index);
		Debug.Log("connected: " + device.connected);
		Debug.Log("hasTracking: " + device.hasTracking);
		Debug.Log("outOfRange: " + device.outOfRange);
		Debug.Log("calibrating: " + device.calibrating);
		Debug.Log("uninitialized: " + device.uninitialized);
		Debug.Log("pos: " + device.transform.pos);
		Debug.Log("rot: " + device.transform.rot.eulerAngles);
		Debug.Log("velocity: " + device.velocity);
		Debug.Log("angularVelocity: " + device.angularVelocity);
		int deviceIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost, ETrackedDeviceClass.Controller, 0);
		int deviceIndex2 = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost, ETrackedDeviceClass.Controller, 0);
		Debug.Log((deviceIndex != deviceIndex2) ? ((deviceIndex != index) ? "right" : "left") : "first");
	}

	
	private void Update()
	{
		foreach (int num in this.controllerIndices)
		{
			SteamVR_Overlay instance = SteamVR_Overlay.instance;
			if (instance && this.point && this.pointer)
			{
				SteamVR_Utils.RigidTransform transform = SteamVR_Controller.Input(num).transform;
				this.pointer.transform.localPosition = transform.pos;
				this.pointer.transform.localRotation = transform.rot;
				SteamVR_Overlay.IntersectionResults intersectionResults = default(SteamVR_Overlay.IntersectionResults);
				bool flag = instance.ComputeIntersection(transform.pos, transform.rot * Vector3.forward, ref intersectionResults);
				if (flag)
				{
					this.point.transform.localPosition = intersectionResults.point;
					this.point.transform.localRotation = Quaternion.LookRotation(intersectionResults.normal);
				}
			}
			else
			{
				foreach (EVRButtonId evrbuttonId in this.buttonIds)
				{
					if (SteamVR_Controller.Input(num).GetPressDown(evrbuttonId))
					{
						Debug.Log(evrbuttonId + " press down");
					}
					if (SteamVR_Controller.Input(num).GetPressUp(evrbuttonId))
					{
						Debug.Log(evrbuttonId + " press up");
						if (evrbuttonId == EVRButtonId.k_EButton_Axis1)
						{
							SteamVR_Controller.Input(num).TriggerHapticPulse(500, EVRButtonId.k_EButton_Axis0);
							this.PrintControllerStatus(num);
						}
					}
					if (SteamVR_Controller.Input(num).GetPress(evrbuttonId))
					{
						Debug.Log(evrbuttonId);
					}
				}
				foreach (EVRButtonId evrbuttonId2 in this.axisIds)
				{
					if (SteamVR_Controller.Input(num).GetTouchDown(evrbuttonId2))
					{
						Debug.Log(evrbuttonId2 + " touch down");
					}
					if (SteamVR_Controller.Input(num).GetTouchUp(evrbuttonId2))
					{
						Debug.Log(evrbuttonId2 + " touch up");
					}
					if (SteamVR_Controller.Input(num).GetTouch(evrbuttonId2))
					{
						Vector2 axis = SteamVR_Controller.Input(num).GetAxis(evrbuttonId2);
						Debug.Log("axis: " + axis);
					}
				}
			}
		}
	}

	
	private List<int> controllerIndices = new List<int>();

	
	private EVRButtonId[] buttonIds;

	
	private EVRButtonId[] axisIds;

	
	public Transform point;

	
	public Transform pointer;
}
