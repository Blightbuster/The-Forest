using System;
using UnityEngine;

namespace ScionEngine
{
	
	public class ScionLensFlare
	{
		
		public ScionLensFlare()
		{
			this.m_lensFlareMat = new Material(Shader.Find("Hidden/ScionLensFlare"));
			this.m_lensFlareMat.hideFlags = HideFlags.HideAndDontSave;
		}

		
		public void ReleaseResources()
		{
			if (this.m_lensFlareMat != null)
			{
				UnityEngine.Object.Destroy(this.m_lensFlareMat);
				this.m_lensFlareMat = null;
			}
		}

		
		public bool PlatformCompatibility()
		{
			return Shader.Find("Hidden/ScionLensFlare").isSupported;
		}

		
		private float GetDownsamplingNormalizer(int origSourceWidth, int downsampledWidth)
		{
			return (float)downsampledWidth / (float)origSourceWidth;
		}

		
		private float GetBlurNormalizer(LensFlareBlurSamples blurSamples)
		{
			float result = 0f;
			if (blurSamples == LensFlareBlurSamples.Off)
			{
				result = 1f;
			}
			else if (blurSamples == LensFlareBlurSamples.x4)
			{
				result = 0.25f;
			}
			else if (blurSamples == LensFlareBlurSamples.x8)
			{
				result = 0.125f;
			}
			return result;
		}

		
		private void SetShaderParameters(LensFlareParameters lensFlareParams, RenderTexture downsampledScene, float downsamplingNormalizer)
		{
			if (lensFlareParams.lensColorTexture != null)
			{
				this.m_lensFlareMat.SetTexture("_LensColorTexture", lensFlareParams.lensColorTexture);
			}
			else
			{
				this.m_lensFlareMat.SetTexture("_LensColorTexture", ScionUtility.WhiteTexture);
			}
			Vector4 value = default(Vector4);
			value.x = lensFlareParams.ghostIntensity / (float)lensFlareParams.ghostSamples;
			value.y = lensFlareParams.ghostDispersal * 10f / (float)lensFlareParams.ghostSamples;
			value.z = lensFlareParams.ghostDistortion * 60f;
			value.w = lensFlareParams.ghostEdgeFade;
			this.m_lensFlareMat.SetVector("_LensFlareParams1", value);
			Vector4 value2 = default(Vector4);
			value2.x = lensFlareParams.haloIntensity;
			value2.y = lensFlareParams.haloWidth;
			value2.z = lensFlareParams.haloDistortion * 25f;
			value2.w = downsamplingNormalizer;
			this.m_lensFlareMat.SetVector("_LensFlareParams2", value2);
			Vector4 value3 = default(Vector4);
			value3.x = lensFlareParams.blurStrength * 50f * this.GetBlurNormalizer(lensFlareParams.blurSamples) * downsamplingNormalizer;
			this.m_lensFlareMat.SetVector("_LensFlareParams3", value3);
			Vector4 value4 = default(Vector4);
			value4.x = 1f / (float)downsampledScene.width;
			value4.y = 1f / (float)downsampledScene.height;
			this.m_lensFlareMat.SetVector("_TextureParams", value4);
		}

		
		public RenderTexture RenderLensFlare(RenderTexture downsampledScene, LensFlareParameters lensFlareParams, int origSourceWidth)
		{
			float downsamplingNormalizer = this.GetDownsamplingNormalizer(origSourceWidth, downsampledScene.width);
			this.SetShaderParameters(lensFlareParams, downsampledScene, downsamplingNormalizer);
			downsampledScene.wrapMode = TextureWrapMode.Clamp;
			RenderTexture temporary = RenderTexture.GetTemporary(downsampledScene.width, downsampledScene.height, 0, downsampledScene.format, RenderTextureReadWrite.Linear);
			temporary.filterMode = FilterMode.Bilinear;
			temporary.wrapMode = TextureWrapMode.Clamp;
			Graphics.Blit(downsampledScene, temporary, this.m_lensFlareMat, (int)lensFlareParams.ghostSamples);
			this.HexagonalBlur(temporary, lensFlareParams.blurSamples);
			return temporary;
		}

		
		public void HexagonalBlur(RenderTexture lensFlareTex, LensFlareBlurSamples blurSamples)
		{
			if (blurSamples == LensFlareBlurSamples.Off)
			{
				return;
			}
			int passNr = (blurSamples != LensFlareBlurSamples.x4) ? 1 : 0;
			int pass = (blurSamples != LensFlareBlurSamples.x4) ? 3 : 2;
			RenderTexture temporary = RenderTexture.GetTemporary(lensFlareTex.width, lensFlareTex.height, 0, lensFlareTex.format, RenderTextureReadWrite.Linear);
			RenderTexture temporary2 = RenderTexture.GetTemporary(lensFlareTex.width, lensFlareTex.height, 0, lensFlareTex.format, RenderTextureReadWrite.Linear);
			lensFlareTex.filterMode = FilterMode.Bilinear;
			temporary.filterMode = FilterMode.Bilinear;
			temporary2.filterMode = FilterMode.Bilinear;
			this.targetBuffers[0] = temporary.colorBuffer;
			this.targetBuffers[1] = temporary2.colorBuffer;
			Graphics.SetRenderTarget(this.targetBuffers, temporary.depthBuffer);
			this.m_lensFlareMat.SetTexture("_MainTex", lensFlareTex);
			ScionGraphics.Blit(this.m_lensFlareMat, passNr);
			this.m_lensFlareMat.SetTexture("_BlurTexture1", temporary2);
			Graphics.Blit(temporary, lensFlareTex, this.m_lensFlareMat, pass);
			RenderTexture.ReleaseTemporary(temporary);
			RenderTexture.ReleaseTemporary(temporary2);
		}

		
		public Material m_lensFlareMat;

		
		private RenderBuffer[] targetBuffers = new RenderBuffer[2];
	}
}
