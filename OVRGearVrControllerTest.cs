using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;


public class OVRGearVrControllerTest : MonoBehaviour
{
	
	private void Start()
	{
		if (this.uiText != null)
		{
			this.uiText.supportRichText = false;
		}
		this.data = new StringBuilder(2048);
		List<OVRGearVrControllerTest.BoolMonitor> list = new List<OVRGearVrControllerTest.BoolMonitor>();
		list.Add(new OVRGearVrControllerTest.BoolMonitor("WasRecentered", () => OVRInput.GetControllerWasRecentered(OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("One", () => OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("OneDown", () => OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("OneUp", () => OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("One (Touch)", () => OVRInput.Get(OVRInput.Touch.One, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("OneDown (Touch)", () => OVRInput.GetDown(OVRInput.Touch.One, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("OneUp (Touch)", () => OVRInput.GetUp(OVRInput.Touch.One, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("Two", () => OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("TwoDown", () => OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("TwoUp", () => OVRInput.GetUp(OVRInput.Button.Two, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("PrimaryIndexTrigger", () => OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("PrimaryIndexTriggerDown", () => OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("PrimaryIndexTriggerUp", () => OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("PrimaryIndexTrigger (Touch)", () => OVRInput.Get(OVRInput.Touch.PrimaryIndexTrigger, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("PrimaryIndexTriggerDown (Touch)", () => OVRInput.GetDown(OVRInput.Touch.PrimaryIndexTrigger, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("PrimaryIndexTriggerUp (Touch)", () => OVRInput.GetUp(OVRInput.Touch.PrimaryIndexTrigger, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("PrimaryHandTrigger", () => OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("PrimaryHandTriggerDown", () => OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("PrimaryHandTriggerUp", () => OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("Up", () => OVRInput.Get(OVRInput.Button.Up, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("Down", () => OVRInput.Get(OVRInput.Button.Down, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("Left", () => OVRInput.Get(OVRInput.Button.Left, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("Right", () => OVRInput.Get(OVRInput.Button.Right, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("Touchpad (Click)", () => OVRInput.Get(OVRInput.Button.PrimaryTouchpad, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("TouchpadDown (Click)", () => OVRInput.GetDown(OVRInput.Button.PrimaryTouchpad, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("TouchpadUp (Click)", () => OVRInput.GetUp(OVRInput.Button.PrimaryTouchpad, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("Touchpad (Touch)", () => OVRInput.Get(OVRInput.Touch.PrimaryTouchpad, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("TouchpadDown (Touch)", () => OVRInput.GetDown(OVRInput.Touch.PrimaryTouchpad, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("TouchpadUp (Touch)", () => OVRInput.GetUp(OVRInput.Touch.PrimaryTouchpad, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("Start", () => OVRInput.Get(OVRInput.RawButton.Start, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("StartDown", () => OVRInput.GetDown(OVRInput.RawButton.Start, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("StartUp", () => OVRInput.GetUp(OVRInput.RawButton.Start, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("Back", () => OVRInput.Get(OVRInput.RawButton.Back, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("BackDown", () => OVRInput.GetDown(OVRInput.RawButton.Back, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("BackUp", () => OVRInput.GetUp(OVRInput.RawButton.Back, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("A", () => OVRInput.Get(OVRInput.RawButton.A, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("ADown", () => OVRInput.GetDown(OVRInput.RawButton.A, OVRInput.Controller.Active), 0.5f));
		list.Add(new OVRGearVrControllerTest.BoolMonitor("AUp", () => OVRInput.GetUp(OVRInput.RawButton.A, OVRInput.Controller.Active), 0.5f));
		this.monitors = list;
	}

	
	private void Update()
	{
		OVRInput.Controller activeController = OVRInput.GetActiveController();
		this.data.Length = 0;
		byte controllerRecenterCount = OVRInput.GetControllerRecenterCount(OVRInput.Controller.Active);
		this.data.AppendFormat("RecenterCount: {0}\n", controllerRecenterCount);
		byte controllerBatteryPercentRemaining = OVRInput.GetControllerBatteryPercentRemaining(OVRInput.Controller.Active);
		this.data.AppendFormat("Battery: {0}\n", controllerBatteryPercentRemaining);
		float appFramerate = OVRPlugin.GetAppFramerate();
		this.data.AppendFormat("Framerate: {0:F2}\n", appFramerate);
		string arg = activeController.ToString();
		this.data.AppendFormat("Active: {0}\n", arg);
		string arg2 = OVRInput.GetConnectedControllers().ToString();
		this.data.AppendFormat("Connected: {0}\n", arg2);
		this.data.AppendFormat("PrevConnected: {0}\n", OVRGearVrControllerTest.prevConnected);
		OVRGearVrControllerTest.controllers.Update();
		OVRGearVrControllerTest.controllers.AppendToStringBuilder(ref this.data);
		OVRGearVrControllerTest.prevConnected = arg2;
		Quaternion localControllerRotation = OVRInput.GetLocalControllerRotation(activeController);
		this.data.AppendFormat("Orientation: ({0:F2}, {1:F2}, {2:F2}, {3:F2})\n", new object[]
		{
			localControllerRotation.x,
			localControllerRotation.y,
			localControllerRotation.z,
			localControllerRotation.w
		});
		Vector3 localControllerAngularVelocity = OVRInput.GetLocalControllerAngularVelocity(activeController);
		this.data.AppendFormat("AngVel: ({0:F2}, {1:F2}, {2:F2})\n", localControllerAngularVelocity.x, localControllerAngularVelocity.y, localControllerAngularVelocity.z);
		Vector3 localControllerAngularAcceleration = OVRInput.GetLocalControllerAngularAcceleration(activeController);
		this.data.AppendFormat("AngAcc: ({0:F2}, {1:F2}, {2:F2})\n", localControllerAngularAcceleration.x, localControllerAngularAcceleration.y, localControllerAngularAcceleration.z);
		Vector3 localControllerPosition = OVRInput.GetLocalControllerPosition(activeController);
		this.data.AppendFormat("Position: ({0:F2}, {1:F2}, {2:F2})\n", localControllerPosition.x, localControllerPosition.y, localControllerPosition.z);
		Vector3 localControllerVelocity = OVRInput.GetLocalControllerVelocity(activeController);
		this.data.AppendFormat("Vel: ({0:F2}, {1:F2}, {2:F2})\n", localControllerVelocity.x, localControllerVelocity.y, localControllerVelocity.z);
		Vector3 localControllerAcceleration = OVRInput.GetLocalControllerAcceleration(activeController);
		this.data.AppendFormat("Acc: ({0:F2}, {1:F2}, {2:F2})\n", localControllerAcceleration.x, localControllerAcceleration.y, localControllerAcceleration.z);
		Vector2 vector = OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad, OVRInput.Controller.Active);
		this.data.AppendFormat("PrimaryTouchpad: ({0:F2}, {1:F2})\n", vector.x, vector.y);
		Vector2 vector2 = OVRInput.Get(OVRInput.Axis2D.SecondaryTouchpad, OVRInput.Controller.Active);
		this.data.AppendFormat("SecondaryTouchpad: ({0:F2}, {1:F2})\n", vector2.x, vector2.y);
		float num = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.Active);
		this.data.AppendFormat("PrimaryIndexTriggerAxis1D: ({0:F2})\n", num);
		float num2 = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.Active);
		this.data.AppendFormat("PrimaryHandTriggerAxis1D: ({0:F2})\n", num2);
		for (int i = 0; i < this.monitors.Count; i++)
		{
			this.monitors[i].Update();
			this.monitors[i].AppendToStringBuilder(ref this.data);
		}
		if (this.uiText != null)
		{
			this.uiText.text = this.data.ToString();
		}
	}

	
	public Text uiText;

	
	private List<OVRGearVrControllerTest.BoolMonitor> monitors;

	
	private StringBuilder data;

	
	private static string prevConnected = string.Empty;

	
	private static OVRGearVrControllerTest.BoolMonitor controllers = new OVRGearVrControllerTest.BoolMonitor("Controllers Changed", () => OVRInput.GetConnectedControllers().ToString() != OVRGearVrControllerTest.prevConnected, 0.5f);

	
	public class BoolMonitor
	{
		
		public BoolMonitor(string name, OVRGearVrControllerTest.BoolMonitor.BoolGenerator generator, float displayTimeout = 0.5f)
		{
			this.m_name = name;
			this.m_generator = generator;
			this.m_displayTimeout = displayTimeout;
		}

		
		public void Update()
		{
			this.m_prevValue = this.m_currentValue;
			this.m_currentValue = this.m_generator();
			if (this.m_currentValue != this.m_prevValue)
			{
				this.m_currentValueRecentlyChanged = true;
				this.m_displayTimer = this.m_displayTimeout;
			}
			if (this.m_displayTimer > 0f)
			{
				this.m_displayTimer -= Time.deltaTime;
				if (this.m_displayTimer <= 0f)
				{
					this.m_currentValueRecentlyChanged = false;
					this.m_displayTimer = 0f;
				}
			}
		}

		
		public void AppendToStringBuilder(ref StringBuilder sb)
		{
			sb.Append(this.m_name);
			if (this.m_currentValue && this.m_currentValueRecentlyChanged)
			{
				sb.Append(": *True*\n");
			}
			else if (this.m_currentValue)
			{
				sb.Append(":  True \n");
			}
			else if (!this.m_currentValue && this.m_currentValueRecentlyChanged)
			{
				sb.Append(": *False*\n");
			}
			else if (!this.m_currentValue)
			{
				sb.Append(":  False \n");
			}
		}

		
		private string m_name = string.Empty;

		
		private OVRGearVrControllerTest.BoolMonitor.BoolGenerator m_generator;

		
		private bool m_prevValue;

		
		private bool m_currentValue;

		
		private bool m_currentValueRecentlyChanged;

		
		private float m_displayTimeout;

		
		private float m_displayTimer;

		
		
		public delegate bool BoolGenerator();
	}
}
