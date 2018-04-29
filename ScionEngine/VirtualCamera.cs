using System;
using UnityEngine;

namespace ScionEngine
{
	
	public class VirtualCamera
	{
		
		public VirtualCamera()
		{
			this.m_virtualCameraMat = new Material(Shader.Find("Hidden/ScionVirtualCamera"));
			this.m_virtualCameraMat.hideFlags = HideFlags.HideAndDontSave;
		}

		
		public void ReleaseResources()
		{
			if (this.m_virtualCameraMat != null)
			{
				UnityEngine.Object.Destroy(this.m_virtualCameraMat);
				this.m_virtualCameraMat = null;
			}
			if (this.m_currentResult1 != null)
			{
				RenderTexture.ReleaseTemporary(this.m_currentResult1);
				this.m_currentResult1 = null;
			}
			if (this.m_currentResult2 != null)
			{
				RenderTexture.ReleaseTemporary(this.m_currentResult2);
				this.m_currentResult2 = null;
			}
		}

		
		public bool PlatformCompatibility()
		{
			return Shader.Find("Hidden/ScionVirtualCamera").isSupported;
		}

		
		private RenderTexture DownsampleTexture(RenderTexture renderTex)
		{
			int num = renderTex.width;
			int num2 = renderTex.height;
			int i = (num <= num2) ? num2 : num;
			renderTex.filterMode = FilterMode.Bilinear;
			RenderTexture renderTexture = renderTex;
			bool flag = true;
			while (i > 1)
			{
				i = i / 2 + i % 2;
				num = Mathf.Max(num / 2 + num % 2, 1);
				num2 = Mathf.Max(num2 / 2 + num2 % 2, 1);
				RenderTexture temporary = RenderTexture.GetTemporary(num, num2, 0, renderTex.format);
				temporary.filterMode = FilterMode.Bilinear;
				temporary.wrapMode = TextureWrapMode.Clamp;
				if (flag)
				{
					Graphics.Blit(renderTexture, temporary, this.m_virtualCameraMat, 3);
					flag = false;
				}
				else
				{
					Graphics.Blit(renderTexture, temporary);
					RenderTexture.ReleaseTemporary(renderTexture);
				}
				renderTexture = temporary;
			}
			return renderTexture;
		}

		
		public void BindVirtualCameraTextures(Material mat)
		{
			mat.SetTexture("_VirtualCameraTexture1", this.m_currentResult1);
			mat.SetTexture("_VirtualCameraTexture2", this.m_currentResult2);
		}

		
		public float CalculateManualExposure(CameraParameters cameraParams, float middleGrey = 0.18f)
		{
			float num = 15.3846149f * cameraParams.fNumber * cameraParams.fNumber / (cameraParams.ISO * cameraParams.shutterSpeed);
			return Mathf.Pow(2f, cameraParams.exposureCompensation) * 3000f * middleGrey / num;
		}

		
		public void BindVirtualCameraParams(Material mat, CameraParameters cameraParams, float focalDistance, float halfResWidth, bool isFirstRender)
		{
			mat.SetVector("_VirtualCameraParams1", new Vector4
			{
				x = 1f / (cameraParams.focalLength * 1000f),
				y = cameraParams.fNumber,
				z = cameraParams.shutterSpeed,
				w = ((!isFirstRender) ? (1f - Mathf.Exp(-Time.deltaTime * cameraParams.adaptionSpeed)) : 1f)
			});
			mat.SetVector("_VirtualCameraParams2", new Vector4
			{
				x = cameraParams.exposureCompensation + 1f,
				y = cameraParams.focalLength,
				z = focalDistance,
				w = ScionUtility.CoCToPixels(halfResWidth)
			});
			mat.SetVector("_VirtualCameraParams3", new Vector4
			{
				x = Mathf.Pow(2f, cameraParams.minMaxExposure.x),
				y = Mathf.Pow(2f, cameraParams.minMaxExposure.y)
			});
		}

		
		public void CalculateVirtualCamera(CameraParameters cameraParams, RenderTexture textureToDownsample, float halfResWidth, float tanHalfFoV, float focalDistance, bool isFirstRender)
		{
			if (cameraParams.cameraMode == CameraMode.Manual || cameraParams.cameraMode == CameraMode.Off)
			{
				return;
			}
			if (this.m_currentResult2 != null)
			{
				RenderTexture.ReleaseTemporary(this.m_currentResult2);
				this.m_currentResult2 = null;
			}
			this.BindVirtualCameraParams(this.m_virtualCameraMat, cameraParams, focalDistance, halfResWidth, isFirstRender);
			RenderTexture renderTexture = this.DownsampleTexture(textureToDownsample);
			this.m_virtualCameraMat.SetTexture("_DownsampledScene", renderTexture);
			if (this.m_previousExposureTexture != null)
			{
				this.m_virtualCameraMat.SetTexture("_PreviousExposureTexture", this.m_previousExposureTexture);
			}
			this.m_currentResult1 = RenderTexture.GetTemporary(1, 1, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
			this.m_currentResult2 = RenderTexture.GetTemporary(1, 1, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
			this.renderBuffers[0] = this.m_currentResult1.colorBuffer;
			this.renderBuffers[1] = this.m_currentResult2.colorBuffer;
			int passNr = cameraParams.cameraMode - CameraMode.AutoPriority;
			Graphics.SetRenderTarget(this.renderBuffers, this.m_currentResult1.depthBuffer);
			ScionGraphics.Blit(this.m_virtualCameraMat, passNr);
			RenderTexture.ReleaseTemporary(renderTexture);
			if (this.m_previousExposureTexture != null)
			{
				RenderTexture.ReleaseTemporary(this.m_previousExposureTexture);
			}
			this.m_previousExposureTexture = this.m_currentResult1;
		}

		
		public void EndOfFrameCleanup()
		{
		}

		
		public const float FilmWidth = 35f;

		
		private const float BuiltinExposureCompensation = 1f;

		
		private Material m_virtualCameraMat;

		
		private RenderTexture m_previousExposureTexture;

		
		private const RenderTextureFormat VCTextureFormat = RenderTextureFormat.ARGBFloat;

		
		private RenderTexture m_currentResult1;

		
		private RenderTexture m_currentResult2;

		
		private RenderBuffer[] renderBuffers = new RenderBuffer[2];

		
		public const float LIGHT_INTENSITY_MULT = 3000f;

		
		private ComputeBuffer readBfr;

		
		private Vector4[] readVec;
	}
}
