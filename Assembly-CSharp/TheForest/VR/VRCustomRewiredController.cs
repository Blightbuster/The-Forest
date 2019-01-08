using System;
using Rewired;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.VR
{
	public class VRCustomRewiredController : MonoBehaviour
	{
		private void Start()
		{
			this._controller = TheForest.Utils.Input.player.controllers.GetController<CustomController>(0);
			if (this._controller != null)
			{
				ReInput.InputSourceUpdateEvent += this.ReInput_InputSourceUpdateEvent;
			}
			this.ResetJoystickIds();
		}

		public void ResetJoystickIds()
		{
			this._joysticks = UnityEngine.Input.GetJoystickNames();
			for (int i = 0; i < this._joysticks.Length; i++)
			{
				if (this._leftJoystickId == -1)
				{
					for (int j = 0; j < this._leftJoysticks.Length; j++)
					{
						if (string.Compare(this._joysticks[i], this._leftJoysticks[j]) == 0)
						{
							this._leftJoystickId = i + 1;
							break;
						}
					}
				}
				if (this._rightJoystickId == -1)
				{
					for (int k = 0; k < this._rightJoysticks.Length; k++)
					{
						if (string.Compare(this._joysticks[i], this._rightJoysticks[k]) == 0)
						{
							this._rightJoystickId = i + 1;
							break;
						}
					}
				}
				if (this._leftJoystickId != -1 && this._rightJoystickId != -1)
				{
					this._axis = new VRCustomRewiredController.InputEntry[]
					{
						new VRCustomRewiredController.InputEntry
						{
							_id = 0,
							_name = "Joy" + this._leftJoystickId + "Axis1"
						},
						new VRCustomRewiredController.InputEntry
						{
							_id = 1,
							_name = "Joy" + this._leftJoystickId + "Axis2"
						},
						new VRCustomRewiredController.InputEntry
						{
							_id = 2,
							_name = "Joy" + this._rightJoystickId + "Axis4"
						},
						new VRCustomRewiredController.InputEntry
						{
							_id = 3,
							_name = "Joy" + this._rightJoystickId + "Axis5"
						},
						new VRCustomRewiredController.InputEntry
						{
							_id = 20,
							_name = "Joy" + this._leftJoystickId + "Axis9"
						},
						new VRCustomRewiredController.InputEntry
						{
							_id = 19,
							_name = "Joy" + this._leftJoystickId + "Axis11"
						},
						new VRCustomRewiredController.InputEntry
						{
							_id = 18,
							_name = "Joy" + this._rightJoystickId + "Axis10"
						},
						new VRCustomRewiredController.InputEntry
						{
							_id = 21,
							_name = "Joy" + this._rightJoystickId + "Axis12"
						}
					};
					this._buttons = new VRCustomRewiredController.InputEntry[]
					{
						new VRCustomRewiredController.InputEntry
						{
							_id = 4,
							_name = "Joy" + this._rightJoystickId + "Button0"
						},
						new VRCustomRewiredController.InputEntry
						{
							_id = 5,
							_name = "Joy" + this._rightJoystickId + "Button1"
						},
						new VRCustomRewiredController.InputEntry
						{
							_id = 6,
							_name = "Joy" + this._leftJoystickId + "Button2"
						},
						new VRCustomRewiredController.InputEntry
						{
							_id = 7,
							_name = "Joy" + this._leftJoystickId + "Button3"
						},
						new VRCustomRewiredController.InputEntry
						{
							_id = 9,
							_name = "Joy" + this._leftJoystickId + "Button14"
						},
						new VRCustomRewiredController.InputEntry
						{
							_id = 15,
							_name = "Joy" + this._leftJoystickId + "Axis11",
							_axisAsButton = true
						},
						new VRCustomRewiredController.InputEntry
						{
							_id = 12,
							_name = "Joy" + this._leftJoystickId + "Button8"
						},
						new VRCustomRewiredController.InputEntry
						{
							_id = 11,
							_name = "Joy" + this._rightJoystickId + "Button15"
						},
						new VRCustomRewiredController.InputEntry
						{
							_id = 16,
							_name = "Joy" + this._rightJoystickId + "Axis12",
							_axisAsButton = true
						},
						new VRCustomRewiredController.InputEntry
						{
							_id = 13,
							_name = "Joy" + this._rightJoystickId + "Button9"
						},
						new VRCustomRewiredController.InputEntry
						{
							_id = 17,
							_name = "Joy" + this._leftJoystickId + "Button7"
						},
						new VRCustomRewiredController.InputEntry
						{
							_id = 22,
							_name = "Joy" + this._leftJoystickId + "Button18"
						},
						new VRCustomRewiredController.InputEntry
						{
							_id = 23,
							_name = "Joy" + this._rightJoystickId + "Button19"
						}
					};
					break;
				}
			}
		}

		private void ReInput_InputSourceUpdateEvent()
		{
			foreach (VRCustomRewiredController.InputEntry inputEntry in this._axis)
			{
				this._controller.SetAxisValueById(inputEntry._id, UnityEngine.Input.GetAxisRaw(inputEntry._name));
			}
			foreach (VRCustomRewiredController.InputEntry inputEntry2 in this._buttons)
			{
				if (inputEntry2._axisAsButton)
				{
					this._controller.SetButtonValueById(inputEntry2._id, UnityEngine.Input.GetAxisRaw(inputEntry2._name) > 0f);
				}
				else
				{
					this._controller.SetButtonValueById(inputEntry2._id, UnityEngine.Input.GetButton(inputEntry2._name));
				}
			}
		}

		public string[] _joysticks;

		public string[] _leftJoysticks = new string[]
		{
			"OpenVR Controller - Left"
		};

		public string[] _rightJoysticks = new string[]
		{
			"OpenVR Controller - Right"
		};

		private int _leftJoystickId = -1;

		private int _rightJoystickId = -1;

		public VRCustomRewiredController.InputEntry[] _axis;

		public VRCustomRewiredController.InputEntry[] _buttons;

		private CustomController _controller;

		[Serializable]
		public class InputEntry
		{
			public int _id;

			public string _name;

			public bool _axisAsButton;
		}
	}
}
