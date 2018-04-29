using System;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;


public class SteamVR_ControllerManager : MonoBehaviour
{
	
	private SteamVR_ControllerManager()
	{
		this.inputFocusAction = SteamVR_Events.InputFocusAction(new UnityAction<bool>(this.OnInputFocus));
		this.deviceConnectedAction = SteamVR_Events.DeviceConnectedAction(new UnityAction<int, bool>(this.OnDeviceConnected));
		this.trackedDeviceRoleChangedAction = SteamVR_Events.SystemAction(EVREventType.VREvent_TrackedDeviceRoleChanged, new UnityAction<VREvent_t>(this.OnTrackedDeviceRoleChanged));
	}

	
	private void SetUniqueObject(GameObject o, int index)
	{
		for (int i = 0; i < index; i++)
		{
			if (this.objects[i] == o)
			{
				return;
			}
		}
		this.objects[index] = o;
	}

	
	public void UpdateTargets()
	{
		GameObject[] array = this.objects;
		int num = (array == null) ? 0 : array.Length;
		this.objects = new GameObject[2 + num];
		this.SetUniqueObject(this.right, 0);
		this.SetUniqueObject(this.left, 1);
		for (int i = 0; i < num; i++)
		{
			this.SetUniqueObject(array[i], 2 + i);
		}
		this.indices = new uint[2 + num];
		for (int j = 0; j < this.indices.Length; j++)
		{
			this.indices[j] = uint.MaxValue;
		}
	}

	
	private void Awake()
	{
		this.UpdateTargets();
	}

	
	private void OnEnable()
	{
		for (int i = 0; i < this.objects.Length; i++)
		{
			GameObject gameObject = this.objects[i];
			if (gameObject != null)
			{
				gameObject.SetActive(false);
			}
			this.indices[i] = uint.MaxValue;
		}
		this.Refresh();
		for (int j = 0; j < SteamVR.connected.Length; j++)
		{
			if (SteamVR.connected[j])
			{
				this.OnDeviceConnected(j, true);
			}
		}
		this.inputFocusAction.enabled = true;
		this.deviceConnectedAction.enabled = true;
		this.trackedDeviceRoleChangedAction.enabled = true;
	}

	
	private void OnDisable()
	{
		this.inputFocusAction.enabled = false;
		this.deviceConnectedAction.enabled = false;
		this.trackedDeviceRoleChangedAction.enabled = false;
	}

	
	private void OnInputFocus(bool hasFocus)
	{
		if (hasFocus)
		{
			for (int i = 0; i < this.objects.Length; i++)
			{
				GameObject gameObject = this.objects[i];
				if (gameObject != null)
				{
					string str = (i >= 2) ? (i - 1).ToString() : SteamVR_ControllerManager.labels[i];
					this.ShowObject(gameObject.transform, SteamVR_ControllerManager.hiddenPrefix + str + SteamVR_ControllerManager.hiddenPostfix);
				}
			}
		}
		else
		{
			for (int j = 0; j < this.objects.Length; j++)
			{
				GameObject gameObject2 = this.objects[j];
				if (gameObject2 != null)
				{
					string str2 = (j >= 2) ? (j - 1).ToString() : SteamVR_ControllerManager.labels[j];
					this.HideObject(gameObject2.transform, SteamVR_ControllerManager.hiddenPrefix + str2 + SteamVR_ControllerManager.hiddenPostfix);
				}
			}
		}
	}

	
	private void HideObject(Transform t, string name)
	{
		if (t.gameObject.name.StartsWith(SteamVR_ControllerManager.hiddenPrefix))
		{
			Debug.Log("Ignoring double-hide.");
			return;
		}
		Transform transform = new GameObject(name).transform;
		transform.parent = t.parent;
		t.parent = transform;
		transform.gameObject.SetActive(false);
	}

	
	private void ShowObject(Transform t, string name)
	{
		Transform parent = t.parent;
		if (parent.gameObject.name != name)
		{
			return;
		}
		t.parent = parent.parent;
		UnityEngine.Object.Destroy(parent.gameObject);
	}

	
	private void SetTrackedDeviceIndex(int objectIndex, uint trackedDeviceIndex)
	{
		if (trackedDeviceIndex != 4294967295u)
		{
			for (int i = 0; i < this.objects.Length; i++)
			{
				if (i != objectIndex && this.indices[i] == trackedDeviceIndex)
				{
					GameObject gameObject = this.objects[i];
					if (gameObject != null)
					{
						gameObject.SetActive(false);
					}
					this.indices[i] = uint.MaxValue;
				}
			}
		}
		if (trackedDeviceIndex != this.indices[objectIndex])
		{
			this.indices[objectIndex] = trackedDeviceIndex;
			GameObject gameObject2 = this.objects[objectIndex];
			if (gameObject2 != null)
			{
				if (trackedDeviceIndex == 4294967295u)
				{
					gameObject2.SetActive(false);
				}
				else
				{
					gameObject2.SetActive(true);
					gameObject2.BroadcastMessage("SetDeviceIndex", (int)trackedDeviceIndex, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
	}

	
	private void OnTrackedDeviceRoleChanged(VREvent_t vrEvent)
	{
		this.Refresh();
	}

	
	private void OnDeviceConnected(int index, bool connected)
	{
		bool flag = this.connected[index];
		this.connected[index] = false;
		if (connected)
		{
			CVRSystem system = OpenVR.System;
			if (system != null)
			{
				ETrackedDeviceClass trackedDeviceClass = system.GetTrackedDeviceClass((uint)index);
				if (trackedDeviceClass == ETrackedDeviceClass.Controller || trackedDeviceClass == ETrackedDeviceClass.GenericTracker)
				{
					this.connected[index] = true;
					flag = !flag;
				}
			}
		}
		if (flag)
		{
			this.Refresh();
		}
	}

	
	public void Refresh()
	{
		int i = 0;
		CVRSystem system = OpenVR.System;
		if (system != null)
		{
			this.leftIndex = system.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
			this.rightIndex = system.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);
		}
		if (this.leftIndex == 4294967295u && this.rightIndex == 4294967295u)
		{
			uint num = 0u;
			while ((ulong)num < (ulong)((long)this.connected.Length))
			{
				if (i >= this.objects.Length)
				{
					break;
				}
				if (this.connected[(int)((UIntPtr)num)])
				{
					this.SetTrackedDeviceIndex(i++, num);
					if (!this.assignAllBeforeIdentified)
					{
						break;
					}
				}
				num += 1u;
			}
		}
		else
		{
			this.SetTrackedDeviceIndex(i++, ((ulong)this.rightIndex >= (ulong)((long)this.connected.Length) || !this.connected[(int)((UIntPtr)this.rightIndex)]) ? uint.MaxValue : this.rightIndex);
			this.SetTrackedDeviceIndex(i++, ((ulong)this.leftIndex >= (ulong)((long)this.connected.Length) || !this.connected[(int)((UIntPtr)this.leftIndex)]) ? uint.MaxValue : this.leftIndex);
			if (this.leftIndex != 4294967295u && this.rightIndex != 4294967295u)
			{
				uint num2 = 0u;
				while ((ulong)num2 < (ulong)((long)this.connected.Length))
				{
					if (i >= this.objects.Length)
					{
						break;
					}
					if (this.connected[(int)((UIntPtr)num2)])
					{
						if (num2 != this.leftIndex && num2 != this.rightIndex)
						{
							this.SetTrackedDeviceIndex(i++, num2);
						}
					}
					num2 += 1u;
				}
			}
		}
		while (i < this.objects.Length)
		{
			this.SetTrackedDeviceIndex(i++, uint.MaxValue);
		}
	}

	
	public GameObject left;

	
	public GameObject right;

	
	[Tooltip("Populate with objects you want to assign to additional controllers")]
	public GameObject[] objects;

	
	[Tooltip("Set to true if you want objects arbitrarily assigned to controllers before their role (left vs right) is identified")]
	public bool assignAllBeforeIdentified;

	
	private uint[] indices;

	
	private bool[] connected = new bool[64];

	
	private uint leftIndex = uint.MaxValue;

	
	private uint rightIndex = uint.MaxValue;

	
	private SteamVR_Events.Action inputFocusAction;

	
	private SteamVR_Events.Action deviceConnectedAction;

	
	private SteamVR_Events.Action trackedDeviceRoleChangedAction;

	
	private static string hiddenPrefix = "hidden (";

	
	private static string hiddenPostfix = ")";

	
	private static string[] labels = new string[]
	{
		"left",
		"right"
	};
}
