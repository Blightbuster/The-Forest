using System;
using UnityEngine;


public class OVRDirectComposition : OVRCameraComposition
{
	
	public OVRDirectComposition(GameObject parentObject, Camera mainCamera, OVRManager.CameraDevice cameraDevice, bool useDynamicLighting, OVRManager.DepthQuality depthQuality) : base(cameraDevice, useDynamicLighting, depthQuality)
	{
		this.directCompositionCameraGameObject = new GameObject();
		this.directCompositionCameraGameObject.name = "MRDirectCompositionCamera";
		this.directCompositionCameraGameObject.transform.parent = parentObject.transform;
		this.directCompositionCamera = this.directCompositionCameraGameObject.AddComponent<Camera>();
		this.directCompositionCamera.stereoTargetEye = StereoTargetEyeMask.None;
		this.directCompositionCamera.depth = float.MaxValue;
		this.directCompositionCamera.rect = new Rect(0f, 0f, 1f, 1f);
		this.directCompositionCamera.clearFlags = mainCamera.clearFlags;
		this.directCompositionCamera.backgroundColor = mainCamera.backgroundColor;
		this.directCompositionCamera.cullingMask = (mainCamera.cullingMask & ~OVRManager.instance.extraHiddenLayers);
		this.directCompositionCamera.nearClipPlane = mainCamera.nearClipPlane;
		this.directCompositionCamera.farClipPlane = mainCamera.farClipPlane;
		if (!this.hasCameraDeviceOpened)
		{
			Debug.LogError("Unable to open camera device " + cameraDevice);
		}
		else
		{
			Debug.Log("DirectComposition activated : useDynamicLighting " + ((!useDynamicLighting) ? "OFF" : "ON"));
			base.CreateCameraFramePlaneObject(parentObject, this.directCompositionCamera, useDynamicLighting);
		}
	}

	
	public override OVRManager.CompositionMethod CompositionMethod()
	{
		return OVRManager.CompositionMethod.Direct;
	}

	
	public override void Update(Camera mainCamera)
	{
		if (!this.hasCameraDeviceOpened)
		{
			return;
		}
		if (!OVRPlugin.SetHandNodePoseStateLatency((double)OVRManager.instance.handPoseStateLatency))
		{
			Debug.LogWarning("HandPoseStateLatency is invalid. Expect a value between 0.0 to 0.5, get " + OVRManager.instance.handPoseStateLatency);
		}
		this.directCompositionCamera.clearFlags = mainCamera.clearFlags;
		this.directCompositionCamera.backgroundColor = mainCamera.backgroundColor;
		this.directCompositionCamera.cullingMask = (mainCamera.cullingMask & ~OVRManager.instance.extraHiddenLayers);
		this.directCompositionCamera.nearClipPlane = mainCamera.nearClipPlane;
		this.directCompositionCamera.farClipPlane = mainCamera.farClipPlane;
		OVRPlugin.CameraExtrinsics extrinsics;
		OVRPlugin.CameraIntrinsics cameraIntrinsics;
		if (OVRMixedReality.useFakeExternalCamera || OVRPlugin.GetExternalCameraCount() == 0)
		{
			OVRPose pose = default(OVRPose);
			pose = OVRExtensions.ToWorldSpacePose(new OVRPose
			{
				position = OVRMixedReality.fakeCameraPositon,
				orientation = OVRMixedReality.fakeCameraRotation
			});
			this.directCompositionCamera.fieldOfView = OVRMixedReality.fakeCameraFov;
			this.directCompositionCamera.aspect = OVRMixedReality.fakeCameraAspect;
			this.directCompositionCamera.transform.FromOVRPose(pose, false);
		}
		else if (OVRPlugin.GetMixedRealityCameraInfo(0, out extrinsics, out cameraIntrinsics))
		{
			OVRPose pose2 = base.ComputeCameraWorldSpacePose(extrinsics);
			float fieldOfView = Mathf.Atan(cameraIntrinsics.FOVPort.UpTan) * 57.29578f * 2f;
			float aspect = cameraIntrinsics.FOVPort.LeftTan / cameraIntrinsics.FOVPort.UpTan;
			this.directCompositionCamera.fieldOfView = fieldOfView;
			this.directCompositionCamera.aspect = aspect;
			this.directCompositionCamera.transform.FromOVRPose(pose2, false);
		}
		else
		{
			Debug.LogWarning("Failed to get external camera information");
		}
		if (this.hasCameraDeviceOpened)
		{
			if (this.boundaryMeshMaskTexture == null || this.boundaryMeshMaskTexture.width != Screen.width || this.boundaryMeshMaskTexture.height != Screen.height)
			{
				this.boundaryMeshMaskTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.R8);
				this.boundaryMeshMaskTexture.Create();
			}
			base.UpdateCameraFramePlaneObject(mainCamera, this.directCompositionCamera, this.boundaryMeshMaskTexture);
			this.directCompositionCamera.GetComponent<OVRCameraComposition.OVRCameraFrameCompositionManager>().boundaryMeshMaskTexture = this.boundaryMeshMaskTexture;
		}
	}

	
	public override void Cleanup()
	{
		base.Cleanup();
		OVRCompositionUtil.SafeDestroy(ref this.directCompositionCameraGameObject);
		this.directCompositionCamera = null;
		Debug.Log("DirectComposition deactivated");
	}

	
	public GameObject directCompositionCameraGameObject;

	
	public Camera directCompositionCamera;

	
	public RenderTexture boundaryMeshMaskTexture;
}
