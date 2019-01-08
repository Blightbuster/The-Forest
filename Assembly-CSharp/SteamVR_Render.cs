using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

public class SteamVR_Render : MonoBehaviour
{
	public static EVREye eye { get; private set; }

	public static SteamVR_Render instance
	{
		get
		{
			if (SteamVR_Render._instance == null)
			{
				SteamVR_Render._instance = UnityEngine.Object.FindObjectOfType<SteamVR_Render>();
				if (SteamVR_Render._instance == null)
				{
					SteamVR_Render._instance = new GameObject("[SteamVR]").AddComponent<SteamVR_Render>();
				}
			}
			return SteamVR_Render._instance;
		}
	}

	private void OnDestroy()
	{
		SteamVR_Render._instance = null;
	}

	private void OnApplicationQuit()
	{
		SteamVR_Render.isQuitting = true;
		SteamVR.SafeDispose();
	}

	public static void Add(SteamVR_Camera vrcam)
	{
		if (!SteamVR_Render.isQuitting)
		{
			SteamVR_Render.instance.AddInternal(vrcam);
		}
	}

	public static void Remove(SteamVR_Camera vrcam)
	{
		if (!SteamVR_Render.isQuitting && SteamVR_Render._instance != null)
		{
			SteamVR_Render.instance.RemoveInternal(vrcam);
		}
	}

	public static SteamVR_Camera Top()
	{
		if (!SteamVR_Render.isQuitting)
		{
			return SteamVR_Render.instance.TopInternal();
		}
		return null;
	}

	private void AddInternal(SteamVR_Camera vrcam)
	{
		if (vrcam == null)
		{
			return;
		}
		Camera component = vrcam.GetComponent<Camera>();
		if (component == null)
		{
			return;
		}
		int num = this.cameras.Length;
		SteamVR_Camera[] array = new SteamVR_Camera[num + 1];
		int num2 = 0;
		bool flag = false;
		for (int i = 0; i < num; i++)
		{
			SteamVR_Camera steamVR_Camera = this.cameras[i];
			if (!(steamVR_Camera == null))
			{
				if (!array.Contains(steamVR_Camera))
				{
					Camera component2 = steamVR_Camera.GetComponent<Camera>();
					if (!(component2 == null))
					{
						if (i == num2 && component2.depth > component.depth)
						{
							array[num2++] = vrcam;
							flag = true;
						}
						array[num2++] = steamVR_Camera;
					}
				}
			}
		}
		if (!flag)
		{
			array[num2] = vrcam;
		}
		this.cameras = array;
	}

	private void RemoveInternal(SteamVR_Camera vrcam)
	{
		int num = this.cameras.Length;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			SteamVR_Camera x = this.cameras[i];
			if (x == vrcam)
			{
				num2++;
			}
		}
		if (num2 == 0)
		{
			return;
		}
		SteamVR_Camera[] array = new SteamVR_Camera[num - num2];
		int num3 = 0;
		for (int j = 0; j < num; j++)
		{
			SteamVR_Camera steamVR_Camera = this.cameras[j];
			if (steamVR_Camera != vrcam)
			{
				array[num3++] = steamVR_Camera;
			}
		}
		this.cameras = array;
	}

	private SteamVR_Camera TopInternal()
	{
		if (this.cameras.Length > 0)
		{
			return this.cameras[this.cameras.Length - 1];
		}
		return null;
	}

	public static bool pauseRendering
	{
		get
		{
			return SteamVR_Render._pauseRendering;
		}
		set
		{
			SteamVR_Render._pauseRendering = value;
			CVRCompositor compositor = OpenVR.Compositor;
			if (compositor != null)
			{
				compositor.SuspendRendering(value);
			}
		}
	}

	private IEnumerator RenderLoop()
	{
		while (Application.isPlaying)
		{
			yield return this.waitForEndOfFrame;
			if (!SteamVR_Render.pauseRendering)
			{
				CVRCompositor compositor = OpenVR.Compositor;
				if (compositor != null)
				{
					if (!compositor.CanRenderScene())
					{
						continue;
					}
					compositor.SetTrackingSpace(this.trackingSpace);
				}
				SteamVR_Overlay overlay = SteamVR_Overlay.instance;
				if (overlay != null)
				{
					overlay.UpdateOverlay();
				}
				this.RenderExternalCamera();
			}
		}
		yield break;
	}

	private void RenderExternalCamera()
	{
		if (this.externalCamera == null)
		{
			return;
		}
		if (!this.externalCamera.gameObject.activeInHierarchy)
		{
			return;
		}
		int num = (int)Mathf.Max(this.externalCamera.config.frameSkip, 0f);
		if (Time.frameCount % (num + 1) != 0)
		{
			return;
		}
		this.externalCamera.AttachToCamera(this.TopInternal());
		this.externalCamera.RenderNear();
		this.externalCamera.RenderFar();
	}

	private void OnInputFocus(bool hasFocus)
	{
		if (hasFocus)
		{
			if (this.pauseGameWhenDashboardIsVisible)
			{
				Time.timeScale = this.timeScale;
			}
			SteamVR_Camera.sceneResolutionScale = this.sceneResolutionScale;
		}
		else
		{
			if (this.pauseGameWhenDashboardIsVisible)
			{
				this.timeScale = Time.timeScale;
				Time.timeScale = 0f;
			}
			this.sceneResolutionScale = SteamVR_Camera.sceneResolutionScale;
			SteamVR_Camera.sceneResolutionScale = 0.5f;
		}
	}

	private void OnQuit(VREvent_t vrEvent)
	{
		Application.Quit();
	}

	private string GetScreenshotFilename(uint screenshotHandle, EVRScreenshotPropertyFilenames screenshotPropertyFilename)
	{
		EVRScreenshotError evrscreenshotError = EVRScreenshotError.None;
		uint screenshotPropertyFilename2 = OpenVR.Screenshots.GetScreenshotPropertyFilename(screenshotHandle, screenshotPropertyFilename, null, 0u, ref evrscreenshotError);
		if (evrscreenshotError != EVRScreenshotError.None && evrscreenshotError != EVRScreenshotError.BufferTooSmall)
		{
			return null;
		}
		if (screenshotPropertyFilename2 <= 1u)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder((int)screenshotPropertyFilename2);
		OpenVR.Screenshots.GetScreenshotPropertyFilename(screenshotHandle, screenshotPropertyFilename, stringBuilder, screenshotPropertyFilename2, ref evrscreenshotError);
		if (evrscreenshotError != EVRScreenshotError.None)
		{
			return null;
		}
		return stringBuilder.ToString();
	}

	private void OnRequestScreenshot(VREvent_t vrEvent)
	{
		uint handle = vrEvent.data.screenshot.handle;
		EVRScreenshotType type = (EVRScreenshotType)vrEvent.data.screenshot.type;
		if (type == EVRScreenshotType.StereoPanorama)
		{
			string screenshotFilename = this.GetScreenshotFilename(handle, EVRScreenshotPropertyFilenames.Preview);
			string screenshotFilename2 = this.GetScreenshotFilename(handle, EVRScreenshotPropertyFilenames.VR);
			if (screenshotFilename == null || screenshotFilename2 == null)
			{
				return;
			}
			SteamVR_Utils.TakeStereoScreenshot(handle, new GameObject("screenshotPosition")
			{
				transform = 
				{
					position = SteamVR_Render.Top().transform.position,
					rotation = SteamVR_Render.Top().transform.rotation,
					localScale = SteamVR_Render.Top().transform.lossyScale
				}
			}, 32, 0.064f, ref screenshotFilename, ref screenshotFilename2);
			OpenVR.Screenshots.SubmitScreenshot(handle, type, screenshotFilename, screenshotFilename2);
		}
	}

	private void OnEnable()
	{
		base.StartCoroutine(this.RenderLoop());
		SteamVR_Events.InputFocus.Listen(new UnityAction<bool>(this.OnInputFocus));
		SteamVR_Events.System(EVREventType.VREvent_Quit).Listen(new UnityAction<VREvent_t>(this.OnQuit));
		SteamVR_Events.System(EVREventType.VREvent_RequestScreenshot).Listen(new UnityAction<VREvent_t>(this.OnRequestScreenshot));
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(this.OnCameraPreCull));
		if (SteamVR.instance == null)
		{
			base.enabled = false;
			return;
		}
		EVRScreenshotType[] pSupportedTypes = new EVRScreenshotType[]
		{
			EVRScreenshotType.StereoPanorama
		};
		OpenVR.Screenshots.HookScreenshot(pSupportedTypes);
	}

	private void OnDisable()
	{
		if (this._isDuplicateCleanup)
		{
			return;
		}
		base.StopAllCoroutines();
		SteamVR_Events.InputFocus.Remove(new UnityAction<bool>(this.OnInputFocus));
		SteamVR_Events.System(EVREventType.VREvent_Quit).Remove(new UnityAction<VREvent_t>(this.OnQuit));
		SteamVR_Events.System(EVREventType.VREvent_RequestScreenshot).Remove(new UnityAction<VREvent_t>(this.OnRequestScreenshot));
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(this.OnCameraPreCull));
	}

	private void Awake()
	{
		if (SteamVR_Render._instance != null && SteamVR_Render._instance != this)
		{
			this._isDuplicateCleanup = true;
			Debug.LogWarning("Duplicate 'SteamVR_Render'! Destroying new instance");
			UnityEngine.Object.Destroy(this);
			return;
		}
		if (this.externalCamera == null && File.Exists(this.externalCameraConfigPath))
		{
			GameObject original = Resources.Load<GameObject>("SteamVR_ExternalCamera");
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(original);
			gameObject.gameObject.name = "External Camera";
			this.externalCamera = gameObject.transform.GetChild(0).GetComponent<SteamVR_ExternalCamera>();
			this.externalCamera.configPath = this.externalCameraConfigPath;
			this.externalCamera.ReadConfig();
		}
	}

	public void UpdatePoses()
	{
		CVRCompositor compositor = OpenVR.Compositor;
		if (compositor != null)
		{
			compositor.GetLastPoses(this.poses, this.gamePoses);
			SteamVR_Events.NewPoses.Send(this.poses);
			SteamVR_Events.NewPosesApplied.Send();
		}
	}

	private void OnCameraPreCull(Camera cam)
	{
		if (cam.cameraType != CameraType.VR)
		{
			return;
		}
		if (Time.frameCount != SteamVR_Render.lastFrameCount)
		{
			SteamVR_Render.lastFrameCount = Time.frameCount;
			this.UpdatePoses();
		}
	}

	private void Update()
	{
		SteamVR_Controller.Update();
		CVRSystem system = OpenVR.System;
		if (system != null)
		{
			VREvent_t arg = default(VREvent_t);
			uint uncbVREvent = (uint)Marshal.SizeOf(typeof(VREvent_t));
			for (int i = 0; i < 64; i++)
			{
				if (!system.PollNextEvent(ref arg, uncbVREvent))
				{
					break;
				}
				EVREventType eventType = (EVREventType)arg.eventType;
				if (eventType != EVREventType.VREvent_InputFocusCaptured)
				{
					if (eventType != EVREventType.VREvent_InputFocusReleased)
					{
						if (eventType != EVREventType.VREvent_HideRenderModels)
						{
							if (eventType != EVREventType.VREvent_ShowRenderModels)
							{
								SteamVR_Events.System((EVREventType)arg.eventType).Send(arg);
							}
							else
							{
								SteamVR_Events.HideRenderModels.Send(false);
							}
						}
						else
						{
							SteamVR_Events.HideRenderModels.Send(true);
						}
					}
					else if (arg.data.process.pid == 0u)
					{
						SteamVR_Events.InputFocus.Send(true);
					}
				}
				else if (arg.data.process.oldPid == 0u)
				{
					SteamVR_Events.InputFocus.Send(false);
				}
			}
		}
		Application.targetFrameRate = -1;
		Application.runInBackground = true;
		QualitySettings.maxQueuedFrames = -1;
		QualitySettings.vSyncCount = 0;
		if (this.lockPhysicsUpdateRateToRenderFrequency && Time.timeScale > 0f)
		{
			SteamVR instance = SteamVR.instance;
			if (instance != null)
			{
				Compositor_FrameTiming compositor_FrameTiming = default(Compositor_FrameTiming);
				compositor_FrameTiming.m_nSize = (uint)Marshal.SizeOf(typeof(Compositor_FrameTiming));
				instance.compositor.GetFrameTiming(ref compositor_FrameTiming, 0u);
				Time.fixedDeltaTime = Time.timeScale / instance.hmd_DisplayFrequency;
			}
		}
	}

	public bool pauseGameWhenDashboardIsVisible = true;

	public bool lockPhysicsUpdateRateToRenderFrequency = true;

	public SteamVR_ExternalCamera externalCamera;

	public string externalCameraConfigPath = "externalcamera.cfg";

	public ETrackingUniverseOrigin trackingSpace = ETrackingUniverseOrigin.TrackingUniverseStanding;

	private bool _isDuplicateCleanup;

	private static SteamVR_Render _instance;

	private static bool isQuitting;

	private SteamVR_Camera[] cameras = new SteamVR_Camera[0];

	public TrackedDevicePose_t[] poses = new TrackedDevicePose_t[64];

	public TrackedDevicePose_t[] gamePoses = new TrackedDevicePose_t[0];

	private static bool _pauseRendering;

	private WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

	private float sceneResolutionScale = 1f;

	private float timeScale = 1f;

	private static int lastFrameCount = -1;
}
