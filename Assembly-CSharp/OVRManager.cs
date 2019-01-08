using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VR;

public class OVRManager : MonoBehaviour
{
	public static OVRManager instance { get; private set; }

	public static OVRDisplay display { get; private set; }

	public static OVRTracker tracker { get; private set; }

	public static OVRBoundary boundary { get; private set; }

	public static OVRProfile profile
	{
		get
		{
			if (OVRManager._profile == null)
			{
				OVRManager._profile = new OVRProfile();
			}
			return OVRManager._profile;
		}
	}

	public static event Action HMDAcquired;

	public static event Action HMDLost;

	public static event Action HMDMounted;

	public static event Action HMDUnmounted;

	public static event Action VrFocusAcquired;

	public static event Action VrFocusLost;

	public static event Action InputFocusAcquired;

	public static event Action InputFocusLost;

	public static event Action AudioOutChanged;

	public static event Action AudioInChanged;

	public static event Action TrackingAcquired;

	public static event Action TrackingLost;

	[Obsolete]
	public static event Action HSWDismissed;

	public static bool isHmdPresent
	{
		get
		{
			if (!OVRManager._isHmdPresentCached)
			{
				OVRManager._isHmdPresentCached = true;
				OVRManager._isHmdPresent = OVRPlugin.hmdPresent;
			}
			return OVRManager._isHmdPresent;
		}
		private set
		{
			OVRManager._isHmdPresentCached = true;
			OVRManager._isHmdPresent = value;
		}
	}

	public static string audioOutId
	{
		get
		{
			return OVRPlugin.audioOutId;
		}
	}

	public static string audioInId
	{
		get
		{
			return OVRPlugin.audioInId;
		}
	}

	public static bool hasVrFocus
	{
		get
		{
			if (!OVRManager._hasVrFocusCached)
			{
				OVRManager._hasVrFocusCached = true;
				OVRManager._hasVrFocus = OVRPlugin.hasVrFocus;
			}
			return OVRManager._hasVrFocus;
		}
		private set
		{
			OVRManager._hasVrFocusCached = true;
			OVRManager._hasVrFocus = value;
		}
	}

	public static bool hasInputFocus
	{
		get
		{
			return OVRPlugin.hasInputFocus;
		}
	}

	public bool chromatic
	{
		get
		{
			return OVRManager.isHmdPresent && OVRPlugin.chromatic;
		}
		set
		{
			if (!OVRManager.isHmdPresent)
			{
				return;
			}
			OVRPlugin.chromatic = value;
		}
	}

	public bool monoscopic
	{
		get
		{
			return !OVRManager.isHmdPresent || OVRPlugin.monoscopic;
		}
		set
		{
			if (!OVRManager.isHmdPresent)
			{
				return;
			}
			OVRPlugin.monoscopic = value;
		}
	}

	public static bool IsAdaptiveResSupportedByEngine()
	{
		return false;
	}

	public int vsyncCount
	{
		get
		{
			if (!OVRManager.isHmdPresent)
			{
				return 1;
			}
			return OVRPlugin.vsyncCount;
		}
		set
		{
			if (!OVRManager.isHmdPresent)
			{
				return;
			}
			OVRPlugin.vsyncCount = value;
		}
	}

	public static float batteryLevel
	{
		get
		{
			if (!OVRManager.isHmdPresent)
			{
				return 1f;
			}
			return OVRPlugin.batteryLevel;
		}
	}

	public static float batteryTemperature
	{
		get
		{
			if (!OVRManager.isHmdPresent)
			{
				return 0f;
			}
			return OVRPlugin.batteryTemperature;
		}
	}

	public static int batteryStatus
	{
		get
		{
			if (!OVRManager.isHmdPresent)
			{
				return -1;
			}
			return (int)OVRPlugin.batteryStatus;
		}
	}

	public static float volumeLevel
	{
		get
		{
			if (!OVRManager.isHmdPresent)
			{
				return 0f;
			}
			return OVRPlugin.systemVolume;
		}
	}

	public static int cpuLevel
	{
		get
		{
			if (!OVRManager.isHmdPresent)
			{
				return 2;
			}
			return OVRPlugin.cpuLevel;
		}
		set
		{
			if (!OVRManager.isHmdPresent)
			{
				return;
			}
			OVRPlugin.cpuLevel = value;
		}
	}

	public static int gpuLevel
	{
		get
		{
			if (!OVRManager.isHmdPresent)
			{
				return 2;
			}
			return OVRPlugin.gpuLevel;
		}
		set
		{
			if (!OVRManager.isHmdPresent)
			{
				return;
			}
			OVRPlugin.gpuLevel = value;
		}
	}

	public static bool isPowerSavingActive
	{
		get
		{
			return OVRManager.isHmdPresent && OVRPlugin.powerSaving;
		}
	}

	public static OVRManager.EyeTextureFormat eyeTextureFormat
	{
		get
		{
			return (OVRManager.EyeTextureFormat)OVRPlugin.GetDesiredEyeTextureFormat();
		}
		set
		{
			OVRPlugin.SetDesiredEyeTextureFormat((OVRPlugin.EyeTextureFormat)value);
		}
	}

	public static bool tiledMultiResSupported
	{
		get
		{
			return OVRPlugin.tiledMultiResSupported;
		}
	}

	public static OVRManager.TiledMultiResLevel tiledMultiResLevel
	{
		get
		{
			if (!OVRPlugin.tiledMultiResSupported)
			{
				Debug.LogWarning("Tiled-based Multi-resolution feature is not supported");
			}
			return (OVRManager.TiledMultiResLevel)OVRPlugin.tiledMultiResLevel;
		}
		set
		{
			if (!OVRPlugin.tiledMultiResSupported)
			{
				Debug.LogWarning("Tiled-based Multi-resolution feature is not supported");
			}
			OVRPlugin.tiledMultiResLevel = (OVRPlugin.TiledMultiResLevel)value;
		}
	}

	public static bool gpuUtilSupported
	{
		get
		{
			return OVRPlugin.gpuUtilSupported;
		}
	}

	public static float gpuUtilLevel
	{
		get
		{
			if (!OVRPlugin.gpuUtilSupported)
			{
				Debug.LogWarning("GPU Util is not supported");
			}
			return OVRPlugin.gpuUtilLevel;
		}
	}

	public OVRManager.TrackingOrigin trackingOriginType
	{
		get
		{
			if (!OVRManager.isHmdPresent)
			{
				return this._trackingOriginType;
			}
			return (OVRManager.TrackingOrigin)OVRPlugin.GetTrackingOriginType();
		}
		set
		{
			if (!OVRManager.isHmdPresent)
			{
				return;
			}
			if (OVRPlugin.SetTrackingOriginType((OVRPlugin.TrackingOrigin)value))
			{
				this._trackingOriginType = value;
			}
		}
	}

	public bool isSupportedPlatform { get; private set; }

	public bool isUserPresent
	{
		get
		{
			if (!OVRManager._isUserPresentCached)
			{
				OVRManager._isUserPresentCached = true;
				OVRManager._isUserPresent = OVRPlugin.userPresent;
			}
			return OVRManager._isUserPresent;
		}
		private set
		{
			OVRManager._isUserPresentCached = true;
			OVRManager._isUserPresent = value;
		}
	}

	public static Version utilitiesVersion
	{
		get
		{
			return OVRPlugin.wrapperVersion;
		}
	}

	public static Version pluginVersion
	{
		get
		{
			return OVRPlugin.version;
		}
	}

	public static Version sdkVersion
	{
		get
		{
			return OVRPlugin.nativeSDKVersion;
		}
	}

	private static bool MixedRealityEnabledFromCmd()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i].ToLower() == "-mixedreality")
			{
				return true;
			}
		}
		return false;
	}

	private static bool UseDirectCompositionFromCmd()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i].ToLower() == "-directcomposition")
			{
				return true;
			}
		}
		return false;
	}

	private static bool UseExternalCompositionFromCmd()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i].ToLower() == "-externalcomposition")
			{
				return true;
			}
		}
		return false;
	}

	private static bool CreateMixedRealityCaptureConfigurationFileFromCmd()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i].ToLower() == "-create_mrc_config")
			{
				return true;
			}
		}
		return false;
	}

	private static bool LoadMixedRealityCaptureConfigurationFileFromCmd()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i].ToLower() == "-load_mrc_config")
			{
				return true;
			}
		}
		return false;
	}

	private void Awake()
	{
		if (OVRManager.instance != null)
		{
			base.enabled = false;
			UnityEngine.Object.DestroyImmediate(this);
			return;
		}
		OVRManager.instance = this;
		Debug.Log(string.Concat(new object[]
		{
			"Unity v",
			Application.unityVersion,
			", Oculus Utilities v",
			OVRPlugin.wrapperVersion,
			", OVRPlugin v",
			OVRPlugin.version,
			", SDK v",
			OVRPlugin.nativeSDKVersion,
			"."
		}));
		string text = GraphicsDeviceType.Direct3D11.ToString() + ", " + GraphicsDeviceType.Direct3D12.ToString();
		if (!text.Contains(SystemInfo.graphicsDeviceType.ToString()))
		{
			Debug.LogWarning("VR rendering requires one of the following device types: (" + text + "). Your graphics device: " + SystemInfo.graphicsDeviceType.ToString());
		}
		RuntimePlatform platform = Application.platform;
		this.isSupportedPlatform |= (platform == RuntimePlatform.Android);
		this.isSupportedPlatform |= (platform == RuntimePlatform.OSXEditor);
		this.isSupportedPlatform |= (platform == RuntimePlatform.OSXPlayer);
		this.isSupportedPlatform |= (platform == RuntimePlatform.WindowsEditor);
		this.isSupportedPlatform |= (platform == RuntimePlatform.WindowsPlayer);
		if (!this.isSupportedPlatform)
		{
			Debug.LogWarning("This platform is unsupported");
			return;
		}
		this.enableMixedReality = false;
		bool flag = OVRManager.LoadMixedRealityCaptureConfigurationFileFromCmd();
		bool flag2 = OVRManager.CreateMixedRealityCaptureConfigurationFileFromCmd();
		if (flag || flag2)
		{
			OVRMixedRealityCaptureSettings ovrmixedRealityCaptureSettings = ScriptableObject.CreateInstance<OVRMixedRealityCaptureSettings>();
			ovrmixedRealityCaptureSettings.ReadFrom(this);
			if (flag)
			{
				ovrmixedRealityCaptureSettings.CombineWithConfigurationFile();
				ovrmixedRealityCaptureSettings.ApplyTo(this);
			}
			if (flag2)
			{
				ovrmixedRealityCaptureSettings.WriteToConfigurationFile();
			}
			UnityEngine.Object.Destroy(ovrmixedRealityCaptureSettings);
		}
		if (OVRManager.MixedRealityEnabledFromCmd())
		{
			this.enableMixedReality = true;
		}
		if (this.enableMixedReality)
		{
			Debug.Log("OVR: Mixed Reality mode enabled");
			if (OVRManager.UseDirectCompositionFromCmd())
			{
				this.compositionMethod = OVRManager.CompositionMethod.Direct;
			}
			if (OVRManager.UseExternalCompositionFromCmd())
			{
				this.compositionMethod = OVRManager.CompositionMethod.External;
			}
			Debug.Log("OVR: CompositionMethod : " + this.compositionMethod);
		}
		if (this.enableAdaptiveResolution && !OVRManager.IsAdaptiveResSupportedByEngine())
		{
			this.enableAdaptiveResolution = false;
			Debug.LogError("Your current Unity Engine " + Application.unityVersion + " might have issues to support adaptive resolution, please disable it under OVRManager");
		}
		this.Initialize();
		if (this.resetTrackerOnLoad)
		{
			OVRManager.display.RecenterPose();
		}
		OVRPlugin.occlusionMesh = true;
	}

	private void Initialize()
	{
		if (OVRManager.display == null)
		{
			OVRManager.display = new OVRDisplay();
		}
		if (OVRManager.tracker == null)
		{
			OVRManager.tracker = new OVRTracker();
		}
		if (OVRManager.boundary == null)
		{
			OVRManager.boundary = new OVRBoundary();
		}
	}

	private void Update()
	{
		if (OVRPlugin.shouldQuit)
		{
			Application.Quit();
		}
		if (this.AllowRecenter && OVRPlugin.shouldRecenter)
		{
			OVRManager.display.RecenterPose();
		}
		if (this.trackingOriginType != this._trackingOriginType)
		{
			this.trackingOriginType = this._trackingOriginType;
		}
		OVRManager.tracker.isEnabled = this.usePositionTracking;
		OVRPlugin.rotation = this.useRotationTracking;
		OVRPlugin.useIPDInPositionTracking = this.useIPDInPositionTracking;
		OVRManager.isHmdPresent = OVRPlugin.hmdPresent;
		if (this.useRecommendedMSAALevel && QualitySettings.antiAliasing != OVRManager.display.recommendedMSAALevel)
		{
			Debug.Log(string.Concat(new object[]
			{
				"The current MSAA level is ",
				QualitySettings.antiAliasing,
				", but the recommended MSAA level is ",
				OVRManager.display.recommendedMSAALevel,
				". Switching to the recommended level."
			}));
			QualitySettings.antiAliasing = OVRManager.display.recommendedMSAALevel;
		}
		if (OVRManager._wasHmdPresent && !OVRManager.isHmdPresent)
		{
			try
			{
				if (OVRManager.HMDLost != null)
				{
					OVRManager.HMDLost();
				}
			}
			catch (Exception arg)
			{
				Debug.LogError("Caught Exception: " + arg);
			}
		}
		if (!OVRManager._wasHmdPresent && OVRManager.isHmdPresent)
		{
			try
			{
				if (OVRManager.HMDAcquired != null)
				{
					OVRManager.HMDAcquired();
				}
			}
			catch (Exception arg2)
			{
				Debug.LogError("Caught Exception: " + arg2);
			}
		}
		OVRManager._wasHmdPresent = OVRManager.isHmdPresent;
		this.isUserPresent = OVRPlugin.userPresent;
		if (OVRManager._wasUserPresent && !this.isUserPresent)
		{
			try
			{
				if (OVRManager.HMDUnmounted != null)
				{
					OVRManager.HMDUnmounted();
				}
			}
			catch (Exception arg3)
			{
				Debug.LogError("Caught Exception: " + arg3);
			}
		}
		if (!OVRManager._wasUserPresent && this.isUserPresent)
		{
			try
			{
				if (OVRManager.HMDMounted != null)
				{
					OVRManager.HMDMounted();
				}
			}
			catch (Exception arg4)
			{
				Debug.LogError("Caught Exception: " + arg4);
			}
		}
		OVRManager._wasUserPresent = this.isUserPresent;
		OVRManager.hasVrFocus = OVRPlugin.hasVrFocus;
		if (OVRManager._hadVrFocus && !OVRManager.hasVrFocus)
		{
			try
			{
				if (OVRManager.VrFocusLost != null)
				{
					OVRManager.VrFocusLost();
				}
			}
			catch (Exception arg5)
			{
				Debug.LogError("Caught Exception: " + arg5);
			}
		}
		if (!OVRManager._hadVrFocus && OVRManager.hasVrFocus)
		{
			try
			{
				if (OVRManager.VrFocusAcquired != null)
				{
					OVRManager.VrFocusAcquired();
				}
			}
			catch (Exception arg6)
			{
				Debug.LogError("Caught Exception: " + arg6);
			}
		}
		OVRManager._hadVrFocus = OVRManager.hasVrFocus;
		bool hasInputFocus = OVRPlugin.hasInputFocus;
		if (OVRManager._hadInputFocus && !hasInputFocus)
		{
			try
			{
				if (OVRManager.InputFocusLost != null)
				{
					OVRManager.InputFocusLost();
				}
			}
			catch (Exception arg7)
			{
				Debug.LogError("Caught Exception: " + arg7);
			}
		}
		if (!OVRManager._hadInputFocus && hasInputFocus)
		{
			try
			{
				if (OVRManager.InputFocusAcquired != null)
				{
					OVRManager.InputFocusAcquired();
				}
			}
			catch (Exception arg8)
			{
				Debug.LogError("Caught Exception: " + arg8);
			}
		}
		OVRManager._hadInputFocus = hasInputFocus;
		if (this.enableAdaptiveResolution)
		{
			if (VRSettings.renderScale < this.maxRenderScale)
			{
				VRSettings.renderScale = this.maxRenderScale;
			}
			else
			{
				this.maxRenderScale = Mathf.Max(this.maxRenderScale, VRSettings.renderScale);
			}
			this.minRenderScale = Mathf.Min(this.minRenderScale, this.maxRenderScale);
			float min = this.minRenderScale / VRSettings.renderScale;
			float num = OVRPlugin.GetEyeRecommendedResolutionScale() / VRSettings.renderScale;
			num = Mathf.Clamp(num, min, 1f);
			VRSettings.renderViewportScale = num;
		}
		string audioOutId = OVRPlugin.audioOutId;
		if (!OVRManager.prevAudioOutIdIsCached)
		{
			OVRManager.prevAudioOutId = audioOutId;
			OVRManager.prevAudioOutIdIsCached = true;
		}
		else if (audioOutId != OVRManager.prevAudioOutId)
		{
			try
			{
				if (OVRManager.AudioOutChanged != null)
				{
					OVRManager.AudioOutChanged();
				}
			}
			catch (Exception arg9)
			{
				Debug.LogError("Caught Exception: " + arg9);
			}
			OVRManager.prevAudioOutId = audioOutId;
		}
		string audioInId = OVRPlugin.audioInId;
		if (!OVRManager.prevAudioInIdIsCached)
		{
			OVRManager.prevAudioInId = audioInId;
			OVRManager.prevAudioInIdIsCached = true;
		}
		else if (audioInId != OVRManager.prevAudioInId)
		{
			try
			{
				if (OVRManager.AudioInChanged != null)
				{
					OVRManager.AudioInChanged();
				}
			}
			catch (Exception arg10)
			{
				Debug.LogError("Caught Exception: " + arg10);
			}
			OVRManager.prevAudioInId = audioInId;
		}
		if (OVRManager.wasPositionTracked && !OVRManager.tracker.isPositionTracked)
		{
			try
			{
				if (OVRManager.TrackingLost != null)
				{
					OVRManager.TrackingLost();
				}
			}
			catch (Exception arg11)
			{
				Debug.LogError("Caught Exception: " + arg11);
			}
		}
		if (!OVRManager.wasPositionTracked && OVRManager.tracker.isPositionTracked)
		{
			try
			{
				if (OVRManager.TrackingAcquired != null)
				{
					OVRManager.TrackingAcquired();
				}
			}
			catch (Exception arg12)
			{
				Debug.LogError("Caught Exception: " + arg12);
			}
		}
		OVRManager.wasPositionTracked = OVRManager.tracker.isPositionTracked;
		OVRManager.display.Update();
		OVRInput.Update();
		if (this.enableMixedReality || OVRManager.prevEnableMixedReality)
		{
			Camera mainCamera = this.FindMainCamera();
			if (Camera.main != null)
			{
				this.suppressDisableMixedRealityBecauseOfNoMainCameraWarning = false;
				if (this.enableMixedReality)
				{
					OVRMixedReality.Update(base.gameObject, mainCamera, this.compositionMethod, this.useDynamicLighting, this.capturingCameraDevice, this.depthQuality);
				}
				if (OVRManager.prevEnableMixedReality && !this.enableMixedReality)
				{
					OVRMixedReality.Cleanup();
				}
				OVRManager.prevEnableMixedReality = this.enableMixedReality;
			}
			else if (!this.suppressDisableMixedRealityBecauseOfNoMainCameraWarning)
			{
				Debug.LogWarning("Main Camera is not set, Mixed Reality disabled");
				this.suppressDisableMixedRealityBecauseOfNoMainCameraWarning = true;
			}
		}
	}

	private Camera FindMainCamera()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("MainCamera");
		List<Camera> list = new List<Camera>(4);
		foreach (GameObject gameObject in array)
		{
			Camera component = gameObject.GetComponent<Camera>();
			if (component != null && component.enabled)
			{
				OVRCameraRig componentInParent = component.GetComponentInParent<OVRCameraRig>();
				if (componentInParent != null && componentInParent.trackingSpace != null)
				{
					list.Add(component);
				}
			}
		}
		if (list.Count == 0)
		{
			return Camera.main;
		}
		if (list.Count == 1)
		{
			return list[0];
		}
		if (!this.multipleMainCameraWarningPresented)
		{
			Debug.LogWarning("Multiple MainCamera found. Assume the real MainCamera is the camera with the least depth");
			this.multipleMainCameraWarningPresented = true;
		}
		list.Sort((Camera c0, Camera c1) => (c0.depth >= c1.depth) ? ((c0.depth <= c1.depth) ? 0 : 1) : -1);
		return list[0];
	}

	private void OnDisable()
	{
		OVRMixedReality.Cleanup();
	}

	private void LateUpdate()
	{
		OVRHaptics.Process();
	}

	private void FixedUpdate()
	{
		OVRInput.FixedUpdate();
	}

	public void ReturnToLauncher()
	{
		OVRManager.PlatformUIConfirmQuit();
	}

	public static void PlatformUIConfirmQuit()
	{
		if (!OVRManager.isHmdPresent)
		{
			return;
		}
		OVRPlugin.ShowUI(OVRPlugin.PlatformUI.ConfirmQuit);
	}

	private static OVRProfile _profile;

	private IEnumerable<Camera> disabledCameras;

	private float prevTimeScale;

	private static bool _isHmdPresentCached = false;

	private static bool _isHmdPresent = false;

	private static bool _wasHmdPresent = false;

	private static bool _hasVrFocusCached = false;

	private static bool _hasVrFocus = false;

	private static bool _hadVrFocus = false;

	private static bool _hadInputFocus = true;

	[Header("Performance/Quality")]
	[Tooltip("If true, distortion rendering work is submitted a quarter-frame early to avoid pipeline stalls and increase CPU-GPU parallelism.")]
	public bool queueAhead = true;

	[Tooltip("If true, Unity will use the optimal antialiasing level for quality/performance on the current hardware.")]
	public bool useRecommendedMSAALevel;

	[Tooltip("If true, dynamic resolution will be enabled On PC")]
	public bool enableAdaptiveResolution;

	[Range(0.5f, 2f)]
	[Tooltip("Min RenderScale the app can reach under adaptive resolution mode")]
	public float minRenderScale = 0.7f;

	[Range(0.5f, 2f)]
	[Tooltip("Max RenderScale the app can reach under adaptive resolution mode")]
	public float maxRenderScale = 1f;

	[HideInInspector]
	public bool expandMixedRealityCapturePropertySheet;

	[HideInInspector]
	[Tooltip("If true, Mixed Reality mode will be enabled. It would be always set to false when the game is launching without editor")]
	public bool enableMixedReality;

	[HideInInspector]
	public OVRManager.CompositionMethod compositionMethod;

	[HideInInspector]
	[Tooltip("Extra hidden layers")]
	public LayerMask extraHiddenLayers;

	[HideInInspector]
	[Tooltip("The camera device for direct composition")]
	public OVRManager.CameraDevice capturingCameraDevice;

	[HideInInspector]
	[Tooltip("Flip the camera frame horizontally")]
	public bool flipCameraFrameHorizontally;

	[HideInInspector]
	[Tooltip("Flip the camera frame vertically")]
	public bool flipCameraFrameVertically;

	[HideInInspector]
	[Tooltip("Delay the touch controller pose by a short duration (0 to 0.5 second) to match the physical camera latency")]
	public float handPoseStateLatency;

	[HideInInspector]
	[Tooltip("Delay the foreground / background image in the sandwich composition to match the physical camera latency. The maximum duration is sandwichCompositionBufferedFrames / {Game FPS}")]
	public float sandwichCompositionRenderLatency;

	[HideInInspector]
	[Tooltip("The number of frames are buffered in the SandWich composition. The more buffered frames, the more memory it would consume.")]
	public int sandwichCompositionBufferedFrames = 8;

	[HideInInspector]
	[Tooltip("Chroma Key Color")]
	public Color chromaKeyColor = Color.green;

	[HideInInspector]
	[Tooltip("Chroma Key Similarity")]
	public float chromaKeySimilarity = 0.6f;

	[HideInInspector]
	[Tooltip("Chroma Key Smooth Range")]
	public float chromaKeySmoothRange = 0.03f;

	[HideInInspector]
	[Tooltip("Chroma Key Spill Range")]
	public float chromaKeySpillRange = 0.06f;

	[HideInInspector]
	[Tooltip("Use dynamic lighting (Depth sensor required)")]
	public bool useDynamicLighting;

	[HideInInspector]
	[Tooltip("The quality level of depth image. The lighting could be more smooth and accurate with high quality depth, but it would also be more costly in performance.")]
	public OVRManager.DepthQuality depthQuality = OVRManager.DepthQuality.Medium;

	[HideInInspector]
	[Tooltip("Smooth factor in dynamic lighting. Larger is smoother")]
	public float dynamicLightingSmoothFactor = 8f;

	[HideInInspector]
	[Tooltip("The maximum depth variation across the edges. Make it smaller to smooth the lighting on the edges.")]
	public float dynamicLightingDepthVariationClampingValue = 0.001f;

	[HideInInspector]
	[Tooltip("Type of virutal green screen ")]
	public OVRManager.VirtualGreenScreenType virtualGreenScreenType;

	[HideInInspector]
	[Tooltip("Top Y of virtual green screen")]
	public float virtualGreenScreenTopY = 10f;

	[HideInInspector]
	[Tooltip("Bottom Y of virtual green screen")]
	public float virtualGreenScreenBottomY = -10f;

	[HideInInspector]
	[Tooltip("When using a depth camera (e.g. ZED), whether to use the depth in virtual green screen culling.")]
	public bool virtualGreenScreenApplyDepthCulling;

	[HideInInspector]
	[Tooltip("The tolerance value (in meter) when using the virtual green screen with a depth camera. Make it bigger if the foreground objects got culled incorrectly.")]
	public float virtualGreenScreenDepthTolerance = 0.2f;

	[Header("Tracking")]
	[SerializeField]
	[Tooltip("Defines the current tracking origin type.")]
	private OVRManager.TrackingOrigin _trackingOriginType;

	[Tooltip("If true, head tracking will affect the position of each OVRCameraRig's cameras.")]
	public bool usePositionTracking = true;

	[HideInInspector]
	public bool useRotationTracking = true;

	[Tooltip("If true, the distance between the user's eyes will affect the position of each OVRCameraRig's cameras.")]
	public bool useIPDInPositionTracking = true;

	[Tooltip("If true, each scene load will cause the head pose to reset.")]
	public bool resetTrackerOnLoad;

	[Tooltip("If true, the Reset View in the universal menu will cause the pose to be reset. This should generally be enabled for applications with a stationary position in the virtual world and will allow the View Reset command to place the person back to a predefined location (such as a cockpit seat). Set this to false if you have a locomotion system because resetting the view would effectively teleport the player to potentially invalid locations.")]
	public bool AllowRecenter = true;

	private static bool _isUserPresentCached = false;

	private static bool _isUserPresent = false;

	private static bool _wasUserPresent = false;

	private static bool prevAudioOutIdIsCached = false;

	private static bool prevAudioInIdIsCached = false;

	private static string prevAudioOutId = string.Empty;

	private static string prevAudioInId = string.Empty;

	private static bool wasPositionTracked = false;

	private static bool prevEnableMixedReality = false;

	private bool suppressDisableMixedRealityBecauseOfNoMainCameraWarning;

	private bool multipleMainCameraWarningPresented;

	public enum TrackingOrigin
	{
		EyeLevel,
		FloorLevel
	}

	public enum EyeTextureFormat
	{
		Default,
		R16G16B16A16_FP = 2,
		R11G11B10_FP
	}

	public enum TiledMultiResLevel
	{
		Off,
		LMSLow,
		LMSMedium,
		LMSHigh
	}

	public enum CompositionMethod
	{
		External,
		Direct,
		Sandwich
	}

	public enum CameraDevice
	{
		WebCamera0,
		WebCamera1,
		ZEDCamera
	}

	public enum DepthQuality
	{
		Low,
		Medium,
		High
	}

	public enum VirtualGreenScreenType
	{
		Off,
		OuterBoundary,
		PlayArea
	}
}
