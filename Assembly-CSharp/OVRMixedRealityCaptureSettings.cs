using System;
using System.IO;
using UnityEngine;

public class OVRMixedRealityCaptureSettings : ScriptableObject
{
	public void ReadFrom(OVRManager manager)
	{
		this.enableMixedReality = manager.enableMixedReality;
		this.compositionMethod = manager.compositionMethod;
		this.extraHiddenLayers = manager.extraHiddenLayers;
		this.capturingCameraDevice = manager.capturingCameraDevice;
		this.flipCameraFrameHorizontally = manager.flipCameraFrameHorizontally;
		this.flipCameraFrameVertically = manager.flipCameraFrameVertically;
		this.handPoseStateLatency = manager.handPoseStateLatency;
		this.sandwichCompositionRenderLatency = manager.sandwichCompositionRenderLatency;
		this.sandwichCompositionBufferedFrames = manager.sandwichCompositionBufferedFrames;
		this.chromaKeyColor = manager.chromaKeyColor;
		this.chromaKeySimilarity = manager.chromaKeySimilarity;
		this.chromaKeySmoothRange = manager.chromaKeySmoothRange;
		this.chromaKeySpillRange = manager.chromaKeySpillRange;
		this.useDynamicLighting = manager.useDynamicLighting;
		this.depthQuality = manager.depthQuality;
		this.dynamicLightingSmoothFactor = manager.dynamicLightingSmoothFactor;
		this.dynamicLightingDepthVariationClampingValue = manager.dynamicLightingDepthVariationClampingValue;
		this.virtualGreenScreenType = manager.virtualGreenScreenType;
		this.virtualGreenScreenTopY = manager.virtualGreenScreenTopY;
		this.virtualGreenScreenBottomY = manager.virtualGreenScreenBottomY;
		this.virtualGreenScreenApplyDepthCulling = manager.virtualGreenScreenApplyDepthCulling;
		this.virtualGreenScreenDepthTolerance = manager.virtualGreenScreenDepthTolerance;
	}

	public void ApplyTo(OVRManager manager)
	{
		manager.enableMixedReality = this.enableMixedReality;
		manager.compositionMethod = this.compositionMethod;
		manager.extraHiddenLayers = this.extraHiddenLayers;
		manager.capturingCameraDevice = this.capturingCameraDevice;
		manager.flipCameraFrameHorizontally = this.flipCameraFrameHorizontally;
		manager.flipCameraFrameVertically = this.flipCameraFrameVertically;
		manager.handPoseStateLatency = this.handPoseStateLatency;
		manager.sandwichCompositionRenderLatency = this.sandwichCompositionRenderLatency;
		manager.sandwichCompositionBufferedFrames = this.sandwichCompositionBufferedFrames;
		manager.chromaKeyColor = this.chromaKeyColor;
		manager.chromaKeySimilarity = this.chromaKeySimilarity;
		manager.chromaKeySmoothRange = this.chromaKeySmoothRange;
		manager.chromaKeySpillRange = this.chromaKeySpillRange;
		manager.useDynamicLighting = this.useDynamicLighting;
		manager.depthQuality = this.depthQuality;
		manager.dynamicLightingSmoothFactor = this.dynamicLightingSmoothFactor;
		manager.dynamicLightingDepthVariationClampingValue = this.dynamicLightingDepthVariationClampingValue;
		manager.virtualGreenScreenType = this.virtualGreenScreenType;
		manager.virtualGreenScreenTopY = this.virtualGreenScreenTopY;
		manager.virtualGreenScreenBottomY = this.virtualGreenScreenBottomY;
		manager.virtualGreenScreenApplyDepthCulling = this.virtualGreenScreenApplyDepthCulling;
		manager.virtualGreenScreenDepthTolerance = this.virtualGreenScreenDepthTolerance;
	}

	public void WriteToConfigurationFile()
	{
		string contents = JsonUtility.ToJson(this, true);
		try
		{
			string text = Path.Combine(Application.dataPath, "mrc.config");
			Debug.Log("Write OVRMixedRealityCaptureSettings to " + text);
			File.WriteAllText(text, contents);
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Exception caught " + ex.Message);
		}
	}

	public void CombineWithConfigurationFile()
	{
		try
		{
			string text = Path.Combine(Application.dataPath, "mrc.config");
			if (File.Exists(text))
			{
				Debug.Log("MixedRealityCapture configuration file found at " + text);
				string json = File.ReadAllText(text);
				Debug.Log("Apply MixedRealityCapture configuration");
				JsonUtility.FromJsonOverwrite(json, this);
			}
			else
			{
				Debug.Log("MixedRealityCapture configuration file doesn't exist at " + text);
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Exception caught " + ex.Message);
		}
	}

	public bool enableMixedReality;

	public LayerMask extraHiddenLayers;

	public OVRManager.CompositionMethod compositionMethod;

	public OVRManager.CameraDevice capturingCameraDevice;

	public bool flipCameraFrameHorizontally;

	public bool flipCameraFrameVertically;

	public float handPoseStateLatency;

	public float sandwichCompositionRenderLatency;

	public int sandwichCompositionBufferedFrames = 8;

	public Color chromaKeyColor = Color.green;

	public float chromaKeySimilarity = 0.6f;

	public float chromaKeySmoothRange = 0.03f;

	public float chromaKeySpillRange = 0.04f;

	public bool useDynamicLighting;

	public OVRManager.DepthQuality depthQuality = OVRManager.DepthQuality.Medium;

	public float dynamicLightingSmoothFactor = 8f;

	public float dynamicLightingDepthVariationClampingValue = 0.001f;

	public OVRManager.VirtualGreenScreenType virtualGreenScreenType;

	public float virtualGreenScreenTopY;

	public float virtualGreenScreenBottomY;

	public bool virtualGreenScreenApplyDepthCulling;

	public float virtualGreenScreenDepthTolerance = 0.2f;

	private const string configFileName = "mrc.config";
}
