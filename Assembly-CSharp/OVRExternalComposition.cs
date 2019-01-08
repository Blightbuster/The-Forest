using System;
using UnityEngine;
using UnityEngine.Rendering;

public class OVRExternalComposition : OVRComposition
{
	public OVRExternalComposition(GameObject parentObject, Camera mainCamera)
	{
		this.backgroundCameraGameObject = new GameObject();
		this.backgroundCameraGameObject.name = "MRBackgroundCamera";
		this.backgroundCameraGameObject.transform.parent = parentObject.transform;
		this.backgroundCamera = this.backgroundCameraGameObject.AddComponent<Camera>();
		this.backgroundCamera.stereoTargetEye = StereoTargetEyeMask.None;
		this.backgroundCamera.depth = float.MaxValue;
		this.backgroundCamera.rect = new Rect(0f, 0f, 0.5f, 1f);
		this.backgroundCamera.clearFlags = mainCamera.clearFlags;
		this.backgroundCamera.backgroundColor = mainCamera.backgroundColor;
		this.backgroundCamera.cullingMask = (mainCamera.cullingMask & ~OVRManager.instance.extraHiddenLayers);
		this.backgroundCamera.nearClipPlane = mainCamera.nearClipPlane;
		this.backgroundCamera.farClipPlane = mainCamera.farClipPlane;
		this.foregroundCameraGameObject = new GameObject();
		this.foregroundCameraGameObject.name = "MRForgroundCamera";
		this.foregroundCameraGameObject.transform.parent = parentObject.transform;
		this.foregroundCamera = this.foregroundCameraGameObject.AddComponent<Camera>();
		this.foregroundCamera.stereoTargetEye = StereoTargetEyeMask.None;
		this.foregroundCamera.depth = float.MaxValue;
		this.foregroundCamera.rect = new Rect(0.5f, 0f, 0.5f, 1f);
		this.foregroundCamera.clearFlags = CameraClearFlags.Color;
		this.foregroundCamera.backgroundColor = OVRMixedReality.chromaKeyColor;
		this.foregroundCamera.cullingMask = (mainCamera.cullingMask & ~OVRManager.instance.extraHiddenLayers);
		this.foregroundCamera.nearClipPlane = mainCamera.nearClipPlane;
		this.foregroundCamera.farClipPlane = mainCamera.farClipPlane;
		this.cameraProxyPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
		this.cameraProxyPlane.name = "MRProxyClipPlane";
		this.cameraProxyPlane.transform.parent = parentObject.transform;
		this.cameraProxyPlane.GetComponent<Collider>().enabled = false;
		this.cameraProxyPlane.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
		Material material = new Material(Shader.Find("Oculus/OVRMRClipPlane"));
		this.cameraProxyPlane.GetComponent<MeshRenderer>().material = material;
		material.SetColor("_Color", OVRMixedReality.chromaKeyColor);
		material.SetFloat("_Visible", 0f);
		this.cameraProxyPlane.transform.localScale = new Vector3(1000f, 1000f, 1000f);
		this.cameraProxyPlane.SetActive(true);
		OVRMRForegroundCameraManager ovrmrforegroundCameraManager = this.foregroundCameraGameObject.AddComponent<OVRMRForegroundCameraManager>();
		ovrmrforegroundCameraManager.clipPlaneGameObj = this.cameraProxyPlane;
	}

	public override OVRManager.CompositionMethod CompositionMethod()
	{
		return OVRManager.CompositionMethod.External;
	}

	public override void Update(Camera mainCamera)
	{
		OVRPlugin.SetHandNodePoseStateLatency(0.0);
		this.backgroundCamera.clearFlags = mainCamera.clearFlags;
		this.backgroundCamera.backgroundColor = mainCamera.backgroundColor;
		this.backgroundCamera.cullingMask = (mainCamera.cullingMask & ~OVRManager.instance.extraHiddenLayers);
		this.backgroundCamera.nearClipPlane = mainCamera.nearClipPlane;
		this.backgroundCamera.farClipPlane = mainCamera.farClipPlane;
		this.foregroundCamera.cullingMask = (mainCamera.cullingMask & ~OVRManager.instance.extraHiddenLayers);
		this.foregroundCamera.nearClipPlane = mainCamera.nearClipPlane;
		this.foregroundCamera.farClipPlane = mainCamera.farClipPlane;
		if (OVRMixedReality.useFakeExternalCamera || OVRPlugin.GetExternalCameraCount() == 0)
		{
			OVRPose pose = default(OVRPose);
			pose = OVRExtensions.ToWorldSpacePose(new OVRPose
			{
				position = OVRMixedReality.fakeCameraPositon,
				orientation = OVRMixedReality.fakeCameraRotation
			});
			this.backgroundCamera.fieldOfView = OVRMixedReality.fakeCameraFov;
			this.backgroundCamera.aspect = OVRMixedReality.fakeCameraAspect;
			this.backgroundCamera.transform.FromOVRPose(pose, false);
			this.foregroundCamera.fieldOfView = OVRMixedReality.fakeCameraFov;
			this.foregroundCamera.aspect = OVRMixedReality.fakeCameraAspect;
			this.foregroundCamera.transform.FromOVRPose(pose, false);
		}
		else
		{
			OVRPlugin.CameraExtrinsics extrinsics;
			OVRPlugin.CameraIntrinsics cameraIntrinsics;
			if (!OVRPlugin.GetMixedRealityCameraInfo(0, out extrinsics, out cameraIntrinsics))
			{
				Debug.LogError("Failed to get external camera information");
				return;
			}
			OVRPose pose2 = base.ComputeCameraWorldSpacePose(extrinsics);
			float fieldOfView = Mathf.Atan(cameraIntrinsics.FOVPort.UpTan) * 57.29578f * 2f;
			float aspect = cameraIntrinsics.FOVPort.LeftTan / cameraIntrinsics.FOVPort.UpTan;
			this.backgroundCamera.fieldOfView = fieldOfView;
			this.backgroundCamera.aspect = aspect;
			this.backgroundCamera.transform.FromOVRPose(pose2, false);
			this.foregroundCamera.fieldOfView = fieldOfView;
			this.foregroundCamera.aspect = cameraIntrinsics.FOVPort.LeftTan / cameraIntrinsics.FOVPort.UpTan;
			this.foregroundCamera.transform.FromOVRPose(pose2, false);
		}
		Vector3 b = mainCamera.transform.position - this.foregroundCamera.transform.position;
		b.y = 0f;
		this.cameraProxyPlane.transform.position = mainCamera.transform.position;
		this.cameraProxyPlane.transform.LookAt(this.cameraProxyPlane.transform.position + b);
	}

	public override void Cleanup()
	{
		OVRCompositionUtil.SafeDestroy(ref this.backgroundCameraGameObject);
		this.backgroundCamera = null;
		OVRCompositionUtil.SafeDestroy(ref this.foregroundCameraGameObject);
		this.foregroundCamera = null;
		OVRCompositionUtil.SafeDestroy(ref this.cameraProxyPlane);
		Debug.Log("ExternalComposition deactivated");
	}

	private GameObject foregroundCameraGameObject;

	private Camera foregroundCamera;

	private GameObject backgroundCameraGameObject;

	private Camera backgroundCamera;

	private GameObject cameraProxyPlane;
}
