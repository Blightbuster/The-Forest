using System;
using UnityEngine;


internal static class OVRMixedReality
{
	
	public static void Update(GameObject parentObject, Camera mainCamera, OVRManager.CompositionMethod compositionMethod, bool useDynamicLighting, OVRManager.CameraDevice cameraDevice, OVRManager.DepthQuality depthQuality)
	{
		if (!OVRPlugin.initialized)
		{
			Debug.LogError("OVRPlugin not initialized");
			return;
		}
		if (!OVRPlugin.IsMixedRealityInitialized())
		{
			OVRPlugin.InitializeMixedReality();
		}
		if (!OVRPlugin.IsMixedRealityInitialized())
		{
			Debug.LogError("Unable to initialize MixedReality");
			return;
		}
		OVRPlugin.UpdateExternalCamera();
		OVRPlugin.UpdateCameraDevices();
		if (OVRMixedReality.currentComposition != null && OVRMixedReality.currentComposition.CompositionMethod() != compositionMethod)
		{
			OVRMixedReality.currentComposition.Cleanup();
			OVRMixedReality.currentComposition = null;
		}
		if (compositionMethod == OVRManager.CompositionMethod.External)
		{
			if (OVRMixedReality.currentComposition == null)
			{
				OVRMixedReality.currentComposition = new OVRExternalComposition(parentObject, mainCamera);
			}
		}
		else if (compositionMethod == OVRManager.CompositionMethod.Direct)
		{
			if (OVRMixedReality.currentComposition == null)
			{
				OVRMixedReality.currentComposition = new OVRDirectComposition(parentObject, mainCamera, cameraDevice, useDynamicLighting, depthQuality);
			}
		}
		else
		{
			if (compositionMethod != OVRManager.CompositionMethod.Sandwich)
			{
				Debug.LogError("Unknown CompositionMethod : " + compositionMethod);
				return;
			}
			if (OVRMixedReality.currentComposition == null)
			{
				OVRMixedReality.currentComposition = new OVRSandwichComposition(parentObject, mainCamera, cameraDevice, useDynamicLighting, depthQuality);
			}
		}
		OVRMixedReality.currentComposition.Update(mainCamera);
	}

	
	public static void Cleanup()
	{
		if (OVRMixedReality.currentComposition != null)
		{
			OVRMixedReality.currentComposition.Cleanup();
			OVRMixedReality.currentComposition = null;
		}
		if (OVRPlugin.IsMixedRealityInitialized())
		{
			OVRPlugin.ShutdownMixedReality();
		}
	}

	
	public static void RecenterPose()
	{
		if (OVRMixedReality.currentComposition != null)
		{
			OVRMixedReality.currentComposition.RecenterPose();
		}
	}

	
	public static Color chromaKeyColor = Color.green;

	
	public static bool useFakeExternalCamera = false;

	
	public static Vector3 fakeCameraPositon = new Vector3(3f, 0f, 3f);

	
	public static Quaternion fakeCameraRotation = Quaternion.LookRotation((new Vector3(0f, 1f, 0f) - OVRMixedReality.fakeCameraPositon).normalized, Vector3.up);

	
	public static float fakeCameraFov = 60f;

	
	public static float fakeCameraAspect = 1.77777779f;

	
	public static OVRComposition currentComposition = null;
}
