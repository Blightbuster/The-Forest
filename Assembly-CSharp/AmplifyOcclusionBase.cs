using System;
using UnityEngine;
using UnityEngine.Rendering;

[AddComponentMenu("")]
public class AmplifyOcclusionBase : MonoBehaviour
{
	private bool UsingTemporalFilter
	{
		get
		{
			return this.FilterEnabled && this.m_targetCamera.cameraType != CameraType.SceneView;
		}
	}

	private bool UsingMotionVectors
	{
		get
		{
			return this.UsingTemporalFilter && this.ApplyMethod != AmplifyOcclusionBase.ApplicationMethod.Deferred;
		}
	}

	private void createCommandBuffer(ref AmplifyOcclusionBase.CmdBuffer aCmdBuffer, string aCmdBufferName, CameraEvent aCameraEvent)
	{
		if (aCmdBuffer.cmdBuffer != null)
		{
			this.cleanupCommandBuffer(ref aCmdBuffer);
		}
		aCmdBuffer.cmdBufferName = aCmdBufferName;
		aCmdBuffer.cmdBuffer = new CommandBuffer();
		aCmdBuffer.cmdBuffer.name = aCmdBufferName;
		aCmdBuffer.cmdBufferEvent = aCameraEvent;
		this.m_targetCamera.AddCommandBuffer(aCameraEvent, aCmdBuffer.cmdBuffer);
	}

	private void cleanupCommandBuffer(ref AmplifyOcclusionBase.CmdBuffer aCmdBuffer)
	{
		CommandBuffer[] commandBuffers = this.m_targetCamera.GetCommandBuffers(aCmdBuffer.cmdBufferEvent);
		for (int i = 0; i < commandBuffers.Length; i++)
		{
			if (commandBuffers[i].name == aCmdBuffer.cmdBufferName)
			{
				this.m_targetCamera.RemoveCommandBuffer(aCmdBuffer.cmdBufferEvent, commandBuffers[i]);
			}
		}
		aCmdBuffer.cmdBufferName = null;
		aCmdBuffer.cmdBufferEvent = CameraEvent.BeforeDepthTexture;
		aCmdBuffer.cmdBuffer = null;
	}

	private void createQuadMesh()
	{
		if (AmplifyOcclusionBase.m_quadMesh == null)
		{
			AmplifyOcclusionBase.m_quadMesh = new Mesh();
			AmplifyOcclusionBase.m_quadMesh.vertices = new Vector3[]
			{
				new Vector3(0f, 0f, 0f),
				new Vector3(0f, 1f, 0f),
				new Vector3(1f, 1f, 0f),
				new Vector3(1f, 0f, 0f)
			};
			AmplifyOcclusionBase.m_quadMesh.uv = new Vector2[]
			{
				new Vector2(0f, 0f),
				new Vector2(0f, 1f),
				new Vector2(1f, 1f),
				new Vector2(1f, 0f)
			};
			AmplifyOcclusionBase.m_quadMesh.triangles = new int[]
			{
				0,
				1,
				2,
				0,
				2,
				3
			};
			AmplifyOcclusionBase.m_quadMesh.normals = new Vector3[0];
			AmplifyOcclusionBase.m_quadMesh.tangents = new Vector4[0];
			AmplifyOcclusionBase.m_quadMesh.colors32 = new Color32[0];
			AmplifyOcclusionBase.m_quadMesh.colors = new Color[0];
		}
	}

	private void PerformBlit(CommandBuffer cb, Material mat, int pass)
	{
		cb.DrawMesh(AmplifyOcclusionBase.m_quadMesh, Matrix4x4.identity, mat, 0, pass);
	}

	private Material createMaterialWithShaderName(string aShaderName, bool aThroughErrorMsg)
	{
		Shader shader = Shader.Find(aShaderName);
		if (shader == null)
		{
			if (aThroughErrorMsg)
			{
				Debug.LogErrorFormat("[AmplifyOcclusion] Cannot find shader: \"{0}\" Please contact support@amplify.pt", new object[]
				{
					aShaderName
				});
			}
			return null;
		}
		return new Material(shader)
		{
			hideFlags = HideFlags.DontSave
		};
	}

	private void checkMaterials(bool aThroughErrorMsg)
	{
		if (AmplifyOcclusionBase.m_occlusionMat == null)
		{
			AmplifyOcclusionBase.m_occlusionMat = this.createMaterialWithShaderName("Hidden/Amplify Occlusion/Occlusion", aThroughErrorMsg);
		}
		if (AmplifyOcclusionBase.m_blurMat == null)
		{
			AmplifyOcclusionBase.m_blurMat = this.createMaterialWithShaderName("Hidden/Amplify Occlusion/Blur", aThroughErrorMsg);
		}
		if (AmplifyOcclusionBase.m_applyOcclusionMat == null)
		{
			AmplifyOcclusionBase.m_applyOcclusionMat = this.createMaterialWithShaderName("Hidden/Amplify Occlusion/Apply", aThroughErrorMsg);
		}
	}

	private bool checkRenderTextureFormats()
	{
		if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB32) && SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
		{
			this.m_occlusionRTFormat = RenderTextureFormat.RGHalf;
			if (!SystemInfo.SupportsRenderTextureFormat(this.m_occlusionRTFormat))
			{
				this.m_occlusionRTFormat = RenderTextureFormat.RGFloat;
				if (!SystemInfo.SupportsRenderTextureFormat(this.m_occlusionRTFormat))
				{
					this.m_occlusionRTFormat = RenderTextureFormat.ARGBHalf;
				}
			}
			this.m_accumTemporalRTFormat = RenderTextureFormat.ARGB32;
			return true;
		}
		return false;
	}

	private void OnEnable()
	{
		if (!this.checkRenderTextureFormats())
		{
			Debug.LogError("[AmplifyOcclusion] Target platform does not meet the minimum requirements for this effect to work properly.");
			base.enabled = false;
			return;
		}
		this.m_targetCamera = base.GetComponent<Camera>();
		this.checkMaterials(false);
		this.createQuadMesh();
		this.useMRTBlendingFallback = (AmplifyOcclusionBase.m_applyOcclusionMat.GetTag("MRTBlending", false).ToUpper() != "TRUE");
	}

	private void Reset()
	{
		if (this.m_commandBuffer_Occlusion.cmdBuffer != null)
		{
			this.cleanupCommandBuffer(ref this.m_commandBuffer_Occlusion);
		}
		if (this.m_commandBuffer_Apply.cmdBuffer != null)
		{
			this.cleanupCommandBuffer(ref this.m_commandBuffer_Apply);
		}
		this.releaseRT();
	}

	private void OnDisable()
	{
		this.Reset();
	}

	private void releaseRT()
	{
		this.safeReleaseRT(ref this.m_occlusionDepthRT);
		this.m_occlusionDepthRT = null;
		if (this.m_temporalAccumRT != null && this.m_temporalAccumRT.Length != 0)
		{
			this.safeReleaseRT(ref this.m_temporalAccumRT[0]);
			this.safeReleaseRT(ref this.m_temporalAccumRT[1]);
		}
		this.m_temporalAccumRT = null;
	}

	private void ClearHistory()
	{
		if (this.m_temporalAccumRT != null)
		{
			Graphics.SetRenderTarget(this.m_temporalAccumRT[0]);
			GL.Clear(false, true, Color.black);
			Graphics.SetRenderTarget(this.m_temporalAccumRT[1]);
			GL.Clear(false, true, Color.black);
		}
	}

	private bool checkParamsChanged()
	{
		bool allowHDR = this.m_targetCamera.allowHDR;
		bool flag = this.m_targetCamera.allowMSAA && this.m_targetCamera.actualRenderingPath != RenderingPath.DeferredLighting && this.m_targetCamera.actualRenderingPath != RenderingPath.DeferredShading && QualitySettings.antiAliasing >= 1;
		int antiAliasing = (!flag) ? 1 : QualitySettings.antiAliasing;
		if (this.m_occlusionDepthRT != null && (this.m_occlusionDepthRT.width != this.m_target.width || this.m_occlusionDepthRT.height != this.m_target.height || this.m_prevMSAA != flag || this.m_prevFilterEnabled != this.FilterEnabled || !this.m_occlusionDepthRT.IsCreated() || (this.m_temporalAccumRT != null && (!this.m_temporalAccumRT[0].IsCreated() || !this.m_temporalAccumRT[1].IsCreated()))))
		{
			this.releaseRT();
			this.m_paramsChanged = true;
		}
		if (this.m_temporalAccumRT != null && this.m_temporalAccumRT.Length != 2)
		{
			this.m_temporalAccumRT = null;
		}
		if (this.m_occlusionDepthRT == null)
		{
			this.m_occlusionDepthRT = this.safeAllocateRT("_AO_OcclusionDepthTexture", this.m_target.width, this.m_target.height, this.m_occlusionRTFormat, RenderTextureReadWrite.Linear, FilterMode.Point, 1);
		}
		bool flag2 = false;
		if (this.m_temporalAccumRT == null && this.FilterEnabled)
		{
			this.m_temporalAccumRT = new RenderTexture[2];
			this.m_temporalAccumRT[0] = this.safeAllocateRT("_AO_TemporalAccum_0", this.m_target.width, this.m_target.height, this.m_accumTemporalRTFormat, RenderTextureReadWrite.Linear, FilterMode.Bilinear, antiAliasing);
			this.m_temporalAccumRT[1] = this.safeAllocateRT("_AO_TemporalAccum_1", this.m_target.width, this.m_target.height, this.m_accumTemporalRTFormat, RenderTextureReadWrite.Linear, FilterMode.Bilinear, antiAliasing);
			flag2 = true;
		}
		if (this.m_prevSampleCount != this.SampleCount || this.m_prevDownsample != this.Downsample || this.m_prevBlurEnabled != this.BlurEnabled || this.m_prevBlurPasses != this.BlurPasses || this.m_prevBlurRadius != this.BlurRadius || this.m_prevFilterEnabled != this.FilterEnabled || this.m_prevHDR != allowHDR || this.m_prevMSAA != flag)
		{
			flag2 |= (this.m_prevHDR != allowHDR);
			flag2 |= (this.m_prevMSAA != flag);
			this.m_HDR = allowHDR;
			this.m_MSAA = flag;
			this.m_paramsChanged = true;
		}
		if (flag2 && this.FilterEnabled)
		{
			this.ClearHistory();
		}
		return this.m_paramsChanged;
	}

	private void updateParams()
	{
		this.m_prevSampleCount = this.SampleCount;
		this.m_prevDownsample = this.Downsample;
		this.m_prevBlurEnabled = this.BlurEnabled;
		this.m_prevBlurPasses = this.BlurPasses;
		this.m_prevBlurRadius = this.BlurRadius;
		this.m_prevFilterEnabled = this.FilterEnabled;
		this.m_prevHDR = this.m_HDR;
		this.m_prevMSAA = this.m_MSAA;
		this.m_paramsChanged = false;
	}

	private void Update()
	{
		if (this.m_targetCamera.actualRenderingPath != RenderingPath.DeferredShading)
		{
			if (this.PerPixelNormals != AmplifyOcclusionBase.PerPixelNormalSource.None && this.PerPixelNormals != AmplifyOcclusionBase.PerPixelNormalSource.Camera)
			{
				this.m_paramsChanged = true;
				this.PerPixelNormals = AmplifyOcclusionBase.PerPixelNormalSource.Camera;
				Debug.LogWarning("[AmplifyOcclusion] GBuffer Normals only available in Camera Deferred Shading mode. Switched to Camera source.");
			}
			if (this.ApplyMethod == AmplifyOcclusionBase.ApplicationMethod.Deferred)
			{
				this.m_paramsChanged = true;
				this.ApplyMethod = AmplifyOcclusionBase.ApplicationMethod.PostEffect;
				Debug.LogWarning("[AmplifyOcclusion] Deferred Method requires a Deferred Shading path. Switching to Post Effect Method.");
			}
		}
		else if (this.PerPixelNormals == AmplifyOcclusionBase.PerPixelNormalSource.Camera)
		{
			this.m_paramsChanged = true;
			this.PerPixelNormals = AmplifyOcclusionBase.PerPixelNormalSource.GBuffer;
			Debug.LogWarning("[AmplifyOcclusion] Camera Normals not supported for Deferred Method. Switching to GBuffer Normals.");
		}
		if ((this.m_targetCamera.depthTextureMode & DepthTextureMode.Depth) == DepthTextureMode.None)
		{
			this.m_targetCamera.depthTextureMode |= DepthTextureMode.Depth;
		}
		if (this.PerPixelNormals == AmplifyOcclusionBase.PerPixelNormalSource.Camera && (this.m_targetCamera.depthTextureMode & DepthTextureMode.DepthNormals) == DepthTextureMode.None)
		{
			this.m_targetCamera.depthTextureMode |= DepthTextureMode.DepthNormals;
		}
		if (this.UsingMotionVectors && (this.m_targetCamera.depthTextureMode & DepthTextureMode.MotionVectors) == DepthTextureMode.None)
		{
			this.m_targetCamera.depthTextureMode |= DepthTextureMode.MotionVectors;
		}
	}

	private void OnPreRender()
	{
		this.checkMaterials(true);
		if (this.m_targetCamera != null)
		{
			bool flag = GraphicsSettings.GetShaderMode(BuiltinShaderType.DeferredReflections) != BuiltinShaderMode.Disabled;
			if (this.m_prevPerPixelNormals != this.PerPixelNormals || this.m_prevApplyMethod != this.ApplyMethod || this.m_prevDeferredReflections != flag || this.m_commandBuffer_Occlusion.cmdBuffer == null || this.m_commandBuffer_Apply.cmdBuffer == null)
			{
				CameraEvent aCameraEvent = CameraEvent.BeforeImageEffectsOpaque;
				if (this.ApplyMethod == AmplifyOcclusionBase.ApplicationMethod.Deferred)
				{
					aCameraEvent = ((!flag) ? CameraEvent.BeforeLighting : CameraEvent.BeforeReflections);
				}
				this.createCommandBuffer(ref this.m_commandBuffer_Occlusion, "AmplifyOcclusion_Compute", aCameraEvent);
				this.createCommandBuffer(ref this.m_commandBuffer_Apply, "AmplifyOcclusion_Apply", aCameraEvent);
				this.m_prevPerPixelNormals = this.PerPixelNormals;
				this.m_prevApplyMethod = this.ApplyMethod;
				this.m_prevDeferredReflections = flag;
				this.m_paramsChanged = true;
			}
			if (this.m_commandBuffer_Occlusion.cmdBuffer != null && this.m_commandBuffer_Apply.cmdBuffer != null)
			{
				this.m_curStepIdx = (this.m_sampleStep & 1u);
				this.UpdateGlobalShaderConstants();
				this.checkParamsChanged();
				this.UpdateGlobalShaderConstants_AmbientOcclusion();
				this.UpdateGlobalShaderConstants_Matrices();
				if (this.m_paramsChanged)
				{
					this.m_commandBuffer_Occlusion.cmdBuffer.Clear();
					this.commandBuffer_FillComputeOcclusion(this.m_commandBuffer_Occlusion.cmdBuffer);
				}
				this.m_commandBuffer_Apply.cmdBuffer.Clear();
				if (this.ApplyMethod == AmplifyOcclusionBase.ApplicationMethod.Debug)
				{
					this.commandBuffer_FillApplyDebug(this.m_commandBuffer_Apply.cmdBuffer);
				}
				else if (this.ApplyMethod == AmplifyOcclusionBase.ApplicationMethod.PostEffect)
				{
					this.commandBuffer_FillApplyPostEffect(this.m_commandBuffer_Apply.cmdBuffer);
				}
				else
				{
					bool logTarget = !this.m_HDR;
					this.commandBuffer_FillApplyDeferred(this.m_commandBuffer_Apply.cmdBuffer, logTarget);
				}
				this.updateParams();
				this.m_sampleStep += 1u;
			}
		}
	}

	private void OnPostRender()
	{
		if (this.m_occlusionDepthRT != null)
		{
			this.m_occlusionDepthRT.MarkRestoreExpected();
		}
		if (this.m_temporalAccumRT != null)
		{
			foreach (RenderTexture renderTexture in this.m_temporalAccumRT)
			{
				renderTexture.MarkRestoreExpected();
			}
		}
	}

	private int safeAllocateTemporaryRT(CommandBuffer cb, string propertyName, int width, int height, RenderTextureFormat format = RenderTextureFormat.Default, RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default, FilterMode filterMode = FilterMode.Point)
	{
		int num = Shader.PropertyToID(propertyName);
		cb.GetTemporaryRT(num, width, height, 0, filterMode, format, readWrite);
		return num;
	}

	private void safeReleaseTemporaryRT(CommandBuffer cb, int id)
	{
		cb.ReleaseTemporaryRT(id);
	}

	private RenderTexture safeAllocateRT(string name, int width, int height, RenderTextureFormat format, RenderTextureReadWrite readWrite, FilterMode filterMode = FilterMode.Point, int antiAliasing = 1)
	{
		width = Mathf.Clamp(width, 1, 65536);
		height = Mathf.Clamp(height, 1, 65536);
		RenderTexture renderTexture = new RenderTexture(width, height, 0, format, readWrite)
		{
			hideFlags = HideFlags.DontSave
		};
		renderTexture.name = name;
		renderTexture.filterMode = filterMode;
		renderTexture.wrapMode = TextureWrapMode.Clamp;
		renderTexture.antiAliasing = Mathf.Max(antiAliasing, 1);
		renderTexture.Create();
		return renderTexture;
	}

	private void safeReleaseRT(ref RenderTexture rt)
	{
		if (rt != null)
		{
			RenderTexture.active = null;
			rt.Release();
			UnityEngine.Object.DestroyImmediate(rt);
			rt = null;
		}
	}

	private void BeginSample(CommandBuffer cb, string name)
	{
		cb.BeginSample(name);
	}

	private void EndSample(CommandBuffer cb, string name)
	{
		cb.EndSample(name);
	}

	private void commandBuffer_FillComputeOcclusion(CommandBuffer cb)
	{
		this.BeginSample(cb, "AO 1 - ComputeOcclusion");
		if (this.PerPixelNormals == AmplifyOcclusionBase.PerPixelNormalSource.GBuffer || this.PerPixelNormals == AmplifyOcclusionBase.PerPixelNormalSource.GBufferOctaEncoded)
		{
			cb.SetGlobalTexture(AmplifyOcclusionBase.PropertyID._AO_GBufferNormals, BuiltinRenderTextureType.GBuffer2);
		}
		Vector4 value = new Vector4(this.m_target.oneOverWidth, this.m_target.oneOverHeight, (float)this.m_target.width, (float)this.m_target.height);
		int num = (int)(this.SampleCount * (AmplifyOcclusionBase.SampleCountLevel)AmplifyOcclusionBase.PerPixelNormalSourceCount);
		int pass = (int)(num + this.PerPixelNormals);
		if (this.Downsample)
		{
			int num2 = this.safeAllocateTemporaryRT(cb, "_AO_SmallOcclusionTexture", this.m_target.width / 2, this.m_target.height / 2, this.m_occlusionRTFormat, RenderTextureReadWrite.Linear, FilterMode.Bilinear);
			cb.SetGlobalVector(AmplifyOcclusionBase.PropertyID._AO_Target_TexelSize, new Vector4(1f / ((float)this.m_target.width / 2f), 1f / ((float)this.m_target.height / 2f), (float)this.m_target.width / 2f, (float)this.m_target.height / 2f));
			cb.SetRenderTarget(num2);
			this.PerformBlit(cb, AmplifyOcclusionBase.m_occlusionMat, pass);
			cb.SetRenderTarget(null);
			this.EndSample(cb, "AO 1 - ComputeOcclusion");
			this.commandBuffer_Blur(cb, num2, this.m_target.width / 2, this.m_target.height / 2);
			this.BeginSample(cb, "AO 2b - Combine");
			cb.SetGlobalTexture(AmplifyOcclusionBase.PropertyID._AO_Source, num2);
			cb.SetGlobalVector(AmplifyOcclusionBase.PropertyID._AO_Target_TexelSize, value);
			cb.SetRenderTarget(this.m_occlusionDepthRT);
			this.PerformBlit(cb, AmplifyOcclusionBase.m_occlusionMat, 16);
			this.safeReleaseTemporaryRT(cb, num2);
			cb.SetRenderTarget(null);
			this.EndSample(cb, "AO 2b - Combine");
		}
		else
		{
			cb.SetGlobalVector(AmplifyOcclusionBase.PropertyID._AO_Source_TexelSize, value);
			cb.SetGlobalVector(AmplifyOcclusionBase.PropertyID._AO_Target_TexelSize, value);
			cb.SetRenderTarget(this.m_occlusionDepthRT);
			this.PerformBlit(cb, AmplifyOcclusionBase.m_occlusionMat, pass);
			cb.SetRenderTarget(null);
			this.EndSample(cb, "AO 1 - ComputeOcclusion");
			if (this.BlurEnabled)
			{
				this.commandBuffer_Blur(cb, this.m_occlusionDepthRT, this.m_target.width, this.m_target.height);
			}
		}
	}

	private void commandBuffer_Blur(CommandBuffer cb, RenderTargetIdentifier aSourceRT, int aSourceWidth, int aSourceHeight)
	{
		this.BeginSample(cb, "AO 2 - Blur");
		int num = this.safeAllocateTemporaryRT(cb, "_AO_BlurTmp", aSourceWidth, aSourceHeight, this.m_occlusionRTFormat, RenderTextureReadWrite.Linear, FilterMode.Point);
		for (int i = 0; i < this.BlurPasses; i++)
		{
			cb.SetGlobalTexture(AmplifyOcclusionBase.PropertyID._AO_Source, aSourceRT);
			int pass = (this.BlurRadius - 1) * 2;
			cb.SetRenderTarget(num);
			this.PerformBlit(cb, AmplifyOcclusionBase.m_blurMat, pass);
			cb.SetGlobalTexture(AmplifyOcclusionBase.PropertyID._AO_Source, num);
			int pass2 = 1 + (this.BlurRadius - 1) * 2;
			cb.SetRenderTarget(aSourceRT);
			this.PerformBlit(cb, AmplifyOcclusionBase.m_blurMat, pass2);
		}
		this.safeReleaseTemporaryRT(cb, num);
		cb.SetRenderTarget(null);
		this.EndSample(cb, "AO 2 - Blur");
	}

	private int getTemporalPass()
	{
		return ((!this.UsingMotionVectors) ? 0 : 2) | ((!this.TemporalDilation) ? 0 : 1);
	}

	private void commandBuffer_TemporalFilter(CommandBuffer cb)
	{
		float value = Mathf.Lerp(0.01f, 0.99f, this.FilterBlending);
		float num = AmplifyOcclusionBase.m_temporalRotations[(int)((UIntPtr)(this.m_sampleStep % 6u))];
		float num2 = AmplifyOcclusionBase.m_spatialOffsets[(int)((UIntPtr)(this.m_sampleStep / 6u % 4u))];
		cb.SetGlobalFloat(AmplifyOcclusionBase.PropertyID._AO_TemporalCurveAdj, value);
		cb.SetGlobalFloat(AmplifyOcclusionBase.PropertyID._AO_TemporalMotionSensibility, this.FilterResponse * this.FilterResponse + 0.01f);
		cb.SetGlobalTexture(AmplifyOcclusionBase.PropertyID._AO_CurrOcclusionDepth, this.m_occlusionDepthRT);
		cb.SetGlobalTexture(AmplifyOcclusionBase.PropertyID._AO_TemporalAccumm, this.m_temporalAccumRT[(int)((UIntPtr)(1u - this.m_curStepIdx))]);
		cb.SetGlobalFloat(AmplifyOcclusionBase.PropertyID._AO_TemporalDirections, (!this.FilterEnabled) ? 0f : (num / 360f));
		cb.SetGlobalFloat(AmplifyOcclusionBase.PropertyID._AO_TemporalOffsets, (!this.FilterEnabled) ? 0f : num2);
	}

	private void commandBuffer_FillApplyDeferred(CommandBuffer cb, bool logTarget)
	{
		this.BeginSample(cb, "AO 3 - ApplyDeferred");
		if (!logTarget)
		{
			if (this.UsingTemporalFilter)
			{
				this.commandBuffer_TemporalFilter(cb);
				int num = 0;
				if (this.useMRTBlendingFallback)
				{
					num = this.safeAllocateTemporaryRT(cb, "_AO_ApplyOcclusionTexture", this.m_target.fullWidth, this.m_target.fullHeight, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, FilterMode.Point);
					this.applyOcclusionTemporal[0] = num;
					this.applyOcclusionTemporal[1] = new RenderTargetIdentifier(this.m_temporalAccumRT[(int)((UIntPtr)this.m_curStepIdx)]);
					cb.SetRenderTarget(this.applyOcclusionTemporal, this.applyOcclusionTemporal[0]);
					this.PerformBlit(cb, AmplifyOcclusionBase.m_applyOcclusionMat, 16 + this.getTemporalPass());
				}
				else
				{
					this.applyDeferredTargetsTemporal[0] = this.m_applyDeferredTargets[0];
					this.applyDeferredTargetsTemporal[1] = this.m_applyDeferredTargets[1];
					this.applyDeferredTargetsTemporal[2] = new RenderTargetIdentifier(this.m_temporalAccumRT[(int)((UIntPtr)this.m_curStepIdx)]);
					cb.SetRenderTarget(this.applyDeferredTargetsTemporal, this.applyDeferredTargetsTemporal[0]);
					this.PerformBlit(cb, AmplifyOcclusionBase.m_applyOcclusionMat, 6 + this.getTemporalPass());
				}
				if (this.useMRTBlendingFallback)
				{
					cb.SetGlobalTexture("_AO_ApplyOcclusionTexture", num);
					this.applyOcclusionTemporal[0] = this.m_applyDeferredTargets[0];
					this.applyOcclusionTemporal[1] = this.m_applyDeferredTargets[1];
					cb.SetRenderTarget(this.applyOcclusionTemporal, this.applyOcclusionTemporal[0]);
					this.PerformBlit(cb, AmplifyOcclusionBase.m_applyOcclusionMat, 21);
					this.safeReleaseTemporaryRT(cb, num);
				}
			}
			else
			{
				cb.SetGlobalTexture(AmplifyOcclusionBase.PropertyID._AO_OcclusionTexture, this.m_occlusionDepthRT);
				cb.SetRenderTarget(this.m_applyDeferredTargets, this.m_applyDeferredTargets[0]);
				this.PerformBlit(cb, AmplifyOcclusionBase.m_applyOcclusionMat, 5);
			}
		}
		else
		{
			int num2 = this.safeAllocateTemporaryRT(cb, "_AO_tmpAlbedo", this.m_target.fullWidth, this.m_target.fullHeight, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, FilterMode.Point);
			int num3 = this.safeAllocateTemporaryRT(cb, "_AO_tmpEmission", this.m_target.fullWidth, this.m_target.fullHeight, this.m_temporaryEmissionRTFormat, RenderTextureReadWrite.Default, FilterMode.Point);
			cb.Blit(BuiltinRenderTextureType.GBuffer0, num2);
			cb.Blit(BuiltinRenderTextureType.GBuffer3, num3);
			cb.SetGlobalTexture(AmplifyOcclusionBase.PropertyID._AO_GBufferAlbedo, num2);
			cb.SetGlobalTexture(AmplifyOcclusionBase.PropertyID._AO_GBufferEmission, num3);
			if (this.UsingTemporalFilter)
			{
				this.commandBuffer_TemporalFilter(cb);
				this.applyDeferredTargets_Log_Temporal[0] = this.m_applyDeferredTargets_Log[0];
				this.applyDeferredTargets_Log_Temporal[1] = this.m_applyDeferredTargets_Log[1];
				this.applyDeferredTargets_Log_Temporal[2] = new RenderTargetIdentifier(this.m_temporalAccumRT[(int)((UIntPtr)this.m_curStepIdx)]);
				cb.SetRenderTarget(this.applyDeferredTargets_Log_Temporal, this.applyDeferredTargets_Log_Temporal[0]);
				this.PerformBlit(cb, AmplifyOcclusionBase.m_applyOcclusionMat, 11 + this.getTemporalPass());
			}
			else
			{
				cb.SetGlobalTexture(AmplifyOcclusionBase.PropertyID._AO_OcclusionTexture, this.m_occlusionDepthRT);
				cb.SetRenderTarget(this.m_applyDeferredTargets_Log, this.m_applyDeferredTargets_Log[0]);
				this.PerformBlit(cb, AmplifyOcclusionBase.m_applyOcclusionMat, 10);
			}
			this.safeReleaseTemporaryRT(cb, num2);
			this.safeReleaseTemporaryRT(cb, num3);
		}
		cb.SetRenderTarget(null);
		this.EndSample(cb, "AO 3 - ApplyDeferred");
	}

	private void commandBuffer_FillApplyPostEffect(CommandBuffer cb)
	{
		this.BeginSample(cb, "AO 3 - ApplyPostEffect");
		if (this.UsingTemporalFilter)
		{
			this.commandBuffer_TemporalFilter(cb);
			int num = 0;
			if (this.useMRTBlendingFallback)
			{
				num = this.safeAllocateTemporaryRT(cb, "_AO_ApplyOcclusionTexture", this.m_target.fullWidth, this.m_target.fullHeight, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, FilterMode.Point);
				this.applyPostEffectTargetsTemporal[0] = num;
			}
			else
			{
				this.applyPostEffectTargetsTemporal[0] = BuiltinRenderTextureType.CameraTarget;
			}
			this.applyPostEffectTargetsTemporal[1] = new RenderTargetIdentifier(this.m_temporalAccumRT[(int)((UIntPtr)this.m_curStepIdx)]);
			cb.SetRenderTarget(this.applyPostEffectTargetsTemporal, this.applyPostEffectTargetsTemporal[0]);
			this.PerformBlit(cb, AmplifyOcclusionBase.m_applyOcclusionMat, 16 + this.getTemporalPass());
			if (this.useMRTBlendingFallback)
			{
				cb.SetGlobalTexture("_AO_ApplyOcclusionTexture", num);
				cb.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
				this.PerformBlit(cb, AmplifyOcclusionBase.m_applyOcclusionMat, 20);
				this.safeReleaseTemporaryRT(cb, num);
			}
		}
		else
		{
			cb.SetGlobalTexture(AmplifyOcclusionBase.PropertyID._AO_OcclusionTexture, this.m_occlusionDepthRT);
			cb.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
			this.PerformBlit(cb, AmplifyOcclusionBase.m_applyOcclusionMat, 15);
		}
		cb.SetRenderTarget(null);
		this.EndSample(cb, "AO 3 - ApplyPostEffect");
	}

	private void commandBuffer_FillApplyDebug(CommandBuffer cb)
	{
		this.BeginSample(cb, "AO 3 - ApplyDebug");
		if (this.UsingTemporalFilter)
		{
			this.commandBuffer_TemporalFilter(cb);
			this.applyDebugTargetsTemporal[0] = BuiltinRenderTextureType.CameraTarget;
			this.applyDebugTargetsTemporal[1] = new RenderTargetIdentifier(this.m_temporalAccumRT[(int)((UIntPtr)this.m_curStepIdx)]);
			cb.SetRenderTarget(this.applyDebugTargetsTemporal, this.applyDebugTargetsTemporal[0]);
			this.PerformBlit(cb, AmplifyOcclusionBase.m_applyOcclusionMat, 1 + this.getTemporalPass());
		}
		else
		{
			cb.SetGlobalTexture(AmplifyOcclusionBase.PropertyID._AO_OcclusionTexture, this.m_occlusionDepthRT);
			cb.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
			this.PerformBlit(cb, AmplifyOcclusionBase.m_applyOcclusionMat, 0);
		}
		cb.SetRenderTarget(null);
		this.EndSample(cb, "AO 3 - ApplyDebug");
	}

	private bool isStereoSinglePassEnabled()
	{
		return false;
	}

	private void UpdateGlobalShaderConstants()
	{
		this.m_target.fullWidth = this.m_targetCamera.pixelWidth;
		this.m_target.fullHeight = this.m_targetCamera.pixelHeight;
		this.m_target.width = this.m_target.fullWidth;
		this.m_target.height = this.m_target.fullHeight;
		this.m_target.oneOverWidth = 1f / (float)this.m_target.width;
		this.m_target.oneOverHeight = 1f / (float)this.m_target.height;
		float num = this.m_targetCamera.fieldOfView * 0.0174532924f;
		float num2 = 1f / Mathf.Tan(num * 0.5f);
		Vector2 vector = new Vector2(num2 * ((float)this.m_target.height / (float)this.m_target.width), num2);
		Vector2 vector2 = new Vector2(1f / vector.x, 1f / vector.y);
		Shader.SetGlobalVector(AmplifyOcclusionBase.PropertyID._AO_UVToView, new Vector4(2f * vector2.x, 2f * vector2.y, -1f * vector2.x, -1f * vector2.y));
		float num3;
		if (this.m_targetCamera.orthographic)
		{
			num3 = (float)this.m_target.height / this.m_targetCamera.orthographicSize;
		}
		else
		{
			num3 = (float)this.m_target.height / (Mathf.Tan(num * 0.5f) * 2f);
		}
		if (this.Downsample)
		{
			num3 = num3 * 0.5f * 0.5f;
		}
		else
		{
			num3 *= 0.5f;
		}
		Shader.SetGlobalFloat(AmplifyOcclusionBase.PropertyID._AO_HalfProjScale, num3);
		if (this.FadeEnabled)
		{
			this.FadeStart = Mathf.Max(0f, this.FadeStart);
			this.FadeLength = Mathf.Max(0.01f, this.FadeLength);
			float y = 1f / this.FadeLength;
			Shader.SetGlobalVector(AmplifyOcclusionBase.PropertyID._AO_FadeParams, new Vector2(this.FadeStart, y));
			float num4 = 1f - this.FadeToThickness;
			Shader.SetGlobalVector(AmplifyOcclusionBase.PropertyID._AO_FadeValues, new Vector4(this.FadeToIntensity, this.FadeToRadius, this.FadeToPowerExponent, (1f - num4 * num4) * 0.98f));
			Shader.SetGlobalColor(AmplifyOcclusionBase.PropertyID._AO_FadeToTint, new Color(this.FadeToTint.r, this.FadeToTint.g, this.FadeToTint.b, 0f));
		}
		else
		{
			Shader.SetGlobalVector(AmplifyOcclusionBase.PropertyID._AO_FadeParams, new Vector2(0f, 0f));
		}
	}

	private void UpdateGlobalShaderConstants_AmbientOcclusion()
	{
		Shader.SetGlobalFloat(AmplifyOcclusionBase.PropertyID._AO_Radius, this.Radius);
		Shader.SetGlobalFloat(AmplifyOcclusionBase.PropertyID._AO_PowExponent, this.PowerExponent);
		Shader.SetGlobalFloat(AmplifyOcclusionBase.PropertyID._AO_Bias, this.Bias * this.Bias);
		Shader.SetGlobalColor(AmplifyOcclusionBase.PropertyID._AO_Levels, new Color(this.Tint.r, this.Tint.g, this.Tint.b, this.Intensity));
		float num = 1f - this.Thickness;
		Shader.SetGlobalFloat(AmplifyOcclusionBase.PropertyID._AO_ThicknessDecay, (1f - num * num) * 0.98f);
		if (this.BlurEnabled)
		{
			Shader.SetGlobalFloat(AmplifyOcclusionBase.PropertyID._AO_BlurSharpness, this.BlurSharpness * 100f);
		}
	}

	private void UpdateGlobalShaderConstants_Matrices()
	{
		if (this.isStereoSinglePassEnabled())
		{
			Matrix4x4 stereoViewMatrix = this.m_targetCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
			Matrix4x4 stereoViewMatrix2 = this.m_targetCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
			Shader.SetGlobalMatrix(AmplifyOcclusionBase.PropertyID._AO_CameraViewLeft, stereoViewMatrix);
			Shader.SetGlobalMatrix(AmplifyOcclusionBase.PropertyID._AO_CameraViewRight, stereoViewMatrix2);
			Matrix4x4 stereoProjectionMatrix = this.m_targetCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
			Matrix4x4 stereoProjectionMatrix2 = this.m_targetCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
			Matrix4x4 gpuprojectionMatrix = GL.GetGPUProjectionMatrix(stereoProjectionMatrix, false);
			Matrix4x4 gpuprojectionMatrix2 = GL.GetGPUProjectionMatrix(stereoProjectionMatrix2, false);
			Shader.SetGlobalMatrix(AmplifyOcclusionBase.PropertyID._AO_ProjMatrixLeft, gpuprojectionMatrix);
			Shader.SetGlobalMatrix(AmplifyOcclusionBase.PropertyID._AO_ProjMatrixRight, gpuprojectionMatrix2);
			if (this.UsingTemporalFilter)
			{
				Matrix4x4 matrix4x = gpuprojectionMatrix * stereoViewMatrix;
				Matrix4x4 matrix4x2 = gpuprojectionMatrix2 * stereoViewMatrix2;
				Matrix4x4 matrix4x3 = Matrix4x4.Inverse(matrix4x);
				Matrix4x4 matrix4x4 = Matrix4x4.Inverse(matrix4x2);
				Shader.SetGlobalMatrix(AmplifyOcclusionBase.PropertyID._AO_InvViewProjMatrixLeft, matrix4x3);
				Shader.SetGlobalMatrix(AmplifyOcclusionBase.PropertyID._AO_PrevViewProjMatrixLeft, this.m_prevViewProjMatrixLeft);
				Shader.SetGlobalMatrix(AmplifyOcclusionBase.PropertyID._AO_PrevInvViewProjMatrixLeft, this.m_prevInvViewProjMatrixLeft);
				Shader.SetGlobalMatrix(AmplifyOcclusionBase.PropertyID._AO_InvViewProjMatrixRight, matrix4x4);
				Shader.SetGlobalMatrix(AmplifyOcclusionBase.PropertyID._AO_PrevViewProjMatrixRight, this.m_prevViewProjMatrixRight);
				Shader.SetGlobalMatrix(AmplifyOcclusionBase.PropertyID._AO_PrevInvViewProjMatrixRight, this.m_prevInvViewProjMatrixRight);
				this.m_prevViewProjMatrixLeft = matrix4x;
				this.m_prevInvViewProjMatrixLeft = matrix4x3;
				this.m_prevViewProjMatrixRight = matrix4x2;
				this.m_prevInvViewProjMatrixRight = matrix4x4;
			}
		}
		else
		{
			Matrix4x4 worldToCameraMatrix = this.m_targetCamera.worldToCameraMatrix;
			Shader.SetGlobalMatrix(AmplifyOcclusionBase.PropertyID._AO_CameraViewLeft, worldToCameraMatrix);
			if (this.UsingTemporalFilter)
			{
				Matrix4x4 gpuprojectionMatrix3 = GL.GetGPUProjectionMatrix(this.m_targetCamera.projectionMatrix, false);
				Matrix4x4 matrix4x5 = gpuprojectionMatrix3 * worldToCameraMatrix;
				Matrix4x4 matrix4x6 = Matrix4x4.Inverse(matrix4x5);
				Shader.SetGlobalMatrix(AmplifyOcclusionBase.PropertyID._AO_InvViewProjMatrixLeft, matrix4x6);
				Shader.SetGlobalMatrix(AmplifyOcclusionBase.PropertyID._AO_PrevViewProjMatrixLeft, this.m_prevViewProjMatrixLeft);
				Shader.SetGlobalMatrix(AmplifyOcclusionBase.PropertyID._AO_PrevInvViewProjMatrixLeft, this.m_prevInvViewProjMatrixLeft);
				this.m_prevViewProjMatrixLeft = matrix4x5;
				this.m_prevInvViewProjMatrixLeft = matrix4x6;
			}
		}
	}

	[Header("Ambient Occlusion")]
	[Tooltip("How to inject the occlusion: Post Effect = Overlay, Deferred = Deferred Injection, Debug - Vizualize.")]
	public AmplifyOcclusionBase.ApplicationMethod ApplyMethod;

	[Tooltip("Number of samples per pass.")]
	public AmplifyOcclusionBase.SampleCountLevel SampleCount = AmplifyOcclusionBase.SampleCountLevel.High;

	[Tooltip("Source of per-pixel normals: None = All, Camera = Forward, GBuffer = Deferred.")]
	public AmplifyOcclusionBase.PerPixelNormalSource PerPixelNormals = AmplifyOcclusionBase.PerPixelNormalSource.Camera;

	[Tooltip("Final applied intensity of the occlusion effect.")]
	[Range(0f, 1f)]
	public float Intensity = 1f;

	[Tooltip("Color tint for occlusion.")]
	public Color Tint = Color.black;

	[Tooltip("Radius spread of the occlusion.")]
	[Range(0f, 32f)]
	public float Radius = 2f;

	[Tooltip("Power exponent attenuation of the occlusion.")]
	[Range(0f, 16f)]
	public float PowerExponent = 1.8f;

	[Tooltip("Controls the initial occlusion contribution offset.")]
	[Range(0f, 0.99f)]
	public float Bias = 0.05f;

	[Tooltip("Controls the thickness occlusion contribution.")]
	[Range(0f, 1f)]
	public float Thickness = 1f;

	[Tooltip("Compute the Occlusion and Blur at half of the resolution.")]
	public bool Downsample = true;

	[Header("Distance Fade")]
	[Tooltip("Control parameters at faraway.")]
	public bool FadeEnabled;

	[Tooltip("Distance in Unity unities that start to fade.")]
	public float FadeStart = 100f;

	[Tooltip("Length distance to performe the transition.")]
	public float FadeLength = 50f;

	[Tooltip("Final Intensity parameter.")]
	[Range(0f, 1f)]
	public float FadeToIntensity;

	public Color FadeToTint = Color.black;

	[Tooltip("Final Radius parameter.")]
	[Range(0f, 32f)]
	public float FadeToRadius = 2f;

	[Tooltip("Final PowerExponent parameter.")]
	[Range(0f, 16f)]
	public float FadeToPowerExponent = 1f;

	[Tooltip("Final Thickness parameter.")]
	[Range(0f, 1f)]
	public float FadeToThickness = 1f;

	[Header("Bilateral Blur")]
	public bool BlurEnabled = true;

	[Tooltip("Radius in screen pixels.")]
	[Range(1f, 4f)]
	public int BlurRadius = 3;

	[Tooltip("Number of times that the Blur will repeat.")]
	[Range(1f, 4f)]
	public int BlurPasses = 2;

	[Tooltip("Sharpness of blur edge-detection: 0 = Softer Edges, 20 = Sharper Edges.")]
	[Range(0f, 20f)]
	public float BlurSharpness = 15f;

	[Header("Temporal Filter")]
	[Tooltip("Accumulates the effect over the time.")]
	public bool FilterEnabled = true;

	[Tooltip("Controls the accumulation decayment: 0 = More flicker with less ghosting, 1 = Less flicker with more ghosting.")]
	[Range(0f, 1f)]
	public float FilterBlending = 0.75f;

	[Tooltip("Controls the discard sensitivity based on the motion of the scene and objects.")]
	[Range(0f, 1f)]
	public float FilterResponse = 0.5f;

	[Tooltip("Reduces ghosting effect near the objects's edges while moving.")]
	private bool TemporalDilation;

	private bool m_HDR = true;

	private bool m_MSAA = true;

	private AmplifyOcclusionBase.PerPixelNormalSource m_prevPerPixelNormals;

	private AmplifyOcclusionBase.ApplicationMethod m_prevApplyMethod;

	private bool m_prevDeferredReflections;

	private AmplifyOcclusionBase.SampleCountLevel m_prevSampleCount;

	private bool m_prevDownsample;

	private bool m_prevBlurEnabled;

	private int m_prevBlurRadius;

	private int m_prevBlurPasses;

	private bool m_prevFilterEnabled = true;

	private bool m_prevHDR = true;

	private bool m_prevMSAA = true;

	private Camera m_targetCamera;

	private RenderTargetIdentifier[] applyDebugTargetsTemporal = new RenderTargetIdentifier[2];

	private RenderTargetIdentifier[] applyDeferredTargets_Log_Temporal = new RenderTargetIdentifier[3];

	private RenderTargetIdentifier[] applyDeferredTargetsTemporal = new RenderTargetIdentifier[3];

	private RenderTargetIdentifier[] applyOcclusionTemporal = new RenderTargetIdentifier[2];

	private RenderTargetIdentifier[] applyPostEffectTargetsTemporal = new RenderTargetIdentifier[2];

	private bool useMRTBlendingFallback;

	private AmplifyOcclusionBase.CmdBuffer m_commandBuffer_Occlusion;

	private AmplifyOcclusionBase.CmdBuffer m_commandBuffer_Apply;

	private static Mesh m_quadMesh = null;

	private static Material m_occlusionMat = null;

	private static Material m_blurMat = null;

	private static Material m_applyOcclusionMat = null;

	private RenderTextureFormat m_occlusionRTFormat = RenderTextureFormat.RGHalf;

	private RenderTextureFormat m_accumTemporalRTFormat;

	private RenderTextureFormat m_temporaryEmissionRTFormat;

	private bool m_paramsChanged = true;

	private RenderTexture m_occlusionDepthRT;

	private RenderTexture[] m_temporalAccumRT;

	private uint m_sampleStep;

	private uint m_curStepIdx;

	private static readonly int PerPixelNormalSourceCount = 4;

	private Matrix4x4 m_prevViewProjMatrixLeft = Matrix4x4.identity;

	private Matrix4x4 m_prevInvViewProjMatrixLeft = Matrix4x4.identity;

	private Matrix4x4 m_prevViewProjMatrixRight = Matrix4x4.identity;

	private Matrix4x4 m_prevInvViewProjMatrixRight = Matrix4x4.identity;

	private static readonly float[] m_temporalRotations = new float[]
	{
		60f,
		300f,
		180f,
		240f,
		120f,
		0f
	};

	private static readonly float[] m_spatialOffsets = new float[]
	{
		0f,
		0.5f,
		0.25f,
		0.75f
	};

	private readonly RenderTargetIdentifier[] m_applyDeferredTargets = new RenderTargetIdentifier[]
	{
		BuiltinRenderTextureType.GBuffer0,
		BuiltinRenderTextureType.CameraTarget
	};

	private readonly RenderTargetIdentifier[] m_applyDeferredTargets_Log = new RenderTargetIdentifier[]
	{
		BuiltinRenderTextureType.GBuffer0,
		BuiltinRenderTextureType.GBuffer3
	};

	private AmplifyOcclusionBase.TargetDesc m_target = default(AmplifyOcclusionBase.TargetDesc);

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

	private struct CmdBuffer
	{
		public CommandBuffer cmdBuffer;

		public CameraEvent cmdBufferEvent;

		public string cmdBufferName;
	}

	private struct TargetDesc
	{
		public int fullWidth;

		public int fullHeight;

		public int width;

		public int height;

		public float oneOverWidth;

		public float oneOverHeight;
	}

	private static class ShaderPass
	{
		public const int CombineDownsampledOcclusionDepth = 16;

		public const int BlurHorizontal1 = 0;

		public const int BlurVertical1 = 1;

		public const int BlurHorizontal2 = 2;

		public const int BlurVertical2 = 3;

		public const int BlurHorizontal3 = 4;

		public const int BlurVertical3 = 5;

		public const int BlurHorizontal4 = 6;

		public const int BlurVertical4 = 7;

		public const int ApplyDebug = 0;

		public const int ApplyDebugTemporal = 1;

		public const int ApplyDeferred = 5;

		public const int ApplyDeferredTemporal = 6;

		public const int ApplyDeferredLog = 10;

		public const int ApplyDeferredLogTemporal = 11;

		public const int ApplyPostEffect = 15;

		public const int ApplyPostEffectTemporal = 16;

		public const int ApplyPostEffectTemporalMultiply = 20;

		public const int ApplyDeferredTemporalMultiply = 21;

		public const int OcclusionLow_None = 0;

		public const int OcclusionLow_Camera = 1;

		public const int OcclusionLow_GBuffer = 2;

		public const int OcclusionLow_GBufferOctaEncoded = 3;
	}

	private static class PropertyID
	{
		public static readonly int _AO_Radius = Shader.PropertyToID("_AO_Radius");

		public static readonly int _AO_PowExponent = Shader.PropertyToID("_AO_PowExponent");

		public static readonly int _AO_Bias = Shader.PropertyToID("_AO_Bias");

		public static readonly int _AO_Levels = Shader.PropertyToID("_AO_Levels");

		public static readonly int _AO_ThicknessDecay = Shader.PropertyToID("_AO_ThicknessDecay");

		public static readonly int _AO_BlurSharpness = Shader.PropertyToID("_AO_BlurSharpness");

		public static readonly int _AO_CameraViewLeft = Shader.PropertyToID("_AO_CameraViewLeft");

		public static readonly int _AO_CameraViewRight = Shader.PropertyToID("_AO_CameraViewRight");

		public static readonly int _AO_ProjMatrixLeft = Shader.PropertyToID("_AO_ProjMatrixLeft");

		public static readonly int _AO_ProjMatrixRight = Shader.PropertyToID("_AO_ProjMatrixRight");

		public static readonly int _AO_InvViewProjMatrixLeft = Shader.PropertyToID("_AO_InvViewProjMatrixLeft");

		public static readonly int _AO_PrevViewProjMatrixLeft = Shader.PropertyToID("_AO_PrevViewProjMatrixLeft");

		public static readonly int _AO_PrevInvViewProjMatrixLeft = Shader.PropertyToID("_AO_PrevInvViewProjMatrixLeft");

		public static readonly int _AO_InvViewProjMatrixRight = Shader.PropertyToID("_AO_InvViewProjMatrixRight");

		public static readonly int _AO_PrevViewProjMatrixRight = Shader.PropertyToID("_AO_PrevViewProjMatrixRight");

		public static readonly int _AO_PrevInvViewProjMatrixRight = Shader.PropertyToID("_AO_PrevInvViewProjMatrixRight");

		public static readonly int _AO_GBufferNormals = Shader.PropertyToID("_AO_GBufferNormals");

		public static readonly int _AO_Target_TexelSize = Shader.PropertyToID("_AO_Target_TexelSize");

		public static readonly int _AO_TemporalCurveAdj = Shader.PropertyToID("_AO_TemporalCurveAdj");

		public static readonly int _AO_TemporalMotionSensibility = Shader.PropertyToID("_AO_TemporalMotionSensibility");

		public static readonly int _AO_CurrOcclusionDepth = Shader.PropertyToID("_AO_CurrOcclusionDepth");

		public static readonly int _AO_TemporalAccumm = Shader.PropertyToID("_AO_TemporalAccumm");

		public static readonly int _AO_TemporalDirections = Shader.PropertyToID("_AO_TemporalDirections");

		public static readonly int _AO_TemporalOffsets = Shader.PropertyToID("_AO_TemporalOffsets");

		public static readonly int _AO_OcclusionTexture = Shader.PropertyToID("_AO_OcclusionTexture");

		public static readonly int _AO_GBufferAlbedo = Shader.PropertyToID("_AO_GBufferAlbedo");

		public static readonly int _AO_GBufferEmission = Shader.PropertyToID("_AO_GBufferEmission");

		public static readonly int _AO_UVToView = Shader.PropertyToID("_AO_UVToView");

		public static readonly int _AO_HalfProjScale = Shader.PropertyToID("_AO_HalfProjScale");

		public static readonly int _AO_FadeParams = Shader.PropertyToID("_AO_FadeParams");

		public static readonly int _AO_FadeValues = Shader.PropertyToID("_AO_FadeValues");

		public static readonly int _AO_FadeToTint = Shader.PropertyToID("_AO_FadeToTint");

		public static readonly int _AO_Source_TexelSize = Shader.PropertyToID("_AO_Source_TexelSize");

		public static readonly int _AO_Source = Shader.PropertyToID("_AO_Source");
	}
}
