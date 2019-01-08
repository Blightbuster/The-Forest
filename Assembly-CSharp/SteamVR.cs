using System;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.VR;
using Valve.VR;

public class SteamVR : IDisposable
{
	private SteamVR()
	{
		this.hmd = OpenVR.System;
		Debug.Log("Connected to " + this.hmd_TrackingSystemName + ":" + this.hmd_SerialNumber);
		this.compositor = OpenVR.Compositor;
		this.overlay = OpenVR.Overlay;
		uint num = 0u;
		uint num2 = 0u;
		this.hmd.GetRecommendedRenderTargetSize(ref num, ref num2);
		this.sceneWidth = num;
		this.sceneHeight = num2;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 0f;
		this.hmd.GetProjectionRaw(EVREye.Eye_Left, ref num3, ref num4, ref num5, ref num6);
		float num7 = 0f;
		float num8 = 0f;
		float num9 = 0f;
		float num10 = 0f;
		this.hmd.GetProjectionRaw(EVREye.Eye_Right, ref num7, ref num8, ref num9, ref num10);
		this.tanHalfFov = new Vector2(Mathf.Max(new float[]
		{
			-num3,
			num4,
			-num7,
			num8
		}), Mathf.Max(new float[]
		{
			-num5,
			num6,
			-num9,
			num10
		}));
		this.textureBounds = new VRTextureBounds_t[2];
		this.textureBounds[0].uMin = 0.5f + 0.5f * num3 / this.tanHalfFov.x;
		this.textureBounds[0].uMax = 0.5f + 0.5f * num4 / this.tanHalfFov.x;
		this.textureBounds[0].vMin = 0.5f - 0.5f * num6 / this.tanHalfFov.y;
		this.textureBounds[0].vMax = 0.5f - 0.5f * num5 / this.tanHalfFov.y;
		this.textureBounds[1].uMin = 0.5f + 0.5f * num7 / this.tanHalfFov.x;
		this.textureBounds[1].uMax = 0.5f + 0.5f * num8 / this.tanHalfFov.x;
		this.textureBounds[1].vMin = 0.5f - 0.5f * num10 / this.tanHalfFov.y;
		this.textureBounds[1].vMax = 0.5f - 0.5f * num9 / this.tanHalfFov.y;
		this.sceneWidth /= Mathf.Max(this.textureBounds[0].uMax - this.textureBounds[0].uMin, this.textureBounds[1].uMax - this.textureBounds[1].uMin);
		this.sceneHeight /= Mathf.Max(this.textureBounds[0].vMax - this.textureBounds[0].vMin, this.textureBounds[1].vMax - this.textureBounds[1].vMin);
		this.aspect = this.tanHalfFov.x / this.tanHalfFov.y;
		this.fieldOfView = 2f * Mathf.Atan(this.tanHalfFov.y) * 57.29578f;
		this.eyes = new SteamVR_Utils.RigidTransform[]
		{
			new SteamVR_Utils.RigidTransform(this.hmd.GetEyeToHeadTransform(EVREye.Eye_Left)),
			new SteamVR_Utils.RigidTransform(this.hmd.GetEyeToHeadTransform(EVREye.Eye_Right))
		};
		GraphicsDeviceType graphicsDeviceType = SystemInfo.graphicsDeviceType;
		switch (graphicsDeviceType)
		{
		case GraphicsDeviceType.OpenGLES2:
		case GraphicsDeviceType.OpenGLES3:
			break;
		default:
			if (graphicsDeviceType != GraphicsDeviceType.OpenGLCore)
			{
				if (graphicsDeviceType != GraphicsDeviceType.Vulkan)
				{
					this.textureType = ETextureType.DirectX;
					goto IL_433;
				}
				this.textureType = ETextureType.Vulkan;
				goto IL_433;
			}
			break;
		}
		this.textureType = ETextureType.OpenGL;
		IL_433:
		SteamVR_Events.Initializing.Listen(new UnityAction<bool>(this.OnInitializing));
		SteamVR_Events.Calibrating.Listen(new UnityAction<bool>(this.OnCalibrating));
		SteamVR_Events.OutOfRange.Listen(new UnityAction<bool>(this.OnOutOfRange));
		SteamVR_Events.DeviceConnected.Listen(new UnityAction<int, bool>(this.OnDeviceConnected));
		SteamVR_Events.NewPoses.Listen(new UnityAction<TrackedDevicePose_t[]>(this.OnNewPoses));
	}

	public static bool active
	{
		get
		{
			return SteamVR._instance != null;
		}
	}

	public static bool enabled
	{
		get
		{
			if (!VRSettings.enabled)
			{
				SteamVR.enabled = false;
			}
			return SteamVR._enabled;
		}
		set
		{
			SteamVR._enabled = value;
			if (!SteamVR._enabled)
			{
				SteamVR.SafeDispose();
			}
		}
	}

	public static SteamVR instance
	{
		get
		{
			if (!SteamVR.enabled)
			{
				return null;
			}
			if (SteamVR._instance == null)
			{
				SteamVR._instance = SteamVR.CreateInstance();
				if (SteamVR._instance == null)
				{
					SteamVR._enabled = false;
				}
			}
			return SteamVR._instance;
		}
	}

	public static bool usingNativeSupport
	{
		get
		{
			return VRDevice.GetNativePtr() != IntPtr.Zero;
		}
	}

	private static SteamVR CreateInstance()
	{
		try
		{
			EVRInitError evrinitError = EVRInitError.None;
			if (!SteamVR.usingNativeSupport)
			{
				Debug.Log("OpenVR initialization failed.  Ensure 'Virtual Reality Supported' is checked in Player Settings, and OpenVR is added to the list of Virtual Reality SDKs.");
				return null;
			}
			OpenVR.GetGenericInterface("IVRCompositor_022", ref evrinitError);
			if (evrinitError != EVRInitError.None)
			{
				SteamVR.ReportError(evrinitError);
				return null;
			}
			OpenVR.GetGenericInterface("IVROverlay_018", ref evrinitError);
			if (evrinitError != EVRInitError.None)
			{
				SteamVR.ReportError(evrinitError);
				return null;
			}
		}
		catch (Exception message)
		{
			Debug.LogError(message);
			return null;
		}
		return new SteamVR();
	}

	private static void ReportError(EVRInitError error)
	{
		if (error != EVRInitError.None)
		{
			if (error != EVRInitError.Init_VRClientDLLNotFound)
			{
				if (error != EVRInitError.Driver_RuntimeOutOfDate)
				{
					if (error != EVRInitError.VendorSpecific_UnableToConnectToOculusRuntime)
					{
						Debug.Log(OpenVR.GetStringForHmdError(error));
					}
					else
					{
						Debug.Log("SteamVR Initialization Failed!  Make sure device is on, Oculus runtime is installed, and OVRService_*.exe is running.");
					}
				}
				else
				{
					Debug.Log("SteamVR Initialization Failed!  Make sure device's runtime is up to date.");
				}
			}
			else
			{
				Debug.Log("SteamVR drivers not found!  They can be installed via Steam under Library > Tools.  Visit http://steampowered.com to install Steam.");
			}
		}
	}

	public CVRSystem hmd { get; private set; }

	public CVRCompositor compositor { get; private set; }

	public CVROverlay overlay { get; private set; }

	public static bool initializing { get; private set; }

	public static bool calibrating { get; private set; }

	public static bool outOfRange { get; private set; }

	public float sceneWidth { get; private set; }

	public float sceneHeight { get; private set; }

	public float aspect { get; private set; }

	public float fieldOfView { get; private set; }

	public Vector2 tanHalfFov { get; private set; }

	public VRTextureBounds_t[] textureBounds { get; private set; }

	public SteamVR_Utils.RigidTransform[] eyes { get; private set; }

	public string hmd_TrackingSystemName
	{
		get
		{
			return this.GetStringProperty(ETrackedDeviceProperty.Prop_TrackingSystemName_String, 0u);
		}
	}

	public string hmd_ModelNumber
	{
		get
		{
			return this.GetStringProperty(ETrackedDeviceProperty.Prop_ModelNumber_String, 0u);
		}
	}

	public string hmd_SerialNumber
	{
		get
		{
			return this.GetStringProperty(ETrackedDeviceProperty.Prop_SerialNumber_String, 0u);
		}
	}

	public float hmd_SecondsFromVsyncToPhotons
	{
		get
		{
			return this.GetFloatProperty(ETrackedDeviceProperty.Prop_SecondsFromVsyncToPhotons_Float, 0u);
		}
	}

	public float hmd_DisplayFrequency
	{
		get
		{
			return this.GetFloatProperty(ETrackedDeviceProperty.Prop_DisplayFrequency_Float, 0u);
		}
	}

	public string GetTrackedDeviceString(uint deviceId)
	{
		ETrackedPropertyError etrackedPropertyError = ETrackedPropertyError.TrackedProp_Success;
		uint stringTrackedDeviceProperty = this.hmd.GetStringTrackedDeviceProperty(deviceId, ETrackedDeviceProperty.Prop_AttachedDeviceId_String, null, 0u, ref etrackedPropertyError);
		if (stringTrackedDeviceProperty > 1u)
		{
			StringBuilder stringBuilder = new StringBuilder((int)stringTrackedDeviceProperty);
			this.hmd.GetStringTrackedDeviceProperty(deviceId, ETrackedDeviceProperty.Prop_AttachedDeviceId_String, stringBuilder, stringTrackedDeviceProperty, ref etrackedPropertyError);
			return stringBuilder.ToString();
		}
		return null;
	}

	public string GetStringProperty(ETrackedDeviceProperty prop, uint deviceId = 0u)
	{
		ETrackedPropertyError etrackedPropertyError = ETrackedPropertyError.TrackedProp_Success;
		uint stringTrackedDeviceProperty = this.hmd.GetStringTrackedDeviceProperty(deviceId, prop, null, 0u, ref etrackedPropertyError);
		if (stringTrackedDeviceProperty > 1u)
		{
			StringBuilder stringBuilder = new StringBuilder((int)stringTrackedDeviceProperty);
			this.hmd.GetStringTrackedDeviceProperty(deviceId, prop, stringBuilder, stringTrackedDeviceProperty, ref etrackedPropertyError);
			return stringBuilder.ToString();
		}
		return (etrackedPropertyError == ETrackedPropertyError.TrackedProp_Success) ? "<unknown>" : etrackedPropertyError.ToString();
	}

	public float GetFloatProperty(ETrackedDeviceProperty prop, uint deviceId = 0u)
	{
		ETrackedPropertyError etrackedPropertyError = ETrackedPropertyError.TrackedProp_Success;
		return this.hmd.GetFloatTrackedDeviceProperty(deviceId, prop, ref etrackedPropertyError);
	}

	private void OnInitializing(bool initializing)
	{
		SteamVR.initializing = initializing;
	}

	private void OnCalibrating(bool calibrating)
	{
		SteamVR.calibrating = calibrating;
	}

	private void OnOutOfRange(bool outOfRange)
	{
		SteamVR.outOfRange = outOfRange;
	}

	private void OnDeviceConnected(int i, bool connected)
	{
		SteamVR.connected[i] = connected;
	}

	private void OnNewPoses(TrackedDevicePose_t[] poses)
	{
		this.eyes[0] = new SteamVR_Utils.RigidTransform(this.hmd.GetEyeToHeadTransform(EVREye.Eye_Left));
		this.eyes[1] = new SteamVR_Utils.RigidTransform(this.hmd.GetEyeToHeadTransform(EVREye.Eye_Right));
		for (int i = 0; i < poses.Length; i++)
		{
			bool bDeviceIsConnected = poses[i].bDeviceIsConnected;
			if (bDeviceIsConnected != SteamVR.connected[i])
			{
				SteamVR_Events.DeviceConnected.Send(i, bDeviceIsConnected);
			}
		}
		if ((long)poses.Length > 0L)
		{
			ETrackingResult eTrackingResult = poses[(int)((UIntPtr)0)].eTrackingResult;
			bool flag = eTrackingResult == ETrackingResult.Uninitialized;
			if (flag != SteamVR.initializing)
			{
				SteamVR_Events.Initializing.Send(flag);
			}
			bool flag2 = eTrackingResult == ETrackingResult.Calibrating_InProgress || eTrackingResult == ETrackingResult.Calibrating_OutOfRange;
			if (flag2 != SteamVR.calibrating)
			{
				SteamVR_Events.Calibrating.Send(flag2);
			}
			bool flag3 = eTrackingResult == ETrackingResult.Running_OutOfRange || eTrackingResult == ETrackingResult.Calibrating_OutOfRange;
			if (flag3 != SteamVR.outOfRange)
			{
				SteamVR_Events.OutOfRange.Send(flag3);
			}
		}
	}

	~SteamVR()
	{
		this.Dispose(false);
	}

	public void Dispose()
	{
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		SteamVR_Events.Initializing.Remove(new UnityAction<bool>(this.OnInitializing));
		SteamVR_Events.Calibrating.Remove(new UnityAction<bool>(this.OnCalibrating));
		SteamVR_Events.OutOfRange.Remove(new UnityAction<bool>(this.OnOutOfRange));
		SteamVR_Events.DeviceConnected.Remove(new UnityAction<int, bool>(this.OnDeviceConnected));
		SteamVR_Events.NewPoses.Remove(new UnityAction<TrackedDevicePose_t[]>(this.OnNewPoses));
		SteamVR._instance = null;
	}

	public static void SafeDispose()
	{
		if (SteamVR._instance != null)
		{
			SteamVR._instance.Dispose();
		}
	}

	private static bool _enabled = true;

	private static SteamVR _instance;

	public static bool[] connected = new bool[64];

	public ETextureType textureType;
}
