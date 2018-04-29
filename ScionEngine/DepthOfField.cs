using System;
using UnityEngine;

namespace ScionEngine
{
	
	public class DepthOfField
	{
		
		public DepthOfField()
		{
			this.m_DoFMat = new Material(Shader.Find("Hidden/ScionDepthOfField"));
			this.m_DoFMat.hideFlags = HideFlags.HideAndDontSave;
			this.m_DoFMatTemporal = new Material(Shader.Find("Hidden/ScionDepthOfFieldTemporal"));
			this.m_DoFMatTemporal.hideFlags = HideFlags.HideAndDontSave;
		}

		
		
		private Camera maskCamera
		{
			get
			{
				if (this.m_maskCamera == null)
				{
					GameObject gameObject = new GameObject();
					gameObject.SetActive(false);
					gameObject.hideFlags = HideFlags.HideAndDontSave;
					gameObject.name = "ScionDoFMaskCamera";
					this.m_maskCamera = gameObject.AddComponent<Camera>();
					this.m_maskCamera.enabled = false;
					this.m_maskCamera.hideFlags = HideFlags.HideAndDontSave;
				}
				return this.m_maskCamera;
			}
		}

		
		
		private static Shader maskShader
		{
			get
			{
				if (DepthOfField.m_maskShader == null)
				{
					DepthOfField.m_maskShader = Shader.Find("Scion/ScionDepthOfFieldMask");
				}
				return DepthOfField.m_maskShader;
			}
		}

		
		
		private Transform maskCameraTransform
		{
			get
			{
				if (this.m_maskCameraTransform == null)
				{
					this.m_maskCameraTransform = this.maskCamera.transform;
				}
				return this.m_maskCameraTransform;
			}
		}

		
		public bool PlatformCompatibility()
		{
			if (!Shader.Find("Hidden/ScionDepthOfField").isSupported)
			{
				Debug.LogWarning("Depth of Field shader not supported");
				return false;
			}
			if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8))
			{
				Debug.LogWarning("R8 texture format not supported");
				return false;
			}
			return true;
		}

		
		public void EndOfFrameCleanup()
		{
		}

		
		public void ReleaseResources()
		{
			if (this.m_maskCamera != null)
			{
				UnityEngine.Object.Destroy(this.m_maskCamera.gameObject);
				this.m_maskCamera = null;
			}
			if (this.m_DoFMat != null)
			{
				UnityEngine.Object.Destroy(this.m_DoFMat);
				this.m_DoFMat = null;
			}
			if (this.m_DoFMatTemporal != null)
			{
				UnityEngine.Object.Destroy(this.m_DoFMatTemporal);
				this.m_DoFMatTemporal = null;
			}
			if (this.previousAlphaTexture != null)
			{
				RenderTexture.ReleaseTemporary(this.previousAlphaTexture);
				this.previousAlphaTexture = null;
			}
			if (this.previousTapsTexture != null)
			{
				RenderTexture.ReleaseTemporary(this.previousTapsTexture);
				this.previousTapsTexture = null;
			}
		}

		
		public RenderTexture RenderDepthOfField(PostProcessParameters postProcessParams, RenderTexture source, RenderTexture downsampledClrDepth, VirtualCamera virtualCamera, RenderTexture exclusionMask)
		{
			if (ShaderSettings.ExposureSettings.IsActive("SC_EXPOSURE_AUTO"))
			{
				virtualCamera.BindVirtualCameraTextures(this.m_DoFMat);
			}
			virtualCamera.BindVirtualCameraParams(this.m_DoFMat, postProcessParams.cameraParams, postProcessParams.DoFParams.focalDistance, (float)postProcessParams.halfWidth, postProcessParams.isFirstRender);
			RenderTexture depthCenterAverage = null;
			if (postProcessParams.DoFParams.depthFocusMode == DepthFocusMode.PointAverage)
			{
				depthCenterAverage = this.PrepatePointAverage(postProcessParams, downsampledClrDepth);
			}
			RenderTexture renderTexture = this.CreateTiledData(downsampledClrDepth, postProcessParams.preCalcValues.tanHalfFoV, postProcessParams.cameraParams.fNumber, postProcessParams.DoFParams.focalDistance, postProcessParams.DoFParams.focalRange, postProcessParams.cameraParams.apertureDiameter, postProcessParams.cameraParams.focalLength, postProcessParams.DoFParams.maxCoCRadius, postProcessParams.cameraParams.nearPlane, postProcessParams.cameraParams.farPlane);
			RenderTexture renderTexture2 = this.TileNeighbourhoodDataGathering(renderTexture);
			RenderTexture renderTexture3 = this.PrefilterSource(downsampledClrDepth);
			RenderTexture.ReleaseTemporary(downsampledClrDepth);
			RenderTexture renderTexture4 = this.Presort(renderTexture3, renderTexture2);
			if (postProcessParams.DoFParams.useTemporal)
			{
				this.UploadTemporalReprojectionVariables(postProcessParams);
			}
			RenderTexture renderTexture5;
			RenderTexture renderTexture6;
			this.BlurTapPass(renderTexture3, renderTexture, renderTexture2, exclusionMask, depthCenterAverage, renderTexture4, postProcessParams.DoFParams.quality, out renderTexture5, out renderTexture6);
			if (postProcessParams.DoFParams.useTemporal && this.previousTapsTexture != null)
			{
				this.TemporalPass(ref renderTexture5, this.previousTapsTexture);
			}
			if (postProcessParams.DoFParams.useMedianFilter)
			{
				renderTexture5 = this.MedianFilterPass(renderTexture5);
			}
			if (postProcessParams.DoFParams.visualizeFocalDistance)
			{
				this.VisualizeFocalDistance(renderTexture5);
			}
			RenderTexture result = this.UpsampleDepthOfField(source, renderTexture5, renderTexture6, renderTexture2, exclusionMask);
			RenderTexture.ReleaseTemporary(renderTexture);
			RenderTexture.ReleaseTemporary(renderTexture2);
			RenderTexture.ReleaseTemporary(renderTexture3);
			RenderTexture.ReleaseTemporary(renderTexture4);
			if (this.copiedDepthBuffer != null)
			{
				RenderTexture.ReleaseTemporary(this.copiedDepthBuffer);
				this.copiedDepthBuffer = null;
			}
			if (this.previousTapsTexture != null)
			{
				RenderTexture.ReleaseTemporary(this.previousTapsTexture);
				this.previousTapsTexture = null;
			}
			if (this.previousAlphaTexture != null)
			{
				RenderTexture.ReleaseTemporary(this.previousAlphaTexture);
				this.previousAlphaTexture = null;
			}
			if (postProcessParams.DoFParams.useTemporal)
			{
				this.previousTapsTexture = renderTexture5;
				this.previousAlphaTexture = renderTexture6;
			}
			else
			{
				RenderTexture.ReleaseTemporary(renderTexture5);
				RenderTexture.ReleaseTemporary(renderTexture6);
			}
			return result;
		}

		
		private void VisualizeFocalDistance(RenderTexture downsampledClrDepth)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(downsampledClrDepth.width, downsampledClrDepth.height, 0, RenderTextureFormat.ARGB32);
			temporary.filterMode = FilterMode.Bilinear;
			temporary.wrapMode = TextureWrapMode.Clamp;
			this.m_DoFMat.SetTexture("_HalfResSourceDepthTexture", downsampledClrDepth);
			ScionGraphics.Blit(temporary, this.m_DoFMat, 13);
			ScionPostProcessBase.ActiveDebug.RegisterTextureForVisualization(temporary, true, false, false);
		}

		
		private void UploadTemporalReprojectionVariables(PostProcessParameters postProcessParams)
		{
			float tanHalfFoV = postProcessParams.preCalcValues.tanHalfFoV;
			float d = tanHalfFoV * postProcessParams.cameraParams.aspect;
			Vector3 forward = postProcessParams.cameraTransform.forward;
			Vector3 vector = postProcessParams.cameraTransform.right * d;
			Vector3 vector2 = postProcessParams.cameraTransform.up * tanHalfFoV;
			this.m_DoFMatTemporal.SetVector("_FrustumCornerBottomLeftVP", forward - vector - vector2);
			this.m_DoFMatTemporal.SetVector("_FrustumCornerWidthVP", vector * 2f);
			this.m_DoFMatTemporal.SetVector("_FrustumCornerHeightVP", vector2 * 2f);
			this.m_DoFMatTemporal.SetMatrix("_PreviousViewProjection", postProcessParams.cameraParams.previousViewProjection);
			float temporalBlend = postProcessParams.DoFParams.temporalBlend;
			this.m_DoFMatTemporal.SetFloat("_TemporalBlendFactor", temporalBlend);
			int temporalSteps = postProcessParams.DoFParams.temporalSteps;
			if (temporalSteps > 0)
			{
				this.temporalUVOffset += 1.372f;
				while (this.temporalUVOffset > 1.372f * (float)temporalSteps - 0.01f)
				{
					this.temporalUVOffset -= 1.372f * (float)temporalSteps;
				}
			}
			else
			{
				this.temporalUVOffset = 0f;
			}
			this.m_DoFMat.SetFloat("_TemporalUVOffset", this.temporalUVOffset);
		}

		
		private float Min(float val1, float val2)
		{
			return (val1 <= val2) ? val1 : val2;
		}

		
		private float Max(float val1, float val2)
		{
			return (val1 >= val2) ? val1 : val2;
		}

		
		private int Min(int val1, int val2)
		{
			return (val1 <= val2) ? val1 : val2;
		}

		
		private int Max(int val1, int val2)
		{
			return (val1 >= val2) ? val1 : val2;
		}

		
		private RenderTexture PrepatePointAverage(PostProcessParameters postProcessParams, RenderTexture downsampledClrDepth)
		{
			float num = this.Max(10f / (float)postProcessParams.halfWidth, postProcessParams.DoFParams.pointAverageRange);
			Vector4 value = default(Vector4);
			value.x = Mathf.Clamp01(postProcessParams.DoFParams.pointAveragePosition.x);
			value.y = Mathf.Clamp01(postProcessParams.DoFParams.pointAveragePosition.y);
			value.z = num * num;
			value.w = 1f / (num * num);
			this.m_DoFMat.SetVector("_DownsampleWeightedParams", value);
			if (this.previousPointAverage != null && !postProcessParams.isFirstRender)
			{
				this.m_DoFMat.SetFloat("_DownsampleWeightedAdaptionSpeed", 1f - Mathf.Exp(-Time.deltaTime * postProcessParams.DoFParams.depthAdaptionSpeed));
				this.m_DoFMat.SetTexture("_PreviousWeightedResult", this.previousPointAverage);
			}
			else
			{
				this.m_DoFMat.SetFloat("_DownsampleWeightedAdaptionSpeed", 1f);
				this.m_DoFMat.SetTexture("_PreviousWeightedResult", null);
			}
			downsampledClrDepth.filterMode = FilterMode.Bilinear;
			int num2 = this.Max(postProcessParams.halfWidth / 2, 1);
			int num3 = this.Max(postProcessParams.halfHeight / 2, 1);
			RenderTexture temporary = RenderTexture.GetTemporary(num2, num3, 0, RenderTextureFormat.RGHalf);
			temporary.filterMode = FilterMode.Bilinear;
			temporary.wrapMode = TextureWrapMode.Clamp;
			Graphics.Blit(downsampledClrDepth, temporary, this.m_DoFMat, 7);
			if (postProcessParams.DoFParams.visualizePointFocus)
			{
				RenderTexture temporary2 = RenderTexture.GetTemporary(num2, num3, 0, RenderTextureFormat.ARGB32);
				this.m_DoFMat.SetTexture("_HalfResSourceDepthTexture", downsampledClrDepth);
				Graphics.Blit(temporary, temporary2, this.m_DoFMat, 9);
				ScionPostProcessBase.ActiveDebug.RegisterTextureForVisualization(temporary2, true, true, false);
			}
			RenderTexture renderTexture = temporary;
			int i = this.Max(num2, num3);
			while (i > 1)
			{
				num2 = this.Max(1, num2 / 2 + num2 % 2);
				num3 = this.Max(1, num3 / 2 + num3 % 2);
				i = i / 2 + i % 2;
				RenderTexture temporary3;
				if (i > 1)
				{
					temporary3 = RenderTexture.GetTemporary(num2, num3, 0, RenderTextureFormat.RGHalf);
					temporary3.filterMode = FilterMode.Bilinear;
					temporary3.wrapMode = TextureWrapMode.Clamp;
					Graphics.Blit(renderTexture, temporary3, this.m_DoFMat, 10);
				}
				else
				{
					temporary3 = RenderTexture.GetTemporary(num2, num3, 0, RenderTextureFormat.RHalf);
					temporary3.filterMode = FilterMode.Bilinear;
					temporary3.wrapMode = TextureWrapMode.Clamp;
					Graphics.Blit(renderTexture, temporary3, this.m_DoFMat, 8);
				}
				RenderTexture.ReleaseTemporary(renderTexture);
				renderTexture = temporary3;
			}
			RenderTexture result = renderTexture;
			if (this.previousPointAverage != null)
			{
				RenderTexture.ReleaseTemporary(this.previousPointAverage);
			}
			this.previousPointAverage = result;
			downsampledClrDepth.filterMode = FilterMode.Point;
			return result;
		}

		
		public RenderTexture RenderExclusionMask(int width, int height, Camera camera, Transform cameraTransform, LayerMask mask)
		{
			this.copiedDepthBuffer = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.RHalf);
			ScionGraphics.Blit(this.copiedDepthBuffer, this.m_DoFMat, 15);
			Shader.SetGlobalTexture("_ScionCopiedFullResDepth", this.copiedDepthBuffer);
			RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.R8);
			temporary.filterMode = FilterMode.Point;
			temporary.wrapMode = TextureWrapMode.Clamp;
			this.maskCameraTransform.position = cameraTransform.position;
			this.maskCameraTransform.rotation = cameraTransform.rotation;
			this.maskCamera.CopyFrom(camera);
			this.maskCamera.cullingMask = mask;
			this.maskCamera.SetTargetBuffers(temporary.colorBuffer, temporary.depthBuffer);
			this.maskCamera.clearFlags = CameraClearFlags.Color;
			this.maskCamera.backgroundColor = Color.white;
			this.maskCamera.renderingPath = RenderingPath.Forward;
			this.maskCamera.allowHDR = false;
			this.maskCamera.RenderWithShader(DepthOfField.maskShader, "RenderType");
			return temporary;
		}

		
		private RenderTexture CreateTiledData(RenderTexture downsampledClrDepth, float tanHalfFoV, float fNumber, float focalDistance, float focalRange, float apertureDiameter, float focalLength, float maxCoCRadius, float nearPlane, float farPlane)
		{
			int width = (downsampledClrDepth.width + 9) / 10;
			int height = (downsampledClrDepth.height + 9) / 10;
			float num = apertureDiameter * focalLength * focalDistance / (focalDistance - focalLength);
			float num2 = -apertureDiameter * focalLength / (focalDistance - focalLength);
			float num3 = ScionUtility.CoCToPixels((float)downsampledClrDepth.width);
			num *= num3;
			num2 *= num3;
			Vector4 value = default(Vector4);
			value.x = num;
			value.y = num2;
			value.z = focalDistance;
			value.w = focalRange * 0.5f;
			this.m_DoFMat.SetVector("_CoCParams1", value);
			Vector4 value2 = default(Vector4);
			value2.x = maxCoCRadius * 0.5f;
			this.m_DoFMat.SetVector("_CoCParams2", value2);
			this.m_DoFMat.SetFloat("_CoCUVOffset", 1f / (float)downsampledClrDepth.width);
			this.m_DoFMat.SetTexture("_HalfResSourceDepthTexture", downsampledClrDepth);
			RenderTexture temporary = RenderTexture.GetTemporary(width, downsampledClrDepth.height, 0, RenderTextureFormat.RGHalf);
			temporary.filterMode = FilterMode.Point;
			temporary.wrapMode = TextureWrapMode.Clamp;
			RenderTexture temporary2 = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.RGHalf);
			temporary2.filterMode = FilterMode.Point;
			temporary2.wrapMode = TextureWrapMode.Clamp;
			downsampledClrDepth.filterMode = FilterMode.Point;
			ScionGraphics.Blit(temporary, this.m_DoFMat, 0);
			this.m_DoFMat.SetTexture("_HorizontalTileResult", temporary);
			this.m_DoFMat.SetFloat("_CoCUVOffset", 1f / (float)downsampledClrDepth.height);
			ScionGraphics.Blit(temporary2, this.m_DoFMat, 1);
			RenderTexture.ReleaseTemporary(temporary);
			return temporary2;
		}

		
		private RenderTexture TileNeighbourhoodDataGathering(RenderTexture tiledData)
		{
			Vector4 value = default(Vector4);
			value.x = 1f / (float)tiledData.width;
			value.y = 1f / (float)tiledData.height;
			this.m_DoFMat.SetVector("_NeighbourhoodParams", value);
			RenderTexture temporary = RenderTexture.GetTemporary(tiledData.width, tiledData.height, 0, RenderTextureFormat.RGHalf);
			temporary.filterMode = FilterMode.Point;
			temporary.wrapMode = TextureWrapMode.Clamp;
			this.m_DoFMat.SetTexture("_TiledData", tiledData);
			ScionGraphics.Blit(temporary, this.m_DoFMat, 2);
			return temporary;
		}

		
		private RenderTexture Presort(RenderTexture downsampledClrDepth, RenderTexture neighbourhoodData)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(downsampledClrDepth.width, downsampledClrDepth.height, 0, RenderTextureFormat.ARGB2101010);
			temporary.filterMode = FilterMode.Point;
			temporary.wrapMode = TextureWrapMode.Clamp;
			this.m_DoFMat.SetTexture("_HalfResSourceDepthTexture", downsampledClrDepth);
			this.m_DoFMat.SetTexture("_TiledNeighbourhoodData", neighbourhoodData);
			ScionGraphics.Blit(temporary, this.m_DoFMat, 11);
			return temporary;
		}

		
		private RenderTexture PrefilterSource(RenderTexture downsampledClrDepth)
		{
			this.m_DoFMat.SetTexture("_HalfResSourceDepthTexture", downsampledClrDepth);
			downsampledClrDepth.filterMode = FilterMode.Point;
			RenderTexture temporary = RenderTexture.GetTemporary(downsampledClrDepth.width, downsampledClrDepth.height, 0, downsampledClrDepth.format);
			temporary.filterMode = FilterMode.Point;
			temporary.wrapMode = TextureWrapMode.Clamp;
			ScionGraphics.Blit(temporary, this.m_DoFMat, 4);
			return temporary;
		}

		
		private void BlurTapPass(RenderTexture prefilteredSource, RenderTexture tiledData, RenderTexture neighbourhoodData, RenderTexture exclusionMask, RenderTexture depthCenterAverage, RenderTexture presortTexture, DepthOfFieldSamples qualityLevel, out RenderTexture tapsTexture, out RenderTexture alphaTexture)
		{
			this.m_DoFMat.SetTexture("_TiledData", tiledData);
			this.m_DoFMat.SetTexture("_TiledNeighbourhoodData", neighbourhoodData);
			this.m_DoFMat.SetTexture("_HalfResSourceDepthTexture", prefilteredSource);
			this.m_DoFMat.SetTexture("_PresortTexture", presortTexture);
			if (exclusionMask != null)
			{
				this.m_DoFMat.SetTexture("_ExclusionMask", exclusionMask);
			}
			if (depthCenterAverage != null)
			{
				this.m_DoFMat.SetTexture("_AvgCenterDepth", depthCenterAverage);
			}
			prefilteredSource.filterMode = FilterMode.Point;
			tapsTexture = RenderTexture.GetTemporary(prefilteredSource.width, prefilteredSource.height, 0, prefilteredSource.format);
			tapsTexture.filterMode = FilterMode.Point;
			tapsTexture.wrapMode = TextureWrapMode.Clamp;
			alphaTexture = RenderTexture.GetTemporary(prefilteredSource.width, prefilteredSource.height, 0, RenderTextureFormat.R8);
			alphaTexture.filterMode = FilterMode.Point;
			alphaTexture.wrapMode = TextureWrapMode.Clamp;
			this.renderBuffers[0] = tapsTexture.colorBuffer;
			this.renderBuffers[1] = alphaTexture.colorBuffer;
			Graphics.SetRenderTarget(this.renderBuffers, tapsTexture.depthBuffer);
			if (qualityLevel == DepthOfFieldSamples.Normal_25)
			{
				ScionGraphics.Blit(this.m_DoFMat, 12);
			}
			if (qualityLevel == DepthOfFieldSamples.High_49)
			{
				ScionGraphics.Blit(this.m_DoFMat, 5);
			}
		}

		
		private RenderTexture MedianFilterPass(RenderTexture inputTexture)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(inputTexture.width, inputTexture.height, 0, inputTexture.format);
			temporary.filterMode = FilterMode.Point;
			temporary.wrapMode = TextureWrapMode.Clamp;
			inputTexture.filterMode = FilterMode.Point;
			Graphics.Blit(inputTexture, temporary, this.m_DoFMat, 3);
			RenderTexture.ReleaseTemporary(inputTexture);
			return temporary;
		}

		
		private void TemporalPass(ref RenderTexture tapsTexture, RenderTexture previousTapsTexture)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(tapsTexture.width, tapsTexture.height, 0, tapsTexture.format);
			temporary.filterMode = FilterMode.Point;
			temporary.wrapMode = TextureWrapMode.Clamp;
			previousTapsTexture.filterMode = FilterMode.Bilinear;
			this.m_DoFMatTemporal.SetTexture("_TapsCurrentTexture", tapsTexture);
			this.m_DoFMatTemporal.SetTexture("_TapsHistoryTexture", previousTapsTexture);
			ScionGraphics.Blit(temporary, this.m_DoFMatTemporal, 0);
			RenderTexture.ReleaseTemporary(tapsTexture);
			tapsTexture = temporary;
		}

		
		private void TemporalPass(ref RenderTexture tapsTexture, ref RenderTexture alphaTexture, RenderTexture previousTapsTexture, RenderTexture previousAlphaTexture)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(tapsTexture.width, tapsTexture.height, 0, tapsTexture.format);
			temporary.filterMode = FilterMode.Point;
			temporary.wrapMode = TextureWrapMode.Clamp;
			RenderTexture temporary2 = RenderTexture.GetTemporary(alphaTexture.width, alphaTexture.height, 0, alphaTexture.format);
			temporary2.filterMode = FilterMode.Point;
			temporary2.wrapMode = TextureWrapMode.Clamp;
			previousTapsTexture.filterMode = FilterMode.Bilinear;
			previousAlphaTexture.filterMode = FilterMode.Bilinear;
			this.m_DoFMatTemporal.SetTexture("_TapsCurrentTexture", tapsTexture);
			this.m_DoFMatTemporal.SetTexture("_AlphaCurrentTexture", alphaTexture);
			this.m_DoFMatTemporal.SetTexture("_TapsHistoryTexture", previousTapsTexture);
			this.m_DoFMatTemporal.SetTexture("_AlphaHistoryTexture", previousAlphaTexture);
			this.renderBuffers[0] = temporary.colorBuffer;
			this.renderBuffers[1] = temporary2.colorBuffer;
			Graphics.SetRenderTarget(this.renderBuffers, temporary.depthBuffer);
			ScionGraphics.Blit(this.m_DoFMatTemporal, 1);
			RenderTexture.ReleaseTemporary(tapsTexture);
			RenderTexture.ReleaseTemporary(alphaTexture);
			tapsTexture = temporary;
			alphaTexture = temporary2;
		}

		
		private RenderTexture UpsampleDepthOfField(RenderTexture source, RenderTexture tapsTexture, RenderTexture alphaTexture, RenderTexture neighbourhoodData, RenderTexture exclusionMask)
		{
			this.m_DoFMat.SetTexture("_TapsTexture", tapsTexture);
			this.m_DoFMat.SetTexture("_AlphaTexture", alphaTexture);
			this.m_DoFMat.SetTexture("_FullResolutionSource", source);
			this.m_DoFMat.SetTexture("_TiledNeighbourhoodData", neighbourhoodData);
			if (exclusionMask != null)
			{
				this.m_DoFMat.SetTexture("_ExclusionMask", exclusionMask);
			}
			neighbourhoodData.filterMode = FilterMode.Bilinear;
			tapsTexture.filterMode = FilterMode.Point;
			alphaTexture.filterMode = FilterMode.Point;
			RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
			source.filterMode = FilterMode.Point;
			source.wrapMode = TextureWrapMode.Clamp;
			ScionGraphics.Blit(temporary, this.m_DoFMat, 6);
			return temporary;
		}

		
		private RenderTexture BilateralAlphaFilter(RenderTexture alphaTexture, RenderTexture tapsTexture)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(alphaTexture.width, alphaTexture.height, 0, alphaTexture.format);
			temporary.filterMode = FilterMode.Point;
			temporary.wrapMode = TextureWrapMode.Clamp;
			alphaTexture.filterMode = FilterMode.Point;
			this.m_DoFMat.SetTexture("_AlphaTexture", alphaTexture);
			tapsTexture.filterMode = FilterMode.Point;
			this.m_DoFMat.SetTexture("_TapsTexture", tapsTexture);
			Graphics.Blit(alphaTexture, temporary, this.m_DoFMat, 14);
			RenderTexture.ReleaseTemporary(alphaTexture);
			return temporary;
		}

		
		private Material m_DoFMat;

		
		private Material m_DoFMatTemporal;

		
		private RenderTexture previousPointAverage;

		
		private Camera m_maskCamera;

		
		private static Shader m_maskShader;

		
		private Transform m_maskCameraTransform;

		
		private RenderTexture previousTapsTexture;

		
		private RenderTexture previousAlphaTexture;

		
		private RenderTexture copiedDepthBuffer;

		
		private float temporalUVOffset;

		
		private RenderBuffer[] renderBuffers = new RenderBuffer[2];
	}
}
