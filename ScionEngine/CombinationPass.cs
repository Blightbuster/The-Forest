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

		
		private void PrepareBloomSampling(RenderTexture bloomTexture, GlareParameters glareParams)
		{
			this.m_combinationMat.SetTexture("_BloomTexture", bloomTexture);
			Vector4 value = default(Vector4);
			value.x = glareParams.intensity;
			value.y = glareParams.brightness;
			value.z = glareParams.bloomNormalizationTerm;
			value.w = glareParams.threshold;
			this.m_combinationMat.SetVector("_GlareParameters", value);
		}

		
		private void PrepareLensDirtSampling(Texture lensDirtTexture, LensDirtParameters lensDirtParams, GlareParameters glareParams)
		{
			this.m_combinationMat.SetTexture("_LensDirtTexture", lensDirtTexture);
			Vector4 value = default(Vector4);
			value.x = lensDirtParams.bloomEffect;
			value.y = lensDirtParams.bloomBrightness * glareParams.brightness;
			value.z = lensDirtParams.lensFlareEffect;
			value.w = lensDirtParams.lensFlareBrightness;
			this.m_combinationMat.SetVector("_LensDirtParameters", value);
		}

		
		private void PrepareLensFlareSampling(LensFlareParameters lensFlareParams, RenderTexture lensFlareTexture, Transform cameraTransform)
		{
			Vector3 lhs = -cameraTransform.right;
			Vector3 forward = cameraTransform.forward;
			float num = Vector3.Dot(lhs, Vector3.forward) + Vector3.Dot(forward, Vector3.up);
			Vector4 value = default(Vector4);
			value.x = Mathf.Cos(num * 2f * 3.14159274f);
			value.y = Mathf.Sin(num * 2f * 3.14159274f);
			value.z = lensFlareParams.starUVScale;
			value.w = (1f - lensFlareParams.starUVScale) * 0.5f;
			this.m_combinationMat.SetVector("_LensStarParams1", value);
			if (lensFlareParams.starTexture != null)
			{
				this.m_combinationMat.SetTexture("_LensFlareStarTexture", lensFlareParams.starTexture);
			}
			else
			{
				this.m_combinationMat.SetTexture("_LensFlareStarTexture", ScionUtility.WhiteTexture);
			}
			this.m_combinationMat.SetTexture("_LensFlareTexture", lensFlareTexture);
		}

		
		private void PrepareExposure(CameraParameters cameraParams, VirtualCamera virtualCamera)
		{
			if (cameraParams.cameraMode == CameraMode.Off)
			{
				this.m_combinationMat.SetFloat("_ManualExposure", 1f);
			}
			else if (cameraParams.cameraMode != CameraMode.Manual)
			{
				virtualCamera.BindVirtualCameraTextures(this.m_combinationMat);
			}
			else
			{
				this.m_combinationMat.SetFloat("_ManualExposure", virtualCamera.CalculateManualExposure(cameraParams, 0.18f));
			}
		}

		
		private bool ShouldInvertVAxis(PostProcessParameters postProcessParams)
		{
			return postProcessParams.camera.actualRenderingPath == RenderingPath.Forward && QualitySettings.antiAliasing > 0;
		}

		
		private void UploadVariables(PostProcessParameters postProcessParams)
		{
			Vector4 value = default(Vector4);
			value.x = postProcessParams.commonPostProcess.grainIntensity;
			value.y = postProcessParams.commonPostProcess.vignetteIntensity;
			value.z = postProcessParams.commonPostProcess.vignetteScale;
			value.w = postProcessParams.commonPostProcess.chromaticAberrationDistortion;
			this.m_combinationMat.SetVector("_PostProcessParams1", value);
			Vector4 value2 = default(Vector4);
			value2.x = postProcessParams.commonPostProcess.vignetteColor.r;
			value2.y = postProcessParams.commonPostProcess.vignetteColor.g;
			value2.z = postProcessParams.commonPostProcess.vignetteColor.b;
			value2.w = postProcessParams.commonPostProcess.chromaticAberrationIntensity;
			this.m_combinationMat.SetVector("_PostProcessParams2", value2);
			Vector4 value3 = default(Vector4);
			value3.x = UnityEngine.Random.value;
			value3.y = ScionUtility.GetWhitePointMultiplier(postProcessParams.commonPostProcess.whitePoint);
			value3.z = 1f / postProcessParams.commonPostProcess.whitePoint;
			this.m_combinationMat.SetVector("_PostProcessParams3", value3);
			Vector4 value4 = default(Vector4);
			value4.x = postProcessParams.lensFlareParams.threshold;
			value4.y = postProcessParams.lensDirtParams.bloomThreshold * postProcessParams.lensDirtParams.bloomBrightness;
			value4.z = postProcessParams.glareParams.threshold * postProcessParams.glareParams.brightness;
			this.m_combinationMat.SetVector("_ThresholdParams", value4);
		}

		
		private void PrepareColorGrading(ColorGradingParameters colorGradingParams)
		{
			if (colorGradingParams.colorGradingMode == ColorGradingMode.Off)
			{
				return;
			}
			this.m_combinationMat.SetTexture("_ColorGradingLUT1", colorGradingParams.colorGradingTex1);
			ColorGrading.UploadColorGradingParams(this.m_combinationMat, (float)colorGradingParams.colorGradingTex1.height);
			if (colorGradingParams.colorGradingMode == ColorGradingMode.On)
			{
				return;
			}
			this.m_combinationMat.SetTexture("_ColorGradingLUT2", colorGradingParams.colorGradingTex2);
			this.m_combinationMat.SetFloat("_ColorGradingBlendFactor", colorGradingParams.colorGradingBlendFactor);
		}

		
		public void Combine(RenderTexture source, RenderTexture dest, PostProcessParameters postProcessParams, VirtualCamera virtualCamera)
		{
			if (postProcessParams.bloom)
			{
				this.PrepareBloomSampling(postProcessParams.bloomTexture, postProcessParams.glareParams);
			}
			if (postProcessParams.lensDirt)
			{
				this.PrepareLensDirtSampling(postProcessParams.lensDirtTexture, postProcessParams.lensDirtParams, postProcessParams.glareParams);
			}
			if (postProcessParams.lensFlare)
			{
				this.PrepareLensFlareSampling(postProcessParams.lensFlareParams, postProcessParams.lensFlareTexture, postProcessParams.cameraTransform);
			}
			this.PrepareExposure(postProcessParams.cameraParams, virtualCamera);
			this.PrepareColorGrading(postProcessParams.colorGradingParams);
			this.UploadVariables(postProcessParams);
			int num = 0;
			if (!postProcessParams.tonemapping)
			{
				num += 4;
			}
			if (!postProcessParams.bloom)
			{
				num += 2;
			}
			if (!postProcessParams.lensDirt || postProcessParams.lensDirtTexture == null)
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
