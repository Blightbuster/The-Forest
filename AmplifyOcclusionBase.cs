using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


[AddComponentMenu("")]
public class AmplifyOcclusionBase : MonoBehaviour
{
	
	private bool CheckParamsChanged()
	{
		return this.prevScreenWidth != this.m_camera.pixelWidth || this.prevScreenHeight != this.m_camera.pixelHeight || this.prevHDR != this.m_camera.allowHDR || this.prevApplyMethod != this.ApplyMethod || this.prevSampleCount != this.SampleCount || this.prevPerPixelNormals != this.PerPixelNormals || this.prevCacheAware != this.CacheAware || this.prevDownscale != this.Downsample || this.prevFadeEnabled != this.FadeEnabled || this.prevFadeToIntensity != this.FadeToIntensity || this.prevFadeToRadius != this.FadeToRadius || this.prevFadeToPowerExponent != this.FadeToPowerExponent || this.prevFadeStart != this.FadeStart || this.prevFadeLength != this.FadeLength || this.prevBlurEnabled != this.BlurEnabled || this.prevBlurRadius != this.BlurRadius || this.prevBlurPasses != this.BlurPasses;
	}

	
	private void UpdateParams()
	{
		this.prevScreenWidth = this.m_camera.pixelWidth;
		this.prevScreenHeight = this.m_camera.pixelHeight;
		this.prevHDR = this.m_camera.allowHDR;
		this.prevApplyMethod = this.ApplyMethod;
		this.prevSampleCount = this.SampleCount;
		this.prevPerPixelNormals = this.PerPixelNormals;
		this.prevCacheAware = this.CacheAware;
		this.prevDownscale = this.Downsample;
		this.prevFadeEnabled = this.FadeEnabled;
		this.prevFadeToIntensity = this.FadeToIntensity;
		this.prevFadeToRadius = this.FadeToRadius;
		this.prevFadeToPowerExponent = this.FadeToPowerExponent;
		this.prevFadeStart = this.FadeStart;
		this.prevFadeLength = this.FadeLength;
		this.prevBlurEnabled = this.BlurEnabled;
		this.prevBlurRadius = this.BlurRadius;
		this.prevBlurPasses = this.BlurPasses;
	}

	
	private void Warmup()
	{
		this.CheckMaterial();
		this.CheckRandomData();
		this.m_depthLayerRT = new int[16];
		this.m_normalLayerRT = new int[16];
		this.m_occlusionLayerRT = new int[16];
		this.m_mrtCount = Mathf.Min(SystemInfo.supportedRenderTargetCount, 4);
		this.m_layerOffsetNames = new string[this.m_mrtCount];
		this.m_layerRandomNames = new string[this.m_mrtCount];
		for (int i = 0; i < this.m_mrtCount; i++)
		{
			this.m_layerOffsetNames[i] = "_AO_LayerOffset" + i;
			this.m_layerRandomNames[i] = "_AO_LayerRandom" + i;
		}
		this.m_layerDepthNames = new string[16];
		this.m_layerNormalNames = new string[16];
		this.m_layerOcclusionNames = new string[16];
		for (int j = 0; j < 16; j++)
		{
			this.m_layerDepthNames[j] = "_AO_DepthLayer" + j;
			this.m_layerNormalNames[j] = "_AO_NormalLayer" + j;
			this.m_layerOcclusionNames[j] = "_AO_OcclusionLayer" + j;
		}
		this.m_depthTargets = new RenderTargetIdentifier[this.m_mrtCount];
		this.m_normalTargets = new RenderTargetIdentifier[this.m_mrtCount];
		int mrtCount = this.m_mrtCount;
		if (mrtCount != 4)
		{
			this.m_deinterleaveDepthPass = 5;
			this.m_deinterleaveNormalPass = 6;
		}
		else
		{
			this.m_deinterleaveDepthPass = 10;
			this.m_deinterleaveNormalPass = 11;
		}
		this.m_applyDeferredTargets = new RenderTargetIdentifier[2];
		if (this.m_blitMesh != null)
		{
			UnityEngine.Object.DestroyImmediate(this.m_blitMesh);
		}
		this.m_blitMesh = new Mesh();
		this.m_blitMesh.vertices = new Vector3[]
		{
			new Vector3(0f, 0f, 0f),
			new Vector3(0f, 1f, 0f),
			new Vector3(1f, 1f, 0f),
			new Vector3(1f, 0f, 0f)
		};
		this.m_blitMesh.uv = new Vector2[]
		{
			new Vector2(0f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f),
			new Vector2(1f, 0f)
		};
		this.m_blitMesh.triangles = new int[]
		{
			0,
			1,
			2,
			0,
			2,
			3
		};
	}

	
	private void Shutdown()
	{
		this.CommandBuffer_UnregisterAll();
		this.SafeReleaseRT(ref this.m_occlusionRT);
		if (this.m_occlusionMat != null)
		{
			UnityEngine.Object.DestroyImmediate(this.m_occlusionMat);
		}
		if (this.m_blurMat != null)
		{
			UnityEngine.Object.DestroyImmediate(this.m_blurMat);
		}
		if (this.m_copyMat != null)
		{
			UnityEngine.Object.DestroyImmediate(this.m_copyMat);
		}
		if (this.m_blitMesh != null)
		{
			UnityEngine.Object.DestroyImmediate(this.m_blitMesh);
		}
	}

	
	private bool CheckRenderTextureFormats()
	{
		if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB32) && SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
		{
			this.m_depthRTFormat = RenderTextureFormat.RFloat;
			if (!SystemInfo.SupportsRenderTextureFormat(this.m_depthRTFormat))
			{
				this.m_depthRTFormat = RenderTextureFormat.RHalf;
				if (!SystemInfo.SupportsRenderTextureFormat(this.m_depthRTFormat))
				{
					this.m_depthRTFormat = RenderTextureFormat.ARGBHalf;
				}
			}
			this.m_normalRTFormat = RenderTextureFormat.ARGB2101010;
			if (!SystemInfo.SupportsRenderTextureFormat(this.m_normalRTFormat))
			{
				this.m_normalRTFormat = RenderTextureFormat.ARGB32;
			}
			this.m_occlusionRTFormat = RenderTextureFormat.RGHalf;
			if (!SystemInfo.SupportsRenderTextureFormat(this.m_occlusionRTFormat))
			{
				this.m_occlusionRTFormat = RenderTextureFormat.RGFloat;
				if (!SystemInfo.SupportsRenderTextureFormat(this.m_occlusionRTFormat))
				{
					this.m_occlusionRTFormat = RenderTextureFormat.ARGBHalf;
				}
			}
			return true;
		}
		return false;
	}

	
	private void OnEnable()
	{
		if (!this.CheckRenderTextureFormats())
		{
			Debug.LogError("[AmplifyOcclusion] Target platform does not meet the minimum requirements for this effect to work properly.");
			base.enabled = false;
			return;
		}
		this.m_camera = base.GetComponent<Camera>();
		this.Warmup();
		this.CommandBuffer_UnregisterAll();
	}

	
	private void OnDisable()
	{
		this.Shutdown();
	}

	
	private void OnDestroy()
	{
		this.Shutdown();
	}

	
	private void Update()
	{
		if (this.m_camera.actualRenderingPath != RenderingPath.DeferredShading)
		{
			if (this.PerPixelNormals != AmplifyOcclusionBase.PerPixelNormalSource.None && this.PerPixelNormals != AmplifyOcclusionBase.PerPixelNormalSource.Camera)
			{
				this.PerPixelNormals = AmplifyOcclusionBase.PerPixelNormalSource.Camera;
				Debug.LogWarning("[AmplifyOcclusion] GBuffer Normals only available in Camera Deferred Shading mode. Switched to Camera source.");
			}
			if (this.ApplyMethod == AmplifyOcclusionBase.ApplicationMethod.Deferred)
			{
				this.ApplyMethod = AmplifyOcclusionBase.ApplicationMethod.PostEffect;
				Debug.LogWarning("[AmplifyOcclusion] Deferred Method requires a Deferred Shading path. Switching to Post Effect Method.");
			}
		}
		if (this.ApplyMethod == AmplifyOcclusionBase.ApplicationMethod.Deferred && this.PerPixelNormals == AmplifyOcclusionBase.PerPixelNormalSource.Camera)
		{
			this.PerPixelNormals = AmplifyOcclusionBase.PerPixelNormalSource.GBuffer;
			Debug.LogWarning("[AmplifyOcclusion] Camera Normals not supported for Deferred Method. Switching to GBuffer Normals.");
		}
		if ((this.m_camera.depthTextureMode & DepthTextureMode.Depth) == DepthTextureMode.None)
		{
			this.m_camera.depthTextureMode |= DepthTextureMode.Depth;
		}
		if (this.PerPixelNormals == AmplifyOcclusionBase.PerPixelNormalSource.Camera && (this.m_camera.depthTextureMode & DepthTextureMode.DepthNormals) == DepthTextureMode.None)
		{
			this.m_camera.depthTextureMode |= DepthTextureMode.DepthNormals;
		}
		this.CheckMaterial();
		this.CheckRandomData();
	}

	
	private void CheckMaterial()
	{
		if (this.m_occlusionMat == null)
		{
			this.m_occlusionMat = new Material(Shader.Find("Hidden/Amplify Occlusion/Occlusion"))
			{
				hideFlags = HideFlags.DontSave
			};
		}
		if (this.m_blurMat == null)
		{
			this.m_blurMat = new Material(Shader.Find("Hidden/Amplify Occlusion/Blur"))
			{
				hideFlags = HideFlags.DontSave
			};
		}
		if (this.m_copyMat == null)
		{
			this.m_copyMat = new Material(Shader.Find("Hidden/Amplify Occlusion/Copy"))
			{
				hideFlags = HideFlags.DontSave
			};
		}
	}

	
	private void CheckRandomData()
	{
		if (this.m_randomData == null)
		{
			this.m_randomData = AmplifyOcclusionBase.GenerateRandomizationData();
		}
		if (this.m_randomTex == null)
		{
			this.m_randomTex = Resources.Load<Texture2D>("Random4x4");
		}
	}

	
	public static Color[] GenerateRandomizationData()
	{
		Color[] array = new Color[16];
		int i = 0;
		int num = 0;
		while (i < 16)
		{
			float num2 = RandomTable.Values[num++];
			float b = RandomTable.Values[num++];
			float f = 6.28318548f * num2 / 8f;
			array[i].r = Mathf.Cos(f);
			array[i].g = Mathf.Sin(f);
			array[i].b = b;
			array[i].a = 0f;
			i++;
		}
		return array;
	}

	
	public static Texture2D GenerateRandomizationTexture(Color[] randomPixels)
	{
		Texture2D texture2D = new Texture2D(4, 4, TextureFormat.ARGB32, false, true)
		{
			hideFlags = HideFlags.DontSave
		};
		texture2D.name = "RandomTexture";
		texture2D.filterMode = FilterMode.Point;
		texture2D.wrapMode = TextureWrapMode.Repeat;
		texture2D.SetPixels(randomPixels);
		texture2D.Apply();
		return texture2D;
	}

	
	private RenderTexture SafeAllocateRT(string name, int width, int height, RenderTextureFormat format, RenderTextureReadWrite readWrite)
	{
		width = Mathf.Max(width, 1);
		height = Mathf.Max(height, 1);
		RenderTexture renderTexture = new RenderTexture(width, height, 0, format, readWrite)
		{
			hideFlags = HideFlags.DontSave
		};
		renderTexture.name = name;
		renderTexture.filterMode = FilterMode.Point;
		renderTexture.wrapMode = TextureWrapMode.Clamp;
		renderTexture.Create();
		return renderTexture;
	}

	
	private void SafeReleaseRT(ref RenderTexture rt)
	{
		if (rt != null)
		{
			RenderTexture.active = null;
			rt.Release();
			UnityEngine.Object.DestroyImmediate(rt);
			rt = null;
		}
	}

	
	private int SafeAllocateTemporaryRT(CommandBuffer cb, string propertyName, int width, int height, RenderTextureFormat format = RenderTextureFormat.Default, RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default, FilterMode filterMode = FilterMode.Point)
	{
		int num = Shader.PropertyToID(propertyName);
		cb.GetTemporaryRT(num, width, height, 0, filterMode, format, readWrite);
		return num;
	}

	
	private void SafeReleaseTemporaryRT(CommandBuffer cb, int id)
	{
		cb.ReleaseTemporaryRT(id);
	}

	
	private void SetBlitTarget(CommandBuffer cb, RenderTargetIdentifier[] targets, int targetWidth, int targetHeight)
	{
		cb.SetGlobalVector("_AO_Target_TexelSize", new Vector4(1f / (float)targetWidth, 1f / (float)targetHeight, (float)targetWidth, (float)targetHeight));
		cb.SetGlobalVector("_AO_Target_Position", Vector2.zero);
		cb.SetRenderTarget(targets, targets[0]);
	}

	
	private void SetBlitTarget(CommandBuffer cb, RenderTargetIdentifier target, int targetWidth, int targetHeight)
	{
		cb.SetGlobalVector("_AO_Target_TexelSize", new Vector4(1f / (float)targetWidth, 1f / (float)targetHeight, (float)targetWidth, (float)targetHeight));
		cb.SetRenderTarget(target);
	}

	
	private void PerformBlit(CommandBuffer cb, Material mat, int pass)
	{
		cb.DrawMesh(this.m_blitMesh, Matrix4x4.identity, mat, 0, pass);
	}

	
	private void PerformBlit(CommandBuffer cb, Material mat, int pass, int x, int y)
	{
		cb.SetGlobalVector("_AO_Target_Position", new Vector2((float)x, (float)y));
		this.PerformBlit(cb, mat, pass);
	}

	
	private void PerformBlit(CommandBuffer cb, RenderTargetIdentifier source, int sourceWidth, int sourceHeight, Material mat, int pass)
	{
		cb.SetGlobalTexture("_AO_Source", source);
		cb.SetGlobalVector("_AO_Source_TexelSize", new Vector4(1f / (float)sourceWidth, 1f / (float)sourceHeight, (float)sourceWidth, (float)sourceHeight));
		this.PerformBlit(cb, mat, pass);
	}

	
	private void PerformBlit(CommandBuffer cb, RenderTargetIdentifier source, int sourceWidth, int sourceHeight, Material mat, int pass, int x, int y)
	{
		cb.SetGlobalVector("_AO_Target_Position", new Vector2((float)x, (float)y));
		this.PerformBlit(cb, source, sourceWidth, sourceHeight, mat, pass);
	}

	
	private CommandBuffer CommandBuffer_Allocate(string name)
	{
		return new CommandBuffer
		{
			name = name
		};
	}

	
	private void CommandBuffer_Register(CameraEvent cameraEvent, CommandBuffer commandBuffer)
	{
		this.m_camera.AddCommandBuffer(cameraEvent, commandBuffer);
		this.m_registeredCommandBuffers.Add(cameraEvent, commandBuffer);
	}

	
	private void CommandBuffer_Unregister(CameraEvent cameraEvent, CommandBuffer commandBuffer)
	{
		if (this.m_camera != null)
		{
			CommandBuffer[] commandBuffers = this.m_camera.GetCommandBuffers(cameraEvent);
			foreach (CommandBuffer commandBuffer2 in commandBuffers)
			{
				if (commandBuffer2.name == commandBuffer.name)
				{
					this.m_camera.RemoveCommandBuffer(cameraEvent, commandBuffer2);
				}
			}
		}
	}

	
	private CommandBuffer CommandBuffer_AllocateRegister(CameraEvent cameraEvent)
	{
		string name = string.Empty;
		if (cameraEvent == CameraEvent.BeforeReflections)
		{
			name = "AO-BeforeRefl";
		}
		else if (cameraEvent == CameraEvent.AfterLighting)
		{
			name = "AO-AfterLighting";
		}
		else if (cameraEvent == CameraEvent.BeforeImageEffectsOpaque)
		{
			name = "AO-BeforePostOpaque";
		}
		else
		{
			Debug.LogError("[AmplifyOcclusion] Unsupported CameraEvent. Please contact support.");
		}
		CommandBuffer commandBuffer = this.CommandBuffer_Allocate(name);
		this.CommandBuffer_Register(cameraEvent, commandBuffer);
		return commandBuffer;
	}

	
	private void CommandBuffer_UnregisterAll()
	{
		foreach (KeyValuePair<CameraEvent, CommandBuffer> keyValuePair in this.m_registeredCommandBuffers)
		{
			this.CommandBuffer_Unregister(keyValuePair.Key, keyValuePair.Value);
		}
		this.m_registeredCommandBuffers.Clear();
	}

	
	private void UpdateLocalMaterialConstants()
	{
		if (this.m_occlusionMat != null)
		{
			this.m_occlusionMat.SetTexture("_AO_RandomTexture", this.m_randomTex);
		}
	}

	
	private void UpdateGlobalShaderConstants(AmplifyOcclusionBase.TargetDesc target)
	{
		float num = this.m_camera.fieldOfView * 0.0174532924f;
		Vector2 vector = new Vector2(1f / Mathf.Tan(num * 0.5f) * ((float)target.height / (float)target.width), 1f / Mathf.Tan(num * 0.5f));
		Vector2 vector2 = new Vector2(1f / vector.x, 1f / vector.y);
		float num2;
		if (this.m_camera.orthographic)
		{
			num2 = (float)target.height / this.m_camera.orthographicSize;
		}
		else
		{
			num2 = (float)target.height / (Mathf.Tan(num * 0.5f) * 2f);
		}
		float num3 = Mathf.Clamp(this.Bias, 0f, 1f);
		this.FadeStart = Mathf.Max(0f, this.FadeStart);
		this.FadeLength = Mathf.Max(0.01f, this.FadeLength);
		float y = (!this.FadeEnabled) ? 0f : (1f / this.FadeLength);
		Shader.SetGlobalMatrix("_AO_CameraProj", GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(0f, 1f, 0f, 1f, -1f, 100f), false));
		Shader.SetGlobalMatrix("_AO_CameraView", this.m_camera.worldToCameraMatrix);
		Shader.SetGlobalVector("_AO_UVToView", new Vector4(2f * vector2.x, -2f * vector2.y, -1f * vector2.x, 1f * vector2.y));
		Shader.SetGlobalFloat("_AO_HalfProjScale", 0.5f * num2);
		Shader.SetGlobalFloat("_AO_Radius", this.Radius);
		Shader.SetGlobalFloat("_AO_PowExponent", this.PowerExponent);
		Shader.SetGlobalFloat("_AO_Bias", num3);
		Shader.SetGlobalFloat("_AO_Multiplier", 1f / (1f - num3));
		Shader.SetGlobalFloat("_AO_BlurSharpness", this.BlurSharpness);
		Shader.SetGlobalColor("_AO_Levels", new Color(this.Tint.r, this.Tint.g, this.Tint.b, this.Intensity));
		Shader.SetGlobalVector("_AO_FadeParams", new Vector2(this.FadeStart, y));
		Shader.SetGlobalVector("_AO_FadeValues", new Vector3(this.FadeToIntensity, this.FadeToRadius, this.FadeToPowerExponent));
	}

	
	private void CommandBuffer_FillComputeOcclusion(CommandBuffer cb, AmplifyOcclusionBase.TargetDesc target)
	{
		this.CheckMaterial();
		this.CheckRandomData();
		cb.SetGlobalVector("_AO_Buffer_PadScale", new Vector4(target.padRatioWidth, target.padRatioHeight, 1f / target.padRatioWidth, 1f / target.padRatioHeight));
		cb.SetGlobalVector("_AO_Buffer_TexelSize", new Vector4(1f / (float)target.width, 1f / (float)target.height, (float)target.width, (float)target.height));
		cb.SetGlobalVector("_AO_QuarterBuffer_TexelSize", new Vector4(1f / (float)target.quarterWidth, 1f / (float)target.quarterHeight, (float)target.quarterWidth, (float)target.quarterHeight));
		cb.SetGlobalFloat("_AO_MaxRadiusPixels", (float)Mathf.Min(target.width, target.height));
		if (this.m_occlusionRT == null || this.m_occlusionRT.width != target.width || this.m_occlusionRT.height != target.height || !this.m_occlusionRT.IsCreated())
		{
			this.SafeReleaseRT(ref this.m_occlusionRT);
			this.m_occlusionRT = this.SafeAllocateRT("_AO_OcclusionTexture", target.width, target.height, this.m_occlusionRTFormat, RenderTextureReadWrite.Linear);
		}
		int num = -1;
		if (this.Downsample)
		{
			num = this.SafeAllocateTemporaryRT(cb, "_AO_SmallOcclusionTexture", target.width / 2, target.height / 2, this.m_occlusionRTFormat, RenderTextureReadWrite.Linear, FilterMode.Bilinear);
		}
		if (this.CacheAware && !this.Downsample)
		{
			int num2 = this.SafeAllocateTemporaryRT(cb, "_AO_OcclusionAtlas", target.width, target.height, this.m_occlusionRTFormat, RenderTextureReadWrite.Linear, FilterMode.Point);
			for (int i = 0; i < 16; i++)
			{
				this.m_depthLayerRT[i] = this.SafeAllocateTemporaryRT(cb, this.m_layerDepthNames[i], target.quarterWidth, target.quarterHeight, this.m_depthRTFormat, RenderTextureReadWrite.Linear, FilterMode.Point);
				this.m_normalLayerRT[i] = this.SafeAllocateTemporaryRT(cb, this.m_layerNormalNames[i], target.quarterWidth, target.quarterHeight, this.m_normalRTFormat, RenderTextureReadWrite.Linear, FilterMode.Point);
				this.m_occlusionLayerRT[i] = this.SafeAllocateTemporaryRT(cb, this.m_layerOcclusionNames[i], target.quarterWidth, target.quarterHeight, this.m_occlusionRTFormat, RenderTextureReadWrite.Linear, FilterMode.Point);
			}
			for (int j = 0; j < 16; j += this.m_mrtCount)
			{
				for (int k = 0; k < this.m_mrtCount; k++)
				{
					int num3 = k + j;
					int num4 = num3 & 3;
					int num5 = num3 >> 2;
					cb.SetGlobalVector(this.m_layerOffsetNames[k], new Vector2((float)num4 + 0.5f, (float)num5 + 0.5f));
					this.m_depthTargets[k] = this.m_depthLayerRT[num3];
					this.m_normalTargets[k] = this.m_normalLayerRT[num3];
				}
				this.SetBlitTarget(cb, this.m_depthTargets, target.quarterWidth, target.quarterHeight);
				this.PerformBlit(cb, this.m_occlusionMat, this.m_deinterleaveDepthPass);
				this.SetBlitTarget(cb, this.m_normalTargets, target.quarterWidth, target.quarterHeight);
				this.PerformBlit(cb, this.m_occlusionMat, (int)(this.m_deinterleaveNormalPass + this.PerPixelNormals));
			}
			for (int l = 0; l < 16; l++)
			{
				cb.SetGlobalVector("_AO_LayerOffset", new Vector2((float)(l & 3) + 0.5f, (float)(l >> 2) + 0.5f));
				cb.SetGlobalVector("_AO_LayerRandom", this.m_randomData[l]);
				cb.SetGlobalTexture("_AO_NormalTexture", this.m_normalLayerRT[l]);
				cb.SetGlobalTexture("_AO_DepthTexture", this.m_depthLayerRT[l]);
				this.SetBlitTarget(cb, this.m_occlusionLayerRT[l], target.quarterWidth, target.quarterHeight);
				this.PerformBlit(cb, this.m_occlusionMat, (int)(15 + this.SampleCount));
			}
			this.SetBlitTarget(cb, num2, target.width, target.height);
			for (int m = 0; m < 16; m++)
			{
				int x = (m & 3) * target.quarterWidth;
				int y = (m >> 2) * target.quarterHeight;
				this.PerformBlit(cb, this.m_occlusionLayerRT[m], target.quarterWidth, target.quarterHeight, this.m_copyMat, 0, x, y);
			}
			cb.SetGlobalTexture("_AO_OcclusionAtlas", num2);
			this.SetBlitTarget(cb, this.m_occlusionRT, target.width, target.height);
			this.PerformBlit(cb, this.m_occlusionMat, 19);
			for (int n = 0; n < 16; n++)
			{
				this.SafeReleaseTemporaryRT(cb, this.m_occlusionLayerRT[n]);
				this.SafeReleaseTemporaryRT(cb, this.m_normalLayerRT[n]);
				this.SafeReleaseTemporaryRT(cb, this.m_depthLayerRT[n]);
			}
			this.SafeReleaseTemporaryRT(cb, num2);
		}
		else
		{
			int pass = (int)(20 + this.SampleCount * (AmplifyOcclusionBase.SampleCountLevel)4 + (int)this.PerPixelNormals);
			if (this.Downsample)
			{
				cb.Blit(null, new RenderTargetIdentifier(num), this.m_occlusionMat, pass);
				this.SetBlitTarget(cb, this.m_occlusionRT, target.width, target.height);
				this.PerformBlit(cb, num, target.width / 2, target.height / 2, this.m_occlusionMat, 41);
			}
			else
			{
				cb.Blit(null, this.m_occlusionRT, this.m_occlusionMat, pass);
			}
		}
		if (this.BlurEnabled)
		{
			int num6 = this.SafeAllocateTemporaryRT(cb, "_AO_TEMP", target.width, target.height, this.m_occlusionRTFormat, RenderTextureReadWrite.Linear, FilterMode.Point);
			for (int num7 = 0; num7 < this.BlurPasses; num7++)
			{
				this.SetBlitTarget(cb, num6, target.width, target.height);
				this.PerformBlit(cb, this.m_occlusionRT, target.width, target.height, this.m_blurMat, (this.BlurRadius - 1) * 2);
				this.SetBlitTarget(cb, this.m_occlusionRT, target.width, target.height);
				this.PerformBlit(cb, num6, target.width, target.height, this.m_blurMat, 1 + (this.BlurRadius - 1) * 2);
			}
			this.SafeReleaseTemporaryRT(cb, num6);
		}
		if (this.Downsample && num >= 0)
		{
			this.SafeReleaseTemporaryRT(cb, num);
		}
		cb.SetRenderTarget(null);
	}

	
	private void CommandBuffer_FillApplyDeferred(CommandBuffer cb, AmplifyOcclusionBase.TargetDesc target, bool logTarget)
	{
		cb.SetGlobalTexture("_AO_OcclusionTexture", this.m_occlusionRT);
		this.m_applyDeferredTargets[0] = BuiltinRenderTextureType.GBuffer0;
		this.m_applyDeferredTargets[1] = ((!logTarget) ? BuiltinRenderTextureType.CameraTarget : BuiltinRenderTextureType.GBuffer3);
		if (!logTarget)
		{
			this.SetBlitTarget(cb, this.m_applyDeferredTargets, target.fullWidth, target.fullHeight);
			this.PerformBlit(cb, this.m_occlusionMat, 37);
		}
		else
		{
			int num = this.SafeAllocateTemporaryRT(cb, "_AO_GBufferAlbedo", target.fullWidth, target.fullHeight, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, FilterMode.Point);
			int num2 = this.SafeAllocateTemporaryRT(cb, "_AO_GBufferEmission", target.fullWidth, target.fullHeight, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, FilterMode.Point);
			cb.Blit(this.m_applyDeferredTargets[0], num);
			cb.Blit(this.m_applyDeferredTargets[1], num2);
			cb.SetGlobalTexture("_AO_GBufferAlbedo", num);
			cb.SetGlobalTexture("_AO_GBufferEmission", num2);
			this.SetBlitTarget(cb, this.m_applyDeferredTargets, target.fullWidth, target.fullHeight);
			this.PerformBlit(cb, this.m_occlusionMat, 38);
			this.SafeReleaseTemporaryRT(cb, num);
			this.SafeReleaseTemporaryRT(cb, num2);
		}
		cb.SetRenderTarget(null);
	}

	
	private void CommandBuffer_FillApplyPostEffect(CommandBuffer cb, AmplifyOcclusionBase.TargetDesc target, bool logTarget)
	{
		cb.SetGlobalTexture("_AO_OcclusionTexture", this.m_occlusionRT);
		if (!logTarget)
		{
			this.SetBlitTarget(cb, BuiltinRenderTextureType.CameraTarget, target.fullWidth, target.fullHeight);
			this.PerformBlit(cb, this.m_occlusionMat, 39);
		}
		else
		{
			int num = this.SafeAllocateTemporaryRT(cb, "_AO_GBufferEmission", target.fullWidth, target.fullHeight, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, FilterMode.Point);
			cb.Blit(BuiltinRenderTextureType.GBuffer3, num);
			cb.SetGlobalTexture("_AO_GBufferEmission", num);
			this.SetBlitTarget(cb, BuiltinRenderTextureType.GBuffer3, target.fullWidth, target.fullHeight);
			this.PerformBlit(cb, this.m_occlusionMat, 40);
			this.SafeReleaseTemporaryRT(cb, num);
		}
		cb.SetRenderTarget(null);
	}

	
	private void CommandBuffer_FillApplyDebug(CommandBuffer cb, AmplifyOcclusionBase.TargetDesc target)
	{
		cb.SetGlobalTexture("_AO_OcclusionTexture", this.m_occlusionRT);
		this.SetBlitTarget(cb, BuiltinRenderTextureType.CameraTarget, target.fullWidth, target.fullHeight);
		this.PerformBlit(cb, this.m_occlusionMat, 36);
		cb.SetRenderTarget(null);
	}

	
	private void CommandBuffer_Rebuild(AmplifyOcclusionBase.TargetDesc target)
	{
		bool flag = this.PerPixelNormals == AmplifyOcclusionBase.PerPixelNormalSource.GBuffer || this.PerPixelNormals == AmplifyOcclusionBase.PerPixelNormalSource.GBufferOctaEncoded;
		CameraEvent cameraEvent = (!flag) ? CameraEvent.BeforeImageEffectsOpaque : CameraEvent.AfterLighting;
		if (this.ApplyMethod == AmplifyOcclusionBase.ApplicationMethod.Debug)
		{
			CommandBuffer cb = this.CommandBuffer_AllocateRegister(cameraEvent);
			this.CommandBuffer_FillComputeOcclusion(cb, target);
			this.CommandBuffer_FillApplyDebug(cb, target);
		}
		else
		{
			bool logTarget = !this.m_camera.allowHDR && flag;
			cameraEvent = ((this.ApplyMethod != AmplifyOcclusionBase.ApplicationMethod.Deferred) ? cameraEvent : CameraEvent.BeforeReflections);
			CommandBuffer cb = this.CommandBuffer_AllocateRegister(cameraEvent);
			this.CommandBuffer_FillComputeOcclusion(cb, target);
			if (this.ApplyMethod == AmplifyOcclusionBase.ApplicationMethod.PostEffect)
			{
				this.CommandBuffer_FillApplyPostEffect(cb, target, logTarget);
			}
			else if (this.ApplyMethod == AmplifyOcclusionBase.ApplicationMethod.Deferred)
			{
				this.CommandBuffer_FillApplyDeferred(cb, target, logTarget);
			}
		}
	}

	
	private void OnPreRender()
	{
		bool allowHDR = this.m_camera.allowHDR;
		this.m_target.fullWidth = this.m_camera.pixelWidth;
		this.m_target.fullHeight = this.m_camera.pixelHeight;
		this.m_target.format = ((!allowHDR) ? RenderTextureFormat.ARGB32 : RenderTextureFormat.ARGBHalf);
		this.m_target.width = ((!this.CacheAware) ? this.m_target.fullWidth : (this.m_target.fullWidth + 3 & -4));
		this.m_target.height = ((!this.CacheAware) ? this.m_target.fullHeight : (this.m_target.fullHeight + 3 & -4));
		this.m_target.quarterWidth = this.m_target.width / 4;
		this.m_target.quarterHeight = this.m_target.height / 4;
		this.m_target.padRatioWidth = (float)this.m_target.width / (float)this.m_target.fullWidth;
		this.m_target.padRatioHeight = (float)this.m_target.height / (float)this.m_target.fullHeight;
		this.UpdateLocalMaterialConstants();
		this.UpdateGlobalShaderConstants(this.m_target);
		if (this.CheckParamsChanged() || this.m_registeredCommandBuffers.Count == 0)
		{
			this.CommandBuffer_UnregisterAll();
			this.CommandBuffer_Rebuild(this.m_target);
			this.UpdateParams();
		}
	}

	
	private void OnPostRender()
	{
		this.m_occlusionRT.MarkRestoreExpected();
	}

	
	[Header("Ambient Occlusion")]
	public AmplifyOcclusionBase.ApplicationMethod ApplyMethod;

	
	public AmplifyOcclusionBase.SampleCountLevel SampleCount = AmplifyOcclusionBase.SampleCountLevel.Medium;

	
	public AmplifyOcclusionBase.PerPixelNormalSource PerPixelNormals;

	
	[Range(0f, 1f)]
	public float Intensity = 1f;

	
	public Color Tint = Color.black;

	
	[Range(0f, 16f)]
	public float Radius = 1f;

	
	[Range(0f, 16f)]
	public float PowerExponent = 1.8f;

	
	[Range(0f, 0.99f)]
	public float Bias = 0.05f;

	
	public bool CacheAware;

	
	public bool Downsample;

	
	[Header("Distance Fade")]
	public bool FadeEnabled;

	
	public float FadeStart = 100f;

	
	public float FadeLength = 50f;

	
	[Range(0f, 1f)]
	public float FadeToIntensity = 1f;

	
	[Range(0f, 16f)]
	public float FadeToRadius = 1f;

	
	[Range(0f, 16f)]
	public float FadeToPowerExponent = 1.8f;

	
	[Header("Bilateral Blur")]
	public bool BlurEnabled = true;

	
	[Range(1f, 4f)]
	public int BlurRadius = 2;

	
	[Range(1f, 4f)]
	public int BlurPasses = 1;

	
	[Range(0f, 20f)]
	public float BlurSharpness = 10f;

	
	private const int PerPixelNormalSourceCount = 4;

	
	private int prevScreenWidth;

	
	private int prevScreenHeight;

	
	private bool prevHDR;

	
	private AmplifyOcclusionBase.ApplicationMethod prevApplyMethod;

	
	private AmplifyOcclusionBase.SampleCountLevel prevSampleCount;

	
	private AmplifyOcclusionBase.PerPixelNormalSource prevPerPixelNormals;

	
	private bool prevCacheAware;

	
	private bool prevDownscale;

	
	private bool prevFadeEnabled;

	
	private float prevFadeToIntensity;

	
	private float prevFadeToRadius;

	
	private float prevFadeToPowerExponent;

	
	private float prevFadeStart;

	
	private float prevFadeLength;

	
	private bool prevBlurEnabled;

	
	private int prevBlurRadius;

	
	private int prevBlurPasses;

	
	private Camera m_camera;

	
	private Material m_occlusionMat;

	
	private Material m_blurMat;

	
	private Material m_copyMat;

	
	private const int RandomSize = 4;

	
	private const int DirectionCount = 8;

	
	private Color[] m_randomData;

	
	private Texture2D m_randomTex;

	
	private string[] m_layerOffsetNames;

	
	private string[] m_layerRandomNames;

	
	private string[] m_layerDepthNames;

	
	private string[] m_layerNormalNames;

	
	private string[] m_layerOcclusionNames;

	
	private RenderTextureFormat m_depthRTFormat = RenderTextureFormat.RFloat;

	
	private RenderTextureFormat m_normalRTFormat = RenderTextureFormat.ARGB2101010;

	
	private RenderTextureFormat m_occlusionRTFormat = RenderTextureFormat.RGHalf;

	
	private RenderTexture m_occlusionRT;

	
	private int[] m_depthLayerRT;

	
	private int[] m_normalLayerRT;

	
	private int[] m_occlusionLayerRT;

	
	private int m_mrtCount;

	
	private RenderTargetIdentifier[] m_depthTargets;

	
	private RenderTargetIdentifier[] m_normalTargets;

	
	private int m_deinterleaveDepthPass;

	
	private int m_deinterleaveNormalPass;

	
	private RenderTargetIdentifier[] m_applyDeferredTargets;

	
	private Mesh m_blitMesh;

	
	private AmplifyOcclusionBase.TargetDesc m_target = default(AmplifyOcclusionBase.TargetDesc);

	
	private Dictionary<CameraEvent, CommandBuffer> m_registeredCommandBuffers = new Dictionary<CameraEvent, CommandBuffer>();

	
	public enum ApplicationMethod
	{
		
		PostEffect,
		
		Deferred,
		
		Debug
	}

	
	public enum PerPixelNormalSource
	{
		
		None,
		
		Camera,
		
		GBuffer,
		
		GBufferOctaEncoded
	}

	
	public enum SampleCountLevel
	{
		
		Low,
		
		Medium,
		
		High,
		
		VeryHigh
	}

	
	private static class ShaderPass
	{
		
		public const int FullDepth = 0;

		
		public const int FullNormal_None = 1;

		
		public const int FullNormal_Camera = 2;

		
		public const int FullNormal_GBuffer = 3;

		
		public const int FullNormal_GBufferOctaEncoded = 4;

		
		public const int DeinterleaveDepth1 = 5;

		
		public const int DeinterleaveNormal1_None = 6;

		
		public const int DeinterleaveNormal1_Camera = 7;

		
		public const int DeinterleaveNormal1_GBuffer = 8;

		
		public const int DeinterleaveNormal1_GBufferOctaEncoded = 9;

		
		public const int DeinterleaveDepth4 = 10;

		
		public const int DeinterleaveNormal4_None = 11;

		
		public const int DeinterleaveNormal4_Camera = 12;

		
		public const int DeinterleaveNormal4_GBuffer = 13;

		
		public const int DeinterleaveNormal4_GBufferOctaEncoded = 14;

		
		public const int OcclusionCache_Low = 15;

		
		public const int OcclusionCache_Medium = 16;

		
		public const int OcclusionCache_High = 17;

		
		public const int OcclusionCache_VeryHigh = 18;

		
		public const int Reinterleave = 19;

		
		public const int OcclusionLow_None = 20;

		
		public const int OcclusionLow_Camera = 21;

		
		public const int OcclusionLow_GBuffer = 22;

		
		public const int OcclusionLow_GBufferOctaEncoded = 23;

		
		public const int OcclusionMedium_None = 24;

		
		public const int OcclusionMedium_Camera = 25;

		
		public const int OcclusionMedium_GBuffer = 26;

		
		public const int OcclusionMedium_GBufferOctaEncoded = 27;

		
		public const int OcclusionHigh_None = 28;

		
		public const int OcclusionHigh_Camera = 29;

		
		public const int OcclusionHigh_GBuffer = 30;

		
		public const int OcclusionHigh_GBufferOctaEncoded = 31;

		
		public const int OcclusionVeryHigh_None = 32;

		
		public const int OcclusionVeryHigh_Camera = 33;

		
		public const int OcclusionVeryHigh_GBuffer = 34;

		
		public const int OcclusionVeryHigh_GBufferNormalEncoded = 35;

		
		public const int ApplyDebug = 36;

		
		public const int ApplyDeferred = 37;

		
		public const int ApplyDeferredLog = 38;

		
		public const int ApplyPostEffect = 39;

		
		public const int ApplyPostEffectLog = 40;

		
		public const int CombineDownsampledOcclusionDepth = 41;

		
		public const int BlurHorizontal1 = 0;

		
		public const int BlurVertical1 = 1;

		
		public const int BlurHorizontal2 = 2;

		
		public const int BlurVertical2 = 3;

		
		public const int BlurHorizontal3 = 4;

		
		public const int BlurVertical3 = 5;

		
		public const int BlurHorizontal4 = 6;

		
		public const int BlurVertical4 = 7;

		
		public const int Copy = 0;
	}

	
	private struct TargetDesc
	{
		
		public int fullWidth;

		
		public int fullHeight;

		
		public RenderTextureFormat format;

		
		public int width;

		
		public int height;

		
		public int quarterWidth;

		
		public int quarterHeight;

		
		public float padRatioWidth;

		
		public float padRatioHeight;
	}
}
