using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VR;

public class OVROverlay : MonoBehaviour
{
	public void OverrideOverlayTextureInfo(Texture srcTexture, IntPtr nativePtr, VRNode node)
	{
		int num = (node != VRNode.RightEye) ? 0 : 1;
		if (this.textures.Length <= num)
		{
			return;
		}
		this.textures[num] = srcTexture;
		this.texturePtrs[num] = nativePtr;
		this.isOverridePending = true;
	}

	private int texturesPerStage
	{
		get
		{
			return (this.layout != OVRPlugin.LayerLayout.Stereo) ? 1 : 2;
		}
	}

	private bool CreateLayer(int mipLevels, int sampleCount, OVRPlugin.EyeTextureFormat etFormat, int flags, OVRPlugin.Sizei size, OVRPlugin.OverlayShape shape)
	{
		if (!this.layerIdHandle.IsAllocated || this.layerIdPtr == IntPtr.Zero)
		{
			this.layerIdHandle = GCHandle.Alloc(this.layerId, GCHandleType.Pinned);
			this.layerIdPtr = this.layerIdHandle.AddrOfPinnedObject();
		}
		if (this.layerIndex == -1)
		{
			for (int i = 0; i < 15; i++)
			{
				if (OVROverlay.instances[i] == null || OVROverlay.instances[i] == this)
				{
					this.layerIndex = i;
					OVROverlay.instances[i] = this;
					break;
				}
			}
		}
		if (!this.isOverridePending && this.layerDesc.MipLevels == mipLevels && this.layerDesc.SampleCount == sampleCount && this.layerDesc.Format == etFormat && this.layerDesc.LayerFlags == flags && this.layerDesc.TextureSize.Equals(size) && this.layerDesc.Shape == shape)
		{
			return false;
		}
		OVRPlugin.LayerDesc desc = OVRPlugin.CalculateLayerDesc(shape, this.layout, size, mipLevels, sampleCount, etFormat, flags);
		OVRPlugin.EnqueueSetupLayer(desc, this.layerIdPtr);
		this.layerId = (int)this.layerIdHandle.Target;
		if (this.layerId > 0)
		{
			this.layerDesc = desc;
			this.stageCount = OVRPlugin.GetLayerTextureStageCount(this.layerId);
		}
		this.isOverridePending = false;
		return true;
	}

	private bool CreateLayerTextures(bool useMipmaps, OVRPlugin.Sizei size, bool isHdr)
	{
		bool result = false;
		if (this.stageCount <= 0)
		{
			return false;
		}
		if (this.layerTextures == null)
		{
			this.frameIndex = 0;
			this.layerTextures = new OVROverlay.LayerTexture[this.texturesPerStage];
		}
		for (int i = 0; i < this.texturesPerStage; i++)
		{
			if (this.layerTextures[i].swapChain == null)
			{
				this.layerTextures[i].swapChain = new Texture[this.stageCount];
			}
			if (this.layerTextures[i].swapChainPtr == null)
			{
				this.layerTextures[i].swapChainPtr = new IntPtr[this.stageCount];
			}
			for (int j = 0; j < this.stageCount; j++)
			{
				Texture texture = this.layerTextures[i].swapChain[j];
				IntPtr intPtr = this.layerTextures[i].swapChainPtr[j];
				if (!(texture != null) || !(intPtr != IntPtr.Zero))
				{
					if (intPtr == IntPtr.Zero)
					{
						intPtr = OVRPlugin.GetLayerTexture(this.layerId, j, (OVRPlugin.Eye)i);
					}
					if (!(intPtr == IntPtr.Zero))
					{
						TextureFormat format = (!isHdr) ? TextureFormat.RGBA32 : TextureFormat.RGBAHalf;
						if (this.currentOverlayShape != OVROverlay.OverlayShape.Cubemap && this.currentOverlayShape != OVROverlay.OverlayShape.OffcenterCubemap)
						{
							texture = Texture2D.CreateExternalTexture(size.w, size.h, format, useMipmaps, true, intPtr);
						}
						this.layerTextures[i].swapChain[j] = texture;
						this.layerTextures[i].swapChainPtr[j] = intPtr;
						result = true;
					}
				}
			}
		}
		return result;
	}

	private void DestroyLayerTextures()
	{
		int num = 0;
		while (this.layerTextures != null && num < this.texturesPerStage)
		{
			if (this.layerTextures[num].swapChain != null)
			{
				for (int i = 0; i < this.stageCount; i++)
				{
					UnityEngine.Object.DestroyImmediate(this.layerTextures[num].swapChain[i]);
				}
			}
			num++;
		}
		this.layerTextures = null;
	}

	private void DestroyLayer()
	{
		if (this.layerIndex != -1)
		{
			OVRPlugin.EnqueueSubmitLayer(true, false, IntPtr.Zero, IntPtr.Zero, -1, 0, OVRPose.identity.ToPosef(), Vector3.one.ToVector3f(), this.layerIndex, (OVRPlugin.OverlayShape)this.prevOverlayShape);
			OVROverlay.instances[this.layerIndex] = null;
			this.layerIndex = -1;
		}
		if (this.layerIdPtr != IntPtr.Zero)
		{
			OVRPlugin.EnqueueDestroyLayer(this.layerIdPtr);
			this.layerIdPtr = IntPtr.Zero;
			this.layerIdHandle.Free();
			this.layerId = 0;
		}
		this.layerDesc = default(OVRPlugin.LayerDesc);
	}

	private bool LatchLayerTextures()
	{
		for (int i = 0; i < this.texturesPerStage; i++)
		{
			if ((this.textures[i] != this.layerTextures[i].appTexture || this.layerTextures[i].appTexturePtr == IntPtr.Zero) && this.textures[i] != null)
			{
				RenderTexture renderTexture = this.textures[i] as RenderTexture;
				if (renderTexture && !renderTexture.IsCreated())
				{
					renderTexture.Create();
				}
				this.layerTextures[i].appTexturePtr = ((!(this.texturePtrs[i] != IntPtr.Zero)) ? this.textures[i].GetNativeTexturePtr() : this.texturePtrs[i]);
				if (this.layerTextures[i].appTexturePtr != IntPtr.Zero)
				{
					this.layerTextures[i].appTexture = this.textures[i];
				}
			}
			if (this.currentOverlayShape == OVROverlay.OverlayShape.Cubemap && this.textures[i] as Cubemap == null)
			{
				Debug.LogError("Need Cubemap texture for cube map overlay");
				return false;
			}
		}
		if (this.currentOverlayShape == OVROverlay.OverlayShape.OffcenterCubemap)
		{
			Debug.LogWarning("Overlay shape " + this.currentOverlayShape + " is not supported on current platform");
			return false;
		}
		return !(this.layerTextures[0].appTexture == null) && !(this.layerTextures[0].appTexturePtr == IntPtr.Zero);
	}

	private OVRPlugin.LayerDesc GetCurrentLayerDesc()
	{
		OVRPlugin.LayerDesc result = new OVRPlugin.LayerDesc
		{
			Format = OVRPlugin.EyeTextureFormat.Default,
			LayerFlags = 8,
			Layout = this.layout,
			MipLevels = 1,
			SampleCount = 1,
			Shape = (OVRPlugin.OverlayShape)this.currentOverlayShape,
			TextureSize = new OVRPlugin.Sizei
			{
				w = this.textures[0].width,
				h = this.textures[0].height
			}
		};
		Texture2D texture2D = this.textures[0] as Texture2D;
		if (texture2D != null)
		{
			if (texture2D.format == TextureFormat.RGBAHalf || texture2D.format == TextureFormat.RGBAFloat)
			{
				result.Format = OVRPlugin.EyeTextureFormat.R16G16B16A16_FP;
			}
			result.MipLevels = texture2D.mipmapCount;
		}
		Cubemap cubemap = this.textures[0] as Cubemap;
		if (cubemap != null)
		{
			if (cubemap.format == TextureFormat.RGBAHalf || cubemap.format == TextureFormat.RGBAFloat)
			{
				result.Format = OVRPlugin.EyeTextureFormat.R16G16B16A16_FP;
			}
			result.MipLevels = cubemap.mipmapCount;
		}
		RenderTexture renderTexture = this.textures[0] as RenderTexture;
		if (renderTexture != null)
		{
			result.SampleCount = renderTexture.antiAliasing;
			if (renderTexture.format == RenderTextureFormat.ARGBHalf || renderTexture.format == RenderTextureFormat.ARGBFloat || renderTexture.format == RenderTextureFormat.RGB111110Float)
			{
				result.Format = OVRPlugin.EyeTextureFormat.R16G16B16A16_FP;
			}
		}
		return result;
	}

	private bool PopulateLayer(int mipLevels, bool isHdr, OVRPlugin.Sizei size, int sampleCount)
	{
		bool result = false;
		RenderTextureFormat format = (!isHdr) ? RenderTextureFormat.ARGB32 : RenderTextureFormat.ARGBHalf;
		for (int i = 0; i < this.texturesPerStage; i++)
		{
			int dstElement = (this.layout != OVRPlugin.LayerLayout.Array) ? 0 : i;
			int num = this.frameIndex % this.stageCount;
			Texture texture = this.layerTextures[i].swapChain[num];
			if (!(texture == null))
			{
				for (int j = 0; j < mipLevels; j++)
				{
					int num2 = size.w >> j;
					if (num2 < 1)
					{
						num2 = 1;
					}
					int num3 = size.h >> j;
					if (num3 < 1)
					{
						num3 = 1;
					}
					RenderTexture temporary = RenderTexture.GetTemporary(num2, num3, 0, format, RenderTextureReadWrite.Linear, sampleCount);
					if (!temporary.IsCreated())
					{
						temporary.Create();
					}
					temporary.DiscardContents();
					RenderTexture renderTexture = this.textures[i] as RenderTexture;
					bool flag = isHdr || QualitySettings.activeColorSpace == ColorSpace.Linear;
					flag |= (renderTexture != null && renderTexture.sRGB);
					if (this.currentOverlayShape != OVROverlay.OverlayShape.Cubemap && this.currentOverlayShape != OVROverlay.OverlayShape.OffcenterCubemap)
					{
						OVROverlay.tex2DMaterial.SetInt("_linearToSrgb", (isHdr || !flag) ? 0 : 1);
						OVROverlay.tex2DMaterial.SetInt("_premultiply", 1);
						Graphics.Blit(this.textures[i], temporary, OVROverlay.tex2DMaterial);
						Graphics.CopyTexture(temporary, 0, 0, texture, dstElement, j);
					}
					RenderTexture.ReleaseTemporary(temporary);
					result = true;
				}
			}
		}
		return result;
	}

	private bool SubmitLayer(bool overlay, bool headLocked, OVRPose pose, Vector3 scale)
	{
		int num = (this.texturesPerStage < 2) ? 0 : 1;
		bool result = OVRPlugin.EnqueueSubmitLayer(overlay, headLocked, this.layerTextures[0].appTexturePtr, this.layerTextures[num].appTexturePtr, this.layerId, this.frameIndex, pose.flipZ().ToPosef(), scale.ToVector3f(), this.layerIndex, (OVRPlugin.OverlayShape)this.currentOverlayShape);
		if (this.isDynamic)
		{
			this.frameIndex++;
		}
		this.prevOverlayShape = this.currentOverlayShape;
		return result;
	}

	private void Awake()
	{
		Debug.Log("Overlay Awake");
		if (OVROverlay.tex2DMaterial == null)
		{
			OVROverlay.tex2DMaterial = new Material(Shader.Find("Oculus/Texture2D Blit"));
		}
		if (OVROverlay.cubeMaterial == null)
		{
			OVROverlay.cubeMaterial = new Material(Shader.Find("Oculus/Cubemap Blit"));
		}
		this.rend = base.GetComponent<Renderer>();
		if (this.textures.Length == 0)
		{
			this.textures = new Texture[1];
		}
		if (this.rend != null && this.textures[0] == null)
		{
			this.textures[0] = this.rend.material.mainTexture;
		}
	}

	private void OnEnable()
	{
		if (!OVRManager.isHmdPresent)
		{
			base.enabled = false;
			return;
		}
	}

	private void OnDisable()
	{
		this.DestroyLayerTextures();
		this.DestroyLayer();
	}

	private void OnDestroy()
	{
		this.DestroyLayerTextures();
		this.DestroyLayer();
	}

	private bool ComputeSubmit(ref OVRPose pose, ref Vector3 scale, ref bool overlay, ref bool headLocked)
	{
		Camera main = Camera.main;
		overlay = (this.currentOverlayType == OVROverlay.OverlayType.Overlay);
		headLocked = false;
		Transform transform = base.transform;
		while (transform != null && !headLocked)
		{
			headLocked |= (transform == main.transform);
			transform = transform.parent;
		}
		pose = ((!headLocked) ? base.transform.ToTrackingSpacePose(main) : base.transform.ToHeadSpacePose(main));
		scale = base.transform.lossyScale;
		for (int i = 0; i < 3; i++)
		{
			int index;
			scale[index = i] = scale[index] / main.transform.lossyScale[i];
		}
		if (this.currentOverlayShape == OVROverlay.OverlayShape.Cubemap)
		{
			pose.position = main.transform.position;
		}
		if (this.currentOverlayShape == OVROverlay.OverlayShape.OffcenterCubemap)
		{
			pose.position = base.transform.position;
			if (pose.position.magnitude > 1f)
			{
				Debug.LogWarning("Your cube map center offset's magnitude is greater than 1, which will cause some cube map pixel always invisible .");
				return false;
			}
		}
		if (this.currentOverlayShape == OVROverlay.OverlayShape.Cylinder)
		{
			float num = scale.x / scale.z / 3.14159274f * 180f;
			if (num > 180f)
			{
				Debug.LogWarning("Cylinder overlay's arc angle has to be below 180 degree, current arc angle is " + num + " degree.");
				return false;
			}
		}
		return true;
	}

	private void LateUpdate()
	{
		if (this.currentOverlayType == OVROverlay.OverlayType.None || this.textures.Length < this.texturesPerStage || this.textures[0] == null)
		{
			return;
		}
		if (Time.frameCount <= this.prevFrameIndex)
		{
			return;
		}
		this.prevFrameIndex = Time.frameCount;
		OVRPose identity = OVRPose.identity;
		Vector3 one = Vector3.one;
		bool overlay = false;
		bool headLocked = false;
		if (!this.ComputeSubmit(ref identity, ref one, ref overlay, ref headLocked))
		{
			return;
		}
		OVRPlugin.LayerDesc currentLayerDesc = this.GetCurrentLayerDesc();
		bool isHdr = currentLayerDesc.Format == OVRPlugin.EyeTextureFormat.R16G16B16A16_FP;
		bool flag = this.CreateLayer(currentLayerDesc.MipLevels, currentLayerDesc.SampleCount, currentLayerDesc.Format, currentLayerDesc.LayerFlags, currentLayerDesc.TextureSize, currentLayerDesc.Shape);
		if (this.layerIndex == -1 || this.layerId <= 0)
		{
			return;
		}
		bool useMipmaps = currentLayerDesc.MipLevels > 1;
		flag |= this.CreateLayerTextures(useMipmaps, currentLayerDesc.TextureSize, isHdr);
		if (this.layerTextures[0].appTexture as RenderTexture != null)
		{
			this.isDynamic = true;
		}
		if (!this.LatchLayerTextures())
		{
			return;
		}
		if (!this.PopulateLayer(currentLayerDesc.MipLevels, isHdr, currentLayerDesc.TextureSize, currentLayerDesc.SampleCount))
		{
			return;
		}
		bool flag2 = this.SubmitLayer(overlay, headLocked, identity, one);
		if (this.rend)
		{
			this.rend.enabled = !flag2;
		}
	}

	public OVROverlay.OverlayType currentOverlayType = OVROverlay.OverlayType.Overlay;

	public bool isDynamic;

	public OVROverlay.OverlayShape currentOverlayShape;

	private OVROverlay.OverlayShape prevOverlayShape;

	public Texture[] textures = new Texture[2];

	protected IntPtr[] texturePtrs = new IntPtr[]
	{
		IntPtr.Zero,
		IntPtr.Zero
	};

	protected bool isOverridePending;

	internal const int maxInstances = 15;

	internal static OVROverlay[] instances = new OVROverlay[15];

	private static Material tex2DMaterial;

	private static Material cubeMaterial;

	private OVRPlugin.LayerLayout layout = OVRPlugin.LayerLayout.Mono;

	private OVROverlay.LayerTexture[] layerTextures;

	private OVRPlugin.LayerDesc layerDesc;

	private int stageCount = -1;

	private int layerIndex = -1;

	private int layerId;

	private GCHandle layerIdHandle;

	private IntPtr layerIdPtr = IntPtr.Zero;

	private int frameIndex;

	private int prevFrameIndex = -1;

	private Renderer rend;

	public enum OverlayShape
	{
		Quad,
		Cylinder,
		Cubemap,
		OffcenterCubemap = 4,
		Equirect
	}

	public enum OverlayType
	{
		None,
		Underlay,
		Overlay
	}

	private struct LayerTexture
	{
		public Texture appTexture;

		public IntPtr appTexturePtr;

		public Texture[] swapChain;

		public IntPtr[] swapChainPtr;
	}
}
