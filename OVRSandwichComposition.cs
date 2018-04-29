using System;
using UnityEngine;
using UnityEngine.Rendering;


public class OVRSandwichComposition : OVRCameraComposition
{
	
	public OVRSandwichComposition(GameObject parentObject, Camera mainCamera, OVRManager.CameraDevice cameraDevice, bool useDynamicLighting, OVRManager.DepthQuality depthQuality) : base(cameraDevice, useDynamicLighting, depthQuality)
	{
		this.frameRealtime = Time.realtimeSinceStartup;
		this.historyRecordCount = OVRManager.instance.sandwichCompositionBufferedFrames;
		if (this.historyRecordCount < 1)
		{
			Debug.LogWarning("Invalid sandwichCompositionBufferedFrames in OVRManager. It should be at least 1");
			this.historyRecordCount = 1;
		}
		if (this.historyRecordCount > 16)
		{
			Debug.LogWarning("The value of sandwichCompositionBufferedFrames in OVRManager is too big. It would consume a lot of memory. It has been override to 16");
			this.historyRecordCount = 16;
		}
		this.historyRecordArray = new OVRSandwichComposition.HistoryRecord[this.historyRecordCount];
		for (int i = 0; i < this.historyRecordCount; i++)
		{
			this.historyRecordArray[i] = new OVRSandwichComposition.HistoryRecord();
		}
		this.historyRecordCursorIndex = 0;
		this.fgCamera = new GameObject("MRSandwichForegroundCamera")
		{
			transform = 
			{
				parent = parentObject.transform
			}
		}.AddComponent<Camera>();
		this.fgCamera.depth = 200f;
		this.fgCamera.clearFlags = CameraClearFlags.Color;
		this.fgCamera.backgroundColor = Color.clear;
		this.fgCamera.cullingMask = (mainCamera.cullingMask & ~OVRManager.instance.extraHiddenLayers);
		this.fgCamera.nearClipPlane = mainCamera.nearClipPlane;
		this.fgCamera.farClipPlane = mainCamera.farClipPlane;
		this.bgCamera = new GameObject("MRSandwichBackgroundCamera")
		{
			transform = 
			{
				parent = parentObject.transform
			}
		}.AddComponent<Camera>();
		this.bgCamera.depth = 100f;
		this.bgCamera.clearFlags = mainCamera.clearFlags;
		this.bgCamera.backgroundColor = mainCamera.backgroundColor;
		this.bgCamera.cullingMask = (mainCamera.cullingMask & ~OVRManager.instance.extraHiddenLayers);
		this.bgCamera.nearClipPlane = mainCamera.nearClipPlane;
		this.bgCamera.farClipPlane = mainCamera.farClipPlane;
		this.cameraProxyPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
		this.cameraProxyPlane.name = "MRProxyClipPlane";
		this.cameraProxyPlane.transform.parent = parentObject.transform;
		this.cameraProxyPlane.GetComponent<Collider>().enabled = false;
		this.cameraProxyPlane.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
		Material material = new Material(Shader.Find("Oculus/OVRMRClipPlane"));
		this.cameraProxyPlane.GetComponent<MeshRenderer>().material = material;
		material.SetColor("_Color", Color.clear);
		material.SetFloat("_Visible", 0f);
		this.cameraProxyPlane.transform.localScale = new Vector3(1000f, 1000f, 1000f);
		this.cameraProxyPlane.SetActive(true);
		OVRMRForegroundCameraManager ovrmrforegroundCameraManager = this.fgCamera.gameObject.AddComponent<OVRMRForegroundCameraManager>();
		ovrmrforegroundCameraManager.clipPlaneGameObj = this.cameraProxyPlane;
		this.compositionCamera = new GameObject("MRSandwichCaptureCamera")
		{
			transform = 
			{
				parent = parentObject.transform
			}
		}.AddComponent<Camera>();
		this.compositionCamera.stereoTargetEye = StereoTargetEyeMask.None;
		this.compositionCamera.depth = float.MaxValue;
		this.compositionCamera.rect = new Rect(0f, 0f, 1f, 1f);
		this.compositionCamera.clearFlags = CameraClearFlags.Depth;
		this.compositionCamera.backgroundColor = mainCamera.backgroundColor;
		this.compositionCamera.cullingMask = 1 << this.cameraFramePlaneLayer;
		this.compositionCamera.nearClipPlane = mainCamera.nearClipPlane;
		this.compositionCamera.farClipPlane = mainCamera.farClipPlane;
		if (!this.hasCameraDeviceOpened)
		{
			Debug.LogError("Unable to open camera device " + cameraDevice);
		}
		else
		{
			Debug.Log("SandwichComposition activated : useDynamicLighting " + ((!useDynamicLighting) ? "OFF" : "ON"));
			base.CreateCameraFramePlaneObject(parentObject, this.compositionCamera, useDynamicLighting);
			this.cameraFramePlaneObject.layer = this.cameraFramePlaneLayer;
			this.RefreshRenderTextures(mainCamera);
			this.compositionManager = this.compositionCamera.gameObject.AddComponent<OVRSandwichComposition.OVRSandwichCompositionManager>();
			this.compositionManager.fgTexture = this.historyRecordArray[this.historyRecordCursorIndex].fgRenderTexture;
			this.compositionManager.bgTexture = this.historyRecordArray[this.historyRecordCursorIndex].bgRenderTexture;
		}
	}

	
	
	public int cameraFramePlaneLayer
	{
		get
		{
			if (this._cameraFramePlaneLayer < 0)
			{
				for (int i = 24; i <= 29; i++)
				{
					string text = LayerMask.LayerToName(i);
					if (text == null || text.Length == 0)
					{
						this._cameraFramePlaneLayer = i;
						break;
					}
				}
				if (this._cameraFramePlaneLayer == -1)
				{
					Debug.LogWarning("Unable to find an unnamed layer between 24 and 29.");
					this._cameraFramePlaneLayer = 25;
				}
				Debug.LogFormat("Set the CameraFramePlaneLayer in SandwichComposition to {0}. Please do NOT put any other gameobject in this layer.", new object[]
				{
					this._cameraFramePlaneLayer
				});
			}
			return this._cameraFramePlaneLayer;
		}
	}

	
	public override OVRManager.CompositionMethod CompositionMethod()
	{
		return OVRManager.CompositionMethod.Sandwich;
	}

	
	public override void Update(Camera mainCamera)
	{
		if (!this.hasCameraDeviceOpened)
		{
			return;
		}
		this.frameRealtime = Time.realtimeSinceStartup;
		this.historyRecordCursorIndex++;
		if (this.historyRecordCursorIndex >= this.historyRecordCount)
		{
			this.historyRecordCursorIndex = 0;
		}
		if (!OVRPlugin.SetHandNodePoseStateLatency((double)OVRManager.instance.handPoseStateLatency))
		{
			Debug.LogWarning("HandPoseStateLatency is invalid. Expect a value between 0.0 to 0.5, get " + OVRManager.instance.handPoseStateLatency);
		}
		this.RefreshRenderTextures(mainCamera);
		this.bgCamera.clearFlags = mainCamera.clearFlags;
		this.bgCamera.backgroundColor = mainCamera.backgroundColor;
		this.bgCamera.cullingMask = (mainCamera.cullingMask & ~OVRManager.instance.extraHiddenLayers);
		this.fgCamera.cullingMask = (mainCamera.cullingMask & ~OVRManager.instance.extraHiddenLayers);
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
			this.RefreshCameraPoses(OVRMixedReality.fakeCameraFov, OVRMixedReality.fakeCameraAspect, pose);
		}
		else if (OVRPlugin.GetMixedRealityCameraInfo(0, out extrinsics, out cameraIntrinsics))
		{
			OVRPose pose2 = base.ComputeCameraWorldSpacePose(extrinsics);
			float fovY = Mathf.Atan(cameraIntrinsics.FOVPort.UpTan) * 57.29578f * 2f;
			float aspect = cameraIntrinsics.FOVPort.LeftTan / cameraIntrinsics.FOVPort.UpTan;
			this.RefreshCameraPoses(fovY, aspect, pose2);
		}
		else
		{
			Debug.LogWarning("Failed to get external camera information");
		}
		this.compositionCamera.GetComponent<OVRCameraComposition.OVRCameraFrameCompositionManager>().boundaryMeshMaskTexture = this.historyRecordArray[this.historyRecordCursorIndex].boundaryMeshMaskTexture;
		OVRSandwichComposition.HistoryRecord historyRecordForComposition = this.GetHistoryRecordForComposition();
		base.UpdateCameraFramePlaneObject(mainCamera, this.compositionCamera, historyRecordForComposition.boundaryMeshMaskTexture);
		OVRSandwichComposition.OVRSandwichCompositionManager component = this.compositionCamera.gameObject.GetComponent<OVRSandwichComposition.OVRSandwichCompositionManager>();
		component.fgTexture = historyRecordForComposition.fgRenderTexture;
		component.bgTexture = historyRecordForComposition.bgRenderTexture;
		this.cameraProxyPlane.transform.position = this.fgCamera.transform.position + this.fgCamera.transform.forward * this.cameraFramePlaneDistance;
		this.cameraProxyPlane.transform.LookAt(this.cameraProxyPlane.transform.position + this.fgCamera.transform.forward);
	}

	
	public override void Cleanup()
	{
		base.Cleanup();
		Camera[] array = new Camera[]
		{
			this.fgCamera,
			this.bgCamera,
			this.compositionCamera
		};
		foreach (Camera camera in array)
		{
			OVRCompositionUtil.SafeDestroy(camera.gameObject);
		}
		this.fgCamera = null;
		this.bgCamera = null;
		this.compositionCamera = null;
		Debug.Log("SandwichComposition deactivated");
	}

	
	private RenderTextureFormat DesiredRenderTextureFormat(RenderTextureFormat originalFormat)
	{
		if (originalFormat == RenderTextureFormat.RGB565)
		{
			return RenderTextureFormat.ARGB1555;
		}
		if (originalFormat == RenderTextureFormat.RGB111110Float)
		{
			return RenderTextureFormat.ARGBHalf;
		}
		return originalFormat;
	}

	
	protected void RefreshRenderTextures(Camera mainCamera)
	{
		int width = Screen.width;
		int height = Screen.height;
		RenderTextureFormat renderTextureFormat = (!mainCamera.targetTexture) ? RenderTextureFormat.ARGB32 : this.DesiredRenderTextureFormat(mainCamera.targetTexture.format);
		int num = (!mainCamera.targetTexture) ? 24 : mainCamera.targetTexture.depth;
		OVRSandwichComposition.HistoryRecord historyRecord = this.historyRecordArray[this.historyRecordCursorIndex];
		historyRecord.timestamp = this.frameRealtime;
		if (historyRecord.fgRenderTexture == null || historyRecord.fgRenderTexture.width != width || historyRecord.fgRenderTexture.height != height || historyRecord.fgRenderTexture.format != renderTextureFormat || historyRecord.fgRenderTexture.depth != num)
		{
			historyRecord.fgRenderTexture = new RenderTexture(width, height, num, renderTextureFormat);
			historyRecord.fgRenderTexture.name = "Sandwich FG " + this.historyRecordCursorIndex.ToString();
		}
		this.fgCamera.targetTexture = historyRecord.fgRenderTexture;
		if (historyRecord.bgRenderTexture == null || historyRecord.bgRenderTexture.width != width || historyRecord.bgRenderTexture.height != height || historyRecord.bgRenderTexture.format != renderTextureFormat || historyRecord.bgRenderTexture.depth != num)
		{
			historyRecord.bgRenderTexture = new RenderTexture(width, height, num, renderTextureFormat);
			historyRecord.bgRenderTexture.name = "Sandwich BG " + this.historyRecordCursorIndex.ToString();
		}
		this.bgCamera.targetTexture = historyRecord.bgRenderTexture;
		if (OVRManager.instance.virtualGreenScreenType != OVRManager.VirtualGreenScreenType.Off)
		{
			if (historyRecord.boundaryMeshMaskTexture == null || historyRecord.boundaryMeshMaskTexture.width != width || historyRecord.boundaryMeshMaskTexture.height != height)
			{
				historyRecord.boundaryMeshMaskTexture = new RenderTexture(width, height, 0, RenderTextureFormat.R8);
				historyRecord.boundaryMeshMaskTexture.name = "Boundary Mask " + this.historyRecordCursorIndex.ToString();
				historyRecord.boundaryMeshMaskTexture.Create();
			}
		}
		else
		{
			historyRecord.boundaryMeshMaskTexture = null;
		}
	}

	
	protected OVRSandwichComposition.HistoryRecord GetHistoryRecordForComposition()
	{
		float num = this.frameRealtime - OVRManager.instance.sandwichCompositionRenderLatency;
		int num2 = this.historyRecordCursorIndex;
		int num3 = num2 - 1;
		if (num3 < 0)
		{
			num3 = this.historyRecordCount - 1;
		}
		while (num3 != this.historyRecordCursorIndex)
		{
			if (this.historyRecordArray[num3].timestamp <= num)
			{
				float num4 = this.historyRecordArray[num2].timestamp - num;
				float num5 = num - this.historyRecordArray[num3].timestamp;
				return (num4 > num5) ? this.historyRecordArray[num3] : this.historyRecordArray[num2];
			}
			num2 = num3;
			num3 = num2 - 1;
			if (num3 < 0)
			{
				num3 = this.historyRecordCount - 1;
			}
		}
		return this.historyRecordArray[num2];
	}

	
	protected void RefreshCameraPoses(float fovY, float aspect, OVRPose pose)
	{
		Camera[] array = new Camera[]
		{
			this.fgCamera,
			this.bgCamera,
			this.compositionCamera
		};
		foreach (Camera camera in array)
		{
			camera.fieldOfView = fovY;
			camera.aspect = aspect;
			camera.transform.FromOVRPose(pose, false);
		}
	}

	
	public float frameRealtime;

	
	public Camera fgCamera;

	
	public Camera bgCamera;

	
	public readonly int historyRecordCount = 8;

	
	public readonly OVRSandwichComposition.HistoryRecord[] historyRecordArray;

	
	public int historyRecordCursorIndex;

	
	public GameObject cameraProxyPlane;

	
	public Camera compositionCamera;

	
	public OVRSandwichComposition.OVRSandwichCompositionManager compositionManager;

	
	private int _cameraFramePlaneLayer = -1;

	
	public class HistoryRecord
	{
		
		public float timestamp = float.MinValue;

		
		public RenderTexture fgRenderTexture;

		
		public RenderTexture bgRenderTexture;

		
		public RenderTexture boundaryMeshMaskTexture;
	}

	
	public class OVRSandwichCompositionManager : MonoBehaviour
	{
		
		private void Start()
		{
			Shader shader = Shader.Find("Oculus/UnlitTransparent");
			if (shader == null)
			{
				Debug.LogError("Unable to create transparent shader");
				return;
			}
			this.alphaBlendMaterial = new Material(shader);
		}

		
		private void OnPreRender()
		{
			if (this.fgTexture == null || this.bgTexture == null || this.alphaBlendMaterial == null)
			{
				Debug.LogError("OVRSandwichCompositionManager has not setup properly");
				return;
			}
			Graphics.Blit(this.bgTexture, RenderTexture.active);
		}

		
		private void OnPostRender()
		{
			if (this.fgTexture == null || this.bgTexture == null || this.alphaBlendMaterial == null)
			{
				Debug.LogError("OVRSandwichCompositionManager has not setup properly");
				return;
			}
			Graphics.Blit(this.fgTexture, RenderTexture.active, this.alphaBlendMaterial);
		}

		
		public RenderTexture fgTexture;

		
		public RenderTexture bgTexture;

		
		public Material alphaBlendMaterial;
	}
}
