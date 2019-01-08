using System;
using System.Runtime.InteropServices;
using UnityEngine;

internal static class OVRPlugin
{
	public static Version version
	{
		get
		{
			if (OVRPlugin._version == null)
			{
				try
				{
					string text = OVRPlugin.OVRP_1_1_0.ovrp_GetVersion();
					if (text != null)
					{
						text = text.Split(new char[]
						{
							'-'
						})[0];
						OVRPlugin._version = new Version(text);
					}
					else
					{
						OVRPlugin._version = OVRPlugin._versionZero;
					}
				}
				catch
				{
					OVRPlugin._version = OVRPlugin._versionZero;
				}
				if (OVRPlugin._version == OVRPlugin.OVRP_0_5_0.version)
				{
					OVRPlugin._version = OVRPlugin.OVRP_0_1_0.version;
				}
				if (OVRPlugin._version > OVRPlugin._versionZero && OVRPlugin._version < OVRPlugin.OVRP_1_3_0.version)
				{
					throw new PlatformNotSupportedException(string.Concat(new object[]
					{
						"Oculus Utilities version ",
						OVRPlugin.wrapperVersion,
						" is too new for OVRPlugin version ",
						OVRPlugin._version.ToString(),
						". Update to the latest version of Unity."
					}));
				}
			}
			return OVRPlugin._version;
		}
	}

	public static Version nativeSDKVersion
	{
		get
		{
			if (OVRPlugin._nativeSDKVersion == null)
			{
				try
				{
					string text = string.Empty;
					if (OVRPlugin.version >= OVRPlugin.OVRP_1_1_0.version)
					{
						text = OVRPlugin.OVRP_1_1_0.ovrp_GetNativeSDKVersion();
					}
					else
					{
						text = OVRPlugin._versionZero.ToString();
					}
					if (text != null)
					{
						text = text.Split(new char[]
						{
							'-'
						})[0];
						OVRPlugin._nativeSDKVersion = new Version(text);
					}
					else
					{
						OVRPlugin._nativeSDKVersion = OVRPlugin._versionZero;
					}
				}
				catch
				{
					OVRPlugin._nativeSDKVersion = OVRPlugin._versionZero;
				}
			}
			return OVRPlugin._nativeSDKVersion;
		}
	}

	public static bool initialized
	{
		get
		{
			return OVRPlugin.OVRP_1_1_0.ovrp_GetInitialized() == OVRPlugin.Bool.True;
		}
	}

	public static bool chromatic
	{
		get
		{
			return !(OVRPlugin.version >= OVRPlugin.OVRP_1_7_0.version) || OVRPlugin.OVRP_1_7_0.ovrp_GetAppChromaticCorrection() == OVRPlugin.Bool.True;
		}
		set
		{
			if (OVRPlugin.version >= OVRPlugin.OVRP_1_7_0.version)
			{
				OVRPlugin.OVRP_1_7_0.ovrp_SetAppChromaticCorrection(OVRPlugin.ToBool(value));
			}
		}
	}

	public static bool monoscopic
	{
		get
		{
			return OVRPlugin.OVRP_1_1_0.ovrp_GetAppMonoscopic() == OVRPlugin.Bool.True;
		}
		set
		{
			OVRPlugin.OVRP_1_1_0.ovrp_SetAppMonoscopic(OVRPlugin.ToBool(value));
		}
	}

	public static bool rotation
	{
		get
		{
			return OVRPlugin.OVRP_1_1_0.ovrp_GetTrackingOrientationEnabled() == OVRPlugin.Bool.True;
		}
		set
		{
			OVRPlugin.OVRP_1_1_0.ovrp_SetTrackingOrientationEnabled(OVRPlugin.ToBool(value));
		}
	}

	public static bool position
	{
		get
		{
			return OVRPlugin.OVRP_1_1_0.ovrp_GetTrackingPositionEnabled() == OVRPlugin.Bool.True;
		}
		set
		{
			OVRPlugin.OVRP_1_1_0.ovrp_SetTrackingPositionEnabled(OVRPlugin.ToBool(value));
		}
	}

	public static bool useIPDInPositionTracking
	{
		get
		{
			return !(OVRPlugin.version >= OVRPlugin.OVRP_1_6_0.version) || OVRPlugin.OVRP_1_6_0.ovrp_GetTrackingIPDEnabled() == OVRPlugin.Bool.True;
		}
		set
		{
			if (OVRPlugin.version >= OVRPlugin.OVRP_1_6_0.version)
			{
				OVRPlugin.OVRP_1_6_0.ovrp_SetTrackingIPDEnabled(OVRPlugin.ToBool(value));
			}
		}
	}

	public static bool positionSupported
	{
		get
		{
			return OVRPlugin.OVRP_1_1_0.ovrp_GetTrackingPositionSupported() == OVRPlugin.Bool.True;
		}
	}

	public static bool positionTracked
	{
		get
		{
			return OVRPlugin.OVRP_1_1_0.ovrp_GetNodePositionTracked(OVRPlugin.Node.EyeCenter) == OVRPlugin.Bool.True;
		}
	}

	public static bool powerSaving
	{
		get
		{
			return OVRPlugin.OVRP_1_1_0.ovrp_GetSystemPowerSavingMode() == OVRPlugin.Bool.True;
		}
	}

	public static bool hmdPresent
	{
		get
		{
			return OVRPlugin.OVRP_1_1_0.ovrp_GetNodePresent(OVRPlugin.Node.EyeCenter) == OVRPlugin.Bool.True;
		}
	}

	public static bool userPresent
	{
		get
		{
			return OVRPlugin.OVRP_1_1_0.ovrp_GetUserPresent() == OVRPlugin.Bool.True;
		}
	}

	public static bool headphonesPresent
	{
		get
		{
			return OVRPlugin.OVRP_1_3_0.ovrp_GetSystemHeadphonesPresent() == OVRPlugin.Bool.True;
		}
	}

	public static int recommendedMSAALevel
	{
		get
		{
			if (OVRPlugin.version >= OVRPlugin.OVRP_1_6_0.version)
			{
				return OVRPlugin.OVRP_1_6_0.ovrp_GetSystemRecommendedMSAALevel();
			}
			return 2;
		}
	}

	public static OVRPlugin.SystemRegion systemRegion
	{
		get
		{
			if (OVRPlugin.version >= OVRPlugin.OVRP_1_5_0.version)
			{
				return OVRPlugin.OVRP_1_5_0.ovrp_GetSystemRegion();
			}
			return OVRPlugin.SystemRegion.Unspecified;
		}
	}

	public static string audioOutId
	{
		get
		{
			try
			{
				if (OVRPlugin._nativeAudioOutGuid == null)
				{
					OVRPlugin._nativeAudioOutGuid = new OVRPlugin.GUID();
				}
				IntPtr intPtr = OVRPlugin.OVRP_1_1_0.ovrp_GetAudioOutId();
				if (intPtr != IntPtr.Zero)
				{
					Marshal.PtrToStructure(intPtr, OVRPlugin._nativeAudioOutGuid);
					Guid guid = new Guid(OVRPlugin._nativeAudioOutGuid.a, OVRPlugin._nativeAudioOutGuid.b, OVRPlugin._nativeAudioOutGuid.c, OVRPlugin._nativeAudioOutGuid.d0, OVRPlugin._nativeAudioOutGuid.d1, OVRPlugin._nativeAudioOutGuid.d2, OVRPlugin._nativeAudioOutGuid.d3, OVRPlugin._nativeAudioOutGuid.d4, OVRPlugin._nativeAudioOutGuid.d5, OVRPlugin._nativeAudioOutGuid.d6, OVRPlugin._nativeAudioOutGuid.d7);
					if (guid != OVRPlugin._cachedAudioOutGuid)
					{
						OVRPlugin._cachedAudioOutGuid = guid;
						OVRPlugin._cachedAudioOutString = OVRPlugin._cachedAudioOutGuid.ToString();
					}
					return OVRPlugin._cachedAudioOutString;
				}
			}
			catch
			{
			}
			return string.Empty;
		}
	}

	public static string audioInId
	{
		get
		{
			try
			{
				if (OVRPlugin._nativeAudioInGuid == null)
				{
					OVRPlugin._nativeAudioInGuid = new OVRPlugin.GUID();
				}
				IntPtr intPtr = OVRPlugin.OVRP_1_1_0.ovrp_GetAudioInId();
				if (intPtr != IntPtr.Zero)
				{
					Marshal.PtrToStructure(intPtr, OVRPlugin._nativeAudioInGuid);
					Guid guid = new Guid(OVRPlugin._nativeAudioInGuid.a, OVRPlugin._nativeAudioInGuid.b, OVRPlugin._nativeAudioInGuid.c, OVRPlugin._nativeAudioInGuid.d0, OVRPlugin._nativeAudioInGuid.d1, OVRPlugin._nativeAudioInGuid.d2, OVRPlugin._nativeAudioInGuid.d3, OVRPlugin._nativeAudioInGuid.d4, OVRPlugin._nativeAudioInGuid.d5, OVRPlugin._nativeAudioInGuid.d6, OVRPlugin._nativeAudioInGuid.d7);
					if (guid != OVRPlugin._cachedAudioInGuid)
					{
						OVRPlugin._cachedAudioInGuid = guid;
						OVRPlugin._cachedAudioInString = OVRPlugin._cachedAudioInGuid.ToString();
					}
					return OVRPlugin._cachedAudioInString;
				}
			}
			catch
			{
			}
			return string.Empty;
		}
	}

	public static bool hasVrFocus
	{
		get
		{
			return OVRPlugin.OVRP_1_1_0.ovrp_GetAppHasVrFocus() == OVRPlugin.Bool.True;
		}
	}

	public static bool hasInputFocus
	{
		get
		{
			if (!(OVRPlugin.version >= OVRPlugin.OVRP_1_18_0.version))
			{
				return true;
			}
			OVRPlugin.Bool @bool = OVRPlugin.Bool.False;
			OVRPlugin.Result result = OVRPlugin.OVRP_1_18_0.ovrp_GetAppHasInputFocus(out @bool);
			if (result == OVRPlugin.Result.Success)
			{
				return @bool == OVRPlugin.Bool.True;
			}
			Debug.LogWarning("ovrp_GetAppHasInputFocus return " + result);
			return false;
		}
	}

	public static bool shouldQuit
	{
		get
		{
			return OVRPlugin.OVRP_1_1_0.ovrp_GetAppShouldQuit() == OVRPlugin.Bool.True;
		}
	}

	public static bool shouldRecenter
	{
		get
		{
			return OVRPlugin.OVRP_1_1_0.ovrp_GetAppShouldRecenter() == OVRPlugin.Bool.True;
		}
	}

	public static string productName
	{
		get
		{
			return OVRPlugin.OVRP_1_1_0.ovrp_GetSystemProductName();
		}
	}

	public static string latency
	{
		get
		{
			return OVRPlugin.OVRP_1_1_0.ovrp_GetAppLatencyTimings();
		}
	}

	public static float eyeDepth
	{
		get
		{
			return OVRPlugin.OVRP_1_1_0.ovrp_GetUserEyeDepth();
		}
		set
		{
			OVRPlugin.OVRP_1_1_0.ovrp_SetUserEyeDepth(value);
		}
	}

	public static float eyeHeight
	{
		get
		{
			return OVRPlugin.OVRP_1_1_0.ovrp_GetUserEyeHeight();
		}
		set
		{
			OVRPlugin.OVRP_1_1_0.ovrp_SetUserEyeHeight(value);
		}
	}

	public static float batteryLevel
	{
		get
		{
			return OVRPlugin.OVRP_1_1_0.ovrp_GetSystemBatteryLevel();
		}
	}

	public static float batteryTemperature
	{
		get
		{
			return OVRPlugin.OVRP_1_1_0.ovrp_GetSystemBatteryTemperature();
		}
	}

	public static int cpuLevel
	{
		get
		{
			return OVRPlugin.OVRP_1_1_0.ovrp_GetSystemCpuLevel();
		}
		set
		{
			OVRPlugin.OVRP_1_1_0.ovrp_SetSystemCpuLevel(value);
		}
	}

	public static int gpuLevel
	{
		get
		{
			return OVRPlugin.OVRP_1_1_0.ovrp_GetSystemGpuLevel();
		}
		set
		{
			OVRPlugin.OVRP_1_1_0.ovrp_SetSystemGpuLevel(value);
		}
	}

	public static int vsyncCount
	{
		get
		{
			return OVRPlugin.OVRP_1_1_0.ovrp_GetSystemVSyncCount();
		}
		set
		{
			OVRPlugin.OVRP_1_2_0.ovrp_SetSystemVSyncCount(value);
		}
	}

	public static float systemVolume
	{
		get
		{
			return OVRPlugin.OVRP_1_1_0.ovrp_GetSystemVolume();
		}
	}

	public static float ipd
	{
		get
		{
			return OVRPlugin.OVRP_1_1_0.ovrp_GetUserIPD();
		}
		set
		{
			OVRPlugin.OVRP_1_1_0.ovrp_SetUserIPD(value);
		}
	}

	public static bool occlusionMesh
	{
		get
		{
			return OVRPlugin.OVRP_1_3_0.ovrp_GetEyeOcclusionMeshEnabled() == OVRPlugin.Bool.True;
		}
		set
		{
			OVRPlugin.OVRP_1_3_0.ovrp_SetEyeOcclusionMeshEnabled(OVRPlugin.ToBool(value));
		}
	}

	public static OVRPlugin.BatteryStatus batteryStatus
	{
		get
		{
			return OVRPlugin.OVRP_1_1_0.ovrp_GetSystemBatteryStatus();
		}
	}

	public static OVRPlugin.Frustumf GetEyeFrustum(OVRPlugin.Eye eyeId)
	{
		return OVRPlugin.OVRP_1_1_0.ovrp_GetNodeFrustum((OVRPlugin.Node)eyeId);
	}

	public static OVRPlugin.Sizei GetEyeTextureSize(OVRPlugin.Eye eyeId)
	{
		return OVRPlugin.OVRP_0_1_0.ovrp_GetEyeTextureSize(eyeId);
	}

	public static OVRPlugin.Posef GetTrackerPose(OVRPlugin.Tracker trackerId)
	{
		return OVRPlugin.GetNodePose((OVRPlugin.Node)(trackerId + 5), OVRPlugin.Step.Render);
	}

	public static OVRPlugin.Frustumf GetTrackerFrustum(OVRPlugin.Tracker trackerId)
	{
		return OVRPlugin.OVRP_1_1_0.ovrp_GetNodeFrustum((OVRPlugin.Node)(trackerId + 5));
	}

	public static bool ShowUI(OVRPlugin.PlatformUI ui)
	{
		return OVRPlugin.OVRP_1_1_0.ovrp_ShowSystemUI(ui) == OVRPlugin.Bool.True;
	}

	public static bool EnqueueSubmitLayer(bool onTop, bool headLocked, IntPtr leftTexture, IntPtr rightTexture, int layerId, int frameIndex, OVRPlugin.Posef pose, OVRPlugin.Vector3f scale, int layerIndex = 0, OVRPlugin.OverlayShape shape = OVRPlugin.OverlayShape.Quad)
	{
		if (!(OVRPlugin.version >= OVRPlugin.OVRP_1_6_0.version))
		{
			return layerIndex == 0 && OVRPlugin.OVRP_0_1_1.ovrp_SetOverlayQuad2(OVRPlugin.ToBool(onTop), OVRPlugin.ToBool(headLocked), leftTexture, IntPtr.Zero, pose, scale) == OVRPlugin.Bool.True;
		}
		uint num = 0u;
		if (onTop)
		{
			num |= 1u;
		}
		if (headLocked)
		{
			num |= 2u;
		}
		if (shape == OVRPlugin.OverlayShape.Cylinder || shape == OVRPlugin.OverlayShape.Cubemap)
		{
			if (shape == OVRPlugin.OverlayShape.Cubemap && OVRPlugin.version >= OVRPlugin.OVRP_1_10_0.version)
			{
				num |= (uint)((uint)shape << 4);
			}
			else
			{
				if (shape != OVRPlugin.OverlayShape.Cylinder || !(OVRPlugin.version >= OVRPlugin.OVRP_1_16_0.version))
				{
					return false;
				}
				num |= (uint)((uint)shape << 4);
			}
		}
		if (shape == OVRPlugin.OverlayShape.OffcenterCubemap)
		{
			return false;
		}
		if (shape == OVRPlugin.OverlayShape.Equirect)
		{
			return false;
		}
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_15_0.version && layerId != -1)
		{
			return OVRPlugin.OVRP_1_15_0.ovrp_EnqueueSubmitLayer(num, leftTexture, rightTexture, layerId, frameIndex, ref pose, ref scale, layerIndex) == OVRPlugin.Result.Success;
		}
		return OVRPlugin.OVRP_1_6_0.ovrp_SetOverlayQuad3(num, leftTexture, rightTexture, IntPtr.Zero, pose, scale, layerIndex) == OVRPlugin.Bool.True;
	}

	public static OVRPlugin.LayerDesc CalculateLayerDesc(OVRPlugin.OverlayShape shape, OVRPlugin.LayerLayout layout, OVRPlugin.Sizei textureSize, int mipLevels, int sampleCount, OVRPlugin.EyeTextureFormat format, int layerFlags)
	{
		OVRPlugin.LayerDesc result = default(OVRPlugin.LayerDesc);
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_15_0.version)
		{
			OVRPlugin.OVRP_1_15_0.ovrp_CalculateLayerDesc(shape, layout, ref textureSize, mipLevels, sampleCount, format, layerFlags, ref result);
		}
		return result;
	}

	public static bool EnqueueSetupLayer(OVRPlugin.LayerDesc desc, IntPtr layerID)
	{
		return OVRPlugin.version >= OVRPlugin.OVRP_1_15_0.version && OVRPlugin.OVRP_1_15_0.ovrp_EnqueueSetupLayer(ref desc, layerID) == OVRPlugin.Result.Success;
	}

	public static bool EnqueueDestroyLayer(IntPtr layerID)
	{
		return OVRPlugin.version >= OVRPlugin.OVRP_1_15_0.version && OVRPlugin.OVRP_1_15_0.ovrp_EnqueueDestroyLayer(layerID) == OVRPlugin.Result.Success;
	}

	public static IntPtr GetLayerTexture(int layerId, int stage, OVRPlugin.Eye eyeId)
	{
		IntPtr zero = IntPtr.Zero;
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_15_0.version)
		{
			OVRPlugin.OVRP_1_15_0.ovrp_GetLayerTexturePtr(layerId, stage, eyeId, ref zero);
		}
		return zero;
	}

	public static int GetLayerTextureStageCount(int layerId)
	{
		int result = 1;
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_15_0.version)
		{
			OVRPlugin.OVRP_1_15_0.ovrp_GetLayerTextureStageCount(layerId, ref result);
		}
		return result;
	}

	public static bool UpdateNodePhysicsPoses(int frameIndex, double predictionSeconds)
	{
		return OVRPlugin.version >= OVRPlugin.OVRP_1_8_0.version && OVRPlugin.OVRP_1_8_0.ovrp_Update2(0, frameIndex, predictionSeconds) == OVRPlugin.Bool.True;
	}

	public static OVRPlugin.Posef GetNodePose(OVRPlugin.Node nodeId, OVRPlugin.Step stepId)
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_12_0.version)
		{
			return OVRPlugin.OVRP_1_12_0.ovrp_GetNodePoseState(stepId, nodeId).Pose;
		}
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_8_0.version && stepId == OVRPlugin.Step.Physics)
		{
			return OVRPlugin.OVRP_1_8_0.ovrp_GetNodePose2(0, nodeId);
		}
		return OVRPlugin.OVRP_0_1_2.ovrp_GetNodePose(nodeId);
	}

	public static OVRPlugin.Vector3f GetNodeVelocity(OVRPlugin.Node nodeId, OVRPlugin.Step stepId)
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_12_0.version)
		{
			return OVRPlugin.OVRP_1_12_0.ovrp_GetNodePoseState(stepId, nodeId).Velocity;
		}
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_8_0.version && stepId == OVRPlugin.Step.Physics)
		{
			return OVRPlugin.OVRP_1_8_0.ovrp_GetNodeVelocity2(0, nodeId).Position;
		}
		return OVRPlugin.OVRP_0_1_3.ovrp_GetNodeVelocity(nodeId).Position;
	}

	public static OVRPlugin.Vector3f GetNodeAngularVelocity(OVRPlugin.Node nodeId, OVRPlugin.Step stepId)
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_12_0.version)
		{
			return OVRPlugin.OVRP_1_12_0.ovrp_GetNodePoseState(stepId, nodeId).AngularVelocity;
		}
		return default(OVRPlugin.Vector3f);
	}

	public static OVRPlugin.Vector3f GetNodeAcceleration(OVRPlugin.Node nodeId, OVRPlugin.Step stepId)
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_12_0.version)
		{
			return OVRPlugin.OVRP_1_12_0.ovrp_GetNodePoseState(stepId, nodeId).Acceleration;
		}
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_8_0.version && stepId == OVRPlugin.Step.Physics)
		{
			return OVRPlugin.OVRP_1_8_0.ovrp_GetNodeAcceleration2(0, nodeId).Position;
		}
		return OVRPlugin.OVRP_0_1_3.ovrp_GetNodeAcceleration(nodeId).Position;
	}

	public static OVRPlugin.Vector3f GetNodeAngularAcceleration(OVRPlugin.Node nodeId, OVRPlugin.Step stepId)
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_12_0.version)
		{
			return OVRPlugin.OVRP_1_12_0.ovrp_GetNodePoseState(stepId, nodeId).AngularAcceleration;
		}
		return default(OVRPlugin.Vector3f);
	}

	public static bool GetNodePresent(OVRPlugin.Node nodeId)
	{
		return OVRPlugin.OVRP_1_1_0.ovrp_GetNodePresent(nodeId) == OVRPlugin.Bool.True;
	}

	public static bool GetNodeOrientationTracked(OVRPlugin.Node nodeId)
	{
		return OVRPlugin.OVRP_1_1_0.ovrp_GetNodeOrientationTracked(nodeId) == OVRPlugin.Bool.True;
	}

	public static bool GetNodePositionTracked(OVRPlugin.Node nodeId)
	{
		return OVRPlugin.OVRP_1_1_0.ovrp_GetNodePositionTracked(nodeId) == OVRPlugin.Bool.True;
	}

	public static OVRPlugin.ControllerState GetControllerState(uint controllerMask)
	{
		return OVRPlugin.OVRP_1_1_0.ovrp_GetControllerState(controllerMask);
	}

	public static OVRPlugin.ControllerState2 GetControllerState2(uint controllerMask)
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_12_0.version)
		{
			return OVRPlugin.OVRP_1_12_0.ovrp_GetControllerState2(controllerMask);
		}
		return new OVRPlugin.ControllerState2(OVRPlugin.OVRP_1_1_0.ovrp_GetControllerState(controllerMask));
	}

	public static OVRPlugin.ControllerState4 GetControllerState4(uint controllerMask)
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_16_0.version)
		{
			OVRPlugin.ControllerState4 result = default(OVRPlugin.ControllerState4);
			OVRPlugin.OVRP_1_16_0.ovrp_GetControllerState4(controllerMask, ref result);
			return result;
		}
		return new OVRPlugin.ControllerState4(OVRPlugin.GetControllerState2(controllerMask));
	}

	public static bool SetControllerVibration(uint controllerMask, float frequency, float amplitude)
	{
		return OVRPlugin.OVRP_0_1_2.ovrp_SetControllerVibration(controllerMask, frequency, amplitude) == OVRPlugin.Bool.True;
	}

	public static OVRPlugin.HapticsDesc GetControllerHapticsDesc(uint controllerMask)
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_6_0.version)
		{
			return OVRPlugin.OVRP_1_6_0.ovrp_GetControllerHapticsDesc(controllerMask);
		}
		return default(OVRPlugin.HapticsDesc);
	}

	public static OVRPlugin.HapticsState GetControllerHapticsState(uint controllerMask)
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_6_0.version)
		{
			return OVRPlugin.OVRP_1_6_0.ovrp_GetControllerHapticsState(controllerMask);
		}
		return default(OVRPlugin.HapticsState);
	}

	public static bool SetControllerHaptics(uint controllerMask, OVRPlugin.HapticsBuffer hapticsBuffer)
	{
		return OVRPlugin.version >= OVRPlugin.OVRP_1_6_0.version && OVRPlugin.OVRP_1_6_0.ovrp_SetControllerHaptics(controllerMask, hapticsBuffer) == OVRPlugin.Bool.True;
	}

	public static float GetEyeRecommendedResolutionScale()
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_6_0.version)
		{
			return OVRPlugin.OVRP_1_6_0.ovrp_GetEyeRecommendedResolutionScale();
		}
		return 1f;
	}

	public static float GetAppCpuStartToGpuEndTime()
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_6_0.version)
		{
			return OVRPlugin.OVRP_1_6_0.ovrp_GetAppCpuStartToGpuEndTime();
		}
		return 0f;
	}

	public static bool GetBoundaryConfigured()
	{
		return OVRPlugin.version >= OVRPlugin.OVRP_1_8_0.version && OVRPlugin.OVRP_1_8_0.ovrp_GetBoundaryConfigured() == OVRPlugin.Bool.True;
	}

	public static OVRPlugin.BoundaryTestResult TestBoundaryNode(OVRPlugin.Node nodeId, OVRPlugin.BoundaryType boundaryType)
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_8_0.version)
		{
			return OVRPlugin.OVRP_1_8_0.ovrp_TestBoundaryNode(nodeId, boundaryType);
		}
		return default(OVRPlugin.BoundaryTestResult);
	}

	public static OVRPlugin.BoundaryTestResult TestBoundaryPoint(OVRPlugin.Vector3f point, OVRPlugin.BoundaryType boundaryType)
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_8_0.version)
		{
			return OVRPlugin.OVRP_1_8_0.ovrp_TestBoundaryPoint(point, boundaryType);
		}
		return default(OVRPlugin.BoundaryTestResult);
	}

	public static bool SetBoundaryLookAndFeel(OVRPlugin.BoundaryLookAndFeel lookAndFeel)
	{
		return OVRPlugin.version >= OVRPlugin.OVRP_1_8_0.version && OVRPlugin.OVRP_1_8_0.ovrp_SetBoundaryLookAndFeel(lookAndFeel) == OVRPlugin.Bool.True;
	}

	public static bool ResetBoundaryLookAndFeel()
	{
		return OVRPlugin.version >= OVRPlugin.OVRP_1_8_0.version && OVRPlugin.OVRP_1_8_0.ovrp_ResetBoundaryLookAndFeel() == OVRPlugin.Bool.True;
	}

	public static OVRPlugin.BoundaryGeometry GetBoundaryGeometry(OVRPlugin.BoundaryType boundaryType)
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_8_0.version)
		{
			return OVRPlugin.OVRP_1_8_0.ovrp_GetBoundaryGeometry(boundaryType);
		}
		return default(OVRPlugin.BoundaryGeometry);
	}

	public static bool GetBoundaryGeometry2(OVRPlugin.BoundaryType boundaryType, IntPtr points, ref int pointsCount)
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_9_0.version)
		{
			return OVRPlugin.OVRP_1_9_0.ovrp_GetBoundaryGeometry2(boundaryType, points, ref pointsCount) == OVRPlugin.Bool.True;
		}
		pointsCount = 0;
		return false;
	}

	public static OVRPlugin.AppPerfStats GetAppPerfStats()
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_9_0.version)
		{
			return OVRPlugin.OVRP_1_9_0.ovrp_GetAppPerfStats();
		}
		return default(OVRPlugin.AppPerfStats);
	}

	public static bool ResetAppPerfStats()
	{
		return OVRPlugin.version >= OVRPlugin.OVRP_1_9_0.version && OVRPlugin.OVRP_1_9_0.ovrp_ResetAppPerfStats() == OVRPlugin.Bool.True;
	}

	public static float GetAppFramerate()
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_12_0.version)
		{
			return OVRPlugin.OVRP_1_12_0.ovrp_GetAppFramerate();
		}
		return 0f;
	}

	public static bool SetHandNodePoseStateLatency(double latencyInSeconds)
	{
		if (!(OVRPlugin.version >= OVRPlugin.OVRP_1_18_0.version))
		{
			return false;
		}
		OVRPlugin.Result result = OVRPlugin.OVRP_1_18_0.ovrp_SetHandNodePoseStateLatency(latencyInSeconds);
		if (result == OVRPlugin.Result.Success)
		{
			return true;
		}
		Debug.LogWarning("ovrp_SetHandNodePoseStateLatency return " + result);
		return false;
	}

	public static double GetHandNodePoseStateLatency()
	{
		if (!(OVRPlugin.version >= OVRPlugin.OVRP_1_18_0.version))
		{
			return 0.0;
		}
		double result = 0.0;
		if (OVRPlugin.OVRP_1_18_0.ovrp_GetHandNodePoseStateLatency(out result) == OVRPlugin.Result.Success)
		{
			return result;
		}
		return 0.0;
	}

	public static OVRPlugin.EyeTextureFormat GetDesiredEyeTextureFormat()
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_11_0.version)
		{
			uint num = (uint)OVRPlugin.OVRP_1_11_0.ovrp_GetDesiredEyeTextureFormat();
			if (num == 1u)
			{
				num = 0u;
			}
			return (OVRPlugin.EyeTextureFormat)num;
		}
		return OVRPlugin.EyeTextureFormat.Default;
	}

	public static bool SetDesiredEyeTextureFormat(OVRPlugin.EyeTextureFormat value)
	{
		return OVRPlugin.version >= OVRPlugin.OVRP_1_11_0.version && OVRPlugin.OVRP_1_11_0.ovrp_SetDesiredEyeTextureFormat(value) == OVRPlugin.Bool.True;
	}

	public static bool InitializeMixedReality()
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_15_0.version)
		{
			OVRPlugin.Result result = OVRPlugin.OVRP_1_15_0.ovrp_InitializeMixedReality();
			if (result != OVRPlugin.Result.Success)
			{
				Debug.LogWarning("ovrp_InitializeMixedReality return " + result);
			}
			return result == OVRPlugin.Result.Success;
		}
		return false;
	}

	public static bool ShutdownMixedReality()
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_15_0.version)
		{
			OVRPlugin.Result result = OVRPlugin.OVRP_1_15_0.ovrp_ShutdownMixedReality();
			if (result != OVRPlugin.Result.Success)
			{
				Debug.LogWarning("ovrp_ShutdownMixedReality return " + result);
			}
			return result == OVRPlugin.Result.Success;
		}
		return false;
	}

	public static bool IsMixedRealityInitialized()
	{
		return OVRPlugin.version >= OVRPlugin.OVRP_1_15_0.version && OVRPlugin.OVRP_1_15_0.ovrp_GetMixedRealityInitialized() == OVRPlugin.Bool.True;
	}

	public static int GetExternalCameraCount()
	{
		if (!(OVRPlugin.version >= OVRPlugin.OVRP_1_15_0.version))
		{
			return 0;
		}
		int result = 0;
		OVRPlugin.Result result2 = OVRPlugin.OVRP_1_15_0.ovrp_GetExternalCameraCount(out result);
		if (result2 != OVRPlugin.Result.Success)
		{
			Debug.LogWarning("ovrp_GetExternalCameraCount return " + result2);
			return 0;
		}
		return result;
	}

	public static bool UpdateExternalCamera()
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_15_0.version)
		{
			OVRPlugin.Result result = OVRPlugin.OVRP_1_15_0.ovrp_UpdateExternalCamera();
			if (result != OVRPlugin.Result.Success)
			{
				Debug.LogWarning("ovrp_UpdateExternalCamera return " + result);
			}
			return result == OVRPlugin.Result.Success;
		}
		return false;
	}

	public static bool GetMixedRealityCameraInfo(int cameraId, out OVRPlugin.CameraExtrinsics cameraExtrinsics, out OVRPlugin.CameraIntrinsics cameraIntrinsics)
	{
		cameraExtrinsics = default(OVRPlugin.CameraExtrinsics);
		cameraIntrinsics = default(OVRPlugin.CameraIntrinsics);
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_15_0.version)
		{
			bool result = true;
			OVRPlugin.Result result2 = OVRPlugin.OVRP_1_15_0.ovrp_GetExternalCameraExtrinsics(cameraId, out cameraExtrinsics);
			if (result2 != OVRPlugin.Result.Success)
			{
				result = false;
				Debug.LogWarning("ovrp_GetExternalCameraExtrinsics return " + result2);
			}
			result2 = OVRPlugin.OVRP_1_15_0.ovrp_GetExternalCameraIntrinsics(cameraId, out cameraIntrinsics);
			if (result2 != OVRPlugin.Result.Success)
			{
				result = false;
				Debug.LogWarning("ovrp_GetExternalCameraIntrinsics return " + result2);
			}
			return result;
		}
		return false;
	}

	public static OVRPlugin.Vector3f GetBoundaryDimensions(OVRPlugin.BoundaryType boundaryType)
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_8_0.version)
		{
			return OVRPlugin.OVRP_1_8_0.ovrp_GetBoundaryDimensions(boundaryType);
		}
		return default(OVRPlugin.Vector3f);
	}

	public static bool GetBoundaryVisible()
	{
		return OVRPlugin.version >= OVRPlugin.OVRP_1_8_0.version && OVRPlugin.OVRP_1_8_0.ovrp_GetBoundaryVisible() == OVRPlugin.Bool.True;
	}

	public static bool SetBoundaryVisible(bool value)
	{
		return OVRPlugin.version >= OVRPlugin.OVRP_1_8_0.version && OVRPlugin.OVRP_1_8_0.ovrp_SetBoundaryVisible(OVRPlugin.ToBool(value)) == OVRPlugin.Bool.True;
	}

	public static OVRPlugin.SystemHeadset GetSystemHeadsetType()
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_9_0.version)
		{
			return OVRPlugin.OVRP_1_9_0.ovrp_GetSystemHeadsetType();
		}
		return OVRPlugin.SystemHeadset.None;
	}

	public static OVRPlugin.Controller GetActiveController()
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_9_0.version)
		{
			return OVRPlugin.OVRP_1_9_0.ovrp_GetActiveController();
		}
		return OVRPlugin.Controller.None;
	}

	public static OVRPlugin.Controller GetConnectedControllers()
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_9_0.version)
		{
			return OVRPlugin.OVRP_1_9_0.ovrp_GetConnectedControllers();
		}
		return OVRPlugin.Controller.None;
	}

	private static OVRPlugin.Bool ToBool(bool b)
	{
		return (!b) ? OVRPlugin.Bool.False : OVRPlugin.Bool.True;
	}

	public static OVRPlugin.TrackingOrigin GetTrackingOriginType()
	{
		return OVRPlugin.OVRP_1_0_0.ovrp_GetTrackingOriginType();
	}

	public static bool SetTrackingOriginType(OVRPlugin.TrackingOrigin originType)
	{
		return OVRPlugin.OVRP_1_0_0.ovrp_SetTrackingOriginType(originType) == OVRPlugin.Bool.True;
	}

	public static OVRPlugin.Posef GetTrackingCalibratedOrigin()
	{
		return OVRPlugin.OVRP_1_0_0.ovrp_GetTrackingCalibratedOrigin();
	}

	public static bool SetTrackingCalibratedOrigin()
	{
		return OVRPlugin.OVRP_1_2_0.ovrpi_SetTrackingCalibratedOrigin() == OVRPlugin.Bool.True;
	}

	public static bool RecenterTrackingOrigin(OVRPlugin.RecenterFlags flags)
	{
		return OVRPlugin.OVRP_1_0_0.ovrp_RecenterTrackingOrigin((uint)flags) == OVRPlugin.Bool.True;
	}

	public static bool UpdateCameraDevices()
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_16_0.version)
		{
			OVRPlugin.Result result = OVRPlugin.OVRP_1_16_0.ovrp_UpdateCameraDevices();
			if (result != OVRPlugin.Result.Success)
			{
				Debug.LogWarning("ovrp_UpdateCameraDevices return " + result);
			}
			return result == OVRPlugin.Result.Success;
		}
		return false;
	}

	public static bool IsCameraDeviceAvailable(OVRPlugin.CameraDevice cameraDevice)
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_16_0.version)
		{
			OVRPlugin.Bool @bool = OVRPlugin.OVRP_1_16_0.ovrp_IsCameraDeviceAvailable(cameraDevice);
			return @bool == OVRPlugin.Bool.True;
		}
		return false;
	}

	public static bool SetCameraDevicePreferredColorFrameSize(OVRPlugin.CameraDevice cameraDevice, int width, int height)
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_16_0.version)
		{
			OVRPlugin.Result result = OVRPlugin.OVRP_1_16_0.ovrp_SetCameraDevicePreferredColorFrameSize(cameraDevice, new OVRPlugin.Sizei
			{
				w = width,
				h = height
			});
			if (result != OVRPlugin.Result.Success)
			{
				Debug.LogWarning("ovrp_SetCameraDevicePreferredColorFrameSize return " + result);
			}
			return result == OVRPlugin.Result.Success;
		}
		return false;
	}

	public static bool OpenCameraDevice(OVRPlugin.CameraDevice cameraDevice)
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_16_0.version)
		{
			OVRPlugin.Result result = OVRPlugin.OVRP_1_16_0.ovrp_OpenCameraDevice(cameraDevice);
			if (result != OVRPlugin.Result.Success)
			{
				Debug.LogWarning("ovrp_OpenCameraDevice return " + result);
			}
			return result == OVRPlugin.Result.Success;
		}
		return false;
	}

	public static bool CloseCameraDevice(OVRPlugin.CameraDevice cameraDevice)
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_16_0.version)
		{
			OVRPlugin.Result result = OVRPlugin.OVRP_1_16_0.ovrp_CloseCameraDevice(cameraDevice);
			if (result != OVRPlugin.Result.Success)
			{
				Debug.LogWarning("ovrp_OpenCameraDevice return " + result);
			}
			return result == OVRPlugin.Result.Success;
		}
		return false;
	}

	public static bool HasCameraDeviceOpened(OVRPlugin.CameraDevice cameraDevice)
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_16_0.version)
		{
			OVRPlugin.Bool @bool = OVRPlugin.OVRP_1_16_0.ovrp_HasCameraDeviceOpened(cameraDevice);
			return @bool == OVRPlugin.Bool.True;
		}
		return false;
	}

	public static bool IsCameraDeviceColorFrameAvailable(OVRPlugin.CameraDevice cameraDevice)
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_16_0.version)
		{
			OVRPlugin.Bool @bool = OVRPlugin.OVRP_1_16_0.ovrp_IsCameraDeviceColorFrameAvailable(cameraDevice);
			return @bool == OVRPlugin.Bool.True;
		}
		return false;
	}

	public static Texture2D GetCameraDeviceColorFrameTexture(OVRPlugin.CameraDevice cameraDevice)
	{
		if (!(OVRPlugin.version >= OVRPlugin.OVRP_1_16_0.version))
		{
			return null;
		}
		OVRPlugin.Sizei sizei = default(OVRPlugin.Sizei);
		OVRPlugin.Result result = OVRPlugin.OVRP_1_16_0.ovrp_GetCameraDeviceColorFrameSize(cameraDevice, out sizei);
		if (result != OVRPlugin.Result.Success)
		{
			Debug.LogWarning("ovrp_GetCameraDeviceColorFrameSize return " + result);
			return null;
		}
		IntPtr data;
		int num;
		result = OVRPlugin.OVRP_1_16_0.ovrp_GetCameraDeviceColorFrameBgraPixels(cameraDevice, out data, out num);
		if (result != OVRPlugin.Result.Success)
		{
			Debug.LogWarning("ovrp_GetCameraDeviceColorFrameBgraPixels return " + result);
			return null;
		}
		if (num != sizei.w * 4)
		{
			Debug.LogWarning(string.Format("RowPitch mismatch, expected {0}, get {1}", sizei.w * 4, num));
			return null;
		}
		if (!OVRPlugin.cachedCameraFrameTexture || OVRPlugin.cachedCameraFrameTexture.width != sizei.w || OVRPlugin.cachedCameraFrameTexture.height != sizei.h)
		{
			OVRPlugin.cachedCameraFrameTexture = new Texture2D(sizei.w, sizei.h, TextureFormat.BGRA32, false);
		}
		OVRPlugin.cachedCameraFrameTexture.LoadRawTextureData(data, num * sizei.h);
		OVRPlugin.cachedCameraFrameTexture.Apply();
		return OVRPlugin.cachedCameraFrameTexture;
	}

	public static bool DoesCameraDeviceSupportDepth(OVRPlugin.CameraDevice cameraDevice)
	{
		OVRPlugin.Bool @bool;
		return OVRPlugin.version >= OVRPlugin.OVRP_1_17_0.version && OVRPlugin.OVRP_1_17_0.ovrp_DoesCameraDeviceSupportDepth(cameraDevice, out @bool) == OVRPlugin.Result.Success && @bool == OVRPlugin.Bool.True;
	}

	public static bool SetCameraDeviceDepthSensingMode(OVRPlugin.CameraDevice camera, OVRPlugin.CameraDeviceDepthSensingMode depthSensoringMode)
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_17_0.version)
		{
			OVRPlugin.Result result = OVRPlugin.OVRP_1_17_0.ovrp_SetCameraDeviceDepthSensingMode(camera, depthSensoringMode);
			return result == OVRPlugin.Result.Success;
		}
		return false;
	}

	public static bool SetCameraDevicePreferredDepthQuality(OVRPlugin.CameraDevice camera, OVRPlugin.CameraDeviceDepthQuality depthQuality)
	{
		if (OVRPlugin.version >= OVRPlugin.OVRP_1_17_0.version)
		{
			OVRPlugin.Result result = OVRPlugin.OVRP_1_17_0.ovrp_SetCameraDevicePreferredDepthQuality(camera, depthQuality);
			return result == OVRPlugin.Result.Success;
		}
		return false;
	}

	public static bool IsCameraDeviceDepthFrameAvailable(OVRPlugin.CameraDevice cameraDevice)
	{
		OVRPlugin.Bool @bool;
		return OVRPlugin.version >= OVRPlugin.OVRP_1_17_0.version && OVRPlugin.OVRP_1_17_0.ovrp_IsCameraDeviceDepthFrameAvailable(cameraDevice, out @bool) == OVRPlugin.Result.Success && @bool == OVRPlugin.Bool.True;
	}

	public static Texture2D GetCameraDeviceDepthFrameTexture(OVRPlugin.CameraDevice cameraDevice)
	{
		if (!(OVRPlugin.version >= OVRPlugin.OVRP_1_17_0.version))
		{
			return null;
		}
		OVRPlugin.Sizei sizei = default(OVRPlugin.Sizei);
		OVRPlugin.Result result = OVRPlugin.OVRP_1_17_0.ovrp_GetCameraDeviceDepthFrameSize(cameraDevice, out sizei);
		if (result != OVRPlugin.Result.Success)
		{
			Debug.LogWarning("ovrp_GetCameraDeviceDepthFrameSize return " + result);
			return null;
		}
		IntPtr data;
		int num;
		result = OVRPlugin.OVRP_1_17_0.ovrp_GetCameraDeviceDepthFramePixels(cameraDevice, out data, out num);
		if (result != OVRPlugin.Result.Success)
		{
			Debug.LogWarning("ovrp_GetCameraDeviceDepthFramePixels return " + result);
			return null;
		}
		if (num != sizei.w * 4)
		{
			Debug.LogWarning(string.Format("RowPitch mismatch, expected {0}, get {1}", sizei.w * 4, num));
			return null;
		}
		if (!OVRPlugin.cachedCameraDepthTexture || OVRPlugin.cachedCameraDepthTexture.width != sizei.w || OVRPlugin.cachedCameraDepthTexture.height != sizei.h)
		{
			OVRPlugin.cachedCameraDepthTexture = new Texture2D(sizei.w, sizei.h, TextureFormat.RFloat, false);
			OVRPlugin.cachedCameraDepthTexture.filterMode = FilterMode.Point;
		}
		OVRPlugin.cachedCameraDepthTexture.LoadRawTextureData(data, num * sizei.h);
		OVRPlugin.cachedCameraDepthTexture.Apply();
		return OVRPlugin.cachedCameraDepthTexture;
	}

	public static Texture2D GetCameraDeviceDepthConfidenceTexture(OVRPlugin.CameraDevice cameraDevice)
	{
		if (!(OVRPlugin.version >= OVRPlugin.OVRP_1_17_0.version))
		{
			return null;
		}
		OVRPlugin.Sizei sizei = default(OVRPlugin.Sizei);
		OVRPlugin.Result result = OVRPlugin.OVRP_1_17_0.ovrp_GetCameraDeviceDepthFrameSize(cameraDevice, out sizei);
		if (result != OVRPlugin.Result.Success)
		{
			Debug.LogWarning("ovrp_GetCameraDeviceDepthFrameSize return " + result);
			return null;
		}
		IntPtr data;
		int num;
		result = OVRPlugin.OVRP_1_17_0.ovrp_GetCameraDeviceDepthConfidencePixels(cameraDevice, out data, out num);
		if (result != OVRPlugin.Result.Success)
		{
			Debug.LogWarning("ovrp_GetCameraDeviceDepthConfidencePixels return " + result);
			return null;
		}
		if (num != sizei.w * 4)
		{
			Debug.LogWarning(string.Format("RowPitch mismatch, expected {0}, get {1}", sizei.w * 4, num));
			return null;
		}
		if (!OVRPlugin.cachedCameraDepthConfidenceTexture || OVRPlugin.cachedCameraDepthConfidenceTexture.width != sizei.w || OVRPlugin.cachedCameraDepthConfidenceTexture.height != sizei.h)
		{
			OVRPlugin.cachedCameraDepthConfidenceTexture = new Texture2D(sizei.w, sizei.h, TextureFormat.RFloat, false);
		}
		OVRPlugin.cachedCameraDepthConfidenceTexture.LoadRawTextureData(data, num * sizei.h);
		OVRPlugin.cachedCameraDepthConfidenceTexture.Apply();
		return OVRPlugin.cachedCameraDepthConfidenceTexture;
	}

	public static bool tiledMultiResSupported
	{
		get
		{
			if (!(OVRPlugin.version >= OVRPlugin.OVRP_1_21_0.version))
			{
				return false;
			}
			OVRPlugin.Bool @bool;
			OVRPlugin.Result result = OVRPlugin.OVRP_1_21_0.ovrp_GetTiledMultiResSupported(out @bool);
			if (result == OVRPlugin.Result.Success)
			{
				return @bool == OVRPlugin.Bool.True;
			}
			Debug.LogWarning("ovrp_GetTiledMultiResSupported return " + result);
			return false;
		}
	}

	public static OVRPlugin.TiledMultiResLevel tiledMultiResLevel
	{
		get
		{
			if (OVRPlugin.version >= OVRPlugin.OVRP_1_21_0.version && OVRPlugin.tiledMultiResSupported)
			{
				OVRPlugin.TiledMultiResLevel result2;
				OVRPlugin.Result result = OVRPlugin.OVRP_1_21_0.ovrp_GetTiledMultiResLevel(out result2);
				if (result != OVRPlugin.Result.Success)
				{
					Debug.LogWarning("ovrp_GetTiledMultiResLevel return " + result);
				}
				return result2;
			}
			return OVRPlugin.TiledMultiResLevel.Off;
		}
		set
		{
			if (OVRPlugin.version >= OVRPlugin.OVRP_1_21_0.version && OVRPlugin.tiledMultiResSupported)
			{
				OVRPlugin.Result result = OVRPlugin.OVRP_1_21_0.ovrp_SetTiledMultiResLevel(value);
				if (result != OVRPlugin.Result.Success)
				{
					Debug.LogWarning("ovrp_SetTiledMultiResLevel return " + result);
				}
			}
		}
	}

	public static bool gpuUtilSupported
	{
		get
		{
			if (!(OVRPlugin.version >= OVRPlugin.OVRP_1_21_0.version))
			{
				return false;
			}
			OVRPlugin.Bool @bool;
			OVRPlugin.Result result = OVRPlugin.OVRP_1_21_0.ovrp_GetGPUUtilSupported(out @bool);
			if (result == OVRPlugin.Result.Success)
			{
				return @bool == OVRPlugin.Bool.True;
			}
			Debug.LogWarning("ovrp_GetGPUUtilSupported return " + result);
			return false;
		}
	}

	public static float gpuUtilLevel
	{
		get
		{
			if (!(OVRPlugin.version >= OVRPlugin.OVRP_1_21_0.version) || !OVRPlugin.gpuUtilSupported)
			{
				return 0f;
			}
			float result2;
			OVRPlugin.Result result = OVRPlugin.OVRP_1_21_0.ovrp_GetGPUUtilLevel(out result2);
			if (result == OVRPlugin.Result.Success)
			{
				return result2;
			}
			Debug.LogWarning("ovrp_GetGPUUtilLevel return " + result);
			return 0f;
		}
	}

	public static float[] systemDisplayFrequenciesAvailable
	{
		get
		{
			if (OVRPlugin._cachedSystemDisplayFrequenciesAvailable == null)
			{
				OVRPlugin._cachedSystemDisplayFrequenciesAvailable = new float[0];
				if (OVRPlugin.version >= OVRPlugin.OVRP_1_21_0.version)
				{
					int num = 0;
					if (OVRPlugin.OVRP_1_21_0.ovrp_GetSystemDisplayAvailableFrequencies(IntPtr.Zero, out num) == OVRPlugin.Result.Success && num > 0)
					{
						int num2 = num;
						OVRPlugin._nativeSystemDisplayFrequenciesAvailable = new OVRNativeBuffer(4 * num2);
						if (OVRPlugin.OVRP_1_21_0.ovrp_GetSystemDisplayAvailableFrequencies(OVRPlugin._nativeSystemDisplayFrequenciesAvailable.GetPointer(0), out num) == OVRPlugin.Result.Success)
						{
							int num3 = (num > num2) ? num2 : num;
							if (num3 > 0)
							{
								OVRPlugin._cachedSystemDisplayFrequenciesAvailable = new float[num3];
								Marshal.Copy(OVRPlugin._nativeSystemDisplayFrequenciesAvailable.GetPointer(0), OVRPlugin._cachedSystemDisplayFrequenciesAvailable, 0, num3);
							}
						}
					}
				}
			}
			return OVRPlugin._cachedSystemDisplayFrequenciesAvailable;
		}
	}

	public static float systemDisplayFrequency
	{
		get
		{
			if (OVRPlugin.version >= OVRPlugin.OVRP_1_21_0.version)
			{
				float result;
				if (OVRPlugin.OVRP_1_21_0.ovrp_GetSystemDisplayFrequency2(out result) == OVRPlugin.Result.Success)
				{
					return result;
				}
				return 0f;
			}
			else
			{
				if (OVRPlugin.version >= OVRPlugin.OVRP_1_1_0.version)
				{
					return OVRPlugin.OVRP_1_1_0.ovrp_GetSystemDisplayFrequency();
				}
				return 0f;
			}
		}
		set
		{
			if (OVRPlugin.version >= OVRPlugin.OVRP_1_21_0.version)
			{
				OVRPlugin.OVRP_1_21_0.ovrp_SetSystemDisplayFrequency(value);
			}
		}
	}

	public static readonly Version wrapperVersion = OVRPlugin.OVRP_1_23_0.version;

	private static Version _version;

	private static Version _nativeSDKVersion;

	private const int OverlayShapeFlagShift = 4;

	public const int AppPerfFrameStatsMaxCount = 5;

	private static OVRPlugin.GUID _nativeAudioOutGuid = new OVRPlugin.GUID();

	private static Guid _cachedAudioOutGuid;

	private static string _cachedAudioOutString;

	private static OVRPlugin.GUID _nativeAudioInGuid = new OVRPlugin.GUID();

	private static Guid _cachedAudioInGuid;

	private static string _cachedAudioInString;

	private static Texture2D cachedCameraFrameTexture = null;

	private static Texture2D cachedCameraDepthTexture = null;

	private static Texture2D cachedCameraDepthConfidenceTexture = null;

	private static OVRNativeBuffer _nativeSystemDisplayFrequenciesAvailable = null;

	private static float[] _cachedSystemDisplayFrequenciesAvailable = null;

	private const string pluginName = "OVRPlugin";

	private static Version _versionZero = new Version(0, 0, 0);

	[StructLayout(LayoutKind.Sequential)]
	private class GUID
	{
		public int a;

		public short b;

		public short c;

		public byte d0;

		public byte d1;

		public byte d2;

		public byte d3;

		public byte d4;

		public byte d5;

		public byte d6;

		public byte d7;
	}

	public enum Bool
	{
		False,
		True
	}

	public enum Result
	{
		Success,
		Failure = -1000,
		Failure_InvalidParameter = -1001,
		Failure_NotInitialized = -1002,
		Failure_InvalidOperation = -1003,
		Failure_Unsupported = -1004,
		Failure_NotYetImplemented = -1005,
		Failure_OperationFailed = -1006,
		Failure_InsufficientSize = -1007
	}

	public enum CameraStatus
	{
		CameraStatus_None,
		CameraStatus_Connected,
		CameraStatus_Calibrating,
		CameraStatus_CalibrationFailed,
		CameraStatus_Calibrated,
		CameraStatus_EnumSize = 2147483647
	}

	public enum Eye
	{
		None = -1,
		Left,
		Right,
		Count
	}

	public enum Tracker
	{
		None = -1,
		Zero,
		One,
		Two,
		Three,
		Count
	}

	public enum Node
	{
		None = -1,
		EyeLeft,
		EyeRight,
		EyeCenter,
		HandLeft,
		HandRight,
		TrackerZero,
		TrackerOne,
		TrackerTwo,
		TrackerThree,
		Head,
		DeviceObjectZero,
		Count
	}

	public enum Controller
	{
		None,
		LTouch,
		RTouch,
		Touch,
		Remote,
		Gamepad = 16,
		Touchpad = 134217728,
		LTrackedRemote = 16777216,
		RTrackedRemote = 33554432,
		Active = -2147483648,
		All = -1
	}

	public enum TrackingOrigin
	{
		EyeLevel,
		FloorLevel,
		Count
	}

	public enum RecenterFlags
	{
		Default,
		Controllers = 1073741824,
		IgnoreAll = -2147483648,
		Count
	}

	public enum BatteryStatus
	{
		Charging,
		Discharging,
		Full,
		NotCharging,
		Unknown
	}

	public enum EyeTextureFormat
	{
		Default,
		R8G8B8A8_sRGB = 0,
		R8G8B8A8,
		R16G16B16A16_FP,
		R11G11B10_FP,
		B8G8R8A8_sRGB,
		B8G8R8A8,
		R5G6B5 = 11,
		EnumSize = 2147483647
	}

	public enum PlatformUI
	{
		None = -1,
		ConfirmQuit = 1,
		GlobalMenuTutorial
	}

	public enum SystemRegion
	{
		Unspecified,
		Japan,
		China
	}

	public enum SystemHeadset
	{
		None,
		GearVR_R320,
		GearVR_R321,
		GearVR_R322,
		GearVR_R323,
		GearVR_R324,
		GearVR_R325,
		Oculus_Go,
		Rift_DK1 = 4096,
		Rift_DK2,
		Rift_CV1
	}

	public enum OverlayShape
	{
		Quad,
		Cylinder,
		Cubemap,
		OffcenterCubemap = 4,
		Equirect
	}

	public enum Step
	{
		Render = -1,
		Physics
	}

	public enum CameraDevice
	{
		None,
		WebCamera0 = 100,
		WebCamera1,
		ZEDCamera = 300
	}

	public enum CameraDeviceDepthSensingMode
	{
		Standard,
		Fill
	}

	public enum CameraDeviceDepthQuality
	{
		Low,
		Medium,
		High
	}

	public enum TiledMultiResLevel
	{
		Off,
		LMSLow,
		LMSMedium,
		LMSHigh,
		EnumSize = 2147483647
	}

	public struct CameraDeviceIntrinsicsParameters
	{
		private float fx;

		private float fy;

		private float cx;

		private float cy;

		private double disto0;

		private double disto1;

		private double disto2;

		private double disto3;

		private double disto4;

		private float v_fov;

		private float h_fov;

		private float d_fov;

		private int w;

		private int h;
	}

	private enum OverlayFlag
	{
		None,
		OnTop,
		HeadLocked,
		ShapeFlag_Quad = 0,
		ShapeFlag_Cylinder = 16,
		ShapeFlag_Cubemap = 32,
		ShapeFlag_OffcenterCubemap = 64,
		ShapeFlagRangeMask = 240
	}

	public struct Vector2f
	{
		public float x;

		public float y;
	}

	public struct Vector3f
	{
		public override string ToString()
		{
			return string.Format("{0}, {1}, {2}", this.x, this.y, this.z);
		}

		public float x;

		public float y;

		public float z;
	}

	public struct Quatf
	{
		public override string ToString()
		{
			return string.Format("{0}, {1}, {2}, {3}", new object[]
			{
				this.x,
				this.y,
				this.z,
				this.w
			});
		}

		public float x;

		public float y;

		public float z;

		public float w;
	}

	public struct Posef
	{
		public override string ToString()
		{
			return string.Format("Position ({0}), Orientation({1})", this.Position, this.Orientation);
		}

		public OVRPlugin.Quatf Orientation;

		public OVRPlugin.Vector3f Position;
	}

	public struct PoseStatef
	{
		public OVRPlugin.Posef Pose;

		public OVRPlugin.Vector3f Velocity;

		public OVRPlugin.Vector3f Acceleration;

		public OVRPlugin.Vector3f AngularVelocity;

		public OVRPlugin.Vector3f AngularAcceleration;

		private double Time;
	}

	public struct ControllerState4
	{
		public ControllerState4(OVRPlugin.ControllerState2 cs)
		{
			this.ConnectedControllers = cs.ConnectedControllers;
			this.Buttons = cs.Buttons;
			this.Touches = cs.Touches;
			this.NearTouches = cs.NearTouches;
			this.LIndexTrigger = cs.LIndexTrigger;
			this.RIndexTrigger = cs.RIndexTrigger;
			this.LHandTrigger = cs.LHandTrigger;
			this.RHandTrigger = cs.RHandTrigger;
			this.LThumbstick = cs.LThumbstick;
			this.RThumbstick = cs.RThumbstick;
			this.LTouchpad = cs.LTouchpad;
			this.RTouchpad = cs.RTouchpad;
			this.LBatteryPercentRemaining = 0;
			this.RBatteryPercentRemaining = 0;
			this.LRecenterCount = 0;
			this.RRecenterCount = 0;
			this.Reserved_27 = 0;
			this.Reserved_26 = 0;
			this.Reserved_25 = 0;
			this.Reserved_24 = 0;
			this.Reserved_23 = 0;
			this.Reserved_22 = 0;
			this.Reserved_21 = 0;
			this.Reserved_20 = 0;
			this.Reserved_19 = 0;
			this.Reserved_18 = 0;
			this.Reserved_17 = 0;
			this.Reserved_16 = 0;
			this.Reserved_15 = 0;
			this.Reserved_14 = 0;
			this.Reserved_13 = 0;
			this.Reserved_12 = 0;
			this.Reserved_11 = 0;
			this.Reserved_10 = 0;
			this.Reserved_09 = 0;
			this.Reserved_08 = 0;
			this.Reserved_07 = 0;
			this.Reserved_06 = 0;
			this.Reserved_05 = 0;
			this.Reserved_04 = 0;
			this.Reserved_03 = 0;
			this.Reserved_02 = 0;
			this.Reserved_01 = 0;
			this.Reserved_00 = 0;
		}

		public uint ConnectedControllers;

		public uint Buttons;

		public uint Touches;

		public uint NearTouches;

		public float LIndexTrigger;

		public float RIndexTrigger;

		public float LHandTrigger;

		public float RHandTrigger;

		public OVRPlugin.Vector2f LThumbstick;

		public OVRPlugin.Vector2f RThumbstick;

		public OVRPlugin.Vector2f LTouchpad;

		public OVRPlugin.Vector2f RTouchpad;

		public byte LBatteryPercentRemaining;

		public byte RBatteryPercentRemaining;

		public byte LRecenterCount;

		public byte RRecenterCount;

		public byte Reserved_27;

		public byte Reserved_26;

		public byte Reserved_25;

		public byte Reserved_24;

		public byte Reserved_23;

		public byte Reserved_22;

		public byte Reserved_21;

		public byte Reserved_20;

		public byte Reserved_19;

		public byte Reserved_18;

		public byte Reserved_17;

		public byte Reserved_16;

		public byte Reserved_15;

		public byte Reserved_14;

		public byte Reserved_13;

		public byte Reserved_12;

		public byte Reserved_11;

		public byte Reserved_10;

		public byte Reserved_09;

		public byte Reserved_08;

		public byte Reserved_07;

		public byte Reserved_06;

		public byte Reserved_05;

		public byte Reserved_04;

		public byte Reserved_03;

		public byte Reserved_02;

		public byte Reserved_01;

		public byte Reserved_00;
	}

	public struct ControllerState2
	{
		public ControllerState2(OVRPlugin.ControllerState cs)
		{
			this.ConnectedControllers = cs.ConnectedControllers;
			this.Buttons = cs.Buttons;
			this.Touches = cs.Touches;
			this.NearTouches = cs.NearTouches;
			this.LIndexTrigger = cs.LIndexTrigger;
			this.RIndexTrigger = cs.RIndexTrigger;
			this.LHandTrigger = cs.LHandTrigger;
			this.RHandTrigger = cs.RHandTrigger;
			this.LThumbstick = cs.LThumbstick;
			this.RThumbstick = cs.RThumbstick;
			this.LTouchpad = new OVRPlugin.Vector2f
			{
				x = 0f,
				y = 0f
			};
			this.RTouchpad = new OVRPlugin.Vector2f
			{
				x = 0f,
				y = 0f
			};
		}

		public uint ConnectedControllers;

		public uint Buttons;

		public uint Touches;

		public uint NearTouches;

		public float LIndexTrigger;

		public float RIndexTrigger;

		public float LHandTrigger;

		public float RHandTrigger;

		public OVRPlugin.Vector2f LThumbstick;

		public OVRPlugin.Vector2f RThumbstick;

		public OVRPlugin.Vector2f LTouchpad;

		public OVRPlugin.Vector2f RTouchpad;
	}

	public struct ControllerState
	{
		public uint ConnectedControllers;

		public uint Buttons;

		public uint Touches;

		public uint NearTouches;

		public float LIndexTrigger;

		public float RIndexTrigger;

		public float LHandTrigger;

		public float RHandTrigger;

		public OVRPlugin.Vector2f LThumbstick;

		public OVRPlugin.Vector2f RThumbstick;
	}

	public struct HapticsBuffer
	{
		public IntPtr Samples;

		public int SamplesCount;
	}

	public struct HapticsState
	{
		public int SamplesAvailable;

		public int SamplesQueued;
	}

	public struct HapticsDesc
	{
		public int SampleRateHz;

		public int SampleSizeInBytes;

		public int MinimumSafeSamplesQueued;

		public int MinimumBufferSamplesCount;

		public int OptimalBufferSamplesCount;

		public int MaximumBufferSamplesCount;
	}

	public struct AppPerfFrameStats
	{
		public int HmdVsyncIndex;

		public int AppFrameIndex;

		public int AppDroppedFrameCount;

		public float AppMotionToPhotonLatency;

		public float AppQueueAheadTime;

		public float AppCpuElapsedTime;

		public float AppGpuElapsedTime;

		public int CompositorFrameIndex;

		public int CompositorDroppedFrameCount;

		public float CompositorLatency;

		public float CompositorCpuElapsedTime;

		public float CompositorGpuElapsedTime;

		public float CompositorCpuStartToGpuEndElapsedTime;

		public float CompositorGpuEndToVsyncElapsedTime;
	}

	public struct AppPerfStats
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
		public OVRPlugin.AppPerfFrameStats[] FrameStats;

		public int FrameStatsCount;

		public OVRPlugin.Bool AnyFrameStatsDropped;

		public float AdaptiveGpuPerformanceScale;
	}

	public struct Sizei
	{
		public int w;

		public int h;
	}

	public struct Sizef
	{
		public float w;

		public float h;
	}

	public struct Vector2i
	{
		public int x;

		public int y;
	}

	public struct Recti
	{
		private OVRPlugin.Vector2i Pos;

		private OVRPlugin.Sizei Size;
	}

	public struct Rectf
	{
		private OVRPlugin.Vector2f Pos;

		private OVRPlugin.Sizef Size;
	}

	public struct Frustumf
	{
		public float zNear;

		public float zFar;

		public float fovX;

		public float fovY;
	}

	public enum BoundaryType
	{
		OuterBoundary = 1,
		PlayArea = 256
	}

	public struct BoundaryTestResult
	{
		public OVRPlugin.Bool IsTriggering;

		public float ClosestDistance;

		public OVRPlugin.Vector3f ClosestPoint;

		public OVRPlugin.Vector3f ClosestPointNormal;
	}

	public struct BoundaryLookAndFeel
	{
		public OVRPlugin.Colorf Color;
	}

	public struct BoundaryGeometry
	{
		public OVRPlugin.BoundaryType BoundaryType;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
		public OVRPlugin.Vector3f[] Points;

		public int PointsCount;
	}

	public struct Colorf
	{
		public float r;

		public float g;

		public float b;

		public float a;
	}

	public struct Fovf
	{
		public float UpTan;

		public float DownTan;

		public float LeftTan;

		public float RightTan;
	}

	public struct CameraIntrinsics
	{
		public bool IsValid;

		public double LastChangedTimeSeconds;

		public OVRPlugin.Fovf FOVPort;

		public float VirtualNearPlaneDistanceMeters;

		public float VirtualFarPlaneDistanceMeters;

		public OVRPlugin.Sizei ImageSensorPixelResolution;
	}

	public struct CameraExtrinsics
	{
		public bool IsValid;

		public double LastChangedTimeSeconds;

		public OVRPlugin.CameraStatus CameraStatusData;

		public OVRPlugin.Node AttachedToNode;

		public OVRPlugin.Posef RelativePose;
	}

	public enum LayerLayout
	{
		Stereo,
		Mono,
		DoubleWide,
		Array,
		EnumSize = 15
	}

	public enum LayerFlags
	{
		Static = 1,
		LoadingScreen,
		SymmetricFov = 4,
		TextureOriginAtBottomLeft = 8,
		ChromaticAberrationCorrection = 16,
		NoAllocation = 32
	}

	public struct LayerDesc
	{
		public override string ToString()
		{
			string text = ", ";
			return string.Concat(new string[]
			{
				this.Shape.ToString(),
				text,
				this.Layout.ToString(),
				text,
				this.TextureSize.w.ToString(),
				"x",
				this.TextureSize.h.ToString(),
				text,
				this.MipLevels.ToString(),
				text,
				this.SampleCount.ToString(),
				text,
				this.Format.ToString(),
				text,
				this.LayerFlags.ToString()
			});
		}

		public OVRPlugin.OverlayShape Shape;

		public OVRPlugin.LayerLayout Layout;

		public OVRPlugin.Sizei TextureSize;

		public int MipLevels;

		public int SampleCount;

		public OVRPlugin.EyeTextureFormat Format;

		public int LayerFlags;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public OVRPlugin.Fovf[] Fov;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public OVRPlugin.Rectf[] VisibleRect;

		public OVRPlugin.Sizei MaxViewportSize;

		private OVRPlugin.EyeTextureFormat DepthFormat;
	}

	public struct LayerSubmit
	{
		private int LayerId;

		private int TextureStage;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		private OVRPlugin.Recti[] ViewportRect;

		private OVRPlugin.Posef Pose;

		private int LayerSubmitFlags;
	}

	private static class OVRP_0_1_0
	{
		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Sizei ovrp_GetEyeTextureSize(OVRPlugin.Eye eyeId);

		public static readonly Version version = new Version(0, 1, 0);
	}

	private static class OVRP_0_1_1
	{
		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_SetOverlayQuad2(OVRPlugin.Bool onTop, OVRPlugin.Bool headLocked, IntPtr texture, IntPtr device, OVRPlugin.Posef pose, OVRPlugin.Vector3f scale);

		public static readonly Version version = new Version(0, 1, 1);
	}

	private static class OVRP_0_1_2
	{
		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Posef ovrp_GetNodePose(OVRPlugin.Node nodeId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_SetControllerVibration(uint controllerMask, float frequency, float amplitude);

		public static readonly Version version = new Version(0, 1, 2);
	}

	private static class OVRP_0_1_3
	{
		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Posef ovrp_GetNodeVelocity(OVRPlugin.Node nodeId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Posef ovrp_GetNodeAcceleration(OVRPlugin.Node nodeId);

		public static readonly Version version = new Version(0, 1, 3);
	}

	private static class OVRP_0_5_0
	{
		public static readonly Version version = new Version(0, 5, 0);
	}

	private static class OVRP_1_0_0
	{
		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.TrackingOrigin ovrp_GetTrackingOriginType();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_SetTrackingOriginType(OVRPlugin.TrackingOrigin originType);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Posef ovrp_GetTrackingCalibratedOrigin();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_RecenterTrackingOrigin(uint flags);

		public static readonly Version version = new Version(1, 0, 0);
	}

	private static class OVRP_1_1_0
	{
		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_GetInitialized();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrp_GetVersion")]
		private static extern IntPtr _ovrp_GetVersion();

		public static string ovrp_GetVersion()
		{
			return Marshal.PtrToStringAnsi(OVRPlugin.OVRP_1_1_0._ovrp_GetVersion());
		}

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrp_GetNativeSDKVersion")]
		private static extern IntPtr _ovrp_GetNativeSDKVersion();

		public static string ovrp_GetNativeSDKVersion()
		{
			return Marshal.PtrToStringAnsi(OVRPlugin.OVRP_1_1_0._ovrp_GetNativeSDKVersion());
		}

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovrp_GetAudioOutId();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovrp_GetAudioInId();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetEyeTextureScale();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_SetEyeTextureScale(float value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_GetTrackingOrientationSupported();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_GetTrackingOrientationEnabled();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_SetTrackingOrientationEnabled(OVRPlugin.Bool value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_GetTrackingPositionSupported();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_GetTrackingPositionEnabled();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_SetTrackingPositionEnabled(OVRPlugin.Bool value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_GetNodePresent(OVRPlugin.Node nodeId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_GetNodeOrientationTracked(OVRPlugin.Node nodeId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_GetNodePositionTracked(OVRPlugin.Node nodeId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Frustumf ovrp_GetNodeFrustum(OVRPlugin.Node nodeId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.ControllerState ovrp_GetControllerState(uint controllerMask);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern int ovrp_GetSystemCpuLevel();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_SetSystemCpuLevel(int value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern int ovrp_GetSystemGpuLevel();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_SetSystemGpuLevel(int value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_GetSystemPowerSavingMode();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetSystemDisplayFrequency();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern int ovrp_GetSystemVSyncCount();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetSystemVolume();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.BatteryStatus ovrp_GetSystemBatteryStatus();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetSystemBatteryLevel();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetSystemBatteryTemperature();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrp_GetSystemProductName")]
		private static extern IntPtr _ovrp_GetSystemProductName();

		public static string ovrp_GetSystemProductName()
		{
			return Marshal.PtrToStringAnsi(OVRPlugin.OVRP_1_1_0._ovrp_GetSystemProductName());
		}

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_ShowSystemUI(OVRPlugin.PlatformUI ui);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_GetAppMonoscopic();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_SetAppMonoscopic(OVRPlugin.Bool value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_GetAppHasVrFocus();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_GetAppShouldQuit();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_GetAppShouldRecenter();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrp_GetAppLatencyTimings")]
		private static extern IntPtr _ovrp_GetAppLatencyTimings();

		public static string ovrp_GetAppLatencyTimings()
		{
			return Marshal.PtrToStringAnsi(OVRPlugin.OVRP_1_1_0._ovrp_GetAppLatencyTimings());
		}

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_GetUserPresent();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetUserIPD();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_SetUserIPD(float value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetUserEyeDepth();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_SetUserEyeDepth(float value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetUserEyeHeight();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_SetUserEyeHeight(float value);

		public static readonly Version version = new Version(1, 1, 0);
	}

	private static class OVRP_1_2_0
	{
		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_SetSystemVSyncCount(int vsyncCount);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrpi_SetTrackingCalibratedOrigin();

		public static readonly Version version = new Version(1, 2, 0);
	}

	private static class OVRP_1_3_0
	{
		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_GetEyeOcclusionMeshEnabled();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_SetEyeOcclusionMeshEnabled(OVRPlugin.Bool value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_GetSystemHeadphonesPresent();

		public static readonly Version version = new Version(1, 3, 0);
	}

	private static class OVRP_1_5_0
	{
		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.SystemRegion ovrp_GetSystemRegion();

		public static readonly Version version = new Version(1, 5, 0);
	}

	private static class OVRP_1_6_0
	{
		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_GetTrackingIPDEnabled();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_SetTrackingIPDEnabled(OVRPlugin.Bool value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.HapticsDesc ovrp_GetControllerHapticsDesc(uint controllerMask);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.HapticsState ovrp_GetControllerHapticsState(uint controllerMask);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_SetControllerHaptics(uint controllerMask, OVRPlugin.HapticsBuffer hapticsBuffer);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_SetOverlayQuad3(uint flags, IntPtr textureLeft, IntPtr textureRight, IntPtr device, OVRPlugin.Posef pose, OVRPlugin.Vector3f scale, int layerIndex);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetEyeRecommendedResolutionScale();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetAppCpuStartToGpuEndTime();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern int ovrp_GetSystemRecommendedMSAALevel();

		public static readonly Version version = new Version(1, 6, 0);
	}

	private static class OVRP_1_7_0
	{
		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_GetAppChromaticCorrection();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_SetAppChromaticCorrection(OVRPlugin.Bool value);

		public static readonly Version version = new Version(1, 7, 0);
	}

	private static class OVRP_1_8_0
	{
		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_GetBoundaryConfigured();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.BoundaryTestResult ovrp_TestBoundaryNode(OVRPlugin.Node nodeId, OVRPlugin.BoundaryType boundaryType);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.BoundaryTestResult ovrp_TestBoundaryPoint(OVRPlugin.Vector3f point, OVRPlugin.BoundaryType boundaryType);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_SetBoundaryLookAndFeel(OVRPlugin.BoundaryLookAndFeel lookAndFeel);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_ResetBoundaryLookAndFeel();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.BoundaryGeometry ovrp_GetBoundaryGeometry(OVRPlugin.BoundaryType boundaryType);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Vector3f ovrp_GetBoundaryDimensions(OVRPlugin.BoundaryType boundaryType);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_GetBoundaryVisible();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_SetBoundaryVisible(OVRPlugin.Bool value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_Update2(int stateId, int frameIndex, double predictionSeconds);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Posef ovrp_GetNodePose2(int stateId, OVRPlugin.Node nodeId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Posef ovrp_GetNodeVelocity2(int stateId, OVRPlugin.Node nodeId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Posef ovrp_GetNodeAcceleration2(int stateId, OVRPlugin.Node nodeId);

		public static readonly Version version = new Version(1, 8, 0);
	}

	private static class OVRP_1_9_0
	{
		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.SystemHeadset ovrp_GetSystemHeadsetType();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Controller ovrp_GetActiveController();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Controller ovrp_GetConnectedControllers();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_GetBoundaryGeometry2(OVRPlugin.BoundaryType boundaryType, IntPtr points, ref int pointsCount);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.AppPerfStats ovrp_GetAppPerfStats();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_ResetAppPerfStats();

		public static readonly Version version = new Version(1, 9, 0);
	}

	private static class OVRP_1_10_0
	{
		public static readonly Version version = new Version(1, 10, 0);
	}

	private static class OVRP_1_11_0
	{
		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_SetDesiredEyeTextureFormat(OVRPlugin.EyeTextureFormat value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.EyeTextureFormat ovrp_GetDesiredEyeTextureFormat();

		public static readonly Version version = new Version(1, 11, 0);
	}

	private static class OVRP_1_12_0
	{
		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetAppFramerate();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.PoseStatef ovrp_GetNodePoseState(OVRPlugin.Step stepId, OVRPlugin.Node nodeId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.ControllerState2 ovrp_GetControllerState2(uint controllerMask);

		public static readonly Version version = new Version(1, 12, 0);
	}

	private static class OVRP_1_15_0
	{
		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_InitializeMixedReality();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_ShutdownMixedReality();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_GetMixedRealityInitialized();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_UpdateExternalCamera();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_GetExternalCameraCount(out int cameraCount);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_GetExternalCameraName(int cameraId, [MarshalAs(UnmanagedType.LPArray, SizeConst = 32)] char[] cameraName);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_GetExternalCameraIntrinsics(int cameraId, out OVRPlugin.CameraIntrinsics cameraIntrinsics);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_GetExternalCameraExtrinsics(int cameraId, out OVRPlugin.CameraExtrinsics cameraExtrinsics);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_CalculateLayerDesc(OVRPlugin.OverlayShape shape, OVRPlugin.LayerLayout layout, ref OVRPlugin.Sizei textureSize, int mipLevels, int sampleCount, OVRPlugin.EyeTextureFormat format, int layerFlags, ref OVRPlugin.LayerDesc layerDesc);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_EnqueueSetupLayer(ref OVRPlugin.LayerDesc desc, IntPtr layerId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_EnqueueDestroyLayer(IntPtr layerId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_GetLayerTextureStageCount(int layerId, ref int layerTextureStageCount);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_GetLayerTexturePtr(int layerId, int stage, OVRPlugin.Eye eyeId, ref IntPtr textureHandle);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_EnqueueSubmitLayer(uint flags, IntPtr textureLeft, IntPtr textureRight, int layerId, int frameIndex, ref OVRPlugin.Posef pose, ref OVRPlugin.Vector3f scale, int layerIndex);

		public const int OVRP_EXTERNAL_CAMERA_NAME_SIZE = 32;

		public static readonly Version version = new Version(1, 15, 0);
	}

	private static class OVRP_1_16_0
	{
		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_UpdateCameraDevices();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_IsCameraDeviceAvailable(OVRPlugin.CameraDevice cameraDevice);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_SetCameraDevicePreferredColorFrameSize(OVRPlugin.CameraDevice cameraDevice, OVRPlugin.Sizei preferredColorFrameSize);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_OpenCameraDevice(OVRPlugin.CameraDevice cameraDevice);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_CloseCameraDevice(OVRPlugin.CameraDevice cameraDevice);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_HasCameraDeviceOpened(OVRPlugin.CameraDevice cameraDevice);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Bool ovrp_IsCameraDeviceColorFrameAvailable(OVRPlugin.CameraDevice cameraDevice);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_GetCameraDeviceColorFrameSize(OVRPlugin.CameraDevice cameraDevice, out OVRPlugin.Sizei colorFrameSize);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_GetCameraDeviceColorFrameBgraPixels(OVRPlugin.CameraDevice cameraDevice, out IntPtr colorFrameBgraPixels, out int colorFrameRowPitch);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_GetControllerState4(uint controllerMask, ref OVRPlugin.ControllerState4 controllerState);

		public static readonly Version version = new Version(1, 16, 0);
	}

	private static class OVRP_1_17_0
	{
		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_GetExternalCameraPose(OVRPlugin.CameraDevice camera, out OVRPlugin.Posef cameraPose);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_ConvertPoseToCameraSpace(OVRPlugin.CameraDevice camera, ref OVRPlugin.Posef trackingSpacePose, out OVRPlugin.Posef cameraSpacePose);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_GetCameraDeviceIntrinsicsParameters(OVRPlugin.CameraDevice camera, out OVRPlugin.Bool supportIntrinsics, out OVRPlugin.CameraDeviceIntrinsicsParameters intrinsicsParameters);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_DoesCameraDeviceSupportDepth(OVRPlugin.CameraDevice camera, out OVRPlugin.Bool supportDepth);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_GetCameraDeviceDepthSensingMode(OVRPlugin.CameraDevice camera, out OVRPlugin.CameraDeviceDepthSensingMode depthSensoringMode);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_SetCameraDeviceDepthSensingMode(OVRPlugin.CameraDevice camera, OVRPlugin.CameraDeviceDepthSensingMode depthSensoringMode);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_GetCameraDevicePreferredDepthQuality(OVRPlugin.CameraDevice camera, out OVRPlugin.CameraDeviceDepthQuality depthQuality);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_SetCameraDevicePreferredDepthQuality(OVRPlugin.CameraDevice camera, OVRPlugin.CameraDeviceDepthQuality depthQuality);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_IsCameraDeviceDepthFrameAvailable(OVRPlugin.CameraDevice camera, out OVRPlugin.Bool available);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_GetCameraDeviceDepthFrameSize(OVRPlugin.CameraDevice camera, out OVRPlugin.Sizei depthFrameSize);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_GetCameraDeviceDepthFramePixels(OVRPlugin.CameraDevice cameraDevice, out IntPtr depthFramePixels, out int depthFrameRowPitch);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_GetCameraDeviceDepthConfidencePixels(OVRPlugin.CameraDevice cameraDevice, out IntPtr depthConfidencePixels, out int depthConfidenceRowPitch);

		public static readonly Version version = new Version(1, 17, 0);
	}

	private static class OVRP_1_18_0
	{
		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_SetHandNodePoseStateLatency(double latencyInSeconds);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_GetHandNodePoseStateLatency(out double latencyInSeconds);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_GetAppHasInputFocus(out OVRPlugin.Bool appHasInputFocus);

		public static readonly Version version = new Version(1, 18, 0);
	}

	private static class OVRP_1_19_0
	{
		public static readonly Version version = new Version(1, 19, 0);
	}

	private static class OVRP_1_21_0
	{
		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_GetTiledMultiResSupported(out OVRPlugin.Bool foveationSupported);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_GetTiledMultiResLevel(out OVRPlugin.TiledMultiResLevel level);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_SetTiledMultiResLevel(OVRPlugin.TiledMultiResLevel level);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_GetGPUUtilSupported(out OVRPlugin.Bool gpuUtilSupported);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_GetGPUUtilLevel(out float gpuUtil);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_GetSystemDisplayFrequency2(out float systemDisplayFrequency);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_GetSystemDisplayAvailableFrequencies(IntPtr systemDisplayAvailableFrequencies, out int numFrequencies);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OVRPlugin.Result ovrp_SetSystemDisplayFrequency(float requestedFrequency);

		public static readonly Version version = new Version(1, 21, 0);
	}

	private static class OVRP_1_23_0
	{
		public static readonly Version version = new Version(1, 23, 0);
	}
}
