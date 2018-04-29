using System;
using UnityEngine;
using UnityEngine.Rendering;


public abstract class OVRCameraComposition : OVRComposition
{
	
	protected OVRCameraComposition(OVRManager.CameraDevice inCameraDevice, bool inUseDynamicLighting, OVRManager.DepthQuality depthQuality)
	{
		this.cameraDevice = OVRCompositionUtil.ConvertCameraDevice(inCameraDevice);
		this.hasCameraDeviceOpened = false;
		this.useDynamicLighting = inUseDynamicLighting;
		bool flag = OVRPlugin.DoesCameraDeviceSupportDepth(this.cameraDevice);
		if (this.useDynamicLighting && !flag)
		{
			Debug.LogWarning("The camera device doesn't support depth. The result of dynamic lighting might not be correct");
		}
		if (OVRPlugin.IsCameraDeviceAvailable(this.cameraDevice))
		{
			OVRPlugin.CameraExtrinsics cameraExtrinsics;
			OVRPlugin.CameraIntrinsics cameraIntrinsics;
			if (OVRPlugin.GetExternalCameraCount() > 0 && OVRPlugin.GetMixedRealityCameraInfo(0, out cameraExtrinsics, out cameraIntrinsics))
			{
				OVRPlugin.SetCameraDevicePreferredColorFrameSize(this.cameraDevice, cameraIntrinsics.ImageSensorPixelResolution.w, cameraIntrinsics.ImageSensorPixelResolution.h);
			}
			if (this.useDynamicLighting)
			{
				OVRPlugin.SetCameraDeviceDepthSensingMode(this.cameraDevice, OVRPlugin.CameraDeviceDepthSensingMode.Fill);
				OVRPlugin.CameraDeviceDepthQuality depthQuality2 = OVRPlugin.CameraDeviceDepthQuality.Medium;
				if (depthQuality == OVRManager.DepthQuality.Low)
				{
					depthQuality2 = OVRPlugin.CameraDeviceDepthQuality.Low;
				}
				else if (depthQuality == OVRManager.DepthQuality.Medium)
				{
					depthQuality2 = OVRPlugin.CameraDeviceDepthQuality.Medium;
				}
				else if (depthQuality == OVRManager.DepthQuality.High)
				{
					depthQuality2 = OVRPlugin.CameraDeviceDepthQuality.High;
				}
				else
				{
					Debug.LogWarning("Unknown depth quality");
				}
				OVRPlugin.SetCameraDevicePreferredDepthQuality(this.cameraDevice, depthQuality2);
			}
			OVRPlugin.OpenCameraDevice(this.cameraDevice);
			if (OVRPlugin.HasCameraDeviceOpened(this.cameraDevice))
			{
				this.hasCameraDeviceOpened = true;
			}
		}
	}

	
	public override void Cleanup()
	{
		OVRCompositionUtil.SafeDestroy(ref this.cameraFramePlaneObject);
		if (this.hasCameraDeviceOpened)
		{
			OVRPlugin.CloseCameraDevice(this.cameraDevice);
		}
	}

	
	public override void RecenterPose()
	{
		this.boundaryMesh = null;
	}

	
	protected void CreateCameraFramePlaneObject(GameObject parentObject, Camera mixedRealityCamera, bool useDynamicLighting)
	{
		this.cameraFramePlaneObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
		this.cameraFramePlaneObject.name = "MRCameraFrame";
		this.cameraFramePlaneObject.transform.parent = parentObject.transform;
		this.cameraFramePlaneObject.GetComponent<Collider>().enabled = false;
		this.cameraFramePlaneObject.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
		Material material = new Material(Shader.Find((!useDynamicLighting) ? "Oculus/OVRMRCameraFrame" : "Oculus/OVRMRCameraFrameLit"));
		this.cameraFramePlaneObject.GetComponent<MeshRenderer>().material = material;
		material.SetColor("_Color", Color.white);
		material.SetFloat("_Visible", 0f);
		this.cameraFramePlaneObject.transform.localScale = new Vector3(4f, 4f, 4f);
		this.cameraFramePlaneObject.SetActive(true);
		OVRCameraComposition.OVRCameraFrameCompositionManager ovrcameraFrameCompositionManager = mixedRealityCamera.gameObject.AddComponent<OVRCameraComposition.OVRCameraFrameCompositionManager>();
		ovrcameraFrameCompositionManager.cameraFrameGameObj = this.cameraFramePlaneObject;
		ovrcameraFrameCompositionManager.composition = this;
	}

	
	protected void UpdateCameraFramePlaneObject(Camera mainCamera, Camera mixedRealityCamera, RenderTexture boundaryMeshMaskTexture)
	{
		bool flag = false;
		Material material = this.cameraFramePlaneObject.GetComponent<MeshRenderer>().material;
		Texture2D texture2D = Texture2D.blackTexture;
		Texture2D value = Texture2D.whiteTexture;
		if (OVRPlugin.IsCameraDeviceColorFrameAvailable(this.cameraDevice))
		{
			texture2D = OVRPlugin.GetCameraDeviceColorFrameTexture(this.cameraDevice);
		}
		else
		{
			Debug.LogWarning("Camera: color frame not ready");
			flag = true;
		}
		bool flag2 = OVRPlugin.DoesCameraDeviceSupportDepth(this.cameraDevice);
		if (this.useDynamicLighting && flag2)
		{
			if (OVRPlugin.IsCameraDeviceDepthFrameAvailable(this.cameraDevice))
			{
				value = OVRPlugin.GetCameraDeviceDepthFrameTexture(this.cameraDevice);
			}
			else
			{
				Debug.LogWarning("Camera: depth frame not ready");
				flag = true;
			}
		}
		if (!flag)
		{
			Vector3 rhs = mainCamera.transform.position - mixedRealityCamera.transform.position;
			float num = Vector3.Dot(mixedRealityCamera.transform.forward, rhs);
			this.cameraFramePlaneDistance = num;
			this.cameraFramePlaneObject.transform.position = mixedRealityCamera.transform.position + mixedRealityCamera.transform.forward * num;
			this.cameraFramePlaneObject.transform.rotation = mixedRealityCamera.transform.rotation;
			float num2 = Mathf.Tan(mixedRealityCamera.fieldOfView * 0.0174532924f * 0.5f);
			this.cameraFramePlaneObject.transform.localScale = new Vector3(num * mixedRealityCamera.aspect * num2 * 2f, num * num2 * 2f, 1f);
			float num3 = num * num2 * 2f;
			float x = num3 * mixedRealityCamera.aspect;
			float maxValue = float.MaxValue;
			this.cameraRig = null;
			if (OVRManager.instance.virtualGreenScreenType != OVRManager.VirtualGreenScreenType.Off)
			{
				this.cameraRig = mainCamera.GetComponentInParent<OVRCameraRig>();
				if (this.cameraRig != null && this.cameraRig.centerEyeAnchor == null)
				{
					this.cameraRig = null;
				}
				this.RefreshBoundaryMesh(mixedRealityCamera, out maxValue);
			}
			material.mainTexture = texture2D;
			material.SetTexture("_DepthTex", value);
			material.SetVector("_FlipParams", new Vector4((!OVRManager.instance.flipCameraFrameHorizontally) ? 0f : 1f, (!OVRManager.instance.flipCameraFrameVertically) ? 0f : 1f, 0f, 0f));
			material.SetColor("_ChromaKeyColor", OVRManager.instance.chromaKeyColor);
			material.SetFloat("_ChromaKeySimilarity", OVRManager.instance.chromaKeySimilarity);
			material.SetFloat("_ChromaKeySmoothRange", OVRManager.instance.chromaKeySmoothRange);
			material.SetFloat("_ChromaKeySpillRange", OVRManager.instance.chromaKeySpillRange);
			material.SetVector("_TextureDimension", new Vector4((float)texture2D.width, (float)texture2D.height, 1f / (float)texture2D.width, 1f / (float)texture2D.height));
			material.SetVector("_TextureWorldSize", new Vector4(x, num3, 0f, 0f));
			material.SetFloat("_SmoothFactor", OVRManager.instance.dynamicLightingSmoothFactor);
			material.SetFloat("_DepthVariationClamp", OVRManager.instance.dynamicLightingDepthVariationClampingValue);
			material.SetFloat("_CullingDistance", maxValue);
			if (OVRManager.instance.virtualGreenScreenType == OVRManager.VirtualGreenScreenType.Off || this.boundaryMesh == null || boundaryMeshMaskTexture == null)
			{
				material.SetTexture("_MaskTex", Texture2D.whiteTexture);
			}
			else if (this.cameraRig == null)
			{
				if (!this.nullcameraRigWarningDisplayed)
				{
					Debug.LogWarning("Could not find the OVRCameraRig/CenterEyeAnchor object. Please check if the OVRCameraRig has been setup properly. The virtual green screen has been temporarily disabled");
					this.nullcameraRigWarningDisplayed = true;
				}
				material.SetTexture("_MaskTex", Texture2D.whiteTexture);
			}
			else
			{
				if (this.nullcameraRigWarningDisplayed)
				{
					Debug.Log("OVRCameraRig/CenterEyeAnchor object found. Virtual green screen is activated");
					this.nullcameraRigWarningDisplayed = false;
				}
				material.SetTexture("_MaskTex", boundaryMeshMaskTexture);
			}
		}
	}

	
	protected void RefreshBoundaryMesh(Camera camera, out float cullingDistance)
	{
		float num = (!OVRManager.instance.virtualGreenScreenApplyDepthCulling) ? float.PositiveInfinity : OVRManager.instance.virtualGreenScreenDepthTolerance;
		cullingDistance = OVRCompositionUtil.GetMaximumBoundaryDistance(camera, OVRCompositionUtil.ToBoundaryType(OVRManager.instance.virtualGreenScreenType)) + num;
		if (this.boundaryMesh == null || this.boundaryMeshType != OVRManager.instance.virtualGreenScreenType || this.boundaryMeshTopY != OVRManager.instance.virtualGreenScreenTopY || this.boundaryMeshBottomY != OVRManager.instance.virtualGreenScreenBottomY)
		{
			this.boundaryMeshTopY = OVRManager.instance.virtualGreenScreenTopY;
			this.boundaryMeshBottomY = OVRManager.instance.virtualGreenScreenBottomY;
			this.boundaryMesh = OVRCompositionUtil.BuildBoundaryMesh(OVRCompositionUtil.ToBoundaryType(OVRManager.instance.virtualGreenScreenType), this.boundaryMeshTopY, this.boundaryMeshBottomY);
			this.boundaryMeshType = OVRManager.instance.virtualGreenScreenType;
		}
	}

	
	protected GameObject cameraFramePlaneObject;

	
	protected float cameraFramePlaneDistance;

	
	protected readonly bool hasCameraDeviceOpened;

	
	protected readonly bool useDynamicLighting;

	
	internal readonly OVRPlugin.CameraDevice cameraDevice = OVRPlugin.CameraDevice.WebCamera0;

	
	private OVRCameraRig cameraRig;

	
	private Mesh boundaryMesh;

	
	private float boundaryMeshTopY;

	
	private float boundaryMeshBottomY;

	
	private OVRManager.VirtualGreenScreenType boundaryMeshType;

	
	private bool nullcameraRigWarningDisplayed;

	
	public class OVRCameraFrameCompositionManager : MonoBehaviour
	{
		
		private void Start()
		{
			Shader shader = Shader.Find("Oculus/Unlit");
			if (!shader)
			{
				Debug.LogError("Oculus/Unlit shader does not exist");
				return;
			}
			this.whiteMaterial = new Material(shader);
			this.whiteMaterial.color = Color.white;
		}

		
		private void OnPreRender()
		{
			if (OVRManager.instance.virtualGreenScreenType != OVRManager.VirtualGreenScreenType.Off && this.boundaryMeshMaskTexture != null && this.composition.boundaryMesh != null)
			{
				RenderTexture active = RenderTexture.active;
				RenderTexture.active = this.boundaryMeshMaskTexture;
				GL.PushMatrix();
				GL.LoadProjectionMatrix(base.GetComponent<Camera>().projectionMatrix);
				GL.Clear(false, true, Color.black);
				for (int i = 0; i < this.whiteMaterial.passCount; i++)
				{
					if (this.whiteMaterial.SetPass(i))
					{
						Graphics.DrawMeshNow(this.composition.boundaryMesh, this.composition.cameraRig.ComputeTrackReferenceMatrix());
					}
				}
				GL.PopMatrix();
				RenderTexture.active = active;
			}
			if (this.cameraFrameGameObj)
			{
				if (this.cameraFrameMaterial == null)
				{
					this.cameraFrameMaterial = this.cameraFrameGameObj.GetComponent<MeshRenderer>().material;
				}
				this.cameraFrameMaterial.SetFloat("_Visible", 1f);
			}
		}

		
		private void OnPostRender()
		{
			if (this.cameraFrameGameObj)
			{
				this.cameraFrameMaterial.SetFloat("_Visible", 0f);
			}
		}

		
		public GameObject cameraFrameGameObj;

		
		public OVRCameraComposition composition;

		
		public RenderTexture boundaryMeshMaskTexture;

		
		private Material cameraFrameMaterial;

		
		private Material whiteMaterial;
	}
}
