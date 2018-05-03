using System;
using UnityEngine;

namespace ScionEngine
{
	
	public class CombinationPass
	{
		
		public CombinationPass()
		{
			this.m_combinationMat = new Material(Shader.Find("Hidden/ScionCombinationPass"));
			this.m_combinationMat.hideFlags = HideFlags.HideAndDontSave;
		}

		
		public void ReleaseResources()
		{
			if (this.m_combinationMat != null)
			{
				UnityEngine.Object.Destroy(this.m_combinationMat);
				this.m_combinationMat = null;
			}
		}

		
		public bool PlatformCompatibility()
		{
			return Shader.Find("Hidden/ScionCombinationPass").isSupported;
		}

		
		private void PrepareBloomSampling(RenderTexture bloomTexture, BloomParameters bloomParams)
		{
			this.m_combinationMat.SetTexture("_BloomTexture", bloomTexture);
			Vector4 value = default(Vector4);
			value.x = ((bloomParams.intensity <= 0.0001f) ? 0.0001f : bloomParams.intensity);
			value.y = bloomParams.brightness;
			this.m_combinationMat.SetVector("_BloomParameters", value);
		}

		
		private void PrepareLensDirtSampling(Texture lensDirtTexture, LensDirtParameters lensDirtParams)
		{
			this.m_combinationMat.SetTexture("_LensDirtTexture", lensDirtTexture);
			Vector4 value = default(Vector4);
			value.x = ((lensDirtParams.intensity <= 0.0001f) ? 0.0001f : lensDirtParams.intensity);
			value.y = lensDirtParams.brightness;
			this.m_combinationMat.SetVector("_LensDirtParameters", value);
		}

		
		private void PrepareExposure(CameraParameters cameraParams, VirtualCamera virtualCamera)
		{
			if (cameraParams.cameraMode == CameraMode.Off)
			{
				this.m_combinationMat.SetFloat("_ManualExposure", 1f);
			}
			else if (cameraParams.cameraMode != CameraMode.Manual)
			{
				virtualCamera.BindExposureTexture(this.m_combinationMat);
			}
			else
			{
				this.m_combinationMat.SetFloat("_ManualExposure", virtualCamera.CalculateManualExposure(cameraParams, 0.18f));
			}
		}

		
		private void UploadVariables(CommonPostProcess commonPostProcess)
		{
			Vector4 value = default(Vector4);
			value.x = commonPostProcess.grainIntensity;
			value.y = commonPostProcess.vignetteIntensity;
			value.z = commonPostProcess.vignetteScale;
			value.w = commonPostProcess.chromaticAberrationDistortion;
			this.m_combinationMat.SetVector("_PostProcessParams1", value);
			Vector4 value2 = default(Vector4);
			value2.x = commonPostProcess.vignetteColor.r;
			value2.y = commonPostProcess.vignetteColor.g;
			value2.z = commonPostProcess.vignetteColor.b;
			value2.w = commonPostProcess.chromaticAberrationIntensity;
			this.m_combinationMat.SetVector("_PostProcessParams2", value2);
			Vector4 value3 = default(Vector4);
			value3.x = UnityEngine.Random.value;
			value3.y = 5f / commonPostProcess.whitePoint;
			value3.z = 1f / commonPostProcess.whitePoint;
			this.m_combinationMat.SetVector("_PostProcessParams3", value3);
		}

		
		private void PrepareColorGrading(ColorGradingParameters colorGradingParams)
		{
			if (colorGradingParams.colorGradingMode == ColorGradingMode.Off)
			{
				return;
			}
			this.m_combinationMat.SetTexture("_ColorGradingLUT1", colorGradingParams.colorGradingTex1);
			Vector2 vector = default(Vector2);
			float num = 32f;
			vector.x = 1024f;
			vector.y = 32f;
			float num2 = 1f / num;
			Vector4 value = default(Vector4);
			value.x = 1f * num2 - 1f / vector.x;
			value.y = 1f - 1f * num2;
			value.z = num - 1f;
			value.w = num;
			Vector4 value2 = default(Vector4);
			value2.x = 0.5f / vector.x;
			value2.y = 0.5f / vector.y;
			value2.z = 0f;
			value2.w = num2;
			this.m_combinationMat.SetVector("_ColorGradingParams1", value);
			this.m_combinationMat.SetVector("_ColorGradingParams2", value2);
		}

		
		public void Combine(RenderTexture source, RenderTexture dest, PostProcessParameters postProcessParams, VirtualCamera virtualCamera)
		{
			if (postProcessParams.bloom)
			{
				this.PrepareBloomSampling(postProcessParams.bloomTexture, postProcessParams.bloomParams);
			}
			if (postProcessParams.lensDirt)
			{
				this.PrepareLensDirtSampling(postProcessParams.lensDirtTexture, postProcessParams.lensDirtParams);
			}
			this.PrepareExposure(postProcessParams.cameraParams, virtualCamera);
			this.PrepareColorGrading(postProcessParams.colorGradingParams);
			this.UploadVariables(postProcessParams.commonPostProcess);
			int num = 0;
			if (!postProcessParams.tonemapping)
			{
				num += 3;
			}
			if (!postProcessParams.bloom)
			{
				num += 2;
			}
			else if (!postProcessParams.lensDirt)
			{
				num++;
			}
			source.filterMode = FilterMode.Bilinear;
			source.wrapMode = TextureWrapMode.Clamp;
			Graphics.Blit(source, dest, this.m_combinationMat, num);
		}

		
		private Material m_combinationMat;

		
		private const float MinValue = 0.0001f;
	}
}
